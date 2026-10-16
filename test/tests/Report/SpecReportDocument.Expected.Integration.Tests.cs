using System.Security.Cryptography;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Report;
using Specht.Tests.Shared;

namespace Specht.Tests.Report;

/// <summary>
/// What a frontmatter violation's <c>expected</c> names under each schema source, over a run on a tree on disk
/// (<c>0001-F3</c> B-009, B-026; decision 0004): the Feature schema file by its path under the configuration folder,
/// whichever source the run read - the embedded schemas, the files on disk, or the upstream copy the manifest records -
/// with the required keys or the failed assertion of the schema it read. The tree's schema files are the tool's own
/// (<see cref="SpecTree"/>), so each source gives the same values.
/// </summary>
[Trait("Tier", "Integration")]
public sealed class SpecReportDocumentExpectedIntegrationTests
{
    /// <summary>
    /// Gets a Feature specification, the rule and key of its violation, the members its <c>expected</c> carries, what the
    /// root holds of the schema source, and the schema source the run selects.
    /// </summary>
    public static TheoryData<string, Action<SpecTree>, string, string?, JsonObject, Action<SpecTree>, SchemaSourceSelection?> Sources
    {
        get
        {
            var shipped = JsonNode.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, SpecTree.FeatureSchemaPath)))!;
            var data = new TheoryData<string, Action<SpecTree>, string, string?, JsonObject, Action<SpecTree>, SchemaSourceSelection?>();

            foreach (var (source, arrange, selection) in new (string, Action<SpecTree>, SchemaSourceSelection?)[]
            {
                ("the embedded schemas, the files on disk", static _ => { }, null),
                ("the embedded schemas, no file on disk", DeleteSchemas, null),
                ("the files on disk, selected", static _ => { }, SchemaSourceSelection.Disk),
                ("the upstream copy the manifest records", RecordUpstream, null),
            })
            {
                data.Add(
                    $"no frontmatter, under {source}",
                    static tree => tree.WriteRaw("epics/0001-epic/F1-feature/spec.md", "# Specification: F1\n\n" + string.Join('\n', SpecTree.Sections)),
                    "SPEC001",
                    null,
                    new JsonObject { ["schema"] = SpecTree.FeatureSchemaPath, ["required"] = shipped["required"]!.DeepClone() },
                    arrange,
                    selection);
                data.Add(
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

            return data;
        }
    }

    [Theory]
    [MemberData(nameof(Sources))]
    public void AFrontmatterViolation_WhenItsDocumentIsMadeUnderEachSchemaSource_ShouldNameTheFeatureSchemaByItsPathUnderTheConfigurationFolder(
        string because,
        Action<SpecTree> write,
        string ruleId,
        string? key,
        JsonObject members,
        Action<SpecTree> arrange,
        SchemaSourceSelection? selection)
    {
        // Given
        using var tree = new SpecTree();
        write(tree);
        arrange(tree);

        // When
        var document = SpecReportDocument.From(EngineServices.Runner().Run(tree.Root, selection));

        // Then
        var expected = document.Violations.Should()
            .ContainSingle(violation => violation.RuleId == ruleId && violation.Identifier == key, because).Which.Expected;
        members.Should().AllSatisfy(member => JsonNode.DeepEquals(expected[member.Key], member.Value).Should().BeTrue(
            "{0}: {1} should be {2}, but expected is {3}", because, member.Key, member.Value?.ToJsonString(), expected.ToJsonString()));
    }

    private static void DeleteSchemas(SpecTree tree) =>
        Directory.Delete(Path.Combine(tree.Root, SpecTree.FrontmatterSchemaFolder), recursive: true);

    private static void RecordUpstream(SpecTree tree)
    {
        var path = Path.Combine(tree.Root, SpecTree.ManifestPath);
        var manifest = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        manifest["spechtSchema"] = new JsonObject
        {
            ["location"] = "https://schemas.example/specht/",
            ["version"] = "v1",
            ["hashes"] = new JsonObject(new[] { "feature", "task", "epic" }.Select(kind => KeyValuePair.Create(
                kind,
                (JsonNode?)("sha256:" + Convert.ToHexStringLower(SHA256.HashData(
                    File.ReadAllBytes(Path.Combine(tree.Root, SpecTree.FrontmatterSchemaFolder, kind + ".schema.json")))))))),
        };
        File.WriteAllText(path, manifest.ToJsonString());
    }
}
