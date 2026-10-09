namespace specht;

/// <summary>
/// A manifest that is not well-formed JSON or does not parse into the shape the engine reads (<c>0001-F2</c> B-007). The
/// message names <see cref="SpecManifest.RelativePath"/>, never the absolute path. A well-formed manifest the engine
/// rejects is a <see cref="SpechtManifestException"/> instead.
/// </summary>
/// <param name="inner">The parser's or the shape's failure.</param>
public sealed class SpechtManifestUnreadableException(Exception inner)
    : Exception($"{SpecManifest.RelativePath}: the manifest does not parse into the manifest's shape.", inner);
