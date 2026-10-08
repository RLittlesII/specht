---
title: "Decision 0002: the copy is checked once at the baseline tag, and the verdicts hold until manifest roles"
description: "B-009's diff becomes a one-time C-2 check recorded at the baseline tag, scoped to the copied files and listing the permitted .csproj edits; C-9 freezes verdicts, not files, until brief § 8 step 5"
type: decision
---

# Decision 0002: the copy is checked once at the baseline tag, and the verdicts hold until manifest roles

**Date:** 2026-10-07
**Decided by:** spec-author, on the spec-reviewer's findings of 2026-10-07 and the coordinator's settlement S6

## The call

1. **B-009 is withdrawn.** "A diff against `hooked` shows the namespace rename
   and nothing else" is a one-time comparison with another repository; no test
   in this suite can fail it. It becomes C-2, checked once at the brief § 8
   step 2 tag, with the diff recorded in that commit.
2. **C-2 names what the copy may differ by:** the namespace rename in each
   copied `*.cs` file; `AssemblyName` and `RootNamespace` set to `specht` and
   the comments about `hooked`'s `Directory.Build.props` dropped in the copied
   `.csproj`; the copied `.spec/` removed. These are brief § 3's own edits and
   B-005's rename. `src/specht/.spec/`, which holds this specification, is not
   part of the copy.
3. **C-9 freezes verdicts, not files.** From the baseline tag to brief § 8
   step 5, no edit changes a violation's rule id, severity, file, line,
   identifier, message or order. `0001-F3` may reshape the report around them at
   step 3 (drop `generatedAtUtc`, add `expected`).
4. **The tag is `baseline/hooked-6afe8ab`** (added 2026-10-08), on the commit
   that lands the copy on `main` - item 0021's merge commit, not a branch
   commit a squash merge discards. Item 0022 places it.

## Why

- The earlier C-2 said "before step 5 the only edit to a copied file is the
  namespace". That was false three ways: brief § 3 drops `.csproj` comments
  and removes the copied `.spec/`; B-005 renames the assembly, a `.csproj`
  edit; and `0001-F3` edits the report and violation types at step 3, two
  steps before step 5.
- What the baseline protects is the verdict (Should-7). Freezing files beyond
  step 2 protects nothing more and blocks `0001-F3`.

## Rejected

- Keeping B-009 as a claim with a test that reads `hooked`'s checkout: a test
  whose input is another repository's working copy fails for reasons that are
  not this Feature's.
- Freezing every copied file until step 5: contradicts `0001-F3` B-006 and
  B-007 at step 3.

## Affects

`B-009` (withdrawn), `C-2`, `C-9` (new), § 5 rows 2 and 4.

## Reversal

None.
