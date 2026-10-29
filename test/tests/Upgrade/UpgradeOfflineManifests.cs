using System.Security.Cryptography;
using System.Text.Json.Nodes;
using Specht.Versioning;

namespace Specht.Tests.Upgrade;

/// <summary>
/// Manifests under which <c>specht upgrade</c> has nothing to fetch, over a root holding version 0.2.0's shipped frontmatter
/// schemas and templates (<c>0001-F7</c> B-033, B-067, B-134): version 0.2.0's shipped manifest recording an upstream schema
/// source whose hashes are the SHA-256 of those schemas, so every local copy matches; the same source also hashing every
/// template with the SHA-256 of its shipped bytes, naming none as the consumer's, so every template matches too; that
/// manifest recording no source, so the embedded source is selected; and that manifest recording no source with the on-disk
/// source selected.
/// </summary>
public sealed class UpgradeOfflineManifests : TheoryData<string>
{
    public UpgradeOfflineManifests()
    {
        var shipped = UpgradeServices.Shipped().At(new SemanticVersion(0, 2, 0));
        var defined = JsonNode.Parse(shipped.Single(static file => file.Relative == ".specht/manifest.json").Bytes)!.AsObject();

        var recorded = defined.DeepClone().AsObject();
        recorded["spechtSchema"] = new JsonObject
        {
            ["location"] = "https://schemas.example/specht/",
            ["version"] = "2026.10",
            ["hashes"] = new JsonObject(new[] { "feature", "task", "epic" }.Select(kind => KeyValuePair.Create(
                kind,
                (JsonNode?)("sha256:" + System.Convert.ToHexStringLower(SHA256.HashData(
                    shipped.Single(file => file.Relative == $".specht/schema/frontmatter/{kind}.schema.json").Bytes)))))),
        };
        Add(recorded.ToJsonString());

        var templates = recorded.DeepClone().AsObject();
        templates["spechtSchema"]!["templates"] = new JsonObject(shipped
            .Where(static file => file.Relative.StartsWith(".specht/templates/", StringComparison.Ordinal))
            .Select(static file => KeyValuePair.Create(
                file.Relative[".specht/templates/".Length..],
                (JsonNode?)("sha256:" + System.Convert.ToHexStringLower(SHA256.HashData(file.Bytes))))));
        Add(templates.ToJsonString());

        Add(defined.ToJsonString());

        var disk = defined.DeepClone().AsObject();
        disk["schemaSource"] = "disk";
        Add(disk.ToJsonString());
    }
}
