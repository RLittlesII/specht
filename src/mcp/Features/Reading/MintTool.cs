using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Specht.Mcp.Features.PinnedTool;

namespace Specht.Mcp.Features.Reading;

/// <summary>
/// The tool <c>mint</c> (<c>0132-F4</c>): the pinned <c>specht mint</c>'s standard output for a record type in the session's root,
/// relayed as one text block unchanged. It offers one required argument, the type, and one optional argument, the Feature, mints
/// nothing itself, writes nothing and runs nothing but the command it is named for.
/// </summary>
/// <param name="tool">Finds the <c>specht</c> to run and runs it.</param>
/// <implements epic="0132" feature="F4" claim="B-012" />
/// <implements epic="0132" feature="F4" claim="B-019" />
/// <implements epic="0132" feature="F4" claim="B-022" />
/// <implements epic="0132" feature="F4" claim="B-023" />
/// <implements epic="0132" feature="F4" claim="B-024" />
/// <implements epic="0132" feature="F4" claim="B-025" />
/// <implements epic="0132" feature="F4" claim="B-026" />
public sealed class MintTool(SpechtTool tool)
{
    /// <summary>The tool's name, the command it serves.</summary>
    public const string Name = "mint";

    /// <summary>Declares the tool to the protocol.</summary>
    /// <returns>The tool, ready to be listed and called.</returns>
    /// <implements epic="0132" feature="F4" claim="B-019" />
    /// <implements epic="0132" feature="F4" claim="B-024" />
    public McpServerTool Declare() =>
        McpServerTool.Create(
            (Func<string, string?, CancellationToken, Task<CallToolResult>>) MintAsync,
            new McpServerToolCreateOptions
            {
                Name = Name,
                Description = "A new id for a record type the root's manifest opts in: what `specht mint <type> [--feature <dir>]` prints.",
                ReadOnly = true,
                OpenWorld = false,
            });

    /// <summary>Asks the tool for a new id of a type, in the Feature the caller names.</summary>
    /// <param name="type">The record type, such as <c>adr</c> or <c>claim</c>; passed to the command as given.</param>
    /// <param name="feature">The folder of the Feature whose ids the type draws from, which passes <c>--feature</c> to the command.</param>
    /// <param name="cancellationToken">Ends the call when the session ends.</param>
    /// <returns>The id, with the unpinned report beside it when no pin chose the tool; an error when the command refused or no tool
    /// answered.</returns>
    /// <implements epic="0132" feature="F4" claim="B-022" />
    /// <implements epic="0132" feature="F4" claim="B-023" />
    /// <implements epic="0132" feature="F4" claim="B-025" />
    /// <implements epic="0132" feature="F4" claim="B-026" />
    public async Task<CallToolResult> MintAsync(
        [Description("The record type to mint an id for, such as `adr` or `claim`; the root's manifest must opt it in.")] string type,
        [Description("The folder, relative to the root, of the Feature a per-Feature type's id belongs to. Omit it for a repository-wide type.")]
        string? feature = null,
        CancellationToken cancellationToken = default)
    {
        var refusal = ReadingRelay.RefuseOption("type", type)
            ?? (feature is null ? null : ReadingRelay.RefuseOption("feature", feature));
        if (refusal is not null)
        {
            return refusal;
        }

        List<string> arguments = ["mint", type];
        if (feature is not null)
        {
            arguments.AddRange(["--feature", feature]);
        }

        return await ReadingRelay.RunAsync(tool, arguments, cancellationToken);
    }
}
