namespace Specht.Model;

/// <summary>
/// One piece of claim input the check could not take claims from, found where the model drops it: the root-relative path
/// it is reported at, its line or <c>0</c> for the whole path, why, and the identifier the violation names, if any
/// (<c>0001-F16</c> B-014 to B-016, B-018, B-019, B-033, B-034).
/// </summary>
/// <param name="Path">The root-relative path the fault is reported at.</param>
/// <param name="Line">The one-based line, or <c>0</c>.</param>
/// <param name="Kind">Why the check could not take claims from the input.</param>
/// <param name="Identifier">The identifier the violation names, or null.</param>
/// <implements epic="0001" feature="F16" claim="B-014" />
/// <implements epic="0001" feature="F16" claim="B-015" />
/// <implements epic="0001" feature="F16" claim="B-016" />
/// <implements epic="0001" feature="F16" claim="B-018" />
/// <implements epic="0001" feature="F16" claim="B-019" />
/// <implements epic="0001" feature="F16" claim="B-033" />
/// <implements epic="0001" feature="F16" claim="B-034" />
public sealed record SpechtClaimFault(string Path, int Line, SpechtClaimFaultKind Kind, string? Identifier);
