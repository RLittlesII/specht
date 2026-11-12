using AwesomeAssertions;
using ModelContextProtocol.Protocol;
using Specht.Mcp.Features.PinnedTool;

namespace Specht.Tests.Mcp;

/// <summary>
/// Holds the tool <c>mint</c> over a scripted set of programs and no process: the answer is what the run its call makes wrote, and the
/// command that run is given, with and without a Feature (0132-F4 B-023, B-025, B-026, and B-022 for its arguments). The tool is the one
/// the host registers, found by name, so a host that registers no <c>mint</c> fails here on its absence and not on a missing type.
/// </summary>
[Trait("Tier", "Unit")]
public sealed class MintToolUnitTests
{
    [Fact]
    public async Task ACallForANewId_WhenMintIsCalled_ShouldAnswerWithWhatItsOneRunWroteOnStandardOutputUnchangedAsOnePart()
    {
        // Given
        var process = ScriptedProcessRun.Tool(pinned: true, "0.6.1");
        process.Verdict = new ProcessResult(0, "0434\n", "ignored");
        using RegisteredTools tools = new RegisteredToolsFixture().WithProcess(process);

        // When
        var result = await tools.CallAsync("mint", "type", "adr");

        // Then
        result.IsError.Should().NotBe(true);
        Texts(result).Should().Equal("0434\n");
        MintRuns(process).Should().ContainSingle();
    }

    [Fact]
    public async Task TwoCallsForANewId_WhenMintIsCalledTwice_ShouldAnswerEachWithWhatTheRunThatCallMadeWrote()
    {
        // Given
        var process = ScriptedProcessRun.Tool(pinned: true, "0.6.1");
        process.Verdict = new ProcessResult(0, "0434\n", string.Empty);
        using RegisteredTools tools = new RegisteredToolsFixture().WithProcess(process);
        var first = await tools.CallAsync("mint", "type", "adr");
        process.Verdict = new ProcessResult(0, "0435\n", string.Empty);

        // When
        var second = await tools.CallAsync("mint", "type", "adr");

        // Then
        Texts(first).Should().Equal("0434\n");
        Texts(second).Should().Equal("0435\n");
        MintRuns(process).Should().HaveCount(2);
    }

    [Fact]
    public async Task ACallNamingAFeature_WhenMintIsCalled_ShouldRunTheMintCommandWithFeatureFollowedByThatValue()
    {
        // Given
        var process = ScriptedProcessRun.Tool(pinned: true, "0.6.1");
        process.Verdict = new ProcessResult(0, "B-k3m9xq\n", string.Empty);
        using RegisteredTools tools = new RegisteredToolsFixture().WithProcess(process);

        // When
        await tools.CallAsync("mint", new Dictionary<string, object?> { ["type"] = "claim", ["feature"] = "features/clean" });

        // Then
        var command = MintRuns(process).Should().ContainSingle().Subject;
        command.Should().HaveCount(4).And.StartWith(["mint", "claim"]).And.ContainInConsecutiveOrder("--feature", "features/clean");
    }

    [Fact]
    public async Task ACallNamingNoFeature_WhenMintIsCalled_ShouldRunTheMintCommandForTheTypeAloneWithNoFeature()
    {
        // Given
        var process = ScriptedProcessRun.Tool(pinned: true, "0.6.1");
        process.Verdict = new ProcessResult(0, "0434\n", string.Empty);
        using RegisteredTools tools = new RegisteredToolsFixture().WithProcess(process);

        // When
        await tools.CallAsync("mint", "type", "adr");

        // Then
        MintRuns(process).Should().ContainSingle().Which.Should().Equal("mint", "adr");
    }

    [Theory]
    [ClassData(typeof(DashedMintArguments))]
    public async Task AMintArgumentWhoseValueStartsWithADash_WhenMintIsCalled_ShouldRunNoCommandAndBeAnError(string type, string? feature)
    {
        // Given
        var process = ScriptedProcessRun.Tool(pinned: true, "0.6.1");
        using RegisteredTools tools = new RegisteredToolsFixture().WithProcess(process);
        var arguments = new Dictionary<string, object?> { ["type"] = type };
        if (feature is not null)
        {
            arguments["feature"] = feature;
        }

        // When
        var result = await tools.CallAsync("mint", arguments);

        // Then
        process.Requests.Should().BeEmpty();
        result.IsError.Should().BeTrue();
    }

    private static List<List<string>> MintRuns(ScriptedProcessRun process) =>
        process.Requests
            .Where(static request => request.Arguments.Take(4).SequenceEqual(["tool", "run", "specht", "--"]))
            .Where(static request => request.Arguments[^1] != "--version")
            .Select(static request => request.Arguments.Skip(4).ToList())
            .ToList();

    private static IReadOnlyList<string> Texts(CallToolResult result) =>
        result.Content.OfType<TextContentBlock>().Select(static block => block.Text).ToList();
}
