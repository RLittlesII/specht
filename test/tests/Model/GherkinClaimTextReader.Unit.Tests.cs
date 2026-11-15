using AwesomeAssertions;
using Specht.Model;

namespace Specht.Tests.Model;

/// <summary>
/// The Gherkin claim reader the check and the analyzer share, over claim file text with no file system: a claim's
/// declaration carries the keyword lines of the Scenarios and Scenario Outlines its tag reaches, the set <c>0001-F12</c>
/// A-1 and <c>0001-F13</c> B-001 count (<c>0001-F16</c> B-025; § 7, item <c>0390</c>); and a claim tag on an Examples block
/// gives a placement finding carrying the claim id it would declare, where a case with no claim gives a finding carrying
/// none (<c>0001-F16</c> B-009; § 7, item <c>0394</c>).
/// </summary>
[Trait("Tier", "Unit")]
public sealed class GherkinClaimTextReaderUnitTests
{
    [Theory]
    [ClassData(typeof(ClaimScenarioSets))]
    public void AClaimTagOnAnElement_WhenRead_ShouldCarryTheKeywordLinesOfEveryScenarioTheElementReachesInDocumentOrder(
        string text, int[] scenarios)
    {
        // Given
        var reader = new GherkinClaimTextReader(new GherkinClaimTags(
            new GherkinClaimTagForm("@{claim}", "claim", false, "B-[0-9]{3}"), null, null, ["@ignore"], [], []));

        // When
        var read = reader.Read(".spec/features/arithmetic/add.feature", text);

        // Then
        read.Claims.Should().ContainSingle().Which.Scenarios.Should().Equal(scenarios);
    }

    [Fact]
    public void AClaimTagOnAnExamplesBlock_WhenRead_ShouldGiveAPlacementFindingCarryingTheClaimIdAndAMissingClaimFindingCarryingNone()
    {
        // Given
        var reader = new GherkinClaimTextReader(new GherkinClaimTags(
            new GherkinClaimTagForm("@{claim}", "claim", false, "B-[0-9]{3}"), null, null, ["@ignore"], [], []));

        // When
        var read = reader.Read(
            ".spec/features/arithmetic/add.feature",
            "Feature: Arithmetic\n\n  Scenario Outline: Adds <a>\n    Given <a>\n\n    @smoke @B-005\n    Examples:\n      | a |\n      | 1 |\n");

        // Then
        read.Findings.Select(static finding => (finding.DiagnosticId, finding.Line, finding.ClaimId))
            .Should()
            .BeEquivalentTo([("SPCT0011", 6, (string?) "B-005"), ("SPCT0012", 3, (string?) null)]);
    }
}
