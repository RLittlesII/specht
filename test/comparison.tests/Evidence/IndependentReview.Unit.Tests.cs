using System.Collections.Immutable;
using AwesomeAssertions;
using Specht.Comparison.Tests.Selection;

namespace Specht.Comparison.Tests.Evidence;

[Trait("Tier", "Unit")]
public sealed class IndependentReviewUnitTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RenamedUnmappedRecords_WhenPairing_ShouldNotInferAddedOrRemovedIdentity(bool currentMapped)
    {
        // Given
        SessionManifest session = new SessionManifestFixture();
        var previous = BenchmarkEvidenceReader.Read(ExportData.FullJson, "previous/full.json", session.Previous, []);
        var renamed = ExportData.FullJson.Replace("Synthetic.Check(1)", "Renamed.Check(1)", StringComparison.Ordinal);
        WorkloadCase workload = new WorkloadCaseFixture();
        var mappings = currentMapped
            ? ImmutableArray.Create(new IdentityMapping("Renamed.Check(1)", session.Current.ProductCommit,
                "Check.cs", new string('d', 64), workload)) : [];
        var current = BenchmarkEvidenceReader.Read(renamed, "current/full.json", session.Current, mappings);

        // When
        var pairs = CasePairing.Pair(previous, current, session);

        // Then
        pairs.Should().HaveCount(2).And.OnlyContain(static pair => pair.Status == PairingStatus.NotComparable);
        pairs.Should().OnlyContain(static pair => pair.Reason!.Contains("identity", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ExpectedAllocationOmittedByObservedContract_WhenPairing_ShouldRetainTheRequiredMetric()
    {
        // Given
        MeasurementInventory inventory = new MeasurementInventoryFixture();
        var original = inventory.Cases[0];
        var timingOnly = inventory with
        {
            Cases = [original with
        {
            Case = original.Case with { RequiredMetrics = [MetricKind.Time] },
            Observations = [original.Observations[0]],
        }]
        };
        SessionManifest session = new SessionManifestFixture();
        session = session with
        {
            Previous = session.Previous with { ExpectedCases = [original.Case] },
            Current = session.Current with { ExpectedCases = [original.Case] },
        };

        // When
        var pair = CasePairing.Pair(timingOnly, timingOnly, session).Single();

        // Then
        pair.Status.Should().Be(PairingStatus.InsufficientEvidence);
        pair.Metrics.Single(static metric => metric.Metric == MetricKind.ManagedAllocation).Status
            .Should().Be(PairingStatus.InsufficientEvidence);
        pair.Metrics.Single(static metric => metric.Metric == MetricKind.Time).Status.Should().Be(PairingStatus.Comparable);
    }

    [Theory]
    [InlineData("operation revision")]
    [InlineData("fixture id")]
    [InlineData("fixture revision")]
    [InlineData("boundary")]
    public void ExpectedContractContradictsObservedCase_WhenPairing_ShouldExposeTheContractConflict(string field)
    {
        // Given
        MeasurementInventory inventory = new MeasurementInventoryFixture();
        var original = inventory.Cases[0].Case;
        var expected = field switch
        {
            "operation revision" => original with { OperationRevision = "2" },
            "fixture id" => original with { FixtureId = "other" },
            "fixture revision" => original with { FixtureRevision = "2" },
            _ => original with { Boundary = "other" },
        };
        SessionManifest session = new SessionManifestFixture();
        session = session with { Current = session.Current with { ExpectedCases = [expected] } };

        // When
        var pair = CasePairing.Pair(inventory, inventory, session).Single();

        // Then
        pair.Status.Should().Be(PairingStatus.NotComparable);
        pair.Reason.Should().Contain(field.Split(' ')[0]);
        pair.Metrics.Should().OnlyContain(static metric => metric.Status == PairingStatus.NotComparable);
    }

    [Theory]
    [InlineData("fixture")]
    [InlineData("boundary")]
    public void SameKeyRecordsWithConflictingContracts_WhenPairing_ShouldExposeTheConflict(string field)
    {
        // Given
        MeasurementInventory inventory = new MeasurementInventoryFixture();
        var original = inventory.Cases[0];
        var conflicting = field == "fixture" ? original.Case with { FixtureRevision = "2" }
            : original.Case with { Boundary = "other" };
        var split = inventory with
        {
            Cases = [
            original with { Case = original.Case with { RequiredMetrics = [MetricKind.Time] }, Observations = [original.Observations[0]] },
            original with { Case = conflicting with { RequiredMetrics = [MetricKind.ManagedAllocation] }, Observations = [original.Observations[1]] },
        ]
        };
        SessionManifest session = new SessionManifestFixture();

        // When
        var pair = CasePairing.Pair(inventory, split, session).Single();

        // Then
        pair.Status.Should().Be(PairingStatus.NotComparable);
        pair.Reason.Should().Contain(field);
        pair.Metrics.Should().HaveCount(2).And.OnlyContain(static metric => metric.Status == PairingStatus.NotComparable);
    }

    [Fact]
    public void SameKeyRecordsWithSplitRequirements_WhenPairing_ShouldRetainMissingAllocation()
    {
        // Given
        MeasurementInventory inventory = new MeasurementInventoryFixture();
        var original = inventory.Cases[0];
        var split = inventory with
        {
            Cases = [
            original with { Case = original.Case with { RequiredMetrics = [MetricKind.Time] }, Observations = [original.Observations[0]] },
            original with { Case = original.Case with { RequiredMetrics = [MetricKind.ManagedAllocation] }, Observations = [] },
        ]
        };
        SessionManifest session = new SessionManifestFixture();

        // When
        var pair = CasePairing.Pair(split, split, session).Single();

        // Then
        pair.Status.Should().Be(PairingStatus.InsufficientEvidence);
        pair.Metrics.Single(static metric => metric.Metric == MetricKind.ManagedAllocation).Status
            .Should().Be(PairingStatus.InsufficientEvidence);
        pair.Metrics.Single(static metric => metric.Metric == MetricKind.Time).Status.Should().Be(PairingStatus.Comparable);
    }

    [Fact]
    public void SavedPairNamesCurrentReleaseAsEarlierPredecessor_WhenRead_ShouldRejectItsImmutableIdentity()
    {
        // Given
        ReleaseRecord current = new ReleaseRecordFixture().WithId(20);
        var pair = ReleaseSelection.Select(current, new ReleaseCatalog([], true, null), "run-1");
        pair = pair with { Previous = current with { PublishedAt = current.PublishedAt!.Value.AddDays(-1) } };

        // When
        var read = () => ComparisonJson.ReadPair(ComparisonJson.Write(pair), current);

        // Then
        read.Should().Throw<InvalidDataException>();
    }
}
