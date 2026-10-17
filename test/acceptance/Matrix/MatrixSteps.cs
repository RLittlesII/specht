using System.Text.Json.Nodes;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Specht.Composition;
using Specht.Model;
using Specht.Report;
using Specht.Rules;
using Specht.Tests.Shared;
using Specht.Tool;

namespace Specht.Acceptance.Matrix;

/// <summary>Exercises the shipped waiver and retirement rules over an invented repository (0001-F12 B-018 to B-020).</summary>
[Binding]
[Scope(Feature = "Matrix resolution")]
public sealed class MatrixSteps
{
    [Given("a repository whose manifest pins schema version 0.2.0")]
    public void GivenRepository()
    {
        _tree = new SpecTree();
        _manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(Tree.Root, SpecTree.ManifestPath)))!.AsObject();
        Manifest["schemaVersion"] = "0.2.0";
    }

    [Given("a Feature specification beside one companion scenario file")]
    public void GivenFeature() => _tags = string.Empty;

    [Given("the manifest sets the waiver rule to error")]
    public void GivenWaiverEnabled() => Rules["SPEC034"] = "error";

    [Given("the manifest sets the retired-claim rule to error")]
    public void GivenRetiredEnabled() => Rules["SPEC035"] = "error";

    [Given("the manifest sets no level for either rule")]
    public void GivenRulesOmitted() => Manifest.Remove("rules");

    [Given("the manifest sets the waiver rule and the retired-claim rule to off")]
    public void GivenRulesOff()
    {
        Rules["SPEC034"] = "off";
        Rules["SPEC035"] = "off";
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

    [Given("the specification's matrix has a covered row no scenario carries and a status that is no known marker")]
    public void GivenUnresolvedOriginalRow()
    {
        _tags = string.Empty;
        _status = "UnknownState";
        _test = "Missing";
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
        var sections = SpecTree.Sections.Select(section => section.Split('\n')[0] switch
        {
            "## 3. Acceptance Criteria" => "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| --- | --- | --- | --- |\n"
                + $"| B-004 | Behavior | Decision | {_claim} |\n",
            "## 9. Traceability Matrix" => "## 9. Traceability Matrix\n\n| Claim ID | Scenario | Test | Status |\n| --- | --- | --- | --- |\n"
                + $"| B-004 | A scenario | {_test} | {_status} |\n",
            "## 12. Sign-off" => $"## 12. Sign-off\n\n| {_headers} |\n| --- | --- | --- | --- |\n| 1-5 | {_signOff} | {_reviewer} | Decision |\n",
            _ => section,
        }).ToArray();
        var path = Tree.WriteFeature("0001", "F1", sections: sections);
        Tree.WriteFeatureFile(path, $"Feature: Sample\n{_tags}Scenario: A scenario\n  Given a thing\n");
        Tree.WriteRaw(SpecTree.ManifestPath, Manifest.ToJsonString());
        _report = EngineServices.Runner().Run(Tree.Root);
    }

    [Then("^the waiver rule reports (nothing|one finding) for that row$")]
    public void ThenWaiverCount(string findings) => Findings("SPEC034").Should().HaveCount(findings == "nothing" ? 0 : 1);

    [Then("the waiver rule reports nothing")]
    public void ThenNoWaiver() => Findings("SPEC034").Should().BeEmpty();

    [Then("the retired-claim rule reports claim B-004")]
    public void ThenRetiredClaim() => Findings("SPEC035").Should().ContainSingle().Which.Identifier.Should().Be("B-004");

    [Then("^the retired-claim rule reports (nothing|one finding)$")]
    public void ThenRetiredCount(string findings) => Findings("SPEC035").Should().HaveCount(findings == "nothing" ? 0 : 1);

    [Then("no scenario-title, missing-scenario, unresolved-test, status, waiver or retired-claim finding is reported")]
    public void ThenNoMatrixFindings() => _report!.Violations.Should().NotContain(static violation =>
        violation.RuleId == "SPEC022" || violation.RuleId == "SPEC023" || violation.RuleId == "SPEC032"
            || violation.RuleId == "SPEC033" || violation.RuleId == "SPEC034" || violation.RuleId == "SPEC035");

    [Then("the report is byte-for-byte the one the tool gave that tree before this Feature")]
    public void ThenOriginalReport()
    {
        using var services = (TypeResolver)new TypeRegistrar(new ServiceCollection().AddSpechtEngine()).Build();
        var original = ((IEnumerable<ISpecRule>)services.Resolve(typeof(IEnumerable<ISpecRule>))!).Where(static rule =>
            rule.Id != "SPEC034" && rule.Id != "SPEC035");
        var baseline = SpechtRunner.Evaluate(EngineServices.Loader().Load(Tree.Root), original);
        SpecReportDocument.From(_report!).ToJson().Should().Be(SpecReportDocument.From(baseline).ToJson());
        _report!.RulesEvaluated.Should().Be(21);
    }

    [AfterScenario]
    public void DisposeTree() => _tree?.Dispose();

    private SpecTree Tree => _tree ?? throw new InvalidOperationException("No tree was prepared.");
    private JsonObject Manifest => _manifest ?? throw new InvalidOperationException("No manifest was read.");
    private JsonObject Rules => Manifest["rules"] as JsonObject ?? (JsonObject)(Manifest["rules"] = new JsonObject());

    private IEnumerable<SpecViolation> Findings(string id) => (_report ?? throw new InvalidOperationException("No check ran.")).Violations
        .Where(violation => violation.RuleId == id);

    private SpecTree? _tree;
    private JsonObject? _manifest;
    private SpechtReport? _report;
    private string _claim = "Active";
    private string _status = "Missing";
    private string _test = "Missing";
    private string _tags = string.Empty;
    private string _reviewer = "spec-reviewer";
    private string _signOff = "approved";
    private string _headers = "Section | Status | Reviewer | Note";
}
