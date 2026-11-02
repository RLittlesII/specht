using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules;

/// <summary>
/// SPEC060's order and guard over a model built in memory under schema 0.1.0 (<c>0001-F1</c> B-004; item 0163): the
/// Features' order and, within one, the matrix's row order; an approved Feature with no matrix section is skipped, whatever
/// SPEC061's level.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class ApprovalCoverageRuleGuardsUnitTests
{
    [Fact]
    public void ApprovedFeaturesWithMissingCells_WhenTheApprovalCoverageRuleEvaluatesTheModel_ShouldReportSpec060InFeatureOrderThenRowOrder()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            Feature("src/b/.spec/README.md", Table(Matrix, ["B-002", "Missing"], ["B-001", "A test"], ["B-003", "Missing"])),
            Feature("src/a/.spec/README.md", Table(Matrix, ["B-001", "Missing"])));
        var rule = new ApprovalCoverageRule();

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.File, finding.Line, finding.Identifier)).Should().Equal(
            ("SPEC060", "src/b/.spec/README.md", 21, "B-002"),
            ("SPEC060", "src/b/.spec/README.md", 23, "B-003"),
            ("SPEC060", "src/a/.spec/README.md", 21, "B-001"));
    }

    [Theory]
    [ClassData(typeof(SiblingLevels))]
    public void AnApprovedFeatureWithNoMatrixSection_WhenTheApprovalCoverageRuleEvaluatesTheModel_ShouldSkipItWhateverTheSiblingLevel(string manifest)
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithManifest(JsonNode.Parse(manifest)!.AsObject()).WithFeatures(
            Feature("src/no-matrix/.spec/README.md", Table(SignOff, ["Owner", "Missing"])),
            Feature("src/matrix/.spec/README.md", Table(Matrix, ["B-001", "Missing"])));
        var rule = new ApprovalCoverageRule();

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.File, finding.Identifier)).Should().Equal(("SPEC060", "src/matrix/.spec/README.md", "B-001"));
    }

    /// <summary>The manifest with SPEC061 at its default level, and with it off.</summary>
    public sealed class SiblingLevels : TheoryData<string>
    {
        public SiblingLevels()
        {
            Add("{}");
            Add("""{ "rules": { "SPEC061": "off" } }""");
        }
    }

    private static SpechtSection Table(string title, params IReadOnlyList<string>[] rows) =>
        new(title, 20, ["Claim ID", "Test"], rows, [.. rows.Select(static (_, index) => 21 + index)]);

    private static FeatureSpec Feature(string path, SpechtSection section) =>
        new FeatureSpecFixture()
            .WithLocation(new SpechtLocationFixture().WithRelativePath(path))
            .WithDocument(new SpechtDocument(path, new FrontmatterFixture().WithNode(new JsonObject { ["spec_status"] = "approved" }), [section]));

    private const string Matrix = "9. Traceability Matrix";

    private const string SignOff = "12. Sign-off";
}
