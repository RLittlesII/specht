---
title: "Decision 0001: the matrix is resolved as text against scenarios and declared test files"
description: "Under schema 0.2.0 or later the check resolves each § 9 row's scenario against the companion scenarios and its test names against the test files the manifest declares, as text, through six new rules that land in the unpublished schema 0.2.0"
type: decision
---

# Decision 0001: the matrix is resolved as text against scenarios and declared test files

**Date:** 2026-10-14
**Decided by:** the repository owner (2026-10-14); recorded by spec-author

## The call

1. Under schema `0.2.0` or later the check reads the § 9 columns by position
   and resolves each row: its scenario against the companion scenarios, by
   the claim tag and, when the manifest asks, by title; each test name in its
   Test cell against the test files the manifest declares, in the forms the
   manifest declares.
2. Six rules report what does not resolve: `SPEC022` a Scenario cell,
   `SPEC023` a live claim no scenario carries, `SPEC032` a test name,
   `SPEC033` a status, `SPEC034` a waiver and `SPEC035` a withdrawn or
   superseded claim. The first four are errors by default; the last two are
   off until a manifest raises them.
3. Resolution is text only. Nothing is compiled, run or fetched.
4. The rules land in schema `0.2.0`, which has never shipped in a published
   package: tag `v0.1.90` carries `0.1.0` alone. `0.1.0` keeps its 21 ids,
   and `0.2.0` is relocked when the rules land (`0001-F7` decision 0014).
5. Added by the owner on 2026-10-15: a covered row names at least one test,
   and every name it names resolves; a covered row whose Test cell is the
   missing marker is reported (B-013, B-016).

## Why

- A § 9 status is a statement about the tree that no gate reads today: a
  `Covered` row may name a test no file holds, and a `Missing` row may name
  tests that all exist.
- A text resolver keeps the check deterministic and offline (brief § 9), and
  one resolver gives `0001-F13` the same answer the check gives (C-3).
- `0.2.0` is unpublished, so adding to its vocabulary moves no consumer's
  verdict; a `0.3.0` would ship a version no one asked for.

## Rejected

Recorded by spec-author as what the call rules out, not as options put to
the owner:

- Running or compiling the tests to find them: rules out a deterministic,
  offline check (C-1).
- Matching a Scenario cell to a title by default: the template anchors a row
  to its tag, and epic `0201` R4 checks bindings against records, not titles
  (B-010).
- A new schema version `0.3.0` for the rules: `0.2.0` is unpublished and may
  be relocked (`0001-F7` decision 0014).

## Affects

- B-001 to B-021; A-1 to A-4; C-1 to C-5; § 5 rows 1 to 9; OQ-1 to OQ-3.
- `0001-F5` B-055 to B-058 and B-061; `0001-F3` B-036, B-037 and B-039 to
  B-042; `0001-F7` B-115, decision 0014; `0001-F13`.

## Reversal

None.
