namespace Specht.Tests.Identifiers;

/// <summary>
/// A Nano ID <c>alphabet</c> that is empty or not a string (<c>0001-F19</c> B-050, C-12), as JSON, one row per row of the
/// scenario: the empty string and a number.
/// </summary>
public sealed class AlphabetsNotNonEmptyText : TheoryData<string>
{
    /// <summary>Initializes the empty and number rows.</summary>
    public AlphabetsNotNonEmptyText()
    {
        Add("\"\"");
        Add("5");
    }
}
