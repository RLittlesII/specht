using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Specht.IdGeneration;

namespace Specht.Tests.IdGeneration;

/// <summary>Item 0256 trial: the strategies and decorators behave as <c>0001-F19</c> B-005 to B-012 need; never merged.</summary>
[Trait("Tier", "Unit")]
public sealed class IdGenerationUnitTests
{
    [Fact]
    public void NanoIdSettings_WhenAnIdIsMinted_ShouldHaveTheSizeAndOnlyAlphabetCharacters()
    {
        // Given
        var generator = new NanoIdGenerator(new NanoIdSettings("ab", 12), new SeededRandomSource(new Random(1)));

        // When
        var id = generator.Next();

        // Then
        id.Should().HaveLength(12).And.MatchRegex("^[ab]+$");
    }

    [Fact]
    public void SnowflakeSettings_WhenAnIdIsMinted_ShouldDecodeToTheMillisecondsSinceTheEpoch()
    {
        // Given
        var generator = new SnowflakeGenerator(SnowflakeSettings.Supplied, new FixedClock(Minted), new SeededRandomSource(new Random(1)));

        // When
        var id = long.Parse(generator.Next(), System.Globalization.CultureInfo.InvariantCulture);

        // Then
        (id >> 22).Should().Be(Minted.ToUnixTimeMilliseconds() - 1767225600000);
    }

    [Fact]
    public void TwoSuppliedGenerators_WhenEachMintsInTheSameMillisecond_ShouldMintTheSameId()
    {
        // Given
        var clock = new FixedClock(Minted);
        var random = new SeededRandomSource(new Random(1));

        // When
        var first = new SnowflakeGenerator(SnowflakeSettings.Supplied, clock, random).Next();
        var second = new SnowflakeGenerator(SnowflakeSettings.Supplied, clock, random).Next();

        // Then
        second.Should().Be(first);
    }

    [Fact]
    public void ASnowflakeGenerator_WhenTheClockMovesBack_ShouldRefuse()
    {
        // Given
        var clock = new FixedClock(Minted);
        var generator = new SnowflakeGenerator(SnowflakeSettings.Supplied, clock, new SeededRandomSource(new Random(1)));
        generator.Next();
        clock.Now = Minted.AddSeconds(-1);

        // When
        var act = () => generator.Next();

        // Then
        act.Should().Throw<IdRefusedException>().WithMessage("clock moved backwards");
    }

    [Fact]
    public void FormattingWithBase32_WhenADecimalIdIsFormatted_ShouldPrefixThirteenCharactersThatDecodeBack()
    {
        // Given
        var formatting = new FormattingDecorator(new Constant("1234567890123456789"), "ADR-", IdEncoding.Base32);

        // When
        var id = formatting.Next();

        // Then
        id.Should().StartWith("ADR-").And.HaveLength(17);
        var expected = System.Numerics.BigInteger.Parse("1234567890123456789", System.Globalization.CultureInfo.InvariantCulture);
        FormattingDecorator.Decode(id[4..]).Should().Be(expected);
    }

    [Fact]
    public void ACollisionGuard_WhenTheFirstCandidateIsHeld_ShouldHandOutAnIdTheTreeDoesNotHold()
    {
        // Given
        var guard = new CollisionGuardDecorator(new Sequence("0001", "0002"), static id => id == "0001", 8);

        // When
        var id = guard.Next();

        // Then
        id.Should().Be("0002");
        guard.Refused.Should().Be(1);
    }

    [Fact]
    public void Validation_WhenTheGrammarRejectsTheCandidate_ShouldRefuse()
    {
        // Given
        var validation = new ValidationDecorator(new Constant("B-7k3m9q"), "claim", new Regex("^B-[0-9]{3}[a-z]?$"));

        // When
        var act = () => validation.Next();

        // Then
        act.Should().Throw<IdRefusedException>();
    }

    [Fact]
    public void AManifest_WhenATypeIsNotOptedIn_ShouldMintNothing()
    {
        // Given
        var manifest = Manifest();

        // When
        var act = () => Create(manifest, "constraint");

        // Then
        act.Should().Throw<IdRefusedException>().WithMessage("constraint is not opted in");
    }

    [Fact]
    public void AManifest_WhenTwoTypesAreOptedIn_ShouldMintEachWithItsOwnSettings()
    {
        // Given
        var manifest = Manifest();

        // When
        var claim = Create(manifest, "claim").Next();
        var lesson = Create(manifest, "lesson").Next();

        // Then
        claim.Should().MatchRegex("^B-[0-9a-hjkmnp-tv-z]{6}$");
        lesson.Should().MatchRegex("^[0-9a-hjkmnp-tv-z]{13}$");
    }

    [Fact]
    public void HeldIds_WhenAFeatureHoldsClaimsAndARootHoldsLessons_ShouldReadEachInItsScope()
    {
        // Given
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["/r/a/.spec/README.md"] = new("| B-001 | x |\n| B-7k3m9q | y |\n| C-1 | z |\n"),
            ["/r/.spec/lessons/0005-an-id.md"] = new("x"),
        });

        // When
        var claims = new HeldIds(fileSystem).Read("/r", "claim", "a");
        var lessons = new HeldIds(fileSystem).Read("/r", "lesson", null);

        // Then
        claims.Should().BeEquivalentTo("B-001", "B-7k3m9q");
        lessons.Should().BeEquivalentTo("0005");
    }

    private static IIdGenerator Create(JsonObject manifest, string type) =>
        IdMinter.Create(manifest, type, static source => source, new FixedClock(Minted), new SeededRandomSource(new Random(1)), static _ => false);

    private static JsonObject Manifest() => JsonNode.Parse("""
        {
          "identifiers": { "claim": "^B-([0-9]{3}[a-z]?|[0-9a-hjkmnp-tv-z]{6})$", "lesson": "^[0-9a-hjkmnp-tv-z]{13}$", "constraint": "^C-[0-9]+$" },
          "mint": {
            "claim": { "strategy": "nanoid", "nanoid": { "alphabet": "0123456789abcdefghjkmnpqrstvwxyz", "size": 6 }, "prefix": "B-" },
            "lesson": { "strategy": "snowflake", "snowflake": { "worker": "random", "sequenceStart": "random" }, "encoding": "base32" }
          }
        }
        """)!.AsObject();

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = now;

        public override DateTimeOffset GetUtcNow() => Now;
    }

    private sealed class Constant(string id) : IIdGenerator
    {
        public string Next() => id;
    }

    private sealed class Sequence(params string[] ids) : IIdGenerator
    {
        public string Next() => ids[_next++];

        private int _next;
    }

    private static readonly DateTimeOffset Minted = new(2026, 11, 6, 12, 0, 0, TimeSpan.Zero);
}
