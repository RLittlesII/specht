using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules;

/// <summary>
/// SPEC031's order and guards over a model built in memory (<c>0001-F1</c> B-004; item 0160): the Features' order and,
/// for one, the claims with no row or several in claim order before the rows naming no claim in row order; a Feature with
/// no claims or no matrix section is skipped, an empty first cell is not read, and a malformed claim is no claim, whatever
/// SPEC030's level.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class ClaimRowRuleUnitTests
{
    [Fact]
    public void MatricesMissingRepeatingAndNamingUnknownClaims_WhenTheClaimRowRuleEvaluatesTheModel_ShouldReportSpec031ClaimsThenUnknownRowsPerFeature()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            Feature(
                "src/b/.spec/README.md",
                Table(Claims, ("B-002", 10), ("B-001", 11), ("B-003", 12)),
                Table(Matrix, ("B-009", 20), ("B-003", 21), ("B-003", 22), ("B-008", 23))),
            Feature("src/a/.spec/README.md", Table(Claims, ("B-001", 10)), Table(Matrix, ("B-007", 20))));
        var rule = new ClaimRowRule();

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.File, finding.Line, finding.Identifier)).Should().Equal(
            ("SPEC031", "src/b/.spec/README.md", 10, "B-002"),
            ("SPEC031", "src/b/.spec/README.md", 11, "B-001"),
            ("SPEC031", "src/b/.spec/README.md", 22, "B-003"),
            ("SPEC031", "src/b/.spec/README.md", 20, "B-009"),
            ("SPEC031", "src/b/.spec/README.md", 23, "B-008"),
            ("SPEC031", "src/a/.spec/README.md", 10, "B-001"),
            ("SPEC031", "src/a/.spec/README.md", 20, "B-007"));
    }

    [Fact]
    public void FeaturesWithNoClaimsSectionOrNoMatrixSection_WhenTheClaimRowRuleEvaluatesTheModel_ShouldSkipThem()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithFeatures(
            Feature("src/no-claims/.spec/README.md", Table(Matrix, ("B-001", 20))),
            Feature("src/no-matrix/.spec/README.md", Table(Claims, ("B-001", 10))),
            Feature("src/both/.spec/README.md", Table(Claims, ("B-001", 10)), Table(Matrix)));
        var rule = new ClaimRowRule();

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.File, finding.Identifier)).Should().Equal(("SPEC031", "src/both/.spec/README.md", "B-001"));
    }

    [Theory]
    [ClassData(typeof(SiblingLevels))]
    public void AMalformedClaimAndEmptyFirstCells_WhenTheClaimRowRuleEvaluatesTheModel_ShouldNotCountThemAsClaimsWhateverTheSiblingLevel(string manifest)
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithManifest(JsonNode.Parse(manifest)!.AsObject()).WithFeatures(
            Feature(
                "src/sample/.spec/README.md",
                Table(Claims, ("B-1", 10), (" ", 11), ("B-001", 12)),
                Table(Matrix, ("B-1", 20), (" ", 21), ("B-001", 22))));
        var rule = new ClaimRowRule();

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.Line, finding.Identifier)).Should().Equal(("SPEC031", 20, "B-1"));
    }

    /// <summary>The manifest with SPEC030 at its default level, and with it off.</summary>
    public sealed class SiblingLevels : TheoryData<string>
    {
        public SiblingLevels()
        {
            Add("{}");
            Add("""{ "rules": { "SPEC030": "off" } }""");
        }
    }

    private static SpechtSection Table(string title, params (string Id, int Line)[] rows) =>
        new(title, 5, ["ID", "Text"], [.. rows.Select(static row => (IReadOnlyList<string>) [row.Id, "It holds."])], [.. rows.Select(static row => row.Line)]);

    private static FeatureSpec Feature(string path, params SpechtSection[] sections) =>
        new FeatureSpecFixture()
            .WithLocation(new SpechtLocationFixture().WithRelativePath(path))
            .WithDocument(new SpechtDocumentFixture().WithRelativePath(path).WithSections(sections));

    private const string Claims = "3. Acceptance Criteria";

    private const string Matrix = "9. Traceability Matrix";
}
