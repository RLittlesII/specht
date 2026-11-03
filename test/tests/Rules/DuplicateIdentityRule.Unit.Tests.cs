using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules;

/// <summary>
/// SPEC012's order and guard over a model built in memory (<c>0001-F1</c> B-004; item 0158): the identities in the order
/// each first appears and, within one, the Features' order; only the Features with both <c>epic</c> and <c>id</c> are grouped.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class DuplicateIdentityRuleUnitTests
{
    [Fact]
    public void TwoIdentitiesEachClaimedTwiceAndInterleaved_WhenTheDuplicateIdentityRuleEvaluatesTheModel_ShouldReportSpec012ByFirstAppearanceThenFeatureOrder()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            Feature("src/d/.spec/README.md", "0001", "F2"),
            Feature("src/c/.spec/README.md", "0001", "F1"),
            Feature("src/b/.spec/README.md", "0001", "F2"),
            Feature("src/a/.spec/README.md", "0001", "F1"));
        var rule = new DuplicateIdentityRule();

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.File, finding.Identifier)).Should().Equal(
            ("SPEC012", "src/d/.spec/README.md", "0001-F2"),
            ("SPEC012", "src/b/.spec/README.md", "0001-F2"),
            ("SPEC012", "src/c/.spec/README.md", "0001-F1"),
            ("SPEC012", "src/a/.spec/README.md", "0001-F1"));
    }

    [Fact]
    public void FeaturesSharingOnlyAnEpicOrOnlyAnId_WhenTheDuplicateIdentityRuleEvaluatesTheModel_ShouldNotGroupThem()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            Feature("src/no-epic-1/.spec/README.md", null, "F2"),
            Feature("src/no-epic-2/.spec/README.md", null, "F2"),
            Feature("src/no-id-1/.spec/README.md", "0001", null),
            Feature("src/no-id-2/.spec/README.md", "0001", null),
            Feature("src/first/.spec/README.md", "0001", "F3"),
            Feature("src/second/.spec/README.md", "0001", "F3"));
        var rule = new DuplicateIdentityRule();

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.File)).Should().Equal(
            ("SPEC012", "src/first/.spec/README.md"),
            ("SPEC012", "src/second/.spec/README.md"));
    }

    private static FeatureSpec Feature(string path, string? epic, string? id) =>
        new FeatureSpecFixture()
            .WithLocation(new SpechtLocationFixture().WithRelativePath(path))
            .WithDocument(
                new SpechtDocumentFixture().WithRelativePath(path).WithFrontmatter(
                    new FrontmatterFixture().WithNode(new JsonObject { ["epic"] = epic, ["id"] = id })));
}
