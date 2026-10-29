using System.Globalization;
using System.IO.Abstractions;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using Reqnroll;
using Specht.Manifest;
using Specht.Report;
using Specht.Tests.Shared;
using Specht.Tool.Features.Init;
using Specht.Versioning;

namespace Specht.Acceptance.Versioning;

/// <summary>
/// Steps for <c>src/tool/.spec/versioning.feature</c> (0001-F7): item 0120 binds B-001, B-002, B-003, B-014, B-022,
/// B-023, B-039 and B-055, with every version written <c>major.minor.patch</c>. "The tool ships schema versions ..." builds
/// a synthetic version set in memory, each member the embedded version 0.1.0 under the number the step names, so B-001,
/// B-014 and B-055 run the engine's runner over that set and B-055 reads the document the engine makes from its report;
/// every other run launches the built tool with <c>--json</c> over a synthetic root, so B-003 and B-039 see the process's
/// stderr and exit code and B-002, B-022 and B-023 read the document it printed. "The tool ships schema version 0.1.0"
/// selects the real embedded version. "The manifest's schema version is ..." writes a value of digits
/// alone as a JSON number, the integer the retired pin was, and any other value as a JSON string. B-004's three steps stay
/// unbound: its proof against what was published is item 0044's. Item 0176 binds B-069 to B-073 and B-076: each writes a
/// synthetic <c>spechtSchema</c> into the root's manifest with one fault and launches the built tool, and the rejection is
/// read from its stderr as the manifest's own name for the member or the kind, in quotes. Item 0092 binds B-024, B-025,
/// B-028, B-032, B-068, B-075 and B-077 to B-081: a recorded source carries the SHA-256 of each schema file the root holds,
/// "the root holds the copy of that source" makes the Feature schema of that copy require a key the embedded one does not
/// and records that copy's hashes, and each run launches the built tool. "The network is unavailable" points the launched
/// process's proxy variables at a closed loopback port, and "no network call was attempted" reads the built engine and
/// tool assemblies for a <c>System.Net</c> reference (OQ-39, the owner's ruling of 2026-10-12).
/// Item 0093 binds B-026, B-027, B-033, B-035, B-067, B-082, B-083 and B-140: "upgrade runs" composes the upgrade command
/// in process over the engine's and the slice's production registrations, with an HTTP client over a synthetic source
/// registered first, so the fetch never leaves the process (ADR-0013). A differing local copy is recorded under the SHA-256
/// of synthetic content the source serves under the manifest's file name for its kind; each refusal is that source
/// answering with other content, an unreachable error, or a 401 or 404 status. Under that run "no network call was
/// attempted" reads the source's requests. B-034 and B-139 launch the built tool's upgrade over a root that needs no fetch.
/// B-074's steps are bound only where its scenarios share them.
/// Item 0290 binds B-125 to B-127: each adds a synthetic <c>templates</c> member to that recorded source and launches the
/// built tool; B-125 reads the templates the engine's parser takes from the root's manifest, and B-126 and B-127 read the
/// rejection from stderr as the template's file name, in quotes.
/// Item 0291 binds B-130 to B-134, B-136, B-137 and B-141 over that in-process upgrade: the record hashes synthetic
/// feature-template bytes the source serves at its location followed by <c>feature.md</c>, the root's feature template is
/// written by the step that says how it stands against that hash, and each refusal is the source answering for that file
/// name, or for every file, as for a schema. B-135's consumer-named template stays unbound for item 0131.
/// Item 0126 binds B-086 and B-087 over the embedded resources and their declared addresses. The
/// partial lock bindings execute B-092 and B-093 over synthetic shipping files on disk.
/// B-012 runs the existing init writer over synthetic versioned manifests on disk and reads the written pin. Item 0046
/// binds B-018 to B-020: each launches the built tool's upgrade over the Background root - against <c>no-such-root</c>
/// beneath it, without its manifest, with a rule set to a level no version defines, or pinned to 0.2.0 with a recorded
/// source and the embedded source selected - and compares every file under the root with its bytes before the run. The
/// rest of the file's scenarios stay pending for their items; a step one of them shares with a scenario above is bound,
/// and its other steps are not.
/// </summary>
[Binding]
[Scope(Feature = "Schema versioning")]
public sealed partial class VersioningSteps
{
    [Given("the tool ships schema version {word}")]
    public void GivenTheToolShipsSchemaVersion(string version) =>
        _embedded = EngineServices.Versions().Find(Version(version))!;

