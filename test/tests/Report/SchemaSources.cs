using System.Text.Json.Nodes;
using Specht.Manifest;
using Specht.Tests.Shared;

namespace Specht.Tests.Report;

/// <summary>
/// A synthetic Feature specification with no frontmatter and one whose priority is outside the enum, each under four
/// schema sources, for 0001-F3 B-009 and B-026: the specification, the rule and key of its violation, the members its
/// <c>expected</c> carries, what the root holds of the schema source, and the schema source the run selects. The expected
/// <c>required</c> and <c>enum</c> are read from the shipped Feature schema, which <see cref="SpecTree"/> copies.
/// </summary>
public sealed class SchemaSources : TheoryData<string, Action<SpecTree>, string, string?, JsonObject, Action<SpecTree>, SchemaSourceSelection?>
{
    public SchemaSources()
    {
        var shipped = JsonNode.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, SpecTree.FeatureSchemaPath)))!;

        foreach (var (source, arrange, selection) in new (string, Action<SpecTree>, SchemaSourceSelection?)[]
        {
            ("the embedded schemas, the files on disk", static _ => { }, null),
            ("the embedded schemas, no file on disk", static tree => tree.DeleteFrontmatterSchemas(), null),
            ("the files on disk, selected", static _ => { }, SchemaSourceSelection.Disk),
            ("the upstream copy the manifest records", static tree => tree.RecordUpstreamSchemaSource(), null),
        })
        {
            Add(
                $"no frontmatter, under {source}",
                static tree => tree.WriteRaw("epics/0001-epic/F1-feature/spec.md", "# Specification: F1\n\n" + string.Join('\n', SpecTree.Sections)),
                "SPEC001",
                null,
                new JsonObject { ["schema"] = SpecTree.FeatureSchemaPath, ["required"] = shipped["required"]!.DeepClone() },
                arrange,
                selection);
            Add(
                $"a priority outside the enum, under {source}",
                static tree => tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["priority"] = "urgent" }),
                "SPEC002",
                "priority",
                new JsonObject
                {
                    ["schema"] = SpecTree.FeatureSchemaPath,
                    ["key"] = "priority",
                    ["constraints"] = new JsonObject { ["enum"] = shipped["properties"]!["priority"]!["enum"]!.DeepClone() },
                },
                arrange,
                selection);
        }
    }
}
