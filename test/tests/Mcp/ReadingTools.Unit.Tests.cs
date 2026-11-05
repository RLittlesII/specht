using AwesomeAssertions;
using ModelContextProtocol.Protocol;
using Specht.Mcp.Features.PinnedTool;

namespace Specht.Tests.Mcp;

/// <summary>
/// Holds the tools <c>explain</c>, <c>rules</c> and <c>manifest</c>, and the arguments <c>check</c> and <c>manifest</c> pass on, as a
/// function of what the command wrote and how it ended, over a scripted set of programs and no process. Each tool is the one the host
/// registers, found by name, so a tool the host does not register fails here on its absence (0132-F4 B-003, B-004, B-005, B-010, B-012,
/// B-014, B-019, B-020, B-021, B-022).
/// </summary>
[Trait("Tier", "Unit")]
public sealed class ReadingToolsUnitTests
{
    [Theory]
    [ClassData(typeof(ReadingCommands))]
    public async Task ACommandThatPrintsAnAnswer_WhenItsToolIsCalled_ShouldRelayItUnchangedAsOnePartAndRunOnlyThatCommand(
        string tool,
        string? argument,
        string? value,
        string[] command)
    {
        // Given
        var process = ScriptedProcessRun.Tool(pinned: true, "0.5.0");
        process.Verdict = new ProcessResult(0, "{\"answer\":\"— café\"}", "ignored");
        using RegisteredTools tools = new RegisteredToolsFixture().WithProcess(process);

        // When
        var result = await tools.CallAsync(tool, argument, value);

        // Then
        result.IsError.Should().NotBe(true);
        Texts(result).Should().Equal("{\"answer\":\"— café\"}");
        process.Requests.Last().Arguments.Skip(4).Should().Equal(command);
    }

    [Theory]
    [ClassData(typeof(FailedReadings))]
    public async Task ACommandThatFails_WhenItsToolIsCalled_ShouldBeAnErrorWhoseTextIsStandardError(
        string tool,
        string? argument,
        string? value,
        int exitCode)
    {
        // Given
        var process = ScriptedProcessRun.Tool(pinned: true, "0.5.0");
        process.Verdict = new ProcessResult(exitCode, "{}", "the command's message");
        using RegisteredTools tools = new RegisteredToolsFixture().WithProcess(process);

        // When
        var result = await tools.CallAsync(tool, argument, value);

        // Then
        result.IsError.Should().BeTrue();
        Texts(result).Should().Equal("the command's message");
    }

    [Fact]
    public async Task AShippedDefaultsRequest_WhenManifestIsCalled_ShouldRunTheCommandWithDefaultsAndAnswerWithWhatItWrites()
    {
        // Given
        var process = ScriptedProcessRun.Tool(pinned: true, "0.5.0");
        process.Verdict = new ProcessResult(0, "{\"defaults\":true}", string.Empty);
        using RegisteredTools tools = new RegisteredToolsFixture().WithProcess(process);

        // When
        var result = await tools.CallAsync("manifest", "defaults", true);

        // Then
        process.Requests.Last().Arguments.Skip(4).Should().BeEquivalentTo(["manifest", "--defaults", "--json"]);
        Texts(result).Should().Equal("{\"defaults\":true}");
    }

    [Fact]
    public async Task ASchemaSourceRequest_WhenCheckIsCalled_ShouldRunTheCommandWithThatSchemaSourceAndAnswerWithWhatItWrites()
    {
        // Given
        var process = ScriptedProcessRun.Tool(pinned: true, "0.5.0");
        process.Verdict = new ProcessResult(1, "{\"schemaSource\":\"disk\"}", string.Empty);
        using RegisteredTools tools = new RegisteredToolsFixture().WithProcess(process);

        // When
        var result = await tools.CallAsync("check", "schemaSource", "disk");

        // Then
        var arguments = process.Requests.Last().Arguments.Skip(4).ToList();
        arguments.Should().HaveCount(3).And.ContainInConsecutiveOrder("--schema-source", "disk").And.Contain("--json");
        Texts(result).Should().Equal("{\"schemaSource\":\"disk\"}");
    }

    [Fact]
    public async Task AStrictRequest_WhenCheckIsCalled_ShouldRunTheCommandWithStrictAndAnswerWithWhatItWrites()
    {
        // Given
        var process = ScriptedProcessRun.Tool(pinned: true, "0.5.0");
        process.Verdict = new ProcessResult(1, "{\"strict\":true}", string.Empty);
        using RegisteredTools tools = new RegisteredToolsFixture().WithProcess(process);

        // When
        var result = await tools.CallAsync("check", "strict", true);

        // Then
        process.Requests.Last().Arguments.Skip(4).Should().Equal("--json", "--strict");
        result.IsError.Should().NotBe(true);
        Texts(result).Should().Equal("{\"strict\":true}");
    }

