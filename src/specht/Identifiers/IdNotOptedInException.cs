namespace Specht.Identifiers;

/// <summary>
/// A request for a type the manifest's <c>mint</c> key does not opt in, the one refusal the command exits apart from the
/// others (<c>0001-F19</c> B-002): <c>mint</c> holds no entry object for it, or its entry names no strategy (B-047).
/// </summary>
/// <param name="message">Why the type is not opted in.</param>
/// <implements epic="0001" feature="F19" claim="B-001" />
/// <implements epic="0001" feature="F19" claim="B-002" />
/// <implements epic="0001" feature="F19" claim="B-047" />
public sealed class IdNotOptedInException(string message) : IdRefusedException(message);
