using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Shared;

namespace Specht.Tests.Rules;

/// <summary>
/// The feature-file rule over a model built in memory under a manifest that renames the claims section (<c>0001-F5</c>
/// B-001, decision 0006), its companion's tags read through an injected file system (ADR-0001 stage D, item 0107): a
/// scenario tag resolves against the section the claims role names, and the title the engine once hardcoded is a section
/// like any other.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class FeatureFileRuleUnitTests
{
    [Fact]
    public void ATagDeclaredOnlyUnderTheTitleNoRoleNames_WhenTheRuleEvaluatesTheModel_ShouldReportSpec021OnItAndNotOnTheTagTheClaimsRolesSectionDeclares()
    {
        // Given
        var manifest = new JsonObject
        {
            ["sections"] = new JsonArray("3. Claims", "9. Traceability Matrix", "12. Sign-off"),
            ["roles"] = new JsonObject { ["claims"] = "3. Claims" },
        };
        var companion = Path.Combine("repo", "src", "sample", ".spec", "sample.feature");
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                [companion] = new(
                    "Feature: it\n\n  @B-001\n  Scenario: It does the thing\n    Given a thing\n\n"
                        + "  @B-404\n  Scenario: Phantom\n    Given nothing\n"),
            });
        FeatureSpec feature = new FeatureSpecFixture()
            .WithDocument(
                SpecDocument.Parse(
                    "## 3. Acceptance Criteria\n\n| ID | Claim |\n| -- | ----- |\n| B-404 | Under the title no role names. |\n\n"
                        + "## 3. Claims\n\n| ID | Claim |\n| -- | ----- |\n| B-001 | Under the claims role's section. |\n",
                    "src/sample/.spec/README.md"))
            .WithFeatureFiles(companion);
        SpecModel model = new SpecModelFixture().WithManifest(manifest).WithFeatures(feature);

        // When
        var violations = new FeatureFileRule(new FeatureFileReader(fileSystem)).Evaluate(model).ToList();

        // Then
        violations.Select(static violation => (violation.RuleId, violation.Identifier)).Should().Equal(("SPEC021", "B-404"));
    }
}
