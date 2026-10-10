---
title: "Decision 0006: the migration summary is removed"
description: "The engine's migration summary, a sentence counting the legacy and the co-located specifications and the share migrated, is removed: no claim covers it, the tool never prints it, and it presumes exactly two layouts by name"
type: decision
---

# Decision 0006: the migration summary is removed

**Date:** 2026-10-09
**Decided by:** the repository owner, asked directly during delivery of item `0005` (§ 11 OQ-6); recorded by spec-author

## The call

`SpecCheckReport.MigrationSummary` is removed, with the assertions on its
text. The summary and the report name each layout and its count (B-009,
`0001-F2` B-002, `0001-F3` B-005) and say nothing about migration between
layouts.

## Why

- No claim in any Feature covers it, and the tool never prints it.
- It names two layouts by literal and computes a share that presumes exactly
  those two, which B-001 and B-009 no longer guarantee.

## Rejected

**Generalising it over the manifest's layouts.** A share migrated needs a
"from" and a "to" layout, which no manifest key declares. Cost of rejecting:
none today; no output carries it.

**Keeping it as it is.** Two layout names would stay literal in the engine,
against B-009. Cost of rejecting: two test assertions go.

## Affects

- B-009: no summary text names a layout except by its manifest name.
- § 11 OQ-6: resolved.
- No § 3 claim is added, changed or withdrawn.

## Reversal

None.
