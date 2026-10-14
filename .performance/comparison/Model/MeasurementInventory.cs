using System.Collections.Immutable;

namespace Specht.Comparison;

/// <summary>Immutable MeasurementInventory evidence for release comparison.</summary>
public sealed record MeasurementInventory(
    bool Known,
    ImmutableArray<CaseEvidence> Cases,
    ImmutableArray<string> Diagnostics);
