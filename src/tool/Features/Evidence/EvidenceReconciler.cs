using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using Specht.Discovery;
using Specht.Model;
using Specht.Tool.Features.Evidence.Contract;

namespace Specht.Tool.Features.Evidence;

/// <summary>Joins current consumer records to supplied version-one build and runner exports.</summary>
/// <implements epic="0201" feature="F5" claim="B-001" />
public sealed class EvidenceReconciler
{
    /// <summary>Builds a deterministic report from the explicitly selected artifacts.</summary>
    /// <implements epic="0201" feature="F5" claim="B-001" />
    public EvidenceResult Reconcile(string root, IReadOnlyList<string> evidencePaths, string? runId)
    {
        var findings = new List<EvidenceFinding>();
        var claims = new List<ClaimEntry>();
        var bindingRows = new List<JsonObject>();
        var caseRows = new List<JsonObject>();
        var runRows = new List<JsonObject>();
        var inputRows = new List<JsonObject>();
        var scope = new JsonObject { ["projects"] = new JsonArray(), ["features"] = new JsonArray(), ["supplied"] = new JsonArray() };
        var configurationInvalid = false;
        var explicitlyMissing = false;
        var inputsIncomplete = false;
        var inputsUnavailable = false;

        try
        {
            if (!Directory.Exists(root))
            {
                throw new EvidenceMissingInputException();
            }

            var manifestPath = Path.Combine(root, ".specht", "manifest.json");
            if (!File.Exists(manifestPath))
            {
                throw new EvidenceMissingInputException();
            }

            var configuration = JsonNode.Parse(File.ReadAllText(manifestPath)) as JsonObject
                ?? throw new EvidenceConfigurationException("The analysis configuration is invalid.");
            var policy = EvidencePolicy.Read(configuration);
            foreach (var project in policy.Projects)
            {
                foreach (var framework in project.Frameworks)
                {
                    scope["projects"]!.AsArray().Add(new JsonObject { ["path"] = project.Path, ["targetFramework"] = framework });
                }
            }

            foreach (var feature in policy.Features)
            {
                scope["features"]!.AsArray().Add(new JsonObject { ["epic"] = feature.Epic, ["feature"] = feature.Feature });
            }

            claims.AddRange(ReadClaims(root, configuration, policy, findings));
            if (findings.Any(static finding => finding.Kind == "invalidInput"))
            {
                configurationInvalid = true;
            }

            var namedFiles = new List<(string File, string Label)>();
            for (var index = 0; index < evidencePaths.Count; index++)
            {
                var named = evidencePaths[index];
                var absolute = Path.GetFullPath(Path.IsPathRooted(named) ? named : Path.Combine(root, named));
                if (File.Exists(absolute))
                {
                    namedFiles.Add((absolute, Label(root, absolute, index)));
                }
                else if (Directory.Exists(absolute))
                {
                    var directoryLabel = Label(root, absolute, index);
                    namedFiles.AddRange(Directory.EnumerateFiles(absolute, "*.json", SearchOption.AllDirectories)
                        .Order(StringComparer.Ordinal).Select(file => (file, directoryLabel.StartsWith("external-evidence-", StringComparison.Ordinal)
                            ? directoryLabel + "/" + Path.GetRelativePath(absolute, file).Replace('\\', '/') : Label(root, file, index))));
                }
                else
                {
                    explicitlyMissing = true;
                    findings.Add(new EvidenceFinding("missingEvidence", Label(root, absolute, index), 0, string.Empty,
                        "A named evidence input was not found."));
                }
            }

            namedFiles = namedFiles.GroupBy(static entry => entry.File, StringComparer.Ordinal)
                .Select(static group => group.First()).ToList();

            if (namedFiles.Count == 0 && !explicitlyMissing)
            {
                inputsIncomplete = true;
                findings.Add(new EvidenceFinding("missingCompilation", "", 0, "", "No evidence was supplied for the declared scope."));
            }

            var artifacts = new List<ArtifactEntry>();
            var reader = new TestEvidenceArtifactReader();
            foreach (var (file, label) in namedFiles.OrderBy(static entry => entry.Label, StringComparer.Ordinal))
            {
                var read = reader.Read(File.ReadAllText(file));
                if (!read.IsValid || read.Artifact is null)
                {
                    inputsIncomplete = true;
                    findings.Add(new EvidenceFinding("invalidInput", label, 0, string.Empty,
                        read.Failure == "unsupportedContract" ? "The evidence contract version is unsupported." : "The evidence contract is invalid."));
                    continue;
                }

                var artifact = read.Artifact;
                var state = "current";
                foreach (var input in artifact.Inputs)
                {
                    var path = Contained(root, input.Path);
                    var present = path is not null && File.Exists(path);
                    var matches = present && Hash(File.ReadAllBytes(path!)) == input.Sha256;
                    inputRows.Add(new JsonObject
                    {
                        ["evidence"] = label,
                        ["path"] = input.Path,
                        ["kind"] = input.Kind,
                        ["sha256"] = input.Sha256,
                        ["state"] = matches ? "current" : present ? "stale" : "unavailable",
                    });
                    if (!matches)
                    {
                        state = "stale";
                        inputsUnavailable |= !present;
                        findings.Add(new EvidenceFinding(present ? "staleEvidence" : "invalidInput", input.Path, 0, label,
                            present ? "The input hash differs from the export." : "An indexed input is unavailable."));
                    }
                }

                var assembly = artifact.AssemblyPath is null ? null : Contained(root, artifact.AssemblyPath);
                var assemblyPresent = assembly is not null && File.Exists(assembly);
                if (!assemblyPresent || Hash(File.ReadAllBytes(assembly!)) != artifact.AssemblySha256)
                {
                    state = "mismatched";
                    inputsUnavailable |= !assemblyPresent;
                    findings.Add(new EvidenceFinding("staleEvidence", label, 0, artifact.ProjectPath,
                        "The current assembly bytes cannot be established."));
                }

                if (!artifact.Inputs.Any(static input => input.Kind == "manifest" && input.Path == ".specht/manifest.json"))
                {
                    state = "mismatched";
                    findings.Add(new EvidenceFinding("invalidInput", label, 0, artifact.ProjectPath,
                        "The current manifest identity is absent."));
                }

                artifacts.Add(new ArtifactEntry(label, artifact, state));
                scope["supplied"]!.AsArray().Add(new JsonObject
                {
                    ["path"] = label,
                    ["project"] = artifact.ProjectPath,
                    ["targetFramework"] = artifact.TargetFramework,
                    ["state"] = state,
                    ["sourceFingerprint"] = artifact.SourceFingerprint,
                    ["policyFingerprint"] = artifact.PolicyFingerprint,
                    ["configurationFingerprint"] = artifact.ConfigurationFingerprint,
                    ["assemblySha256"] = artifact.AssemblySha256,
                });
            }

            foreach (var pair in scope["projects"]!.AsArray())
            {
                var project = pair!["path"]!.GetValue<string>();
                var framework = pair["targetFramework"]!.GetValue<string>();
                var matching = artifacts.Where(entry => entry.Artifact.ProjectPath == project
                    && entry.Artifact.TargetFramework == framework && entry.Current).ToArray();
                if (matching.Length == 0)
                {
                    inputsIncomplete = true;
                    findings.Add(new EvidenceFinding("missingCompilation", project, 0, framework,
                        "The declared project and target framework have no current export."));
                }
                else if (matching.Select(static entry => entry.Artifact.ConfigurationFingerprint + entry.Artifact.SourceFingerprint
                    + entry.Artifact.AssemblySha256 + entry.Artifact.PolicyFingerprint).Distinct(StringComparer.Ordinal).Count() > 1)
                {
                    inputsIncomplete = true;
                    findings.Add(new EvidenceFinding("invalidInput", project, 0, framework,
                        "Conflicting current exports name the same project and target framework."));
                }
                else if (matching.Select(static entry => JsonSerializer.Serialize(entry.Artifact.Discovery)
                    + JsonSerializer.Serialize(entry.Artifact.Bindings) + JsonSerializer.Serialize(entry.Artifact.Inputs))
                    .Distinct(StringComparer.Ordinal).Count() > 1)
                {
                    inputsIncomplete = true;
                    findings.Add(new EvidenceFinding("invalidInput", project, 0, framework,
                        "Current exports disagree about bindings, discovery or inputs."));
                }
            }

            var currentArtifacts = artifacts.Where(entry => entry.Current && policy.Projects.Any(project => project.Path == entry.Artifact.ProjectPath
                && project.Frameworks.Contains(entry.Artifact.TargetFramework, StringComparer.Ordinal))).ToArray();
            foreach (var entry in currentArtifacts)
            {
                foreach (var record in claims.Select(static claim => claim.Path).Distinct(StringComparer.Ordinal))
                {
                    if (entry.Artifact.Inputs.Any(input => input.Kind == "record" && input.Path == record))
                    {
                        continue;
                    }

                    inputsIncomplete = true;
                    findings.Add(new EvidenceFinding("missingCompilation", record, 0, entry.Label,
                        "A selected record is absent from this compilation's indexed policy inputs."));
                }
            }

            var availableRuns = currentArtifacts.Where(static entry => entry.Artifact.Results.State == "available")
                .Select(static entry => entry.Artifact.Results.RunId!).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
            if (runId is not null && !availableRuns.Contains(runId, StringComparer.Ordinal))
            {
                explicitlyMissing = true;
                findings.Add(new EvidenceFinding("missingRun", "", 0, runId, "The requested run was not found."));
            }
            else if (runId is null && availableRuns.Length > 1)
            {
                inputsIncomplete = true;
                findings.Add(new EvidenceFinding("ambiguousRun", "", 0, "", "Several runs are available; select one with --run."));
            }

            var selectedRun = runId ?? (availableRuns.Length == 1 ? availableRuns[0] : null);
            foreach (var entry in artifacts.OrderBy(static item => item.Label, StringComparer.Ordinal))
            {
                var artifact = entry.Artifact;
                var discoveryCurrent = entry.Current && artifact.Discovery.State == TestEvidenceDiscoveryState.Complete;
                if (entry.Current && !discoveryCurrent)
                {
                    inputsIncomplete = true;
                    inputsUnavailable |= artifact.Discovery.State is TestEvidenceDiscoveryState.Unavailable
                        or TestEvidenceDiscoveryState.UnsupportedContract;
                    findings.Add(new EvidenceFinding("deferredCases", entry.Label, 0, artifact.ProjectPath,
                        "Current case enumeration is unavailable or deferred."));
                }

                foreach (var binding in artifact.Bindings)
                {
                    bindingRows.Add(new JsonObject
                    {
                        ["evidence"] = entry.Label,
                        ["project"] = artifact.ProjectPath,
                        ["targetFramework"] = artifact.TargetFramework,
                        ["symbolId"] = binding.SymbolId,
                        ["role"] = binding.Role,
                        ["status"] = binding.Status,
                        ["shapeValid"] = binding.ShapeValid,
                        ["isEligible"] = binding.IsEligible,
                        ["current"] = entry.Current,
                        ["evidenceState"] = entry.State,
                        ["attributes"] = JsonSerializer.SerializeToNode(binding.Attributes, WireJson),
                        ["location"] = JsonSerializer.SerializeToNode(binding.Location, WireJson),
                        ["target"] = binding.Target is null ? null : JsonSerializer.SerializeToNode(binding.Target, WireJson),
                        ["matches"] = JsonSerializer.SerializeToNode(binding.Matches, WireJson),
                    });
                }

                foreach (var testCase in artifact.Discovery.Cases)
                {
                    var result = artifact.Results.Cases.FirstOrDefault(item => item.CaseId == testCase.CaseId);
                    var outcome = entry.Current && artifact.Results.RunId == selectedRun && result is not null
                        ? result.Outcome : "absent";
                    caseRows.Add(new JsonObject
                    {
                        ["evidence"] = entry.Label,
                        ["project"] = artifact.ProjectPath,
                        ["targetFramework"] = artifact.TargetFramework,
                        ["caseId"] = testCase.CaseId,
                        ["methodSymbolId"] = testCase.MethodSymbolId,
                        ["rowId"] = testCase.RowId,
                        ["displayName"] = testCase.DisplayName,
                        ["discoveryState"] = discoveryCurrent ? "current" : entry.State,
                        ["runId"] = artifact.Results.RunId,
                        ["outcome"] = outcome,
                        ["executionState"] = entry.Current ? artifact.Results.State : entry.State,
                        ["scenarios"] = new JsonArray(),
                    });
                }

                runRows.Add(new JsonObject
                {
                    ["evidence"] = entry.Label,
                    ["project"] = artifact.ProjectPath,
                    ["targetFramework"] = artifact.TargetFramework,
                    ["runId"] = artifact.Results.RunId,
                    ["state"] = entry.Current ? artifact.Results.State : entry.State,
                    ["selected"] = entry.Current && artifact.Results.RunId == selectedRun,
                    ["cases"] = JsonSerializer.SerializeToNode(artifact.Results.Cases, WireJson),
                });
            }

            foreach (var claim in claims)
            {
                foreach (var kind in policy.RequiredKinds)
                {
                    if (kind == "scenario")
                    {
                        continue;
                    }

                    var exemption = policy.Exemption(claim.Epic, claim.Feature, claim.Id, kind);
                    var matching = currentArtifacts.SelectMany(entry => entry.Artifact.Bindings.Select(binding => (entry, binding)))
                        .Where(pair => pair.binding.Status == "resolved" && pair.binding.IsEligible == true
                            && pair.binding.Target is { Kind: "claim" } target && target.Epic == claim.Epic
                            && target.Feature == claim.Feature && target.Id == claim.Id
                            && pair.binding.Role == Role(kind)).ToArray();
                    var state = EvidenceGate.LinkState(exemption is not null, matching.Length > 0, inputsIncomplete);
                    claim.Links[kind] = new JsonObject { ["state"] = state, ["reason"] = exemption };
                    if (state == "missing")
                    {
                        findings.Add(new EvidenceFinding("missingRequiredLink", claim.Path, claim.Line, claim.Identity,
                            $"Required {kind} evidence is missing."));
                    }

                    if (kind != "test" || exemption is not null || matching.Length == 0)
                    {
                        continue;
                    }

                    var caseKeys = matching.Where(pair => selectedRun is null || pair.entry.Artifact.Results.RunId == selectedRun)
                        .SelectMany(pair => pair.entry.Artifact.Discovery.Cases
                        .Where(testCase => testCase.MethodSymbolId == pair.binding.SymbolId)
                        .Select(testCase => (pair.entry.Label, testCase.CaseId))).Distinct().ToArray();
                    if (caseKeys.Length == 0 && !inputsIncomplete)
                    {
                        findings.Add(new EvidenceFinding("missingResult", claim.Path, claim.Line, claim.Identity,
                            "A required test binding has no discovered case."));
                    }

                    foreach (var (label, caseId) in caseKeys)
                    {
                        claim.Cases.Add(caseId);
                        var caseRow = caseRows.Single(row => row["evidence"]!.GetValue<string>() == label
                            && row["caseId"]!.GetValue<string>() == caseId);
                        if (!EvidenceGate.Passed(caseRow["outcome"]!.GetValue<string>()) && !inputsIncomplete)
                        {
                            findings.Add(new EvidenceFinding("missingResult", claim.Path, claim.Line, claim.Identity,
                                $"Required case {caseId} did not pass."));
                        }
                    }
                }

                foreach (var entry in currentArtifacts)
                {
                    foreach (var scenario in entry.Artifact.Scenarios)
                    {
                        foreach (var caseId in EvidenceScenarioJoin.Cases(scenario, claim.Epic, claim.Feature, claim.Id,
                            entry.Artifact.Discovery.Cases.Select(static testCase => testCase.CaseId).ToArray()))
                        {
                            var scenarioId = EvidenceScenarioJoin.Identity(scenario);
                            claim.Scenarios.Add(scenarioId);
                            var linkedCase = caseRows.Single(row => row["evidence"]!.GetValue<string>() == entry.Label
                                && row["caseId"]!.GetValue<string>() == caseId);
                            if (!linkedCase["scenarios"]!.AsArray().Any(item => item?.GetValue<string>() == scenarioId))
                            {
                                linkedCase["scenarios"]!.AsArray().Add(scenarioId);
                            }
                        }
                    }
                }

                if (policy.RequiredKinds.Contains("scenario", StringComparer.Ordinal))
                {
                    var reason = policy.Exemption(claim.Epic, claim.Feature, claim.Id, "scenario");
                    var state = EvidenceGate.LinkState(reason is not null, claim.Scenarios.Count > 0, inputsIncomplete);
                    claim.Links["scenario"] = new JsonObject { ["state"] = state, ["reason"] = reason };
                    if (state == "missing")
                    {
                        findings.Add(new EvidenceFinding("missingRequiredLink", claim.Path, claim.Line, claim.Identity,
                            "Required scenario evidence is missing."));
                    }
                }
            }
        }
        catch (EvidenceMissingInputException)
        {
            inputsIncomplete = true;
            inputsUnavailable = true;
            findings.Add(new EvidenceFinding("missingCompilation", "", 0, "",
                "The repository root or manifest is unavailable."));
        }
        catch (Exception exception) when (exception is EvidenceConfigurationException or JsonException or InvalidOperationException
            or ArgumentException or IOException or UnauthorizedAccessException)
        {
            configurationInvalid = true;
            findings.Add(new EvidenceFinding("invalidInput", ".specht/manifest.json", 0, "",
                "The analysis configuration or selected records are invalid."));
        }

        var exit = EvidenceGate.ExitCode(configurationInvalid, explicitlyMissing, inputsIncomplete,
            findings.Any(static finding => finding.Kind is "missingRequiredLink" or "missingResult"));
        var stateName = configurationInvalid ? "invalid" : inputsUnavailable && inputsIncomplete ? "unavailable"
            : inputsIncomplete || explicitlyMissing ? "incomplete" : "complete";
        var orderedClaims = claims.OrderBy(static claim => claim.Epic, StringComparer.Ordinal)
            .ThenBy(static claim => claim.Feature, StringComparer.Ordinal).ThenBy(static claim => claim.Id, StringComparer.Ordinal).ToArray();
        var links = claims.SelectMany(static claim => claim.Links.Select(static pair => pair.Value?["state"]?.GetValue<string>())).ToArray();
        var report = new JsonObject
        {
            ["format"] = "specht.evidence-report",
            ["formatVersion"] = 1,
            ["scope"] = scope,
            ["inputState"] = new JsonObject { ["state"] = stateName, ["inputs"] = Array(inputRows) },
            ["claims"] = new JsonArray(orderedClaims.Select(static claim => (JsonNode?)claim.ToJson()).ToArray()),
            ["bindings"] = Array(bindingRows.OrderBy(static row => row["project"]!.GetValue<string>(), StringComparer.Ordinal)
                .ThenBy(static row => row["symbolId"]!.GetValue<string>(), StringComparer.Ordinal)),
            ["cases"] = Array(caseRows.OrderBy(static row => row["project"]!.GetValue<string>(), StringComparer.Ordinal)
                .ThenBy(static row => row["caseId"]!.GetValue<string>(), StringComparer.Ordinal)),
            ["runs"] = Array(runRows.OrderBy(static row => row["project"]!.GetValue<string>(), StringComparer.Ordinal)
                .ThenBy(static row => row["runId"]?.GetValue<string>(), StringComparer.Ordinal)),
            ["findings"] = new JsonArray(findings.OrderBy(static finding => finding.Path, StringComparer.Ordinal)
                .ThenBy(static finding => finding.Line).ThenBy(static finding => finding.Kind, StringComparer.Ordinal)
                .ThenBy(static finding => finding.Identity, StringComparer.Ordinal)
                .Select(static finding => (JsonNode?)finding.ToJson()).ToArray()),
            ["summary"] = new JsonObject
            {
                ["selectedClaims"] = claims.Count,
                ["coveredClaims"] = claims.Count(claim => claim.Links["test"]?["state"]?.GetValue<string>() == "satisfied"),
                ["exemptClaims"] = claims.Count(claim => claim.Links["test"]?["state"]?.GetValue<string>() == "exempt"),
                ["passed"] = caseRows.Count(row => row["outcome"]?.GetValue<string>() == "passed"),
                ["failed"] = caseRows.Count(row => row["outcome"]?.GetValue<string>() == "failed"),
                ["findings"] = findings.Count,
                ["exitCode"] = exit,
                ["inputs"] = new JsonObject
                {
                    ["current"] = inputRows.Count(row => row["state"]?.GetValue<string>() == "current"),
                    ["stale"] = inputRows.Count(row => row["state"]?.GetValue<string>() == "stale"),
                    ["unavailable"] = inputRows.Count(row => row["state"]?.GetValue<string>() == "unavailable"),
                },
                ["bindings"] = new JsonObject
                {
                    ["resolved"] = bindingRows.Count(row => row["status"]?.GetValue<string>() == "resolved"),
                    ["unresolved"] = bindingRows.Count(row => row["status"]?.GetValue<string>() == "unresolved"),
                    ["invalid"] = bindingRows.Count(row => row["status"]?.GetValue<string>() == "invalid"),
                    ["ambiguous"] = bindingRows.Count(row => row["status"]?.GetValue<string>() == "ambiguous"),
                    ["ineligible"] = bindingRows.Count(row => row["isEligible"]?.GetValue<bool>() == false),
                },
                ["links"] = new JsonObject
                {
                    ["satisfied"] = links.Count(static state => state == "satisfied"),
                    ["missing"] = links.Count(static state => state == "missing"),
                    ["exempt"] = links.Count(static state => state == "exempt"),
                    ["unavailable"] = links.Count(static state => state == "unavailable"),
                },
                ["execution"] = new JsonObject
                {
                    ["passed"] = caseRows.Count(row => row["outcome"]?.GetValue<string>() == "passed"),
                    ["failed"] = caseRows.Count(row => row["outcome"]?.GetValue<string>() == "failed"),
                    ["skipped"] = caseRows.Count(row => row["outcome"]?.GetValue<string>() == "skipped"),
                    ["notRun"] = caseRows.Count(row => row["outcome"]?.GetValue<string>() == "notRun"),
                    ["aborted"] = caseRows.Count(row => row["outcome"]?.GetValue<string>() == "aborted"),
                    ["absent"] = caseRows.Count(row => row["outcome"]?.GetValue<string>() == "absent"),
                },
            },
        };
        return new EvidenceResult(report, exit);
    }

