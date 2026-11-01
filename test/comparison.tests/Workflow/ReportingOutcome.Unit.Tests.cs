using AwesomeAssertions;
using Specht.Comparison.Tests.Reporting;

namespace Specht.Comparison.Tests.Workflow;

[Trait("Tier", "Unit")]
public sealed class ReportingOutcomeUnitTests
{
    [Fact]
    public void ComparisonOwnedStages_WhenListed_ShouldCoverEveryStepAfterPublication()
    {
        // Given
        var expected = new[]
        {
            "PairPersistence", "Receipt", "ReceiptWait", "Collection", "Parsing", "Evaluation", "Rendering", "ResultTransport", "ResultWait",
            "Attachment",
        };

        // When
        var stages = Enum.GetNames<ReportingStage>();

        // Then
        stages.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [ClassData(typeof(ReportingStages))]
    public void FailedStage_WhenMapped_ShouldBeIncompleteWithAnActionableReasonAndNoVerdict(ReportingStage stage)
    {
        // Given
        string? detail = null;

        // When
        var outcome = ReportingOutcome.Failed(stage, detail);

        // Then
        outcome.Stage.Should().Be(stage);
        outcome.State.Should().Be(ReportingState.Incomplete);
        outcome.Judgment.Should().BeNull();
        outcome.Flags.Should().BeEmpty();
        outcome.Report.Should().BeNull();
        outcome.Reason.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void EveryFailedStage_WhenMapped_ShouldNameItsOwnReason()
    {
        // Given
        var stages = Enum.GetValues<ReportingStage>();

        // When
        var reasons = stages.Select(static stage => ReportingOutcome.Failed(stage, null).Reason).ToArray();

        // Then
        reasons.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void FailureDetail_WhenMapped_ShouldBeKeptInTheReason()
    {
        // Given
        const string Detail = "Release tag v1 cannot be resolved to a commit.";

        // When
        var outcome = ReportingOutcome.Failed(ReportingStage.Collection, Detail);

        // Then
        outcome.Reason.Should().Contain(Detail);
    }

    [Fact]
    public void FailureDetailWithARootedPath_WhenMapped_ShouldNotEchoThePath()
    {
        // Given
        var rooted = Path.Combine(Path.GetTempPath(), "runner-workspace", "credentials.json");

        // When
        var outcome = ReportingOutcome.Failed(ReportingStage.ResultTransport, $"Download failed at {rooted}.");

        // Then
        outcome.Reason.Should().NotBeNullOrWhiteSpace().And.NotContain(rooted).And.NotContain(Path.GetTempPath());
    }

    [Fact]
    public void FlaggedRegression_WhenCompleted_ShouldBeACompletedRunListingEveryFlag()
    {
        // Given
        ComparisonDocument document = new ComparisonDocumentFixture();
        var flagged = document.Cases[0];
        document = document with
        {
            Cases = [flagged with { Metrics = [flagged.Metrics[0], flagged.Metrics[1] with
            {
                Judgment = MetricJudgment.ReviewRequired, Reason = "Managed allocation rose more than 10% above a positive previous allocation.",
            }] }, .. document.Cases[1..]],
        };

        // When
        var outcome = ReportingOutcome.Completed(document, null);

        // Then
        outcome.State.Should().Be(ReportingState.Completed);
        outcome.Judgment.Should().Be(ComparisonJudgment.ReviewRequired);
        outcome.Flags.Should().Equal(
            new ReviewFlag(flagged.Key, MetricKind.Time, flagged.Metrics[0].Reason!),
            new ReviewFlag(flagged.Key, MetricKind.ManagedAllocation, "Managed allocation rose more than 10% above a positive previous allocation."));
        outcome.Report.Should().BeNull();
    }

    [Fact]
    public void KnownFlags_WhenALaterStageFails_ShouldSurviveBesideTheIncompleteStateWithoutAReport()
    {
        // Given
        ComparisonDocument document = new ComparisonDocumentFixture();
        var completed = ReportingOutcome.Completed(document, null);

        // When
        var outcome = completed.Fail(ReportingStage.Attachment, null);

        // Then
        outcome.Stage.Should().Be(ReportingStage.Attachment);
        outcome.State.Should().Be(ReportingState.Incomplete);
        outcome.Judgment.Should().Be(ComparisonJudgment.ReviewRequired);
        outcome.Flags.Should().Equal(completed.Flags).And.NotBeEmpty();
        outcome.Reason.Should().Be(ReportingOutcome.Failed(ReportingStage.Attachment, null).Reason);
        outcome.Report.Should().BeNull();
    }

    [Fact]
    public void ConfirmedPublication_WhenAttached_ShouldReferenceTheRetainedReport()
    {
        // Given
        var completed = ReportingOutcome.Completed(new ComparisonDocumentFixture(), null);
        ReportReference report = new ReportReferenceFixture();

        // When
        var outcome = completed.Attached(report);

        // Then
        outcome.Stage.Should().Be(ReportingStage.Attachment);
        outcome.State.Should().Be(ReportingState.Completed);
        outcome.Judgment.Should().Be(ComparisonJudgment.ReviewRequired);
        outcome.Flags.Should().Equal(completed.Flags);
        outcome.Report.Should().Be(report);
    }

    [Fact]
    public void ComparisonWithoutABaseline_WhenCompleted_ShouldCarryNoFlagAndNoRegressionJudgment()
    {
        // Given
        ComparisonDocument document = new ComparisonDocumentFixture().WithJudgment(ComparisonJudgment.NoBaseline).WithCases([]).WithGaps([]);

        // When
        var outcome = ReportingOutcome.Completed(document, null);

        // Then
        outcome.State.Should().Be(ReportingState.Completed);
        outcome.Judgment.Should().Be(ComparisonJudgment.NoBaseline);
        outcome.Flags.Should().BeEmpty();
    }
}
