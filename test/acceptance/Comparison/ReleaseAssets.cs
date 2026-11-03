using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Specht.Acceptance.Comparison;

internal sealed class ReleaseAssets : HttpMessageHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var uri = request.RequestUri!;
        var query = HttpUtility.ParseQueryString(uri.Query);
        if (request.Method == HttpMethod.Post && uri.Host == "uploads.github.com" && uri.AbsolutePath == "/repos/example/tool/releases/20/assets")
        {
            var asset = (Id: 1000L + _assets.Count, Name: query["name"]!, Content: await request.Content!.ReadAsByteArrayAsync(cancellationToken));
            _assets.Add(asset);
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(JsonSerializer.Serialize(new { id = asset.Id, name = asset.Name, state = "uploaded" })),
            };
        }

        if (request.Method != HttpMethod.Get || uri.Host != "api.github.com")
        {
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }

        if (uri.AbsolutePath == "/repos/example/tool/releases/20/assets")
        {
            var size = int.Parse(query["per_page"] ?? "30", CultureInfo.InvariantCulture);
            var page = int.Parse(query["page"] ?? "1", CultureInfo.InvariantCulture);
            var listed = _assets.Skip((page - 1) * size).Take(size).Select(static asset => new { id = asset.Id, name = asset.Name, state = "uploaded" });
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(listed)) };
        }

        var stored = _assets.Where(asset => uri.AbsolutePath == $"/repos/example/tool/releases/assets/{asset.Id}").ToArray();
        return stored.Length == 1
            ? new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(stored[0].Content) }
            : new HttpResponseMessage(HttpStatusCode.NotFound);
    }

    private readonly List<(long Id, string Name, byte[] Content)> _assets = [];
}
