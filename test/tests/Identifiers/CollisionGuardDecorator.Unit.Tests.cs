using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Identifiers;
using Specht.Tests.Mint;

namespace Specht.Tests.Identifiers;

/// <summary>
/// The collision guard around a strategy (<c>0001-F19</c> C-4; B-010, B-017, B-034 to B-037, B-052), in memory: the strategy
/// is <see cref="NanoIdStrategy"/> drawing one character from a short alphabet, so each draw from
/// <see cref="FixedRandomSource"/> is one candidate and the number of candidates drawn is the number of bounds it records.
/// For a Snowflake type it is <see cref="SnowflakeStrategy"/> under a <see cref="FixedClock"/>, and the candidates held are
/// the ones a second strategy built from the same settings, draws and readings produces.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class CollisionGuardDecoratorUnitTests
{
    [Fact]
    public void AHeldNameStartingWithTheFirstCandidateInCapitalLetters_WhenDrawn_ShouldGiveACandidateNoHeldNameStartsWithInAnyLetterCase()
    {
        // Given
        var random = new FixedRandomSource(0, 1);
        var strategy = new NanoIdStrategy(JsonNode.Parse("""{ "alphabet": "ab", "size": 1 }"""), "adr", random);
        var guard = new CollisionGuardDecorator(strategy, null, "adr", ["A-held.md"]);

        // When
        var id = guard.Next();

        // Then
        id.Should().NotBeEmpty();
        "A-held.md".StartsWith(id, StringComparison.OrdinalIgnoreCase).Should().BeFalse($"'{id}' is held as 'A'");
    }

    [Fact]
    public void NoAttemptsNamedAndTheFirstSevenCandidatesHeld_WhenDrawn_ShouldGiveTheEighthCandidate()
    {
        // Given
        var random = new FixedRandomSource(0, 1, 2, 3, 4, 5, 6, 7, 8);
        var strategy = new NanoIdStrategy(JsonNode.Parse("""{ "alphabet": "abcdefghi", "size": 1 }"""), "adr", random);
        var guard = new CollisionGuardDecorator(strategy, null, "adr", [.. "abcdefg".Select(static held => $"{held}-held.md")]);

        // When
        var id = guard.Next();

        // Then
        id.Should().Be("h");
    }

    [Theory]
    [ClassData(typeof(SnowflakeReadingsNotEarlier))]
    public void ASnowflakeWhoseFirstCandidateIsHeldAndAClockNotReadingEarlier_WhenDrawn_ShouldGiveACandidateTheTreeDoesNotHold(int later)
    {
        // Given
        var first = new SnowflakeStrategy(null, "adr", new FixedRandomSource(0), new FixedClock(FixedClock.CheckTime)).Next();
        var clock = new FixedClock(FixedClock.CheckTime, FixedClock.CheckTime.AddMilliseconds(later));
        var guard = new CollisionGuardDecorator(new SnowflakeStrategy(null, "adr", new FixedRandomSource(0), clock), null, "adr", [$"{first}-held.md"]);

        // When
        var id = guard.Next();

        // Then
        id.Should().NotBeEmpty();
        $"{first}-held.md".StartsWith(id, StringComparison.OrdinalIgnoreCase).Should().BeFalse($"'{id}' is held as '{first}'");
    }

    [Fact]
    public void NoAttemptsNamedAndTheFirstSevenSnowflakeCandidatesHeld_WhenDrawn_ShouldGiveTheEighthCandidate()
    {
        // Given
        var candidates = new SnowflakeStrategy(null, "adr", new FixedRandomSource(0), new FixedClock(FixedClock.CheckTime));
        var eight = Enumerable.Range(0, 8).Select(_ => candidates.Next()).ToList();
        var strategy = new SnowflakeStrategy(null, "adr", new FixedRandomSource(0), new FixedClock(FixedClock.CheckTime));
        var guard = new CollisionGuardDecorator(strategy, null, "adr", [.. eight[..7].Select(static held => $"{held}-held.md")]);

        // When
        var id = guard.Next();

        // Then
        id.Should().Be(eight[7]);
    }

    [Theory]
    [ClassData(typeof(GuardsHoldingEveryAttempt))]
    public void EveryCandidateItsAttemptsDrawHeld_WhenDrawn_ShouldBeRefusedAfterDrawingThatManyCandidates(int? attempts, int held)
    {
        // Given
        var random = new FixedRandomSource([.. Enumerable.Range(0, held + 1)]);
        var strategy = new NanoIdStrategy(JsonNode.Parse("""{ "alphabet": "abcdefghi", "size": 1 }"""), "adr", random);
        var limit = attempts is null ? null : JsonValue.Create(attempts.Value);
        var guard = new CollisionGuardDecorator(strategy, limit, "adr", [.. "abcdefghi"[..held].Select(static name => $"{name}-held.md")]);

        // When
        var draw = guard.Next;

        // Then
        draw.Should().Throw<IdRefusedException>();
        random.Bounds.Should().HaveCount(held);
    }

    [Theory]
    [ClassData(typeof(AttemptsNotPositiveWholeNumbers))]
    public void AnAttemptsThatIsNotAPositiveWholeNumber_WhenDrawn_ShouldBeRefusedWithNoCandidateDrawn(string attempts)
    {
        // Given
        var random = new FixedRandomSource(0);
        var strategy = new NanoIdStrategy(JsonNode.Parse("""{ "alphabet": "ab", "size": 1 }"""), "adr", random);

        // When
        var draw = () => new CollisionGuardDecorator(strategy, JsonNode.Parse(attempts), "adr", []).Next();

        // Then
        draw.Should().Throw<IdRefusedException>();
        random.Bounds.Should().BeEmpty();
    }
}
