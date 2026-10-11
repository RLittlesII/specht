using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Json.Schema;
using Specht.Manifest;
using Specht.Model;
using Specht.Tests.Shared;
using Specht.Versioning;

namespace Specht.Tests.Versioning;

/// <summary>
/// The schema version set (<c>0001-F7</c> B-003, B-004, B-014, B-022, B-023, B-055; C-6): the embedded set names its
/// versions as <c>major.minor.patch</c> and holds <c>0.1.0</c>, a set given its versions out of order holds them ascending
/// by major, then minor, then patch, each frontmatter schema sits in its own slot, version 0.1.0's epic schema accepts a
/// non-empty title and description and rejects an empty one, version 0.1.0's vocabulary is the twenty-one rule ids, a pin
/// selects exactly the version it names although a later patch and a later minor are held, and selecting a version the set
/// does not hold is a rejected manifest naming the pin and the set.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SchemaVersionsUnitTests
{
    /// <summary>Gets each frontmatter schema's file name and the slot of a version that carries it.</summary>
    public static TheoryData<string, Func<SchemaVersion, string>> Slots =>
        new()
        {
            { "feature-spec.frontmatter.schema.json", static version => version.FeatureSchema },
            { "task.frontmatter.schema.json", static version => version.ItemSchema },
            { "epic.frontmatter.schema.json", static version => version.EpicSchema },
        };

    /// <summary>Gets an epic's title and description, and the frontmatter locations version 0.1.0 rejects (B-022, B-023).</summary>
    public static TheoryData<string, string, string, string[]> EpicTitles =>
        new()
        {
            { "a non-empty title and description are accepted", "A synthetic epic", "An epic built for the check", [] },
            { "an empty title is rejected", string.Empty, "An epic built for the check", ["/title"] },
            { "an empty description is rejected", "A synthetic epic", string.Empty, ["/description"] },
        };

    [Fact]
    public void TheEmbeddedSet_WhenEnumerated_ShouldHoldVersionZeroOneZeroAndAscendByPrecedence()
    {
        // Given
        var embedded = SchemaVersions.Embedded;

        // When
        var numbers = embedded.Versions.Select(static version => version.Number).ToList();

        // Then
        numbers.Should().Contain(new SemanticVersion(0, 1, 0)).And.BeInAscendingOrder().And.OnlyHaveUniqueItems();
    }

    [Fact]
    public void VersionsGivenOutOfOrder_WhenEnumerated_ShouldAscendByMajorThenMinorThenPatch()
    {
        // Given
        var versions = new SchemaVersions(
        [
            new SchemaVersionFixture().WithNumber(new SemanticVersion(1, 0, 0)),
            new SchemaVersionFixture().WithNumber(new SemanticVersion(0, 10, 0)),
            new SchemaVersionFixture().WithNumber(new SemanticVersion(0, 9, 1)),
            new SchemaVersionFixture().WithNumber(new SemanticVersion(0, 9, 0)),
        ]);

        // When
        var numbers = versions.Versions.Select(static version => version.Number);

        // Then
        numbers.Should().Equal(
            new SemanticVersion(0, 9, 0),
            new SemanticVersion(0, 9, 1),
            new SemanticVersion(0, 10, 0),
            new SemanticVersion(1, 0, 0));
    }

    [Theory]
    [MemberData(nameof(Slots))]
    public void EachEmbeddedVersion_WhenItsSlotIsRead_ShouldHoldThatFilesSchemaForThatVersion(string file, Func<SchemaVersion, string> slot)
    {
        // Given
        var versions = SchemaVersions.Embedded.Versions;

        // When
        var ids = versions
            .Select(version => (Folder: EmbeddedFolder.Of(version), Id: JsonNode.Parse(slot(version))!["$id"]!.GetValue<string>()))
            .ToList();

        // Then
        ids.Should().NotBeEmpty().And.AllSatisfy(pair =>
            pair.Id.Should().Be($"https://github.com/rlittlesii/specht/schema/v{pair.Folder}/{file}"));
    }

    [Fact]
    public void EmbeddedVersionZeroOneZero_WhenItsVocabularyIsRead_ShouldNameExactlyItsTwentyOneRuleIds()
    {
        // Given
        var embedded = SchemaVersions.Embedded;

        // When
        var ids = embedded.Select(new SemanticVersion(0, 1, 0)).RuleIds;

        // Then
        ids.Should().BeEquivalentTo(
            "SPEC001", "SPEC002", "SPEC003", "SPEC004", "SPEC010", "SPEC011", "SPEC012", "SPEC013", "SPEC020", "SPEC021", "SPEC030",
            "SPEC031", "SPEC040", "SPEC041", "SPEC043", "SPEC044", "SPEC050", "SPEC051", "SPEC052", "SPEC060", "SPEC061");
    }

    [Theory]
    [MemberData(nameof(EpicTitles))]
    public void EmbeddedVersionZeroOneZerosEpicSchema_WhenAnEpicCarriesATitleAndADescription_ShouldRejectOnlyAnEmptyOne(
        string because,
        string title,
        string description,
        string[] expected)
    {
        // Given
        var schema = JsonSchema.FromText(
            SchemaVersions.Embedded.Select(new SemanticVersion(0, 1, 0)).EpicSchema,
            new BuildOptions { SchemaRegistry = new SchemaRegistry() });
        var epic = new JsonObject
        {
            ["id"] = "0001",
            ["type"] = "epic",
            ["status"] = "ready",
            ["priority"] = "med",
            ["milestone"] = null,
            ["children"] = new JsonArray(),
            ["created"] = "2026-10-07",
            ["updated"] = "2026-10-07",
            ["github_issue"] = null,
            ["title"] = title,
            ["description"] = description,
        };

        // When
        using var instance = JsonDocument.Parse(epic.ToJsonString());
        var result = schema.Evaluate(instance.RootElement, SpecSchemas.Options);

        // Then
        (result.Details ?? [])
            .Where(static detail => !detail.IsValid && detail.Errors is { Count: > 0 })
            .Select(static detail => detail.InstanceLocation.ToString())
            .Where(static location => location.Length > 0)
            .Distinct()
            .Should()
            .BeEquivalentTo(expected, because);
        result.IsValid.Should().Be(expected.Length == 0, because);
    }

    [Fact]
    public void AVersionHeldBesideALaterPatchAndALaterMinor_WhenSelected_ShouldBeExactlyTheVersionNamed()
    {
        // Given
        SchemaVersion pinned = new SchemaVersionFixture();
        SchemaVersion laterPatch = new SchemaVersionFixture().WithNumber(new SemanticVersion(0, 1, 1));
        SchemaVersion laterMinor = new SchemaVersionFixture().WithNumber(new SemanticVersion(0, 2, 0));
        var versions = new SchemaVersions([laterMinor, laterPatch, pinned]);

        // When
        var selected = versions.Select(new SemanticVersion(0, 1, 0));

        // Then
        selected.Should().BeSameAs(pinned);
    }

    [Fact]
    public void AVersionTheSetDoesNotHold_WhenSelected_ShouldRejectTheManifestNamingThePinnedVersionAndEveryShippedVersion()
    {
        // Given
        var versions = new SchemaVersions([new SchemaVersionFixture(), new SchemaVersionFixture().WithNumber(new SemanticVersion(0, 2, 0))]);

        // When
        var select = () => versions.Select(new SemanticVersion(7, 0, 0));

        // Then
        var message = select.Should().ThrowExactly<SpechtManifestException>().Which.Message;
        foreach (var named in new[] { "7.0.0", "0.1.0", "0.2.0" })
        {
            message.Should().MatchRegex($@"(?<![\w.]){Regex.Escape(named)}(?!\.?\w)", "the message names version {0}", named);
        }
    }
}
