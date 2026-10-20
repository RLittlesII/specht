using System.Text.Json.Nodes;
using AwesomeAssertions;
using Reqnroll;
using Specht.Tests.Shared;

namespace Specht.Acceptance.Reconciliation;

/// <summary>The public evidence gate over a synthetic consumer tree (0201-F5 B-001 to B-017).</summary>
[Binding]
[Scope(Tag = "0201-F5")]
public sealed class ReconciliationSteps
{
    [Given("a claim bound to a discovered test through the common runner-evidence contract and a matching result with no scenario policy")]
    public void GivenDirectChain() { }

    [Given("a declared repository scope omitting a required test project or input")]
    public void GivenIncompleteScope() => _tree.AddProject("test/other.csproj");

    [Given("a resolved binding with a failed, skipped or absent test result")]
    public void GivenNonpassingOutcome() => _outcome = "failed";

    [Given("an explicit policy requiring a link absent from a claim's evidence")]
    public void GivenRequiredLinkGap() => _tree.Require("implementation", true);

    [Given("scenario bridge evidence supplied for part of a declared scope")]
    public void GivenScenarioBridge()
    {
        _scenario = true;
        _tree.Require("scenario", true);
    }

    [Given("a declared specification claim requiring test evidence with no test binding at all")]
    public void GivenUnboundClaim() => _binding = false;

    [Given("a resolving target classified as ineligible by the binding lifecycle policy")]
    public void GivenIneligibleTarget() => _historical = true;

    [Given("a binding to that target")]
    public void GivenHistoricalBinding() { }

    [Given("a non-exempt current claim in a selected Feature")]
    public void GivenCurrentClaim() { }

    [Given("no valid test binding to that claim")]
    public void GivenMissingTestBinding() => _binding = false;

    [Given("a current claim with no {string} link")]
    public void GivenOptionalLink(string kind) => _optionalKind = kind;

    [Given("the consumer {string} that link requirement")]
    public void GivenOptionalPolicy(string setting)
    {
        if (setting == "has explicitly enabled")
        {
            _tree.Require(_optionalKind!, true);
        }
    }

    [Given("an explicit coverage exemption with a reason")]
    public void GivenExemption()
    {
        _tree.Exempt("test", "A recorded exception.");
        _binding = false;
    }

    [Given("a required scenario link with explicit bridge evidence")]
    public void GivenRequiredScenarioForExemptTest()
    {
        _tree.Require("scenario", true);
        _scenario = true;
    }

    [Given("a declared set of project and target-framework combinations")]
    public void GivenDeclaredCombinations() { }

    [Given("a stale export beside a current export for the same declared combination")]
    public void GivenStaleExtraExport() => _staleExtra = true;

    [Given("a declared compilation export omits the selected claim record from its input index")]
    public void GivenRecordOmittedFromIndex() => _omitRecord = true;

    [Given("{string}")]
    public void GivenAvailabilityOrFreshness(string value)
    {
        switch (value)
        {
            case "a project combination omitted":
                _tree.AddProject("test/other.csproj");
                break;
            case "a target-framework combination omitted":
                _tree.AddFramework("net11.0");
                break;
            case "stale required inputs for one combination":
            case "stale":
                _stale = true;
                break;
            case "mismatched":
                _mismatched = true;
                break;
        }
    }

    [Given("{string} discovery or result evidence")]
    public void GivenStaleDiscoveryOrResult(string state) => GivenAvailabilityOrFreshness(state);

    [Given("a valid binding with {string} test results")]
    public void GivenIndependentExecution(string state)
    {
        if (state == "absent")
        {
            _outcome = null;
        }
        else
        {
            GivenAvailabilityOrFreshness(state);
        }
    }

    [Given("supplied traceability evidence")]
    public void GivenSuppliedEvidence() { }

    [Given("the existing checker report contract")]
    public void GivenCheckerReport()
    {
        _checker = new SpecTree();
        _checkerBefore = AcceptanceTool.Launch(_checker.Root, "--root", _checker.Root, "--json").Stdout;
    }

    [Given("discovery and result artifacts from a normal test workflow")]
    public void GivenRunnerArtifacts() { }

    [Given("complete current inputs and required links for a selected claim")]
    public void GivenCompleteChain() { }

