using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using Specht.Versioning;

namespace Specht.Tests.Upgrade;

/// <summary>
/// Manifests recording an upstream schema source that hashes one template the manifest does not name as the consumer's
/// (<c>0001-F7</c> B-130, B-136, B-137, B-141), each with that template's file name and the text the synthetic source
/// serves for it. Each manifest is version 0.2.0's shipped manifest, so it lacks no key that version defines and names no
/// template in <c>consumerTemplates</c>, with a <c>spechtSchema</c> whose <c>location</c> carries user information, which no
/// request may carry (C-15), whose schema hashes are the SHA-256 of version 0.2.0's shipped frontmatter schemas, so a root
/// holding those schemas has none to fetch, and whose <c>templates</c> member records the SHA-256 of the served text,
/// which is not the pinned version's. Once for the feature template and once for the lesson template.
/// </summary>
public sealed class UpstreamTemplateRoots : TheoryData<string, string, string>
{
    public UpstreamTemplateRoots()
    {
        var shipped = UpgradeServices.Shipped().At(new SemanticVersion(0, 2, 0));
        foreach (var name in new[] { "feature.md", "lesson.md" })
        {
            var manifest = JsonNode.Parse(shipped.Single(static file => file.Relative == ".specht/manifest.json").Bytes)!.AsObject();
            var text = $"# A synthetic upstream {name} template\r\n";
            manifest["spechtSchema"] = new JsonObject
            {
                ["location"] = "https://reader:secret@schemas.example/specht/",
                ["version"] = "2026.10",
                ["hashes"] = new JsonObject(new[] { "feature", "task", "epic" }.Select(kind => KeyValuePair.Create(
                    kind,
                    (JsonNode?)("sha256:" + System.Convert.ToHexStringLower(SHA256.HashData(
                        shipped.Single(file => file.Relative == $".specht/schema/frontmatter/{kind}.schema.json").Bytes)))))),
                ["templates"] = new JsonObject
                {
                    [name] = "sha256:" + System.Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text))),
                },
            };
            Add(manifest.ToJsonString(), name, text);
        }
    }
}
