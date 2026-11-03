using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules;

/// <summary>
/// SPEC041 over a model built in memory (<c>0001-F1</c> B-004; item 0161): each id a Feature's <c>spikes</c> declares
/// resolves to exactly one spike anywhere in the repository, reported in the Features' order and the declaration order,
/// whatever the levels of SPEC040, SPEC043 and SPEC044.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class DeclaredSpikesRuleUnitTests
{
    [Fact]
    public void FeaturesDeclaringSpikesNoItemCarries_WhenTheDeclaredSpikesRuleEvaluatesTheModel_ShouldReportSpec041InFeatureOrderThenDeclarationOrder()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            Feature("src/b/.spec/README.md", "0001-03", "0001-02"),
            Feature("src/a/.spec/README.md", "0001-04"));
        var rule = new DeclaredSpikesRule(new DeclaredItemCheck());

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.File, finding.Identifier)).Should().Equal(
            ("SPEC041", "src/b/.spec/README.md", "0001-03"),
            ("SPEC041", "src/b/.spec/README.md", "0001-02"),
            ("SPEC041", "src/a/.spec/README.md", "0001-04"));
    }

    [Theory]
    [ClassData(typeof(SiblingLevels))]
    public void SpikesResolvingToTwoToNoneOrToATask_WhenTheDeclaredSpikesRuleEvaluatesTheModel_ShouldReportSpec041OnEachWhateverTheSiblingLevels(
        string manifest)
    {
        // Given
        ChildItem nearby = new ChildItemFixture();
        ChildItem spike = Item("src/other/.spec/0001-01-spike.md", "spike").WithParentDirectory(nearby.ParentDirectory + "/other");
        ChildItem firstTwin = Item("src/sample/.spec/0001-02-a.md", "spike");
        ChildItem secondTwin = Item("src/sample/.spec/0001-02-b.md", "spike");
        ChildItem task = Item("src/sample/.spec/0001-04-task.md", "task");
        SpechtModel model = new SpechtModelFixture()
            .WithManifest(JsonNode.Parse(manifest)!.AsObject())
            .WithFeatures(Feature("src/sample/.spec/README.md", "0001-01", "0001-02", "0001-03", "0001-04"))
            .WithItems(spike, firstTwin, secondTwin, task);
        var rule = new DeclaredSpikesRule(new DeclaredItemCheck());

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.File, finding.Identifier)).Should().Equal(
            ("SPEC041", "src/sample/.spec/README.md", "0001-02"),
            ("SPEC041", "src/sample/.spec/README.md", "0001-03"),
            ("SPEC041", "src/sample/.spec/0001-04-task.md", "0001-04"));
    }

    /// <summary>The manifest with every sibling id at its default level, and with every sibling id off.</summary>
    public sealed class SiblingLevels : TheoryData<string>
    {
        public SiblingLevels()
        {
            Add("{}");
            Add("""{ "rules": { "SPEC040": "off", "SPEC043": "off", "SPEC044": "off" } }""");
        }
    }

    private static ChildItemFixture Item(string path, string type) =>
        new ChildItemFixture()
            .WithRelativePath(path)
            .WithFileName(path[(path.LastIndexOf('/') + 1)..])
            .WithFrontmatter(new FrontmatterFixture().WithNode(new JsonObject { ["type"] = type }));

    private static FeatureSpec Feature(string path, params string[] spikes) =>
        new FeatureSpecFixture()
            .WithLocation(new SpechtLocationFixture().WithRelativePath(path))
            .WithDocument(
                new SpechtDocumentFixture()
                    .WithRelativePath(path)
                    .WithFrontmatter(new FrontmatterFixture().WithNode(
                        new JsonObject { ["spikes"] = new JsonArray([.. spikes.Select(static spike => (JsonNode?) spike)]) })));
}
