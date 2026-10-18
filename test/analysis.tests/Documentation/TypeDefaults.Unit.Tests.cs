using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Specht.Analysis.Bindings;
using Specht.Analysis.Documentation;
using Specht.Analysis.Policy;
using Specht.Analysis.Tests.Policy;

namespace Specht.Analysis.Tests.Documentation;

[Trait("Tier", "Unit")]
public sealed class TypeDefaultsUnitTests
{
    [Theory]
    [ClassData(typeof(TypeDefaultCases))]
    public void LexicalAndPartialDefaults_WhenSelected_ShouldSupplyWholeContextWithoutEvidence(string source, string symbolId, string? expected)
    {
        // Given
        var tree = CSharpSyntaxTree.ParseText(source, path: "src/Example.cs", cancellationToken: TestContext.Current.CancellationToken);
        var compilation = CSharpCompilation.Create("Example", [tree], [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var snapshot = new AnalysisInputReader().Read(new AnalysisInputs().Inputs, TestContext.Current.CancellationToken);
        var symbol = DocumentationCommentId.GetFirstSymbolForDeclarationId(symbolId, compilation)!;

        // When
        var result = new DocumentationBindingReader().Read(symbol, new BindingResolver(snapshot.Catalog!, snapshot.Policy!.Bindings),
            cancellationToken: TestContext.Current.CancellationToken);

        // Then
        compilation.GetDiagnostics(TestContext.Current.CancellationToken).Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .Should().BeEmpty();
        var actual = result.AuthoringDefault?.Attributes["feature"];
        actual.Should().Be(expected);
        result.Relationships.Should().BeEmpty();
        result.Findings.Should().BeEmpty();
        if (expected == "cap:inner")
        {
            result.AuthoringDefault!.Attributes.Keys.Should().Equal("feature");
        }

        tree.ToString().Should().Be(source);
    }

    [Fact]
    public void ConflictingPartialDefaults_WhenReadAcrossFiles_ShouldBlockContextWithoutChangingExplicitEvidence()
    {
        // Given
        var sources = new[]
        {
            "/// <spec feature=\"cap:outer\" />\npublic partial class Outer {\n"
                + "/// <spec feature=\"cap:read\" />\npublic partial class Inner {} }",
            "public partial class Outer {\n/// <spec feature=\"cap:other\" />\npublic partial class Inner {\n"
                + "/// <implements feature=\"cap:read\" claim=\"need/7\" />\npublic void Run() {} } }",
        };
        var trees = sources.Select(static (source, index) => CSharpSyntaxTree.ParseText(source, path: "src/Part" + index + ".cs",
            cancellationToken: TestContext.Current.CancellationToken)).ToArray();
        var snapshot = new AnalysisInputReader().Read(new AnalysisInputs().Inputs, TestContext.Current.CancellationToken);
        var reader = new DocumentationBindingReader();
        var resolver = new BindingResolver(snapshot.Catalog!, snapshot.Policy!.Bindings);

        // When
        foreach (var ordered in new[] { trees, trees.Reverse().ToArray() })
        {
            var compilation = CSharpCompilation.Create("Example", ordered, [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            var type = compilation.GetTypeByMetadataName("Outer+Inner")!;
            var typeResult = reader.Read(type, resolver, cancellationToken: TestContext.Current.CancellationToken);
            var memberResult = reader.Read(type.GetMembers("Run").Single(), resolver, cancellationToken: TestContext.Current.CancellationToken);

            // Then
            compilation.GetDiagnostics(TestContext.Current.CancellationToken)
                .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
            typeResult.AuthoringDefault.Should().BeNull();
            typeResult.Findings.Select(static finding => finding.Id).Should().Equal("SPCT0008", "SPCT0008");
            typeResult.Findings.Select(static finding => finding.Location.SourceTree!.FilePath).Should().Equal("src/Part0.cs", "src/Part1.cs");
            memberResult.AuthoringDefault.Should().BeNull();
            memberResult.Findings.Should().BeEmpty();
            memberResult.Relationships.Should().ContainSingle().Which.Resolution!.Target!.Id.Should().Be("need/7");
        }
    }

    [Theory]
    [ClassData(typeof(InvalidDefaultCases))]
    public void InvalidDefault_WhenSelectingContext_ShouldRemainExcluded(string xml)
    {
        // Given
        var tree = CSharpSyntaxTree.ParseText("/// " + xml + "\npublic class Example { public void Run() {} }",
            path: "src/Example.cs", cancellationToken: TestContext.Current.CancellationToken);
        var compilation = CSharpCompilation.Create("Example", [tree], [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var snapshot = new AnalysisInputReader().Read(new AnalysisInputs().Inputs, TestContext.Current.CancellationToken);
        var resolver = new BindingResolver(snapshot.Catalog!, snapshot.Policy!.Bindings);
        var reader = new DocumentationBindingReader();
        var type = compilation.GetTypeByMetadataName("Example")!;

        // When
        var result = reader.Read(type.GetMembers("Run").Single(), resolver, cancellationToken: TestContext.Current.CancellationToken);
        var typeResult = reader.Read(type, resolver, cancellationToken: TestContext.Current.CancellationToken);

        // Then
        compilation.GetDiagnostics(TestContext.Current.CancellationToken).Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .Should().BeEmpty();
        result.AuthoringDefault.Should().BeNull();
        typeResult.AuthoringDefault.Should().BeNull();
        typeResult.Findings.Should().ContainSingle().Which.Id.Should().Be("SPCT0002");
    }
}
