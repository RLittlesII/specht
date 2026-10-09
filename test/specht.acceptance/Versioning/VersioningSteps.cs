using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Reqnroll;
using specht.tests;

namespace specht.acceptance.Versioning;

/// <summary>
/// Steps for <c>src/specht.tool/.spec/versioning.feature</c> (0001-F7): 0042 binds B-001, B-002, B-003, B-004, B-014, B-022
/// and B-023. "The tool ships schema versions 1 and 2" builds a synthetic version set in memory from the embedded version 1,
/// so B-001 and B-014 run the engine's runner over that set; every other run launches the built tool with <c>--json</c>
/// over a synthetic root, so B-003 sees the process's stderr and exit code and B-002, B-022 and B-023 read the document.
/// "The tool ships schema version 1 only" is the real embedded set, asserted, not built. The rest of the file's scenarios
/// stay pending for their items.
/// </summary>
[Binding]
[Scope(Feature = "Schema versioning")]
public sealed partial class VersioningSteps
{
    [Given("a repository root holding a manifest and the three frontmatter schemas")]
    public void GivenARepositoryRootHoldingAManifestAndTheThreeFrontmatterSchemas() => _tree = new SpecTree();

    [Given("the tool ships schema versions 1 and 2")]
    public void GivenTheToolShipsSchemaVersions1And2()
    {
        var shipped = SchemaVersions.Embedded.Select(1);
        _one = shipped;
        _two = shipped with { Number = 2, FeatureSchema = Rehome(shipped.FeatureSchema, 2) };
    }

    [Given("the tool ships schema version 1 only")]
    public void GivenTheToolShipsSchemaVersion1Only() =>
        SchemaVersions.Embedded.Versions.Select(static version => version.Number).Should().Equal(1);

