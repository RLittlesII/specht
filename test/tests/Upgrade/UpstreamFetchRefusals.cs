using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using Specht.Versioning;

namespace Specht.Tests.Upgrade;

/// <summary>
/// A recorded upstream schema source that refuses one schema (<c>0001-F7</c> B-027, B-035): the manifest, the content the
/// synthetic source serves for every other file, as a JSON object from file name to text, the file refused, and how - served
/// content that fails its hash, an unreachable source, or an HTTP status of 401 or 404. The manifest is version 0.2.0's
/// shipped manifest without <c>claim</c>, so the run plans a manifest write, recording the source at
/// <c>https://schemas.example/specht/</c> with the SHA-256 of the served text for each kind. A refusal of the item or epic
/// schema follows a fetch of the Feature schema that verifies.
/// </summary>
public sealed class UpstreamFetchRefusals : TheoryData<string, string, string, string>
{
    public UpstreamFetchRefusals()
    {
        var manifest = JsonNode.Parse(
            UpgradeServices.Shipped().At(new SemanticVersion(0, 2, 0)).Single(static file => file.Relative == ".specht/manifest.json").Bytes)!.AsObject();
        manifest.Remove("claim");
        var served = new JsonObject();
        var hashes = new JsonObject();
        foreach (var kind in new[] { "feature", "task", "epic" })
        {
            var text = $"{{\"title\":\"A synthetic upstream {kind} schema\"}}\r\n";
            served[$"{kind}.schema.json"] = text;
            hashes[kind] = "sha256:" + System.Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
        }

        manifest["spechtSchema"] = new JsonObject
        {
            ["location"] = "https://schemas.example/specht/",
            ["version"] = "2026.10",
            ["hashes"] = hashes,
        };
        foreach (var (refused, failure) in new[]
                 {
                     ("feature.schema.json", "mismatch"),
                     ("task.schema.json", "mismatch"),
                     ("feature.schema.json", "unreachable"),
                     ("task.schema.json", "401"),
                     ("epic.schema.json", "404"),
                 })
        {
            Add(manifest.ToJsonString(), served.ToJsonString(), refused, failure);
        }
    }
}
