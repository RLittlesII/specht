---
title: "Decision 0009: Diagnostic identity and versioning"
description: "Use stable SPCT diagnostic IDs and analyzer package releases without widening the pinned checker vocabulary"
type: decision
---

# Decision 0009: Diagnostic identity and versioning

**Date:** 2026-10-17
**Decided by:** Repository owner

## The call

Use `SPCT####` for analyzer diagnostic IDs: the `SPCT` prefix followed by four
decimal digits, starting with `SPCT0001`. Individual finding-to-ID assignments
remain downstream; this decision selects the vocabulary and permanence policy.

A published diagnostic retains its ID across analyzer releases. A retired ID
is never reused for another diagnostic. Consumer settings and citations must
continue to identify the same finding across releases.

Analyzer rules follow analyzer package releases. An analyzer-only rule addition
does not add to or alter the checker's pinned `SPEC###` vocabulary. A change to
the manifest/schema contract still follows the existing schema-version rules;
an analyzer package release is not a waiver of those rules.

This does not decide the public deployment package ID, release cadence or
version relationship to `specht.tool` (0201-F6 OQ-1 / epic OQ-1). Severity,
suppression and gradual-adoption policies remain in 0201-F6 OQ-2 / epic OQ-9.

## Why

Stable diagnostic IDs let settings, suppressions and references remain useful
across releases. A separate analyzer vocabulary permits analyzer rules to evolve
through their package releases while preserving the existing checker schema's
versioned vocabulary.

## Rejected

- Reuse `SPEC###` for new analyzer findings: couples analyzer additions to the
  checker's pinned rule vocabulary and its schema-version obligations.
- Renumber IDs to tidy releases or reuse retired IDs: breaks citations and can
  make existing consumer settings apply to a different finding.
- Treat an analyzer release as permission to change a manifest/schema contract
  without its required schema version: weakens the consumer pin guarantee.

The accepted cost is a separate stable diagnostic vocabulary and a retained
history of retired IDs alongside the existing checker vocabulary.

## Affects

0201-F2 B-035 through B-038 and OQ-2; epic 0201 OQ-9's identity/versioning portion.
0201-F6 retains installation, package/version relationship, severity and
suppression questions. Exact diagnostic assignments remain implementation
planning, rather than being inferred from examples or claim numbers.

## Reversal

None.
