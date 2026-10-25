using System.Security.Cryptography;
using System.Text.Json.Nodes;
using Specht.Tool.Features.Evidence;
using Specht.Tool.Features.Evidence.Contract;

namespace Specht.Tests.Evidence;

internal sealed class PartitionedEvidenceRoot : IDisposable
{
    public PartitionedEvidenceRoot()
    {
        Directory.CreateDirectory(Path.Combine(_root, ".specht"));
        Directory.CreateDirectory(Path.Combine(_root, "src", "read", ".spec"));
        Directory.CreateDirectory(Path.Combine(_root, "test", "a"));
        Directory.CreateDirectory(Path.Combine(_root, "test", "b"));
        File.WriteAllText(Path.Combine(_root, "test", "Shared.cs"), "class Shared { }");
        File.WriteAllBytes(Path.Combine(_root, "test", "a", "a.dll"), [1]);
        File.WriteAllBytes(Path.Combine(_root, "test", "b", "b.dll"), [2]);
        File.WriteAllText(Path.Combine(_root, "src", "read", ".spec", "README.md"), """
            ---
            id: cap:read
            spec_status: approved
            ---
            ## 3. Acceptance Criteria
            | ID | Claim | Source | Status |
            | --- | --- | --- | --- |
            | need/7 | A synthetic need | Agreement | Active |
            """);
        var configuration = new JsonObject
        {
            ["revision"] = "0.1.0",
            ["idPatterns"] = new JsonObject { ["feature"] = "^cap:[a-z]+$", ["claim"] = "^need/[0-9]+$" },
            ["claimHeading"] = "3. Acceptance Criteria",
            ["lifecycleMarkers"] = new JsonObject { ["approved"] = "approved", ["withdrawn"] = "Withdrawn", ["superseded"] = "Superseded" },
            ["codeAnalysis"] = new JsonObject
            {
                ["mappings"] = new JsonObject
                {
                    ["/schemaVersion"] = "/revision",
                    ["/identifiers"] = "/idPatterns",
                    ["/claimsSection"] = "/claimHeading",
                    ["/markers"] = "/lifecycleMarkers",
                    ["/policy"] = "/codeAnalysis",
                },
                ["projects"] = new JsonArray(
                    new JsonObject { ["path"] = "test/a/a.csproj", ["targetFrameworks"] = new JsonArray("net10.0") },
                    new JsonObject { ["path"] = "test/b/b.csproj", ["targetFrameworks"] = new JsonArray("net10.0") }),
                ["features"] = new JsonArray(new JsonObject { ["feature"] = "cap:read" }),
                ["requiredLinks"] = new JsonObject { ["test"] = true },
                ["enforcement"] = new JsonArray(),
                ["exemptions"] = new JsonArray(),
            },
        };
        File.WriteAllText(Path.Combine(_root, ".specht", "manifest.json"), configuration.ToJsonString());
    }

    public void Write(string name, string project, int formatVersion, string runId, JsonArray cases, JsonArray results)
    {
        var inputs = new[]
        {
            new TestEvidenceInput(".specht/manifest.json", Hash(".specht/manifest.json"), "manifest"),
            new TestEvidenceInput("src/read/.spec/README.md", Hash("src/read/.spec/README.md"), "record"),
            new TestEvidenceInput("test/Shared.cs", Hash("test/Shared.cs"), "source"),
        };
        var fingerprint = new TestEvidenceFingerprint();
        var artifact = new JsonObject
        {
            ["format"] = "specht.test-evidence",
            ["formatVersion"] = formatVersion,
            ["project"] = new JsonObject { ["path"] = $"test/{project}/{project}.csproj", ["targetFramework"] = "net10.0" },
            ["configuration"] = new JsonObject { ["name"] = "Debug", ["fingerprint"] = new string('a', 64) },
            ["assembly"] = new JsonObject
            {
                ["name"] = project,
                ["sha256"] = Hash($"test/{project}/{project}.dll"),
                ["path"] = $"test/{project}/{project}.dll",
            },
            ["sourceFingerprint"] = fingerprint.ComputeDigests(inputs.Where(static input => input.Kind == "source")),
            ["policyFingerprint"] = fingerprint.ComputeDigests(inputs.Where(static input => input.Kind != "source")),
            ["adapter"] = new JsonObject { ["id"] = "vstest", ["version"] = "1.0.0", ["contractVersion"] = "1" },
            ["discovery"] = new JsonObject
            {
                ["state"] = "complete",
                ["sessionId"] = "session-" + project,
                ["methods"] = new JsonArray(Method("M:Shared.Checks", "Checks"), Method("M:Shared.Other", "Other")),
                ["cases"] = cases,
            },
            ["bindings"] = new JsonArray(new JsonObject
            {
                ["symbolId"] = "M:Shared.Checks",
                ["role"] = "verifies",
                ["attributes"] = new JsonObject(),
                ["shapeValid"] = true,
                ["status"] = "resolved",
                ["isEligible"] = true,
                ["target"] = new JsonObject
                {
                    ["kind"] = "claim",
                    ["epic"] = null,
                    ["feature"] = "cap:read",
                    ["id"] = "need/7",
                    ["state"] = "Active",
                    ["featureState"] = "approved",
                    ["relativePath"] = "src/read/.spec/README.md",
                    ["line"] = 8,
                },
                ["matches"] = new JsonArray(),
                ["location"] = null,
            }),
            ["results"] = new JsonObject { ["state"] = "available", ["runId"] = runId, ["cases"] = results },
            ["inputs"] = new JsonArray(inputs.Select(static input => (JsonNode?)new JsonObject
            {
                ["path"] = input.Path,
                ["sha256"] = input.Sha256,
                ["kind"] = input.Kind,
            }).ToArray()),
        };
        File.WriteAllText(Path.Combine(_root, name), artifact.ToJsonString());
    }

    public void Edit(string relativePath, string text) => File.WriteAllText(Path.Combine(_root, relativePath), text);

    public EvidenceResult Reconcile(params string[] evidence) => new EvidenceReconciler().Reconcile(_root, evidence, null);

    public void Dispose() => Directory.Delete(_root, true);

    private static JsonObject Method(string symbolId, string name) => new()
    {
        ["symbolId"] = symbolId,
        ["declaringType"] = "Shared",
        ["name"] = name,
        ["genericArity"] = 0,
        ["parameterTypes"] = new JsonArray(),
        ["location"] = null,
    };

    private string Hash(string relativePath) => Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(Path.Combine(_root, relativePath))));

    private readonly string _root = Directory.CreateTempSubdirectory("specht-partitioned-evidence-").FullName;
}
