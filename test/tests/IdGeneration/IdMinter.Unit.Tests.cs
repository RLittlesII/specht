using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.IdGeneration;

namespace Specht.Tests.IdGeneration;

/// <summary>
/// The minting pipeline beneath <c>0001-F19</c> B-005, B-006, B-008, B-019 and B-020 (C-3, C-12; ADR-0033; item
/// <c>0414</c>): <see cref="IdMinter"/> reads the requested type's entry under the manifest's top-level <c>mint</c> key,
/// builds the Nano ID strategy from that entry's own <c>nanoid</c> sub-object alone, fills an absent alphabet with
/// lower-case base32 and an absent <c>adr</c> or <c>lesson</c> size with 8, and returns one id. No prefix decorator exists
/// at this step, so the whole id is the part after the type's prefix. Each manifest is written in memory, and the draws
/// come from a <see cref="FixedRandomSource"/>.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class IdMinterUnitTests
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
        var minter = new IdMinter(new FixedRandomSource(1));

        // When
        var adr = minter.Mint(manifest, "adr");
        var lesson = minter.Mint(manifest, "lesson");

        // Then
        adr.Should().Be("bb");
        lesson.Should().Be("xxxxx");
    }

    [Fact]
    public void ATypeOptedInToNanoIdNamingNoAlphabet_WhenMinted_ShouldDrawFromLowerCaseBase32()
    {
        // Given
        var manifest = JsonNode.Parse(
            """
            {
              "schemaVersion": "0.3.1",
              "identifiers": { "decision": "^[0-9a-z]{32}$" },
              "mint": { "decision": { "strategy": "nanoid", "nanoid": { "size": 32 } } }
            }
            """)!.AsObject();
        var minter = new IdMinter(new FixedRandomSource([.. Enumerable.Range(0, 32)]));

        // When
        var id = minter.Mint(manifest, "decision");

        // Then
        id.Should().Be("0123456789abcdefghjkmnpqrstvwxyz");
    }

    [Theory]
    [ClassData(typeof(RepositoryWideTypes))]
    public void ARepositoryWideTypeOptedInToNanoIdNamingNoSize_WhenMinted_ShouldBeEightCharactersLong(string type)
    {
        // Given
        var manifest = new JsonObject
        {
            ["schemaVersion"] = "0.3.1",
            ["identifiers"] = new JsonObject { [type] = "^[0-9a-z]{8}$" },
            ["mint"] = new JsonObject
            {
                [type] = new JsonObject
                {
                    ["strategy"] = "nanoid",
                    ["nanoid"] = new JsonObject { ["alphabet"] = "0123456789abcdefghjkmnpqrstvwxyz" },
                },
            },
        };
        var minter = new IdMinter(new FixedRandomSource(0));

        // When
        var id = minter.Mint(manifest, type);

        // Then
        id.Should().HaveLength(8);
    }

    [Fact]
    public void ANanoIdEntryThatAlsoHoldsTheSnowflakeSubObject_WhenMinted_ShouldReadOnlyItsOwnNanoIdSettings()
    {
        // Given
        var manifest = JsonNode.Parse(
            """
            {
              "schemaVersion": "0.3.1",
              "identifiers": { "adr": "^[ab]{3}$" },
              "mint": {
                "adr": {
                  "strategy": "nanoid",
                  "nanoid": { "alphabet": "ab", "size": 3 },
                  "snowflake": {
                    "epoch": "2026-01-01",
                    "datacenterBits": 5,
                    "workerBits": 5,
                    "sequenceBits": 12,
                    "datacenter": 1,
                    "worker": 2,
                    "sequenceStart": 0
                  }
                }
              }
            }
            """)!.AsObject();
        var minter = new IdMinter(new FixedRandomSource(1, 0, 1));

        // When
        var id = minter.Mint(manifest, "adr");

        // Then
        id.Should().Be("bab");
    }
}
