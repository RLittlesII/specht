using System.Text.Json.Nodes;
using Specht.Model;
using Specht.Rules;

namespace Specht.Manifest;

/// <summary>Projects the check's manifest and pinned effective rule levels without reading schemas or evaluating rules.</summary>
/// <param name="schemas">The existing manifest loading boundary.</param>
/// <param name="rules">The shared pinned rule catalogue and level precedence.</param>
/// <implements epic="0001" feature="F9" claim="B-005" />
public sealed class EffectiveManifestReader(SpecSchemasLoader schemas, RuleEntriesReader rules)
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
}
