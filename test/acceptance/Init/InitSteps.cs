using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Reqnroll;
using Specht.Tool;
using Specht.Versioning;

namespace Specht.Acceptance.Init;

/// <summary>
/// Steps for <c>src/tool/Features/Init/.spec/init.feature</c> (0001-F4). B-004 compares the tool's embedded copies
/// of the newest version with this repository's live <c>.specht/manifest.json</c>, <c>.specht/schema/frontmatter/</c> and
/// <c>.specht/templates/</c> (<c>0001-F5</c> decision 0011), copied into the output; the live manifest is read and given a
/// rule setting in memory, which the comparison must ignore. B-007 and B-009 launch the built tool over a synthetic root
/// inside a temporary sandbox, from the sandbox so writing beside the root is seen, or from the root when no root is named.
/// B-007's sandbox holds no root: the root is typed relative to it, and the sandbox is snapshotted before the run. B-010 and
/// B-011's manifest is the embedded one with <c>schemaVersion</c> set to the newest version shipped. Item 0186 binds the
/// part selections: no option, schemas, templates, every part and schemas plus templates. Bare roots omit the epic schema;
/// beside the existing epic-declaring manifest, the schemas selection includes it.
/// </summary>
[Binding]
[Scope(Feature = "init")]
public sealed partial class InitSteps
{
    [Given("the tool is built from this repository at one commit")]
    public void GivenTheToolIsBuiltFromThisRepositoryAtOneCommit() =>
        Shipped.GetManifestResourceNames().Should().NotBeEmpty("the built tool embeds its shipping copy");

    [Given("this repository's manifest lowers one rule's severity")]
    public void GivenThisRepositorysManifestLowersOneRulesSeverity()
    {
        File.Exists(Live(Manifest)).Should().BeTrue($"this repository's live manifest is .specht/{Manifest}");
        _liveManifest = JsonNode.Parse(File.ReadAllText(Live(Manifest)))!.AsObject();
        _liveManifest["rules"] = new JsonObject { ["SPEC031"] = "warning" };
    }

    [Given("a root directory with no configuration folder")]
    [Given("a working directory with no configuration folder")]
    public void GivenARootDirectoryWithNoConfigurationFolder() => Prepare();

    [Given("two root directories with no configuration folder")]
    public void GivenTwoRootDirectoriesWithNoConfigurationFolder()
    {
        Prepare();
        Directory.CreateDirectory(SecondRoot);
    }

    [Given("no upstream schema source is recorded")]
    public void GivenNoUpstreamSchemaSourceIsRecorded() =>
        File.Exists(Written(Manifest)).Should().BeFalse("a root with no manifest records no upstream schema source");

    [Given("a root directory holding only the feature template")]
    public void GivenARootDirectoryHoldingOnlyTheFeatureTemplate()
    {
        Prepare();
        Directory.CreateDirectory(Path.GetDirectoryName(Written(FeatureTemplate))!);
        File.WriteAllText(Written(FeatureTemplate), "# A synthetic feature template with local edits\n");
    }

