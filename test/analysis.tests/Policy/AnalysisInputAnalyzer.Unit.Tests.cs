using System.Text.Json.Nodes;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;
using Specht.Analysis.Policy;

namespace Specht.Analysis.Tests.Policy;

[Trait("Tier", "Unit")]
public sealed class AnalysisInputAnalyzerUnitTests
{
    [Fact]
    public async Task MissingInputs_WhenAnalyzed_ShouldReportAnErrorWithoutCompilerErrors()
    {
        // Given
        var builder = GeneratorTestContextBuilder.Create().WithAnalyzer<AnalysisInputAnalyzer>()
            .WithDiagnosticSeverity(null).AddSource("src/Example.cs", "public class Example { }");

        // When
        var result = await builder.GenerateAsync(TestContext.Current.CancellationToken);

        // Then
        result.InputDiagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        var diagnostic = result.AnalyzerResults[typeof(AnalysisInputAnalyzer)].Diagnostics.Should().ContainSingle().Subject;
        diagnostic.Id.Should().Be("SPCT0001");
        diagnostic.Severity.Should().Be(DiagnosticSeverity.Error);
        diagnostic.Location.Should().Be(Location.None);
        diagnostic.Properties["SpechtRelativePath"].Should().Be("analysis/inputs");
    }

    [Fact]
    public async Task ActualAdditionalFiles_WhenAnalyzed_ShouldReadConfiguredConsumerRecordsWithoutInputErrors()
    {
        // Given
        var inputs = new AnalysisInputs().Inputs;
        var builder = GeneratorTestContextBuilder.Create().WithAnalyzer<AnalysisInputAnalyzer>()
            .WithDiagnosticSeverity(null).AddSource("src/Example.cs", "public class Example { }");
        foreach (var input in inputs.Where(static input => input.Kind != "source"))
        {
            builder = builder.AddAdditionalText(input.RelativePath, input.Text!)
                .AddOption(input.RelativePath, "build_metadata.AdditionalFiles.SpechtInputKind", input.Kind)
                .AddOption(input.RelativePath, "build_metadata.AdditionalFiles.SpechtRelativePath", input.RelativePath);
        }

        // When
        var result = await builder.GenerateAsync(TestContext.Current.CancellationToken);

        // Then
        result.InputDiagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        result.AnalyzerResults[typeof(AnalysisInputAnalyzer)].Diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task AMalformedRequiredCaseScope_WhenAnalyzed_ShouldReportSpct0001WithTheTagExpressionMessage()
    {
        // Given
        var inputs = new AnalysisInputs().Inputs;
        var configuration = JsonNode.Parse(inputs[0].Text!)!;
        configuration["codeAnalysis"]!["projects"]![0]!["requiredCaseScope"] = "@a and";
        inputs[0] = inputs[0] with { Text = configuration.ToJsonString() };
        var builder = GeneratorTestContextBuilder.Create().WithAnalyzer<AnalysisInputAnalyzer>()
            .WithDiagnosticSeverity(null).AddSource("src/Example.cs", "public class Example { }");
        foreach (var input in inputs.Where(static input => input.Kind != "source"))
        {
            builder = builder.AddAdditionalText(input.RelativePath, input.Text!)
                .AddOption(input.RelativePath, "build_metadata.AdditionalFiles.SpechtInputKind", input.Kind)
                .AddOption(input.RelativePath, "build_metadata.AdditionalFiles.SpechtRelativePath", input.RelativePath);
        }

        // When
        var result = await builder.GenerateAsync(TestContext.Current.CancellationToken);

        // Then
        var diagnostic = result.AnalyzerResults[typeof(AnalysisInputAnalyzer)].Diagnostics.Should().ContainSingle().Subject;
        diagnostic.Id.Should().Be("SPCT0001");
        diagnostic.GetMessage().Should().Be("Project required-case scope is not a valid tag expression.");
        diagnostic.Properties["SpechtRelativePath"].Should().Be("config/consumer.json");
    }
}
