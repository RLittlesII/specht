namespace Specht.Comparison;

/// <summary>Version 1 proof that this run and attempt published the current release and saved its pair.</summary>
public sealed record ComparisonReceipt(
    int FormatVersion,
    string Repository,
    string RunId,
    int Attempt,
    string Commit,
    long ReleaseId,
    string PairDigest);
