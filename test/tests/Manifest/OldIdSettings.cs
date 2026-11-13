namespace Specht.Tests.Manifest;

/// <summary>
/// The members of a schema 0.4.0 manifest holding a setting <c>0001-F5</c> B-092 rejects, and the <c>ids</c> record the
/// rejection names for each type the setting holds (B-093): the top-level <c>mint</c>, empty or opting types in, and an
/// <c>identifiers</c> grammar for each of the seven types.
/// </summary>
public sealed class OldIdSettings : TheoryData<string, string[]>
{
    /// <summary>Initializes the rows.</summary>
    public OldIdSettings()
    {
        Add("""{ "mint": {} }""", []);
        Add("""{ "mint": { "claim": { "strategy": "nanoid" } } }""", ["ids.claim"]);
        Add("""{ "mint": { "adr": { "strategy": "nanoid" }, "lesson": { "strategy": "nanoid" } } }""", ["ids.adr", "ids.lesson"]);
        Add("""{ "identifiers": { "claim": "^B-[0-9]{3}$" } }""", ["ids.claim"]);
        Add("""{ "identifiers": { "constraint": "^C-[0-9]+$" } }""", ["ids.constraint"]);
        Add("""{ "identifiers": { "openQuestion": "^OQ-[0-9]+$" } }""", ["ids.openQuestion"]);
        Add("""{ "identifiers": { "scenario": "^S-[0-9]+$" } }""", ["ids.scenario"]);
        Add("""{ "identifiers": { "decision": "^[0-9]{4}$" } }""", ["ids.decision"]);
        Add("""{ "identifiers": { "adr": "^[0-9]{4}$" } }""", ["ids.adr"]);
        Add("""{ "identifiers": { "lesson": "^[0-9]{4}$" } }""", ["ids.lesson"]);
    }
}
