using System.Text.Json.Nodes;
using AwesomeAssertions;

namespace Specht.Tests.Evidence;

/// <summary>
/// Several projects' runs, the incomplete-run finding, the undiscovered bound method (0201-F3 B-014, B-021, B-026; OQ-4, OQ-5)
/// and the owned-key and planned-row gate (0201-F5 B-017 to B-022).
/// </summary>
[Trait("Tier", "Integration")]
public sealed class EvidenceReconcilerIntegrationTests : IDisposable
{
    [Fact]
    public void TwoProjectsRunsOverTheSameClaim_WhenReconciledWithoutARun_ShouldUseEachProjectsOwnRunAndKeepTheirCaseKeysApart()
    {
        // Given
        _root.Write("a.json", "a", 1, "run-a", new JsonArray(Case("case-1", "M:Shared.Checks")), new JsonArray(Outcome("case-1")));
        _root.Write("b.json", "b", 1, "run-b", new JsonArray(Case("case-1", "M:Shared.Checks")), new JsonArray(Outcome("case-1")));

        // When
        var result = _root.Reconcile("a.json", "b.json");

        // Then
        Kinds(result.Report).Should().NotContain("ambiguousRun");
        result.ExitCode.Should().Be(0);
        result.Report["claims"]!.AsArray().Single()!["cases"]!.AsArray().Should().HaveCount(2);
        result.Report["cases"]!.AsArray().Select(static row => (row!["project"]!.GetValue<string>(), row["outcome"]!.GetValue<string>()))
            .Should().BeEquivalentTo([("test/a/a.csproj", "passed"), ("test/b/b.csproj", "passed")]);
    }

    [Fact]
    public void SeveralRunsForOneProject_WhenReconciledWithoutARun_ShouldReportAnAmbiguousRunForThatProjectOnly()
    {
        // Given
        _root.Write("a1.json", "a", 1, "run-a1", new JsonArray(Case("case-1", "M:Shared.Checks")), new JsonArray(Outcome("case-1")));
        _root.Write("a2.json", "a", 1, "run-a2", new JsonArray(Case("case-1", "M:Shared.Checks")), new JsonArray(Outcome("case-1")));
        _root.Write("b.json", "b", 1, "run-b", new JsonArray(Case("case-1", "M:Shared.Checks")), new JsonArray(Outcome("case-1")));

        // When
        var result = _root.Reconcile("a1.json", "a2.json", "b.json");

        // Then
        var ambiguous = result.Report["findings"]!.AsArray().Where(static finding => finding!["kind"]!.GetValue<string>() == "ambiguousRun")
            .Select(static finding => finding!["path"]!.GetValue<string>() + " " + finding["identity"]!.GetValue<string>()).ToArray();
        ambiguous.Should().ContainSingle().Which.Should().Contain("test/a/a.csproj").And.NotContain("test/b/b.csproj");
        result.Report["cases"]!.AsArray().Single(static row => row!["project"]!.GetValue<string>() == "test/b/b.csproj")!["outcome"]!
            .GetValue<string>().Should().Be("passed");
    }

    [Fact]
    public void AVersionThreeRunLeavingAnOwnedCaseUnselected_WhenReconciled_ShouldReportAnIncompleteRunWithoutChangingTheExitCode()
    {
        // Given
        _root.Write("a.json", "a", 3, "run-a",
            new JsonArray(OwnedCase("case-1", true), Case("case-2", "M:Shared.Other", owned: true, selected: false)),
            new JsonArray(Outcome("case-1")));
        _root.Write("b.json", "b", 1, "run-b", new JsonArray(Case("case-1", "M:Shared.Checks")), new JsonArray(Outcome("case-1")));

        // When
        var result = _root.Reconcile("a.json", "b.json");

        // Then
        var finding = result.Report["findings"]!.AsArray().Should()
            .ContainSingle(static item => item!["kind"]!.GetValue<string>() == "incompleteRun").Subject!;
        finding["path"]!.GetValue<string>().Should().Be("a.json");
        finding["identity"]!.GetValue<string>().Should().Contain("test/a/a.csproj").And.Contain("net10.0");
        finding["message"]!.GetValue<string>().Should().Contain("1");
        result.ExitCode.Should().Be(0);
    }

