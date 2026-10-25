using System.Text.Json.Nodes;
using AwesomeAssertions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;
using Specht.Analysis.Policy;
using Specht.Analysis.TestEvidence;
using Specht.Analysis.Tests.Gherkin;
using Specht.Analysis.Tests.Policy;

namespace Specht.Analysis.Tests.TestEvidence;

/// <summary>The compiler export's method locations and required-case scope (0201-F3 B-012, B-025).</summary>
[Trait("Tier", "Unit")]
public sealed class TestEvidenceCompilationExporterUnitTests
{
    [Fact]
    public async Task AMethodLinkedFromOutsideItsProjectDirectory_WhenExported_ShouldCarryThatFilesRepositoryRelativePath()
    {
        // Given
        var inputs = new AnalysisInputs().Inputs;

        // When
        var export = await Export(inputs);

        // Then
        export["methods"]!.AsArray().Single(static item => item!["symbolId"]!.GetValue<string>() == "M:Shared.LinkedCases.Fact")!
            ["location"]!["path"]!.GetValue<string>().Should().Be("shared/LinkedCases.cs");
    }

    [Fact]
    public async Task ProjectsDeclaringDifferentRequiredCaseScopes_WhenExported_ShouldCarryTheCompilingProjectsExpression()
    {
        // Given
        var inputs = new AnalysisInputs().Inputs;
        var configuration = JsonNode.Parse(inputs[0].Text!)!;
        configuration["codeAnalysis"]!["projects"] = JsonNode.Parse("""
            [{"path":"other/other.csproj","targetFrameworks":["net10.0"],"requiredCaseScope":"@visual"},
             {"path":"test/tests.csproj","targetFrameworks":["net10.0"],"requiredCaseScope":"not @visual"}]
            """);
        inputs[0] = inputs[0] with { Text = configuration.ToJsonString() };

        // When
        var export = await Export(inputs);

        // Then
        export["requiredCaseScope"]!.GetValue<string>().Should().Be("not @visual");
    }

    [Fact]
    public async Task NativeClaimInput_WhenExported_ShouldCarryTheCatalogMember()
    {
        // Given
        var inputs = new GherkinInputs().Inputs;

        // When
        var export = await Export(inputs);

        // Then
        JsonNode.DeepEquals(export["catalog"], JsonNode.Parse(TestEvidenceJson.CatalogJson)).Should().BeTrue();
    }

    [Fact]
    public async Task MarkdownClaimInput_WhenExported_ShouldCarryNoCatalogMember()
    {
        // Given
        var inputs = new AnalysisInputs().Inputs;

        // When
        var export = await Export(inputs);

        // Then
        export.AsObject().ContainsKey("catalog").Should().BeFalse();
    }

    private static async Task<JsonNode> Export(IEnumerable<AnalysisInput> inputs)
    {
        var root = Path.GetFullPath("consumer");
        var builder = GeneratorTestContextBuilder.Create().WithGenerator<TestEvidenceCompilationExporter>()
            .WithDiagnosticSeverity(null)
            .AddSource(Path.Combine(root, "shared", "LinkedCases.cs"), "namespace Shared { public class LinkedCases { public void Fact() { } } }")
            .AddGlobalOption("build_property.SpechtRoot", root)
            .AddGlobalOption("build_property.SpechtProjectPath", "test/tests.csproj")
            .AddGlobalOption("build_property.TargetFramework", "net10.0")
            .AddGlobalOption("build_property.Configuration", "Debug");
        foreach (var input in inputs.Where(static input => input.Kind != "source"))
        {
            builder = builder.AddAdditionalText(input.RelativePath, input.Text!)
                .AddOption(input.RelativePath, "build_metadata.AdditionalFiles.SpechtInputKind", input.Kind)
                .AddOption(input.RelativePath, "build_metadata.AdditionalFiles.SpechtRelativePath", input.RelativePath);
        }

        var result = await builder.GenerateAsync(TestContext.Current.CancellationToken);
        var literal = result.Results[typeof(TestEvidenceCompilationExporter)].SyntaxTrees.Should().ContainSingle().Subject
            .GetRoot(TestContext.Current.CancellationToken).DescendantNodes().OfType<LiteralExpressionSyntax>().Single();
        return JsonNode.Parse(literal.Token.ValueText)!;
    }
}
