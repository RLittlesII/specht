using System.Text.Json.Nodes;
using AwesomeAssertions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Reqnroll;
using Specht.Tool.Features.Evidence;
using VsTestResult = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult;

namespace Specht.Acceptance.TestEvidence;

/// <summary>VSTest collection through the logger VSTest constructs, and its reconciliation (0201-F3 B-010 to B-016, B-020, B-021, B-025, B-026).</summary>
[Binding]
public sealed class VsTestEvidenceSteps
{
    [Given("a bound xUnit v2 theory with two inline data rows in a VSTest test project")]
    public void GivenTheory()
    {
        Plan("a", null, [Theory("a", FirstRow), Theory("a", SecondRow)], null);
        _expected[FirstRow] = "passed";
        _expected[SecondRow] = "failed";
    }

    [Given("a bound test method whose source file sits outside its test project's directory and is linked into that project")]
    public void GivenLinkedMethod() => Plan("a", null, [Tree.Case("a", FirstRow, "LocalCases", "Fact")], null);

    [Given("^a bound VSTest case that (failed|was skipped|was not executed)$")]
    public void GivenNonpassingCase(string outcome)
    {
        Plan("a", null, [Tree.Case("a", FirstRow, "LocalCases", "Fact")], null);
        _expected[FirstRow] = RunnerOutcomes[outcome].Expected;
    }

    [Given("a bound test method for which VSTest discovered no case")]
    public void GivenUndiscoveredMethod()
    {
        Tree.BindUndiscovered();
        Plan("a", null, [Theory("a", FirstRow), Theory("a", SecondRow), Tree.Case("a", ThirdRow, "LocalCases", "Fact")], null);
        _expected[FirstRow] = "passed";
        _expected[SecondRow] = "passed";
        _expected[ThirdRow] = "passed";
    }

    [Given("^a bound xUnit v2 theory whose first case passed and whose second case (failed|was skipped|was not executed)$")]
    public void GivenMixedTheory(string outcome)
    {
        Plan("a", null, [Theory("a", FirstRow), Theory("a", SecondRow)], null);
        _expected[FirstRow] = "passed";
        _expected[SecondRow] = RunnerOutcomes[outcome].Expected;
    }

    [Given("Reqnroll-generated tests for a Scenario Outline with two example rows executed under VSTest")]
    public void GivenReqnrollGeneratedCases()
    {
        Plan("a", null, [Tree.Case("a", FirstRow, "SharedClaimsFeature", "TwoRows"), Tree.Case("a", SecondRow, "SharedClaimsFeature", "TwoRows")],
            null);
        _expected[FirstRow] = "passed";
        _expected[SecondRow] = "failed";
    }

    [Given("projects A and B compile the same central claim files holding one visual and one nonvisual case")]
    public void GivenPartitionedProjects()
    {
    }

    [Given("^project (A|B) declares the required-case scope \"(.*)\" and its run selects \"(.*)\"$")]
    public void GivenPartition(string name, string scope, string filter)
    {
        var project = name.ToLowerInvariant();
        var nonvisual = Tree.Case(project, Ids[project].Nonvisual, "LocalCases", "Fact");
        var visual = Tree.Case(project, Ids[project].Visual, "LocalCases", "Theory", "visual");
        var selected = filter == "Category=visual" ? visual : nonvisual;
        Plan(project, scope, [nonvisual, visual], null);
        _expected[selected.Id.ToString()] = "passed";
    }

    [Given("project A declares the required-case scope {string}")]
    public void GivenScope(string scope) => _scope = scope;

    [Given("project A's run selected only part of the nonvisual cases")]
    public void GivenPartialSelection()
    {
        var cases = new[] { Tree.Case("a", FirstRow, "LocalCases", "Fact"), Theory("a", SecondRow), Theory("a", ThirdRow) };
        Plan("a", _scope, cases, [cases[0], cases[1]]);
        _expected[FirstRow] = "passed";
        _expected[SecondRow] = "passed";
    }

    [When("its cases are discovered under VSTest")]
    [When("its cases are discovered")]
    public void WhenDiscovered()
    {
        foreach (var (project, plan) in _plans)
        {
            Tree.Discover(project, plan.Cases);
        }
    }

    [When("its cases are executed under VSTest")]
    public void WhenExecuted()
    {
        WhenDiscovered();
        foreach (var (project, plan) in _plans)
        {
            Tree.Run(project, plan.Named, Results(plan.Cases));
        }
    }

    [When("its evidence is collected")]
    [When("their evidence is collected")]
    [When("both runs' evidence is collected")]
    public void WhenCollected()
    {
        WhenExecuted();
        _result = Tree.Reconcile();
    }

    [Then("each theory case is recorded with its own stable runner case identity")]
    public void ThenEachTheoryCaseHasItsIdentity() =>
        Cases("a").Select(static row => row["caseId"]!.GetValue<string>()).Should().BeEquivalentTo([FirstRow, SecondRow]);

