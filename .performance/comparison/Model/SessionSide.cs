using System.Collections.Immutable;

namespace Specht.Comparison;

/// <summary>Immutable SessionSide evidence for release comparison.</summary>
public sealed record SessionSide(
    string ProductCommit,
    string HarnessVersion,
    string ExporterVersion,
    ImmutableArray<string> Invocation,
    MeasurementConditions Conditions,
    bool InventoryKnown,
    ImmutableArray<WorkloadCase> ExpectedCases);
