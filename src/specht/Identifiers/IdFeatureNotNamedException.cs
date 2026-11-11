namespace Specht.Identifiers;

/// <summary>A request for a per-Feature type the manifest opts in that names no Feature (<c>0001-F19</c> B-057, B-058).</summary>
/// <param name="message">Why no id is given.</param>
/// <implements epic="0001" feature="F19" claim="B-057" />
/// <implements epic="0001" feature="F19" claim="B-058" />
public sealed class IdFeatureNotNamedException(string message) : IdRefusedException(message);
