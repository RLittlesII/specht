namespace Specht.Tests.Manifest;

/// <summary>
/// The members of a schema 0.4.0 manifest whose claim record's <c>id</c> <c>0001-F5</c> B-085 rejects, and the value the
/// rejection names, the word "empty" for empty text, which has no characters to quote: empty text, a value that is not
/// text, and a pattern that does not compile.
/// </summary>
public sealed class UnusableIdPatterns : TheoryData<string, string>
{
    /// <summary>Initializes the rows.</summary>
    public UnusableIdPatterns()
    {
        Add("""{ "ids": { "claim": { "id": "" } } }""", "empty");
        Add("""{ "ids": { "claim": { "id": 7 } } }""", "7");
        Add("""{ "ids": { "claim": { "id": "[0-9" } } }""", "[0-9");
    }
}
