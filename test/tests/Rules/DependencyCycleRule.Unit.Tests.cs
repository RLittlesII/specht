using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules;

/// <summary>
/// SPEC052 over a model built in memory (<c>0001-F1</c> B-004; item 0162): every self-dependency in the Features' order
/// before any cycle, the cycles in the order the walk finds them, each identity's cycle once, read only from the Features
/// that have both <c>epic</c> and <c>id</c>.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class DependencyCycleRuleUnitTests
{
    [Fact]
    public void ACycleBeforeASelfDependency_WhenTheDependencyCycleRuleEvaluatesTheModel_ShouldReportSpec052SelfDependenciesFirstThenCyclesInWalkOrder()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            Feature("src/a/.spec/README.md", "0001", "F1", "F2"),
            Feature("src/b/.spec/README.md", "0001", "F2", "F1"),
            Feature("src/c/.spec/README.md", "0001", "F3", "F3"));
        var rule = new DependencyCycleRule();

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.File, finding.Message)).Should().Equal(
            ("SPEC052", "src/c/.spec/README.md", "frontmatter depends_on names this Feature itself"),
            ("SPEC052", "src/a/.spec/README.md", "dependency cycle: 0001-F1 -> 0001-F2 -> 0001-F1"),
            ("SPEC052", "src/b/.spec/README.md", "dependency cycle: 0001-F2 -> 0001-F1 -> 0001-F2"),
            ("SPEC052", "src/c/.spec/README.md", "dependency cycle: 0001-F3 -> 0001-F3"));
    }

    [Fact]
    public void TwoFeaturesOfOneIdentityOnACycleAndAnUnidentifiedSelfDependency_WhenTheDependencyCycleRuleEvaluatesTheModel_ShouldReportSpec052OncePerIdentity()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            Feature("src/a/.spec/README.md", "0001", "F1", "F2"),
            Feature("src/copy/.spec/README.md", "0001", "F1", "F2"),
            Feature("src/b/.spec/README.md", "0001", "F2", "F1"),
            Feature("src/unidentified/.spec/README.md", null, "F5", "F5"));
        var rule = new DependencyCycleRule();

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.File)).Should().Equal(
            ("SPEC052", "src/a/.spec/README.md"),
            ("SPEC052", "src/b/.spec/README.md"));
    }

    private static FeatureSpec Feature(string path, string? epic, string id, string dependsOn) =>
        new FeatureSpecFixture()
            .WithLocation(new SpechtLocationFixture().WithRelativePath(path))
            .WithDocument(
                new SpechtDocumentFixture()
                    .WithRelativePath(path)
                    .WithFrontmatter(new FrontmatterFixture().WithNode(
                        new JsonObject { ["epic"] = epic, ["id"] = id, ["depends_on"] = new JsonArray(dependsOn) })));
}
