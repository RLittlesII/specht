using System.Text.Json.Nodes;
using Specht.Versioning;

namespace Specht.Tests.Upgrade;

/// <summary>
/// The listings of <c>0001-F7</c> B-006, B-061, B-062 and B-142 for a root pinned to version 0.2.0 that holds that version's
/// frontmatter schemas and templates: a manifest naming templates as the consumer's, the file names of the templates edited
/// by hand, the file names of the templates the root lacks, and the standard output's lines. A named template edited, left
/// as the pinned version's, or absent is skipped and the root is current (B-006, B-142); beside an unnamed template edited by
/// hand, the skip lines follow the line naming it (B-061, B-062); and under the on-disk source with no upstream schema
/// source recorded, the schema skips and the template skip are one listing in ordinal path order. Each manifest is version
/// 0.2.0's shipped manifest with <c>consumerTemplates</c> set, so it lacks no key that version defines.
/// </summary>
public sealed class ConsumerTemplateListings : TheoryData<string, string[], string[], string[]>
{
    public ConsumerTemplateListings()
    {
        var defined = JsonNode.Parse(
            UpgradeServices.Shipped().At(new SemanticVersion(0, 2, 0)).Single(static file => file.Relative == ".specht/manifest.json").Bytes)!.AsObject();
        const string current = ".specht/ is current at schemaVersion 0.2.0";
        const string feature = "skipped .specht/templates/feature.md: named in consumerTemplates";

        var named = defined.DeepClone().AsObject();
        named["consumerTemplates"] = new JsonArray("feature.md");
        Add(named.ToJsonString(), ["feature.md"], [], [current, feature]);
        Add(named.ToJsonString(), [], [], [current, feature]);
        Add(named.ToJsonString(), [], ["feature.md"], [current, feature]);

        var two = defined.DeepClone().AsObject();
        two["consumerTemplates"] = new JsonArray("lesson.md", "feature.md");
        Add(
            two.ToJsonString(),
            ["adr.md", "feature.md"],
            [],
            [
                ".specht/ rewritten to schemaVersion 0.2.0",
                "rewritten .specht/templates/adr.md",
                feature,
                "skipped .specht/templates/lesson.md: named in consumerTemplates",
            ]);

        var disk = named.DeepClone().AsObject();
        disk["schemaSource"] = "disk";
        Add(
            disk.ToJsonString(),
            ["feature.md"],
            [],
            [
                current,
                "skipped .specht/schema/frontmatter/epic.schema.json: the on-disk source is selected",
                "skipped .specht/schema/frontmatter/feature.schema.json: the on-disk source is selected",
                "skipped .specht/schema/frontmatter/task.schema.json: the on-disk source is selected",
                feature,
            ]);
    }
}
