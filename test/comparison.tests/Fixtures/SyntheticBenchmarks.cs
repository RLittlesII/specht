using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Specht.Comparison.Tests.Collection;

internal static class SyntheticBenchmarks
{
    public const string Check = """
        namespace Specht.Benchmarks.Engine
        {
            public class CheckBenchmarks
            {
                [BenchmarkDotNet.Attributes.Params(1, 10)]
                public int Specifications { get; set; }

                [BenchmarkDotNet.Attributes.Benchmark]
                [Specht.Benchmarks.WorkloadIdentity("check", "1", "synthetic-tree", "1", "engine-run")]
                public void Check() { }
            }
        }
        """;

    public const string Startup = """
        namespace Specht.Benchmarks.Startup
        {
            public class ColdStartBenchmarks
            {
                [BenchmarkDotNet.Attributes.Benchmark]
                [Specht.Benchmarks.WorkloadIdentity("start", "2", "empty-tree", "3", "process-start")]
                public void Start() { }

                [BenchmarkDotNet.Attributes.Benchmark]
                public void Unmapped() { }
            }
        }
        """;

    public const string Undeclared = """
        namespace Specht.Benchmarks.Engine
        {
            public class CheckBenchmarks
            {
                [BenchmarkDotNet.Attributes.Params(1, 10)]
                public int Specifications { get; set; }

                [BenchmarkDotNet.Attributes.Benchmark]
                public void Check() { }
            }
        }
        """;

    public static byte[] Compile(params (string Path, string Source)[] files)
    {
        var trees = files.Select(static file => CSharpSyntaxTree.ParseText(file.Source, path: file.Path, encoding: Encoding.UTF8))
            .Append(CSharpSyntaxTree.ParseText(Declarations, path: "declarations/Declarations.cs", encoding: Encoding.UTF8));
        var compilation = CSharpCompilation.Create("specht.benchmarks", trees,
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)], new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        using var assembly = new MemoryStream();
        var emitted = compilation.Emit(assembly);
        if (!emitted.Success)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, emitted.Diagnostics.Select(static diagnostic => diagnostic.ToString())));
        }

        return assembly.ToArray();
    }

    private const string Declarations = """
        namespace Specht.Benchmarks
        {
            [System.AttributeUsage(System.AttributeTargets.Method)]
            public sealed class WorkloadIdentityAttribute : System.Attribute
            {
                public WorkloadIdentityAttribute(string operationId, string operationRevision, string fixtureId, string fixtureRevision,
                    string boundary, [System.Runtime.CompilerServices.CallerFilePath] string sourceFile = "")
                {
                }

                public WorkloadIdentityAttribute(string operationId, string operationRevision, string fixtureId, string fixtureRevision)
                {
                }
            }
        }

        namespace BenchmarkDotNet.Attributes
        {
            [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Field)]
            public sealed class ParamsAttribute : System.Attribute
            {
                public ParamsAttribute(params object[] values)
                {
                }
            }

            [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Field)]
            public sealed class ParamsSourceAttribute : System.Attribute
            {
                public ParamsSourceAttribute(string name)
                {
                }
            }

            [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Field)]
            public sealed class ParamsAllValuesAttribute : System.Attribute
            {
            }

            [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = true)]
            public sealed class ArgumentsAttribute : System.Attribute
            {
                public ArgumentsAttribute(params object[] values)
                {
                }
            }

            [System.AttributeUsage(System.AttributeTargets.Method)]
            public sealed class ArgumentsSourceAttribute : System.Attribute
            {
                public ArgumentsSourceAttribute(string name)
                {
                }
            }

            [System.AttributeUsage(System.AttributeTargets.Method)]
            public sealed class BenchmarkAttribute : System.Attribute
            {
            }
        }
        """;
}
