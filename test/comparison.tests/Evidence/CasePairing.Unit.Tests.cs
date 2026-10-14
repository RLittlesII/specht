using System.Collections.Immutable;
using AwesomeAssertions;

namespace Specht.Comparison.Tests.Evidence;

[Trait("Tier", "Unit")]
public sealed class CasePairingUnitTests
{
    [Theory]
    [InlineData(true, "Added")]
    [InlineData(false, "Removed")]
    public void KnownOneSidedWorkload_WhenPairing_ShouldRetainItWithoutJudgment(bool added, string status)
    {
        // Given
        var previous = ((MeasurementInventory)new MeasurementInventoryFixture());
        var current = ((MeasurementInventory)new MeasurementInventoryFixture());
        if (added) previous = previous with { Cases = [] };
        else current = current with { Cases = [] };

        // When
        var pairs = CasePairing.Pair(previous, current, ((SessionManifest)new SessionManifestFixture()));

        // Then
        pairs.Should().ContainSingle().Which.Status.ToString().Should().Be(status);
    }

    [Theory]
    [InlineData("operating system")]
    [InlineData("runtime")]
    [InlineData("architecture")]
    [InlineData("toolchain")]
    [InlineData("garbage collection")]
    [InlineData("job")]
    [InlineData("fixture")]
    [InlineData("operation revision")]
    [InlineData("boundary")]
    [InlineData("observer")]
    [InlineData("confidence")]
    [InlineData("unknown identity")]
    public void ChangedOrUnknownEquivalence_WhenPairing_ShouldNameTheCondition(string condition)
    {
        // Given
        var previous = ((MeasurementInventory)new MeasurementInventoryFixture());
        var current = ((MeasurementInventory)new MeasurementInventoryFixture());
        var session = ((SessionManifest)new SessionManifestFixture());
        (current, session) = EvidenceData.Change(condition, current, session);

        // When
        var pair = CasePairing.Pair(previous, current, session).Single();

        // Then
        pair.Status.Should().Be(PairingStatus.NotComparable);
        string.Join(" ", pair.Metrics.Select(static x => x.Reason).Append(pair.Reason)).ToLowerInvariant().Should().Contain(condition);
    }

    [Fact]
    public void MatchingCases_WhenPairing_ShouldKeepSeparateSizesAndBothMetrics()
    {
        // Given
        var first = ((MeasurementInventory)new MeasurementInventoryFixture());
        var larger = first.Cases[0] with
        {
            Case = first.Cases[0].Case with
            {
                Parameters = ImmutableSortedDictionary<string, ParameterValue>.Empty.Add("Specifications", new ParameterValue("number", "10")),
            },
        };
        var inventory = first with { Cases = first.Cases.Add(larger) };

        // When
        var pairs = CasePairing.Pair(inventory, inventory, ((SessionManifest)new SessionManifestFixture()));

        // Then
        pairs.Should().HaveCount(2).And.OnlyContain(static x => x.Status == PairingStatus.Comparable);
        pairs.Should().OnlyContain(static x => x.Metrics.Length == 2);
        pairs.Select(static x => x.Key).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void UnavailableInventory_WhenPairing_ShouldNotInventAddedOrRemovedCases()
    {
        // Given
        var previous = new MeasurementInventory(false, [], ["Discovery failed"]);

        // When
        var pairs = CasePairing.Pair(previous, ((MeasurementInventory)new MeasurementInventoryFixture()), ((SessionManifest)new SessionManifestFixture()));

        // Then
        pairs.Should().ContainSingle().Which.Status.Should().Be(PairingStatus.InsufficientEvidence);
    }

    [Fact]
    public void InvalidTimingWithDifferentConditions_WhenPairing_ShouldValidateBeforeEquivalenceAndKeepAllocation()
    {
        // Given
        var inventory = ((MeasurementInventory)new MeasurementInventoryFixture());
        var evidence = inventory.Cases[0];
        var invalid = evidence.Observations[0] with { Value = null, InvalidReason = "Invalid time" };
        var current = inventory with { Cases = [evidence with { Observations = [invalid, evidence.Observations[1]] }] };
        var session = ((SessionManifest)new SessionManifestFixture());
        session = session with { Current = session.Current with { Conditions = session.Current.Conditions with { Runtime = "other" } } };

        // When
        var pair = CasePairing.Pair(inventory, current, session).Single();

        // Then
        pair.Metrics.Single(static x => x.Metric == MetricKind.Time).Status.Should().Be(PairingStatus.InsufficientEvidence);
        pair.Metrics.Single(static x => x.Metric == MetricKind.ManagedAllocation).Status.Should().Be(PairingStatus.NotComparable);
    }

    [Fact]
    public void DuplicateRecords_WhenPairing_ShouldKeepAllSourceLocatorsAndInvalidateOnlyThatMetric()
    {
        // Given
        var inventory = ((MeasurementInventory)new MeasurementInventoryFixture());
        var first = inventory.Cases[0];
        var duplicate = first.Observations[0] with { Locator = "/Benchmarks/1/Statistics" };
        var current = inventory with { Cases = [first with { Observations = first.Observations.Add(duplicate) }] };

        // When
        var pair = CasePairing.Pair(inventory, current, ((SessionManifest)new SessionManifestFixture())).Single();

        // Then
        var timing = pair.Metrics.Single(static x => x.Metric == MetricKind.Time);
        timing.Status.Should().Be(PairingStatus.InsufficientEvidence);
        timing.Current.Select(static x => x.Locator).Should().Equal("/Benchmarks/0/Statistics", "/Benchmarks/1/Statistics");
        pair.Metrics.Single(static x => x.Metric == MetricKind.ManagedAllocation).Status.Should().Be(PairingStatus.Comparable);
    }
}
