using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using AwesomeAssertions;
using Microsoft.Extensions.Time.Testing;
using Specht.Comparison.Tests.Evidence;
using Specht.Comparison.Tests.Reporting;
using Specht.Comparison.Tests.Selection;

namespace Specht.Comparison.Tests.Workflow;

[Trait("Tier", "Integration")]
public sealed class ReleaseReportingIntegrationTests : IDisposable
{
    public ReleaseReportingIntegrationTests()
    {
        Directory.CreateDirectory(_root);
        _catalog["/repos/example/tool/releases?per_page=100&page=1"] = """
            [{"id":20,"tag_name":"v2","draft":false,"prerelease":false,"published_at":"2026-01-04T00:00:00Z"},
             {"id":10,"tag_name":"v1","draft":false,"prerelease":false,"published_at":"2026-01-01T00:00:00Z"}]
            """;
        _catalog["/repos/example/tool/git/ref/tags/v2"] = """{"object":{"type":"commit","sha":"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"}}""";
        _catalog["/repos/example/tool/git/ref/tags/v1"] = """{"object":{"type":"commit","sha":"eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee"}}""";
        _handler = new ScriptedHttpHandler(request =>
        {
            if (_artifacts.Respond(request) is { } artifact)
            {
                return artifact;
            }

            if (request.Method == HttpMethod.Get && request.RequestUri!.Host == "api.github.com"
                && _catalog.TryGetValue(request.RequestUri.PathAndQuery, out var json))
            {
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
            }

            return _storage.Respond(request);
        });
        _client = new HttpClient(_handler);
    }

    [Fact]
    public async Task PublishedRelease_WhenRequestingItsComparison_ShouldSaveThePairAndWriteAReceiptForThisAttempt()
    {
        // When
        var outcome = await Reporting().RequestAsync(new RunContextFixture(), "request", TestContext.Current.CancellationToken);

        // Then
        outcome.Should().BeNull();
        var saved = _storage.Assets.Should().ContainSingle(static asset => asset.Name == "comparison-pair-v1.json").Subject;
        var pair = ComparisonJson.ReadPair(Encoding.UTF8.GetString(saved.Content), ((ReleasePair) ReportFixtures.Pair()).Current);
        pair.Previous!.Id.Should().Be(10);
        pair.Previous.Commit.Should().Be(new string('e', 40));
        (await File.ReadAllBytesAsync(Path.Combine(_root, "request", "pair.json"), TestContext.Current.CancellationToken)).Should().Equal(saved.Content);
        (await File.ReadAllTextAsync(Path.Combine(_root, "request", "receipt.json"), TestContext.Current.CancellationToken))
            .Should().Be(ComparisonJson.Write((ComparisonReceipt) new ComparisonReceiptFixture().WithPairDigest(ComparisonJson.Digest(pair))));
    }

    [Fact]
    public async Task SavedPairWithAnUnresolvedPredecessor_WhenRequestingAgain_ShouldReuseItThoughTheCatalogNowResolves()
    {
        // Given
        ReleasePair saved = ReportFixtures.Pair().WithPrevious(new ReleaseRecordFixture().WithId(10).WithTag("v1").WithCommit(null)
            .WithResolutionFailure("Selected release tag cannot be resolved to an exact commit."));
        _storage.Assets.Add(new ReleaseAsset(50, "comparison-pair-v1.json", "uploaded", Encoding.UTF8.GetBytes(ComparisonJson.Write(saved))));

        // When
        var outcome = await Reporting().RequestAsync(new RunContextFixture(), "request", TestContext.Current.CancellationToken);

        // Then
        outcome.Should().BeNull();
        _storage.Requests.Should().NotContain(static request => request.Method == HttpMethod.Post);
        (await File.ReadAllTextAsync(Path.Combine(_root, "request", "pair.json"), TestContext.Current.CancellationToken))
            .Should().Be(ComparisonJson.Write(saved));
        (await File.ReadAllTextAsync(Path.Combine(_root, "request", "receipt.json"), TestContext.Current.CancellationToken))
            .Should().Be(ComparisonJson.Write((ComparisonReceipt) new ComparisonReceiptFixture().WithPairDigest(ComparisonJson.Digest(saved))));
    }

    [Fact]
    public async Task PairThatCannotBeSaved_WhenRequesting_ShouldBeIncompleteWithoutAReceipt()
    {
        // Given
        _storage.UploadStatus = HttpStatusCode.InternalServerError;

        // When
        var outcome = await Reporting().RequestAsync(new RunContextFixture(), "request", TestContext.Current.CancellationToken);

        // Then
        outcome.Should().NotBeNull();
        outcome!.Stage.Should().Be(ReportingStage.PairPersistence);
        outcome.State.Should().Be(ReportingState.Incomplete);
        outcome.Judgment.Should().BeNull();
        File.Exists(Path.Combine(_root, "request", "receipt.json")).Should().BeFalse();
    }

