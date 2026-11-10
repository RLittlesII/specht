namespace Specht.Tests.Identifiers;

/// <summary>
/// The encodings a Nano ID type may name that formatting offers it (<c>0001-F19</c> C-15), as JSON: none named, and
/// <c>none</c>. Under either, B-009's prefix starts the id and B-005's and B-006's draw follows it.
/// </summary>
public sealed class NanoIdEncodingsOffered : TheoryData<string?>
{
    /// <summary>Initializes the row naming no encoding and the <c>"none"</c> row.</summary>
    public NanoIdEncodingsOffered()
    {
        Add(default(string));
        Add("\"none\"");
    }
}
