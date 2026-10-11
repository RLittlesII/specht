using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;
using Specht.Model;
using Specht.Report;
using Specht.Tests.Shared;

namespace Specht.Acceptance.Discovery;

/// <summary>
/// Steps for <c>src/specht/Discovery/.spec/discovery.feature</c> (0001-F6): item 0005 binds B-001, B-002, B-003, B-009 and B-012,
/// and item 0006 binds B-004.
/// The manifest is edited in memory from the tree's default copy and written when the check runs, and "the check runs" is
/// the engine's runner; the summary is the lines its report document makes and the report is that document serialized,
/// which <c>0001-F2</c> B-002 and <c>0001-F3</c> B-001 hold the tool's own output to. A specification a step writes has no
/// companion beside it unless the step says so, so one that is discovered is reported under its own path and one that is
/// not is reported nowhere. A layout's path identity is written into its <c>layouts</c> entry as <c>identity</c>, and a
/// specification is reported for the mismatch when <c>SPEC011</c> names its file once (B-004; decision 0009). The scenarios
/// of B-005 to B-008, B-010 and B-011 stay pending for their items.
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
            Layout("epics", EpicsGlob),
            Layout("features", FeaturesGlob),
            Layout("documentation", "docs/**/specification.md"));

    [Given("the root holds a specification at that place")]
    public void GivenTheRootHoldsASpecificationAtThatPlace() =>
        Write(_subject = "docs/guide/specification.md", "F1", sections: ThreeDefects);

    [Given("the manifest declares one layout only, the features layout")]
    public void GivenTheManifestDeclaresOneLayoutOnlyTheFeaturesLayout() =>
        Manifest["layouts"] = new JsonArray(Layout("features", FeaturesGlob));

    [Given("the root holds a specification where the default epics layout would find it")]
    public void GivenTheRootHoldsASpecificationWhereTheDefaultEpicsLayoutWouldFindIt() =>
        Write(_underEpics = "epics/0001-epic/F1-feature/spec.md", "F1");

    [Given("the root holds a specification where the features layout finds it")]
    public void GivenTheRootHoldsASpecificationWhereTheFeaturesLayoutFindsIt() =>
        Write(_besideCode = "src/area/.spec/README.md", "F2");

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
        Manifest["taskFiles"] = new JsonArray("{task}-*.markdown");
        Manifest["epicFiles"] = new JsonArray("epics/*/epic.md");
        Manifest["companionFiles"] = new JsonArray("*.gherkin");
    }

    [Given("the manifest declares two item file shapes, two epic file globs and two companion globs")]
    public void GivenTheManifestDeclaresTwoItemFileShapesTwoEpicFileGlobsAndTwoCompanionGlobs()
    {
        Manifest["taskFiles"] = new JsonArray("{task}-*.md", "{task}-*.markdown");
        Manifest["epicFiles"] = new JsonArray("epics/**/epic.md", "portfolio/*/epic.md");
        Manifest["companionFiles"] = new JsonArray("*.feature", "*.gherkin");
    }

    [Given("the root holds, beside a specification, one item matching the first item shape and one matching the second")]
    public void GivenTheRootHoldsBesideASpecificationOneItemMatchingTheFirstItemShapeAndOneMatchingTheSecond()
    {
        var specification = Write(
            "src/area/.spec/README.md",
            "F1",
            new Dictionary<string, string> { ["children"] = "[\"0001-01\", \"0001-02\"]" });
        var directory = Path.GetDirectoryName(specification)!;
        Tree.WriteItem(specification, "0001-01", "F1");
        Tree.WriteItem(specification, "0001-02", "F1");
        File.Move(Path.Combine(directory, "0001-02-item.md"), Path.Combine(directory, "0001-02-item.markdown"));
    }

    [Given("the root holds one epic file matching the first epic glob and one matching the second")]
    public void GivenTheRootHoldsOneEpicFileMatchingTheFirstEpicGlobAndOneMatchingTheSecond()
    {
        var urgent = new Dictionary<string, string> { ["priority"] = "urgent" };
        Tree.WriteEpic("0002", urgent);
        var written = Path.Combine(Tree.Root, "epics", "0002-epic");
        File.Move(Path.Combine(written, "epic.md"), Tree.WriteRaw(SecondEpic, string.Empty), overwrite: true);
        Directory.Delete(written);
        Tree.WriteEpic("0001", urgent);
    }

    [Given("the root holds, beside a second specification, one companion matching the second companion glob only")]
    public void GivenTheRootHoldsBesideASecondSpecificationOneCompanionMatchingTheSecondCompanionGlobOnly()
    {
        Write(SecondSpecification, "F2");
        Tree.WriteRaw(SecondCompanion, PhantomTag);
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
    public void GivenTheManifestNamesItsLayouts(string first, string second) =>
        Manifest["layouts"] = new JsonArray(Layout(first, EpicsGlob), Layout(second, FeaturesGlob));

    [Given("the root holds one specification in each layout")]
    [Given("the root holds one specification under the epics folder and one beside code")]
    public void GivenTheRootHoldsOneSpecificationInEachLayout()
    {
        Write("epics/0001-epic/F1-feature/spec.md", "F1");
        Write("src/area/.spec/README.md", "F2");
    }

    [Given("the manifest's epics layout declares which path segments carry the epic and the Feature id")]
    public void GivenTheManifestsEpicsLayoutDeclaresWhichPathSegmentsCarryTheEpicAndTheFeatureId() =>
        Entry("epics")["identity"] = Identity(1, 2);

    [Given("the manifest's features layout declares none")]
    public void GivenTheManifestsFeaturesLayoutDeclaresNone() => Entry("features").Remove("identity");

    [Given("the root holds a specification in the epics layout whose epic directory does not match its frontmatter")]
    public void GivenTheRootHoldsASpecificationInTheEpicsLayoutWhoseEpicDirectoryDoesNotMatchItsFrontmatter() =>
        Write(_underEpics = "epics/0002-epic/F1-feature/spec.md", "F1");

    [Given("the root holds a specification in the features layout under a folder named nothing like its identity")]
    public void GivenTheRootHoldsASpecificationInTheFeaturesLayoutUnderAFolderNamedNothingLikeItsIdentity() =>
        Write(_besideCode = "src/area/.spec/README.md", "F2");

    [Given("the manifest declares a third layout under a documentation folder, two folders deeper than the epics layout")]
    public void GivenTheManifestDeclaresAThirdLayoutUnderADocumentationFolderTwoFoldersDeeperThanTheEpicsLayout()
    {
        _layoutFolder = "docs/archive/tree";
        Manifest["layouts"]!.AsArray().Add(Layout("documentation", $"{_layoutFolder}/**/spec.md"));
    }

    [Given("that layout declares the path segments that carry the epic and the Feature id at their places in it")]
    public void GivenThatLayoutDeclaresThePathSegmentsThatCarryTheEpicAndTheFeatureIdAtTheirPlacesInIt() =>
        Entry("documentation")["identity"] = Identity(3, 4);

    [Given("the root holds a specification in that layout whose epic and Feature folders match its frontmatter")]
    public void GivenTheRootHoldsASpecificationInThatLayoutWhoseEpicAndFeatureFoldersMatchItsFrontmatter() =>
        Write(_first = $"{_layoutFolder}/0001-epic/F1-feature/spec.md", "F1");

    [Given("the root holds a second specification in that layout whose Feature folder does not match its frontmatter")]
    public void GivenTheRootHoldsASecondSpecificationInThatLayoutWhoseFeatureFolderDoesNotMatchItsFrontmatter() =>
        Write(_subject = $"{_layoutFolder}/0001-epic/F9-feature/spec.md", "F2");

    [Given("the manifest renames its epics layout {string} and keeps the path segments that layout declares")]
    public void GivenTheManifestRenamesItsEpicsLayoutAndKeepsThePathSegmentsThatLayoutDeclares(string name)
    {
        _layoutFolder = "epics";
        Entry("epics")["name"] = name;
    }

    [Given("the root holds a specification in that layout whose Feature folder does not match its frontmatter")]
    public void GivenTheRootHoldsASpecificationInThatLayoutWhoseFeatureFolderDoesNotMatchItsFrontmatter() =>
        Write(_subject = $"{_layoutFolder}/0001-epic/F9-feature/spec.md", "F1");

    [When("the check runs")]
    public void WhenTheCheckRuns()
    {
        File.WriteAllText(ManifestPath, Manifest.ToJsonString());
        _report = SpechtRunner.Run(Tree.Root).UnwrapOk().Value;
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

    [Then("only the specification in the features layout is discovered")]
    public void ThenOnlyTheSpecificationInTheFeaturesLayoutIsDiscovered()
    {
        Reported.Should().Contain(_besideCode).And.NotContain(_underEpics);
        Report.SpecificationCount.Should().Be(1);
    }

    [Then("the summary names one layout")]
    public void ThenTheSummaryNamesOneLayout() => Summary[0].Should().Be("specifications: features 1");

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

    [Then("the summary counts two items")]
    public void ThenTheSummaryCountsTwoItems() => Summary[1].Should().Be("items: 2");

    [Then("both epics' frontmatter is checked")]
    public void ThenBothEpicsFrontmatterIsChecked() =>
        Report.Violations.Where(static violation => violation.RuleId == "SPEC004")
            .Select(static violation => violation.File)
            .Distinct()
            .Should().BeEquivalentTo(FirstEpic, SecondEpic);

    [Then("that companion's tags are resolved against the second specification")]
    public void ThenThatCompanionsTagsAreResolvedAgainstTheSecondSpecification()
    {
        Report.Violations.Should().NotContain(static violation => violation.RuleId == "SPEC020" && violation.File == SecondSpecification);
        Report.Violations.Where(static violation => violation.RuleId == "SPEC021")
            .Should().ContainSingle()
            .Which.Should().Match<SpecViolation>(static violation => violation.File == SecondCompanion && violation.Identifier == "B-404");
    }

    [Then("the summary and the report name the layouts {string} and {string}")]
    [Then("the summary and the report name the layouts {string} and {string}, in that order")]
    public void ThenTheSummaryAndTheReportNameTheLayouts(string first, string second)
    {
        Summary[0].Should().Be($"specifications: {first} 1, {second} 1");
        JsonNode.Parse(SpecReportDocument.From(Report).ToJson())!["layouts"]!.AsArray()
            .Select(static layout => layout!["layout"]!.GetValue<string>())
            .Should().Equal(first, second);
    }

    [Then("the specification in the epics layout is reported for the mismatch")]
    public void ThenTheSpecificationInTheEpicsLayoutIsReportedForTheMismatch() => Mismatches(_underEpics).Should().ContainSingle();

    [Then("the specification in the features layout is not")]
    public void ThenTheSpecificationInTheFeaturesLayoutIsNot() => Mismatches(_besideCode).Should().BeEmpty();

    [Then("the second specification is reported for the mismatch")]
    [Then("that specification is reported for the mismatch")]
    public void ThenThatSpecificationIsReportedForTheMismatch() => Mismatches(Subject).Should().ContainSingle();

    [Then("the first is not")]
    public void ThenTheFirstIsNot() => Mismatches(_first).Should().BeEmpty();

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

    private SpechtReport Report => _report ?? throw new InvalidOperationException("The check gave no report.");

    private string Subject => _subject ?? throw new InvalidOperationException("No step named that specification.");

    private string ManifestPath => Path.Combine(Tree.Root, ".spec", "schema", "spec-structure.schema.json");

    private IEnumerable<string> Reported => Report.Violations.Select(static violation => violation.File).Distinct();

    private IReadOnlyList<string> Summary => SpecReportDocument.From(Report).SummaryLines();

    private static JsonObject Layout(string name, string glob) => new() { ["name"] = name, ["glob"] = glob };

    private static JsonObject Identity(int epic, int feature) => new() { ["epic"] = epic, ["feature"] = feature };

    private static (string RuleId, SpecSeverity Severity, int Line, string? Identifier, string Message) Verdict(SpecViolation violation) =>
        (violation.RuleId, violation.Severity, violation.Line, violation.Identifier, violation.Message);

    private JsonObject Entry(string layout) =>
        Manifest["layouts"]!.AsArray().Select(static entry => entry!.AsObject()).Single(entry => entry["name"]!.GetValue<string>() == layout);

    private IEnumerable<SpecViolation> Mismatches(string? specification) =>
        Report.Violations.Where(violation =>
            violation.RuleId == "SPEC011"
            && violation.File == (specification ?? throw new InvalidOperationException("No step wrote that specification.")));

    private string Write(
        string path,
        string id,
        IReadOnlyDictionary<string, string>? frontmatter = null,
        IReadOnlyList<string>? sections = null) =>
        Tree.WriteSpecification(path, "0001", id, frontmatter, sections);

    private const string EpicsGlob = "epics/**/spec.md";

    private const string FeaturesGlob = "**/.spec/README.md";

    private const string Companion = "src/area/.spec/thing.gherkin";

    private const string FirstEpic = "epics/0001-epic/epic.md";

    private const string SecondEpic = "portfolio/0002-epic/epic.md";

    private const string SecondSpecification = "src/other/.spec/README.md";

    private const string SecondCompanion = "src/other/.spec/thing.gherkin";

    private const string PhantomTag =
        "Feature: it\n\n  @B-001\n  Scenario: It does the thing\n    Given a thing\n\n  @B-404\n  Scenario: Phantom\n    Given nothing\n";

    private SpecTree? _tree;
    private JsonObject? _manifest;
    private SpechtReport? _report;
    private string? _subject;
    private string? _underEpics;
    private string? _besideCode;
    private string? _rootReadme;
    private string? _layoutFolder;
    private string? _first;
    private string? _epic;
    private IReadOnlyList<string> _pair = [];
}
