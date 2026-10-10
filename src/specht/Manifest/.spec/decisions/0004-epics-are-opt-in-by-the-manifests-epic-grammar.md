---
title: "Decision 0004: Epics are opt-in by the manifest's epic grammar"
description: "A repository has an epic tier only when its manifest declares the epic grammar; without it a Feature's identity is its id alone, unique across the repository, shipped as an additive change to schema version 1 with no new rule id"
type: decision
---

# Decision 0004: Epics are opt-in by the manifest's epic grammar

**Date:** 2026-10-09
**Decided by:** the repository owner (2026-10-09); recorded by spec-author

## The call

Epics are opt-in. Resolves OQ-3.

- **The signal** is the manifest's epic grammar, `identifiers.epic`. Every
  manifest that exists today declares it, including `hooked`'s and this
  repository's, so every existing run is unchanged and B-016 still reproduces
  the golden report. A manifest without it has no epic tier.
- **Identity.** With the epic grammar, a Feature's identity is
  `{epic}-{id}` (B-024). Without it, the identity is the frontmatter `id`
  alone, and it must be unique across the repository (B-025, B-026).
- **Every Feature is checked.** `SPEC012` and `SPEC050`-`SPEC052` never leave
  out a Feature for carrying no `epic` (C-11, B-027). An edge in the qualified
  `NNNN/` form, in a repository without epics, is reported under `SPEC050`
  (B-029), never dropped.
- **Items.** An item's `parent` equals its Feature's identity (`SPEC043`:
  B-030, B-031). `SPEC044` numbers by the task grammar's named `epic` group
  (B-006). Without the epic grammar, and with no such group, numbering is per
  parent Feature (B-032).
- **The epic frontmatter schema** is read only when the epic grammar is
  declared (B-008, B-033, B-034).
- **The epic grammar is the one value never filled from the default manifest.**
  Its absence is the signal, so B-019's fill excludes it. This follows from the
  signal. It was not a separate call.
- **Schema version 1, additive.** In version 1's Feature frontmatter schema,
  `epic` is no longer `required`. The task `parent` pattern widens to
  `^([0-9]{4}-)?F[0-9]+[a-z]?$`, and the task `id` pattern widens to allow a
  per-Feature sequence, in the form OQ-5 settles. With the epic grammar
  declared, epic mode stays as strict as it is today: a missing `epic` is a
  `SPEC011` violation in either layout (B-035), and a bare `parent` is a
  `SPEC043` violation (B-031).
- **No new `SPEC###`.** The rules that exist carry the epic-less checks.
- **`specht init`** writes the epic-less shape unless it is given `--epics`
  .
- **This repository keeps its epics,** `0001` and `0101`. Its manifest keeps
  the epic grammar.

## Why

- Standalone Features with no epic tier are a real consumer shape. Today the
  engine cannot check one. `epic` is required, the identity has an epic in
  it, and `SPEC012` and `SPEC050`-`SPEC052` silently skip a Feature with no
  `epic`. A tree made only of such Features would never get a duplicate or
  edge verdict.
- **Additive, not version 2.** Version 1 is still open. No package has been
  published, and `0001-F7` decision 0002 freezes version 1 only at the first
  publish, as `0001-F7` decision 0003 already used. The change only loosens
  version 1: a consumer's stricter on-disk copy, which requires `epic`, stays
  a valid version 1 copy. So "pinned to 1" still means one contract. The rule
  vocabulary is unchanged (`0001-F1` C-5, C-3 here). Only the schema loosens.
  The engine's checks, keyed on the manifest, take back the strictness the
  schema gave up.
- **The epic grammar as the signal.** The grammar is already a declared
  manifest value, and no rule reads it today. Using it adds no manifest key.
  B-012 would reject a new key in every older tool.

## Rejected

**Ship epic opt-in as schema version 2.** The owner turned it down. Every
consumer would face a version move for a change that only loosens version 1,
and version 2 is already reserved for epic `0101`'s rules (`0001-F7` § 5
row 3).

**Derive the identity from the path when there are no epics.** Turned down.
Identity comes from the frontmatter (`0001-F1` B-003). A path-derived
identity would change when a specification moves, and the co-located layout
exists so that a move keeps the identity.

**A new rule id for epic-less checks.** The vocabulary is fixed per version
(`0001-F1` C-5, C-3 here). `SPEC011`, `SPEC012`, `SPEC043`, `SPEC044` and
`SPEC050` already state these checks.

**A dedicated manifest key, such as `"epics": false`.** Turned down. It
duplicates what the epic grammar already says, and two signals could
disagree.

**Fill an omitted epic grammar from the default manifest, as B-019 fills every
other grammar.** Turned down, because then no manifest could opt out. The
cost is that a manifest written from scratch with no `identifiers` has no
epic tier. B-016's check "by omission" has to keep the epic grammar.

**This repository drops its epics.** Turned down. Epics `0001` and `0101` stay.

## Affects

- `0001-F5` § 2 need 6 added. § 3 B-008 and B-019 amended, and B-024 to
  B-035 added. § 4 C-10 and C-11 added. § 5 rows 8 to 11 added. § 11 OQ-3
  resolved, and OQ-4 to OQ-7 opened.
- `0001-F1` § 2 A-3 amended. § 3 B-002, B-003 and B-011 amended to depend
  on whether the epic grammar is declared. § 5 row 14 added.
- `0001-F4` § 2 need 5 added and A-1 amended. § 3 B-001 withdrawn, B-003
  amended, and B-018 to B-022 added. § 4 C-5 and § 5 row 7 added. § 11 OQ-2
  opened.
- `0001-F6` § 5 row 7.
- `0001-F7` § 2 A-2 amended. § 3 B-037 and B-038 added. § 5 row 7 added.
- brief § 6, the row "Are epics required?".
- `.spec/schema/feature-spec.frontmatter.schema.json` and
  `task.frontmatter.schema.json`, the live copy of version 1, and the embedded
  `v1` copies. They change with the items cut from these claims, not with this
  record.

## Reversal

None.
