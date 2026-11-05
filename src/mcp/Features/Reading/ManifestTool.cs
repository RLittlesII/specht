using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Specht.Mcp.Features.PinnedTool;

namespace Specht.Mcp.Features.Reading;

/// <summary>
/// The tool <c>manifest</c> (<c>0132-F4</c>): the pinned <c>specht manifest --json</c>'s standard output for the session's root,
/// relayed as one text block unchanged. It offers one optional argument, the shipped defaults in place of the root's manifest, keeps
/// nothing of an earlier call and runs nothing but the command it is named for.
/// </summary>
/// <param name="tool">Finds the <c>specht</c> to run and runs it.</param>
/// <implements epic="0132" feature="F4" claim="B-005" />
/// <implements epic="0132" feature="F4" claim="B-010" />
/// <implements epic="0132" feature="F4" claim="B-012" />
/// <implements epic="0132" feature="F4" claim="B-019" />
public sealed class ManifestTool(SpechtTool tool)
{
    /// <summary>The tool's name, the command it serves.</summary>
    public const string Name = "manifest";

    /// <summary>Declares the tool to the protocol.</summary>
    /// <returns>The tool, ready to be listed and called.</returns>
    /// <implements epic="0132" feature="F4" claim="B-019" />
    public McpServerTool Declare() =>
        McpServerTool.Create(
            (Func<bool, CancellationToken, Task<CallToolResult>>) ManifestAsync,
            new McpServerToolCreateOptions
            {
                Name = Name,
                Description = "The manifest in effect for the root: what `specht manifest --json` prints, or the shipped defaults when asked.",
                ReadOnly = true,
                OpenWorld = false,
            });

    /// <summary>Asks the tool for the manifest.</summary>
    /// <param name="defaults">Whether to ask for the newest shipped default manifest in place of the root's.</param>
    /// <param name="cancellationToken">Ends the call when the session ends.</param>
    /// <returns>The manifest, with the unpinned report beside it when no pin chose the tool; an error when no tool answered.</returns>
    /// <implements epic="0132" feature="F4" claim="B-005" />
    /// <implements epic="0132" feature="F4" claim="B-010" />
    /// <implements epic="0132" feature="F4" claim="B-012" />
    public Task<CallToolResult> ManifestAsync(
        [Description("Set to true for the newest shipped default manifest in place of the root's own.")] bool defaults = false,
        CancellationToken cancellationToken = default) =>
        ReadingRelay.RunAsync(tool, defaults ? ["manifest", "--defaults", "--json"] : ["manifest", "--json"], cancellationToken);
}
