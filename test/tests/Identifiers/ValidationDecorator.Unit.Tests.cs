using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Identifiers;

namespace Specht.Tests.Identifiers;

/// <summary>
/// The validation decorator around a strategy (<c>0001-F19</c> C-4; B-011, B-012), in memory: the strategy is
/// <see cref="NanoIdStrategy"/> drawing from <see cref="FixedRandomSource"/>, so the candidate is known and the number of
/// candidates drawn is the number of bounds it records. A candidate the type's grammar does not match is refused at the
/// first draw, and no other is drawn; one it matches, the pattern as written and in one letter case (§ 7), is the id; and
/// a type with no grammar has its candidate as drawn (§ 7; OQ-19). The grammar is handed over as text, with the key a refusal
/// names (item <c>0437</c>). A candidate that starts with the other type's longer head, the reserved head, under the claim tag
/// case setting's comparison is refused at the first draw too, and one that does not, or a request with no reserved head, is
/// the id (B-076, B-077).
/// </summary>
[Trait("Tier", "Unit")]
public sealed class ValidationDecoratorUnitTests
{
    [Theory]
    [ClassData(typeof(GrammarsRejectingTheDraw))]
    public void ACandidateItsTypesGrammarDoesNotMatch_WhenDrawn_ShouldBeRefusedWithNoOtherCandidateDrawn(string alphabet, int size, string grammar)
    {
        // Given
        var random = new FixedRandomSource(0);
        var strategy = new NanoIdStrategy(new JsonObject { ["alphabet"] = alphabet, ["size"] = size }, "adr", random);
        var validation = new ValidationDecorator(strategy, grammar, "identifiers.adr", "adr", null, StringComparison.Ordinal);

        // When
        var draw = validation.Next;

        // Then
        draw.Should().Throw<IdRefusedException>();
        random.Bounds.Should().HaveCount(size);
    }

    [Theory]
    [ClassData(typeof(GrammarsMatchingTheDraw))]
    public void ACandidateItsTypesGrammarMatches_WhenDrawn_ShouldBeThatCandidate(string alphabet, int size, string grammar)
    {
        // Given
        var strategy = new NanoIdStrategy(new JsonObject { ["alphabet"] = alphabet, ["size"] = size }, "adr", new FixedRandomSource(0));
        var validation = new ValidationDecorator(strategy, grammar, "identifiers.adr", "adr", null, StringComparison.Ordinal);

        // When
        var id = validation.Next();

        // Then
        id.Should().Be(new string(alphabet[0], size));
    }

    [Fact]
    public void ATypeWithNoGrammar_WhenDrawn_ShouldBeTheCandidateAsDrawn()
    {
        // Given
        var strategy = new NanoIdStrategy(JsonNode.Parse("""{ "alphabet": "ab", "size": 2 }"""), "adr", new FixedRandomSource(1, 0));
        var validation = new ValidationDecorator(strategy, null, "identifiers.adr", "adr", null, StringComparison.Ordinal);

        // When
        var id = validation.Next();

        // Then
        id.Should().Be("ba");
    }

    [Theory]
    [ClassData(typeof(CandidatesStartingWithTheReservedHead))]
    public void ACandidateStartingWithTheReservedHead_WhenDrawn_ShouldBeRefusedWithNoOtherCandidateDrawn(
        string alphabet, string reserved, StringComparison comparison)
    {
        // Given
        var random = new FixedRandomSource(0);
        var strategy = new NanoIdStrategy(new JsonObject { ["alphabet"] = alphabet, ["size"] = 2 }, "claim", random);
        var validation = new ValidationDecorator(strategy, null, "ids.claim.id", "claim", reserved, comparison);

        // When
        var draw = validation.Next;

        // Then
        draw.Should().Throw<IdRefusedException>();
        random.Bounds.Should().HaveCount(2);
    }

    [Theory]
    [ClassData(typeof(CandidatesNotStartingWithTheReservedHead))]
    public void ACandidateNotStartingWithTheReservedHead_WhenDrawn_ShouldBeThatCandidate(string alphabet, string? reserved, StringComparison comparison)
    {
        // Given
        var strategy = new NanoIdStrategy(new JsonObject { ["alphabet"] = alphabet, ["size"] = 2 }, "claim", new FixedRandomSource(0));
        var validation = new ValidationDecorator(strategy, null, "ids.claim.id", "claim", reserved, comparison);

        // When
        var id = validation.Next();

        // Then
        id.Should().Be(new string(alphabet[0], 2));
    }
}
