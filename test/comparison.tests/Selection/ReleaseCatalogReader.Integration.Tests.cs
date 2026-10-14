using System.Net;
using System.Text;
using AwesomeAssertions;

namespace Specht.Comparison.Tests.Selection;

[Trait("Tier", "Integration")]
public sealed class ReleaseCatalogReaderIntegrationTests
{
    [Fact]
    public async Task PaginatedCatalogWithAnnotatedTags_WhenReading_ShouldReadAllPagesAndPeelToExactCommits()
    {
        // Given
        var paths = new List<string>();
        using var handler = new ScriptedHttpHandler(request =>
        {
            var path = request.RequestUri!.PathAndQuery;
            paths.Add(path);
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(path switch
            {
                "/repos/example/tool/releases?per_page=100&page=1" => """
                    [{"id":10,"tag_name":"v1","draft":false,"prerelease":false,"published_at":"2026-01-01T00:00:00Z"}]
                    """,
                "/repos/example/tool/releases?per_page=100&page=2" => """
                    [{"id":15,"tag_name":"preview","draft":false,"prerelease":true,"published_at":"2026-01-02T00:00:00Z"}]
                    """,
                "/repos/example/tool/git/ref/tags/v1" => """{"object":{"type":"commit","sha":"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"}}""",
                "/repos/example/tool/git/ref/tags/preview" => """{"object":{"type":"tag","sha":"bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb"}}""",
                "/repos/example/tool/git/tags/bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb" =>
                    """{"object":{"type":"commit","sha":"cccccccccccccccccccccccccccccccccccccccc"}}""",
                _ => throw new InvalidOperationException("Unexpected request"),
            }, Encoding.UTF8, "application/json");
            if (path.EndsWith("page=1", StringComparison.Ordinal))
                response.Headers.Add("Link", "<https://api.github.com/repos/example/tool/releases?per_page=100&page=2>; rel=\"next\"");
            return response;
        });
        using var client = new HttpClient(handler);

        // When
        var catalog = await new ReleaseCatalogReader(client).ReadAsync("example/tool", TestContext.Current.CancellationToken);

        // Then
        catalog.Complete.Should().BeTrue();
        catalog.Releases.Select(static x => x.Id).Should().Equal(10, 15);
        catalog.Releases[1].Commit.Should().Be(new string('c', 40));
        paths.Should().Contain("/repos/example/tool/releases?per_page=100&page=2");
    }

    [Fact]
    public async Task FailedPage_WhenReading_ShouldReturnAnIncompleteCatalog()
    {
        // Given
        using var handler = new ScriptedHttpHandler(static _ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        using var client = new HttpClient(handler);

        // When
        var catalog = await new ReleaseCatalogReader(client).ReadAsync("example/tool", TestContext.Current.CancellationToken);

        // Then
        catalog.Complete.Should().BeFalse();
        catalog.Failure.Should().NotBeNullOrEmpty();
    }
}
