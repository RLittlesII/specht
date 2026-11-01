using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Specht.Comparison;

/// <summary>
/// The repository-tooling entry point the release workflow runs: <c>request</c> and <c>attach</c> in the release job, <c>compare</c> in the
/// comparison job. Each composes the real adapters from the Actions environment and always exits zero.
/// </summary>
internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        if (args is not [var command, var directory] || command is not ("request" or "attach" or "compare"))
        {
            await Console.Error.WriteLineAsync("Usage: specht.comparison (request|attach|compare) <repository-relative directory>");
            return 2;
        }

        using var cancellation = new CancellationTokenSource();
        using var interrupt = PosixSignalRegistration.Create(PosixSignal.SIGINT, signal => Stop(signal, cancellation));
        using var terminate = PosixSignalRegistration.Create(PosixSignal.SIGTERM, signal => Stop(signal, cancellation));
        var environment = Environment.GetEnvironmentVariables().Cast<DictionaryEntry>()
            .ToDictionary(static entry => (string)entry.Key, static entry => (string?)entry.Value ?? "", StringComparer.Ordinal);
        var fileSystem = new FileSystem();
        var root = fileSystem.Directory.GetCurrentDirectory();
        var summary = environment.GetValueOrDefault("GITHUB_STEP_SUMMARY") ?? fileSystem.Path.Combine(root, ".artifacts", "benchmarks", "summary.md");
        var context = RunContext.From(environment);
        using var client = Client(environment.GetValueOrDefault("GITHUB_TOKEN"));
        var time = TimeProvider.System;
        return command switch
        {
            "request" => await AdvisoryStep.RunAsync(cancellationToken => context is null
                    ? Task.FromResult<ReportingOutcome?>(null)
                    : new ReleaseReporting(client, fileSystem, time, root).RequestAsync(context, directory, cancellationToken),
                ReportingStage.PairPersistence, fileSystem, summary, cancellation.Token),
            "attach" => await AdvisoryStep.RunAsync(async cancellationToken => context is null
                    ? null
                    : await new ReleaseReporting(client, fileSystem, time, root).AttachAsync(context, directory, cancellationToken),
                ReportingStage.Attachment, fileSystem, summary, cancellation.Token),
            _ => await AdvisoryStep.RunAsync(cancellationToken => context is null
                    ? Task.FromResult<ReportingOutcome?>(null)
                    : new ComparisonRun(client, fileSystem, time,
                        new PairedCollector(fileSystem, time, new ProcessRunner(root, time).RunAsync, root, environment), root,
                        environment.GetValueOrDefault("RUNNER_NAME") ?? "unrecorded").RunAsync(context, directory, cancellationToken),
                ReportingStage.Collection, fileSystem, summary, cancellation.Token),
        };
    }

    private static HttpClient Client(string? token)
    {
        var client = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("specht-comparison", "1"));
        client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
        if (!string.IsNullOrEmpty(token)) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static void Stop(PosixSignalContext signal, CancellationTokenSource cancellation)
    {
        signal.Cancel = true;
        cancellation.Cancel();
    }
}
