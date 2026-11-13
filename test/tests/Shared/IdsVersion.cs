using System.Text.Json.Nodes;
using Specht.Versioning;

namespace Specht.Tests.Shared;

/// <summary>
/// A schema 0.4.0 that holds the seven <c>ids</c> records of <c>0001-F5</c> decision 0024, so the engine's reading of a
/// record is tested apart from the records the tool ships (A-8): the shipped 0.3.1 version, its vocabulary, rule names,
/// frontmatter schemas and pages kept, numbered 0.4.0, with its manifest's <c>identifiers</c> kept to the task, Feature
/// and epic grammars and an <c>ids</c> key added. The catalog holds it in place of any shipped 0.4.0.
/// </summary>
internal static class IdsVersion
{
    /// <summary>Gets the version's number, 0.4.0.</summary>
    public static SemanticVersion Number { get; } = new(0, 4, 0);

    /// <summary>The version's manifest: the shipped 0.3.1 one with the seven grammars moved to <c>ids</c> records.</summary>
    /// <returns>A new manifest object.</returns>
    public static JsonObject Defaults()
    {
        var defaults = JsonNode.Parse(ZeroThreeOne().Manifest)!.AsObject();
        defaults["schemaVersion"] = Number.ToString();
        var identifiers = defaults["identifiers"]!.AsObject();
        foreach (var type in new[] { "claim", "constraint", "openQuestion", "scenario", "decision", "adr", "lesson" })
        {
            identifiers.Remove(type);
        }

        defaults["ids"] = JsonNode.Parse(Records);
        return defaults;
    }

    /// <summary>The shipped versions with this 0.4.0 in place of any shipped one.</summary>
    /// <returns>A catalog.</returns>
    public static SchemaVersions Catalog() =>
        new([.. EngineServices.Versions().Versions.Where(static version => version.Number != Number),
            ZeroThreeOne() with { Number = Number, Manifest = Defaults().ToJsonString() }]);

    private static SchemaVersion ZeroThreeOne() => EngineServices.Versions().Find(new SemanticVersion(0, 3, 1))!;

    private const string Records = """
        {
          "claim": { "form": "{prefix}{id}", "plain": "B-{id}", "id": "[0-9]{3}[a-z]?|[0-9a-hjkmnp-tv-z]{6}|[0-9a-hjkmnp-tv-z]{13}" },
          "constraint": { "plain": "C-{id}", "id": "[0-9]+|[0-9a-hjkmnp-tv-z]{6}|[0-9a-hjkmnp-tv-z]{13}" },
          "openQuestion": { "plain": "OQ-{id}", "id": "[0-9]+|[0-9a-hjkmnp-tv-z]{6}|[0-9a-hjkmnp-tv-z]{13}" },
          "scenario": { "plain": "S-{id}", "id": "[0-9]+|[0-9a-hjkmnp-tv-z]{6}|[0-9a-hjkmnp-tv-z]{13}" },
          "decision": { "plain": "{id}", "id": "[0-9]{4}|[0-9a-hjkmnp-tv-z]{6}|[0-9a-hjkmnp-tv-z]{13}" },
          "adr": { "plain": "{id}", "id": "[0-9]{4}|[0-9a-hjkmnp-tv-z]{8}|[0-9a-hjkmnp-tv-z]{13}" },
          "lesson": { "plain": "{id}", "id": "[0-9]{4}|[0-9a-hjkmnp-tv-z]{8}|[0-9a-hjkmnp-tv-z]{13}" }
        }
        """;
}
