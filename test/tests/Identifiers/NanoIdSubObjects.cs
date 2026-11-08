namespace Specht.Tests.Identifiers;

/// <summary>
/// An <c>adr</c> entry's <c>nanoid</c> sub-object, or none, and the id it mints over draws of 1 and 0 when the entry itself
/// names the alphabet <c>wxyz</c> and the size 5, and its <c>snowflake</c> sub-object the alphabet <c>pq</c> and the size 7
/// (<c>0001-F19</c> C-3): the sub-object naming both, naming only the alphabet so the size is <c>adr</c>'s default of 8
/// (B-020), naming only the size so the alphabet is lower-case base32 (B-019), and absent so both defaults apply. Reading
/// either outside value, in place of a member the sub-object names or as a fallback for one it leaves out, changes the id.
/// </summary>
public sealed class NanoIdSubObjects : TheoryData<string?, string>
{
    /// <summary>Initializes the four rows.</summary>
    public NanoIdSubObjects()
    {
        Add("""{ "alphabet": "ab", "size": 3 }""", "bab");
        Add("""{ "alphabet": "ab" }""", "babababa");
        Add("""{ "size": 3 }""", "101");
        Add(null, "10101010");
    }
}
