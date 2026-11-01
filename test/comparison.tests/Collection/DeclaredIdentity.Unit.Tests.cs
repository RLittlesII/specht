using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using AwesomeAssertions;
using Specht.Comparison.Tests.Evidence;

namespace Specht.Comparison.Tests.Collection;

[Trait("Tier", "Unit")]
public sealed class DeclaredIdentityUnitTests
{
    [Fact]
    public void DeclaredBenchmarks_WhenRead_ShouldMapEachDeclaredNameToItsWorkloadAtThatRevisionWithItsFileAndDigest()
    {
        // Given
        var check = Encoding.UTF8.GetBytes(SyntheticBenchmarks.Check);
        var startup = Encoding.UTF8.GetBytes(SyntheticBenchmarks.Startup);
        var sources = new Dictionary<string, byte[]>(StringComparer.Ordinal)
        {
            [CheckFile] = check,
            [StartupFile] = startup,
        };
        using var assembly = new MemoryStream(SyntheticBenchmarks.Compile(($"checkout/{CheckFile}", SyntheticBenchmarks.Check),
            ($"checkout/{StartupFile}", SyntheticBenchmarks.Startup)));

        // When
        var mappings = DeclaredIdentity.Read(assembly, new string('1', 40), "checkout", path => sources.GetValueOrDefault(path));

        // Then
        var checkDigest = Convert.ToHexStringLower(SHA256.HashData(check));
        var startupDigest = Convert.ToHexStringLower(SHA256.HashData(startup));
        mappings.Select(static mapping => (mapping.FullName, mapping.ProductCommit, mapping.SourceFile, mapping.SourceDigest)).Should().BeEquivalentTo(
        new[]
        {
            ("Specht.Benchmarks.Engine.CheckBenchmarks.Check(Specifications: 1)", new string('1', 40), CheckFile, checkDigest),
            ("Specht.Benchmarks.Engine.CheckBenchmarks.Check(Specifications: 10)", new string('1', 40), CheckFile, checkDigest),
            ("Specht.Benchmarks.Startup.ColdStartBenchmarks.Start", new string('1', 40), StartupFile, startupDigest),
        });
        mappings.Select(static mapping => mapping.Case).Should().BeEquivalentTo(
        new WorkloadCase[]
        {
            new WorkloadCaseFixture(),
            new WorkloadCaseFixture().WithParameters(
                ImmutableSortedDictionary<string, ParameterValue>.Empty.Add("Specifications", new ParameterValue("number", "10"))),
            new WorkloadCaseFixture().WithOperationId("start").WithOperationRevision("2").WithFixtureId("empty-tree").WithFixtureRevision("3")
                .WithBoundary("process-start").WithParameters(ImmutableSortedDictionary<string, ParameterValue>.Empty),
        }, static options => options.Excluding(static workload => workload.RequiredMetrics));
    }

    [Fact]
    public void BenchmarksDeclaringNoWorkloadIdentity_WhenRead_ShouldGiveNoMappings()
    {
        // Given
        var sources = new Dictionary<string, byte[]>(StringComparer.Ordinal) { [CheckFile] = Encoding.UTF8.GetBytes(SyntheticBenchmarks.Undeclared) };
        using var assembly = new MemoryStream(SyntheticBenchmarks.Compile(($"checkout/{CheckFile}", SyntheticBenchmarks.Undeclared)));

        // When
        var mappings = DeclaredIdentity.Read(assembly, new string('1', 40), "checkout", path => sources.GetValueOrDefault(path));

        // Then
        mappings.Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(UnverifiableDeclarations))]
    public void DeclarationWhoseFileIsNotThatRevisions_WhenRead_ShouldNotBeMapped(string compiledPath, bool readable)
    {
        // Given
        var sources = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        if (readable) sources[CheckFile] = Encoding.UTF8.GetBytes(SyntheticBenchmarks.Check);
        using var assembly = new MemoryStream(SyntheticBenchmarks.Compile((compiledPath, SyntheticBenchmarks.Check)));

        // When
        var mappings = DeclaredIdentity.Read(assembly, new string('1', 40), "checkout", path => sources.GetValueOrDefault(path));

        // Then
        mappings.Should().BeEmpty();
    }

    public sealed class UnverifiableDeclarations : TheoryData<string, bool>
    {
        public UnverifiableDeclarations()
        {
            Add($"elsewhere/{CheckFile}", true);
            Add($"checkout/{CheckFile}", false);
        }
    }

    private const string CheckFile = ".performance/benchmarks/Check/CheckBenchmarks.cs";
    private const string StartupFile = ".performance/benchmarks/Startup/ColdStartBenchmarks.cs";
}
