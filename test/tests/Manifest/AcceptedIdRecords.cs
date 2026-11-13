namespace Specht.Tests.Manifest;

/// <summary>
/// The members of a schema 0.4.0 manifest whose <c>ids</c> records <c>0001-F5</c> accepts (B-081, B-083 to B-085, B-094):
/// a record for one type or several, annotations on the key and on a record (B-020), a record's unread <c>mint</c>
/// (B-091), the task, Feature and epic grammars left under <c>identifiers</c> (B-092), a claim head that begins a longer
/// scenario head, and two heads that differ only in case while claim tags are compared with case.
/// </summary>
public sealed class AcceptedIdRecords : TheoryData<string>
{
    /// <summary>Initializes the rows.</summary>
    public AcceptedIdRecords()
    {
        Add("""{ "ids": {} }""");
        Add("""{ "ids": { "claim": { "form": "{prefix}-{id}", "plain": "B-{id}", "id": "[0-9a-hjkmnp-tv-z]{6}" } } }""");
        Add("""{ "ids": { "scenario": { "plain": "S-{id}", "id": "[0-9]+" } } }""");
        Add("""
            { "ids": {
              "claim": { "plain": "B-{id}", "id": "[0-9]{3}" },
              "constraint": { "plain": "C-{id}", "id": "[0-9]+" },
              "openQuestion": { "plain": "OQ-{id}", "id": "[0-9]+" },
              "scenario": { "plain": "S-{id}", "id": "[0-9]+" },
              "decision": { "plain": "{id}", "id": "[0-9]{4}" } } }
            """);
        Add("""{ "ids": { "adr": { "plain": "{id}", "id": "[0-9]{4}" }, "lesson": { "plain": "ADR-{id}", "id": "[0-9]{4}" } } }""");
        Add("""{ "ids": { "$comment": "ids", "claim": { "plain": "B-{id}", "id": "[0-9]{3}", "$comment": "the claim record" } } }""");
        Add("""
            { "ids": { "claim": { "id": "[0-9]{3}",
              "mint": { "strategy": "nanoid", "nanoid": { "alphabet": "0123456789abcdefghjkmnpqrstvwxyz", "size": 6 } } } } }
            """);
        Add("""{ "identifiers": { "task": "^(?<epic>[0-9]{4})-(?<sequence>[0-9]{2})$", "feature": "^F[0-9]+[a-z]?$", "epic": "^[0-9]{4}$" } }""");
        Add("""{ "ids": { "claim": { "form": "{prefix}-{id}" }, "scenario": { "form": "{prefix}-SCENARIO-{id}" } } }""");
        Add("""{ "claim": { "tagIgnoreCase": false }, "ids": { "claim": { "plain": "X-{id}" }, "scenario": { "plain": "x-{id}" } } }""");
    }
}
