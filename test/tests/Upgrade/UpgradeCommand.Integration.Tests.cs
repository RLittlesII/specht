using System.IO.Abstractions.TestingHelpers;
using System.Text;
using AwesomeAssertions;
using Specht.Tests.Shared;
using Specht.Versioning;
using Spectre.Console.Testing;

namespace Specht.Tests.Upgrade;

/// <summary>
/// <c>specht upgrade</c> through Spectre's command tester over an in-memory file system and the shipped files (<c>0001-F7</c>
/// B-005, B-006, B-007, B-052): a root's frontmatter schemas and templates are brought to the pin from an older or a newer
/// version, a root already at its pin prints that it is current and rewrites nothing, only the manifest, the frontmatter
/// schema folder and the templates folder may change, and the manifest is never rewritten.
/// </summary>
[Trait("Tier", "Integration")]
public sealed class UpgradeCommandIntegrationTests
{
    [Theory]
    [ClassData(typeof(UpgradeVersionPairs))]
    public void FilesOfAnotherVersionWithAHandEditedTemplate_WhenUpgraded_ShouldBringSchemasAndTemplatesToThePinAndExitZero(string files, string pin)
    {
        // Given
        var root = Path.GetFullPath("repo");
        var shipped = UpgradeServices.Shipped();
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile(Path.Combine(root, ".specht/manifest.json"), new MockFileData($"{{\"schemaVersion\":\"{pin}\"}}"));
        foreach (var file in shipped.At(Version(files)).Where(static file => file.Relative != ".specht/manifest.json"))
        {
            fileSystem.AddFile(Path.Combine(root, file.Relative), new MockFileData(file.Bytes));
        }

        var edited = Path.Combine(root, ".specht/templates/feature.md");
        fileSystem.File.WriteAllText(edited, "# A synthetic template with local edits\n");
        var app = UpgradeServices.App(fileSystem, new TestConsole());

        // When
        var result = app.Run("upgrade", "--root", "repo");

        // Then
        result.ExitCode.Should().Be(0, result.Output);
        foreach (var file in shipped.At(Version(pin)).Where(static file => file.Relative != ".specht/manifest.json"))
        {
            fileSystem.File.ReadAllBytes(Path.Combine(root, file.Relative)).Should().Equal(file.Bytes, file.Relative);
        }
    }

    [Theory]
    [ClassData(typeof(UpgradeVersionPairs))]
    public void RootAlreadyAtItsPin_WhenUpgraded_ShouldSayCurrentRewriteNothingAndExitZero(string pin, string other)
    {
        // Given
        var root = Path.GetFullPath("repo");
        var stamp = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var shipped = UpgradeServices.Shipped();
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile(Path.Combine(root, ".specht/manifest.json"), new MockFileData($"{{\"schemaVersion\":\"{pin}\"}}") { LastWriteTime = stamp });
        foreach (var file in shipped.At(Version(pin)).Where(static file => file.Relative != ".specht/manifest.json"))
        {
            fileSystem.AddFile(Path.Combine(root, file.Relative), new MockFileData(file.Bytes) { LastWriteTime = stamp });
        }

        var before = fileSystem.AllFiles.ToDictionary(
            static path => path, path => (fileSystem.File.ReadAllBytes(path), fileSystem.File.GetLastWriteTimeUtc(path)));
        var app = UpgradeServices.App(fileSystem, new TestConsole());

        // When
        var result = app.Run("upgrade", "--root", "repo");

        // Then
        result.ExitCode.Should().Be(0, other + result.Output);
        result.Output.Should().Contain($".specht/ is current at schemaVersion {pin}");
        fileSystem.AllFiles.Should().BeEquivalentTo(before.Keys);
        foreach (var (path, (bytes, written)) in before)
        {
            fileSystem.File.ReadAllBytes(path).Should().Equal(bytes);
            fileSystem.File.GetLastWriteTimeUtc(path).Should().Be(written, path);
        }
    }

    [Theory]
    [ClassData(typeof(UpgradeVersionPairs))]
    public void RootHoldingDocuments_WhenUpgraded_ShouldChangeNothingOutsideTheManifestTheSchemaFolderAndTheTemplatesFolder(string files, string pin)
    {
        // Given
        var root = Path.GetFullPath("repo");
        var shipped = UpgradeServices.Shipped();
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile(Path.Combine(root, ".specht/manifest.json"), new MockFileData($"{{\"schemaVersion\":\"{pin}\"}}"));
        foreach (var file in shipped.At(Version(files)).Where(static file => file.Relative != ".specht/manifest.json"))
        {
            fileSystem.AddFile(Path.Combine(root, file.Relative), new MockFileData(file.Bytes));
        }

        fileSystem.AddFile(Path.Combine(root, "examples/.spec/README.md"), new MockFileData("synthetic specification"));
        fileSystem.AddFile(Path.Combine(root, "examples/.spec/example.feature"), new MockFileData("Feature: synthetic companion"));
        fileSystem.AddFile(Path.Combine(root, "examples/.spec/decisions/example.md"), new MockFileData("synthetic record"));
        fileSystem.AddFile(Path.Combine(root, ".specht/schema/notes.md"), new MockFileData("synthetic note beside the schema folder"));
        var before = fileSystem.AllFiles.ToDictionary(static path => path, path => fileSystem.File.ReadAllBytes(path));
        var app = UpgradeServices.App(fileSystem, new TestConsole());

        // When
        var result = app.Run("upgrade", "--root", "repo");

        // Then
        result.ExitCode.Should().Be(0, result.Output);
        var allowed = new[] { ".specht/manifest.json", ".specht/schema/frontmatter/", ".specht/templates/" };
        var changed = fileSystem.AllFiles.Union(before.Keys).Where(path =>
            !fileSystem.File.Exists(path) || !before.TryGetValue(path, out var bytes) || !fileSystem.File.ReadAllBytes(path).SequenceEqual(bytes));
        changed.Select(path => Path.GetRelativePath(root, path).Replace('\\', '/'))
            .Should().OnlyContain(relative => allowed.Any(prefix => relative.StartsWith(prefix, StringComparison.Ordinal)));
        fileSystem.AllFiles.Should().BeEquivalentTo(before.Keys);
    }

    [Theory]
    [ClassData(typeof(UpgradeVersionPairs))]
    public void FilesOfAnotherVersion_WhenUpgraded_ShouldLeaveTheManifestBytesAsTheyWere(string files, string pin)
    {
        // Given
        var root = Path.GetFullPath("repo");
        var manifest = Encoding.UTF8.GetBytes($"{{\r\n  \"schemaVersion\" : \"{pin}\",\r\n  \"rules\": {{\"SPEC031\": \"warning\"}}\r\n}}\r\n");
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile(Path.Combine(root, ".specht/manifest.json"), new MockFileData(manifest));
        foreach (var file in UpgradeServices.Shipped().At(Version(files)).Where(static file => file.Relative != ".specht/manifest.json"))
        {
            fileSystem.AddFile(Path.Combine(root, file.Relative), new MockFileData(file.Bytes));
        }

        var app = UpgradeServices.App(fileSystem, new TestConsole());

        // When
        var result = app.Run("upgrade", "--root", "repo");

        // Then
        result.ExitCode.Should().Be(0, result.Output);
        fileSystem.File.ReadAllBytes(Path.Combine(root, ".specht/manifest.json")).Should().Equal(manifest);
    }

    private static SemanticVersion Version(string text) =>
        SemanticVersion.TryParse(text, out var version) ? version : throw new FormatException(text);
}
