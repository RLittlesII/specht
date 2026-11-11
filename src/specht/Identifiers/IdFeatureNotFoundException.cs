namespace Specht.Identifiers;

/// <summary>A request for a per-Feature type whose Feature folder holds no Feature's <c>.spec/</c> (<c>0001-F19</c> B-059, B-060).</summary>
/// <param name="message">Why no id is given, naming the type and never the folder.</param>
/// <implements epic="0001" feature="F19" claim="B-059" />
/// <implements epic="0001" feature="F19" claim="B-060" />
public sealed class IdFeatureNotFoundException(string message) : IdRefusedException(message);
