using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules;

/// <summary>
/// SPEC010 over a model built in memory under a manifest's own section contract (<c>0001-F1</c> B-004; item 0157): each
/// Feature's missing and duplicated sections in the contract's order, then at most one out-of-order section, the first.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SectionContractRuleUnitTests
{
    [Fact]
    public void FeaturesMissingDuplicatingAndMisorderingSections_WhenTheSectionContractRuleEvaluatesTheModel_ShouldReportSpec010InContractOrderThenTheOrder()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithManifest(Contract).WithFeatures(
            Feature("src/b/.spec/README.md", Section(SignOff, 5), Section(Claims, 9), Section(SignOff, 13)),
            Feature("src/a/.spec/README.md", Section(Claims, 5)));
        var rule = new SectionContractRule();

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.File, finding.Line, finding.Identifier)).Should().Equal(
            ("SPEC010", "src/b/.spec/README.md", 0, Matrix),
            ("SPEC010", "src/b/.spec/README.md", 0, SignOff),
            ("SPEC010", "src/b/.spec/README.md", 5, SignOff),
            ("SPEC010", "src/a/.spec/README.md", 0, Matrix),
            ("SPEC010", "src/a/.spec/README.md", 0, SignOff));
    }

    [Fact]
    public void AFeatureWhoseSectionsAreAllInReverseOrder_WhenTheSectionContractRuleEvaluatesTheModel_ShouldReportSpec010OnTheFirstSectionOutOfPlaceAlone()
    {
        // Given
        SpechtModel model = new SpechtModelFixture().WithManifest(Contract).WithFeatures(
            Feature("src/sample/.spec/README.md", Section(SignOff, 5), Section(Matrix, 9), Section(Claims, 13)));
        var rule = new SectionContractRule();

        // When
        var findings = rule.Evaluate(model).ToList();

        // Then
        findings.Select(finding => (rule.Id, finding.Line, finding.Identifier)).Should().Equal(("SPEC010", 5, SignOff));
    }

    private static JsonObject Contract => new() { ["sections"] = new JsonArray(Claims, Matrix, SignOff) };

    private static SpechtSection Section(string title, int line) => new(title, line, [], [], []);

    private static FeatureSpec Feature(string path, params SpechtSection[] sections) =>
        new FeatureSpecFixture()
            .WithLocation(new SpechtLocationFixture().WithRelativePath(path))
            .WithDocument(new SpechtDocumentFixture().WithRelativePath(path).WithSections(sections));

    private const string Claims = "3. Acceptance Criteria";

    private const string Matrix = "9. Traceability Matrix";

    private const string SignOff = "12. Sign-off";
}