    [Given("a root directory holding a schema set and templates at the retired location and no configuration folder")]
    public void GivenARootDirectoryHoldingASchemaSetAndTemplatesAtTheRetiredLocationAndNoConfigurationFolder()
    {
        Prepare();
        foreach (var file in RetiredFiles)
        {
            var path = Path.Combine([Root, ".spec", .. file.Split('/')]);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, $"A synthetic {file} at the retired location\n");
        }
    }

    [Given("a root path that does not exist")]
    public void GivenARootPathThatDoesNotExist()
    {
        _sandbox = Path.Combine(Path.GetTempPath(), "specht-init-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Sandbox);
        File.WriteAllText(Path.Combine(Sandbox, "beside-the-root.txt"), "synthetic\n");
        _typed = "./no-such-root";
        Directory.Exists(Root).Should().BeFalse();
        _snapshot = Snapshot();
    }

    [Given("a root directory holding only a manifest pinning the newest version the tool ships and declaring the epic grammar")]
    public void GivenARootDirectoryHoldingOnlyAManifestPinningTheNewestVersionTheToolShipsAndDeclaringTheEpicGrammar()
    {
        Prepare();
        var version = NewestVersion();
        var embedded = Read(Embedded(Manifest, version));
        embedded.Should().NotBeNull($"the tool embeds {Embedded(Manifest, version)}");
        var manifest = JsonNode.Parse(embedded)!.AsObject();
        manifest["schemaVersion"] = SchemaVersions.Embedded.Versions[^1].Number.ToString();
        (manifest["identifiers"]?["epic"]).Should().NotBeNull("the manifest declares the epic grammar");
        _manifest = System.Text.Encoding.UTF8.GetBytes(manifest.ToJsonString());
        Directory.CreateDirectory(Path.GetDirectoryName(Written(Manifest))!);
        File.WriteAllBytes(Written(Manifest), _manifest);
    }

    [Given("a snapshot of every file under the root")]
    public void GivenASnapshotOfEveryFileUnderTheRoot() => _snapshot = Snapshot();

    [When("init runs against it")]
    public void WhenInitRunsAgainstIt() => (_stdout, _stderr, _exitCode) = AcceptanceTool.Launch(Sandbox, "init", "--root", _typed);

    [When("init runs against it with no option")]
    public void WhenInitRunsAgainstItWithNoOption() => WhenInitRunsAgainstIt();

    [When("init runs against it with schemas")]
    public void WhenInitRunsAgainstItWithSchemas() => Run("--schemas");

    [When("init runs against it with templates")]
    public void WhenInitRunsAgainstItWithTemplates() => Run("--templates");

    [When("init runs against it with every part")]
    public void WhenInitRunsAgainstItWithEveryPart() => Run("--all");

    [When("init runs against the first with every part")]
    public void WhenInitRunsAgainstTheFirstWithEveryPart() => Run("--all");

    [When("init runs against the second with schemas and templates")]
    public void WhenInitRunsAgainstTheSecondWithSchemasAndTemplates() =>
        (_stdout, _stderr, _exitCode) = AcceptanceTool.Launch(Sandbox, "init", "--root", _secondTyped, "--schemas", "--templates");

    [When("init runs there without naming a root")]
    public void WhenInitRunsThereWithoutNamingARoot() => (_stdout, _stderr, _exitCode) = AcceptanceTool.Launch(Root, "init");

    [When("its embedded copies of the newest version are compared with this repository's live manifest, frontmatter schemas and templates")]
    public void WhenItsEmbeddedCopiesOfTheNewestVersionAreComparedWithThisRepositorysLiveManifestFrontmatterSchemasAndTemplates()
    {
        var version = NewestVersion();
        _files = ToolOwnedFiles.ToDictionary(
            static file => file,
            file => (Read(Embedded(file, version)), File.Exists(Live(file)) ? File.ReadAllBytes(Live(file)) : null));
        var manifest = Read(Embedded(Manifest, version));
        _embeddedManifest = manifest is null ? null : JsonNode.Parse(manifest)!.AsObject();
    }

    [Then("the frontmatter schema folder holds the Feature, item and epic frontmatter schemas")]
    public void ThenTheFrontmatterSchemaFolderHoldsTheFeatureItemAndEpicFrontmatterSchemas() =>
        ToolOwnedFiles.Where(static file => file.StartsWith(FrontmatterSchemas, StringComparison.Ordinal))
            .Should()
            .HaveCount(3)
            .And.AllSatisfy(file => File.Exists(Written(file)).Should().BeTrue($"init writes .specht/{file}; {_stdout}{_stderr}"));

    [Then("the templates folder holds the four templates")]
    public void ThenTheTemplatesFolderHoldsTheFourTemplates() =>
        ToolOwnedFiles.Where(static file => file.StartsWith("templates/", StringComparison.Ordinal))
            .Should()
            .HaveCount(4)
            .And.AllSatisfy(file => File.Exists(Written(file)).Should().BeTrue($"init writes .specht/{file}; {_stdout}{_stderr}"));

    [Then("the only file written is the manifest")]
    public void ThenTheOnlyFileWrittenIsTheManifest() => ConfiguredFiles(Root).Should().Equal(Manifest);

    [Then("the files written are exactly the manifest and the Feature and item frontmatter schemas")]
    public void ThenTheFilesWrittenAreExactlyTheManifestAndTheFeatureAndItemFrontmatterSchemas() =>
        ConfiguredFiles(Root).Should().Equal(Manifest, FeatureSchema, ItemSchema);

    [Then("the files written are exactly the manifest and the four templates")]
    public void ThenTheFilesWrittenAreExactlyTheManifestAndTheFourTemplates() =>
        ConfiguredFiles(Root).Should().Equal([Manifest, .. Templates]);

    [Then("the two roots hold the same files with the same content")]
    public void ThenTheTwoRootsHoldTheSameFilesWithTheSameContent()
    {
        var first = ConfiguredSnapshot(Root);
        var second = ConfiguredSnapshot(SecondRoot);
        second.Keys.Should().Equal(first.Keys);
        foreach (var (path, bytes) in first)
        {
            second[path].Should().Equal(bytes, path);
        }
    }

    [Then("the manifest is written")]
    public void ThenTheManifestIsWritten() => File.Exists(Written(Manifest)).Should().BeTrue(_stdout + _stderr);

    [Then("the frontmatter schema folder holds the Feature and item frontmatter schemas")]
    public void ThenTheFrontmatterSchemaFolderHoldsTheFeatureAndItemFrontmatterSchemas() =>
        ConfiguredFiles(Root).Where(static file => file.StartsWith(FrontmatterSchemas, StringComparison.Ordinal))
            .Should()
            .Equal(FeatureSchema, ItemSchema);

    [Then("the frontmatter schema folder holds no epic frontmatter schema")]
    public void ThenTheFrontmatterSchemaFolderHoldsNoEpicFrontmatterSchema() =>
        File.Exists(Written(EpicSchema)).Should().BeFalse(_stdout + _stderr);

    [Then("the exit code is {int}")]
    public void ThenTheExitCodeIs(int code) => _exitCode.Should().Be(code, _stdout + _stderr);

    [Then("each written file other than the manifest is byte-identical to the tool's embedded copy")]
    public void ThenEachWrittenFileOtherThanTheManifestIsByteIdenticalToTheToolsEmbeddedCopy()
    {
        var version = NewestVersion();
        var written = ToolOwnedFiles.Where(file => File.Exists(Written(file))).ToList();
        written.Should().NotBeEmpty($"init wrote a file other than the manifest; {_stdout}{_stderr}");
        foreach (var file in written)
        {
            File.ReadAllBytes(Written(file)).Should().Equal(Read(Embedded(file, version)), $"init writes .specht/{file} from the embedded copy");
        }
    }

    [Then(
        "each frontmatter schema written carries an id made of this repository's schema address, the version written, "
        + "the frontmatter folder and its own file name")]
    public void ThenEachFrontmatterSchemaWrittenCarriesAnId()
    {
        var version = NewestVersion();
        var written = ToolOwnedFiles
            .Where(static file => file.StartsWith(FrontmatterSchemas, StringComparison.Ordinal))
            .Where(file => File.Exists(Written(file)))
            .ToList();
        written.Should().NotBeEmpty($"init wrote a frontmatter schema; {_stdout}{_stderr}");
        foreach (var file in written)
        {
            JsonNode.Parse(File.ReadAllText(Written(file)))!["$id"]!.GetValue<string>()
                .Should()
                .Be($"https://github.com/rlittlesii/specht/schema/v{version}/frontmatter/{Path.GetFileName(file)}");
        }
    }

    [Then("the standard output lists the manifest, each frontmatter schema and each template relative to the root")]
    public void ThenTheStandardOutputListsTheManifestEachFrontmatterSchemaAndEachTemplateRelativeToTheRoot()
    {
        foreach (var file in FilesWithoutEpics)
        {
            Listing(file).Should().ContainSingle($"stdout lists .specht/{file} once; {_stdout}{_stderr}");
        }

        _stdout.Should().NotContain(Sandbox).And.NotContain("\\");
    }

    [Then("marks the feature template as skipped and the rest as written")]
    public void ThenMarksTheFeatureTemplateAsSkippedAndTheRestAsWritten()
    {
        foreach (var file in FilesWithoutEpics)
        {
            var (marker, other) = file == FeatureTemplate ? ("skipped", "written") : ("written", "skipped");
            Listing(file).Should().ContainSingle(_stdout).Which.Should().Contain(marker).And.NotContain(other);
        }
    }

    [Then("the standard output lists the manifest and no other file")]
    public void ThenTheStandardOutputListsTheManifestAndNoOtherFile() =>
        _stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Should()
            .Equal($"written .specht/{Manifest}");

    [Then("the only files created are the manifest and files under the frontmatter schema folder and the templates folder")]
    public void ThenTheOnlyFilesCreatedAreTheManifestAndFilesUnderTheFrontmatterSchemaFolderAndTheTemplatesFolder()
    {
        var before = _snapshot ?? throw new InvalidOperationException("No snapshot was taken.");
        var created = Snapshot().Keys.Except(before.Keys).ToList();
        created.Should().NotBeEmpty($"init ran against a bare root; {_stdout}{_stderr}");
        created.Should().AllSatisfy(static path =>
            path.Should().Match(static path => path == $"repo/.specht/{Manifest}"
                                               || path.StartsWith($"repo/.specht/{FrontmatterSchemas}", StringComparison.Ordinal)
                                               || path.StartsWith("repo/.specht/templates/", StringComparison.Ordinal)));
    }

    [Then("no file at the retired location was created, modified or deleted")]
    public void ThenNoFileAtTheRetiredLocationWasCreatedModifiedOrDeleted()
    {
        var before = UnderDotSpec(_snapshot ?? throw new InvalidOperationException("No snapshot was taken."));
        var after = UnderDotSpec(Snapshot());
        after.Keys.Should().BeEquivalentTo(before.Keys, _stdout + _stderr);
        foreach (var (path, bytes) in before)
        {
            after[path].Should().Equal(bytes, $"init leaves {path} as it was");
        }
    }

    [Then("no other file was modified")]
    public void ThenNoOtherFileWasModified()
    {
        var before = _snapshot ?? throw new InvalidOperationException("No snapshot was taken.");
        var after = Snapshot();
        foreach (var (path, bytes) in before)
        {
            after.Should().ContainKey(path);
            after[path].Should().Equal(bytes, $"init leaves {path} as it was");
        }
    }

    [Then("the standard error names that path exactly as it was given")]
    public void ThenTheStandardErrorNamesThatPathExactlyAsItWasGiven() =>
        _stderr.Should().Contain(_typed).And.NotContain(Path.GetFileName(Sandbox));

    [Then("nothing is written")]
    public void ThenNothingIsWritten()
    {
        var before = _snapshot ?? throw new InvalidOperationException("No snapshot was taken.");
        var after = Snapshot();
        after.Keys.Should().BeEquivalentTo(before.Keys, _stdout + _stderr);
        foreach (var (path, bytes) in before)
        {
            after[path].Should().Equal(bytes, $"init leaves {path} as it was");
        }

        Directory.Exists(Root).Should().BeFalse($"init never creates the root; {_stdout}{_stderr}");
    }

    [Then("the three frontmatter schemas and the four templates are written")]
    public void ThenTheThreeFrontmatterSchemasAndTheFourTemplatesAreWritten()
    {
        var version = NewestVersion();
        foreach (var file in ToolOwnedFiles)
        {
            File.Exists(Written(file)).Should().BeTrue($"init writes .specht/{file}; {_stdout}{_stderr}");
            File.ReadAllBytes(Written(file)).Should().Equal(Read(Embedded(file, version)), $"init writes .specht/{file} from the embedded copy");
        }

        File.ReadAllBytes(Written(Manifest)).Should().Equal(_manifest, "init leaves the manifest that was there as it was");
    }

    [Then("the manifest is written under the working directory")]
    public void ThenTheManifestIsWrittenUnderTheWorkingDirectory() =>
        File.Exists(Written(Manifest)).Should().BeTrue($"init writes .specht/{Manifest} under the working directory; {_stdout}{_stderr}");

    [Then("each embedded frontmatter schema and template is byte-identical to its file")]
    public void ThenEachEmbeddedFrontmatterSchemaAndTemplateIsByteIdenticalToItsFile()
    {
        foreach (var (file, (embedded, live)) in Files)
        {
            embedded.Should().NotBeNull($"the tool embeds {file} at the newest version");
            live.Should().NotBeNull($"this repository's live copy holds .specht/{file}");
            embedded.Should().Equal(live, $"the embedded {file} ships the bytes of .specht/{file}");
        }
    }

    [Then("the embedded manifest equals the live manifest on every tool-owned key")]
    public void ThenTheEmbeddedManifestEqualsTheLiveManifestOnEveryToolOwnedKey()
    {
        _embeddedManifest.Should().NotBeNull($"the tool embeds {Manifest} at the newest version");
        var live = _liveManifest ?? throw new InvalidOperationException("No live manifest was read.");
        foreach (var (key, value) in _embeddedManifest!)
        {
            live.ContainsKey(key).Should().BeTrue($".specht/{Manifest} carries the tool-owned key {key}");
            JsonNode.DeepEquals(value, live[key]).Should().BeTrue($"the embedded and live manifests agree on the tool-owned key {key}");
        }
    }

    [AfterScenario]
    public void DeleteSandbox()
    {
        if (_sandbox is not null && Directory.Exists(_sandbox))
        {
            Directory.Delete(_sandbox, recursive: true);
        }
    }

    private static Assembly Shipped => typeof(ExitCodes).Assembly;

    private static string[] FilesWithoutEpics => [Manifest, FeatureSchema, ItemSchema, .. Templates];

    private string Sandbox => _sandbox ?? throw new InvalidOperationException("No root was prepared.");

    private string Root => Path.Combine(Sandbox, _typed);

    private string SecondRoot => Path.Combine(Sandbox, _secondTyped);

    private Dictionary<string, (byte[]? Embedded, byte[]? Live)> Files =>
        _files ?? throw new InvalidOperationException("Nothing was compared.");

    private static string Live(string file) => Path.Combine([AppContext.BaseDirectory, ".specht", .. file.Split('/')]);

    private static string Embedded(string file, SemanticVersion version)
    {
        var folder = file.IndexOf('/', StringComparison.Ordinal);

        return folder < 0 ? $"manifest/v{version}/{file}" : $"{file[..folder]}/v{version}{file[folder..]}";
    }

    [GeneratedRegex("^schema/v([0-9]+\\.[0-9]+\\.[0-9]+)/")]
    private static partial Regex SchemaVersion();

    private static byte[]? Read(string name)
    {
        using var stream = Shipped.GetManifestResourceStream(name);
        if (stream is null)
        {
            return null;
        }

        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        return buffer.ToArray();
    }

    private static SemanticVersion NewestVersion()
    {
        var versions = Shipped.GetManifestResourceNames()
            .Select(static name => SchemaVersion().Match(name))
            .Where(static match => match.Success)
            .Select(static match => SemanticVersion.TryParse(match.Groups[1].Value, out var version)
                ? version
                : throw new InvalidOperationException("Invalid version folder."))
            .ToList();
        versions.Should().NotBeEmpty("the tool embeds at least one schema/v<major.minor.patch>/ resource");
        return versions.Max();
    }

    private static Dictionary<string, byte[]> UnderDotSpec(Dictionary<string, byte[]> snapshot) =>
        snapshot.Where(static entry => entry.Key.StartsWith("repo/.spec/", StringComparison.Ordinal))
            .ToDictionary(static entry => entry.Key, static entry => entry.Value, StringComparer.Ordinal);

    private static string[] ConfiguredFiles(string root) =>
        Directory.EnumerateFiles(Path.Combine(root, ".specht"), "*", SearchOption.AllDirectories)
            .Select(file => Path.GetRelativePath(Path.Combine(root, ".specht"), file).Replace(Path.DirectorySeparatorChar, '/'))
            .Order(StringComparer.Ordinal)
            .ToArray();

    private static Dictionary<string, byte[]> ConfiguredSnapshot(string root) =>
        ConfiguredFiles(root).ToDictionary(
            static file => file,
            file => File.ReadAllBytes(Path.Combine([root, ".specht", .. file.Split('/')])),
            StringComparer.Ordinal);

    private void Prepare()
    {
        _sandbox = Path.Combine(Path.GetTempPath(), "specht-init-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(Root, ".spec"));
        File.WriteAllText(Path.Combine(Sandbox, "beside-the-root.txt"), "synthetic\n");
        File.WriteAllText(Path.Combine(Root, "README.md"), "# A synthetic repository\n");
        File.WriteAllText(Path.Combine(Root, ".spec", "README.md"), "# A synthetic specification\n");
    }

    private string Written(string file) => Path.Combine([Root, ".specht", .. file.Split('/')]);

    private void Run(params string[] options)
    {
        var arguments = new[] { "init", "--root", _typed }.Concat(options).ToArray();
        (_stdout, _stderr, _exitCode) = AcceptanceTool.Launch(Sandbox, arguments);
    }

    private string[] Listing(string file) =>
        _stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(line => line.Contains($".specht/{file}", StringComparison.Ordinal))
            .ToArray();

    private Dictionary<string, byte[]> Snapshot() =>
        Directory.EnumerateFiles(Sandbox, "*", SearchOption.AllDirectories)
            .ToDictionary(
                path => Path.GetRelativePath(Sandbox, path).Replace(Path.DirectorySeparatorChar, '/'),
                File.ReadAllBytes,
                StringComparer.Ordinal);

    private const string Manifest = "manifest.json";

    private const string FrontmatterSchemas = "schema/frontmatter/";

    private const string FeatureTemplate = "templates/feature.md";

    private const string FeatureSchema = FrontmatterSchemas + "feature.schema.json";

    private const string ItemSchema = FrontmatterSchemas + "task.schema.json";

    private const string EpicSchema = FrontmatterSchemas + "epic.schema.json";

    private static readonly string[] Templates =
    [
        "templates/adr.md",
        "templates/decision.md",
        "templates/feature.md",
        "templates/lesson.md",
    ];

    private static readonly string[] ToolOwnedFiles =
    [
        FrontmatterSchemas + "feature.schema.json",
        FrontmatterSchemas + "task.schema.json",
        FrontmatterSchemas + "epic.schema.json",
        "templates/feature.md",
        "templates/decision.md",
        "templates/adr.md",
        "templates/lesson.md",
    ];

    private static readonly string[] RetiredFiles =
    [
        "schema/spec-structure.schema.json",
        "schema/feature-spec.frontmatter.schema.json",
        "schema/task.frontmatter.schema.json",
        "schema/epic.frontmatter.schema.json",
        "templates/feature.md",
        "templates/decision.md",
        "templates/adr.md",
        "templates/lesson.md",
    ];

    private readonly string _secondTyped = "repo-2";
    private JsonObject? _liveManifest;
    private Dictionary<string, (byte[]? Embedded, byte[]? Live)>? _files;
    private JsonObject? _embeddedManifest;
    private string? _sandbox;
    private Dictionary<string, byte[]>? _snapshot;
    private byte[]? _manifest;
    private string _typed = "repo";
    private string _stdout = string.Empty;
    private string _stderr = string.Empty;
    private int _exitCode = -1;
}
