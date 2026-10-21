namespace Specht.Manifest;

/// <summary>
/// A manifest the engine rejects (<c>0001-F5</c> B-012). The message names what it rejects, with no absolute path in it.
/// </summary>
/// <param name="message">What is wrong with the manifest, naming the offending key.</param>
/// <implements epic="0001" feature="F5" claim="B-048" />
public sealed class SpechtManifestException(string message) : Exception(message);
