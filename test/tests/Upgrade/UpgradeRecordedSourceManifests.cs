using System.Security.Cryptography;
using System.Text.Json.Nodes;
using Specht.Versioning;

namespace Specht.Tests.Upgrade;

/// <summary>
/// Manifests of <c>0001-F7</c> B-143 recording an upstream schema source, over a root holding version 0.2.0's shipped
/// frontmatter schemas and templates: version 0.2.0's shipped manifest whose <c>spechtSchema</c> hashes are the SHA-256 of
/// those schemas, so every local copy matches and nothing is fetched (B-033), once selecting no source and once with
/// <c>schemaSource</c> set to <c>disk</c>.
/// </summary>
public sealed class UpgradeRecordedSourceManifests : TheoryData<string>
{
    public UpgradeRecordedSourceManifests()
    {
        var shipped = UpgradeServices.Shipped().At(new SemanticVersion(0, 2, 0));
        var recorded = JsonNode.Parse(shipped.Single(static file => file.Relative == ".specht/manifest.json").Bytes)!.AsObject();
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

        var disk = recorded.DeepClone().AsObject();
        disk["schemaSource"] = "disk";
        Add(disk.ToJsonString());
    }
}
