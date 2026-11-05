using AwesomeAssertions;
using Specht.Acceptance;
using Specht.Tests.Shared;

namespace Specht.Tests.Mcp;

/// <summary>
/// Holds the fake tools the pinned-tool tests run against to behave as the real ones do, with no MCP in the way: the local tool
/// lookup finds the pinned fake and only it, and a restored copy that is removed leaves it unfound. These pass or fail on the
/// fixture alone, before the host runs a tool, and no claim of 0132-F3 rests on them; they are why a test that sees no fake run
/// can be believed.
/// </summary>
[Trait("Tier", "Integration")]
public sealed class McpHostFakeToolsIntegrationTests
{
    public static bool IsWindows => OperatingSystem.IsWindows();

    [Fact(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    public void ARestoredPinnedFake_WhenTheLocalToolLookupRunsIt_ShouldBeTheOnlyFakeToRunAndStateItsVersion()
    {
        // Given
        using var tree = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture()
            .WithPin(tree.Root)
            .WithGlobal()
            .WithOnPath();

        // When
        var (stdout, stderr, exitCode) = new SpechtRunFixture().WithRoot(tree.Root).WithTools(tools).WithArguments("--version");

        // Then
        exitCode.Should().Be(0, stderr);
        stdout.Trim().Should().Be(AcceptanceTool.FakeTools.PinnedVersion);
        tools.Runs.Should().Equal(new AcceptanceTool.FakeRun(AcceptanceTool.FakeTools.Pinned, AcceptanceTool.FakeTools.PinnedVersion, "--version"));
    }

    [Fact(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    public void APinnedFakeWhoseRestoredCopyIsRemoved_WhenTheLocalToolLookupRunsIt_ShouldFindNothingToRun()
    {
        // Given
        using var tree = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture()
            .WithPin(tree.Root)
            .WithGlobal()
            .WithUnrestored();

        // When
        var (_, _, exitCode) = new SpechtRunFixture().WithRoot(tree.Root).WithTools(tools).WithArguments("--version");

        // Then
        exitCode.Should().NotBe(0);
        tools.Runs.Should().BeEmpty();
        tools.AnythingRestored.Should().BeFalse();
    }

    [Fact(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    public void AGlobalFakeAndAPathFake_WhenSpechtIsRunFromThePath_ShouldRunThePathFakeFirstAndNoRealTool()
    {
        // Given
        using var tree = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture().WithGlobal().WithOnPath();

        // When
        var (stdout, stderr, exitCode) = new SpechtRunFixture().WithRoot(tree.Root).WithTools(tools).WithShellCommand("specht --version");

        // Then
        exitCode.Should().Be(0, stderr);
        stdout.Trim().Should().Be(AcceptanceTool.FakeTools.PathVersion);
        tools.Runs.Should().ContainSingle().Which.Label.Should().Be(AcceptanceTool.FakeTools.OnPath);
    }

    [Fact(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    public void APinnedFakeOnATreeWithAViolation_WhenRunWithJson_ShouldWriteTheStatedOutputAndExitOneUntilTheViolationIsFixed()
    {
        // Given
        using var tree = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture().WithPin(tree.Root);
        AcceptanceTool.FakeTools.WriteSpecifications(tree.Root);

        // When
        var (violatingOut, violatingErr, violatingExit) = new SpechtRunFixture().WithRoot(tree.Root).WithTools(tools).WithArguments("--json");
        var violatingExpected = AcceptanceTool.FakeTools.JsonOutput(AcceptanceTool.FakeTools.Pinned, tree.Root);
        AcceptanceTool.FakeTools.FixViolation(tree.Root);
        var (editedOut, editedErr, editedExit) = new SpechtRunFixture().WithRoot(tree.Root).WithTools(tools).WithArguments("--json");

        // Then
        violatingExit.Should().Be(1, violatingErr);
        violatingOut.Should().Be(violatingExpected).And.Contain(AcceptanceTool.FakeTools.Violation);
        editedExit.Should().Be(0, editedErr);
        editedOut.Should()
            .Be(AcceptanceTool.FakeTools.JsonOutput(AcceptanceTool.FakeTools.Pinned, tree.Root))
            .And.NotContain(AcceptanceTool.FakeTools.Violation);
    }

    [Fact(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    public void APinnedFakeRunFromTheDirectoryAboveARoot_WhenRunWithJsonAndThatRoot_ShouldWriteTheOutputForThatRoot()
    {
        // Given
        using var tree = new SpechtTree(["below"]);
        var above = Path.GetDirectoryName(tree.Root)!;
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture().WithPin(above);
        AcceptanceTool.FakeTools.WriteSpecifications(tree.Root);

        // When
        var (stdout, stderr, _) = new SpechtRunFixture().WithRoot(above).WithTools(tools).WithArguments("--root", tree.Root, "--json");

        // Then
        stdout.Should().Be(AcceptanceTool.FakeTools.JsonOutput(AcceptanceTool.FakeTools.Pinned, tree.Root), stderr);
    }
}
