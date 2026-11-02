using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Report;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules;

/// <summary>
/// SPEC002 over a model built in memory under synthetic schemas (<c>0001-F1</c> B-004; item 0156): each Feature's
/// frontmatter is evaluated against the Feature schema in the model's order, each file's failures in the order the
/// failure walk yields them, and a Feature with no frontmatter is SPEC001's alone.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class FeatureFrontmatterRuleUnitTests
{
    [Fact]
    public void FeaturesWhoseFrontmatterTheFeatureSchemaRejects_WhenTheFeatureFrontmatterRuleEvaluatesTheModel_ShouldReportSpec002InModelOrderAndWalkOrder()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithSchemas(Synthetic.Schemas).WithFeatures(
            Feature("src/b/.spec/README.md", new JsonObject { ["priority"] = "none", ["id"] = 1 }),
            Feature("src/a/.spec/README.md", new JsonObject { ["id"] = "F1" }));
        var rule = new FeatureFrontmatterRule(new FrontmatterSchemaCheck());

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.File, finding.Identifier, finding.Message)).Should().Equal(
            ("SPEC002", "src/b/.spec/README.md", null, "frontmatter Some properties did not match the required schema: [\"id\",\"priority\"]"),
            ("SPEC002", "src/b/.spec/README.md", "id", "frontmatter 'id' Value is \"integer\" but should be \"string\""),
            ("SPEC002", "src/b/.spec/README.md", "priority", "frontmatter 'priority' Value should match one of the values specified by the enum"),
            ("SPEC002", "src/b/.spec/README.md", null, "frontmatter Required properties [\"scored_by\"] are not present"),
            ("SPEC002", "src/b/.spec/README.md", null, "frontmatter Required properties [\"kind\"] are not present"),
            ("SPEC002", "src/a/.spec/README.md", null, "frontmatter Required properties [\"priority\"] are not present"),
            ("SPEC002", "src/a/.spec/README.md", null, "frontmatter Required properties [\"scored_by\"] are not present"),
            ("SPEC002", "src/a/.spec/README.md", null, "frontmatter Required properties [\"kind\"] are not present"));
    }

    [Fact]
    public void AFeatureWithNoFrontmatterUnderASchemaRequiringAnObject_WhenTheFeatureFrontmatterRuleEvaluatesTheModel_ShouldReportNothingForIt()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithSchemas(Synthetic.Schemas).WithFeatures(
            Feature("src/bare/.spec/README.md", null),
            Feature("src/rejected/.spec/README.md", new JsonObject { ["id"] = "F1" }));
        var rule = new FeatureFrontmatterRule(new FrontmatterSchemaCheck());

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.File)).Should().NotBeEmpty()
            .And.AllSatisfy(finding => finding.Should().Be(("SPEC002", "src/rejected/.spec/README.md")));
    }

    private static SpechtModel Synthetic => new SyntheticSchemaModelFixture();

    private static FeatureSpec Feature(string path, JsonObject? frontmatter) =>
        new FeatureSpecFixture()
            .WithLocation(new SpechtLocationFixture().WithRelativePath(path))
            .WithDocument(new SpechtDocumentFixture().WithRelativePath(path).WithFrontmatter(new FrontmatterFixture().WithNode(frontmatter)));
}
