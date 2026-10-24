using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;
using Specht.Acceptance.Documentation;
using Specht.Analysis.Bindings;
using Specht.Analysis.Scenarios;
using Specht.Analysis.TestEvidence;

namespace Specht.Acceptance.Scenarios;

[Binding]
[Scope(Tag = "0201-F4")]
public sealed class ScenarioBridgeSteps
{
    [Given("a scenario carrying a local claim tag and a determined effective scope")]
    public void GivenScenarioWithClaim() => Use(claimTags: ["B-001"], rows: [new ScenarioRow(null, "case-1", false)]);

    [Given("an outline producing two discovered test cases")]
    public void GivenOutlineWithTwoCases() =>
        Use(rows: [new ScenarioRow("row-1", "case-1", false), new ScenarioRow("row-2", "case-2", false)]);

    [Given("two scenarios sharing a handwritten step method")]
    public void GivenTwoScenariosSharingAStep()
    {
        Use(id: "scenario-1", line: 10, rows: [new ScenarioRow(null, "case-1", false)]);
        Use(id: "scenario-2", line: 20, rows: [new ScenarioRow(null, "case-2", false)]);
    }

    [Given("a selected Reqnroll project missing its required generator or discovery input")]
    public void GivenMissingInput() => Use(claimTags: ["B-001"], rows: [], generatorInputPresent: false);

    [Given("a consumer selecting only bound xUnit or Playwright .NET tests")]
    public void GivenOnlyDirectBindings() => _artifactJson = AcceptanceTestEvidence.ForMethod("M:Example.Tests.Sample.Run");

    [Given("Gherkin Feature-level scope")]
    public void GivenFeatureScope() => _featureScope = new ScenarioScope("0201", "F3");

    [Given("Gherkin Feature-level scope including an epic")]
    public void GivenFeatureScopeWithEpic() => _featureScope = new ScenarioScope("0201", "F4");

    [Given("a {string} without an explicit scope override")]
    public void GivenWithoutOverride(string kind) => Use(claimTags: ["B-001"], rows: Rows(kind));

    [Given("a {string} with its own explicit Feature scope and no epic")]
    public void GivenOverrideWithoutEpic(string kind) =>
        Use(claimTags: ["B-001"], rows: Rows(kind), scopeOverride: new ScenarioScope(null, "F3"));

    [Given("generator or runner scenario-to-case metadata supplied by {string}")]
    public void GivenProducerMetadata(string producer)
    {
        producer.Should().NotBeNullOrEmpty();
        Use(claimTags: ["B-001"], rows: [new ScenarioRow(null, "acceptance-case", false)]);
    }

    [Given("an outline whose first Examples block is marked active and whose second is marked planned")]
    public void GivenActiveThenPlanned() => Use(rows:
    [
        new ScenarioRow("row-1", "case-1", false), new ScenarioRow("row-2", "case-2", false),
        new ScenarioRow("row-3", "case-3", true), new ScenarioRow("row-4", "case-4", true),
    ]);

    [Given("an outline carrying one claim whose rows are partly planned")]
    public void GivenOneClaimPartlyPlanned() => Use(claimTags: ["B-001"], rows:
        [new ScenarioRow("row-1", "case-1", false), new ScenarioRow("row-2", "case-2", true)]);

    [Given("an outline row marked planned that has no discovered test case")]
    public void GivenPlannedRowWithoutCase() => _plannedRows.Add(new ScenarioRow("row-1", null, true));

    [Given("an outline row marked active whose generator input is missing")]
    public void GivenActiveRowWithoutInput()
    {
        _plannedRows.Add(new ScenarioRow("row-2", null, false));
        Use(rows: _plannedRows, generatorInputPresent: false);
    }

    [When("the declared analysis is requested")]
    [When("effective scope is determined")]
    [When("scenario evidence is mapped")]
    public void WhenMapped()
    {
        var resolver = new BindingResolver(Catalog(), Policy());
        _records = new ScenarioEvidenceBridge(resolver).Map(_metadata);
        if (_artifactJson is not null)
        {
            _result = new TestEvidenceArtifactReader().Read(_artifactJson);
        }
    }

    [Then("the tag resolves through the same identity contract as a code binding")]
    public void ThenTagResolves()
    {
        var expected = new BindingResolver(Catalog(), Policy()).Resolve(new ClaimReference("0201", "F3", "B-001"));
        var claim = Record().GetProperty("claims")[0];
        claim.GetProperty("status").GetString().Should().Be("resolved");
        claim.GetProperty("target").GetProperty("relativePath").GetString().Should().Be(expected.Target!.RelativePath);
    }

    [Then("each case links to the originating scenario and row through generator or runner identity")]
    public void ThenCasesLinkToRows()
    {
        Record().GetProperty("scenarioId").GetString().Should().Be("scenario-1");
        Links(Record()).Should().Equal(("row-1", "case-1", "resolved", "active"), ("row-2", "case-2", "resolved", "active"));
    }

    [Then("the scenario-to-test links remain separate and are not inferred from step documentation")]
    public void ThenLinksStaySeparate()
    {
        _records.Should().HaveCount(2);
        Links(_records[0]).Should().Equal((null, "case-1", "resolved", "active"));
        Links(_records[1]).Should().Equal((null, "case-2", "resolved", "active"));
    }

