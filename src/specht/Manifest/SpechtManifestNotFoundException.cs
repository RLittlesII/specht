namespace Specht.Manifest;

/// <summary>A root with no manifest at <see cref="SpecManifest.RelativePath"/> (<c>0001-F2</c> B-006).</summary>
public sealed class SpechtManifestNotFoundException()
    : Exception($"{SpecManifest.RelativePath}: there is no manifest at the manifest path.");
