using AwesomeAssertions;
using Specht.IdGeneration;

namespace Specht.Tests.IdGeneration;

/// <summary>
/// The Nano ID strategy beneath <c>0001-F19</c> B-005 and B-006 (ADR-0033): it draws one character for each of its
/// configured size, and each character is the configured alphabet's character at a draw below the alphabet's length. The
/// draws come from a <see cref="FixedRandomSource"/>, so nothing here is random.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class NanoIdStrategyUnitTests
{
    [Theory]
    [ClassData(typeof(NanoIdSizes))]
    public void ANanoIdWithAConfiguredSize_WhenDrawn_ShouldBeThatManyCharactersLong(int size)
    {
        // Given
        var strategy = new NanoIdStrategy(new NanoIdSettings("0123456789abcdefghjkmnpqrstvwxyz", size), new FixedRandomSource(0));

        // When
        var id = strategy.Next();

        // Then
        id.Should().HaveLength(size);
    }

    [Fact]
    public void ANanoIdWithAConfiguredAlphabet_WhenDrawn_ShouldTakeEachCharacterFromThatAlphabetAtItsDraw()
    {
        // Given
        var random = new FixedRandomSource(2, 0, 1, 1);
        var strategy = new NanoIdStrategy(new NanoIdSettings("abc", 4), random);

        // When
        var id = strategy.Next();

        // Then
        id.Should().Be("cabb");
        random.Bounds.Should().Equal(3, 3, 3, 3);
    }
}
