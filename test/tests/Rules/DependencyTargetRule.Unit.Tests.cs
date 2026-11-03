using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules;

/// <summary>
/// SPEC050 over a model built in memory (<c>0001-F1</c> B-004; item 0162): each dependency edge names a discovered
/// Feature, reported in the Features' order, a Feature's <c>depends_on</c> edges before its <c>blocks</c> edges, and read
/// only from the Features that have both <c>epic</c> and <c>id</c>.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class DependencyTargetRuleUnitTests
{
    [Fact]
    public void FeaturesNamingUndiscoveredFeatures_WhenTheDependencyTargetRuleEvaluatesTheModel_ShouldReportSpec050InFeatureOrderDependsOnBeforeBlocks()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            Feature("src/b/.spec/README.md", new JsonObject
            {
                ["epic"] = "0001",
                ["id"] = "F2",
                ["blocks"] = new JsonArray("F9"),
                ["depends_on"] = new JsonArray("F8"),
            }),
            Feature("src/a/.spec/README.md", new JsonObject { ["epic"] = "0001", ["id"] = "F1", ["depends_on"] = new JsonArray("0002/F1") }));
        var rule = new DependencyTargetRule();

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.File, finding.Identifier)).Should().Equal(
            ("SPEC050", "src/b/.spec/README.md", "0001-F8"),
            ("SPEC050", "src/b/.spec/README.md", "0001-F9"),
            ("SPEC050", "src/a/.spec/README.md", "0002-F1"));
    }

    [Fact]
    public void FeaturesMissingAnEpicOrAnId_WhenTheDependencyTargetRuleEvaluatesTheModel_ShouldNotReadTheirEdges()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            Feature("src/no-id/.spec/README.md", new JsonObject { ["epic"] = "0001", ["depends_on"] = new JsonArray("F7") }),
            Feature("src/no-epic/.spec/README.md", new JsonObject { ["id"] = "F3", ["blocks"] = new JsonArray("F7") }),
            Feature("src/identified/.spec/README.md", new JsonObject { ["epic"] = "0001", ["id"] = "F1", ["depends_on"] = new JsonArray("F7") }));
        var rule = new DependencyTargetRule();

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.File, finding.Identifier)).Should().Equal(
            ("SPEC050", "src/identified/.spec/README.md", "0001-F7"));
    }

    private static FeatureSpec Feature(string path, JsonObject frontmatter) =>
        new FeatureSpecFixture()
            .WithLocation(new SpechtLocationFixture().WithRelativePath(path))
            .WithDocument(new SpechtDocumentFixture().WithRelativePath(path).WithFrontmatter(new FrontmatterFixture().WithNode(frontmatter)));
}
