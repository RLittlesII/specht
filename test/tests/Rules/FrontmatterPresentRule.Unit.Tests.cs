using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Report;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules;

/// <summary>
/// SPEC001 over a model built in memory (<c>0001-F1</c> B-004; item 0156): a document with no frontmatter is reported,
/// Features first, then items, then epics, each in the model's order, and no schema is evaluated.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class FrontmatterPresentRuleUnitTests
{
    [Fact]
    public void DocumentsWithNoFrontmatter_WhenTheFrontmatterPresentRuleEvaluatesTheModel_ShouldReportSpec001OnFeaturesThenItemsThenEpicsInModelOrder()
    {
        // Given
        FeatureSpec secondFeature = new FeatureSpecFixture().WithLocation(new SpechtLocationFixture().WithRelativePath("src/b/.spec/README.md"));
        FeatureSpec firstFeature = new FeatureSpecFixture().WithLocation(new SpechtLocationFixture().WithRelativePath("src/a/.spec/README.md"));
        ChildItem secondItem = new ChildItemFixture().WithRelativePath("src/sample/.spec/0001-02-task.md");
        ChildItem firstItem = new ChildItemFixture().WithRelativePath("src/sample/.spec/0001-01-task.md");
        EpicFile secondEpic = new EpicFileFixture().WithRelativePath("epics/0002-epic/epic.md");
        EpicFile firstEpic = new EpicFileFixture().WithRelativePath("epics/0001-epic/epic.md");
        SpechtModel model = new SpechtModelFixture()
            .WithList([secondEpic, firstEpic])
            .WithItems(secondItem, firstItem)
            .WithFeatures(secondFeature, firstFeature);
        var rule = new FrontmatterPresentRule(new FrontmatterSchemaCheck());

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.File)).Should().Equal(
            ("SPEC001", "src/b/.spec/README.md"),
            ("SPEC001", "src/a/.spec/README.md"),
            ("SPEC001", "src/sample/.spec/0001-02-task.md"),
            ("SPEC001", "src/sample/.spec/0001-01-task.md"),
            ("SPEC001", "epics/0002-epic/epic.md"),
            ("SPEC001", "epics/0001-epic/epic.md"));
    }

    [Fact]
    public void DocumentsWhoseFrontmatterEachKindsSchemaRejects_WhenTheFrontmatterPresentRuleEvaluatesTheModel_ShouldReportSpec001OnlyOnTheOneWithNone()
    {
        // Given
        SpechtModel synthetic = new SyntheticSchemaModelFixture();
        Frontmatter rejected = new FrontmatterFixture().WithNode(new JsonObject { ["priority"] = "none" });
        FeatureSpec feature = new FeatureSpecFixture().WithDocument(new SpechtDocumentFixture().WithFrontmatter(rejected));
        FeatureSpec bare = new FeatureSpecFixture().WithLocation(new SpechtLocationFixture().WithRelativePath("src/bare/.spec/README.md"));
        ChildItem item = new ChildItemFixture().WithFrontmatter(rejected);
        EpicFile epic = new EpicFileFixture().WithFrontmatter(rejected);
        SpechtModel model = new SpechtModelFixture()
            .WithSchemas(synthetic.Schemas)
            .WithFeatures(feature, bare)
            .WithItems(item)
            .WithList([epic]);
        var rule = new FrontmatterPresentRule(new FrontmatterSchemaCheck());

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.File)).Should().Equal(("SPEC001", "src/bare/.spec/README.md"));
    }
}
