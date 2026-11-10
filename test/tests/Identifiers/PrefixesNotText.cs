namespace Specht.Tests.Identifiers;

/// <summary>
/// A type's <c>prefix</c> that is not text (<c>0001-F19</c> B-053, C-12), as JSON, one row per row of the scenario: the
/// number 7, a list, a group of named settings, and a yes-or-no value.
/// </summary>
public sealed class PrefixesNotText : TheoryData<string>
{
    /// <summary>Initializes the 7, list, object and boolean rows.</summary>
    public PrefixesNotText()
    {
        Add("7");
        Add("""["ADR-"]""");
        Add("""{ "text": "ADR-" }""");
        Add("true");
    }
}
