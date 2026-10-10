using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Json.Schema;
using Specht.Report;

namespace Specht.Tests.Runner;

/// <summary>
/// The runner over a tree on disk whose manifest declares discovery's inputs (<c>0001-F6</c> B-001, B-003, B-009, C-7;
/// <c>0001-F3</c> B-008): the layouts the report document and the summary name, by the manifest's names, in the manifest's
/// order and with a layout holding no specification at zero, and an item file whose name follows the manifest's task
/// grammar. A layout is read from the serialized document and the summary line, which is where a consumer reads it.
/// </summary>
[Trait("Tier", "Integration")]
public sealed class SpechtRunnerDiscoveryIntegrationTests
{
    /// <summary>Gets the layouts a manifest declares, and each layout's name and count as the outputs must list them.</summary>
    public static TheoryData<string, string, string> DeclaredLayouts { get; } = new()
    {
        {
            "the layouts are named as the manifest names them, in the manifest's order",
            """[{ "name": "beside-code", "glob": "**/.spec/README.md" }, { "name": "old-tree", "glob": "epics/**/spec.md" }]""",
            "beside-code 1, old-tree 2"
        },
        {
            "a third layout is listed after the two the default manifest declares",
            """
            [
              { "name": "epics", "glob": "epics/**/spec.md" },
              { "name": "features", "glob": "**/.spec/README.md" },
              { "name": "documentation", "glob": "docs/**/specification.md" }
            ]
            """,
            "epics 2, features 1, documentation 1"
        },
        {
            "a layout holding no specification is listed at zero",
            """
            [
              { "name": "epics", "glob": "epics/**/spec.md" },
              { "name": "features", "glob": "**/.spec/README.md" },
              { "name": "archive", "glob": "archive/**/spec.md" }
            ]
            """,
            "epics 2, features 1, archive 0"
        },
        {
            "a manifest declaring one layout lists one, and no default layout is added back",
            """[{ "name": "features", "glob": "**/.spec/README.md" }]""",
            "features 1"
        },
    };

    [Theory]
    [MemberData(nameof(DeclaredLayouts))]
    public void AManifestDeclaringItsLayouts_WhenChecked_ShouldListEachByItsManifestNameInTheManifestsOrderInTheReportAndTheSummary(
        string because,
        string layouts,
        string expected)
    {
        // Given
        using var tree = TreeInThreePlaces();
        Declare(tree, "layouts", JsonNode.Parse(layouts)!);

        // When
        var document = SpecReportDocument.From(tree.Run());

        // Then
        string.Join(", ", Listed(document)).Should().Be(expected, because);
        document.SummaryLines()[0].Should().Be($"specifications: {expected}", because);
    }

    [Fact]
    public void AManifestNamingItsLayouts_WhenItsDocumentIsSerialized_ShouldValidateAgainstThePublishedReportSchema()
    {
        // Given
        using var tree = TreeInThreePlaces();
        Declare(
            tree,
            "layouts",
            JsonNode.Parse(
                """[{ "name": "old-tree", "glob": "epics/**/spec.md" }, { "name": "documentation", "glob": "docs/**/specification.md" }]""")!);
        var schema = JsonSchema.FromText(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "docs", "schema", "report.schema.json")));

        // When
        using var json = JsonDocument.Parse(SpecReportDocument.From(tree.Run()).ToJson());
        var result = schema.Evaluate(json.RootElement, new EvaluationOptions { OutputFormat = OutputFormat.List });

        // Then
        result.IsValid.Should().BeTrue(
            string.Join("; ", (result.Details ?? []).Where(static detail => detail.Errors is not null)
                .SelectMany(static detail => detail.Errors!.Select(error => $"{detail.InstanceLocation} {error.Value}"))));
    }

    [Fact]
    public void AManifestWhoseTaskGrammarHasAThreeDigitSequence_WhenChecked_ShouldCountTheItemWhoseFileNameFollowsThatGrammar()
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteItem(tree.WriteFeature("0001", "F1"), "0001-001", "F1");
        Declare(tree, "identifiers", new JsonObject { ["task"] = "^[0-9]{4}-[0-9]{3}$" });

        // When
        var report = tree.Run();

        // Then
        report.ItemCount.Should().Be(1);
    }

    private static SpecTree TreeInThreePlaces()
    {
        var tree = new SpecTree();
        tree.WriteFeature("0001", "F1");
        tree.WriteFeature("0001", "F2");
        tree.WriteCoLocatedFeature("src/area", "0002", "F1");
        tree.WriteSpecification("docs/guide/specification.md", "0003", "F1");

        return tree;
    }

    private static void Declare(SpecTree tree, string key, JsonNode value)
    {
        var path = Path.Combine(tree.Root, ".spec", "schema", "spec-structure.schema.json");
        var manifest = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        manifest[key] = value;
        File.WriteAllText(path, manifest.ToJsonString());
    }

    private static IEnumerable<string> Listed(SpecReportDocument document) =>
        JsonNode.Parse(document.ToJson())!["layouts"]!.AsArray().Select(static layout => string.Create(
            CultureInfo.InvariantCulture,
            $"{layout!["layout"]!.GetValue<string>()} {layout["specificationCount"]!.GetValue<int>()}"));
}
