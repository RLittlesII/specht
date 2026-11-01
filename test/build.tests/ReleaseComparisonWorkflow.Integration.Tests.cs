using AwesomeAssertions;
using YamlDotNet.RepresentationModel;

namespace Specht.Build.Tests;

[Trait("Tier", "Integration")]
public sealed class ReleaseComparisonWorkflowIntegrationTests
{
    [Fact]
    public void ComparisonJob_WhenGenerated_ShouldNeedOnlyBuildAndRunOnlyForATagPush()
    {
        // Given
        var jobs = Jobs();

        // When
        var comparison = (YamlMappingNode)jobs["comparison"];

        // Then
        Scalars(comparison["needs"]).Should().Equal("build");
        comparison["if"].ToString().Should().Be("${{ github.event_name == 'push' && startsWith(github.ref, 'refs/tags/v') }}");
        Scalars(((YamlMappingNode)jobs["release"])["needs"]).Should().Equal("build");
    }

    [Fact]
    public void ComparisonJob_WhenGenerated_ShouldBeGrantedOnlyContentsReadAndActionsRead()
    {
        // Given
        var comparison = (YamlMappingNode)Jobs()["comparison"];

        // When
        var granted = Granted((YamlMappingNode)comparison["permissions"]);

        // Then
        granted.Should().BeEquivalentTo(new Dictionary<string, string>
        {
            ["actions"] = "read",
            ["contents"] = "read",
        });
        comparison.AllNodes.OfType<YamlScalarNode>().Select(static scalar => scalar.Value ?? string.Empty)
            .Should().NotContain(static value => value.Contains("secrets.", StringComparison.Ordinal));
    }

    [Fact]
    public void ComparisonJob_WhenGenerated_ShouldRunOnTheSelfHostedPool()
    {
        // Given
        var comparison = (YamlMappingNode)Jobs()["comparison"];

        // When
        var runner = Scalars(comparison["runs-on"]);

        // Then
        runner.Should().Equal("self-hosted", "macOS", "ARM64");
    }

    [Fact]
    public void ComparisonJob_WhenGenerated_ShouldCheckOutTheFullHistoryWithoutPersistedCredentials()
    {
        // Given
        var steps = Steps("comparison");

        // When
        var checkout = steps.Single(static step => Uses(step).StartsWith("actions/checkout@", StringComparison.Ordinal));

        // Then
        var with = (YamlMappingNode)checkout["with"];
        with["persist-credentials"].ToString().Should().Be("false");
        with["fetch-depth"].ToString().Should().Be("0");
    }

    [Fact]
    public void ComparisonJob_WhenGenerated_ShouldContinueOnErrorAtEveryStepAndAlwaysSummarise()
    {
        // Given
        var steps = Steps("comparison");

        // When
        var advisory = steps.Select(static step => Value(step, "continue-on-error")).ToArray();

        // Then
        advisory.Should().AllBe("true");
        Value(steps[^1], "if").Should().Contain("always()");
    }

    [Fact]
    public void ReleaseJob_WhenGenerated_ShouldKeepTheOnlyWriteGrantAndGainActionsRead()
    {
        // Given
        var workflow = Workflow();
        var jobs = Jobs();

        // When
        var release = Granted((YamlMappingNode)((YamlMappingNode)jobs["release"])["permissions"]);

        // Then
        release.Should().BeEquivalentTo(new Dictionary<string, string>
        {
            ["actions"] = "read",
            ["contents"] = "write",
            ["packages"] = "write",
        });
        Granted((YamlMappingNode)workflow["permissions"]).Values.Should().NotContain("write");
        jobs.Children.Where(static job => job.Key.ToString() != "release")
            .Select(static job => (YamlMappingNode)job.Value)
            .Where(static job => job.Children.ContainsKey("permissions"))
            .SelectMany(static job => Granted((YamlMappingNode)job["permissions"]).Values)
            .Should().NotContain("write");
    }

    [Fact]
    public void ReleaseJob_WhenGenerated_ShouldKeepPublicationFailingAndRunTheComparisonStepsAfterItAsAdvisory()
    {
        // Given
        var steps = Steps("release");

        // When
        var publication = Array.FindIndex(steps, static step => Value(step, "name") == "Create GitHub Release");
        var push = Array.FindIndex(steps, static step => Value(step, "name") == "Push release packages");

        // Then
        push.Should().BeGreaterThan(-1).And.BeLessThan(publication);
        foreach (var step in steps.Take(publication + 1))
        {
            step.Children.ContainsKey("continue-on-error").Should().BeFalse(Value(step, "name"));
            step.Children.ContainsKey("if").Should().BeFalse(Value(step, "name"));
        }

        steps.Skip(publication + 1).Should().NotBeEmpty();
        steps.Skip(publication + 1).Select(static step => Value(step, "continue-on-error")).Should().AllBe("true");
    }

    [Fact]
    public void ReleaseWorkflow_WhenGenerated_ShouldNotSignalTheComparisonThroughReleaseJobOutputs()
    {
        // Given
        var release = (YamlMappingNode)Jobs()["release"];

        // When
        var text = File.ReadAllText(WorkflowPath());

        // Then
        text.Should().NotContain("needs.release.outputs");
        release.Children.ContainsKey("outputs").Should().BeFalse();
        Jobs().Children.Keys.Select(static key => key.ToString()).Should().BeEquivalentTo("build", "release", "comparison");
    }

    [Fact]
    public void BuildJob_WhenGenerated_ShouldPublishTheComparatorApartFromThePackageForBothLaterJobs()
    {
        // Given
        var uploads = Steps("build").Where(static step => Uses(step).StartsWith("actions/upload-artifact@", StringComparison.Ordinal))
            .Select(static step => ((YamlMappingNode)step["with"])["name"].ToString()).ToArray();

        // When
        var comparator = uploads.Except(["nupkg"]).ToArray();

        // Then
        comparator.Should().ContainSingle();
        foreach (var job in new[] { "release", "comparison" })
        {
            Steps(job).Where(static step => Uses(step).StartsWith("actions/download-artifact@", StringComparison.Ordinal))
                .Select(static step => ((YamlMappingNode)step["with"])["name"].ToString())
                .Should().Contain(comparator[0], job);
        }
    }

    private static string WorkflowPath() => Path.Combine(FindRepositoryRoot(), ".github", "workflows", "release.yml");

    private static YamlMappingNode Workflow()
    {
        var stream = new YamlStream();
        using var input = File.OpenText(WorkflowPath());
        stream.Load(input);
        return (YamlMappingNode)stream.Documents[0].RootNode;
    }

    private static YamlMappingNode Jobs() => (YamlMappingNode)Workflow()["jobs"];

    private static YamlMappingNode[] Steps(string job) =>
        ((YamlSequenceNode)((YamlMappingNode)Jobs()[job])["steps"]).Cast<YamlMappingNode>().ToArray();

    private static Dictionary<string, string> Granted(YamlMappingNode permissions) => permissions.Children
        .Where(static permission => permission.Value.ToString() != "none")
        .ToDictionary(static permission => permission.Key.ToString(), static permission => permission.Value.ToString());

    private static string Uses(YamlMappingNode step) => Value(step, "uses");

    private static string Value(YamlMappingNode step, string key) =>
        step.Children.TryGetValue(new YamlScalarNode(key), out var value) ? value.ToString() : string.Empty;

    private static string[] Scalars(YamlNode node) => ((YamlSequenceNode)node).Select(static item => item.ToString()).ToArray();

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "specht.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Could not locate the repository root.");
    }
}
