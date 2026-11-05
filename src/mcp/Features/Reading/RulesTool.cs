using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Specht.Mcp.Features.PinnedTool;

namespace Specht.Mcp.Features.Reading;

/// <summary>
/// The tool <c>rules</c> (<c>0132-F4</c>): the pinned <c>specht rules --json</c>'s standard output for the session's root, relayed as
/// one text block unchanged. It takes no argument, keeps nothing of an earlier call and runs nothing but the command it is named for.
/// </summary>
/// <param name="tool">Finds the <c>specht</c> to run and runs it.</param>
/// <implements epic="0132" feature="F4" claim="B-004" />
/// <implements epic="0132" feature="F4" claim="B-012" />
/// <implements epic="0132" feature="F4" claim="B-019" />
public sealed class RulesTool(SpechtTool tool)
{
    /// <summary>The tool's name, the command it serves.</summary>
    public const string Name = "rules";

    /// <summary>Declares the tool to the protocol.</summary>
    /// <returns>The tool, ready to be listed and called.</returns>
    /// <implements epic="0132" feature="F4" claim="B-019" />
    public McpServerTool Declare() =>
        McpServerTool.Create(
            RulesAsync,
            new McpServerToolCreateOptions
            {
                Name = Name,
                Description = "The rules of the schema version the root's manifest pins: what `specht rules --json` prints.",
                ReadOnly = true,
                OpenWorld = false,
            });

    /// <summary>Asks the tool for the rule list.</summary>
    /// <param name="cancellationToken">Ends the call when the session ends.</param>
    /// <returns>The list, with the unpinned report beside it when no pin chose the tool; an error when no tool answered.</returns>
    /// <implements epic="0132" feature="F4" claim="B-004" />
    /// <implements epic="0132" feature="F4" claim="B-012" />
    public Task<CallToolResult> RulesAsync(CancellationToken cancellationToken) =>
        ReadingRelay.RunAsync(tool, ["rules", "--json"], cancellationToken);
}