    [Then("the bridge reports an unresolved link")]
    public void ThenUnresolvedLink() => Links(Record()).Should().Equal((null, null, "unresolved", "active"));

    [Then("the absence of Reqnroll scenarios does not invalidate the core test bindings")]
    public void ThenDirectBindingsStayValid()
    {
        _records.Should().BeEmpty();
        _result!.IsValid.Should().BeTrue();
        _result.Artifact!.Scenarios.Should().BeEmpty();
        _result.Artifact.Bindings.Should().ContainSingle();
    }

    [Then("it uses the Feature-level scope")]
    public void ThenFeatureLevelScope()
    {
        Record().GetProperty("scope").GetProperty("epic").GetString().Should().Be("0201");
        Record().GetProperty("scope").GetProperty("feature").GetString().Should().Be("F3");
    }

    [Then("only the explicit override components are used")]
    public void ThenOnlyOverrideUsed() => Record().GetProperty("scope").GetProperty("feature").GetString().Should().Be("F3");

    [Then("no epic is merged from the Gherkin Feature-level scope")]
    public void ThenNoEpicMerged()
    {
        Record().GetProperty("scope").GetProperty("epic").ValueKind.Should().Be(JsonValueKind.Null);
        Record().GetProperty("claims")[0].GetProperty("status").GetString().Should().Be("incomplete");
    }

    [Then("it uses the common runner-evidence contract")]
    public void ThenCommonContract()
    {
        _records.Should().ContainSingle();
        var artifact = JsonNode.Parse(AcceptanceTestEvidence.ForMethod("M:Example.Tests.Sample.Run"))!;
        artifact["scenarios"] = new JsonArray(_records.Select(static record => JsonNode.Parse(record.GetRawText())).ToArray());
        new TestEvidenceArtifactReader().Read(artifact.ToJsonString()).IsValid.Should().BeTrue();
    }

    [Then("the case links of the first block's rows are active")]
    public void ThenFirstBlockActive() => Links(Record()).Take(2).Select(static link => link.State).Should().OnlyContain(static state => state == "active");

    [Then("the case links of the second block's rows are planned")]
    public void ThenSecondBlockPlanned() => Links(Record()).Skip(2).Select(static link => link.State).Should().OnlyContain(static state => state == "planned");

    [Then("every row's case link keeps that same claim")]
    public void ThenSameClaim()
    {
        Record().GetProperty("claims").GetArrayLength().Should().Be(1);
        Record().GetProperty("claims")[0].GetProperty("status").GetString().Should().Be("resolved");
        Links(Record()).Select(static link => link.State).Should().Equal("active", "planned");
    }

    [Then("the planned row's link is planned and has no case")]
    public void ThenPlannedLink() => Links(Record())[0].Should().Be(("row-1", null, "unresolved", "planned"));

    [Then("the active row's link is the unresolved link for missing input")]
    public void ThenMissingInputLink() => Links(Record())[1].Should().Be((null, null, "unresolved", "active"));

    private static ScenarioRow[] Rows(string kind) => kind == "Outline"
        ? [new ScenarioRow("row-1", "case-1", false), new ScenarioRow("row-2", "case-2", false)]
        : [new ScenarioRow(null, "case-1", false)];

    private static (string? RowId, string? CaseId, string Status, string State)[] Links(JsonElement record) =>
        record.GetProperty("cases").EnumerateArray().Select(static link => (
            link.GetProperty("rowId").GetString(),
            link.GetProperty("caseId").GetString(),
            link.GetProperty("status").GetString()!,
            link.GetProperty("state").GetString()!)).ToArray();

    private static BindingCatalog Catalog() => new(
    [
        new BindingFeature(new FeatureIdentity("0201", "F4"), "approved", [new BindingClaim("B-001", "Active", 12)],
            "src/analyzers/Scenarios/.spec/README.md"),
        new BindingFeature(new FeatureIdentity("0201", "F3"), "approved", [new BindingClaim("B-001", "Active", 12)],
            "src/analyzers/TestEvidence/.spec/README.md"),
    ], []);

    private static BindingPolicy Policy() => new("0.5.0", new Dictionary<string, string>
    {
        ["epic"] = "^[0-9]{4}$",
        ["feature"] = "^F[0-9]+$",
        ["claim"] = "^B-[0-9]{3}$",
    }, new Dictionary<string, string>
    {
        ["approved"] = "approved",
        ["withdrawn"] = "Withdrawn",
        ["superseded"] = "Superseded",
    }, true, "3. Acceptance Criteria", new LifecycleOmissions(false, false, false));

    private void Use(
        string id = "scenario-1",
        int line = 10,
        string[]? claimTags = null,
        IReadOnlyList<ScenarioRow>? rows = null,
        bool generatorInputPresent = true,
        ScenarioScope? scopeOverride = null) =>
        _metadata.Add(new ScenarioMetadata("src/feature/.spec/tests.feature", id, line, _featureScope, scopeOverride,
            claimTags ?? [], rows ?? [], generatorInputPresent));

    private JsonElement Record() => _records.Single();

    private readonly List<ScenarioMetadata> _metadata = [];
    private readonly List<ScenarioRow> _plannedRows = [];
    private ScenarioScope _featureScope = new("0201", "F3");
    private IReadOnlyList<JsonElement> _records = [];
    private string? _artifactJson;
    private TestEvidenceReadResult? _result;
}
