using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Reqnroll;
using specht.Report;
using specht.tests;
using specht.tool;

namespace specht.acceptance.Check;

/// <summary>
/// Steps for <c>src/specht.tool/Features/Check/.spec/check.feature</c> (0001-F2). A run launches the built tool over a
/// synthetic tree, so a scenario sees the process's stdout, stderr and exit code as a shell does: stderr is not captured
/// by Spectre's command tester, which the integration tier uses (C-7). Scoped to the feature: <c>engine.feature</c> words
/// some of its steps the same way, and its steps are 0001-F1's to bind.
/// </summary>
[Binding]
[Scope(Feature = "The check command")]
public sealed partial class CheckSteps
{
    [Given("a repository root holding a manifest and the three frontmatter schemas")]
    public void GivenARepositoryRootHoldingAManifestAndTheThreeFrontmatterSchemas() => _tree = new SpecTree();

    [Given("the root holds a specification whose claim B-002 has no traceability row")]
    public void GivenTheRootHoldsASpecificationWhoseClaimB002HasNoTraceabilityRow() =>
        Tree.WriteFeature("0001", "F1", sections: SpecTree.SectionsWith("3. Acceptance Criteria", TwoClaims));

    [Given("the root holds two specifications in the legacy layout and one in the co-located layout")]
    public void GivenTheRootHoldsTwoSpecificationsInTheLegacyLayoutAndOneInTheCoLocatedLayout()
    {
        Tree.WriteFeature("0001", "F1");
        Tree.WriteFeature("0001", "F2");
        Tree.WriteCoLocatedFeature("src/area", "0001", "F3");
        var report = Tree.Run();
        report.LegacyCount.Should().Be(2);
        report.CoLocatedCount.Should().Be(1);
    }

    [Given("the root holds a specification with one error-severity violation")]
    [Given("the root holds a specification with one violation")]
    public void GivenTheRootHoldsASpecificationWithOneErrorSeverityViolation()
    {
        Tree.WriteFeature("0001", "F1", featureFile: null);
        Tree.Run().Violations.Should().ContainSingle().Which.Severity.Should().Be(SpecSeverity.Error);
    }

    [Given("the root holds specifications with no violation")]
    public void GivenTheRootHoldsSpecificationsWithNoViolation()
    {
        Tree.WriteFeature("0001", "F1");
        Tree.WriteCoLocatedFeature("src/area", "0001", "F2");
        Tree.Run().Violations.Should().BeEmpty();
    }

    [Given("the root is a deeply nested directory on this machine")]
    public void GivenTheRootIsADeeplyNestedDirectoryOnThisMachine() =>
        _nested = Path.Combine(Path.GetTempPath(), "specht-check", Guid.NewGuid().ToString("N"), "a", "b", "c", "d", "e", "f");

    [Given("a root path that does not exist")]
    public void GivenARootPathThatDoesNotExist() => _typed = "no-such-root";

    [Given("a root directory with no manifest at the manifest path")]
    public void GivenARootDirectoryWithNoManifestAtTheManifestPath()
    {
        File.Delete(Path.Combine(Tree.Root, SpecManifest.RelativePath));
        _typed = ".";
    }

    [Given("the root's manifest is not well-formed JSON")]
    public void GivenTheRootsManifestIsNotWellFormedJson() => Tree.WriteRaw(SpecManifest.RelativePath, "{ \"sections\": ");

    [When("the check runs")]
    [When("the check runs without asking for the JSON document")]
    public void WhenTheCheckRuns() => Launch(Tree.Root, "--root", ".");

    [When("the check runs against it")]
    public void WhenTheCheckRunsAgainstIt() => Launch(Tree.Root, "--root", _typed ?? throw new InvalidOperationException("No root was typed."));

    [When("the check runs from inside the root without naming it")]
    public void WhenTheCheckRunsFromInsideTheRootWithoutNamingIt()
    {
        Copy(Tree.Root, Nested);
        Launch(Nested);
    }

    [Then("the standard output carries one line per violation")]
    public void ThenTheStandardOutputCarriesOneLinePerViolation()
    {
        Violations.Should().NotBeEmpty();
        Lines(_stdout).Should().Equal(Expected);
    }

    [Then("each line names the file, the line, the severity, the rule id and the message in the build's diagnostic form")]
    public void ThenEachLineNamesTheFileTheLineTheSeverityTheRuleIdAndTheMessageInTheBuildsDiagnosticForm() =>
        ViolationLines().Should().NotBeEmpty().And.AllSatisfy(line =>
        {
            var match = Diagnostic().Match(line);
            match.Success.Should().BeTrue(line);
            File.Exists(Path.Combine(Tree.Root, match.Groups["file"].Value)).Should().BeTrue(line);
        });

    [Then("the standard output ends with the specification count per layout")]
    public void ThenTheStandardOutputEndsWithTheSpecificationCountPerLayout()
    {
        var summary = Summary;
        Lines(_stdout).TakeLast(summary.Count).Should().Equal(summary);
        SummaryLine(0).Should().Contain("legacy 2").And.Contain("coLocated 1");
    }

    [Then("the item count and the count of rule ids evaluated")]
    public void ThenTheItemCountAndTheCountOfRuleIdsEvaluated()
    {
        var report = Tree.Run();
        var summary = Summary;
        SummaryLine(1).Should().Be(summary[1]).And.Contain(report.ItemCount.ToString(CultureInfo.InvariantCulture));
        SummaryLine(2).Should().Be(summary[2]).And.Contain(report.RulesEvaluated.ToString(CultureInfo.InvariantCulture));
    }

