---
title: "SPEC031: Every claim has one traceability row"
description: A § 3 claim has no row or several rows in § 9, or § 9 has a row for an id that is not a § 3 claim.
type: rule
---

# SPEC031: Every claim has one traceability row

## Metadata

| Property         | Value   |
| ---------------- | ------- |
| Rule ID          | SPEC031 |
| Family           | Claims  |
| Default severity | Error   |
| Schema version   | 1       |

## Cause

The rows of the table in a Feature specification's matrix section do not correspond one to one with the claims in the table of its claims section. The two sections are the ones the manifest's `matrix` and `claims` roles name, `## 9. Traceability Matrix` and `## 3. Acceptance Criteria` in the default manifest; the messages call them § 9 and § 3 whatever their titles, and so does this page. The rule reports three conditions:

| Condition                                | Line reported                 | Message                                                                                                                    |
| ---------------------------------------- | ----------------------------- | -------------------------------------------------------------------------------------------------------------------------- |
| A § 3 claim has no row in § 9            | The claim's row in § 3        | `claim '{id}' has no row in § 9 - every § 3 claim appears there exactly once, with 'Missing' as its test until one exists` |
| A § 3 claim has more than one row in § 9 | The claim's second row in § 9 | `claim '{id}' has {count} rows in § 9 - expected exactly one`                                                              |
| A § 9 row's id is not a claim in § 3     | The first such row in § 9     | `§ 9 cites '{id}', which is not a claim in § 3 - a withdrawn claim is marked Withdrawn, not deleted`                       |

A row is matched to a claim by its first cell, compared exactly after surrounding spaces are removed. Rows with an empty first cell are skipped. The claims are the § 3 ids that are well formed and counted once; an id SPEC030 reports as malformed is not a claim, so its § 9 row is reported as citing a non-claim.

What the other cells of a § 9 row hold is not checked here. In particular a test or status of `Missing` - the manifest's `missing` marker, by default - is not a violation of this rule: it is the honest value until a test exists. A claim's Status in § 3 is not read either, so a claim marked Withdrawn still needs its row.

The rule needs both sections. A specification with no § 3 or no § 9 is not reported here; SPEC010 reports the missing section.

## Rule description

§ 9 is the specification's account of its own coverage: one row per claim, saying which scenario states it and which test proves it. One row each, in both directions, is what lets the table be read as that account. A claim with no row is coverage nobody is tracking; a claim with two rows has two answers; a row for an id § 3 does not declare is coverage of nothing.

The row is written when the claim is, not when the test is. Until a test exists the row says `Missing`, which keeps the gap visible. A specification with `spec_status: approved` may not carry one (SPEC060).

### Example violation

A claim added to § 3 with no row in § 9:

```markdown
## 3. Acceptance Criteria

| ID    | Claim                                   | Source | Status |
| ----- | --------------------------------------- | ------ | ------ |
| B-001 | An order with no lines is rejected.     | need 1 | Active |
| B-002 | An order total is the sum of its lines. | need 1 | Active |

## 9. Traceability Matrix

| Claim ID | Scenario                   | Test    | Status  |
| -------- | -------------------------- | ------- | ------- |
| B-001    | An empty order is rejected | Missing | Missing |
```

```text
src/orders/.spec/README.md(43): error SPEC031: claim 'B-002' has no row in § 9 - every § 3 claim appears there exactly once, with 'Missing' as its test until one exists
```

### Corrected

```markdown
## 9. Traceability Matrix

| Claim ID | Scenario                        | Test    | Status  |
| -------- | ------------------------------- | ------- | ------- |
| B-001    | An empty order is rejected      | Missing | Missing |
| B-002    | A total is the sum of its lines | Missing | Missing |
```

## How to fix violations

- **No row.** Add a row to § 9 whose first cell is the claim's id, with the scenario that states it and `Missing` for the test and the status until a test exists.
- **Several rows.** Merge them into one. A claim proved by several scenarios or tests lists them all in the one row's cells.
- **A row for a non-claim.** Two fixes are valid, and which applies depends on what happened to the claim.
  - If the claim was deleted from § 3, restore it there and mark its Status Withdrawn. A claim id is permanent; a withdrawn claim keeps its row in § 3 and its row in § 9.
  - If the row was never a claim - a mistyped id, or a row copied from another specification - correct the id to the claim it was meant for, or remove the row.

If the message names an id that does appear in § 3, compare the two spellings: `B-7` in § 3 and `B-007` in § 9 are different ids, and SPEC030 reports the first.