    [Fact]
    public async Task ConfirmedResultForThisAttempt_WhenAttaching_ShouldPublishTheBundleAndReferenceIt()
    {
        // Given
        Requested();
        var bundle = Bundle(2, ReportFixtures.Pair(), out var document);
        Result("comparison-result-1234-2", "comparison-20-1234-2.zip", bundle, document);

        // When
        var outcome = await Reporting().AttachAsync(new RunContextFixture(), "request", TestContext.Current.CancellationToken);

        // Then
        var retained = _storage.Assets.Should().ContainSingle(static asset => asset.Name == "comparison-20-1234-2.zip").Subject;
        retained.Content.Should().Equal(bundle);
        outcome!.State.Should().Be(ReportingState.Completed);
        outcome.Stage.Should().Be(ReportingStage.Attachment);
        outcome.Judgment.Should().Be(ComparisonJudgment.ReviewRequired);
        outcome.Flags.Should().Equal(ReportingOutcome.Completed(document, null).Flags);
        outcome.Report.Should().Be(new ReportReference(((ReleasePair) ReportFixtures.Pair()).Current,
            new PublishedBundle(retained.Id, "comparison-20-1234-2.zip", Convert.ToHexStringLower(SHA256.HashData(bundle)))));
    }

    [Fact]
    public async Task PublishedRelease_WhenRequestingItsComparison_ShouldEnterPairPersistenceThenTheReceipt()
    {
        // Given
        List<ReportingStage> stages = [];

        // When
        await Reporting(stages.Add).RequestAsync(new RunContextFixture(), "request", TestContext.Current.CancellationToken);

        // Then
        stages.Should().Equal(ReportingStage.PairPersistence, ReportingStage.Receipt);
    }

    [Fact]
    public async Task ConfirmedResultForThisAttempt_WhenAttaching_ShouldEnterEachStageInOrder()
    {
        // Given
        Requested();
        Result("comparison-result-1234-2", "comparison-20-1234-2.zip", Bundle(2, ReportFixtures.Pair(), out var document), document);
        List<ReportingStage> stages = [];

        // When
        await Reporting(stages.Add).AttachAsync(new RunContextFixture(), "request", TestContext.Current.CancellationToken);

        // Then
        stages.Should().Equal(ReportingStage.Receipt, ReportingStage.ResultWait, ReportingStage.ResultTransport, ReportingStage.Attachment);
    }

    [Fact]
    public async Task ResultThatCannotBeAttached_WhenAttaching_ShouldKeepItsFlagsBesideTheIncompleteStateWithoutAReport()
    {
        // Given
        Requested();
        _storage.UploadStatus = HttpStatusCode.InternalServerError;
        Result("comparison-result-1234-2", "comparison-20-1234-2.zip", Bundle(2, ReportFixtures.Pair(), out var document), document);

        // When
        var outcome = await Reporting().AttachAsync(new RunContextFixture(), "request", TestContext.Current.CancellationToken);

        // Then
        outcome!.Stage.Should().Be(ReportingStage.Attachment);
        outcome.State.Should().Be(ReportingState.Incomplete);
        outcome.Judgment.Should().Be(ComparisonJudgment.ReviewRequired);
        outcome.Flags.Should().NotBeEmpty().And.Equal(ReportingOutcome.Completed(document, null).Flags);
        outcome.Report.Should().BeNull();
    }

    [Fact(Timeout = 20_000)]
    public async Task NoResultForThisAttempt_WhenTheHundredMinuteWaitPasses_ShouldBeIncompleteWithoutPublishing()
    {
        // Given
        Requested();
        Result("comparison-result-1234-1", "comparison-20-1234-1.zip", Bundle(1, ReportFixtures.Pair(), out var document), document);
        var attaching = Reporting().AttachAsync(new RunContextFixture(), "request", TestContext.Current.CancellationToken);
        await _artifacts.Listed.Task;

        // When
        _time.Advance(TimeSpan.FromMinutes(100) - TimeSpan.FromSeconds(1));
        var beforeTheBound = attaching.IsCompleted;
        _time.Advance(TimeSpan.FromSeconds(1));
        var outcome = await attaching;

        // Then
        beforeTheBound.Should().BeFalse();
        outcome!.Stage.Should().Be(ReportingStage.ResultWait);
        outcome.State.Should().Be(ReportingState.Incomplete);
        outcome.Judgment.Should().BeNull();
        _storage.Requests.Should().NotContain(static request => request.Method == HttpMethod.Post);
    }

    [Theory]
    [ClassData(typeof(ForeignBundles))]
    public async Task ResultCarryingAnotherRequestsBundle_WhenAttaching_ShouldBeIncompleteWithoutPublishing(int attempt, ReleasePair bundled)
    {
        // Given
        Requested();
        var bundle = Bundle(attempt, bundled, out var document);
        Result("comparison-result-1234-2", $"comparison-20-1234-{attempt}.zip", bundle, document);

        // When
        var outcome = await Reporting().AttachAsync(new RunContextFixture(), "request", TestContext.Current.CancellationToken);

        // Then
        outcome!.Stage.Should().Be(ReportingStage.ResultTransport);
        outcome.State.Should().Be(ReportingState.Incomplete);
        outcome.Report.Should().BeNull();
        _storage.Requests.Should().NotContain(static request => request.Method == HttpMethod.Post);
    }

