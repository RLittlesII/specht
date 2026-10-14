using System.Collections.Immutable;

namespace Specht.Comparison;

/// <summary>Immutable MetricPair evidence for release comparison.</summary>
public sealed record MetricPair(
    MetricKind Metric,
    PairingStatus Status,
    ImmutableArray<Observation> Previous,
    ImmutableArray<Observation> Current,
    string? Reason);
