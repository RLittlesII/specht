---
title: "Decision 0010: item 0008 proceeds with the dialect questions open"
description: "Git mode passes each glob to git as a prefilter and keeps Pattern the one reader of the glob dialect, so item 0008 is built with OQ-7 (d) and OQ-8 open; neither question is answered and the owner was not asked"
type: decision
---

# Decision 0010: item 0008 proceeds with the dialect questions open

**Date:** 2026-11-09
**Decided by:** spec-author, under `deliver-change` rule 4, from § 7 "Design for item `0008`" (implementer, 2026-11-09). The repository owner was not asked, and neither question is answered.

## The call

Item `0008` is built with § 11 OQ-7 (d) and OQ-8 open. Both Blocks cells
said each must be answered before item `0008` passes a glob to git; this
record is the exception `deliver-change` rule 4 allows, and names what is
assumed.

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

## Why

- C-6 rules out a glob one mode reads differently from the other. With one
  reader, both modes give item `0005`'s reading today, whatever the answers.
- The facts the questions turn on are git's, and § 7 observed them.
- Item `0008`'s claims, B-005, B-006 and B-010, state no glob token; both
  questions are about the dialect, which is B-001's and C-6's.

## Rejected

**Holding item `0008` until the owner answers.** Cost of rejecting: item
`0008` builds on the dialect as item `0005` built it, and an answer later
moves `Pattern` and, for OQ-7 (d) as wildcards, the escape table.

**Git as the reader in git mode.** Passing globs unescaped and keeping git's
list unfiltered would answer both questions by assumption, git's way, in one
mode only, which C-6 rules out.

## Affects

- § 11 OQ-7 (d) and OQ-8: their Blocks cells cite this record; both stay
  open.
- B-005, B-010, C-3 and C-6: unchanged.
- Not decided here: what `?`, `[...]` and a final `**` mean (OQ-7 (d),
  OQ-8).

## Reversal

None.
