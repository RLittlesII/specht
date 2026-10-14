using System.IO.Abstractions.TestingHelpers;
using System.Text;
using System.Text.Json;
using AwesomeAssertions;
using Specht.Manifest;
using Specht.Tests.Shared;
using Specht.Tool.Features.Init;

namespace Specht.Tests.Init;

/// <summary>
/// The init writer over an in-memory file system (<c>0001-F4</c> B-001, B-003, B-006, B-008, B-010; C-1, C-3, C-4): which
/// shipped version it writes, where each embedded name lands - the manifest at <c>.specht/manifest.json</c>, a frontmatter
/// schema under <c>.specht/schema/frontmatter/</c>, a template under <c>.specht/templates/</c> (<c>0001-F5</c> decision 0011) -
/// and the choice between written and skipped, file by file.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class InitWriterUnitTests
{
    /// <summary>Gets the selected parts, whether a manifest already exists, and the files the writer must list.</summary>
    public static TheoryData<bool, bool, bool, string[]> PartSelections =>
        new()
        {
            { false, false, false, [".specht/manifest.json"] },
            {
                true,
                false,
                false,
                [".specht/manifest.json", ".specht/schema/frontmatter/feature.schema.json", ".specht/schema/frontmatter/task.schema.json"]
            },
            { false, true, false, [".specht/manifest.json", ".specht/templates/feature.md"] },
            {
                true,
                true,
                false,
                [
                    ".specht/manifest.json",
                    ".specht/schema/frontmatter/feature.schema.json",
                    ".specht/schema/frontmatter/task.schema.json",
                    ".specht/templates/feature.md",
                ]
            },
            {
                true,
                false,
                true,
                [
                    ".specht/manifest.json",
                    ".specht/schema/frontmatter/epic.schema.json",
                    ".specht/schema/frontmatter/feature.schema.json",
                    ".specht/schema/frontmatter/task.schema.json",
                ]
            },
        };

    /// <summary>Gets the versions a shipping copy carries, and the one the writer must write.</summary>
    public static TheoryData<string[], string> Versions =>
        new()
        {
            { ["0.1.0"], "0.1.0" },
            { ["0.1.0", "0.2.0"], "0.2.0" },
            { ["0.9.0", "0.10.0"], "0.10.0" },
            { ["0.1.9", "0.1.10"], "0.1.10" },
            { ["0.99.99", "1.0.0"], "1.0.0" },
            { ["1.0.0", "0.10.0", "0.9.0"], "1.0.0" },
        };

    /// <summary>Gets an embedded name and the path relative to the root it must be written to.</summary>
    public static TheoryData<string, string> Names =>
        new()
        {
            { "manifest/v0.1.0/manifest.json", ".specht/manifest.json" },
            { "schema/v0.1.0/frontmatter/task.schema.json", ".specht/schema/frontmatter/task.schema.json" },
            { "templates/v0.1.0/feature.md", ".specht/templates/feature.md" },
            { "templates/v0.1.0/lesson.md", ".specht/templates/lesson.md" },
        };

    /// <summary>Gets file systems on which the root is not a directory.</summary>
    public static TheoryData<string, MockFileSystem> RootsThatAreNotDirectories =>
        new()
        {
            { "no root at all", new MockFileSystem() },
            { "a root that is a file", new MockFileSystem(new Dictionary<string, MockFileData> { [Root] = new(string.Empty) }) },
        };

    [Theory]
    [MemberData(nameof(Versions))]
    public async Task AShippingCopyOfSeveralVersions_WhenWritten_ShouldWriteOnlyTheHighestNumberedVersion(string[] versions, string newest)
    {
        // Given
        var shippingCopy = versions
            .SelectMany(static version => new[] { $"manifest/v{version}/manifest.json", $"templates/v{version}/feature.md" })
            .ToDictionary(static name => name, static name => Encoding.UTF8.GetBytes(name));
        var fileSystem = BareRoot();

        // When
        var listing = await new InitWriter(
            shippingCopy,
            fileSystem,
            EngineServices.Parser(),
            new SpecManifestStore(fileSystem)).Write(Root, schemas: true, templates: true, CancellationToken.None);

        // Then
        listing.Should().Equal((".specht/manifest.json", true), (".specht/templates/feature.md", true));
        fileSystem.File.ReadAllText(Path.Combine(Root, SpecTree.ManifestPath))
            .Should()
            .Be($"manifest/v{newest}/manifest.json");
        fileSystem.File.ReadAllText(Path.Combine(Root, SpecTree.TemplatesFolder, "feature.md")).Should().Be($"templates/v{newest}/feature.md");
    }

    [Theory]
    [MemberData(nameof(Versions))]
    public async Task ShippedManifests_WhenInitWrites_ShouldPinTheNewestVersionBySemverPrecedence(string[] versions, string newest)
    {
        // Given
        var shippingCopy = versions.ToDictionary(
            static version => $"manifest/v{version}/manifest.json",
            static version => JsonSerializer.SerializeToUtf8Bytes(new { schemaVersion = version }));
        var fileSystem = BareRoot();

        // When
        await new InitWriter(
            shippingCopy,
            fileSystem,
            EngineServices.Parser(),
            new SpecManifestStore(fileSystem)).Write(Root, schemas: false, templates: false, CancellationToken.None);

        // Then
        using var manifest = JsonDocument.Parse(fileSystem.File.ReadAllText(Path.Combine(Root, SpecTree.ManifestPath)));
        manifest.RootElement.GetProperty("schemaVersion").GetString().Should().Be(newest);
    }

    [Theory]
    [MemberData(nameof(Names))]
    public async Task AnEmbeddedName_WhenWritten_ShouldLandAtItsPathInDotSpechtWithItsBytesUnchanged(string name, string relative)
    {
        // Given
        byte[] bytes = [0xEF, 0xBB, 0xBF, 0x7B, 0x0D, 0x0A, 0x7D];
        var fileSystem = BareRoot();

        // When
        var listing = await new InitWriter(
            new Dictionary<string, byte[]> { [name] = bytes },
            fileSystem,
            EngineServices.Parser(),
            new SpecManifestStore(fileSystem))
            .Write(Root, schemas: true, templates: true, CancellationToken.None);

        // Then
        listing.Should().Equal((relative, true));
        fileSystem.File.ReadAllBytes(Path.Combine([Root, .. relative.Split('/')])).Should().Equal(bytes);
    }

    [Fact]
    public async Task AFileThatExists_WhenWritten_ShouldBeListedSkippedAndLeftAsItWas()
    {
        // Given
        var present = Path.Combine(Root, SpecTree.TemplatesFolder, "feature.md");
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> { [present] = new("# Local edits\n") });
        var shippingCopy = new Dictionary<string, byte[]>
        {
            ["templates/v0.1.0/feature.md"] = "# Shipped\n"u8.ToArray(),
            ["templates/v0.1.0/adr.md"] = "# Shipped\n"u8.ToArray(),
        };

        // When
        var listing = await new InitWriter(
            shippingCopy,
            fileSystem,
            EngineServices.Parser(),
            new SpecManifestStore(fileSystem)).Write(Root, schemas: true, templates: true, CancellationToken.None);

        // Then
        listing.Should().Equal((".specht/templates/adr.md", true), (".specht/templates/feature.md", false));
        fileSystem.File.ReadAllText(present).Should().Be("# Local edits\n");
    }

    [Fact]
    public async Task SomeFilesPresentAndOthersAbsent_WhenWritten_ShouldWriteEachAbsentOneWithItsShippedBytes()
    {
        // Given
        var present = Path.Combine(Root, SpecTree.TemplatesFolder, "decision.md");
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> { [present] = new("# Local edits\n") });
        var shippingCopy = new Dictionary<string, byte[]>
        {
            ["templates/v0.1.0/adr.md"] = "# Shipped adr\n"u8.ToArray(),
            ["templates/v0.1.0/decision.md"] = "# Shipped decision\n"u8.ToArray(),
            ["templates/v0.1.0/feature.md"] = "# Shipped feature\n"u8.ToArray(),
            ["schema/v0.1.0/frontmatter/task.schema.json"] = "{}\n"u8.ToArray(),
        };

        // When
        await new InitWriter(
            shippingCopy,
            fileSystem,
            EngineServices.Parser(),
            new SpecManifestStore(fileSystem)).Write(Root, schemas: true, templates: true, CancellationToken.None);

        // Then
        foreach (var (name, bytes) in shippingCopy.Where(static entry => entry.Key != "templates/v0.1.0/decision.md"))
        {
            var absent = Path.Combine([Root, ".specht", .. name.Replace("/v0.1.0/", "/", StringComparison.Ordinal).Split('/')]);
            fileSystem.File.Exists(absent).Should().BeTrue($"{name} was absent");
            fileSystem.File.ReadAllBytes(absent).Should().Equal(bytes, name);
        }
    }

    [Theory]
    [MemberData(nameof(PartSelections))]
    public async Task SelectedParts_WhenWritten_ShouldListOnlyTheManifestAndAskedForParts(
        bool schemas,
        bool templates,
        bool manifestPresent,
        string[] expected)
    {
        // Given
        var fileSystem = BareRoot();
        if (manifestPresent)
        {
            fileSystem.AddFile(Path.Combine(Root, SpecTree.ManifestPath), new MockFileData("{ \"identifiers\": { \"epic\": \"^[0-9]+$\" } }"));
        }

        var shippingCopy = new Dictionary<string, byte[]>
        {
            ["manifest/v0.1.0/manifest.json"] = "{}\n"u8.ToArray(),
            ["schema/v0.1.0/frontmatter/epic.schema.json"] = "{}\n"u8.ToArray(),
            ["schema/v0.1.0/frontmatter/feature.schema.json"] = "{}\n"u8.ToArray(),
            ["schema/v0.1.0/frontmatter/task.schema.json"] = "{}\n"u8.ToArray(),
            ["templates/v0.1.0/feature.md"] = "# Shipped feature\n"u8.ToArray(),
        };

        // When
        var listing = await new InitWriter(
            shippingCopy,
            fileSystem,
            EngineServices.Parser(),
            new SpecManifestStore(fileSystem)).Write(Root, schemas, templates, CancellationToken.None);

        // Then
        listing.Select(static entry => entry.Path).Should().Equal(expected);
    }

    [Fact]
    public async Task FilesAtTheRetiredLocation_WhenWritten_ShouldCreateModifyOrDeleteNoFileUnderTheRootsSpecFolder()
    {
        // Given
        var specFolder = Path.Combine(Root, ".spec") + Path.DirectorySeparatorChar;
        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>
            {
                [Path.Combine(Root, SpecTree.RetiredManifestPath)] = new("{ \"sections\": [\"A synthetic section\"] }\n"),
                [Path.Combine(Root, ".spec", "templates", "feature.md")] = new("# Local edits\n"),
                [Path.Combine(Root, ".spec", "brief.md")] = new("# A synthetic brief\n"),
            });
        var shippingCopy = new Dictionary<string, byte[]>
        {
            ["manifest/v0.1.0/manifest.json"] = "{}\n"u8.ToArray(),
            ["schema/v0.1.0/frontmatter/task.schema.json"] = "{}\n"u8.ToArray(),
            ["templates/v0.1.0/feature.md"] = "# Shipped feature\n"u8.ToArray(),
            ["templates/v0.1.0/adr.md"] = "# Shipped adr\n"u8.ToArray(),
        };
        Dictionary<string, string> UnderTheSpecFolder() =>
            fileSystem.AllFiles.Where(file => file.Contains(specFolder, StringComparison.Ordinal))
                .ToDictionary(static file => file, fileSystem.File.ReadAllText);
        var before = UnderTheSpecFolder();

        // When
        await new InitWriter(
            shippingCopy,
            fileSystem,
            EngineServices.Parser(),
            new SpecManifestStore(fileSystem)).Write(Root, schemas: true, templates: true, CancellationToken.None);

        // Then
        UnderTheSpecFolder().Should().Equal(before);
    }

    [Theory]
    [MemberData(nameof(RootsThatAreNotDirectories))]
    public async Task ARootThatIsNotADirectory_WhenWritten_ShouldThrowRootNotFoundAndCreateNothing(string because, MockFileSystem fileSystem)
    {
        // Given
        var before = fileSystem.AllPaths.ToList();
        var writer = new InitWriter(
            new Dictionary<string, byte[]> { ["templates/v0.1.0/feature.md"] = [] },
            fileSystem,
            EngineServices.Parser(),
            new SpecManifestStore(fileSystem));

        // When
        var write = () => writer.Write(Root, schemas: false, templates: false, CancellationToken.None);

        // Then
        await write.Should().ThrowExactlyAsync<SpechtRootNotFoundException>(because);
        fileSystem.AllPaths.Should().Equal(before, because);
    }

    [Fact]
    public async Task TheEmbeddedShippingCopy_WhenWrittenIntoABareRoot_ShouldWriteTheManifestAndTheFilesUnderTheTwoFoldersAndNothingElse()
    {
        // Given
        var fileSystem = BareRoot();
        var root = fileSystem.Path.GetFullPath(Root);

        // When
        var listing = await new InitWriter(
            InitWriter.ShippingCopy(typeof(InitWriter).Assembly),
            fileSystem,
            EngineServices.Parser(),
            new SpecManifestStore(fileSystem))
            .Write(Root, schemas: true, templates: true, CancellationToken.None);

        // Then
        listing.Should().Equal(
            (".specht/manifest.json", true),
            (".specht/schema/frontmatter/feature.schema.json", true),
            (".specht/schema/frontmatter/task.schema.json", true),
            (".specht/templates/adr.md", true),
            (".specht/templates/decision.md", true),
            (".specht/templates/feature.md", true),
            (".specht/templates/lesson.md", true));
        fileSystem.AllFiles
            .Select(file => fileSystem.Path.GetRelativePath(root, file).Replace(fileSystem.Path.DirectorySeparatorChar, '/'))
            .Should()
            .HaveCount(7)
            .And.AllSatisfy(static path => path.Should().Match(static file =>
                file == SpecTree.ManifestPath
                || file.StartsWith(SpecTree.FrontmatterSchemaFolder + "/", StringComparison.Ordinal)
                || file.StartsWith(SpecTree.TemplatesFolder + "/", StringComparison.Ordinal)));
    }

    private static MockFileSystem BareRoot()
    {
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory(Root);

        return fileSystem;
    }

    private const string Root = "repo";
}
