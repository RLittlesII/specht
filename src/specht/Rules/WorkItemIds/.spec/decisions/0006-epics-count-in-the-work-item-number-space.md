---
title: "Decision 0006: epics count in the work-item number space"
description: "A work item that takes an epic's id is an id claimed twice, and epics count toward the highest id the sequence file must reach; judging work items alone was turned down, and two epic files sharing an id stays unasked"
type: decision
---

# Decision 0006: epics count in the work-item number space

**Date:** 2026-10-09
**Decided by:** the repository owner (2026-10-09, answering `0101-F8` OQ-7); recorded by spec-author

## The call

A work-item file and an epic file that carry the same id are one id claimed
twice. The highest id the sequence file must reach is the highest of the
work-item ids and the epic ids.

## Why

Stated by the owner:

- Of "Epics count" and "Work items only", the first.

The question's wording beside that option, not the owner's statement:

- A work item taking an epic's id is a duplicate, and epics count toward the
  highest id the sequence file must reach.

The author's inference, not stated by the owner:

- The finding sits on the work-item file and on the epic file, as on any two
  claimants (B-002).
- Epics count in every repository whose manifest declares both the epic
  grammar and work-item files, with no separate switch (A-6).

## Rejected

**Work items only.** Rejected by the owner. The cost is the author's
assessment, from OQ-7, not the owner's: a repository that numbers its epics
and its work items apart with the same grammar is misreported, and its
remedy is the rule settings.

Not asked, and not decided here: two epic files sharing an id that no work
item carries (OQ-10; § 5 row 13).

## Affects

- `0101-F8` B-008, B-021 and A-6 (Source; claim wording unchanged); OQ-7
  (resolved); OQ-10 (raised); § 5 row 13.
- Decision 0002: its "Epics" reading is now the owner's answer.

## Reversal

None.
