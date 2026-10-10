---
title: "Decision 0005: a work-item id is read from the file name only"
description: "A work-item file claims the id its file name begins with and no content of the file is read; reading the id key as well, and reporting a key that disagrees with the name, was turned down"
type: decision
---

# Decision 0005: a work-item id is read from the file name only

**Date:** 2026-10-09
**Decided by:** the repository owner (2026-10-09, answering `0101-F8` OQ-4); recorded by spec-author

## The call

A work-item file's id is the prefix of its file name. No content of a
work-item file is read.

## Why

Stated by the owner:

- Of "File name only" and "File name and `id:` key", the first.

The author's inference, not stated by the owner:

- The tool opens no work-item file, so it needs no knowledge of an item
  schema it does not ship, and a finding sits on the file with no line of its
  own (C-1).

## Rejected

**The file name and the `id` key**, with a mismatch reported as `SPEC043`
does for a child item. Rejected by the owner. The cost is the question's
wording, not the owner's statement: an `id` key that disagrees with the file
name goes unreported.

## Affects

- `0101-F8` C-1 and § 5 row 10 (citation; what C-1 requires is unchanged);
  OQ-4 (resolved).

## Reversal

None.
