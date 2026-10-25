using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;
using Specht.Analysis.Bindings.Gherkin;
using Specht.Analysis.Policy;
using Specht.Analysis.Tests.Policy;

namespace Specht.Analysis.Tests.Gherkin;

[Trait("Tier", "Unit")]
public sealed class GherkinClaimAnalyzerUnitTests
{
    [Theory]
    [ClassData(typeof(GherkinFindingCases))]
    public async Task ClaimFileFinding_WhenAnalyzed_ShouldBeReportedAsAWarningAtTheSuppliedFilesLine(
        string text, string id, int line, string message)
    {
        // Given
        var inputs = new GherkinInputs().ClaimFile(text).Inputs;

        // When
        var diagnostics = await Analyze(inputs);

        // Then
        var diagnostic = diagnostics.Where(candidate => candidate.Id == id && candidate.Location.GetLineSpan().StartLinePosition.Line + 1 == line)
            .Should().ContainSingle().Subject;
        diagnostic.Severity.Should().Be(DiagnosticSeverity.Warning);
        diagnostic.GetMessage().Should().Be(message);
        diagnostic.Location.GetLineSpan().Path.Should().Be(GherkinInputs.ClaimFilePath);
        diagnostic.Properties["SpechtRelativePath"].Should().Be(GherkinInputs.ClaimFilePath);
        diagnostic.Properties["SpechtLine"].Should().Be(line.ToString(System.Globalization.CultureInfo.InvariantCulture));
        diagnostic.Descriptor.Category.Should().Be("Traceability");
        diagnostic.Descriptor.HelpLinkUri.Should().Be("https://github.com/RLittlesII/specht/blob/main/src/codeanalysis/docs/" + id + ".md");
    }

    [Fact]
    public async Task CleanClaimFile_WhenAnalyzed_ShouldReportNothing()
    {
        // Given
        var inputs = new GherkinInputs().Inputs;

        // When
        var diagnostics = await Analyze(inputs);

        // Then
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task UnavailableSnapshot_WhenAnalyzed_ShouldLeaveTheInputFailureToTheInputAnalyzer()
    {
        // Given
        var inputs = new GherkinInputs().ClaimFile("Scenario: Orphan\n  Given x\n").Inputs;

        // When
        var diagnostics = await Analyze(inputs);

        // Then
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task MarkdownClaimInput_WhenAnalyzed_ShouldReportNothing()
    {
        // Given
        var inputs = new AnalysisInputs().Inputs;

        // When
        var diagnostics = await Analyze(inputs);

        // Then
        diagnostics.Should().BeEmpty();
    }

    private static async Task<IReadOnlyList<Diagnostic>> Analyze(IEnumerable<AnalysisInput> inputs)
    {
        var builder = GeneratorTestContextBuilder.Create().WithAnalyzer<GherkinClaimAnalyzer>()
            .WithDiagnosticSeverity(null).AddSource("src/Example.cs", "public class Example { }");
        foreach (var input in inputs.Where(static input => input.Kind != "source"))
        {
            builder = builder.AddAdditionalText(input.RelativePath, input.Text!)
                .AddOption(input.RelativePath, "build_metadata.AdditionalFiles.SpechtInputKind", input.Kind)
                .AddOption(input.RelativePath, "build_metadata.AdditionalFiles.SpechtRelativePath", input.RelativePath);
        }

        var result = await builder.GenerateAsync(TestContext.Current.CancellationToken);
        return result.AnalyzerResults[typeof(GherkinClaimAnalyzer)].Diagnostics;
    }
}
