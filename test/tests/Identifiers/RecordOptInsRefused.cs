namespace Specht.Tests.Identifiers;

/// <summary>
/// An <c>adr</c> record's <c>mint</c> under schema <c>0.4.0</c> that mints no id, and the key its refusal names, the setting's
/// place under the record and not under a top-level <c>mint</c> (<c>0001-F19</c> § 7; item <c>0437</c>): Nano ID settings that
/// are not an object, a size and an alphabet no id is drawn from, a Snowflake epoch and field that are not read, an encoding
/// and a number of attempts that are not offered, a strategy that is not offered, and no strategy.
/// </summary>
public sealed class RecordOptInsRefused : TheoryData<string, string>
{
    /// <summary>Initializes one row per refused setting.</summary>
    public RecordOptInsRefused()
    {
        Add("""{ "strategy": "nanoid", "nanoid": "text" }""", "ids.adr.mint.nanoid");
        Add("""{ "strategy": "nanoid", "nanoid": { "size": 0 } }""", "ids.adr.mint.nanoid.size");
        Add("""{ "strategy": "nanoid", "nanoid": { "alphabet": "" } }""", "ids.adr.mint.nanoid.alphabet");
        Add("""{ "strategy": "snowflake", "snowflake": { "epoch": "soon" } }""", "ids.adr.mint.snowflake.epoch");
        Add("""{ "strategy": "snowflake", "snowflake": { "worker": -1 } }""", "ids.adr.mint.snowflake.worker");
        Add("""{ "strategy": "nanoid", "encoding": "hex" }""", "ids.adr.mint.encoding");
        Add("""{ "strategy": "nanoid", "attempts": 0 }""", "ids.adr.mint.attempts");
        Add("""{ "strategy": "uuid" }""", "ids.adr.mint");
        Add("""{ "nanoid": { "size": 8 } }""", "ids.adr.mint");
    }
}
