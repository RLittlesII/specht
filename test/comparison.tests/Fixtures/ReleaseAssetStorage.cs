using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Web;

namespace Specht.Comparison.Tests.Reporting;

internal sealed class ReleaseAssetStorage
{
    public List<ReleaseAsset> Assets { get; } = [];

    public List<(HttpMethod Method, string Uri, string? ContentType)> Requests { get; } = [];

    public HttpStatusCode UploadStatus { get; set; } = HttpStatusCode.Created;

    public string UploadedState { get; set; } = "uploaded";

    public Func<byte[], byte[]> Retain { get; set; } = static content => content;

    public HttpStatusCode DownloadStatus { get; set; } = HttpStatusCode.OK;

    public Func<byte[], byte[]>? Racer { get; set; }

    public HttpResponseMessage Respond(HttpRequestMessage request)
    {
        var uri = request.RequestUri!;
        Requests.Add((request.Method, uri.ToString(), request.Content?.Headers.ContentType?.MediaType));
        var query = HttpUtility.ParseQueryString(uri.Query);
        if (request.Method == HttpMethod.Post && uri.Host == "uploads.github.com" && uri.AbsolutePath == "/repos/example/tool/releases/20/assets")
        {
            using var content = new MemoryStream();
            request.Content!.ReadAsStream().CopyTo(content);
            if ((int)UploadStatus is < 200 or > 299)
            {
                if (Racer is not null) Assets.Add(new ReleaseAsset(1000 + Assets.Count, query["name"]!, "uploaded", Racer.Invoke(content.ToArray())));
                return new HttpResponseMessage(UploadStatus);
            }
            var asset = new ReleaseAsset(1000 + Assets.Count, query["name"]!, UploadedState, Retain.Invoke(content.ToArray()));
            Assets.Add(asset);
            return new HttpResponseMessage(UploadStatus)
            {
                Content = new StringContent(JsonSerializer.Serialize(new { id = asset.Id, name = asset.Name, state = asset.State })),
            };
        }
        if (request.Method == HttpMethod.Get && uri.Host == "api.github.com" && uri.AbsolutePath == "/repos/example/tool/releases/20/assets")
        {
            var size = int.Parse(query["per_page"] ?? "30", CultureInfo.InvariantCulture);
            var page = int.Parse(query["page"] ?? "1", CultureInfo.InvariantCulture);
            var listed = Assets.Skip((page - 1) * size).Take(size).Select(static asset => new { id = asset.Id, name = asset.Name, state = asset.State });
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(listed)) };
        }
        var stored = Assets.FirstOrDefault(asset => uri.AbsolutePath == $"/repos/example/tool/releases/assets/{asset.Id}");
        if (request.Method == HttpMethod.Get && uri.Host == "api.github.com" && stored is not null
            && request.Headers.Accept.Any(static accept => accept.MediaType == "application/octet-stream"))
        {
            return DownloadStatus == HttpStatusCode.OK
                ? new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(stored.Content) }
                : new HttpResponseMessage(DownloadStatus);
        }

        return new HttpResponseMessage(HttpStatusCode.NotFound);
    }
}
