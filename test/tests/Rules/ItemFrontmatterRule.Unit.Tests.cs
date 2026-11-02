using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Report;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules;

/// <summary>
/// SPEC003 over a model built in memory under synthetic schemas (<c>0001-F1</c> B-004; item 0156): each item's
/// frontmatter is evaluated against the item schema in the model's order, each file's failures in the order the failure
/// walk yields them, and an item with no frontmatter is SPEC001's alone.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class ItemFrontmatterRuleUnitTests
{
    [Fact]
    public void ItemsWhoseFrontmatterTheItemSchemaRejects_WhenTheItemFrontmatterRuleEvaluatesTheModel_ShouldReportSpec003InModelOrderAndWalkOrder()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithSchemas(Synthetic.Schemas).WithItems(
            Item("src/sample/.spec/0001-02-task.md", new JsonObject { ["priority"] = "low" }),
            Item("src/sample/.spec/0001-01-task.md", new JsonObject { ["parent"] = "0001-F1" }));
        var rule = new ItemFrontmatterRule(new FrontmatterSchemaCheck());

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.File, finding.Identifier)).Should().Equal(
            ("SPEC003", "src/sample/.spec/0001-02-task.md", null),
            ("SPEC003", "src/sample/.spec/0001-02-task.md", "priority"),
            ("SPEC003", "src/sample/.spec/0001-01-task.md", null));
    }

    [Fact]
    public void AnItemWithNoFrontmatterUnderASchemaRequiringAnObject_WhenTheItemFrontmatterRuleEvaluatesTheModel_ShouldReportNothingForIt()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithSchemas(Synthetic.Schemas).WithItems(
            Item("src/sample/.spec/0001-01-task.md", null),
            Item("src/sample/.spec/0001-02-task.md", new JsonObject { ["parent"] = "0001-F1" }));
        var rule = new ItemFrontmatterRule(new FrontmatterSchemaCheck());

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.File)).Should().Equal(("SPEC003", "src/sample/.spec/0001-02-task.md"));
    }

    private static SpechtModel Synthetic => new SyntheticSchemaModelFixture();

    private static ChildItem Item(string path, JsonObject? frontmatter) =>
        new ChildItemFixture().WithRelativePath(path).WithFrontmatter(new FrontmatterFixture().WithNode(frontmatter));
}
