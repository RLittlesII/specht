namespace Specht.Tests.Identifiers;

/// <summary>
/// A Nano ID alphabet and size that draw a known candidate, and a type's <c>identifiers</c> grammar that does not match it
/// (<c>0001-F19</c> B-011, B-012): four-digit ADR numbers over a letter, as the scenarios' grammar rejects a Snowflake id, and
/// a grammar in capital letters over a draw in lower case, since the match is in one letter case (§ 7).
/// </summary>
public sealed class GrammarsRejectingTheDraw : TheoryData<string, int, string>
{
    /// <summary>Initializes the four-digit row and the letter-case row.</summary>
    public GrammarsRejectingTheDraw()
    {
        Add("a", 1, "^[0-9]{4}$");
        Add("a", 2, "^AA$");
    }
}
