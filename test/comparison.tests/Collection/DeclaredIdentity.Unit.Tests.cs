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

    [Theory]
    [ClassData(typeof(UnreadableDeclarations))]
    public void DeclarationTheReaderCannotRead_WhenRead_ShouldMapNothingFromIt(string source)
    {
        // Given
        var sources = new Dictionary<string, byte[]>(StringComparer.Ordinal)
        {
            [CheckFile] = Encoding.UTF8.GetBytes(source),
            [StartupFile] = Encoding.UTF8.GetBytes(SyntheticBenchmarks.Startup),
        };
        using var assembly = new MemoryStream(SyntheticBenchmarks.Compile(($"checkout/{CheckFile}", source),
            ($"checkout/{StartupFile}", SyntheticBenchmarks.Startup)));

        // When
        var mappings = DeclaredIdentity.Read(assembly, new string('1', 40), "checkout", path => sources.GetValueOrDefault(path));

        // Then
        mappings.Select(static mapping => mapping.FullName).Should().Equal("Specht.Benchmarks.Startup.ColdStartBenchmarks.Start");
    }

    [Theory]
    [ClassData(typeof(SeveralParameters))]
    public void SeveralParameters_WhenRead_ShouldNameEachCombinationFieldsBeforePropertiesAsBenchmarkDotNetExportsIt(string source, string[] expected)
    {
        // Given
        var sources = new Dictionary<string, byte[]>(StringComparer.Ordinal) { [CheckFile] = Encoding.UTF8.GetBytes(source) };
        using var assembly = new MemoryStream(SyntheticBenchmarks.Compile(($"checkout/{CheckFile}", source)));

        // When
        var mappings = DeclaredIdentity.Read(assembly, new string('1', 40), "checkout", path => sources.GetValueOrDefault(path));

        // Then
        mappings.Select(static mapping => mapping.FullName).Should().BeEquivalentTo(expected);
    }

    public sealed class UnverifiableDeclarations : TheoryData<string, bool>
    {
        public UnverifiableDeclarations()
        {
            Add($"elsewhere/{CheckFile}", true);
            Add($"checkout/{CheckFile}", false);
        }
    }

    public sealed class UnreadableDeclarations : TheoryData<string>
    {
        public UnreadableDeclarations()
        {
            Add("""
                namespace Specht.Benchmarks.Engine
                {
                    public class CheckBenchmarks
                    {
                        [BenchmarkDotNet.Attributes.Params(1.5, 2.5)]
                        public double Scale { get; set; }

                        [Specht.Benchmarks.WorkloadIdentity("check", "1", "synthetic-tree", "1", "engine-run")]
                        public void Check() { }
                    }
                }
                """);
            Add("""
                namespace Specht.Benchmarks.Engine
                {
                    public class CheckBenchmarks
                    {
                        [BenchmarkDotNet.Attributes.Params("small", "large")]
                        public string Size { get; set; }

                        [Specht.Benchmarks.WorkloadIdentity("check", "1", "synthetic-tree", "1", "engine-run")]
                        public void Check() { }
                    }
                }
                """);
            Add("""
                namespace Specht.Benchmarks.Engine
                {
                    public class CheckBenchmarks
                    {
                        [BenchmarkDotNet.Attributes.Arguments(1)]
                        [Specht.Benchmarks.WorkloadIdentity("check", "1", "synthetic-tree", "1", "engine-run")]
                        public void Check(int specifications) { }
                    }
                }
                """);
            Add("""
                namespace Specht.Benchmarks.Engine
                {
                    public class CheckBenchmarks
                    {
                        public static int[] Counts => new[] { 1, 10 };

                        [BenchmarkDotNet.Attributes.ArgumentsSource("Counts")]
                        [Specht.Benchmarks.WorkloadIdentity("check", "1", "synthetic-tree", "1", "engine-run")]
                        public void Check(int specifications) { }
                    }
                }
                """);
            Add("""
                namespace Specht.Benchmarks.Engine
                {
                    public class CheckBenchmarks
                    {
                        public static int[] Counts => new[] { 1, 10 };

                        [BenchmarkDotNet.Attributes.ParamsSource("Counts")]
                        public int Specifications { get; set; }

                        [Specht.Benchmarks.WorkloadIdentity("check", "1", "synthetic-tree", "1", "engine-run")]
                        public void Check() { }
                    }
                }
                """);
            Add("""
                namespace Specht.Benchmarks.Engine
                {
                    public class CheckBenchmarks
                    {
                        [BenchmarkDotNet.Attributes.ParamsAllValues]
                        public bool Cached;

                        [Specht.Benchmarks.WorkloadIdentity("check", "1", "synthetic-tree", "1", "engine-run")]
                        public void Check() { }
                    }
                }
                """);
            Add("""
                namespace Specht.Benchmarks.Engine
                {
                    public class Outer
                    {
                        public class CheckBenchmarks
                        {
                            [Specht.Benchmarks.WorkloadIdentity("check", "1", "synthetic-tree", "1", "engine-run")]
                            public void Check() { }
                        }
                    }
                }
                """);
            Add("""
                namespace Specht.Benchmarks.Engine
                {
                    public class CheckBenchmarks<T>
                    {
                        [Specht.Benchmarks.WorkloadIdentity("check", "1", "synthetic-tree", "1", "engine-run")]
                        public void Check() { }
                    }
                }
                """);
            Add("""
                namespace Specht.Benchmarks.Engine
                {
                    public class CheckBenchmarks
                    {
                        [Specht.Benchmarks.WorkloadIdentity("", "1", "synthetic-tree", "1", "engine-run")]
                        public void Check() { }
                    }
                }
                """);
            Add("""
                namespace Specht.Benchmarks.Engine
                {
                    public class CheckBenchmarks
                    {
                        [Specht.Benchmarks.WorkloadIdentity("check", "1", null, "1", "engine-run")]
                        public void Check() { }
                    }
                }
                """);
            Add("""
                namespace Specht.Benchmarks.Engine
                {
                    public class CheckBenchmarks
                    {
                        [Specht.Benchmarks.WorkloadIdentity("check", "1", "synthetic-tree", "1", "engine-run", "")]
                        public void Check() { }
                    }
                }
                """);
            Add("""
                namespace Specht.Benchmarks.Engine
                {
                    public class CheckBenchmarks
                    {
                        [Specht.Benchmarks.WorkloadIdentity("check", "1", "synthetic-tree", "1")]
                        public void Check() { }
                    }
                }
                """);
        }
    }

    public sealed class SeveralParameters : TheoryData<string, string[]>
    {
        public SeveralParameters()
        {
            Add("""
                namespace Specht.Benchmarks.Analysis
                {
                    public class BindingLookupBenchmarks
                    {
                        [BenchmarkDotNet.Attributes.Params(1, 40, 160)]
                        public int Records { get; set; }

                        [BenchmarkDotNet.Attributes.Params(1, 40)]
                        public int Bindings { get; set; }

                        [Specht.Benchmarks.WorkloadIdentity("resolve-bindings", "1", "synthetic-binding-catalog", "1", "in-process")]
                        public int ResolveBindings() => 0;
                    }
                }
                """,
                [
                    "Specht.Benchmarks.Analysis.BindingLookupBenchmarks.ResolveBindings(Records: 1, Bindings: 1)",
                    "Specht.Benchmarks.Analysis.BindingLookupBenchmarks.ResolveBindings(Records: 1, Bindings: 40)",
                    "Specht.Benchmarks.Analysis.BindingLookupBenchmarks.ResolveBindings(Records: 40, Bindings: 1)",
                    "Specht.Benchmarks.Analysis.BindingLookupBenchmarks.ResolveBindings(Records: 40, Bindings: 40)",
                    "Specht.Benchmarks.Analysis.BindingLookupBenchmarks.ResolveBindings(Records: 160, Bindings: 1)",
                    "Specht.Benchmarks.Analysis.BindingLookupBenchmarks.ResolveBindings(Records: 160, Bindings: 40)",
                ]);
            Add("""
                namespace Specht.Benchmarks.Analysis
                {
                    public class BindingLookupBenchmarks
                    {
                        [BenchmarkDotNet.Attributes.Params(1, 40)]
                        public int Bindings { get; set; }

                        [BenchmarkDotNet.Attributes.Params(160)]
                        public int Records;

                        [Specht.Benchmarks.WorkloadIdentity("resolve-bindings", "1", "synthetic-binding-catalog", "1", "in-process")]
                        public int ResolveBindings() => 0;
                    }
                }
                """,
                [
                    "Specht.Benchmarks.Analysis.BindingLookupBenchmarks.ResolveBindings(Records: 160, Bindings: 1)",
                    "Specht.Benchmarks.Analysis.BindingLookupBenchmarks.ResolveBindings(Records: 160, Bindings: 40)",
                ]);
        }
    }

    private const string CheckFile = ".performance/benchmarks/Check/CheckBenchmarks.cs";
    private const string StartupFile = ".performance/benchmarks/Startup/ColdStartBenchmarks.cs";
}
