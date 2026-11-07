using System.Text.Json.Nodes;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;
using Specht.Analysis.Documentation;
using Specht.Analysis.Policy;
using Specht.Analysis.Tests.Policy;

namespace Specht.Analysis.Tests.Documentation;

/// <summary>Authoritative and provisional test-method recognition, observed through the analyzer (0201-F3 B-017, B-018).</summary>
[Trait("Tier", "Unit")]
public sealed class DocumentationTestMethodsUnitTests
{
    [Fact]
    public async Task NoEvidenceArtifact_WhenAnalyzed_ShouldAcceptVerifiesOnAnOrdinaryMethodAndNotRequireItElsewhere()
    {
        // Given
        var source = "public class Example\n{\n    /// <verifies feature=\"cap:read\" claim=\"need/7\" />\n"
            + "    public void Run() { }\n    public void Other() { }\n}";

        // When
        var diagnostics = await Analyze(source, new AnalysisInputs().Inputs, null, "Debug");

        // Then
        diagnostics.Select(static diagnostic => diagnostic.Id).Should().BeEmpty();
    }

    [Fact]
    public async Task AnArtifactMatchingTheCompilation_WhenARecognizedMethodLacksVerifies_ShouldRequireIt()
    {
        // Given
        var source = "public class Example\n{\n    public void Run() { }\n}";
        var inputs = new AnalysisInputs().Inputs;
        var artifact = await Artifact(source, inputs);

        // When
        var diagnostics = await Analyze(source, inputs, artifact, "Debug");

        // Then
        var required = diagnostics.Should().ContainSingle().Subject;
        required.Id.Should().Be("SPCT0007");
        required.GetMessage().Should().Contain("Example.Run");
    }

    [Fact]
    public async Task ADiscoveredTestMethodWithNoVerifies_WhenAnalyzedInMandatoryScope_ShouldReportSPCT0007AsAWarning()
    {
        // Given
        var source = "public class Example\n{\n    public void Run() { }\n}";
        var inputs = new AnalysisInputs().Inputs;
        var artifact = await Artifact(source, inputs);

        // When
        var diagnostics = await Analyze(source, inputs, artifact, "Debug");

        // Then
        var required = diagnostics.Should().ContainSingle().Subject;
        required.Id.Should().Be("SPCT0007");
        required.Severity.Should().Be(DiagnosticSeverity.Warning);
        required.GetMessage().Should().Contain("Example.Run");
    }

    [Fact]
    public async Task AnExportedMethodNamedByNoDiscoveredCase_WhenAnalyzedInMandatoryScope_ShouldNotRequireVerifies()
    {
        // Given
        var source = "public class Example\n{\n    public void Run() { }\n    public void Helper() { }\n}";
        var inputs = new AnalysisInputs().Inputs;
        var artifact = await Artifact(source, inputs);

        // When
        var diagnostics = await Analyze(source, inputs, artifact, "Debug");

        // Then
        diagnostics.Should().NotContain(static diagnostic => diagnostic.GetMessage().Contains("Example.Helper", StringComparison.Ordinal));
    }

    [Theory]
    [ClassData(typeof(ProvisionalArtifactCases))]
    public async Task AnArtifactThatNoLongerMatchesTheCompilation_WhenARecognizedMethodLacksVerifies_ShouldNotRequireIt(
        string sourceSuffix, string configuration, bool policyEdited, string artifactEdit)
    {
        // Given
        var source = "public class Example\n{\n    public void Run() { }\n}";
        var exported = new AnalysisInputs().Inputs;
        var artifact = Edited(await Artifact(source, exported), artifactEdit);
        var current = new AnalysisInputs().Inputs;
        if (policyEdited)
        {
            current[0] = current[0] with { Text = current[0].Text + "\n" };
        }

        // When
        var diagnostics = await Analyze(source + sourceSuffix, current, artifact, configuration);

        // Then
        diagnostics.Select(static diagnostic => diagnostic.Id).Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(ProvisionalShapeCases))]
    public async Task NoEvidenceArtifact_WhenVerifiesIsMisplacedMalformedOrUnresolved_ShouldStillReportIt(string source, string id)
    {
        // Given
        var inputs = new AnalysisInputs().Inputs;

        // When
        var diagnostics = await Analyze(source, inputs, null, "Debug");

        // Then
        diagnostics.Select(static diagnostic => diagnostic.Id).Should().Equal(id);
    }

    private static string Edited(string artifact, string edit)
    {
        var node = JsonNode.Parse(artifact)!;
        switch (edit)
        {
            case "incomplete-discovery":
                node["discovery"]!["state"] = "incomplete";
                break;
            case "other-project":
                node["project"]!["path"] = "test/other.csproj";
                break;
            case "other-framework":
                node["project"]!["targetFramework"] = "net11.0";
                break;
        }

        return node.ToJsonString();
    }

    private static GeneratorTestContextBuilder Configure(GeneratorTestContextBuilder builder, string source,
        IEnumerable<AnalysisInput> inputs, string configuration)
    {
        builder = builder.WithDiagnosticSeverity(null)
            .AddSource(Path.Combine(Root, "test", "Example.cs"), source)
            .AddGlobalOption("build_property.SpechtRoot", Root)
            .AddGlobalOption("build_property.SpechtProjectPath", "test/tests.csproj")
            .AddGlobalOption("build_property.TargetFramework", "net10.0")
            .AddGlobalOption("build_property.Configuration", configuration);
        foreach (var input in inputs.Where(static input => input.Kind != "source"))
        {
            builder = builder.AddAdditionalText(input.RelativePath, input.Text!)
                .AddOption(input.RelativePath, "build_metadata.AdditionalFiles.SpechtInputKind", input.Kind)
                .AddOption(input.RelativePath, "build_metadata.AdditionalFiles.SpechtRelativePath", input.RelativePath);
        }

        return builder;
    }

    private static async Task<IReadOnlyList<Diagnostic>> Analyze(string source, IEnumerable<AnalysisInput> inputs, string? artifact,
        string configuration)
    {
        var builder = Configure(GeneratorTestContextBuilder.Create().WithAnalyzer<DocumentationBindingAnalyzer>(), source, inputs, configuration);
        if (artifact is not null)
        {
            builder = builder.AddAdditionalText("obj/specht-test-evidence/tests-net10.0.json", artifact)
                .AddOption("obj/specht-test-evidence/tests-net10.0.json", "build_metadata.AdditionalFiles.SpechtInputKind", "test-evidence")
                .AddOption("obj/specht-test-evidence/tests-net10.0.json", "build_metadata.AdditionalFiles.SpechtRelativePath",
                    "obj/specht-test-evidence/tests-net10.0.json");
        }

        var result = await builder.GenerateAsync(TestContext.Current.CancellationToken);
        result.InputDiagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
        return result.AnalyzerResults[typeof(DocumentationBindingAnalyzer)].Diagnostics;
    }

    private static Task<string> Artifact(string source, IEnumerable<AnalysisInput> inputs) =>
        new CurrentArtifact().For("M:Example.Run", [(Path.Combine(Root, "test", "Example.cs"), source)], inputs);

    private static readonly string Root = CurrentArtifact.Root;
}
