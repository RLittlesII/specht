using System.Collections.Immutable;

namespace Specht.Comparison;

/// <summary>Immutable metric comparisons of one paired workload case.</summary>
public sealed record CaseComparison(
    string Key,
    PairingStatus Status,
    ImmutableArray<MetricComparison> Metrics,
    string? Reason,
    PairedCase Evidence);
