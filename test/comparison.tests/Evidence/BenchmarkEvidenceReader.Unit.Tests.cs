using System.Collections.Immutable;
using System.Text.Json.Nodes;
using AwesomeAssertions;

namespace Specht.Comparison.Tests.Evidence;

[Trait("Tier", "Unit")]
public sealed class BenchmarkEvidenceReaderUnitTests
{
    [Fact]
    public void RecognisedExport_WhenReading_ShouldDecodeUnitsConfidenceAndMappedIdentity()
    {
        // Given
        var side = ((SessionManifest)new SessionManifestFixture()).Current;
        var mapping = new IdentityMapping(
            "Synthetic.Check(1)",
            side.ProductCommit,
            "Check.cs",
            new string('d', 64),
            ((MeasurementInventory)new MeasurementInventoryFixture()).Cases[0].Case);

        // When
        var inventory = BenchmarkEvidenceReader.Read(ExportData.FullJson, "current/full.json", side, [mapping]);

        // Then
        inventory.Known.Should().BeTrue();
        var evidence = inventory.Cases.Should().ContainSingle().Subject;
        evidence.IdentityKnown.Should().BeTrue();
        evidence.Case.OperationId.Should().Be("check");
        var timing = evidence.Observations.Single(static x => x.Metric == MetricKind.Time);
        timing.Value.Should().Be(100);
        timing.Interval!.ConfidenceLevel.Should().Be(0.999m);
        timing.Interval.Lower.Should().Be(98);
        timing.Unit.Should().Be("ns");
        evidence.Observations.Single(static x => x.Metric == MetricKind.ManagedAllocation).Value.Should().Be(32);
    }

    [Theory]
    [InlineData("0.9.9")]
    [InlineData("future")]
    public void UnsupportedExporter_WhenReading_ShouldReturnUnavailableInsteadOfAnEmptyKnownInventory(string version)
    {
        // Given
        var side = ((SessionManifest)new SessionManifestFixture()).Current with { ExporterVersion = version };

        // When
        var inventory = BenchmarkEvidenceReader.Read(ExportData.FullJson, "current/full.json", side, []);

        // Then
        inventory.Known.Should().BeFalse();
        inventory.Diagnostics.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("mean")]
    [InlineData("interval")]
    [InlineData("confidence")]
    [InlineData("negative allocation")]
    [InlineData("missing allocation")]
    public void InvalidMetric_WhenReading_ShouldRetainTheCaseAndIndependentMetric(string invalid)
    {
        // Given
        var json = JsonNode.Parse(ExportData.FullJson)!;
        var benchmark = json["Benchmarks"]![0]!;
        switch (invalid)
        {
            case "mean": benchmark["Statistics"]!["Mean"] = "NaN"; break;
            case "interval": benchmark["Statistics"]!["ConfidenceInterval"]!["Lower"] = 101; break;
            case "confidence": benchmark["Statistics"]!["ConfidenceInterval"]!["Level"] = 80; break;
            case "negative allocation": benchmark["Memory"]!["BytesAllocatedPerOperation"] = -1; break;
            case "missing allocation": benchmark["Memory"] = null; break;
        }
        var side = ((SessionManifest)new SessionManifestFixture()).Current;

        // When
        var inventory = BenchmarkEvidenceReader.Read(json.ToJsonString(), "current/full.json", side, []);

        // Then
        var observations = inventory.Cases.Should().ContainSingle().Subject.Observations;
        observations.Should().ContainSingle(static x => x.InvalidReason != null);
        observations.Should().ContainSingle(static x => x.InvalidReason == null);
    }

    [Fact]
    public void SameNameWithoutAuditedMapping_WhenReading_ShouldKeepUnknownIdentity()
    {
        // Given
        var side = ((SessionManifest)new SessionManifestFixture()).Current;

        // When
        var inventory = BenchmarkEvidenceReader.Read(ExportData.FullJson, "current/full.json", side, []);

        // Then
        inventory.Cases.Should().ContainSingle().Which.IdentityKnown.Should().BeFalse();
    }

    [Fact]
    public void DuplicateRows_WhenReading_ShouldRetainBothLocators()
    {
        // Given
        var json = JsonNode.Parse(ExportData.FullJson)!;
        json["Benchmarks"]!.AsArray().Add(json["Benchmarks"]![0]!.DeepClone());

        // When
        var inventory = BenchmarkEvidenceReader.Read(json.ToJsonString(), "current/full.json", ((SessionManifest)new SessionManifestFixture()).Current, []);

        // Then
        inventory.Cases.SelectMany(static x => x.Observations).Where(static x => x.Metric == MetricKind.Time)
            .Select(static x => x.Locator).Should().Equal("/Benchmarks/0/Statistics", "/Benchmarks/1/Statistics");
    }
}
