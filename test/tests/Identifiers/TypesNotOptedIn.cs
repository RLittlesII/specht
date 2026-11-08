namespace Specht.Tests.Identifiers;

/// <summary>
/// A manifest's <c>mint</c> key that opts no <c>adr</c> entry in (<c>0001-F19</c> B-046, C-2), one row per row of the
/// scenario: no <c>mint</c> key at all, and a <c>mint</c> key opting only <c>lesson</c> in to Nano ID.
/// </summary>
public sealed class TypesNotOptedIn : TheoryData<string?>
{
    /// <summary>Initializes the no-key and lesson-only rows.</summary>
    public TypesNotOptedIn()
    {
        Add(default(string));
        Add("""{ "lesson": { "strategy": "nanoid", "nanoid": { "alphabet": "ab", "size": 2 } } }""");
    }
}
