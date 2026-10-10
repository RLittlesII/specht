using System.IO.Abstractions.TestingHelpers;
using System.Text.Json;
using AwesomeAssertions;
using Json.Schema;

namespace specht.tests;

/// <summary>
/// The on-disk schema load over an in-memory file system (0001-F5 B-008, decision 0003): each frontmatter schema is read
/// by the file name the manifest gives its kind.
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

    private const string Root = "repo";
}
