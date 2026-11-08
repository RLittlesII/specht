using System.Security.Cryptography;
using System.Text.Json.Nodes;
using Specht.Model;
using Specht.Tool.Features.Evidence;
using Specht.Tool.Features.Evidence.Contract;

namespace Specht.Tests.Evidence;

internal sealed class NativeClaimEvidenceRoot : IDisposable
{
    public NativeClaimEvidenceRoot()
    {
        Directory.CreateDirectory(Path.Combine(Root, ".specht"));
        Directory.CreateDirectory(Path.Combine(Root, "src", "read", ".spec"));
        Directory.CreateDirectory(Path.Combine(Root, "test", "a"));
        File.WriteAllText(Path.Combine(Root, "test", "Shared.cs"), "class Shared { }");
        File.WriteAllBytes(Path.Combine(Root, "test", "a", "a.dll"), [1]);
        Write(RecordPath, """
            ---
            id: cap:read
            spec_status: approved
            ---
            # Read
            """);
        _configuration = new JsonObject
        {
            ["revision"] = "0.1.0",
            ["idPatterns"] = new JsonObject { ["feature"] = "^cap:[a-z]+$", ["claim"] = "^need/[0-9]+$" },
            ["lifecycleMarkers"] = new JsonObject { ["approved"] = "approved", ["withdrawn"] = "Withdrawn", ["superseded"] = "Superseded" },
            ["claimTag"] = new JsonObject { ["tag"] = "@{claim}", ["tagIgnoreCase"] = false },
            ["codeAnalysis"] = new JsonObject
            {
                ["mappings"] = new JsonObject
                {
                    ["/schemaVersion"] = "/revision",
                    ["/identifiers"] = "/idPatterns",
                    ["/markers"] = "/lifecycleMarkers",
                    ["/claim"] = "/claimTag",
                    ["/policy"] = "/codeAnalysis",
                },
                ["projects"] = new JsonArray(new JsonObject { ["path"] = ProjectPath, ["targetFrameworks"] = new JsonArray("net10.0") }),
                ["features"] = new JsonArray(new JsonObject { ["feature"] = "cap:read" }),
                ["requiredLinks"] = new JsonObject { ["test"] = true },
                ["enforcement"] = new JsonArray(new JsonObject
                {
                    ["project"] = ProjectPath,
                    ["include"] = new JsonArray("test/**/*.cs"),
                    ["exclude"] = new JsonArray(),
                    ["role"] = "verifies",
                    ["declarationKinds"] = new JsonArray("testMethod"),
                }),
                ["exemptions"] = new JsonArray(),
                ["records"] = new JsonObject
                {
                    ["claimSource"] = "gherkin",
                    ["claimFileSets"] = new JsonArray(new JsonObject
                    {
                        ["glob"] = "src/read/.spec/*.feature",
                        ["feature"] = "cap:read",
                        ["record"] = RecordPath,
                    }),
                    ["plannedTags"] = new JsonArray("@ignore"),
                    ["withdrawnTags"] = new JsonArray("@withdrawn"),
                    ["supersededTags"] = new JsonArray("@superseded"),
                },
            },
        };
        File.WriteAllText(Path.Combine(Root, ".specht", "manifest.json"), _configuration.ToJsonString());
    }

    public const string ClaimFile = "src/read/.spec/read.feature";

    public string Root { get; } = Directory.CreateTempSubdirectory("specht-native-evidence-").FullName;

    public void Write(string relativePath, string text)
    {
        var path = Path.Combine(Root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, text);
    }

    public void Configure(Action<JsonObject> edit)
    {
        edit(_configuration["codeAnalysis"]!.AsObject());
        File.WriteAllText(Path.Combine(Root, ".specht", "manifest.json"), _configuration.ToJsonString());
    }

    public void WriteEvidence(JsonArray bindings, JsonArray cases, JsonArray results)
    {
        var claimFiles = Directory.EnumerateFiles(Path.Combine(Root, "src", "read", ".spec"), "*.feature")
            .Select(file => Path.GetRelativePath(Root, file).Replace('\\', '/'));
        var inputs = new[] { (".specht/manifest.json", "manifest"), (RecordPath, "record"), ("test/Shared.cs", "source") }
            .Concat(claimFiles.Select(static file => (file, "record")))
            .Select(entry => new TestEvidenceInput(entry.Item1, Hash(entry.Item1), entry.Item2))
            .OrderBy(static input => input.Path, StringComparer.Ordinal).ThenBy(static input => input.Kind, StringComparer.Ordinal)
            .ToArray();
        var fingerprint = new TestEvidenceFingerprint();
        var artifact = new JsonObject
        {
            ["format"] = "specht.test-evidence",
            ["formatVersion"] = 3,
            ["project"] = new JsonObject { ["path"] = ProjectPath, ["targetFramework"] = "net10.0" },
            ["configuration"] = new JsonObject { ["name"] = "Debug", ["fingerprint"] = new string('a', 64) },
            ["assembly"] = new JsonObject { ["name"] = "a", ["sha256"] = Hash("test/a/a.dll"), ["path"] = "test/a/a.dll" },
            ["sourceFingerprint"] = fingerprint.ComputeDigests(inputs.Where(static input => input.Kind == "source")),
            ["policyFingerprint"] = fingerprint.ComputeDigests(inputs.Where(static input => input.Kind != "source")),
            ["adapter"] = new JsonObject { ["id"] = "vstest", ["version"] = "1.0.0", ["contractVersion"] = "1" },
            ["discovery"] = new JsonObject
            {
                ["state"] = "complete",
                ["sessionId"] = "session-a",
                ["methods"] = new JsonArray(Method("M:Shared.Checks", "Checks"), Method("M:Shared.Other", "Other")),
                ["cases"] = cases,
            },
            ["bindings"] = bindings,
            ["results"] = new JsonObject { ["state"] = "available", ["runId"] = "run-a", ["cases"] = results },
            ["inputs"] = new JsonArray(inputs.Select(static input => (JsonNode?) new JsonObject
            {
                ["path"] = input.Path,
                ["sha256"] = input.Sha256,
                ["kind"] = input.Kind,
            }).ToArray()),
        };
        File.WriteAllText(Path.Combine(Root, "a.json"), artifact.ToJsonString());
    }

    public EvidenceResult Reconcile(params string[] evidence) =>
        new EvidenceReconciler(new SpechtDocumentParser(new FrontmatterReader())).Reconcile(Root, evidence, null);

    public void Dispose() => Directory.Delete(Root, true);

    private static JsonObject Method(string symbolId, string name) => new()
    {
        ["symbolId"] = symbolId,
        ["declaringType"] = "Shared",
        ["name"] = name,
        ["genericArity"] = 0,
        ["parameterTypes"] = new JsonArray(),
        ["location"] = null,
    };

    private string Hash(string relativePath) => Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(Path.Combine(Root, relativePath))));

    private const string ProjectPath = "test/a/a.csproj";
    private const string RecordPath = "src/read/.spec/README.md";

    private readonly JsonObject _configuration;
}
