using System.Text.Json.Nodes;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;
using Specht.Analysis.Policy;
using Specht.Analysis.TestEvidence;
using Specht.Analysis.Tests.TestEvidence;

namespace Specht.Analysis.Tests.Documentation;

/// <summary>An evidence artifact that names the compilation it was exported from, so recognition is authoritative.</summary>
internal sealed class CurrentArtifact
{
    public static readonly string Root = Path.GetFullPath(".");

    public async Task<string> For(string symbolId, IEnumerable<(string Path, string Source)> sources, IEnumerable<AnalysisInput> inputs)
    {
        var builder = GeneratorTestContextBuilder.Create().WithGenerator<TestEvidenceCompilationExporter>()
            .WithDiagnosticSeverity(null)
            .AddGlobalOption("build_property.SpechtRoot", Root)
            .AddGlobalOption("build_property.SpechtProjectPath", "test/tests.csproj")
            .AddGlobalOption("build_property.TargetFramework", "net10.0")
            .AddGlobalOption("build_property.Configuration", "Debug");
        foreach (var source in sources)
        {
            builder = builder.AddSource(source.Path, source.Source);
        }

        foreach (var input in inputs.Where(static input => input.Kind != "source"))
        {
            builder = builder.AddAdditionalText(input.RelativePath, input.Text!)
                .AddOption(input.RelativePath, "build_metadata.AdditionalFiles.SpechtInputKind", input.Kind)
                .AddOption(input.RelativePath, "build_metadata.AdditionalFiles.SpechtRelativePath", input.RelativePath);
        }

        var result = await builder.GenerateAsync(TestContext.Current.CancellationToken);
        var literal = result.Results[typeof(TestEvidenceCompilationExporter)].SyntaxTrees.Should().ContainSingle().Subject
            .GetRoot(TestContext.Current.CancellationToken).DescendantNodes().OfType<LiteralExpressionSyntax>().Single();
        var export = JsonNode.Parse(literal.Token.ValueText)!;
        var artifact = JsonNode.Parse(TestEvidenceJson.ForMethod(symbolId))!;
        artifact["project"] = new JsonObject { ["path"] = export["projectPath"]!.DeepClone(), ["targetFramework"] = export["targetFramework"]!.DeepClone() };
        artifact["configuration"]!["name"] = export["configurationName"]!.DeepClone();
        artifact["configuration"]!["fingerprint"] = export["configurationFingerprint"]!.DeepClone();
        artifact["sourceFingerprint"] = export["sourceFingerprint"]!.DeepClone();
        artifact["policyFingerprint"] = export["policyFingerprint"]!.DeepClone();
        artifact["inputs"] = export["inputs"]!.DeepClone();
        return artifact.ToJsonString();
    }
}
