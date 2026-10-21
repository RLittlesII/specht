namespace Specht.Manifest;

/// <summary>
/// A manifest that is not well-formed JSON or does not parse into the shape the engine reads (<c>0001-F2</c> B-007). The
/// message names <see cref="SpecManifestStore.RelativePath"/>, never the absolute path. A well-formed manifest the engine
/// rejects is a <see cref="SpechtManifestException"/> instead.
/// </summary>
/// <param name="inner">The parser's or the shape's failure.</param>
/// <implements epic="0001" feature="F5" claim="B-048" />
public sealed class SpechtManifestUnreadableException(Exception inner)
    : Exception($"{SpecManifestStore.RelativePath}: the manifest does not parse into the manifest's shape.", inner);
