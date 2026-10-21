using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Specht.Tool.Features.Evidence;

/// <summary>Resolves only explicit current scenario-to-claim-to-case links.</summary>
/// <implements epic="0201" feature="F5" claim="B-005" />
internal static class EvidenceScenarioJoin
{
    /// <implements epic="0201" feature="F5" claim="B-005" />
    public static string Identity(JsonElement scenario)
    {
        var path = scenario.GetProperty("path").GetString();
        var id = scenario.GetProperty("scenarioId").GetString();
        return id is null ? path + ":" + scenario.GetProperty("line").GetInt32() : path + "#" + id;
    }

    /// <implements epic="0201" feature="F5" claim="B-005" />
    public static string[] Cases(JsonElement scenario, string? epic, string feature, string claim, IReadOnlyList<string> availableCases)
    {
        var scope = scenario.GetProperty("scope");
        if (scope.GetProperty("epic").GetString() != epic || scope.GetProperty("feature").GetString() != feature
            || !scenario.GetProperty("claims").EnumerateArray().Any(item => item.GetProperty("status").GetString() == "resolved"
                && item.GetProperty("isEligible").GetBoolean() && item.GetProperty("id").GetString() == claim))
        {
            return [];
        }

        return scenario.GetProperty("cases").EnumerateArray()
            .Where(static item => item.GetProperty("status").GetString() == "resolved")
            .Select(static item => item.GetProperty("caseId").GetString()!)
            .Where(caseId => availableCases.Contains(caseId, StringComparer.Ordinal))
            .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
    }
}
