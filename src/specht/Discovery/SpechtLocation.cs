namespace Specht.Discovery;

/// <summary>A discovered specification file and where its companions live, both relative to the root it was found under.</summary>
/// <param name="RelativePath">Path relative to the repository root, its segments separated by <c>/</c>.</param>
/// <param name="Layout">The manifest layout whose glob matched it.</param>
/// <param name="Directory">The root-relative directory holding it, empty at the root.</param>
/// <implements epic="0001" feature="F6" claim="B-001" />
public sealed record SpechtLocation(string RelativePath, SpechtLayout Layout, string Directory);
