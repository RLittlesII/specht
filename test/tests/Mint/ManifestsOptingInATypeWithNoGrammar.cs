namespace Specht.Tests.Mint;

/// <summary>
/// A manifest pinned to <c>0.3.1</c> that opts in a type for which <c>identifiers</c> holds no grammar, and the type a request
/// names (<c>0001-F19</c> B-043, C-14): <c>adr</c> and <c>lesson</c> each requested with no grammar of its own, and a
/// <c>lesson</c> request with a grammar of its own beside an <c>adr</c> entry that has none, since B-043 states the exit for a
/// request to mint any id of such a manifest.
/// </summary>
public sealed class ManifestsOptingInATypeWithNoGrammar : TheoryData<string, string>
{
    /// <summary>Initializes the three rows.</summary>
    public ManifestsOptingInATypeWithNoGrammar()
    {
        Add("""
            {
              "schemaVersion": "0.3.1",
              "mint": { "adr": { "strategy": "nanoid", "nanoid": { "alphabet": "z", "size": 3 } } }
            }
            """, "adr");
        Add("""
            {
              "schemaVersion": "0.3.1",
              "mint": { "lesson": { "strategy": "nanoid", "nanoid": { "alphabet": "z", "size": 3 } } }
            }
            """, "lesson");
        Add(
            """
            {
              "schemaVersion": "0.3.1",
              "identifiers": { "lesson": "^z{3}$" },
              "mint": {
                "adr": { "strategy": "nanoid", "nanoid": { "alphabet": "z", "size": 3 } },
                "lesson": { "strategy": "nanoid", "nanoid": { "alphabet": "z", "size": 3 } }
              }
            }
            """,
            "lesson");
    }
}
