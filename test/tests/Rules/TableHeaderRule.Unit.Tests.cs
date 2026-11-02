using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules;

/// <summary>
/// The table-header check over a model built in memory under a manifest whose <c>tables</c> is keyed by role
/// (<c>0001-F5</c> B-002, decision 0006): the headers a role declares are expected of the section that role names.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class TableHeaderRuleUnitTests
{
    [Fact]
    public void HeadersDeclaredForTheMatrixRole_WhenTheTableHeaderRuleEvaluatesTheModel_ShouldReportSpec013OnTheMatrixThatDoesNotCarryThem()
    {
        // Given
        var manifest = new JsonObject
        {
            ["tables"] = new JsonObject { ["matrix"] = new JsonArray("Claim ID", "Scenario", "Proof", "Status") },
        };
        SpechtModel model = new SpechtModelFixture().WithManifest(manifest).WithFeatures(
            Feature(
                "src/first/.spec/README.md",
                "## 9. Traceability Matrix\n\n"
                    + "| Claim ID | Scenario | Proof | Status |\n| - | - | - | - |\n| B-001 | It | A test | Covered |\n"),
            Feature(
                "src/second/.spec/README.md",
                "## 9. Traceability Matrix\n\n"
                    + "| Claim ID | Scenario | Test | Status |\n| - | - | - | - |\n| B-001 | It | A test | Covered |\n"));
        var rule = new TableHeaderRule();

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.File)).Should().Equal(("SPEC013", "src/second/.spec/README.md"));
    }

    [Fact]
    public void HeadersDeclaredForARoleWhoseSectionIsRenamed_WhenTheTableHeaderRuleEvaluatesTheModel_ShouldReportSpec013OnTheRenamedSectionAlone()
    {
        // Given
        var manifest = new JsonObject
        {
            ["sections"] = new JsonArray("3. Acceptance Criteria", "9. Coverage", "12. Sign-off"),
            ["roles"] = new JsonObject { ["matrix"] = "9. Coverage" },
            ["tables"] = new JsonObject { ["matrix"] = new JsonArray("Claim ID", "Scenario", "Test", "Status") },
        };
        SpechtModel model = new SpechtModelFixture().WithManifest(manifest).WithFeatures(
            Feature(
                "src/first/.spec/README.md",
                "## 9. Traceability Matrix\n\n"
                    + "| Claim ID | Scenario | Status |\n| - | - | - |\n| B-001 | It | Covered |\n"),
            Feature(
                "src/second/.spec/README.md",
                "## 9. Coverage\n\n"
                    + "| Claim ID | Scenario | Status |\n| - | - | - |\n| B-001 | It | Covered |\n"));
        var rule = new TableHeaderRule();

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.File)).Should().Equal(("SPEC013", "src/second/.spec/README.md"));
    }

    private static SpechtDocumentParser DocumentParser => new(new FrontmatterReader());

    private static FeatureSpec Feature(string path, string text) =>
        new FeatureSpecFixture()
            .WithLocation(new SpechtLocationFixture().WithRelativePath(path))
            .WithDocument(DocumentParser.Parse(text, path));
}
