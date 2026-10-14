using AwesomeAssertions;
using Specht.Model;

namespace Specht.Tests.Model;

/// <summary>
/// The claim tags parsed from in-memory Gherkin text (ADR-0010 stage 2): each tag's id
/// and one-based line, and no tag from a commented line. Nothing touches the disk.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class FeatureTagParserLegacyUnitTests
{
    [Fact]
    public void AFeatureFileCarryingClaimTags_WhenItsTagsAreRead_ShouldGiveEachTagsIdAndItsOneBasedLine()
    {
        // Given
        var text = "Feature: it\n\n  @B-001\n  Scenario: It does the thing\n    Given a thing\n\n"
            + "  @B-002 @B-006b @boundary\n  Scenario: It does two more\n    Given a thing\n";

        // When
        var tags = new FeatureTagParser().Parse(text);

        // Then
        tags.Should().Equal(new FeatureTag("B-001", 3), new FeatureTag("B-002", 7), new FeatureTag("B-006b", 7));
    }

    [Fact]
    public void AClaimTagOnACommentedLine_WhenTheFilesTagsAreRead_ShouldNotBeAmongThem()
    {
        // Given
        var text = "Feature: it\n\n  # Superseded: @B-404 and @B-405 were withdrawn\n  @B-001\n  Scenario: It does the thing\n";

        // When
        var tags = new FeatureTagParser().Parse(text);

        // Then
        tags.Should().Equal(new FeatureTag("B-001", 4));
    }

}
