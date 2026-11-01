using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Specht.Comparison;

/// <summary>
/// The comparison job's side: it waits for this attempt's request, collects both pinned revisions, evaluates and renders the bundle, and
/// leaves the outcome and bundle for the release job to attach. Each stage it enters is reported to <c>enter</c>, so an unexpected failure
/// is summarised at the stage in progress.
/// </summary>
public sealed class ComparisonRun(HttpClient client, IFileSystem fileSystem, TimeProvider time, PairedCollector collector, string repositoryRoot,
    string runnerIdentity = "unrecorded", Action<ReportingStage>? enter = null)
{
    public async Task<ReportingOutcome?> RunAsync(RunContext context, string directory, CancellationToken cancellationToken)
    {
        if (!ReleaseTrigger.IsReleasePublication(context)) return null;
        var outcome = await CompareAsync(context, directory, cancellationToken);
        enter?.Invoke(ReportingStage.ResultTransport);
        try
        {
            fileSystem.Directory.CreateDirectory(Full(directory));
            await fileSystem.File.WriteAllTextAsync(Full($"{directory}/outcome.json"), ComparisonJson.Write(outcome), Encoding.UTF8,
                CancellationToken.None);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
        }

        return outcome;
    }

    private static string Capitalised(string side) => side == "previous" ? "Previous" : "Current";

    private async Task<ReportingOutcome> CompareAsync(RunContext context, string directory, CancellationToken cancellationToken)
    {
        var session = $"{context.RunId}-{context.Attempt}";
        var request = $"{Workspace}/{session}/request";
        bool found;
        enter?.Invoke(ReportingStage.ReceiptWait);
        try
        {
            found = await new RunArtifacts(client, fileSystem, time, repositoryRoot).WaitAsync(context.Repository, context.RunId,
                $"comparison-request-{context.RunId}-{context.Attempt}", request, ReceiptBound, cancellationToken);
        }
        catch (Exception exception) when (exception is InvalidDataException or HttpRequestException or IOException or JsonException
            or InvalidOperationException or KeyNotFoundException or UnauthorizedAccessException
            || exception is OperationCanceledException && !cancellationToken.IsCancellationRequested)
        {
            return ReportingOutcome.Failed(ReportingStage.Receipt, null);
        }

        if (!found) return ReportingOutcome.Failed(ReportingStage.ReceiptWait, null);
        enter?.Invoke(ReportingStage.Receipt);
        ReleasePair pair;
        try
        {
            pair = ComparisonJson.ReadPair(await fileSystem.File.ReadAllTextAsync(Full($"{request}/pair.json"), cancellationToken));
            var receipt = ComparisonJson.ReadReceipt(await fileSystem.File.ReadAllTextAsync(Full($"{request}/receipt.json"), cancellationToken));
            if (ReleaseTrigger.Validate(receipt, context, pair) is { } rejection) return ReportingOutcome.Failed(ReportingStage.Receipt, rejection);
        }
        catch (Exception exception) when (exception is InvalidDataException or IOException or UnauthorizedAccessException)
        {
            return ReportingOutcome.Failed(ReportingStage.Receipt, null);
        }

        if (pair.Previous is null) return NoBaseline(pair, context, session, directory);
        enter?.Invoke(ReportingStage.Collection);
        if (pair.Previous.Commit is null)
            return ReportingOutcome.Failed(ReportingStage.Collection, $"Previous release {pair.Previous.Tag}: {pair.Previous.ResolutionFailure}");
        var plan = CollectionPlan.For(pair, context.Sha, Version(), context.RunId, context.Attempt, session, runnerIdentity, Filter, Job);
        if (plan is null) return ReportingOutcome.Failed(ReportingStage.Collection, null);
        var collection = await collector.CollectAsync(plan, cancellationToken);
        var sides = new[] { collection.Previous, collection.Current };
        if (sides.Any(static side => side.Status != CollectionStatus.Collected))
            return ReportingOutcome.Failed(ReportingStage.Collection, string.Join(' ', sides.Where(static side => side.Status != CollectionStatus.Collected)
                .Select(static side => $"{Capitalised(side.Side)} revision: {side.Reason}")));

        enter?.Invoke(ReportingStage.Parsing);
        MeasurementInventory previous;
        MeasurementInventory current;
        try
        {
            previous = await InventoryAsync(collection.Previous, collection.Session.Previous, cancellationToken);
            current = await InventoryAsync(collection.Current, collection.Session.Current, cancellationToken);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return ReportingOutcome.Failed(ReportingStage.Parsing, null);
        }

        var unread = new[] { (Side: "previous", Inventory: previous), (Side: "current", Inventory: current) }
            .Where(static read => !read.Inventory.Diagnostics.IsEmpty)
            .Select(static read => $"{Capitalised(read.Side)} revision: {string.Join(' ', read.Inventory.Diagnostics.Distinct(StringComparer.Ordinal))}")
            .ToArray();
        if (unread.Length > 0) return ReportingOutcome.Failed(ReportingStage.Parsing, string.Join(' ', unread));
        enter?.Invoke(ReportingStage.Evaluation);
        var document = ComparisonEvaluation.Evaluate(collection.Session, CasePairing.Pair(previous, current, collection.Session));
        var reasons = sides.Where(static side => side.InventoryReason is not null)
            .Select(static side => $"{Capitalised(side.Side)} revision: {side.InventoryReason}").ToArray();
        var outcome = ReportingOutcome.Completed(document, reasons.Length == 0 ? null : string.Join(' ', reasons));
        ImmutableArray<RawExport> exports = [.. sides.SelectMany(static side => side.ResultFiles.Select(file => new RawExport(side.Side, file)))];
        return Render(outcome, pair, document, [.. collection.Previous.Mappings, .. collection.Current.Mappings], exports, directory);
    }

    private ReportingOutcome NoBaseline(ReleasePair pair, RunContext context, string session, string directory)
    {
        enter?.Invoke(ReportingStage.Evaluation);
        var unmeasured = new MeasurementConditions(null, null, null, null, null, null);
        var manifest = new SessionManifest(1, ComparisonJson.Digest(pair), context.Sha, Version(), context.RunId, context.Attempt, session,
            runnerIdentity, new SessionSide(NoPredecessor, Unmeasured, Unmeasured, [], unmeasured, false, []),
            new SessionSide(context.Sha, Unmeasured, Unmeasured, [], unmeasured, false, []));
        var document = ComparisonEvaluation.NoBaseline(manifest);
        return Render(ReportingOutcome.Completed(document, null), pair, document, [], [], directory);
    }

    private ReportingOutcome Render(ReportingOutcome outcome, ReleasePair pair, ComparisonDocument document,
        ImmutableArray<IdentityMapping> mappings, ImmutableArray<RawExport> exports, string directory)
    {
        enter?.Invoke(ReportingStage.Rendering);
        try
        {
            _ = new ReportBundler(fileSystem, repositoryRoot).Create(pair, document, mappings, exports, directory);
            return outcome;
        }
        catch (Exception exception) when (exception is InvalidDataException or IOException or UnauthorizedAccessException)
        {
            return outcome.Fail(ReportingStage.Rendering, null);
        }
    }

    private async Task<MeasurementInventory> InventoryAsync(RevisionCollection collected, SessionSide side, CancellationToken cancellationToken)
    {
        var unexpected = side with { ExpectedCases = [] };
        var readings = new List<MeasurementInventory>();
        foreach (var file in collected.ResultFiles)
        {
            readings.Add(BenchmarkEvidenceReader.Read(await fileSystem.File.ReadAllTextAsync(Full(file), cancellationToken), file, unexpected,
                collected.Mappings));
        }

        return new MeasurementInventory(readings.Count > 0 && readings.All(static reading => reading.Known),
            [.. readings.SelectMany(static reading => reading.Cases)], [.. readings.SelectMany(static reading => reading.Diagnostics)]);
    }

    private string Full(string relative) => fileSystem.Path.GetFullPath(fileSystem.Path.Combine(repositoryRoot, relative));

    private string Version() => GetType().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unversioned";

    private const string Workspace = ".artifacts/benchmarks/comparison";
    private const string Filter = "*";
    private const string Job = "short";
    private const string Unmeasured = "unavailable";
    private const string NoPredecessor = "0000000000000000000000000000000000000000";

    private static readonly TimeSpan ReceiptBound = TimeSpan.FromMinutes(30);
}
