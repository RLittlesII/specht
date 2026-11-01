namespace Specht.Comparison;

/// <summary>Immutable normalised values, unrounded deltas and judgment of one paired metric.</summary>
public sealed record MetricComparison(
    MetricKind Metric,
    MetricJudgment Judgment,
    string? Unit,
    decimal? Previous,
    decimal? Current,
    decimal? AbsoluteChange,
    decimal? PercentChange,
    TimingInterval? PreviousInterval,
    TimingInterval? CurrentInterval,
    string? Reason,
    MetricPair Evidence);
