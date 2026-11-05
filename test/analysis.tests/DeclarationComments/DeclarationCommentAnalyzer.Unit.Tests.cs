using System.Collections.Immutable;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;
using Specht.Analysis.DeclarationComments;

namespace Specht.Analysis.Tests.DeclarationComments;

[Trait("Tier", "Unit")]
public sealed class DeclarationCommentAnalyzerUnitTests
{
    [Theory]
    [ClassData(typeof(SurfaceCommentCases))]
    public async Task SurfaceComments_WhenAnalyzedWithoutSpecificationInputs_ShouldReportEachCommentOnceAtItsSpan(string source, string[] comments)
    {
        // Given
        var builder = GeneratorTestContextBuilder.Create().WithAnalyzer<DeclarationCommentAnalyzer>()
            .WithDiagnosticSeverity(null).AddSource("src/Example.cs", source);

        // When
        var result = await builder.GenerateAsync(TestContext.Current.CancellationToken);

        // Then
        result.InputDiagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        var diagnostics = result.AnalyzerResults[typeof(DeclarationCommentAnalyzer)].Diagnostics
            .OrderBy(static diagnostic => diagnostic.Location.SourceSpan.Start).ToArray();
        diagnostics.Should().HaveCount(comments.Length);
        for (var index = 0; index < comments.Length; index++)
        {
            diagnostics[index].Id.Should().Be("SPCT0013");
            diagnostics[index].Severity.Should().Be(DiagnosticSeverity.Warning);
            diagnostics[index].Location.SourceTree!.FilePath.Should().Be("src/Example.cs");
            diagnostics[index].Location.SourceSpan.Should().Be(new TextSpan(source.IndexOf(comments[index], StringComparison.Ordinal), comments[index].Length));
        }
    }

    [Theory]
    [ClassData(typeof(AllowedCommentCases))]
    public async Task DocumentationAndOtherLocations_WhenAnalyzed_ShouldProduceNoDeclarationCommentDiagnostic(string source)
    {
        // Given
        var builder = GeneratorTestContextBuilder.Create().WithAnalyzer<DeclarationCommentAnalyzer>()
            .WithDiagnosticSeverity(null).AddSource("src/Example.cs", source);

        // When
        var result = await builder.GenerateAsync(TestContext.Current.CancellationToken);

        // Then
        result.InputDiagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        result.AnalyzerResults[typeof(DeclarationCommentAnalyzer)].Diagnostics.Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(GeneratedCommentCases))]
    public async Task GeneratedSource_WhenAnalyzed_ShouldProduceNoDeclarationCommentDiagnostic(string path, string source)
    {
        // Given
        var builder = GeneratorTestContextBuilder.Create().WithAnalyzer<DeclarationCommentAnalyzer>()
            .WithDiagnosticSeverity(null).AddSource(path, source);

        // When
        var result = await builder.GenerateAsync(TestContext.Current.CancellationToken);

        // Then
        result.AnalyzerResults[typeof(DeclarationCommentAnalyzer)].Diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task PartialDeclarations_WhenAnalyzed_ShouldReportEachPartIndependently()
    {
        // Given
        var builder = GeneratorTestContextBuilder.Create().WithAnalyzer<DeclarationCommentAnalyzer>()
            .WithDiagnosticSeverity(null).AddSource("src/First.cs", "// first\npartial class Example {}")
            .AddSource("src/Second.cs", "// second\npartial class Example {}");

        // When
        var result = await builder.GenerateAsync(TestContext.Current.CancellationToken);

        // Then
        result.AnalyzerResults[typeof(DeclarationCommentAnalyzer)].Diagnostics.Select(static diagnostic => diagnostic.Location.SourceTree!.FilePath)
            .Should().BeEquivalentTo("src/First.cs", "src/Second.cs");
    }

    [Fact]
    public void DiagnosticContract_WhenLoaded_ShouldPublishAnEnabledWarningWithHelp()
    {
        // Given
        var analyzer = new DeclarationCommentAnalyzer();

        // When
        var descriptor = analyzer.SupportedDiagnostics.Should().ContainSingle().Subject;

        // Then
        descriptor.Id.Should().Be("SPCT0013");
        descriptor.DefaultSeverity.Should().Be(DiagnosticSeverity.Warning);
        descriptor.IsEnabledByDefault.Should().BeTrue();
        descriptor.HelpLinkUri.Should().EndWith("/src/codeanalysis/docs/SPCT0013.md");
    }

    [Theory]
    [InlineData(ReportDiagnostic.Error)]
    [InlineData(ReportDiagnostic.Suppress)]
    public async Task ReportingOverride_WhenAnalyzed_ShouldHonorTheCompilerControl(ReportDiagnostic control)
    {
        // Given
        var tree = CSharpSyntaxTree.ParseText("class Example {\n// method\nvoid Run() {} }", path: "src/Example.cs",
            cancellationToken: TestContext.Current.CancellationToken);
        var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            .WithSpecificDiagnosticOptions(ImmutableDictionary<string, ReportDiagnostic>.Empty.Add("SPCT0013", control));
        var compilation = CSharpCompilation.Create("Example", [tree], options: options);

        // When
        var diagnostics = await compilation.WithAnalyzers([new DeclarationCommentAnalyzer()])
            .GetAnalyzerDiagnosticsAsync(TestContext.Current.CancellationToken);

        // Then
        if (control == ReportDiagnostic.Suppress)
        {
            diagnostics.Should().BeEmpty();
        }
        else
        {
            diagnostics.Should().ContainSingle().Which.Severity.Should().Be(DiagnosticSeverity.Error);
        }
    }

    [Fact]
    public async Task SuppressionPragma_WhenAnalyzed_ShouldSuppressTheCommentDiagnostic()
    {
        // Given
        var source = "#pragma warning disable SPCT0013 // chosen exception\nclass Example {\n// method\nvoid Run() {} }";
        var builder = GeneratorTestContextBuilder.Create().WithAnalyzer<DeclarationCommentAnalyzer>()
            .WithDiagnosticSeverity(null).AddSource("src/Example.cs", source);

        // When
        var result = await builder.GenerateAsync(TestContext.Current.CancellationToken);

        // Then
        result.AnalyzerResults[typeof(DeclarationCommentAnalyzer)].Diagnostics.Should().BeEmpty();
    }
}
