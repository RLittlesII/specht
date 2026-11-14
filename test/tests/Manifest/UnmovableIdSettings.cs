namespace Specht.Tests.Manifest;

/// <summary>
/// A manifest pinned to schema <c>0.4.0</c> whose id settings <c>specht upgrade</c> cannot move exactly (<c>0001-F7</c>
/// B-159), and every member the refusal names: a grammar that is not B-155's shape - unanchored, not text, an alternation
/// outside a group, an empty pattern, an escaped closing dollar; a <c>mint</c> prefix that is not the head of the type's
/// <c>plain</c> after the move - the shipped one, a moved one, or not text; an <c>ids</c> record for a type an old setting also
/// holds, the grammar customised or a shipped default; claim and scenario grammars whose move gives one head; and three faults
/// at once.
/// </summary>
public sealed class UnmovableIdSettings : TheoryData<string, string[]>
{
    public UnmovableIdSettings()
    {
        Add("""{"schemaVersion":"0.4.0","identifiers":{"claim":"B-[0-9]{3}"}}""", ["identifiers.claim"]);
        Add("""{"schemaVersion":"0.4.0","identifiers":{"claim":5}}""", ["identifiers.claim"]);
        Add("""{"schemaVersion":"0.4.0","identifiers":{"claim":"^B-[0-9]{3}|X-[0-9]{3}$"}}""", ["identifiers.claim"]);
        Add("""{"schemaVersion":"0.4.0","identifiers":{"claim":"^B-$"}}""", ["identifiers.claim"]);
        Add("""{"schemaVersion":"0.4.0","identifiers":{"claim":"^B-[0-9]+\\$"}}""", ["identifiers.claim"]);
        Add("""{"schemaVersion":"0.4.0","mint":{"decision":{"strategy":"nanoid","prefix":"D-"}}}""", ["mint.decision.prefix"]);
        Add(
            """{"schemaVersion":"0.4.0","identifiers":{"decision":"^D-[0-9]{4}$"},"mint":{"decision":{"strategy":"nanoid","prefix":"E-"}}}""",
            ["mint.decision.prefix"]);
        Add("""{"schemaVersion":"0.4.0","mint":{"claim":{"strategy":"nanoid","prefix":5}}}""", ["mint.claim.prefix"]);
        Add(
            """{"schemaVersion":"0.4.0","ids":{"claim":{"plain":"B-{id}","id":"[0-9]{4}"}},"identifiers":{"claim":"^B-[0-9]{5}$"}}""",
            ["identifiers.claim"]);
        Add(
            """{"schemaVersion":"0.4.0","ids":{"claim":{"plain":"B-{id}","id":"[0-9]{4}"}},"identifiers":{"claim":"^B-[0-9]{3}[a-z]?$"}}""",
            ["identifiers.claim"]);
        Add("""{"schemaVersion":"0.4.0","ids":{"adr":{"plain":"{id}","id":"[0-9]{4}"}},"mint":{"adr":{"strategy":"nanoid"}}}""", ["mint.adr"]);
        Add("""{"schemaVersion":"0.4.0","identifiers":{"claim":"^X-[0-9]+$","scenario":"^X-[0-9]+$"}}""", ["identifiers.claim", "identifiers.scenario"]);
        Add(
            """
            {
              "schemaVersion": "0.4.0",
              "identifiers": { "claim": "B-[0-9]{3}", "adr": "^ADR-[0-9]{4}$" },
              "mint": { "decision": { "strategy": "nanoid", "prefix": "D-" } },
              "ids": { "adr": { "plain": "ADR-{id}", "id": "[0-9]{4}" } }
            }
            """,
            ["identifiers.claim", "mint.decision.prefix", "identifiers.adr"]);
    }
}
