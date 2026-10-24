using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Specht.Analysis.Policy;

namespace Specht.Analysis.Bindings.Gherkin;

/// <summary>Reads the native claim keys of the consumer manifest into a policy, or none under the Markdown claims table.</summary>
/// <implements epic="0201" feature="F7" claim="B-001" />
public sealed class GherkinClaimPolicyReader
{
    public GherkinClaimPolicy? Read(
        JsonNode? claim, JsonObject? records, JsonObject? scopes, IReadOnlyDictionary<string, string> identifiers, bool declaresEpic)
    {
        if (records?["claimSource"] is not { } source)
        {
            return null;
        }

        if (source is not JsonValue value || !value.TryGetValue<string>(out var name) || name != "gherkin")
        {
            throw new GherkinInputException(null, "codeAnalysis.records.claimSource must be \"gherkin\".");
        }

        var form = new GherkinTagForm(Tag(claim), "claim", false, identifiers["claim"]);
        var ignoreCase = claim!["tagIgnoreCase"] is not { } node ? false
            : node is JsonValue flag && flag.TryGetValue<bool>(out var folded) ? folded
            : throw new GherkinInputException(null, "claim.tagIgnoreCase must be a Boolean.");
        return new GherkinClaimPolicy(
            form with { IgnoreCase = ignoreCase },
            Scope(scopes, "feature"), Scope(scopes, "epic"),
            Tags(records["plannedTags"], ["@ignore"]), Tags(records["withdrawnTags"], []), Tags(records["supersededTags"], []),
            Sets(records["claimFileSets"], identifiers, declaresEpic));
    }

    private static string Tag(JsonNode? claim) => claim is JsonObject mapped && mapped["tag"] is JsonValue tag && tag.TryGetValue<string>(out var text)
        ? text : throw new GherkinInputException(null, "Native claim input requires claim.tag, mapped as /claim.");

    private static GherkinTagForm? Scope(JsonObject? scopes, string role) =>
        scopes?[role + "Tag"]?.GetValue<string>() is { } form ? new GherkinTagForm(form, role, false, null) : null;

    private static IReadOnlyList<string> Tags(JsonNode? node, string[] fallback)
    {
        if (node is null)
        {
            return fallback;
        }

        var tags = new List<string>();
        foreach (var entry in node as JsonArray ?? throw TagLists())
        {
            if (entry is not JsonValue value || !value.TryGetValue<string>(out var tag) || tag.Length < 2 || tag[0] != '@' || tag.Any(char.IsWhiteSpace))
            {
                throw TagLists();
            }

            tags.Add(tag);
        }

        return Array.AsReadOnly(tags.ToArray());
    }

    private static string Text(JsonNode? entry, string key) => entry is JsonObject item && item[key] is JsonValue value
        && value.TryGetValue<string>(out var text) && !string.IsNullOrWhiteSpace(text) ? text : throw InvalidSets();

    private static bool Matches(string grammar, string value) =>
        Regex.IsMatch(value, "\\A(?:" + grammar + ")\\z", RegexOptions.CultureInvariant);

    private static GherkinInputException TagLists() => new(null, "Claim tag lists must hold @-led tags without whitespace.");

    private static GherkinInputException InvalidSets() => new(null, "codeAnalysis.records.claimFileSets is missing or invalid.");

    private IReadOnlyList<GherkinClaimFileSet> Sets(JsonNode? node, IReadOnlyDictionary<string, string> identifiers, bool declaresEpic)
    {
        if (node is not JsonArray array || array.Count == 0)
        {
            throw InvalidSets();
        }

        var sets = new List<GherkinClaimFileSet>();
        foreach (var entry in array)
        {
            var glob = Text(entry, "glob");
            var feature = Text(entry, "feature");
            var epic = entry?["epic"] is null ? null : Text(entry, "epic");
            var record = entry?["record"] is null ? null : Text(entry, "record");
            try
            {
                _ = _paths.Normalize(glob);
                _ = record is null ? null : _paths.Normalize(record);
            }
            catch (ArgumentException)
            {
                throw InvalidSets();
            }

            if (!Matches(identifiers["feature"], feature) || declaresEpic != (epic is not null)
                || epic is not null && !Matches(identifiers["epic"], epic))
            {
                throw InvalidSets();
            }

            sets.Add(new GherkinClaimFileSet(glob, feature, epic, record));
        }

        if (sets.GroupBy(static set => (set.Epic, set.Feature)).Any(static group => group.Count() > 1))
        {
            throw new GherkinInputException(null, "Two claim file sets name one Feature.");
        }

        return Array.AsReadOnly(sets.ToArray());
    }

    private readonly AnalysisPaths _paths = new();
}
