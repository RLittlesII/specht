---
title: "Decision 0004: the native claim rules take SPEC4011 to SPEC4013"
description: "The owner's selection of November 14 2026: the working names SPEC025, SPEC026 and SPEC027 take SPEC4011, SPEC4012 and SPEC4013 in the claims group, in old-id order, whatever order their items merge in; the group, the precedent and the consequences are spec-author's reading"
type: decision
---

# Decision 0004: the native claim rules take SPEC4011 to SPEC4013

**Date:** 2026-11-14
**Decided by:** the repository owner, selecting an option relayed through the coordinating session; the question and the option's wording are the coordinating session's, and the group, the precedent and the consequences below are spec-author's reading; recorded by spec-author

## The call

The owner selected **"Fixed 4011, 4012, 4013 (Recommended)"**.

The option, in the coordinating session's wording, not the owner's:
"SPEC025=SPEC4011, SPEC026=SPEC4012, SPEC027=SPEC4013 in old-id order, as
ADR-0029 ruling 5 did for 0001-F12, whatever order the PRs merge in".

| Working name | Id         | Item   | Claims                                     |
| ------------ | ---------- | ------ | ------------------------------------------ |
| `SPEC025`    | `SPEC4011` | `0393` | B-010                                      |
| `SPEC026`    | `SPEC4012` | `0394` | B-009                                      |
| `SPEC027`    | `SPEC4013` | `0395` | B-014 to B-016, B-018, B-019, B-033, B-034 |

Spec-author's reading, not the owner's words:

1. **The group.** Each id's thousands digit is the group of
   [ADR-0010](../../../../../../.spec/adr/0010-layered-engine-boundaries-with-io-at-the-edge-and-a-mediator.md)
   § "Rule ids", as
   [ADR-0029](../../../../../../.spec/adr/0029-rule-ids-take-four-digits-in-schema-0-3-0-and-published-versions-keep-theirs.md)
   § "The map" has it: `4xxx`, claims and feature files. Each rule reports a
   claim tag or a claim file the reader cannot take claims from.
2. **The numbers.** `0.4.0` on `main` (`d760c454`) holds `SPEC4001` to
   `SPEC4010`, and no open pull request named an id from `SPEC4011` on. The
   order is the working names' order, as ADR-0029 ruling 5 gave `0001-F12`'s
   four rules `SPEC4007` to `SPEC4010`.
3. **When each enters a version.** Each id enters `0.4.0`'s vocabulary in the
   item that builds its rule, with its rule page and a relock (C-3;
   [decision 0001](0001-native-claims-fold-into-schema-0-4-0.md)). Until then
   no shipped version holds it, and no other rule may take it.

## Why

The owner's reason for the selection is not in this record. Spec-author's
reading of what it gives: each item names its id before it is built, so the
three pull requests cannot take the same next free id, and an item that
merges out of order does not move another's id.

## Rejected

Each item taking the next free id in its group when its pull request merges,
as items `0393` to `0395` stated before this decision. Spec-author's reading
of its cost: the ids would follow the merge order, and two branches cut from
one `main` would each take `SPEC4011`.

## Affects

OQ-5, whose resolution left the ids undecided, noted with this record;
B-009, B-010, B-014 to B-016, B-018, B-019, B-033 and B-034, each amended
to name its id; their scenarios in `native-claims.feature`; § 10; Tasks;
items `0393` to `0395`. § 7's working names are the implementer's to rename.
[ADR-0029](../../../../../../.spec/adr/0029-rule-ids-take-four-digits-in-schema-0-3-0-and-published-versions-keep-theirs.md)
is accepted and unchanged; this record extends its map for these three ids.

## Reversal

None.
