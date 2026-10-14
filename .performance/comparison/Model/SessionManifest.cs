namespace Specht.Comparison;

/// <summary>Immutable SessionManifest evidence for release comparison.</summary>
public sealed record SessionManifest(
    int FormatVersion,
    string PairDigest,
    string ComparatorCommit,
    string ComparatorVersion,
    string RunId,
    int Attempt,
    string SessionId,
    string RunnerIdentity,
    SessionSide Previous,
    SessionSide Current);
