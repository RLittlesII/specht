namespace Specht.Manifest;

/// <summary>A root with no manifest at <see cref="SpecManifest.RelativePath"/> (<c>0001-F2</c> B-006).</summary>
/// <param name="retiredManifestPresent">A file is at <see cref="SpecManifest.RetiredRelativePath"/>, which the message then says is not read (B-038).</param>
public sealed class SpechtManifestNotFoundException(bool retiredManifestPresent)
    : Exception(
        retiredManifestPresent
            ? $"{SpecManifest.RelativePath}: there is no manifest at the manifest path; the manifest at {SpecManifest.RetiredRelativePath} is not read."
            : $"{SpecManifest.RelativePath}: there is no manifest at the manifest path.");
