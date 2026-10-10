---
title: "Decision 0002: constraint and open-question id uniqueness is checked by the tool"
description: "A constraint or open-question id declared twice inside one specification is reported by specht as schema version 2 rules, over the one tree it is given; a build-target check in one repository and a base or diff input were turned down"
type: decision
---

# Decision 0002: constraint and open-question id uniqueness is checked by the tool

**Date:** 2026-10-09
**Decided by:** the repository owner (2026-10-09, recorded on work item `0128`); recorded by spec-author

## The call

`specht` reports a constraint id declared twice in a specification's § 4 and
an open-question id declared twice in its § 11, as `SPEC###` rules. A new
rule is a new major schema version (`0001-F7` C-11); these belong to schema
version 2, with the rest of epic `0101`.

The check stays a deterministic, offline read of one tree. An id a pull
request adds that its base already holds is found by running the check a
second time over the merge tree, where it is a duplicate like any other;
that run is a `0055-F2` delta.

Claim-id uniqueness stays `SPEC030`.

## Why

- Lesson 0005: constraint and open-question ids were claimed twice by
  parallel pull requests, git reported nothing, and the clashes were found
  by reading.
- No version 1 rule reads § 4 or § 11, so the stable-id rule (AGENTS.md
  § "Stable IDs") is enforced for claims and by review alone for these two
  kinds.

## Rejected

**A build-target check in this repository, run in CI beside the
self-check.** It was the first proposal. Cost of rejecting: the rules wait
for schema version 2 and for `0001-F5`'s rule settings, and until then
review enforces the rule. Taken.

**A base, diff or pull-request input to `specht`.** The check would compare
two revisions and stop being a read of one tree. Cost of rejecting: CI runs
the check twice on a pull request. Taken.

**Leaving these two id kinds out**, as the first scope of the work did.
Reversed by the owner on 2026-10-09.

## Affects

- `0101-F6` B-001, B-002, B-008, B-011, B-014; C-1, C-6, C-8, C-9; § 5 rows
  1 to 5 and 8.
- `0055-F2`: the merge-tree run, not yet specified.
- Two later Features, proposed `0101-F7` and `0101-F8`, not yet specified:
  record numbers and work-item ids.

## Reversal

None.
