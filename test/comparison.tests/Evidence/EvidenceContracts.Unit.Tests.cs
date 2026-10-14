using System.Collections.Immutable;
using System.Globalization;
using AwesomeAssertions;

namespace Specht.Comparison.Tests.Evidence;

[Trait("Tier", "Unit")]
public sealed class EvidenceContractsUnitTests
{
    [Fact]
    public void SessionManifest_WhenEncoded_ShouldRoundTripWithoutAmbientCultureOrDictionaryOrder()
    {
        // Given
        var session = ((SessionManifest)new SessionManifestFixture());
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");

            // When
            var encoded = ComparisonJson.Write(session);
            var actual = ComparisonJson.ReadSession(encoded);

            // Then
            actual.Should().BeEquivalentTo(session);
            ComparisonJson.Write(actual).Should().Be(encoded);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Theory]
    [InlineData("version")]
    [InlineData("commit")]
    [InlineData("pair digest")]
    public void InvalidSessionContract_WhenRead_ShouldRefuseTheEvidence(string invalid)
    {
        // Given
        var session = ((SessionManifest)new SessionManifestFixture());
        session = invalid switch
        {
            "version" => session with { FormatVersion = 2 },
            "commit" => session with { Current = session.Current with { ProductCommit = "branch-name" } },
            _ => session with { PairDigest = "unknown" },
        };

        // When
        var read = () => ComparisonJson.ReadSession(ComparisonJson.Write(session));

        // Then
        read.Should().Throw<InvalidDataException>();
    }

    [Theory]
    [InlineData("1", "1.00", true)]
    [InlineData("1", "1e0", true)]
    [InlineData("1", "10", false)]
    public void NumericParameters_WhenKeyed_ShouldUseTypedCanonicalValues(string first, string second, bool equal)
    {
        // Given
        var workload = ((MeasurementInventory)new MeasurementInventoryFixture()).Cases[0].Case;
        var a = workload with { Parameters = ImmutableSortedDictionary<string, ParameterValue>.Empty.Add("size", new ParameterValue("number", first)) };
        var b = workload with { Parameters = ImmutableSortedDictionary<string, ParameterValue>.Empty.Add("size", new ParameterValue("number", second)) };

        // When
        var same = WorkloadIdentity.Key(a) == WorkloadIdentity.Key(b);

        // Then
        same.Should().Be(equal);
        WorkloadIdentity.Key(a with { Parameters = a.Parameters.SetItem("size", new ParameterValue("string", first)) })
            .Should().NotBe(WorkloadIdentity.Key(a));
    }

    [Theory]
    [InlineData(MetricKind.Time, "ns", 1)]
    [InlineData(MetricKind.Time, "us", 1000)]
    [InlineData(MetricKind.Time, "µs", 1000)]
    [InlineData(MetricKind.Time, "ms", 1000000)]
    [InlineData(MetricKind.Time, "s", 1000000000)]
    [InlineData(MetricKind.ManagedAllocation, "KB", 1000)]
    [InlineData(MetricKind.ManagedAllocation, "KiB", 1024)]
    [InlineData(MetricKind.ManagedAllocation, "MB", 1000000)]
    [InlineData(MetricKind.ManagedAllocation, "MiB", 1048576)]
    public void RecognisedUnits_WhenDecoded_ShouldKeepTheDimensionalScale(MetricKind metric, string unit, int scale)
    {
        // Given / When
        var actual = MetricUnits.Scale(metric, unit);

        // Then
        actual.Should().Be(scale);
        MetricUnits.Scale(metric == MetricKind.Time ? MetricKind.ManagedAllocation : MetricKind.Time, unit).Should().BeNull();
    }

    [Theory]
    [InlineData("rooted")]
    [InlineData("parent traversal")]
    [InlineData("nested traversal")]
    public void UnsafeSourceReference_WhenDecoded_ShouldProduceUnavailableEvidence(string kind)
    {
        // Given
        var source = kind switch
        {
            "rooted" => Path.GetFullPath("report.json"),
            "parent traversal" => "../report.json",
            _ => "reports/../../outside.json",
        };
        SessionManifest session = new SessionManifestFixture();

        // When
        var inventory = BenchmarkEvidenceReader.Read(ExportData.FullJson, source, session.Current, []);

        // Then
        inventory.Known.Should().BeFalse();
        string.Join(" ", inventory.Diagnostics).Should().NotContain(source);
    }
}
