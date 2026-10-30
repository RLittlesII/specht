using System.IO.Abstractions.TestingHelpers;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Specht.Acceptance;
using Specht.Tests.Shared;
using Specht.Tool.Features;
using Specht.Versioning;
using Spectre.Console.Cli.Testing;

namespace Specht.Tests.Upgrade;

/// <summary>
/// <c>specht upgrade</c> through Spectre's command tester over an in-memory file system and the shipped files (<c>0001-F7</c>
/// B-005, B-006, B-007, B-016, B-017, B-018, B-019, B-020, B-052, B-059): a root's frontmatter schemas and templates are
/// brought to the pin from an older or a newer version, the keys the pin defines are added to the manifest and the
/// consumer's settings kept, a root already at its pin prints that it is current and rewrites nothing, only the manifest,
/// the frontmatter schema folder and the templates folder may change, a manifest that lacks no key is never rewritten, a
/// rewrite prints the pinned version and each rewritten file, and a missing root or manifest exits <c>2</c> and an invalid
/// manifest <c>3</c>, writing nothing and naming no absolute path. Under a recorded upstream schema source whose fetch the
/// synthetic source refuses - content failing its hash, an unreachable source, or a 401 or 404 - the command names the
/// source on standard error, writes nothing and exits <c>3</c> (B-027, B-035), and so for a refused fetch of a template that
/// source hashes (B-132, B-133). Under a pin the check rejects (B-021) the built tool is launched for both commands over a root
/// on disk, since the check writes its standard error to the process's own stream, and the upgrade's standard error is the
/// check's byte for byte. Under the on-disk source with no recorded source, one skip line per present shipped frontmatter
/// schema follows the lines naming the rewritten files (B-030), or the current line when nothing is rewritten, and the run
/// exits <c>0</c> (B-142). A template the manifest names in <c>consumerTemplates</c> and the pinned version ships gets one
/// skip line, edited, identical or absent, after the lines naming the rewritten files or the current line, in one ordinal
/// listing with the on-disk schema skips, and the run exits <c>0</c> (B-006, B-061, B-062, B-142); a name the pinned version
/// does not ship gives the output and exit code of the same root without the name (B-144).
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
        ShippedFiles shipped = new ShippedFilesFixture();
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile(Path.Combine(root, ".specht/manifest.json"), new MockFileData($"{{\"schemaVersion\":\"{pin}\"}}"));
        foreach (var file in shipped.At(Version(files)).Where(static file => file.Relative != ".specht/manifest.json"))
        {
            fileSystem.AddFile(Path.Combine(root, file.Relative), new MockFileData(file.Bytes));
        }

        var edited = Path.Combine(root, ".specht/templates/feature.md");
        fileSystem.File.WriteAllText(edited, "# A synthetic template with local edits\n");
        CommandAppTester app = new UpgradeAppFixture().WithFileSystem(fileSystem);

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
        ShippedFiles shipped = new ShippedFilesFixture();
        var fileSystem = new MockFileSystem();
        foreach (var file in shipped.At(Version(pin)))
        {
            fileSystem.AddFile(Path.Combine(root, file.Relative), new MockFileData(file.Bytes) { LastWriteTime = stamp });
        }

        var before = fileSystem.AllFiles.ToDictionary(
            static path => path, path => (fileSystem.File.ReadAllBytes(path), fileSystem.File.GetLastWriteTimeUtc(path)));
        CommandAppTester app = new UpgradeAppFixture().WithFileSystem(fileSystem);

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
        ShippedFiles shipped = new ShippedFilesFixture();
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
        CommandAppTester app = new UpgradeAppFixture().WithFileSystem(fileSystem);

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
            _shipped.At(Version(pin)).Single(static file => file.Relative == ".specht/manifest.json").Bytes)!.AsObject();
        defined["schemaVersion"] = pin;
        defined["rules"] = new JsonObject { ["SPEC031"] = "warning" };
        var manifest = Encoding.UTF8.GetBytes(
            defined.ToJsonString(new JsonSerializerOptions { WriteIndented = true }).ReplaceLineEndings("\r\n") + "\r\n");
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile(Path.Combine(root, ".specht/manifest.json"), new MockFileData(manifest));
        foreach (var file in _shipped.At(Version(files)).Where(static file => file.Relative != ".specht/manifest.json"))
        {
            fileSystem.AddFile(Path.Combine(root, file.Relative), new MockFileData(file.Bytes));
        }

        CommandAppTester app = new UpgradeAppFixture().WithFileSystem(fileSystem);

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
        ShippedFiles shipped = new ShippedFilesFixture();
        var present = shipped.At(Version(files)).Where(static file => file.Relative != ".specht/manifest.json").ToList();
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile(Path.Combine(root, ".specht/manifest.json"), new MockFileData($"{{\"schemaVersion\":\"{pin}\"}}"));
        foreach (var file in present)
        {
            fileSystem.AddFile(Path.Combine(root, file.Relative), new MockFileData(file.Bytes));
        }

        CommandAppTester app = new UpgradeAppFixture().WithFileSystem(fileSystem);

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
        foreach (var file in _shipped.At(Version("0.2.0")))
        {
            fileSystem.AddFile(Path.Combine(root, file.Relative), new MockFileData(file.Bytes));
        }

        var manifest = JsonNode.Parse(fileSystem.File.ReadAllBytes(Path.Combine(root, ".specht/manifest.json")))!.AsObject();
        manifest.Remove("claim").Should().BeTrue();
        fileSystem.File.WriteAllText(Path.Combine(root, ".specht/manifest.json"), manifest.ToJsonString());
        CommandAppTester app = new UpgradeAppFixture().WithFileSystem(fileSystem);

        // When
        var result = app.Run("upgrade", "--root", "repo");

        // Then
        result.ExitCode.Should().Be(0, result.Output);
        result.Output.Split('\n').Should().Equal(".specht/ rewritten to schemaVersion 0.2.0", "rewritten .specht/manifest.json");
    }

    [Theory]
    [ClassData(typeof(UpgradeSkipListings))]
    public void SchemasOfTheConsumersUnderTheOnDiskSource_WhenUpgraded_ShouldPrintTheCurrentOrRewrittenLinesThenOneSkipLinePerSchemaAndExitZero(
        string edited, string[] lines)
    {
        // Given
        var root = Path.GetFullPath("repo");
        var shipped = _shipped.At(Version("0.2.0"));
        var manifest = JsonNode.Parse(shipped.Single(static file => file.Relative == ".specht/manifest.json").Bytes)!.AsObject();
        manifest["schemaSource"] = "disk";
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile(Path.Combine(root, ".specht/manifest.json"), new MockFileData(manifest.ToJsonString()));
        foreach (var file in shipped.Where(static file => file.Relative != ".specht/manifest.json"))
        {
            fileSystem.AddFile(
                Path.Combine(root, file.Relative),
                file.Relative.StartsWith(".specht/schema/frontmatter/", StringComparison.Ordinal)
                    ? new MockFileData("{\"title\":\"A synthetic consumer schema\"}\n")
                    : new MockFileData(file.Bytes));
        }

        if (edited.Length > 0)
        {
            fileSystem.File.WriteAllText(Path.Combine(root, ".specht/templates", edited), "# A synthetic template with local edits\n");
        }

        CommandAppTester app = new UpgradeAppFixture().WithFileSystem(fileSystem);

        // When
        var result = app.Run("upgrade", "--root", "repo");

        // Then
        result.ExitCode.Should().Be(0, result.Output);
        result.Output.Split('\n').Should().Equal(lines);
    }

    [Fact]
    public void ConsumerSettingsAndADefinedKeyWithTheConsumersValue_WhenUpgraded_ShouldKeepThemAndAddEveryOtherDefinedKey()
    {
        // Given
        var root = Path.GetFullPath("repo");
        ShippedFiles shipped = new ShippedFilesFixture();
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

        CommandAppTester app = new UpgradeAppFixture().WithFileSystem(fileSystem);

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
            new MockFileData(_shipped.At(Version("0.1.0")).Single(static file => file.Relative == ".specht/manifest.json").Bytes));
        fileSystem.AddFile(
            Path.Combine(root, ".specht/templates/feature.md"),
            new MockFileData("# A synthetic template with local edits\n") { Attributes = FileAttributes.ReadOnly });
        var errors = new StringWriter();
        CommandAppTester app = new UpgradeAppFixture().WithFileSystem(fileSystem).WithErrors(errors);

        // When
        var result = app.Run("upgrade", "--root", "repo");

        // Then
        result.ExitCode.Should().Be(3, result.Output);
        errors.ToString().Should().Contain(".specht/templates/feature.md: could not be replaced.").And.NotContain(root);
    }

    [Theory]
    [ClassData(typeof(UpgradeMissingInputs))]
    public void AMissingRootOrManifest_WhenUpgraded_ShouldWriteNothingAndExitTwoWithNoAbsolutePathOnStandardError(string typedRoot, bool manifest, Type fault)
    {
        // Given
        var root = Path.GetFullPath("repo");
        var stamp = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var fileSystem = new MockFileSystem();
        if (manifest)
        {
            fileSystem.AddFile(Path.Combine(root, ".specht/manifest.json"), new MockFileData("{\"schemaVersion\":\"0.2.0\"}") { LastWriteTime = stamp });
        }

        foreach (var file in _shipped.At(Version("0.1.0")).Where(static file => file.Relative != ".specht/manifest.json"))
        {
            fileSystem.AddFile(Path.Combine(root, file.Relative), new MockFileData(file.Bytes) { LastWriteTime = stamp });
        }

        var before = fileSystem.AllFiles.ToDictionary(
            static path => path, path => (fileSystem.File.ReadAllBytes(path), fileSystem.File.GetLastWriteTimeUtc(path)));
        var errors = new StringWriter();
        CommandAppTester app = new UpgradeAppFixture().WithFileSystem(fileSystem).WithErrors(errors);

        // When
        var result = app.Run("upgrade", "--root", Path.Combine("repo", typedRoot));

        // Then
        result.ExitCode.Should().Be(2, $"{fault.Name}: {errors}");
        fileSystem.AllFiles.Should().BeEquivalentTo(before.Keys);
        foreach (var (path, (bytes, written)) in before)
        {
            fileSystem.File.ReadAllBytes(path).Should().Equal(bytes);
            fileSystem.File.GetLastWriteTimeUtc(path).Should().Be(written, path);
        }

        errors.ToString().Should().NotContain(root);
    }

    [Theory]
    [ClassData(typeof(UpgradeInvalidManifests))]
    public void AnInvalidManifest_WhenUpgraded_ShouldWriteNothingAndExitThreeWithNoAbsolutePathOnStandardError(string manifest, Type fault)
    {
        // Given
        var root = Path.GetFullPath("repo");
        var stamp = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile(Path.Combine(root, ".specht/manifest.json"), new MockFileData(manifest) { LastWriteTime = stamp });
        foreach (var file in _shipped.At(Version("0.1.0")).Where(static file => file.Relative != ".specht/manifest.json"))
        {
            fileSystem.AddFile(Path.Combine(root, file.Relative), new MockFileData(file.Bytes) { LastWriteTime = stamp });
        }

        var before = fileSystem.AllFiles.ToDictionary(
            static path => path, path => (fileSystem.File.ReadAllBytes(path), fileSystem.File.GetLastWriteTimeUtc(path)));
        var errors = new StringWriter();
        CommandAppTester app = new UpgradeAppFixture().WithFileSystem(fileSystem).WithErrors(errors);

        // When
        var result = app.Run("upgrade", "--root", "repo");

        // Then
        result.ExitCode.Should().Be(3, $"{fault.Name}: {errors}");
        fileSystem.AllFiles.Should().BeEquivalentTo(before.Keys);
        foreach (var (path, (bytes, written)) in before)
        {
            fileSystem.File.ReadAllBytes(path).Should().Equal(bytes);
            fileSystem.File.GetLastWriteTimeUtc(path).Should().Be(written, path);
        }

        errors.ToString().Should().NotContain(root);
    }

    [Theory]
    [ClassData(typeof(UpgradeRejectedPins))]
    public void APinTheCheckRejects_WhenUpgraded_ShouldPrintTheChecksStandardErrorWriteNothingAndExitThree(string schemaVersion)
    {
        // Given
        using var tree = new SpecTree();
        tree.WriteRaw(SpecTree.ManifestPath, $"{{\"schemaVersion\":{schemaVersion}}}");
        var check = AcceptanceTool.Launch(tree.Root);
        var before = Directory.GetFiles(tree.Root, "*", SearchOption.AllDirectories)
            .ToDictionary(path => Path.GetRelativePath(tree.Root, path), static path => File.ReadAllBytes(path));

        // When
        var result = AcceptanceTool.Launch(tree.Root, "upgrade");

        // Then
        check.ExitCode.Should().Be(3, check.Stderr);
        result.ExitCode.Should().Be(3, result.Stderr);
        result.Stderr.Should().NotBeEmpty().And.Be(check.Stderr);
        Directory.GetFiles(tree.Root, "*", SearchOption.AllDirectories)
            .ToDictionary(path => Path.GetRelativePath(tree.Root, path), static path => File.ReadAllBytes(path))
            .Should().BeEquivalentTo(before);
    }

    [Theory]
    [ClassData(typeof(UpstreamFetchRefusals))]
    public void ARefusedFetch_WhenUpgraded_ShouldNameTheSourceOnStandardErrorWriteNothingAndExitThree(
        string manifest, string served, string refused, string failure)
    {
        // Given
        var root = Path.GetFullPath("repo");
        var content = JsonNode.Parse(served)!.AsObject()
            .ToDictionary(static pair => pair.Key, static pair => Encoding.UTF8.GetBytes(pair.Value!.GetValue<string>()), StringComparer.Ordinal);
        var stamp = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile(Path.Combine(root, ".specht/manifest.json"), new MockFileData(manifest) { LastWriteTime = stamp });
        foreach (var name in content.Keys.Where(static name => name.EndsWith(".schema.json", StringComparison.Ordinal)))
        {
            fileSystem.AddFile(
                Path.Combine(root, ".specht/schema/frontmatter", name), new MockFileData("{\"title\":\"A synthetic local copy\"}\n") { LastWriteTime = stamp });
        }

        fileSystem.AddFile(
            Path.Combine(root, ".specht/templates/feature.md"), new MockFileData("# A synthetic template with local edits\n") { LastWriteTime = stamp });
        var before = fileSystem.AllFiles.ToDictionary(
            static path => path, path => (fileSystem.File.ReadAllBytes(path), fileSystem.File.GetLastWriteTimeUtc(path)));
        using var source = new HttpClient(new SyntheticSource(request =>
        {
            var name = request.RequestUri!.Segments[^1];
            return name != refused
                ? new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(content[name]) }
                : failure switch
                {
                    "mismatch" => new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent("{}"u8.ToArray()) },
                    "unreachable" => throw new HttpRequestException("Synthetic source is unreachable."),
                    _ => new HttpResponseMessage((HttpStatusCode)int.Parse(failure, System.Globalization.CultureInfo.InvariantCulture)),
                };
        }));
        var errors = new StringWriter();
        CommandAppTester app = new UpgradeAppFixture().WithFileSystem(fileSystem).WithErrors(errors).WithSource(source);

        // When
        var result = app.Run("upgrade", "--root", "repo");

        // Then
        result.ExitCode.Should().Be(3, $"{refused} {failure}: {errors}{result.Output}");
        errors.ToString().Should().Contain("https://schemas.example/specht/").And.NotContain(root);
        fileSystem.AllFiles.Should().BeEquivalentTo(before.Keys);
        foreach (var (path, (bytes, written)) in before)
        {
            fileSystem.File.ReadAllBytes(path).Should().Equal(bytes, path);
            fileSystem.File.GetLastWriteTimeUtc(path).Should().Be(written, path);
        }
    }

    [Theory]
    [ClassData(typeof(ConsumerTemplateListings))]
    public void TemplatesTheManifestNamesAsTheConsumers_WhenUpgraded_ShouldPrintTheCurrentOrRewrittenLinesThenOneSkipLinePerSkippedFileAndExitZero(
        string manifest, string[] edited, string[] absent, string[] lines)
    {
        // Given
        var root = Path.GetFullPath("repo");
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile(Path.Combine(root, ".specht/manifest.json"), new MockFileData(manifest));
        foreach (var file in _shipped.At(Version("0.2.0"))
                     .Where(file => file.Relative != ".specht/manifest.json" && !absent.Any(name => file.Relative == ".specht/templates/" + name)))
        {
            fileSystem.AddFile(Path.Combine(root, file.Relative), new MockFileData(file.Bytes));
        }

        foreach (var name in edited)
        {
            fileSystem.File.WriteAllText(Path.Combine(root, ".specht/templates", name), "# A synthetic template with local edits\n");
        }

        CommandAppTester app = new UpgradeAppFixture().WithFileSystem(fileSystem);

        // When
        var result = app.Run("upgrade", "--root", "repo");

        // Then
        result.ExitCode.Should().Be(0, result.Output);
        result.Output.Split('\n').Should().Equal(lines);
    }

    [Theory]
    [ClassData(typeof(UnshippedTemplateNames))]
    public void ATemplateNameThePinnedVersionDoesNotShip_WhenUpgraded_ShouldPrintAndExitAsTheSameRootWithoutTheName(string manifest, string name)
    {
        // Given
        var root = Path.GetFullPath("repo");
        var named = JsonNode.Parse(manifest)!.AsObject();
        named["consumerTemplates"] = new JsonArray(name);
        var withName = new MockFileSystem();
        var without = new MockFileSystem();
        foreach (var (fileSystem, text) in new[] { (withName, named.ToJsonString()), (without, manifest) })
        {
            fileSystem.AddFile(Path.Combine(root, ".specht/manifest.json"), new MockFileData(text));
            foreach (var file in _shipped.At(Version("0.1.0")).Where(static file => file.Relative != ".specht/manifest.json"))
            {
                fileSystem.AddFile(Path.Combine(root, file.Relative), new MockFileData(file.Bytes));
            }

            fileSystem.File.WriteAllText(Path.Combine(root, ".specht/templates/feature.md"), "# A synthetic template with local edits\n");
            fileSystem.File.WriteAllText(Path.Combine(root, ".specht/templates/runbook.md"), "# A synthetic runbook template of the consumer's\n");
        }

        // When
        CommandAppTester withoutApp = new UpgradeAppFixture().WithFileSystem(without);
        var expected = withoutApp.Run("upgrade", "--root", "repo");
        CommandAppTester withNameApp = new UpgradeAppFixture().WithFileSystem(withName);
        var result = withNameApp.Run("upgrade", "--root", "repo");

        // Then
        expected.Output.Should().Contain("rewritten .specht/templates/feature.md");
        result.Output.Should().Be(expected.Output);
        result.ExitCode.Should().Be(expected.ExitCode);
    }

    private static SemanticVersion Version(string text) =>
        SemanticVersion.TryParse(text, out var version) ? version : throw new FormatException(text);

    private sealed class SyntheticSource(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(respond(request));
    }

    private readonly ShippedFiles _shipped = new ShippedFilesFixture();
}
