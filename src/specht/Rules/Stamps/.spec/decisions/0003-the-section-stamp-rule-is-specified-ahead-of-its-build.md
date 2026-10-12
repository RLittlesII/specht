---
title: "Decision 0003: the section stamp rule is specified ahead of its build"
description: "0101-F9 is written as a specification now, with its rule id and its schema version left as open questions like the rest of epic 0101, and is built from work items once 0001-F7 unblocks the epic"
type: decision
---

# Decision 0003: the section stamp rule is specified ahead of its build

**Date:** 2026-10-11
**Decided by:** the repository owner (2026-10-11); recorded by spec-author

## The call

This change writes the specification and nothing else. The rule is a
`SPEC###` of a schema version after `0.1.0`; which id and which version stay
open questions, as they are for the rest of epic `0101`. No work item is
cut, and no code or test is written, until `0001-F7` unblocks the epic.

## Why

Stated by the owner:

- Asked "How far should this change go?", the owner selected "Spec only
  (Recommended)" over "Spec + build, schema 0.2.0" and "Spec + build, into
  0.1.0".

That option's description, the asker's wording and not restated by the owner:

- spec-author writes the new Feature `0101-F9` (claims, the `.feature` file,
  out of scope) on a `spec/` branch, with the rule id and the schema version
  left as open questions like its `0101` siblings'. The build follows as
  items once `0001-F7` unblocks epic `0101`.

The author's inference, not stated by the owner:

- A new rule is a new schema version (`0001-F7` C-11; `0001-F1` C-5), so a
  repository pinned to `0.1.0` is never checked by it (B-016; C-7).
- The rule id joins `0101-F1` OQ-1 (OQ-1), and the version is `0001-F7`
  OQ-18 (OQ-2).

## Rejected

Two options were put to the owner beside the one selected, and the owner
selected neither. Each description is the asker's wording, not the owner's;
each cost is the author's assessment.

**"Spec + build, schema 0.2.0".** Description: the specification, then
tests and code in one branch; adds a second embedded schema version
carrying the new id; touches versioning, the rule pages, the golden
baseline and the report schema; the rule id must be picked now. Cost of
rejecting: stamps keep growing unreported until the epic is unblocked, and
the form is held meanwhile only by `specht-conventions` § Specs, which this
change amends to state it.

**"Spec + build, into 0.1.0".** Description: the specification, then tests
and code, with the new id added to `0.1.0` in place; needs a decision
record overriding `0001-F7` C-1 and C-11 and `0001-F1` C-5, on the "nothing
shipped yet" precedent from ADR-0010; the rule id must be picked now. Cost
of rejecting: the same wait; `0.1.0`'s rule vocabulary stays as it is
(B-016; C-7).

## Affects

- `0101-F9` B-016; C-7; OQ-1, OQ-2; frontmatter `status: blocked` and
  `depends_on`.
- Epic `0101`: gains the child.
- `0001-F5` and `0001-F7`: gain the reverse `blocks` edge.

## Reversal

None.
