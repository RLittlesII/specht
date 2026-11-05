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
    public static bool IsWindows => OperatingSystem.IsWindows();

    [Fact(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    public void ARepositoryThatPinsARestoredTool_WhenCheckIsCalled_ShouldRunThePinnedVersion()
    {
        // Given
        using var tree = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture().WithPin(tree.Root);
        McpSessionStart start = new McpSessionFixture().WithRoot(tree.Root).WithTools(tools);

        // When
        using var session = AcceptanceTool.McpSession.Start(start.Root, start.Environment, start.Arguments);
        var answer = session.CallTool(10, "check");
        session.Close();

        // Then
        AcceptanceTool.McpFailure(answer).Should().BeNull();
        tools.Runs.Where(static run => !run.IsVersionCheck).Should().NotBeEmpty().And.OnlyContain(static run =>
            run.Label == AcceptanceTool.FakeTools.Pinned && run.Version == AcceptanceTool.FakeTools.PinnedVersion);
    }

    [Fact(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    public void ARepositoryThatPinsARestoredTool_WhenOtherVersionsAreReachableFromThePath_ShouldRunNoneOfThem()
    {
        // Given
        using var tree = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture()
            .WithPin(tree.Root)
            .WithOnPath()
            .WithGlobal();
        McpSessionStart start = new McpSessionFixture().WithRoot(tree.Root).WithTools(tools);

        // When
        using var session = AcceptanceTool.McpSession.Start(start.Root, start.Environment, start.Arguments);
        session.CallTool(10, "check");
        session.Close();

        // Then
        tools.Runs.Should().NotBeEmpty().And.OnlyContain(static run => run.Label == AcceptanceTool.FakeTools.Pinned);
    }

    [Fact(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    public void ARootBelowTheDirectoryThatHoldsTheLocalToolManifest_WhenCheckIsCalled_ShouldRunTheVersionThatManifestPins()
    {
        // Given
        using var tree = new SpechtTree(["below"]);
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture().WithPin(Path.GetDirectoryName(tree.Root)!).WithGlobal();
        McpSessionStart start = new McpSessionFixture().WithRoot(tree.Root).WithTools(tools);

        // When
        using var session = AcceptanceTool.McpSession.Start(start.Root, start.Environment, start.Arguments);
        session.CallTool(10, "check");
        session.Close();

        // Then
        tools.Runs.Where(static run => !run.IsVersionCheck).Should().NotBeEmpty().And.OnlyContain(static run => run.Label == AcceptanceTool.FakeTools.Pinned);
    }

    [Fact(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    public void APinnedToolThatIsNotRestored_WhenCheckIsCalled_ShouldNotBeRestored()
    {
        // Given
        using var tree = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture().WithPin(tree.Root, restored: false);
        var before = AcceptanceTool.Snapshot(tree.Root);
        McpSessionStart start = new McpSessionFixture().WithRoot(tree.Root).WithTools(tools);

        // When
        using var session = AcceptanceTool.McpSession.Start(start.Root, start.Environment, start.Arguments);
        session.CallTool(10, "check");
        session.Close();

        // Then
        tools.AnythingRestored.Should().BeFalse();
        tools.Runs.Should().BeEmpty();
        AcceptanceTool.Snapshot(tree.Root).Should().BeEquivalentTo(before);
    }

    [Fact(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    public void APinnedToolThatIsNotRestored_WhenCheckIsCalled_ShouldFailWithAnErrorNamingTheToolAndTheVersion()
    {
        // Given
        using var tree = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture().WithPin(tree.Root, restored: false);
        McpSessionStart start = new McpSessionFixture().WithRoot(tree.Root).WithTools(tools);

        // When
        using var session = AcceptanceTool.McpSession.Start(start.Root, start.Environment, start.Arguments);
        var answer = session.CallTool(10, "check");
        session.Close();

        // Then
        AcceptanceTool.McpFailure(answer).Should().Contain("specht.tool").And.Contain(AcceptanceTool.FakeTools.PinnedVersion);
    }

    [Fact(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    public void APinnedToolThatIsNotRestored_WhenCheckIsCalled_ShouldFailWithAnErrorNamingWhereToReadHowToRestoreIt()
    {
        // Given
        using var tree = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture().WithPin(tree.Root, restored: false);
        McpSessionStart start = new McpSessionFixture().WithRoot(tree.Root).WithTools(tools);

        // When
        using var session = AcceptanceTool.McpSession.Start(start.Root, start.Environment, start.Arguments);
        var answer = session.CallTool(10, "check");
        session.Close();

        // Then
        AcceptanceTool.McpFailure(answer).Should().Contain("docs/mcp.md#restore-the-tool");
    }

    [Fact(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    public void APinnedToolThatIsNotRestored_WhenAGlobalToolAndAPathToolExist_ShouldRunNeitherOfThem()
    {
        // Given
        using var tree = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture()
            .WithPin(tree.Root, restored: false)
            .WithGlobal()
            .WithOnPath();
        McpSessionStart start = new McpSessionFixture().WithRoot(tree.Root).WithTools(tools);

        // When
        using var session = AcceptanceTool.McpSession.Start(start.Root, start.Environment, start.Arguments);
        session.CallTool(10, "check");
        session.Close();

        // Then
        tools.Runs.Should().BeEmpty();
    }

    [Theory(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    [ClassData(typeof(UnpinnedManifests))]
    public void ARootWithNoPin_WhenCheckIsCalledAndAGlobalToolIsInstalled_ShouldRunTheGlobalTool(bool manifestRemoved)
    {
        // Given
        using var tree = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture()
            .WithPin(tree.Root)
            .WithUnpinned(tree.Root, manifestRemoved)
            .WithGlobal();
        McpSessionStart start = new McpSessionFixture().WithRoot(tree.Root).WithTools(tools);

        // When
        using var session = AcceptanceTool.McpSession.Start(start.Root, start.Environment, start.Arguments);
        session.CallTool(10, "check");
        session.Close();

        // Then
        tools.Runs.Where(static run => !run.IsVersionCheck).Should().NotBeEmpty().And.OnlyContain(static run => run.Label == AcceptanceTool.FakeTools.Global);
    }

    [Fact(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    public void ARootWithNoPin_WhenCheckIsCalledAndAGlobalToolIsInstalled_ShouldReportTheAnswerAsUnpinnedApartFromTheCommandsOutput()
    {
        // Given
        using var tree = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture().WithUnpinned(tree.Root).WithGlobal();
        McpSessionStart start = new McpSessionFixture().WithRoot(tree.Root).WithTools(tools);

        // When
        using var session = AcceptanceTool.McpSession.Start(start.Root, start.Environment, start.Arguments);
        var answer = session.CallTool(10, "check");
        session.Close();

        // Then
        var blocks = AcceptanceTool.McpTextBlocks(answer);
        blocks.Where(static block => block.Contains(AcceptanceTool.FakeTools.OutputMarker, StringComparison.Ordinal)).Should().ContainSingle();
        blocks
            .Where(static block => !block.Contains(AcceptanceTool.FakeTools.OutputMarker, StringComparison.Ordinal))
            .Where(static block => block.Contains("unpinned", StringComparison.OrdinalIgnoreCase))
            .Should()
            .ContainSingle();
    }

    [Fact(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    public void ARootWithNoPin_WhenCheckIsCalledAndAGlobalToolIsInstalled_ShouldNameWhereToReadHowToPinTheToolInTheReport()
    {
        // Given
        using var tree = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture().WithUnpinned(tree.Root).WithGlobal();
        McpSessionStart start = new McpSessionFixture().WithRoot(tree.Root).WithTools(tools);

        // When
        using var session = AcceptanceTool.McpSession.Start(start.Root, start.Environment, start.Arguments);
        var answer = session.CallTool(10, "check");
        session.Close();

        // Then
        AcceptanceTool.McpTextBlocks(answer)
            .Where(static block => !block.Contains(AcceptanceTool.FakeTools.OutputMarker, StringComparison.Ordinal))
            .Where(static block => block.Contains("unpinned", StringComparison.OrdinalIgnoreCase))
            .Should()
            .ContainSingle()
            .Which.Should()
            .Contain("docs/mcp.md#pin-the-tool");
    }

    [Theory(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    [ClassData(typeof(PinnedOrGlobalTools))]
    public void ASessionOfTwoCalls_WhenCheckIsCalledTwice_ShouldCheckTheToolsVersionOnce(bool pinned)
    {
        // Given
        using var tree = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture().WithPinnedOrGlobal(pinned, tree.Root);
        McpSessionStart start = new McpSessionFixture().WithRoot(tree.Root).WithTools(tools);

        // When
        using var session = AcceptanceTool.McpSession.Start(start.Root, start.Environment, start.Arguments);
        var answers = new[] { session.CallTool(10, "check"), session.CallTool(20, "check") };
        session.Close();

        // Then
        answers.Select(AcceptanceTool.McpFailure).Should().AllSatisfy(static failure => failure.Should().BeNull());
        tools.Runs.Where(static run => !run.IsVersionCheck).Should().HaveCountGreaterThanOrEqualTo(2);
        tools.Runs.Count(static run => run.IsVersionCheck).Should().Be(1);
    }

    [Theory(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    [ClassData(typeof(PinnedOrGlobalTools))]
    public void AToolOlderThanTheMinimum_WhenCheckIsCalled_ShouldFailTheCall(bool pinned)
    {
        // Given
        using var tree = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture().WithPinnedOrGlobal(pinned, tree.Root, AcceptanceTool.FakeTools.OldVersion);
        McpSessionStart start = new McpSessionFixture().WithRoot(tree.Root).WithTools(tools);

        // When
        using var session = AcceptanceTool.McpSession.Start(start.Root, start.Environment, start.Arguments);
        var answer = session.CallTool(10, "check");
        session.Close();

        // Then
        AcceptanceTool.McpFailure(answer).Should().NotBeNull();
    }

    [Theory(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    [ClassData(typeof(PinnedOrGlobalTools))]
    public void AToolOlderThanTheMinimum_WhenCheckIsCalled_ShouldBeGivenNoWork(bool pinned)
    {
        // Given
        using var tree = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture().WithPinnedOrGlobal(pinned, tree.Root, AcceptanceTool.FakeTools.OldVersion);
        McpSessionStart start = new McpSessionFixture().WithRoot(tree.Root).WithTools(tools);

        // When
        using var session = AcceptanceTool.McpSession.Start(start.Root, start.Environment, start.Arguments);
        session.CallTool(10, "check");
        session.Close();

        // Then
        tools.Runs.Where(static run => !run.IsVersionCheck).Should().BeEmpty();
    }

    [Theory(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    [ClassData(typeof(PinnedOrGlobalTools))]
    public void AToolOlderThanTheMinimum_WhenCheckIsCalled_ShouldFailWithAnErrorNamingTheMinimumVersion(bool pinned)
    {
        // Given
        using var tree = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture().WithPinnedOrGlobal(pinned, tree.Root, AcceptanceTool.FakeTools.OldVersion);
        McpSessionStart start = new McpSessionFixture().WithRoot(tree.Root).WithTools(tools);

        // When
        using var session = AcceptanceTool.McpSession.Start(start.Root, start.Environment, start.Arguments);
        var answer = session.CallTool(10, "check");
        session.Close();

        // Then
        var failure = AcceptanceTool.McpFailure(answer);
        failure.Should().NotBeNull();
        Regex.Matches(failure!, @"\d+\.\d+\.\d+")
            .Select(static match => match.Value)
            .Should()
            .Contain(static version => version != AcceptanceTool.FakeTools.OldVersion);
    }

    [Theory(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    [ClassData(typeof(PinnedOrGlobalTools))]
    public void AToolOlderThanTheMinimum_WhenCheckIsCalled_ShouldFailWithAnErrorNamingWhereToReadHowToUpdateIt(bool pinned)
    {
        // Given
        using var tree = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture().WithPinnedOrGlobal(pinned, tree.Root, AcceptanceTool.FakeTools.OldVersion);
        McpSessionStart start = new McpSessionFixture().WithRoot(tree.Root).WithTools(tools);

        // When
        using var session = AcceptanceTool.McpSession.Start(start.Root, start.Environment, start.Arguments);
        var answer = session.CallTool(10, "check");
        session.Close();

        // Then
        AcceptanceTool.McpFailure(answer).Should().Contain("docs/mcp.md#update-the-tool");
    }

    [Fact(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    public void ARootWithNoPinAndNoGlobalTool_WhenCheckIsCalled_ShouldFailWithAnErrorNamingThePinAndTheGlobalToolAsNotFound()
    {
        // Given
        using var tree = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture().WithUnpinned(tree.Root);
        McpSessionStart start = new McpSessionFixture().WithRoot(tree.Root).WithTools(tools);

        // When
        using var session = AcceptanceTool.McpSession.Start(start.Root, start.Environment, start.Arguments);
        var answer = session.CallTool(10, "check");
        session.Close();

        // Then
        var failure = AcceptanceTool.McpFailure(answer);
        failure.Should().NotBeNull();
        failure.Should().ContainEquivalentOf("pin").And.ContainEquivalentOf("global").And.ContainEquivalentOf("not found");
    }
}
