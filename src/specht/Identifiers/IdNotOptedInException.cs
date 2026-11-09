namespace Specht.Identifiers;

/// <summary>
/// A request for a type the manifest's <c>mint</c> key does not name, the one refusal the command exits apart from the others
/// (<c>0001-F19</c> B-002).
/// </summary>
/// <param name="type">The type requested.</param>
/// <implements epic="0001" feature="F19" claim="B-001" />
/// <implements epic="0001" feature="F19" claim="B-002" />
public sealed class IdNotOptedInException(string type) : IdRefusedException($"The type '{type}' is not opted in under 'mint'.");
