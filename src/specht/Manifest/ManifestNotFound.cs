namespace Specht.Manifest;

/// <summary>A root with no manifest at <see cref="SpecManifest.RelativePath"/> (<c>0001-F2</c> B-006).</summary>
public sealed record ManifestNotFound : InputFailure
{
    /// <summary>Gets what is missing, naming the manifest's relative path.</summary>
    public string Message => $"{SpecManifest.RelativePath}: there is no manifest at the manifest path.";
}
