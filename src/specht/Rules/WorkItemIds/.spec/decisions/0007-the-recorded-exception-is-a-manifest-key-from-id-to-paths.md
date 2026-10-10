---
title: "Decision 0007: the recorded exception is a manifest key from id to paths"
description: "A doubled work-item id knowingly kept is recorded under one manifest key that maps the id to the exact paths allowed to share it, a further claimant is still reported and a stale entry is itself a finding; a marker in the item files was turned down, and the key's name, an entry's exact shape and where a stale finding sits stay the author's proposal"
type: decision
---

# Decision 0007: the recorded exception is a manifest key from id to paths

**Date:** 2026-10-09
**Decided by:** the repository owner (2026-10-09, answering `0101-F8` OQ-8); recorded by spec-author

## The call

The recorded exception of decision 0003 is one manifest key that maps a
work-item id to the exact paths allowed to share it.

## Why

Stated by the owner:

- Of "Manifest key, id to paths" and "Marker in the item files", the first.

The question's wording beside that option, not the owner's statement:

- One manifest key maps an id to the exact paths allowed to share it; a
  further claimant still fails; a stale entry is itself a finding.

Still the author's proposal, not put to the owner:

- The key's name (OQ-5).
- An entry's exact shape - two or more distinct root-relative paths - and
  that any other shape is an invalid manifest (B-015, B-016; OQ-9).
- That a stale-entry finding sits on the manifest file, what its message
  names, and its line (B-013, B-014; OQ-9).

## Rejected

**A marker in the item files.** Rejected by the owner. The cost is the
author's assessment, not the owner's: the record sits away from the two
files it excuses, and can go stale when one of them moves.

## Affects

- `0101-F8` B-011 to B-015 and C-6 (Source; claim wording unchanged); OQ-8
  (resolved as to the mechanism); OQ-9 (raised).
- Decision 0003: its mechanism is now the owner's choice.

## Reversal

**Noted 2026-10-09, after review round 1.** Nothing is reversed, and the
sections above stand as written. The owner selected the option "Manifest
key, id to paths" and did not restate its description, which read: "One
manifest key maps an id to the exact paths allowed to share it. Lives with
the rest of this repository's settings; a third `0118` still fails; a stale
entry is itself a finding." The mechanism is the owner's. "A further
claimant still fails" (B-012) and "a stale entry is itself a finding"
(B-013) are that description and not the owner's own words; item `0128`'s
acceptance criteria also say a third item numbered `0118` is still reported.
`0101-F8` OQ-8's Resolution and the Sources of B-012 and B-013 say the same.
The description field above this record's title is unchanged and carries the
same two phrases.
