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

The rows of the matrix table do not match the claims of the claims table one to one, by first cell. The two sections are the ones the manifest's `matrix` and `claims` roles name, `## 9. Traceability Matrix` and `## 3. Acceptance Criteria` in the default manifest; the messages say § 9 and § 3 whatever their titles.

| Condition                         | Line reported              | Message                                                                                                                    |
| --------------------------------- | -------------------------- | -------------------------------------------------------------------------------------------------------------------------- |
| A claim has no row in § 9         | The claim's row in § 3     | `claim '{id}' has no row in § 9 - every § 3 claim appears there exactly once, with 'Missing' as its test until one exists` |
| A claim has several rows in § 9   | Its second row in § 9      | `claim '{id}' has {count} rows in § 9 - expected exactly one`                                                              |
| A § 9 row's id is not a § 3 claim | That id's first row in § 9 | `§ 9 cites '{id}', which is not a claim in § 3 - a withdrawn claim is marked Withdrawn, not deleted`                       |

## Rule description

§ 9 has exactly one row for each § 3 claim and no row for anything else. A `Missing` cell is not a violation of this rule.

### Example violation

§ 3 declares `B-001` and `B-002`. § 9 is:

```markdown
| Claim ID | Scenario                   | Test    | Status  |
| -------- | -------------------------- | ------- | ------- |
| B-001    | An empty order is rejected | Missing | Missing |
```

```text
src/orders/.spec/README.md(43): error SPEC031: claim 'B-002' has no row in § 9 - every § 3 claim appears there exactly once, with 'Missing' as its test until one exists
```

### Corrected

```markdown
| B-002 | A total is the sum of its lines | Missing | Missing |
```

## How to fix violations

- **No row.** Add a row to § 9 whose first cell is the claim's id, with `Missing` until a test exists.
- **Several rows.** Merge them into one.
- **A row for a non-claim.** Correct a mistyped id or remove the row; if the claim was deleted from § 3, restore it there marked Withdrawn.
