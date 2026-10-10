using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;
using specht.Report;
using specht.tests;

namespace specht.acceptance.Discovery;

/// <summary>
/// Steps for <c>src/specht/Discovery/.spec/discovery.feature</c> (0001-F6): item 0005 binds B-001, B-002, B-003 and B-009.
/// The manifest is edited in memory from the tree's default copy and written when the check runs, and "the check runs" is
/// the engine's runner; the summary is the lines its report document makes and the report is that document serialized,
/// which <c>0001-F2</c> B-002 and <c>0001-F3</c> B-001 hold the tool's own output to. A specification a step writes has no
/// companion beside it unless the step says so, so one that is discovered is reported under its own path and one that is
/// not is reported nowhere. The scenarios of B-004 to B-008, B-010 and B-011 stay pending for their items.
/// </summary>
[Binding]
[Scope(Feature = "Discovery")]
public sealed class DiscoverySteps
{
    [Given("a repository root holding a manifest and the three frontmatter schemas")]
    public void GivenARepositoryRootHoldingAManifestAndTheThreeFrontmatterSchemas() => _tree = new SpecTree();

    [Given("the manifest is the default one this repository checks itself with")]
    public void GivenTheManifestIsTheDefaultOneThisRepositoryChecksItselfWith() =>
        _manifest = JsonNode.Parse(File.ReadAllText(ManifestPath))!.AsObject();

    [Given("the manifest declares a third layout whose specification file is a differently named markdown file under a documentation folder")]
    public void GivenTheManifestDeclaresAThirdLayout() =>
        Manifest["layouts"] = new JsonArray(
            Layout("legacy", LegacyGlob),
            Layout("coLocated", CoLocatedGlob),
            Layout("documentation", "docs/**/specification.md"));

    [Given("the root holds a specification at that place")]
    public void GivenTheRootHoldsASpecificationAtThatPlace() =>
        Write(_subject = "docs/guide/specification.md", "F1", sections: ThreeDefects);

    [Given("the manifest declares one layout only, the co-located one")]
    public void GivenTheManifestDeclaresOneLayoutOnlyTheCoLocatedOne() =>
        Manifest["layouts"] = new JsonArray(Layout("coLocated", CoLocatedGlob));

    [Given("the root holds a specification where the default legacy layout would find it")]
    public void GivenTheRootHoldsASpecificationWhereTheDefaultLegacyLayoutWouldFindIt() =>
        Write(_legacy = "epics/0001-epic/F1-feature/spec.md", "F1");

    [Given("the root holds a co-located specification")]
    public void GivenTheRootHoldsACoLocatedSpecification() => Write(_coLocated = "src/area/.spec/README.md", "F2");

    [Given("the manifest excludes the directory name {string}")]
    [Given("the manifest excludes the root-relative path {string}")]
    public void GivenTheManifestExcludes(string entry) => Manifest["exclusions"] = new JsonArray(entry);

    [Given("the root holds a co-located specification three levels below a {string} directory")]
    public void GivenTheRootHoldsACoLocatedSpecificationThreeLevelsBelowADirectory(string directory) =>
        Write(_subject = $"src/{directory}/one/two/three/.spec/README.md", "F1");

    [Given("the root holds a README in its root {string} folder")]
    public void GivenTheRootHoldsAReadmeInItsRootFolder(string folder) => Write(_rootReadme = $"{folder}/README.md", "F1");

    [Given("the root holds a co-located specification under {string}")]
    public void GivenTheRootHoldsACoLocatedSpecificationUnder(string folder) => Write($"{folder}/README.md", "F2");

    [Given("the root holds co-located specifications under {string} and {string}")]
    public void GivenTheRootHoldsCoLocatedSpecificationsUnder(string first, string second)
    {
        Write($"{first}/README.md", "F1");
        Write($"{second}/README.md", "F2");
        _pair = [$"{first}/README.md", $"{second}/README.md"];
    }