    [Then("both cases link to the same bound method")]
    public void ThenBothCasesLinkToTheMethod() =>
        Cases("a").Select(static row => row["methodSymbolId"]!.GetValue<string>()).Should()
            .Equal("M:Shared.LocalCases.Theory(System.Int32)", "M:Shared.LocalCases.Theory(System.Int32)");

    [Then("each case's outcome is recorded against that case's own runner identity")]
    [Then("each case keeps its own outcome")]
    [Then("the case is recorded with that outcome")]
    public void ThenOutcomesBelongToTheirCases() => Outcomes("a").Should().BeEquivalentTo(_expected);

    [Then("the method identity resolves to the bound method")]
    public void ThenTheMethodResolves() =>
        Cases("a").Should().ContainSingle().Which!["methodSymbolId"]!.GetValue<string>().Should().Be("M:Shared.LocalCases.Fact");

    [Then("its source location is the linked file's repository-relative path")]
    public void ThenTheLocationIsTheLinkedFile() =>
        Tree.Artifact("a")["discovery"]!["methods"]!.AsArray().Single(static method => method!["symbolId"]!.GetValue<string>() == "M:Shared.LocalCases.Fact")!
            ["location"]!["path"]!.GetValue<string>().Should().Be("test/shared/SharedCases.cs");

    [Then("the case is never recorded as passed")]
    public void ThenNeverPassed()
    {
        Outcomes("a").Values.Should().NotContain("passed");
        _result!.Report["cases"]!.AsArray().Select(static row => row!["outcome"]?.GetValue<string>()).Should().NotContain("passed");
    }

    [Then("its discovery is reported as unavailable or incomplete")]
    public void ThenDiscoveryIsIncomplete()
    {
        Cases("a").Select(static row => row["methodSymbolId"]!.GetValue<string>()).Should().NotContain("M:Shared.LocalCases.Undiscovered");
        _result!.Report["findings"]!.AsArray().Where(static finding => finding!["kind"]!.GetValue<string>() == "missingResult")
            .Select(static finding => finding!["message"]!.GetValue<string>()).Should().ContainSingle()
            .Which.Should().Contain("M:Shared.LocalCases.Undiscovered");
    }

    [Then("it is reported as neither passing nor a complete empty set")]
    public void ThenNeitherPassingNorEmpty()
    {
        _result!.ExitCode.Should().Be(1);
        _result.Report["cases"]!.AsArray().Select(static row => row!["methodSymbolId"]!.GetValue<string>()).Should()
            .NotContain("M:Shared.LocalCases.Undiscovered");
    }

    [Then("no single method-level outcome replaces them")]
    public void ThenNoMethodLevelOutcome() =>
        _result!.Report["cases"]!.AsArray().Where(static row => row!["project"]!.GetValue<string>() == "test/a/a.csproj")
            .Select(static row => (row!["caseId"]!.GetValue<string>(), row["outcome"]!.GetValue<string>())).Should()
            .BeEquivalentTo(_expected.Select(static pair => (pair.Key, pair.Value)));

    [Then("both generated cases and their outcomes are recorded in the common evidence contract")]
    public void ThenGeneratedCasesAreRecorded()
    {
        Cases("a").Select(static row => row["methodSymbolId"]!.GetValue<string>()).Should()
            .Equal("M:Shared.SharedClaimsFeature.TwoRows(System.String,System.String[])",
                "M:Shared.SharedClaimsFeature.TwoRows(System.String,System.String[])");
        Outcomes("a").Should().BeEquivalentTo(_expected);
    }

    [Then("no binding is required on the generated code")]
    public void ThenNoBindingIsRequired() =>
        Tree.Artifact("a")["bindings"]!.AsArray().Select(static binding => binding!["symbolId"]!.GetValue<string>()).Should()
            .NotContain(static symbolId => symbolId.StartsWith("M:Shared.SharedClaimsFeature.", StringComparison.Ordinal));

    [Then("four case keys are discovered: A.nonvisual, A.visual, B.nonvisual and B.visual")]
    public void ThenFourCaseKeys() =>
        new[] { "a", "b" }.SelectMany(project => Cases(project).Select(row => project + "/" + row["caseId"]!.GetValue<string>())).Should()
            .BeEquivalentTo(["a/" + Ids["a"].Nonvisual, "a/" + Ids["a"].Visual, "b/" + Ids["b"].Nonvisual, "b/" + Ids["b"].Visual]);

    [Then("each case records its categories and, separately, whether its run selected it")]
    public void ThenCategoriesAndSelection()
    {
        Facts("a").Should().BeEquivalentTo([(Ids["a"].Nonvisual, "", true), (Ids["a"].Visual, "visual", false)]);
        Facts("b").Should().BeEquivalentTo([(Ids["b"].Nonvisual, "", false), (Ids["b"].Visual, "visual", true)]);
    }

