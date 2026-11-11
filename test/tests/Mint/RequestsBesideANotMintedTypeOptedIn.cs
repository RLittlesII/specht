namespace Specht.Tests.Mint;

/// <summary>
/// A manifest pinned to <c>0.3.1</c> that opts in an <c>epic</c>, <c>feature</c> or <c>task</c> type, and a request for
/// another type (<c>0001-F19</c> B-061): B-061 states the exit for a request to mint any id of such a manifest. An
/// <c>adr</c> request beside an opted-in <c>epic</c>, the scenario's ADR-number row, with an <c>adr</c> entry and grammar
/// that mint its id without the <c>epic</c> entry; and a <c>lesson</c> request beside an opted-in <c>feature</c>, with no
/// <c>lesson</c> entry, so B-061 is checked before B-002's refusal of a type not opted in. The <c>epic</c> and
/// <c>feature</c> entries have no grammar, as B-061 refuses the entry whatever <c>identifiers</c> holds.
/// </summary>
public sealed class RequestsBesideANotMintedTypeOptedIn : TheoryData<string, string>
{
    /// <summary>Initializes the <c>adr</c> and <c>lesson</c> rows.</summary>
    public RequestsBesideANotMintedTypeOptedIn()
    {
        Add(
            """
            {
              "schemaVersion": "0.3.1",
              "identifiers": { "adr": "^z{3}$" },
              "mint": {
                "adr": { "strategy": "nanoid", "nanoid": { "alphabet": "z", "size": 3 } },
                "epic": { "strategy": "nanoid", "nanoid": { "alphabet": "z", "size": 3 } }
              }
            }
            """,
            "adr");
        Add(
            """{"schemaVersion":"0.3.1","mint":{"feature":{"strategy":"nanoid","nanoid":{"alphabet":"z","size":3}}}}""",
            "lesson");
    }
}
