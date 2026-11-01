using System.IO.Abstractions;
using System.Net;
using AwesomeAssertions;
using Microsoft.Extensions.Time.Testing;
using Specht.Comparison.Tests.Selection;

namespace Specht.Comparison.Tests.Workflow;

[Trait("Tier", "Integration")]
public sealed class RunArtifactsIntegrationTests : IDisposable
{
    public RunArtifactsIntegrationTests()
    {
        Directory.CreateDirectory(_root);
        _handler = new ScriptedHttpHandler(request => Fault(request) ?? _artifacts.Respond(request) ?? new HttpResponseMessage(HttpStatusCode.NotFound));
        _client = new HttpClient(_handler);
    }

    [Fact]
    public async Task ExactArtifactOnALaterPage_WhenWaiting_ShouldDownloadAndUnpackOnlyIt()
    {
        // Given
        _artifacts.Add(1, "comparison-request-1234-1", false, ActionsArtifacts.Zip(("receipt.json", "old attempt")));
        _artifacts.Add(2, "comparison-result-1234-2", false, ActionsArtifacts.Zip(("outcome.json", "result")));
        _artifacts.Add(3, "comparison-request-12345-2", false, ActionsArtifacts.Zip(("receipt.json", "other run")));
        _artifacts.Add(4, "comparison-request-1234-2", false, ActionsArtifacts.Zip(("receipt.json", "this attempt"), ("pair.json", "pair")));

        // When
        var found = await Waiter().WaitAsync("example/tool", "1234", "comparison-request-1234-2", "request", TimeSpan.FromMinutes(5),
            TestContext.Current.CancellationToken);

        // Then
        found.Should().BeTrue();
        Directory.GetFiles(Path.Combine(_root, "request")).Select(Path.GetFileName).Should().BeEquivalentTo("receipt.json", "pair.json");
        (await File.ReadAllTextAsync(Path.Combine(_root, "request", "receipt.json"), TestContext.Current.CancellationToken)).Should().Be("this attempt");
        _artifacts.Requests.Where(static request => request.EndsWith("/zip", StringComparison.Ordinal))
            .Should().Equal("/repos/example/tool/actions/artifacts/4/zip");
    }

    [Fact(Timeout = 20_000)]
    public async Task ArtifactUploadedWhileWaiting_WhenPolled_ShouldBeDownloadedBeforeTheBound()
    {
        // Given
        var waiting = Waiter().WaitAsync("example/tool", "1234", "comparison-result-1234-2", "result", TimeSpan.FromMinutes(5),
            TestContext.Current.CancellationToken);
        await _artifacts.Listed.Task;
        _artifacts.Add(7, "comparison-result-1234-2", false, ActionsArtifacts.Zip(("outcome.json", "result")));

        // When
        _time.Advance(TimeSpan.FromMinutes(5) - TimeSpan.FromSeconds(1));
        var found = await waiting;

        // Then
        found.Should().BeTrue();
        File.Exists(Path.Combine(_root, "result", "outcome.json")).Should().BeTrue();
    }

    [Fact(Timeout = 20_000)]
    public async Task OnlyAnotherAttemptsOrAnExpiredArtifact_WhenTheBoundPasses_ShouldStopWaitingWithoutDownloading()
    {
        // Given
        _artifacts.Add(1, "comparison-request-1234-1", false, ActionsArtifacts.Zip(("receipt.json", "old attempt")));
        _artifacts.Add(2, "comparison-request-1234-2", true, ActionsArtifacts.Zip(("receipt.json", "expired")));
        var waiting = Waiter().WaitAsync("example/tool", "1234", "comparison-request-1234-2", "request", TimeSpan.FromMinutes(5),
            TestContext.Current.CancellationToken);
        await _artifacts.Listed.Task;

        // When
        _time.Advance(TimeSpan.FromMinutes(5) - TimeSpan.FromSeconds(1));
        var beforeTheBound = waiting.IsCompleted;
        _time.Advance(TimeSpan.FromSeconds(1));
        var found = await waiting;

        // Then
        beforeTheBound.Should().BeFalse();
        found.Should().BeFalse();
        _artifacts.Requests.Should().NotContain(static request => request.EndsWith("/zip", StringComparison.Ordinal));
        Directory.Exists(Path.Combine(_root, "request")).Should().BeFalse();
    }

