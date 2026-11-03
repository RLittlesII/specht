using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules;

/// <summary>
/// The table-header check over a model built in memory under a manifest whose <c>tables</c> is keyed by role
/// (<c>0001-F5</c> B-002, decision 0006): the headers a role declares are expected of the section that role names. SPEC013's
/// order and guard (<c>0001-F1</c> B-004; item 0157): the Features' order and, within one, the order of the manifest's
/// <c>tables</c>; a section with no table is reported once, its headers never compared.
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

    [Fact]
    public void FeaturesWhoseSignOffAndMatrixHeadersDiffer_WhenTheTableHeaderRuleEvaluatesTheModel_ShouldReportSpec013InFeatureOrderThenTablesOrder()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithManifest(Tables).WithFeatures(
            Feature("src/b/.spec/README.md", Section(Matrix, 20, "Claim ID", "Proof"), Section(SignOff, 30, "Role", "Verdict")),
            Feature("src/a/.spec/README.md", Section(Matrix, 20, "Claim ID", "Proof"), Section(SignOff, 30, "Role", "Verdict")));
        var rule = new TableHeaderRule();

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.File, finding.Line, finding.Identifier)).Should().Equal(
            ("SPEC013", "src/b/.spec/README.md", 30, SignOff),
            ("SPEC013", "src/b/.spec/README.md", 20, Matrix),
            ("SPEC013", "src/a/.spec/README.md", 30, SignOff),
            ("SPEC013", "src/a/.spec/README.md", 20, Matrix));
    }

    [Fact]
    public void AMatrixSectionWithNoTable_WhenTheTableHeaderRuleEvaluatesTheModel_ShouldReportSpec013OnceWithoutComparingHeaders()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithManifest(Tables).WithFeatures(
            Feature("src/sample/.spec/README.md", Section(Matrix, 20), Section(SignOff, 30, "Role", "Name")));
        var rule = new TableHeaderRule();

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.Line, finding.Message)).Should().Equal(
            ("SPEC013", 20, $"section '## {Matrix}' carries no table - expected a table with headers Claim ID | Test"));
    }

    private static SpechtDocumentParser DocumentParser => new(new FrontmatterReader());

    private static JsonObject Tables => new()
    {
        ["tables"] = new JsonObject
        {
            ["signOff"] = new JsonArray("Role", "Name"),
            ["matrix"] = new JsonArray("Claim ID", "Test"),
        },
    };

    private static FeatureSpec Feature(string path, string text) =>
        new FeatureSpecFixture()
            .WithLocation(new SpechtLocationFixture().WithRelativePath(path))
            .WithDocument(DocumentParser.Parse(text, path));

    private static SpechtSection Section(string title, int line, params string[] headers) => new(title, line, headers, [], []);

    private static FeatureSpec Feature(string path, params SpechtSection[] sections) =>
        new FeatureSpecFixture()
            .WithLocation(new SpechtLocationFixture().WithRelativePath(path))
            .WithDocument(new SpechtDocumentFixture().WithRelativePath(path).WithSections(sections));

    private const string Matrix = "9. Traceability Matrix";

    private const string SignOff = "12. Sign-off";
}
