---
title: "Decision 0004: check the merge tree on a pull request"
description: "On a pull request a run checks the specification tree twice, the head and the merge with main, so a violation only the merge carries is reported before the merge; specht stays a check over one tree; B-017 to B-022, C-7 to C-10 and § 5 #21 to #26 record it"
type: decision
---

# Decision 0004: check the merge tree on a pull request

**Date:** 2026-10-09
**Decided by:** the repository owner, 2026-10-09 (item 0128, decision 4); recorded by spec-author

## The call

- On a pull request a run checks the specification tree a second time, over
  the pull request's merge tree: its head merged into `main` as the run
  obtained `main` (A-3, B-017).
- `specht` stays a deterministic, offline check over one tree. It gains no
  base, diff or pull-request input; the build hands it a second tree (C-7).
- A push to `main` has no second run: the pushed commit is the merged tree
  (B-018).

Derived by spec-author from claims already agreed, not stated by the owner:

- The second run is on ubuntu only and only where the pull request is built,
  since a pull request runs no target on windows (B-015) and none at all when
  B-012 applies.
- The head stays what every other gate builds (B-001, C-8).
- The second run annotates nothing; the head run alone annotates (B-006,
  B-020, § 5 #16). OQ-4 asks the owner whether that is enough.
- The second run is part of the ubuntu check and gates when `SpecCheck` does
  (C-4, C-10), so no check name is added to the two `0055-F8` B-002 requires.
- A merge tree that cannot be computed is reported, never passed as clean
  (B-019). Whether it fails the check once `SpecCheck` gates is OQ-3.

## Why

Two branches cut from one `main` each take the same next id, and git merges
two records that share a number under different file names without a conflict
(lesson 0005). One duplicate reached `main` that way: the two work items
numbered 0118.

Rules that report an id claimed twice inside one tree are proposed for epic
`0101` and are not yet specified. A pull request's run checks the pull
request's head (B-001), so such a rule never sees `main` and the branch
together there, and the collision first appears on `main`, after the merge.
In the merge tree the collision is two records in one tree, which a
single-tree rule can report.

Until one of those rules exists, the second run reports only what today's
rules find in the merge tree.

## Rejected

**A base or diff option on `specht`.** Cost of rejecting: the tool alone cannot
see a collision between a branch and its base, so a consumer gets this only
by checking a merge tree in its own build, as this repository does. Rejected
because the tool would call git for more than the file list (`0001-F6` C-2),
and the same tree would give different reports depending on a second input,
against "the same tree gives the same report" (AGENTS.md § Invariants).

**Catching the collision only on the push to `main`.** Cost of rejecting: a
second self-check on every pull request's ubuntu leg. The `SpecCheck` step
took about 10 s on the windows leg of the run decision 0003 measured; the
merge-tree run was not measured. Rejected because the duplicate is then
already on `main`, where an id is permanent and one of the two records has to
be renumbered against AGENTS.md § Stable IDs.

## Affects

- B-017 to B-022 (added).
- C-7 to C-10 (added); A-3; § 2 #9; § 1 and the description.
- § 5 #21 to #26 (added).
- OQ-3, OQ-4 (raised).
- No existing claim or constraint is amended: B-001, B-006, C-3, C-4 and C-6
  hold as written.
- The `@B-017` to `@B-022` scenarios and a `@B-001 @boundary` scenario.

## Reversal

None.
