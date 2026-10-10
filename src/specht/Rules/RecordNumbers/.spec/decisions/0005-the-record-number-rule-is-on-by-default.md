---
title: "Decision 0005: the record number rule is on by default"
description: "The rule of 0101-F7 and the record values it reads ship default-filled, so every repository on the major schema version that adds the rule is checked; opt-in by manifest, as 0101-F8 decision 0004 has it for work items, was turned down"
type: decision
---

# Decision 0005: the record number rule is on by default

**Date:** 2026-10-09
**Decided by:** the repository owner (2026-10-09, answering `0101-F7` OQ-7); recorded by spec-author

## The call

The rule runs in every repository that pins a schema version carrying it.
The record values it reads are tool-owned: every shipping manifest of such a
version carries them, and a manifest that leaves one out reads the default.

## Why

Stated by the owner:

- Of "On by default" and "Opt-in by manifest", the first.

The question's wording beside that option, not the owner's statement:

- The rule and its folder names ship default-filled for every repository on
  the major version that adds it.

The author's inference, not stated by the owner:

- "Its folder names" is read as both the folder names beside a specification
  and the root record folders (B-018, B-019).
- The record file shape and the record number grammar are default-filled as
  well (B-020; C-13). The option named the folder names; a rule that is on
  where a manifest declares nothing reads no number without the other two.
- The values are tool-owned under `0001-F4` decision 0001, so the shipping
  manifest carries them (B-022). Which keys carry them is OQ-8.

## Rejected

**Opt-in by manifest**, as the owner chose for work items (`0101-F8`
decision 0004). Rejected by the owner. The cost of rejecting is the
question's wording, not the owner's statement: a consumer with date-named
files such as `2024-10-01-review.md` in `decisions/` sees `2024` reported as
claimed twice until it changes the manifest.

## Affects

- `0101-F7` B-018, B-019, B-020, B-022 and C-13 (Source; wording unchanged
  in what it requires); OQ-7 (resolved).
- Decision 0001: the proposal its "Rejected" section rests on is now the
  owner's answer.
- Decision 0002: its "Who is checked" reading is now the owner's answer.
- `0101-F8` decision 0004: the opposite call for work items; unchanged.
- `0001-F4`, `0001-F5` and `0001-F6`: the keys are still OQ-8; not changed
  by this record.

## Reversal

None.
