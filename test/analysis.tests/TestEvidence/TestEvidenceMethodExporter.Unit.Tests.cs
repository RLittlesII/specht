using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Specht.Analysis.TestEvidence;

namespace Specht.Analysis.Tests.TestEvidence;

[Trait("Tier", "Unit")]
public sealed class TestEvidenceMethodExporterUnitTests
{
    [Fact]
    public void OverloadedMethods_WhenExported_ShouldHaveDistinctRoslynIdentitiesAndParameters()
    {
        // Given
        var tree = CSharpSyntaxTree.ParseText("""
            namespace Example;
            public class Sample
            {
                public void Run(int value) { }
                public void Run(string value) { }
                public T Convert<T>(T value) => value;
            }
            """, path: "test/Sample.cs", cancellationToken: TestContext.Current.CancellationToken);
        var compilation = CSharpCompilation.Create("Example", [tree],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);
        var type = compilation.GetTypeByMetadataName("Example.Sample")!;
        var exporter = new TestEvidenceMethodExporter();

        // When
        var integer = exporter.Export((IMethodSymbol)type.GetMembers("Run")[0], "test/Sample.cs");
        var text = exporter.Export((IMethodSymbol)type.GetMembers("Run")[1], "test/Sample.cs");
        var generic = exporter.Export((IMethodSymbol)type.GetMembers("Convert").Single(), "test/Sample.cs");

        // Then
        integer.SymbolId.Should().NotBe(text.SymbolId);
        integer.ParameterTypes.Should().ContainSingle().Which.Should().Contain("System.Int32");
        text.ParameterTypes.Should().ContainSingle().Which.Should().Contain("System.String");
        generic.GenericArity.Should().Be(1);
        integer.Location!.Path.Should().Be("test/Sample.cs");
        integer.Location.Line.Should().Be(4);
    }

    [Fact]
    public void GeneratedMethod_WhenExported_ShouldOmitSourceLocation()
    {
        // Given
        var tree = CSharpSyntaxTree.ParseText("public class Sample { public void Run() { } }", path: "test/Sample.g.cs",
            cancellationToken: TestContext.Current.CancellationToken);
        var compilation = CSharpCompilation.Create("Example", [tree],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)]);
        var method = (IMethodSymbol)compilation.GetTypeByMetadataName("Sample")!.GetMembers("Run").Single();

        // When
        var result = new TestEvidenceMethodExporter().Export(method, "test/Sample.g.cs");

        // Then
        result.Location.Should().BeNull();
    }
}
