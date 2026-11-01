using System.Collections.Immutable;

namespace Specht.Comparison;

/// <summary>Version-1 listing of every other file in a report bundle.</summary>
public sealed record EvidenceManifest(int FormatVersion, ImmutableArray<EvidenceFile> Files);
