using System.Collections.Immutable;

namespace Specht.Comparison;

/// <summary>One revision's collection outcome, its repository-relative full-JSON exports and its session evidence.</summary>
public sealed record RevisionCollection(
    string Side,
    CollectionStatus Status,
    string? Reason,
    ImmutableArray<string> ResultFiles,
    SessionSide Evidence);
