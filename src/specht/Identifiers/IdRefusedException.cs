namespace Specht.Identifiers;

/// <summary>A request the minter gives no id for. The message names the type and the key, with no absolute path in it.</summary>
/// <param name="message">Why no id is given.</param>
/// <implements epic="0001" feature="F19" claim="B-008" />
public sealed class IdRefusedException(string message) : Exception(message);
