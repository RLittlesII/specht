---
title: "Decision 0004: a named root record folder is read under an exclusion"
description: "A root record folder the manifest names, such as .spec/adr, is read at that path whatever exclusion covers a directory above it, which amends 0001-F6 C-1; narrowing the default exclusion and leaving the repository-wide folders unchecked were turned down"
type: decision
---

# Decision 0004: a named root record folder is read under an exclusion

**Date:** 2026-10-09
**Decided by:** the repository owner (2026-10-09, answering `0101-F7` OQ-5, part (b)); recorded by spec-author

## The call

A root record folder the manifest names is read at the path it names,
whatever exclusion covers a directory above it. Every exclusion goes on
pruning everything else: nothing else under the excluded directory is read,
and a record folder beside a specification under an excluded directory is
not read.

## Why

Stated by the owner:

- Of "Named root paths are read", "Narrow the default exclusion" and "Leave
  repo-wide folders unchecked", the first.

The question's wording beside that option, not the owner's statement:

- A root record folder the manifest names is read whatever exclusion covers
  a directory above it, the way the manifest and the schemas are read from
  `.spec/schema/` by path.
- It amends `0001-F6` C-1.
- Other exclusions still prune everything else.

The author's inference, not stated by the owner:

- The path is matched exactly and by text (C-5; C-14); no glob reaches a
  root record folder.
- In git mode `0001-F6` B-010 passes the exclusions to git as exclude
  pathspecs, so the amendment must reach that claim as well as C-1, or git
  never lists the named folder. The question named C-1 alone (OQ-8, part
  (f)).
- `0001-F6` C-4 stands: the root `.spec` stays in the default manifest's
  exclusions.

## Rejected

**Narrow the default exclusion**: exclude only `.spec/templates` and
`.spec/schema`. Rejected by the owner. The cost of rejecting is the author's
assessment, not the owner's: `0001-F6` C-1 gains an exception, and discovery
reads one named path under a directory it otherwise never opens.

**Leave repo-wide folders unchecked.** Rejected by the owner. The cost of
rejecting is the author's assessment, not the owner's: the same amendment,
taken so that the two folders every branch shares
([lesson 0005](../../../../../../.spec/lessons/0005-an-id-is-not-reserved-until-it-merges.md))
are not left to review.

Not asked, and not decided here: how record folders are found beside a
specification, the values that declare them, what a declared list replaces,
and a folder reached twice or a path that leaves the root (OQ-8).

## Affects

- `0101-F7` B-015, B-016 and C-5 (Source; wording unchanged in what it
  requires); § 5 rows 15 and 17; OQ-5 (resolved as to part (b)); OQ-8
  (raised).
- Decision 0002: its "Which folders" reading is, as to reading the
  repository-wide pair under the excluded root `.spec`, now the owner's
  answer.
- `0001-F6` C-1: to be amended by a delta this record does not make. It must
  come to say that discovery opens no directory the manifest excludes,
  except to read a root record folder the manifest names, at that exact
  path, and that nothing else under the excluded directory is opened.
  `0101-F7` depends on `0001-F6` for it.
- `0001-F6` B-010 and C-4, and `0001-F5`: not changed by this record.

## Reversal

None.
