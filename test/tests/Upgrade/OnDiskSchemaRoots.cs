using System.Text.Json.Nodes;
using Specht.Tool.Features;
using Specht.Versioning;

namespace Specht.Tests.Upgrade;

/// <summary>
/// Roots of <c>0001-F7</c> B-030 under the on-disk source with no upstream schema source recorded, each with the file names
/// the root holds under its frontmatter schema folder and the root-relative paths the upgrade lists as skipped, in ordinal
/// order. Each manifest is version 0.2.0's shipped manifest with <c>schemaSource</c> set to <c>disk</c>, so it lacks no key
/// that version defines: once over all three shipped schemas, once over the Feature and task schemas with no epic schema,
/// and once with <c>frontmatterSchemas</c> naming a file of the consumer's for each kind over that file for the Feature
/// schema and the task schema under its shipped name, so only the file present by its shipped name is listed.
/// </summary>
public sealed class OnDiskSchemaRoots : TheoryData<string, string[], string[]>
{
    public OnDiskSchemaRoots()
    {
        var defined = JsonNode.Parse(
            _shipped.At(new SemanticVersion(0, 2, 0)).Single(static file => file.Relative == ".specht/manifest.json").Bytes)!.AsObject();
        var disk = defined.DeepClone().AsObject();
        disk["schemaSource"] = "disk";
        Add(
            disk.ToJsonString(),
            ["epic.schema.json", "feature.schema.json", "task.schema.json"],
            [".specht/schema/frontmatter/epic.schema.json", ".specht/schema/frontmatter/feature.schema.json", ".specht/schema/frontmatter/task.schema.json"]);
        Add(
            disk.ToJsonString(),
            ["feature.schema.json", "task.schema.json"],
            [".specht/schema/frontmatter/feature.schema.json", ".specht/schema/frontmatter/task.schema.json"]);
        var named = disk.DeepClone().AsObject();
        named["frontmatterSchemas"] = new JsonObject
        {
            ["feature"] = "consumer-feature.schema.json",
            ["task"] = "consumer-task.schema.json",
            ["epic"] = "consumer-epic.schema.json",
        };
        Add(named.ToJsonString(), ["consumer-feature.schema.json", "task.schema.json"], [".specht/schema/frontmatter/task.schema.json"]);
    }

    private readonly ShippedFiles _shipped = new ShippedFilesFixture();
}
