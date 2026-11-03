using System.Globalization;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Web;

namespace Specht.Comparison.Tests.Workflow;

internal sealed class ActionsArtifacts
{
    public IReadOnlyList<string> Requests
    {
        get
        {
            lock (_gate)
            {
                return [.. _requests];
            }
        }
    }

    public TaskCompletionSource Listed { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public static byte[] Zip(params (string Name, string Content)[] entries) =>
        Zip([.. entries.Select(static entry => (entry.Name, Encoding.UTF8.GetBytes(entry.Content)))]);

    public static byte[] Zip(params (string Name, byte[] Content)[] entries)
    {
        using var buffer = new MemoryStream();
        using (var archive = new ZipArchive(buffer, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var (name, content) in entries)
            {
                using var entry = archive.CreateEntry(name).Open();
                entry.Write(content);
            }
        }

        return buffer.ToArray();
    }

    public void Add(long id, string name, bool expired, byte[] zip)
    {
        lock (_gate)
        {
            _artifacts.Add((id, name, expired, zip));
        }
    }

    public HttpResponseMessage? Respond(HttpRequestMessage request)
    {
        var uri = request.RequestUri!;
        if (request.Method != HttpMethod.Get || uri.Host != "api.github.com")
        {
            return null;
        }

        List<(long Id, string Name, bool Expired, byte[] Zip)> listed;
        lock (_gate)
        {
            listed = [.. _artifacts];
        }

        if (uri.AbsolutePath == "/repos/example/tool/actions/runs/1234/artifacts")
        {
            lock (_gate)
            {
                _requests.Add(uri.PathAndQuery);
            }

            var query = HttpUtility.ParseQueryString(uri.Query);
            var page = int.Parse(query["page"] ?? "1", CultureInfo.InvariantCulture);
            var named = query["name"] is { } name ? listed.Where(artifact => artifact.Name == name).ToList() : listed;
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    total_count = named.Count,
                    artifacts = named.Skip((page - 1) * PageSize).Take(PageSize).Select(static artifact => new
                    {
                        id = artifact.Id,
                        name = artifact.Name,
                        expired = artifact.Expired,
                        workflow_run = new { id = 1234 },
                    }),
                }), Encoding.UTF8, "application/json"),
            };
            if (page * PageSize < named.Count)
            {
                var next = HttpUtility.ParseQueryString(uri.Query);
                next["page"] = (page + 1).ToString(CultureInfo.InvariantCulture);
                response.Headers.Add("Link", $"<https://api.github.com{uri.AbsolutePath}?{next}>; rel=\"next\"");
            }

            Listed.TrySetResult();
            return response;
        }

        var stored = listed.Where(artifact => uri.AbsolutePath == $"/repos/example/tool/actions/artifacts/{artifact.Id}/zip").ToArray();
        if (stored.Length != 1)
        {
            return null;
        }

        lock (_gate)
        {
            _requests.Add(uri.PathAndQuery);
        }

        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(stored[0].Zip) };
    }

    private const int PageSize = 2;

    private readonly Lock _gate = new();
    private readonly List<(long Id, string Name, bool Expired, byte[] Zip)> _artifacts = [];
    private readonly List<string> _requests = [];
}
