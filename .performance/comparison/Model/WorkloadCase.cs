using System.Collections.Immutable;

namespace Specht.Comparison;

/// <summary>Immutable WorkloadCase evidence for release comparison.</summary>
public sealed record WorkloadCase(
    string OperationId,
    string OperationRevision,
    string FixtureId,
    string FixtureRevision,
    ImmutableSortedDictionary<string,
    ParameterValue> Parameters,
    string Boundary,
    ImmutableArray<MetricKind> RequiredMetrics);
