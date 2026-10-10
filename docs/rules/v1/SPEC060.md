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

A Feature specification's `spec_status` is the approved value and a cell of its matrix table is exactly the missing marker. The manifest sets all three:

| What               | Manifest key       | Default manifest            |
| ------------------ | ------------------ | --------------------------- |
| The approved value | `markers.approved` | `approved`                  |
| The missing marker | `markers.missing`  | `Missing`                   |
| The matrix section | `roles.matrix`     | `## 9. Traceability Matrix` |

Reported once per such row, at the row, in fixed text whatever the manifest gives:

`spec_status is 'approved' while § 9 still carries a 'Missing' cell`

## Rule description

An approved specification's matrix has no `Missing` cell. Under any other `spec_status`, `Missing` cells are not reported.

### Example violation

A specification with `spec_status: approved` and this § 9 row:

```markdown
| B-002 | A total is the sum of its lines | Missing | Missing |
```

```text
src/orders/.spec/README.md(74): error SPEC060: spec_status is 'approved' while § 9 still carries a 'Missing' cell
```

### Corrected

```markdown
| B-002 | A total is the sum of its lines | `OrderTests.SumsLines` | Covered |
```

## How to fix violations

- **The test exists.** Replace `Missing` in the row with the test and its status.
- **The test does not exist.** Write it, or set `spec_status` back to `in-review`.
