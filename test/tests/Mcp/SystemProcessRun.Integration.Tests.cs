using System.Diagnostics;
using System.Globalization;
using AwesomeAssertions;
using Specht.Mcp.Features.PinnedTool;

namespace Specht.Tests.Mcp;

/// <summary>
/// Holds <see cref="SystemProcessRun"/> against real programs started with <c>sh</c>: what a program writes and how it ends comes
/// back as it was, a program never reads the session's standard input, a cancelled run leaves no program alive, and a program that
/// does not exist is none (0132-F3 B-017).
/// </summary>
[Trait("Tier", "Integration")]
public sealed class SystemProcessRunIntegrationTests
{
    public static bool IsWindows => OperatingSystem.IsWindows();

    [Fact(Skip = "The fixture program is a shell script.", SkipWhen = nameof(IsWindows), Timeout = 30_000)]
    public async Task AProgramThatWrites_WhenItEnds_ShouldGiveBackItsCodeAndBothStreamsUnchanged()
    {
        // Given
        var request = new ProcessRequest("sh", ["-c", "printf ' a — b '; printf 'e\\n' >&2; exit 3"], Directory.GetCurrentDirectory());

        // When
        var result = await new SystemProcessRun().RunAsync(request, TestContext.Current.CancellationToken);

        // Then
        result.Should().Be(new ProcessResult(3, " a — b ", "e\n"));
    }

    [Fact(Skip = "The fixture program is a shell script.", SkipWhen = nameof(IsWindows), Timeout = 30_000)]
    public async Task AProgramThatReadsStandardInput_WhenItRuns_ShouldFindItClosedAndEnd()
    {
        // Given
        var request = new ProcessRequest("sh", ["-c", "cat >/dev/null; printf ended"], Directory.GetCurrentDirectory());

        // When
        var result = await new SystemProcessRun().RunAsync(request, TestContext.Current.CancellationToken);

        // Then
        result.Should().Be(new ProcessResult(0, "ended", string.Empty));
    }

    [Fact(Timeout = 30_000)]
    public async Task AProgramThatDoesNotExist_WhenItRuns_ShouldGiveBackNothing()
    {
        // Given
        var request = new ProcessRequest("specht-program-that-does-not-exist", [], Directory.GetCurrentDirectory());

        // When
        var result = await new SystemProcessRun().RunAsync(request, TestContext.Current.CancellationToken);

        // Then
        result.Should().BeNull();
    }

    [Fact(Skip = "The fixture program is a shell script.", SkipWhen = nameof(IsWindows), Timeout = 30_000)]
    public async Task AProgramThatIsRunning_WhenTheRunIsCancelled_ShouldThrowCancelledAndLeaveNoProgramAlive()
    {
        // Given
        var token = TestContext.Current.CancellationToken;
        var directory = Directory.CreateTempSubdirectory("specht-process-").FullName;
        var pids = Path.Combine(directory, "pids");
        using var cancel = new CancellationTokenSource();
        var request = new ProcessRequest("sh", ["-c", "sleep 300 & printf '%s\\n%s\\n' \"$$\" \"$!\" >pids.next && mv pids.next pids; wait"], directory);
        var run = new SystemProcessRun().RunAsync(request, cancel.Token);
        try
        {
            using (var waiting = CancellationTokenSource.CreateLinkedTokenSource(token))
            {
                waiting.CancelAfter(TimeSpan.FromSeconds(10));
                while (!File.Exists(pids))
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(20), waiting.Token);
                }
            }

            var written = await File.ReadAllLinesAsync(pids, token);
            var shell = int.Parse(written[0], CultureInfo.InvariantCulture);
            var sleeper = int.Parse(written[1], CultureInfo.InvariantCulture);

            // When
            await cancel.CancelAsync();
            var thrown = await Record.ExceptionAsync(() => run);
            var survivors = new List<int>();
            using var exiting = CancellationTokenSource.CreateLinkedTokenSource(token);
            exiting.CancelAfter(TimeSpan.FromSeconds(10));
            foreach (var pid in new[] { shell, sleeper })
            {
                try
                {
                    using var program = Process.GetProcessById(pid);
                    try
                    {
                        await program.WaitForExitAsync(exiting.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        survivors.Add(pid);
                        program.Kill();
                    }
                }
                catch (ArgumentException)
                {
                }
            }

            // Then
            thrown.Should().BeAssignableTo<OperationCanceledException>();
            survivors.Should().BeEmpty();
        }
        finally
        {
            await cancel.CancelAsync();
            await run.ContinueWith(static _ => { }, TaskScheduler.Default);
            Directory.Delete(directory, recursive: true);
        }
    }
}
