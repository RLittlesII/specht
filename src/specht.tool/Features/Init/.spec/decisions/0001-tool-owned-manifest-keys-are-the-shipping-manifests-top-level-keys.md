---
title: "Decision 0001: tool-owned manifest keys are the shipping manifest's top-level keys"
description: "B-004 compares the embedded and live manifests on the top-level keys the embedded manifest of that version carries; any further key in the live manifest is consumer configuration and outside the comparison"
type: decision
---

# Decision 0001: tool-owned manifest keys are the shipping manifest's top-level keys

**Date:** 2026-10-09
**Decided by:** the repository owner, during planning of item `0051`; recorded by spec-author

## The call

For B-004 and C-2, a version's tool-owned manifest keys are the top-level keys
the embedded (shipping) manifest of that version carries. The live manifest
must equal the embedded one on each of those keys. Any top-level key the live
manifest carries beyond them - rule settings, other consumer configuration - is
outside the comparison.

## Why

- No specification lists the tool-owned keys: `0001-F7` decision 0001 names
  none, and Manifest OQ-1 leaves the rule-settings key names open.
- The embedded manifest is the tool's own statement of what a version ships, so
  it defines the set without a second list to keep in step.
- B-004's comparison stays bound while Manifest OQ-1 is open: a key name chosen
  later is consumer configuration in the live manifest until the shipping
  manifest carries it.

## Rejected

**A named exclusion list.** Needs the rule-settings key name before Manifest
OQ-1 decides it. Cost of rejecting: a key the live manifest adds by mistake
passes the comparison unflagged.

**Full manifest equality until `0001-F5` lands.** Leaves B-004's rule-severity
scenario unbound, since this repository's live manifest carries settings the
shipping one does not. Cost of rejecting: the comparison checks less of the
live manifest than full equality would.

## Affects

- `0001-F4` § 3 B-004; § 4 C-2; § 2 A-2 (amended).
- `0001-F7` decision 0001: names the tool-owned files, not the keys; this
  decision supplies the keys.
- `0001-F5` (Manifest) OQ-1: unchanged; this decision does not depend on its
  answer.

## Reversal

None.
