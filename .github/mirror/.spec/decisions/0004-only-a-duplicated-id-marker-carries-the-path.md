---
title: "Decision 0004: only a duplicated id's marker carries the path"
description: "An id claimed by one file keeps the plain specht-item marker; an id claimed by more than one file also carries the file's repository-relative path in its marker"
type: decision
---

# Decision 0004: only a duplicated id's marker carries the path

**Date:** 2026-10-24
**Decided by:** the repository owner (2026-10-24), answering OQ-7; recorded by spec-author

## The call

An issue for an id that one file claims carries the plain marker
`<!-- specht-item: <id> -->`. An issue for an id that more than one file claims
carries a marker that also names that file's repository-relative path, so each
file has its own issue.

## Why

Id `0220` is claimed by two files (A-5), and a marker naming the id alone
would make both files claim one issue. Adding the path only where an id is
duplicated tells them apart and leaves every other issue identified by its id,
which does not change when a file moves.

## Rejected

**Every marker adds the path.** Cost of rejecting: a duplicated id's issue is
lost, and a new one created, if that file moves. Taking it would do the same
to every issue: moving any item file, as moving a specification with its code
does (epic `0055` § Placement), would orphan its issue and create another.

## Affects

- `0055-F9` B-003 (amended), B-049 (new); OQ-7 resolved.

## Reversal

None.
