using System.Text.Json.Nodes;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;
using Specht.Analysis.Documentation;
using Specht.Analysis.Tests.Policy;
using Specht.Analysis.Tests.TestEvidence;

namespace Specht.Analysis.Tests.Documentation;

[Trait("Tier", "Unit")]
public sealed class DocumentationBindingAnalyzerUnitTests
{
    [Fact]
    public async Task WholeProjectKinds_WhenAnalyzed_ShouldRequireOnlyTypesConstructorsAndMethods()
    {
        // Given
        var source = "public enum Mode { First }\npublic record Entry(string Name);\npublic class Example { public Example() {} public void Run() {} }";
        var inputs = new AnalysisInputs().Inputs;
        var configuration = JsonNode.Parse(inputs[0].Text!)!;
        configuration["codeAnalysis"]!["enforcement"] = JsonNode.Parse("""
            [{"project":"test/tests.csproj","include":["test/**/*.cs"],"exclude":[],
              "role":"implements","declarationKinds":["type","constructor","method"]}]
            """);
        inputs[0] = inputs[0] with { Text = configuration.ToJsonString() };
        var builder = GeneratorTestContextBuilder.Create().WithAnalyzer<DocumentationBindingAnalyzer>()
            .WithDiagnosticSeverity(null).AddSource("test/Example.cs", source)
            .AddSource("test/Program.cs", "int value = 1;")
            .AddGlobalOption("build_property.SpechtProjectPath", "test/tests.csproj")
            .AddGlobalOption("build_property.TargetFramework", "net10.0");
        foreach (var input in inputs.Where(static input => input.Kind != "source"))
        {
            builder = builder.AddAdditionalText(input.RelativePath, input.Text!)
                .AddOption(input.RelativePath, "build_metadata.AdditionalFiles.SpechtInputKind", input.Kind)
                .AddOption(input.RelativePath, "build_metadata.AdditionalFiles.SpechtRelativePath", input.RelativePath);
        }

        // When
        var result = await builder.GenerateAsync(TestContext.Current.CancellationToken);

        // Then
        // The source-generator test harness compiles a library, so its top-level statement produces CS8805.
        result.InputDiagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error && diagnostic.Id != "CS8805")
            .Should().BeEmpty();
        var required = result.AnalyzerResults[typeof(DocumentationBindingAnalyzer)].Diagnostics
            .Where(static diagnostic => diagnostic.Id == "SPCT0007").Select(static diagnostic => diagnostic.GetMessage()).ToArray();
        required.Should().Contain(static message => message.Contains("T:Mode", StringComparison.Ordinal));
        required.Should().Contain(static message => message.Contains("T:Entry", StringComparison.Ordinal));
        required.Should().Contain(static message => message.Contains("T:Example", StringComparison.Ordinal));
        required.Should().Contain(static message => message.Contains("Example.#ctor", StringComparison.Ordinal));
        required.Should().Contain(static message => message.Contains("Example.Run", StringComparison.Ordinal));
        required.Should().NotContain(static message => message.Contains("F:Mode", StringComparison.Ordinal)
            || message.Contains("P:Entry", StringComparison.Ordinal)
            || message.Contains("Program", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ConflictingTypeDefaults_WhenAnalyzed_ShouldReportEachDefaultWithoutMemberDuplicates()
    {
        // Given
        var first = "/// <spec feature=\"cap:read\" />\npublic partial class Example { }";
        var second = "/// <spec feature=\"cap:other\" />\npublic partial class Example { public void Run() {} }";
        var builder = GeneratorTestContextBuilder.Create().WithAnalyzer<DocumentationBindingAnalyzer>()
            .WithDiagnosticSeverity(null).AddSource("src/First.cs", first).AddSource("src/Second.cs", second)
            .AddGlobalOption("build_property.SpechtProjectPath", "test/tests.csproj")
            .AddGlobalOption("build_property.TargetFramework", "net10.0");
        foreach (var input in new AnalysisInputs().Inputs.Where(static input => input.Kind != "source"))
        {
            builder = builder.AddAdditionalText(input.RelativePath, input.Text!)
                .AddOption(input.RelativePath, "build_metadata.AdditionalFiles.SpechtInputKind", input.Kind)
                .AddOption(input.RelativePath, "build_metadata.AdditionalFiles.SpechtRelativePath", input.RelativePath);
        }

        // When
        var result = await builder.GenerateAsync(TestContext.Current.CancellationToken);

        // Then
        result.InputDiagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        var diagnostics = result.AnalyzerResults[typeof(DocumentationBindingAnalyzer)].Diagnostics
            .OrderBy(static diagnostic => diagnostic.Location.SourceTree!.FilePath, StringComparer.Ordinal).ToArray();
        diagnostics.Should().HaveCount(2);
        diagnostics.Select(static diagnostic => diagnostic.Id).Should().Equal("SPCT0008", "SPCT0008");
        diagnostics.Select(static diagnostic => diagnostic.Location.SourceTree!.FilePath).Should().Equal("src/First.cs", "src/Second.cs");
        foreach (var diagnostic in diagnostics)
        {
            diagnostic.Severity.Should().Be(DiagnosticSeverity.Warning);
            diagnostic.Location.SourceSpan.Should().Be(new TextSpan(5, 4));
            diagnostic.GetMessage().Should().Contain("feature=");
        }
    }

    [Theory]
    [ClassData(typeof(RequiredAuthoringCases))]
    public async Task RunnerSelectedMethods_WhenAnalyzed_ShouldSeparateExplicitValidationFromRequiredAuthoring(
        string path, bool recognized, bool explicitBinding, string? expectedId)
    {
        // Given
        var source = "public class Example\n{\n"
            + (explicitBinding ? "/// <implements feature=\"cap:read\" claim=\"bad\" />\n" : string.Empty) + "public void Run() {}\n}";
        var builder = GeneratorTestContextBuilder.Create().WithAnalyzer<DocumentationBindingAnalyzer>()
            .WithDiagnosticSeverity(null).AddSource(path, source)
            .AddGlobalOption("build_property.SpechtRoot", CurrentArtifact.Root)
            .AddGlobalOption("build_property.SpechtProjectPath", "test/tests.csproj")
            .AddGlobalOption("build_property.TargetFramework", "net10.0")
            .AddGlobalOption("build_property.Configuration", "Debug");
        var inputs = new AnalysisInputs().Inputs;
        foreach (var input in inputs.Where(static input => input.Kind != "source"))
        {
            builder = builder.AddAdditionalText(input.RelativePath, input.Text!)
                .AddOption(input.RelativePath, "build_metadata.AdditionalFiles.SpechtInputKind", input.Kind)
                .AddOption(input.RelativePath, "build_metadata.AdditionalFiles.SpechtRelativePath", input.RelativePath);
        }

        if (recognized)
        {
            builder = builder.AddAdditionalText("test/evidence.json", await new CurrentArtifact().For("M:Example.Run", [(path, source)], inputs))
                .AddOption("test/evidence.json", "build_metadata.AdditionalFiles.SpechtInputKind", "test-evidence")
                .AddOption("test/evidence.json", "build_metadata.AdditionalFiles.SpechtRelativePath", "test/evidence.json");
        }

        // When
        var result = await builder.GenerateAsync(TestContext.Current.CancellationToken);

        // Then
        result.InputDiagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        var diagnostics = result.AnalyzerResults[typeof(DocumentationBindingAnalyzer)].Diagnostics;
        if (expectedId is null)
        {
            diagnostics.Should().BeEmpty();
        }
        else
        {
            diagnostics.Select(static diagnostic => diagnostic.Id).OrderBy(static id => id, StringComparer.Ordinal)
                .Should().Equal(expectedId.Split(','));
        }
    }

    [Theory]
    [ClassData(typeof(XmlDiagnosticCases))]
    public async Task InvalidReservedXml_WhenAnalyzed_ShouldReportOnePreciseFinding(string xml, string id, string offending)
    {
        // Given
        var source = "public class Example\n{\n    /// " + xml + "\n    public void Run() { }\n}";
        var builder = GeneratorTestContextBuilder.Create().WithAnalyzer<DocumentationBindingAnalyzer>()
            .WithDiagnosticSeverity(null).AddSource("test/Example.cs", source)
            .AddGlobalOption("build_property.SpechtProjectPath", "test/tests.csproj")
            .AddGlobalOption("build_property.TargetFramework", "net10.0");
        foreach (var input in new AnalysisInputs().Inputs.Where(static input => input.Kind != "source"))
        {
            builder = builder.AddAdditionalText(input.RelativePath, input.Text!)
                .AddOption(input.RelativePath, "build_metadata.AdditionalFiles.SpechtInputKind", input.Kind)
                .AddOption(input.RelativePath, "build_metadata.AdditionalFiles.SpechtRelativePath", input.RelativePath);
        }

        // When
        var result = await builder.GenerateAsync(TestContext.Current.CancellationToken);

        // Then
        result.InputDiagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        var diagnostic = result.AnalyzerResults[typeof(DocumentationBindingAnalyzer)].Diagnostics.Should().ContainSingle().Subject;
        diagnostic.Id.Should().Be(id);
        diagnostic.Severity.Should().Be(DiagnosticSeverity.Warning);
        diagnostic.Location.SourceTree!.FilePath.Should().Be("test/Example.cs");
        var expectedText = id == "SPCT0002" ? offending : "claim=\"" + offending + "\"";
        diagnostic.Location.SourceSpan.Should().Be(new TextSpan(source.IndexOf(expectedText, StringComparison.Ordinal), expectedText.Length));
        diagnostic.Location.GetLineSpan().StartLinePosition.Line.Should().Be(2);
        diagnostic.GetMessage().Should().Contain(xml.Contains("claim=") ? "need/" : xml.Split(' ')[0].TrimStart('<'));
    }
}
