namespace Specht.Comparison;

/// <summary>A case or required metric that cannot count as evidence of no regression.</summary>
public sealed record CoverageGap(
    string CaseKey,
    MetricKind? Metric,
    string Reason);
