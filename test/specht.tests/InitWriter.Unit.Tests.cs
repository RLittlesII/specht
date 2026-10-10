using System.IO.Abstractions.TestingHelpers;
using System.Text;
using AwesomeAssertions;
using specht.tool.Features.Init;

namespace specht.tests;

/// <summary>
/// The init writer over an in-memory file system (<c>0001-F4</c> B-001, B-003, B-006, B-008, B-010; C-1, C-3, C-4): which
/// shipped version it writes, where each embedded name lands, and the choice between written and skipped, file by file.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class InitWriterUnitTests
{
    /// <summary>Gets the versions a shipping copy carries, and the one the writer must write.</summary>
    public static TheoryData<int[], int> Versions =>
        new()
        {
            { [1], 1 },
            { [1, 2], 2 },
            { [2, 10], 10 },
        };

    /// <summary>Gets an embedded name, and the path relative to the root it must be written to.</summary>
    public static TheoryData<string, string> Names =>
        new()
        {
            { "schema/v1/spec-structure.schema.json", ".spec/schema/spec-structure.schema.json" },
            { "schema/v1/task.frontmatter.schema.json", ".spec/schema/task.frontmatter.schema.json" },
            { "templates/v1/feature.md", ".spec/templates/feature.md" },
            { "templates/v1/lesson.md", ".spec/templates/lesson.md" },
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
    public async Task AShippingCopyOfSeveralVersions_WhenWritten_ShouldWriteOnlyTheHighestNumberedVersion(int[] versions, int newest)
    {
        // Given
        var shippingCopy = versions
            .SelectMany(static version => new[] { $"schema/v{version}/spec-structure.schema.json", $"templates/v{version}/feature.md" })
            .ToDictionary(static name => name, static name => Encoding.UTF8.GetBytes(name));
        var fileSystem = BareRoot();

        // When
        var listing = await new InitWriter(shippingCopy, fileSystem).Write(Root, CancellationToken.None);

        // Then
        listing.Should().Equal((".spec/schema/spec-structure.schema.json", true), (".spec/templates/feature.md", true));
        fileSystem.File.ReadAllText(Path.Combine(Root, ".spec", "schema", "spec-structure.schema.json"))
            .Should()
            .Be($"schema/v{newest}/spec-structure.schema.json");
        fileSystem.File.ReadAllText(Path.Combine(Root, ".spec", "templates", "feature.md")).Should().Be($"templates/v{newest}/feature.md");
    }

    [Theory]
    [MemberData(nameof(Names))]
    public async Task AnEmbeddedName_WhenWritten_ShouldLandUnderItsFolderInDotSpecWithItsBytesUnchanged(string name, string relative)
    {
        // Given
        byte[] bytes = [0xEF, 0xBB, 0xBF, 0x7B, 0x0D, 0x0A, 0x7D];
        var fileSystem = BareRoot();

        // When
        var listing = await new InitWriter(new Dictionary<string, byte[]> { [name] = bytes }, fileSystem).Write(Root, CancellationToken.None);

        // Then
        listing.Should().Equal((relative, true));
        fileSystem.File.ReadAllBytes(Path.Combine([Root, .. relative.Split('/')])).Should().Equal(bytes);
    }

    [Fact]
    public async Task AFileThatExists_WhenWritten_ShouldBeListedSkippedAndLeftAsItWas()
    {
        // Given
        var present = Path.Combine(Root, ".spec", "templates", "feature.md");
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> { [present] = new("# Local edits\n") });
        var shippingCopy = new Dictionary<string, byte[]>
        {
            ["templates/v1/feature.md"] = "# Shipped\n"u8.ToArray(),
            ["templates/v1/adr.md"] = "# Shipped\n"u8.ToArray(),
        };

        // When
        var listing = await new InitWriter(shippingCopy, fileSystem).Write(Root, CancellationToken.None);

        // Then
        listing.Should().Equal((".spec/templates/adr.md", true), (".spec/templates/feature.md", false));
        fileSystem.File.ReadAllText(present).Should().Be("# Local edits\n");
    }

    [Fact]
    public async Task SomeFilesPresentAndOthersAbsent_WhenWritten_ShouldWriteEachAbsentOneWithItsShippedBytes()
    {
        // Given
        var present = Path.Combine(Root, ".spec", "templates", "decision.md");
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData> { [present] = new("# Local edits\n") });
        var shippingCopy = new Dictionary<string, byte[]>
        {
            ["templates/v1/adr.md"] = "# Shipped adr\n"u8.ToArray(),
            ["templates/v1/decision.md"] = "# Shipped decision\n"u8.ToArray(),
            ["templates/v1/feature.md"] = "# Shipped feature\n"u8.ToArray(),
            ["schema/v1/task.frontmatter.schema.json"] = "{}\n"u8.ToArray(),
        };

        // When
        await new InitWriter(shippingCopy, fileSystem).Write(Root, CancellationToken.None);

        // Then
        foreach (var (name, bytes) in shippingCopy.Where(static entry => entry.Key != "templates/v1/decision.md"))
        {
            var absent = Path.Combine([Root, ".spec", .. name.Replace("/v1/", "/", StringComparison.Ordinal).Split('/')]);
            fileSystem.File.Exists(absent).Should().BeTrue($"{name} was absent");
            fileSystem.File.ReadAllBytes(absent).Should().Equal(bytes, name);
        }
    }

    [Theory]
    [MemberData(nameof(RootsThatAreNotDirectories))]
    public async Task ARootThatIsNotADirectory_WhenWritten_ShouldThrowRootNotFoundAndCreateNothing(string because, MockFileSystem fileSystem)
    {
        // Given
        var before = fileSystem.AllPaths.ToList();
        var writer = new InitWriter(new Dictionary<string, byte[]> { ["templates/v1/feature.md"] = [] }, fileSystem);

        // When
        var write = () => writer.Write(Root, CancellationToken.None);

        // Then
        await write.Should().ThrowExactlyAsync<SpechtRootNotFoundException>(because);
        fileSystem.AllPaths.Should().Equal(before, because);
    }

    [Fact]
    public async Task TheEmbeddedShippingCopy_WhenWrittenIntoABareRoot_ShouldWriteTheEightFilesAndNothingOutsideTheTwoFolders()
    {
        // Given
        var fileSystem = BareRoot();
        var schema = Path.Combine(Root, ".spec", "schema") + Path.DirectorySeparatorChar;
        var templates = Path.Combine(Root, ".spec", "templates") + Path.DirectorySeparatorChar;

        // When
        var listing = await new InitWriter(InitWriter.ShippingCopy(typeof(InitWriter).Assembly), fileSystem).Write(Root, CancellationToken.None);

        // Then
        listing.Should().Equal(
            (".spec/schema/epic.frontmatter.schema.json", true),
            (".spec/schema/feature-spec.frontmatter.schema.json", true),
            (".spec/schema/spec-structure.schema.json", true),
            (".spec/schema/task.frontmatter.schema.json", true),
            (".spec/templates/adr.md", true),
            (".spec/templates/decision.md", true),
            (".spec/templates/feature.md", true),
            (".spec/templates/lesson.md", true));
        fileSystem.AllFiles.Should()
            .HaveCount(8)
            .And.AllSatisfy(path => path.Should().Match(file => file.Contains(schema, StringComparison.Ordinal)
                || file.Contains(templates, StringComparison.Ordinal)));
    }

    private static MockFileSystem BareRoot()
    {
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory(Root);

        return fileSystem;
    }

    private const string Root = "repo";
}
