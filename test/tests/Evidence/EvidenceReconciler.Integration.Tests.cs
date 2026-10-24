using System.Text.Json.Nodes;
using AwesomeAssertions;

namespace Specht.Tests.Evidence;

/// <summary>Several projects' runs, the incomplete-run finding and the undiscovered bound method (0201-F3 B-014, B-021, B-026; OQ-4, OQ-5).</summary>
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
            new JsonArray(Case("case-1", "M:Shared.Checks", owned: true, selected: true), Case("case-2", "M:Shared.Other", owned: true, selected: false)),
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
            new JsonArray(Case("case-1", "M:Shared.Checks", owned: true, selected: true), Case("case-2", "M:Shared.Other", owned: false, selected: false)),
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
    public void ABoundMethodWhoseOnlyCaseItsRunFilteredOut_WhenReconciled_ShouldFailWithAMissingResultNamingTheMethod()
    {
        // Given
        _root.Write("a.json", "a", 3, "run-a",
            new JsonArray(Case("case-1", "M:Shared.Checks", owned: true, selected: false), Case("case-2", "M:Shared.Other", owned: true, selected: true)),
            new JsonArray(Outcome("case-2")));
        _root.Write("b.json", "b", 1, "run-b", new JsonArray(Case("case-1", "M:Shared.Other")), new JsonArray(Outcome("case-1")));

        // When
        var result = _root.Reconcile("a.json", "b.json");

        // Then
        result.ExitCode.Should().Be(1);
        result.Report["findings"]!.AsArray().Where(static item => item!["kind"]!.GetValue<string>() == "missingResult")
            .Select(static item => item!["message"]!.GetValue<string>()).Should().ContainSingle()
            .Which.Should().Contain("M:Shared.Checks");
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

    private static JsonObject Outcome(string caseId) => new() { ["caseId"] = caseId, ["outcome"] = "passed", ["detail"] = null };

    private readonly PartitionedEvidenceRoot _root = new();
}
