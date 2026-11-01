using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Specht.Comparison;

/// <summary>Builds and runs each pinned revision's own benchmark harness in turn, within one bounded session.</summary>
public sealed class PairedCollector(IFileSystem fileSystem, TimeProvider time,
    Func<ProcessRequest, CancellationToken, Task<ProcessOutcome>> run, string repositoryRoot,
    IReadOnlyDictionary<string, string> parentEnvironment)
{
    public async Task<CollectionResult> CollectAsync(CollectionPlan plan, CancellationToken cancellationToken)
    {
        using var budget = new CancellationTokenSource(PairBound, time);
        using var total = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, budget.Token);
        var previous = await CollectRevisionAsync(plan, "previous", plan.PreviousCommit, cancellationToken, total.Token);
        var current = await CollectRevisionAsync(plan, "current", plan.CurrentCommit, cancellationToken, total.Token);
        var session = new SessionManifest(1, plan.PairDigest, plan.ComparatorCommit, plan.ComparatorVersion, plan.RunId, plan.Attempt,
            plan.SessionId, plan.RunnerIdentity, previous.Evidence, current.Evidence);
        fileSystem.Directory.CreateDirectory(Full(plan.Workspace));
        await WriteOnceAsync(Full($"{plan.Workspace}/session-manifest.json"), ComparisonJson.Write(session));
        return new CollectionResult(session, previous, current);
    }

    private static ImmutableArray<string> Invocation(CollectionPlan plan) => plan.Job is null
        ? ["--filter", plan.Filter, "--artifacts", "../results"]
        : ["--filter", plan.Filter, "--artifacts", "../results", "--job", plan.Job];

    private static RevisionCollection Unavailable(string side, string commit, ImmutableArray<string> invocation, string reason) =>
        new(side, CollectionStatus.Unavailable, reason, [], new SessionSide(commit, UnavailableEvidence, UnavailableEvidence, invocation,
            new MeasurementConditions(null, null, null, null, null, null), false, []), [], null);

    private async Task<RevisionCollection> CollectRevisionAsync(CollectionPlan plan, string side, string commit,
        CancellationToken caller, CancellationToken total)
    {
        var invocation = Invocation(plan);
        if (total.IsCancellationRequested)
            return Unavailable(side, commit, invocation, caller.IsCancellationRequested ? Cancelled : PairExceeded);
        var workspace = $"{plan.Workspace}/{side}";
        if (fileSystem.Directory.Exists(Full(workspace))) return Unavailable(side, commit, invocation, "Revision workspace already exists.");
        using var bound = new CancellationTokenSource(RevisionBound, time);
        using var limit = CancellationTokenSource.CreateLinkedTokenSource(total, bound.Token);
        try
        {
            return await MeasureAsync(plan, side, commit, workspace, invocation, limit.Token);
        }
        catch (OperationCanceledException) when (limit.IsCancellationRequested)
        {
            return Unavailable(side, commit, invocation,
                caller.IsCancellationRequested ? Cancelled : total.IsCancellationRequested ? PairExceeded : RevisionExceeded);
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            return Unavailable(side, commit, invocation, "Revision collection failed unexpectedly.");
        }
        finally
        {
            foreach (var folder in new[] { "source", "home", "temp" }) Remove(Full($"{workspace}/{folder}"));
        }
    }

    private async Task<RevisionCollection> MeasureAsync(CollectionPlan plan, string side, string commit, string workspace,
        ImmutableArray<string> invocation, CancellationToken cancellationToken)
    {
        var source = $"{workspace}/source";
        var environment = RevisionEvidence.ChildEnvironment(parentEnvironment, Full($"{workspace}/home"), Full($"{workspace}/temp"),
            Full($"{plan.Workspace}/nuget"));
        fileSystem.Directory.CreateDirectory(Full($"{workspace}/home"));
        fileSystem.Directory.CreateDirectory(Full($"{workspace}/temp"));

        var clone = await Step("git", ".", ["clone", "--quiet", "--no-checkout", "--shared", ".", source]);
        if (clone.ExitCode != 0) return Failed("Revision clone", clone);
        var checkout = await Step("git", source, ["checkout", "--quiet", "--detach", commit]);
        if (checkout.ExitCode != 0) return Failed("Revision checkout", checkout);
        var head = await Step("git", source, ["rev-parse", "HEAD"]);
        if (head.ExitCode != 0) return Failed("Revision identity check", head);
        if (head.Output is not [var printed] || printed.Trim() != commit)
            return Unavailable(side, commit, invocation, "Checked-out revision does not match the pinned commit.");
        if (!fileSystem.File.Exists(Full($"{source}/{Harness}")))
            return Unavailable(side, commit, invocation, "Revision has no native benchmark harness.");

        var build = await Step("dotnet", source,
            ["build", Harness, "--configuration", "Release", "--framework", "net10.0", "--disable-build-servers"]);
        if (build.ExitCode != 0) return Failed("Benchmark build", build);
        var listing = await Step("dotnet", source, [.. RunPrefix, .. invocation, "--list", "flat"]);
        if (listing.ExitCode != 0) return Failed("Benchmark listing", listing);
        var selected = RevisionEvidence.Select(listing.Output);
        if (selected.IsEmpty) return Unavailable(side, commit, invocation, "No benchmarks selected for the filter.");
        if (RevisionEvidence.NeedsTool(selected))
        {
            var pack = await Step("dotnet", source,
                ["pack", "src/tool/tool.csproj", "--configuration", "Release", "--output", ".artifacts/nupkg", "--disable-build-servers"]);
            if (pack.ExitCode != 0) return Failed("Tool pack", pack);
        }

        var measured = await Step("dotnet", source, [.. RunPrefix, .. invocation]);
        if (measured.ExitCode != 0) return Failed("Benchmark run", measured);

        var results = $"{workspace}/results/results";
        var files = fileSystem.Directory.Exists(Full(results))
            ? fileSystem.Directory.GetFiles(Full(results), "*-report-full.json")
                .Select(file => $"{results}/{fileSystem.Path.GetFileName(file)}").Order(StringComparer.Ordinal).ToImmutableArray()
            : [];
        var exports = ImmutableArray.CreateBuilder<string>(files.Length);
        foreach (var file in files) exports.Add(await fileSystem.File.ReadAllTextAsync(Full(file), cancellationToken));
        var reading = RevisionEvidence.Read(exports.MoveToImmutable());
        if (reading is not { ExporterVersion: { } exporter, Conditions: { } conditions })
            return Unavailable(side, commit, invocation, reading.Failure!);

        var mappings = Declarations(source, commit);
        var expected = mappings.IsEmpty ? null : RevisionEvidence.Inventory(mappings, commit, selected);
        var evidence = new SessionSide(commit, NativeHarness, exporter, invocation, conditions, expected is not null, expected ?? []);
        return new RevisionCollection(side, CollectionStatus.Collected, null, files, evidence, mappings,
            mappings.IsEmpty ? Undeclared : expected is null ? PartlyDeclared : null);

        Task<ProcessOutcome> Step(string fileName, string directory, ImmutableArray<string> arguments) =>
            run(new ProcessRequest(fileName, arguments, directory, environment), cancellationToken);

        RevisionCollection Failed(string step, ProcessOutcome outcome) => Unavailable(side, commit, invocation,
            $"{step} failed with exit code {outcome.ExitCode.ToString(CultureInfo.InvariantCulture)}.");
    }

    private ImmutableArray<IdentityMapping> Declarations(string source, string commit)
    {
        var assembly = Full($"{source}/{DeclaringAssembly}");
        if (!fileSystem.File.Exists(assembly)) return [];
        using var stream = fileSystem.File.OpenRead(assembly);
        return DeclaredIdentity.Read(stream, commit, Full(source), relative =>
        {
            var path = Full($"{source}/{relative}");
            return fileSystem.File.Exists(path) ? fileSystem.File.ReadAllBytes(path) : null;
        });
    }

    private async Task WriteOnceAsync(string path, string content)
    {
        try
        {
            await using var stream = fileSystem.FileStream.New(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            await stream.WriteAsync(Encoding.UTF8.GetBytes(content), CancellationToken.None);
        }
        catch (IOException) when (fileSystem.File.Exists(path))
        {
        }
    }

    private void Remove(string path)
    {
        try
        {
            if (fileSystem.Directory.Exists(path)) fileSystem.Directory.Delete(path, true);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
        }
    }

    private string Full(string relative) => fileSystem.Path.GetFullPath(fileSystem.Path.Combine(repositoryRoot, relative));

    private const string Harness = ".performance/benchmarks/benchmarks.csproj";
    private const string DeclaringAssembly = ".performance/benchmarks/bin/Release/net10.0/specht.benchmarks.dll";
    private const string NativeHarness = "native-v1";
    private const string UnavailableEvidence = "unavailable";
    private const string Cancelled = "Collection was cancelled.";
    private const string PairExceeded = "Paired collection exceeded its 60-minute bound.";
    private const string RevisionExceeded = "Revision collection exceeded its 25-minute bound.";
    private const string Undeclared = "Revision benchmarks declare no workload identity.";
    private const string PartlyDeclared = "Revision benchmarks do not declare one workload identity for every selected benchmark.";

    private static readonly ImmutableArray<string> RunPrefix =
        ["run", "--project", Harness, "--configuration", "Release", "--framework", "net10.0", "--no-build", "--"];

    private static readonly TimeSpan PairBound = TimeSpan.FromMinutes(60);
    private static readonly TimeSpan RevisionBound = TimeSpan.FromMinutes(25);
}
