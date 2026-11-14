---
title: "Decision 0008: the status rule defers at any level, and an empty Scenario cell does not resolve"
description: "SPEC4010 stays silent at a row whenever SPEC4008's condition holds, whatever SPEC4008's level (B-029), and under matrix.scenarioTitles a Scenario cell naming no title does not resolve, with SPEC4007 unchanged (B-030)"
type: decision
---

# Decision 0008: the status rule defers at any level, and an empty Scenario cell does not resolve

**Date:** 2026-11-13
**Decided by:** the repository owner (2026-11-13), selecting an option on each of two questions relayed by the session coordinating item `0337`; recorded by spec-author

## The call

The owner selected, verbatim:

1. "(a) Silent at any level (Recommended)"
2. "(b) No, report it (Recommended)"

The questions were test-writer's cases 1 and 6 for item `0337`, which
decision 0007 put to the owner; their wording was the coordinating
session's, not the owner's. As relayed:

1. With `rules` setting `SPEC4008` to `off`, B-012's "a row `SPEC4008`
   reports" is its condition: `SPEC4010` stays silent whenever `SPEC4008`'s
   condition holds, whatever `SPEC4008`'s level. Claimed as B-029.
2. Under `matrix.scenarioTitles`, a Scenario cell that names no title does
   not resolve, so `SPEC4010` judges the row as one whose scenario does not
   resolve; `SPEC4007` is unchanged. Claimed as B-030, with A-1 amended.

## Why

Spec-author's account of the recommendations the owner selected:

- `SPEC4010`'s verdict then depends on the tree alone, read from the
  resolver answer `SPEC4008` reads (C-3), and setting `SPEC4008` to `off`
  silences the missing scenario rather than moving it to another id.
- Under title matching the Scenario cell anchors the row, and a covered row
  that names no scenario overclaims, as the owner ruled for a covered row
  that names no test (decision 0001 call 5). `SPEC4007` is in the
  published schema `0.3.0` and cannot change its verdict there (`0001-F7`
  C-1).

## Rejected

Spec-author's reading of the options the owner did not select:

- **`SPEC4010` reports when `SPEC4008` is `off`.** One rule's verdict would
  depend on another's level, and the missing scenario would resurface as an
  overclaimed status.
- **An empty cell resolves on the tag alone.** A-1 read literally; a covered
  row could name no scenario under the setting that makes the cell an
  anchor.

## Affects

- B-029 and B-030, new in § 3, each citing this decision; their scenarios
  and § 9 rows, `Missing` until test-writer binds them.
- A-1, amended. § 5 row 12: `SPEC4007` reporting a cell that names no
  title.
- Item `0337`'s `claims`.

Spec-author's readings, not stated by the owner:

- **B-029 is its own claim.** B-012 states the silence; B-029 states that
  `SPEC4008`'s level does not move it, a separate assertion with its own
  test. B-012's text is the owner's of 2026-10-14 and is unchanged.
- **B-030 states resolution, not a report.** What `SPEC4010` reports
  follows from B-013 and B-014: a covered row is reported, and a partial
  row only when no test name in it resolves either.
- **"Names no title" is A-1's split.** An empty cell and one holding only
  `;` and spaces both name no title.

## Reversal

None.
