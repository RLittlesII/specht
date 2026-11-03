using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Specht.Comparison;

/// <summary>Waits, within a bound, for one exact artifact of this workflow run and unpacks it under the repository.</summary>
public sealed class RunArtifacts(HttpClient client, IFileSystem fileSystem, TimeProvider time, string repositoryRoot)
{
    public async Task<bool> WaitAsync(string repository, string runId, string name, string directory, TimeSpan bound,
        CancellationToken cancellationToken)
    {
        if (!ComparisonJson.IsRepository(repository) || !EvidencePaths.IsRelative(directory) || runId.Any(static c => !char.IsAsciiDigit(c)))
            throw new InvalidDataException("Run artifact request is not this repository's run or not a repository-relative directory.");
        using var limit = new CancellationTokenSource(bound, time);
        using var waiting = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, limit.Token);
        try
        {
            while (true)
            {
                var poll = Task.Delay(PollInterval, time, waiting.Token);
                try
                {
                    if (await FindAsync(repository, runId, name, waiting.Token) is { } id)
                    {
                        await UnpackAsync(repository, id, directory, cancellationToken);
                        return true;
                    }
                }
                catch (Exception exception) when (IsTransient(exception, waiting.Token, cancellationToken))
                {
                }

                await poll;
            }
        }
        catch (OperationCanceledException) when (limit.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            return false;
        }
    }

    private static bool IsTransient(Exception exception, CancellationToken waiting, CancellationToken cancellationToken) => exception switch
    {
        HttpRequestException { StatusCode: null or HttpStatusCode.TooManyRequests or >= HttpStatusCode.InternalServerError } => true,
        OperationCanceledException => !waiting.IsCancellationRequested && !cancellationToken.IsCancellationRequested,
        _ => false,
    };

    private async Task<long?> FindAsync(string repository, string runId, string name, CancellationToken cancellationToken)
    {
        var found = new List<long>();
        for (var page = 1; page <= MaximumPages; page++)
        {
            using var response = await client.GetAsync(
                $"https://api.github.com/repos/{repository}/actions/runs/{runId}/artifacts?name={Uri.EscapeDataString(name)}&per_page=100&page={page}",
                cancellationToken);
            response.EnsureSuccessStatusCode();
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            foreach (var artifact in document.RootElement.GetProperty("artifacts").EnumerateArray())
            {
                if (artifact.GetProperty("name").GetString() == name && !artifact.GetProperty("expired").GetBoolean())
                    found.Add(artifact.GetProperty("id").GetInt64());
            }

            if (!response.Headers.TryGetValues("Link", out var links)
                || !links.Any(static link => link.Contains("rel=\"next\"", StringComparison.Ordinal)))
            {
                break;
            }
        }

        return found.Count switch
        {
            0 => null,
            1 => found[0],
            _ => throw new InvalidDataException("Run has more than one artifact of that name."),
        };
    }

    private async Task UnpackAsync(string repository, long id, string directory, CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync($"https://api.github.com/repos/{repository}/actions/artifacts/{id}/zip", cancellationToken);
        response.EnsureSuccessStatusCode();
        var entries = new List<(string Name, byte[] Content)>();
        try
        {
            using var archive = new ZipArchive(new MemoryStream(await response.Content.ReadAsByteArrayAsync(cancellationToken)), ZipArchiveMode.Read);
            foreach (var entry in archive.Entries)
            {
                if (entry.FullName.EndsWith('/') && entry.Length == 0) continue;
                if (!EvidencePaths.IsRelative(entry.FullName) || ((entry.ExternalAttributes >> 16) & 0xF000) is not (0 or 0x8000)
                    || entries.Any(written => string.Equals(written.Name, entry.FullName, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new InvalidDataException("Run artifact has an unsafe or duplicated entry.");
                }

                using var content = new MemoryStream();
                await using (var stream = entry.Open()) await stream.CopyToAsync(content, cancellationToken);
                entries.Add((entry.FullName, content.ToArray()));
            }
        }
        catch (Exception exception) when (exception is IOException or ArgumentException or NotSupportedException)
        {
            throw new InvalidDataException("Run artifact is not a readable archive.");
        }

        var target = fileSystem.Path.Combine(repositoryRoot, directory);
        foreach (var (name, content) in entries)
        {
            var path = fileSystem.Path.Combine([target, .. name.Split('/')]);
            fileSystem.Directory.CreateDirectory(fileSystem.Path.GetDirectoryName(path)!);
            await using var stream = fileSystem.FileStream.New(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            await stream.WriteAsync(content, cancellationToken);
        }
    }

    private const int MaximumPages = 1000;

    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(30);
}
