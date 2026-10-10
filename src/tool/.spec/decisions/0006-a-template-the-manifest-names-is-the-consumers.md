---
title: "Decision 0006: a template the manifest names is the consumer's"
description: "The manifest names, one by one, the templates that are the consumer's; specht upgrade writes none of those and rewrites the rest, never touches a template file the tool does not ship, and treats template ownership and the schema source as independent; an upstream template source is a later stage"
type: decision
---

# Decision 0006: a template the manifest names is the consumer's

**Date:** 2026-10-09
**Decided by:** the repository owner, answering `0001-F7` OQ-25 (2026-10-09); recorded by spec-author

## The call

- The manifest names which templates are the consumer's, one by one.
  `specht upgrade` does not write a template so named, and rewrites every
  other template the pinned version ships with that version's.
- `upgrade` prints each template it skipped and why (B-061). The option the
  owner chose says so: `upgrade` "skips the templates and prints each skipped
  file and why".
- `upgrade` "never creates, modifies or deletes a template file it does not
  ship, whichever source is selected" - the owner's words, under "leave alone,
  always" (B-063).
- An upstream template source - templates fetched from a recorded source, as
  decision 0004 does for the frontmatter schemas - is wanted, as a later
  stage. This decision is the first stage, ownership on disk, and does not
  give the later one a shape (OQ-26).

The author's inference, not stated by the owner:

- Only the manifest's naming makes a template the consumer's. Selecting the
  on-disk schema source names no template (OQ-8 stands, B-031), and naming a
  template selects no schema source (B-005; C-14). "Only" binds this stage
  and does not prejudge the upstream source (OQ-26).
- B-063's "whatever the manifest names" reads the owner's "always"; the owner
  named the source and not the manifest (OQ-27).
- "Whichever source is selected" reaches the later stage: an upstream
  template source is bound by B-063 too (OQ-26).
- The check reads no template, so its report is unchanged (B-065).
- The key is part of `0.1.0`, which is unpublished (decision 0002). Its name
  is the `implementer`'s to propose (OQ-1).

Not decided: whether a manifest naming a template the pinned version does not
ship is invalid or merely without effect (OQ-27).

## Why

- A consumer whose records need another shape had no way to keep it:
  `upgrade` overwrote every template (decision 0001), while a frontmatter
  schema could be kept by selecting the on-disk source (B-029).
- Per template, a consumer that changes one template still gets the pinned
  version's copy of the others.
- A file the tool never shipped is not the tool's to touch; saying so keeps a
  later `upgrade` from tidying the folder.

## Rejected

**All or nothing: one switch that makes every template the consumer's.**
Offered to the owner and not chosen. Cost of rejecting: the manifest carries a
list, and `upgrade` reports per template. Taken.

**Inferring ownership from the on-disk schema source.** One setting would
then do two things, and OQ-8's answer - the templates are still rewritten
under the on-disk source - would be reversed. Cost of rejecting: a consumer
that owns both its schemas and a template says so twice. Taken.

**Skipping any template that was edited by hand.** Decision 0001 already
rejected it: the tool cannot tell a deliberate template from a stale one, and
the tree is left partly at another version with nothing saying which part.
Cost of rejecting: a hand edit to a template the manifest does not name is
still overwritten, and is recoverable from version control. Taken.

**Leaving a file the tool does not ship unspecified.** Offered to the owner
and not chosen. Cost of rejecting: none found.

**An upstream template source in the same change.** Deferred, not rejected;
its shape is OQ-26.

## Affects

- `0001-F7` § 2 need 8; § 3 B-005, B-006 and B-031 (amended), B-060 to B-063
  and B-065 (added; B-064 was added and withdrawn into B-005 at review); § 4 C-2 and C-10 (amended), C-14 (added); § 5 rows 14 to 18; § 11
  OQ-1 (extended), OQ-25 (resolved), OQ-26 and OQ-27 (raised).
- `0001-F7` decision 0001: narrowed, see its Reversal.
- `0001-F5` § 5 row 3: the key is this Feature's, as `schemaVersion` and the
  schema source are.
- brief § 6 ("Templates: in the tool or per repository" and "What a "schema
  version" is in the file"); AGENTS.md § Configuration.
