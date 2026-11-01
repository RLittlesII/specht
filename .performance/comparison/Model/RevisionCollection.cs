using System.Collections.Immutable;

namespace Specht.Comparison;

/// <summary>
/// One revision's collection outcome, its repository-relative full-JSON exports, its session evidence, the identity mappings its own
/// benchmarks declare and, when those leave its inventory unknown, why.
/// </summary>
public sealed record RevisionCollection(
    string Side,
    CollectionStatus Status,
    string? Reason,
    ImmutableArray<string> ResultFiles,
    SessionSide Evidence,
    ImmutableArray<IdentityMapping> Mappings,
    string? InventoryReason);
