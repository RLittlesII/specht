using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using Specht.Tool.Features;
using Specht.Versioning;

namespace Specht.Tests.Upgrade;

/// <summary>
/// The manifests of <c>0001-F7</c> B-029 whose frontmatter schemas come from the root, each with the text of the root's
/// hand-edited schemas: the on-disk selection and a recorded upstream source, each over version 0.2.0's shipped manifest, so
/// the manifest lacks no key that version defines (OQ-64 (a)), once declaring the epic grammar with an epic hash and once
/// without <c>identifiers</c> and hashing the Feature and item schemas only, which no version defines either (B-016, B-138,
/// OQ-68). A recorded source's hashes are the SHA-256 of the edited text, so each local copy matches its hash and is current
/// (B-033): one that differed would be fetched (B-026).
/// </summary>
public sealed class UpgradeRootSources : TheoryData<string, string>
{
    public UpgradeRootSources()
    {
        const string edited = "{\"title\":\"A synthetic consumer schema\"}\r\n";
        var hash = "sha256:" + System.Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(edited)));
        var defined = JsonNode.Parse(
            _shipped.At(new SemanticVersion(0, 2, 0)).Single(static file => file.Relative == ".specht/manifest.json").Bytes)!.AsObject();
        var undeclared = defined.DeepClone().AsObject();
        undeclared.Remove("identifiers");
        foreach (var (manifest, hashes) in new[]
                 {
                     (defined, new JsonObject { ["feature"] = hash, ["task"] = hash, ["epic"] = hash }),
                     (undeclared, new JsonObject { ["feature"] = hash, ["task"] = hash }),
                 })
        {
            var disk = manifest.DeepClone().AsObject();
            disk["schemaSource"] = "disk";
            Add(disk.ToJsonString(), edited);
            var upstream = manifest.DeepClone().AsObject();
            upstream["spechtSchema"] = new JsonObject
            {
                ["location"] = "https://schemas.example/specht/",
                ["version"] = "v1",
                ["hashes"] = hashes,
            };
            Add(upstream.ToJsonString(), edited);
        }
    }

    private readonly ShippedFiles _shipped = new ShippedFilesFixture();
}
