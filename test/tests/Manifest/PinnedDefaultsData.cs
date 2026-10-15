using System.Text.Json.Nodes;
using Specht.Resources;
using Specht.Versioning;

namespace Specht.Tests.Manifest;

/// <summary>Two synthetic version defaults whose filled values differ (0001-F5 B-048).</summary>
internal sealed class PinnedDefaultsData
{
    public PinnedDefaultsData()
    {
        var shipped = new SpechtResources();
        OriginalManifest = shipped.DefaultManifest;
        var resources = shipped.Texts.ToDictionary(StringComparer.Ordinal);
        foreach (var (pin, label) in new[] { ("0.1.0", "Older"), ("0.2.0", "Newer") })
        {
            var defaults = JsonNode.Parse(OriginalManifest)!.AsObject();
            defaults["schemaVersion"] = pin;
            defaults["sections"] = new JsonArray($"3. {label} claims", $"9. {label} matrix", $"12. {label} sign-off");
            defaults["roles"] = new JsonObject
            {
                ["claims"] = $"3. {label} claims",
                ["matrix"] = $"9. {label} matrix",
                ["signOff"] = $"12. {label} sign-off",
            };
            defaults["markers"]!["missing"] = $"{label}Missing";
            defaults["identifiers"]!["claim"] = $"^{label}-[0-9]+$";
            defaults["frontmatterSchemas"]!["feature"] = $"{label}.schema.json";
            defaults["tables"] = new JsonObject { ["matrix"] = new JsonArray($"{label} ID", "Test") };
            defaults["layouts"] = new JsonArray(new JsonObject { ["name"] = label, ["glob"] = $"{label}/**/spec.md" });
            foreach (var key in new[] { "exclusions", "taskFiles", "epicFiles", "companionFiles" })
            {
                defaults[key] = new JsonArray(label);
            }

            defaults["rules"] = new JsonObject { ["SPEC060"] = "warning" };
            defaults["spechtSchema"] = new JsonObject
            {
                ["location"] = "https://schemas.example/synthetic/",
                ["version"] = label,
                ["hashes"] = new JsonObject(),
            };
            resources[$"manifest/v{pin}/manifest.json"] = defaults.ToJsonString();
        }

        Versions = new SchemaVersions(resources);
    }

    public string OriginalManifest { get; }

    public SchemaVersions Versions { get; }
}
