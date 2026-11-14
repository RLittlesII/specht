using System.Text.Json.Nodes;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Specht.Composition;
using Specht.Evaluation;
using Specht.Model;
using Specht.Report;
using Specht.Rules;
using Specht.Tests.Shared;
using Specht.Tool;

namespace Specht.Acceptance.Matrix;

/// <summary>Exercises the shipped matrix rules over an invented repository (0001-F12 B-001 to B-020, B-022 to B-028).</summary>
[Binding]
[Scope(Feature = "Matrix resolution")]
public sealed class MatrixSteps
{
    [Given("a repository whose manifest pins schema version 0.2.0")]
    public void GivenRepository()
    {
        _tree = new SpechtTree();
        Manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(Tree.Root, SpechtTree.ManifestPath)))!.AsObject();
        Manifest["schemaVersion"] = "0.2.0";
    }

    [Given("a Feature specification beside one companion scenario file")]
    public void GivenFeature() => _tags = string.Empty;

    [Given("the manifest sets the waiver rule to error")]
    public void GivenWaiverEnabled() => Rules[RuleIds.Under(Manifest, "SPEC034")] = "error";

    [Given("the manifest sets the retired-claim rule to error")]
    public void GivenRetiredEnabled() => Rules[RuleIds.Under(Manifest, "SPEC035")] = "error";

    [Given("the manifest sets no level for either rule")]
    public void GivenRulesOmitted() => Manifest.Remove("rules");

    [Given("the manifest sets the waiver rule and the retired-claim rule to off")]
    public void GivenRulesOff()
    {
        Rules[RuleIds.Under(Manifest, "SPEC034")] = "off";
        Rules[RuleIds.Under(Manifest, "SPEC035")] = "off";
    }

    [Given("the specification's sign-off has a row by {string} marked {word}")]
    public void GivenSignOff(string reviewer, string status)
    {
        _reviewer = reviewer;
        _signOff = status is "draft" or "blocked" ? Manifest["markers"]![status]!.GetValue<string>() : status;
    }

    [Given("a row whose Status is {string} and whose Test cell is {string}")]
    public void GivenMatrixRow(string status, string cell)
    {
        _status = status;
        _test = cell;
    }

    [Given("a row whose Status is {string} with nothing in its Test cell")]
    public void GivenEmptyTest(string status) => GivenMatrixRow(status, string.Empty);

    [Given("a claim B-004 marked {string}")]
    public void GivenClaim(string status) => _claim = status;

    [Given("a scenario tagged @B-004")]
    public void GivenTag() => _tags = "@B-004\n";

    [Given("a claim B-004 marked {string} whose tag no scenario carries")]
    public void GivenUntaggedClaim(string status)
    {
        _claim = status;
        _tags = string.Empty;
    }

    [Given("its matrix row's Status is {string}")]
    public void GivenMatrixStatus(string status) => _status = status;

    [Given("an active claim B-004 whose tag no scenario carries")]
    public void GivenActiveUntaggedClaim() => GivenUntaggedClaim("Active");

    [Given("an active claim B-004 whose only tag is on a Rule that holds no scenario")]
    public void GivenTagOnEmptyRule()
    {
        GivenActiveUntaggedClaim();
        _scenarios = "@B-004\nRule: A rule\n";
    }

    [Given("an active claim B-004 whose only tag is on the companion's Feature, which holds no scenario")]
    public void GivenTagOnEmptyFeature()
    {
        GivenActiveUntaggedClaim();
        _featureTags = "@B-004\n";
        _defaultScenario = string.Empty;
    }

    [Given("two matrix rows for claim B-004, each with Status {string}")]
    public void GivenTwoRows(string status)
    {
        _status = status;
        _rowCount = 2;
    }

    [Given("no matrix row for claim B-004")]
    public void GivenNoRow() => _rowCount = 0;

    [Given("its matrix row's Status is the missing marker")]
    public void GivenMissingStatus() => _status = Manifest["markers"]!["missing"]!.GetValue<string>();

    [Given("a claim marked {string} that a scenario still carries")]
    public void GivenRetiredTaggedClaim(string status)
    {
        GivenClaim(status);
        GivenTag();
    }

    [Given("the manifest pins schema version {string}")]
    public void GivenPin(string pin) => Manifest["schemaVersion"] = pin;

    [Given("the manifest pins schema version 0.1.0 instead")]
    public void GivenOriginalPin() => GivenPin("0.1.0");

    [Given("the manifest pins schema version 0.3.0 instead")]
    public void GivenMatrixPin() => GivenPin("0.3.0");

    [Given("the manifest pins schema version 0.4.0 instead")]
    public void GivenMissingScenarioPin() => GivenPin("0.4.0");

    [Given("the manifest names the matrix headers {string}, {string}, {string} and {string}")]
    public void GivenMatrixHeaders(string claim, string scenario, string test, string status)
    {
        Manifest["tables"]!["matrix"] = new JsonArray(claim, scenario, test, status);
        _renamed = $"{claim} | {scenario} | {test} | {status}";
    }

    [Given("the specification's matrix uses those headers")]
    public void GivenMatrixUsesHeaders() => _matrix = _renamed;

    [Given("the manifest declares the test files")]
    public void GivenDefaultTestFiles() => GivenTestFiles("test/**/*.cs");

    [Given("the manifest declares the test files {string}")]
    public void GivenTestFiles(string glob)
    {
        Manifest["tests"] = new JsonObject { ["files"] = new JsonArray(glob) };
        _extension = Path.GetExtension(glob);
    }

    [Given("the manifest declares the test files {string} and no declaration forms")]
    public void GivenTestFilesWithoutForms(string glob) => GivenTestFiles(glob);

    [Given("the manifest declares the test files {string} and the declaration form {string}")]
    public void GivenTestFilesWithForm(string glob, string form)
    {
        GivenTestFiles(glob);
        Manifest["tests"]!["declarations"] = new JsonArray(form);
    }

    [Given("the manifest declares no tests")]
    public void GivenNoTests() => Manifest.Remove("tests");

    [Given("the manifest excludes {string}")]
    public void GivenExclusion(string exclusion)
    {
        var exclusions = Manifest["exclusions"] as JsonArray ?? (JsonArray) (Manifest["exclusions"] = new JsonArray());
        if (!exclusions.Any(existing => existing!.GetValue<string>() == exclusion))
        {
            exclusions.Add(exclusion);
        }
    }

    [Given("a row whose third cell names a test no declared file holds")]
    public void GivenUnresolvedThirdCell() => _test = "`ScenarioNobodyWrote`";

    [Given("a row whose fourth cell is {string} and whose third cell names no approver")]
    public void GivenUnapprovedFourthCell(string status) => GivenMatrixRow(status, string.Empty);

    [Given("a matrix row for claim {word} whose Test cell is {string}")]
    public void GivenClaimTestCell(string claim, string cell)
    {
        _claimId = claim;
        _test = cell;
    }

    [Given("no file under test holds that name")]
    public void GivenNameNowhere() => WriteTest("Other", "class Other {}\n");

    [Given("a matrix row whose Test cell names a test held only under {string}")]
    public void GivenExcludedTest(string folder)
    {
        Tree.WriteRaw(folder + "Hidden" + _extension, "void HiddenTest() {}\n");
        _test = "`HiddenTest`";
    }

    [Given("a matrix row whose Test cell is the missing marker")]
    public void GivenMissingTestCell() => _test = Manifest["markers"]!["missing"]!.GetValue<string>();

    [Given("a matrix row whose Status is {string} and whose Test cell is {string}")]
    public void GivenStatusAndTestCell(string status, string cell) => GivenMatrixRow(status, cell);

    [Given("a matrix row whose Test cell is {string}")]
    public void GivenTestCell(string cell) => _test = cell;

    [Given("a matrix row whose Test cell is {string} with no inline code")]
    public void GivenPlainTestCell(string cell) => _test = cell;

    [Given("a test file holding {string}")]
    public void GivenTestText(string text) => WriteTest("Holding", text + "\n");

    [Given("a test file holding {string} and {string}")]
    public void GivenTestNames(string first, string second) => WriteTest("Names", $"void {first}() {{}}\nvoid {second}() {{}}\n");

    [Given("a test file holding the word {string}")]
    public void GivenTestWord(string word) => WriteTest("Word", $"void {word}() {{}}\n");

    [Given("^a test file holding the word \"(\\w+)\" and not the word \"\\w+\"$")]
    public void GivenLongerWord(string word) => GivenTestWord(word);

    [Given("a test file that holds {string} only in a comment")]
    public void GivenCommentedName(string name) => WriteTest("Commented", $"// {name}\n");

    [Given("a test file holding the class {string} and the method {string}")]
    public void GivenClassAndMethod(string type, string method) => WriteTest("Steps", $"class {type}\n{{\n    void {method}() {{}}\n}}\n");

    [Given("one test file holding the class {string} and another holding the method {string}")]
    public void GivenSplitClassAndMethod(string type, string method)
    {
        WriteTest("Type", $"class {type}\n{{\n}}\n");
        WriteTest("Method", $"void {method}() {{}}\n");
    }

    [Given("a test file holding the method {string} and the words {string} and {string}")]
    public void GivenMethodAndWords(string method, string first, string second) =>
        WriteTest("Qualified", $"void {method}() {{}}\n// {first} {second}\n");

    [Given("a test file holding the method {string} and the words {string} alone")]
    public void GivenMethodAndWord(string method, string word) => WriteTest("Qualified", $"void {method}() {{}}\n// {word}\n");

    [Given("the manifest turns scenario title matching on")]
    public void GivenTitlesOn() => Manifest["matrix"] = new JsonObject { ["scenarioTitles"] = true };

    [Given("the manifest turns scenario title matching off")]
    public void GivenTitlesOff() => Manifest["matrix"] = new JsonObject { ["scenarioTitles"] = false };

    [Given("the manifest does not mention title matching")]
    public void GivenTitlesUnmentioned() => Manifest.Remove("matrix");

    [Given("a matrix row whose Scenario cell is {string}")]
    public void GivenScenarioCell(string cell) => _scenario = cell;

    [Given("the matrix row for claim {word} whose Scenario cell is {string}")]
    public void GivenClaimScenarioCell(string claim, string cell)
    {
        _claimId = claim;
        _scenario = cell;
    }

    [Given("scenarios titled {string} and {string}, both tagged with the row's claim")]
    public void GivenTaggedTitles(string first, string second)
    {
        GivenTitledScenario(first, _claimId);
        GivenTitledScenario(second, _claimId);
    }

    [Given("a scenario titled {string} tagged @{word}")]
    public void GivenTitledScenario(string title, string claim) => _scenarios += $"@{claim}\nScenario: {title}\n  Given a thing\n";

    [Given("a scenario titled {string} tagged with the row's claim")]
    public void GivenClaimTaggedScenario(string title) => GivenTitledScenario(title, _claimId);

    [Given("^a scenario titled \"([^\"]*)\" whose only @(\\S+) tag is on the (.+)$")]
    public void GivenInheritedTag(string title, string claim, string where)
    {
        _scenarios += where switch
        {
            "companion's Feature" => $"Scenario: {title}\n  Given a thing\n",
            "Rule the scenario sits under" => $"@{claim}\nRule: A rule\nScenario: {title}\n  Given a thing\n",
            "Examples of the outline the scenario is" => $"Scenario Outline: {title}\n  Given <thing>\n@{claim}\nExamples:\n  | thing |\n  | one |\n",
            _ => throw new ArgumentOutOfRangeException(nameof(where), where, "No such place for a claim tag."),
        };
        _featureTags = where == "companion's Feature" ? $"@{claim}\n" : _featureTags;
    }

    [Given("a matrix row whose Status is {string} and whose Scenario cell is {string}")]
    public void GivenStatusAndScenarioCell(string status, string cell)
    {
        _status = status;
        _scenario = cell;
    }

    [Given("the manifest reads claim tags ignoring letter case")]
    public void GivenTagIgnoreCase() => Claim["tagIgnoreCase"] = true;

    [Given("the manifest does not mention the claim tag's case")]
    public void GivenTagCaseUnmentioned() => Claim.Remove("tagIgnoreCase");

    [Given("the specification's matrix has a covered row no scenario carries and a status that is no known marker")]
    public void GivenUnresolvedOriginalRow()
    {
        _tags = string.Empty;
        _status = "UnknownState";
        _test = "Missing";
    }

    [Given("^a row whose Status is \"([^\"]*)\", whose scenario (resolves|does not resolve) and whose tests (.+)$")]
    public void GivenStatusScenarioAndTests(string status, string scenario, string tests)
    {
        GivenStatusAndScenario(status, scenario);
        GivenTestNames("AlphaTest", "BetaTest");
        _test = tests switch
        {
            "all resolve" => "`AlphaTest`, `BetaTest`",
            "resolve only in part" => "`AlphaTest`, `GammaTest`",
            "none resolve" => "`GammaTest`, `DeltaTest`",
            "are the missing marker" => Manifest["markers"]!["missing"]!.GetValue<string>(),
            _ => throw new ArgumentOutOfRangeException(nameof(tests), tests, "No such set of tests."),
        };
    }

    [Given("^a row whose Status is \"([^\"]*)\", whose scenario (resolves|does not resolve) and whose Test cell is the missing marker$")]
    public void GivenStatusScenarioAndMissingTestCell(string status, string scenario)
    {
        GivenStatusAndScenario(status, scenario);
        GivenMissingTestCell();
    }

    [Given("^a row whose Status is \"([^\"]*)\", whose scenario (resolves|does not resolve) and whose Test cell names a test no file holds$")]
    public void GivenStatusScenarioAndUnheldTest(string status, string scenario)
    {
        GivenStatusAndScenario(status, scenario);
        GivenUnresolvedThirdCell();
    }

    [Given("a row whose Status is {string} and whose Test cell names tests that all resolve")]
    public void GivenStatusAndResolvedTests(string status) => GivenStatusAndHeldTests(status, "`AlphaTest`, `BetaTest`");

    [Given("a row whose Status is {string} and whose Test cell names one test that resolves and one that does not")]
    public void GivenStatusAndPartlyResolvedTests(string status) => GivenStatusAndHeldTests(status, "`AlphaTest`, `GammaTest`");

    [Given("a row whose Status is {string} and whose Test cell names a test")]
    public void GivenStatusAndNamedTest(string status) => GivenStatusAndHeldTests(status, "`AlphaTest`");

    [Given("a row whose Status is {string}")]
    public void GivenStatusOfACarriedClaim(string status)
    {
        _tags = "@B-004\n";
        _status = status;
    }

    [Given("^a row whose Status is \"([^\"]*)\", whose scenario (resolves|does not resolve) and whose Test cell is \"([^\"]*)\" with no inline code$")]
    public void GivenStatusScenarioAndPlainTestCell(string status, string scenario, string cell)
    {
        GivenStatusAndScenario(status, scenario);
        GivenPlainTestCell(cell);
    }

    [Given("a matrix row for claim B-004 whose Status is {string} and whose tests all resolve")]
    public void GivenRowWithResolvedTests(string status) => GivenStatusAndHeldTests(status, "`AlphaTest`, `BetaTest`");

    [Given("its matrix row's Status is {string} and its tests all resolve")]
    public void GivenMatrixStatusWithResolvedTests(string status) => GivenRowWithResolvedTests(status);

    [Given("claim B-004 is marked {string}, and no scenario carries its tag")]
    public void GivenRetiredUncarriedClaim(string status) => GivenUntaggedClaim(status);

    [Given("claim B-004 is not among the acceptance criteria, and no scenario carries its tag")]
    public void GivenUndeclaredUncarriedClaim()
    {
        _criterionId = "B-001";
        _tags = string.Empty;
    }

    [Given("the sign-off table uses the manifest's renamed column headers")]
    public void GivenRenamedHeaders()
    {
        _headers = "Area | Verdict | Approver | Reason";
        Manifest["tables"]!["signOff"] = new JsonArray("Area", "Verdict", "Approver", "Reason");
    }

    [Given("a waived row names an approver with a completed sign-off")]
    public void GivenCompletedWaiver() => GivenMatrixRow("Waived", "spec-reviewer, 2026-10-14");

    [When("the check runs with the waiver rule raised")]
    public void WhenWaiverRuns()
    {
        GivenWaiverEnabled();
        WhenCheckRuns();
    }

    [When("the check runs")]
    public void WhenCheckRuns()
    {
        var sections = SpechtTree.Sections.Select(section => section.Split('\n')[0] switch
        {
            "## 3. Acceptance Criteria" => "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| --- | --- | --- | --- |\n"
                + $"| {_criterionId ?? _claimId} | Behavior | Decision | {_claim} |\n",
            "## 9. Traceability Matrix" => $"## 9. Traceability Matrix\n\n| {_matrix} |\n| --- | --- | --- | --- |\n"
                + string.Concat(Enumerable.Repeat($"| {_claimId} | {_scenario} | {_test} | {_status} |\n", _rowCount)),
            "## 12. Sign-off" => $"## 12. Sign-off\n\n| {_headers} |\n| --- | --- | --- | --- |\n| 1-5 | {_signOff} | {_reviewer} | Decision |\n",
            _ => section,
        }).ToArray();
        var path = Tree.WriteFeature("0001", "F1", sections: sections);
        Tree.WriteFeatureFile(path, $"{_featureTags}Feature: Sample\n{_tags}{_defaultScenario}{_scenarios}");
        Tree.WriteRaw(SpechtTree.ManifestPath, Manifest.ToJsonString());
        _rows = [.. File.ReadAllLines(path)
            .Select(static (line, index) => (line, index))
            .Where(entry => entry.line.StartsWith($"| {_claimId} | {_scenario} |", StringComparison.Ordinal))
            .Select(static entry => entry.index + 1)];
        _row = _rows.FirstOrDefault();
        _report = EngineServices.Runner().Run(Tree.Root);
    }

    [Then("^the waiver rule reports (nothing|one finding) for that row$")]
    public void ThenWaiverCount(string findings) => Findings(RuleIds.Under(Manifest, "SPEC034")).Should().HaveCount(findings == "nothing" ? 0 : 1);

    [Then("the waiver rule reports that row")]
    public void ThenWaiverRow() => Findings(RuleIds.Under(Manifest, "SPEC034")).Should().ContainSingle().Which.Line.Should().Be(_row);

    [Then("the unresolved-test rule reports that row")]
    public void ThenUnresolvedRow() => UnresolvedRow();

    [Then("the unresolved-test rule reports that row, with {word} as its identifier")]
    public void ThenUnresolvedIdentifier(string claim) => UnresolvedRow().Identifier.Should().Be(claim);

    [Then("the unresolved-test rule reports one finding, {word} for that row")]
    public void ThenUnresolvedName(string name) => UnresolvedRow().Message.Should().Contain(name);

    [Then("the unresolved-test rule reports that row, expecting a test name in inline code")]
    public void ThenNoInlineCode() => UnresolvedRow().Message.Should().Contain("inline code");

    [Then("^the unresolved-test rule reports (nothing|one finding) for that row$")]
    public void ThenUnresolvedCount(string findings)
    {
        if (findings == "nothing")
        {
            Findings(RuleIds.Under(Manifest, "SPEC032")).Should().BeEmpty();
        }
        else
        {
            UnresolvedRow();
        }
    }

    [Then("the unresolved-test rule reports nothing")]
    public void ThenNoUnresolved() => Findings(RuleIds.Under(Manifest, "SPEC032")).Should().BeEmpty();

    [Then("the scenario-title rule reports that row")]
    public void ThenTitleRow() => Findings(RuleIds.Under(Manifest, "SPEC022")).Should().ContainSingle().Which.Line.Should().Be(_row);

    [Then("^the scenario-title rule reports (nothing|one finding) for that row$")]
    public void ThenTitleCount(string findings)
    {
        if (findings == "nothing")
        {
            Findings(RuleIds.Under(Manifest, "SPEC022")).Should().BeEmpty();
        }
        else
        {
            ThenTitleRow();
        }
    }

    [Then("the scenario-title rule reports nothing")]
    public void ThenNoTitle() => Findings(RuleIds.Under(Manifest, "SPEC022")).Should().BeEmpty();

    [Then("the missing-scenario rule reports that row")]
    public void ThenMissingScenarioRow() => Findings(RuleIds.Under(Manifest, "SPEC023")).Should().ContainSingle().Which.Line.Should().Be(_row);

    [Then("the missing-scenario rule reports each of those rows")]
    public void ThenMissingScenarioRows() =>
        Findings(RuleIds.Under(Manifest, "SPEC023")).Select(static finding => finding.Line).Should().BeEquivalentTo(_rows).And.HaveCount(2);

    [Then("the missing-scenario rule reports nothing")]
    public void ThenNoMissingScenario() => Findings(RuleIds.Under(Manifest, "SPEC023")).Should().BeEmpty();

    [Then("the status rule reports that row")]
    public void ThenStatusRow() => StatusRow();

    [Then("^the status rule reports (nothing|one finding) for that row$")]
    public void ThenStatusCount(string findings)
    {
        if (findings == "nothing")
        {
            Findings(RuleIds.Under(Manifest, "SPEC033")).Should().BeEmpty();
        }
        else
        {
            StatusRow();
        }
    }

    [Then("the status rule reports that row as stale")]
    public void ThenStaleRow() => StatusRow().Message.Should().Contain("stale");

    [Then("the status rule reports that row, expecting one of the covered, partial, missing, waived, withdrawn and superseded markers")]
    public void ThenUnknownStatus()
    {
        var finding = StatusRow();
        foreach (var marker in new[] { "covered", "partial", "missing", "waived", "withdrawn", "superseded" })
        {
            finding.Message.Should().Contain(Manifest["markers"]![marker]!.GetValue<string>());
        }
    }

    [Then("the waiver rule reports nothing")]
    public void ThenNoWaiver() => Findings(RuleIds.Under(Manifest, "SPEC034")).Should().BeEmpty();

    [Then("the retired-claim rule reports claim B-004")]
    public void ThenRetiredClaim() => Findings(RuleIds.Under(Manifest, "SPEC035")).Should().ContainSingle().Which.Identifier.Should().Be("B-004");

    [Then("^the retired-claim rule reports (nothing|one finding)$")]
    public void ThenRetiredCount(string findings) => Findings(RuleIds.Under(Manifest, "SPEC035")).Should().HaveCount(findings == "nothing" ? 0 : 1);

    [Then("no scenario-title, missing-scenario, unresolved-test, status, waiver or retired-claim finding is reported")]
    public void ThenNoMatrixFindings()
    {
        string[] matrix = [.. new[] { "SPEC022", "SPEC023", "SPEC032", "SPEC033", "SPEC034", "SPEC035" }.Select(id => RuleIds.Under(Manifest, id))];
        _report!.Violations.Should().NotContain(violation => matrix.Contains(violation.RuleId));
    }

    [Then("the report is byte-for-byte the one the tool gave that tree before this Feature")]
    public void ThenOriginalReport()
    {
        using var services = (TypeResolver) new TypeRegistrar(new ServiceCollection().AddSpechtEngine()).Build();
        var original = ((IEnumerable<ISpechtRule>) services.Resolve(typeof(IEnumerable<ISpechtRule>))!).Where(static rule =>
            rule.Id is not ("SPEC034" or "SPEC035"));
        var baseline = new SpechtEvaluator(original, new SpechtViolationOrder()).Evaluate(EngineServices.Reader().Read(Tree.Root, null));
        SpechtReportDocument.From(_report!).ToJson().Should().Be(SpechtReportDocument.From(baseline).ToJson());
        _report!.RulesEvaluated.Should().Be(21);
    }

    [AfterScenario]
    public void DisposeTree() => _tree?.Dispose();

    private SpechtTree Tree => _tree ?? throw new InvalidOperationException("No tree was prepared.");
    private JsonObject Manifest { get => field ?? throw new InvalidOperationException("No manifest was read."); set; }
    private JsonObject Rules => Manifest["rules"] as JsonObject ?? (JsonObject) (Manifest["rules"] = new JsonObject());
    private JsonObject Claim => Manifest["claim"] as JsonObject ?? (JsonObject) (Manifest["claim"] = new JsonObject());

    private SpechtViolation UnresolvedRow()
    {
        var finding = Findings(RuleIds.Under(Manifest, "SPEC032")).Should().ContainSingle().Subject;
        finding.Line.Should().Be(_row);
        return finding;
    }

    private SpechtViolation StatusRow()
    {
        var finding = Findings(RuleIds.Under(Manifest, "SPEC033")).Should().ContainSingle().Subject;
        finding.Line.Should().Be(_row);
        return finding;
    }

    private void GivenStatusAndScenario(string status, string scenario)
    {
        _tags = "@B-004\n";
        _status = status;
        _scenario = scenario == "resolves" ? "A scenario" : "A title no scenario has";
    }

    private void GivenStatusAndHeldTests(string status, string cell)
    {
        GivenTestNames("AlphaTest", "BetaTest");
        _status = status;
        _test = cell;
    }

    private void WriteTest(string name, string text) => Tree.WriteRaw($"test/{name}{_extension}", text);

    private IEnumerable<SpechtViolation> Findings(string id) => (_report ?? throw new InvalidOperationException("No check ran.")).Violations
        .Where(violation => violation.RuleId == id);

    private SpechtTree? _tree;
    private SpechtReport? _report;
    private string _claim = "Active";
    private string _claimId = "B-004";
    private string? _criterionId;
    private string _matrix = "Claim ID | Scenario | Test | Status";
    private string _renamed = string.Empty;
    private string _extension = ".cs";
    private int _row;
    private int[] _rows = [];
    private int _rowCount = 1;
    private string _defaultScenario = "Scenario: A scenario\n  Given a thing\n";
    private string _status = "Missing";
    private string _test = "Missing";
    private string _tags = string.Empty;
    private string _scenario = "A scenario";
    private string _scenarios = string.Empty;
    private string _featureTags = string.Empty;
    private string _reviewer = "spec-reviewer";
    private string _signOff = "approved";
    private string _headers = "Section | Status | Reviewer | Note";
}
