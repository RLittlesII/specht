using System.Collections.Immutable;

namespace Specht.Comparison.Model;

/// <summary>Immutable MeasurementInventory evidence for release comparison.</summary>
public sealed record MeasurementInventory(
    bool Known,
    ImmutableArray<CaseEvidence> Cases,
    ImmutableArray<string> Diagnostics);
