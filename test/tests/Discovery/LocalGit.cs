using System.Diagnostics;
using AwesomeAssertions;

namespace Specht.Tests.Discovery;

/// <summary>
/// Runs the real <c>git</c> in a test's temporary root and fails the test unless it exits <c>0</c>, so a work tree a test
/// makes is the one it asserts over (<c>0001-F6</c> B-005, B-010, B-011).
/// </summary>
internal static class LocalGit
{
    public static void Run(string root, params string[] arguments)
    {
        var start = new ProcessStartInfo("git")
        {
            WorkingDirectory = root,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        foreach (var argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        using var process = Process.Start(start)!;
        var error = process.StandardError.ReadToEndAsync(TestContext.Current.CancellationToken);
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        process.ExitCode.Should().Be(0, output + error.GetAwaiter().GetResult());
    }
}
