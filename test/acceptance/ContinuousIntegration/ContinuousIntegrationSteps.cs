using AwesomeAssertions;
using Reqnroll;
using YamlDotNet.RepresentationModel;

namespace Specht.Acceptance.ContinuousIntegration;

/// <summary>
/// Steps for <c>.build/ContinuousIntegration/.spec/continuous-integration.feature</c> (0055-F2). Each scenario reads the
/// committed <c>.github/workflows/ci.yml</c>, the file GitHub runs, and never calls GitHub (C-3). What a runner then
/// does with it - a red check on a failing target - is observed on the pull request that delivers it.
/// </summary>
[Binding]
public sealed class ContinuousIntegrationSteps
{
    [When("a pull request targeting the main branch is opened")]
    public void WhenAPullRequestTargetingTheMainBranchIsOpened() => Branches("pull_request").Should().Equal("main");

    [When("a commit is pushed to the main branch")]
    public void WhenACommitIsPushedToTheMainBranch() => Branches("push").Should().Equal("main");

    [When("integration runs")]
    [When("integration runs on one operating system")]
    [When("integration runs on a pull request or on the main branch")]
    [Given("a change whose tests fail on Windows only")]
    [Given("two runs on different commits")]
    public void WhenIntegrationRuns() => Jobs().Should().NotBeEmpty();

    [Then("the build runs against the pull request's head")]
    public void ThenTheBuildRunsAgainstThePullRequestsHead() =>
        Jobs().Select(static job => Checkout(job)["ref"]).Should().AllBe(HeadCommit);

    // On a push the head-commit expression is empty, and checkout falls back to the pushed commit.
    [Then("the build runs against that commit")]
    public void ThenTheBuildRunsAgainstThatCommit() =>
        Jobs().Select(static job => Checkout(job)["ref"]).Should().AllBe(HeadCommit);

    [Then("the build runs on Linux and on Windows")]
    public void ThenTheBuildRunsOnLinuxAndOnWindows() =>
        Jobs().Should().AllSatisfy(static job =>
        {
            Scalar(job, "runs-on").Should().Be("${{ matrix.os }}");
            Images(job).Should().BeEquivalentTo("ubuntu-latest", "windows-latest");
        });

    [Then("the format, compile, test, self-check and pack gates each run")]
    public void ThenTheFormatCompileTestSelfCheckAndPackGatesEachRun()
    {
        foreach (var job in Jobs())
        {
            Runs(job).Should().AllSatisfy(static run => run.Should().StartWith("./build.cmd --target "));
            Runs(job).Select(static run => run.Split(' ')[2]).Should().Contain(["Format", "Compile", "Test", "Specht", "Pack"]);
        }
    }

    [Then("the Windows check fails")]
    public void ThenTheWindowsCheckFails()
    {
        // A failing target exits the entry script non-zero (0055-F1 B-018); nothing here may swallow that, and a failure
        // on one leg never cancels the other.
        var job = Jobs().Single(static job => Images(job).Contains("windows-latest"));
        Runs(job).Should().Contain(static run => run.Contains("--target Test", StringComparison.Ordinal));
        FailFast(job).Should().Be("false");
        job.Children.Keys.Select(static key => key.ToString()).Should().NotContain(["continue-on-error", "needs"]);
        Steps(job).Should().AllSatisfy(static step => step.Children.Keys.Select(static key => key.ToString()).Should().NotContain("continue-on-error"));
    }

    // A matrix leg's check is named by its job and its image, both literals in the committed file.
    [Then("each operating system's check has the same name in both")]
    public void ThenEachOperatingSystemsCheckHasTheSameNameInBoth() =>
        ((YamlMappingNode)Workflow["jobs"]).Children.Keys.Select(static key => key.ToString())
            .Concat(Jobs().SelectMany(Images))
            .Should().AllSatisfy(static name => name.Should().NotContain("${{"));

    [Then("no package is pushed to any feed")]
    public void ThenNoPackageIsPushedToAnyFeed()
    {
        Permissions().Should().NotContain(static permission => permission.Value == "write");

        var steps = Jobs().SelectMany(Steps).Select(static step => step.ToString()).ToArray();
        steps.Should().NotContain(static step => step.Contains("push", StringComparison.OrdinalIgnoreCase));
        steps.Should().NotContain(static step => step.Contains("upload-artifact", StringComparison.Ordinal));
    }

    [Then("the Linux and Windows builds each report as a separate check")]
    public void ThenTheLinuxAndWindowsBuildsEachReportAsASeparateCheck()
    {
        var job = Jobs().Should().ContainSingle().Subject;
        Images(job).Should().HaveCount(2).And.OnlyHaveUniqueItems();
        FailFast(job).Should().Be("false");
    }

    private static YamlMappingNode Workflow { get; } = Load();

    private static YamlMappingNode Load()
    {
        var stream = new YamlStream();
        stream.Load(new StringReader(File.ReadAllText(Path.Combine(FindRepository(AppContext.BaseDirectory), ".github", "workflows", "ci.yml"))));
        return (YamlMappingNode)stream.Documents[0].RootNode;
    }

    private static string FindRepository(string directory) =>
        File.Exists(Path.Combine(directory, "build.sh")) && Directory.Exists(Path.Combine(directory, ".nuke"))
            ? directory
            : FindRepository(Path.GetDirectoryName(directory.TrimEnd(Path.DirectorySeparatorChar))
                             ?? throw new InvalidOperationException("No repository root above the test assembly."));

    private static IEnumerable<string> Branches(string trigger) =>
        ((YamlSequenceNode)((YamlMappingNode)((YamlMappingNode)Workflow["on"])[trigger])["branches"]).Select(static branch => branch.ToString());

    private static IEnumerable<KeyValuePair<string, string>> Permissions() =>
        ((YamlMappingNode)Workflow["permissions"]).Children.Select(static pair => KeyValuePair.Create(pair.Key.ToString(), pair.Value.ToString()));

    private static IEnumerable<YamlMappingNode> Jobs() => ((YamlMappingNode)Workflow["jobs"]).Children.Values.Cast<YamlMappingNode>();

    private static IEnumerable<YamlMappingNode> Steps(YamlMappingNode job) => ((YamlSequenceNode)job["steps"]).Cast<YamlMappingNode>();

    private static string Scalar(YamlMappingNode node, string key) => node[key].ToString();

    private static Dictionary<string, string> Checkout(YamlMappingNode job)
    {
        var checkout = Steps(job).Single(static step =>
            step.Children.TryGetValue("uses", out var uses) && uses.ToString().StartsWith("actions/checkout@", StringComparison.Ordinal));
        return ((YamlMappingNode)checkout["with"]).Children.ToDictionary(static pair => pair.Key.ToString(), static pair => pair.Value.ToString());
    }

    private static IEnumerable<string> Runs(YamlMappingNode job) =>
        Steps(job).Where(static step => step.Children.ContainsKey("id")).Select(static step => step["run"].ToString().Trim());

    private static IEnumerable<string> Images(YamlMappingNode job) =>
        ((YamlSequenceNode)((YamlMappingNode)((YamlMappingNode)job["strategy"])["matrix"])["os"]).Select(static image => image.ToString());

    private static string FailFast(YamlMappingNode job) => ((YamlMappingNode)job["strategy"])["fail-fast"].ToString();

    private const string HeadCommit = "${{ github.event.pull_request.head.sha }}";
}
