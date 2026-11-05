using AwesomeAssertions;
using Specht.Mcp.Features.Host;
using Specht.Mcp.Features.PinnedTool;

namespace Specht.Tests.Mcp;

/// <summary>
/// Holds the decisions the host makes before it runs a command, over a scripted set of programs and no process: which tool runs, what
/// it refuses and says, the minimum version and the once-per-session check (0132-F3 B-009, B-017, B-018, B-020 to B-024, B-026 to
/// B-030, B-034).
/// </summary>
[Trait("Tier", "Unit")]
public sealed class SpechtToolUnitTests
{
    [Fact]
    public async Task ARestoredPin_WhenTheToolRuns_ShouldRunThePinnedToolInTheRootAndNoOther()
    {
        // Given
        const string root = "the-root-the-tool-runs-in";
        var process = new ScriptedProcessRun { ListedPin = ScriptedProcessRun.Pin, PinnedReports = ScriptedProcessRun.Pin, GlobalReports = "96.54.32" };
        SpechtTool tool = new SpechtToolFixture().WithProcess(process).WithRoot(root);

        // When
        var outcome = await tool.RunAsync(["--json"], CancellationToken.None);

        // Then
        outcome.Should().BeOfType<ToolOutcome.Ran>().Which.UnpinnedReport.Should().BeNull();
        process.Requests.Should().NotBeEmpty().And.OnlyContain(static request => request.FileName == "dotnet" && request.WorkingDirectory == root);
    }

    [Fact]
    public async Task APinThatIsNotRestored_WhenTheToolRuns_ShouldRefuseAndRestoreNothingAndRunNoOtherTool()
    {
        // Given
        var process = new ScriptedProcessRun { ListedPin = ScriptedProcessRun.Pin, GlobalReports = "96.54.32" };
        SpechtTool tool = new SpechtToolFixture().WithProcess(process);

        // When
        var outcome = await tool.RunAsync(["--json"], CancellationToken.None);

        // Then
        var message = outcome.Should().BeOfType<ToolOutcome.Refused>().Which.Message;
        message.Should().Contain(SpechtTool.PackageId).And.Contain(ScriptedProcessRun.Pin).And.Contain("docs/mcp.md#restore-the-tool");
        process.Requests.Should().OnlyContain(static request => request.FileName == "dotnet");
        process.Requests.SelectMany(static request => request.Arguments).Should().NotContain(["restore", "install", "update", "--json"]);
    }

    [Fact]
    public async Task NoPinAndAGlobalTool_WhenTheToolRuns_ShouldRunTheGlobalToolAndReportTheAnswerUnpinned()
    {
        // Given
        var process = new ScriptedProcessRun { GlobalReports = "96.54.32", PinnedReports = ScriptedProcessRun.Pin };
        SpechtTool tool = new SpechtToolFixture().WithProcess(process);

        // When
        var outcome = await tool.RunAsync(["--json"], CancellationToken.None);

        // Then
        var ran = outcome.Should().BeOfType<ToolOutcome.Ran>().Subject;
        ran.Result.StandardOutput.Should().Be(process.Verdict!.StandardOutput);
        ran.UnpinnedReport.Should().Contain("unpinned").And.Contain("docs/mcp.md#pin-the-tool");
        process.Requests.SelectMany(static request => request.Arguments).Should().NotContain("run");
    }

    [Fact]
    public async Task NoPinAndNoGlobalTool_WhenTheToolRuns_ShouldRefuseNamingThePinAndTheGlobalToolAsNotFound()
    {
        // Given
        var process = new ScriptedProcessRun();
        SpechtTool tool = new SpechtToolFixture().WithProcess(process);

        // When
        var outcome = await tool.RunAsync(["--json"], CancellationToken.None);

        // Then
        outcome.Should().BeOfType<ToolOutcome.Refused>().Which.Message
            .Should().ContainEquivalentOf("pin").And.ContainEquivalentOf("global").And.ContainEquivalentOf("not found");
    }

    [Theory]
    [ClassData(typeof(PinnedOrGlobalTools))]
    public async Task AToolAtTheMinimumVersion_WhenTheToolRuns_ShouldRunTheCommand(bool pinned)
    {
        // Given
        var process = ScriptedProcessRun.Tool(pinned, "0.1.90");
        SpechtTool tool = new SpechtToolFixture().WithProcess(process);

        // When
        var outcome = await tool.RunAsync(["--json"], CancellationToken.None);

        // Then
        outcome.Should().BeOfType<ToolOutcome.Ran>();
    }

