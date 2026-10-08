---
title: "Decision 0001: both layouts are held to the same rules"
description: "hooked's draft constraint C-14 carried into 0001-F1: the legacy and co-located layouts are read by the same rules, except SPEC011, and neither fails for its layout"
type: decision
---

# Decision 0001: both layouts are held to the same rules

**Date:** 2026-10-07
**Decided by:** `hooked`'s draft specification 0008-F3 (its C-14); carried into this repository by spec-author

## The call

Every rule applies to a specification in the legacy layout and one in the
co-located layout alike, except `SPEC011`, which checks a legacy specification's
`epic` and `id` against its directory and has no co-located meaning (README
§ 4). No violation is raised for the layout a specification is in.

The source is `hooked`'s draft 0008-F3, C-14, at `a6d056f`
(`tools/SpecGovernance/.spec/README.md`): "Both layouts are read by the same
rules for as long as `.spec/migration.md` has an unconverted row, and a
specification never fails for being un-migrated." That file is not in this
repository until README § 3 copies it to `docs/reference/0008-F3-draft-spec.md`;
this record is the citation until then.

## Why

- The engine as copied already behaves this way; B-002 states what the 56
  tests and the baseline report hold, so it is a claim about the copy, not a
  new requirement.
- Transporter's tree predates the schema (README § 2). A rule that failed a
  specification for its layout would fail the "done" install for a reason the
  agent cannot repair from the report.
- C-14's "for as long as `.spec/migration.md` has an unconverted row" is
  `hooked`'s migration plan, not this tool's; the tool reads no migration file,
  so the condition is dropped and the rule holds unconditionally in version 1.

## Rejected

- A layout-specific rule, or a "not yet migrated" violation: it turns a layout
  choice into an error and forces a migration the README never asks for.
- Citing `hooked` C-14 alone: it is unreachable from this repository.

## Affects

`B-002`; README § 4 `SPEC011`.

## Reversal

None.
