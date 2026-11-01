using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using Specht.Comparison.Tests.Reporting;

namespace Specht.Comparison.Tests.Workflow;

[Trait("Tier", "Unit")]
public sealed class AdvisoryStepUnitTests
{
    [Fact]
    public async Task FlaggedRegression_WhenTheStepCompletes_ShouldExitZeroWithItsSummary()
    {
        // Given
        var fileSystem = new MockFileSystem();
        var outcome = ReportingOutcome.Completed(new ComparisonDocumentFixture(), null).Attached(new ReportReferenceFixture());

        // When
        var exitCode = await AdvisoryStep.RunAsync(_ => Task.FromResult<ReportingOutcome?>(outcome), ReportingStage.Attachment, fileSystem,
            "summary.md", TestContext.Current.CancellationToken);

        // Then
        exitCode.Should().Be(0);
        outcome.Judgment.Should().Be(ComparisonJudgment.ReviewRequired);
        (await fileSystem.File.ReadAllTextAsync("summary.md", TestContext.Current.CancellationToken)).Should().Be(RunSummary.Markdown(outcome));
    }

    [Theory]
    [ClassData(typeof(StepFailures))]
    public async Task StepThatFails_WhenRun_ShouldExitZeroWithAnIncompleteSummaryForItsStage(ReportingStage stage, Exception failure)
    {
        // Given
        var fileSystem = new MockFileSystem();

        // When
        var exitCode = await AdvisoryStep.RunAsync(_ => Task.FromException<ReportingOutcome?>(failure), stage, fileSystem, "summary.md",
            TestContext.Current.CancellationToken);

        // Then
        exitCode.Should().Be(0);
        var summary = await fileSystem.File.ReadAllTextAsync("summary.md", TestContext.Current.CancellationToken);
        summary.Should().Be(RunSummary.Markdown(ReportingOutcome.Failed(stage, null)));
        summary.Should().NotContain("secret-detail");
    }

    [Fact]
    public async Task StepStillWaiting_WhenItsSummaryIsRead_ShouldShowThePendingSummaryUntilTheOutcomeReplacesIt()
    {
        // Given
        var fileSystem = new MockFileSystem();
        var outcome = ReportingOutcome.Failed(ReportingStage.ResultWait, null);
        string? whileWaiting = null;

        // When
        var exitCode = await AdvisoryStep.RunAsync(async cancellationToken =>
        {
            whileWaiting = await fileSystem.File.ReadAllTextAsync("summary.md", cancellationToken);
            return outcome;
        }, ReportingStage.ResultWait, fileSystem, "summary.md", TestContext.Current.CancellationToken);

        // Then
        exitCode.Should().Be(0);
        whileWaiting.Should().Be(RunSummary.Pending);
        var summary = await fileSystem.File.ReadAllTextAsync("summary.md", TestContext.Current.CancellationToken);
        summary.Should().Be(RunSummary.Markdown(outcome)).And.NotContain("comparison pending");
    }

    public sealed class StepFailures : TheoryData<ReportingStage, Exception>
    {
        public StepFailures()
        {
            Add(ReportingStage.PairPersistence, new HttpRequestException("secret-detail: the release store refused the pair"));
            Add(ReportingStage.Collection, new IOException("secret-detail: the disk is full"));
            Add(ReportingStage.ResultTransport, new InvalidDataException("secret-detail: the result archive is malformed"));
            Add(ReportingStage.Attachment, new TaskCanceledException("secret-detail: the upload timed out"));
        }
    }
}
