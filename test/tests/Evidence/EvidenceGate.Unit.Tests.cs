using AwesomeAssertions;
using Specht.Tool.Features.Evidence;

namespace Specht.Tests.Evidence;

/// <summary>Required-link and outcome decisions under 0201-F5 B-003, B-004, B-008 to B-010 and B-017.</summary>
[Trait("Tier", "Unit")]
public sealed class EvidenceGateUnitTests
{
    [Theory]
    [InlineData(true, false, false, "exempt")]
    [InlineData(true, true, false, "exempt")]
    [InlineData(false, true, false, "satisfied")]
    [InlineData(false, false, true, "unavailable")]
    [InlineData(false, false, false, "missing")]
    public void LinkInputs_WhenEvaluated_ShouldRetainTheirIndependentState(bool exempt, bool satisfied, bool incomplete, string expected)
    {
        // Given

        // When
        var state = EvidenceGate.LinkState(exempt, satisfied, incomplete);

        // Then
        state.Should().Be(expected);
    }

    [Theory]
    [InlineData("passed", true)]
    [InlineData("failed", false)]
    [InlineData("skipped", false)]
    [InlineData("notRun", false)]
    [InlineData("aborted", false)]
    [InlineData(null, false)]
    public void AnExecutionOutcome_WhenEvaluated_ShouldPassOnlyIfPassed(string? outcome, bool expected)
    {
        // Given

        // When
        var passes = EvidenceGate.Passed(outcome);

        // Then
        passes.Should().Be(expected);
    }

    [Theory]
    [InlineData(false, false, false, false, 0)]
    [InlineData(false, false, false, true, 1)]
    [InlineData(false, false, true, true, 2)]
    [InlineData(false, true, true, true, 4)]
    [InlineData(true, true, true, true, 3)]
    public void OverlappingFailures_WhenFolded_ShouldUseDocumentedPrecedence(
        bool invalid, bool selectionMissing, bool incomplete, bool policyFailure, int expected)
    {
        // Given

        // When
        var code = EvidenceGate.ExitCode(invalid, selectionMissing, incomplete, policyFailure);

        // Then
        code.Should().Be(expected);
    }
}
