using System;
using System.Diagnostics;
using specht.tool.Features.Check;

namespace specht.acceptance;

/// <summary>
/// One launch of the built tool, as a shell sees it: <c>dotnet</c> over the tool's assembly with stdout and stderr redirected.
/// Spectre's command tester captures stdout alone, so a step that reads stderr or the process's exit code launches the tool.
/// </summary>
/// <param name="ExitCode">The process's exit code.</param>
/// <param name="Stdout">Everything the process wrote on stdout.</param>
/// <param name="Stderr">Everything the process wrote on stderr.</param>
public sealed record ToolRun(int ExitCode, string Stdout, string Stderr)
{
    /// <summary>Launches the tool in a working directory with the given arguments and waits for it to exit.</summary>
    /// <param name="workingDirectory">The directory the process starts in.</param>
    /// <param name="args">The arguments after the tool's assembly.</param>
    /// <returns>The exit code and both streams.</returns>
    public static ToolRun Launch(string workingDirectory, params string[] args)
    {
        var start = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        start.ArgumentList.Add(typeof(CheckCommand).Assembly.Location);
        foreach (var arg in args)
        {
            start.ArgumentList.Add(arg);
        }

        using var process = Process.Start(start) ?? throw new InvalidOperationException("dotnet did not start.");
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEnd();
        var output = stdout.GetAwaiter().GetResult();
        process.WaitForExit();

        return new ToolRun(process.ExitCode, output, stderr);
    }
}
