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
/// The tool <c>check</c> (<c>0132-F4</c>): the pinned <c>specht --json</c>'s standard output for the session's root, relayed as one
/// text block unchanged. It offers two optional arguments, the source of the frontmatter schemas and whether any violation is a
/// failure, keeps nothing of an earlier call and runs nothing but the command it is named for.
/// </summary>
/// <param name="tool">Finds the <c>specht</c> to run and runs it.</param>
/// <implements epic="0132" feature="F4" claim="B-001" />
/// <implements epic="0132" feature="F4" claim="B-002" />
/// <implements epic="0132" feature="F4" claim="B-010" />
/// <implements epic="0132" feature="F4" claim="B-011" />
/// <implements epic="0132" feature="F4" claim="B-012" />
/// <implements epic="0132" feature="F4" claim="B-019" />
public sealed class CheckTool(SpechtTool tool)
{
    /// <summary>The tool's name, the command it serves.</summary>
    public const string Name = "check";

    /// <summary>Declares the tool to the protocol.</summary>
    /// <returns>The tool, ready to be listed and called.</returns>
    /// <implements epic="0132" feature="F4" claim="B-010" />
    /// <implements epic="0132" feature="F4" claim="B-019" />
    public McpServerTool Declare() =>
        McpServerTool.Create(
            (Func<string?, bool, CancellationToken, Task<CallToolResult>>) CheckAsync,
            new McpServerToolCreateOptions
            {
                Name = Name,
                Description = "The repository's verdict: what `specht --json` prints for the whole specification tree of the root served.",
                ReadOnly = true,
                OpenWorld = false,
            });

    /// <summary>Asks the tool for the verdict, with the schemas read from a source the caller names and any violation a failure when asked.</summary>
    /// <param name="schemaSource">Where the check reads the frontmatter schemas, <c>embedded</c> or <c>disk</c>; passed to the command as given.</param>
    /// <param name="strict">Whether any violation is a failure, which passes <c>--strict</c> to the command.</param>
    /// <param name="cancellationToken">Ends the call when the session ends.</param>
    /// <returns>The verdict, with the unpinned report beside it when no pin chose the tool; an error when no tool answered.</returns>
    /// <implements epic="0132" feature="F4" claim="B-001" />
    /// <implements epic="0132" feature="F4" claim="B-010" />
    /// <implements epic="0132" feature="F4" claim="B-011" />
    /// <implements epic="0132" feature="F4" claim="B-012" />
    public async Task<CallToolResult> CheckAsync(
        [Description("Where the check reads the frontmatter schemas from: `embedded` or `disk`. Omit it to use the root's manifest.")]
        string? schemaSource = null,
        [Description("Set to true to count any violation as a failure, as `--strict` does. Omit it to count errors only.")] bool strict = false,
        CancellationToken cancellationToken = default)
    {
        var refusal = schemaSource is null ? null : ReadingRelay.RefuseOption("schemaSource", schemaSource);
        if (refusal is not null)
        {
            return refusal;
        }

        List<string> arguments = ["--json"];
        if (schemaSource is not null)
        {
            arguments.AddRange(["--schema-source", schemaSource]);
        }

        if (strict)
        {
            arguments.Add("--strict");
        }

        return await ReadingRelay.RunAsync(tool, arguments, cancellationToken);
    }
}
