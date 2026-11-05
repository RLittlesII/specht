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
        ProcessStartInfo start = new SpechtRunFixture().WithRoot(tree.Root).WithTools(tools).WithArguments("--version");

        // When
        var (stdout, stderr, exitCode) = AcceptanceTool.Run(start);

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
        ProcessStartInfo start = new SpechtRunFixture().WithRoot(tree.Root).WithTools(tools).WithArguments("--version");

        // When
        var (_, _, exitCode) = AcceptanceTool.Run(start);

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
        ProcessStartInfo start = new SpechtRunFixture().WithRoot(tree.Root).WithTools(tools).WithShellCommand("specht --version");

        // When
        var (stdout, stderr, exitCode) = AcceptanceTool.Run(start);

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
        ProcessStartInfo start = new SpechtRunFixture().WithRoot(tree.Root).WithTools(tools).WithArguments("--json");

        // When
        var (violatingOut, violatingErr, violatingExit) = AcceptanceTool.Run(start);
        var violatingExpected = AcceptanceTool.FakeTools.JsonOutput(AcceptanceTool.FakeTools.Pinned, tree.Root);
        AcceptanceTool.FakeTools.FixViolation(tree.Root);
        var (editedOut, editedErr, editedExit) = AcceptanceTool.Run(start);

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
        ProcessStartInfo start = new SpechtRunFixture().WithRoot(above).WithTools(tools).WithArguments("--root", tree.Root, "--json");

        // When
        var (stdout, stderr, _) = AcceptanceTool.Run(start);

        // Then
        stdout.Should().Be(AcceptanceTool.FakeTools.JsonOutput(AcceptanceTool.FakeTools.Pinned, tree.Root), stderr);
    }

    [Fact(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    public void APinnedFake_WhenRunWithTheSchemaSourceOption_ShouldWriteTheStatedOutputNamingIt()
    {
        // Given
        using var tree = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture().WithPin(tree.Root);
        AcceptanceTool.FakeTools.WriteSpecifications(tree.Root);
        ProcessStartInfo start = new SpechtRunFixture().WithRoot(tree.Root).WithTools(tools).WithArguments("--json", "--schema-source", "disk");

        // When
        var (stdout, stderr, _) = AcceptanceTool.Run(start);

        // Then
        stdout.Should().Be(AcceptanceTool.FakeTools.JsonOutput(AcceptanceTool.FakeTools.Pinned, tree.Root, "disk"), stderr);
    }

    [Fact(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    public void APinnedFake_WhenRunWithTheStrictOption_ShouldWriteTheStatedOutputNamingIt()
    {
        // Given
        using var tree = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture().WithPin(tree.Root);
        AcceptanceTool.FakeTools.WriteSpecifications(tree.Root);
        ProcessStartInfo start = new SpechtRunFixture().WithRoot(tree.Root).WithTools(tools).WithArguments("--json", "--strict");

        // When
        var (stdout, stderr, _) = AcceptanceTool.Run(start);

        // Then
        stdout.Should().Be(AcceptanceTool.FakeTools.JsonOutput(AcceptanceTool.FakeTools.Pinned, tree.Root, strict: true), stderr);
    }

    [Fact(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    public void APinnedFake_WhenRunToExplainARuleItHas_ShouldWriteTheStatedOutputAndEndZero()
    {
        // Given
        using var tree = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture().WithPin(tree.Root);
        ProcessStartInfo start = new SpechtRunFixture().WithRoot(tree.Root).WithTools(tools).WithArguments("--explain", AcceptanceTool.FakeTools.KnownRule);

        // When
        var (stdout, stderr, exitCode) = AcceptanceTool.Run(start);

        // Then
        exitCode.Should().Be(0, stderr);
        stdout.Should().Be(AcceptanceTool.FakeTools.ExplainOutput(AcceptanceTool.FakeTools.Pinned, tree.Root));
    }

    [Fact(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    public void APinnedFake_WhenRunToListTheRulesAsJson_ShouldWriteTheStatedOutputAndEndZero()
    {
        // Given
        using var tree = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture().WithPin(tree.Root);
        ProcessStartInfo start = new SpechtRunFixture().WithRoot(tree.Root).WithTools(tools).WithArguments("rules", "--json");

        // When
        var (stdout, stderr, exitCode) = AcceptanceTool.Run(start);

        // Then
        exitCode.Should().Be(0, stderr);
        stdout.Should().Be(AcceptanceTool.FakeTools.RulesOutput(AcceptanceTool.FakeTools.Pinned, tree.Root));
    }

    [Fact(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    public void APinnedFake_WhenRunToPrintTheManifestAsJson_ShouldWriteTheStatedOutputAndEndZero()
    {
        // Given
        using var tree = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture().WithPin(tree.Root);
        ProcessStartInfo start = new SpechtRunFixture().WithRoot(tree.Root).WithTools(tools).WithArguments("manifest", "--json");

        // When
        var (stdout, stderr, exitCode) = AcceptanceTool.Run(start);

        // Then
        exitCode.Should().Be(0, stderr);
        stdout.Should().Be(AcceptanceTool.FakeTools.ManifestOutput(AcceptanceTool.FakeTools.Pinned, tree.Root));
    }

    [Fact(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    public void APinnedFake_WhenRunToPrintTheManifestWithDefaults_ShouldWriteTheShippedDefaultsAndEndZero()
    {
        // Given
        using var tree = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture().WithPin(tree.Root);
        ProcessStartInfo start = new SpechtRunFixture().WithRoot(tree.Root).WithTools(tools).WithArguments("manifest", "--defaults", "--json");

        // When
        var (stdout, stderr, exitCode) = AcceptanceTool.Run(start);

        // Then
        exitCode.Should().Be(0, stderr);
        stdout.Should().Be(AcceptanceTool.FakeTools.ManifestOutput(AcceptanceTool.FakeTools.Pinned, tree.Root, defaults: true));
    }

    [Theory(Skip = "The fake tools are shell scripts, which Windows does not run.", SkipWhen = nameof(IsWindows))]
    [ClassData(typeof(FailingFakeCommands))]
    public void APinnedFake_WhenRunOnARootItCannotServe_ShouldEndWithTheStatedCodeAndWriteTheStatedMessageOnStandardError(
        string[] command,
        string manifest,
        int expectedExit,
        string expectedMessage)
    {
        // Given
        using var tree = new SpechtTree();
        using AcceptanceTool.FakeTools tools = new FakeToolsFixture().WithPin(tree.Root);
        if (manifest == "removed")
        {
            File.Delete(Path.Combine(tree.Root, SpechtTree.ManifestPath));
        }

        if (manifest == "rejected")
        {
            tree.WriteRaw(SpechtTree.ManifestPath, AcceptanceTool.FakeTools.RejectedManifest);
        }

        ProcessStartInfo start = new SpechtRunFixture().WithRoot(tree.Root).WithTools(tools).WithArguments(command);

        // When
        var (stdout, stderr, exitCode) = AcceptanceTool.Run(start);

        // Then
        exitCode.Should().Be(expectedExit);
        stderr.Should().Be(expectedMessage);
        stdout.Should().BeEmpty();
    }
}
