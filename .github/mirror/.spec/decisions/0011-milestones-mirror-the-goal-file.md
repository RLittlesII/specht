---
title: "Decision 0011: GitHub milestones mirror the goal file's milestones"
description: "The mirror keeps one GitHub milestone per versioned section of .issue/.goal, found by a marker in its description and never deleted, and gives each open item's issue the milestone of the earliest section whose closure holds it; done items keep theirs; the goal file stays the record"
type: decision
---

# Decision 0011: GitHub milestones mirror the goal file's milestones

**Date:** 2026-10-25
**Decided by:** the repository owner (2026-10-25), asking "so no `.github/milestones.yml`? No milestones cut?" of pull request 565 and then directing that the goal file's milestones be mirrored through the work-item mirror; recorded by spec-author

## The call

1. `.issue/.goal` stays the record of the milestones (item `0299`). A GitHub
   milestone is a read-only view of one of its versioned sections, as an
   issue is of an item: nothing on GitHub is read back (C-2).
2. Each section headed by a version line has exactly one open GitHub
   milestone titled with that version (B-055).
3. The mirror finds its milestones by the hidden marker
   `<!-- specht-milestone -->` in the description. An unmarked milestone with
   a section's title is adopted by writing the marker (B-056); an unmarked
   milestone with no section's title is left alone (B-061).
4. An open item's issue takes the milestone of the earliest section whose
   closure holds it - the queue's `reach` over `depends_on` and `parent`
   children - and an open item in no closure has none (B-057, B-058).
5. A done item's issue keeps the milestone it has, so a milestone's progress
   counts the work done in it (B-059). An epic's issue keeps its milestone
   too (B-063).
6. A marked milestone whose version no section names is closed, never
   deleted (B-060). A goal file with no version line changes no issue's
   milestone (B-062).

This reverses § 5 row 4 for milestones. Project boards stay out of scope.

## Why

Pull request 565 put the release plan into `.issue/.goal` as milestones, and
on GitHub a reader of an issue still cannot see which release it belongs to.
The mirror already reads the goal file for the `goal` label (B-010), and the
queue already computes each open item's milestone with `reach`, so mirroring
the milestones adds a view and no second record. Closing the section's
milestone, rather than deleting it, matches a closed milestone's section being
deleted from the goal file, and keeps the done items' history on GitHub.

## Rejected

- **A `.github/milestones.yml` file.** Cost of rejecting: none. Taking it
  would keep a second record of what the goal file already says, and the two
  would drift.
- **The `milestone:` frontmatter key on Feature specifications.** Cost of
  rejecting: a specification does not show its milestone. Taking it would edit
  many specifications for a delivery fact, which the item and the goal file
  own, and an item with no specification - a bug, a spike, a chore - would
  have no milestone.

## Affects

- `0055-F9` B-055 to B-063 (new); B-029 and B-041 (amended); § 5 row 4.
- AGENTS.md § SDLC, `specht-conventions` `references/delivery.md`, `0055-F8`
  § 5 row 1 and `docs/roadmap.md`: GitHub issues and milestones are a
  read-only mirror, never the tracker.

## Reversal

None.
