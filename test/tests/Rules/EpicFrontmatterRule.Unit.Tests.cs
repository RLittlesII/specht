using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Report;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules;

/// <summary>
/// SPEC004 over a model built in memory under synthetic schemas (<c>0001-F1</c> B-004; item 0156): each epic's
/// frontmatter is evaluated against the epic schema in the model's order, each file's failures in the order the failure
/// walk yields them, and an epic with no frontmatter is SPEC001's alone.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class EpicFrontmatterRuleUnitTests
{
    [Fact]
    public void EpicsWhoseFrontmatterTheEpicSchemaRejects_WhenTheEpicFrontmatterRuleEvaluatesTheModel_ShouldReportSpec004InModelOrderAndWalkOrder()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithSchemas(Synthetic.Schemas).WithList(
            [
                Epic("epics/0002-epic/epic.md", new JsonObject { ["priority"] = "low" }),
                Epic("epics/0001-epic/epic.md", new JsonObject { ["priority"] = "high" }),
            ]);
        var rule = new EpicFrontmatterRule(new FrontmatterSchemaCheck());

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.File, finding.Identifier)).Should().Equal(
            ("SPEC004", "epics/0002-epic/epic.md", null),
            ("SPEC004", "epics/0002-epic/epic.md", "priority"),
            ("SPEC004", "epics/0001-epic/epic.md", null),
            ("SPEC004", "epics/0001-epic/epic.md", "priority"));
    }

    [Fact]
    public void AnEpicWithNoFrontmatterUnderASchemaRequiringAnObject_WhenTheEpicFrontmatterRuleEvaluatesTheModel_ShouldReportNothingForIt()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithSchemas(Synthetic.Schemas).WithList(
            [
                Epic("epics/0001-epic/epic.md", null),
                Epic("epics/0002-epic/epic.md", new JsonObject { ["priority"] = "low" }),
            ]);
        var rule = new EpicFrontmatterRule(new FrontmatterSchemaCheck());

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.File)).Should().NotBeEmpty()
            .And.AllSatisfy(finding => finding.Should().Be(("SPEC004", "epics/0002-epic/epic.md")));
    }

    private static SpechtModel Synthetic => new SyntheticSchemaModelFixture();

    private static EpicFile Epic(string path, JsonObject? frontmatter) =>
        new EpicFileFixture().WithRelativePath(path).WithFrontmatter(new FrontmatterFixture().WithNode(frontmatter));
}