    [When("the folders holding its schemas, its templates and its rule pages are read")]
    public void WhenTheFoldersHoldingItsSchemasItsTemplatesAndItsRulePagesAreRead()
    {
        var names = new[] { typeof(SchemaVersions).Assembly, typeof(Specht.Tool.ExitCodes).Assembly }
            .SelectMany(static assembly => assembly.GetManifestResourceNames())
            .Where(static name => name.StartsWith("schema/", StringComparison.Ordinal)
                                  || name.StartsWith("templates/", StringComparison.Ordinal)
                                  || name.StartsWith("rules/", StringComparison.Ordinal))
            .Where(name => string.Equals(name.Split('/')[1], $"v{_embedded!.Number}", StringComparison.Ordinal))
            .ToList();
        foreach (var tree in new[] { "schema", "templates", "rules" })
        {
            names.Should().Contain(name => name.StartsWith($"{tree}/", StringComparison.Ordinal));
        }

        _folders = names
            .Select(static name => name.Split('/')[1])
            .Distinct()
            .ToList();
    }

    [Then("each is named v0.1.0")]
    public void ThenEachIsNamedVersionZeroOneZero() => _folders.Should().Equal($"v{_embedded!.Number}");

    [When("the address each of its frontmatter schemas declares is read")]
    public void WhenTheAddressEachOfItsFrontmatterSchemasDeclaresIsRead() =>
        _addresses = new Dictionary<string, string>
        {
            ["feature.schema.json"] = _embedded!.FeatureSchema,
            ["task.schema.json"] = _embedded.ItemSchema,
            ["epic.schema.json"] = _embedded.EpicSchema,
        }.ToDictionary(static pair => pair.Key, static pair => JsonNode.Parse(pair.Value)!["$id"]!.GetValue<string>());

    [Then("it is this repository's schema address, then v0.1.0, then the frontmatter folder and the file's name")]
    public void ThenItIsThisRepositorysSchemaAddressThenTheVersionAndTheFrontmatterFile() =>
        _addresses.Should().HaveCount(3).And.AllSatisfy(pair =>
            pair.Value.Should().Be($"https://github.com/rlittlesii/specht/schema/v{_embedded!.Number}/frontmatter/{pair.Key}"));

    [Given("a repository root holding a manifest and the three frontmatter schemas")]
    public void GivenARepositoryRootHoldingAManifestAndTheThreeFrontmatterSchemas() => _tree = new SpecTree();

    [Given(@"^the tool ships schema versions (.+) and (\S+)$")]
    public void GivenTheToolShipsSchemaVersions(string earlier, string last)
    {
        var embedded = EngineServices.Versions().Find(new SemanticVersion(0, 1, 0))!;
        _shipped = earlier.Split(", ").Append(last).Select(Version).ToDictionary(
            static number => number,
            number => embedded with { Number = number });
    }

    [Given("a root directory with no manifest")]
    [Given("the manifest has been removed from the root")]
    public void GivenARootDirectoryWithNoManifest() => File.Delete(Path.Combine(Tree.Root, SpecTree.ManifestPath));

    [When("init runs against it")]
    public async Task WhenInitRunsAgainstIt()
    {
        var shippingCopy = Shipped.Keys.ToDictionary(
            static version => $"manifest/v{version}/manifest.json",
            static version => JsonSerializer.SerializeToUtf8Bytes(new { schemaVersion = version.ToString() }));
        await new InitWriter(
            shippingCopy,
            new FileSystem(),
            EngineServices.Parser(),
            new SpecManifestStore(new FileSystem())).Write(Tree.Root, schemas: false, templates: false, CancellationToken.None);
    }

