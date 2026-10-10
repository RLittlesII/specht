---
title: "Decision 0004: check the merge tree on a pull request"
description: "On a pull request a run checks the specification tree twice, the head and the merge with main, so a violation only the merge carries is reported before the merge, in the log and not on the diff; specht stays a check over one tree; B-017 to B-024, the B-006 amendment, C-7 to C-10 and § 5 #21 to #27 record it"
type: decision
---

# Decision 0004: check the merge tree on a pull request

**Date:** 2026-10-09
**Decided by:** the repository owner, 2026-10-09 (item 0128, decision 4; and
the answers to OQ-3 and OQ-4 the same day); recorded by spec-author

## The call

- On a pull request a run checks the specification tree a second time, over
  the pull request's merge tree: its head merged into `main` as the run
  obtained `main` (A-3, B-017).
- `specht` stays a deterministic, offline check over one tree. It gains no
  base, diff or pull-request input; the build hands it a second tree (C-7).
- A push to `main` has no second run: the pushed commit is the merged tree
  (B-018).

Answered by the owner, 2026-10-09, on the reviewer's Round 12:

- OQ-4: log only. A violation only the merge tree carries is printed in the
  log and is not annotated on the diff (B-020, § 5 #26). The owner accepted
  that B-006 is narrowed to violations in the head tree. Cost, as the
  question stated it: a reader must open the log to see the collision.
- OQ-3: no. Once `Specht` gates, a merge tree that cannot be computed is
  reported, "no merge tree was checked", and does not by itself fail the
  ubuntu check (B-019, B-024).

Derived by spec-author from claims already agreed, not stated by the owner:

- The second run is on ubuntu only and only where the pull request is built,
  since a pull request runs no target on windows (B-015) and none at all when
  B-012 applies. A pull request B-012 applies to therefore has no merge tree
  checked, one that adds only an `epics/<id>-*/epic.md` among them (§ 5 #27).
- The head stays what every other gate builds (B-001, C-8).
- The second run is part of the ubuntu check and gates when `Specht` does
  (C-4, C-10), so no check name is added to the two `0055-F8` B-002 requires;
  a violation it reports then fails that check (B-023).
- A merge tree that cannot be computed is reported, never passed as clean
  (B-019). GitHub starts no run for a pull request in conflict when the event
  fires, so this is reached only when `main` moves afterwards or git fails
  (A-4).

## Why

Two branches cut from one `main` each take the same next id, and git merges
two records that share a number under different file names without a conflict
(lesson 0005). One duplicate reached `main` that way: the two work items
numbered 0118.

Rules that report an id claimed twice inside one tree belong to epic `0101`.
`0101-F6` specifies, as a draft, those for a constraint id or an open-question
id declared twice in one specification; those for record numbers and for
work-item ids, proposed as `0101-F7` and `0101-F8`, are not yet specified. A
pull request's run checks the pull request's head (B-001), so such a rule
never sees `main` and the branch together there, and the collision first
appears on `main`, after the merge.
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
second self-check on every pull request's ubuntu leg. The `Specht` step
took about 10 s on the windows leg of the run decision 0003 measured; the
merge-tree run was not measured. Rejected because the duplicate is then
already on `main`, where an id is permanent and one of the two records has to
be renumbered against AGENTS.md § Stable IDs.

**Annotating the violations only the merge tree carries** (owner, 2026-10-09,
OQ-4: the owner chose log only). Cost of rejecting: such a violation is not on
the diff, and a reader must open the log to see it. The reason is the
question's, written by spec-author when asking and not stated by the owner: it
needs the two runs' violations de-duplicated and each line mapped from the
merge tree to the diff.

**Making the merge-tree run the one that annotates** (owner, 2026-10-09,
OQ-4: the owner chose log only). Cost of rejecting: the same as above. The
reason is the question's, as above, and not stated by the owner: its line
numbers are wrong wherever `main` changed the same file.

**Failing the ubuntu check when the merge tree cannot be computed** (owner,
2026-10-09, OQ-3). Cost of rejecting, as spec-author reads it, not stated by
the owner: a run whose merge tree was not checked can pass; it says so on the
run (B-019), and the run on the push to `main` is the backstop (§ 5 #21).

## Affects

- B-017 to B-022 (added); B-023 and B-024 (added on Round 12).
- B-006 (Amended: scoped to a violation the pull request's head carries). It
  had named no tree, so it promised an annotation B-020 denies for a violation
  only the merge tree carries. Decision 0002's call is otherwise unchanged.
- C-7 to C-10 (added); A-3 and A-4; § 2 #9; § 1 and the description.
- § 5 #21 to #27 (added).
- OQ-3, OQ-4 (raised, and resolved 2026-10-09).
- B-001, C-3, C-4 and C-6 hold as written.
- The `@B-017` to `@B-024` scenarios, a `@B-001 @boundary` scenario, and the
  `@B-006` scenario's Given.

## Reversal

None.
