using System.Security.Cryptography;
using System.Text.Json.Nodes;
using Specht.Tool.Features;
using Specht.Versioning;

namespace Specht.Tests.Upgrade;

/// <summary>
/// Manifests pinned to version 0.2.0 that declare no epic grammar over a root holding version 0.1.0's Feature and item schemas
/// and no epic schema (<c>0001-F7</c> B-138, OQ-68): a pin with the on-disk selection; version 0.2.0's shipped manifest without
/// <c>identifiers</c>, recording an upstream source that hashes those two schemas; and that manifest with an
/// <c>identifiers</c> that has no <c>epic</c> member, under the on-disk selection.
/// </summary>
public sealed class UndeclaredEpicGrammarManifests : TheoryData<string>
{
    public UndeclaredEpicGrammarManifests()
    {
        ShippedFiles shipped = new ShippedFilesFixture();
        var defined = JsonNode.Parse(
            shipped.At(new SemanticVersion(0, 2, 0)).Single(static file => file.Relative == ".specht/manifest.json").Bytes)!.AsObject();
        var schemas = shipped.At(new SemanticVersion(0, 1, 0)).ToDictionary(static file => file.Relative, static file => file.Bytes, StringComparer.Ordinal);

        Add("{\"schemaVersion\":\"0.2.0\",\"schemaSource\":\"disk\"}");

        var upstream = defined.DeepClone().AsObject();
        upstream.Remove("identifiers");
        upstream["spechtSchema"] = new JsonObject
        {
            ["location"] = "https://schemas.example/specht/",
            ["version"] = "v1",
            ["hashes"] = new JsonObject
            {
                ["feature"] = "sha256:" + System.Convert.ToHexStringLower(SHA256.HashData(schemas[".specht/schema/frontmatter/feature.schema.json"])),
                ["task"] = "sha256:" + System.Convert.ToHexStringLower(SHA256.HashData(schemas[".specht/schema/frontmatter/task.schema.json"])),
            },
        };
        Add(upstream.ToJsonString());

        var partial = defined.DeepClone().AsObject();
        partial["identifiers"]!.AsObject().Remove("epic");
        partial["schemaSource"] = "disk";
        Add(partial.ToJsonString());
    }
}
