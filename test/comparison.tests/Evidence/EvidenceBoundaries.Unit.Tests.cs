using System.Collections.Immutable;
using System.Text.Json.Nodes;
using AwesomeAssertions;

namespace Specht.Comparison.Tests.Evidence;

[Trait("Tier", "Unit")]
public sealed class EvidenceBoundariesUnitTests
{
    [Fact]
    public void DifferentInputs_WhenPairing_ShouldRetainBothParameterCasesAsNotComparable()
    {
        // Given
        MeasurementInventory previous = new MeasurementInventoryFixture();
        var original = previous.Cases[0];
        var current = previous with
        {
            Cases = [original with { Case = original.Case with
            { Parameters = original.Case.Parameters.SetItem("Specifications", new ParameterValue("number", "10")) } }]
        };
        SessionManifest session = new SessionManifestFixture();

        // When
        var pairs = CasePairing.Pair(previous, current, session);

        // Then
        pairs.Should().HaveCount(2).And.OnlyContain(static p => p.Status == PairingStatus.NotComparable);
        pairs.Should().OnlyContain(static p => p.Reason!.Contains("input parameters", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(true, PairingStatus.Added)]
    [InlineData(false, PairingStatus.Removed)]
    public void OneParameterCaseAddedOrRemoved_WhenPairing_ShouldKeepItsOneSidedStatus(bool added, PairingStatus status)
    {
        // Given
        MeasurementInventory smaller = new MeasurementInventoryFixture();
        var original = smaller.Cases[0];
        var extra = original with
        {
            Case = original.Case with
            { Parameters = original.Case.Parameters.SetItem("Specifications", new ParameterValue("number", "10")) }
        };
        var larger = smaller with { Cases = smaller.Cases.Add(extra) };
        SessionManifest session = new SessionManifestFixture();

        // When
        var pairs = CasePairing.Pair(added ? smaller : larger, added ? larger : smaller, session);

        // Then
        pairs.Should().HaveCount(2);
        pairs.Should().ContainSingle(static pair => pair.Status == PairingStatus.Comparable);
        pairs.Should().ContainSingle(pair => pair.Status == status);
    }

    [Fact]
    public void ExpectedCaseWithNoResult_WhenPairing_ShouldKeepTheMissingRequiredMetrics()
    {
        // Given
        MeasurementInventory inventory = new MeasurementInventoryFixture();
        SessionManifest session = new SessionManifestFixture();
        session = session with { Previous = session.Previous with { ExpectedCases = [inventory.Cases[0].Case] } };

        // When
        var pairs = CasePairing.Pair(inventory with { Cases = [] }, inventory, session);

        // Then
        var pair = pairs.Should().ContainSingle().Subject;
        pair.Status.Should().Be(PairingStatus.InsufficientEvidence);
        pair.Metrics.Should().HaveCount(2).And.OnlyContain(static p => p.Status == PairingStatus.InsufficientEvidence);
    }

    [Fact]
    public void ConfidenceIntervalCrossingZero_WhenReading_ShouldAcceptFiniteBoundsAroundAPositiveMean()
    {
        // Given
        var json = JsonNode.Parse(ExportData.FullJson)!;
        json["Benchmarks"]![0]!["Statistics"]!["ConfidenceInterval"]!["Lower"] = -2;
        SessionManifest session = new SessionManifestFixture();

        // When
        var inventory = BenchmarkEvidenceReader.Read(json.ToJsonString(), "current/full.json", session.Current, []);

        // Then
        inventory.Cases[0].Observations.Single(static x => x.Metric == MetricKind.Time).InvalidReason.Should().BeNull();
    }

    [Fact]
    public void DuplicateJsonFields_WhenReading_ShouldRefuseAmbiguousEvidence()
    {
        // Given
        var json = ExportData.FullJson.Replace("\"Mean\": 100", "\"Mean\": 100, \"Mean\": 200", StringComparison.Ordinal);
        SessionManifest session = new SessionManifestFixture();

        // When
        var inventory = BenchmarkEvidenceReader.Read(json, "current/full.json", session.Current, []);

        // Then
        inventory.Known.Should().BeFalse();
        inventory.Diagnostics.Should().NotBeEmpty();
    }

    [Fact]
    public void ReorderedExpectedCases_WhenEncoded_ShouldHaveTheSameSessionDigest()
    {
        // Given
        WorkloadCase workload = new WorkloadCaseFixture();
        var second = workload with { Parameters = workload.Parameters.SetItem("Specifications", new ParameterValue("number", "10")) };
        SessionManifest session = new SessionManifestFixture();
        var a = session with { Current = session.Current with { ExpectedCases = [workload, second] } };
        var b = session with { Current = session.Current with { ExpectedCases = [second, workload] } };

        // When / Then
        ComparisonJson.Digest(a).Should().Be(ComparisonJson.Digest(b));
    }

    [Fact]
    public void DuplicateExpectedCases_WhenReadingSession_ShouldRejectTheAmbiguousInventory()
    {
        // Given
        WorkloadCase workload = new WorkloadCaseFixture();
        SessionManifest session = new SessionManifestFixture();
        session = session with { Current = session.Current with { ExpectedCases = [workload, workload] } };

        // When
        var read = () => ComparisonJson.ReadSession(ComparisonJson.Write(session));

        // Then
        read.Should().Throw<InvalidDataException>().WithMessage("*duplicated*");
    }

    [Fact]
    public void IncompatibleObserver_WhenPairing_ShouldNeverSubstituteChildMemoryForHarnessAllocation()
    {
        // Given
        MeasurementInventory previous = new MeasurementInventoryFixture();
        var original = previous.Cases[0];
        var current = previous with
        {
            Cases = [original with { Observations = [original.Observations[0],
            original.Observations[1] with { Observer = "child-tool-process" }] }]
        };
        SessionManifest session = new SessionManifestFixture();

        // When
        var pair = CasePairing.Pair(previous, current, session).Single();

        // Then
        pair.Metrics.Single(static p => p.Metric == MetricKind.ManagedAllocation).Status.Should().Be(PairingStatus.NotComparable);
        pair.Metrics.Single(static p => p.Metric == MetricKind.Time).Status.Should().Be(PairingStatus.Comparable);
    }

    [Theory]
    [InlineData("missing confidence")]
    [InlineData("missing runtime")]
    [InlineData("unknown mapping")]
    public void UnknownConditions_WhenPairing_ShouldDistinguishInvalidTimingFromUnknownEquivalence(string gap)
    {
        // Given
        MeasurementInventory inventory = new MeasurementInventoryFixture();
        SessionManifest session = new SessionManifestFixture();
        var current = inventory;
        if (gap == "missing confidence") current = inventory with
        {
            Cases = [inventory.Cases[0] with
            { Observations = [inventory.Cases[0].Observations[0] with { Interval = null }, inventory.Cases[0].Observations[1]] }]
        };
        if (gap == "unknown mapping") current = inventory with { Cases = [inventory.Cases[0] with { IdentityKnown = false }] };
        if (gap == "missing runtime") session = session with
        {
            Current = session.Current with
            { Conditions = session.Current.Conditions with { Runtime = null } }
        };

        // When
        var pair = CasePairing.Pair(inventory, current, session).Single();

        // Then
        pair.Metrics.Single(static p => p.Metric == MetricKind.Time).Status.Should().Be(gap == "missing confidence"
            ? PairingStatus.InsufficientEvidence : PairingStatus.NotComparable);
    }
}
