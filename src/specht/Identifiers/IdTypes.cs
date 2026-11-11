namespace Specht.Identifiers;

/// <summary>
/// The two scopes ADR-0033 gives the first slice's types (<c>0001-F19</c> C-13): repository-wide ids, numbered once for the
/// tree, and per-Feature ids, numbered within the Feature <c>--feature</c> names (A-7).
/// </summary>
/// <implements epic="0001" feature="F19" claim="B-016" />
/// <implements epic="0001" feature="F19" claim="B-020" />
/// <implements epic="0001" feature="F19" claim="B-021" />
public static class IdTypes
{
    /// <summary>Gets the repository-wide types, <c>adr</c> and <c>lesson</c>.</summary>
    /// <implements epic="0001" feature="F19" claim="B-016" />
    /// <implements epic="0001" feature="F19" claim="B-020" />
    public static IReadOnlyList<string> RepositoryWide { get; } = ["adr", "lesson"];

    /// <summary>Gets the per-Feature types, <c>decision</c>, <c>claim</c>, <c>constraint</c>, <c>openQuestion</c> and <c>scenario</c>.</summary>
    /// <implements epic="0001" feature="F19" claim="B-021" />
    /// <implements epic="0001" feature="F19" claim="B-058" />
    public static IReadOnlyList<string> PerFeature { get; } = ["decision", "claim", "constraint", "openQuestion", "scenario"];
}
