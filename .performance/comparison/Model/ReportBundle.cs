namespace Specht.Comparison.Model;

/// <summary>A written per-attempt report bundle, its repository-relative path, digest and manifest.</summary>
public sealed record ReportBundle(string Name, string Path, string Sha256, EvidenceManifest Manifest);
