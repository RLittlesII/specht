---
title: "Decision 0004: the work-item id rules are opt-in by manifest"
description: "The rules of 0101-F8 run only in a repository whose manifest declares its work-item files, and nothing is filled in when the declaration is left out, so a repository without one sees nothing; shipping this repository's tracker layout as every repository's default was turned down"
type: decision
---

# Decision 0004: the work-item id rules are opt-in by manifest

**Date:** 2026-10-09
**Decided by:** the repository owner (2026-10-09, answering `0101-F8` OQ-2); recorded by spec-author

## The call

The rules run only where the manifest declares work-item files. What declares
them is never filled in from a default, so a repository whose manifest leaves
it out is not checked and sees no finding.

## Why

Stated by the owner:

- Of "Opt-in by manifest" and "On by default", the first.

The question's wording beside that option, not the owner's statement:

- The manifest declares the work-item files by a glob, an id grammar and a
  sequence file; the keys are never default-filled; a consumer without them
  sees nothing.

The author's inference, not stated by the owner:

- The option named the three values together. That the sequence rule runs
  only when a sequence file is also named, and the claimed-twice rule without
  one (B-028), is OQ-2's proposed default and was not asked apart. A manifest
  that declares some of the values and not others is OQ-5.
- The shared-id record is consumer configuration in the same way (C-12;
  decision 0003).
- No shipping manifest carries any of them (B-029), as none carries the epic
  grammar (`0001-F5` decision 0004).

## Rejected

**On by default**: `specht` shipping `.issue/*.yml` and `.issue/.sequence` as
defaults for every consumer. Rejected by the owner. The cost is the author's
assessment, not the owner's: a repository that wants the rules writes the
declaration itself, and one that forgets to is told nothing.

## Affects

- `0101-F8` B-027, B-028, B-029 and C-12 (Source; wording unchanged); OQ-2
  (resolved).
- Decision 0002: its "Who is checked" reading is now the owner's answer.
- `0001-F5` and `0001-F6`: the keys are still OQ-5; not changed by this
  record.

## Reversal

**Noted 2026-10-09, after review round 1.** Nothing is reversed, and the
sections above stand as written. The owner selected the option "Opt-in by
manifest"; "the question's wording beside that option" above is that
option's description, which the owner did not restate. What the bullets
under "The author's inference" hold was not asked and is now `0101-F8`
OQ-11: the claimed-twice rule running where no sequence file is named
(B-028), and no shipping manifest holding a shared-id entry (B-029).
OQ-2's Resolution no longer cites B-028 or B-029 as answered.
