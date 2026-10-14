using System.Collections.Immutable;

namespace Specht.Comparison;

/// <summary>Immutable PairedCase evidence for release comparison.</summary>
public sealed record PairedCase(
    string Key,
    PairingStatus Status,
    CaseEvidence? Previous,
    CaseEvidence? Current,
    ImmutableArray<MetricPair> Metrics,
    string? Reason);
