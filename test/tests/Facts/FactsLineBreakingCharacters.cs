namespace Specht.Tests.Facts;

/// <summary>
/// The characters that would add a field or a line to a text line of the facts, each with what a field writes in its
/// place (<c>0132-F2</c> B-024, OQ-5's default): a tab, a line feed and a carriage return each become one space, and a
/// carriage return before a line feed becomes two, since nothing is collapsed.
/// </summary>
public sealed class FactsLineBreakingCharacters : TheoryData<string, string>
{
    public FactsLineBreakingCharacters()
    {
        Add("\t", " ");
        Add("\n", " ");
        Add("\r", " ");
        Add("\r\n", "  ");
    }
}
