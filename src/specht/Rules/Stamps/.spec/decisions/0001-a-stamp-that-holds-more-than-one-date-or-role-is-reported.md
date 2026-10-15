---
title: "Decision 0001: a stamp that holds more than one date or role is reported"
description: "The check reports a section stamp that holds more than one date or names more than one role; a stamp is one role, one date and an optional trailing note, and a second stamp under a sub-heading of the same section stays legal"
type: decision
---

# Decision 0001: a stamp that holds more than one date or role is reported

**Date:** 2026-10-11
**Decided by:** the repository owner (2026-10-11); recorded by spec-author

## The call

A section stamp says who wrote the section last. The check reports a stamp
that holds more than one date or names more than one role.

## Why

Stated by the owner, on 2026-10-11:

- "can we add a SPEC warning for
  `<!-- last written by: spec-author, 2026-10-11 (row 4) -->` if the
  `last written by` becomes an array, flag it!"
- The owner's example of the defect:
  ``<!-- last written by: spec-author, 2026-10-09 (C-9 amended, item 0024; C-8 amended, `0001-F7` decision 0005), 2026-10-11 (C-8 amended, ADR-0011) -->``.
- Asked "What counts as the stamp 'becoming an array'?", the owner selected
  "More than one date or role (Recommended)" over "More than one date only"
  and "Also flag a note-less grammar break".

That option's description, the asker's wording and not restated by the owner:

- A stamp must be one role, one date, and an optional one trailing note in
  parentheses.
- It flags the multi-date lines, including the 2 that also name two roles
  with `;`.
- A second stamp under § 8's `### Performance` sub-heading stays legal.

The need as briefed to the author on 2026-10-11, not the owner's words:

- The finding sits on the stamp's line, and its message names how many dates
  and how many roles the stamp holds (B-001, B-002, B-010).
- A stamp over on both counts gets one finding, not two (B-003).
- A date or a role name inside the note is not read (B-006, B-007).
- A stamp carrying a template's date placeholder is not reported (B-009).
- The rule never writes a file and proposes no rewrite (B-018, B-019).

The author's inference, not stated by the owner:

- Beyond what the owner declined under Rejected, a stamp with one role and
  one date is not reported for a second note, a note that is not last, or a
  role that does not own the section, and a section with no stamp is not
  reported: none was in any option put to the owner (B-021, B-022; C-12;
  § 5 rows 1, 2 and 5).
- A date written twice is two dates (B-012; OQ-6).
- The stamp text quoted as code is not a stamp (B-017; OQ-5).
- Stamps are judged in every Feature specification discovery finds and in no
  other file (B-020; OQ-8).

## Rejected

Two options were put to the owner beside the one selected, and the owner
selected neither. Each description is the asker's wording, not the owner's;
each cost is the author's assessment.

**"More than one date only".** Description: flags a stamp with two or more
dated entries, exactly the owner's example; says nothing separate about a
second role. Cost of rejecting: none found; every stamp it reports the
selected option reports too.

**"Also flag a note-less grammar break".** Description: as the selected
option, plus anything not matching `role, date (note)`: text after the
closing parenthesis, an unknown role name, a malformed date; a broader lint.
Cost of rejecting: a stamp with stray text after its note, a name that is
no role, or a date the check cannot read passes when it holds at most one
date and one role (B-021, B-023; C-12; § 5 rows 3 to 5).

Not put to the owner, and not chosen by the author:

**Counting the stamps in a section.** The selected option's description
keeps a second stamp under a sub-heading legal. Cost: two stamps directly
under one heading both pass.

## Affects

- `0101-F9` B-001 to B-012, B-017 to B-023; C-3, C-4, C-5, C-6, C-12; § 5
  rows 1 to 7.
- `specht-conventions` § Specs: states the stamp's form.

## Reversal

2026-10-14, in part, by the owner ([decision 0004](0004-a-stamp-whose-role-does-not-own-its-section-is-reported-from-the-manifests-owners.md)):
a stamp whose role does not own its section is reported, by `SPEC015` and
from the manifest's `owners` key, not by the form rule. The rest of this
decision stands; the form rule still reports nothing for that role (C-12).
