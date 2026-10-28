---
title: "SPEC034: A waiver names a signed-off approver and date"
description: A waived matrix row has no valid dated approver matching a completed sign-off row.
type: rule
---

# SPEC034: A waiver names a signed-off approver and date

## Metadata

| Property         | Value   |
| ---------------- | ------- |
| Rule ID          | SPEC034 |
| Family           | Claims  |
| Default severity | Off     |
| Schema version   | 0.2.1   |

## Cause

A row marked with the manifest's waived marker has no `<approver>, <YYYY-MM-DD>` in its Test cell, the date is invalid, or that approver has no sign-off row whose status is neither the draft nor blocked marker. The diagnostic names the claim at its matrix row.

## Rule description

The matrix and sign-off sections follow their manifest roles. Matrix columns are read by position; sign-off status and reviewer are the second and third columns of `tables.signOff`. Renaming headers preserves those roles. Dates use the invariant calendar form; no current date is consulted.

### Example violation

The matrix has `B-001 | A waiver | spec-reviewer | Waived`.

```text
src/sample/.spec/README.md(43): error SPEC034: waiver must name a signed-off approver and date as '<approver>, <YYYY-MM-DD>' [B-001]
```

### Corrected

The Test cell is `spec-reviewer, 2026-10-14`, and the sign-off table has that reviewer with a completed status.

## How to fix violations

Record the waiver's approver and valid date, and obtain that approver's completed sign-off.
