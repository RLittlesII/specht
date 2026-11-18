using System.Collections.Immutable;

namespace Specht.Comparison.Model;

/// <summary>Immutable CaseEvidence evidence for release comparison.</summary>
public sealed record CaseEvidence(
    WorkloadCase Case,
    ImmutableArray<Observation> Observations,
    bool IdentityKnown,
    string? Failure);