    [Given("its required test case has outcome {string}")]
    public void GivenCaseOutcome(string outcome) => _outcome = outcome == "absent" ? null : outcome;

    [When("the evidence gate is requested for the declared scope")]
    [When("evidence is reconciled for the current scope")]
    [When("coverage requirements are evaluated")]
    [When("coverage is presented")]
    [When("analysis completeness is evaluated")]
    [When("evidence is reconciled")]
    [When("the separate trace report is introduced")]
    [When("reconciliation runs")]
    [When("the supplied evidence is reconciled by specht evidence")]
    public void WhenReconciled() => Run(true);

    [When("specht evidence is requested with {string} output")]
    public void WhenFormatRequested(string format) => Run(format == "versioned JSON");

    [When("specht evidence writes a caller-named report")]
    public void WhenReportRequested()
    {
        _tree.WriteEvidence();
        _result = _tree.Run("--json", "--report", "report.json");
    }

    [When("specht evidence cannot write the caller-named report")]
    public void WhenReportCannotBeWritten()
    {
        _tree.WriteEvidence();
        _result = _tree.Run("--json", "--report", Path.Combine(_tree.Root, "missing", "report.json"));
    }

    [When("evidence is supplied from an external directory containing two exports")]
    public void WhenExternalDirectoryIsSupplied()
    {
        _tree.WriteEvidence();
        _result = _tree.Run("--json", "--evidence", _tree.WriteExternalDirectory());
    }

    [When("the same export is selected by file and directory")]
    public void WhenSelectionsOverlap()
    {
        _tree.WriteEvidence();
        var directory = _tree.WriteOverlappingDirectory();
        _result = _tree.Run("--json", "--evidence", directory, "--evidence", directory + "/copy.json");
    }

    [Then("reconciliation presents the claim-to-method-to-case-to-result chain without requiring Reqnroll")]
    public void ThenDirectChain()
    {
        Claim()["links"]!["test"]!["state"]!.GetValue<string>().Should().Be("satisfied");
        Claim()["cases"]!.AsArray().Select(static item => item!.GetValue<string>()).Should().Contain("case-1");
        Report()["bindings"]![0]!["symbolId"]!.GetValue<string>().Should().Be("M:test");
        Report()["bindings"]![0]!["target"]!["id"]!.GetValue<string>().Should().Be("need/7");
        Report()["cases"]![0]!["methodSymbolId"]!.GetValue<string>().Should().Be("M:test");
        Report()["cases"]![0]!["outcome"]!.GetValue<string>().Should().Be("passed");
    }

    [Then("the report marks coverage incomplete and identifies the missing input")]
    public void ThenIncomplete()
    {
        _result.ExitCode.Should().Be(2);
        Report()["inputState"]!["state"]!.GetValue<string>().Should().Be("incomplete");
        Kinds().Should().Contain("missingCompilation");
    }

    [Then("the report keeps that outcome distinct from a passing result")]
    public void ThenOutcomeDistinct()
    {
        Claim()["links"]!["test"]!["state"]!.GetValue<string>().Should().Be("satisfied");
        Report()["cases"]![0]!["outcome"]!.GetValue<string>().Should().Be("failed");
        _result.ExitCode.Should().Be(1);
    }

    [Then("the report identifies that missing link instead of treating reference resolution as complete coverage")]
    public void ThenMissingPolicyLink()
    {
        Claim()["links"]!["implementation"]!["state"]!.GetValue<string>().Should().Be("missing");
        Kinds().Should().Contain("missingRequiredLink");
    }

    [Then("those scenario links augment the direct chains without inventing links for other tests")]
    public void ThenOnlyExplicitScenario()
    {
        Claim()["links"]!["scenario"]!["state"]!.GetValue<string>().Should().Be("satisfied");
        Claim()["scenarios"]!.AsArray().Select(static item => item!.GetValue<string>()).Should().ContainSingle();
        Report()["cases"]!.AsArray().Should().HaveCount(2);
        Report()["cases"]![0]!["scenarios"]!.AsArray().Should().ContainSingle();
        Report()["cases"]![1]!["scenarios"]!.AsArray().Should().BeEmpty();
    }

