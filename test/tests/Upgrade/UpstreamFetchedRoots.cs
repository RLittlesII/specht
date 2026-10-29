using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using Specht.Versioning;

namespace Specht.Tests.Upgrade;

/// <summary>
/// Manifests recording an upstream schema source the root's copies must be fetched from (<c>0001-F7</c> B-026, B-034, B-082,
/// B-083, B-140), each with the content the synthetic source serves, as a JSON object from file name to text. Each manifest
/// is version 0.2.0's shipped manifest, so it lacks no key that version defines, with a <c>spechtSchema</c> whose
/// <c>location</c> carries user information, which no request may carry (C-15), and whose hashes are the SHA-256 of the
/// served text for each kind. Once under the shipped file names and once under file names of the consumer's, which
/// <c>frontmatterSchemas</c> gives each kind.
/// </summary>
public sealed class UpstreamFetchedRoots : TheoryData<string, string>
{
    public UpstreamFetchedRoots()
    {
        foreach (var prefix in new[] { string.Empty, "consumer-" })
        {
            var manifest = JsonNode.Parse(
                UpgradeServices.Shipped().At(new SemanticVersion(0, 2, 0)).Single(static file => file.Relative == ".specht/manifest.json").Bytes)!.AsObject();
            var served = new JsonObject();
            var hashes = new JsonObject();
            var names = new JsonObject();
            foreach (var kind in new[] { "feature", "task", "epic" })
            {
                var name = $"{prefix}{kind}.schema.json";
                var text = $"{{\"title\":\"A synthetic upstream {kind} schema\"}}\r\n";
                names[kind] = name;
                served[name] = text;
                hashes[kind] = "sha256:" + System.Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
            }

            manifest["frontmatterSchemas"] = names;
            manifest["spechtSchema"] = new JsonObject
            {
                ["location"] = "https://reader:secret@schemas.example/specht/",
                ["version"] = "2026.10",
                ["hashes"] = hashes,
            };
            Add(manifest.ToJsonString(), served.ToJsonString());
        }
    }
}
