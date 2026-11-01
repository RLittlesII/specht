using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Specht.Comparison;

/// <summary>
/// The release job's side of the comparison: it saves the pair and writes the request after publication, then attaches the returned
/// report to the release once it is confirmed to be this request's.
/// </summary>
public sealed class ReleaseReporting(HttpClient client, IFileSystem fileSystem, TimeProvider time, string repositoryRoot)
{
    public async Task<ReportingOutcome?> RequestAsync(RunContext context, string directory, CancellationToken cancellationToken)
    {
        if (!ReleaseTrigger.IsReleasePublication(context)) return null;
        ReleasePair pair;
        try
        {
            var catalog = await new ReleaseCatalogReader(client).ReadAsync(context.Repository, cancellationToken);
            var tag = context.Ref[TagReference.Length..];
            var current = catalog.Releases.IsDefault ? null : catalog.Releases.SingleOrDefault(release => release.Tag == tag);
            if (current is null || current.Commit != context.Sha)
                return ReportingOutcome.Failed(ReportingStage.PairPersistence, "The release catalog holds no release of this tag at this commit.");
            pair = await new ReleasePairAssetStore(client).GetOrCreateAsync(current,
                _ => Task.FromResult(ReleaseSelection.Select(current, catalog, context.RunId)), cancellationToken);
        }
        catch (Exception exception) when (exception is InvalidDataException or InvalidOperationException or HttpRequestException)
        {
            return ReportingOutcome.Failed(ReportingStage.PairPersistence, null);
        }

        var receipt = new ComparisonReceipt(1, context.Repository, context.RunId, context.Attempt, context.Sha, pair.Current.Id,
            ComparisonJson.Digest(pair));
        try
        {
            fileSystem.Directory.CreateDirectory(Full(directory));
            await WriteNewAsync(Full($"{directory}/{PairFile}"), ComparisonJson.Write(pair), cancellationToken);
            await WriteNewAsync(Full($"{directory}/{ReceiptFile}"), ComparisonJson.Write(receipt), cancellationToken);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return ReportingOutcome.Failed(ReportingStage.Receipt, null);
        }

        return null;
    }

    public async Task<ReportingOutcome> AttachAsync(RunContext context, string directory, CancellationToken cancellationToken)
    {
        if (!ReleaseTrigger.IsReleasePublication(context)) return ReportingOutcome.Failed(ReportingStage.Receipt, "This run published no release.");
        ReleasePair pair;
        ComparisonReceipt receipt;
        try
        {
            pair = ComparisonJson.ReadPair(await fileSystem.File.ReadAllTextAsync(Full($"{directory}/{PairFile}"), cancellationToken));
            receipt = ComparisonJson.ReadReceipt(await fileSystem.File.ReadAllTextAsync(Full($"{directory}/{ReceiptFile}"), cancellationToken));
        }
        catch (Exception exception) when (exception is InvalidDataException or IOException or UnauthorizedAccessException)
        {
            return ReportingOutcome.Failed(ReportingStage.Receipt, null);
        }

        if (ReleaseTrigger.Validate(receipt, context, pair) is { } rejection) return ReportingOutcome.Failed(ReportingStage.Receipt, rejection);
        var returned = $"{Workspace}/{context.RunId}-{context.Attempt}/result";
        bool found;
        try
        {
            found = await new RunArtifacts(client, fileSystem, time, repositoryRoot).WaitAsync(context.Repository, context.RunId,
                $"comparison-result-{context.RunId}-{context.Attempt}", returned, ResultBound, cancellationToken);
        }
        catch (Exception exception) when (IsUnusable(exception, cancellationToken))
        {
            return ReportingOutcome.Failed(ReportingStage.ResultTransport, null);
        }

        if (!found) return ReportingOutcome.Failed(ReportingStage.ResultWait, null);
        ReportingOutcome outcome;
        byte[] bundle;
        var name = $"comparison-{pair.Current.Id}-{context.RunId}-{context.Attempt}.zip";
        try
        {
            outcome = ComparisonJson.ReadOutcome(await fileSystem.File.ReadAllTextAsync(Full($"{returned}/{OutcomeFile}"), cancellationToken));
            if (outcome.Report is not null) return ReportingOutcome.Failed(ReportingStage.ResultTransport, null);
            if (outcome.State == ReportingState.Incomplete) return outcome;
            bundle = await fileSystem.File.ReadAllBytesAsync(Full($"{returned}/{name}"), cancellationToken);
            if (!IsThisRequests(bundle, pair, context, outcome)) return ReportingOutcome.Failed(ReportingStage.ResultTransport, null);
        }
        catch (Exception exception) when (IsUnusable(exception, cancellationToken))
        {
            return ReportingOutcome.Failed(ReportingStage.ResultTransport, null);
        }

        try
        {
            return outcome.Attached(new ReportReference(pair.Current,
                await new ReportBundleAssetStore(client).PublishAsync(pair.Current, name, bundle, cancellationToken)));
        }
        catch (Exception exception) when (IsUnusable(exception, cancellationToken))
        {
            return outcome.Fail(ReportingStage.Attachment, null);
        }
    }

    private static bool IsThisRequests(byte[] bundle, ReleasePair pair, RunContext context, ReportingOutcome outcome)
    {
        _ = ReportBundler.Validate(new MemoryStream(bundle));
        using var archive = new ZipArchive(new MemoryStream(bundle), ZipArchiveMode.Read);
        var session = ComparisonJson.ReadSession(Entry(archive, "session.json"));
        using var report = JsonDocument.Parse(Entry(archive, "report.json"));
        return Entry(archive, "pair.json") == ComparisonJson.Write(pair) && session.RunId == context.RunId && session.Attempt == context.Attempt
            && report.RootElement.GetProperty("comparison").GetProperty("judgment").GetString() == outcome.Judgment?.ToString();
    }

    private static string Entry(ZipArchive archive, string name)
    {
        using var reader = new StreamReader((archive.GetEntry(name) ?? throw new InvalidDataException("Report bundle entry is missing.")).Open(),
            Encoding.UTF8);
        return reader.ReadToEnd();
    }

    private static bool IsUnusable(Exception exception, CancellationToken cancellationToken) =>
        exception is InvalidDataException or HttpRequestException or IOException or JsonException or InvalidOperationException
            or KeyNotFoundException or UnauthorizedAccessException
        || exception is OperationCanceledException && !cancellationToken.IsCancellationRequested;

    private async Task WriteNewAsync(string path, string content, CancellationToken cancellationToken)
    {
        await using var stream = fileSystem.FileStream.New(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await stream.WriteAsync(Encoding.UTF8.GetBytes(content), cancellationToken);
    }

    private string Full(string relative) => fileSystem.Path.GetFullPath(fileSystem.Path.Combine(repositoryRoot, relative));

    private const string TagReference = "refs/tags/";
    private const string PairFile = "pair.json";
    private const string ReceiptFile = "receipt.json";
    private const string OutcomeFile = "outcome.json";
    private const string Workspace = ".artifacts/benchmarks/comparison";

    private static readonly TimeSpan ResultBound = TimeSpan.FromMinutes(100);
}
