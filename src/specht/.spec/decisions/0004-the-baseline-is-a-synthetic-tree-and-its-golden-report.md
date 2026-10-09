---
title: "Decision 0004: the baseline is a synthetic tree and its golden report"
description: "B-004's input is a tree this repository's tests build, and its expected verdicts are the engine's output on it at commit e7dba24, committed as test data; supersedes decision 0003, and the commit citation replaces the baseline tag of decision 0002"
type: decision
---

# Decision 0004: the baseline is a synthetic tree and its golden report

**Date:** 2026-10-08
**Decided by:** the owner, during item 0022; recorded by spec-author

## The call

1. **B-004's input is synthetic.** The tests build the baseline tree with
   `SpecTree` (`test/specht.tests/SpecTree.cs`). It breaks each of the
   twenty-one rules, `SPEC001` to `SPEC061`, in each layout the rule applies
   to, and is never copied from a real `.spec/` tree.
2. **The golden report is the engine's output on that tree at commit
   `e7dba24`**, item 0021's merge commit on `main`, where the copy is a pure
   move. It is committed at `test/specht.tests/Baseline/engine-e7dba24.json`
   as test data: each violation's rule id, severity, file, line, identifier
   and message, in order (C-9). It is an intentional fixture, not a generated
   report, and not under `docs/reference/` or `.artifacts/`.
3. **No git tag is placed.** Wherever decision 0002 and the specifications
   name the baseline tag or `baseline/hooked-6afe8ab`, the start of C-9 and the
   commit C-2's diff is recorded against are cited as commit `e7dba24`.
4. **Provenance is unchanged.** The engine was copied from `hooked`
   `salvage/spec-check-engine` at `6afe8ab` (A-2). That is where the code came
   from, not a test input.
5. **`0001-F5` B-016 reuses this golden report** (guard item 0012), on the
   same verdict definition.

This supersedes decision 0003, and replaces item 4 of decision 0002.

## Why

- `specht` is upstream of `hooked`, its first consumer. A test here that reads
  `hooked`'s source inverts the dependency direction.
- `hooked` is a private repository. A test that needs its checkout cannot run
  in this repository's CI.
- C-9 protects the verdicts of the copy. A tree that breaks every rule in both
  layouts exercises every verdict the copy can give, and the engine's output at
  the commit where the copy is a pure move is that copy's verdict.

## Rejected

- A `hooked` checkout as the test input (decision 0003): inverts the
  dependency, and cannot run in CI without checking out a private repository.
- A git tag `baseline/hooked-6afe8ab`: a commit citation does the same job, as
  `REQUIREMENTS.md` is cited at commit `254aabc`, and needs no outward-facing
  ref to create and protect.
- Withdrawing B-004: leaves C-9 and guard 0012 (`0001-F5` B-016) with no
  baseline to hold the verdicts to.

## Affects

`B-004` (input and expected report replaced), `C-2` and `C-9` (commit
`e7dba24` in place of the tag), `C-10` (new), § 5 rows 8 and 11; decision 0002
item 4; decision 0003 (superseded); `0001-F5` B-016; items 0012, 0020, 0021,
0022, 0074 and 0100; `0055-F4` C-1, § 1 and § 5 row 3.

## Reversal

None.
