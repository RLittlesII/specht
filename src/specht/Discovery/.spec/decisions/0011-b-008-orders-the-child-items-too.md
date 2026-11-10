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
- **Amended 2026-11-10, on `spec-reviewer`'s § 12 finding 8 for item `0009`.**
  This bullet first said that only the order of tied violations could move,
  because the report is ordered by `SpechtViolationOrder`. That was wrong for
  `SPEC044`. `ItemSequenceRule`'s reuse check reports the second item it reads
  with an id and names the first, so the item order decides which file is
  reported.
- Under the flat order, a reused id's `SPEC044` moves to the other file on two
  trees: one specification beside another and below its directory, within
  `epics`; and a co-located item whose path sorts before an `epics/` one. Its
  message and `expected.firstDeclaration` swap with it. The trees, the example
  and the test that pins it are in § 7 "What it touches". This reading still
  holds: path order is what B-008 states, and the move is a consequence of it.

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
- `SPEC044`: on the two trees above, which file it reports and the first
  declaration it names (§ 7 "What it touches").
- B-003, B-012 and C-3: unchanged.

## Reversal

None.
