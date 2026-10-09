using System;
using System.Diagnostics;
using specht.tool.Features.Check;

namespace specht.acceptance;

/// <summary>
/// The built tool, launched as a shell launches it, so a scenario sees the process's stdout, stderr and exit code; Spectre's
/// command tester captures stdout alone (0001-F2 C-7).
/// </summary>
internal static class Tool
{
    /// <summary>Runs the tool in <paramref name="workingDirectory"/> with <paramref name="args"/> and waits for it to exit.</summary>
    /// <param name="workingDirectory">The directory the process starts in.</param>
    /// <param name="args">The arguments, each passed as one.</param>
    /// <returns>What the process wrote to stdout and to stderr, and its exit code.</returns>
    public static (string Stdout, string Stderr, int ExitCode) Launch(string workingDirectory, params string[] args)
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

        return (output, stderr, process.ExitCode);
    }
}
