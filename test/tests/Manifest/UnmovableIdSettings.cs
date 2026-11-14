namespace Specht.Tests.Manifest;

/// <summary>
/// A manifest pinned to schema <c>0.4.0</c> whose id settings <c>specht upgrade</c> cannot move exactly (<c>0001-F7</c>
/// B-159), every member the refusal names, and every member it does not because the consumer never wrote it: a grammar that is
/// not B-155's shape - unanchored, not text, an alternation outside a group, an empty pattern, an escaped closing dollar; a
/// <c>mint</c> prefix that is not the head of the type's <c>plain</c> after the move - the shipped one, a moved one, or not text;
/// an <c>ids</c> record for a type an old setting also holds, the grammar customised or a shipped default; claim and scenario
/// grammars whose move gives one head, the same text, or text differing only in case where <c>claim.tagIgnoreCase</c> is
/// <c>true</c> (<c>0001-F5</c> B-094); a moved claim grammar whose head is that of the consumer's own <c>ids.scenario</c> record;
/// a <c>mint</c> entry for <c>epic</c> or <c>task</c>, names outside the seven <c>0001-F19</c> C-13 types, an object or text,
/// named <c>mint.&lt;name&gt;</c> and never as the <c>ids</c> record a move would have written, alone and beside another fault;
/// and three faults at once.
/// </summary>
public sealed class UnmovableIdSettings : TheoryData<string, string[], string[]>
{
    public UnmovableIdSettings()
    {
        Add("""{"schemaVersion":"0.4.0","identifiers":{"claim":"B-[0-9]{3}"}}""", ["identifiers.claim"], []);
        Add("""{"schemaVersion":"0.4.0","identifiers":{"claim":5}}""", ["identifiers.claim"], []);
        Add("""{"schemaVersion":"0.4.0","identifiers":{"claim":"^B-[0-9]{3}|X-[0-9]{3}$"}}""", ["identifiers.claim"], []);
        Add("""{"schemaVersion":"0.4.0","identifiers":{"claim":"^B-$"}}""", ["identifiers.claim"], []);
        Add("""{"schemaVersion":"0.4.0","identifiers":{"claim":"^B-[0-9]+\\$"}}""", ["identifiers.claim"], []);
        Add("""{"schemaVersion":"0.4.0","mint":{"decision":{"strategy":"nanoid","prefix":"D-"}}}""", ["mint.decision.prefix"], []);
        Add(
            """{"schemaVersion":"0.4.0","identifiers":{"decision":"^D-[0-9]{4}$"},"mint":{"decision":{"strategy":"nanoid","prefix":"E-"}}}""",
            ["mint.decision.prefix"],
            []);
        Add("""{"schemaVersion":"0.4.0","mint":{"claim":{"strategy":"nanoid","prefix":5}}}""", ["mint.claim.prefix"], []);
        Add(
            """{"schemaVersion":"0.4.0","ids":{"claim":{"plain":"B-{id}","id":"[0-9]{4}"}},"identifiers":{"claim":"^B-[0-9]{5}$"}}""",
            ["identifiers.claim"],
            []);
        Add(
            """{"schemaVersion":"0.4.0","ids":{"claim":{"plain":"B-{id}","id":"[0-9]{4}"}},"identifiers":{"claim":"^B-[0-9]{3}[a-z]?$"}}""",
            ["identifiers.claim"],
            []);
        Add("""{"schemaVersion":"0.4.0","ids":{"adr":{"plain":"{id}","id":"[0-9]{4}"}},"mint":{"adr":{"strategy":"nanoid"}}}""", ["mint.adr"], []);
        Add("""{"schemaVersion":"0.4.0","identifiers":{"claim":"^X-[0-9]+$","scenario":"^X-[0-9]+$"}}""", ["identifiers.claim", "identifiers.scenario"], []);
        Add(
            """{"schemaVersion":"0.4.0","claim":{"tagIgnoreCase":true},"identifiers":{"claim":"^X-[0-9]+$","scenario":"^x-[0-9]+$"}}""",
            ["identifiers.claim", "identifiers.scenario"],
            []);
        Add(
            """{"schemaVersion":"0.4.0","ids":{"scenario":{"plain":"X-{id}","id":"[0-9]+"}},"identifiers":{"claim":"^X-[0-9]+$"}}""",
            ["identifiers.claim", "ids.scenario"],
            []);
        Add("""{"schemaVersion":"0.4.0","mint":{"epic":{"strategy":"nanoid"}}}""", ["mint.epic"], ["ids.epic"]);
        Add("""{"schemaVersion":"0.4.0","mint":{"task":"nanoid"}}""", ["mint.task"], ["ids.task"]);
        Add(
            """{"schemaVersion":"0.4.0","identifiers":{"claim":"B-[0-9]{3}"},"mint":{"epic":{"strategy":"nanoid","prefix":"E-"}}}""",
            ["identifiers.claim", "mint.epic"],
            ["ids.epic"]);
        Add(
            """
            {
              "schemaVersion": "0.4.0",
              "identifiers": { "claim": "B-[0-9]{3}", "adr": "^ADR-[0-9]{4}$" },
              "mint": { "decision": { "strategy": "nanoid", "prefix": "D-" } },
              "ids": { "adr": { "plain": "ADR-{id}", "id": "[0-9]{4}" } }
            }
            """,
            ["identifiers.claim", "mint.decision.prefix", "identifiers.adr"],
            []);
    }
}
