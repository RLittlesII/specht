namespace Specht.Tests.Identifiers;

/// <summary>
/// A Nano ID alphabet and size that draw a known candidate, and a type's <c>identifiers</c> grammar that matches it
/// (<c>0001-F19</c> B-011, B-012; § 7): a grammar anchored with <c>^</c> and <c>$</c>, as every shipped grammar is, and one
/// that finds itself in the candidate unanchored, since the pattern is matched as written and not anchored again.
/// </summary>
public sealed class GrammarsMatchingTheDraw : TheoryData<string, int, string>
{
    /// <summary>Initializes the anchored row and the unanchored row.</summary>
    public GrammarsMatchingTheDraw()
    {
        Add("a", 2, "^a{2}$");
        Add("a", 2, "a");
    }
}
