using System.Collections.Immutable;

namespace Specht.Comparison;

/// <summary>Immutable ReleaseCatalog evidence for release comparison.</summary>
public sealed record ReleaseCatalog(
    ImmutableArray<ReleaseRecord> Releases,
    bool Complete,
    string? Failure);
