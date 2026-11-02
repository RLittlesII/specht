using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules;

/// <summary>
/// SPEC013's order and guard over a model built in memory (<c>0001-F1</c> B-004; item 0157): the Features' order and,
/// within one, the order of the manifest's <c>tables</c>; a section with no table is reported once, its headers never compared.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class TableHeaderRuleGuardsUnitTests
{
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

    private static JsonObject Tables => new()
    {
        ["tables"] = new JsonObject
        {
            ["signOff"] = new JsonArray("Role", "Name"),
            ["matrix"] = new JsonArray("Claim ID", "Test"),
        },
    };

    private static SpechtSection Section(string title, int line, params string[] headers) => new(title, line, headers, [], []);

    private static FeatureSpec Feature(string path, params SpechtSection[] sections) =>
        new FeatureSpecFixture()
            .WithLocation(new SpechtLocationFixture().WithRelativePath(path))
            .WithDocument(new SpechtDocument(path, new FrontmatterFixture(), sections));

    private const string Matrix = "9. Traceability Matrix";

    private const string SignOff = "12. Sign-off";
}
