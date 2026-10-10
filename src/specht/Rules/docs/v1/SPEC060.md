---
title: "SPEC060: An approved specification has no missing coverage"
description: A specification whose spec_status is approved still has a Missing cell in its traceability matrix.
type: rule
---

# SPEC060: An approved specification has no missing coverage

## Metadata

| Property         | Value    |
| ---------------- | -------- |
| Rule ID          | SPEC060  |
| Family           | Approval |
| Default severity | Error    |
| Schema version   | 1        |

## Cause

A Feature specification's frontmatter has `spec_status: approved`, and a row of its `## 9. Traceability Matrix` table has a cell that reads `Missing`. The rule reports once for each such row, at the row's line, with the message:

`spec_status is 'approved' while § 9 still carries a 'Missing' cell`

The rule applies only when `spec_status` is exactly `approved`. A specification with any other status - `draft`, `in-review`, `superseded` - may carry any number of `Missing` cells and is never reported.

A cell counts when its whole text, with surrounding spaces removed, is the word `Missing` with that capital. Any column of the row can hold it; a row with `Missing` in two columns is reported once. A cell that only mentions the word in a longer note does not count. The table read is the first table of § 9, and a specification with no § 9 is not reported here.

## Rule description

`Missing` is the honest value of a § 9 cell until a test exists: SPEC031 asks for a row per claim from the day the claim is written, and the row says `Missing` so the gap stays visible. While a specification is being drafted and reviewed, that is the expected state.

`approved` is the point at which the gap stops being acceptable. An approved specification that still says `Missing` claims an agreement whose claims are not all proved, and this rule is where the two statements are compared.

### Example violation

A specification with `spec_status: approved` in its frontmatter and this matrix:

```markdown
## 9. Traceability Matrix

| Claim ID | Scenario                        | Test                      | Status  |
| -------- | ------------------------------- | ------------------------- | ------- |
| B-001    | An empty order is rejected      | `OrderTests.RejectsEmpty` | Covered |
| B-002    | A total is the sum of its lines | Missing                   | Missing |
```

```text
src/orders/.spec/README.md(74): error SPEC060: spec_status is 'approved' while § 9 still carries a 'Missing' cell
```

### Corrected

```markdown
## 9. Traceability Matrix

| Claim ID | Scenario                        | Test                      | Status  |
| -------- | ------------------------------- | ------------------------- | ------- |
| B-001    | An empty order is rejected      | `OrderTests.RejectsEmpty` | Covered |
| B-002    | A total is the sum of its lines | `OrderTests.SumsLines`    | Covered |
```

## How to fix violations

Two fixes are valid, and which applies depends on whether the test exists.

- **The test exists and the row was not updated.** Replace `Missing` in the row with the test that proves the claim and the status it now has.
- **The test does not exist.** The specification is not yet at the state `approved` describes. Write the test and record it in the row, or set `spec_status` back to `in-review` until it exists.

Do not clear the line by rewording the cell - `missing`, `TBD`, a dash - or by deleting the row. A reworded cell hides the gap this rule reports, and a deleted row is reported by SPEC031.
