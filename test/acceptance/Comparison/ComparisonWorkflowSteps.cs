using AwesomeAssertions;
using Reqnroll;
using YamlDotNet.RepresentationModel;

namespace Specht.Acceptance.Comparison;

[Binding]
[Scope(Feature = "Release baseline comparison")]
public sealed class ComparisonWorkflowSteps
{
    [Given("the generated release workflow")]
    public void GivenTheGeneratedReleaseWorkflow()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "specht.slnx")))
        {
            directory = directory.Parent;
        }

        directory.Should().NotBeNull();
        var stream = new YamlStream();
        using var reader = File.OpenText(Path.Combine(directory!.FullName, ".github", "workflows", "release.yml"));
        stream.Load(reader);
        _jobs = (YamlMappingNode) ((YamlMappingNode) stream.Documents[0].RootNode)["jobs"];
    }

    [When("its {string} comparator steps are inspected")]
    public void WhenTheComparatorStepsAreInspected(string job) =>
        _steps = ((YamlSequenceNode) ((YamlMappingNode) _jobs![job])["steps"]).Cast<YamlMappingNode>().ToArray();

    [Then("the job builds the comparator directly only after a successful checkout and before invoking it")]
    public void ThenTheJobBuildsFromSource()
    {
        var build = Array.FindIndex(_steps, static step => Value(step, "id") == "comparatorBuild");
        build.Should().BeGreaterThan(-1);
        Value(_steps[build], "if").Should().Be("${{ steps.checkout.outcome == 'success' }}");
        Value(_steps.Single(static step => Value(step, "id") == "checkout"), "uses").Should().StartWith("actions/checkout@");
        Value(_steps[build], "run").Trim().Should().Be(
            "dotnet publish .performance/comparison/comparison.csproj -c Release -f net10.0 -o .artifacts/comparator");
        _steps.Take(build).Should().Contain(static step => Value(step, "uses").StartsWith("actions/checkout@", StringComparison.Ordinal));
        Array.FindIndex(_steps, static step => Value(step, "run").StartsWith("dotnet .artifacts/comparator/specht.comparison.dll", StringComparison.Ordinal))
            .Should().BeGreaterThan(build);
    }

    [Then("comparator build failures leave invocation skipped and an advisory incomplete summary")]
    public void ThenBuildFailuresAreAdvisory()
    {
        Value(_steps.Single(static step => Value(step, "id") == "comparatorBuild"), "continue-on-error").Should().Be("true");
        var invocations = _steps.Where(
            static step => Value(step, "run").StartsWith("dotnet .artifacts/comparator/specht.comparison.dll", StringComparison.Ordinal)).ToArray();
        invocations.Should().NotBeEmpty();
        invocations.Select(static step => Value(step, "if")).Should().AllBe("${{ steps.comparatorBuild.outcome == 'success' }}");
        Value(_steps[^1], "if").Should().Contain("outcome != 'success'");
        Value(_steps[^1], "run").Should().Contain("Incomplete").And.Contain("Re-run all jobs");
        Value(_steps[^1], "continue-on-error").Should().Be("true");
    }

    [Then("the workflow transfers no comparator binary artifact")]
    public void ThenNoComparatorBinaryArtifact()
    {
        var transfers = _jobs!.Children.Values.Cast<YamlMappingNode>()
            .SelectMany(static job => ((YamlSequenceNode) job["steps"]).Cast<YamlMappingNode>())
            .Where(static step => Value(step, "uses").StartsWith("actions/upload-artifact@", StringComparison.Ordinal)
                || Value(step, "uses").StartsWith("actions/download-artifact@", StringComparison.Ordinal));
        transfers.Select(static step => ((YamlMappingNode) step["with"])["name"].ToString()).Should().NotContain("comparator");
    }

    private static string Value(YamlMappingNode node, string key) =>
        node.Children.TryGetValue(key, out var value) ? value.ToString() : string.Empty;

    private YamlMappingNode? _jobs;
    private YamlMappingNode[] _steps = [];
}
