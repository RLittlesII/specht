using AwesomeAssertions;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules;

/// <summary>
/// SPEC030's order and guard over a model built in memory (<c>0001-F1</c> B-004; item 0160): the Features' order and,
/// within one, the claim table's row order; a row whose first cell is empty is skipped.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class ClaimIdRuleGuardsUnitTests
{
    [Fact]
    public void ClaimTablesWithMalformedAndRepeatedIds_WhenTheClaimIdRuleEvaluatesTheModel_ShouldReportSpec030InFeatureOrderThenRowOrder()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            Feature("src/b/.spec/README.md", Claims(("B-1", 10), ("B-001", 11), ("B-001", 12), ("claim", 13))),
            Feature("src/a/.spec/README.md", Claims(("B-002", 10), ("B-002", 11))));
        var rule = new ClaimIdRule();

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.File, finding.Line, finding.Identifier)).Should().Equal(
            ("SPEC030", "src/b/.spec/README.md", 10, "B-1"),
            ("SPEC030", "src/b/.spec/README.md", 12, "B-001"),
            ("SPEC030", "src/b/.spec/README.md", 13, "claim"),
            ("SPEC030", "src/a/.spec/README.md", 11, "B-002"));
    }

    [Fact]
    public void AClaimRowWhoseFirstCellIsEmpty_WhenTheClaimIdRuleEvaluatesTheModel_ShouldSkipIt()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            Feature("src/sample/.spec/README.md", Claims((" ", 10), (" ", 11), ("B-1", 12))));
        var rule = new ClaimIdRule();

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.Line, finding.Identifier)).Should().Equal(("SPEC030", 12, "B-1"));
    }

    private static SpechtSection Claims(params (string Id, int Line)[] rows) =>
        new(
            "3. Acceptance Criteria",
            5,
            ["ID", "Claim"],
            [.. rows.Select(static row => (IReadOnlyList<string>)[row.Id, "It holds."])],
            [.. rows.Select(static row => row.Line)]);

    private static FeatureSpec Feature(string path, SpechtSection claims) =>
        new FeatureSpecFixture()
            .WithLocation(new SpechtLocationFixture().WithRelativePath(path))
            .WithDocument(new SpechtDocument(path, new FrontmatterFixture(), [claims]));
}
