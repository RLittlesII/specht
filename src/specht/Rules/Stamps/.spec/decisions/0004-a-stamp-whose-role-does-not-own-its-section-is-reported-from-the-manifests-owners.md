---
title: "Decision 0004: a stamp whose role does not own its section is reported from the manifest's owners"
description: "SPEC015, a warning in schema 0.2.0, reports a section stamp whose role is not among the roles the manifest's consumer-owned owners key lists for that section; it reverses decision 0001's not-asked-for reading of section ownership and leaves the form rule unchanged"
type: decision
---

# Decision 0004: a stamp whose role does not own its section is reported from the manifest's owners

**Date:** 2026-10-14
**Decided by:** the repository owner (2026-10-14); recorded by spec-author

## The call

1. A section's owners are the manifest's `owners` key (`0001-F5` B-059): a
   `sections` title mapped to a non-empty list of roles. The key is the
   consumer's, like `rules` and `exclusions` (`0001-F5` C-13); the tool ships
   no default.
2. `SPEC015` reports, as a warning, each stamp in a section `owners` names
   whose role is not in that section's list (B-024). A section `owners` does
   not name, or one with no stamp, is not reported (B-025).
3. The role is the text before the stamp's first comma, compared ordinally;
   the note is never read (B-026; C-3).
4. `SPEC015` ships in schema `0.2.0`, apart from the form rule, whose id and
   schema version stay OQ-1 and OQ-2.

## Why

- Section ownership is written once, in this repository's companion skill,
  and a stamp that names the wrong role is found only by a reader.
- Putting the map in the manifest makes it the consumer's data, not a table
  the tool would ship (`0001-F5` C-13).

## Rejected

Recorded by spec-author as what the call rules out:

- A shipped default owner map: every consumer's roles differ, and the tool
  ships no role names.
- Judging ownership inside the form rule: the form rule asks two things of a
  stamp (C-12), and the owner kept it so.

## Affects

- Reverses decision 0001's "a role that does not own the section" under the
  author's inference, and § 5 row 1's "Not asked for", which is amended in
  place.
- Added: B-024 to B-026; C-13. Amended in place: C-12; § 5 row 1.
- The `@B-004` boundary scenario now states that the manifest names no
  owners for the section.
- `0001-F5` B-059, C-13; `0001-F3` B-035; `0001-F7` B-115.

## Reversal

None.
