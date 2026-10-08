using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using specht.tests;
using specht.tool;
using specht.tool.Features.Check;
using Spectre.Console.Cli.Testing;
using Spectre.Console.Testing;

namespace specht.acceptance.Check;

/// <summary>
/// Steps for <c>src/specht.tool/Features/Check/.spec/check.feature</c> (0001-F2). A run goes through Spectre's command
/// tester over a synthetic tree (C-7); the one run from inside the root launches the built tool, because the working
/// directory is the process's. Scoped to the feature: <c>engine.feature</c> words some of its steps the same way, and its
/// steps are 0001-F1's to bind.
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

    [When("the check runs")]
    [When("the check runs without asking for the JSON document")]
    public void WhenTheCheckRuns()
    {
        var services = new ServiceCollection();
        services.AddSingleton<Func<string, SpecCheckReport>>(SpecCheckRunner.Run);

        var app = new CommandAppTester(new TypeRegistrar(services), console: new TestConsole().Width(80));
        app.SetDefaultCommand<CheckCommand>();

        var result = app.Run("--root", Tree.Root);
        _stdout = result.Output;
        _exitCode = result.ExitCode;
        _expected = Tree.Run().Violations.Select(static violation => violation.ToString()).ToArray();
    }

    [When("the check runs from inside the root without naming it")]
    public void WhenTheCheckRunsFromInsideTheRootWithoutNamingIt()
    {
        Copy(Tree.Root, Nested);

        var start = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = Nested,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        start.ArgumentList.Add(typeof(CheckCommand).Assembly.Location);

        using var process = Process.Start(start) ?? throw new InvalidOperationException("dotnet did not start.");
        var stdout = process.StandardOutput.ReadToEndAsync();
        _stderr = process.StandardError.ReadToEnd();
        _stdout = stdout.GetAwaiter().GetResult();
        process.WaitForExit();
        _exitCode = process.ExitCode;
    }

    [Then("the standard output carries one line per violation")]
    public void ThenTheStandardOutputCarriesOneLinePerViolation()
    {
        _expected.Should().NotBeEmpty();
        Lines(_stdout).Should().Equal(_expected);
    }

    [Then("each line names the file, the line, the severity, the rule id and the message in the build's diagnostic form")]
    public void ThenEachLineNamesTheFileTheLineTheSeverityTheRuleIdAndTheMessageInTheBuildsDiagnosticForm() =>
        Lines(_stdout).Should().NotBeEmpty().And.AllSatisfy(line =>
        {
            var match = Diagnostic().Match(line);
            match.Success.Should().BeTrue(line);
            File.Exists(Path.Combine(Tree.Root, match.Groups["file"].Value)).Should().BeTrue(line);
        });

    [Then("the exit code is {int}")]
    public void ThenTheExitCodeIs(int code) => _exitCode.Should().Be(code);

    [Then("the standard output holds the violation lines and the summary and nothing else")]
    public void ThenTheStandardOutputHoldsTheViolationLinesAndTheSummaryAndNothingElse() =>
        Lines(_stdout).Should().Equal(_expected);

    [Then("nothing about the tool itself appears on the standard output")]
    public void ThenNothingAboutTheToolItselfAppearsOnTheStandardOutput() =>
        Lines(_stdout).Should().AllSatisfy(static line => Diagnostic().IsMatch(line).Should().BeTrue(line));

    [Then("every path on the standard output is relative to the root")]
    public void ThenEveryPathOnTheStandardOutputIsRelativeToTheRoot()
    {
        _exitCode.Should().Be(ExitCodes.Violations);
        Lines(_stdout).Should().NotBeEmpty().And.AllSatisfy(line =>
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

    private const string TwoClaims =
        "## 3. Acceptance Criteria\n\n| ID | Claim | Source | Status |\n| -- | ----- | ------ | ------ |\n"
            + "| B-001 | It does the thing. | brd | Active |\n| B-002 | It does another thing. | brd | Active |\n";

    private SpecTree? _tree;
    private string? _nested;
    private string _stdout = string.Empty;
    private string _stderr = string.Empty;
    private string[] _expected = [];
    private int _exitCode = -1;
}
