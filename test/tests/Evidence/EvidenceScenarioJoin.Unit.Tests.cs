using System.Text.Json;
using AwesomeAssertions;
using Specht.Tool.Features.Evidence;

namespace Specht.Tests.Evidence;

/// <summary>The explicit scenario join under 0201-F5 B-005.</summary>
[Trait("Tier", "Unit")]
public sealed class EvidenceScenarioJoinUnitTests
{
    [Fact]
    public void ExplicitCaseAndClaim_WhenJoined_ShouldExcludeUnrelatedCasesAndClaims()
    {
        // Given
        using var document = JsonDocument.Parse("""
            {
              "scope":{"epic":null,"feature":"cap:read"},
              "claims":[{"id":"need/7","status":"resolved","isEligible":true}],
              "cases":[
                {"caseId":"case-1","status":"resolved"},
                {"caseId":"case-2","status":"resolved"},
                {"caseId":null,"status":"unresolved"}
              ]
            }
            """);

        // When
        var matched = EvidenceScenarioJoin.Cases(document.RootElement, null, "cap:read", "need/7", ["case-1"]);
        var unrelatedClaim = EvidenceScenarioJoin.Cases(document.RootElement, null, "cap:read", "need/8", ["case-1"]);

        // Then
        matched.Should().ContainSingle().Which.Should().Be("case-1");
        unrelatedClaim.Should().BeEmpty();
    }

    [Theory]
    [InlineData("scenario-7", "src/read/.spec/read.feature#scenario-7")]
    [InlineData(null, "src/read/.spec/read.feature:12")]
    public void ScenarioIdentity_WhenProjected_ShouldUseExplicitIdOrRelativeLocation(string? id, string expected)
    {
        // Given
        using var document = JsonDocument.Parse($$"""
            {"scenarioId":{{(id is null ? "null" : "\"" + id + "\"")}},"path":"src/read/.spec/read.feature","line":12}
            """);

        // When
        var identity = EvidenceScenarioJoin.Identity(document.RootElement);

        // Then
        identity.Should().Be(expected);
    }
}
