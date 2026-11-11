using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Identifiers;

namespace Specht.Tests.Identifiers;

/// <summary>
/// The Nano ID strategy beneath <c>0001-F19</c> B-005, B-006, B-019 and B-020 (ADR-0033): built from a type's <c>nanoid</c>
/// sub-object and the type's name, it draws one character for each of the size the sub-object names, each the alphabet's
/// character at a draw below the alphabet's length; an absent alphabet is lower-case base32 and an absent <c>adr</c> or
/// <c>lesson</c> size is 8, an absent <c>decision</c>, <c>claim</c>, <c>constraint</c>, <c>openQuestion</c> or <c>scenario</c> size is 6
/// (B-021, item <c>0418</c>), and an absent sub-object takes both. It mints no id from a sub-object that is not an object
/// (B-049), an alphabet that is empty or not a string (B-050), or a size that is not a positive whole number (B-051): its
/// constructor refuses, and nothing is drawn. The draws come from a <see cref="FixedRandomSource"/>, so nothing here is
/// random.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class NanoIdStrategyUnitTests
{
    [Theory]
    [ClassData(typeof(NanoIdSizes))]
    public void ANanoIdWithAConfiguredSize_WhenDrawn_ShouldBeThatManyCharactersLong(int size)
    {
        // Given
        var settings = new JsonObject { ["alphabet"] = "0123456789abcdefghjkmnpqrstvwxyz", ["size"] = size };
        var strategy = new NanoIdStrategy(settings, "adr", new FixedRandomSource(0));

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
        var strategy = new NanoIdStrategy(new JsonObject { ["alphabet"] = "abc", ["size"] = 4 }, "adr", random);

        // When
        var id = strategy.Next();

        // Then
        id.Should().Be("cabb");
        random.Bounds.Should().Equal(3, 3, 3, 3);
    }

    [Fact]
    public void ANanoIdNamingNoAlphabet_WhenDrawn_ShouldDrawFromLowerCaseBase32()
    {
        // Given
        var strategy = new NanoIdStrategy(new JsonObject { ["size"] = 32 }, "decision", new FixedRandomSource([.. Enumerable.Range(0, 32)]));

        // When
        var id = strategy.Next();

        // Then
        id.Should().Be("0123456789abcdefghjkmnpqrstvwxyz");
    }

    [Theory]
    [ClassData(typeof(RepositoryWideTypes))]
    public void ARepositoryWideTypesNanoIdNamingNoSize_WhenDrawn_ShouldBeEightCharactersLong(string type)
    {
        // Given
        var settings = new JsonObject { ["alphabet"] = "0123456789abcdefghjkmnpqrstvwxyz" };
        var strategy = new NanoIdStrategy(settings, type, new FixedRandomSource(0));

        // When
        var id = strategy.Next();

        // Then
        id.Should().HaveLength(8);
    }

    [Theory]
    [ClassData(typeof(PerFeatureTypes))]
    public void APerFeatureTypesNanoIdNamingNoSize_WhenDrawn_ShouldBeSixCharactersLong(string type)
    {
        // Given
        var settings = new JsonObject { ["alphabet"] = "0123456789abcdefghjkmnpqrstvwxyz" };
        var strategy = new NanoIdStrategy(settings, type, new FixedRandomSource(0));

        // When
        var id = strategy.Next();

        // Then
        id.Should().HaveLength(6);
    }

    [Theory]
    [ClassData(typeof(RepositoryWideTypes))]
    public void ARepositoryWideTypeWithNoNanoIdSubObject_WhenDrawn_ShouldDrawEightCharactersFromLowerCaseBase32(string type)
    {
        // Given
        var strategy = new NanoIdStrategy(null, type, new FixedRandomSource([.. Enumerable.Range(24, 8)]));

        // When
        var id = strategy.Next();

        // Then
        id.Should().Be("rstvwxyz");
    }

    [Theory]
    [ClassData(typeof(NanoIdSettingsNotObjects))]
    public void NanoIdSettingsThatAreNotAnObject_WhenDrawn_ShouldBeRefusedWithNoIdDrawn(string settings)
    {
        // Given
        var random = new FixedRandomSource(0);

        // When
        var draw = () => new NanoIdStrategy(JsonNode.Parse(settings), "adr", random).Next();

        // Then
        draw.Should().Throw<IdRefusedException>();
        random.Bounds.Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(AlphabetsNotNonEmptyText))]
    public void ANanoIdAlphabetThatIsEmptyOrNotAString_WhenDrawn_ShouldBeRefusedWithNoIdDrawn(string alphabet)
    {
        // Given
        var settings = new JsonObject { ["alphabet"] = JsonNode.Parse(alphabet), ["size"] = 4 };
        var random = new FixedRandomSource(0);

        // When
        var draw = () => new NanoIdStrategy(settings, "adr", random).Next();

        // Then
        draw.Should().Throw<IdRefusedException>();
        random.Bounds.Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(SizesNotPositiveWholeNumbers))]
    public void ANanoIdSizeThatIsNotAPositiveWholeNumber_WhenDrawn_ShouldBeRefusedWithNoIdDrawn(string size)
    {
        // Given
        var settings = new JsonObject { ["alphabet"] = "ab", ["size"] = JsonNode.Parse(size) };
        var random = new FixedRandomSource(0);

        // When
        var draw = () => new NanoIdStrategy(settings, "adr", random).Next();

        // Then
        draw.Should().Throw<IdRefusedException>();
        random.Bounds.Should().BeEmpty();
    }
}
