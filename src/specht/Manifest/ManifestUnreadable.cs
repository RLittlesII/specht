namespace Specht.Manifest;

/// <summary>
/// A manifest that is not well-formed JSON or does not parse into the shape the engine reads (<c>0001-F2</c> B-007). The
/// message names <see cref="SpecManifest.RelativePath"/>, never the absolute path. A well-formed manifest the engine
/// rejects is a <see cref="ManifestRejected"/> instead.
/// </summary>
public sealed record ManifestUnreadable : InputFailure
{
    /// <summary>Gets what is wrong, naming the manifest's relative path.</summary>
    public string Message => $"{SpecManifest.RelativePath}: the manifest does not parse into the manifest's shape.";
}
