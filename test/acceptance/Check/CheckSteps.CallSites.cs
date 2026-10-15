using System.Diagnostics;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Reqnroll;
using Reqnroll.UnitTestProvider;
using Specht.Tests.Shared;
using Specht.Tool;

namespace Specht.Acceptance.Check;

public sealed partial class CheckSteps(IUnitTestRuntimeProvider runtime, PackedTool packed)
{
    [Given("the tool honours rule settings in the manifest")]
    public void GivenTheToolHonoursRuleSettingsInTheManifest()
    {
        if (OperatingSystem.IsWindows())
        {
            runtime.TestIgnore("The pre-commit hook is POSIX sh and the Specht target is reached through build.sh; neither runs on Windows.");
        }

        JsonNode.Parse(File.ReadAllText(Path.Combine(packed.Repository, SpecTree.ManifestPath)))!["rules"]!.AsObject().Should().NotBeEmpty();
    }

    [Given("a prepared root holding one error-severity and one warning-severity violation")]
    public void GivenAPreparedRootHoldingOneErrorSeverityAndOneWarningSeverityViolation()
    {
        var specification = Tree.WriteFeature(
            "0001",
            "F1",
            new Dictionary<string, string> { ["spec_status"] = "approved" },
            SpecTree.SectionsWith("12. Sign-off", SignedOff));
        Tree.WriteFeatureFile(specification, UnresolvedTag);
        _feature = Path.GetRelativePath(Tree.Root, Path.Combine(Path.GetDirectoryName(specification)!, "feature.feature")).Replace('\\', '/');
    }

    [Given("the root carries this repository's source, which the build target builds the tool from")]
    public void GivenTheRootCarriesThisRepositorysSourceWhichTheBuildTargetBuildsTheToolFrom()
    {
        CopyFromRepository(
            Listed().Where(static file => file.Split('/') is var parts && parts[0] != "epics" && !parts.Any(static part => part is ".spec" or ".issue")));
        var manifestPath = Path.Combine(Tree.Root, SpecTree.ManifestPath);
        var manifest = JsonNode.Parse(File.ReadAllText(manifestPath))!.AsObject();
        manifest["rules"] = new JsonObject { ["SPEC060"] = "warning" };
        File.WriteAllText(manifestPath, manifest.ToJsonString());

        Succeed(Tree.Root, "git", "init", "--quiet");
        Succeed(Tree.Root, "git", "add", "--all");
        Succeed(Tree.Root, "git", "rm", "--cached", "--quiet", Feature);
        Succeed(Tree.Root, "git", [.. Identity, "commit", "--quiet", "--no-verify", "--message", "The root before the staged edit"]);
        Succeed(Tree.Root, "git", "add", Feature);
        _source = Succeed(Tree.Root, "git", "rev-parse", "--show-toplevel").TrimEnd('\n');
        Succeed(Source, "dotnet", "tool", "restore");
        Succeed(Source, "dotnet", "husky", "install");
    }

    [Given("this repository's own tree")]
    public void GivenThisRepositorysOwnTree()
    {
        CopyFromRepository(Listed("--cached", "--others", "--exclude-standard"));
        _source = Succeed(Tree.Root, "pwd", "-P").TrimEnd('\n');
        Launch(Source, "--root", Source);
    }

    [When("the check runs against that root from a shell")]
    public void WhenTheCheckRunsAgainstThatRootFromAShell() => Launch(Source, "--root", Source);

    [When("the check runs against that root through the build target")]
    public void WhenTheCheckRunsAgainstThatRootThroughTheBuildTarget() =>
        _target = RunProcess(Source, "bash", Path.Combine(packed.Repository, "build.sh"), "Specht", "--root", Source);

    [When("the check runs against that root through the pre-commit hook")]
    public void WhenTheCheckRunsAgainstThatRootThroughThePreCommitHook() => _hook = RunProcess(Source, "sh", ".husky/pre-commit");

    [When("the build's Specht target runs")]
    public void WhenTheBuildsSpechtTargetRuns() => _target = RunProcess(Source, "bash", Path.Combine(Source, "build.sh"), "Specht");

    [Then("all three carry the lines the check prints, unchanged and in order")]
    public void ThenAllThreeCarryTheLinesTheCheckPrintsUnchangedAndInOrder()
    {
        _exitCode.Should().Be(ExitCodes.Violations, _stderr);
        Lines(_stdout)
            .Where(static line => Diagnostic().IsMatch(line))
            .Select(static line => Diagnostic().Match(line).Groups["severity"].Value)
            .Should().BeEquivalentTo(["error", "warning"], _stdout);

        var printed = Printed(_stdout);
        Carried(Target.Stdout).Should().ContainInConsecutiveOrder(printed, Target.Stdout);
        Carried(Hook.Stdout).Should().ContainInConsecutiveOrder(printed, Hook.Stdout);
    }

