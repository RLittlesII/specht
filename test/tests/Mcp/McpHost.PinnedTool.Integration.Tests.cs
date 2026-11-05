using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Specht.Acceptance;
using Specht.Tests.Shared;

namespace Specht.Tests.Mcp;

/// <summary>
/// Runs the built MCP as a process against fake <c>specht</c> tools and holds which tool a call to <c>check</c> runs, what the host
/// says when it can run none, and what it asks of the tool (0132-F3 B-017, B-018, B-020 to B-030, B-034). Every test reads the tool list
/// first and fails when the host offers no <c>check</c>, which <c>0330</c> registers, so none passes on the error a missing tool is.
/// </summary>
[Trait("Tier", "Integration")]
public sealed class McpHostPinnedToolIntegrationTests
{
    [Fact]
    public void ARepositoryThatPinsARestoredTool_WhenCheckIsCalled_ShouldRunThePinnedVersion()
    {
        // Given
        Assert.SkipWhen(OperatingSystem.IsWindows(), FakeToolsAreScripts);
        using var tree = new SpechtTree();
        using var tools = new AcceptanceTool.FakeTools();
        tools.Pin(tree.Root, AcceptanceTool.FakeTools.PinnedVersion, restored: true);

        // When
        var answers = Call(tree.Root, tools, calls: 1);

        // Then
        AcceptanceTool.McpFailure(answers.Single()).Should().BeNull();
        Work(tools).Should().NotBeEmpty().And.OnlyContain(static run =>
            run.Label == AcceptanceTool.FakeTools.Pinned && run.Version == AcceptanceTool.FakeTools.PinnedVersion);
    }

    [Fact]
    public void ARepositoryThatPinsARestoredTool_WhenOtherVersionsAreReachableFromThePath_ShouldRunNoneOfThem()
    {
        // Given
        Assert.SkipWhen(OperatingSystem.IsWindows(), FakeToolsAreScripts);
        using var tree = new SpechtTree();
        using var tools = new AcceptanceTool.FakeTools();
        tools.Pin(tree.Root, AcceptanceTool.FakeTools.PinnedVersion, restored: true);
        tools.InstallOnPath(AcceptanceTool.FakeTools.PathVersion);
        tools.InstallGlobal(AcceptanceTool.FakeTools.GlobalVersion);

        // When
        Call(tree.Root, tools, calls: 1);

        // Then
        tools.Runs.Should().NotBeEmpty().And.OnlyContain(static run => run.Label == AcceptanceTool.FakeTools.Pinned);
    }

    [Fact]
    public void ARootBelowTheDirectoryThatHoldsTheLocalToolManifest_WhenCheckIsCalled_ShouldRunTheVersionThatManifestPins()
    {
        // Given
        Assert.SkipWhen(OperatingSystem.IsWindows(), FakeToolsAreScripts);
        using var tree = new SpechtTree(["below"]);
        using var tools = new AcceptanceTool.FakeTools();
        tools.Pin(Path.GetDirectoryName(tree.Root)!, AcceptanceTool.FakeTools.PinnedVersion, restored: true);
        tools.InstallGlobal(AcceptanceTool.FakeTools.GlobalVersion);

        // When
        Call(tree.Root, tools, calls: 1);

        // Then
        Work(tools).Should().NotBeEmpty().And.OnlyContain(static run => run.Label == AcceptanceTool.FakeTools.Pinned);
    }

    [Fact]
    public void APinnedToolThatIsNotRestored_WhenCheckIsCalled_ShouldNotBeRestored()
    {
        // Given
        Assert.SkipWhen(OperatingSystem.IsWindows(), FakeToolsAreScripts);
        using var tree = new SpechtTree();
        using var tools = new AcceptanceTool.FakeTools();
        tools.Pin(tree.Root, AcceptanceTool.FakeTools.PinnedVersion, restored: false);
        var before = AcceptanceTool.Snapshot(tree.Root);

        // When
        Call(tree.Root, tools, calls: 1);

        // Then
        tools.AnythingRestored.Should().BeFalse();
        tools.Runs.Should().BeEmpty();
        AcceptanceTool.Snapshot(tree.Root).Should().BeEquivalentTo(before);
    }

