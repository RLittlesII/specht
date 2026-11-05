using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Specht.Mcp.Features.PinnedTool;

namespace Specht.Mcp.Features.Reading;

/// <summary>
/// The tool <c>check</c> (<c>0132-F4</c>): the pinned <c>specht --json</c>'s standard output for the session's root, relayed as one
/// text block unchanged. It takes no argument, keeps nothing of an earlier call and runs nothing but the command it is named for.
/// </summary>
/// <param name="tool">Finds the <c>specht</c> to run and runs it.</param>
/// <implements epic="0132" feature="F4" claim="B-001" />
/// <implements epic="0132" feature="F4" claim="B-002" />
/// <implements epic="0132" feature="F4" claim="B-011" />
/// <implements epic="0132" feature="F4" claim="B-012" />
/// <implements epic="0132" feature="F4" claim="B-019" />
public sealed class CheckTool(SpechtTool tool)
{
    /// <summary>The tool's name, the command it serves.</summary>
    public const string Name = "check";

    /// <summary>Declares the tool to the protocol.</summary>
    /// <returns>The tool, ready to be listed and called.</returns>
    /// <implements epic="0132" feature="F4" claim="B-019" />
    public McpServerTool Declare() =>
        McpServerTool.Create(
            CheckAsync,
            new McpServerToolCreateOptions
            {
                Name = Name,
                Description = "The repository's verdict: what `specht --json` prints for the whole specification tree of the root served.",
                ReadOnly = true,
                OpenWorld = false,
            });

    /// <summary>Asks the tool for the verdict.</summary>
    /// <param name="cancellationToken">Ends the call when the session ends.</param>
    /// <returns>The verdict, with the unpinned report beside it when no pin chose the tool; an error when no tool answered.</returns>
    /// <implements epic="0132" feature="F4" claim="B-001" />
    /// <implements epic="0132" feature="F4" claim="B-011" />
    /// <implements epic="0132" feature="F4" claim="B-012" />
    public async Task<CallToolResult> CheckAsync(CancellationToken cancellationToken)
    {
        var outcome = await tool.RunAsync(["--json"], cancellationToken);

        return outcome switch
        {
            ToolOutcome.Ran { Result: { ExitCode: 0 or 1, StandardOutput.Length: > 0 } } ran => Answer(ran),
            ToolOutcome.Ran { Result: { ExitCode: 0 or 1 } empty } => Failure(
                $"The check ended with code {empty.ExitCode} and wrote no verdict. A pinned tool that is no longer restored ends this way: "
                + "restore it (docs/mcp.md#restore-the-tool)."),
            ToolOutcome.Ran ran => Failure(
                ran.Result.StandardError.Length > 0
                    ? ran.Result.StandardError
                    : $"specht exited with code {ran.Result.ExitCode} and wrote nothing to standard error."),
            ToolOutcome.Refused refused => Failure(refused.Message),
            _ => throw new InvalidOperationException("The tool answered with an outcome the host does not know."),
        };
    }

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