    [Then("the error count and the warning count")]
    public void ThenTheErrorCountAndTheWarningCount()
    {
        var report = Tree.Run();
        SummaryLine(3).Should().Be(Summary[3])
            .And.Contain(report.ErrorCount.ToString(CultureInfo.InvariantCulture))
            .And.Contain(report.WarningCount.ToString(CultureInfo.InvariantCulture));
    }

    [Then("the exit code is {int}")]
    public void ThenTheExitCodeIs(int code) => _exitCode.Should().Be(code, _stderr);

    [Then("the standard output holds the violation lines and the summary and nothing else")]
    public void ThenTheStandardOutputHoldsTheViolationLinesAndTheSummaryAndNothingElse() =>
        Lines(_stdout).Should().Equal(Expected);

    [Then("nothing about the tool itself appears on the standard output")]
    public void ThenNothingAboutTheToolItselfAppearsOnTheStandardOutput()
    {
        var summary = Summary;
        ViolationLines().Should().AllSatisfy(static line => Diagnostic().IsMatch(line).Should().BeTrue(line));
        Lines(_stdout).TakeLast(summary.Count).Should().Equal(summary);
    }

    [Then("the standard error names that path as it was typed")]
    public void ThenTheStandardErrorNamesThatPathAsItWasTyped() => _stderr.Should().Contain(_typed);

    [Then("the standard error names the manifest path")]
    public void ThenTheStandardErrorNamesTheManifestPath() => _stderr.Should().Contain(SpecManifest.RelativePath);

    [Then("the standard error names the manifest path relative to the root")]
    public void ThenTheStandardErrorNamesTheManifestPathRelativeToTheRoot() =>
        _stderr.Should().Contain(SpecManifest.RelativePath).And.NotContain(Tree.Root);

    [Then("the standard output is empty")]
    public void ThenTheStandardOutputIsEmpty() => _stdout.Should().BeEmpty();

    [Then("every path on the standard output is relative to the root")]
    public void ThenEveryPathOnTheStandardOutputIsRelativeToTheRoot()
    {
        _exitCode.Should().Be(ExitCodes.Violations);
        ViolationLines().Should().NotBeEmpty().And.AllSatisfy(line =>
        {
            var file = Diagnostic().Match(line).Groups["file"].Value;
            Path.IsPathRooted(file).Should().BeFalse(line);
            File.Exists(Path.Combine(Nested, file)).Should().BeTrue(line);
        });
        _stdout.Should().NotContain(Nested).And.NotContain(Tree.Root);
    }

    [Then("every path on the standard error is relative to the root")]
    public void ThenEveryPathOnTheStandardErrorIsRelativeToTheRoot() =>
        _stderr.Should().NotContain(Nested).And.NotContain(Tree.Root).And.NotContain(Path.GetTempPath());

    [Then("no path in either uses the platform's directory separator where it differs from a forward slash")]
    public void ThenNoPathInEitherUsesThePlatformsDirectorySeparatorWhereItDiffersFromAForwardSlash()
    {
        if (Path.DirectorySeparatorChar != '/')
        {
            (_stdout + _stderr).Should().NotContain(Path.DirectorySeparatorChar.ToString());
        }
    }

    [AfterScenario]
    public void DeleteRoots()
    {
        _tree?.Dispose();
        if (_nested is not null && Directory.Exists(_nested))
        {
            Directory.Delete(Path.GetFullPath(Path.Combine(_nested, "..", "..", "..", "..", "..", "..")), recursive: true);
        }
    }

    private SpecTree Tree => _tree ?? throw new InvalidOperationException("No repository root was prepared.");

    private string Nested => _nested ?? throw new InvalidOperationException("No nested root was named.");

    private string[] Violations => Tree.Run().Violations.Select(static violation => violation.ToString()).ToArray();

    private IReadOnlyList<string> Summary => SpecReportDocument.From(Tree.Run()).SummaryLines();

    private string[] Expected => [.. Violations, .. Summary];

    [GeneratedRegex(@"^(?<file>[^\s(:][^(:]*)(\((?<line>\d+)\))?: (?<severity>error|warning) (?<rule>SPEC\d{3}): \S.*$")]
    private static partial Regex Diagnostic();

    private static string[] Lines(string output) =>
        output.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static void Copy(string from, string to)
    {
        foreach (var directory in Directory.GetDirectories(from, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(Path.Combine(to, Path.GetRelativePath(from, directory)));
        }

        Directory.CreateDirectory(to);
        foreach (var file in Directory.GetFiles(from, "*", SearchOption.AllDirectories))
        {
            File.Copy(file, Path.Combine(to, Path.GetRelativePath(from, file)));
        }
    }

    private string[] ViolationLines() => Lines(_stdout).SkipLast(Summary.Count).ToArray();

    private string SummaryLine(int index)
    {
        var tail = Lines(_stdout).TakeLast(Summary.Count).ToArray();
        tail.Should().HaveCount(Summary.Count, _stdout);
        return tail[index];
    }

    private void Launch(string workingDirectory, params string[] args) =>
        (_exitCode, _stdout, _stderr) = ToolRun.Launch(workingDirectory, args);

    private const string TwoClaims =
        "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n"
            + "| B-001 | It does the thing. | brd | Active |\n| B-002 | It does another thing. | brd | Active |\n";

    private SpecTree? _tree;
    private string? _nested;
    private string? _typed;
    private string _stdout = string.Empty;
    private string _stderr = string.Empty;
    private int _exitCode = -1;
}