    [Fact]
    public void APinnedToolThatIsNotRestored_WhenCheckIsCalled_ShouldFailWithAnErrorNamingTheToolAndTheVersion()
    {
        // Given
        Assert.SkipWhen(OperatingSystem.IsWindows(), FakeToolsAreScripts);
        using var tree = new SpechtTree();
        using var tools = new AcceptanceTool.FakeTools();
        tools.Pin(tree.Root, AcceptanceTool.FakeTools.PinnedVersion, restored: false);

        // When
        var answers = Call(tree.Root, tools, calls: 1);

        // Then
        AcceptanceTool.McpFailure(answers.Single()).Should().Contain("specht.tool").And.Contain(AcceptanceTool.FakeTools.PinnedVersion);
    }

    [Fact]
    public void APinnedToolThatIsNotRestored_WhenCheckIsCalled_ShouldFailWithAnErrorNamingWhereToReadHowToRestoreIt()
    {
        // Given
        Assert.SkipWhen(OperatingSystem.IsWindows(), FakeToolsAreScripts);
        using var tree = new SpechtTree();
        using var tools = new AcceptanceTool.FakeTools();
        tools.Pin(tree.Root, AcceptanceTool.FakeTools.PinnedVersion, restored: false);

        // When
        var answers = Call(tree.Root, tools, calls: 1);

        // Then
        AcceptanceTool.McpFailure(answers.Single()).Should().Contain("docs/mcp.md#restore-the-tool");
    }

