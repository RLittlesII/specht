---
title: "Decision 0008: Claim-file planned marks mark no discovered case"
description: "The owner selected option A: planned marks read from native Gherkin claim files keep the claim required and mark no discovered case planned, and the report JSON does not change"
type: decision
---

# Decision 0008: Claim-file planned marks mark no discovered case

**Date:** 2026-11-07
**Decided by:** Repository owner, by selecting an option; recorded by spec-author

## The call

The question, as asked: "What should `specht evidence` do with planned marks
read from native Gherkin claim files? (A, recommended) the planned marks keep
the claim required and mark no discovered case planned; only scenario records
mark cases planned, and the report JSON does not change. (B) Each native claim
in the report's `claims[]` also lists its Gherkin cases (path, line, planned),
as an additive member under formatVersion 1, with the gate unchanged."

The owner selected option A, twice on 2026-11-07:

- In the item 0409 session, the option labelled "A: no report change".
- In the coordinating session, the option labelled "A: no case marked
  (Recommended) — Planned marks keep the claim required and mark no discovered
  case planned. Only scenario records mark cases planned. The report JSON is
  unchanged."

The rest of this record is the recorder's reading of that selection, not the
owner's wording.

1. A planned mark a claim file carries keeps its claim required (0201-F7
   B-024) and marks no discovered case planned. A discovered case is planned
   only through scenario records, as decision 0004 item 3 states (B-032,
   B-033).
2. The report's `claims[]` gains no Gherkin case listing, and `formatVersion`
   stays 1.
3. This refines decision 0004 item 7 (planned source) for native claims, and
   decision 0007 item 3, which says the claim files supply planned state: they
   supply it to the claim, never to a discovered case. Neither record's call
   is edited.

## Why

A Gherkin case, a Scenario keyword line or an Examples row, has no join to a
discovered xUnit or VSTest case. The reconciler joins only on explicit
identities and never on a display name or a guessed scenario title (§ 7), so
a planned mark in a claim file has no discovered case it can name.

## Rejected

- Option B, listing each native claim's Gherkin cases in `claims[]` as an
  additive member under `formatVersion` 1, with the gate unchanged: not
  selected.

## Affects

0201-F5 B-018, B-032, B-033, § 5 row 6, § 7, decision 0004 item 7, decision
0007 item 3; 0201-F7 B-016, B-024, B-026; item 0409.

## Reversal

None.
