using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
namespace Specht.Acceptance.Reconciliation;

internal sealed class EvidenceTree : IDisposable
{
    public EvidenceTree()
    {
        Root = Path.Combine(Path.GetTempPath(), "specht-evidence-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(Root, ".specht"));
        Directory.CreateDirectory(Path.Combine(Root, "src", "read", ".spec"));
        Directory.CreateDirectory(Path.Combine(Root, "test"));
        File.WriteAllText(Path.Combine(Root, "test", "tests.csproj"), "<Project />");
        File.WriteAllText(Path.Combine(Root, "test", "Tests.cs"), "class Tests { }");
        File.WriteAllBytes(Path.Combine(Root, "test", "tests.dll"), [1, 2, 3]);
        File.WriteAllText(Path.Combine(Root, "src", "read", ".spec", "README.md"), """
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
                ["projects"] = new JsonArray(new JsonObject { ["path"] = "test/tests.csproj", ["targetFrameworks"] = new JsonArray("net10.0") }),
                ["features"] = new JsonArray(new JsonObject { ["feature"] = "cap:read" }),
                ["requiredLinks"] = new JsonObject { ["test"] = true },
                ["enforcement"] = new JsonArray(),
                ["exemptions"] = new JsonArray(),
            },
        };
        File.WriteAllText(Path.Combine(Root, ".specht", "manifest.json"), configuration.ToJsonString());
    }

    public string Root { get; }

    public void AppendToRecord(string text) => File.AppendAllText(Path.Combine(Root, "src", "read", ".spec", "README.md"), text);

    public void EditRecord()
    {
        var path = Path.Combine(Root, "src", "read", ".spec", "README.md");
        File.WriteAllText(path, File.ReadAllText(path).Replace("A synthetic need", "An edited synthetic need", StringComparison.Ordinal));
    }

    public void EditAssembly() => File.WriteAllBytes(Path.Combine(Root, "test", "tests.dll"), [4, 5, 6]);

    public void OmitIndexedRecord()
    {
        var path = Path.Combine(Root, "evidence.json");
        var artifact = JsonNode.Parse(File.ReadAllText(path))!;
        var inputs = artifact["inputs"]!.AsArray();
        var record = inputs.Single(static item => item!["kind"]!.GetValue<string>() == "record");
        inputs.Remove(record);
        var manifest = inputs.Single(static item => item!["kind"]!.GetValue<string>() == "manifest");
        artifact["policyFingerprint"] = Fingerprint((manifest!["path"]!.GetValue<string>(), "manifest",
            manifest["sha256"]!.GetValue<string>()));
        File.WriteAllText(path, artifact.ToJsonString());
    }

    public void AddProject(string project)
    {
        var path = Path.Combine(Root, ".specht", "manifest.json");
        var manifest = JsonNode.Parse(File.ReadAllText(path))!;
        manifest["codeAnalysis"]!["projects"]!.AsArray().Add(new JsonObject
        {
            ["path"] = project,
            ["targetFrameworks"] = new JsonArray("net10.0"),
        });
        File.WriteAllText(path, manifest.ToJsonString());
    }

    public void Require(string kind, bool enabled)
    {
        var path = Path.Combine(Root, ".specht", "manifest.json");
        var manifest = JsonNode.Parse(File.ReadAllText(path))!;
        manifest["codeAnalysis"]!["requiredLinks"]![kind] = enabled;
        File.WriteAllText(path, manifest.ToJsonString());
    }

    public void Exempt(string kind, string reason)
    {
        var path = Path.Combine(Root, ".specht", "manifest.json");
        var manifest = JsonNode.Parse(File.ReadAllText(path))!;
        manifest["codeAnalysis"]!["exemptions"]!.AsArray().Add(new JsonObject
        {
            ["target"] = new JsonObject { ["feature"] = "cap:read", ["claim"] = "need/7" },
            ["links"] = new JsonArray(kind),
            ["reason"] = reason,
        });
        File.WriteAllText(path, manifest.ToJsonString());
    }

    public void AddFramework(string framework)
    {
        var path = Path.Combine(Root, ".specht", "manifest.json");
        var manifest = JsonNode.Parse(File.ReadAllText(path))!;
        manifest["codeAnalysis"]!["projects"]![0]!["targetFrameworks"]!.AsArray().Add(framework);
        File.WriteAllText(path, manifest.ToJsonString());
    }

    public void WriteEvidence(string? outcome = "passed", bool binding = true, bool historicalBinding = false, bool scenario = false) =>
        File.WriteAllText(Path.Combine(Root, "evidence.json"), BuildEvidence(outcome, binding, historicalBinding, scenario).ToJsonString());

    public void UsePartitions(params string[] names)
    {
        var path = Path.Combine(Root, ".specht", "manifest.json");
        var manifest = JsonNode.Parse(File.ReadAllText(path))!;
        var projects = manifest["codeAnalysis"]!["projects"]!.AsArray();
        projects.Clear();
        foreach (var name in names)
        {
            Directory.CreateDirectory(Path.Combine(Root, "test", name));
            File.WriteAllBytes(Path.Combine(Root, "test", name, name + ".dll"), [(byte)name[0], 9]);
            projects.Add(new JsonObject { ["path"] = ProjectPath(name), ["targetFrameworks"] = new JsonArray("net10.0") });
        }

        File.WriteAllText(path, manifest.ToJsonString());
    }

    public void WriteCases(string name, string file, IReadOnlyList<CaseSpec> cases)
    {
        var artifact = BuildEvidence();
        artifact["formatVersion"] = 3;
        artifact["project"]!["path"] = ProjectPath(name);
        if (name != "tests")
        {
            var assembly = $"test/{name}/{name}.dll";
            artifact["assembly"] = new JsonObject
            {
                ["name"] = name,
                ["sha256"] = Hash(File.ReadAllBytes(Path.Combine(Root, assembly))),
                ["path"] = assembly,
            };
        }

        artifact["discovery"]!["cases"] = new JsonArray(cases.Select(static item => (JsonNode?)new JsonObject
        {
            ["caseId"] = item.CaseId,
            ["methodSymbolId"] = "M:test",
            ["rowId"] = null,
            ["displayName"] = item.CaseId,
            ["categories"] = new JsonArray(),
            ["owned"] = item.Owned,
            ["selected"] = item.Selected,
        }).ToArray());
        artifact["results"] = new JsonObject
        {
            ["state"] = "available",
            ["runId"] = "run-" + name,
            ["cases"] = new JsonArray(cases.Where(static item => item.Selected && item.Outcome is not null)
                .Select(static item => (JsonNode?)new JsonObject { ["caseId"] = item.CaseId, ["outcome"] = item.Outcome, ["detail"] = null }).ToArray()),
        };
        var linked = cases.Where(static item => item.Link is not null).ToArray();
        if (linked.Length > 0)
        {
            var target = artifact["bindings"]![0]!["target"]!.DeepClone();
            artifact["scenarios"] = new JsonArray(new JsonObject
            {
                ["path"] = "src/read/.spec/read.feature",
                ["scenarioId"] = "outline-1",
                ["line"] = 1,
                ["scope"] = new JsonObject { ["epic"] = null, ["feature"] = "cap:read" },
                ["claims"] = new JsonArray(new JsonObject
                {
                    ["id"] = "need/7",
                    ["status"] = "resolved",
                    ["isEligible"] = true,
                    ["target"] = target,
                    ["matches"] = new JsonArray(),
                }),
                ["cases"] = new JsonArray(linked.Select(static item => (JsonNode?)new JsonObject
                {
                    ["rowId"] = null,
                    ["caseId"] = item.CaseId,
                    ["status"] = "resolved",
                    ["state"] = item.Link,
                }).ToArray()),
            });
        }

        File.WriteAllText(Path.Combine(Root, file), artifact.ToJsonString());
    }

    public (string Stdout, string Stderr, int ExitCode) Run(params string[] options) =>
        AcceptanceTool.Launch(Root, ["evidence", "--root", Root, "--evidence", "evidence.json", .. options]);

    public (string Stdout, string Stderr, int ExitCode) RunWith(params string[] arguments) =>
        AcceptanceTool.Launch(Root, ["evidence", "--root", Root, .. arguments]);

    public string WriteExternalDirectory()
    {
        _externalDirectory = Path.Combine(Path.GetTempPath(), "specht-external-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_externalDirectory);
        File.Copy(Path.Combine(Root, "evidence.json"), Path.Combine(_externalDirectory, "first.json"));
        File.Copy(Path.Combine(Root, "evidence.json"), Path.Combine(_externalDirectory, "second.json"));
        return _externalDirectory;
    }

    public string WriteOverlappingDirectory()
    {
        var directory = Path.Combine(Root, "evidence-dir");
        Directory.CreateDirectory(directory);
        File.Copy(Path.Combine(Root, "evidence.json"), Path.Combine(directory, "copy.json"));
        return "evidence-dir";
    }

    public void Dispose()
    {
        Directory.Delete(Root, true);
        if (_externalDirectory is not null)
        {
            Directory.Delete(_externalDirectory, true);
        }
    }

    private static string ProjectPath(string name) => name == "tests" ? "test/tests.csproj" : $"test/{name}/{name}.csproj";

    private static string Hash(byte[] bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));

    private static string Fingerprint(params (string Path, string Kind, string Digest)[] inputs)
    {
        using var stream = new MemoryStream();
        foreach (var input in inputs.OrderBy(static entry => entry.Path, StringComparer.Ordinal)
            .ThenBy(static entry => entry.Kind, StringComparer.Ordinal))
        {
            foreach (var value in new[] { input.Path, input.Kind, input.Digest })
            {
                var bytes = Encoding.UTF8.GetBytes(value);
                stream.WriteByte((byte)(bytes.Length >> 24));
                stream.WriteByte((byte)(bytes.Length >> 16));
                stream.WriteByte((byte)(bytes.Length >> 8));
                stream.WriteByte((byte)bytes.Length);
                stream.Write(bytes);
            }
        }

        return Hash(stream.ToArray());
    }

    private JsonObject BuildEvidence(string? outcome = "passed", bool binding = true, bool historicalBinding = false, bool scenario = false)
    {
        var recordPath = "src/read/.spec/README.md";
        if (historicalBinding)
        {
            AppendToRecord("| need/8 | A past need | Agreement | Withdrawn |\n");
        }

        var inputHash = Hash(File.ReadAllBytes(Path.Combine(Root, recordPath)));
        var manifestPath = ".specht/manifest.json";
        var manifestHash = Hash(File.ReadAllBytes(Path.Combine(Root, manifestPath)));
        var sourcePath = "test/Tests.cs";
        var sourceHash = Hash(File.ReadAllBytes(Path.Combine(Root, sourcePath)));
        var inputs = new JsonArray(
            new JsonObject { ["path"] = manifestPath, ["sha256"] = manifestHash, ["kind"] = "manifest" },
            new JsonObject { ["path"] = recordPath, ["sha256"] = inputHash, ["kind"] = "record" },
            new JsonObject { ["path"] = sourcePath, ["sha256"] = sourceHash, ["kind"] = "source" });
        var target = new JsonObject
        {
            ["kind"] = "claim",
            ["epic"] = null,
            ["feature"] = "cap:read",
            ["id"] = historicalBinding ? "need/8" : "need/7",
            ["state"] = historicalBinding ? "Withdrawn" : "Active",
            ["featureState"] = "approved",
            ["relativePath"] = recordPath,
            ["line"] = historicalBinding ? 8 : 7,
        };
        var evidence = new JsonObject
        {
            ["format"] = "specht.test-evidence",
            ["formatVersion"] = 1,
            ["project"] = new JsonObject { ["path"] = "test/tests.csproj", ["targetFramework"] = "net10.0" },
            ["configuration"] = new JsonObject { ["name"] = "Debug", ["fingerprint"] = new string('a', 64) },
            ["assembly"] = new JsonObject
            {
                ["name"] = "tests",
                ["sha256"] = Hash(File.ReadAllBytes(Path.Combine(Root, "test", "tests.dll"))),
                ["path"] = "test/tests.dll",
            },
            ["sourceFingerprint"] = Fingerprint((sourcePath, "source", sourceHash)),
            ["policyFingerprint"] = Fingerprint((manifestPath, "manifest", manifestHash), (recordPath, "record", inputHash)),
            ["adapter"] = new JsonObject { ["id"] = "xunit", ["version"] = "1", ["contractVersion"] = "1" },
            ["discovery"] = new JsonObject
            {
                ["state"] = "complete",
                ["sessionId"] = "session-1",
                ["methods"] = new JsonArray(new JsonObject
                {
                    ["symbolId"] = "M:test",
                    ["declaringType"] = "Tests",
                    ["name"] = "Checks",
                    ["genericArity"] = 0,
                    ["parameterTypes"] = new JsonArray(),
                    ["location"] = null,
                }),
                ["cases"] = new JsonArray(new JsonObject
                {
                    ["caseId"] = "case-1",
                    ["methodSymbolId"] = "M:test",
                    ["rowId"] = null,
                    ["displayName"] = "Checks",
                }),
            },
            ["bindings"] = binding ? new JsonArray(new JsonObject
            {
                ["symbolId"] = "M:test",
                ["role"] = "verifies",
                ["attributes"] = new JsonObject(),
                ["shapeValid"] = true,
                ["status"] = "resolved",
                ["isEligible"] = !historicalBinding,
                ["target"] = target,
                ["matches"] = new JsonArray(),
                ["location"] = null,
            }) : new JsonArray(),
            ["results"] = outcome is null ? new JsonObject { ["state"] = "absent" }
                : new JsonObject
                {
                    ["state"] = "available",
                    ["runId"] = "run-1",
                    ["cases"] = new JsonArray(new JsonObject { ["caseId"] = "case-1", ["outcome"] = outcome, ["detail"] = null }),
                },
            ["inputs"] = inputs,
        };
        if (scenario)
        {
            evidence["discovery"]!["methods"]!.AsArray().Add(new JsonObject
            {
                ["symbolId"] = "M:other",
                ["declaringType"] = "Tests",
                ["name"] = "Other",
                ["genericArity"] = 0,
                ["parameterTypes"] = new JsonArray(),
                ["location"] = null,
            });
            evidence["discovery"]!["cases"]!.AsArray().Add(new JsonObject
            {
                ["caseId"] = "case-2",
                ["methodSymbolId"] = "M:other",
                ["rowId"] = null,
                ["displayName"] = "Other",
            });
            evidence["results"]!["cases"]!.AsArray().Add(new JsonObject
            {
                ["caseId"] = "case-2",
                ["outcome"] = "passed",
                ["detail"] = null,
            });
            evidence["scenarios"] = new JsonArray(new JsonObject
            {
                ["path"] = "src/read/.spec/read.feature",
                ["scenarioId"] = "scenario-1",
                ["line"] = 1,
                ["scope"] = new JsonObject { ["epic"] = null, ["feature"] = "cap:read" },
                ["claims"] = new JsonArray(new JsonObject
                {
                    ["id"] = "need/7",
                    ["status"] = "resolved",
                    ["isEligible"] = true,
                    ["target"] = target.DeepClone(),
                    ["matches"] = new JsonArray(),
                }),
                ["cases"] = new JsonArray(new JsonObject { ["rowId"] = null, ["caseId"] = "case-1", ["status"] = "resolved", ["state"] = "active" }),
            });
        }
        return evidence;
    }

    private string? _externalDirectory;
}