    [Given("the manifest declares an item file shape, an epic file glob and a companion glob")]
    public void GivenTheManifestDeclaresAnItemFileShapeAnEpicFileGlobAndACompanionGlob()
    {
        Manifest["taskFiles"] = "{task}-*.markdown";
        Manifest["epicFiles"] = "epics/*/epic.md";
        Manifest["companionFiles"] = "*.gherkin";
    }

    [Given("the root holds one item and one companion matching them beside a specification")]
    public void GivenTheRootHoldsOneItemAndOneCompanionMatchingThemBesideASpecification()
    {
        var specification = Write("src/area/.spec/README.md", "F1", new Dictionary<string, string> { ["children"] = "[\"0001-01\"]" });
        var directory = Path.GetDirectoryName(specification)!;
        Tree.WriteItem(specification, "0001-01", "F1");
        File.Move(Path.Combine(directory, "0001-01-item.md"), Path.Combine(directory, "0001-01-item.markdown"));
        Tree.WriteRaw(Companion, PhantomTag);
    }

    [Given("the root holds an epic file at {string}")]
    public void GivenTheRootHoldsAnEpicFileAt(string path)
    {
        Tree.WriteEpic("0001", new Dictionary<string, string> { ["priority"] = "urgent" });
        var written = Path.Combine(Tree.Root, "epics", "0001-epic");
        File.Move(Path.Combine(written, "epic.md"), Tree.WriteRaw(path, string.Empty), overwrite: true);
        Directory.Delete(written);
        _epic = path;
    }

    [Given("the manifest names its layouts {string} and {string}")]
    public void GivenTheManifestNamesItsLayouts(string legacy, string coLocated) =>
        Manifest["layouts"] = new JsonArray(Layout(legacy, LegacyGlob), Layout(coLocated, CoLocatedGlob));

    [Given("the root holds one specification in each layout")]
    public void GivenTheRootHoldsOneSpecificationInEachLayout()
    {
        Write("epics/0001-epic/F1-feature/spec.md", "F1");
        Write("src/area/.spec/README.md", "F2");
    }

    [When("the check runs")]
    public void WhenTheCheckRuns()
    {
        File.WriteAllText(ManifestPath, Manifest.ToJsonString());
        _report = SpecCheckRunner.Run(Tree.Root);
    }

    [Then("that specification is discovered")]
    public void ThenThatSpecificationIsDiscovered() => Reported.Should().Contain(Subject);

    [Then("that specification is not discovered")]
    public void ThenThatSpecificationIsNotDiscovered() => Reported.Should().NotContain(Subject);

    [Then("it is checked by every rule")]
    public void ThenItIsCheckedByEveryRule()
    {
        using var twin = new SpecTree();
        twin.WriteSpecification("src/area/.spec/README.md", "0001", "F1", sections: ThreeDefects);
        var expected = twin.Run().Violations.Select(Verdict).ToList();
        expected.Select(static verdict => verdict.RuleId).Distinct().Should().HaveCountGreaterThanOrEqualTo(3);
        Report.Violations.Where(violation => violation.File == Subject).Select(Verdict).Should().Equal(expected);
    }

    [Then("only the co-located specification is discovered")]
    public void ThenOnlyTheCoLocatedSpecificationIsDiscovered()
    {
        Reported.Should().Contain(_coLocated).And.NotContain(_legacy);
        Report.SpecificationCount.Should().Be(1);
    }

    [Then("the summary names one layout")]
    public void ThenTheSummaryNamesOneLayout() => Summary[0].Should().Be("specifications: coLocated 1");

    [Then("the root README is not discovered")]
    public void ThenTheRootReadmeIsNotDiscovered() => Reported.Should().NotContain(_rootReadme);

    [Then("the specification under {string} is discovered")]
    public void ThenTheSpecificationUnderIsDiscovered(string folder) => Reported.Should().Contain($"{folder}/README.md");

