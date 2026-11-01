using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Specht.Comparison;

/// <summary>Runs one child process with exactly the requested environment and kills its whole tree on cancellation.</summary>
public sealed class ProcessRunner(string repositoryRoot, TimeProvider time)
{
    public async Task<ProcessOutcome> RunAsync(ProcessRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var start = new ProcessStartInfo(request.FileName)
        {
            WorkingDirectory = Path.GetFullPath(request.WorkingDirectory, repositoryRoot),
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };
        foreach (var argument in request.Arguments) start.ArgumentList.Add(argument);
        start.Environment.Clear();
        foreach (var (name, value) in request.Environment) start.Environment[name] = value;

        var output = ImmutableArray.CreateBuilder<string>();
        using var process = new Process { StartInfo = start };
        process.OutputDataReceived += (_, line) =>
        {
            if (line.Data is null) return;
            lock (output) output.Add(line.Data);
        };
        process.Start();
        process.BeginOutputReadLine();
        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            using var grace = new CancellationTokenSource(KillGrace, time);
            try
            {
                await process.WaitForExitAsync(grace.Token);
            }
            catch (OperationCanceledException) when (grace.IsCancellationRequested)
            {
            }

            throw;
        }

        lock (output) return new ProcessOutcome(process.ExitCode, output.ToImmutable());
    }

    private static readonly TimeSpan KillGrace = TimeSpan.FromSeconds(30);
}
