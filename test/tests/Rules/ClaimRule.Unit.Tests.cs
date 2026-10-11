using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules;

/// <summary>
/// The claim rule over a model built in memory under a manifest that renames a section (<c>0001-F5</c> B-001, decision
/// 0006): the claims and the matrix are read from the section the role names, and the title the engine once hardcoded is
/// a section like any other.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class ClaimRuleUnitTests
{
    [Fact]
    public void AMalformedClaimIdUnderTheSectionTheClaimsRoleNames_WhenTheClaimRuleEvaluatesTheModel_ShouldReportSpec030OnThatSectionAlone()
    {
        // Given
        var manifest = new JsonObject
        {
            ["sections"] = new JsonArray("3. Claims", "9. Traceability Matrix", "12. Sign-off"),
            ["roles"] = new JsonObject { ["claims"] = "3. Claims" },
        };
        FeatureSpec feature = new FeatureSpecFixture().WithDocument(
            SpecDocument.Parse(
                "## 3. Acceptance Criteria\n\n| ID | Claim |\n| -- | ----- |\n| OLD-1 | Under the title no role names. |\n\n"
                    + "## 3. Claims\n\n| ID | Claim |\n| -- | ----- |\n| B-1 | Under the claims role's section. |\n",
                "src/sample/.spec/README.md"));
        SpecModel model = new SpecModelFixture().WithManifest(manifest).WithFeatures(feature);

        // When
        var violations = new ClaimRule().Evaluate(model).ToList();

        // Then
        violations.Select(static violation => (violation.RuleId, violation.Identifier)).Should().Equal(("SPEC030", "B-1"));
    }

    [Fact]
    public void AClaimWithNoRowUnderTheSectionTheMatrixRoleNames_WhenTheClaimRuleEvaluatesTheModel_ShouldReportSpec031AgainstThatSectionAlone()
    {
        // Given
        var manifest = new JsonObject
        {
            ["sections"] = new JsonArray("3. Acceptance Criteria", "9. Coverage", "12. Sign-off"),
            ["roles"] = new JsonObject { ["matrix"] = "9. Coverage" },
        };
        FeatureSpec feature = new FeatureSpecFixture().WithDocument(
            SpecDocument.Parse(
                "## 3. Acceptance Criteria\n\n| ID | Claim |\n| -- | ----- |\n| B-001 | It does the thing. |\n| B-002 | It does another. |\n\n"
                    + "## 9. Traceability Matrix\n\n| Claim ID | Test |\n| -------- | ---- |\n| B-001 | A test |\n| B-002 | A test |\n\n"
                    + "## 9. Coverage\n\n| Claim ID | Test |\n| -------- | ---- |\n| B-001 | A test |\n",
                "src/sample/.spec/README.md"));
        SpecModel model = new SpecModelFixture().WithManifest(manifest).WithFeatures(feature);

        // When
        var violations = new ClaimRule().Evaluate(model).ToList();

        // Then
        violations.Select(static violation => (violation.RuleId, violation.Identifier)).Should().Equal(("SPEC031", "B-002"));
    }
}