    [Fact]
    public void AVersionThreeRunLeavingOnlyUnownedCasesUnselected_WhenReconciled_ShouldReportNoIncompleteRun()
    {
        // Given
        _root.Write("a.json", "a", 3, "run-a",
            new JsonArray(OwnedCase("case-1", true), Case("case-2", "M:Shared.Other", owned: false, selected: false)),
            new JsonArray(Outcome("case-1")));
        _root.Write("b.json", "b", 1, "run-b", new JsonArray(Case("case-1", "M:Shared.Checks")), new JsonArray(Outcome("case-1")));

        // When
        var result = _root.Reconcile("a.json", "b.json");

        // Then
        result.Report["findings"]!.AsArray().Should().BeEmpty();
        result.ExitCode.Should().Be(0);
    }

    [Fact]
    public void ABoundMethodWithNoDiscoveredCase_WhenReconciled_ShouldFailWithAMissingResultNamingTheMethod()
    {
        // Given
        _root.Write("a.json", "a", 1, "run-1", new JsonArray(Case("case-1", "M:Shared.Other")), new JsonArray(Outcome("case-1")));
        _root.Write("b.json", "b", 1, "run-1", new JsonArray(Case("case-1", "M:Shared.Other")), new JsonArray(Outcome("case-1")));

        // When
        var result = _root.Reconcile("a.json", "b.json");

        // Then
        result.ExitCode.Should().Be(1);
        result.Report["findings"]!.AsArray().Where(static item => item!["kind"]!.GetValue<string>() == "missingResult")
            .Select(static item => item!["message"]!.GetValue<string>()).Should().ContainSingle()
            .Which.Should().Contain("M:Shared.Checks");
    }

    [Fact]
    public void AnOwnedCaseItsRunDidNotSelect_WhenReconciled_ShouldFailWithAMissingResultNamingTheCase()
    {
        // Given
        _root.Write("a.json", "a", 3, "run-a",
            new JsonArray(OwnedCase("case-1", false), Case("case-2", "M:Shared.Other", owned: true, selected: true)),
            new JsonArray(Outcome("case-2")));
        _root.Write("b.json", "b", 1, "run-b", new JsonArray(Case("case-1", "M:Shared.Checks")), new JsonArray(Outcome("case-1")));

        // When
        var result = _root.Reconcile("a.json", "b.json");

        // Then
        result.ExitCode.Should().Be(1);
        MissingResults(result.Report).Should().ContainSingle().Which.Should().Contain("case-1");
    }

    [Theory]
    [ClassData(typeof(NotPassingOutcomes))]
    public void AnOwnedKeyNotPassingInItsOwnRun_WhenAnotherProjectPassedTheSameKeyUnowned_ShouldFailForTheOwnedKeyOnly(string outcome)
    {
        // Given
        _root.Write("a.json", "a", 3, "run-a",
            new JsonArray(OwnedCase("plain-case", true), UnownedCase("visual-case", true)),
            new JsonArray(Outcome("plain-case"), Outcome("visual-case")));
        _root.Write("b.json", "b", 3, "run-b",
            new JsonArray(UnownedCase("plain-case", false), OwnedCase("visual-case", true)),
            outcome == "absent" ? new JsonArray() : new JsonArray(Outcome("visual-case", outcome)));

        // When
        var result = _root.Reconcile("a.json", "b.json");

        // Then
        result.ExitCode.Should().Be(1);
        MissingResults(result.Report).Should().ContainSingle().Which.Should().Contain("visual-case");
        result.Report["summary"]!["passed"]!.GetValue<int>().Should().Be(1);
    }

    [Fact]
    public void TwoProjectsEachPassingTheKeysTheyOwn_WhenReconciled_ShouldSucceedWithoutCountingOrListingUnownedKeys()
    {
        // Given
        _root.Write("a.json", "a", 3, "run-a",
            new JsonArray(OwnedCase("plain-case", true), UnownedCase("visual-case", false)),
            new JsonArray(Outcome("plain-case")));
        _root.Write("b.json", "b", 3, "run-b",
            new JsonArray(UnownedCase("plain-case", false), OwnedCase("visual-case", true)),
            new JsonArray(Outcome("visual-case")));

        // When
        var result = _root.Reconcile("a.json", "b.json");

        // Then
        result.ExitCode.Should().Be(0);
        MissingResults(result.Report).Should().BeEmpty();
        result.Report["claims"]!.AsArray().Single()!["cases"]!.AsArray().Select(static item => item!.GetValue<string>())
            .Should().Equal("plain-case", "visual-case");
        result.Report["summary"]!["passed"]!.GetValue<int>().Should().Be(2);
        result.Report["summary"]!["failed"]!.GetValue<int>().Should().Be(0);
        result.Report["cases"]!.AsArray()
            .Select(static row => (row!["project"]!.GetValue<string>(), row["caseId"]!.GetValue<string>(), row["owned"]!.GetValue<bool>()))
            .Should().BeEquivalentTo([
                ("test/a/a.csproj", "plain-case", true), ("test/a/a.csproj", "visual-case", false),
                ("test/b/b.csproj", "plain-case", false), ("test/b/b.csproj", "visual-case", true)]);
    }

