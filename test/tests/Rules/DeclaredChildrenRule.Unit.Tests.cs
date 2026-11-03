using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules;

/// <summary>
/// SPEC040 over a model built in memory (<c>0001-F1</c> B-004; item 0161): each id a Feature's <c>children</c> declares
/// resolves to exactly one item beside the specification, reported in the Features' order and the declaration order,
/// whatever the levels of SPEC041, SPEC043 and SPEC044.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class DeclaredChildrenRuleUnitTests
{
    [Fact]
    public void FeaturesDeclaringChildrenNoItemCarries_WhenTheDeclaredChildrenRuleEvaluatesTheModel_ShouldReportSpec040InFeatureOrderThenDeclarationOrder()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            Feature("src/b/.spec/README.md", "0001-03", "0001-02"),
            Feature("src/a/.spec/README.md", "0001-04"));
        var rule = new DeclaredChildrenRule(new DeclaredItemCheck());

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.File, finding.Identifier)).Should().Equal(
            ("SPEC040", "src/b/.spec/README.md", "0001-03"),
            ("SPEC040", "src/b/.spec/README.md", "0001-02"),
            ("SPEC040", "src/a/.spec/README.md", "0001-04"));
    }

    [Theory]
    [ClassData(typeof(SiblingLevels))]
    public void ChildrenResolvingToTwoItemsOrOnlyElsewhere_WhenTheDeclaredChildrenRuleEvaluatesTheModel_ShouldReportSpec040OnBothWhateverTheSiblingLevels(
        string manifest)
    {
        // Given
        ChildItem beside = new ChildItemFixture().WithRelativePath("src/sample/.spec/0001-01-task.md").WithFileName("0001-01-task.md");
        ChildItem firstTwin = new ChildItemFixture().WithRelativePath("src/sample/.spec/0001-02-a.md").WithFileName("0001-02-a.md");
        ChildItem secondTwin = new ChildItemFixture().WithRelativePath("src/sample/.spec/0001-02-b.md").WithFileName("0001-02-b.md");
        ChildItem elsewhere = new ChildItemFixture()
            .WithRelativePath("src/other/.spec/0001-03-task.md")
            .WithFileName("0001-03-task.md")
            .WithParentDirectory(beside.ParentDirectory + "/other");
        SpechtModel model = new SpechtModelFixture()
            .WithManifest(JsonNode.Parse(manifest)!.AsObject())
            .WithFeatures(Feature("src/sample/.spec/README.md", "0001-01", "0001-02", "0001-03"))
            .WithItems(beside, firstTwin, secondTwin, elsewhere);
        var rule = new DeclaredChildrenRule(new DeclaredItemCheck());

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.Identifier, finding.Message)).Should().Equal(
            ("SPEC040", "0001-02", "frontmatter children names '0001-02', which resolves to 2 files"),
            ("SPEC040", "0001-03", "frontmatter children names '0001-03', but no '0001-03-*.md' was discovered"));
    }

    /// <summary>The manifest with every sibling id at its default level, and with every sibling id off.</summary>
    public sealed class SiblingLevels : TheoryData<string>
    {
        public SiblingLevels()
        {
            Add("{}");
            Add("""{ "rules": { "SPEC041": "off", "SPEC043": "off", "SPEC044": "off" } }""");
        }
    }

    private static FeatureSpec Feature(string path, params string[] children) =>
        new FeatureSpecFixture()
            .WithLocation(new SpechtLocationFixture().WithRelativePath(path))
            .WithDocument(
                new SpechtDocumentFixture()
                    .WithRelativePath(path)
                    .WithFrontmatter(new FrontmatterFixture().WithNode(
                        new JsonObject { ["children"] = new JsonArray([.. children.Select(static child => (JsonNode?) child)]) })));
}
