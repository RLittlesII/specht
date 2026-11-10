namespace Specht.Tests.Mint;

/// <summary>
/// A manifest pinned to <c>0.3.1</c> that opts in a type for which <c>identifiers</c> holds no grammar, and the type a request
/// names (<c>0001-F19</c> B-043, C-14): <c>adr</c> and <c>lesson</c> each requested with no grammar of its own, and a
/// <c>lesson</c> request with a grammar of its own beside an <c>adr</c> entry that has none, since B-043 states the exit for a
/// request to mint any id of such a manifest. Two rows request a <c>lesson</c> that is not opted in beside an <c>adr</c> entry
/// with no grammar, one with no <c>lesson</c> entry and one whose entry names no strategy: B-043 is checked first, so each
/// is refused as an invalid manifest and not as a type not opted in (B-002's scope).
/// </summary>
public sealed class ManifestsOptingInATypeWithNoGrammar : TheoryData<string, string>
{
    /// <summary>Initializes the five rows.</summary>
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
        Add("""
            {
              "schemaVersion": "0.3.1",
              "mint": { "adr": { "strategy": "nanoid", "nanoid": { "alphabet": "z", "size": 3 } } }
            }
            """, "lesson");
        Add(
            """
            {
              "schemaVersion": "0.3.1",
              "mint": {
                "adr": { "strategy": "nanoid", "nanoid": { "alphabet": "z", "size": 3 } },
                "lesson": { "nanoid": { "alphabet": "z", "size": 3 } }
              }
            }
            """,
            "lesson");
    }
}
