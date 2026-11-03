---
title: "Decision 0020: the epics opt-in holds from schema 0.3.0"
description: "The owner's nine rulings of November 3 2026 on item 0345: the epic-less reading first holds at schema 0.3.0, the default manifest keeps the epic grammar, items number per the Feature that holds them, no epic file is read without the grammar, and a Feature with no epic under the grammar keeps its rendered identity and is reported by SPEC3001"
type: decision
---

# Decision 0020: the epics opt-in holds from schema 0.3.0

**Date:** 2026-11-03
**Decided by:** the repository owner, answering item `0345`'s nine questions, each on its own; recorded by spec-author

## The calls

1. **Pins (question A), "A 0.3.0+ only (Recommended)".** The epic-less
   reading holds only under a manifest pinned to schema `0.3.0` or later.
   `0.1.0`, `0.2.0` and `0.2.1` keep their reading (B-049). B-024 to
   B-032, B-034, B-035 and B-042 say so, and A-6 lists them. B-033 holds
   at every pin; item `0043` built it there.
2. **OQ-4, "A stays, as -F2 (Recommended)".** Under schema `0.3.0` or later
   with the epic grammar declared, a Feature whose frontmatter has no `epic`
   stays in `SPEC3002` and `SPEC6001`-`SPEC6003`. Its identity is the
   identity form rendered with an empty epic, `-F2` under `{epic}-{id}`
   (B-069). B-035 reports the cause.
3. **OQ-13, "A keep it on (Recommended)".** The shipped default manifest
   keeps declaring the epic grammar. "Off by default" is `specht init`
   without `--epics` (`0001-F4` B-021, B-022). A-4, C-4 and B-016 are
   unchanged.
4. **OQ-26, "A {id} (Recommended)".** Under schema `0.3.0` or later with no
   epic grammar declared, `expected.form` for `SPEC3002`, and for `SPEC5003`
   on a parent mismatch, is `{id}`. `0001-F3` B-012 is amended.
5. **Grouping (question B), "A by holding Feature (Recommended)".** B-032
   numbers an item under the Feature whose folder holds it. An item whose
   folder holds no Feature is left out of the sequence check (B-070).
6. **Grouping with an epic group (question C), "A per Feature
   (Recommended)".** Without the epic grammar, items number per Feature even
   when the task grammar names an `epic` group, as the default reached by
   omission does (B-068). The grammar's groups do not say whether a tree
   has epics; the epic grammar does (C-10).
7. **Message (question D), "A proposed text (Recommended)".** Without the
   epic grammar, the `SPEC5004` gap message reads
   `Feature {identity}'s item sequence skips {n} - tasks are numbered per Feature, contiguously from 01`
   (B-071). `{n}` is padded as B-053 says.
8. **Epic files (question E), "A read no epics (Recommended)".** Under
   schema `0.3.0` or later with no epic grammar declared, the check reads no
   epic file, so neither `SPEC1001` nor `SPEC1004` reports one (B-034).
   `specht frontmatter --kind epic` still checks the document it is given
   (`0001-F8`): that is an explicit request.
9. **The missing-epic finding (question F), "A proposed (Recommended)".**
   B-035's `SPEC3001` finding sits at the line of the `id` key, carries the
   rendered identity as its identifier, and reads
   `frontmatter has no epic - the manifest declares the epic grammar`. It
   holds from schema `0.3.0` only. This settles `0001-F7` OQ-72 with
   reading (a): no package carries schema `0.3.0` until B-035 is built.

The widening of B-005, B-006, B-053 and B-054 to a manifest declaring no
epic grammar was ruled on item `0016` (decision 0018, Affects) and is
written into those claims in the same change.

## Why

- Schemas below `0.3.0` require `epic` in a Feature's frontmatter. The
  epic-less reading there would change a published version's verdict
  (`0001-F7` C-1, C-4).
- A default without the epic grammar would break the live copy's match
  with the newest default (`0001-F4` B-004), the B-068 grammar and the
  source `init --epics` writes from.
- Under the epic grammar `{epic}-{id}` is the identity form, so an empty
  epic renders `-F2`. The Feature stays checked (C-11), and B-035 names
  the cause on the same file.
- Without epics, the identity is `{id}`, so an expectation of
  `{epic}-{feature}` points at a form the tree does not use.

## Rejected

- **Call 1:** the epic-less reading at every pin. It changes the
  verdicts B-049 keeps for `0.1.0` and `0.2.x`.
- **Call 2:** leaving the Feature out, against C-11; or identifying it by
  `id` alone, a second identity rule under one manifest.
- **Call 3:** a default without the epic grammar.
- **Call 4:** `{epic}-{id}`, `{epic}-{feature}`, or no `form` at all.
- Calls 5 to 9: no alternative beside the recommendation is recorded.

## Affects

- `0001-F5`: A-6, which also lists C-11 on spec-author's reading of
  call 1; B-005, B-006, B-024 to B-032, B-034, B-035, B-042, B-053 and
  B-054 amended; B-069 to B-071 added; OQ-4, OQ-13 and OQ-26 resolved;
  OQ-27 raised for the finding's `expected`, which call 9 does not state;
  § 5 rows 18 to 20 added.
- `0001-F3` B-012 amended.
- `0001-F7` OQ-72 resolved.
- Item `0345`.

## Reversal

None.
