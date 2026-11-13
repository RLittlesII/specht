namespace Specht.Tests.Identifiers;

/// <summary>
/// A schema <c>0.4.0</c> manifest one of whose <c>ids</c> records names a <c>prefix</c> in its <c>mint</c>, the type
/// requested, the folder <c>--feature</c> names or none, and the key the refusal names (<c>0001-F19</c> B-075; item
/// <c>0437</c>): the requested type's record, with or without a strategy; another type's record, with a prefix that is not
/// text; and a request for a type no record opts in, or for a folder that holds no Feature, which the prefix refuses first.
/// </summary>
public sealed class RecordsNamingAPrefix : TheoryData<string, string, string?, string>
{
    /// <summary>Initializes the requested-record, no-strategy, other-record, not-opted-in and no-Feature rows.</summary>
    public RecordsNamingAPrefix()
    {
        Add(
            """{ "schemaVersion": "0.4.0", "ids": { "claim": { "mint": { "strategy": "nanoid", "prefix": "B-" } } } }""",
            "claim",
            null,
            "ids.claim.mint.prefix");
        Add("""{ "schemaVersion": "0.4.0", "ids": { "claim": { "mint": { "prefix": "B-" } } } }""", "claim", null, "ids.claim.mint.prefix");
        Add(
            """
            {
              "schemaVersion": "0.4.0",
              "ids": { "claim": { "mint": { "strategy": "nanoid" } }, "adr": { "mint": { "strategy": "nanoid", "prefix": 7 } } }
            }
            """,
            "claim",
            null,
            "ids.adr.mint.prefix");
        Add(
            """{ "schemaVersion": "0.4.0", "ids": { "lesson": { "mint": { "strategy": "nanoid", "prefix": "L-" } } } }""",
            "adr",
            null,
            "ids.lesson.mint.prefix");
        Add(
            """{ "schemaVersion": "0.4.0", "ids": { "decision": { "mint": { "strategy": "nanoid", "prefix": "D-" } } } }""",
            "decision",
            "absent",
            "ids.decision.mint.prefix");
    }
}
