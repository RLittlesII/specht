using System.Net;
using System.Text;
using AwesomeAssertions;

namespace Specht.Comparison.Tests.Selection;

[Trait("Tier", "Integration")]
public sealed class ReleasePairAssetStoreIntegrationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SavedAssetOrCompetingCreation_WhenPersisting_ShouldUseTheVerifiedDurableWinner(bool conflict)
    {
        // Given
        ReleaseRecord current = new ReleaseRecordFixture().WithId(20);
        ReleaseRecord previous = new ReleaseRecordFixture().WithId(10);
        var winner = ReleaseSelection.Select(current, new ReleaseCatalog([previous], true, null), "winner");
        var proposed = ReleaseSelection.Select(current, new ReleaseCatalog([], true, null), "other-run");
        var listed = false;
        var methods = new List<HttpMethod>();
        using var handler = new ScriptedHttpHandler(request =>
        {
            methods.Add(request.Method);
            if (request.Method == HttpMethod.Post)
                return new HttpResponseMessage(HttpStatusCode.UnprocessableEntity);
            var path = request.RequestUri!.PathAndQuery;
            if (path == "/repos/example/tool/releases/20/assets?per_page=100&page=1")
            {
                var result = conflict && !listed ? "[]" : """
                    [{"id":50,"name":"comparison-pair-v1.json","state":"uploaded"}]
                    """;
                listed = true;
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(result) };
            }
            path.Should().Be("/repos/example/tool/releases/assets/50");
            request.Headers.Accept.Select(static x => x.MediaType).Should().Contain("application/octet-stream");
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(ComparisonJson.Write(winner)) };
        });
        using var client = new HttpClient(handler);

        // When
        var pair = await new ReleasePairAssetStore(client).GetOrCreateAsync(current, _ =>
        {
            if (!conflict) throw new InvalidOperationException("Existing durable pair must skip selection");
            return Task.FromResult(proposed);
        }, TestContext.Current.CancellationToken);

        // Then
        pair.Should().BeEquivalentTo(winner);
        methods.Count(static method => method == HttpMethod.Post).Should().Be(conflict ? 1 : 0);
        methods.Should().NotContain(HttpMethod.Delete).And.NotContain(HttpMethod.Patch);
    }

    [Fact]
    public async Task NewPair_WhenUploading_ShouldVerifyTheRetainedAssetBeforeReturning()
    {
        // Given
        ReleaseRecord current = new ReleaseRecordFixture().WithId(20);
        var pair = ReleaseSelection.Select(current, new ReleaseCatalog([], true, null), "run-1");
        var uploaded = false;
        using var handler = new ScriptedHttpHandler(request =>
        {
            if (request.Method == HttpMethod.Post)
            {
                request.RequestUri!.Host.Should().Be("uploads.github.com");
                request.RequestUri.PathAndQuery.Should().Be("/repos/example/tool/releases/20/assets?name=comparison-pair-v1.json");
                request.Content!.Headers.ContentType!.MediaType.Should().Be("application/json");
                uploaded = true;
                return new HttpResponseMessage(HttpStatusCode.Created);
            }
            if (request.RequestUri!.AbsolutePath.EndsWith("/assets", StringComparison.Ordinal))
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(uploaded
                    ? """[{"id":50,"name":"comparison-pair-v1.json","state":"uploaded"}]""" : "[]")
                };
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(ComparisonJson.Write(pair)) };
        });
        using var client = new HttpClient(handler);

        // When
        var actual = await new ReleasePairAssetStore(client)
            .GetOrCreateAsync(current, _ => Task.FromResult(pair), TestContext.Current.CancellationToken);

        // Then
        uploaded.Should().BeTrue();
        actual.Should().BeEquivalentTo(pair);
    }

    [Fact]
    public async Task InvalidExistingAsset_WhenLoading_ShouldRefuseReselectionOrOverwrite()
    {
        // Given
        ReleaseRecord current = new ReleaseRecordFixture();
        using var handler = new ScriptedHttpHandler(request =>
        {
            request.Method.Should().Be(HttpMethod.Get);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                request.RequestUri!.AbsolutePath.EndsWith("/assets", StringComparison.Ordinal)
                    ? """[{"id":50,"name":"comparison-pair-v1.json","state":"uploaded"}]""" : "broken")
            };
        });
        using var client = new HttpClient(handler);

        // When
        var load = () => new ReleasePairAssetStore(client).GetOrCreateAsync(current,
            _ => throw new InvalidOperationException("No reselection"), TestContext.Current.CancellationToken);

        // Then
        await load.Should().ThrowAsync<InvalidDataException>();
    }
}
