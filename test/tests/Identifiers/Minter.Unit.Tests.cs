using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Identifiers;

namespace Specht.Tests.Identifiers;

/// <summary>
/// The minting pipeline beneath <c>0001-F19</c> B-008 and C-3 (C-12; ADR-0033; item <c>0414</c>): <see cref="Minter"/>
/// reads the requested type's entry under the manifest's top-level <c>mint</c> key and builds the Nano ID strategy from that
/// entry's own <c>nanoid</c> sub-object and the type alone, absent or not, and returns one id. No prefix decorator exists at
/// this step, so the whole id is the part after the type's prefix. It mints no id for a type <c>mint</c> does not name
/// (B-046), an entry naming no strategy (B-047), or a strategy other than <c>nanoid</c> or <c>snowflake</c> (B-048): it
/// refuses, and draws nothing. Each manifest is written in memory, and the draws come from a
/// <see cref="FixedRandomSource"/>.
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
        var minter = new Minter(new FixedRandomSource(1));

        // When
        var adr = minter.Mint(manifest, "adr");
        var lesson = minter.Mint(manifest, "lesson");

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

        var minter = new Minter(new FixedRandomSource(1, 0));

        // When
        var id = minter.Mint(manifest, "adr");

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
        var mintAdr = () => new Minter(random).Mint(manifest, "adr");

        // Then
        mintAdr.Should().Throw<IdRefusedException>();
        random.Bounds.Should().BeEmpty();
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
        var mintAdr = () => new Minter(random).Mint(manifest, "adr");

        // Then
        mintAdr.Should().Throw<IdRefusedException>();
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
        var mintAdr = () => new Minter(random).Mint(manifest, "adr");

        // Then
        mintAdr.Should().Throw<IdRefusedException>();
        random.Bounds.Should().BeEmpty();
    }
}
