using AwesomeAssertions;
using Specht.Model;

namespace Specht.Tests.Model;

/// <summary>
/// The Gherkin claim reader the check and the analyzer share, over claim file text with no file system: a claim's
/// declaration carries the keyword lines of the Scenarios and Scenario Outlines its tag reaches, the set <c>0001-F12</c>
/// A-1 and <c>0001-F13</c> B-001 count (<c>0001-F16</c> B-025; § 7, item <c>0390</c>).
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
}
