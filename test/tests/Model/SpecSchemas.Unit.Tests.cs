using System.IO.Abstractions.TestingHelpers;
using System.Text.Json;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Json.Schema;
using Specht.Manifest;
using Specht.Model;
using Specht.Tests.Shared;
using Specht.Versioning;

namespace Specht.Tests.Model;

/// <summary>
/// The schema loads over an in-memory file system. The on-disk load reads each frontmatter schema by the file name the
/// manifest gives its kind (0001-F5 B-008, decision 0003). Both loads keep the version they validate with (ADR-0008, item
/// 0105; 0001-F1 § 7): the version-set load the version the manifest's pin selects, and the on-disk load the embedded
/// version the manifest pins with the schema texts it read, rejecting a pin the tool does not ship before any frontmatter
/// schema is read. A rule setting is held to the vocabulary of the version the pin selects (0001-F5 B-013, item 0014).
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpecSchemasUnitTests
{
    /// <summary>Gets each kind, its default file name, the file name a manifest gives it, and the schema the load reads for it.</summary>
    public static TheoryData<string, string, string, Func<SpecSchemas, JsonSchema>> NamedSchemas { get; } = new()
    {
        { "feature", "feature-spec.frontmatter.schema.json", "feature.json", static schemas => schemas.Feature },
        { "task", "task.frontmatter.schema.json", "item.json", static schemas => schemas.Item },
        { "epic", "epic.frontmatter.schema.json", "epic.json", static schemas => schemas.Epic },
    };

    [Theory]
    [MemberData(nameof(NamedSchemas))]
    public void AManifestNamingAKindsSchemaFile_WhenLoaded_ShouldReadThatKindsSchemaFromThatFile(
        string kind,
        string defaultName,
        string name,
        Func<SpecSchemas, JsonSchema> schemaOf)
    {
        // Given
        var schema = Path.Combine(Root, ".spec", "schema");
        var files = new Dictionary<string, MockFileData>
        {
            [Path.Combine(schema, "spec-structure.schema.json")] = new($$"""{ "frontmatterSchemas": { "{{kind}}": "{{name}}" } }"""),
            [Path.Combine(schema, "feature-spec.frontmatter.schema.json")] = new("{}"),
            [Path.Combine(schema, "task.frontmatter.schema.json")] = new("{}"),
            [Path.Combine(schema, "epic.frontmatter.schema.json")] = new("{}"),
        };
        files.Remove(Path.Combine(schema, defaultName));
        files[Path.Combine(schema, name)] = new("""{ "const": "read from the named file" }""");
        var fileSystem = new MockFileSystem(files);

        // When
        var schemas = (SpecSchemas)SpecSchemas.Load(fileSystem, Root).Value!;

        // Then
        schemaOf(schemas).Evaluate(JsonSerializer.SerializeToElement("read from elsewhere")).IsValid.Should().BeFalse();
    }

    [Fact]
    public void AManifestPinningAVersionOfTheSet_WhenLoadedThroughTheSet_ShouldKeepThatVersion()
    {
        // Given
        SchemaVersion one = new SchemaVersionFixture();
        SchemaVersion two = new SchemaVersionFixture()
            .WithNumber(new SemanticVersion(0, 2, 0))
            .WithRuleIds(new HashSet<string>(["FAKE001"], StringComparer.Ordinal));
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                [Path.Combine(Root, ".spec", "schema", "spec-structure.schema.json")] = new("""{ "schemaVersion": "0.2.0" }"""),
            });

        // When
        var schemas = (SpecSchemas)SpecSchemas.Load(fileSystem, Root, new SchemaVersions([one, two])).Value!;

        // Then
        schemas.Version.Should().Be(two);
    }

    [Fact]
    public void AnOnDiskSchemaSet_WhenLoaded_ShouldKeepTheEmbeddedVersionItsManifestPinsWithThatVersionsRuleIds()
    {
        // Given
        var embedded = (SchemaVersion)SchemaVersions.Embedded.Select(new SemanticVersion(0, 1, 0)).Value!;
        var schema = Path.Combine(Root, ".spec", "schema");
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                [Path.Combine(schema, "spec-structure.schema.json")] = new("""{ "schemaVersion": "0.1.0" }"""),
                [Path.Combine(schema, "feature-spec.frontmatter.schema.json")] = new("{}"),
                [Path.Combine(schema, "task.frontmatter.schema.json")] = new("{}"),
                [Path.Combine(schema, "epic.frontmatter.schema.json")] = new("{}"),
            });

        // When
        var schemas = (SpecSchemas)SpecSchemas.Load(fileSystem, Root).Value!;

        // Then
        schemas.Version.Number.Should().Be(new SemanticVersion(0, 1, 0));
        schemas.Version.RuleIds.Should().BeEquivalentTo(embedded.RuleIds);
    }

    [Fact]
    public void AnOnDiskSchemaSet_WhenLoaded_ShouldKeepTheSchemaTextsItReadInPlaceOfTheEmbeddedOnes()
    {
        // Given
        const string feature = """{ "title": "the feature schema on disk" }""";
        const string item = """{ "title": "the item schema on disk" }""";
        const string epic = """{ "title": "the epic schema on disk" }""";
        var schema = Path.Combine(Root, ".spec", "schema");
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                [Path.Combine(schema, "spec-structure.schema.json")] = new("{}"),
                [Path.Combine(schema, "feature-spec.frontmatter.schema.json")] = new(feature),
                [Path.Combine(schema, "task.frontmatter.schema.json")] = new(item),
                [Path.Combine(schema, "epic.frontmatter.schema.json")] = new(epic),
            });

        // When
        var schemas = (SpecSchemas)SpecSchemas.Load(fileSystem, Root).Value!;

        // Then
        (schemas.Version.FeatureSchema, schemas.Version.ItemSchema, schemas.Version.EpicSchema).Should().Be((feature, item, epic));
    }

    [Fact]
    public void AnOnDiskManifestPinningAVersionTheToolDoesNotShip_WhenLoaded_ShouldRejectTheManifestNamingThePinBeforeReadingAFrontmatterSchema()
    {
        // Given
        var newest = SchemaVersions.Embedded.Versions.Max(static version => version.Number);
        var unshipped = new SemanticVersion(newest.Major + 1, 0, 0).ToString();
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                [Path.Combine(Root, ".spec", "schema", "spec-structure.schema.json")] = new($$"""{ "schemaVersion": "{{unshipped}}" }"""),
            });

        // When
        var loaded = SpecSchemas.Load(fileSystem, Root);

        // Then
        loaded.Value.Should().BeOfType<ManifestRejected>().Which.Message.Should().MatchRegex($@"(?<![\w.]){Regex.Escape(unshipped)}(?!\.?\w)");
    }

    [Fact]
    public void AManifestSettingARuleIdOutsideThePinnedVocabulary_WhenLoadedThroughTheSet_ShouldRejectItNamingTheId()
    {
        // Given
        SchemaVersion version = new SchemaVersionFixture().WithRuleIds(new HashSet<string>(["FAKE001"], StringComparer.Ordinal));
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                [Path.Combine(Root, ".spec", "schema", "spec-structure.schema.json")] =
                    new("""{ "rules": { "FAKE001": "warning", "FAKE404": "off" } }"""),
            });

        // When
        var loaded = SpecSchemas.Load(fileSystem, Root, new SchemaVersions([version]));

        // Then
        loaded.Value.Should().BeOfType<ManifestRejected>().Which.Message.Should().Contain("FAKE404");
    }

    [Fact]
    public void AManifestSettingOnlyRuleIdsOfThePinnedVocabulary_WhenLoadedThroughTheSet_ShouldAcceptIt()
    {
        // Given
        SchemaVersion version = new SchemaVersionFixture().WithRuleIds(new HashSet<string>(["FAKE001", "FAKE002"], StringComparer.Ordinal));
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                [Path.Combine(Root, ".spec", "schema", "spec-structure.schema.json")] =
                    new("""{ "rules": { "FAKE001": "warning", "FAKE002": "off" } }"""),
            });

        // When
        var loaded = SpecSchemas.Load(fileSystem, Root, new SchemaVersions([version]));

        // Then
        loaded.Value.Should().BeOfType<SpecSchemas>();
    }

    [Fact]
    public void AnOnDiskManifestSettingARuleIdOutsideThePinnedVocabulary_WhenLoaded_ShouldRejectItNamingTheId()
    {
        // Given
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                [Path.Combine(Root, ".spec", "schema", "spec-structure.schema.json")] =
                    new("""{ "schemaVersion": "0.1.0", "rules": { "SPEC010": "warning", "SPEC999": "off" } }"""),
            });

        // When
        var loaded = SpecSchemas.Load(fileSystem, Root);

        // Then
        loaded.Value.Should().BeOfType<ManifestRejected>().Which.Message.Should().Contain("SPEC999");
    }

    private const string Root = "repo";
}
