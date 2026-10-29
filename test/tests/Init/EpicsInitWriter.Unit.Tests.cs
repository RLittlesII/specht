using System.IO.Abstractions.TestingHelpers;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Specht.Tests.Shared;
using Specht.Tool.Features.Init;

namespace Specht.Tests.Init;

/// <summary>
/// The init writer's epic tier over an in-memory file system (<c>0001-F4</c> B-018, B-021, B-022, B-023; C-5, C-6): into a
/// bare root only the epics choice decides the epic frontmatter schema, beside an existing manifest only that manifest's epic
/// grammar decides it, and a manifest written without epics declares no epic grammar and no named <c>epic</c> group in its
/// task grammar.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class EpicsInitWriterUnitTests
{
    [Theory]
    [ClassData(typeof(EpicSchemaRoots))]
    public async Task ARootAndAnEpicsChoice_WhenSchemasAreWritten_ShouldWriteTheEpicSchemaOnlyWhereTheEpicTierIsDeclared(
        string because,
        string? manifest,
        bool epics,
        bool epicSchema)
    {
        // Given
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory(Root);
        if (manifest is not null)
        {
            fileSystem.AddFile(Path.Combine(Root, SpecTree.ManifestPath), new MockFileData(manifest));
        }

        var shippingCopy = new Dictionary<string, byte[]>
        {
            ["manifest/v0.1.0/manifest.json"] = """{ "identifiers": { "task": "^[0-9]{4}-[0-9]{2}$", "epic": "^[0-9]{4}$" } }"""u8.ToArray(),
            ["schema/v0.1.0/frontmatter/epic.schema.json"] = "{}\n"u8.ToArray(),
            ["schema/v0.1.0/frontmatter/feature.schema.json"] = "{}\n"u8.ToArray(),
            ["schema/v0.1.0/frontmatter/task.schema.json"] = "{}\n"u8.ToArray(),
        };

        InitWriter writer = new InitWriterFixture()
            .WithFileSystem(fileSystem)
            .WithShippingCopy(shippingCopy);

        // When
        var listing = await writer.Write(Root, schemas: true, templates: false, CancellationToken.None, epics: epics);

        // Then
        listing.Select(static entry => entry.Path).Contains(SpecTree.EpicSchemaPath).Should().Be(epicSchema, because);
        fileSystem.File.Exists(Path.Combine(Root, SpecTree.EpicSchemaPath)).Should().Be(epicSchema, because);
    }

    [Fact]
    public async Task ABareRootWithoutEpics_WhenTheManifestIsWritten_ShouldDeclareNoEpicGrammar()
    {
        // Given
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory(Root);
        var shippingCopy = new Dictionary<string, byte[]>
        {
            ["manifest/v0.1.0/manifest.json"] = """{ "identifiers": { "task": "^[0-9]{4}-[0-9]{2}$", "epic": "^[0-9]{4}$" } }"""u8.ToArray(),
        };

        InitWriter writer = new InitWriterFixture()
            .WithFileSystem(fileSystem)
            .WithShippingCopy(shippingCopy);

        // When
        await writer.Write(Root, schemas: false, templates: false, CancellationToken.None, epics: false);

        // Then
        var written = JsonNode.Parse(fileSystem.File.ReadAllText(Path.Combine(Root, SpecTree.ManifestPath)))!.AsObject();
        (written["identifiers"] as JsonObject ?? new JsonObject()).Should().NotContainKey("epic");
    }

    [Fact]
    public async Task ABareRootWithoutEpics_WhenTheManifestIsWritten_ShouldWriteATaskGrammarWithNoNamedEpicGroup()
    {
        // Given
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory(Root);

        InitWriter writer = new InitWriterFixture()
            .WithFileSystem(fileSystem);

        // When
        await writer.Write(Root, schemas: false, templates: false, CancellationToken.None, epics: false);

        // Then
        var written = JsonNode.Parse(fileSystem.File.ReadAllText(Path.Combine(Root, SpecTree.ManifestPath)))!.AsObject();
        var task = written["identifiers"]?["task"]?.GetValue<string>();
        task.Should().NotBeNull("the manifest written declares a task grammar");
        new Regex(task!).GetGroupNames().Should().NotContain("epic");
    }

    private const string Root = "repo";
}