    [Then("every case keeps its project, assembly and source provenance")]
    public void ThenProvenance()
    {
        foreach (var project in new[] { "a", "b" })
        {
            var artifact = Tree.Artifact(project);
            artifact["project"]!["path"]!.GetValue<string>().Should().Be($"test/{project}/{project}.csproj");
            artifact["assembly"]!["path"]!.GetValue<string>().Should().Be($"test/{project}/bin/{project}.dll");
            artifact["inputs"]!.AsArray().Select(static input => input!["path"]!.GetValue<string>()).Should().Contain("test/shared/SharedCases.cs");
        }
    }

    [Then("project A owns A.nonvisual and project B owns B.visual")]
    public void ThenOwnership()
    {
        Owned("a").Should().Equal(Ids["a"].Nonvisual);
        Owned("b").Should().Equal(Ids["b"].Visual);
    }

    [Then("no case key is merged with another project's key")]
    public void ThenNoMergedKeys()
    {
        _result!.Report["findings"]!.AsArray().Select(static finding => finding!["kind"]!.GetValue<string>()).Should().NotContain("ambiguousRun");
        _result.Report["cases"]!.AsArray().Select(static row => row!["project"]!.GetValue<string>() + "/" + row["caseId"]!.GetValue<string>())
            .Should().OnlyHaveUniqueItems().And.HaveCount(4);
    }

    [Then("the run is reported incomplete for the owned keys it did not select")]
    public void ThenIncompleteRun()
    {
        Facts("a").Should().ContainSingle(static fact => !fact.Selected).Which.CaseId.Should().Be(ThirdRow);
        var finding = _result!.Report["findings"]!.AsArray().Should()
            .ContainSingle(static item => item!["kind"]!.GetValue<string>() == "incompleteRun").Subject!;
        finding["identity"]!.GetValue<string>().Should().Contain("test/a/a.csproj");
        _result.ExitCode.Should().Be(0);
    }

    [AfterScenario]
    public void Dispose() => _tree?.Dispose();

    private VsTestEvidenceTree Tree => _tree ??= new VsTestEvidenceTree();

    private void Plan(string project, string? scope, TestCase[] cases, TestCase[]? named)
    {
        Tree.Declare(project, scope);
        _plans[project] = new ProjectPlan(cases, named);
    }

    private TestCase Theory(string project, string id) => Tree.Case(project, id, "LocalCases", "Theory");

    private IEnumerable<VsTestResult> Results(IEnumerable<TestCase> cases)
    {
        foreach (var testCase in cases)
        {
            if (_expected.TryGetValue(testCase.Id.ToString(), out var outcome))
            {
                yield return new VsTestResult(testCase) { Outcome = RunnerOutcomes.Values.Single(row => row.Expected == outcome).Outcome };
            }
        }
    }

    private JsonObject[] Cases(string project) =>
        Tree.Artifact(project)["discovery"]!["cases"]!.AsArray().Select(static row => row!.AsObject()).ToArray();

    private Dictionary<string, string> Outcomes(string project) =>
        Tree.Artifact(project)["results"]!["cases"]!.AsArray()
            .ToDictionary(static row => row!["caseId"]!.GetValue<string>(), static row => row!["outcome"]!.GetValue<string>());

    private (string CaseId, string Categories, bool Selected)[] Facts(string project) =>
        Cases(project).Select(static row => (row["caseId"]!.GetValue<string>(),
            string.Join(",", row["categories"]!.AsArray().Select(static category => category!.GetValue<string>())),
            row["selected"]!.GetValue<bool>())).ToArray();

    private string[] Owned(string project) =>
        Cases(project).Where(static row => row["owned"]!.GetValue<bool>()).Select(static row => row["caseId"]!.GetValue<string>()).ToArray();

    private sealed record ProjectPlan(TestCase[] Cases, TestCase[]? Named);

    private const string FirstRow = "11111111-1111-4111-8111-111111111111";
    private const string SecondRow = "22222222-2222-4222-8222-222222222222";
    private const string ThirdRow = "33333333-3333-4333-8333-333333333333";

    private static readonly Dictionary<string, (TestOutcome Outcome, string Expected)> RunnerOutcomes = new(StringComparer.Ordinal)
    {
        ["passed"] = (TestOutcome.Passed, "passed"),
        ["failed"] = (TestOutcome.Failed, "failed"),
        ["was skipped"] = (TestOutcome.Skipped, "skipped"),
        ["was not executed"] = (TestOutcome.None, "notRun"),
    };

    private static readonly Dictionary<string, (string Nonvisual, string Visual)> Ids = new(StringComparer.Ordinal)
    {
        ["a"] = ("aaaaaaaa-0000-4000-8000-000000000001", "aaaaaaaa-0000-4000-8000-000000000002"),
        ["b"] = ("bbbbbbbb-0000-4000-8000-000000000001", "bbbbbbbb-0000-4000-8000-000000000002"),
    };

    private readonly Dictionary<string, ProjectPlan> _plans = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _expected = new(StringComparer.Ordinal);
    private EvidenceResult? _result;
    private VsTestEvidenceTree? _tree;
    private string? _scope;
}
