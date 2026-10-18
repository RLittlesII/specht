---
title: "Decision 0001: Hardened hybrid bindings"
description: "Type defaults aid authoring; explicit member IDs carry checked evidence, with a framework-independent model and optional Reqnroll bridge"
type: decision
---

# Decision 0001: Hardened hybrid bindings

**Date:** 2026-10-17
**Decided by:** Repository owner

## The call

Adopt type-level defaults for authoring and explicit member identities for checked
traceability. An invoked fix materializes a known complete target; inherited-only
bindings do not count. Explicit member identity overrides the type in full.
Check both ends: selected methods lacking bindings produce diagnostics; declared
claims lacking policy-required evidence produce reconciliation gaps even when
no method references them.

Keep binding identity framework-independent and Reqnroll optional. Direct chains
support xUnit facts and theories and Playwright .NET tests. Reqnroll augments them
with scenario links. Consumers retain configured Feature/claim grammars, including
an absent epic tier. The exact XML encoding remains open in 0201-F2.

## Why

The initial three-leg spike demonstrated silent retargeting after moving an
inherited member between Features sharing a local claim ID. Explicit identities
survive the move; type context still narrows safe authoring choices. Checking
only existing annotations would miss an entirely unbound claim, so reconciliation
starts with the declared claims and code analysis starts with the selected methods.

## Rejected

- Path-scoped inheritance (A): less repetition, but both member moves and
  specification moves can change or break targets.
- Explicit members alone (B): preserves identity but loses useful authoring
  context and offers broader claim choices.
- Implicit hybrid (basic C): combines convenient defaults with silent retargeting.
- Mandatory Reqnroll: would exclude useful direct test chains and couple identity
  resolution to a scenario framework.
- Automatic intent inference from names: a matching name or regex cannot prove
  which claim an assertion tests.

The accepted cost is repeated persisted IDs and separate discovery/result
adapters. A wrong but existing target remains possible and requires review of
the test's meaning; the tool must not label resolution as behavioral proof.

## Affects

Epic 0201 OQ-2 and OQ-12 (model decision resolved, remaining details retained);
0201-F1 through F6, especially 0201-F2 B-001 to B-004 and B-007, 0201-F3 B-001 to
B-003, and 0201-F5 B-001 and B-006. The spike is experimental; no public package,
diagnostic vocabulary or assembly architecture is adopted here.

## Reversal

None.