    [Theory]
    [ClassData(typeof(UnownedOutcomes))]
    public void AnUnownedCaseWithAnyOutcome_WhenReconciled_ShouldNeitherCountNorFailForIt(string outcome)
    {
        // Given
        _root.Write("a.json", "a", 3, "run-a",
            new JsonArray(OwnedCase("plain-case", true), UnownedCase("visual-case", true)),
            new JsonArray(Outcome("plain-case"), Outcome("visual-case", outcome)));
        _root.Write("b.json", "b", 1, "run-b", new JsonArray(Case("plain-case", "M:Shared.Checks")), new JsonArray(Outcome("plain-case")));

        // When
        var result = _root.Reconcile("a.json", "b.json");

        // Then
        result.ExitCode.Should().Be(0);
        result.Report["summary"]!["passed"]!.GetValue<int>().Should().Be(2);
        result.Report["summary"]!["failed"]!.GetValue<int>().Should().Be(0);
        result.Report["claims"]!.AsArray().Single()!["cases"]!.AsArray().Select(static item => item!.GetValue<string>())
            .Should().Equal("plain-case", "plain-case");
    }

    [Theory]
    [ClassData(typeof(CarriedWithoutOwnership))]
    public void AnArtifactCarryingNoOwnership_WhenItsCaseDidNotPass_ShouldGateItAsOwned(int formatVersion)
    {
        // Given
        _root.Write("a.json", "a", formatVersion, "run-a", new JsonArray(Case("case-1", "M:Shared.Checks")), new JsonArray(Outcome("case-1", "failed")));
        _root.Write("b.json", "b", formatVersion, "run-b", new JsonArray(Case("case-1", "M:Shared.Checks")), new JsonArray(Outcome("case-1")));

        // When
        var result = _root.Reconcile("a.json", "b.json");

        // Then
        result.ExitCode.Should().Be(1);
        MissingResults(result.Report).Should().ContainSingle().Which.Should().Contain("case-1");
        result.Report["cases"]!.AsArray().Should().OnlyContain(static row => row!["owned"]!.GetValue<bool>());
    }

    [Fact]
    public void ADeclaredProjectWithNoEvidence_WhenReconciled_ShouldFailAsIncompleteInputNamingTheProjectAndReportNoPassForItsKeys()
    {
        // Given
        _root.Write("a.json", "a", 3, "run-a",
            new JsonArray(OwnedCase("plain-case", true), UnownedCase("visual-case", false)),
            new JsonArray(Outcome("plain-case")));

        // When
        var result = _root.Reconcile("a.json");

        // Then
        result.ExitCode.Should().Be(2);
        result.Report["findings"]!.AsArray().Should().Contain(static finding => finding!["kind"]!.GetValue<string>() == "missingCompilation"
            && finding["path"]!.GetValue<string>() == "test/b/b.csproj");
        result.Report["cases"]!.AsArray().Should().NotContain(static row => row!["project"]!.GetValue<string>() == "test/b/b.csproj");
        result.Report["summary"]!["passed"]!.GetValue<int>().Should().Be(1);
    }

