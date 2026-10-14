using System;

namespace Specht.Comparison;

/// <summary>Immutable ReleaseRecord evidence for release comparison.</summary>
public sealed record ReleaseRecord(
    string Repository,
    long Id,
    string Tag,
    DateTimeOffset? PublishedAt,
    bool Draft,
    bool Prerelease,
    string? Commit,
    string? ResolutionFailure);
