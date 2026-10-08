namespace specht;

/// <summary>A discovered specification file and where its companions live.</summary>
/// <param name="AbsolutePath">Absolute path to the specification file.</param>
/// <param name="RelativePath">Path relative to the repository root.</param>
/// <param name="Layout">Which layout it was found in.</param>
/// <param name="Directory">The directory holding it.</param>
public sealed record SpecLocation(string AbsolutePath, string RelativePath, SpecLayout Layout, string Directory);
