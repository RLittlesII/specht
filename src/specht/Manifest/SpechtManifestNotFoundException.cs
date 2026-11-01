namespace Specht.Manifest;

/// <summary>A root with no manifest at <see cref="SpechtManifestStore.RelativePath"/> (<c>0001-F2</c> B-006).</summary>
/// <param name="retiredManifestPresent">Whether the retired path holds a file, which the message says is not read (B-038).</param>
/// <implements epic="0001" feature="F5" claim="B-048" />
public sealed class SpechtManifestNotFoundException(bool retiredManifestPresent)
    : Exception(
        retiredManifestPresent
            ? $"{SpechtManifestStore.RelativePath}: there is no manifest at the manifest path; "
              + $"the manifest at {SpechtManifestStore.RetiredRelativePath} is not read."
            : $"{SpechtManifestStore.RelativePath}: there is no manifest at the manifest path.");
