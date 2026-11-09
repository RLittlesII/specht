namespace Specht.Tests.Mint;

/// <summary>
/// One request of each refusal this command's claims state, as the manifest, the requested type and the folder
/// <c>--feature</c> names, or none (AGENTS.md § Invariants; <c>0001-F19</c> B-001, B-015, B-016, B-043): a type not opted in,
/// a repository-wide type named for a Feature, a type opted in with no grammar, and id settings under a pin before
/// <c>0.3.1</c>.
/// </summary>
public sealed class RefusedRequests : TheoryData<string, string, string?>
{
    /// <summary>Initializes the four rows.</summary>
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
    }
}
