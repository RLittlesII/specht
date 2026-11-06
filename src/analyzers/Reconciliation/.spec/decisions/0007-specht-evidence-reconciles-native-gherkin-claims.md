---
title: "Decision 0007: specht evidence reconciles native Gherkin claims"
description: "The owner puts native Gherkin claim input in this Feature's scope, read from the catalogue in the evidence artifact, resolving OQ-3"
type: decision
---

# Decision 0007: specht evidence reconciles native Gherkin claims

**Date:** 2026-11-06
**Decided by:** Repository owner; recorded by spec-author

## The call

1. `specht evidence` reconciles native Gherkin claims
   (`codeAnalysis.records.claimSource: "gherkin"`). The owner: "Yes, in scope".
2. The reconciler reads those claims, and their planned marks, from the native
   claim catalogue the analyzer exports in the test-evidence artifact
   (`catalog`; 0201-F7 B-026, B-030). It parses no `.feature` file; § 5 row 5
   stands.
3. OQ-3 is resolved by this call: for a consumer without scenario records, the
   catalogue supplies planned state.
4. Item 0409 writes the agreement: new § 3 claims, and the amendment of § 5
   row 6 that brings it into line with 0201-F7 § 5 row 11 and consumer request
   #256. This decision changes no claim.
5. Which declared projects the default gate needs a current export from is
   OQ-5. This decision does not settle it.

## Why

Item 0302's packaged consumer fixture (branch
`0302/packaged-gherkin-vstest-fixture`, commit `38670c0d`) has a Gherkin-only
manifest. `specht evidence` exits 3 with `invalidInput` on it: the policy
requires a mapped claims section, and the reconciler reads claims only from
Markdown claims tables. 0201-F7 § 5 row 11 already assigns reading the
catalogue to this Feature, and 0302 cites 0201-F7 B-030 for the gate reading
it; § 5 row 6 and OQ-3's proposed default left such a consumer with no claim to
reconcile.

## Rejected

- OQ-3's proposed default, planned state from scenario records only, with the
  catalogue unread: a native-only consumer cannot reach the gate.
- Widening item 0302 to build the reading. The owner chose "Cut items, block
  0302" (0302's decisions, 2026-11-06).

## Affects

0201-F5 OQ-3, OQ-5, § 3, § 5 row 6, § 7, B-018, B-019; 0201-F7 § 5 row 11,
B-026, B-030; items 0302 and 0409.

## Reversal

None.
