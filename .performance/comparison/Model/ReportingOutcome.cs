using System.Collections.Immutable;
using System.Linq;

namespace Specht.Comparison;

/// <summary>
/// Where release reporting stopped, whether it completed, the judgment and flags computed before it stopped, an actionable reason and
/// the confirmed report reference, never one that was not confirmed.
/// </summary>
public sealed record ReportingOutcome(
    ReportingStage Stage,
    ReportingState State,
    ComparisonJudgment? Judgment,
    ImmutableArray<ReviewFlag> Flags,
    string? Reason,
    ReportReference? Report)
{
    public static ReportingOutcome Failed(ReportingStage stage, string? detail) =>
        new(stage, ReportingState.Incomplete, null, [], Explain(stage, detail), null);

    public static ReportingOutcome Completed(ComparisonDocument document, string? reason) =>
        new(ReportingStage.Evaluation, ReportingState.Completed, document.Judgment,
            [.. document.Cases.SelectMany(static c => c.Metrics.Where(static m => m.Judgment == MetricJudgment.ReviewRequired)
                .Select(m => new ReviewFlag(c.Key, m.Metric, m.Reason ?? "")))],
            reason is null ? null : Sanitise(reason), null);

    public ReportingOutcome Fail(ReportingStage stage, string? detail) =>
        this with { Stage = stage, State = ReportingState.Incomplete, Reason = Explain(stage, detail), Report = null };

    public ReportingOutcome Attached(ReportReference report) => this with { Stage = ReportingStage.Attachment, Report = report };

    private static string Explain(ReportingStage stage, string? detail)
    {
        var cause = stage switch
        {
            ReportingStage.PairPersistence => "The release pair could not be saved on the release, so no comparison was requested.",
            ReportingStage.Receipt => "The comparison request for this run and attempt is missing, unreadable or does not match this publication.",
            ReportingStage.ReceiptWait => "No comparison request for this run and attempt arrived within 30 minutes.",
            ReportingStage.Collection => "The benchmarks of both release revisions could not be collected.",
            ReportingStage.Parsing => "The collected benchmark exports could not be read.",
            ReportingStage.Evaluation => "The collected measurements could not be compared.",
            ReportingStage.Rendering => "The comparison report could not be written.",
            ReportingStage.ResultTransport => "The comparison result for this run and attempt is unreadable or does not match its request.",
            ReportingStage.ResultWait => "No comparison result for this run and attempt arrived within 100 minutes.",
            _ => "The comparison report could not be attached to the release.",
        };
        var remedy = stage == ReportingStage.PairPersistence ? "Re-run all jobs to retry." : "Re-run all jobs to retry with the saved pair.";
        return detail is null ? $"{cause} {remedy}" : $"{cause} {Sanitise(detail)} {remedy}";
    }

    private static string Sanitise(string text) => string.Join(' ', new string([.. text.Select(static c => char.IsControl(c) ? ' ' : c)])
        .Split(' ').Select(static token => ReportBundler.IsRooted(token) || ReportBundler.EmbedsPath(token) ? "[path removed]" : token));
}
