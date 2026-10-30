using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using Specht.Tool.Features;
using Specht.Versioning;

namespace Specht.Tests.Upgrade;

/// <summary>
/// Manifests naming the feature template as the consumer's (<c>0001-F7</c> B-061), each with what the root holds under that
/// template's name: nothing, version 0.2.0's shipped bytes, or bytes of the consumer's. Each manifest is version 0.2.0's
/// shipped manifest with <c>consumerTemplates</c> holding <c>feature.md</c>, so it lacks no key that version defines. Three
/// rows record no upstream schema source; the fourth records one whose schema hashes are the SHA-256 of version 0.2.0's
/// shipped frontmatter schemas, so a root holding them has none to fetch, and whose <c>templates</c> member hashes the
/// feature template by the SHA-256 of synthetic text that is neither the pinned version's nor the consumer's.
/// </summary>
public sealed class NamedTemplateHoldings : TheoryData<string, byte[]?>
{
    public NamedTemplateHoldings()
    {
        var shipped = _shipped.At(new SemanticVersion(0, 2, 0));
        var named = JsonNode.Parse(shipped.Single(static file => file.Relative == ".specht/manifest.json").Bytes)!.AsObject();
        named["consumerTemplates"] = new JsonArray("feature.md");
        var consumers = Encoding.UTF8.GetBytes("# A synthetic template of the consumer's\n");
        Add(named.ToJsonString(), null);
        Add(named.ToJsonString(), shipped.Single(static file => file.Relative == ".specht/templates/feature.md").Bytes);
        Add(named.ToJsonString(), consumers);

        var recorded = named.DeepClone().AsObject();
        recorded["spechtSchema"] = new JsonObject
        {
            ["location"] = "https://schemas.example/specht/",
            ["version"] = "2026.10",
            ["hashes"] = new JsonObject(new[] { "feature", "task", "epic" }.Select(kind => KeyValuePair.Create(
                kind,
                (JsonNode?)("sha256:" + System.Convert.ToHexStringLower(SHA256.HashData(
                    shipped.Single(file => file.Relative == $".specht/schema/frontmatter/{kind}.schema.json").Bytes)))))),
            ["templates"] = new JsonObject
            {
                ["feature.md"] = "sha256:" + System.Convert.ToHexStringLower(SHA256.HashData(
                    Encoding.UTF8.GetBytes("# A synthetic upstream feature.md template\r\n"))),
            },
        };
        Add(recorded.ToJsonString(), consumers);
    }

    private readonly ShippedFiles _shipped = new ShippedFilesFixture();
}
