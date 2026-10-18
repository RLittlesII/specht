---
title: "Decision 0005: Checked target boundaries"
description: "Limit checked relationships to claims and ADRs while Feature and epic identities establish scope"
type: decision
---

# Decision 0005: Checked target boundaries

**Date:** 2026-10-17
**Decided by:** Repository owner

## The call

Checked relationship targets are claims and ADRs. Feature and optional epic
components establish owner scope; they do not introduce separate Feature-only
or epic-only checked relationships. The `spec` role remains Feature authoring
context, not checked evidence. Defer additional record kinds, including tasks,
constraints and lessons.

This is a record-kind boundary, not an extra restriction on consumer ID grammars.
A valid claim ID is still governed by the consumer's claim grammar. Existing XML
roles, ADR ownership and lifecycle policies remain unchanged.

## Why

Claims and ADRs have agreed relationship roles, ownership and lifecycle behavior.
Additional kinds would need their own semantics before analysis could report
meaningful checked evidence.

## Rejected

- Treat every repository record as a checked target immediately: broadens the
  model without deciding relationship roles or lifecycle semantics.
- Treat Feature defaults as checked Feature evidence: confuses authoring context
  with an explicit checked relationship.

The accepted cost is deferring additional kinds until their behavior is specified.

## Affects

0201-F1 B-012; epic 0201 OQ-3's remaining target-kind question.

## Reversal

None.