    [Fact]
    public void AnOutlineWithAPlannedAndAnActiveRow_WhenBothAreSkipped_ShouldReportThePlannedRowAndFailOnlyForTheActiveRow()
    {
        // Given
        _root.Write("a.json", "a", 3, "run-a",
            new JsonArray(OwnedCase("active-row", true), OwnedCase("planned-row", true)),
            new JsonArray(Outcome("active-row", "skipped"), Outcome("planned-row", "skipped")),
            new JsonArray(Scenario("outline-1", ("active-row", "active"), ("planned-row", "planned"))));
        _root.Write("b.json", "b", 1, "run-b", new JsonArray(Case("case-1", "M:Shared.Checks")), new JsonArray(Outcome("case-1")));

        // When
        var result = _root.Reconcile("a.json", "b.json");

        // Then
        result.ExitCode.Should().Be(1);
        MissingResults(result.Report).Should().ContainSingle().Which.Should().Contain("active-row");
        Row(result.Report, "planned-row")["planned"]!.GetValue<bool>().Should().BeTrue();
        (Row(result.Report, "active-row")["planned"]?.GetValue<bool>() ?? false).Should().BeFalse();
        result.Report["claims"]!.AsArray().Single()!["cases"]!.AsArray().Select(static item => item!.GetValue<string>())
            .Should().NotContain("planned-row");
    }

    [Fact]
    public void AnOutlineWithAPlannedAndAPassingActiveRow_WhenReconciled_ShouldSucceedCoveringTheClaimWithTheActiveRowOnly()
    {
        // Given
        _root.Write("a.json", "a", 3, "run-a",
            new JsonArray(OwnedCase("active-row", true), OwnedCase("planned-row", true)),
            new JsonArray(Outcome("active-row"), Outcome("planned-row", "skipped")),
            new JsonArray(Scenario("outline-1", ("active-row", "active"), ("planned-row", "planned"))));
        _root.Write("b.json", "b", 1, "run-b", new JsonArray(Case("case-1", "M:Shared.Checks")), new JsonArray(Outcome("case-1")));

        // When
        var result = _root.Reconcile("a.json", "b.json");

        // Then
        result.ExitCode.Should().Be(0);
        MissingResults(result.Report).Should().BeEmpty();
        Row(result.Report, "planned-row")["planned"]!.GetValue<bool>().Should().BeTrue();
        result.Report["claims"]!.AsArray().Single()!["cases"]!.AsArray().Select(static item => item!.GetValue<string>())
            .Should().Equal("active-row", "case-1");
    }

    [Theory]
    [ClassData(typeof(NotPassingOutcomes))]
    public void AnActiveRowBesideAPlannedBlock_WhenItsCaseDidNotPass_ShouldFailTheGateForThatRow(string outcome)
    {
        // Given
        _root.Write("a.json", "a", 3, "run-a",
            new JsonArray(OwnedCase("active-row", true), OwnedCase("planned-row", true)),
            outcome == "absent"
                ? new JsonArray(Outcome("planned-row", "skipped"))
                : new JsonArray(Outcome("active-row", outcome), Outcome("planned-row", "skipped")),
            new JsonArray(Scenario("outline-1", ("active-row", "active"), ("planned-row", "planned"))));
        _root.Write("b.json", "b", 1, "run-b", new JsonArray(Case("case-1", "M:Shared.Checks")), new JsonArray(Outcome("case-1")));

        // When
        var result = _root.Reconcile("a.json", "b.json");

        // Then
        result.ExitCode.Should().Be(1);
        MissingResults(result.Report).Should().ContainSingle().Which.Should().Contain("active-row");
    }

    [Fact]
    public void ACaseLinkedByAPlannedAndAnActiveScenario_WhenItFailed_ShouldBeActiveAndFailTheGate()
    {
        // Given
        _root.Write("a.json", "a", 3, "run-a",
            new JsonArray(OwnedCase("case-1", true)),
            new JsonArray(Outcome("case-1", "failed")),
            new JsonArray(Scenario("outline-1", ("case-1", "planned")), Scenario("outline-2", ("case-1", "active"))));
        _root.Write("b.json", "b", 1, "run-b", new JsonArray(Case("case-1", "M:Shared.Checks")), new JsonArray(Outcome("case-1")));

        // When
        var result = _root.Reconcile("a.json", "b.json");

        // Then
        result.ExitCode.Should().Be(1);
        MissingResults(result.Report).Should().ContainSingle().Which.Should().Contain("case-1");
        (Row(result.Report, "case-1")["planned"]?.GetValue<bool>() ?? false).Should().BeFalse();
    }

