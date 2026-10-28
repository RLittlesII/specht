using System.Text.Json;
using System.Text.Json.Nodes;
using Specht.Model;
using Specht.Rules;
using Specht.Versioning;

namespace Specht.Manifest;

/// <summary>Projects the check's manifest and pinned effective rule levels without reading schemas or evaluating rules.</summary>
/// <param name="schemas">The existing manifest loading boundary.</param>
/// <param name="rules">The shared pinned rule catalogue and level precedence.</param>
/// <param name="versions">The embedded manifests that define printed key order.</param>
/// <implements epic="0001" feature="F9" claim="B-005" />
public sealed class EffectiveManifestReader(SpecSchemasLoader schemas, RuleEntriesReader rules, SchemaVersions versions)
{
    /// <summary>Returns the root's effective manifest as an owned JSON object.</summary>
    /// <param name="root">The repository root.</param>
    /// <returns>The preserved declarations, selected defaults and effective rule settings.</returns>
    /// <implements epic="0001" feature="F9" claim="B-003" />
    /// <implements epic="0001" feature="F9" claim="B-004" />
    /// <implements epic="0001" feature="F9" claim="B-005" />
    /// <implements epic="0001" feature="F9" claim="B-006" />
    /// <implements epic="0001" feature="F9" claim="B-007" />
    public JsonObject Read(string root)
    {
        var manifest = schemas.LoadManifestDocument(root);
        var levels = new JsonObject();
        foreach (var entry in rules.Read(manifest.Structure))
        {
            levels[entry.Id] = entry.EffectiveLevel.ToString().ToLowerInvariant();
        }

        manifest.Document["rules"] = levels;
        return manifest.Document;
    }

    /// <summary>Returns the effective document as JSON in its pinned default's key order with the selected indentation.</summary>
    /// <implements epic="0001" feature="F9" claim="B-020" />
    public string ReadText(string root, bool indented = true)
    {
        var document = Read(root);
        SemanticVersion.TryParse(document["schemaVersion"]!.GetValue<string>(), out var pin);
        return Format(document, versions.Find(pin)!, indented);
    }

    /// <summary>Returns the newest embedded default without reading a root or projecting effective settings.</summary>
    /// <implements epic="0001" feature="F9" claim="B-008" />
    /// <implements epic="0001" feature="F9" claim="B-009" />
    /// <implements epic="0001" feature="F9" claim="B-012" />
    public string ReadDefaultText(bool indented = true)
    {
        var version = versions.Versions[^1];
        return Format(JsonNode.Parse(version.Manifest)!.AsObject(), version, indented);
    }

    /// <implements epic="0001" feature="F9" claim="B-020" />
    private static string Format(JsonObject document, SchemaVersion version, bool indented)
    {
        var defaults = JsonNode.Parse(version.Manifest)!.AsObject();
        var ordered = new JsonObject();
        foreach (var key in defaults.Select(static entry => entry.Key).Where(document.ContainsKey)
            .Concat(document.Select(static entry => entry.Key).Where(key => !defaults.ContainsKey(key)).Order(StringComparer.Ordinal)))
        {
            if (key == "rules" && document[key] is JsonObject levels)
            {
                var orderedLevels = new JsonObject();
                foreach (var entry in levels.OrderBy(static entry => entry.Key, StringComparer.Ordinal))
                {
                    orderedLevels[entry.Key] = entry.Value?.DeepClone();
                }
                ordered[key] = orderedLevels;
            }
            else
            {
                ordered[key] = document[key]?.DeepClone();
            }
        }
        return ordered.ToJsonString(new JsonSerializerOptions { WriteIndented = indented });
    }
}