    [Theory]
    [ClassData(typeof(ReadingCommands))]
    public async Task ThePinMovedBetweenTwoCalls_WhenTheToolIsCalledAgain_ShouldRunTheCommandAgainAndAnswerWithWhatItWritesThen(
        string tool,
        string? argument,
        string? value,
        string[] command)
    {
        // Given
        var process = ScriptedProcessRun.Tool(pinned: true, "0.5.0");
        process.Verdict = new ProcessResult(0, "under the first pin", string.Empty);
        using RegisteredTools tools = new RegisteredToolsFixture().WithProcess(process);
        var first = await tools.CallAsync(tool, argument, value);
        process.Verdict = new ProcessResult(0, "under the second pin", string.Empty);

        // When
        var second = await tools.CallAsync(tool, argument, value);

        // Then
        Texts(first).Should().Equal("under the first pin");
        Texts(second).Should().Equal("under the second pin");
        var ran = process.Requests
            .Where(static request => request.Arguments.Take(4).SequenceEqual(["tool", "run", "specht", "--"]))
            .Where(static request => request.Arguments[^1] != "--version")
            .Select(static request => request.Arguments.Skip(4).ToList())
            .ToList();
        ran.Should().HaveCount(2).And.AllBeEquivalentTo(command);
    }

    [Theory]
    [ClassData(typeof(EmptyReadings))]
    public async Task AnEmptyAnswer_WhenTheCommandEndsWithACleanOrViolatingCode_ShouldBeAnErrorThatNamesWhereToReadHowToRestoreTheTool(
        string tool,
        string? argument,
        string? value,
        int exitCode)
    {
        // Given
        var process = ScriptedProcessRun.Tool(pinned: true, "0.5.0");
        process.Verdict = new ProcessResult(exitCode, string.Empty, "dotnet standard error marker");
        using RegisteredTools tools = new RegisteredToolsFixture().WithProcess(process);

        // When
        var result = await tools.CallAsync(tool, argument, value);

        // Then
        result.IsError.Should().BeTrue();
        Texts(result).Should().Contain(static text => text.Contains("docs/mcp.md#restore-the-tool", StringComparison.Ordinal));
    }

    [Theory]
    [ClassData(typeof(EmptyReadings))]
    public async Task AnEmptyAnswer_WhenTheCommandEndsWithACleanOrViolatingCode_ShouldCarryNoTextTheCommandWrote(
        string tool,
        string? argument,
        string? value,
        int exitCode)
    {
        // Given
        var process = ScriptedProcessRun.Tool(pinned: true, "0.5.0");
        process.Verdict = new ProcessResult(exitCode, string.Empty, "dotnet standard error marker");
        using RegisteredTools tools = new RegisteredToolsFixture().WithProcess(process);

        // When
        var result = await tools.CallAsync(tool, argument, value);

        // Then
        string.Concat(Texts(result)).Should().NotContain("standard error marker");
    }

    [Theory]
    [ClassData(typeof(DashedArguments))]
    public async Task AnArgumentWhoseValueStartsWithADash_WhenItsToolIsCalled_ShouldRunNoCommandAndBeAnErrorThatNamesTheArgumentAndNotTheValue(
        string tool,
        string argument,
        string value)
    {
        // Given
        var process = ScriptedProcessRun.Tool(pinned: true, "0.5.0");
        using RegisteredTools tools = new RegisteredToolsFixture().WithProcess(process);

        // When
        var result = await tools.CallAsync(tool, argument, value);

        // Then
        process.Requests.Should().BeEmpty();
        result.IsError.Should().BeTrue();
        var text = string.Concat(Texts(result));
        text.Should().Contain($"'{argument}'").And.Contain("no command was run").And.NotContain(value);
    }

    [Theory]
    [ClassData(typeof(DeclaredArguments))]
    public void ATool_WhenItsDeclarationIsRead_ShouldOfferOnlyTheArgumentsItsCommandTakesAndRequireOnlyWhatItCannotRunWithout(
        string tool,
        string[] offered,
        string[] required)
    {
        // Given
        using RegisteredTools tools = new RegisteredToolsFixture();

        // When
        var schema = tools.Declared(tool).InputSchema;

        // Then
        var properties = schema.TryGetProperty("properties", out var found) ? found.EnumerateObject().Select(static property => property.Name) : [];
        var requirements = schema.TryGetProperty("required", out var listed) ? listed.EnumerateArray().Select(static name => name.GetString()!) : [];
        properties.Should().BeEquivalentTo(offered);
        requirements.Should().BeEquivalentTo(required);
    }

    [Fact]
    public void TheRegisteredTools_WhenResolvedFromTheHostsRegistrations_ShouldIncludeEachToolNamedForTheCommandItServes()
    {
        // Given
        using RegisteredTools tools = new RegisteredToolsFixture();

        // When
        var names = tools.Names;

        // Then
        names.Should().Contain(["check", "explain", "rules", "manifest"]);
        names.Should().BeSubsetOf(["check", "explain", "rules", "manifest", "facts", "trace", "schema_list"]);
    }

    private static IReadOnlyList<string> Texts(CallToolResult result) =>
        result.Content.OfType<TextContentBlock>().Select(static block => block.Text).ToList();
}
