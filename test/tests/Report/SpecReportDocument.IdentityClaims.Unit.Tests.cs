using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Model;
using Specht.Report;
using Specht.Rules;

namespace Specht.Tests.Report;

/// <summary>Rule derivations and their document mapping for 0001-F3 B-012 to B-015, in memory.</summary>
[Trait("Tier", "Unit")]
public sealed class SpecReportDocumentIdentityClaimsUnitTests
{
    [Theory]
    [ClassData(typeof(IdentityClaimExpectations))]
    public void AnIdentityOrTraceabilityViolation_WhenItsDocumentIsMade_ShouldCarryTheAgreedStructuredExpectation(
        string because, SpecModel model, ISpecRule rule, string ruleId, string identifier, JsonObject expected)
    {
        // Given
        var report = SpechtRunner.Evaluate(model, [rule]);

        // When
        var document = SpecReportDocument.From(report);

        // Then
        var violations = document.Violations.Where(violation => violation.RuleId == ruleId && violation.Identifier == identifier).ToList();
        violations.Should().NotBeEmpty(because).And.AllSatisfy(violation =>
            JsonNode.DeepEquals(violation.Expected, expected).Should().BeTrue(
                "{0}: expected {1}, but got {2}", because, expected.ToJsonString(), violation.Expected.ToJsonString()));
        if (expected["rowsByClaim"] is JsonObject counts)
        {
            foreach (var violation in violations)
            {
                violation.Expected["rowsByClaim"]!.AsObject().Select(static pair => pair.Key)
                    .Should().Equal(counts.Select(static pair => pair.Key), because);
            }
        }

        document.ToJson().Should().Be(SpecReportDocument.From(SpechtRunner.Evaluate(model, [rule])).ToJson(), because);
    }
}
