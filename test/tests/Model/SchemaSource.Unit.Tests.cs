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
            ["repo/" + SpecTree.ManifestPath] = new("""{ "frontmatterSchemas": { "feature": "local.json" } }"""),
            ["repo/" + SpecTree.FrontmatterSchemaFolder + "/local.json"] = new("\uFEFF{ \"required\": [\"local_key\"] }"),
            ["repo/" + SpecTree.ItemSchemaPath] = new("{}"),
        });

        // When
        var schemas = EngineServices.Schemas(files).LoadOnDisk("repo");

        // Then
        schemas.Source.Should().Be(SpecSchemaSource.Disk);
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
            ["repo/" + SpecTree.ManifestPath] = new("""{ "identifiers": { "epic": "^[0-9]{4}$" } }"""),
            ["repo/" + SpecTree.FeatureSchemaPath] = new("{}"),
            ["repo/" + SpecTree.ItemSchemaPath] = new("{}"),
            ["repo/" + SpecTree.EpicSchemaPath] = new("{}"),
        });
        var relative = SpecTree.FrontmatterSchemaFolder + "/" + kind + ".schema.json";
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
        var load = () => EngineServices.Schemas(files).LoadOnDisk(root);

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
            ["repo/" + SpecTree.ManifestPath] = new("""{ "identifiers": { "epic": "^[0-9]{4}$" } }"""),
        });
        if (first == "task")
        {
            files.AddFile("repo/" + SpecTree.FeatureSchemaPath, new MockFileData("{}"));
        }
        var loader = EngineServices.Schemas(files);

        // When
        var load = () => loader.Load("repo", SchemaSourceSelection.Disk);

        // Then
        load.Should().Throw<SpechtManifestException>().Which.Message
            .Should().Contain(SpecTree.FrontmatterSchemaFolder + "/" + first + ".schema.json")
            .And.NotContain(SpecTree.EpicSchemaPath);
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
    [InlineData("disk", SchemaSourceSelection.Embedded, SpecSchemaSource.Embedded, true)]
    [InlineData("embedded", SchemaSourceSelection.Disk, SpecSchemaSource.Disk, false)]
    [InlineData("disk", null, SpecSchemaSource.Disk, false)]
    [InlineData(null, null, SpecSchemaSource.Embedded, true)]
    public void OpposingSelections_WhenLoaded_ShouldChooseTheRunSelectionBeforeTheManifest(
        string? configured, SchemaSourceSelection? selected, SpecSchemaSource expected, bool valid)
    {
        // Given
        var manifest = "{ \"schemaVersion\": \"0.2.0\""
            + (configured is null ? " }" : ", \"schemaSource\": \"" + configured + "\" }");
        var files = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            ["repo/" + SpecTree.ManifestPath] = new(manifest),
            ["repo/" + SpecTree.FeatureSchemaPath] = new("{ \"required\": [\"local_key\"] }"),
            ["repo/" + SpecTree.ItemSchemaPath] = new("{}"),
        });
        var version = EngineServices.Versions().Find(new SemanticVersion(0, 2, 0))! with { FeatureSchema = "{}" };
        var loader = EngineServices.Schemas(files, new SchemaVersions([version]));

        // When
        var schemas = loader.Load("repo", selected);

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
            ["repo/" + SpecTree.ManifestPath] = new("{}"),
            ["repo/" + SpecTree.FeatureSchemaPath] = new("{}"),
            ["repo/" + SpecTree.ItemSchemaPath] = new("{}"),
        });
        var loader = EngineServices.Schemas(files);

        // When
        var disk = loader.Load("repo", SchemaSourceSelection.Disk);
        var embedded = loader.Load("repo", SchemaSourceSelection.Embedded);
        var automatic = loader.Load("repo");

        // Then
        disk.Source.Should().Be(SpecSchemaSource.Disk);
        embedded.Source.Should().Be(SpecSchemaSource.Embedded);
        automatic.Source.Should().Be(SpecSchemaSource.Embedded);
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
            ["repo/" + SpecTree.ManifestPath] = new(manifest),
            ["repo/" + SpecTree.FeatureSchemaPath] = new(changed ? "{}\n" : "{}"),
            ["repo/" + SpecTree.ItemSchemaPath] = new("{}"),
        });
        var loader = EngineServices.Schemas(files);

        // When
        var load = () => loader.Load("repo", selected);

        // Then
        if (valid)
        {
            load().Source.Should().Be(SpecSchemaSource.Upstream);
        }
        else
        {
            load.Should().Throw<SpechtManifestException>().Which.Message.Should().Contain(changed ? "SHA-256" : "embedded");
        }
    }
}
