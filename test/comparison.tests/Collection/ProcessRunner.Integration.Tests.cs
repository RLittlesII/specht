using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using AwesomeAssertions;
using Microsoft.Extensions.Time.Testing;

namespace Specht.Comparison.Tests.Collection;

[Trait("Tier", "Integration")]
public sealed class ProcessRunnerIntegrationTests : IDisposable
{
    public ProcessRunnerIntegrationTests() => Directory.CreateDirectory(Path.Combine(_root, "work"));

    [Fact(Timeout = 20_000)]
    public async Task RequestedEnvironment_WhenRunning_ShouldGiveTheChildNothingElse()
    {
        // Given
        if (OperatingSystem.IsWindows())
        {
            Assert.Skip(UnixOnly);
        }

        Environment.SetEnvironmentVariable(ParentOnly, "parent-value");

        // When
        var outcome = await new ProcessRunner(_root, new FakeTimeProvider()).RunAsync(Request("env"), TestContext.Current.CancellationToken);

        // Then
        outcome.ExitCode.Should().Be(0);
        outcome.Output.Select(static line => line.Split('=')[0]).Except(ShellOwned).Should().BeEquivalentTo("PATH", "SPECHT_CHILD");
        outcome.Output.Should().Contain("SPECHT_CHILD=child-value").And.NotContain(static line => line.StartsWith(ParentOnly, StringComparison.Ordinal));
    }

    [Fact(Timeout = 20_000)]
    public async Task RelativeDirectory_WhenRunning_ShouldRunThereAndReturnItsOutputLinesAndExitCode()
    {
        // Given
        if (OperatingSystem.IsWindows())
        {
            Assert.Skip(UnixOnly);
        }

        var request = Request("printf 'first\\nsecond\\n'; : > marker; exit 3");

        // When
        var outcome = await new ProcessRunner(_root, new FakeTimeProvider()).RunAsync(request, TestContext.Current.CancellationToken);

        // Then
        outcome.ExitCode.Should().Be(3);
        outcome.Output.Should().Equal("first", "second");
        File.Exists(Path.Combine(_root, "work", "marker")).Should().BeTrue();
    }

    [Fact(Timeout = 20_000)]
    public async Task CancelledChild_WhenItStartedItsOwnChild_ShouldKillBothAndRethrowTheCancellation()
    {
        // Given
        if (OperatingSystem.IsWindows())
        {
            Assert.Skip(UnixOnly);
        }

        var (watcher, ready) = WatchForReady();
        using var watching = watcher;
        using var caller = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var running = new ProcessRunner(_root, new FakeTimeProvider())
            .RunAsync(Request("sleep 300 & echo $! > child.pid; echo $$ > shell.pid; : > ready; wait"), caller.Token);
        await ready.WaitAsync(TestContext.Current.CancellationToken);

        // When
        await caller.CancelAsync();

        // Then
        await FluentActions.Awaiting(() => running).Should().ThrowAsync<OperationCanceledException>();
        foreach (var file in new[] { "shell.pid", "child.pid" })
        {
            var pid = int.Parse(await File.ReadAllTextAsync(Path.Combine(_root, "work", file), TestContext.Current.CancellationToken),
                CultureInfo.InvariantCulture);
            (await Exited(pid)).Should().BeTrue();
        }
    }

    [Fact(Timeout = 20_000)]
    public async Task OrphanHoldingTheOutput_WhenTheChildIsKilled_ShouldStopWaitingWithinThirtySeconds()
    {
        // Given
        if (OperatingSystem.IsWindows())
        {
            Assert.Skip(UnixOnly);
        }

        var (watcher, ready) = WatchForReady();
        using var watching = watcher;
        var time = new ObservedTime();
        using var caller = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        var running = new ProcessRunner(_root, time)
            .RunAsync(Request("(sleep 300 & echo $! > orphan.pid); : > ready; exec sleep 301"), caller.Token);
        await ready.WaitAsync(TestContext.Current.CancellationToken);

        // When
        await caller.CancelAsync();
        await time.TimerCreated.WaitAsync(TestContext.Current.CancellationToken);
        time.Advance(TimeSpan.FromSeconds(30));

        // Then
        await FluentActions.Awaiting(() => running).Should().ThrowAsync<OperationCanceledException>();
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable(ParentOnly, null);
        foreach (var file in Directory.EnumerateFiles(Path.Combine(_root, "work"), "*.pid"))
        {
            try
            {
                using var process = Process.GetProcessById(int.Parse(File.ReadAllText(file), CultureInfo.InvariantCulture));
                process.Kill();
            }
            catch (ArgumentException)
            {
            }
            catch (InvalidOperationException)
            {
            }
        }

        Directory.Delete(_root, true);
    }

    private static ProcessRequest Request(string script) => new("sh", ["-c", script], "work",
        ImmutableSortedDictionary.CreateRange(StringComparer.Ordinal, new Dictionary<string, string>
        {
            ["PATH"] = Environment.GetEnvironmentVariable("PATH") ?? string.Empty,
            ["SPECHT_CHILD"] = "child-value",
        }));

    private static async Task<bool> Exited(int pid)
    {
        try
        {
            using var process = Process.GetProcessById(pid);
            await process.WaitForExitAsync(TestContext.Current.CancellationToken);
            return process.HasExited;
        }
        catch (ArgumentException)
        {
            return true;
        }
    }

    private (FileSystemWatcher Watcher, Task Ready) WatchForReady()
    {
        var created = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var watcher = new FileSystemWatcher(Path.Combine(_root, "work"), "ready");
        watcher.Created += (_, _) => created.TrySetResult();
        watcher.EnableRaisingEvents = true;
        return (watcher, created.Task);
    }

    private sealed class ObservedTime : TimeProvider
    {
        public Task TimerCreated => _created.Task;

        public override long TimestampFrequency => _inner.TimestampFrequency;

        public void Advance(TimeSpan delta) => _inner.Advance(delta);

        public override DateTimeOffset GetUtcNow() => _inner.GetUtcNow();

        public override long GetTimestamp() => _inner.GetTimestamp();

        public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
        {
            var timer = _inner.CreateTimer(callback, state, dueTime, period);
            _created.TrySetResult();
            return timer;
        }

        private readonly FakeTimeProvider _inner = new();
        private readonly TaskCompletionSource _created = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private const string ParentOnly = "SPECHT_RUNNER_PARENT_ONLY";
    private const string UnixOnly = "The runner proof starts sh, which Windows does not provide.";

    private static readonly string[] ShellOwned = ["PWD", "OLDPWD", "SHLVL", "_"];

    private readonly string _root = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
}
