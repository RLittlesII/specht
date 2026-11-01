using AwesomeAssertions;
using Specht.Comparison.Tests.Reporting;

namespace Specht.Comparison.Tests.Workflow;

[Trait("Tier", "Unit")]
public sealed class RunSummaryUnitTests
{
    [Fact]
    public void FlaggedOutcomeWithAConfirmedReport_WhenSummarised_ShouldListEveryFlagAndLinkTheRetainedAsset()
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
        var outcome = ReportingOutcome.Completed(document, null).Attached(new ReportReferenceFixture());

        // When
        var summary = RunSummary.Markdown(outcome);

        // Then
        summary.Should().Contain("Review required").And.Contain(flagged.Key).And.Contain(flagged.Metrics[0].Reason!)
            .And.Contain("Managed allocation rose more than 10% above a positive previous allocation.")
            .And.Contain("https://github.com/example/tool/releases/download/v2/comparison-20-1234-2.zip");
        summary.Should().NotContain("Incomplete");
    }

    [Fact]
    public void FlaggedOutcomeWithoutAConfirmedReport_WhenSummarised_ShouldKeepTheFlagsAndPresentNoLink()
    {
        // Given
        ComparisonDocument document = new ComparisonDocumentFixture();
        var outcome = ReportingOutcome.Completed(document, null).Fail(ReportingStage.Attachment, null);

        // When
        var summary = RunSummary.Markdown(outcome);

        // Then
        summary.Should().Contain("Incomplete").And.Contain(outcome.Reason!).And.Contain("Review required")
            .And.Contain(document.Cases[0].Key).And.Contain(document.Cases[0].Metrics[0].Reason!);
        summary.Should().NotContain("releases/download").And.NotContain("comparison-20-1234-2.zip");
    }

    [Theory]
    [ClassData(typeof(ReportingStages))]
    public void FailedStage_WhenSummarised_ShouldStateIncompleteWithItsReasonAndNoVerdict(ReportingStage stage)
    {
        // Given
        var outcome = ReportingOutcome.Failed(stage, null);

        // When
        var summary = RunSummary.Markdown(outcome);

        // Then
        summary.Should().Contain("Incomplete").And.Contain(outcome.Reason!);
        summary.Should().NotContain("No regression flagged").And.NotContain("Review required").And.NotContain("releases/download");
    }

    [Fact]
    public void HostileDisplayText_WhenSummarised_ShouldBeEscapedAsReportTableCellsAre()
    {
        // Given
        ComparisonDocument document = new ComparisonDocumentFixture();
        var flagged = document.Cases[0];
        document = document with
        {
            Cases = [flagged with { Metrics = [flagged.Metrics[0] with { Reason = "Timing | rose <script>`x`</script>\nforged row" }, flagged.Metrics[1]] },
                .. document.Cases[1..]],
        };
        var outcome = ReportingOutcome.Completed(document, null);

        // When
        var summary = RunSummary.Markdown(outcome);

        // Then
        summary.Should().Contain(@"Timing \| rose &lt;script&gt;\`x\`&lt;/script&gt; forged row");
        summary.Should().NotContain("<script>");
        summary.Split('\n').Should().NotContain(static line => line.StartsWith("forged row", StringComparison.Ordinal));
    }

    [Fact]
    public void ComparisonWithoutABaseline_WhenSummarised_ShouldReadNoBaselineWithoutARegressionJudgment()
    {
        // Given
        ComparisonDocument document = new ComparisonDocumentFixture().WithJudgment(ComparisonJudgment.NoBaseline).WithCases([]).WithGaps([]);
        var outcome = ReportingOutcome.Completed(document, null).Attached(new ReportReferenceFixture());

        // When
        var summary = RunSummary.Markdown(outcome);

        // Then
        summary.Should().Contain("No baseline").And.Contain("https://github.com/example/tool/releases/download/v2/comparison-20-1234-2.zip");
        summary.Should().NotContain("No regression flagged").And.NotContain("Review required");
    }

    [Fact]
    public void ComparisonNotYetReported_WhenSummarisedBeforeWaiting_ShouldReadIncompleteComparisonPending()
    {
        // Given
        var pending = RunSummary.Pending;

        // When
        var lines = pending.Split('\n');

        // Then
        lines.Should().Contain(static line => line.Contains("Incomplete: comparison pending", StringComparison.Ordinal));
        pending.Should().NotContain("No regression flagged").And.NotContain("releases/download");
    }
}
