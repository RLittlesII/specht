using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Discovery;
using Specht.Identifiers;
using Specht.Tests.Discovery;
using Specht.Tests.Mint;

namespace Specht.Tests.Identifiers;

/// <summary>
/// The minting pipeline beneath <c>0001-F19</c> B-008 and C-3 (C-12; ADR-0033; item <c>0414</c>): <see cref="Minter"/>
/// reads the requested type's entry under the manifest's top-level <c>mint</c> key and builds the Nano ID strategy from that
/// entry's own <c>nanoid</c> sub-object and the type alone, absent or not, and returns one id. An entry that names no prefix
/// mints an id that is the part after the type's prefix alone. A prefix the entry names starts the id (B-009, item
/// <c>0417</c>), and the guard compares the id in that form, the candidate of A-5, with the names the tree holds (B-038). It
/// mints no id for a Nano ID entry with the encoding <c>base32</c> (B-033). It mints no id for a type <c>mint</c> does not name
/// (B-001), an entry naming no strategy (B-047), a strategy other than <c>nanoid</c> or <c>snowflake</c> (B-048), or any
/// request of a manifest whose <c>mint</c> names a type <c>identifiers</c> holds no grammar for (B-043): it refuses, and
/// draws nothing. The refusal of a type not opted in, or of an entry naming no strategy, is an
/// <see cref="IdNotOptedInException"/>, which the B-043 refusal is not, so the command can give it its own exit (B-002). A
/// <c>mint</c> member that opts no C-13 type in, one naming no such type or one naming no strategy, is outside the B-043
/// check, so it leaves a request for an opted-in type to mint its id (B-003). A Snowflake entry is minted from its own
/// <c>snowflake</c> sub-object and the clock, in base32 when it names no encoding (B-007, B-056; C-3, item <c>0419</c>), and
/// a Snowflake entry whose first candidate the tree holds gives no id when the clock reads earlier at the guard's next draw
/// (B-029). Each manifest is written in memory, the draws come from a <see cref="FixedRandomSource"/> and the time from a
/// <see cref="FixedClock"/>.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class MinterUnitTests
{
    [Fact]
    public void TwoTypesOptedInWithDifferentNanoIdSettings_WhenEachIsMinted_ShouldMintEachWithItsOwnSizeAndAlphabet()
    {
        // Given
        var manifest = JsonNode.Parse(
            """
            {
              "schemaVersion": "0.3.1",
              "identifiers": { "adr": "^[ab]{2}$", "lesson": "^[wxyz]{5}$" },
              "mint": {
                "adr": { "strategy": "nanoid", "nanoid": { "alphabet": "ab", "size": 2 } },
                "lesson": { "strategy": "nanoid", "nanoid": { "alphabet": "wxyz", "size": 5 } }
              }
            }
            """)!.AsObject();
        var minter = new Minter(
            new FixedRandomSource(1),
            new HeldIds(new SpechtDiscovery(new MockFileSystem(), new UnexpectedChildProcess())),
            new FixedClock(FixedClock.CheckTime));

        // When
        var adr = minter.Mint(manifest, "adr", ".", []);
        var lesson = minter.Mint(manifest, "lesson", ".", []);

        // Then
        adr.Should().Be("bb");
        lesson.Should().Be("xxxxx");
    }

    [Theory]
    [ClassData(typeof(NanoIdSubObjects))]
    public void ANanoIdEntryWithAlphabetsAndSizesOutsideItsNanoIdSubObject_WhenMinted_ShouldReadOnlyTheNanoIdSubObject(string? nanoid, string expected)
    {
        // Given
        var manifest = JsonNode.Parse(
            """
            {
              "schemaVersion": "0.3.1",
              "identifiers": { "adr": "^[0-9a-z]+$" },
              "mint": {
                "adr": {
                  "strategy": "nanoid",
                  "alphabet": "wxyz",
                  "size": 5,
                  "snowflake": {
                    "epoch": "2026-01-01",
                    "datacenterBits": 5,
                    "workerBits": 5,
                    "sequenceBits": 12,
                    "datacenter": 1,
                    "worker": 2,
                    "sequenceStart": 0,
                    "alphabet": "pq",
                    "size": 7
                  }
                }
              }
            }
            """)!.AsObject();
        if (nanoid is not null)
        {
            manifest["mint"]!["adr"]!["nanoid"] = JsonNode.Parse(nanoid);
        }

        var minter = new Minter(
            new FixedRandomSource(1, 0),
            new HeldIds(new SpechtDiscovery(new MockFileSystem(), new UnexpectedChildProcess())),
            new FixedClock(FixedClock.CheckTime));

        // When
        var id = minter.Mint(manifest, "adr", ".", []);

        // Then
        id.Should().Be(expected);
    }

    [Theory]
    [ClassData(typeof(TypesNotOptedIn))]
    public void ATypeTheManifestDoesNotOptIn_WhenMinted_ShouldBeRefusedWithNoIdDrawn(string? mint)
    {
        // Given
        var manifest = JsonNode.Parse(
            """
            {
              "schemaVersion": "0.3.1",
              "identifiers": { "adr": "^[ab]{2}$", "lesson": "^[ab]{2}$" }
            }
            """)!.AsObject();
        if (mint is not null)
        {
            manifest["mint"] = JsonNode.Parse(mint);
        }

        var random = new FixedRandomSource(0);

        // When
        var mintAdr = () => new Minter(
            random, new HeldIds(new SpechtDiscovery(new MockFileSystem(), new UnexpectedChildProcess())), new FixedClock(FixedClock.CheckTime))
            .Mint(manifest, "adr", ".", []);

        // Then
        mintAdr.Should().Throw<IdRefusedException>();
        random.Bounds.Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(TypesNotOptedIn))]
    public void ATypeTheManifestDoesNotOptIn_WhenMinted_ShouldBeRefusedAsNotOptedIn(string? mint)
    {
        // Given
        var manifest = JsonNode.Parse(
            """
            {
              "schemaVersion": "0.3.1",
              "identifiers": { "adr": "^[ab]{2}$", "lesson": "^[ab]{2}$" }
            }
            """)!.AsObject();
        if (mint is not null)
        {
            manifest["mint"] = JsonNode.Parse(mint);
        }

        // When
        var mintAdr = () => new Minter(
            new FixedRandomSource(0),
            new HeldIds(new SpechtDiscovery(new MockFileSystem(), new UnexpectedChildProcess())),
            new FixedClock(FixedClock.CheckTime))
            .Mint(manifest, "adr", ".", []);

        // Then
        mintAdr.Should().Throw<IdNotOptedInException>();
    }

    [Theory]
    [ClassData(typeof(EntriesNamingNoStrategy))]
    public void AnEntryNamingNoStrategy_WhenMinted_ShouldBeRefusedWithNoIdDrawn(string entry)
    {
        // Given
        var manifest = JsonNode.Parse(
            """
            {
              "schemaVersion": "0.3.1",
              "identifiers": { "adr": "^[ab]{2}$" }
            }
            """)!.AsObject();
        manifest["mint"] = new JsonObject { ["adr"] = JsonNode.Parse(entry) };
        var random = new FixedRandomSource(0);

        // When
        var mintAdr = () => new Minter(
            random, new HeldIds(new SpechtDiscovery(new MockFileSystem(), new UnexpectedChildProcess())), new FixedClock(FixedClock.CheckTime))
            .Mint(manifest, "adr", ".", []);

        // Then
        mintAdr.Should().Throw<IdNotOptedInException>();
        random.Bounds.Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(UnknownStrategies))]
    public void AnEntryNamingAStrategyOtherThanNanoIdOrSnowflake_WhenMinted_ShouldBeRefusedWithNoIdDrawn(string strategy)
    {
        // Given
        var manifest = JsonNode.Parse(
            """
            {
              "schemaVersion": "0.3.1",
              "identifiers": { "adr": "^[ab]{2}$" },
              "mint": { "adr": { "nanoid": { "alphabet": "ab", "size": 2 } } }
            }
            """)!.AsObject();
        manifest["mint"]!["adr"]!["strategy"] = strategy;
        var random = new FixedRandomSource(0);

        // When
        var mintAdr = () => new Minter(
            random, new HeldIds(new SpechtDiscovery(new MockFileSystem(), new UnexpectedChildProcess())), new FixedClock(FixedClock.CheckTime))
            .Mint(manifest, "adr", ".", []);

        // Then
        mintAdr.Should().Throw<IdRefusedException>();
        random.Bounds.Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(ManifestsOptingInATypeWithNoGrammar))]
    public void AManifestOptingInATypeWithNoGrammar_WhenAnyTypeIsMinted_ShouldBeRefusedWithNoIdDrawn(string manifest, string type)
    {
        // Given
        var document = JsonNode.Parse(manifest)!.AsObject();
        var random = new FixedRandomSource(0);

        // When
        var mint = () => new Minter(
            random,
            new HeldIds(new SpechtDiscovery(new MockFileSystem(), new UnexpectedChildProcess())),
            new FixedClock(FixedClock.CheckTime)).Mint(document, type, ".", []);

        // Then
        mint.Should().Throw<IdRefusedException>().Which.Should().NotBeOfType<IdNotOptedInException>();
        random.Bounds.Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(MintMembersOptingNoTypeIn))]
    public void AMintMemberOptingNoTypeInWithNoGrammar_WhenAnOptedInTypeIsMinted_ShouldMintItsId(string member)
    {
        // Given
        var manifest = JsonNode.Parse(
            """
            {
              "schemaVersion": "0.3.1",
              "identifiers": { "adr": "^z{3}$" },
              "mint": { "adr": { "strategy": "nanoid", "nanoid": { "alphabet": "z", "size": 3 } } }
            }
            """)!.AsObject();
        foreach (var (name, entry) in JsonNode.Parse(member)!.AsObject())
        {
            manifest["mint"]![name] = entry?.DeepClone();
        }

        // When
        var id = new Minter(
            new FixedRandomSource(0),
            new HeldIds(new SpechtDiscovery(new MockFileSystem(), new UnexpectedChildProcess())),
            new FixedClock(FixedClock.CheckTime))
            .Mint(manifest, "adr", ".", []);

        // Then
        id.Should().Be("zzz");
    }

    [Fact]
    public void AnEntryNamingAPrefix_WhenMinted_ShouldMintAnIdStartingWithThatPrefix()
    {
        // Given
        var manifest = JsonNode.Parse(
            """
            {
              "schemaVersion": "0.3.1",
              "identifiers": { "adr": "^ADR-[ab]{2}$" },
              "mint": { "adr": { "strategy": "nanoid", "nanoid": { "alphabet": "ab", "size": 2 }, "prefix": "ADR-" } }
            }
            """)!.AsObject();
        var minter = new Minter(
            new FixedRandomSource(1),
            new HeldIds(new SpechtDiscovery(new MockFileSystem(), new UnexpectedChildProcess())),
            new FixedClock(FixedClock.CheckTime));

        // When
        var id = minter.Mint(manifest, "adr", ".", []);

        // Then
        id.Should().Be("ADR-bb");
    }

    [Fact]
    public void AnEntryNamingAPrefixAndAHeldNameStartingWithThePrefixedFirstCandidate_WhenMinted_ShouldMintTheNextCandidateWithThatPrefix()
    {
        // Given
        var manifest = JsonNode.Parse(
            """
            {
              "schemaVersion": "0.3.1",
              "identifiers": { "adr": "^ADR-[xy]$" },
              "mint": { "adr": { "strategy": "nanoid", "nanoid": { "alphabet": "xy", "size": 1 }, "prefix": "ADR-" } }
            }
            """)!.AsObject();
        var files = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["repo/.spec/adr/ADR-x-held.md"] = new("# Held\n"),
        });
        var minter = new Minter(
            new FixedRandomSource(0, 1), new HeldIds(new SpechtDiscovery(files, new UnexpectedChildProcess())), new FixedClock(FixedClock.CheckTime));

        // When
        var id = minter.Mint(manifest, "adr", files.Path.GetFullPath("repo"), []);

        // Then
        id.Should().Be("ADR-y");
    }

    [Fact]
    public void ANanoIdEntryWithTheEncodingBase32_WhenMinted_ShouldBeRefusedWithNoIdDrawn()
    {
        // Given
        var manifest = JsonNode.Parse(
            """
            {
              "schemaVersion": "0.3.1",
              "identifiers": { "adr": "^[0-9a-z]+$" },
              "mint": { "adr": { "strategy": "nanoid", "nanoid": { "size": 8 }, "encoding": "base32" } }
            }
            """)!.AsObject();
        var random = new FixedRandomSource(0);

        // When
        var mintAdr = () => new Minter(
            random, new HeldIds(new SpechtDiscovery(new MockFileSystem(), new UnexpectedChildProcess())), new FixedClock(FixedClock.CheckTime))
            .Mint(manifest, "adr", ".", []);

        // Then
        mintAdr.Should().Throw<IdRefusedException>();
        random.Bounds.Should().BeEmpty();
    }

    [Fact]
    public void ASnowflakeEntryNamingNoEncodingWithSettingsOutsideItsSnowflakeSubObject_WhenMinted_ShouldMintFromTheSubObjectAndTheClockInBase32()
    {
        // Given
        var manifest = JsonNode.Parse(
            """
            {
              "schemaVersion": "0.3.1",
              "identifiers": { "adr": "^[0-9a-z]+$" },
              "mint": {
                "adr": {
                  "strategy": "snowflake",
                  "epoch": "2025-01-01",
                  "datacenter": 7,
                  "nanoid": { "epoch": "2024-01-01", "worker": 5, "sequenceStart": 9 },
                  "snowflake": { "datacenter": 0, "worker": 0, "sequenceStart": 0 }
                }
              }
            }
            """)!.AsObject();
        var minter = new Minter(
            new FixedRandomSource(1),
            new HeldIds(new SpechtDiscovery(new MockFileSystem(), new UnexpectedChildProcess())),
            new FixedClock(FixedClock.CheckTime));

        // When
        var id = minter.Mint(manifest, "adr", ".", []);

        // Then
        id.Should().Be("033y1mp000000");
    }

    [Fact]
    public void ASnowflakeEntryWhoseFirstCandidateIsHeldAndAClockReadingEarlierAtTheNextDraw_WhenMinted_ShouldBeRefused()
    {
        // Given
        var manifest = JsonNode.Parse(
            """
            {
              "schemaVersion": "0.3.1",
              "identifiers": { "adr": "^[0-9a-z]+$" },
              "mint": { "adr": { "strategy": "snowflake", "snowflake": { "datacenter": 0, "worker": 0, "sequenceStart": 0 } } }
            }
            """)!.AsObject();
        var files = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["repo/.spec/adr/033y1mp000000-held.md"] = new("# Held\n"),
        });
        var clock = new FixedClock(FixedClock.CheckTime, FixedClock.CheckTime.AddMilliseconds(-1));
        var minter = new Minter(new FixedRandomSource(0), new HeldIds(new SpechtDiscovery(files, new UnexpectedChildProcess())), clock);

        // When
        var mint = () => minter.Mint(manifest, "adr", files.Path.GetFullPath("repo"), []);

        // Then
        mint.Should().Throw<IdRefusedException>();
    }
}
