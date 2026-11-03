using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Specht.Comparison;

/// <summary>Inserts and verifies a report bundle release asset using authority supplied by its caller.</summary>
public sealed class ReportBundleAssetStore(HttpClient client)
{
    public async Task<PublishedBundle> PublishAsync(ReleaseRecord current, string name, byte[] zip, CancellationToken cancellationToken = default)
    {
        ComparisonJson.ValidateRelease(current);
        Expect(current, name, zip);
        var digest = Convert.ToHexStringLower(SHA256.HashData(zip));
        try
        {
            var retained = await FindAsync(current, name, cancellationToken);
            if (retained is null)
            {
                using var request = new HttpRequestMessage(HttpMethod.Post,
                    $"https://uploads.github.com/repos/{current.Repository}/releases/{current.Id}/assets?name={Uri.EscapeDataString(name)}")
                {
                    Content = new ByteArrayContent(zip),
                };
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
                using var response = await client.SendAsync(request, cancellationToken);
                if (!response.IsSuccessStatusCode && response.StatusCode is not (HttpStatusCode.Conflict or HttpStatusCode.UnprocessableEntity))
                {
                    throw new InvalidDataException("Report bundle upload was rejected.");
                }

                retained = await FindAsync(current, name, cancellationToken)
                    ?? throw new InvalidDataException("Report bundle was not confirmed in durable release storage.");
            }

            var content = await DownloadAsync(current, retained.Value, cancellationToken);
            if (Convert.ToHexStringLower(SHA256.HashData(content)) != digest)
            {
                throw new InvalidDataException("Retained report bundle differs from this attempt's bundle and is not replaced.");
            }

            _ = ReportBundler.Validate(new MemoryStream(content));
            return new PublishedBundle(retained.Value, name, digest);
        }
        catch (Exception exception) when (exception is HttpRequestException or JsonException or InvalidOperationException
            or KeyNotFoundException or FormatException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
        {
            throw new InvalidDataException("Report bundle publication is unconfirmed; no retained report is claimed.");
        }
    }

    private static void Expect(ReleaseRecord current, string name, byte[] zip)
    {
        _ = ReportBundler.Validate(new MemoryStream(zip));
        using var archive = new ZipArchive(new MemoryStream(zip), ZipArchiveMode.Read);
        var session = ComparisonJson.ReadSession(Text(archive, "session.json"));
        _ = ComparisonJson.ReadPair(Text(archive, "pair.json"), current);
        if (name != ReportBundler.Name(current.Id, session))
        {
            throw new InvalidDataException("Asset name does not identify this release, run and attempt.");
        }
    }

    private static string Text(ZipArchive archive, string path)
    {
        using var reader = new StreamReader(archive.GetEntry(path)!.Open(), Encoding.UTF8);
        return reader.ReadToEnd();
    }

    private async Task<long?> FindAsync(ReleaseRecord current, string name, CancellationToken cancellationToken)
    {
        long? found = null;
        for (var page = 1; page <= 10000; page++)
        {
            using var response = await client.GetAsync(
                $"https://api.github.com/repos/{current.Repository}/releases/{current.Id}/assets?per_page=100&page={page}", cancellationToken);
            response.EnsureSuccessStatusCode();
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                throw new JsonException();
            }

            foreach (var asset in document.RootElement.EnumerateArray())
            {
                if (asset.GetProperty("name").GetString() != name)
                {
                    continue;
                }

                if (found is not null || asset.GetProperty("state").GetString() != "uploaded")
                {
                    throw new InvalidDataException("Report bundle asset is duplicated or its upload is incomplete.");
                }

                found = asset.GetProperty("id").GetInt64();
                if (found <= 0)
                {
                    throw new JsonException();
                }
            }

            if (document.RootElement.GetArrayLength() < 100)
            {
                return found;
            }
        }

        throw new InvalidDataException("Report bundle asset catalog pagination is incomplete.");
    }

    private async Task<byte[]> DownloadAsync(ReleaseRecord current, long assetId, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.github.com/repos/{current.Repository}/releases/assets/{assetId}");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
        using var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }
}
