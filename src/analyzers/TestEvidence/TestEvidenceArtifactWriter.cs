using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Specht.Analysis.TestEvidence;

public sealed class TestEvidenceArtifactWriter
{
    public string Write(TestEvidenceArtifact artifact)
    {
        var value = new JsonObject
        {
            ["format"] = "specht.test-evidence",
            ["formatVersion"] = artifact.Discovery.Cases.Any(StatesCaseFacts) ? 3 : artifact.Catalog is null ? 1 : 2,
            ["project"] = new JsonObject { ["path"] = artifact.ProjectPath, ["targetFramework"] = artifact.TargetFramework },
            ["configuration"] = new JsonObject
            {
                ["name"] = artifact.ConfigurationName,
                ["fingerprint"] = artifact.ConfigurationFingerprint,
            },
            ["assembly"] = Assembly(artifact),
            ["sourceFingerprint"] = artifact.SourceFingerprint,
            ["policyFingerprint"] = artifact.PolicyFingerprint,
            ["adapter"] = new JsonObject
            {
                ["id"] = artifact.AdapterId,
                ["version"] = artifact.AdapterVersion,
                ["contractVersion"] = artifact.AdapterContractVersion,
            },
            ["discovery"] = Discovery(artifact.Discovery),
            ["bindings"] = new JsonArray(artifact.Bindings.Select(Binding).ToArray()),
            ["results"] = Results(artifact.Results),
            ["inputs"] = new JsonArray(artifact.Inputs.OrderBy(static input => input.Path, System.StringComparer.Ordinal)
                .ThenBy(static input => input.Kind, System.StringComparer.Ordinal).Select(Input).ToArray()),
        };
        if (artifact.Scenarios.Count > 0)
        {
            value["scenarios"] = new JsonArray(artifact.Scenarios.Select(static scenario =>
                (JsonNode?)JsonNode.Parse(scenario.GetRawText())).ToArray());
        }

        if (artifact.Catalog is not null)
        {
            value["catalog"] = Catalog(artifact.Catalog);
        }

        return value.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
    }

    private static JsonObject Catalog(IReadOnlyList<TestEvidenceCatalogFeature> features) => new()
    {
        ["features"] = new JsonArray(features.Select(static feature => (JsonNode?)new JsonObject
        {
            ["epic"] = feature.Epic,
            ["feature"] = feature.Feature,
            ["state"] = feature.State,
            ["path"] = feature.Path,
            ["claims"] = new JsonArray(feature.Claims.Select(static claim => (JsonNode?)new JsonObject
            {
                ["id"] = claim.Id,
                ["state"] = claim.State,
                ["element"] = claim.Element,
                ["path"] = claim.Path,
                ["line"] = claim.Line,
                ["cases"] = new JsonArray(claim.Cases.Select(static item => (JsonNode?)new JsonObject
                {
                    ["line"] = item.Line,
                    ["planned"] = item.Planned,
                }).ToArray()),
            }).ToArray()),
        }).ToArray()),
    };

    private static JsonObject Assembly(TestEvidenceArtifact artifact)
    {
        var value = new JsonObject { ["name"] = artifact.AssemblyName, ["sha256"] = artifact.AssemblySha256 };
        if (artifact.AssemblyPath is not null)
        {
            value["path"] = artifact.AssemblyPath;
        }

        return value;
    }

    private static JsonObject Discovery(TestEvidenceDiscovery discovery) => new()
    {
        ["state"] = discovery.State.ToString().ToLowerInvariant(),
        ["sessionId"] = discovery.SessionId,
        ["methods"] = new JsonArray(discovery.Methods.Select(static method => new JsonObject
        {
            ["symbolId"] = method.SymbolId,
            ["declaringType"] = method.DeclaringType,
            ["name"] = method.Name,
            ["genericArity"] = method.GenericArity,
            ["parameterTypes"] = new JsonArray(method.ParameterTypes.Select(static parameter => (JsonNode?)JsonValue.Create(parameter)).ToArray()),
            ["location"] = Location(method.Location),
        }).ToArray()),
        ["cases"] = new JsonArray(discovery.Cases.Select(static item => (JsonNode?)Case(item)).ToArray()),
    };

    private static JsonObject Case(TestEvidenceCase item)
    {
        var value = new JsonObject
        {
            ["caseId"] = item.CaseId,
            ["methodSymbolId"] = item.MethodSymbolId,
            ["rowId"] = item.RowId,
            ["displayName"] = item.DisplayName,
        };
        if (StatesCaseFacts(item))
        {
            value["categories"] = new JsonArray(item.Categories!.Select(static category => (JsonNode?)JsonValue.Create(category)).ToArray());
            value["owned"] = item.Owned;
            value["selected"] = item.Selected;
        }

        return value;
    }

    private static bool StatesCaseFacts(TestEvidenceCase item) => item.Categories is not null || item.Owned is not null || item.Selected is not null;

    private static JsonObject Binding(TestEvidenceBinding binding)
    {
        var attributes = new JsonObject();
        foreach (var attribute in binding.Attributes.OrderBy(static item => item.Key, System.StringComparer.Ordinal))
        {
            attributes[attribute.Key] = attribute.Value;
        }

        return new JsonObject
        {
            ["symbolId"] = binding.SymbolId,
            ["role"] = binding.Role,
            ["attributes"] = attributes,
            ["shapeValid"] = binding.ShapeValid,
            ["status"] = binding.Status,
            ["isEligible"] = binding.IsEligible,
            ["target"] = Target(binding.Target),
            ["matches"] = new JsonArray(binding.Matches.Select(static target => (JsonNode?)Target(target)).ToArray()),
            ["location"] = Location(binding.Location),
        };
    }

    private static JsonObject? Target(TestEvidenceTarget? target) => target is null ? null : new JsonObject
    {
        ["kind"] = target.Kind,
        ["epic"] = target.Epic,
        ["feature"] = target.Feature,
        ["id"] = target.Id,
        ["state"] = target.State,
        ["featureState"] = target.FeatureState,
        ["relativePath"] = target.RelativePath,
        ["line"] = target.Line,
    };

    private static JsonObject? Location(TestEvidenceLocation? location) => location is null ? null : new JsonObject
    {
        ["path"] = location.Path,
        ["line"] = location.Line,
        ["column"] = location.Column,
    };

    private static JsonObject Results(TestEvidenceResults results) => results.State == "absent"
        ? new JsonObject { ["state"] = "absent" }
        : new JsonObject
        {
            ["state"] = "available",
            ["runId"] = results.RunId,
            ["cases"] = new JsonArray(results.Cases.Select(static item => (JsonNode?)new JsonObject
            {
                ["caseId"] = item.CaseId,
                ["outcome"] = item.Outcome,
                ["detail"] = item.Detail,
            }).ToArray()),
        };

    private static JsonObject Input(TestEvidenceInput input) => new()
    {
        ["path"] = input.Path,
        ["sha256"] = input.Sha256,
        ["kind"] = input.Kind,
    };
}
