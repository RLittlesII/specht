---
title: "Decision 0011: B-008 orders the child items too"
description: "B-008's discovered files include FindChildItems' list, so the flat item list is ordered by root-relative path ordinally, as the specifications are; spec-author's reading of the claim as written, the owner not asked"
type: decision
---

# Decision 0011: B-008 orders the child items too

**Date:** 2026-11-10
**Decided by:** spec-author, on `spec-reviewer`'s § 12 finding 4 for item `0009`. The reading is spec-author's; the repository owner was not asked.

## The call

B-008's "discovered files" include the child items. B-003 has discovery find
them, so they are discovered files, and B-008 orders every discovered file by
its root-relative path, ordinally. `implementer` orders `FindChildItems`' list
that way under B-008, as one flat list across every specification, in both
modes. No claim, constraint or scenario changes.

The same reading covers the other finders: `FindEpics` and `FindTestFiles`
already order through `Matching`, and `FindCompanions` lists one
specification's directory, which is ordinal.

## Why

- The claim says "discovered files", not "specifications", and B-003 names
  items among what discovery finds. The narrower reading adds a limit the
  text does not state.
- Today, with one specification below another's directory, as
  `epics/E/F/spec.md` and `epics/E/F/G/spec.md`, the item list is in
  specification order, not path order (§ 7 "What it leaves").
- What a consumer reads barely moves: the report is ordered by
  `SpechtViolationOrder`, so a violation moves only among violations equal on
  severity, file, line and rule id (§ 7 "What it touches").

## Rejected

**B-008 covers the specifications alone.** This keeps `FindChildItems` as built
and `Covered` as § 9 marks it. The cost is a claim read narrower than its text,
with the item order left to no claim. A later reader of B-008 would take it as
a defect.

**Amend B-008 to name each finder.** This restates what the text already says,
and it takes a claim change and a review round for no change in meaning.

## Affects

- B-008: unchanged in text. Its mechanism gains `FindChildItems`' order, which
  is owed to `implementer`. A test of that order with one specification below
  another's directory is owed to `test-writer`. § 9 reads `Covered` only once
  that test passes.
- B-003, B-012 and C-3: unchanged.

## Reversal

None.
