using System.Text.Json;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Model;
using Specht.Tests.Shared;
using Specht.Versioning;

namespace Specht.Tests.Model;

/// <summary>
/// The schema compiler, which reads nothing (item 0166; <c>0001-F1</c> B-004): a version's schema text compiled into the
/// three frontmatter schemas with the source it is told; a local set read through the delegate it is handed, by the file
/// name the manifest gives each kind, in Feature, task, epic order, the first file that is not a JSON Schema named by its
/// path under the root (<c>0001-F7</c> B-024, B-077); and evaluation options under which <c>format</c> is an assertion
/// (<c>0001-F5</c> B-008).
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtSchemaCompilerUnitTests
{
    [Fact]
    public void AVersionsSchemaText_WhenCompiled_ShouldBuildEachKindsSchemaFromThatTextWithTheVersionAndTheSourceGiven()
    {
        // Given
        var version = (SchemaVersion) new SchemaVersionFixture() with
        {
            FeatureSchema = """{ "const": "feature" }""",
            ItemSchema = """{ "const": "task" }""",
            EpicSchema = """{ "const": "epic" }""",
        };

        // When
        var schemas = new SpechtSchemaCompiler().Compile(version, EngineServices.Parser().Parse("{}"), SpechtSchemaSource.Upstream);

        // Then
        schemas.Version.Should().Be(version);
        schemas.Source.Should().Be(SpechtSchemaSource.Upstream);
        new[] { (schemas.Feature, "feature"), (schemas.Item, "task"), (schemas.Epic, "epic") }
            .Select(static pair => (
                Own: pair.Item1.Evaluate(JsonSerializer.SerializeToElement(pair.Item2)).IsValid,
                Other: pair.Item1.Evaluate(JsonSerializer.SerializeToElement("another kind")).IsValid))
            .Should().Equal((true, false), (true, false), (true, false));
    }

    [Fact]
    public void CompiledSchemas_WhenAValueBreakingTheirFormatIsEvaluatedWithTheirOptions_ShouldBeInvalid()
    {
        // Given
        var version = (SchemaVersion) new SchemaVersionFixture() with { FeatureSchema = """{ "format": "date" }""" };
        var schemas = new SpechtSchemaCompiler().Compile(version, EngineServices.Parser().Parse("{}"), SpechtSchemaSource.Embedded);

        // When
        var result = schemas.Feature.Evaluate(JsonSerializer.SerializeToElement("not a date"), schemas.EvaluationOptions);

        // Then
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ALocalSetWhoseTaskSchemaIsNotJsonSchema_WhenCompiled_ShouldNameThatFileAndReadNoFileAfterIt()
    {
        // Given
        var structure = EngineServices.Parser().Parse("""{ "identifiers": { "epic": "^[0-9]{4}$" } }""");
        var read = new List<string>();

        // When
        var compiling = () => new SpechtSchemaCompiler().CompileDisk(new SchemaVersionFixture(), structure, name =>
        {
            read.Add(name);
            return name == "task.schema.json" ? "not JSON" : "{}";
        });

        // Then
        compiling.Should().ThrowExactly<SpechtManifestException>()
            .Which.Message.Should().StartWith(".specht/schema/frontmatter/task.schema.json:");
        read.Should().Equal("feature.schema.json", "task.schema.json");
    }

    [Fact]
    public void ALocalSetUnderAManifestDeclaringNoEpicGrammar_WhenCompiled_ShouldKeepTheReadTextsAndTheVersionsEpicSchemaUnread()
    {
        // Given
        var version = (SchemaVersion) new SchemaVersionFixture() with { EpicSchema = """{ "const": "the version's epic" }""" };
        var read = new List<string>();

        // When
        var schemas = new SpechtSchemaCompiler().CompileDisk(version, EngineServices.Parser().Parse("{}"), name =>
        {
            read.Add(name);
            return $$"""{ "title": "{{name}}" }""";
        });

        // Then
        read.Should().Equal("feature.schema.json", "task.schema.json");
        schemas.Source.Should().Be(SpechtSchemaSource.Disk);
        (schemas.Version.FeatureSchema, schemas.Version.ItemSchema, schemas.Version.EpicSchema).Should().Be(
            ("""{ "title": "feature.schema.json" }""", """{ "title": "task.schema.json" }""", version.EpicSchema));
        schemas.Epic.Evaluate(JsonSerializer.SerializeToElement("the version's epic")).IsValid.Should().BeTrue();
        schemas.Epic.Evaluate(JsonSerializer.SerializeToElement("anything else")).IsValid.Should().BeFalse();
    }
}
