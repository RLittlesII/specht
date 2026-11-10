namespace Specht.Tests.Mint;

/// <summary>
/// A manifest that opts no <c>adr</c> entry in, for a request of an <c>adr</c> id (<c>0001-F19</c> B-001, B-002, C-2), one row
/// per Examples row of B-001's scenario: no <c>mint</c> key at all, and a <c>mint</c> key opting only <c>lesson</c> in to Nano ID.
/// </summary>
public sealed class TypesNotOptedInRequests : TheoryData<string>
{
    /// <summary>Initializes the no-key and lesson-only rows.</summary>
    public TypesNotOptedInRequests()
    {
        Add("""{"schemaVersion":"0.3.1"}""");
        Add("""
            {
              "schemaVersion": "0.3.1",
              "identifiers": { "lesson": "^z{3}$" },
              "mint": { "lesson": { "strategy": "nanoid", "nanoid": { "alphabet": "z", "size": 3 } } }
            }
            """);
    }
}
