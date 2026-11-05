using System.Collections.Concurrent;
using Specht.Mcp.Features.PinnedTool;

namespace Specht.Tests.Mcp;

/// <summary>
/// The programs the host starts, as a script a test sets: the root's local tool manifest, the pinned tool when it is restored, the
/// global tool when it is installed, and what the command writes. It records every request, so a test sees which program the host
/// asked for and which it did not.
/// </summary>
internal sealed class ScriptedProcessRun : IProcessRun
{
    public const string Pin = "98.76.54";

    public string? ListedPin { get; set; }

    public string? PinnedReports { get; set; }

    public string? GlobalReports { get; set; }

    public ProcessResult? Verdict { get; set; } = new(0, "{}\n", string.Empty);

    public ProcessResult? FailedListing { get; set; }

    public bool DotnetMissing { get; set; }

    public TaskCompletionSource? VersionGate { get; set; }

    public ConcurrentQueue<ProcessRequest> Requests { get; } = [];

    public IEnumerable<ProcessRequest> VersionChecks => Requests.Where(static request => request.Arguments[^1] == "--version");

    public static ScriptedProcessRun Tool(bool pinned, string version) =>
        pinned ? new ScriptedProcessRun { ListedPin = Pin, PinnedReports = version } : new ScriptedProcessRun { GlobalReports = version };

    public async Task<ProcessResult?> RunAsync(ProcessRequest request, CancellationToken cancellationToken)
    {
        Requests.Enqueue(request);
        if (VersionGate is not null && request.Arguments[^1] == "--version")
        {
            await VersionGate.Task.WaitAsync(cancellationToken);
        }

        return Answer(request);
    }

    private ProcessResult? Answer(ProcessRequest request)
    {
        var arguments = request.Arguments;
        if (request.FileName == "dotnet" && DotnetMissing)
        {
            return null;
        }

        if (request.FileName == "dotnet" && arguments.SequenceEqual(["tool", "list"]))
        {
            return FailedListing ?? new ProcessResult(
                0,
                "Package Id      Version      Commands\n---------------------------------------\n"
                + (ListedPin is null ? string.Empty : $"specht.tool     {ListedPin}      specht\n"),
                string.Empty);
        }

        if (request.FileName == "dotnet" && arguments.Take(4).SequenceEqual(["tool", "run", "specht", "--"]))
        {
            return PinnedReports is null
                ? new ProcessResult(1, string.Empty, "Run \"dotnet tool restore\" to make the tool available.")
                : Run(PinnedReports, [.. arguments.Skip(4)]);
        }

        return request.FileName == "specht" && GlobalReports is not null ? Run(GlobalReports, arguments) : null;
    }

    private ProcessResult? Run(string reports, IReadOnlyList<string> arguments) =>
        arguments.SequenceEqual(["--version"]) ? new ProcessResult(0, reports + "\n", string.Empty) : Verdict;
}
