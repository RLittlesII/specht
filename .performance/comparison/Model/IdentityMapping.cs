namespace Specht.Comparison;

/// <summary>Immutable IdentityMapping evidence for release comparison.</summary>
public sealed record IdentityMapping(
    string FullName,
    string ProductCommit,
    string SourceFile,
    string SourceDigest,
    WorkloadCase Case);
