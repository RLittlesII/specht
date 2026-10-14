namespace Specht.Manifest;

/// <summary>A root with no manifest at <see cref="SpecManifestStore.RelativePath"/> (<c>0001-F2</c> B-006).</summary>
/// <param name="retiredManifestPresent">Whether the retired path holds a file, which the message says is not read (B-038).</param>
public sealed class SpechtManifestNotFoundException(bool retiredManifestPresent)
    : Exception(
        retiredManifestPresent
            ? $"{SpecManifestStore.RelativePath}: there is no manifest at the manifest path; "
              + $"the manifest at {SpecManifestStore.RetiredRelativePath} is not read."
            : $"{SpecManifestStore.RelativePath}: there is no manifest at the manifest path.");
