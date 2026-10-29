---
title: "Decision 0001: verbosity is its own Feature under epic 0001"
description: "Output verbosity spans the check, init and upgrade with one invariant, so it is 0001-F17 beside the commands rather than claims in one command's Feature or in epic 0194"
type: decision
---

# Decision 0001: verbosity is its own Feature under epic 0001

**Date:** 2026-10-29
**Decided by:** `spec-author`, on the owner's question of 2026-10-29; open to the owner's review with OQ-5

## The call

Output verbosity is `0001-F17`, specified at `src/tool/Features/Verbosity/.spec/`. Its invariant is that a level changes only stdout outside `--json` and never a rule's level (C-1, C-2).

## Why

- The candidate commands are the check (`0001-F2`), `init` (`0001-F4`) and `upgrade` (`0001-F7`) (OQ-3). One invariant over three commands' output is a Feature of its own; a Feature is cut where the invariant changes (epic `0001` § Features).
- Every command lives in the tool, so the specification sits under `src/tool/Features/`, beside the commands it governs.

## Rejected

- **Claims in `0001-F2`.** The check's Feature would then own `init`'s and `upgrade`'s output.
- **Claims in `0001-F7`.** It is the 0.3.0 milestone's work and owns schema versioning, not output.
- **Epic `0194`.** It explores presentation widgets - trees, tables, colour - not how much is printed.
- **A per-rule switch.** `0001-F11` already sets a rule `off` or `warning`; a second switch would give a rule two off states (C-2).

## Affects

- `0001-F17` C-1, C-2; § 5 rows 1, 4 and 5.
- Epic `0001`: `children` and § Features gain `0001-F17`.

## Reversal

None.