    [Theory]
    [ClassData(typeof(RunsPublishingNoRelease))]
    public async Task RunThatPublishesNoRelease_WhenAttaching_ShouldDoNothingAndLeaveNoSummary(string eventName, string reference)
    {
        // Given
        Requested();
        Result("comparison-result-1234-2", "comparison-20-1234-2.zip", Bundle(2, ReportFixtures.Pair(), out var document), document);

        // When
        var outcome = await Reporting().AttachAsync(new RunContextFixture().WithEventName(eventName).WithRef(reference), "request",
            TestContext.Current.CancellationToken);

        // Then
        outcome.Should().BeNull();
        _artifacts.Requests.Should().BeEmpty();
        _storage.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task ResultWhoseOutcomeFlagsDisagreeWithItsReport_WhenAttaching_ShouldBeIncompleteWithoutPublishing()
    {
        // Given
        Requested();
        var bundle = Bundle(2, ReportFixtures.Pair(), out var document);
        var unflagged = ReportingOutcome.Completed(document, null) with { Flags = [] };
        _artifacts.Add(9, "comparison-result-1234-2", false, ActionsArtifacts.Zip(("outcome.json", Encoding.UTF8.GetBytes(ComparisonJson.Write(unflagged))),
            ("comparison-20-1234-2.zip", bundle)));

        // When
        var outcome = await Reporting().AttachAsync(new RunContextFixture(), "request", TestContext.Current.CancellationToken);

        // Then
        outcome!.Stage.Should().Be(ReportingStage.ResultTransport);
        outcome.State.Should().Be(ReportingState.Incomplete);
        outcome.Flags.Should().BeEmpty();
        outcome.Report.Should().BeNull();
        _storage.Requests.Should().NotContain(static request => request.Method == HttpMethod.Post);
    }

    public void Dispose()
    {
        _client.Dispose();
        _handler.Dispose();
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, true);
        }
    }

    public sealed class ForeignBundles : TheoryData<int, ReleasePair>
    {
        public ForeignBundles()
        {
            Add(1, ReportFixtures.Pair());
            Add(2, ReportFixtures.Pair().WithPrevious(new ReleaseRecordFixture().WithId(11).WithTag("v1.1").WithCommit(new string('c', 40))
                .WithPublishedAt(new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero))));
        }
    }

    private static byte[] Bundle(int attempt, ReleasePair pair, out ComparisonDocument document)
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            [Path.Combine("repo", ComparisonDocumentFixture.PreviousExport)] = new(ExportData.FullJson),
            [Path.Combine("repo", ComparisonDocumentFixture.CurrentExport)] = new(ExportData.CurrentJson),
        });
        document = new ComparisonDocumentFixture().WithSession(new SessionManifestFixture().WithPairDigest(ComparisonJson.Digest(pair))
            .WithRunId("1234").WithAttempt(attempt));
        RawExport previous = new RawExportFixture().WithSide("previous").WithSourcePath(ComparisonDocumentFixture.PreviousExport);
        var bundle = new ReportBundler(fileSystem, "repo").Create(pair, document, [ReportFixtures.Mapping()], [previous, new RawExportFixture()],
            ".artifacts/benchmarks/comparison");
        return fileSystem.File.ReadAllBytes(Path.Combine("repo", bundle.Path));
    }

    private ReleaseReporting Reporting(Action<ReportingStage>? enter = null) => new(_client, new FileSystem(), _time, _root, enter);

    private void Requested()
    {
        Directory.CreateDirectory(Path.Combine(_root, "request"));
        File.WriteAllText(Path.Combine(_root, "request", "pair.json"), ComparisonJson.Write((ReleasePair) ReportFixtures.Pair()));
        File.WriteAllText(Path.Combine(_root, "request", "receipt.json"), ComparisonJson.Write((ComparisonReceipt) new ComparisonReceiptFixture()));
    }

    private void Result(string artifact, string name, byte[] bundle, ComparisonDocument document) => _artifacts.Add(9, artifact, false,
        ActionsArtifacts.Zip(("outcome.json", Encoding.UTF8.GetBytes(ComparisonJson.Write(ReportingOutcome.Completed(document, null)))), (name, bundle)));

    private readonly string _root = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
    private readonly FakeTimeProvider _time = new();
    private readonly ActionsArtifacts _artifacts = new();
    private readonly ReleaseAssetStorage _storage = new();
    private readonly Dictionary<string, string> _catalog = new(StringComparer.Ordinal);
    private readonly ScriptedHttpHandler _handler;
    private readonly HttpClient _client;
}
