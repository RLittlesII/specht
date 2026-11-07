namespace Specht.IdGeneration;

/// <summary>A request the strategy and its decorators cannot answer with an id.</summary>
/// <param name="reason">Why no id is printed.</param>
public sealed class IdRefusedException(string reason) : Exception(reason);