    [Then("reconciliation reports the missing evidence starting from that claim even when no method names it")]
    [Then("the claim has missing required test evidence")]
    public void ThenUnboundClaim()
    {
        Claim()["id"]!.GetValue<string>().Should().Be("need/7");
        Claim()["links"]!["test"]!["state"]!.GetValue<string>().Should().Be("missing");
        Kinds().Should().Contain("missingRequiredLink");
    }

    [Then("that binding does not satisfy a current evidence requirement")]
    public void ThenIneligibleDoesNotCover() => ThenUnboundClaim();

    [Then("the historical link retains the target identity and lifecycle state")]
    public void ThenHistoricalIdentity()
    {
        Report()["bindings"]![0]!["target"]!["id"]!.GetValue<string>().Should().Be("need/8");
        Report()["bindings"]![0]!["target"]!["state"]!.GetValue<string>().Should().Be("Withdrawn");
    }

    [Then("the missing link is {string}")]
    public void ThenOptionalRequirement(string outcome)
    {
        var link = Claim()["links"]![_optionalKind!];
        if (outcome == "not a required-link gap")
        {
            link.Should().BeNull();
        }
        else
        {
            link!["state"]!.GetValue<string>().Should().Be("missing");
        }
    }

    [Then("the exemption and reason remain visible")]
    public void ThenExemptionReason()
    {
        Claim()["links"]!["test"]!["reason"]!.GetValue<string>().Should().Be("A recorded exception.");
    }

    [Then("the exemption is classified as exempt rather than covered")]
    public void ThenExemptNotCovered()
    {
        Claim()["links"]!["test"]!["state"]!.GetValue<string>().Should().Be("exempt");
        Report()["summary"]!["coveredClaims"]!.GetValue<int>().Should().Be(0);
    }

    [Then("the test link is exempt and the scenario link is satisfied")]
    public void ThenScenarioRemainsRequired()
    {
        _result.ExitCode.Should().Be(0);
        Claim()["links"]!["test"]!["state"]!.GetValue<string>().Should().Be("exempt");
        Claim()["links"]!["scenario"]!["state"]!.GetValue<string>().Should().Be("satisfied");
    }

    [Then("the analysis is {string}")]
    public void ThenAnalysisState(string state) => Report()["inputState"]!["state"]!.GetValue<string>().Should().Be(state);

    [Then("the stale export remains visible without displacing current evidence")]
    public void ThenStaleExtraDoesNotDisplaceCurrent()
    {
        _result.ExitCode.Should().Be(0);
        Report()["scope"]!["supplied"]!.AsArray().Select(static item => item!["state"]!.GetValue<string>())
            .Should().Contain("current").And.Contain("stale");
        Claim()["links"]!["test"]!["state"]!.GetValue<string>().Should().Be("satisfied");
    }

    [Then("the missing record is identified as a required input")]
    public void ThenMissingIndexedRecordIsReported()
    {
        _result.ExitCode.Should().Be(2);
        Report()["findings"]!.AsArray().Should().Contain(static item => item!["kind"]!.GetValue<string>() == "missingCompilation"
            && item["path"]!.GetValue<string>() == "src/read/.spec/README.md");
    }

    [Then("the evidence remains visibly {string}")]
    public void ThenEvidenceState(string state) => Report()["scope"]!["supplied"]![0]!["state"]!.GetValue<string>().Should().Be(state);

    [Then("it does not establish current discovery or execution")]
    public void ThenNoCurrentExecution() => Report()["cases"]![0]!["outcome"]!.GetValue<string>().Should().Be("absent");

    [Then("binding validity is evaluated independently of the execution-evidence state")]
    public void ThenBindingIndependent() => Claim()["links"]!["test"]!["state"]!.GetValue<string>()
        .Should().Be(_stale || _mismatched ? "unavailable" : "satisfied");

    [Then("the results are not treated as passing")]
    public void ThenNotPassing() => Report()["cases"]![0]!["outcome"]!.GetValue<string>().Should().NotBe("passed");

    [Then("reconciliation is presented in that format")]
    public void ThenFormat()
    {
        if (_json)
        {
            Report()["format"]!.GetValue<string>().Should().Be("specht.evidence-report");
            Report()["formatVersion"]!.GetValue<int>().Should().Be(1);
        }
        else
        {
            _result.Stdout.Should().Contain("claims:").And.Contain("passed:");
        }
    }

