using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Specht.Comparison;

/// <summary>Loads or inserts a durable pair using authority supplied by its caller.</summary>
public sealed class ReleasePairAssetStore(HttpClient client)
{
    public async Task<ReleasePair> GetOrCreateAsync(ReleaseRecord current, Func<CancellationToken, Task<ReleasePair>> create,
        CancellationToken cancellationToken = default)
    {
        ComparisonJson.ValidateRelease(current);
        try
        {
            var existing = await FindAsync(current, cancellationToken);
            if (existing is not null) return await ReadAsync(current, existing.Value, cancellationToken);
            var proposed = await create(cancellationToken);
            ComparisonJson.ValidatePair(proposed, current);
            using var request = new HttpRequestMessage(HttpMethod.Post,
                $"https://uploads.github.com/repos/{current.Repository}/releases/{current.Id}/assets?name=comparison-pair-v1.json")
            {
                Content = new StringContent(ComparisonJson.Write(proposed), Encoding.UTF8, "application/json"),
            };
            using var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode && response.StatusCode is not (HttpStatusCode.Conflict or HttpStatusCode.UnprocessableEntity))
                throw new InvalidDataException("Pair asset could not be persisted; collection must not start.");
            var retained = await FindAsync(current, cancellationToken)
                ?? throw new InvalidDataException("Pair asset was not confirmed in durable release storage.");
            var winner = await ReadAsync(current, retained, cancellationToken);
            if (response.IsSuccessStatusCode && ComparisonJson.Digest(winner) != ComparisonJson.Digest(proposed))
                throw new InvalidDataException("Uploaded pair asset does not match the selected evidence.");
            return winner;
        }
        catch (Exception exception) when (exception is HttpRequestException or JsonException or InvalidOperationException
            or System.Collections.Generic.KeyNotFoundException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            throw new InvalidDataException("Pair asset is unavailable or malformed; do not replace the saved selection.");
        }
    }

    private async Task<long?> FindAsync(ReleaseRecord current, CancellationToken cancellationToken)
    {
        long? found = null;
        for (var page = 1; page <= 10000; page++)
        {
            using var response = await client.GetAsync(
                $"https://api.github.com/repos/{current.Repository}/releases/{current.Id}/assets?per_page=100&page={page}", cancellationToken);
            response.EnsureSuccessStatusCode();
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            if (document.RootElement.ValueKind != JsonValueKind.Array) throw new JsonException();
            foreach (var asset in document.RootElement.EnumerateArray())
            {
                if (asset.GetProperty("name").GetString() != "comparison-pair-v1.json") continue;
                if (found is not null || asset.GetProperty("state").GetString() != "uploaded")
                    throw new InvalidDataException("Saved pair asset is duplicated or its upload is incomplete.");
                found = asset.GetProperty("id").GetInt64();
                if (found <= 0) throw new JsonException();
            }
            if (document.RootElement.GetArrayLength() < 100) return found;
        }
        throw new InvalidDataException("Pair asset catalog pagination is incomplete.");
    }

    private async Task<ReleasePair> ReadAsync(ReleaseRecord current, long assetId, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get,
            $"https://api.github.com/repos/{current.Repository}/releases/assets/{assetId}");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
        using var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return ComparisonJson.ReadPair(await response.Content.ReadAsStringAsync(cancellationToken), current);
    }
}
