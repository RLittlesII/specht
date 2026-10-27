---
title: "Decision 0003: proceed on the recorded no-manifest and text-form defaults"
description: "The command delivery uses the existing B-013 and B-019 defaults while OQ-1 and OQ-2 remain open and attributed to their original authors"
type: decision
---

# Decision 0003: proceed on the recorded no-manifest and text-form defaults

**Date:** 2026-10-26
**Decided by:** `spec-author`, agreement audit. This is a delivery exception on existing derived defaults, not owner ratification.

## The call

Proceed against B-013 and B-019 as written. A missing manifest gives exit `2`
with no listing. Text output has four tab-separated fields per rule, with
lower-case severity and effective level, as decision 0002 specifies.

OQ-1 and OQ-2 remain open. Their defaults are sufficient to build and test the
existing claims; this assessment neither answers the owner questions nor
changes their provenance. An owner answer requires an amendment to the claims
and scenarios before the implementation follows it.

## Why

The command item explicitly names both defaults and the claims already state
their observable outcomes. Proceeding exercises that recorded agreement without
selecting a new behavior. The missing-manifest outcome also matches the check
and `--explain`, whose loader supplies the pinned version and effective levels.

## Rejected

**List the newest version without a manifest.** This would replace B-013 and
choose the unresolved alternative in OQ-1. Cost of rejecting: a caller needs a
manifest before listing rules.

**Choose a table or another text layout.** This would replace B-019 and decision
0002 while OQ-2 is still open. Cost of rejecting: the default remains a plain
line format rather than a visually aligned table.

**Treat delivery as owner ratification.** Implementing existing claims is not
evidence that the owner answered their open questions. Cost of rejecting: those
questions remain visible for a later owner decision.

## Affects

- `0001-F10` B-013, B-019 and OQ-1/OQ-2; no claim text changes.
- A-2 and the off-default scenario remain unchanged.
- JSON, level mutation, titles, filtering and architecture migration remain
  outside this text-command delivery.

## Reversal

None. A future owner decision supersedes these defaults by amending the agreement.
