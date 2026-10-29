using System.Text.Json.Nodes;
using Specht.Versioning;

namespace Specht.Tests.Upgrade;

/// <summary>
/// The manifests of <c>0001-F7</c> B-029 whose frontmatter schemas come from the root: the on-disk selection and a recorded
/// upstream source, each over version 0.2.0's shipped manifest, so the manifest lacks no key that version defines (OQ-64 (a)),
/// once declaring the epic grammar with an epic hash and once without <c>identifiers</c> and hashing the Feature and item
/// schemas only, which no version defines either (B-016, B-138, OQ-68).
/// </summary>
public sealed class UpgradeRootSources : TheoryData<string>
{
    public UpgradeRootSources()
    {
        var defined = JsonNode.Parse(
            UpgradeServices.Shipped().At(new SemanticVersion(0, 2, 0)).Single(static file => file.Relative == ".specht/manifest.json").Bytes)!.AsObject();
        var undeclared = defined.DeepClone().AsObject();
        undeclared.Remove("identifiers");
        foreach (var (manifest, hashes) in new[]
                 {
                     (defined, new JsonObject { ["feature"] = Hash, ["task"] = Hash, ["epic"] = Hash }),
                     (undeclared, new JsonObject { ["feature"] = Hash, ["task"] = Hash }),
                 })
        {
            var disk = manifest.DeepClone().AsObject();
            disk["schemaSource"] = "disk";
            Add(disk.ToJsonString());
            var upstream = manifest.DeepClone().AsObject();
            upstream["spechtSchema"] = new JsonObject
            {
                ["location"] = "https://schemas.example/specht/",
                ["version"] = "v1",
                ["hashes"] = hashes,
            };
            Add(upstream.ToJsonString());
        }
    }

    private const string Hash = "sha256:0000000000000000000000000000000000000000000000000000000000000000";
}