    [Then("a log prefix on each line and a call site's own lines around them are all that differ")]
    public void ThenALogPrefixOnEachLineAndACallSitesOwnLinesAroundThemAreAllThatDiffer()
    {
        Lines(_stdout).Should().Equal(Expected);

        var printed = Printed(_stdout);
        Unprefixed(Target.Stdout).Should().ContainInConsecutiveOrder(printed, Target.Stdout);
        Unprefixed(Hook.Stdout).Should().ContainInConsecutiveOrder(printed, Hook.Stdout);
    }

    [Then("all three exit with the same code")]
    public void ThenAllThreeExitWithTheSameCode()
    {
        Target.ExitCode.Should().Be(_exitCode, Target.Stdout);
        Hook.ExitCode.Should().Be(_exitCode, Hook.Stdout);
    }

    [Then("the check runs against this repository's root")]
    public void ThenTheCheckRunsAgainstThisRepositorysRoot()
    {
        _stdout.Should().NotBeEmpty(_stderr);
        Carried(Target.Stdout).Should().Equal(Printed(_stdout), Target.Stdout);
    }

    [Then("the target exits with the check's exit code")]
    public void ThenTheTargetExitsWithTheChecksExitCode() => Target.ExitCode.Should().Be(_exitCode, Target.Stdout);

    private static string[] Identity => ["-c", "user.name=specht-acceptance", "-c", "user.email=acceptance@example.invalid", "-c", "commit.gpgsign=false"];

    private string Source => _source ?? throw new InvalidOperationException("The root carries no source.");

    private string Feature => _feature ?? throw new InvalidOperationException("No specification was prepared.");

    private (string Stdout, string Stderr, int ExitCode) Target => _target ?? throw new InvalidOperationException("The build target did not run.");

    private (string Stdout, string Stderr, int ExitCode) Hook => _hook ?? throw new InvalidOperationException("The pre-commit hook did not run.");

    [GeneratedRegex(@"\x1b\[[0-9;]*m")]
    private static partial Regex AnsiEscape();

    [GeneratedRegex(@"^\d{2}:\d{2}:\d{2} \[DBG\] (?<line>.*)$")]
    private static partial Regex LogPrefix();

    private static string[] Printed(string stdout) => stdout.TrimEnd('\n').Split('\n');

    private static string[] Plain(string output) => AnsiEscape().Replace(output, string.Empty).Split('\n');

    private static string[] Carried(string output) =>
        [
            .. Plain(output)
                .Select(static line => LogPrefix().Match(line))
                .Where(static match => match.Success)
                .Select(static match => match.Groups["line"].Value),
        ];

    private static string[] Unprefixed(string output) =>
        [.. Plain(output).Select(static line => LogPrefix().Match(line) is { Success: true } match ? match.Groups["line"].Value : line)];

    private static (string Stdout, string Stderr, int ExitCode) RunProcess(string workingDirectory, string fileName, params string[] arguments)
    {
        var start = new ProcessStartInfo(fileName)
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardInput = true,
            Environment = { ["MSBUILDDISABLENODEREUSE"] = "1", ["NO_COLOR"] = "1" },
        };
        foreach (var argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        return AcceptanceTool.Run(start);
    }

    private static string Succeed(string workingDirectory, string fileName, params string[] arguments)
    {
        var (stdout, stderr, exitCode) = RunProcess(workingDirectory, fileName, arguments);
        exitCode.Should().Be(0, stdout + stderr);

        return stdout;
    }

    private string[] Listed(params string[] options) =>
        Succeed(packed.Repository, "git", ["ls-files", "-z", .. options]).Split('\0', StringSplitOptions.RemoveEmptyEntries);

    private void CopyFromRepository(IEnumerable<string> files)
    {
        foreach (var file in files.Where(file => File.Exists(Path.Combine(packed.Repository, file))))
        {
            var copy = Path.Combine(Tree.Root, file);
            Directory.CreateDirectory(Path.GetDirectoryName(copy)!);
            File.Copy(Path.Combine(packed.Repository, file), copy, overwrite: true);
        }
    }

    private const string SignedOff =
        "## 12. Sign-off\n\n| Section | Status | Reviewer | Note |\n| ------- | ------ | -------- | ---- |\n| 1-5 | \U0001F7E2 | spec-reviewer | Approved |\n";

    private const string UnresolvedTag =
        "Feature: it\n\n  @B-001\n  Scenario: It does the thing\n    Given a thing\n\n"
            + "  @B-002\n  Scenario: It does a thing no claim states\n    Given another thing\n";

    private string? _source;
    private string? _feature;
    private (string Stdout, string Stderr, int ExitCode)? _target;
    private (string Stdout, string Stderr, int ExitCode)? _hook;
}
