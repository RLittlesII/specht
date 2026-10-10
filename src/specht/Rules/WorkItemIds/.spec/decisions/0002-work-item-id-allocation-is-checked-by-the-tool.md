---
title: "Decision 0002: work-item id allocation is checked by the tool"
description: "A work-item id claimed by two files and a sequence file disagreeing with the ids present are reported by specht as rules of a new major schema version, over the one tree it is given; a build-target check in one repository and a base or diff input were turned down"
type: decision
---

# Decision 0002: work-item id allocation is checked by the tool

**Date:** 2026-10-09
**Decided by:** the repository owner (2026-10-09, recorded on work item `0128`); recorded by spec-author

## The call

`specht` reports two work-item files that share an id (item `0128`, case
(b)) and a sequence file that disagrees with the ids present (case (d)), as
`SPEC###` rules. A new rule is a new major schema version (`0001-F7` C-11).

The check stays a deterministic, offline read of one tree. An id a pull
request adds that its base already holds is found by running the check a
second time over the merge tree, where it is two claimants like any other;
that run is `0055-F2`'s (`0055-F2` B-017).

The author's reading, not stated by the owner:

- **What "disagreeing" means.** The owner named the case and did not define
  it. Read from `specht-conventions` § Delivery, which says the sequence
  file holds the last id claimed and that a range is reserved by landing its
  bump first: the file is missing (B-017), is not one id (B-018), or holds
  an id lower than the highest present (B-019, B-021). An id higher than
  every id present is a reservation and is not reported (B-023), and a
  skipped number is not judged (B-024).
- **Epics.** AGENTS.md § "Stable IDs" puts work items and epics in one
  number space, and item `0128` says so for case (d). That a work item
  taking an epic's id is itself a finding (B-008) is OQ-7.
- **Scope of the version.** These rules ship in the same major schema
  version as the rest of epic `0101`. The number is `0001-F7` OQ-18, and
  whether this Feature shares it is OQ-3.
- **Who is checked.** The rules run only where a manifest declares
  work-item files. The owner was not asked; it is OQ-2.
- **How ids compare.** By text for uniqueness, by number for the sequence
  file. The owner answered by text for `0101-F6` (its decision 0003) and was
  not asked here; it is OQ-6.

## Why

- Lesson 0005: work item `0118` was claimed by three open pull requests at
  once, two of them merged with it, and git reported nothing.
- No rule of schema version `0.1.0` reads a work-item file or a sequence
  file, so the stable-id rule (AGENTS.md § "Stable IDs") is enforced for
  work items by review alone.

## Rejected

**A build-target check in this repository, run in CI beside the
self-check.** It was the first proposal. Rejected by the owner. Cost of
rejecting: the rules wait for a new major schema version and for `0001-F5`'s
rule settings, and until then review enforces the rule. Taken.

**A base, diff or pull-request input to `specht`.** Rejected by the owner.
The check would compare two revisions and stop being a read of one tree.
Cost of rejecting: CI runs the check twice on a pull request. Taken.

**Leaving the sequence file out**, as the first scope of item `0128` did.
Reversed by the owner on 2026-10-09.

Turned down by the author in reading "disagreeing", not put to the owner:

**The sequence file must equal the highest id present.** A reservation
lands the bump before the items exist, and would fail. Cost of rejecting: a
sequence file left far ahead by mistake is never reported.

**Reporting a skipped number.** Reserved ranges and withdrawn branches leave
gaps by design; lesson 0005 skipped seven ids on purpose. Cost of rejecting:
a number skipped by mistake is never reported.

## Affects

- `0101-F8` B-001 to B-010, B-017 to B-026, B-032; C-2, C-8, C-9, C-10;
  § 5 rows 1 to 5.
- `0055-F2`: the merge-tree run is that Feature's; relied on, unchanged.
- `0101-F6` decision 0002: the same call for constraint and open-question
  ids; unchanged.
- A later Feature, proposed `0101-F7` and not yet specified: record numbers.

## Reversal

None.
