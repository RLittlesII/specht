using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;
using Specht.Analysis.TestEvidence;

namespace Specht.Acceptance.Documentation;

internal static class AcceptanceTestEvidence
{
    public static string Root => Path.GetFullPath(".");

    public static string ForMethod(string symbolId)
    {
        var artifact = JsonNode.Parse("""
            {
              "format": "specht.test-evidence",
              "formatVersion": 1,
              "project": { "path": "test/tests.csproj", "targetFramework": "net10.0" },
              "configuration": {
                "name": "Debug",
                "fingerprint": "cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc"
              },
              "assembly": {
                "name": "tests",
                "sha256": "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb"
              },
              "sourceFingerprint": "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
              "policyFingerprint": "dddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd",
              "adapter": { "id": "acceptance", "version": "1", "contractVersion": "1" },
              "discovery": {
                "state": "complete",
                "sessionId": "acceptance-session",
                "methods": [{
                  "symbolId": "M:Example.Run",
                  "declaringType": "Example",
                  "name": "Run",
                  "genericArity": 0,
                  "parameterTypes": [],
                  "location": null
                }],
                "cases": [{
                  "caseId": "acceptance-case",
                  "methodSymbolId": "M:Example.Run",
                  "rowId": null,
                  "displayName": "Example.Run"
                }]
              },
              "bindings": [{
                "symbolId": "M:Example.Run",
                "role": "verifies",
                "attributes": { "feature": "0201-F3", "claim": "B-001" },
                "shapeValid": true,
                "status": "resolved",
                "isEligible": true,
                "target": {
                  "kind": "claim",
                  "epic": "0201",
                  "feature": "F3",
                  "id": "B-001",
                  "state": "Active",
                  "featureState": "approved",
                  "relativePath": "src/analyzers/TestEvidence/.spec/README.md",
                  "line": 54
                },
                "matches": [],
                "location": null
              }],
              "results": {
                "state": "available",
                "runId": "acceptance-run",
                "cases": [{ "caseId": "acceptance-case", "outcome": "passed", "detail": null }]
              },
              "inputs": []
            }
            """)!;
        artifact["discovery"]!["methods"]![0]!["symbolId"] = symbolId;
        artifact["discovery"]!["cases"]![0]!["methodSymbolId"] = symbolId;
        artifact["bindings"]![0]!["symbolId"] = symbolId;
        var inputs = new JsonArray
        {
            Input(".specht/manifest.json", new string('e', 64), "manifest"),
            Input(".specht/schema/feature.json", new string('f', 64), "schema"),
            Input("src/feature/.spec/README.md", new string('1', 64), "record"),
            Input("test/Example.cs", new string('a', 64), "source"),
        };
        artifact["inputs"] = inputs;
        var fingerprint = new TestEvidenceFingerprint();
        var identities = inputs.Select(static item => item!.AsObject()).Select(static item => new TestEvidenceInput(
            item["path"]!.GetValue<string>(), item["sha256"]!.GetValue<string>(), item["kind"]!.GetValue<string>())).ToArray();
        artifact["sourceFingerprint"] = fingerprint.ComputeDigests(identities.Where(static input => input.Kind is "source" or "generated-source"));
        artifact["policyFingerprint"] = fingerprint.ComputeDigests(identities.Where(static input => input.Kind is "manifest" or "schema" or "record"));
        return artifact.ToJsonString();
    }

    public static async Task<string> Current(string symbolId, IEnumerable<(string Path, string Source)> sources,
        IEnumerable<(string Kind, string Path, string Text)> inputs)
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

        foreach (var (kind, path, text) in inputs.Where(static input => input.Kind != "test-evidence"))
        {
            builder = builder.AddAdditionalText(path, text)
                .AddOption(path, "build_metadata.AdditionalFiles.SpechtInputKind", kind)
                .AddOption(path, "build_metadata.AdditionalFiles.SpechtRelativePath", path);
        }

        var result = await builder.GenerateAsync(CancellationToken.None);
        var literal = result.Results[typeof(TestEvidenceCompilationExporter)].SyntaxTrees.Single()
            .GetRoot(CancellationToken.None).DescendantNodes().OfType<LiteralExpressionSyntax>().Single();
        var export = JsonNode.Parse(literal.Token.ValueText)!;
        var artifact = JsonNode.Parse(ForMethod(symbolId))!;
        artifact["project"] = new JsonObject { ["path"] = export["projectPath"]!.DeepClone(), ["targetFramework"] = export["targetFramework"]!.DeepClone() };
        artifact["configuration"]!["name"] = export["configurationName"]!.DeepClone();
        artifact["configuration"]!["fingerprint"] = export["configurationFingerprint"]!.DeepClone();
        artifact["sourceFingerprint"] = export["sourceFingerprint"]!.DeepClone();
        artifact["policyFingerprint"] = export["policyFingerprint"]!.DeepClone();
        artifact["inputs"] = export["inputs"]!.DeepClone();
        return artifact.ToJsonString();
    }

    private static JsonObject Input(string path, string sha256, string kind) => new()
    {
        ["path"] = path,
        ["sha256"] = sha256,
        ["kind"] = kind,
    };
}
