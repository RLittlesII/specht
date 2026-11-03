---
title: "ADR-0030: Schema 0.3.0 first carries the gap width and the malformed-edge text"
description: "Narrows ADR-0029 ruling 6 for 0001-F5 B-053 and B-054: 'a schema 0.2.0 manifest' there now means 0.3.0 or later, on the owner's November 2 2026 ruling that item 0016's contract first holds at schema 0.3.0. Published 0.1.0, 0.2.0 and 0.2.1 keep today's verdicts and bytes. ADR-0029 is not edited."
type: adr
---

# ADR-0030: Schema 0.3.0 first carries the gap width and the malformed-edge text

## Status

accepted - 2026-11-02. Decider: the repository owner, answering item
[`0016`](../../src/specht/Manifest/.issue/0016-keys-forms-and-grammars.yml)'s
question 1 with "A 0.3.0 in place (Recommended)". Recorded by spec-author.
It narrows ruling 6 of
[ADR-0029](0029-rule-ids-take-four-digits-in-schema-0-3-0-and-published-versions-keep-theirs.md)
and edits no accepted record.

## Context

- ADR-0029 ruling 6 reads "a schema `0.2.0` manifest" in `0001-F5` B-007,
  B-036, B-037, B-053 and B-054 as `0.2.0` or later.
- Item `0134` built B-007, B-036 and B-037 at `0.2.0`, so the reading holds
  for them.
- B-053 (the `SPEC5004` gap message pads to the captured sequence width) and
  B-054 (`SPEC6001` reports a malformed edge by its original text) are not
  built. Under ruling 6, B-054 would reach the published `0.2.0` and `0.2.1`.
- `0.2.1`, and the versions below it, are in a published package; `0.3.0`
  is in none (ADR-0029 ruling 2; lesson 0016). The tag `v0.3.1` embeds
  `v0.1.0`, `v0.2.0` and `v0.2.1` and no `v0.3.0`.
- A published version never changes in place (`0001-F7` C-1), and an older
  pin never takes a newer interpretation (`0001-F7` C-4). The verdict
  includes the identifier and the message (`0001-F1` C-9).
- The owner ruled that item `0016`'s contract first holds at schema
  `0.3.0`, written in place with a relock (`0001-F5` decision 0016).

## Decision drivers

1. No published verdict changes (`0001-F7` C-1, C-4).
2. One version boundary for one item's contract, so that one pin decides
   it.

## Considered options

1. **Narrow ruling 6 for B-053 and B-054 to `0.3.0` or later.** Chosen.
2. **Keep ruling 6 as written.** A tree with a malformed edge changes its
   `0.2.0` and `0.2.1` verdict. B-053 changes nothing observable there,
   because the legacy read sees only two-digit sequences.
3. **Edit ADR-0029.** It is accepted and immutable except its status.

## Decision

- In `0001-F5` B-053 and B-054, "a manifest pinned to schema `0.2.0` or
  later" becomes "a manifest pinned to schema `0.3.0` or later".
- Under `0.1.0`, `0.2.0` and `0.2.1`, the `SPEC044` gap message keeps its
  two-digit padding, and `SPEC050` keeps reporting an edge by its expanded
  text (`0001-F5` B-049).
- Ruling 6 stands for B-007, B-036 and B-037.
- ADR-0029's reading of how a specification names a rule is unchanged.
  B-053 and B-054 name `SPEC5004` and `SPEC6001`, the `0.3.0` ids.

## Consequences

- A consumer pinned to a published version sees no change. One pinned to
  `0.3.0` gets the captured width and the original edge text.
- `0.3.0` is relocked once more by the change that builds item `0016`
  (`0001-F7` C-17, decision 0014).
- The boundary holds only while no package carries `0.3.0`. Item `0016`'s
  ruling holds back every `v*` tag until it merges (`0001-F5` decision
  0016).
