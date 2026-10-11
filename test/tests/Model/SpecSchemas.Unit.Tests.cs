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
/// schema is read.
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
        var schemas = SpecSchemas.Load(fileSystem, Root);

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
        var schemas = SpecSchemas.Load(fileSystem, Root, new SchemaVersions([one, two]));

        // Then
        schemas.Version.Should().Be(two);
    }

    [Fact]
    public void AnOnDiskSchemaSet_WhenLoaded_ShouldKeepTheEmbeddedVersionItsManifestPinsWithThatVersionsRuleIds()
    {
        // Given
        var embedded = SchemaVersions.Embedded.Select(new SemanticVersion(0, 1, 0));
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
        var schemas = SpecSchemas.Load(fileSystem, Root);

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
        var schemas = SpecSchemas.Load(fileSystem, Root);

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
        var load = () => SpecSchemas.Load(fileSystem, Root);

        // Then
        load.Should().ThrowExactly<SpechtManifestException>().Which.Message.Should().MatchRegex($@"(?<![\w.]){Regex.Escape(unshipped)}(?!\.?\w)");
    }

    private const string Root = "repo";
}
