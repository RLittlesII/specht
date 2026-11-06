---
title: "Decision 0007: specht evidence reconciles native Gherkin claims"
description: "The owner puts native Gherkin claim input in this Feature's scope, read from the claim files through the engine's Gherkin claim reader, resolving OQ-3"
type: decision
---

# Decision 0007: specht evidence reconciles native Gherkin claims

**Date:** 2026-11-06
**Decided by:** Repository owner; recorded by spec-author

## The call

1. `specht evidence` reconciles native Gherkin claims
   (`codeAnalysis.records.claimSource: "gherkin"`). The owner: "Yes, in scope".
2. The reconciler reads those claims, and their planned marks, from the
   consumer's claim files through the engine's Gherkin claim reader (0201-F7
   decision 0004), not from the artifact's `catalog`. Asked where that reader
   should live so the tool can use it, the owner: "In the specht engine".
3. OQ-3 is resolved by this call: for a consumer without scenario records, the
   claim files, read through the engine's reader, supply planned state.
4. Item 0409 writes the agreement: new § 3 claims, and the amendment of § 5
   row 5 (parsing `.feature` files) and row 6 (reading the catalogue), which
   brings them into line with 0201-F7 § 5 row 11 as item 0411 amends it, and
   with consumer request #256. This decision changes no claim.
5. Which declared projects the default gate needs a current export from is
   OQ-5. This decision does not settle it.

## Why

Item 0302's packaged consumer fixture (branch
`0302/packaged-gherkin-vstest-fixture`, commit `38670c0d`) has a Gherkin-only
manifest. `specht evidence` exits 3 with `invalidInput` on it: the policy
requires a mapped claims section, and the reconciler reads claims only from
Markdown claims tables. § 5 row 6 and OQ-3's proposed default left such a
consumer with no claim to reconcile. Of the export that § 5 row 5 relies on,
the owner said: "This sounds like uncessary coupling. It does not sounds like
what I would ask you to do." (0201-F7 decision 0004).

## Rejected

- OQ-3's proposed default, planned state from scenario records only, with the
  native claims unread: a native-only consumer cannot reach the gate.
- Reading the claims from the artifact's `catalog` (0201-F7 B-030, decision
  0003): the owner calls the export "uncessary coupling" (0201-F7 decision
  0004).
- Widening item 0302 to build the reading. The owner chose "Cut items, block
  0302" (0302's decisions, 2026-11-06).

## Affects

0201-F5 OQ-3, OQ-5, § 3, § 5 rows 5 and 6, § 7, B-018, B-019; 0201-F7 decision
0004, § 5 row 11, B-026, B-030; items 0302, 0409 and 0411.

## Reversal

None.
