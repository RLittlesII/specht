---
title: "Decision 0002: the trace reads every pin and takes a report path"
description: "Under a schema 0.1.0 pin the trace reads the tree with the 0.2.0 defaults; specht trace takes --report <path> on the check's contract, the one file it writes; a claim with no § 9 row reads none and 0/0; a waived, withdrawn or superseded row's tests are skipped"
type: decision
---

# Decision 0002: the trace reads every pin and takes a report path

**Date:** 2026-10-15
**Decided by:** the repository owner (2026-10-15, requirements session); recorded by spec-author

## The call

1. Under a manifest that pins schema `0.1.0`, the trace reads the tree with
   the `0.2.0` defaults: the § 9 columns by their position in
   `tables.matrix`, the covered, partial, waived, withdrawn and superseded
   markers at their `0.2.0` defaults (`0001-F5` B-058), and every test's
   resolution `unconfigured` (B-004). A-2 holds: the command runs the same
   under every shipped pin. This resolves OQ-2.
2. `specht trace` takes `--report <path>` now, on the check's `--report`
   contract (`0001-F3` B-003, B-004, B-022 to B-025, C-5, C-7): it writes
   the trace document to the path, creates the directories above it, and
   replaces a file that exists; without `--json`, stdout keeps the trace
   lines; with `--json`, the document on stdout and the file are identical;
   the exit code is the one the same run gives without it; and no other file
   is written. This resolves OQ-1 and withdraws § 5 row 4.
3. A claim with no § 9 row reads as B-001 and B-003 state it: the line shows
   the status `none` and the tests `0/0`, and the document leaves the row
   out. The owner confirmed spec-author's reading as written.

Further calls, after `spec-reviewer` round 3, the same day:

4. In a § 9 row whose Status is the waived, withdrawn or superseded marker,
   the trace does not resolve the tests the Test cell names, under every pin,
   as `0001-F12` B-002 skips them. This is what the marker reading of call 1
   decides in the output (B-011, B-019).
5. Such a test is listed with a fourth resolution, `skipped`, and no file or
   line; A-1 widens to `resolved`, `unresolved`, `unconfigured` and
   `skipped`. On the line B-001 prints, a `skipped` test counts in neither
   number (B-001, B-003).
6. `skipped` comes before `unconfigured`: a test in such a row is `skipped`
   whether or not the manifest declares `tests` (B-004, B-012, B-020). This is
   spec-author's reading of "under every pin" in call 4: a `0.1.0` manifest
   cannot declare `tests`, so call 4 holds under that pin only if `skipped`
   wins over `unconfigured`, and a `0.2.0` manifest with no `tests` key is
   read the same way.

## Why

Recorded by spec-author from the session:

- One reading under every pin keeps A-2 true: a tree that has not moved its
  pin is traced like any other, and a `0.1.0` manifest cannot declare
  `tests` (`0001-F5` B-055), so `unconfigured` is the only honest
  resolution there for a test the trace resolves; a test in a waived,
  withdrawn or superseded row is `skipped` instead (calls 4 to 6).
- A script or dashboard that wants the document and the lines from one run
  gets both, as it does from the check; copying the check's contract leaves
  one behaviour to learn and one to test.
- The `--report` file is the caller's own request, as it is for the check,
  so it is the one write the read-only command allows.

## Rejected

The owner was offered these and chose against them; the cost after each is
spec-author's reading:

- **Printing the raw status cells under `0.1.0`, without reading the
  markers.** The trace would then read the same row two ways across pins.
- **Refusing a `0.1.0` tree with a non-zero exit.** It breaks A-2 and the
  exit-`0` contract of B-006 for a tree the check accepts.
- **Never overwriting an existing report file.** A second run would fail or
  leave a stale document; the check replaces it (`0001-F3` B-024).
- **Leaving the tests of a waived, withdrawn or superseded row out of the
  trace entirely.** A reader could not tell a skipped test from one the row
  never named.
- **A normalized marker field in the document.** The row's four cells
  already carry the status; `skipped` shows what the marker decided.
- **Withdrawing B-011.** It now states what the marker reading decides.
- **The report file replacing stdout.** The check keeps its lines on stdout
  beside the file (`0001-F3` B-025); the trace does the same.

## Affects

- B-001, B-003, B-004, B-008; B-010 to B-020; A-1, A-2; C-2; § 5 rows 4, 6,
  7 and 8; OQ-1, OQ-2.
- `0001-F12` B-002, as the rows whose tests are not resolved.
- `0001-F3` B-003, B-004, B-022 to B-025, C-5, C-7, as the contract copied.

## Reversal

None.
