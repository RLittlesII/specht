---
title: "Decision 0004: Owned-key gate mechanics"
description: "What the reconciler gates, reports and fails for owned, unowned and planned case keys"
type: decision
---

# Decision 0004: Owned-key gate mechanics

**Date:** 2026-10-24
**Decided by:** Repository owner (design pass); recorded by spec-author

## The call

Decision 0003 states the rule; this note states how the reconciler applies it.
Section 7 belongs to the implementer, who restates it from this note.

1. **Gated set.** For each required test link, take the cases of its bound
   methods per project/TFM artifact, in that artifact's own selected run. A case is
   gated when `owned != false` and it is not planned. Version 1 and 2 artifacts
   carry no ownership and count as owned. An owned case that is unselected, has no
   result or has a nonpassing outcome is a `missingResult` finding (exit 1).
   This amends the rule in § 7 that a `selected: false` case is not gated;
   this note takes precedence for the gate. `incompleteRun` stays informational.
2. **Unowned cases.** Never gated, never listed in `claims[].cases`, never counted
   in the summary's passed or failed counts.
3. **Planned cases.** A case is planned when every current scenario record link
   (artifact `scenarios`, `cases[].state == "planned"`, resolved `caseId`) to it
   is planned; one active link makes it active. A planned case is reported with
   `planned: true` on its case row (case rows also gain `owned`), is not in
   `claims[].cases`, covers nothing and is never gated. An active row of the same
   outline with no result, a skipped result or a failed one still fails (B-019).
   A planned link with no `caseId` names no case and gates nothing.
4. **Planned-only claim.** The claim stays required (0201-F7 B-024). A claim whose
   only cases are planned has no covering case, so the existing no-case
   `missingResult` applies.
5. **No owned case.** A bound method whose cases are all unowned in every
   artifact, or all planned, gets `missingResult` "has no owned case".
6. **Omitted owning run (B-022).** A declared project/TFM with no current
   evidence already yields `missingCompilation` (exit 2, incomplete). Its owned
   keys cannot be known without its artifact, so the gate fails with exit 2
   naming the project. No new finding kind, and the keys are not reported as
   passed. B-022 was reworded to say exit 2.
7. **Planned source.** Planned state comes from scenario records only. The
   reconciler does not read the claim catalogue; § 7 describes
   `claimSource: "gherkin"` but it is not built (OQ-3).

## Why

The gate must not let a key owned elsewhere fail or satisfy another project
(decision 0003). Treating owned-but-unselected as a failure is what makes a
narrowed run fail closed; `incompleteRun` alone never changed the exit code.

## Rejected

- A new finding kind for an omitted owning run: `missingCompilation` already
  carries exit 2 and names the project.
- Failing a planned-only claim as a planned finding: the claim already has no
  covering case, so the existing finding states it.

## Affects

B-017 to B-022; § 7 sentences on `selected: false`, `incompleteRun` and
"the gate on it is item 0262's" (implementer restates).

TestEvidence § 7 is not unchanged. Its B-026 paragraph (line 493 at this
writing) says: "In this item it is reported and does not change the exit code;
gating on owned keys is item 0262 (0201-F5 decision 0003)." That sentence is
superseded for the gate: an owned unselected case bound to a required claim is a
`missingResult` and exits 1, while `incompleteRun` itself stays an informational
finding. This pull request cannot edit that README, which another session owns,
so the TestEvidence owner must reword the sentence; Reconciliation § 10 records
the hand-off and OQ-4 tracks it. The TestEvidence acceptance step
`ThenIncompleteRun` now expects exit 1 for this reason.

## Reversal

None.