    /// <implements epic="0201" feature="F5" claim="B-001" />
    private static IEnumerable<ClaimEntry> ReadClaims(string root, JsonObject configuration, EvidencePolicy policy,
        List<EvidenceFinding> findings)
    {
        var layouts = configuration["layouts"] is JsonArray declared
            ? declared.Select(static item => new SpecLayout(item!["name"]!.GetValue<string>(), item["glob"]!.GetValue<string>())).ToArray()
            : [new SpecLayout("co-located", "**/.spec/README.md"), new SpecLayout("legacy", "epics/**/spec.md")];
        var exclusions = configuration["exclusions"] is JsonArray excluded
            ? excluded.Select(static item => item!.GetValue<string>()).ToArray() : System.Array.Empty<string>();
        var inputs = new SpecDiscoveryInputs(layouts, exclusions, [], [], []);
        var discovered = new SpecDiscovery(new FileSystem()).FindSpecifications(root, inputs);
        var found = new HashSet<string>(StringComparer.Ordinal);
        foreach (var location in discovered)
        {
            var document = SpecDocument.Parse(File.ReadAllText(location.AbsolutePath), location.RelativePath);
            var feature = document.Frontmatter.Text(policy.FeatureKey);
            var epic = policy.HasEpic ? document.Frontmatter.Text(policy.EpicKey) : null;
            if (!policy.Features.Any(item => item.Epic == epic && item.Feature == feature))
            {
                continue;
            }

            found.Add((epic ?? "") + "\0" + feature);
            var lifecycle = document.Frontmatter.Text(policy.FeatureLifecycleKey);
            if (lifecycle is not ("draft" or "in-review") && lifecycle != policy.ApprovedMarker)
            {
                continue;
            }

            var section = document.Section(policy.ClaimsSection);
            if (section is null)
            {
                findings.Add(new EvidenceFinding("invalidInput", location.RelativePath, 0, feature!, "The mapped claims section is missing."));
                continue;
            }

            var idColumn = section.Headers.ToList().IndexOf(policy.ClaimIdColumn);
            var stateColumn = section.Headers.ToList().IndexOf(policy.ClaimStatusColumn);
            if (idColumn < 0 || stateColumn < 0)
            {
                findings.Add(new EvidenceFinding("invalidInput", location.RelativePath, section.Line, feature!, "Mapped claim columns are missing."));
                continue;
            }

            for (var index = 0; index < section.Rows.Count; index++)
            {
                var row = section.Rows[index];
                var id = row[idColumn].Trim();
                var state = row[stateColumn].Trim();
                if (state is not ("Active" or "Amended") || state == policy.WithdrawnMarker || state == policy.SupersededMarker)
                {
                    continue;
                }

                if (!policy.Matches("claim", id))
                {
                    findings.Add(new EvidenceFinding("invalidInput", location.RelativePath, section.RowLines[index], id,
                        "A selected claim violates its mapped grammar."));
                    continue;
                }

                yield return new ClaimEntry(epic, feature!, id, state, location.RelativePath, section.RowLines[index]);
            }
        }

        foreach (var selected in policy.Features)
        {
            if (!found.Contains((selected.Epic ?? "") + "\0" + selected.Feature))
            {
                findings.Add(new EvidenceFinding("invalidInput", "", 0, selected.Feature, "A selected Feature record is missing."));
            }
        }
    }