    [Then("the written manifest pins version {word}")]
    public void ThenTheWrittenManifestPinsVersion(string version)
    {
        var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(Tree.Root, SpecTree.ManifestPath)))!.AsObject();
        manifest["schemaVersion"]!.GetValue<string>().Should().Be(version);
    }

    [Given(@"^version (\S+) holds a rule version (\S+) does not$")]
    public void GivenVersionHoldsARuleVersionDoesNot(string holder, string lacking)
    {
        Shipped[Version(holder)].RuleIds.Should().Contain(MissingRule);
        var without = Shipped[Version(lacking)];
        Shipped[Version(lacking)] = without with
        {
            RuleIds = without.RuleIds.Where(static id => id != MissingRule).ToHashSet(StringComparer.Ordinal),
        };
    }

    [Given("the manifest pins version {word}")]
    [Given("the manifest pins version {word} and records no upstream schema source")]
    [Given("the manifest pins version {word} with no pin policy")]
    public void GivenTheManifestPinsVersion(string version) => WriteSchemaVersion(JsonValue.Create(version));

    [Given("the manifest carries no schema version")]
    public void GivenTheManifestCarriesNoSchemaVersion() => WriteSchemaVersion(null);

    [Given("the manifest's schema version is {word}")]
    public void GivenTheManifestsSchemaVersionIs(string value) =>
        WriteSchemaVersion(
            value.All(char.IsAsciiDigit) ? JsonValue.Create(int.Parse(value, CultureInfo.InvariantCulture)) : JsonValue.Create(value));

    [Given(@"^the root holds a specification whose frontmatter version (\S+) accepts and version (\S+) rejects$")]
    public void GivenTheRootHoldsASpecificationWhoseFrontmatterOneVersionAcceptsAndAnotherRejects(string accepting, string rejecting)
    {
        Shipped.Should().ContainKey(Version(accepting));
        var stricter = Shipped[Version(rejecting)];
        var schema = JsonNode.Parse(stricter.FeatureSchema)!.AsObject();
        schema["properties"]!["priority"]!["enum"] = new JsonArray("critical");
        Shipped[Version(rejecting)] = stricter with { FeatureSchema = schema.ToJsonString() };
        Tree.WriteFeature("0001", "F1", new Dictionary<string, string> { ["priority"] = "med" });
    }

    [Given("the root holds a specification that breaks only that rule")]
    public void GivenTheRootHoldsASpecificationThatBreaksOnlyThatRule()
    {
        Tree.WriteFeature("0001", "F1", sections: SpecTree.SectionsWith("3. Acceptance Criteria", TwoClaims));
        Tree.Run().Violations.Should().NotBeEmpty().And.OnlyContain(static violation => violation.RuleId == MissingRule);
    }

    [Given("the root holds an epic whose frontmatter carries a title and a description")]
    public void GivenTheRootHoldsAnEpicWhoseFrontmatterCarriesATitleAndADescription() =>
        Tree.WriteEpic(
            "0001",
            new Dictionary<string, string> { ["title"] = "\"A synthetic epic\"", ["description"] = "\"An epic built for the check\"" });

    [Given("the root holds an epic whose frontmatter carries an empty {word}")]
    public void GivenTheRootHoldsAnEpicWhoseFrontmatterCarriesAnEmpty(string key) =>
        Tree.WriteEpic("0001", new Dictionary<string, string> { [key] = "\"\"" });

    [Given(@"^the manifest records an upstream schema source without its (address|version|content hashes)$")]
    public void GivenTheManifestRecordsAnUpstreamSchemaSourceWithoutIts(string part) =>
        WriteUpstreamSchemaSource(source => source.Remove(SourceNames[part]));

    [Given(@"^the manifest records an upstream schema source at (a web address with no security|a folder on the machine|a package name)$")]
    public void GivenTheManifestRecordsAnUpstreamSchemaSourceAt(string address) =>
        WriteUpstreamSchemaSource(source => source["location"] = address switch
        {
            "a web address with no security" => "http://schemas.example/specht/",
            "a folder on the machine" => "schemas/upstream/",
            _ => "specht.schemas",
        });

    [Given("the manifest records an upstream schema source with an empty version")]
    public void GivenTheManifestRecordsAnUpstreamSchemaSourceWithAnEmptyVersion() =>
        WriteUpstreamSchemaSource(static source => source["version"] = string.Empty);

    [Given("the manifest records an upstream schema source with no content hash for the item schema")]
    public void GivenTheManifestRecordsAnUpstreamSchemaSourceWithNoContentHashForTheItemSchema() =>
        WriteUpstreamSchemaSource(static source => source["hashes"]!.AsObject().Remove("task"));

    [Given(
        @"^the manifest records an upstream schema source whose content hash for the item schema is "
            + "(written without its algorithm|written in capital digits|shorter than the algorithm's|another algorithm's)$")]
    public void GivenTheManifestRecordsAnUpstreamSchemaSourceWhoseContentHashForTheItemSchemaIs(string form) =>
        WriteUpstreamSchemaSource(source => source["hashes"]!["task"] = form switch
        {
            "written without its algorithm" => new string('b', 64),
            "written in capital digits" => "sha256:" + new string('B', 64),
            "shorter than the algorithm's" => "sha256:" + new string('b', 63),
            _ => "sha512:" + new string('b', 128),
        });

    [Given("the manifest records an upstream schema source with content hashes for the Feature and item schemas only")]
    public void GivenTheManifestRecordsAnUpstreamSchemaSourceWithContentHashesForTheFeatureAndItemSchemasOnly() =>
        WriteUpstreamSchemaSource(static source => source["hashes"]!.AsObject().Remove("epic"));

    [Given("the manifest declares the epic grammar")]
    public void GivenTheManifestDeclaresTheEpicGrammar() =>
        (JsonNode.Parse(File.ReadAllText(Path.Combine(Tree.Root, SpecTree.ManifestPath)))!["identifiers"]?["epic"])
            .Should()
            .NotBeNull("the manifest declares the epic grammar");

    [Given("the manifest declares no epic grammar")]
    public void GivenTheManifestDeclaresNoEpicGrammar() =>
        EditManifest(static manifest => manifest["identifiers"]!.AsObject().Remove("epic"));

    [Given("the manifest records an upstream schema source")]
    [Given("the manifest records an upstream schema source and selects no source")]
    [Given("the manifest records an upstream schema source with its version and content hash")]
    [Given("the manifest records an upstream schema source with its address, its version and a content hash for each schema it fetches")]
    public void GivenTheManifestRecordsAnUpstreamSchemaSource() => WriteUpstreamSchemaSource(static _ => { });

    [Given("the manifest pins version {word} and records an upstream schema source")]
    public void GivenTheManifestPinsVersionAndRecordsAnUpstreamSchemaSource(string version)
    {
        WriteSchemaVersion(JsonValue.Create(version));
        WriteUpstreamSchemaSource(static _ => { });
    }

    [Given("the root holds the copy of that source under its schema folder")]
    public void GivenTheRootHoldsTheCopyOfThatSourceUnderItsSchemaFolder()
    {
        var path = Path.Combine(Tree.Root, SpecTree.FeatureSchemaPath);
        var schema = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        schema["required"]!.AsArray().Add(UpstreamKey);
        File.WriteAllText(path, schema.ToJsonString());
        EditManifest(manifest =>
        {
            var hashes = manifest["spechtSchema"]!["hashes"]!.AsObject();
            foreach (var kind in hashes.Select(static hash => hash.Key).ToList())
            {
                hashes[kind] = HashOf(SchemaPaths[kind]);
            }
        });
        Tree.WriteFeature("0001", "F1");
    }

    [Given("the root's schema folder does not hold the item schema")]
    public void GivenTheRootsSchemaFolderDoesNotHoldTheItemSchema() => File.Delete(Path.Combine(Tree.Root, SpecTree.ItemSchemaPath));

    [Given("the root's copy of the item schema has been edited since it was fetched")]
    public void GivenTheRootsCopyOfTheItemSchemaHasBeenEditedSinceItWasFetched() =>
        File.AppendAllText(Path.Combine(Tree.Root, SpecTree.ItemSchemaPath), "\n");

    [Given("the network is unavailable")]
    public void GivenTheNetworkIsUnavailable()
    {
        _environment = new[] { "HTTP_PROXY", "HTTPS_PROXY", "ALL_PROXY", "http_proxy", "https_proxy", "all_proxy" }
            .ToDictionary(static name => name, static _ => ClosedLoopbackPort, StringComparer.Ordinal);
        Upstream.Refused["*"] = "unreachable";
    }

    [When("the check runs")]
    [When("the check runs with JSON output")]
    public void WhenTheCheckRuns()
    {
        if (_shipped is not null)
        {
            _report = EngineServices.Runner(new SchemaVersions(_shipped.Values)).Run(Tree.Root);
            return;
        }

        string[] args = _selection is null ? ["--root", ".", "--json"] : ["--root", ".", "--json", "--schema-source", _selection];
        (_stdout, _stderr, _exitCode) = AcceptanceTool.Launch(Tree.Root, _environment, args);
    }

    [Then("the frontmatter is validated against the local copy")]
    public void ThenTheFrontmatterIsValidatedAgainstTheLocalCopy() =>
        Violations
            .Where(static violation => violation["file"]!.GetValue<string>() == FeaturePath
                                       && FrontmatterRules.Contains(violation["ruleId"]!.GetValue<string>()))
            .Should()
            .NotBeEmpty("the local copy requires the key '{0}', which the embedded schema does not and the Feature lacks", UpstreamKey);

    [Then("the document says the schemas came from the upstream copy")]
    public void ThenTheDocumentSaysTheSchemasCameFromTheUpstreamCopy() =>
        Document["schemaSource"]!.GetValue<string>().Should().Be("upstream");

    [Then(@"^exactly version (\S+)'s rule ids are evaluated$")]
    public void ThenExactlyThatVersionsRuleIdsAreEvaluated(string version) =>
        Document["rulesEvaluated"]!.GetValue<int>().Should().Be(EngineServices.Versions().Find(Version(version))!.RuleIds.Count);

    [Then("the check completes")]
    public void ThenTheCheckCompletes()
    {
        _exitCode.Should().BeOneOf([0, 1], _stderr);
        Document["violations"].Should().NotBeNull("the check wrote its report");
    }

    [Then("no network call was attempted")]
    public void ThenNoNetworkCallWasAttempted()
    {
        if (_upgradedInProcess)
        {
            _exitCode.Should().Be(0, _stderr);
            Upstream.Requests.Should().BeEmpty();
            return;
        }

        EngineAssembly.CheckNetworkReferencesIn(EngineAssembly.Built).Should().BeEmpty();
        EngineAssembly.CheckNetworkReferencesIn(EngineAssembly.BuiltTool).Should().BeEmpty();
    }

    [Then("the standard error names the item schema's file")]
    public void ThenTheStandardErrorNamesTheItemSchemasFile() => _stderr.Should().Contain(SpecTree.ItemSchemaPath);

    [Then("the standard error names the command that fetches it")]
    public void ThenTheStandardErrorNamesTheCommandThatFetchesIt() => _stderr.Should().Contain("specht init");

    [Then("no frontmatter violation is reported")]
    public void ThenNoFrontmatterViolationIsReported() =>
        Report.Violations.Should().NotContain(static violation => FrontmatterRules.Contains(violation.RuleId));

    [Then("no violation is reported")]
    public void ThenNoViolationIsReported() => Report.Violations.Should().BeEmpty();

    [Then("the document names schema version {word}")]
    public void ThenTheDocumentNamesSchemaVersion(string version) =>
        Document["schemaVersion"]!.GetValue<string>().Should().Be(version);

    [Then("the standard error names version {word} and the versions the tool ships")]
    [Then("the standard error names {word} and the versions the tool ships")]
    public void ThenTheStandardErrorNamesVersionAndTheVersionsTheToolShips(string version)
    {
        var shipped = EngineServices.Versions().Versions.Select(static shippedVersion => shippedVersion.Number.ToString());
        foreach (var named in shipped.Prepend(version))
        {
            _stderr.Should().MatchRegex(Naming(named), "the standard error names version {0}", named);
        }
    }

    [Then("the standard error names {word}")]
    public void ThenTheStandardErrorNames(string value) =>
        _stderr.Should().MatchRegex(Naming(value), "the standard error names {0}", value);

    [Then("the standard output is empty")]
    public void ThenTheStandardOutputIsEmpty() => _stdout.Should().BeEmpty();

    [Then("the exit code is {int}")]
    public void ThenTheExitCodeIs(int code) => _exitCode.Should().Be(code, _stderr);

    [Then("the manifest is rejected")]
    public void ThenTheManifestIsRejected() => _exitCode.Should().Be(3, _stderr);

    [Then(@"^the rejection names the (address|version|content hashes|item schema|epic schema|feature template|runbook template)$")]
    public void ThenTheRejectionNamesThe(string part) =>
        _stderr.Should().Contain($"'{SourceNames[part]}'", "the rejection names the {0}", part);

    [Then("no frontmatter violation is reported for the epic")]
    public void ThenNoFrontmatterViolationIsReportedForTheEpic() =>
        Violations.Should().NotContain(static violation =>
            violation["file"]!.GetValue<string>() == EpicPath && FrontmatterRules.Contains(violation["ruleId"]!.GetValue<string>()));

    [Then("the epic is reported for a frontmatter violation of its {word}")]
    public void ThenTheEpicIsReportedForAFrontmatterViolationOfIts(string key) =>
        Violations
            .Where(violation => violation["file"]!.GetValue<string>() == EpicPath
                                && violation["ruleId"]!.GetValue<string>() == "SPEC004"
                                && violation["identifier"]?.GetValue<string>() == key)
            .Should()
            .ContainSingle();

    [AfterScenario]
    public void DeleteTree() => _tree?.Dispose();

    private SpecTree Tree => _tree ?? throw new InvalidOperationException("No repository root was prepared.");

    private Dictionary<SemanticVersion, SchemaVersion> Shipped =>
        _shipped ?? throw new InvalidOperationException("No version set was built.");

    private SpechtReport Report => _report ?? throw new InvalidOperationException("The check has not run in process.");

    private JsonObject Document
    {
        get
        {
            if (_report is not null)
            {
                return JsonNode.Parse(SpecReportDocument.From(_report).ToJson())!.AsObject();
            }

            _stdout.Should().NotBeEmpty("the check wrote its document: {0}", _stderr);
            return JsonNode.Parse(_stdout)!.AsObject();
        }
    }

    private IReadOnlyList<JsonNode> Violations => Document["violations"]!.AsArray().Select(static violation => violation!).ToList();

    private static SemanticVersion Version(string text) =>
        SemanticVersion.TryParse(text, out var version)
            ? version
            : throw new InvalidOperationException($"'{text}' is not a major.minor.patch version.");

    private static string Naming(string value) => $@"(?<![\w.]){Regex.Escape(value)}(?!\.?\w)";

    private void WriteSchemaVersion(JsonNode? value)
    {
        var path = Path.Combine(Tree.Root, SpecTree.ManifestPath);
        var manifest = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        manifest.Remove("schemaVersion");
        if (value is not null)
        {
            manifest["schemaVersion"] = value;
        }

        File.WriteAllText(path, manifest.ToJsonString());
    }

    private void WriteUpstreamSchemaSource(Action<JsonObject> fault) =>
        EditManifest(manifest =>
        {
            var source = new JsonObject
            {
                ["location"] = "https://schemas.example/specht/",
                ["version"] = "2026.10",
                ["hashes"] = new JsonObject(SchemaPaths.Select(schema => KeyValuePair.Create(schema.Key, (JsonNode?)HashOf(schema.Value)))),
            };
            fault(source);
            manifest["spechtSchema"] = source;
        });

    private void EditManifest(Action<JsonObject> edit)
    {
        var path = Path.Combine(Tree.Root, SpecTree.ManifestPath);
        var manifest = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        edit(manifest);
        File.WriteAllText(path, manifest.ToJsonString());
    }

    private string HashOf(string schemaPath) =>
        "sha256:" + Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(Path.Combine(Tree.Root, schemaPath))));

    private const string MissingRule = "SPEC031";

    private const string EpicPath = "epics/0001-epic/epic.md";

    private const string FeaturePath = "epics/0001-epic/F1-feature/spec.md";

    private const string UpstreamKey = "upstream_copy";

    private const string ClosedLoopbackPort = "http://127.0.0.1:9";

    private const string TwoClaims =
        "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n"
            + "| B-001 | It does the thing. | brd | Active |\n| B-002 | It does another thing. | brd | Active |\n";

    private static readonly HashSet<string> FrontmatterRules = new(StringComparer.Ordinal) { "SPEC001", "SPEC002", "SPEC003", "SPEC004" };

    private static readonly Dictionary<string, string> SourceNames = new(StringComparer.Ordinal)
    {
        ["address"] = "location",
        ["version"] = "version",
        ["content hashes"] = "hashes",
        ["item schema"] = "task",
        ["epic schema"] = "epic",
        ["feature template"] = "feature.md",
        ["runbook template"] = "runbook.md",
    };

    private static readonly Dictionary<string, string> SchemaPaths = new(StringComparer.Ordinal)
    {
        ["feature"] = SpecTree.FeatureSchemaPath,
        ["task"] = SpecTree.ItemSchemaPath,
        ["epic"] = SpecTree.EpicSchemaPath,
    };

    private Dictionary<string, string> _environment = new(StringComparer.Ordinal);
    private SchemaVersion? _embedded;
    private IReadOnlyList<string> _folders = [];
    private IReadOnlyDictionary<string, string> _addresses = new Dictionary<string, string>();
    private SpecTree? _tree;
    private Dictionary<SemanticVersion, SchemaVersion>? _shipped;
    private SpechtReport? _report;
    private string _stdout = string.Empty;
    private string _stderr = string.Empty;
    private int _exitCode = -1;
}
