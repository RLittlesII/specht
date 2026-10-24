using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Specht.Analysis.Bindings;

namespace Specht.Analysis.Scenarios;

/// <summary>Maps scenario metadata to the scenario records of the common runner-evidence contract.</summary>
public sealed class ScenarioEvidenceBridge
{
    public ScenarioEvidenceBridge(BindingResolver resolver) => _resolver = resolver;

    /// <implements epic="0201" feature="F4" claim="B-008" />
    public IReadOnlyList<JsonElement> Map(IReadOnlyList<ScenarioMetadata> scenarios) =>
        scenarios.Select(Record).ToArray();

    private static JsonObject? Target(BindingTarget? target) => target is null ? null : new JsonObject
    {
        ["kind"] = target.Kind.ToString().ToLowerInvariant(),
        ["epic"] = target.Epic,
        ["feature"] = target.Feature,
        ["id"] = target.Id,
        ["state"] = target.State,
        ["featureState"] = target.FeatureState,
        ["relativePath"] = target.RelativePath,
        ["line"] = target.Line,
    };

    /// <implements epic="0201" feature="F4" claim="B-002" />
    /// <implements epic="0201" feature="F4" claim="B-004" />
    /// <implements epic="0201" feature="F4" claim="B-011" />
    private static IEnumerable<JsonNode?> Links(ScenarioMetadata scenario)
    {
        foreach (var row in scenario.Rows.Where(row => scenario.GeneratorInputPresent || row.Planned))
        {
            yield return Link(row.RowId, row.CaseId, row.CaseId is null ? "unresolved" : "resolved", row.Planned ? "planned" : "active");
        }

        if (!scenario.GeneratorInputPresent)
        {
            yield return Link(null, null, "unresolved", "active");
        }
    }

    private static JsonObject Link(string? rowId, string? caseId, string status, string state) => new()
    {
        ["rowId"] = rowId,
        ["caseId"] = caseId,
        ["status"] = status,
        ["state"] = state,
    };

    /// <implements epic="0201" feature="F4" claim="B-001" />
    private JsonObject Claim(ScenarioScope scope, string tag)
    {
        var result = _resolver.Resolve(new ClaimReference(scope.Epic, scope.Feature, tag));
        return new JsonObject
        {
            ["id"] = tag,
            ["status"] = result.Status.ToString().ToLowerInvariant(),
            ["isEligible"] = result.IsEligible,
            ["target"] = Target(result.Target),
            ["matches"] = new JsonArray(result.Matches.Select(static target => (JsonNode?)Target(target)).ToArray()),
        };
    }

    /// <implements epic="0201" feature="F4" claim="B-003" />
    private JsonElement Record(ScenarioMetadata scenario)
    {
        var scope = scenario.ScopeOverride ?? scenario.FeatureScope;
        var record = new JsonObject
        {
            ["path"] = scenario.Path,
            ["scenarioId"] = scenario.ScenarioId,
            ["line"] = scenario.Line,
            ["scope"] = new JsonObject { ["epic"] = scope.Epic, ["feature"] = scope.Feature },
            ["claims"] = new JsonArray(scenario.ClaimTags.Select(tag => (JsonNode?)Claim(scope, tag)).ToArray()),
            ["cases"] = new JsonArray(Links(scenario).ToArray()),
        };
        using var document = JsonDocument.Parse(record.ToJsonString());
        return document.RootElement.Clone();
    }

    private readonly BindingResolver _resolver;
}
