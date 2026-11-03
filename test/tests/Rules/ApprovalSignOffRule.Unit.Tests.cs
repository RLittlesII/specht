using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules;

/// <summary>
/// SPEC061's order and guard over a model built in memory (<c>0001-F1</c> B-004; item 0163): the Features' order and,
/// within one, the sign-off's row order; an approved Feature with no sign-off section is skipped, whatever SPEC060's level.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class ApprovalSignOffRuleUnitTests
{
    [Fact]
    public void ApprovedFeaturesWithDraftAndBlockedSignOffRows_WhenTheApprovalSignOffRuleEvaluatesTheModel_ShouldReportSpec061InFeatureOrderThenRowOrder()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            Feature("src/b/.spec/README.md", Table(SignOff, ["Owner", Draft], ["Reviewer", "Approved"], ["Tester", Blocked])),
            Feature("src/a/.spec/README.md", Table(SignOff, ["Owner", Blocked])));
        var rule = new ApprovalSignOffRule();

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.File, finding.Line, finding.Identifier)).Should().Equal(
            ("SPEC061", "src/b/.spec/README.md", 31, "Owner"),
            ("SPEC061", "src/b/.spec/README.md", 33, "Tester"),
            ("SPEC061", "src/a/.spec/README.md", 31, "Owner"));
    }

    [Theory]
    [ClassData(typeof(SiblingLevels))]
    public void AnApprovedFeatureWithNoSignOffSection_WhenTheApprovalSignOffRuleEvaluatesTheModel_ShouldSkipItWhateverTheSiblingLevel(string manifest)
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithManifest(JsonNode.Parse(manifest)!.AsObject()).WithFeatures(
            Feature("src/no-sign-off/.spec/README.md", Table(Matrix, ["Owner", Draft])),
            Feature("src/sign-off/.spec/README.md", Table(SignOff, ["Owner", Draft])));
        var rule = new ApprovalSignOffRule();

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.File, finding.Identifier)).Should().Equal(("SPEC061", "src/sign-off/.spec/README.md", "Owner"));
    }

    /// <summary>The manifest with SPEC060 at its default level, and with it off.</summary>
    public sealed class SiblingLevels : TheoryData<string>
    {
        public SiblingLevels()
        {
            Add("{}");
            Add("""{ "rules": { "SPEC060": "off" } }""");
        }
    }

    private static SpechtSection Table(string title, params IReadOnlyList<string>[] rows) =>
        new(title, 30, ["Role", "Verdict"], rows, [.. rows.Select(static (_, index) => 31 + index)]);

    private static FeatureSpec Feature(string path, SpechtSection section) =>
        new FeatureSpecFixture()
            .WithLocation(new SpechtLocationFixture().WithRelativePath(path))
            .WithDocument(new SpechtDocument(path, new FrontmatterFixture().WithNode(new JsonObject { ["spec_status"] = "approved" }), [section]));

    private const string Matrix = "9. Traceability Matrix";

    private const string SignOff = "12. Sign-off";

    private const string Draft = "\U0001F7E1 Draft";

    private const string Blocked = "\U0001F534 Blocked";
}
