namespace Specht.Tests.Identifiers;

/// <summary>
/// A type's <c>encoding</c> that is neither <c>none</c> nor <c>base32</c> (<c>0001-F19</c> B-054, C-15), as JSON: the
/// scenario's <c>base64</c> and <c>decimal</c>, and a number.
/// </summary>
public sealed class EncodingsNotOffered : TheoryData<string>
{
    /// <summary>Initializes the "base64", "decimal" and 7 rows.</summary>
    public EncodingsNotOffered()
    {
        Add("\"base64\"");
        Add("\"decimal\"");
        Add("7");
    }
}
