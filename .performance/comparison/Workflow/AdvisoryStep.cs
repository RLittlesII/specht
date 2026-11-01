using System;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Specht.Comparison;

/// <summary>Runs one comparison-owned workflow step so that it always succeeds and always leaves its summary.</summary>
public static class AdvisoryStep
{
    public static async Task<int> RunAsync(Func<CancellationToken, Task<ReportingOutcome?>> step, ReportingStage stage, IFileSystem fileSystem,
        string summaryPath, CancellationToken cancellationToken)
    {
        await WriteAsync(fileSystem, summaryPath, RunSummary.Pending);
        ReportingOutcome? outcome;
        try
        {
            outcome = await step(cancellationToken);
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            outcome = ReportingOutcome.Failed(stage, null);
        }

        await WriteAsync(fileSystem, summaryPath, outcome is null ? "" : RunSummary.Markdown(outcome));
        return 0;
    }

    private static async Task WriteAsync(IFileSystem fileSystem, string path, string content)
    {
        try
        {
            await fileSystem.File.WriteAllTextAsync(path, content, CancellationToken.None);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
        }
    }
}
