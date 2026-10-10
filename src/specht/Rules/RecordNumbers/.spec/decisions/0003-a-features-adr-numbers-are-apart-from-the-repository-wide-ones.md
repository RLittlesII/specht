---
title: "Decision 0003: a Feature's ADR numbers are apart from the repository-wide ones"
description: "A Feature's adr folder and the repository-wide .spec/adr are two number spaces, so the same number in both is not a duplicate; one ADR number space for the repository was turned down, and how a Feature-level ADR is cited stays unasked"
type: decision
---

# Decision 0003: a Feature's ADR numbers are apart from the repository-wide ones

**Date:** 2026-10-09
**Decided by:** the repository owner (2026-10-09, answering `0101-F7` OQ-2); recorded by spec-author

## The call

A Feature's `adr/` folder and the repository-wide `.spec/adr` are numbered
apart. A Feature's `adr/0002` and `.spec/adr/0002` are two records, and the
rule reports nothing for the pair.

## Why

Stated by the owner:

- Of "Separate, per folder" and "One ADR space", the first.

The question's wording beside that option, not the owner's statement:

- A Feature's `adr/0002` and `.spec/adr/0002` are not duplicates.

The author's inference, not stated by the owner:

- The pair is one case of the per-folder rule (C-1; B-007): no claim is
  written for ADR folders alone.
- `specht-conventions` § Specs and the ADR template already number a
  Feature's `adr/` "per Feature from 0001" and `.spec/adr/` "repo-wide from
  0001"; the answer keeps both as written.

## Rejected

**One ADR space.** Rejected by the owner. The cost of rejecting is the
question's wording, not the owner's statement: a bare `ADR-0002` citation
could mean either record.

Not asked, and not decided here: how a Feature-level ADR is cited so that it
names one record. That is a conventions question, and this Feature resolves
no citation (§ 5 row 9).

## Affects

- `0101-F7` B-007 and C-1 (Source; wording unchanged in what it requires);
  § 5 row 6; OQ-2 (resolved).
- Decision 0002: its "Per folder" reading is, for the ADR pair, now the
  owner's answer.
- `specht-conventions` § Specs: relied on, unchanged.

## Reversal

None.