    [Fact]
    public void APinnedToolThatIsNotRestored_WhenAGlobalToolAndAPathToolExist_ShouldRunNeitherOfThem()
    {
        // Given
        Assert.SkipWhen(OperatingSystem.IsWindows(), FakeToolsAreScripts);
        using var tree = new SpechtTree();
        using var tools = new AcceptanceTool.FakeTools();
        tools.Pin(tree.Root, AcceptanceTool.FakeTools.PinnedVersion, restored: false);
        tools.InstallGlobal(AcceptanceTool.FakeTools.GlobalVersion);
        tools.InstallOnPath(AcceptanceTool.FakeTools.PathVersion);

        // When
        Call(tree.Root, tools, calls: 1);

        // Then
        tools.Runs.Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(UnpinnedManifests))]
    public void ARootWithNoPin_WhenCheckIsCalledAndAGlobalToolIsInstalled_ShouldRunTheGlobalTool(bool manifestRemoved)
    {
        // Given
        Assert.SkipWhen(OperatingSystem.IsWindows(), FakeToolsAreScripts);
        using var tree = new SpechtTree();
        using var tools = new AcceptanceTool.FakeTools();
        tools.Pin(tree.Root, AcceptanceTool.FakeTools.PinnedVersion, restored: true);
        tools.Unpin(tree.Root, manifestRemoved);
        tools.InstallGlobal(AcceptanceTool.FakeTools.GlobalVersion);

        // When
        Call(tree.Root, tools, calls: 1);

        // Then
        Work(tools).Should().NotBeEmpty().And.OnlyContain(static run => run.Label == AcceptanceTool.FakeTools.Global);
    }

    [Fact]
    public void ARootWithNoPin_WhenCheckIsCalledAndAGlobalToolIsInstalled_ShouldReportTheAnswerAsUnpinnedApartFromTheCommandsOutput()
    {
        // Given
        Assert.SkipWhen(OperatingSystem.IsWindows(), FakeToolsAreScripts);
        using var tree = new SpechtTree();
        using var tools = new AcceptanceTool.FakeTools();
        tools.Unpin(tree.Root, removed: false);
        tools.InstallGlobal(AcceptanceTool.FakeTools.GlobalVersion);

        // When
        var answers = Call(tree.Root, tools, calls: 1);

        // Then
        var blocks = AcceptanceTool.McpTextBlocks(answers.Single());
        blocks.Where(static block => block.Contains(AcceptanceTool.FakeTools.OutputMarker, StringComparison.Ordinal)).Should().ContainSingle();
        UnpinnedReports(blocks).Should().ContainSingle();
    }

    [Fact]
    public void ARootWithNoPin_WhenCheckIsCalledAndAGlobalToolIsInstalled_ShouldNameWhereToReadHowToPinTheToolInTheReport()
    {
        // Given
        Assert.SkipWhen(OperatingSystem.IsWindows(), FakeToolsAreScripts);
        using var tree = new SpechtTree();
        using var tools = new AcceptanceTool.FakeTools();
        tools.Unpin(tree.Root, removed: false);
        tools.InstallGlobal(AcceptanceTool.FakeTools.GlobalVersion);

        // When
        var answers = Call(tree.Root, tools, calls: 1);

        // Then
        UnpinnedReports(AcceptanceTool.McpTextBlocks(answers.Single())).Should().ContainSingle().Which.Should().Contain("docs/mcp.md#pin-the-tool");
    }

    [Theory]
    [ClassData(typeof(PinnedOrGlobalTools))]
    public void ASessionOfTwoCalls_WhenCheckIsCalledTwice_ShouldCheckTheToolsVersionOnce(bool pinned)
    {
        // Given
        Assert.SkipWhen(OperatingSystem.IsWindows(), FakeToolsAreScripts);
        using var tree = new SpechtTree();
        using var tools = new AcceptanceTool.FakeTools();
        Install(tree, tools, pinned, AcceptanceTool.FakeTools.PinnedVersion, AcceptanceTool.FakeTools.GlobalVersion);

        // When
        var answers = Call(tree.Root, tools, calls: 2);

        // Then
        answers.Select(AcceptanceTool.McpFailure).Should().AllSatisfy(static failure => failure.Should().BeNull());
        Work(tools).Should().HaveCountGreaterThanOrEqualTo(2);
        tools.Runs.Count(static run => run.IsVersionCheck).Should().Be(1);
    }

    [Theory]
    [ClassData(typeof(PinnedOrGlobalTools))]
    public void AToolOlderThanTheMinimum_WhenCheckIsCalled_ShouldFailTheCall(bool pinned)
    {
        // Given
        Assert.SkipWhen(OperatingSystem.IsWindows(), FakeToolsAreScripts);
        using var tree = new SpechtTree();
        using var tools = new AcceptanceTool.FakeTools();
        Install(tree, tools, pinned, AcceptanceTool.FakeTools.OldVersion, AcceptanceTool.FakeTools.OldVersion);

        // When
        var answers = Call(tree.Root, tools, calls: 1);

        // Then
        AcceptanceTool.McpFailure(answers.Single()).Should().NotBeNull();
    }

    [Theory]
    [ClassData(typeof(PinnedOrGlobalTools))]
    public void AToolOlderThanTheMinimum_WhenCheckIsCalled_ShouldBeGivenNoWork(bool pinned)
    {
        // Given
        Assert.SkipWhen(OperatingSystem.IsWindows(), FakeToolsAreScripts);
        using var tree = new SpechtTree();
        using var tools = new AcceptanceTool.FakeTools();
        Install(tree, tools, pinned, AcceptanceTool.FakeTools.OldVersion, AcceptanceTool.FakeTools.OldVersion);

        // When
        Call(tree.Root, tools, calls: 1);

        // Then
        Work(tools).Should().BeEmpty();
    }

    [Theory]
    [ClassData(typeof(PinnedOrGlobalTools))]
    public void AToolOlderThanTheMinimum_WhenCheckIsCalled_ShouldFailWithAnErrorNamingTheMinimumVersion(bool pinned)
    {
        // Given
        Assert.SkipWhen(OperatingSystem.IsWindows(), FakeToolsAreScripts);
        using var tree = new SpechtTree();
        using var tools = new AcceptanceTool.FakeTools();
        Install(tree, tools, pinned, AcceptanceTool.FakeTools.OldVersion, AcceptanceTool.FakeTools.OldVersion);

        // When
        var answers = Call(tree.Root, tools, calls: 1);

        // Then
        var failure = AcceptanceTool.McpFailure(answers.Single());
        failure.Should().NotBeNull();
        Regex.Matches(failure!, @"\d+\.\d+\.\d+")
            .Select(static match => match.Value)
            .Should()
            .Contain(static version => version != AcceptanceTool.FakeTools.OldVersion);
    }

    [Theory]
    [ClassData(typeof(PinnedOrGlobalTools))]
    public void AToolOlderThanTheMinimum_WhenCheckIsCalled_ShouldFailWithAnErrorNamingWhereToReadHowToUpdateIt(bool pinned)
    {
        // Given
        Assert.SkipWhen(OperatingSystem.IsWindows(), FakeToolsAreScripts);
        using var tree = new SpechtTree();
        using var tools = new AcceptanceTool.FakeTools();
        Install(tree, tools, pinned, AcceptanceTool.FakeTools.OldVersion, AcceptanceTool.FakeTools.OldVersion);

        // When
        var answers = Call(tree.Root, tools, calls: 1);

        // Then
        AcceptanceTool.McpFailure(answers.Single()).Should().Contain("docs/mcp.md#update-the-tool");
    }

    [Fact]
    public void ARootWithNoPinAndNoGlobalTool_WhenCheckIsCalled_ShouldFailWithAnErrorNamingThePinAndTheGlobalToolAsNotFound()
    {
        // Given
        Assert.SkipWhen(OperatingSystem.IsWindows(), FakeToolsAreScripts);
        using var tree = new SpechtTree();
        using var tools = new AcceptanceTool.FakeTools();
        tools.Unpin(tree.Root, removed: false);

        // When
        var answers = Call(tree.Root, tools, calls: 1);

        // Then
        var failure = AcceptanceTool.McpFailure(answers.Single());
        failure.Should().NotBeNull();
        failure.Should().ContainEquivalentOf("pin").And.ContainEquivalentOf("global").And.ContainEquivalentOf("not found");
    }

    private static void Install(SpechtTree tree, AcceptanceTool.FakeTools tools, bool pinned, string pinnedVersion, string globalVersion)
    {
        if (pinned)
        {
            tools.Pin(tree.Root, pinnedVersion, restored: true);
        }
        else
        {
            tools.Unpin(tree.Root, removed: false);
            tools.InstallGlobal(globalVersion);
        }
    }

    private static IReadOnlyList<AcceptanceTool.FakeRun> Work(AcceptanceTool.FakeTools tools) =>
        tools.Runs.Where(static run => !run.IsVersionCheck).ToList();

    private static IReadOnlyList<string> UnpinnedReports(IReadOnlyList<string> blocks) =>
        blocks
            .Where(static block => !block.Contains(AcceptanceTool.FakeTools.OutputMarker, StringComparison.Ordinal))
            .Where(static block => block.Contains("unpinned", StringComparison.OrdinalIgnoreCase))
            .ToList();

    private static IReadOnlyList<JsonObject> Call(string root, AcceptanceTool.FakeTools tools, int calls)
    {
        using var session = AcceptanceTool.McpSession.Start(root, tools.Environment);
        var answers = Enumerable.Range(0, calls).Select(index => session.CallTool(10 + (index * 10), "check")).ToList();
        session.Close();

        return answers;
    }

    private const string FakeToolsAreScripts = "The fake tools are shell scripts, which Windows does not run.";
}
