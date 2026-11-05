using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Protocol;
using Specht.Mcp.Features.PinnedTool;

namespace Specht.Mcp.Features.Reading;

/// <summary>
/// What every tool of this slice does with a command (<c>0132-F4</c> § 7): run it with the pinned <c>specht</c> and relay what it
/// wrote on standard output as one text block unchanged, with the unpinned report beside it as a second block; a command that ends
/// with <c>2</c>, <c>3</c> or <c>4</c>, and any refusal of the tool, is an error whose text is standard error or the refusal's message.
/// </summary>
/// <implements epic="0132" feature="F4" claim="B-011" />
/// <implements epic="0132" feature="F4" claim="B-012" />
/// <implements epic="0132" feature="F4" claim="B-020" />
/// <implements epic="0132" feature="F4" claim="B-021" />
/// <implements epic="0132" feature="F4" claim="B-022" />
internal static class ReadingRelay
{
    /// <summary>Runs <paramref name="arguments"/> with the pinned <c>specht</c> and answers with what it wrote.</summary>
    /// <param name="tool">Finds the <c>specht</c> to run and runs it.</param>
    /// <param name="arguments">The command's arguments.</param>
    /// <param name="cancellationToken">Ends the call when the session ends.</param>
    /// <returns>The command's answer; an error when it reported a failure, wrote no answer or no tool ran.</returns>
    /// <implements epic="0132" feature="F4" claim="B-012" />
    public static async Task<CallToolResult> RunAsync(
        SpechtTool tool,
        IReadOnlyList<string> arguments,
        CancellationToken cancellationToken)
    {
        var outcome = await tool.RunAsync(arguments, cancellationToken);

        return outcome switch
        {
            ToolOutcome.Ran { Result: { ExitCode: 0 or 1, StandardOutput.Length: > 0 } } ran => Answer(ran),
            ToolOutcome.Ran { Result: { ExitCode: 0 or 1 } empty } => Failure(
                $"The command ended with code {empty.ExitCode} and wrote no answer. A pinned tool that is no longer restored ends this way: "
                + "restore it (docs/mcp.md#restore-the-tool)."),
            ToolOutcome.Ran ran => Failure(
                ran.Result.StandardError.Length > 0
                    ? ran.Result.StandardError
                    : $"specht exited with code {ran.Result.ExitCode} and wrote nothing to standard error."),
            ToolOutcome.Refused refused => Failure(refused.Message),
            _ => throw new InvalidOperationException("The tool answered with an outcome the host does not know."),
        };
    }

    /// <summary>Answers a call whose argument would be read by the command as an option of its own.</summary>
    /// <param name="name">The argument's name.</param>
    /// <param name="value">The argument's value.</param>
    /// <returns>An error when <paramref name="value"/> starts with a dash; otherwise <see langword="null"/>.</returns>
    /// <implements epic="0132" feature="F4" claim="B-022" />
    public static CallToolResult? RefuseOption(string name, string value) =>
        value.StartsWith('-') ? Failure($"The argument '{name}' cannot start with a dash, so no command was run.") : null;

    private static CallToolResult Answer(ToolOutcome.Ran ran)
    {
        List<ContentBlock> content = [new TextContentBlock { Text = ran.Result.StandardOutput }];
        if (ran.UnpinnedReport is { } report)
        {
            content.Add(new TextContentBlock { Text = report });
        }

        return new CallToolResult { Content = content };
    }

    private static CallToolResult Failure(string message) =>
        new() { IsError = true, Content = [new TextContentBlock { Text = message }] };
}
