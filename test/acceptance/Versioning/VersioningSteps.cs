using System.Globalization;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Reqnroll;
using Specht.Report;
using Specht.Tests.Shared;
using Specht.Versioning;

namespace Specht.Acceptance.Versioning;

/// <summary>
/// Steps for <c>src/tool/.spec/versioning.feature</c> (0001-F7): item 0120 binds B-001, B-002, B-003, B-014, B-022,
/// B-023, B-039 and B-055, with every version written <c>major.minor.patch</c>. "The tool ships schema versions ..." builds
/// a synthetic version set in memory, each member the embedded version 0.1.0 under the number the step names, so B-001,
/// B-014 and B-055 run the engine's runner over that set and B-055 reads the document the engine makes from its report;
/// every other run launches the built tool with <c>--json</c> over a synthetic root, so B-003 and B-039 see the process's
/// stderr and exit code and B-002, B-022 and B-023 read the document it printed. "The tool ships schema version 0.1.0
/// only" is the real embedded set, asserted, not built. "The manifest's schema version is ..." writes a value of digits
/// alone as a JSON number, the integer the retired pin was, and any other value as a JSON string. B-004's three steps stay
/// unbound: its proof against what was published is item 0044's. Item 0176 binds B-069 to B-073 and B-076: each writes a
/// synthetic <c>spechtSchema</c> into the root's manifest with one fault and launches the built tool, and the rejection is
/// read from its stderr as the manifest's own name for the member or the kind, in quotes. B-068 and B-075 are item 0092's
/// and B-074 is item 0093's. The rest of the file's scenarios stay pending for their items; a step one of them shares with
/// a scenario above is bound, and its other steps are not.
/// </summary>
[Binding]
[Scope(Feature = "Schema versioning")]
public sealed class VersioningSteps
{
    [Given("a repository root holding a manifest and the three frontmatter schemas")]
    public void GivenARepositoryRootHoldingAManifestAndTheThreeFrontmatterSchemas() => _tree = new SpecTree();

    [Given(@"^the tool ships schema versions (.+) and (\S+)$")]
    public void GivenTheToolShipsSchemaVersions(string earlier, string last)
    {
        var embedded = SchemaVersions.Embedded.Select(new SemanticVersion(0, 1, 0));
        _shipped = earlier.Split(", ").Append(last).Select(Version).ToDictionary(
            static number => number,
            number => embedded with { Number = number });
    }

    [Given("the tool ships schema version {word} only")]
    public void GivenTheToolShipsSchemaVersionOnly(string version) =>
        SchemaVersions.Embedded.Versions.Select(static shipped => shipped.Number.ToString()).Should().Equal(version);

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

    [When("the check runs")]
    [When("the check runs with JSON output")]
    public void WhenTheCheckRuns()
    {
        if (_shipped is not null)
        {
            _report = EngineServices.Runner().Run(Tree.Root, new SchemaVersions(_shipped.Values));
            return;
        }

        (_stdout, _stderr, _exitCode) = AcceptanceTool.Launch(Tree.Root, "--root", ".", "--json");
    }

    [Then("no frontmatter violation is reported")]
    public void ThenNoFrontmatterViolationIsReported() =>
        Report.Violations.Should().NotContain(static violation => FrontmatterRules.Contains(violation.RuleId));

    [Then("no violation is reported")]
    public void ThenNoViolationIsReported() => Report.Violations.Should().BeEmpty();

    [Then("the document names schema version {word}")]
    public void ThenTheDocumentNamesSchemaVersion(string version) =>
        Document["schemaVersion"]!.GetValue<string>().Should().Be(version);

    [Then("the standard error names version {word} and the versions the tool ships")]
    public void ThenTheStandardErrorNamesVersionAndTheVersionsTheToolShips(string version)
    {
        var shipped = SchemaVersions.Embedded.Versions.Select(static shippedVersion => shippedVersion.Number.ToString());
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

    [Then(@"^the rejection names the (address|version|content hashes|item schema|epic schema)$")]
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

    private void WriteUpstreamSchemaSource(Action<JsonObject> fault)
    {
        var path = Path.Combine(Tree.Root, SpecTree.ManifestPath);
        var manifest = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        var source = new JsonObject
        {
            ["location"] = "https://schemas.example/specht/",
            ["version"] = "2026.10",
            ["hashes"] = new JsonObject
            {
                ["feature"] = "sha256:" + new string('a', 64),
                ["task"] = "sha256:" + new string('b', 64),
                ["epic"] = "sha256:" + new string('c', 64),
            },
        };
        fault(source);
        manifest["spechtSchema"] = source;
        File.WriteAllText(path, manifest.ToJsonString());
    }

    private const string MissingRule = "SPEC031";

    private const string EpicPath = "epics/0001-epic/epic.md";

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
    };

    private SpecTree? _tree;
    private Dictionary<SemanticVersion, SchemaVersion>? _shipped;
    private SpechtReport? _report;
    private string _stdout = string.Empty;
    private string _stderr = string.Empty;
    private int _exitCode = -1;
}
