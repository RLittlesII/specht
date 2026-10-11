using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Model;
using Specht.Rules;

namespace Specht.Tests;

/// <summary>
/// The table-header check over a model built in memory under a manifest whose <c>tables</c> is keyed by role
/// (<c>0001-F5</c> B-002, decision 0006): the headers a role declares are expected of the section that role names.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SectionStructureRuleUnitTests
{
    [Fact]
    public void HeadersDeclaredForTheMatrixRole_WhenTheSectionStructureRuleEvaluatesTheModel_ShouldReportSpec013OnTheMatrixThatDoesNotCarryThem()
    {
        // Given
        var manifest = new JsonObject
        {
            ["tables"] = new JsonObject { ["matrix"] = new JsonArray("Claim ID", "Scenario", "Proof", "Status") },
        };
        SpecModel model = new SpecModelFixture().WithManifest(manifest).WithFeatures(
            Feature(
                "src/first/.spec/README.md",
                "## 9. Traceability Matrix\n\n"
                    + "| Claim ID | Scenario | Proof | Status |\n| - | - | - | - |\n| B-001 | It | A test | Covered |\n"),
            Feature(
                "src/second/.spec/README.md",
                "## 9. Traceability Matrix\n\n"
                    + "| Claim ID | Scenario | Test | Status |\n| - | - | - | - |\n| B-001 | It | A test | Covered |\n"));

        // When
        var violations = new SectionStructureRule().Evaluate(model).Where(static violation => violation.RuleId == "SPEC013").ToList();

        // Then
        violations.Select(static violation => violation.File).Should().Equal("src/second/.spec/README.md");
    }

    [Fact]
    public void HeadersDeclaredForARoleWhoseSectionIsRenamed_WhenTheSectionStructureRuleEvaluatesTheModel_ShouldReportSpec013OnTheRenamedSectionAlone()
    {
        // Given
        var manifest = new JsonObject
        {
            ["sections"] = new JsonArray("3. Acceptance Criteria", "9. Coverage", "12. Sign-off"),
            ["roles"] = new JsonObject { ["matrix"] = "9. Coverage" },
            ["tables"] = new JsonObject { ["matrix"] = new JsonArray("Claim ID", "Scenario", "Test", "Status") },
        };
        SpecModel model = new SpecModelFixture().WithManifest(manifest).WithFeatures(
            Feature(
                "src/first/.spec/README.md",
                "## 9. Traceability Matrix\n\n"
                    + "| Claim ID | Scenario | Status |\n| - | - | - |\n| B-001 | It | Covered |\n"),
            Feature(
                "src/second/.spec/README.md",
                "## 9. Coverage\n\n"
                    + "| Claim ID | Scenario | Status |\n| - | - | - |\n| B-001 | It | Covered |\n"));

        // When
        var violations = new SectionStructureRule().Evaluate(model).Where(static violation => violation.RuleId == "SPEC013").ToList();

        // Then
        violations.Select(static violation => violation.File).Should().Equal("src/second/.spec/README.md");
    }

    private static FeatureSpec Feature(string path, string text) =>
        new FeatureSpecFixture()
            .WithLocation(new SpecLocationFixture().WithRelativePath(path))
            .WithDocument(SpecDocument.Parse(text, path));
}
