---
title: "Decision 0010: item 0008 proceeds with the dialect questions and OQ-2 open"
description: "Git mode passes each glob to git as a prefilter and keeps Pattern the one reader of the glob dialect, and walks when git exits other than 0, so item 0008 is built with OQ-2, OQ-7 (d) and OQ-8 open; none is answered and the owner was not asked"
type: decision
---

# Decision 0010: item 0008 proceeds with the dialect questions and OQ-2 open

**Date:** 2026-11-09
**Decided by:** spec-author, under `deliver-change` rule 4, from § 7 "Design for item `0008`" (implementer, 2026-11-09); extended the same day to OQ-2 on `spec-reviewer`'s § 12 finding 2 for item `0008`. The repository owner was not asked, and no question is answered.

## The call

Item `0008` is built with § 11 OQ-2, OQ-7 (d) and OQ-8 open. OQ-7's and
OQ-8's Blocks cells said each must be answered before item `0008` passes a
glob to git, and the item's `out_of_scope` said OQ-2's path is not built
before a claim exists; this record is the exception `deliver-change` rule 4
allows, and names what is assumed.

## Assumed

1. **Git reads no glob for the verdict.** Each glob reaches git as a
   `:(glob)` pathspec with `\`, `?` and `[` escaped, and git's list is a
   prefilter: `Matching` filters it through `Pattern` as it filters the
   walk's list. `Pattern` stays the one reader of the dialect (C-6), in both
   modes.
2. **Git's list holds every file `Pattern` matches.** Escaped, `?` and `[`
   are literals to git as to `Pattern`; `*` and a `**` segment read the same;
   a final `**`, as `docs/**`, git reads more widely, and `Pattern` drops the
   surplus. Observed with git 2.50.1 (§ 7). A git that listed fewer files
   would break C-3, and no claim pins this.
3. **An answer changes `Pattern` for both modes at once.** Either answer to
   OQ-8 and a rejection under OQ-7 (d) need no change to what git is passed.
   An answer making `?` or `[...]` wildcards also drops their escapes in the
   pathspec, or git's list loses files `Pattern` keeps.
4. **A git that exits other than `0` inside a work tree gives the walk.**
   `Listed` gives `null`, the walk runs as item `0007` built it, nothing is
   printed, and git's standard error is dropped. No claim and no test pins
   it. Any answer to OQ-2 changes only `Listed`'s fallback.

## Why

- C-6 rules out a glob one mode reads differently from the other. With one
  reader, both modes give item `0005`'s reading today, whatever the answers.
- The facts the dialect questions turn on are git's, and § 7 observed them.
- Item `0008`'s claims, B-005, B-006 and B-010, state no glob token and no
  failing git; the dialect questions are B-001's and C-6's, and OQ-2 asks
  for a claim of its own.
- Of OQ-2's answers, the walk is the one that adds no exit code and no
  output, so nothing a consumer reads depends on the assumption.

## Rejected

**Holding item `0008` until the owner answers.** Cost of rejecting: item
`0008` builds on the dialect as item `0005` built it, and an answer later
moves `Pattern` and, for OQ-7 (d) as wildcards, the escape table; an answer
to OQ-2 that fails the run adds its exit code in `Listed`'s fallback.

**Git as the reader in git mode.** Passing globs unescaped and keeping git's
list unfiltered would answer both dialect questions by assumption, git's
way, in one mode only, which C-6 rules out.

**Failing the run when git exits other than `0`.** It would answer OQ-2 by
assumption and choose an exit code no claim states.

## Affects

- § 11 OQ-2, OQ-7 (d) and OQ-8: their Blocks cells cite this record; all
  three stay open.
- Item `0008`'s `out_of_scope`: OQ-2's path walks as built, unpinned.
- B-005, B-006, B-010, C-3 and C-6: unchanged.
- Not decided here: what `?`, `[...]` and a final `**` mean (OQ-7 (d),
  OQ-8), and what a failing git means (OQ-2).

## Reversal

None.
