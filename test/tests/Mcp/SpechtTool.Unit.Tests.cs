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
        var process = new ScriptedProcessRun { ListedPin = ScriptedProcessRun.Pin, PinnedReports = ScriptedProcessRun.Pin, GlobalReports = "96.54.32" };

        // When
        var outcome = await Tool(process).RunAsync(Json, CancellationToken.None);

        // Then
        outcome.Should().BeOfType<ToolOutcome.Ran>().Which.UnpinnedReport.Should().BeNull();
        process.Requests.Should().NotBeEmpty().And.OnlyContain(static request => request.FileName == "dotnet" && request.WorkingDirectory == Root);
    }

    [Fact]
    public async Task APinThatIsNotRestored_WhenTheToolRuns_ShouldRefuseAndRestoreNothingAndRunNoOtherTool()
    {
        // Given
        var process = new ScriptedProcessRun { ListedPin = ScriptedProcessRun.Pin, GlobalReports = "96.54.32" };

        // When
        var outcome = await Tool(process).RunAsync(Json, CancellationToken.None);

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

        // When
        var outcome = await Tool(process).RunAsync(Json, CancellationToken.None);

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

        // When
        var outcome = await Tool(process).RunAsync(Json, CancellationToken.None);

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

        // When
        var outcome = await Tool(process).RunAsync(Json, CancellationToken.None);

        // Then
        outcome.Should().BeOfType<ToolOutcome.Ran>();
    }

    [Theory]
    [ClassData(typeof(PinnedOrGlobalTools))]
    public async Task AToolOneBelowTheMinimumVersion_WhenTheToolRuns_ShouldRefuseNamingTheMinimumAndGiveItNoWork(bool pinned)
    {
        // Given
        var process = ScriptedProcessRun.Tool(pinned, "0.1.89");

        // When
        var outcome = await Tool(process).RunAsync(Json, CancellationToken.None);

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

        // When
        var outcome = await Tool(process).RunAsync(Json, CancellationToken.None);

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
        var tool = Tool(process);

        // When
        await tool.RunAsync(Json, CancellationToken.None);
        await tool.RunAsync(Json, CancellationToken.None);

        // Then
        process.VersionChecks.Should().ContainSingle();
        process.Requests.Count(static request => request.Arguments[^1] == "--json").Should().Be(2);
    }

    [Fact]
    public async Task AVersionCheckThatFailed_WhenTheToolRunsAgain_ShouldAskAgainAndRunOnceTheToolIsRestored()
    {
        // Given
        var process = new ScriptedProcessRun { ListedPin = ScriptedProcessRun.Pin };
        var tool = Tool(process);
        var first = await tool.RunAsync(Json, CancellationToken.None);
        process.PinnedReports = ScriptedProcessRun.Pin;

        // When
        var second = await tool.RunAsync(Json, CancellationToken.None);

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

        // When
        var outcome = await Tool(process).RunAsync(Json, CancellationToken.None);

        // Then
        outcome.Should().BeOfType<ToolOutcome.Refused>().Which.Message.Should().NotBeNullOrWhiteSpace();
    }

    private static SpechtTool Tool(ScriptedProcessRun process) => new(process, new SessionRoot { Value = Root });

    private const string Root = "repository";

    private static readonly string[] Json = ["--json"];
}
