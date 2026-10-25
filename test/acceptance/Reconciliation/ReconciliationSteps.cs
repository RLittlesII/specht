using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Reqnroll;
using Specht.Tests.Shared;

namespace Specht.Acceptance.Reconciliation;

/// <summary>The public evidence gate over a synthetic consumer tree (0201-F5 B-001 to B-022).</summary>
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

    [Given("a required claim whose outline has an active Examples block that passed and a planned Examples block that was skipped")]
    public void GivenActiveAndPlannedRows() => _cases =
    [
        new CaseSpec("active-row", true, true, "passed", "active"),
        new CaseSpec("planned-row", true, true, "skipped", "planned"),
    ];

    [Given("a required claim whose outline has a planned Examples block and an active row whose case was {string}")]
    public void GivenPlannedBlockBesideActiveRow(string outcome) => _cases =
    [
        new CaseSpec("active-row", true, true, outcome == "absent" ? null : outcome, "active"),
        new CaseSpec("planned-row", true, true, "skipped", "planned"),
    ];

    [Given("a required claim whose outline has only planned Examples blocks")]
    public void GivenOnlyPlannedRows() => _cases =
    [
        new CaseSpec("planned-row", true, true, "skipped", "planned"),
        new CaseSpec("planned-row-2", true, true, "skipped", "planned"),
    ];

    [Given("a bound test whose every discovered case is owned by no project")]
    public void GivenNoOwnedCase() => _cases =
    [
        new CaseSpec("plain-case", false, false, null),
        new CaseSpec("visual-case", false, true, "passed"),
    ];

    [Given("projects A and B compile the same claim files holding one visual and one nonvisual case")]
    public void GivenPartitionedProjects() => _partitioned = true;

    [Given("projects A and B own {string} and {string} over the same claim files")]
    [Given("project A owns {string} and project B owns {string}")]
    public void GivenOwnership(string first, string second) => _partitioned = true;

    [Given("the discovered case keys are A.nonvisual, A.visual, B.nonvisual and B.visual")]
    [Given("A.nonvisual passed in project A's run")]
    [Given("A.nonvisual passed in project A's run and B.visual passed in project B's run")]
    [Given("A.visual and B.nonvisual were not selected by their runs")]
    public void GivenPartitionKeysAndRuns() { }

    [Given(@"^B\.visual (failed|was skipped|has no outcome) in project B's run$")]
    public void GivenOwnedKeyNotPassing(string outcome) => _visualOutcome = outcome switch
    {
        "failed" => "failed",
        "was skipped" => "skipped",
        _ => null,
    };

    [Given("project B has no current evidence")]
    public void GivenOmittedOwningRun() => _omittedRun = true;

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
        _result = _tree.Run("--json", _tree.WriteExternalDirectory());
    }

    [When("the same export is selected by file and directory")]
    public void WhenSelectionsOverlap()
    {
        _tree.WriteEvidence();
        var directory = _tree.WriteOverlappingDirectory();
        _result = _tree.Run("--json", directory, directory + "/copy.json");
    }

    [Given("supplied traceability evidence in two locations")]
    public void GivenEvidenceInTwoLocations()
    {
        _tree.WriteEvidence();
        _external = _tree.WriteExternalDirectory();
    }

    [Given("a declared scope and no evidence path")]
    public void GivenNoEvidencePath() { }

    [Given("a request for specht evidence help")]
    public void GivenEvidenceHelpRequest() { }

    [When("specht evidence is requested with both locations as positional arguments")]
    public void WhenBothLocationsArePositional() => _result = _tree.RunWith("--json", "evidence.json", _external!);

    [When("the same export is named twice as positional arguments")]
    public void WhenTheSameExportIsNamedTwice()
    {
        _tree.WriteEvidence();
        var directory = _tree.WriteOverlappingDirectory();
        _result = _tree.RunWith("--json", directory, directory + "/copy.json", "evidence.json");
    }

    [When("specht evidence is requested")]
    public void WhenEvidenceIsRequestedWithNoPath() => _result = _tree.RunWith("--json");

    [When("the help is shown")]
    public void WhenTheHelpIsShown() => _result = AcceptanceTool.Launch(_tree.Root, "evidence", "--help");

    [Then("both are supplied as evidence")]
    public void ThenBothLocationsAreSupplied()
    {
        _result.ExitCode.Should().Be(0, _result.Stdout + _result.Stderr);
        Supplied().Should().Equal("evidence.json", "external-evidence-2/first.json", "external-evidence-2/second.json");
    }

    [Then("both are supplied in command-line order, with no error")]
    public void ThenBothSuppliedInCommandLineOrder()
    {
        _result.ExitCode.Should().Be(0, _result.Stdout + _result.Stderr);
        _result.Stderr.Should().BeEmpty();
        Supplied().Should().Equal("evidence.json", "external-evidence-2/first.json", "external-evidence-2/second.json");
    }

    [Then("the command fails with incomplete input and reports that no evidence was supplied")]
    public void ThenNoEvidenceReported()
    {
        _result.ExitCode.Should().Be(2);
        Report()["findings"]!.AsArray().Should().Contain(static item => item!["kind"]!.GetValue<string>() == "missingCompilation"
            && item["message"]!.GetValue<string>().Contains("No evidence was supplied", StringComparison.Ordinal));
    }

    [Then("the optional positional paths are in the usage and no evidence option is offered")]
    public void ThenHelpShowsPositionalPaths()
    {
        var help = Regex.Replace(_result.Stdout, @"\x1b\[[0-9;]*m", string.Empty);
        _result.ExitCode.Should().Be(0, help);
        Regex.IsMatch(help, @"evidence\s+\[PATH\w*\]\s+\[OPTIONS\]").Should().BeTrue($"the usage shows optional paths, but read {help}");
        help.Should().NotContain("--evidence");
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

    [Then("the planned rows are reported as planned")]
    public void ThenPlannedRowsReported()
    {
        Row("test/tests.csproj", "planned-row")["planned"]!.GetValue<bool>().Should().BeTrue();
        (Row("test/tests.csproj", "active-row")["planned"]?.GetValue<bool>() ?? false).Should().BeFalse();
    }

    [Then("the planned rows neither cover the claim nor fail the default trace gate")]
    public void ThenPlannedRowsNeitherCoverNorFail()
    {
        _result.ExitCode.Should().Be(0);
        Claim()["cases"]!.AsArray().Select(static item => item!.GetValue<string>()).Should().Equal("active-row");
    }

    [Then("the default trace gate fails for that active row")]
    public void ThenActiveRowFails()
    {
        _result.ExitCode.Should().Be(1);
        MissingResults().Should().ContainSingle().Which.Should().Contain("active-row");
    }

    [Then("the claim has no covering case")]
    public void ThenNoCoveringCase() => Claim()["cases"]!.AsArray().Should().BeEmpty();

    [Then("the default trace gate fails for the claim's missing test evidence")]
    public void ThenPlannedOnlyClaimFails()
    {
        _result.ExitCode.Should().Be(1);
        MissingResults().Should().NotBeEmpty();
    }

    [Then("the default trace gate fails for that test as having no owned case")]
    public void ThenNoOwnedCaseFails()
    {
        _result.ExitCode.Should().Be(1);
        MissingResults().Should().ContainSingle().Which.Should().Contain("M:test").And.Contain("no owned case");
    }

    [Then("the default trace gate succeeds")]
    public void ThenGateSucceeds() => _result.ExitCode.Should().Be(0);

    [Then("A.visual and B.nonvisual are counted neither as passed nor as failures")]
    public void ThenUnownedKeysNotCounted()
    {
        Report()["summary"]!["passed"]!.GetValue<int>().Should().Be(2);
        Report()["summary"]!["failed"]!.GetValue<int>().Should().Be(0);
        Row("test/a/a.csproj", "visual-case")["owned"]!.GetValue<bool>().Should().BeFalse();
        Row("test/b/b.csproj", "plain-case")["owned"]!.GetValue<bool>().Should().BeFalse();
    }

    [Then("no case key is merged with another project's key")]
    public void ThenKeysNotMerged()
    {
        Report()["cases"]!.AsArray().Select(static row => (row!["project"]!.GetValue<string>(), row["caseId"]!.GetValue<string>()))
            .Distinct().Should().HaveCount(4);
        Claim()["cases"]!.AsArray().Select(static item => item!.GetValue<string>()).Should().Equal("plain-case", "visual-case");
    }

    [Then("the default trace gate fails for B.visual")]
    public void ThenOwnedKeyFails()
    {
        _result.ExitCode.Should().Be(1);
        MissingResults().Should().ContainSingle().Which.Should().Contain("visual-case");
    }

    [Then("A.visual does not stand in for B.visual")]
    public void ThenNoStandIn()
    {
        Row("test/a/a.csproj", "visual-case")["owned"]!.GetValue<bool>().Should().BeFalse();
        Row("test/b/b.csproj", "visual-case")["outcome"]!.GetValue<string>().Should().NotBe("passed");
        Report()["summary"]!["passed"]!.GetValue<int>().Should().Be(1);
    }

    [Then("the default trace gate fails as incomplete input naming project B")]
    public void ThenOmittedRunFails()
    {
        _result.ExitCode.Should().Be(2);
        Report()["findings"]!.AsArray().Should().Contain(static item => item!["kind"]!.GetValue<string>() == "missingCompilation"
            && item["path"]!.GetValue<string>() == "test/b/b.csproj");
    }

    [Then("project B's owned keys are not reported as passed")]
    public void ThenOmittedKeysNotPassed()
    {
        Report()["cases"]!.AsArray().Should().NotContain(static row => row!["project"]!.GetValue<string>() == "test/b/b.csproj");
        Report()["summary"]!["passed"]!.GetValue<int>().Should().Be(1);
    }

    [AfterScenario]
    public void Dispose()
    {
        _tree.Dispose();
        _checker?.Dispose();
    }

    private void Run(bool json)
    {
        _json = json;
        if (_partitioned)
        {
            _tree.UsePartitions("a", "b");
            _tree.WriteCases("a", "evidence.json",
                [new CaseSpec("plain-case", true, true, "passed"), new CaseSpec("visual-case", false, false, null)]);
            if (!_omittedRun)
            {
                _tree.WriteCases("b", "evidence-b.json",
                    [new CaseSpec("plain-case", false, false, null), new CaseSpec("visual-case", true, true, _visualOutcome)]);
            }
        }
        else if (_cases is not null)
        {
            _tree.WriteCases("tests", "evidence.json", _cases);
        }
        else
        {
            _tree.WriteEvidence(_outcome, _binding, _historical, _scenario);
        }

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
        string[] second = _partitioned && !_omittedRun ? ["evidence-b.json"] : [];
        _result = _staleExtra ? _tree.Run("--json", "stale.json")
            : json ? _tree.Run(["--json", .. second]) : _tree.Run();
    }

    private string[] Supplied() => Report()["scope"]!["supplied"]!.AsArray().Select(static item => item!["path"]!.GetValue<string>()).ToArray();

    private JsonNode Report() => JsonNode.Parse(_result.Stdout)!;
    private JsonNode Claim() => Report()["claims"]![0]!;
    private JsonNode Row(string project, string caseId) => Report()["cases"]!.AsArray()
        .Single(row => row!["project"]!.GetValue<string>() == project && row["caseId"]!.GetValue<string>() == caseId)!;

    private string[] MissingResults() => Report()["findings"]!.AsArray()
        .Where(static item => item!["kind"]!.GetValue<string>() == "missingResult")
        .Select(static item => item!["message"]!.GetValue<string>()).ToArray();

    private string[] Kinds() => Report()["findings"]!.AsArray().Select(static item => item!["kind"]!.GetValue<string>()).ToArray();

    private readonly EvidenceTree _tree = new();
    private SpecTree? _checker;
    private string? _checkerBefore;
    private (string Stdout, string Stderr, int ExitCode) _result;
    private string? _external;
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
    private IReadOnlyList<CaseSpec>? _cases;
    private bool _partitioned;
    private bool _omittedRun;
    private string? _visualOutcome = "passed";
}
