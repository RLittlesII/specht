namespace Specht.Tests.Mint;

/// <summary>
/// A request the mint command folds, and the exit the <c>0001-F19</c> § 7 exit-code table gives it: the manifest text or
/// none, the type, the folder <c>--feature</c> names or none, whether the root exists, and the exit. One row per table row's
/// source: the minted id (B-003, B-004), <c>--feature</c> with each repository-wide type (B-016), a missing root or manifest,
/// an unreadable manifest and <c>mint</c> under a pin before <c>0.3.1</c> (B-015), and the minter's refusals of a type not
/// opted in (B-001, B-002), a type with no grammar (B-043) and an entry naming no strategy (B-047). A <c>mint</c> member
/// naming none of C-13's types, with no grammar, beside an opted-in <c>adr</c>, leaves the <c>adr</c> request its id (B-003).
/// </summary>
public sealed class MintRequestFolds : TheoryData<string?, string, string?, bool, int>
{
    /// <summary>Initializes one row per source of the exit.</summary>
    public MintRequestFolds()
    {
        Add(
            """{"schemaVersion":"0.3.1","identifiers":{"adr":"^z{3}$"},"mint":{"adr":{"strategy":"nanoid","nanoid":{"alphabet":"z","size":3}}}}""",
            "adr", null, true, 0);
        Add(
            """{"schemaVersion":"0.3.1","identifiers":{"lesson":"^z{3}$"},"mint":{"lesson":{"strategy":"nanoid","nanoid":{"alphabet":"z","size":3}}}}""",
            "lesson", null, true, 0);
        Add(
            """{"schemaVersion":"0.3.1","identifiers":{"adr":"^z{3}$"},"mint":{"adr":{"strategy":"nanoid","nanoid":{"alphabet":"z","size":3}}}}""",
            "adr", "features/one", true, 2);
        Add(
            """{"schemaVersion":"0.3.1","identifiers":{"lesson":"^z{3}$"},"mint":{"lesson":{"strategy":"nanoid","nanoid":{"alphabet":"z","size":3}}}}""",
            "lesson", "absent", true, 2);
        Add(
            """
            {
              "schemaVersion": "0.3.1",
              "identifiers": { "adr": "^z{3}$" },
              "mint": {
                "adr": { "strategy": "nanoid", "nanoid": { "alphabet": "z", "size": 3 } },
                "foo": { "strategy": "nanoid", "nanoid": { "alphabet": "z", "size": 3 } }
              }
            }
            """,
            "adr", null, true, 0);
        Add(null, "adr", null, false, 2);
        Add(null, "adr", null, true, 2);
        Add("{", "adr", null, true, 3);
        Add(
            """{"schemaVersion":"0.3.0","identifiers":{"adr":"^z{3}$"},"mint":{"adr":{"strategy":"nanoid","nanoid":{"alphabet":"z","size":3}}}}""",
            "adr", null, true, 3);
        Add("""{"schemaVersion":"0.3.1"}""", "adr", null, true, 3);
        Add("""{"schemaVersion":"0.3.1","mint":{"adr":{"strategy":"nanoid","nanoid":{"alphabet":"z","size":3}}}}""", "adr", null, true, 3);
        Add("""{"schemaVersion":"0.3.1","identifiers":{"adr":"^z{3}$"},"mint":{"adr":{"nanoid":{"alphabet":"z","size":3}}}}""", "adr", null, true, 3);
    }
}
