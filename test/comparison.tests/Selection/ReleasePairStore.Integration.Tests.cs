using System.IO.Abstractions;
using AwesomeAssertions;

namespace Specht.Comparison.Tests.Selection;

[Trait("Tier", "Integration")]
public sealed class ReleasePairStoreIntegrationTests
{
    [Fact]
    public async Task SavedPair_WhenCatalogChangesOrTransportExpires_ShouldKeepTheOriginalSelection()
    {
        // Given
        var directory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            ReleaseRecord current = new ReleaseRecordFixture().WithId(20);
            ReleaseRecord previous = new ReleaseRecordFixture().WithId(10);
            var pair = ReleaseSelection.Select(current, new ReleaseCatalog([previous], true, null), "run-1");
            var store = new ReleasePairStore(new FileSystem(), directory);
            await store.GetOrCreateAsync(current, _ => Task.FromResult(pair), TestContext.Current.CancellationToken);

            // When
            var reloaded = await new ReleasePairStore(new FileSystem(), directory).GetOrCreateAsync(current,
                _ => throw new InvalidOperationException("Rerun must not read the catalog"), TestContext.Current.CancellationToken);

            // Then
            reloaded.Should().BeEquivalentTo(pair);
            Directory.GetFiles(directory).Select(Path.GetFileName).Should().Equal("comparison-pair-v1.json");
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }

    [Fact]
    public async Task CompetingCreation_WhenSaving_ShouldReadTheWinnerWithoutClobberingIt()
    {
        // Given
        var directory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            ReleaseRecord current = new ReleaseRecordFixture().WithId(20);
            ReleaseRecord previous = new ReleaseRecordFixture().WithId(10);
            var winner = ReleaseSelection.Select(current, new ReleaseCatalog([], true, null), "winner");
            var loser = ReleaseSelection.Select(current, new ReleaseCatalog([previous], true, null), "loser");
            var store = new ReleasePairStore(new FileSystem(), directory);

            // When
            var actual = await store.GetOrCreateAsync(current, async token =>
            {
                await new ReleasePairStore(new FileSystem(), directory).GetOrCreateAsync(current, _ => Task.FromResult(winner), token);
                return loser;
            }, TestContext.Current.CancellationToken);

            // Then
            actual.SelectingRunId.Should().Be("winner");
            actual.Previous.Should().BeNull();
            Directory.GetFiles(directory).Should().HaveCount(1);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }

    [Theory]
    [InlineData("broken")]
    [InlineData("{\"formatVersion\":99}")]
    public async Task InvalidExistingAsset_WhenLoading_ShouldRefuseToReselect(string json)
    {
        // Given
        var directory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(directory);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(directory, "comparison-pair-v1.json"), json, TestContext.Current.CancellationToken);
            ReleaseRecord current = new ReleaseRecordFixture();
            var store = new ReleasePairStore(new FileSystem(), directory);

            // When
            var load = () => store.GetOrCreateAsync(current, _ => throw new InvalidOperationException("No reselection"));

            // Then
            await load.Should().ThrowAsync<InvalidDataException>();
            (await File.ReadAllTextAsync(Path.Combine(directory, "comparison-pair-v1.json"), TestContext.Current.CancellationToken)).Should().Be(json);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }
}
