using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Rules;

namespace Specht.Tests.Rules;

/// <summary>
/// The claim tags of a Gherkin file read through an injected file system (ADR-0001 stage D, item 0107): each tag's id
/// and one-based line, and no tag from a commented line. Nothing touches the disk.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class FeatureFileReaderUnitTests
{
    [Fact]
    public void AFeatureFileCarryingClaimTags_WhenItsTagsAreRead_ShouldGiveEachTagsIdAndItsOneBasedLine()
    {
        // Given
        var reader = new FeatureFileReader(
            new MockFileSystem(
                new Dictionary<string, MockFileData>
                {
                    [RelativePath] = new(
                        "Feature: it\n\n  @B-001\n  Scenario: It does the thing\n    Given a thing\n\n"
                            + "  @B-002 @B-006b @boundary\n  Scenario: It does two more\n    Given a thing\n"),
                }));

        // When
        var tags = reader.ReadTags(RelativePath);

        // Then
        tags.Should().Equal(new FeatureTag("B-001", 3), new FeatureTag("B-002", 7), new FeatureTag("B-006b", 7));
    }

    [Fact]
    public void AClaimTagOnACommentedLine_WhenTheFilesTagsAreRead_ShouldNotBeAmongThem()
    {
        // Given
        var reader = new FeatureFileReader(
            new MockFileSystem(
                new Dictionary<string, MockFileData>
                {
                    [RelativePath] = new(
                        "Feature: it\n\n  # Superseded: @B-404 and @B-405 were withdrawn\n  @B-001\n  Scenario: It does the thing\n"),
                }));

        // When
        var tags = reader.ReadTags(RelativePath);

        // Then
        tags.Should().Equal(new FeatureTag("B-001", 4));
    }

    private static readonly string RelativePath = Path.Combine("repo", "src", "sample", ".spec", "sample.feature");
}
