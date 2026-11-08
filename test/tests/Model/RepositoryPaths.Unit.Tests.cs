using AwesomeAssertions;
using Specht.Model;

namespace Specht.Tests.Model;

[Trait("Tier", "Unit")]
public sealed class RepositoryPathsUnitTests
{
    [Theory]
    [ClassData(typeof(RepositoryGlobMatches))]
    public void ARootRelativePathAndGlob_WhenMatched_ShouldMatchByNormalisedSegmentsWithAFinalDoubleStarTakingEveryFileBeneath(
        string path, string glob, bool matches)
    {
        // Given
        var paths = new RepositoryPaths();

        // When
        var matched = paths.Matches(path, glob);

        // Then
        matched.Should().Be(matches);
    }

    [Theory]
    [ClassData(typeof(ClaimFileSetParityCases))]
    public void AClaimFileSetGlobSpelling_WhenNormalisedAndMatched_ShouldReadTheClaimFileOrRejectTheGlobAsTheAnalyzerDoes(
        string glob, string claimFile, bool? read)
    {
        // Given
        var paths = new RepositoryPaths();

        // When
        bool? matched;
        try
        {
            matched = paths.Matches(claimFile, paths.Normalize(glob));
        }
        catch (ArgumentException)
        {
            matched = null;
        }

        // Then
        matched.Should().Be(read);
    }
}
