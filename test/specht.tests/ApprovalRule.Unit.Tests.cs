using System.Text.Json.Nodes;
using AwesomeAssertions;
using specht.Rules;

namespace specht.tests;

/// <summary>
/// The approval rule over a model built in memory under a manifest that renames a section or a marker (<c>0001-F5</c>
/// B-001, B-003, decision 0006): the matrix and the sign-off are read from the section the role names, and the missing,
/// approved, draft and blocked texts are the manifest's, so the text the engine once hardcoded marks nothing.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class ApprovalRuleUnitTests
{
    /// <summary>Gets each sign-off marker and the text the default manifest gives it, which a manifest renaming it no longer reads.</summary>
    public static TheoryData<string, string> SignOffMarkers { get; } = new()
    {
        { "draft", "\U0001F7E1" },
        { "blocked", "\U0001F534" },
    };

    [Fact]
    public void AMissingCellUnderTheSectionTheMatrixRoleNames_WhenTheApprovalRuleEvaluatesTheModel_ShouldReportSpec060OnThatSectionAlone()
    {
        // Given
        var manifest = new JsonObject
        {
            ["sections"] = new JsonArray("3. Acceptance Criteria", "9. Coverage", "12. Sign-off"),
            ["roles"] = new JsonObject { ["matrix"] = "9. Coverage" },
        };
        FeatureSpec feature = new FeatureSpecFixture().WithDocument(
            SpecDocument.Parse(
                "---\nspec_status: approved\n---\n\n"
                    + "## 9. Traceability Matrix\n\n| Claim ID | Test |\n| -------- | ---- |\n| B-001 | Missing |\n\n"
                    + "## 9. Coverage\n\n| Claim ID | Test |\n| -------- | ---- |\n| B-002 | Missing |\n",
                "src/sample/.spec/README.md"));
        SpecModel model = new SpecModelFixture().WithManifest(manifest).WithFeatures(feature);

        // When
        var violations = new ApprovalRule().Evaluate(model).ToList();

        // Then
        violations.Select(static violation => (violation.RuleId, violation.Identifier)).Should().Equal(("SPEC060", "B-002"));
    }

    [Fact]
    public void ACellHoldingTheManifestsMissingMarker_WhenTheApprovalRuleEvaluatesTheModel_ShouldReportSpec060OnItAndNotOnTheDefaultText()
    {
        // Given
        var manifest = new JsonObject { ["markers"] = new JsonObject { ["missing"] = "TBD" } };
        FeatureSpec feature = new FeatureSpecFixture().WithDocument(
            SpecDocument.Parse(
                "---\nspec_status: approved\n---\n\n"
                    + "## 9. Traceability Matrix\n\n| Claim ID | Test |\n| -------- | ---- |\n| B-001 | Missing |\n| B-002 | TBD |\n",
                "src/sample/.spec/README.md"));
        SpecModel model = new SpecModelFixture().WithManifest(manifest).WithFeatures(feature);

        // When
        var violations = new ApprovalRule().Evaluate(model).ToList();

        // Then
        violations.Select(static violation => (violation.RuleId, violation.Identifier)).Should().Equal(("SPEC060", "B-002"));
    }

    [Fact]
    public void ASpecificationWhoseStatusIsTheManifestsApprovedMarker_WhenTheApprovalRuleEvaluatesTheModel_ShouldReportSpec060OnItAndNotOnTheDefaultStatus()
    {
        // Given
        const string matrix = "## 9. Traceability Matrix\n\n| Claim ID | Test |\n| -------- | ---- |\n| B-001 | Missing |\n";
        var manifest = new JsonObject { ["markers"] = new JsonObject { ["approved"] = "agreed" } };
        FeatureSpec defaultStatus = new FeatureSpecFixture()
            .WithLocation(new SpecLocationFixture().WithRelativePath("src/first/.spec/README.md"))
            .WithDocument(SpecDocument.Parse("---\nspec_status: approved\n---\n\n" + matrix, "src/first/.spec/README.md"));
        FeatureSpec manifestStatus = new FeatureSpecFixture()
            .WithLocation(new SpecLocationFixture().WithRelativePath("src/second/.spec/README.md"))
            .WithDocument(SpecDocument.Parse("---\nspec_status: agreed\n---\n\n" + matrix, "src/second/.spec/README.md"));
        SpecModel model = new SpecModelFixture().WithManifest(manifest).WithFeatures(defaultStatus, manifestStatus);

        // When
        var violations = new ApprovalRule().Evaluate(model).ToList();

        // Then
        violations.Select(static violation => (violation.RuleId, violation.File)).Should().Equal(("SPEC060", "src/second/.spec/README.md"));
    }

    [Fact]
    public void AnUnapprovedRowUnderTheSectionTheSignOffRoleNames_WhenTheApprovalRuleEvaluatesTheModel_ShouldReportSpec061OnThatSectionAlone()
    {
        // Given
        var manifest = new JsonObject
        {
            ["sections"] = new JsonArray("3. Acceptance Criteria", "9. Traceability Matrix", "12. Approval"),
            ["roles"] = new JsonObject { ["signOff"] = "12. Approval" },
        };
        FeatureSpec feature = new FeatureSpecFixture().WithDocument(
            SpecDocument.Parse(
                "---\nspec_status: approved\n---\n\n"
                    + "## 12. Sign-off\n\n| Section | Status |\n| ------- | ------ |\n| 1-5 | \U0001F534 |\n\n"
                    + "## 12. Approval\n\n| Section | Status |\n| ------- | ------ |\n| 6-8 | \U0001F534 |\n",
                "src/sample/.spec/README.md"));
        SpecModel model = new SpecModelFixture().WithManifest(manifest).WithFeatures(feature);

        // When
        var violations = new ApprovalRule().Evaluate(model).ToList();

        // Then
        violations.Select(static violation => (violation.RuleId, violation.Identifier)).Should().Equal(("SPEC061", "6-8"));
    }

    [Theory]
    [MemberData(nameof(SignOffMarkers))]
    public void ASignOffCellHoldingTheManifestsMarker_WhenTheApprovalRuleEvaluatesTheModel_ShouldReportSpec061OnItAndNotOnTheDefaultText(
        string marker,
        string defaultText)
    {
        // Given
        var manifest = new JsonObject { ["markers"] = new JsonObject { [marker] = "HELD" } };
        FeatureSpec feature = new FeatureSpecFixture().WithDocument(
            SpecDocument.Parse(
                "---\nspec_status: approved\n---\n\n"
                    + $"## 12. Sign-off\n\n| Section | Status |\n| ------- | ------ |\n| 1-5 | {defaultText} |\n| 6-8 | HELD for review |\n",
                "src/sample/.spec/README.md"));
        SpecModel model = new SpecModelFixture().WithManifest(manifest).WithFeatures(feature);

        // When
        var violations = new ApprovalRule().Evaluate(model).ToList();

        // Then
        violations.Select(static violation => (violation.RuleId, violation.Identifier)).Should().Equal(("SPEC061", "6-8"));
    }
}
