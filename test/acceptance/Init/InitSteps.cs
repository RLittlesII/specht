using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Reqnroll;
using Specht.Tool;

namespace Specht.Acceptance.Init;

/// <summary>
/// Steps for <c>src/tool/Features/Init/.spec/init.feature</c> (0001-F4). B-004 compares the tool's embedded copies
/// of the newest version with this repository's live <c>.spec/schema/</c> and <c>.spec/templates/</c>, copied into the
/// output; the live manifest is read and given a rule setting in memory, which the comparison must ignore. B-005, B-006,
/// B-008, B-009, B-010 and B-011 launch the built tool over a synthetic root inside a temporary sandbox, from the sandbox
/// so writing beside the root is seen, or from the root when no root is named. B-007's sandbox holds no root: the root is
/// typed relative to it, and the sandbox is snapshotted before the run. B-010 and B-011's manifest is the embedded one
/// with <c>schemaVersion</c> set to the newest version shipped.
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
        _liveManifest = JsonNode.Parse(File.ReadAllText(Live(Manifest)))!.AsObject();
        _liveManifest["rules"] = new JsonObject { ["SPEC031"] = "warning" };
    }

    [Given("a root directory with no schema folder and no templates folder")]
    [Given("a root directory with no schema folder")]
    [Given("a working directory with no schema folder and no templates folder")]
    public void GivenARootDirectoryWithNoSchemaFolderAndNoTemplatesFolder() => Prepare();

    [Given("no upstream schema source is recorded")]
    public void GivenNoUpstreamSchemaSourceIsRecorded() =>
        File.Exists(Written(Manifest)).Should().BeFalse("a root with no manifest records no upstream schema source");

    [Given("a root directory whose templates folder already holds the feature template")]
    public void GivenARootDirectoryWhoseTemplatesFolderAlreadyHoldsTheFeatureTemplate()
    {
        Prepare();
        Directory.CreateDirectory(Path.GetDirectoryName(Written(FeatureTemplate))!);
        File.WriteAllText(Written(FeatureTemplate), "# A synthetic feature template with local edits\n");
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

    [Given("a root directory whose schema folder holds a manifest pinning the newest version the tool ships and declaring the epic grammar")]
    public void GivenARootDirectoryWhoseSchemaFolderHoldsAManifestPinningTheNewestVersionTheToolShipsAndDeclaringTheEpicGrammar()
    {
        Prepare();
        var version = NewestVersion();
        var manifest = JsonNode.Parse(Read(Embedded(Manifest, version)))!.AsObject();
        manifest["schemaVersion"] = SchemaVersions.Embedded.Versions[^1].Number.ToString();
        (manifest["identifiers"]?["epic"]).Should().NotBeNull("the manifest declares the epic grammar");
        _manifest = System.Text.Encoding.UTF8.GetBytes(manifest.ToJsonString());
        Directory.CreateDirectory(Path.GetDirectoryName(Written(Manifest))!);
        File.WriteAllBytes(Written(Manifest), _manifest);
    }

    [Given("no other schema or template file beside it")]
    public void GivenNoOtherSchemaOrTemplateFileBesideIt() =>
        Snapshot().Keys
            .Where(static path => path.StartsWith("repo/.spec/schema/", StringComparison.Ordinal)
                                  || path.StartsWith("repo/.spec/templates/", StringComparison.Ordinal))
            .Should()
            .Equal($"repo/.spec/{Manifest}");

    [Given("a snapshot of every file under the root")]
    public void GivenASnapshotOfEveryFileUnderTheRoot() => _snapshot = Snapshot();

    [When("init runs against it")]
    public void WhenInitRunsAgainstIt() => (_stdout, _stderr, _exitCode) = AcceptanceTool.Launch(Sandbox, "init", "--root", _typed);

    [When("init runs there without naming a root")]
    public void WhenInitRunsThereWithoutNamingARoot() => (_stdout, _stderr, _exitCode) = AcceptanceTool.Launch(Root, "init");

    [When("its embedded copies of the newest version are compared with the files under this repository's schema and templates folders")]
    public void WhenItsEmbeddedCopiesOfTheNewestVersionAreComparedWithTheFilesUnderThisRepositorysSchemaAndTemplatesFolders()
    {
        var version = NewestVersion();
        _files = ToolOwnedFiles.ToDictionary(
            static file => file,
            file => (Read(Embedded(file, version)), File.ReadAllBytes(Live(file))));
        var manifest = Read(Embedded(Manifest, version));
        _embeddedManifest = manifest is null ? null : JsonNode.Parse(manifest)!.AsObject();
    }

    [Then("the schema folder holds the manifest and the three frontmatter schemas")]
    public void ThenTheSchemaFolderHoldsTheManifestAndTheThreeFrontmatterSchemas() =>
        EightFiles.Where(static file => file.StartsWith("schema/", StringComparison.Ordinal))
            .Should()
            .HaveCount(4)
            .And.AllSatisfy(file => File.Exists(Written(file)).Should().BeTrue($"init writes .spec/{file}; {_stdout}{_stderr}"));

    [Then("the templates folder holds the four templates")]
    public void ThenTheTemplatesFolderHoldsTheFourTemplates() =>
        EightFiles.Where(static file => file.StartsWith("templates/", StringComparison.Ordinal))
            .Should()
            .HaveCount(4)
            .And.AllSatisfy(file => File.Exists(Written(file)).Should().BeTrue($"init writes .spec/{file}; {_stdout}{_stderr}"));

    [Then("the exit code is {int}")]
    public void ThenTheExitCodeIs(int code) => _exitCode.Should().Be(code, _stdout + _stderr);

    [Then("each written file is byte-identical to the tool's embedded copy")]
    public void ThenEachWrittenFileIsByteIdenticalToTheToolsEmbeddedCopy()
    {
        var version = NewestVersion();
        foreach (var file in EightFiles)
        {
            File.Exists(Written(file)).Should().BeTrue($"init writes .spec/{file}; {_stdout}{_stderr}");
            File.ReadAllBytes(Written(file)).Should().Equal(Read(Embedded(file, version)), $"init writes .spec/{file} from the embedded copy");
        }
    }

    [Then("each frontmatter schema written carries an id under this repository's schema address for the version written")]
    public void ThenEachFrontmatterSchemaWrittenCarriesAnIdUnderThisRepositorysSchemaAddressForTheVersionWritten()
    {
        var version = NewestVersion();
        foreach (var file in ToolOwnedFiles.Where(static file => file.EndsWith(".frontmatter.schema.json", StringComparison.Ordinal)))
        {
            File.Exists(Written(file)).Should().BeTrue($"init writes .spec/{file}; {_stdout}{_stderr}");
            JsonNode.Parse(File.ReadAllText(Written(file)))!["$id"]!.GetValue<string>()
                .Should()
                .Be($"https://github.com/rlittlesii/specht/schema/v{version}/{Path.GetFileName(file)}");
        }
    }

    [Then("the standard output lists every one of the eight files relative to the root")]
    public void ThenTheStandardOutputListsEveryOneOfTheEightFilesRelativeToTheRoot()
    {
        foreach (var file in EightFiles)
        {
            Listing(file).Should().ContainSingle($"stdout lists .spec/{file} once; {_stdout}{_stderr}");
        }

        _stdout.Should().NotContain(Sandbox).And.NotContain("\\");
    }

    [Then("marks the feature template as skipped and the rest as written")]
    public void ThenMarksTheFeatureTemplateAsSkippedAndTheRestAsWritten()
    {
        foreach (var file in EightFiles)
        {
            var (marker, other) = file == FeatureTemplate ? ("skipped", "written") : ("written", "skipped");
            Listing(file).Should().ContainSingle(_stdout).Which.Should().Contain(marker).And.NotContain(other);
        }
    }

    [Then("the only files created are under the schema folder and the templates folder")]
    public void ThenTheOnlyFilesCreatedAreUnderTheSchemaFolderAndTheTemplatesFolder()
    {
        var before = _snapshot ?? throw new InvalidOperationException("No snapshot was taken.");
        var created = Snapshot().Keys.Except(before.Keys).ToList();
        created.Should().NotBeEmpty($"init ran against a bare root; {_stdout}{_stderr}");
        created.Should().AllSatisfy(static path =>
            path.Should().Match(static path => path.StartsWith("repo/.spec/schema/", StringComparison.Ordinal)
                                               || path.StartsWith("repo/.spec/templates/", StringComparison.Ordinal)));
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

    [Then("the seven other files are written")]
    public void ThenTheSevenOtherFilesAreWritten()
    {
        var version = NewestVersion();
        foreach (var file in ToolOwnedFiles)
        {
            File.Exists(Written(file)).Should().BeTrue($"init writes .spec/{file}; {_stdout}{_stderr}");
            File.ReadAllBytes(Written(file)).Should().Equal(Read(Embedded(file, version)), $"init writes .spec/{file} from the embedded copy");
        }

        File.ReadAllBytes(Written(Manifest)).Should().Equal(_manifest, "init leaves the manifest that was there as it was");
    }

    [Then("the schema folder and the templates folder are written under the working directory")]
    public void ThenTheSchemaFolderAndTheTemplatesFolderAreWrittenUnderTheWorkingDirectory() =>
        EightFiles.Should()
            .AllSatisfy(file => File.Exists(Written(file)).Should().BeTrue($"init writes .spec/{file} under the working directory; {_stdout}{_stderr}"));

    [Then("each embedded frontmatter schema and template is byte-identical to its file")]
    public void ThenEachEmbeddedFrontmatterSchemaAndTemplateIsByteIdenticalToItsFile()
    {
        foreach (var (file, (embedded, live)) in Files)
        {
            embedded.Should().NotBeNull($"the tool embeds {file} at the newest version");
            embedded.Should().Equal(live, $"the embedded {file} ships the bytes of .spec/{file}");
        }
    }

    [Then("the embedded manifest equals the live manifest on every tool-owned key")]
    public void ThenTheEmbeddedManifestEqualsTheLiveManifestOnEveryToolOwnedKey()
    {
        _embeddedManifest.Should().NotBeNull($"the tool embeds {Manifest} at the newest version");
        var live = _liveManifest ?? throw new InvalidOperationException("No live manifest was read.");
        foreach (var (key, value) in _embeddedManifest!)
        {
            live.ContainsKey(key).Should().BeTrue($".spec/{Manifest} carries the tool-owned key {key}");
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

    private static string[] EightFiles => [Manifest, .. ToolOwnedFiles];

    private string Sandbox => _sandbox ?? throw new InvalidOperationException("No root was prepared.");

    private string Root => Path.Combine(Sandbox, _typed);

    private Dictionary<string, (byte[]? Embedded, byte[] Live)> Files =>
        _files ?? throw new InvalidOperationException("Nothing was compared.");

    private static string Live(string file) => Path.Combine([AppContext.BaseDirectory, ".spec", .. file.Split('/')]);

    private static string Embedded(string file, int version) => file.Replace("/", $"/v{version}/", StringComparison.Ordinal);

    [GeneratedRegex("^schema/v([0-9]+)/")]
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

    private static int NewestVersion()
    {
        var versions = Shipped.GetManifestResourceNames()
            .Select(static name => SchemaVersion().Match(name))
            .Where(static match => match.Success)
            .Select(static match => int.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture))
            .ToList();
        versions.Should().NotBeEmpty("the tool embeds at least one schema/v<n>/ resource");
        return versions.Max();
    }

    private void Prepare()
    {
        _sandbox = Path.Combine(Path.GetTempPath(), "specht-init-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(Root, ".spec"));
        File.WriteAllText(Path.Combine(Sandbox, "beside-the-root.txt"), "synthetic\n");
        File.WriteAllText(Path.Combine(Root, "README.md"), "# A synthetic repository\n");
        File.WriteAllText(Path.Combine(Root, ".spec", "README.md"), "# A synthetic specification\n");
    }

    private string Written(string file) => Path.Combine([Root, ".spec", .. file.Split('/')]);

    private string[] Listing(string file) =>
        _stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(line => line.Contains($".spec/{file}", StringComparison.Ordinal))
            .ToArray();

    private Dictionary<string, byte[]> Snapshot() =>
        Directory.EnumerateFiles(Sandbox, "*", SearchOption.AllDirectories)
            .ToDictionary(
                path => Path.GetRelativePath(Sandbox, path).Replace(Path.DirectorySeparatorChar, '/'),
                File.ReadAllBytes,
                StringComparer.Ordinal);

    private const string Manifest = "schema/spec-structure.schema.json";

    private const string FeatureTemplate = "templates/feature.md";

    private static readonly string[] ToolOwnedFiles =
    [
        "schema/feature-spec.frontmatter.schema.json",
        "schema/task.frontmatter.schema.json",
        "schema/epic.frontmatter.schema.json",
        "templates/feature.md",
        "templates/decision.md",
        "templates/adr.md",
        "templates/lesson.md",
    ];

    private JsonObject? _liveManifest;
    private Dictionary<string, (byte[]? Embedded, byte[] Live)>? _files;
    private JsonObject? _embeddedManifest;
    private string? _sandbox;
    private Dictionary<string, byte[]>? _snapshot;
    private byte[]? _manifest;
    private string _typed = "repo";
    private string _stdout = string.Empty;
    private string _stderr = string.Empty;
    private int _exitCode = -1;
}
