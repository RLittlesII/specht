using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Specht.Acceptance.Comparison;

internal sealed class ActionsRun : DelegatingHandler
{
    public ActionsRun() => InnerHandler = new ReleaseAssets();

    public Dictionary<string, string> Catalog { get; } = new(StringComparer.Ordinal);

    public bool FailUploads { get; set; }

    public IReadOnlyList<string> Requests
    {
        get
        {
            lock (_gate) return [.. _requests];
        }
    }

    public TaskCompletionSource Listed { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public void Add(string name, byte[] zip)
    {
        lock (_gate) _artifacts.Add((100L + _artifacts.Count, name, zip));
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var uri = request.RequestUri!;
        List<(long Id, string Name, byte[] Zip)> artifacts;
        lock (_gate)
        {
            _requests.Add($"{request.Method} {uri.PathAndQuery}");
            artifacts = [.. _artifacts];
        }

        if (request.Method == HttpMethod.Post && FailUploads) return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        if (request.Method == HttpMethod.Get && uri.Host == "api.github.com" && uri.AbsolutePath == "/repos/example/tool/actions/runs/1234/artifacts")
        {
            var name = HttpUtility.ParseQueryString(uri.Query)["name"];
            var named = artifacts.Where(artifact => name is null || artifact.Name == name).ToArray();
            Listed.TrySetResult();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    total_count = named.Length,
                    artifacts = named.Select(static artifact => new
                    {
                        id = artifact.Id,
                        name = artifact.Name,
                        expired = false,
                        workflow_run = new { id = 1234 },
                    }),
                }), Encoding.UTF8, "application/json"),
            });
        }

        var stored = artifacts.Where(artifact => uri.AbsolutePath == $"/repos/example/tool/actions/artifacts/{artifact.Id}/zip").ToArray();
        if (request.Method == HttpMethod.Get && stored.Length == 1)
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(stored[0].Zip) });
        if (request.Method == HttpMethod.Get && uri.Host == "api.github.com" && Catalog.TryGetValue(uri.PathAndQuery, out var json))
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") });
        if (uri.AbsolutePath.StartsWith("/repos/example/tool/git/", StringComparison.Ordinal))
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        return base.SendAsync(request, cancellationToken);
    }

    private readonly Lock _gate = new();
    private readonly List<(long Id, string Name, byte[] Zip)> _artifacts = [];
    private readonly List<string> _requests = [];
}
