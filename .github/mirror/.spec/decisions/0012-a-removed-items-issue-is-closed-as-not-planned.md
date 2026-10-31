---
title: "Decision 0012: a removed item's issue is closed as not planned"
description: "When an item or epic file is gone from main, the mirror closes its open issue with the reason not planned instead of leaving it open labelled stale; the issue is still never deleted, still labelled stale and still named in the step summary, and a closed one keeps its state"
type: decision
---

# Decision 0012: a removed item's issue is closed as not planned

**Date:** 2026-10-30
**Decided by:** the repository owner (2026-10-30, twice, in chat): "If they are out of scope, close them" and "if they aren't supposed to happen due to bad planning, remove them"; recorded by spec-author

## The call

1. A mirrored issue whose item or epic file is no longer in the tree, and
   which is open, is closed with the reason not planned (B-064). It was left
   open, labelled `stale`, by decision 0001 item 11.
2. It is still never deleted (B-033), still labelled `stale` (B-034) and still
   named as a warning in the step summary (B-035). Whether the closed issue
   keeps the label is OQ-13, which defaults to keeping it.
3. A mirrored issue in that state that is already closed keeps its state and
   its reason (B-065), so an item delivered and later removed keeps the
   `completed` it earned.
4. An item whose file returns is an item again: B-013 or B-045 reopens the
   issue and the `stale` label comes off with the owned labels.

Decision 0001 item 11 is superseded by this call.

## Why

Pull request #604 removed six items and their issues stayed open in
milestone 0.3.0 for two days (lesson 0002), so the public board counted
removed work as open. The owner reads a removal as out of scope or as bad
planning, and either way the work is not planned, which is the reason GitHub
has for a closed issue that will not be done.

## Rejected

- **Leaving the issue open for the owner to close** (decision 0001 item 11,
  OQ-12's default). Cost of rejecting: none beyond the write. Taking it kept
  a hand step that nobody was told to take.
- **Deleting the issue.** Cost of rejecting: a removed item keeps a closed
  issue on the board. Taking it would lose the issue's number and history,
  and the mirror's GitHub surface has no delete (B-033).

## Affects

- `0055-F9` B-033 (amended), B-064 and B-065 (new); § 5 row 6; OQ-12
  (resolved) and OQ-13 (new).
- `references/delivery.md` § "Removing an item leaves its issue on the board".
- The mirror script and its tests, which still leave the issue open: a work
  item to be cut from B-033, B-064 and B-065.
