using System.Collections.Immutable;
using AwesomeAssertions;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;
using Specht.Comparison.Model;
using Specht.Comparison.Selection;

namespace Specht.Comparison.Tests.Selection;

[Trait("Tier", "Unit")]
public sealed class ReleaseSelectionUnitTests
{
    [Fact]
    public void MixedPublicationOrder_WhenSelecting_ShouldChooseImmediateEligiblePredecessor()
    {
        // Given
        ReleaseRecord current = new Fixtures.ReleaseRecordFixture().WithId(20).WithTag("v2");
        ReleaseRecord stable = new Fixtures.ReleaseRecordFixture().WithId(10).WithTag("v99").WithPublishedAt(current.PublishedAt!.Value.AddDays(-2));
        ReleaseRecord prerelease = new Fixtures.ReleaseRecordFixture().WithId(15).WithTag("preview").WithPrerelease(true)
            .WithPublishedAt(current.PublishedAt!.Value.AddDays(-1));
        ReleaseRecord draft = new Fixtures.ReleaseRecordFixture().WithId(19).WithDraft(true);
        ReleaseRecord unpublished = new Fixtures.ReleaseRecordFixture().WithId(18).WithPublishedAt(null);
        var catalog = new ReleaseCatalog([current, draft, stable, unpublished, prerelease], true, null);

        // When
        var pair = ReleaseSelection.Select(current, catalog, "run-1");

        // Then
        pair.Previous.Should().Be(prerelease);
        pair.Current.Should().Be(current);
        pair.CatalogDigest.Should().HaveLength(64);
    }

    [Fact]
    public void EqualPublicationTimes_WhenSelecting_ShouldUseImmutableIdAndIgnoreLaterIds()
    {
        // Given
        ReleaseRecord current = new Fixtures.ReleaseRecordFixture().WithId(20);
        ReleaseRecord lower = new Fixtures.ReleaseRecordFixture().WithId(10);
        ReleaseRecord higher = new Fixtures.ReleaseRecordFixture().WithId(19);
        ReleaseRecord future = new Fixtures.ReleaseRecordFixture().WithId(21);
        var catalog = new ReleaseCatalog([future, lower, current, higher], true, null);

        // When
        var pair = ReleaseSelection.Select(current, catalog, "run-1");

        // Then
        pair.Previous.Should().Be(higher);
        ReleaseSelection.Select(current, catalog with { Releases = catalog.Releases.Reverse().ToImmutableArray() }, "run-1")
            .CatalogDigest.Should().Be(pair.CatalogDigest);
    }

    [Fact]
    public void UnresolvedImmediatePredecessor_WhenSelecting_ShouldRetainItInsteadOfAnOlderUsableRelease()
    {
        // Given
        ReleaseRecord current = new Fixtures.ReleaseRecordFixture().WithId(20);
        ReleaseRecord old = new Fixtures.ReleaseRecordFixture().WithId(10);
        ReleaseRecord unresolved = new Fixtures.ReleaseRecordFixture().WithId(19).WithCommit(null).WithResolutionFailure("Tag unavailable");

        // When
        var pair = ReleaseSelection.Select(current, new ReleaseCatalog([old, unresolved], true, null), "run-1");

        // Then
        pair.Previous.Should().Be(unresolved);
        pair.Previous!.ResolutionFailure.Should().Be("Tag unavailable");
    }

    [Fact]
    public void FirstPublication_WhenSelecting_ShouldKeepAnExplicitNullPredecessor()
    {
        // Given
        ReleaseRecord current = new Fixtures.ReleaseRecordFixture();

        // When
        var pair = ReleaseSelection.Select(current, new ReleaseCatalog([], true, null), "run-1");

        // Then
        pair.Previous.Should().BeNull();
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void IncompleteOrAmbiguousCatalog_WhenSelecting_ShouldRefuseSelection(bool complete, bool duplicate)
    {
        // Given
        ReleaseRecord current = new Fixtures.ReleaseRecordFixture();
        var catalog = new ReleaseCatalog(duplicate ? [current, current with { Tag = "other" }] : [], complete, null);

        // When
        var select = () => ReleaseSelection.Select(current, catalog, "run-1");

        // Then
        select.Should().Throw<InvalidDataException>();
    }
}
