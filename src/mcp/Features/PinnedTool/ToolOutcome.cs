namespace Specht.Mcp.Features.PinnedTool;

/// <summary>What asking the tool to run a command came to.</summary>
public abstract record ToolOutcome
{
    /// <summary>The tool ran the command.</summary>
    /// <param name="Result">What it wrote and its exit code.</param>
    /// <param name="UnpinnedReport">The report that the global tool answered because nothing is pinned; <see langword="null"/> when a pinned tool ran.</param>
    public sealed record Ran(ProcessResult Result, string? UnpinnedReport) : ToolOutcome;

    /// <summary>No tool ran, and the message says why and where to read what to do.</summary>
    /// <param name="Message">The error, never carrying a path of the machine.</param>
    public sealed record Refused(string Message) : ToolOutcome;
}
