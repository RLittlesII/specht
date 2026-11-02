using System.IO.Abstractions.TestingHelpers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Model;
using Specht.Tests.Shared;
using Specht.Versioning;

namespace Specht.Tests.Model;

/// <summary>Explicit local loading, the versioned selection key and controlled schema failures (0001-F7 B-009, B-102, B-104 to B-107).</summary>
[Trait("Tier", "Unit")]
public sealed class SchemaSourceUnitTests
{
    [Fact]
    public void DiskSchemasWithoutAnEpicDeclaration_WhenLoaded_ShouldUseNamedFilesAndReportDisk()
    {
        // Given
        var files = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["repo/" + SpechtTree.ManifestPath] = new("""{ "frontmatterSchemas": { "feature": "local.json" } }"""),
            ["repo/" + SpechtTree.FrontmatterSchemaFolder + "/local.json"] = new("\uFEFF{ \"required\": [\"local_key\"] }"),
            ["repo/" + SpechtTree.ItemSchemaPath] = new("{}"),
        });

        // When
        var schemas = EngineServices.Reader(files).ReadSchemas("repo", SchemaSourceSelection.Disk);

        // Then
        schemas.Source.Should().Be(SpechtSchemaSource.Disk);
        schemas.Feature.Evaluate(JsonSerializer.SerializeToElement(new { })).IsValid.Should().BeFalse();
        schemas.Version.Number.Should().Be(new SemanticVersion(0, 1, 0));
    }

    [Theory]
    [InlineData("feature", null)]
    [InlineData("feature", "{")]
    [InlineData("feature", "{ \"type\": 42 }")]
    [InlineData("task", null)]
    [InlineData("task", "{")]
    [InlineData("task", "{ \"type\": 42 }")]
    [InlineData("epic", null)]
    [InlineData("epic", "{")]
    [InlineData("epic", "{ \"type\": 42 }")]
    public void AFaultyNamedDiskSchema_WhenLoaded_ShouldRejectItWithARelativeFileDiagnostic(string kind, string? text)
    {
        // Given
        var files = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["repo/" + SpechtTree.ManifestPath] = new("""{ "identifiers": { "epic": "^[0-9]{4}$" } }"""),
            ["repo/" + SpechtTree.FeatureSchemaPath] = new("{}"),
            ["repo/" + SpechtTree.ItemSchemaPath] = new("{}"),
            ["repo/" + SpechtTree.EpicSchemaPath] = new("{}"),
        });
        var relative = SpechtTree.FrontmatterSchemaFolder + "/" + kind + ".schema.json";
        var path = "repo/" + relative;
        if (text is null)
        {
            files.RemoveFile(path);
        }
        else
        {
            files.AddFile(path, new MockFileData(text));
        }
        var root = files.Path.GetFullPath("repo");

        // When
        var load = () => EngineServices.Reader(files).ReadSchemas(root, SchemaSourceSelection.Disk);

        // Then
        load.Should().Throw<SpechtManifestException>().Which.Message
            .Should().Contain(relative).And.NotContain(root);
    }

    [Theory]
    [InlineData("feature")]
    [InlineData("task")]
    public void SeveralRequiredSchemasMissing_WhenLoaded_ShouldNameTheFirstKindInFeatureTaskEpicOrder(string first)
    {
        // Given
        var files = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["repo/" + SpechtTree.ManifestPath] = new("""{ "identifiers": { "epic": "^[0-9]{4}$" } }"""),
        });
        if (first == "task")
        {
            files.AddFile("repo/" + SpechtTree.FeatureSchemaPath, new MockFileData("{}"));
        }
        var reader = EngineServices.Reader(files);

        // When
        var load = () => reader.ReadSchemas("repo", SchemaSourceSelection.Disk);

        // Then
        load.Should().Throw<SpechtManifestException>().Which.Message
            .Should().Contain(SpechtTree.FrontmatterSchemaFolder + "/" + first + ".schema.json")
            .And.NotContain(SpechtTree.EpicSchemaPath);
    }

    [Theory]
    [InlineData("0.2.0", "disk", true)]
    [InlineData("0.2.0", "embedded", true)]
    [InlineData("0.2.0", "upstream", false)]
    [InlineData("0.2.0", "Disk", false)]
    [InlineData("0.1.0", "disk", false)]
    public void AManifestSourceSelection_WhenParsed_ShouldRespectItsVersionAndAllowedValues(string version, string source, bool valid)
    {
        // Given
        var manifest = $$"""{ "schemaVersion": "{{version}}", "schemaSource": "{{source}}" }""";

        // When
        var parse = () => EngineServices.Parser().Parse(manifest);

        // Then
        if (valid)
        {
            parse.Should().NotThrow();
        }
        else
        {
            parse.Should().Throw<SpechtManifestException>().Which.Message.Should().Contain("schemaSource");
        }
    }

    [Theory]
    [InlineData("null")]
    [InlineData("42")]
    [InlineData("true")]
    [InlineData("[]")]
    [InlineData("{}")]
    public void ANonTextSourceSelection_WhenParsed_ShouldRejectTheNamedKey(string value)
    {
        // Given
        var manifest = "{ \"schemaVersion\": \"0.2.0\", \"schemaSource\": " + value + " }";

        // When
        var parse = () => EngineServices.Parser().Parse(manifest);

        // Then
        parse.Should().Throw<SpechtManifestException>().Which.Message.Should().Contain("schemaSource");
    }

    [Theory]
    [InlineData("disk", SchemaSourceSelection.Embedded, SpechtSchemaSource.Embedded, true)]
    [InlineData("embedded", SchemaSourceSelection.Disk, SpechtSchemaSource.Disk, false)]
    [InlineData("disk", null, SpechtSchemaSource.Disk, false)]
    [InlineData(null, null, SpechtSchemaSource.Embedded, true)]
    public void OpposingSelections_WhenLoaded_ShouldChooseTheRunSelectionBeforeTheManifest(
        string? configured, SchemaSourceSelection? selected, SpechtSchemaSource expected, bool valid)
    {
        // Given
        var manifest = "{ \"schemaVersion\": \"0.2.0\""
            + (configured is null ? " }" : ", \"schemaSource\": \"" + configured + "\" }");
        var files = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["repo/" + SpechtTree.ManifestPath] = new(manifest),
            ["repo/" + SpechtTree.FeatureSchemaPath] = new("{ \"required\": [\"local_key\"] }"),
            ["repo/" + SpechtTree.ItemSchemaPath] = new("{}"),
        });
        var version = EngineServices.Versions().Find(new SemanticVersion(0, 2, 0))! with { FeatureSchema = "{}" };
        var reader = EngineServices.Reader(files, new SchemaVersions([version]));

        // When
        var schemas = reader.ReadSchemas("repo", selected);

        // Then
        schemas.Source.Should().Be(expected);
        var frontmatter = JsonSerializer.SerializeToElement(new { });
        schemas.Feature.Evaluate(frontmatter).IsValid.Should().Be(valid);
        schemas.Version.Number.Should().Be(new SemanticVersion(0, 2, 0));
    }

    [Fact]
    public void ConsecutiveOpposingRunSelections_WhenLoaded_ShouldRemainIndependent()
    {
        // Given
        var files = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["repo/" + SpechtTree.ManifestPath] = new("{}"),
            ["repo/" + SpechtTree.FeatureSchemaPath] = new("{}"),
            ["repo/" + SpechtTree.ItemSchemaPath] = new("{}"),
        });
        var reader = EngineServices.Reader(files);

        // When
        var disk = reader.ReadSchemas("repo", SchemaSourceSelection.Disk);
        var embedded = reader.ReadSchemas("repo", SchemaSourceSelection.Embedded);
        var automatic = reader.ReadSchemas("repo", null);

        // Then
        disk.Source.Should().Be(SpechtSchemaSource.Disk);
        embedded.Source.Should().Be(SpechtSchemaSource.Embedded);
        automatic.Source.Should().Be(SpechtSchemaSource.Embedded);
    }

    [Theory]
    [InlineData(null, SchemaSourceSelection.Disk, false, true)]
    [InlineData(null, SchemaSourceSelection.Disk, true, false)]
    [InlineData(null, SchemaSourceSelection.Embedded, false, false)]
    [InlineData("embedded", SchemaSourceSelection.Disk, false, false)]
    public void ARecordedSource_WhenSelectionIsResolved_ShouldRetainVerificationAndRejectContradictions(
        string? configured, SchemaSourceSelection selected, bool changed, bool valid)
    {
        // Given
        var hash = "sha256:" + Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes("{}")));
        var selection = configured is null ? string.Empty : ", \"schemaSource\": \"" + configured + "\"";
        var manifest = $$"""
            { "schemaVersion": "0.2.0", "spechtSchema": {
              "location": "https://schemas.example/specht/", "version": "v1",
              "hashes": { "feature": "{{hash}}", "task": "{{hash}}" }
            }{{selection}} }
            """;
        var files = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["repo/" + SpechtTree.ManifestPath] = new(manifest),
            ["repo/" + SpechtTree.FeatureSchemaPath] = new(changed ? "{}\n" : "{}"),
            ["repo/" + SpechtTree.ItemSchemaPath] = new("{}"),
        });
        var reader = EngineServices.Reader(files);

        // When
        var load = () => reader.ReadSchemas("repo", selected);

        // Then
        if (valid)
        {
            load().Source.Should().Be(SpechtSchemaSource.Upstream);
        }
        else
        {
            load.Should().Throw<SpechtManifestException>().Which.Message.Should().Contain(changed ? "SHA-256" : "embedded");
        }
    }
}
