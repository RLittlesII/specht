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
}