    [Theory]
    [ClassData(typeof(PinnedOrGlobalTools))]
    public async Task AToolOneBelowTheMinimumVersion_WhenTheToolRuns_ShouldRefuseNamingTheMinimumAndGiveItNoWork(bool pinned)
    {
        // Given
        var process = ScriptedProcessRun.Tool(pinned, "0.1.89");
        SpechtTool tool = new SpechtToolFixture().WithProcess(process);

        // When
        var outcome = await tool.RunAsync(["--json"], CancellationToken.None);

        // Then
        outcome.Should().BeOfType<ToolOutcome.Refused>().Which.Message.Should().Contain("0.1.90").And.Contain("docs/mcp.md#update-the-tool");
        process.Requests.SelectMany(static request => request.Arguments).Should().NotContain("--json");
    }

    [Theory]
    [ClassData(typeof(PinnedOrGlobalTools))]
    public async Task AToolThatStatesNoVersion_WhenTheToolRuns_ShouldRefuseAndGiveItNoWork(bool pinned)
    {
        // Given
        var process = ScriptedProcessRun.Tool(pinned, "not a version");
        SpechtTool tool = new SpechtToolFixture().WithProcess(process);

        // When
        var outcome = await tool.RunAsync(["--json"], CancellationToken.None);

        // Then
        outcome.Should().BeOfType<ToolOutcome.Refused>().Which.Message.Should().Contain("docs/mcp.md#update-the-tool");
        process.Requests.SelectMany(static request => request.Arguments).Should().NotContain("--json");
    }

    [Theory]
    [ClassData(typeof(PinnedOrGlobalTools))]
    public async Task ASessionOfTwoRuns_WhenTheToolRunsTwice_ShouldAskTheToolForItsVersionOnce(bool pinned)
    {
        // Given
        var process = ScriptedProcessRun.Tool(pinned, "0.5.0-preview+abc");
        SpechtTool tool = new SpechtToolFixture().WithProcess(process);

        // When
        await tool.RunAsync(["--json"], CancellationToken.None);
        await tool.RunAsync(["--json"], CancellationToken.None);

        // Then
        process.VersionChecks.Should().ContainSingle();
        process.Requests.Count(static request => request.Arguments[^1] == "--json").Should().Be(2);
    }

    [Fact]
    public async Task AVersionCheckThatFailed_WhenTheToolRunsAgain_ShouldAskAgainAndRunOnceTheToolIsRestored()
    {
        // Given
        var process = new ScriptedProcessRun { ListedPin = ScriptedProcessRun.Pin };
        SpechtTool tool = new SpechtToolFixture().WithProcess(process);
        var first = await tool.RunAsync(["--json"], CancellationToken.None);
        process.PinnedReports = ScriptedProcessRun.Pin;

        // When
        var second = await tool.RunAsync(["--json"], CancellationToken.None);

        // Then
        first.Should().BeOfType<ToolOutcome.Refused>();
        second.Should().BeOfType<ToolOutcome.Ran>();
        process.VersionChecks.Should().HaveCount(2);
    }

