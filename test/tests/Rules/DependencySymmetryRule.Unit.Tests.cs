using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules;

/// <summary>
/// SPEC051 over a model built in memory (<c>0001-F1</c> B-004; item 0162): each dependency edge between two discovered
/// Features is declared from both ends, reported in the Features' order, <c>depends_on</c> before <c>blocks</c>; an edge
/// to an undiscovered Feature or to the Feature itself is not tested, whatever the levels of SPEC050 and SPEC052.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class DependencySymmetryRuleUnitTests
{
    [Fact]
    public void EdgesDeclaredFromOneEndOnly_WhenTheDependencySymmetryRuleEvaluatesTheModel_ShouldReportSpec051InFeatureOrderDependsOnBeforeBlocks()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            Feature("src/b/.spec/README.md", new JsonObject
            {
                ["epic"] = "0001",
                ["id"] = "F2",
                ["blocks"] = new JsonArray("F3"),
                ["depends_on"] = new JsonArray("F1"),
            }),
            Feature("src/a/.spec/README.md", new JsonObject { ["epic"] = "0001", ["id"] = "F1", ["depends_on"] = new JsonArray("F3") }),
            Feature("src/c/.spec/README.md", new JsonObject { ["epic"] = "0001", ["id"] = "F3" }));
        var rule = new DependencySymmetryRule();

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.File, finding.Message)).Should().Equal(
            ("SPEC051", "src/b/.spec/README.md", "'0001-F2' declares depends_on '0001-F1', but '0001-F1' does not declare blocks '0001-F2'"),
            ("SPEC051", "src/b/.spec/README.md", "'0001-F2' declares blocks '0001-F3', but '0001-F3' does not declare depends_on '0001-F2'"),
            ("SPEC051", "src/a/.spec/README.md", "'0001-F1' declares depends_on '0001-F3', but '0001-F3' does not declare blocks '0001-F1'"));
    }

    [Theory]
    [ClassData(typeof(SiblingLevels))]
    public void EdgesToAnUndiscoveredFeatureAndToItself_WhenTheDependencySymmetryRuleEvaluatesTheModel_ShouldNotTestThemWhateverTheSiblingLevels(
        string manifest)
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithManifest(JsonNode.Parse(manifest)!.AsObject()).WithFeatures(
            Feature("src/a/.spec/README.md", new JsonObject { ["epic"] = "0001", ["id"] = "F1", ["depends_on"] = new JsonArray("F9", "F1", "F2") }),
            Feature("src/b/.spec/README.md", new JsonObject { ["epic"] = "0001", ["id"] = "F2" }),
            Feature("src/c/.spec/README.md", new JsonObject { ["id"] = "F3", ["depends_on"] = new JsonArray("F2") }));
        var rule = new DependencySymmetryRule();

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.File, finding.Identifier)).Should().Equal(
            ("SPEC051", "src/a/.spec/README.md", "0001-F2"));
    }

    /// <summary>The manifest with every sibling id at its default level, and with every sibling id off.</summary>
    public sealed class SiblingLevels : TheoryData<string>
    {
        public SiblingLevels()
        {
            Add("{}");
            Add("""{ "rules": { "SPEC050": "off", "SPEC052": "off" } }""");
        }
    }

    private static FeatureSpec Feature(string path, JsonObject frontmatter) =>
        new FeatureSpecFixture()
            .WithLocation(new SpechtLocationFixture().WithRelativePath(path))
            .WithDocument(new SpechtDocumentFixture().WithRelativePath(path).WithFrontmatter(new FrontmatterFixture().WithNode(frontmatter)));
}
