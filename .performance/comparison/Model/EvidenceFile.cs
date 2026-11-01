using System.Collections.Immutable;

namespace Specht.Comparison;

/// <summary>One bundle-relative file of an evidence manifest, with its digest, role and source linkage.</summary>
public sealed record EvidenceFile(
    string Path,
    string MediaType,
    string Sha256,
    long Length,
    string Role,
    string? Side,
    string? Source,
    string? SourceSha256,
    string Transformation,
    ImmutableArray<string> Cases);
