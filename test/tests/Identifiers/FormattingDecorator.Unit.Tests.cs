using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Identifiers;

namespace Specht.Tests.Identifiers;

/// <summary>
/// The formatting decorator around a strategy (<c>0001-F19</c> C-4, C-12, C-15; B-009, B-033, B-053, B-054, B-055), in
/// memory: the strategy is <see cref="NanoIdStrategy"/> drawing from <see cref="FixedRandomSource"/>, so the draw is known
/// and the number of draws is the number of bounds it records. A prefix starts the id, ahead of the draw, under an encoding
/// named <c>none</c> or none named; a type naming none gets its in-tree prefix, and the id of <c>adr</c>, <c>lesson</c> and
/// <c>decision</c> is the draw alone. It mints no id for a prefix that is not text, a Nano ID encoded in <c>base32</c>, or an
/// encoding formatting does not offer, and draws nothing.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class FormattingDecoratorUnitTests
{
    [Theory]
    [ClassData(typeof(NanoIdEncodingsOffered))]
    public void ATypeWhoseFormattingNamesAPrefix_WhenDrawn_ShouldStartWithThatPrefixAheadOfTheDraw(string? encoding)
    {
        // Given
        var strategy = new NanoIdStrategy(JsonNode.Parse("""{ "alphabet": "ab", "size": 2 }"""), "adr", new FixedRandomSource(1));
        var formatting = new FormattingDecorator(
            strategy, JsonValue.Create("ADR-"), encoding is null ? null : JsonNode.Parse(encoding), "nanoid", "adr");

        // When
        var id = formatting.Next();

        // Then
        id.Should().Be("ADR-bb");
    }

    [Theory]
    [ClassData(typeof(TypesNamingNoPrefix))]
    public void ATypeWhoseFormattingNamesNoPrefix_WhenDrawn_ShouldStartWithItsInTreePrefixAheadOfTheDraw(string type, string prefix)
    {
        // Given
        var strategy = new NanoIdStrategy(JsonNode.Parse("""{ "alphabet": "ab", "size": 2 }"""), type, new FixedRandomSource(1));
        var formatting = new FormattingDecorator(strategy, null, null, "nanoid", type);

        // When
        var id = formatting.Next();

        // Then
        id.Should().Be(prefix + "bb");
    }

    [Theory]
    [ClassData(typeof(PrefixesNotText))]
    public void APrefixThatIsNotText_WhenDrawn_ShouldBeRefusedWithNoCandidateDrawn(string prefix)
    {
        // Given
        var random = new FixedRandomSource(0);
        var strategy = new NanoIdStrategy(JsonNode.Parse("""{ "alphabet": "ab", "size": 1 }"""), "adr", random);

        // When
        var draw = () => new FormattingDecorator(strategy, JsonNode.Parse(prefix), null, "nanoid", "adr").Next();

        // Then
        draw.Should().Throw<IdRefusedException>();
        random.Bounds.Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(NanoIdAlphabetsEncodedInBase32))]
    public void ANanoIdWithTheEncodingBase32_WhenDrawn_ShouldBeRefusedWithNoCandidateDrawn(string alphabet)
    {
        // Given
        var random = new FixedRandomSource(0);
        var strategy = new NanoIdStrategy(new JsonObject { ["alphabet"] = alphabet, ["size"] = 8 }, "adr", random);

        // When
        var draw = () => new FormattingDecorator(strategy, null, JsonValue.Create("base32"), "nanoid", "adr").Next();

        // Then
        draw.Should().Throw<IdRefusedException>();
        random.Bounds.Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(EncodingsNotOffered))]
    public void AnEncodingNeitherNoneNorBase32_WhenDrawn_ShouldBeRefusedWithNoCandidateDrawn(string encoding)
    {
        // Given
        var random = new FixedRandomSource(0);
        var strategy = new NanoIdStrategy(JsonNode.Parse("""{ "alphabet": "ab", "size": 1 }"""), "adr", random);

        // When
        var draw = () => new FormattingDecorator(strategy, null, JsonNode.Parse(encoding), "nanoid", "adr").Next();

        // Then
        draw.Should().Throw<IdRefusedException>();
        random.Bounds.Should().BeEmpty();
    }
}