    [Fact]
    public void AClaimWhoseOnlyCasesArePlanned_WhenReconciled_ShouldHaveNoCoveringCaseAndFail()
    {
        // Given
        _root.Write("a.json", "a", 3, "run-a",
            new JsonArray(OwnedCase("planned-row", true)),
            new JsonArray(Outcome("planned-row")),
            new JsonArray(Scenario("outline-1", ("planned-row", "planned"))));
        _root.Write("b.json", "b", 3, "run-b",
            new JsonArray(OwnedCase("planned-row", true)),
            new JsonArray(Outcome("planned-row")),
            new JsonArray(Scenario("outline-1", ("planned-row", "planned"))));

        // When
        var result = _root.Reconcile("a.json", "b.json");

        // Then
        result.ExitCode.Should().Be(1);
        result.Report["claims"]!.AsArray().Single()!["cases"]!.AsArray().Should().BeEmpty();
        MissingResults(result.Report).Should().NotBeEmpty().And.OnlyContain(static message => message.Contains("M:Shared.Checks"));
    }

    [Fact]
    public void ABoundMethodWhoseEveryCaseIsUnownedInEveryProject_WhenReconciled_ShouldFailAsHavingNoOwnedCase()
    {
        // Given
        _root.Write("a.json", "a", 3, "run-a",
            new JsonArray(UnownedCase("case-1", true)), new JsonArray(Outcome("case-1")));
        _root.Write("b.json", "b", 3, "run-b",
            new JsonArray(UnownedCase("case-1", false)), new JsonArray());

        // When
        var result = _root.Reconcile("a.json", "b.json");

        // Then
        result.ExitCode.Should().Be(1);
        MissingResults(result.Report).Should().ContainSingle().Which.Should().Contain("M:Shared.Checks").And.Contain("no owned case");
        result.Report["summary"]!["passed"]!.GetValue<int>().Should().Be(0);
    }

    [Fact]
    public void APlannedLinkNamingNoCase_WhenReconciled_ShouldGateNothing()
    {
        // Given
        _root.Write("a.json", "a", 3, "run-a",
            new JsonArray(OwnedCase("case-1", true)),
            new JsonArray(Outcome("case-1")),
            new JsonArray(Scenario("outline-1", ("case-1", "active")), UnresolvedPlannedScenario("outline-2")));
        _root.Write("b.json", "b", 1, "run-b", new JsonArray(Case("case-1", "M:Shared.Checks")), new JsonArray(Outcome("case-1")));

        // When
        var result = _root.Reconcile("a.json", "b.json");

        // Then
        result.ExitCode.Should().Be(0);
        MissingResults(result.Report).Should().BeEmpty();
    }

    [Fact]
    public void ARequiredClaimsBoundMethodWithAnOwnedCaseTheRunDidNotSelect_WhenReconciled_ShouldReportBothTheIncompleteRunAndTheMissingResultAndExitOne()
    {
        // Given
        _root.Write("a.json", "a", 3, "run-a",
            new JsonArray(OwnedCase("case-1", false), OwnedCase("case-2", true)),
            new JsonArray(Outcome("case-2")));
        _root.Write("b.json", "b", 1, "run-b", new JsonArray(Case("case-1", "M:Shared.Checks")), new JsonArray(Outcome("case-1")));

        // When
        var result = _root.Reconcile("a.json", "b.json");

        // Then
        result.ExitCode.Should().Be(1);
        Kinds(result.Report).Should().Contain("incompleteRun").And.Contain("missingResult");
        MissingResults(result.Report).Should().ContainSingle().Which.Should().Contain("case-1");
    }

    [Fact]
    public void APlannedOwnedCaseTheRunDidNotSelect_WhenReconciled_ShouldNotBeCountedInTheIncompleteRunMessage()
    {
        // Given
        _root.Write("a.json", "a", 3, "run-a",
            new JsonArray(
                OwnedCase("active-row", true),
                OwnedCase("planned-row", false),
                Case("case-3", "M:Shared.Other", owned: true, selected: false)),
            new JsonArray(Outcome("active-row")),
            new JsonArray(Scenario("outline-1", ("active-row", "active"), ("planned-row", "planned"))));
        _root.Write("b.json", "b", 1, "run-b", new JsonArray(Case("case-1", "M:Shared.Checks")), new JsonArray(Outcome("case-1")));

        // When
        var result = _root.Reconcile("a.json", "b.json");

        // Then
        result.Report["findings"]!.AsArray().Single(static item => item!["kind"]!.GetValue<string>() == "incompleteRun")!["message"]!
            .GetValue<string>().Should().Contain("select 1 owned");
    }

