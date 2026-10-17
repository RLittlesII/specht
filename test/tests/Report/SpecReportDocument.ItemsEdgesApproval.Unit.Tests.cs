using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Model;
using Specht.Report;
using Specht.Rules;

namespace Specht.Tests.Report;

/// <summary>Item, dependency and approval repair information derived by the rule and copied into the report.</summary>
[Trait("Tier", "Unit")]
public sealed class SpecReportDocumentItemsEdgesApprovalUnitTests
{
    [Theory]
    [ClassData(typeof(ItemsEdgesApprovalExpectations))]
    public void AnItemEdgeOrApprovalViolation_WhenItsDocumentIsMade_ShouldCarryTheAgreedRepairInformation(
        string because, SpecModel model, ISpecRule rule, string ruleId, string messagePart, string? identifier, JsonObject expected)
    {
        // Given
        var report = SpechtRunner.Evaluate(model, [rule]);

        // When
        var document = SpecReportDocument.From(report);

        // Then
        var violations = document.Violations.Where(violation => violation.RuleId == ruleId
            && violation.Message.Contains(messagePart, StringComparison.Ordinal)
            && (identifier is null || violation.Identifier == identifier)).ToList();
        violations.Should().NotBeEmpty(because).And.AllSatisfy(violation =>
            JsonNode.DeepEquals(violation.Expected, expected).Should().BeTrue(
                "{0}: expected {1}, but got {2}", because, expected.ToJsonString(), violation.Expected.ToJsonString()));
        document.ToJson().Should().Be(SpecReportDocument.From(SpechtRunner.Evaluate(model, [rule])).ToJson(), because);
    }
}
