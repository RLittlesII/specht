using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules;

/// <summary>
/// SPEC043 over a model built in memory (<c>0001-F1</c> B-004; item 0161): each item's id agrees with its file name and
/// its parent with the Feature whose directory holds it, reported in the items' order, the file name before the parent,
/// whatever the levels of SPEC040, SPEC041 and SPEC044.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class ItemPlacementRuleUnitTests
{
    [Fact]
    public void ItemsWhoseIdAndParentDisagreeWithWhereTheySit_WhenTheItemPlacementRuleEvaluatesTheModel_ShouldReportSpec043InItemOrderFileNameBeforeParent()
    {
        // Given
        SpechtModel model = new SpechtModelFixture()
            .WithFeatures(new FeatureSpecFixture().WithDocument(new SpechtDocumentFixture().WithFrontmatter(Identity)))
            .WithItems(
                Item("0001-02-task.md", new JsonObject { ["id"] = "0001-02", ["parent"] = "0001-F3" }),
                Item("0001-01-task.md", new JsonObject { ["id"] = "0001-09", ["parent"] = "0001-F2" }));
        var rule = new ItemPlacementRule();

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.File, finding.Line)).Should().Equal(
            ("SPEC043", "src/sample/.spec/0001-02-task.md", ParentLine),
            ("SPEC043", "src/sample/.spec/0001-01-task.md", IdLine),
            ("SPEC043", "src/sample/.spec/0001-01-task.md", ParentLine));
    }

    [Theory]
    [ClassData(typeof(SiblingLevels))]
    public void ItemsWithNoIdNoParentOrNoFeatureBesideThem_WhenTheItemPlacementRuleEvaluatesTheModel_ShouldSkipThemWhateverTheSiblingLevels(
        string manifest)
    {
        // Given
        ChildItem placed = Item("0001-01-task.md", new JsonObject { ["id"] = "0001-01", ["parent"] = "0001-F2" });
        ChildItem noId = Item("0001-02-task.md", new JsonObject { ["parent"] = "0001-F2" });
        ChildItem noParent = Item("0001-03-task.md", new JsonObject { ["id"] = "0001-03" });
        ChildItem noFeature = new ChildItemFixture()
            .WithRelativePath("src/other/0001-04-task.md")
            .WithFileName("0001-04-task.md")
            .WithParentDirectory(placed.ParentDirectory + "/other")
            .WithFrontmatter(new FrontmatterFixture().WithNode(new JsonObject { ["id"] = "0001-04", ["parent"] = "0001-F2" }));
        SpechtModel model = new SpechtModelFixture()
            .WithManifest(JsonNode.Parse(manifest)!.AsObject())
            .WithFeatures(new FeatureSpecFixture().WithDocument(new SpechtDocumentFixture().WithFrontmatter(Identity)))
            .WithItems(noId, noParent, noFeature, placed);
        var rule = new ItemPlacementRule();

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.File, finding.Line)).Should().Equal(
            ("SPEC043", "src/sample/.spec/0001-01-task.md", ParentLine));
    }

    /// <summary>The manifest with every sibling id at its default level, and with every sibling id off.</summary>
    public sealed class SiblingLevels : TheoryData<string>
    {
        public SiblingLevels()
        {
            Add("{}");
            Add("""{ "rules": { "SPEC040": "off", "SPEC041": "off", "SPEC044": "off" } }""");
        }
    }

    private static FrontmatterFixture Identity => new FrontmatterFixture().WithNode(new JsonObject { ["epic"] = "0001", ["id"] = "F1" });

    private static ChildItem Item(string fileName, JsonObject frontmatter) =>
        new ChildItemFixture()
            .WithRelativePath("src/sample/.spec/" + fileName)
            .WithFileName(fileName)
            .WithFrontmatter(
                new FrontmatterFixture()
                    .WithNode(frontmatter)
                    .WithKeyLines(new Dictionary<string, int>(StringComparer.Ordinal) { ["id"] = IdLine, ["parent"] = ParentLine }));

    private const int IdLine = 2;

    private const int ParentLine = 3;
}
