using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Discovery;
using Specht.Identifiers;
using Specht.Model;
using Specht.Tests.Discovery;
using Specht.Tests.Shared;
using Specht.Tree;

namespace Specht.Tests.Identifiers;

/// <summary>
/// Minting from a type's <c>ids</c> record under schema <c>0.4.0</c> (<c>0001-F19</c> A-1, A-9, C-4, C-19, C-21; item
/// <c>0437</c>): <see cref="Minter"/> is handed the manifest the parser reads, structure and effective document, and reads a
/// type's settings from its record's <c>mint</c>. A per-Feature id takes the head <c>0001-F5</c> A-8 renders for the claim
/// prefix the Feature's specification declares, and an <c>adr</c> or <c>lesson</c> number the head for none (B-063, B-065 to
/// B-067, B-072); the guard compares the headed candidate with the Feature's places (B-070, B-071), and a Snowflake number is
/// whole after the head (B-073). A draw the record's <c>id</c> rejects (B-068), or that starts with the other type's longer
/// head under the claim tag case setting (B-076, B-077), is refused at the first candidate with nothing drawn again. A record
/// holding only its opt-in mints for every type and strategy (B-074). A <c>prefix</c> in any record's <c>mint</c> refuses
/// every request before anything is drawn (B-075). A refusal names the setting under the record, <c>ids.&lt;type&gt;.mint</c>
/// and <c>ids.&lt;type&gt;.id</c>. Each tree is a <see cref="MockFileSystem"/> holding <c>features/one</c>, the draws come
/// from a <see cref="FixedRandomSource"/> and the time from a <see cref="FixedClock"/>.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class MinterIdsUnitTests
{
    [Theory]
    [ClassData(typeof(RecordHeads))]
    public void ATypesRecordAndItsFeaturesClaimPrefix_WhenTheTypeIsMinted_ShouldWriteTheRecordsHeadAheadOfTheDraw(
        string record, string type, string frontmatter, string? feature, string expected)
    {
        // Given
        var files = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["repo/features/one/.spec/README.md"] = new($"{frontmatter}# Specification: F1\n"),
        });
        var root = files.Path.GetFullPath("repo");
        var manifest = EngineServices.Parser().ParseDocument($$"""{ "schemaVersion": "0.4.0", "ids": { "{{type}}": {{record}} } }""");
        var minter = new Minter(
            new FixedRandomSource(0),
            new HeldIds(
                new SpechtDiscovery(files, new UnexpectedChildProcess()),
                new SpechtTreeStore(files),
                new SpechtDocumentParser(new FrontmatterReader()),
                new FeatureTagParser()),
            new FixedClock(FixedClock.CheckTime));

        // When
        var id = minter.Mint(manifest, type, root, feature);

        // Then
        id.Should().Be(expected);
    }

    [Fact]
    public void AClaimDrawItsRecordsIdRejects_WhenMinted_ShouldBeRefusedNamingTheRecordsIdWithNoOtherCandidateDrawn()
    {
        // Given
        var files = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["repo/features/one/.spec/README.md"] = new("---\nclaim_prefix: \"ACTIVITY\"\n---\n# Specification: F1\n"),
        });
        var root = files.Path.GetFullPath("repo");
        var manifest = EngineServices.Parser().ParseDocument(
            """
            {
              "schemaVersion": "0.4.0",
              "ids": {
                "claim": { "form": "{prefix}-{id}", "id": "[0-9]{3}", "mint": { "strategy": "nanoid", "nanoid": { "alphabet": "ab", "size": 2 } } }
              }
            }
            """);
        var random = new FixedRandomSource(0);
        var minter = new Minter(
            random,
            new HeldIds(
                new SpechtDiscovery(files, new UnexpectedChildProcess()),
                new SpechtTreeStore(files),
                new SpechtDocumentParser(new FrontmatterReader()),
                new FeatureTagParser()),
            new FixedClock(FixedClock.CheckTime));

        // When
        var mint = () => minter.Mint(manifest, "claim", root, "features/one");

        // Then
        mint.Should().ThrowExactly<IdRefusedException>().WithMessage("*'ids.claim.id'*");
        random.Bounds.Should().HaveCount(2);
    }

    [Theory]
    [ClassData(typeof(ManifestsReservingTheOtherHead))]
    public void ADrawStartingWithTheOtherTypesLongerHead_WhenMinted_ShouldBeRefusedWithNoOtherCandidateDrawn(string text, string type)
    {
        // Given
        var files = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["repo/features/one/.spec/README.md"] = new("---\nclaim_prefix: \"ACTIVITY\"\n---\n# Specification: F1\n"),
        });
        var root = files.Path.GetFullPath("repo");
        var random = new FixedRandomSource(0);
        var minter = new Minter(
            random,
            new HeldIds(
                new SpechtDiscovery(files, new UnexpectedChildProcess()),
                new SpechtTreeStore(files),
                new SpechtDocumentParser(new FrontmatterReader()),
                new FeatureTagParser()),
            new FixedClock(FixedClock.CheckTime));

        // When
        var mint = () => minter.Mint(EngineServices.Parser().ParseDocument(text), type, root, "features/one");

        // Then
        mint.Should().ThrowExactly<IdRefusedException>();
        random.Bounds.Should().HaveCount(2);
    }

    [Theory]
    [ClassData(typeof(ManifestsReservingNoHeadTheDrawBegins))]
    public void ADrawNotStartingWithTheOtherTypesHead_WhenMinted_ShouldBeThatDrawUnderItsHead(string text, string type, string expected)
    {
        // Given
        var files = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["repo/features/one/.spec/README.md"] = new("---\nclaim_prefix: \"ACTIVITY\"\n---\n# Specification: F1\n"),
        });
        var root = files.Path.GetFullPath("repo");
        var minter = new Minter(
            new FixedRandomSource(0),
            new HeldIds(
                new SpechtDiscovery(files, new UnexpectedChildProcess()),
                new SpechtTreeStore(files),
                new SpechtDocumentParser(new FrontmatterReader()),
                new FeatureTagParser()),
            new FixedClock(FixedClock.CheckTime));

        // When
        var id = minter.Mint(EngineServices.Parser().ParseDocument(text), type, root, "features/one");

        // Then
        id.Should().Be(expected);
    }

    [Theory]
    [ClassData(typeof(HeadedFirstCandidatesHeld))]
    public void APrefixedFeatureHoldingTheFirstHeadedCandidate_WhenItsTypeIsMinted_ShouldMintTheNextCandidate(string type, string form, string expected)
    {
        // Given
        var files = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["repo/features/one/.spec/README.md"] = new(
                "---\nclaim_prefix: \"ACTIVITY\"\n---\n# Specification: F1\n\n"
                + "## 3. Acceptance Criteria\n\n| ID | Claim |\n| -- | ----- |\n| ACTIVITY-aa | Held. |\n\n"
                + "## 4. Constraints\n\n| ID | Constraint |\n| -- | ---------- |\n| ACTIVITY-C-aa | Held. |\n\n"
                + "## 11. Open Questions\n\n| ID | Question |\n| -- | -------- |\n| ACTIVITY-Q-aa | Held? |\n"),
            ["repo/features/one/.spec/one.feature"] = new("Feature: it\n\n  @ACTIVITY-S-aa\n  Scenario: It is held\n    Given a thing\n"),
        });
        var root = files.Path.GetFullPath("repo");
        var manifest = EngineServices.Parser().ParseDocument(
            $$"""
            {
              "schemaVersion": "0.4.0",
              "ids": {
                "{{type}}": { "form": "{{form}}", "id": "[ab]+", "mint": { "strategy": "nanoid", "nanoid": { "alphabet": "ab", "size": 2 } } }
              }
            }
            """);
        var minter = new Minter(
            new FixedRandomSource(0, 0, 1, 1),
            new HeldIds(
                new SpechtDiscovery(files, new UnexpectedChildProcess()),
                new SpechtTreeStore(files),
                new SpechtDocumentParser(new FrontmatterReader()),
                new FeatureTagParser()),
            new FixedClock(FixedClock.CheckTime));

        // When
        var id = minter.Mint(manifest, type, root, "features/one");

        // Then
        id.Should().Be(expected);
    }

    [Fact]
    public void AClaimRecordOptedInToSnowflakeInAPrefixedFeature_WhenMinted_ShouldHoldTheTimeSinceItsEpochAfterTheHead()
    {
        // Given
        var files = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["repo/features/one/.spec/README.md"] = new("---\nclaim_prefix: \"ACTIVITY\"\n---\n# Specification: F1\n"),
        });
        var root = files.Path.GetFullPath("repo");
        var manifest = EngineServices.Parser().ParseDocument(
            """
            {
              "schemaVersion": "0.4.0",
              "ids": {
                "claim": {
                  "form": "{prefix}-{id}",
                  "mint": {
                    "strategy": "snowflake",
                    "snowflake": { "epoch": "2026-01-01", "datacenter": 0, "worker": 0, "sequenceStart": 0 }
                  }
                }
              }
            }
            """);
        var minter = new Minter(
            new FixedRandomSource(0),
            new HeldIds(
                new SpechtDiscovery(files, new UnexpectedChildProcess()),
                new SpechtTreeStore(files),
                new SpechtDocumentParser(new FrontmatterReader()),
                new FeatureTagParser()),
            new FixedClock(FixedClock.CheckTime));

        // When
        var id = minter.Mint(manifest, "claim", root, "features/one");

        // Then
        id.Should().StartWith("ACTIVITY-");
        id["ACTIVITY-".Length..].Aggregate(0L, static (number, character) => (number << 5) | (long) Base32.IndexOf(character, StringComparison.Ordinal))
            .Should().Be((long) (FixedClock.CheckTime - new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalMilliseconds << 22);
    }

    [Theory]
    [ClassData(typeof(RecordsHoldingOnlyAnOptIn))]
    public void ARecordHoldingOnlyItsOptIn_WhenItsTypeIsMinted_ShouldMintAnId(string type, string strategy, string frontmatter, string? feature)
    {
        // Given
        var files = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["repo/features/one/.spec/README.md"] = new($"{frontmatter}# Specification: F1\n"),
        });
        var root = files.Path.GetFullPath("repo");
        var manifest = EngineServices.Parser().ParseDocument(
            $$"""{ "schemaVersion": "0.4.0", "ids": { "{{type}}": { "mint": { "strategy": "{{strategy}}" } } } }""");
        var minter = new Minter(
            new FixedRandomSource(0),
            new HeldIds(
                new SpechtDiscovery(files, new UnexpectedChildProcess()),
                new SpechtTreeStore(files),
                new SpechtDocumentParser(new FrontmatterReader()),
                new FeatureTagParser()),
            new FixedClock(FixedClock.CheckTime));

        // When
        var mint = () => minter.Mint(manifest, type, root, feature);

        // Then
        mint.Should().NotThrow().Which.Should().NotBeEmpty();
    }

    [Theory]
    [ClassData(typeof(RecordsNamingAPrefix))]
    public void ARecordWhoseOptInNamesAPrefix_WhenAnyTypeIsMinted_ShouldBeRefusedNamingThatPrefixWithNoIdDrawn(
        string text, string type, string? feature, string key)
    {
        // Given
        var manifest = EngineServices.Parser().ParseDocument(text);
        var random = new FixedRandomSource(0);
        var minter = new Minter(
            random,
            new HeldIds(
                new SpechtDiscovery(new MockFileSystem(), new UnexpectedChildProcess()),
                new SpechtTreeStore(new MockFileSystem()),
                new SpechtDocumentParser(new FrontmatterReader()),
                new FeatureTagParser()),
            new FixedClock(FixedClock.CheckTime));

        // When
        var mint = () => minter.Mint(manifest, type, ".", feature);

        // Then
        mint.Should().ThrowExactly<IdRefusedException>().WithMessage($"*'{key}'*");
        random.Bounds.Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(RecordOptInsRefused))]
    public void AnAdrRecordsOptInThatMintsNoId_WhenMinted_ShouldBeRefusedNamingTheSettingUnderItsRecord(string mint, string key)
    {
        // Given
        var manifest = EngineServices.Parser().ParseDocument($$"""{ "schemaVersion": "0.4.0", "ids": { "adr": { "mint": {{mint}} } } }""");
        var minter = new Minter(
            new FixedRandomSource(0),
            new HeldIds(
                new SpechtDiscovery(new MockFileSystem(), new UnexpectedChildProcess()),
                new SpechtTreeStore(new MockFileSystem()),
                new SpechtDocumentParser(new FrontmatterReader()),
                new FeatureTagParser()),
            new FixedClock(FixedClock.CheckTime));

        // When
        var minting = () => minter.Mint(manifest, "adr", ".", null);

        // Then
        minting.Should().Throw<IdRefusedException>().WithMessage($"*'{key}'*");
    }

    [Fact]
    public void AnAdrRecordWhoseEveryCandidateTheTreeHolds_WhenMinted_ShouldBeRefusedNamingTheAttemptsUnderItsRecord()
    {
        // Given
        var files = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["repo/.spec/adr/aa-held.md"] = new("# Held\n"),
        });
        var manifest = EngineServices.Parser().ParseDocument(
            """
            {
              "schemaVersion": "0.4.0",
              "ids": { "adr": { "id": "a+", "mint": { "strategy": "nanoid", "nanoid": { "alphabet": "a", "size": 2 }, "attempts": 1 } } }
            }
            """);
        var minter = new Minter(
            new FixedRandomSource(0),
            new HeldIds(
                new SpechtDiscovery(files, new UnexpectedChildProcess()),
                new SpechtTreeStore(files),
                new SpechtDocumentParser(new FrontmatterReader()),
                new FeatureTagParser()),
            new FixedClock(FixedClock.CheckTime));

        // When
        var mint = () => minter.Mint(manifest, "adr", files.Path.GetFullPath("repo"), null);

        // Then
        mint.Should().Throw<IdRefusedException>().WithMessage("*'ids.adr.mint.attempts'*");
    }

    private const string Base32 = "0123456789abcdefghjkmnpqrstvwxyz";
}