    [Fact]
    public async Task ACommandThatCannotBeStarted_WhenTheToolRuns_ShouldRefuseInsteadOfAnswering()
    {
        // Given
        var process = new ScriptedProcessRun { GlobalReports = "96.54.32", Verdict = null };
        SpechtTool tool = new SpechtToolFixture().WithProcess(process);

        // When
        var outcome = await tool.RunAsync(["--json"], CancellationToken.None);

        // Then
        outcome.Should().BeOfType<ToolOutcome.Refused>().Which.Message.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task AListingThatEndsWithAFailureCode_WhenTheToolRuns_ShouldRefuseNamingTheCodeAndNotTheListingsTextAndRunNothingMore()
    {
        // Given
        var process = new ScriptedProcessRun
        {
            GlobalReports = "96.54.32",
            FailedListing = new ProcessResult(145, "listing stdout marker", "listing stderr marker"),
        };
        SpechtTool tool = new SpechtToolFixture().WithProcess(process);

        // When
        var outcome = await tool.RunAsync(["--json"], CancellationToken.None);

        // Then
        outcome.Should().BeOfType<ToolOutcome.Refused>().Which.Message
            .Should().Contain("145").And.NotContain("listing stderr marker").And.NotContain("listing stdout marker");
        process.Requests.Should().ContainSingle().Which.Arguments.Should().Equal("tool", "list");
    }

    [Fact]
    public async Task AListingThatCannotBeStarted_WhenAGlobalToolIsInstalled_ShouldReadNoPinAndRunTheGlobalToolUnpinned()
    {
        // Given
        var process = new ScriptedProcessRun { DotnetMissing = true, GlobalReports = "96.54.32" };
        SpechtTool tool = new SpechtToolFixture().WithProcess(process);

        // When
        var outcome = await tool.RunAsync(["--json"], CancellationToken.None);

        // Then
        outcome.Should().BeOfType<ToolOutcome.Ran>().Which.UnpinnedReport.Should().Contain("docs/mcp.md#pin-the-tool");
        process.Requests.Where(static request => request.FileName == "specht").Should().NotBeEmpty();
    }

    [Fact]
    public async Task AListingThatCannotBeStarted_WhenNoGlobalToolIsInstalled_ShouldRefuseNamingThePinAndTheGlobalToolAsNotFound()
    {
        // Given
        var process = new ScriptedProcessRun { DotnetMissing = true };
        SpechtTool tool = new SpechtToolFixture().WithProcess(process);

        // When
        var outcome = await tool.RunAsync(["--json"], CancellationToken.None);

        // Then
        outcome.Should().BeOfType<ToolOutcome.Refused>().Which.Message
            .Should().ContainEquivalentOf("pin").And.ContainEquivalentOf("global").And.ContainEquivalentOf("not found");
    }

    [Theory(Timeout = 30_000)]
    [ClassData(typeof(PinnedOrGlobalTools))]
    public async Task TwoFirstCallsInFlight_WhenTheVersionCheckIsHeldOpen_ShouldAskTheToolForItsVersionOnceAndLetBothRun(bool pinned)
    {
        // Given
        var process = ScriptedProcessRun.Tool(pinned, "0.5.0");
        process.VersionGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        SpechtTool tool = new SpechtToolFixture().WithProcess(process);

        // When
        var first = tool.RunAsync(["--json"], TestContext.Current.CancellationToken);
        var second = tool.RunAsync(["--json"], TestContext.Current.CancellationToken);
        process.VersionChecks.Should().ContainSingle();
        process.VersionGate.SetResult();
        var outcomes = await Task.WhenAll(first, second);

        // Then
        outcomes.Should().AllBeOfType<ToolOutcome.Ran>();
        process.VersionChecks.Should().ContainSingle();
        process.Requests.Count(static request => request.Arguments[^1] == "--json").Should().Be(2);
    }

    [Fact(Timeout = 30_000)]
    public async Task TwoFirstCallsInFlight_WhenTheFirstVersionCheckFails_ShouldAskAgainForTheSecond()
    {
        // Given
        var process = new ScriptedProcessRun { ListedPin = ScriptedProcessRun.Pin, VersionGate = new TaskCompletionSource() };
        SpechtTool tool = new SpechtToolFixture().WithProcess(process);

        // When
        var first = tool.RunAsync(["--json"], TestContext.Current.CancellationToken);
        var second = tool.RunAsync(["--json"], TestContext.Current.CancellationToken);
        process.VersionGate.SetResult();
        var outcomes = await Task.WhenAll(first, second);

        // Then
        outcomes.Should().AllBeOfType<ToolOutcome.Refused>();
        process.VersionChecks.Should().HaveCount(2);
    }

    [Fact(Timeout = 30_000)]
    public async Task TwoFirstCallsInFlight_WhenTheFirstCallerIsCancelled_ShouldLetTheSecondCheckTheVersionAndRun()
    {
        // Given
        var process = ScriptedProcessRun.Tool(pinned: true, "0.5.0");
        process.VersionGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        SpechtTool tool = new SpechtToolFixture().WithProcess(process);
        using var cancel = new CancellationTokenSource();
        var first = tool.RunAsync(["--json"], cancel.Token);
        var second = tool.RunAsync(["--json"], TestContext.Current.CancellationToken);

        // When
        await cancel.CancelAsync();
        var cancelled = await Record.ExceptionAsync(() => first);
        process.VersionGate.SetResult();
        var outcome = await second;

        // Then
        cancelled.Should().BeAssignableTo<OperationCanceledException>();
        outcome.Should().BeOfType<ToolOutcome.Ran>();
    }
}
