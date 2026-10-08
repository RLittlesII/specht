---
title: "Decision 0001: epic files live under epics/"
description: "An epic file lives at epics/<epic>/epic.md, where schema version 1's epic glob epics/**/epic.md finds it; this repository's epic moved there, and a second epic glob was turned down"
type: decision
---

# Decision 0001: epic files live under epics/

**Date:** 2026-10-07
**Decided by:** the repository owner (amendment brief D4, epic 0001 round 1); recorded by spec-author

## The call

An epic file lives at `epics/<epic>/epic.md`. This repository's epic moved
from `.spec/epics/0001-specht/epic.md` to `epics/0001-specht/epic.md`. The
default manifest keeps the one epic glob `epics/**/epic.md`; schema version 1
does not change.

## Why

- At `.spec/epics/` the epic sat inside the root `.spec/`, which the default
  manifest excludes (C-4), and outside `epics/**/epic.md`. Its frontmatter was
  never checked, by either cause on its own.
- `epics/**/epic.md` is `hooked`'s glob. Keeping it keeps the default manifest
  equal to `hooked`'s (`0001-F5` C-4) and needs no schema change before version 1
  freezes at first publish.

## Rejected

**A second epic glob in the default manifest** (for example
`.spec/epics/**/epic.md`).

- It would also have needed a carve-out from the root `.spec/` exclusion, which
  is the hardcoded special case C-4 rules out.
- A default that differs from `hooked`'s for this repository's sake.
- Cost of rejecting: a consumer that keeps epics elsewhere declares its own glob
  in its manifest (B-003). Taken.

## Affects

- § 3 B-003 (source cell).
- § 2 row 5.
- § 11 OQ-1, resolved.
- `epics/0001-specht/epic.md` § "Placement" (already amended by the coordinator).

## Reversal

None.
