using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Specht.Mcp.Features.Host;

namespace Specht.Mcp.Features.PinnedTool;

/// <summary>
/// Finds the <c>specht</c> the session runs and runs a command with it (<c>0132-F3</c> § 7, ADR-0031). The root's local tool
/// manifest decides: a pin runs through <c>dotnet tool run</c> in the root, and no other <c>specht</c> is looked for while a pin
/// exists. With no pin the global <c>specht</c> runs and the answer is reported as unpinned. Nothing is restored or installed. The
/// tool's version is read once per session for each tool run and must reach <see cref="Minimum"/>.
/// </summary>
/// <param name="process">Starts the programs.</param>
/// <param name="root">The root every program runs in.</param>
/// <implements epic="0132" feature="F3" claim="B-017" />
/// <implements epic="0132" feature="F3" claim="B-018" />
/// <implements epic="0132" feature="F3" claim="B-020" />
/// <implements epic="0132" feature="F3" claim="B-022" />
/// <implements epic="0132" feature="F3" claim="B-023" />
/// <implements epic="0132" feature="F3" claim="B-024" />
/// <implements epic="0132" feature="F3" claim="B-026" />
/// <implements epic="0132" feature="F3" claim="B-027" />
/// <implements epic="0132" feature="F3" claim="B-030" />
public sealed class SpechtTool(IProcessRun process, SessionRoot root)
{
    /// <summary>The package a repository pins.</summary>
    public const string PackageId = "specht.tool";

    /// <summary>Gets the oldest <c>specht.tool</c> the host runs, the first release with <c>--version</c> and the check's <c>--json</c>.</summary>
    public ToolVersion Minimum { get; } = new(0, 1, 90);

    /// <summary>Runs <c>specht</c> with <paramref name="arguments"/> in the root.</summary>
    /// <param name="arguments">The command's arguments.</param>
    /// <param name="cancellationToken">Ends the call when the session ends.</param>
    /// <returns>What the tool wrote, or why none ran.</returns>
    /// <implements epic="0132" feature="F3" claim="B-017" />
    public async Task<ToolOutcome> RunAsync(IReadOnlyList<string> arguments, CancellationToken cancellationToken)
    {
        var listing = await process.RunAsync(new ProcessRequest("dotnet", ["tool", "list"], root.Value), cancellationToken);
        if (listing is { ExitCode: not 0 })
        {
            return new ToolOutcome.Refused($"The root's local tool manifest could not be read: {listing.StandardError.Trim()}");
        }

        var pinned = listing is null ? null : PinnedVersion(listing.StandardOutput);
        var target = pinned is null
            ? new Target("global", "specht", "specht", [])
            : new Target($"pinned {pinned}", $"{PackageId} {pinned}", "dotnet", ["tool", "run", "specht", "--"]);

        var refusal = await CheckVersionAsync(target, pinned is not null, cancellationToken);
        if (refusal is not null)
        {
            return refusal;
        }

        var ran = await Run(target, arguments, cancellationToken);
        if (ran is null)
        {
            return new ToolOutcome.Refused($"{target.Name} could not be started.");
        }

        return new ToolOutcome.Ran(ran, pinned is null ? UnpinnedReport(_versions[target.Key]) : null);
    }

    private static string? PinnedVersion(string listing) =>
        listing
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(static line => line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Where(static columns => columns.Length >= 2 && string.Equals(columns[0], PackageId, StringComparison.OrdinalIgnoreCase))
            .Select(static columns => columns[1])
            .FirstOrDefault();

    private static string UnpinnedReport(ToolVersion version) =>
        $"unpinned: no local tool manifest for this root pins {PackageId}, so the global specht tool {version} answered. "
        + "A pinned tool is the version the repository chose; pin it to get that (docs/mcp.md#pin-the-tool).";

    private async Task<ToolOutcome.Refused?> CheckVersionAsync(Target target, bool pinned, CancellationToken cancellationToken)
    {
        if (!_versions.TryGetValue(target.Key, out var version))
        {
            var answer = await Run(target, ["--version"], cancellationToken);
            if (answer is null && !pinned)
            {
                return new ToolOutcome.Refused(
                    "No pin and no global tool: the pin in a local tool manifest for this root is not found, and the global specht tool "
                    + $"is not found. Pin {PackageId} (docs/mcp.md#pin-the-tool) or install it as a global tool.");
            }

            if (pinned && answer is not { ExitCode: 0 })
            {
                return new ToolOutcome.Refused(
                    $"{target.Name} is pinned in the local tool manifest and cannot run, as it is not restored. The host never restores "
                    + "it: restore it, then call again (docs/mcp.md#restore-the-tool).");
            }

            if (answer is not { ExitCode: 0 } || !ToolVersion.TryParse(answer.StandardOutput, out version))
            {
                return new ToolOutcome.Refused(
                    $"{target.Name} did not state a version for --version, so it cannot be checked against the minimum {Minimum}. "
                    + $"Update {PackageId} to {Minimum} or later (docs/mcp.md#update-the-tool).");
            }

            _versions[target.Key] = version;
        }

        return version.CompareTo(Minimum) < 0
            ? new ToolOutcome.Refused(
                $"{target.Name} states version {version}, older than the minimum {Minimum} this host requires. "
                + $"Update {PackageId} to {Minimum} or later (docs/mcp.md#update-the-tool).")
            : null;
    }

    private Task<ProcessResult?> Run(Target target, IReadOnlyList<string> arguments, CancellationToken cancellationToken) =>
        process.RunAsync(new ProcessRequest(target.FileName, [.. target.Prefix, .. arguments], root.Value), cancellationToken);

    private sealed record Target(string Key, string Name, string FileName, IReadOnlyList<string> Prefix);

    private readonly ConcurrentDictionary<string, ToolVersion> _versions = new(StringComparer.Ordinal);
}
