using AwesomeAssertions;
using Specht.Identifiers;

namespace Specht.Tests.Identifiers;

/// <summary>
/// The random source beneath <c>0001-F19</c> B-006 and C-7: <see cref="RandomSource"/> answers each request with a whole
/// number from zero to below the bound it is given, so the Nano ID strategy's index into its alphabet never falls outside it.
/// It draws from the platform's generator in memory, with no network. Only the range is asserted, not how the draws spread.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class RandomSourceUnitTests
{
    [Theory]
    [ClassData(typeof(RandomSourceBounds))]
    public void ABound_WhenDrawnBelowRepeatedly_ShouldOnlyGiveWholeNumbersFromZeroToBelowTheBound(int bound)
    {
        // Given
        var random = new RandomSource();

        // When
        var draws = Enumerable.Range(0, 256).Select(_ => random.Next(bound)).ToList();

        // Then
        draws.Should().OnlyContain(draw => draw >= 0 && draw < bound);
    }
}