    [Fact]
    public async Task ArtifactWithAnEntryOutsideItsDirectory_WhenUnpacking_ShouldBeRejectedWithoutWritingIt()
    {
        // Given
        _artifacts.Add(4, "comparison-request-1234-2", false, ActionsArtifacts.Zip(("../escaped.json", "outside"), ("receipt.json", "receipt")));

        // When
        var waiting = () => Waiter().WaitAsync("example/tool", "1234", "comparison-request-1234-2", "request", TimeSpan.FromMinutes(5),
            TestContext.Current.CancellationToken);

        // Then
        await waiting.Should().ThrowAsync<InvalidDataException>();
        File.Exists(Path.Combine(_root, "escaped.json")).Should().BeFalse();
    }

    [Theory(Timeout = 20_000)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(null)]
    public async Task ListingThatFailsTransientlyOnce_WhenTheNextPollSucceeds_ShouldDownloadTheArtifact(HttpStatusCode? status)
    {
        // Given
        _artifacts.Add(7, "comparison-result-1234-2", false, ActionsArtifacts.Zip(("outcome.json", "result")));
        _faults = 1;
        _status = status;
        var waiting = Waiter().WaitAsync("example/tool", "1234", "comparison-result-1234-2", "result", TimeSpan.FromMinutes(5),
            TestContext.Current.CancellationToken);
        await _faulted.Task;

        // When
        _time.Advance(TimeSpan.FromSeconds(30));
        var found = await waiting;

        // Then
        found.Should().BeTrue();
        File.Exists(Path.Combine(_root, "result", "outcome.json")).Should().BeTrue();
    }

    [Fact(Timeout = 20_000)]
    public async Task ListingThatKeepsFailingTransiently_WhenTheBoundPasses_ShouldStopWaitingWithoutDownloading()
    {
        // Given
        _artifacts.Add(7, "comparison-result-1234-2", false, ActionsArtifacts.Zip(("outcome.json", "result")));
        _faults = int.MaxValue;
        _status = HttpStatusCode.BadGateway;
        var waiting = Waiter().WaitAsync("example/tool", "1234", "comparison-result-1234-2", "result", TimeSpan.FromMinutes(5),
            TestContext.Current.CancellationToken);
        await _faulted.Task;

        // When
        _time.Advance(TimeSpan.FromMinutes(5));
        var found = await waiting;

        // Then
        found.Should().BeFalse();
        _artifacts.Requests.Should().BeEmpty();
        Directory.Exists(Path.Combine(_root, "result")).Should().BeFalse();
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.NotFound)]
    public async Task ListingThatIsRefused_WhenWaiting_ShouldFailWithoutWaitingForTheBound(HttpStatusCode status)
    {
        // Given
        _faults = int.MaxValue;
        _status = status;

        // When
        var waiting = () => Waiter().WaitAsync("example/tool", "1234", "comparison-result-1234-2", "result", TimeSpan.FromMinutes(5),
            TestContext.Current.CancellationToken);

        // Then
        (await waiting.Should().ThrowAsync<HttpRequestException>()).Which.StatusCode.Should().Be(status);
    }

    public void Dispose()
    {
        _client.Dispose();
        _handler.Dispose();
        if (Directory.Exists(_root)) Directory.Delete(_root, true);
    }

    private RunArtifacts Waiter() => new(_client, new FileSystem(), _time, _root);

    private HttpResponseMessage? Fault(HttpRequestMessage request)
    {
        if (!request.RequestUri!.AbsolutePath.EndsWith("/artifacts", StringComparison.Ordinal) || Interlocked.Decrement(ref _faults) < 0) return null;
        _faulted.TrySetResult();
        return _status is { } status ? new HttpResponseMessage(status) : throw new HttpRequestException("The connection was reset.");
    }

    private readonly string _root = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
    private readonly FakeTimeProvider _time = new();
    private readonly ActionsArtifacts _artifacts = new();
    private readonly TaskCompletionSource _faulted = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly ScriptedHttpHandler _handler;
    private readonly HttpClient _client;
    private int _faults;
    private HttpStatusCode? _status;
}
