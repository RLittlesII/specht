namespace Specht.Comparison;

/// <summary>Immutable ReleasePair evidence for release comparison.</summary>
public sealed record ReleasePair(
    int FormatVersion,
    ReleaseRecord Current,
    ReleaseRecord? Previous,
    string SelectionPolicy,
    string CatalogDigest,
    string SelectingRunId);
