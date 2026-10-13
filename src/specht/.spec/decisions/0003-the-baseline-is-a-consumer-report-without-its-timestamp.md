---
title: "Decision 0003: the baseline is a consumer report without its timestamp"
description: "The baseline every later Feature is measured against is the baseline report before the import with generatedAtUtc removed, committed in this repository; a consumer's tree is read in place, never copied in as a fixture"
type: decision
---

# Decision 0003: the baseline is a consumer report without its timestamp

**Date:** 2026-10-07
**Decided by:** the repository owner (brief § 6, "Report timestamp"); settled across `0001-F1` and `0001-F5` by the coordinator (S7); recorded by spec-author
**Status:** superseded 2026-10-08 by decision 0004 - see Reversal

## The call

The baseline report is the report the baseline engine writes on a consumer's tree
before the import, with `generatedAtUtc` removed. It is committed at
`docs/reference/consumer-baseline-report.json` in the brief § 8 step 2 commit.
B-004 compares **verdicts** against it, not whole reports: each violation's
rule id, severity, file, line, identifier and message, and their order (the
C-9 field list). `generatedAtUtc` and every field `0001-F3` adds at brief § 8
step 3 (`expected`, counts, layouts, schema source) are outside the
comparison. `0001-F5` B-016 compares the default manifest's verdicts with the
same file on the same definition (settlement S9).

A consumer's tree is the input and is read from a checkout before the import. It is
never copied into this repository as a test fixture. Where that comparison
runs, and how a run without the checkout reports it, is `test-writer`'s § 8.

## Why

- `0001-F3` drops `generatedAtUtc` (its decision 0001). A baseline that kept
  it would make every later comparison "all fields but one", with an exclusion
  list to maintain.
- Committing the expected report gives B-004 and `0001-F5` B-016 one named
  file to agree on, instead of each re-running the baseline engine.

- A whole-report comparison breaks at step 3: once `0001-F3` adds `expected`
  and other fields, no later report equals the consumer's, with or without the
  timestamp. The verdict is what Should-7 protects.

## Rejected

- Whole-report equality minus `generatedAtUtc`: fails from step 3 onward
  (settlement S9, raised by the `0001-F5` author).

- A synthesised baseline over the 56 tests' fixture tree: Should-7 names
  the baseline run, and a synthetic tree cannot show it unchanged.
- Copying the consumer's `.spec/` tree in as a fixture: a fixture copied from a
  real repository's specifications breaks `test-from-scenarios` § "Fixtures
  are synthetic".
- Restating B-004 as a constraint: `0001-F5` B-016 cites the baseline B-004
  names, so B-004 keeps its id.

## Affects

`B-004`; `C-9`; `0001-F5` B-016 (cites both; not edited here).

## Reversal

Superseded 2026-10-08 by decision 0004, the owner: B-004's input is a
synthetic tree the tests build, and its golden report is the engine's output on
it at commit `e7dba24`, committed at
`test/specht.tests/Baseline/engine-e7dba24.json`. A consumer's tree is not a
test input, and `docs/reference/consumer-baseline-report.json` is never
committed. The verdict field list and the exclusion of `generatedAtUtc` and
`0001-F3`'s fields stand.
