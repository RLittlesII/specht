namespace Specht.Tests.Identifiers;

/// <summary>
/// A manifest pinned to schema <c>0.4.0</c> that does not opt a type in, or whose record for it names no strategy in its
/// <c>mint</c>, and the type requested (<c>0001-F19</c> B-002, C-2; item <c>0437</c>): no <c>ids</c> key, a record for
/// <c>lesson</c> alone, an <c>adr</c> record holding no <c>mint</c>, an <c>adr</c> record whose <c>mint</c> has no
/// <c>strategy</c> or a number in its place, <c>epic</c>, a type with no record, and <c>$x</c>, a <c>$</c>-led member the
/// parser skips, which names no type whatever its <c>mint</c> holds (C-10). Each opt-in that names no strategy holds a
/// <c>nanoid</c> sub-object an id could be drawn from, so a default strategy would mint one.
/// </summary>
public sealed class RecordsNotOptingTheTypeIn : TheoryData<string, string>
{
    /// <summary>Initializes one row per manifest and requested type.</summary>
    public RecordsNotOptingTheTypeIn()
    {
        Add("""{ "schemaVersion": "0.4.0" }""", "adr");
        Add("""{ "schemaVersion": "0.4.0", "ids": { "lesson": { "mint": { "strategy": "nanoid" } } } }""", "adr");
        Add("""{ "schemaVersion": "0.4.0", "ids": { "adr": { "id": "[ab]+" } } }""", "adr");
        Add("""{ "schemaVersion": "0.4.0", "ids": { "adr": { "mint": { "nanoid": { "alphabet": "ab", "size": 2 } } } } }""", "adr");
        Add("""{ "schemaVersion": "0.4.0", "ids": { "adr": { "mint": { "strategy": 7, "nanoid": { "alphabet": "ab", "size": 2 } } } } }""", "adr");
        Add("""{ "schemaVersion": "0.4.0", "ids": { "adr": { "mint": { "strategy": "nanoid" } } } }""", "epic");
        Add(
            """
            {
              "schemaVersion": "0.4.0",
              "ids": { "$x": { "mint": { "strategy": "snowflake", "snowflake": { "epoch": "2026-01-01", "datacenter": 0, "worker": 0, "sequenceStart": 0 } } } }
            }
            """,
            "$x");
        Add("""{ "schemaVersion": "0.4.0", "ids": { "$x": { "mint": { "strategy": "nanoid", "nanoid": { "alphabet": "ab", "size": 2 } } } } }""", "$x");
    }
}
