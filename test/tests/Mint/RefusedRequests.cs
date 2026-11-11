namespace Specht.Tests.Mint;

/// <summary>
/// One request of each refusal this command's claims state, as the manifest, the requested type and the folder
/// <c>--feature</c> names, or none (AGENTS.md § Invariants; <c>0001-F19</c> B-001, B-015, B-016, B-043): a type not opted in,
/// a repository-wide type named for a Feature, a type opted in with no grammar, id settings under a pin before
/// <c>0.3.1</c>, and a per-Feature type requested without a Feature (B-057) and for a folder that holds none (B-059).
/// </summary>
public sealed class RefusedRequests : TheoryData<string, string, string?>
{
    /// <summary>Initializes the six rows.</summary>
    public RefusedRequests()
    {
        Add("""{"schemaVersion":"0.3.1"}""", "adr", null);
        Add("""
            {
              "schemaVersion": "0.3.1",
              "identifiers": { "adr": "^z{3}$" },
              "mint": { "adr": { "strategy": "nanoid", "nanoid": { "alphabet": "z", "size": 3 } } }
            }
            """, "adr", "features/one");
        Add("""
            {
              "schemaVersion": "0.3.1",
              "mint": { "adr": { "strategy": "nanoid", "nanoid": { "alphabet": "z", "size": 3 } } }
            }
            """, "adr", null);
        Add("""
            {
              "schemaVersion": "0.3.0",
              "identifiers": { "adr": "^z{3}$" },
              "mint": { "adr": { "strategy": "nanoid", "nanoid": { "alphabet": "z", "size": 3 } } }
            }
            """, "adr", null);
        Add("""
            {
              "schemaVersion": "0.3.1",
              "identifiers": { "claim": "^B-z{3}$" },
              "mint": { "claim": { "strategy": "nanoid", "nanoid": { "alphabet": "z", "size": 3 } } }
            }
            """, "claim", null);
        Add("""
            {
              "schemaVersion": "0.3.1",
              "identifiers": { "claim": "^B-z{3}$" },
              "mint": { "claim": { "strategy": "nanoid", "nanoid": { "alphabet": "z", "size": 3 } } }
            }
            """, "claim", "absent");
    }
}