    /// <implements epic="0201" feature="F5" claim="B-001" />
    private static JsonArray Array(IEnumerable<JsonObject> entries) => new(entries.Select(static entry => (JsonNode?)entry).ToArray());

    /// <implements epic="0201" feature="F5" claim="B-001" />
    private static string Role(string kind) => kind switch { "test" => "verifies", "implementation" => "implements", _ => "follows" };

    /// <implements epic="0201" feature="F5" claim="B-001" />
    private static string? Contained(string root, string path)
    {
        if (path.StartsWith('/') || path.Contains('\\') || path.Split('/').Any(static part => part is "" or "." or ".."))
        {
            return null;
        }

        var full = Path.GetFullPath(Path.Combine(root, path));
        return full.StartsWith(Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar,
            StringComparison.Ordinal) ? full : null;
    }

    /// <implements epic="0201" feature="F5" claim="B-001" />
    private static string Label(string root, string path, int index)
    {
        var relative = Path.GetRelativePath(root, path).Replace('\\', '/');
        return relative == ".." || relative.StartsWith("../", StringComparison.Ordinal)
            ? $"external-evidence-{index + 1}" : relative;
    }

    /// <implements epic="0201" feature="F5" claim="B-001" />
    private static string Hash(byte[] bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));

    /// <implements epic="0201" feature="F5" claim="B-001" />
    private sealed record ArtifactEntry(string Label, TestEvidenceArtifact Artifact, string State)
    {
        public bool Current => State == "current";
    }

    /// <implements epic="0201" feature="F5" claim="B-001" />
    private sealed record EvidenceFinding(string Kind, string Path, int Line, string Identity, string Message)
    {
        /// <implements epic="0201" feature="F5" claim="B-001" />
        public JsonObject ToJson() => new()
        {
            ["kind"] = Kind,
            ["path"] = Path,
            ["line"] = Line,
            ["identity"] = Identity,
            ["message"] = Message,
        };
    }

    /// <implements epic="0201" feature="F5" claim="B-001" />
    private sealed class ClaimEntry(string? epic, string feature, string id, string state, string path, int line)
    {
        public string? Epic => epic;
        public string Feature => feature;
        public string Id => id;
        public string Path => path;
        public int Line => line;
        public string Identity => (epic is null ? "" : epic + "-") + feature + " " + id;
        public JsonObject Links { get; } = new();
        public List<string> Cases { get; } = [];
        public List<string> Scenarios { get; } = [];

        /// <implements epic="0201" feature="F5" claim="B-001" />
        public JsonObject ToJson() => new()
        {
            ["epic"] = epic,
            ["feature"] = feature,
            ["id"] = id,
            ["state"] = state,
            ["path"] = path,
            ["line"] = line,
            ["links"] = Links,
            ["cases"] = new JsonArray(Cases.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)
                .Select(static item => (JsonNode?)JsonValue.Create(item)).ToArray()),
            ["scenarios"] = new JsonArray(Scenarios.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)
                .Select(static item => (JsonNode?)JsonValue.Create(item)).ToArray()),
        };
    }

    private static readonly JsonSerializerOptions WireJson = new(JsonSerializerDefaults.Web);
}
