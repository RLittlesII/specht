using System.IO.Abstractions;
using System.Net;
using AwesomeAssertions;

namespace Specht.Comparison.Tests.Selection;

[Trait("Tier", "Integration")]
public sealed class InvalidSavedPairIntegrationTests
{
    [Fact]
    public async Task SelfReferencingSavedFile_WhenLoading_ShouldRejectWithoutReselectingOrOverwriting()
    {
        // Given
        ReleaseRecord current = new ReleaseRecordFixture().WithId(20);
        var pair = ReleaseSelection.Select(current, new ReleaseCatalog([], true, null), "run-1");
        pair = pair with { Previous = current with { PublishedAt = current.PublishedAt!.Value.AddDays(-1) } };
        var json = ComparisonJson.Write(pair);
        var directory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "comparison-pair-v1.json");
        try
        {
            await File.WriteAllTextAsync(path, json, TestContext.Current.CancellationToken);
            var selected = false;
            var store = new ReleasePairStore(new FileSystem(), directory);

            // When
            var load = () => store.GetOrCreateAsync(current, _ =>
            {
                selected = true;
                return Task.FromResult(pair);
            }, TestContext.Current.CancellationToken);

            // Then
            await load.Should().ThrowAsync<InvalidDataException>();
            selected.Should().BeFalse();
            (await File.ReadAllTextAsync(path, TestContext.Current.CancellationToken)).Should().Be(json);
            Directory.GetFiles(directory).Should().ContainSingle();
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public async Task SelfReferencingSavedAsset_WhenLoading_ShouldRejectWithoutProviderMutation()
    {
        // Given
        ReleaseRecord current = new ReleaseRecordFixture().WithId(20);
        var pair = ReleaseSelection.Select(current, new ReleaseCatalog([], true, null), "run-1");
        pair = pair with { Previous = current with { PublishedAt = current.PublishedAt!.Value.AddDays(-1) } };
        var json = ComparisonJson.Write(pair);
        var methods = new List<HttpMethod>();
        using var handler = new ScriptedHttpHandler(request =>
        {
            methods.Add(request.Method);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                request.RequestUri!.AbsolutePath.EndsWith("/assets", StringComparison.Ordinal)
                    ? """[{"id":50,"name":"comparison-pair-v1.json","state":"uploaded"}]""" : json)
            };
        });
        using var client = new HttpClient(handler);
        var selected = false;

        // When
        var load = () => new ReleasePairAssetStore(client).GetOrCreateAsync(current, _ =>
        {
            selected = true;
            return Task.FromResult(pair);
        }, TestContext.Current.CancellationToken);

        // Then
        await load.Should().ThrowAsync<InvalidDataException>();
        selected.Should().BeFalse();
        methods.Should().HaveCount(2).And.OnlyContain(static method => method == HttpMethod.Get);
    }
}
