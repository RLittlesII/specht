using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Json.Schema;

namespace specht.tests;

/// <summary>
/// The schema version set (<c>0001-F7</c> B-003, B-004, B-014, B-022, B-023; C-6): the embedded set holds every version
/// from 1 to the newest, each frontmatter schema sits in its own slot, version 1's epic schema accepts a non-empty title
/// and description and rejects an empty one, version 1's vocabulary is the twenty-one rule ids, and selecting a version the
/// set does not hold is a rejected manifest naming the pin and the set.
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

    /// <summary>Gets an epic's title and description, and the frontmatter locations version 1 rejects (B-022, B-023).</summary>
    public static TheoryData<string, string, string, string[]> EpicTitles =>
        new()
        {
            { "a non-empty title and description are accepted", "A synthetic epic", "An epic built for the check", [] },
            { "an empty title is rejected", string.Empty, "An epic built for the check", ["/title"] },
            { "an empty description is rejected", "A synthetic epic", string.Empty, ["/description"] },
        };

    [Fact]
    public void TheEmbeddedSet_WhenEnumerated_ShouldHoldEveryVersionFromOneToTheNewest()
    {
        // Given
        var embedded = SchemaVersions.Embedded;

        // When
        var numbers = embedded.Versions.Select(static version => version.Number).ToList();

        // Then
        numbers.Should().NotBeEmpty();
        numbers.Should().Equal(Enumerable.Range(1, numbers.Max()));
    }

    [Theory]
    [MemberData(nameof(Slots))]
    public void EachEmbeddedVersion_WhenItsSlotIsRead_ShouldHoldThatFilesSchemaForThatVersion(string file, Func<SchemaVersion, string> slot)
    {
        // Given
        var versions = SchemaVersions.Embedded.Versions;

        // When
        var ids = versions.Select(version => (version.Number, Id: JsonNode.Parse(slot(version))!["$id"]!.GetValue<string>())).ToList();

        // Then
        ids.Should().NotBeEmpty().And.AllSatisfy(pair =>
            pair.Id.Should().Be($"https://github.com/rlittlesii/specht/schema/v{pair.Number}/{file}"));
    }

    [Fact]
    public void EmbeddedVersion1_WhenItsVocabularyIsRead_ShouldNameExactlyTheTwentyOneVersion1RuleIds()
    {
        // Given
        var embedded = SchemaVersions.Embedded;

        // When
        var ids = embedded.Select(1).RuleIds;

        // Then
        ids.Should().BeEquivalentTo(
            "SPEC001", "SPEC002", "SPEC003", "SPEC004", "SPEC010", "SPEC011", "SPEC012", "SPEC013", "SPEC020", "SPEC021", "SPEC030",
            "SPEC031", "SPEC040", "SPEC041", "SPEC043", "SPEC044", "SPEC050", "SPEC051", "SPEC052", "SPEC060", "SPEC061");
    }

    [Theory]
    [MemberData(nameof(EpicTitles))]
    public void EmbeddedVersion1sEpicSchema_WhenAnEpicCarriesATitleAndADescription_ShouldRejectOnlyAnEmptyOne(
        string because,
        string title,
        string description,
        string[] expected)
    {
        // Given
        var schema = JsonSchema.FromText(SchemaVersions.Embedded.Select(1).EpicSchema, new BuildOptions { SchemaRegistry = new SchemaRegistry() });
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
    public void AShippedVersion_WhenSelected_ShouldBeThatVersion()
    {
        // Given
        SchemaVersion one = new SchemaVersionFixture();
        SchemaVersion two = new SchemaVersionFixture().WithNumber(2);
        var versions = new SchemaVersions([one, two]);

        // When
        var selected = versions.Select(2);

        // Then
        selected.Should().BeSameAs(two);
    }

    [Fact]
    public void AVersionTheSetDoesNotHold_WhenSelected_ShouldRejectTheManifestNamingThePinnedVersionAndEveryShippedVersion()
    {
        // Given
        var versions = new SchemaVersions([new SchemaVersionFixture(), new SchemaVersionFixture().WithNumber(2)]);

        // When
        var select = () => versions.Select(9);

        // Then
        var message = select.Should().ThrowExactly<SpechtManifestException>().Which.Message;
        foreach (var named in new[] { 9, 1, 2 })
        {
            message.Should().MatchRegex($@"\b{named}\b", "the message names version {0}", named);
        }
    }
}
