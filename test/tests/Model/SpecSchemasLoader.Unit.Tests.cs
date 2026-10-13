using System.IO.Abstractions.TestingHelpers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Json.Schema;
using Specht.Manifest;
using Specht.Model;
using Specht.Report;
using Specht.Tests.Shared;
using Specht.Versioning;

namespace Specht.Tests.Model;

/// <summary>
/// The manifest stage as an instance over an injected file system (ADR-0005 (a); ADR-0001 stage D, item 0107): the
/// structure is the one the manifest in that file system declares, and the version kept is the one its pin selects from
/// the set handed in. Under a manifest that records an upstream schema source (<c>0001-F7</c>, item 0092) the schema of
/// each fetched kind is built from the file under the root's frontmatter schema folder (B-024), the schemas say they came
/// from the upstream copy (B-032), the version kept is still the pinned one with its rule ids (B-028), a kind that is not
/// fetched keeps the pinned version's schema whatever the root holds for it (B-075), and a fetched file that is absent
/// or does not hash to what the manifest records rejects the manifest, naming the file by its path under the root (B-077,
/// B-078, B-080, B-081). Under a manifest that records none the schemas are the pinned version's and no file under the
/// root is read for them. Every file is bytes in a <see cref="MockFileSystem"/>; nothing touches the disk and nothing is
/// fetched.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpecSchemasLoaderUnitTests
{
    /// <summary>Gets each kind a manifest declaring the epic grammar fetches, and the schema the load builds for it.</summary>
    public static TheoryData<string, Func<SpecSchemas, JsonSchema>> FetchedKinds { get; } = new()
    {
        { "feature", static schemas => schemas.Feature },
        { "task", static schemas => schemas.Item },
        { "epic", static schemas => schemas.Epic },
    };

    /// <summary>Gets what the root holds at the epic schema's path while the epic kind is not fetched.</summary>
    public static TheoryData<string, byte[]?> UnfetchedEpicFiles =>
        new()
        {
            { "no file", null },
            { "a file that is not the pinned version's epic schema", Local },
        };

    /// <summary>Gets what the root holds at the item schema's path in place of the bytes whose hash the manifest records.</summary>
    public static TheoryData<string, byte[]?> FaultyItemFiles =>
        new()
        {
            { "no file (B-077, B-078)", null },
            { "a file edited after its hash was recorded (B-080, B-081)", [.. Local, (byte)'\n'] },
        };

    [Fact]
    public void AManifestInTheInjectedFileSystem_WhenLoaded_ShouldCarryTheStructureThatManifestDeclares()
    {
        // Given
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                [Path.Combine(Root, SpecTree.ManifestPath)] = new("""{ "exclusions": ["vendor"] }"""),
                [Path.Combine(Root, SpecTree.FeatureSchemaPath)] = new("{}"),
                [Path.Combine(Root, SpecTree.ItemSchemaPath)] = new("{}"),
                [Path.Combine(Root, SpecTree.EpicSchemaPath)] = new("{}"),
            });
        var loader = new SpecSchemasLoader(fileSystem, new FrontmatterSchemaStore(fileSystem));

        // When
        var schemas = loader.Load(Root);

        // Then
        schemas.Structure.Discovery.Exclusions.Should().Equal("vendor");
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
                [Path.Combine(Root, SpecTree.ManifestPath)] = new("""{ "schemaVersion": "0.2.0" }"""),
            });
        var loader = new SpecSchemasLoader(fileSystem, new FrontmatterSchemaStore(fileSystem));

        // When
        var schemas = loader.Load(Root, new SchemaVersions([one, two]));

        // Then
        schemas.Version.Should().Be(two);
    }

    [Theory]
    [MemberData(nameof(FetchedKinds))]
    public void AManifestRecordingAnUpstreamSchemaSource_WhenLoaded_ShouldBuildEachFetchedKindsSchemaFromTheFileUnderTheRoot(
        string kind,
        Func<SpecSchemas, JsonSchema> schemaOf)
    {
        // Given
        SchemaVersion pinned = new SchemaVersionFixture();
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                [Path.Combine(Root, SpecTree.ManifestPath)] = Recording(DeclaringTheEpicGrammar(), ("feature", Local), ("task", Local), ("epic", Local)),
                [Path.Combine(Root, SpecTree.FeatureSchemaPath)] = new(Local),
                [Path.Combine(Root, SpecTree.ItemSchemaPath)] = new(Local),
                [Path.Combine(Root, SpecTree.EpicSchemaPath)] = new(Local),
            });
        var loader = new SpecSchemasLoader(fileSystem, new FrontmatterSchemaStore(fileSystem));

        // When
        var schemas = loader.Load(Root, new SchemaVersions([pinned]));

        // Then
        schemaOf(schemas).Evaluate(JsonSerializer.SerializeToElement("read from the pinned version")).IsValid.Should().BeFalse(kind);
    }

    [Fact]
    public void AManifestRecordingAnUpstreamSchemaSource_WhenLoaded_ShouldSayItsSchemasCameFromTheUpstreamCopy()
    {
        // Given
        SchemaVersion pinned = new SchemaVersionFixture();
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                [Path.Combine(Root, SpecTree.ManifestPath)] = Recording(new JsonObject(), ("feature", Local), ("task", Local)),
                [Path.Combine(Root, SpecTree.FeatureSchemaPath)] = new(Local),
                [Path.Combine(Root, SpecTree.ItemSchemaPath)] = new(Local),
            });
        var loader = new SpecSchemasLoader(fileSystem, new FrontmatterSchemaStore(fileSystem));

        // When
        var schemas = loader.Load(Root, new SchemaVersions([pinned]));

        // Then
        schemas.Source.Should().Be(SpecSchemaSource.Upstream);
    }

    [Fact]
    public void AManifestPinningAVersionAndRecordingAnUpstreamSchemaSource_WhenLoaded_ShouldKeepThePinnedVersionsNumberAndRuleIds()
    {
        // Given
        SchemaVersion one = new SchemaVersionFixture().WithRuleIds(new HashSet<string>(["FAKE001", "FAKE002"], StringComparer.Ordinal));
        SchemaVersion two = new SchemaVersionFixture()
            .WithNumber(new SemanticVersion(0, 2, 0))
            .WithRuleIds(new HashSet<string>(["FAKE003"], StringComparer.Ordinal));
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                [Path.Combine(Root, SpecTree.ManifestPath)] =
                    Recording(new JsonObject { ["schemaVersion"] = "0.2.0" }, ("feature", Local), ("task", Local)),
                [Path.Combine(Root, SpecTree.FeatureSchemaPath)] = new(Local),
                [Path.Combine(Root, SpecTree.ItemSchemaPath)] = new(Local),
            });
        var loader = new SpecSchemasLoader(fileSystem, new FrontmatterSchemaStore(fileSystem));

        // When
        var schemas = loader.Load(Root, new SchemaVersions([one, two]));

        // Then
        schemas.Version.Number.Should().Be(new SemanticVersion(0, 2, 0));
        schemas.Version.RuleIds.Should().BeEquivalentTo("FAKE003");
    }

    [Theory]
    [MemberData(nameof(UnfetchedEpicFiles))]
    public void AManifestRecordingAnUpstreamSchemaSourceAndDeclaringNoEpicGrammar_WhenLoaded_ShouldKeepThePinnedVersionsEpicSchema(
        string because,
        byte[]? epicFile)
    {
        // Given
        var pinned = (SchemaVersion)new SchemaVersionFixture() with { EpicSchema = """{ "const": "read from the pinned version" }""" };
        var files = new Dictionary<string, MockFileData>
        {
            [Path.Combine(Root, SpecTree.ManifestPath)] = Recording(new JsonObject(), ("feature", Local), ("task", Local)),
            [Path.Combine(Root, SpecTree.FeatureSchemaPath)] = new(Local),
            [Path.Combine(Root, SpecTree.ItemSchemaPath)] = new(Local),
        };
        if (epicFile is not null)
        {
            files[Path.Combine(Root, SpecTree.EpicSchemaPath)] = new(epicFile);
        }

        var fileSystem = new MockFileSystem(files);
        var loader = new SpecSchemasLoader(fileSystem, new FrontmatterSchemaStore(fileSystem));

        // When
        var schemas = loader.Load(Root, new SchemaVersions([pinned]));

        // Then
        schemas.Epic.Evaluate(JsonSerializer.SerializeToElement("read from the pinned version")).IsValid.Should().BeTrue(because);
    }

    [Theory]
    [MemberData(nameof(FaultyItemFiles))]
    public void AFetchedKindsFileAbsentOrNotMatchingItsRecordedHash_WhenLoaded_ShouldRejectTheManifestNamingTheFileByItsPathUnderTheRoot(
        string because,
        byte[]? itemFile)
    {
        // Given
        SchemaVersion pinned = new SchemaVersionFixture();
        var files = new Dictionary<string, MockFileData>
        {
            [Path.Combine(Root, SpecTree.ManifestPath)] = Recording(new JsonObject(), ("feature", Local), ("task", Local)),
            [Path.Combine(Root, SpecTree.FeatureSchemaPath)] = new(Local),
        };
        if (itemFile is not null)
        {
            files[Path.Combine(Root, SpecTree.ItemSchemaPath)] = new(itemFile);
        }

        var fileSystem = new MockFileSystem(files);
        var root = fileSystem.Path.GetFullPath(Root);
        var loader = new SpecSchemasLoader(fileSystem, new FrontmatterSchemaStore(fileSystem));

        // When
        var loading = () => loader.Load(root, new SchemaVersions([pinned]));

        // Then
        loading.Should().Throw<SpechtManifestException>(because)
            .Which.Message.Should().Contain(SpecTree.ItemSchemaPath, because)
            .And.NotContain(root, because);
    }

    [Fact]
    public void AManifestRecordingNoUpstreamSchemaSource_WhenLoaded_ShouldSayItsSchemasCameFromTheEmbeddedSet()
    {
        // Given
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData> { [Path.Combine(Root, SpecTree.ManifestPath)] = new("{}") });
        var loader = new SpecSchemasLoader(fileSystem, new FrontmatterSchemaStore(fileSystem));

        // When
        var schemas = loader.Load(Root);

        // Then
        schemas.Source.Should().Be(SpecSchemaSource.Embedded);
    }

    [Fact]
    public void AManifestRecordingNoUpstreamSchemaSource_WhenLoaded_ShouldBuildItsSchemasFromThePinnedVersionAndNotTheFilesUnderTheRoot()
    {
        // Given
        SchemaVersion pinned = new SchemaVersionFixture();
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                [Path.Combine(Root, SpecTree.ManifestPath)] = new("{}"),
                [Path.Combine(Root, SpecTree.FeatureSchemaPath)] = new(Local),
                [Path.Combine(Root, SpecTree.ItemSchemaPath)] = new(Local),
                [Path.Combine(Root, SpecTree.EpicSchemaPath)] = new(Local),
            });
        var loader = new SpecSchemasLoader(fileSystem, new FrontmatterSchemaStore(fileSystem));

        // When
        var schemas = loader.Load(Root, new SchemaVersions([pinned]));

        // Then
        schemas.Feature.Evaluate(JsonSerializer.SerializeToElement("read from the pinned version")).IsValid.Should().BeTrue();
    }

    private static JsonObject DeclaringTheEpicGrammar() => new() { ["identifiers"] = new JsonObject { ["epic"] = "^[0-9]{4}$" } };

    private static MockFileData Recording(JsonObject manifest, params (string Kind, byte[] Fetched)[] files)
    {
        manifest["spechtSchema"] = new JsonObject
        {
            ["location"] = "https://schemas.example/specht/",
            ["version"] = "2026.10",
            ["hashes"] = new JsonObject(
                files.Select(static file => KeyValuePair.Create(
                    file.Kind,
                    (JsonNode?)("sha256:" + Convert.ToHexStringLower(SHA256.HashData(file.Fetched)))))),
        };

        return new MockFileData(manifest.ToJsonString());
    }

    private const string Root = "repo";

    private static readonly byte[] Local = Encoding.UTF8.GetBytes("""{ "const": "read from the local copy" }""");
}
