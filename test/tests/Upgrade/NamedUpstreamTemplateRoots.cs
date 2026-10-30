using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using Specht.Tool.Features;
using Specht.Versioning;

namespace Specht.Tests.Upgrade;

/// <summary>
/// Manifests recording an upstream schema source that hashes one template the manifest names as the consumer's
/// (<c>0001-F7</c> B-135), each with that template's file name. Each manifest is version 0.2.0's shipped manifest, so it
/// lacks no key that version defines, with <c>consumerTemplates</c> holding that file name and a <c>spechtSchema</c> whose
/// schema hashes are the SHA-256 of version 0.2.0's shipped frontmatter schemas, so a root holding them has none to fetch,
/// and whose <c>templates</c> member records the SHA-256 of synthetic text that is neither the pinned version's nor the
/// consumer's. Once for the feature template and once for the lesson template.
/// </summary>
public sealed class NamedUpstreamTemplateRoots : TheoryData<string, string>
{
    public NamedUpstreamTemplateRoots()
    {
        var shipped = _shipped.At(new SemanticVersion(0, 2, 0));
        foreach (var name in new[] { "feature.md", "lesson.md" })
        {
            var manifest = JsonNode.Parse(shipped.Single(static file => file.Relative == ".specht/manifest.json").Bytes)!.AsObject();
            manifest["consumerTemplates"] = new JsonArray(name);
            manifest["spechtSchema"] = new JsonObject
            {
                ["location"] = "https://schemas.example/specht/",
                ["version"] = "2026.10",
                ["hashes"] = new JsonObject(new[] { "feature", "task", "epic" }.Select(kind => KeyValuePair.Create(
                    kind,
                    (JsonNode?)("sha256:" + System.Convert.ToHexStringLower(SHA256.HashData(
                        shipped.Single(file => file.Relative == $".specht/schema/frontmatter/{kind}.schema.json").Bytes)))))),
                ["templates"] = new JsonObject
                {
                    [name] = "sha256:" + System.Convert.ToHexStringLower(
                        SHA256.HashData(Encoding.UTF8.GetBytes($"# A synthetic upstream {name} template\r\n"))),
                },
            };
            Add(manifest.ToJsonString(), name);
        }
    }

    private readonly ShippedFiles _shipped = new ShippedFilesFixture();
}