    [Given("the tool ships schema version n")]
    public void GivenTheToolShipsSchemaVersionN()
    {
        var versions = typeof(SchemaVersions).Assembly.GetManifestResourceNames()
            .Select(static name => EmbeddedSchema().Match(name))
            .Where(static match => match.Success)
            .Select(static match => int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture))
            .ToList();
        versions.Should().NotBeEmpty("the engine embeds at least one schema/v<n>/ resource");
        _newest = versions.Max();
    }

    [Given("version 2 holds a rule version 1 does not")]
    public void GivenVersion2HoldsARuleVersion1DoesNot()
    {
        One.RuleIds.Should().Contain(MissingRule);
        _one = One with { RuleIds = One.RuleIds.Where(static id => id != MissingRule).ToHashSet(StringComparer.Ordinal) };
    }

    [Given("the manifest pins version {int}")]
    [Given("the manifest pins version {int} and records no upstream schema source")]
    public void GivenTheManifestPinsVersion(int version) => Pin(version);

    [Given("the manifest carries no schema version")]
    public void GivenTheManifestCarriesNoSchemaVersion() => Pin(null);

    [Given("the root holds a specification whose frontmatter version 1 accepts and version 2 rejects")]
    public void GivenTheRootHoldsASpecificationWhoseFrontmatterVersion1AcceptsAndVersion2Rejects()
    {
        var schema = JsonNode.Parse(Two.FeatureSchema)!.AsObject();
        schema["properties"]!["priority"]!["enum"] = new JsonArray("critical");
        _two = Two with { FeatureSchema = schema.ToJsonString() };
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

    [When("the check runs")]
    [When("the check runs with JSON output")]
    public void WhenTheCheckRuns()
    {
        if (_one is not null)
        {
            _report = SpecCheckRunner.Run(Tree.Root, new SchemaVersions([One, Two]));
            return;
        }

        (_stdout, _stderr, _exitCode) = Tool.Launch(Tree.Root, "--root", ".", "--json");
    }

    [When("its embedded versions are enumerated")]
    public void WhenItsEmbeddedVersionsAreEnumerated() =>
        _enumerated = SchemaVersions.Embedded.Versions.Select(static version => version.Number).ToList();

    [Then("no frontmatter violation is reported")]
    public void ThenNoFrontmatterViolationIsReported() =>
        Report.Violations.Should().NotContain(static violation => FrontmatterRules.Contains(violation.RuleId));

    [Then("no violation is reported")]
    public void ThenNoViolationIsReported() => Report.Violations.Should().BeEmpty();

    [Then("the document names schema version {int}")]
    public void ThenTheDocumentNamesSchemaVersion(int version) =>
        Document["schemaVersion"]!.GetValue<int>().Should().Be(version);

    [Then("the standard error names version {int} and the versions the tool ships")]
    public void ThenTheStandardErrorNamesVersionAndTheVersionsTheToolShips(int version)
    {
        var shipped = SchemaVersions.Embedded.Versions.Select(static shippedVersion => shippedVersion.Number);
        foreach (var named in shipped.Prepend(version))
        {
            _stderr.Should().MatchRegex($@"\b{named}\b", "the standard error names version {0}", named);
        }
    }

    [Then("the standard output is empty")]
    public void ThenTheStandardOutputIsEmpty() => _stdout.Should().BeEmpty();

    [Then("the exit code is {int}")]
    public void ThenTheExitCodeIs(int code) => _exitCode.Should().Be(code, _stderr);

    [Then("every version from 1 to n is present")]
    public void ThenEveryVersionFrom1ToNIsPresent() =>
        Enumerated.Should().Equal(Enumerable.Range(1, _newest ?? throw new InvalidOperationException("No newest version was read.")));

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

    private SchemaVersion One => _one ?? throw new InvalidOperationException("No version 1 was built.");

    private SchemaVersion Two => _two ?? throw new InvalidOperationException("No version 2 was built.");

    private SpecCheckReport Report => _report ?? throw new InvalidOperationException("The check has not run in process.");

    private IReadOnlyList<int> Enumerated => _enumerated ?? throw new InvalidOperationException("No version was enumerated.");

    private JsonObject Document
    {
        get
        {
            _stdout.Should().NotBeEmpty("the check wrote its document: {0}", _stderr);
            return JsonNode.Parse(_stdout)!.AsObject();
        }
    }

    private IReadOnlyList<JsonNode> Violations => Document["violations"]!.AsArray().Select(static violation => violation!).ToList();

    [GeneratedRegex("^schema/v([0-9]+)/")]
    private static partial Regex EmbeddedSchema();

    private static string Rehome(string schema, int version)
    {
        var node = JsonNode.Parse(schema)!.AsObject();
        node["$id"] = Regex.Replace(node["$id"]!.GetValue<string>(), "/v[0-9]+/", $"/v{version}/");
        return node.ToJsonString();
    }

    private void Pin(int? version)
    {
        var path = Path.Combine(Tree.Root, SpecManifest.RelativePath);
        var manifest = JsonNode.Parse(File.ReadAllText(path))!.AsObject();
        manifest.Remove("schemaVersion");
        if (version is { } pinned)
        {
            manifest["schemaVersion"] = pinned;
        }

        File.WriteAllText(path, manifest.ToJsonString());
    }

    private const string MissingRule = "SPEC031";

    private const string EpicPath = "epics/0001-epic/epic.md";

    private const string TwoClaims =
        "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n"
            + "| B-001 | It does the thing. | brd | Active |\n| B-002 | It does another thing. | brd | Active |\n";

    private static readonly HashSet<string> FrontmatterRules = new(StringComparer.Ordinal) { "SPEC001", "SPEC002", "SPEC003", "SPEC004" };

    private SpecTree? _tree;
    private SchemaVersion? _one;
    private SchemaVersion? _two;
    private SpecCheckReport? _report;
    private IReadOnlyList<int>? _enumerated;
    private int? _newest;
    private string _stdout = string.Empty;
    private string _stderr = string.Empty;
    private int _exitCode = -1;
}