    [Then("neither specification is discovered")]
    public void ThenNeitherSpecificationIsDiscovered()
    {
        _pair.Should().HaveCount(2);
        Reported.Should().NotContain(_pair);
    }

    [Then("the summary counts one item")]
    public void ThenTheSummaryCountsOneItem() => Summary[1].Should().Be("items: 1");

    [Then("the epic's frontmatter is checked")]
    public void ThenTheEpicsFrontmatterIsChecked() =>
        Report.Violations.Should().Contain(violation => violation.RuleId == "SPEC004" && violation.File == _epic);

    [Then("the companion's tags are resolved against the specification")]
    public void ThenTheCompanionsTagsAreResolvedAgainstTheSpecification()
    {
        Report.Violations.Should().NotContain(static violation => violation.RuleId == "SPEC020");
        Report.Violations.Where(static violation => violation.RuleId == "SPEC021")
            .Should().ContainSingle()
            .Which.Should().Match<SpecViolation>(static violation => violation.File == Companion && violation.Identifier == "B-404");
    }

    [Then("the summary and the report name the layouts {string} and {string}")]
    public void ThenTheSummaryAndTheReportNameTheLayouts(string first, string second)
    {
        Summary[0].Should().Be($"specifications: {first} 1, {second} 1");
        JsonNode.Parse(SpecReportDocument.From(Report).ToJson())!["layouts"]!.AsArray()
            .Select(static layout => layout!["layout"]!.GetValue<string>())
            .Should().Equal(first, second);
    }

    [AfterScenario]
    public void DeleteTree() => _tree?.Dispose();

    private static IReadOnlyList<string> ThreeDefects =>
        SpecTree.SectionsWith(
                "3. Acceptance Criteria",
                "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n"
                    + "| B-001 | It does the thing. | brd | Active |\n| B-002 | It does another thing. | brd | Active |\n")
            .Where(static section => !section.StartsWith("## 11.", StringComparison.Ordinal))
            .ToList();

    private SpecTree Tree => _tree ?? throw new InvalidOperationException("No repository root was prepared.");

    private JsonObject Manifest => _manifest ?? throw new InvalidOperationException("No manifest was read.");

    private SpecCheckReport Report => _report ?? throw new InvalidOperationException("The check gave no report.");

    private string Subject => _subject ?? throw new InvalidOperationException("No step named that specification.");

    private string ManifestPath => Path.Combine(Tree.Root, ".spec", "schema", "spec-structure.schema.json");

    private IEnumerable<string> Reported => Report.Violations.Select(static violation => violation.File).Distinct();

    private IReadOnlyList<string> Summary => SpecReportDocument.From(Report).SummaryLines();

    private static JsonObject Layout(string name, string glob) => new() { ["name"] = name, ["glob"] = glob };

    private static (string RuleId, SpecSeverity Severity, int Line, string? Identifier, string Message) Verdict(SpecViolation violation) =>
        (violation.RuleId, violation.Severity, violation.Line, violation.Identifier, violation.Message);

    private string Write(
        string path,
        string id,
        IReadOnlyDictionary<string, string>? frontmatter = null,
        IReadOnlyList<string>? sections = null) =>
        Tree.WriteSpecification(path, "0001", id, frontmatter, sections);

    private const string LegacyGlob = "epics/**/spec.md";

    private const string CoLocatedGlob = "**/.spec/README.md";

    private const string Companion = "src/area/.spec/thing.gherkin";

    private const string PhantomTag =
        "Feature: it\n\n  @B-001\n  Scenario: It does the thing\n    Given a thing\n\n  @B-404\n  Scenario: Phantom\n    Given nothing\n";

    private SpecTree? _tree;
    private JsonObject? _manifest;
    private SpecCheckReport? _report;
    private string? _subject;
    private string? _legacy;
    private string? _coLocated;
    private string? _rootReadme;
    private string? _epic;
    private IReadOnlyList<string> _pair = [];
}
