using System.Collections.Immutable;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;
using Specht.Analysis.Documentation;
using Specht.Analysis.Policy;
using Specht.Analysis.Tests.Documentation;
using Specht.Analysis.Tests.Policy;
using Specht.Analysis.Tests.TestEvidence;

namespace Specht.Analysis.Tests.Gherkin;

[Trait("Tier", "Unit")]
public sealed class GherkinEnforcementUnitTests
{
    [Theory]
    [InlineData("implements", "method", false)]
    [InlineData("verifies", "testMethod", true)]
    public async Task NativeClaimsWithAMissingRequiredRelationship_WhenAnalyzed_ShouldReportTheDiagnosticAMarkdownClaimReceives(
        string role, string kind, bool recognized)
    {
        // Given
        var source = "public class Example\n{\n    public void Run() {}\n}";
        var enforcement = "[{\"project\":\"test/tests.csproj\",\"include\":[\"test/**/*.cs\"],\"exclude\":[],\"role\":\"" + role
            + "\",\"declarationKinds\":[\"" + kind + "\"]}]";
        var native = new GherkinInputs().Merge("{\"codeAnalysis\":{\"enforcement\":" + enforcement + "}}").Inputs;
        var markdown = new AnalysisInputs().Inputs;
        var configuration = JsonNode.Parse(markdown[0].Text!)!;
        configuration["codeAnalysis"]!["enforcement"] = JsonNode.Parse(enforcement);
        markdown[0] = markdown[0] with { Text = configuration.ToJsonString() };

        // When
        var received = await Analyze(markdown, source, recognized);
        var actual = await Analyze(native, source, recognized);

        // Then
        received.Select(static diagnostic => diagnostic.Id).Should().Contain("SPCT0007");
        actual.Select(Describe).Should().Equal(received.Select(Describe));
    }

    [Fact]
    public async Task NativeVerifiesNamingAnAbsentClaim_WhenAnalyzed_ShouldReportTheTargetResolutionFailureAMarkdownClaimReceives()
    {
        // Given
        var native = new GherkinInputs().Merge("""
            {"codeAnalysis":{"enforcement":[
              {"project":"test/tests.csproj","include":["test/**/*.cs"],"exclude":[],"role":"verifies","declarationKinds":["testMethod"]}]}}
            """).Inputs;
        var markdown = new AnalysisInputs().Inputs;

        // When
        var received = await Analyze(markdown, Source("cap:read", "need/999"), true);
        var actual = await Analyze(native, Source("arithmetic", "REQ-FIX-999"), true);

        // Then
        received.Select(static diagnostic => diagnostic.Id).Should().Contain("SPCT0004");
        var absent = actual.Single(static diagnostic => diagnostic.Id == "SPCT0004");
        absent.Severity.Should().Be(received.Single(static diagnostic => diagnostic.Id == "SPCT0004").Severity);
        absent.GetMessage().Should().Contain("REQ-FIX-999");
    }

    [Fact]
    public async Task NativeVerifiesNamingADeclaredClaim_WhenAnalyzed_ShouldReportNoResolutionFailure()
    {
        // Given
        var native = new GherkinInputs().Merge("""
            {"codeAnalysis":{"enforcement":[
              {"project":"test/tests.csproj","include":["test/**/*.cs"],"exclude":[],"role":"verifies","declarationKinds":["testMethod"]}]}}
            """).Inputs;

        // When
        var actual = await Analyze(native, Source("arithmetic", "REQ-FIX-001"), true);

        // Then
        actual.Select(static diagnostic => diagnostic.Id).Should().NotContain(["SPCT0001", "SPCT0004", "SPCT0006", "SPCT0007"]);
    }

    private static string Describe(Diagnostic diagnostic) => diagnostic.Id + "|" + diagnostic.Severity + "|" + diagnostic.GetMessage();

    private static string Source(string feature, string claim) =>
        "public class Example\n{\n    /// <verifies feature=\"" + feature + "\" claim=\"" + claim + "\" />\n    public void Run() {}\n}";

    private static async Task<ImmutableArray<Diagnostic>> Analyze(IEnumerable<AnalysisInput> inputs, string source, bool recognized)
    {
        var builder = GeneratorTestContextBuilder.Create().WithAnalyzer<DocumentationBindingAnalyzer>()
            .WithDiagnosticSeverity(null).AddSource("test/Example.cs", source)
            .AddGlobalOption("build_property.SpechtRoot", CurrentArtifact.Root)
            .AddGlobalOption("build_property.SpechtProjectPath", "test/tests.csproj")
            .AddGlobalOption("build_property.TargetFramework", "net10.0")
            .AddGlobalOption("build_property.Configuration", "Debug");
        foreach (var input in inputs.Where(static input => input.Kind != "source"))
        {
            builder = builder.AddAdditionalText(input.RelativePath, input.Text!)
                .AddOption(input.RelativePath, "build_metadata.AdditionalFiles.SpechtInputKind", input.Kind)
                .AddOption(input.RelativePath, "build_metadata.AdditionalFiles.SpechtRelativePath", input.RelativePath);
        }

        if (recognized)
        {
            builder = builder.AddAdditionalText("test/evidence.json", await new CurrentArtifact().For("M:Example.Run", [("test/Example.cs", source)], inputs))
                .AddOption("test/evidence.json", "build_metadata.AdditionalFiles.SpechtInputKind", "test-evidence")
                .AddOption("test/evidence.json", "build_metadata.AdditionalFiles.SpechtRelativePath", "test/evidence.json");
        }

        var result = await builder.GenerateAsync(TestContext.Current.CancellationToken);
        return result.AnalyzerResults[typeof(DocumentationBindingAnalyzer)].Diagnostics;
    }
}
