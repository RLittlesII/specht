---
title: "Decision 0009: the pull request that set an item done is the one that delivered it"
description: "A done item's Delivered by line names the pull request whose commit first set it done; failing that the latest pull request whose subject names its id; failing that the commit's sha"
type: decision
---

# Decision 0009: the pull request that set an item done is the one that delivered it

**Date:** 2026-10-24
**Decided by:** the repository owner (2026-10-24), answering review round 3; recorded by spec-author

## The call

A done item's "Delivered by" line names, in this order of precedence:

1. the pull request named by the subject of the first commit on `main` that
   set the item's `status` to `done` (`#270` for `0045`);
2. failing that, the latest pull request whose commit subject names the item's
   id;
3. failing that, the sha of the first commit that set it `done`.

This supersedes the ordering in decision 0001's Agreed design item 7, which
put the subject naming the item first.

## Why

The pull request that writes `done` into the item is the one that delivered
it (AGENTS.md § SDLC). 22 done ids are named by more than one subject, so a
subject-first rule needs a tie-break, and any tie-break can pick a design or
follow-up pull request instead: `0045` is named by #264 (its design) and #270
(its delivery).

## Rejected

**Subject first, latest wins.** Cost of rejecting: none beyond departing from
the first ordering. Taking it would name a later follow-up pull request,
rather than the delivery, for any item whose id a later subject repeats.

## Affects

- `0055-F9` B-017, B-018, B-019 (sources); decision 0001 Agreed design item 7.

## Reversal

None.
