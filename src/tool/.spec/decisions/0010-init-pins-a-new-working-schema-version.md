---
title: "Decision 0010: Init pins a new working schema version"
description: "Deliver init's explicit schemaVersion through working schema 0.1.1 while preserving locked 0.1.0."
type: decision
---

# Decision 0010: Init pins a new working schema version

**Date:** 2026-10-13
**Decided by:** repository owner

## The call

Create schema `0.1.1` with an explicit `schemaVersion: "0.1.1"` in its
shipping manifest. Preserve every locked `0.1.0` file and its lock entry.
The owner selected this option when the process test for B-012 found that
the `0.1.0` manifest has no explicit pin.

Copy the complete version set, rewrite the frontmatter schema addresses and
rule-page version rows, and retain the same rule vocabulary and validation
semantics. A patch is sufficient because the explicit pin changes no tree's
verdict (C-11). Keep `0.1.1` unlocked as the working version (B-090, B-091).

Bring this repository's live manifest and frontmatter schemas to `0.1.1`
to preserve `0001-F4` B-004. This is the live-copy update for this delivery;
it does not decide OQ-56's automatic cut-target behavior. The cut target
itself remains outside this change.

## Why

B-012 requires the written manifest to carry the newest version explicitly.
Init already selects by semver precedence and copies the shipping bytes
unchanged (`0001-F4` B-003). Adding the pin to a new shipping manifest
satisfies both claims and C-17's lock guarantee.

## Rejected

- Editing `0.1.0` or regenerating its lock entry would violate C-17 and
  [ADR-0002](../adr/0002-schema-locks-are-independent-of-tool-versions.md).
- Stopping at a failing process test would leave B-012 undelivered. The
  owner selected the new version instead of deferring to a cut dependency.
- Rewriting the pin inside init would change its byte-copy contract and is
  unnecessary when the shipping manifest supplies the value.

## Affects

- B-012; C-11 and C-17; sections 6 to 9.
- The working version is `0.1.1`; the frozen version remains `0.1.0`.
- `0001-F4` B-003 and B-004 remain in force.
