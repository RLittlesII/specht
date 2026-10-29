using System.IO.Abstractions.TestingHelpers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Tests.Shared;
using Specht.Versioning;
using Spectre.Console.Testing;

namespace Specht.Tests.Upgrade;

/// <summary>
/// <c>specht upgrade</c> through Spectre's command tester over an in-memory file system and the shipped files (<c>0001-F7</c>
/// B-005, B-006, B-007, B-016, B-017, B-052, B-059): a root's frontmatter schemas and templates are brought to the pin from an
/// older or a newer version, the keys the pin defines are added to the manifest and the consumer's settings kept, a root
/// already at its pin prints that it is current and rewrites nothing, only the manifest, the frontmatter schema folder and
/// the templates folder may change, a manifest that lacks no key is never rewritten, and a rewrite prints the pinned
/// version and each rewritten file.
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
        foreach (var file in shipped.At(Version(pin)))
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
        var defined = JsonNode.Parse(
            UpgradeServices.Shipped().At(Version(pin)).Single(static file => file.Relative == ".specht/manifest.json").Bytes)!.AsObject();
        defined["schemaVersion"] = pin;
        defined["rules"] = new JsonObject { ["SPEC031"] = "warning" };
        var manifest = Encoding.UTF8.GetBytes(
            defined.ToJsonString(new JsonSerializerOptions { WriteIndented = true }).ReplaceLineEndings("\r\n") + "\r\n");
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

    [Theory]
    [ClassData(typeof(UpgradeVersionPairs))]
    public void FilesOfAnotherVersionAndAManifestLackingKeys_WhenUpgraded_ShouldPrintTheVersionLineThenOneLinePerRewrittenFileAndNoDiff(
        string files, string pin)
    {
        // Given
        var root = Path.GetFullPath("repo");
        var shipped = UpgradeServices.Shipped();
        var present = shipped.At(Version(files)).Where(static file => file.Relative != ".specht/manifest.json").ToList();
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile(Path.Combine(root, ".specht/manifest.json"), new MockFileData($"{{\"schemaVersion\":\"{pin}\"}}"));
        foreach (var file in present)
        {
            fileSystem.AddFile(Path.Combine(root, file.Relative), new MockFileData(file.Bytes));
        }

        var app = UpgradeServices.App(fileSystem, new TestConsole());

        // When
        var result = app.Run("upgrade", "--root", "repo");

        // Then
        result.ExitCode.Should().Be(0, result.Output);
        var differing = shipped.At(Version(pin))
            .Where(file => present.Any(old => old.Relative == file.Relative && !old.Bytes.SequenceEqual(file.Bytes)))
            .Select(static file => $"rewritten {file.Relative}");
        result.Output.Split('\n').Should().Equal(
            [$".specht/ rewritten to schemaVersion {pin}", "rewritten .specht/manifest.json", .. differing]);
    }

    [Fact]
    public void CurrentFilesAndAManifestLackingOneKey_WhenUpgraded_ShouldExitZeroAndNameTheVersionAndTheManifest()
    {
        // Given
        var root = Path.GetFullPath("repo");
        var fileSystem = new MockFileSystem();
        foreach (var file in UpgradeServices.Shipped().At(Version("0.2.0")))
        {
            fileSystem.AddFile(Path.Combine(root, file.Relative), new MockFileData(file.Bytes));
        }

        var manifest = JsonNode.Parse(fileSystem.File.ReadAllBytes(Path.Combine(root, ".specht/manifest.json")))!.AsObject();
        manifest.Remove("claim").Should().BeTrue();
        fileSystem.File.WriteAllText(Path.Combine(root, ".specht/manifest.json"), manifest.ToJsonString());
        var app = UpgradeServices.App(fileSystem, new TestConsole());

        // When
        var result = app.Run("upgrade", "--root", "repo");

        // Then
        result.ExitCode.Should().Be(0, result.Output);
        result.Output.Split('\n').Should().Equal(".specht/ rewritten to schemaVersion 0.2.0", "rewritten .specht/manifest.json");
    }

    [Fact]
    public void ConsumerSettingsAndADefinedKeyWithTheConsumersValue_WhenUpgraded_ShouldKeepThemAndAddEveryOtherDefinedKey()
    {
        // Given
        var root = Path.GetFullPath("repo");
        var shipped = UpgradeServices.Shipped();
        var defined = JsonNode.Parse(
            shipped.At(Version("0.2.0")).Single(static file => file.Relative == ".specht/manifest.json").Bytes)!.AsObject();
        var sections = defined["sections"]!.DeepClone().AsArray();
        sections[2] = "3. Release criteria";
        var roles = defined["roles"]!.DeepClone().AsObject();
        roles["claims"] = "3. Release criteria";
        var consumer = new JsonObject
        {
            ["schemaVersion"] = "0.2.0",
            ["rules"] = new JsonObject { ["SPEC031"] = "warning", ["SPEC020"] = "off" },
            ["sections"] = sections,
            ["roles"] = roles,
            ["claim"] = new JsonObject { ["tag"] = "@req:{claim}", ["tagIgnoreCase"] = true },
        };
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile(Path.Combine(root, ".specht/manifest.json"), new MockFileData(consumer.ToJsonString()));
        foreach (var file in shipped.At(Version("0.1.0")).Where(static file => file.Relative != ".specht/manifest.json"))
        {
            fileSystem.AddFile(Path.Combine(root, file.Relative), new MockFileData(file.Bytes));
        }

        var app = UpgradeServices.App(fileSystem, new TestConsole());

        // When
        var result = app.Run("upgrade", "--root", "repo");

        // Then
        result.ExitCode.Should().Be(0, result.Output);
        var written = JsonNode.Parse(fileSystem.File.ReadAllBytes(Path.Combine(root, ".specht/manifest.json")))!.AsObject();
        foreach (var (key, value) in consumer)
        {
            JsonNode.DeepEquals(written[key], value).Should().BeTrue(key);
        }

        foreach (var (key, value) in defined.Where(member =>
                     member.Key is not ("schemaVersion" or "identifiers") && !member.Key.StartsWith('$') && !consumer.ContainsKey(member.Key)))
        {
            JsonNode.DeepEquals(written[key], value).Should().BeTrue(key);
        }
    }

    [Fact]
    public void ATemplateThatCannotBeReplaced_WhenUpgraded_ShouldNameTheFileOnStandardErrorAndExitThree()
    {
        // Given
        var root = Path.GetFullPath("repo");
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile(
            Path.Combine(root, ".specht/manifest.json"),
            new MockFileData(UpgradeServices.Shipped().At(Version("0.1.0")).Single(static file => file.Relative == ".specht/manifest.json").Bytes));
        fileSystem.AddFile(
            Path.Combine(root, ".specht/templates/feature.md"),
            new MockFileData("# A synthetic template with local edits\n") { Attributes = FileAttributes.ReadOnly });
        var errors = new StringWriter();
        var app = UpgradeServices.App(fileSystem, new TestConsole(), errors);

        // When
        var result = app.Run("upgrade", "--root", "repo");

        // Then
        result.ExitCode.Should().Be(3, result.Output);
        errors.ToString().Should().Contain(".specht/templates/feature.md: could not be replaced.").And.NotContain(root);
    }

    private static SemanticVersion Version(string text) =>
        SemanticVersion.TryParse(text, out var version) ? version : throw new FormatException(text);
}