    [Then("that report contains the same versioned JSON as stdout")]
    public void ThenReportMatchesStdout()
    {
        _result.ExitCode.Should().Be(0);
        File.ReadAllText(Path.Combine(_tree.Root, "report.json")).Should().Be(_result.Stdout);
        Report()["format"]!.GetValue<string>().Should().Be("specht.evidence-report");
    }

    [Then("the command reports the write failure without exposing an absolute path")]
    public void ThenReportFailureIsRelative()
    {
        _result.ExitCode.Should().Be(4);
        _result.Stderr.Should().Contain("could not be written").And.NotContain(_tree.Root);
        _result.Stdout.Should().BeEmpty();
    }

    [Then("both external exports have distinct root-safe labels")]
    public void ThenExternalLabelsAreDistinct()
    {
        _result.ExitCode.Should().Be(0);
        var labels = Report()["scope"]!["supplied"]!.AsArray()
            .Select(static item => item!["path"]!.GetValue<string>()).ToArray();
        labels.Should().Contain("external-evidence-2/first.json").And.Contain("external-evidence-2/second.json");
        labels.Distinct(StringComparer.Ordinal).Should().HaveCount(labels.Length);
        _result.Stdout.Should().NotContain(_tree.Root);
    }

    [Then("it is reconciled once")]
    public void ThenOverlappingFileIsDeduplicated()
    {
        _result.ExitCode.Should().Be(0);
        Report()["scope"]!["supplied"]!.AsArray().Should().HaveCount(2);
        Report()["cases"]!.AsArray().Should().HaveCount(2);
    }

    [Then("the checker report contract is unchanged")]
    public void ThenCheckerUnchanged() => AcceptanceTool.Launch(_checker!.Root, "--root", _checker.Root, "--json")
        .Stdout.Should().Be(_checkerBefore);

    [Then("it consumes those artifacts without launching test execution")]
    public void ThenNoTestLaunch()
    {
        _result.ExitCode.Should().Be(0);
        Directory.EnumerateFiles(_tree.Root, "*", SearchOption.AllDirectories).Count().Should().Be(_fileCount);
    }

    [Then("the default trace gate {string}")]
    public void ThenDefaultGate(string gate) => _result.ExitCode.Should().Be(gate == "succeeds" ? 0 : 1);

    [Then("binding validity and execution outcome remain separate")]
    public void ThenOutcomeSeparated() => Claim()["links"]!["test"]!["state"]!.GetValue<string>().Should().Be("satisfied");

    [AfterScenario]
    public void Dispose()
    {
        _tree.Dispose();
        _checker?.Dispose();
    }

    private void Run(bool json)
    {
        _json = json;
        _tree.WriteEvidence(_outcome, _binding, _historical, _scenario);
        if (_omitRecord)
        {
            _tree.OmitIndexedRecord();
        }
        else if (_staleExtra)
        {
            File.Copy(Path.Combine(_tree.Root, "evidence.json"), Path.Combine(_tree.Root, "stale.json"));
            _tree.EditRecord();
            _tree.WriteEvidence(_outcome, _binding, _historical, _scenario);
        }
        else if (_stale)
        {
            _tree.EditRecord();
        }
        else if (_mismatched)
        {
            _tree.EditAssembly();
        }

        _fileCount = Directory.EnumerateFiles(_tree.Root, "*", SearchOption.AllDirectories).Count();
        _result = _staleExtra ? _tree.Run("--json", "--evidence", "stale.json")
            : json ? _tree.Run("--json") : _tree.Run();
    }

    private JsonNode Report() => JsonNode.Parse(_result.Stdout)!;
    private JsonNode Claim() => Report()["claims"]![0]!;
    private string[] Kinds() => Report()["findings"]!.AsArray().Select(static item => item!["kind"]!.GetValue<string>()).ToArray();

    private readonly EvidenceTree _tree = new();
    private SpecTree? _checker;
    private string? _checkerBefore;
    private (string Stdout, string Stderr, int ExitCode) _result;
    private string? _optionalKind;
    private string? _outcome = "passed";
    private int _fileCount;
    private bool _binding = true;
    private bool _historical;
    private bool _scenario;
    private bool _stale;
    private bool _mismatched;
    private bool _staleExtra;
    private bool _omitRecord;
    private bool _json = true;
}
