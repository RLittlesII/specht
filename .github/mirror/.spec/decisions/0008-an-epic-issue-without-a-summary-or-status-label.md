---
title: "Decision 0008: a done epic carries no status label, and a missing Summary falls back to the description"
description: "A done epic's issue carries no status label, as a done item's does not; an epic with no Summary section has its frontmatter description in the Summary's place in its issue's body"
type: decision
---

# Decision 0008: a done epic carries no status label, and a missing Summary falls back to the description

**Date:** 2026-10-24
**Decided by:** the repository owner (2026-10-24), answering OQ-9 and review round 2; recorded by spec-author

## The call

1. A done epic's issue carries no `status:` label; its closed state says it is
   done, as B-009 says for an item.
2. When an epic has no `## Summary` section, its issue's body carries the
   epic's frontmatter `description` in the Summary's place, followed by the
   fenced file as for every epic.

## Why

1. Decision 0003 named `status:<status>` without saying what a done epic
   carries. Treating epics as items are treated keeps one rule for a reader
   filtering by status.
2. Epics `0193` and `0194` have no Summary section, so decision 0003's body
   would have nothing above the fenced file. Every epic has a `description`.

## Rejected

- **`status:done` on a done epic.** Cost of rejecting: none beyond the
  difference from decision 0003's literal wording. Taking it would make a
  done epic and a done item read differently.
- **The fenced file only, for an epic with no Summary.** Cost of rejecting: a
  second source for the rendered text. Taking it would leave two epics'
  issues with no rendered text above the raw file.

## Affects

- `0055-F9` B-047 (amended), B-050, B-051 (new); OQ-9 resolved.

## Reversal

None.
