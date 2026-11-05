using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Specht.Mcp.Features.PinnedTool;

namespace Specht.Mcp.Features.Reading;

/// <summary>
/// The tool <c>explain</c> (<c>0132-F4</c>): the pinned <c>specht --explain</c>'s standard output for a rule id, relayed as one text
/// block unchanged. It offers one required argument, the rule id, keeps nothing of an earlier call and runs nothing but the command it
/// is named for.
/// </summary>
/// <param name="tool">Finds the <c>specht</c> to run and runs it.</param>
/// <implements epic="0132" feature="F4" claim="B-003" />
/// <implements epic="0132" feature="F4" claim="B-010" />
/// <implements epic="0132" feature="F4" claim="B-012" />
/// <implements epic="0132" feature="F4" claim="B-019" />
public sealed class ExplainTool(SpechtTool tool)
{
    /// <summary>The tool's name, the command it serves.</summary>
    public const string Name = "explain";

    /// <summary>Declares the tool to the protocol.</summary>
    /// <returns>The tool, ready to be listed and called.</returns>
    /// <implements epic="0132" feature="F4" claim="B-019" />
    public McpServerTool Declare() =>
        McpServerTool.Create(
            (Func<string, CancellationToken, Task<CallToolResult>>) ExplainAsync,
            new McpServerToolCreateOptions
            {
                Name = Name,
                Description = "The page of one rule: what `specht --explain <rule>` prints for the schema version the root's manifest pins.",
                ReadOnly = true,
                OpenWorld = false,
            });

    /// <summary>Asks the tool for the page of a rule.</summary>
    /// <param name="rule">The rule id, such as <c>SPEC4004</c>; passed to the command as given.</param>
    /// <param name="cancellationToken">Ends the call when the session ends.</param>
    /// <returns>The page, with the unpinned report beside it when no pin chose the tool; an error when the pinned
    /// version has no such rule or no tool answered.</returns>
    /// <implements epic="0132" feature="F4" claim="B-003" />
    /// <implements epic="0132" feature="F4" claim="B-012" />
    public async Task<CallToolResult> ExplainAsync(
        [Description("The id of the rule to explain, such as `SPEC4004`.")] string rule,
        CancellationToken cancellationToken) =>
        ReadingRelay.RefuseOption("rule", rule) ?? await ReadingRelay.RunAsync(tool, ["--explain", rule], cancellationToken);
}
