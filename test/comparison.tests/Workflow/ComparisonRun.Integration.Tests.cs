using System.IO.Abstractions;
using System.IO.Compression;
using System.Net;
using System.Text.Json;
using AwesomeAssertions;
using Microsoft.Extensions.Time.Testing;
using Specht.Comparison.Tests.Collection;
using Specht.Comparison.Tests.Reporting;
using Specht.Comparison.Tests.Selection;

namespace Specht.Comparison.Tests.Workflow;

[Trait("Tier", "Integration")]
public sealed class ComparisonRunIntegrationTests : IDisposable
{
    public ComparisonRunIntegrationTests()
    {
        Directory.CreateDirectory(_root);
        _processes = new ScriptedRevisions(_root);
        _processes.Heads["previous"] = new string('e', 40);
        _processes.Heads["current"] = new string('a', 40);
        _handler = new ScriptedHttpHandler(request =>
        {
            lock (_requests) _requests.Add(request.RequestUri!.PathAndQuery);
            return _artifacts.Respond(request) ?? new HttpResponseMessage(HttpStatusCode.NotFound);
        });
        _client = new HttpClient(_handler);
    }

    [Fact]
    public async Task ReceiptOfThisRunsPublication_WhenComparing_ShouldCollectBothPinnedRevisionsAndShareTheResult()
    {
        // Given
        Request(new ComparisonReceiptFixture(), ReportFixtures.Pair());

        // When
        var outcome = await Comparison().RunAsync(new RunContextFixture(), "result", TestContext.Current.CancellationToken);

        // Then
        outcome.Should().NotBeNull();
        outcome!.State.Should().Be(ReportingState.Completed);
        Checkouts().Should().Equal(new string('e', 40), new string('a', 40));
        (await File.ReadAllTextAsync(Path.Combine(_root, "result", "outcome.json"), TestContext.Current.CancellationToken))
            .Should().Be(ComparisonJson.Write(outcome));
        await using var bundle = File.OpenRead(Path.Combine(_root, "result", "comparison-20-1234-2.zip"));
        ReportBundler.Validate(bundle).Should().NotBeNull();
        _requests.Should().NotContain(static request => request.Contains("/releases", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ReceiptThatDoesNotMatchThisRun_WhenComparing_ShouldStartNoCollection()
    {
        // Given
        Request(new ComparisonReceiptFixture().WithAttempt(1), ReportFixtures.Pair());

        // When
        var outcome = await Comparison().RunAsync(new RunContextFixture(), "result", TestContext.Current.CancellationToken);

        // Then
        outcome.Should().NotBeNull();
        outcome!.Stage.Should().Be(ReportingStage.Receipt);
        outcome.State.Should().Be(ReportingState.Incomplete);
        outcome.Judgment.Should().BeNull();
        _processes.Requests.Should().BeEmpty();
        (await File.ReadAllTextAsync(Path.Combine(_root, "result", "outcome.json"), TestContext.Current.CancellationToken))
            .Should().Be(ComparisonJson.Write(outcome));
    }

    [Fact(Timeout = 20_000)]
    public async Task NoReceiptForThisAttempt_WhenTheThirtyMinuteWaitPasses_ShouldBeIncompleteWithoutCollecting()
    {
        // Given
        _artifacts.Add(1, "comparison-request-1234-1", false, ActionsArtifacts.Zip(("receipt.json",
            ComparisonJson.Write((ComparisonReceipt)new ComparisonReceiptFixture().WithAttempt(1))),
            ("pair.json", ComparisonJson.Write((ReleasePair)ReportFixtures.Pair()))));
        var comparing = Comparison().RunAsync(new RunContextFixture(), "result", TestContext.Current.CancellationToken);
        await _artifacts.Listed.Task;

        // When
        _time.Advance(TimeSpan.FromMinutes(30) - TimeSpan.FromSeconds(1));
        var beforeTheBound = comparing.IsCompleted;
        _time.Advance(TimeSpan.FromSeconds(1));
        var outcome = await comparing;

        // Then
        beforeTheBound.Should().BeFalse();
        outcome.Should().NotBeNull();
        outcome!.Stage.Should().Be(ReportingStage.ReceiptWait);
        outcome.State.Should().Be(ReportingState.Incomplete);
        _processes.Requests.Should().BeEmpty();
        _requests.Should().NotContain(static request => request.EndsWith("/zip", StringComparison.Ordinal));
    }

    [Theory]
    [ClassData(typeof(RunsPublishingNoRelease))]
    public async Task RunThatPublishesNoRelease_WhenComparing_ShouldStartNothing(string eventName, string reference)
    {
        // Given
        Request(new ComparisonReceiptFixture(), ReportFixtures.Pair());

        // When
        var outcome = await Comparison().RunAsync(new RunContextFixture().WithEventName(eventName).WithRef(reference), "result",
            TestContext.Current.CancellationToken);

        // Then
        outcome.Should().BeNull();
        _requests.Should().BeEmpty();
        _processes.Requests.Should().BeEmpty();
        Directory.Exists(Path.Combine(_root, "result")).Should().BeFalse();
    }

    [Fact]
    public async Task PairWithoutAPredecessor_WhenComparing_ShouldShareANoBaselineReportWithoutCollecting()
    {
        // Given
        ReleasePair pair = ReportFixtures.Pair().WithPrevious(null);
        Request(new ComparisonReceiptFixture().WithPairDigest(ComparisonJson.Digest(pair)), pair);

        // When
        var outcome = await Comparison().RunAsync(new RunContextFixture(), "result", TestContext.Current.CancellationToken);

        // Then
        outcome.Should().NotBeNull();
        outcome!.State.Should().Be(ReportingState.Completed);
        outcome.Judgment.Should().Be(ComparisonJudgment.NoBaseline);
        outcome.Flags.Should().BeEmpty();
        _processes.Requests.Should().BeEmpty();
        using var bundle = ZipFile.OpenRead(Path.Combine(_root, "result", "comparison-20-1234-2.zip"));
        using var report = await JsonDocument.ParseAsync(bundle.GetEntry("report.json")!.Open(), cancellationToken: TestContext.Current.CancellationToken);
        report.RootElement.GetProperty("comparison").GetProperty("judgment").GetString().Should().Be("NoBaseline");
        using var markdown = new StreamReader(bundle.GetEntry("report.md")!.Open());
        (await markdown.ReadToEndAsync(TestContext.Current.CancellationToken)).Should().Contain("Overall judgment: No baseline")
            .And.NotContain("No regression flagged").And.NotContain("Review required");
    }

    [Fact]
    public async Task UnresolvedPredecessor_WhenComparing_ShouldBeIncompleteWithTheResolutionFailureWithoutCollectingOrSelectingAgain()
    {
        // Given
        ReleasePair pair = ReportFixtures.Pair().WithPrevious(new ReleaseRecordFixture().WithId(10).WithTag("v1").WithCommit(null)
            .WithResolutionFailure("Release tag v1 cannot be resolved to a commit."));
        Request(new ComparisonReceiptFixture().WithPairDigest(ComparisonJson.Digest(pair)), pair);

        // When
        var outcome = await Comparison().RunAsync(new RunContextFixture(), "result", TestContext.Current.CancellationToken);

        // Then
        outcome.Should().NotBeNull();
        outcome!.State.Should().Be(ReportingState.Incomplete);
        outcome.Judgment.Should().BeNull();
        outcome.Reason.Should().Contain("Release tag v1 cannot be resolved to a commit.");
        _processes.Requests.Should().BeEmpty();
        _requests.Should().NotContain(static request => request.Contains("/releases", StringComparison.Ordinal));
    }

    [Fact]
    public async Task PredecessorWithoutAHarness_WhenComparing_ShouldBeIncompleteWithoutCheckingOutAnotherRevision()
    {
        // Given
        _processes.WithoutHarness.Add("previous");
        Request(new ComparisonReceiptFixture(), ReportFixtures.Pair());

        // When
        var outcome = await Comparison().RunAsync(new RunContextFixture(), "result", TestContext.Current.CancellationToken);

        // Then
        outcome.Should().NotBeNull();
        outcome!.State.Should().Be(ReportingState.Incomplete);
        outcome.Stage.Should().Be(ReportingStage.Collection);
        outcome.Reason.Should().Contain("Revision has no native benchmark harness.");
        Checkouts().Should().Equal(new string('e', 40), new string('a', 40));
        _requests.Should().NotContain(static request => request.Contains("/releases", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ExistingBundleForThisAttempt_WhenRendering_ShouldBeIncompleteAtRenderingAndLeaveItIntact()
    {
        // Given
        Request(new ComparisonReceiptFixture(), ReportFixtures.Pair());
        Directory.CreateDirectory(Path.Combine(_root, "result"));
        await File.WriteAllBytesAsync(Path.Combine(_root, "result", "comparison-20-1234-2.zip"), [1, 2, 3], TestContext.Current.CancellationToken);

        // When
        var outcome = await Comparison().RunAsync(new RunContextFixture(), "result", TestContext.Current.CancellationToken);

        // Then
        outcome.Should().NotBeNull();
        outcome!.Stage.Should().Be(ReportingStage.Rendering);
        outcome.State.Should().Be(ReportingState.Incomplete);
        outcome.Report.Should().BeNull();
        (await File.ReadAllBytesAsync(Path.Combine(_root, "result", "comparison-20-1234-2.zip"), TestContext.Current.CancellationToken))
            .Should().Equal(1, 2, 3);
    }

    [Theory]
    [ClassData(typeof(ExportsTheReaderRejects))]
    public async Task ExportTheReaderRejects_WhenComparing_ShouldBeIncompleteAtParsingNamingEachRejectedSideWithoutABundle(string[] sides, string reason)
    {
        // Given
        foreach (var side in sides)
            _processes.Exports[side] = [.. _processes.Exports[side].Select(static export =>
                (export.Name, export.Json.Replace("\"Configuration\": \"RELEASE\"", "\"Configuration\": \"DEBUG\"", StringComparison.Ordinal)))];
        Request(new ComparisonReceiptFixture(), ReportFixtures.Pair());

        // When
        var outcome = await Comparison().RunAsync(new RunContextFixture(), "result", TestContext.Current.CancellationToken);

        // Then
        outcome.Should().NotBeNull();
        outcome!.Stage.Should().Be(ReportingStage.Parsing);
        outcome.State.Should().Be(ReportingState.Incomplete);
        outcome.Judgment.Should().BeNull();
        outcome.Flags.Should().BeEmpty();
        outcome.Report.Should().BeNull();
        outcome.Reason.Should().Be(reason);
        File.Exists(Path.Combine(_root, "result", "comparison-20-1234-2.zip")).Should().BeFalse();
        (await File.ReadAllTextAsync(Path.Combine(_root, "result", "outcome.json"), TestContext.Current.CancellationToken))
            .Should().Be(ComparisonJson.Write(outcome));
    }

    [Fact]
    public async Task ExportUnreadableAfterCollection_WhenComparing_ShouldBeIncompleteAtParsingWithoutABundle()
    {
        // Given
        var previousResults = Path.Combine(_root, ".artifacts", "benchmarks", "comparison", "1234-2", "previous", "results", "results");
        _processes.During = (side, step, _) =>
        {
            if (side == "current" && step == "run")
                foreach (var export in Directory.GetFiles(previousResults, "*-report-full.json")) File.Delete(export);
            return Task.CompletedTask;
        };
        Request(new ComparisonReceiptFixture(), ReportFixtures.Pair());

        // When
        var outcome = await Comparison().RunAsync(new RunContextFixture(), "result", TestContext.Current.CancellationToken);

        // Then
        outcome.Should().NotBeNull();
        outcome!.Stage.Should().Be(ReportingStage.Parsing);
        outcome.State.Should().Be(ReportingState.Incomplete);
        outcome.Judgment.Should().BeNull();
        outcome.Flags.Should().BeEmpty();
        outcome.Report.Should().BeNull();
        outcome.Reason.Should().Be("The collected benchmark exports could not be read. Re-run all jobs to retry with the saved pair.");
        File.Exists(Path.Combine(_root, "result", "comparison-20-1234-2.zip")).Should().BeFalse();
    }

    [Fact]
    public async Task ReceiptOfThisRunsPublication_WhenComparing_ShouldEnterEachStageInOrder()
    {
        // Given
        Request(new ComparisonReceiptFixture(), ReportFixtures.Pair());
        List<ReportingStage> stages = [];

        // When
        await Comparison(stages.Add).RunAsync(new RunContextFixture(), "result", TestContext.Current.CancellationToken);

        // Then
        stages.Should().Equal(ReportingStage.ReceiptWait, ReportingStage.Receipt, ReportingStage.Collection, ReportingStage.Parsing,
            ReportingStage.Evaluation, ReportingStage.Rendering, ReportingStage.ResultTransport);
    }

    [Fact]
    public async Task PairWithoutAPredecessor_WhenComparing_ShouldEnterNoCollectionOrParsingStage()
    {
        // Given
        ReleasePair pair = ReportFixtures.Pair().WithPrevious(null);
        Request(new ComparisonReceiptFixture().WithPairDigest(ComparisonJson.Digest(pair)), pair);
        List<ReportingStage> stages = [];

        // When
        await Comparison(stages.Add).RunAsync(new RunContextFixture(), "result", TestContext.Current.CancellationToken);

        // Then
        stages.Should().Equal(ReportingStage.ReceiptWait, ReportingStage.Receipt, ReportingStage.Evaluation, ReportingStage.Rendering,
            ReportingStage.ResultTransport);
    }

    [Fact(Timeout = 20_000)]
    public async Task NoReceiptForThisAttempt_WhenTheWaitPasses_ShouldEnterOnlyTheReceiptWaitAndTheResultTransport()
    {
        // Given
        List<ReportingStage> stages = [];
        var comparing = Comparison(stages.Add).RunAsync(new RunContextFixture(), "result", TestContext.Current.CancellationToken);
        await _artifacts.Listed.Task;

        // When
        _time.Advance(TimeSpan.FromMinutes(30));
        await comparing;

        // Then
        stages.Should().Equal(ReportingStage.ReceiptWait, ReportingStage.ResultTransport);
    }

    [Fact]
    public async Task PreviousReleaseDeclaringNoWorkloadIdentity_WhenComparing_ShouldBeIncompleteWithThatReason()
    {
        // Given
        _processes.Declarations["previous"] = [(CheckFile, SyntheticBenchmarks.Undeclared)];
        _processes.Declarations["current"] = [(CheckFile, SyntheticBenchmarks.Check), (StartupFile, SyntheticBenchmarks.Startup)];
        Request(new ComparisonReceiptFixture(), ReportFixtures.Pair());

        // When
        var outcome = await Comparison().RunAsync(new RunContextFixture(), "result", TestContext.Current.CancellationToken);

        // Then
        outcome.Should().NotBeNull();
        outcome!.Judgment.Should().Be(ComparisonJudgment.Incomplete);
        outcome.Reason.Should().Contain("Revision benchmarks declare no workload identity.");
        using var bundle = ZipFile.OpenRead(Path.Combine(_root, "result", "comparison-20-1234-2.zip"));
        using var mappings = await JsonDocument.ParseAsync(bundle.GetEntry("identity-mappings.json")!.Open(),
            cancellationToken: TestContext.Current.CancellationToken);
        mappings.RootElement.EnumerateArray().Select(static mapping => mapping.GetProperty("productCommit").GetString())
            .Should().NotBeEmpty().And.OnlyContain(static commit => commit == new string('a', 40));
    }

    public void Dispose()
    {
        _client.Dispose();
        _handler.Dispose();
        if (Directory.Exists(_root)) Directory.Delete(_root, true);
    }

    private ComparisonRun Comparison(Action<ReportingStage>? enter = null) =>
        new(_client, new FileSystem(), _time, new PairedCollector(new FileSystem(), _time, _processes.RunAsync, _root, new Dictionary<string, string>()),
            _root, enter: enter);

    private void Request(ComparisonReceipt receipt, ReleasePair pair) => _artifacts.Add(4, "comparison-request-1234-2", false,
        ActionsArtifacts.Zip(("receipt.json", ComparisonJson.Write(receipt)), ("pair.json", ComparisonJson.Write(pair))));

    private string[] Checkouts() => [.. _processes.Requests.Where(static request => request.Arguments.FirstOrDefault() == "checkout")
        .Select(static request => request.Arguments[^1])];

    private const string CheckFile = ".performance/benchmarks/Check/CheckBenchmarks.cs";
    private const string StartupFile = ".performance/benchmarks/Startup/ColdStartBenchmarks.cs";

    private readonly string _root = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
    private readonly FakeTimeProvider _time = new();
    private readonly ActionsArtifacts _artifacts = new();
    private readonly List<string> _requests = [];
    private readonly ScriptedRevisions _processes;
    private readonly ScriptedHttpHandler _handler;
    private readonly HttpClient _client;
}
