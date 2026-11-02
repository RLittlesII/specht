using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Evaluation;
using Specht.Model;
using Specht.Rules;
using Specht.Tests.Shared;

namespace Specht.Tests.Evaluation;

/// <summary>
/// The id the evaluator reports a rule's finding under, by the model's pinned version (<c>0001-F7</c> B-151, B-152;
/// ADR-0029): under 0.3.0 a rule is reported under its four-digit id, and under 0.1.0, 0.2.0 and 0.2.1 under its
/// three-digit id. The rule is the shipped one, the model is built in memory, and no tree on disk is read.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtEvaluatorVersionsUnitTests
{
    [Theory]
    [ClassData(typeof(ClaimRowIdsByVersion))]
    public void AMissingTraceabilityRow_WhenEvaluatedUnderAPinnedVersion_ShouldBeReportedUnderThatVersionsIdForTheRule(
        string pin,
        string expected)
    {
        // Given
        FeatureSpec feature = new FeatureSpecFixture().WithDocument(
            new SpechtDocumentParser(new FrontmatterReader()).Parse(
                "## 3. Acceptance Criteria\n\n| ID | Claim |\n| -- | ----- |\n| B-001 | It does the thing. |\n| B-002 | It does another. |\n\n"
                    + "## 9. Traceability Matrix\n\n| Claim ID | Test |\n| -------- | ---- |\n| B-001 | A test |\n",
                "src/sample/.spec/README.md"));
        SpechtModel model = new SpechtModelFixture().WithManifest(new JsonObject { ["schemaVersion"] = pin }).WithFeatures(feature);
        var evaluator = new SpechtEvaluator([new ClaimRowRule()], new SpechtViolationOrder());

        // When
        var report = evaluator.Evaluate(model);

        // Then
        report.Violations.Select(static violation => (violation.RuleId, violation.Identifier)).Should().Equal((expected, "B-002"));
    }
}
