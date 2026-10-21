namespace Specht;

/// <summary>
/// A root that is not a directory (<c>0001-F2</c> B-005). It carries no path: the engine holds only the resolved root, and
/// the message names the root as it was typed, which only the caller has.
/// </summary>
/// <implements epic="0001" feature="F1" claim="B-001" />
public sealed class SpechtRootNotFoundException() : Exception("The root is not a directory.");
