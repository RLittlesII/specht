using System.Diagnostics;
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
    [Fact]
    public void ARestoredPinnedFake_WhenTheLocalToolLookupRunsIt_ShouldBeTheOnlyFakeToRunAndStateItsVersion()
    {
        // Given
        Assert.SkipWhen(OperatingSystem.IsWindows(), "The fake tools are shell scripts, which Windows does not run.");
        using var tree = new SpechtTree();
        using var tools = new AcceptanceTool.FakeTools();
        tools.Pin(tree.Root, AcceptanceTool.FakeTools.PinnedVersion, restored: true);
        tools.InstallGlobal(AcceptanceTool.FakeTools.GlobalVersion);
        tools.InstallOnPath(AcceptanceTool.FakeTools.PathVersion);

        // When
        var (stdout, stderr, exitCode) = Specht(tree.Root, tools, "--version");

        // Then
        exitCode.Should().Be(0, stderr);
        stdout.Trim().Should().Be(AcceptanceTool.FakeTools.PinnedVersion);
        tools.Runs.Should().Equal(new AcceptanceTool.FakeRun(AcceptanceTool.FakeTools.Pinned, AcceptanceTool.FakeTools.PinnedVersion, "--version"));
    }

    [Fact]
    public void APinnedFakeWhoseRestoredCopyIsRemoved_WhenTheLocalToolLookupRunsIt_ShouldFindNothingToRun()
    {
        // Given
        Assert.SkipWhen(OperatingSystem.IsWindows(), "The fake tools are shell scripts, which Windows does not run.");
        using var tree = new SpechtTree();
        using var tools = new AcceptanceTool.FakeTools();
        tools.Pin(tree.Root, AcceptanceTool.FakeTools.PinnedVersion, restored: true);
        tools.InstallGlobal(AcceptanceTool.FakeTools.GlobalVersion);
        tools.Unrestore();

        // When
        var (_, _, exitCode) = Specht(tree.Root, tools, "--version");

        // Then
        exitCode.Should().NotBe(0);
        tools.Runs.Should().BeEmpty();
        tools.AnythingRestored.Should().BeFalse();
    }

    [Fact]
    public void AGlobalFakeAndAPathFake_WhenSpechtIsRunFromThePath_ShouldRunThePathFakeFirstAndNoRealTool()
    {
        // Given
        Assert.SkipWhen(OperatingSystem.IsWindows(), "The fake tools are shell scripts, which Windows does not run.");
        using var tree = new SpechtTree();
        using var tools = new AcceptanceTool.FakeTools();
        tools.InstallGlobal(AcceptanceTool.FakeTools.GlobalVersion);
        tools.InstallOnPath(AcceptanceTool.FakeTools.PathVersion);

        // When
        var (stdout, stderr, exitCode) = Path(tree.Root, tools);

        // Then
        exitCode.Should().Be(0, stderr);
        stdout.Trim().Should().Be(AcceptanceTool.FakeTools.PathVersion);
        tools.Runs.Should().ContainSingle().Which.Label.Should().Be(AcceptanceTool.FakeTools.OnPath);
    }

    private static (string Stdout, string Stderr, int ExitCode) Specht(string root, AcceptanceTool.FakeTools tools, params string[] args) =>
        Start(new ProcessStartInfo("dotnet"), root, tools, ["specht", .. args]);

    private static (string Stdout, string Stderr, int ExitCode) Path(string root, AcceptanceTool.FakeTools tools) =>
        Start(new ProcessStartInfo("sh"), root, tools, ["-c", "specht --version"]);

    private static (string Stdout, string Stderr, int ExitCode) Start(ProcessStartInfo start, string root, AcceptanceTool.FakeTools tools, string[] args)
    {
        start.WorkingDirectory = root;
        foreach (var (name, value) in tools.Environment)
        {
            start.Environment[name] = value;
        }

        foreach (var arg in args)
        {
            start.ArgumentList.Add(arg);
        }

        return AcceptanceTool.Run(start);
    }
}
