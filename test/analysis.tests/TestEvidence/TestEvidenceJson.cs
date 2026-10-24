using System.Text.Json.Nodes;
using Specht.Analysis.TestEvidence;

namespace Specht.Analysis.Tests.TestEvidence;

internal static class TestEvidenceJson
{
    public const string AssemblyHash = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
    public const string ConfigurationFingerprint = "cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc";

    public static string Valid => Build(Template);
    public static string SourceFingerprint => Fingerprint(Valid, "sourceFingerprint");
    public static string PolicyFingerprint => Fingerprint(Valid, "policyFingerprint");

    public static string VersionThree
    {
        get
        {
            var artifact = JsonNode.Parse(Valid)!;
            artifact["formatVersion"] = 3;
            var cases = artifact["discovery"]!["cases"]!.AsArray();
            cases[0]!["categories"] = new JsonArray("Smoke", "visual");
            cases[0]!["owned"] = true;
            cases[0]!["selected"] = true;
            cases.Add(new JsonObject
            {
                ["caseId"] = "case-18",
                ["methodSymbolId"] = "M:Example.Tests.Sample.Run",
                ["rowId"] = "row-2",
                ["displayName"] = "Sample.Run(2)",
                ["categories"] = new JsonArray(),
                ["owned"] = true,
                ["selected"] = false,
            });
            return artifact.ToJsonString();
        }
    }

    public static string ForMethod(string symbolId)
    {
        var artifact = JsonNode.Parse(Valid)!;
        artifact["discovery"]!["methods"]![0]!["symbolId"] = symbolId;
        artifact["discovery"]!["cases"]![0]!["methodSymbolId"] = symbolId;
        artifact["bindings"]![0]!["symbolId"] = symbolId;
        return artifact.ToJsonString();
    }

    public static string WithDuplicateDisplayNames()
    {
        var artifact = JsonNode.Parse(Valid)!;
        artifact["discovery"]!["cases"]!.AsArray().Add(JsonNode.Parse("""
            {
              "caseId": "case-18",
              "methodSymbolId": "M:Example.Tests.Sample.Run",
              "rowId": "row-2",
              "displayName": "Sample.Run"
            }
            """));
        return artifact.ToJsonString();
    }

    public static string DeferredWithoutCases()
    {
        var artifact = JsonNode.Parse(Valid)!;
        artifact["discovery"]!["state"] = "deferred";
        artifact["discovery"]!["cases"] = new JsonArray();
        artifact["results"] = new JsonObject { ["state"] = "absent" };
        return artifact.ToJsonString();
    }

    private static string Build(string template)
    {
        var artifact = JsonNode.Parse(template)!;
        var inputs = new JsonArray
        {
            Input(".specht/manifest.json", "eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee", "manifest"),
            Input(".specht/schema/feature.json", "ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff", "schema"),
            Input("src/feature/.spec/README.md", "1111111111111111111111111111111111111111111111111111111111111111", "record"),
            Input("test/Sample.cs", "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "source"),
        };
        var fingerprint = new TestEvidenceFingerprint();
        artifact["inputs"] = inputs;
        artifact["sourceFingerprint"] = fingerprint.ComputeDigests(inputs.Select(static item => item!.AsObject())
            .Select(static item => new TestEvidenceInput(item["path"]!.GetValue<string>(), item["sha256"]!.GetValue<string>(),
                item["kind"]!.GetValue<string>())).Where(static item => item.Kind is "source" or "generated-source"));
        artifact["policyFingerprint"] = fingerprint.ComputeDigests(inputs.Select(static item => item!.AsObject())
            .Select(static item => new TestEvidenceInput(item["path"]!.GetValue<string>(), item["sha256"]!.GetValue<string>(),
                item["kind"]!.GetValue<string>())).Where(static item => item.Kind is "manifest" or "schema" or "record"));
        return artifact.ToJsonString();
    }

    private static string Fingerprint(string artifactJson, string property) => JsonNode.Parse(artifactJson)![property]!.GetValue<string>();

    private static JsonObject Input(string path, string sha256, string kind) => new()
    {
        ["path"] = path,
        ["sha256"] = sha256,
        ["kind"] = kind,
    };

    private const string Template = """
        {
          "format": "specht.test-evidence",
          "formatVersion": 1,
          "project": {
            "path": "test/Example.Tests.csproj",
            "targetFramework": "net10.0"
          },
          "configuration": {
            "name": "Debug",
            "fingerprint": "cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc"
          },
          "assembly": {
            "name": "Example.Tests",
            "sha256": "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb",
            "path": "test/bin/Example.Tests.dll"
          },
          "sourceFingerprint": "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa",
          "policyFingerprint": "dddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd",
          "adapter": {
            "id": "custom.runner",
            "version": "1.2.3",
            "contractVersion": "1"
          },
          "discovery": {
            "state": "complete",
            "sessionId": "session-2026-01",
            "methods": [
              {
                "symbolId": "M:Example.Tests.Sample.Run",
                "declaringType": "Example.Tests.Sample",
                "name": "Run",
                "genericArity": 0,
                "parameterTypes": [],
                "location": {
                  "path": "test/Sample.cs",
                  "line": 4,
                  "column": 5
                }
              }
            ],
            "cases": [
              {
                "caseId": "case-17",
                "methodSymbolId": "M:Example.Tests.Sample.Run",
                "rowId": null,
                "displayName": "Sample.Run"
              }
            ]
          },
          "bindings": [
            {
              "symbolId": "M:Example.Tests.Sample.Run",
              "role": "verifies",
              "attributes": {
                "feature": "0201-F3",
                "claim": "B-001"
              },
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
              "location": {
                "path": "test/Sample.cs",
                "line": 4,
                "column": 5
              }
            }
          ],
          "results": {
            "state": "available",
            "runId": "run-2026-01",
            "cases": [
              {
                "caseId": "case-17",
                "outcome": "passed",
                "detail": null
              }
            ]
          },
          "inputs": []
        }
        """;

}
