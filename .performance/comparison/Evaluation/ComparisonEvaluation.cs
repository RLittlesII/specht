using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;

namespace Specht.Comparison;

/// <summary>Judges paired evidence against the strict review screens and reduces the overall judgment.</summary>
public static class ComparisonEvaluation
{
    public static ComparisonDocument Evaluate(SessionManifest session, ImmutableArray<PairedCase> cases)
    {
        var policy = ComparisonPolicy.Initial;
        var comparisons = cases.Select(paired => new CaseComparison(paired.Key, paired.Status,
            paired.Metrics.Select(metric => Compare(metric, policy)).ToImmutableArray(), paired.Reason, paired)).ToImmutableArray();
        var gaps = comparisons.SelectMany(Gaps).ToImmutableArray();
        var judgment = comparisons.Any(static c => c.Metrics.Any(static m => m.Judgment == MetricJudgment.ReviewRequired))
            ? ComparisonJudgment.ReviewRequired
            : comparisons.IsEmpty || !gaps.IsEmpty ? ComparisonJudgment.Incomplete : ComparisonJudgment.NoRegressionFlagged;
        return new ComparisonDocument(1, session, policy, comparisons, judgment, gaps);
    }

    private static IEnumerable<CoverageGap> Gaps(CaseComparison comparison)
    {
        if (!comparison.Metrics.Any(static m => m.Metric is MetricKind.Time or MetricKind.ManagedAllocation))
            return [new CoverageGap(comparison.Key, null, comparison.Reason
                ?? comparison.Evidence.Previous?.Failure ?? comparison.Evidence.Current?.Failure ?? comparison.Status switch
                {
                    PairingStatus.Added => "Workload is only in the current release; it has no regression judgment.",
                    PairingStatus.Removed => "Workload is only in the previous release; it has no regression judgment.",
                    _ => "Workload case has no timing or managed-allocation result to judge.",
                })];
        return comparison.Metrics
            .Where(static m => m.Judgment is MetricJudgment.InsufficientEvidence or MetricJudgment.NotComparable)
            .Select(m => new CoverageGap(comparison.Key, m.Metric, m.Reason!));
    }

    private static MetricComparison Compare(MetricPair pair, ComparisonPolicy policy)
    {
        var informational = pair.Metric == MetricKind.ToolPeakMemory;
        if (pair.Status != PairingStatus.Comparable)
            return Unmeasured(pair, informational ? MetricJudgment.Informational
                : pair.Status == PairingStatus.NotComparable ? MetricJudgment.NotComparable : MetricJudgment.InsufficientEvidence, pair.Reason);
        var before = pair.Previous[0];
        var after = pair.Current[0];
        try
        {
            var beforeScale = MetricUnits.Scale(pair.Metric, before.Unit)!.Value;
            var afterScale = MetricUnits.Scale(pair.Metric, after.Unit)!.Value;
            var previous = before.Value!.Value * beforeScale;
            var current = after.Value!.Value * afterScale;
            var previousInterval = Scale(before.Interval, beforeScale);
            var currentInterval = Scale(after.Interval, afterScale);
            var change = current - previous;
            decimal? percent = previous > 0 ? change * 100m / previous : null;
            var (judgment, reason) = pair.Metric switch
            {
                MetricKind.Time => percent > policy.TimingIncreasePercent && currentInterval!.Lower > previousInterval!.Upper
                    ? (MetricJudgment.ReviewRequired, Invariant($"Mean time rose more than {policy.TimingIncreasePercent}% ")
                        + "and the current confidence interval lies strictly above the previous interval.")
                    : (MetricJudgment.NoRegressionFlagged, null),
                MetricKind.ManagedAllocation when previous == 0 => current == 0 ? (MetricJudgment.NoRegressionFlagged, null)
                    : (MetricJudgment.InsufficientEvidence, "Managed allocation rose from zero bytes; its percentage change is not defined."),
                MetricKind.ManagedAllocation => percent > policy.AllocationIncreasePercent
                    ? (MetricJudgment.ReviewRequired, Invariant(
                        $"Managed allocation rose more than {policy.AllocationIncreasePercent}% above a positive previous allocation."))
                    : (MetricJudgment.NoRegressionFlagged, (string?)null),
                _ => (MetricJudgment.Informational, null),
            };
            return new MetricComparison(pair.Metric, judgment, pair.Metric == MetricKind.Time ? "ns" : "B", previous, current, change, percent,
                previousInterval, currentInterval, reason, pair);
        }
        catch (OverflowException)
        {
            return Unmeasured(pair, informational ? MetricJudgment.Informational : MetricJudgment.InsufficientEvidence,
                "Metric value overflows its normalised unit or change arithmetic.");
        }
    }

    private static MetricComparison Unmeasured(MetricPair pair, MetricJudgment judgment, string? reason) =>
        new(pair.Metric, judgment, null, null, null, null, null, null, null, reason, pair);

    private static TimingInterval? Scale(TimingInterval? interval, decimal scale) =>
        interval is null ? null : interval with { Lower = interval.Lower * scale, Upper = interval.Upper * scale };

    private static string Invariant(FormattableString text) => text.ToString(CultureInfo.InvariantCulture);
}
