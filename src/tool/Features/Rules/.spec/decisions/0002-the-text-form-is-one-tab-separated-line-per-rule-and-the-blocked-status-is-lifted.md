---
title: "Decision 0002: the text form is one tab-separated line per rule, and the blocked status is lifted"
description: "specht rules prints one line per rule of four tab-separated fields, with the severity and level in the manifest's lower-case spelling, as a default the owner has not been asked; the Feature moves from blocked to ready-for-architecture because the Features it waited on are built"
type: decision
---

# Decision 0002: the text form is one tab-separated line per rule, and the blocked status is lifted

**Date:** 2026-10-24
**Decided by:** `spec-author`, grooming. The text form is a derived default (OQ-2), not the owner's call.

## The call

1. **Status.** The Feature was drafted `blocked` on `0001-F2`, `0001-F3`,
   `0001-F5` and `0001-F7`. `0001-F2`, `0001-F3` and `0001-F5` are built.
   `0001-F7`'s list and pin commands are built (items `0121` to `0125`); only
   `upgrade` (item `0047`) is outstanding, and no claim here depends on it. The
   status is `ready-for-architecture`. `spec_status` stays `draft`: the review
   in § 12 is the spec-reviewer's.
2. **Text form (B-019).** One line per rule, four fields in the order rule id,
   family, default severity, effective level, separated by single tab
   characters. Severity and level are written in lower case (`error`,
   `warning`, `off`), the spelling the manifest uses for a level. The family is
   written as the rule page gives it.
3. **Branch.** `rules` is a branch whose default command is this one, so
   `0001-F11`'s set command can hang under it. That is naming only (A-1).

## Why

- A test cannot fail against a form no claim fixes (OQ-2).
- The rule pages spell severity `Error` and `Warning`; the manifest, and so the
  effective level, spell `error`, `warning`, `off`. One lower-case spelling for
  both columns lets a script compare them without a case rule. B-007 holds the
  listing to the page's value, not its capitalisation.
- Families contain spaces (`Feature file`), so a single space cannot separate
  the fields; a tab can, and plain text stays line-oriented for a pipe
  (`0001-F2` C-3).

## Rejected

**Single spaces.** Cost of rejecting: a reader splitting on whitespace needs
the tab; a family with a space would otherwise break the line.

**An aligned table.** It renders a layout, which C-6 rules out on stdout.

**The page's capitalisation (`Error`).** Cost of rejecting: B-007 compares the
value and not the capitalisation.

## Affects

- `0001-F10` B-019 (new), OQ-2, § 9, A-1.
- Not decided: a title column (OQ-3), a published schema for `--json` (OQ-4),
  the renumbering of ids (OQ-5).

## Reversal

None. The owner's answer to OQ-2 supersedes B-019 by amendment.
