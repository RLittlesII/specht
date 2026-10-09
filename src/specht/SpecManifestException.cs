namespace specht;

/// <summary>
/// A manifest the engine rejects (<c>0001-F5</c> B-012). The message names what it rejects, with no absolute path in it.
/// </summary>
/// <param name="message">What is wrong with the manifest, naming the offending key.</param>
public sealed class SpecManifestException(string message) : Exception(message);