    [Fact]
    public void AResolvedScenarioLinkWithNoCaseId_WhenReconciled_ShouldRejectTheArtifactAsInvalidInputAndGateNothing()
    {
        // Given
        _root.Write("a.json", "a", 3, "run-a",
            new JsonArray(OwnedCase("case-1", true)),
            new JsonArray(Outcome("case-1")),
            new JsonArray(Scenario("outline-1", ("case-1", "active")), ScenarioRecord("outline-2",
                [new JsonObject { ["rowId"] = null, ["caseId"] = null, ["status"] = "resolved", ["state"] = "active" }])));
        _root.Write("b.json", "b", 1, "run-b", new JsonArray(Case("case-1", "M:Shared.Checks")), new JsonArray(Outcome("case-1")));

        // When
        var result = _root.Reconcile("a.json", "b.json");

        // Then
        result.ExitCode.Should().Be(2);
        Kinds(result.Report).Should().Contain("invalidInput");
        MissingResults(result.Report).Should().BeEmpty();
        result.Report["cases"]!.AsArray().Should().NotContain(static row => row!["project"]!.GetValue<string>() == "test/a/a.csproj");
    }

    public void Dispose() => _root.Dispose();

    private static string[] Kinds(JsonObject report) =>
        report["findings"]!.AsArray().Select(static finding => finding!["kind"]!.GetValue<string>()).ToArray();

    private static JsonObject Case(string caseId, string methodSymbolId, bool? owned = null, bool? selected = null)
    {
        var testCase = new JsonObject
        {
            ["caseId"] = caseId,
            ["methodSymbolId"] = methodSymbolId,
            ["rowId"] = null,
            ["displayName"] = caseId,
        };
        if (owned is not null)
        {
            testCase["categories"] = new JsonArray();
            testCase["owned"] = owned;
            testCase["selected"] = selected;
        }

        return testCase;
    }

    private static JsonObject OwnedCase(string caseId, bool selected) => Case(caseId, "M:Shared.Checks", owned: true, selected: selected);

    private static JsonObject UnownedCase(string caseId, bool selected) => Case(caseId, "M:Shared.Checks", owned: false, selected: selected);

    private static JsonObject Outcome(string caseId, string outcome = "passed") => new() { ["caseId"] = caseId, ["outcome"] = outcome, ["detail"] = null };

    private static string[] MissingResults(JsonObject report) => report["findings"]!.AsArray()
        .Where(static finding => finding!["kind"]!.GetValue<string>() == "missingResult")
        .Select(static finding => finding!["message"]!.GetValue<string>()).ToArray();

    private static JsonNode Row(JsonObject report, string caseId) => report["cases"]!.AsArray()
        .Single(row => row!["project"]!.GetValue<string>() == "test/a/a.csproj" && row["caseId"]!.GetValue<string>() == caseId)!;

    private static JsonObject Scenario(string scenarioId, params (string CaseId, string State)[] links) => ScenarioRecord(scenarioId,
        links.Select(static link => (JsonNode?)new JsonObject
        {
            ["rowId"] = null,
            ["caseId"] = link.CaseId,
            ["status"] = "resolved",
            ["state"] = link.State,
        }).ToArray());

    private static JsonObject UnresolvedPlannedScenario(string scenarioId) => ScenarioRecord(scenarioId,
        [new JsonObject { ["rowId"] = "row-9", ["caseId"] = null, ["status"] = "unresolved", ["state"] = "planned" }]);

    private static JsonObject ScenarioRecord(string scenarioId, JsonNode?[] links) => new()
    {
        ["path"] = "src/read/.spec/read.feature",
        ["scenarioId"] = scenarioId,
        ["line"] = 1,
        ["scope"] = new JsonObject { ["epic"] = null, ["feature"] = "cap:read" },
        ["claims"] = new JsonArray(new JsonObject
        {
            ["id"] = "need/7",
            ["status"] = "resolved",
            ["isEligible"] = true,
            ["target"] = new JsonObject
            {
                ["kind"] = "claim",
                ["epic"] = null,
                ["feature"] = "cap:read",
                ["id"] = "need/7",
                ["state"] = "Active",
                ["featureState"] = "approved",
                ["relativePath"] = "src/read/.spec/README.md",
                ["line"] = 8,
            },
            ["matches"] = new JsonArray(),
        }),
        ["cases"] = new JsonArray(links),
    };

    private readonly PartitionedEvidenceRoot _root = new();
}
