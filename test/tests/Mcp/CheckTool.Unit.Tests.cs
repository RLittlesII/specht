using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Specht.Mcp.Features.Host;
using Specht.Mcp.Features.PinnedTool;
using Specht.Mcp.Features.Reading;

namespace Specht.Tests.Mcp;

/// <summary>
/// Holds the tool <c>check</c> as a function of what the command wrote and how it ended, over a scripted set of programs and no
/// process: the verdict relayed as it was written, what is and is not an error, the result's parts, the tool's declaration and the set
/// the host registers (0132-F4 B-001, B-002, B-011, B-012, B-015, B-017, B-019; 0132-F3 B-024).
/// </summary>
[Trait("Tier", "Unit")]
public sealed class CheckToolUnitTests
{
    [Theory]
    [ClassData(typeof(ExitCodesThatAreAnswers))]
    public async Task AVerdictWithNoTrailingNewline_WhenTheCommandEndsWithACleanOrViolatingCode_ShouldBeRelayedUnchangedAndNotMarkedAnError(int exitCode)
    {
        // Given
        var process = ScriptedProcessRun.Tool(pinned: true, "0.5.0");
        process.Verdict = new ProcessResult(exitCode, "{\"violations\":1,\"dash\":\"— café\"}", "ignored");

        // When
        var result = await Check(process).CheckAsync(CancellationToken.None);

        // Then
        result.IsError.Should().NotBe(true);
        Texts(result).Should().Equal("{\"violations\":1,\"dash\":\"— café\"}");
    }

    [Theory]
    [ClassData(typeof(ExitCodesThatAreFailures))]
    public async Task ACommandThatFails_WhenTheCommandEndsWithAFailureCode_ShouldBeAnErrorWhoseTextIsStandardError(int exitCode)
    {
        // Given
        var process = ScriptedProcessRun.Tool(pinned: true, "0.5.0");
        process.Verdict = new ProcessResult(exitCode, "{}", "the manifest is invalid");

        // When
        var result = await Check(process).CheckAsync(CancellationToken.None);

        // Then
        result.IsError.Should().BeTrue();
        Texts(result).Should().Equal("the manifest is invalid");
    }

    [Theory]
    [ClassData(typeof(EmptyVerdicts))]
    public async Task AnEmptyVerdict_WhenTheCommandEndsWithACleanOrViolatingCode_ShouldBeAnErrorThatPointsToRestoringTheToolAndRelaysNoText(
        int exitCode,
        bool pinned)
    {
        // Given
        var process = ScriptedProcessRun.Tool(pinned, "0.5.0");
        process.Verdict = new ProcessResult(exitCode, string.Empty, "dotnet standard error marker");

        // When
        var result = await Check(process).CheckAsync(CancellationToken.None);

        // Then
        result.IsError.Should().BeTrue();
        Texts(result).Should().ContainSingle().Which.Should().Contain("docs/mcp.md#restore-the-tool").And.NotContain("standard error marker");
    }

    [Fact]
    public async Task ACommandThatCannotBeStarted_WhenCheckIsCalled_ShouldBeAnError()
    {
        // Given
        var process = ScriptedProcessRun.Tool(pinned: false, "0.5.0");
        process.Verdict = null;

        // When
        var result = await Check(process).CheckAsync(CancellationToken.None);

        // Then
        result.IsError.Should().BeTrue();
        Texts(result).Should().ContainSingle().Which.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task APinnedTool_WhenCheckIsCalled_ShouldRunTheCommandWithJsonAndAnswerWithOnePart()
    {
        // Given
        var process = ScriptedProcessRun.Tool(pinned: true, "0.5.0");

        // When
        var result = await Check(process).CheckAsync(CancellationToken.None);

        // Then
        process.Requests.Last().Arguments[^1].Should().Be("--json");
        process.Requests.Last().Arguments.Where(static argument => argument is not ("--json" or "--"))
            .Should().NotContain(static argument => argument.StartsWith("--", StringComparison.Ordinal));
        Texts(result).Should().Equal(process.Verdict!.StandardOutput);
    }

    [Fact]
    public async Task AGlobalTool_WhenCheckIsCalled_ShouldAnswerWithTheVerdictAndTheUnpinnedReportAsSeparateParts()
    {
        // Given
        var process = ScriptedProcessRun.Tool(pinned: false, "0.5.0");

        // When
        var result = await Check(process).CheckAsync(CancellationToken.None);

        // Then
        var parts = Texts(result);
        parts.Should().HaveCount(2);
        parts[0].Should().Be(process.Verdict!.StandardOutput);
        parts[1].Should().Contain("unpinned").And.NotContain(process.Verdict.StandardOutput);
    }

    [Fact]
    public async Task ATreeEditedBetweenTwoCalls_WhenCheckIsCalledAgain_ShouldAnswerWithWhatTheCommandWritesThen()
    {
        // Given
        var process = ScriptedProcessRun.Tool(pinned: true, "0.5.0");
        var check = Check(process);
        var first = await check.CheckAsync(CancellationToken.None);
        process.Verdict = new ProcessResult(0, "{\"violations\":0}\n", string.Empty);

        // When
        var second = await check.CheckAsync(CancellationToken.None);

        // Then
        Texts(first).Should().Equal("{}\n");
        Texts(second).Should().Equal("{\"violations\":0}\n");
    }

    [Fact]
    public void TheDeclaration_WhenRead_ShouldNameTheToolCheckAndTakeNoArgument()
    {
        // Given
        var check = Check(new ScriptedProcessRun());

        // When
        var tool = check.Declare().ProtocolTool;

        // Then
        tool.Name.Should().Be("check");
        var schema = tool.InputSchema;
        (schema.TryGetProperty("properties", out var properties) ? properties.EnumerateObject().Count() : 0).Should().Be(0);
        schema.TryGetProperty("required", out var required).Should().BeFalse(required.ToString());
    }

    [Fact]
    public void TheRegisteredTools_WhenResolvedFromTheHostsRegistrations_ShouldBeCheckAndNoWriteAndNoQueue()
    {
        // Given
        using var provider = new ServiceCollection()
            .AddSingleton<IProcessRun>(new ScriptedProcessRun())
            .AddHost()
            .AddPinnedTool()
            .AddReading()
            .BuildServiceProvider();

        // When
        var names = provider.GetServices<McpServerTool>().Select(static tool => tool.ProtocolTool.Name).ToList();

        // Then
        names.Should().Contain("check");
        names.Should().OnlyContain(static name => CommandNames.Contains(name));
        names.Should().NotContain(["init", "upgrade", "schema_pin", "format", "rules_set", "report"]);
    }

    private static CheckTool Check(ScriptedProcessRun process) => new(new SpechtTool(process, new SessionRoot { Value = "repository" }));

    private static IReadOnlyList<string> Texts(CallToolResult result) => result.Content.OfType<TextContentBlock>().Select(static block => block.Text).ToList();

    private static readonly string[] CommandNames = ["check", "explain", "rules", "manifest", "facts", "trace", "schema_list"];
}
