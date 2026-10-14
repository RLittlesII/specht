using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Specht.Comparison;

/// <summary>Reads every catalog page and peels tags through a caller-supplied HTTP client.</summary>
public sealed class ReleaseCatalogReader(HttpClient client)
{
    public async Task<ReleaseCatalog> ReadAsync(string repository, CancellationToken cancellationToken = default)
    {
        if (!ComparisonJson.IsRepository(repository)) throw new InvalidDataException("Repository identity is invalid.");
        var releases = ImmutableArray.CreateBuilder<ReleaseRecord>();
        var next = new Uri($"https://api.github.com/repos/{repository}/releases?per_page=100&page=1");
        var visited = new HashSet<Uri>();
        try
        {
            while (next is not null)
            {
                if (!visited.Add(next) || visited.Count > 10000 || next.Scheme != "https" || next.Host != "api.github.com"
                    || next.AbsolutePath != $"/repos/{repository}/releases" || !next.IsDefaultPort || next.UserInfo.Length != 0)
                    return new ReleaseCatalog(releases.ToImmutable(), false, "Release catalog pagination identity is invalid.");
                using var response = await client.GetAsync(next, cancellationToken);
                response.EnsureSuccessStatusCode();
                using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
                if (document.RootElement.ValueKind != JsonValueKind.Array) throw new JsonException();
                foreach (var item in document.RootElement.EnumerateArray())
                {
                    var tag = item.GetProperty("tag_name").GetString() ?? throw new JsonException();
                    var published = item.GetProperty("published_at");
                    var draft = item.GetProperty("draft").GetBoolean();
                    var (commit, failure) = draft || published.ValueKind == JsonValueKind.Null
                        ? ((string?)null, "Release is unpublished.") : await ResolveAsync(repository, tag, cancellationToken);
                    releases.Add(new ReleaseRecord(repository, item.GetProperty("id").GetInt64(), tag,
                        published.ValueKind == JsonValueKind.Null ? null : published.GetDateTimeOffset(), draft,
                        item.GetProperty("prerelease").GetBoolean(), commit, failure));
                }
                next = null;
                if (response.Headers.TryGetValues("Link", out var values))
                {
                    var links = string.Join(',', values);
                    var matches = Regex.Matches(links, "<([^>]+)>;\\s*rel=\"next\"");
                    if (matches.Count > 1) throw new JsonException();
                    if (matches.Count == 1) next = new Uri(matches[0].Groups[1].Value, UriKind.Absolute);
                }
            }
            return new ReleaseCatalog(releases.ToImmutable(), true, null);
        }
        catch (Exception exception) when (exception is HttpRequestException or JsonException or InvalidOperationException
            or KeyNotFoundException or UriFormatException or FormatException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            return new ReleaseCatalog(releases.ToImmutable(), false, "Release catalog page is unavailable or malformed.");
        }
    }

    private async Task<(string? Commit, string? Failure)> ResolveAsync(string repository, string tag, CancellationToken cancellationToken)
    {
        var path = $"https://api.github.com/repos/{repository}/git/ref/tags/{Uri.EscapeDataString(tag)}";
        var seen = new HashSet<string>(StringComparer.Ordinal);
        try
        {
            for (var depth = 0; depth < 16; depth++)
            {
                using var response = await client.GetAsync(path, cancellationToken);
                response.EnsureSuccessStatusCode();
                using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
                var value = document.RootElement.GetProperty("object");
                var sha = value.GetProperty("sha").GetString();
                if (!ComparisonJson.IsCommit(sha) || !seen.Add(sha!)) break;
                var kind = value.GetProperty("type").GetString();
                if (kind == "commit") return (sha, null);
                if (kind != "tag") break;
                path = $"https://api.github.com/repos/{repository}/git/tags/{sha}";
            }
        }
        catch (Exception exception) when (exception is HttpRequestException or JsonException or InvalidOperationException
            or KeyNotFoundException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
        { }
        return (null, "Selected release tag cannot be resolved to an exact commit.");
    }
}
