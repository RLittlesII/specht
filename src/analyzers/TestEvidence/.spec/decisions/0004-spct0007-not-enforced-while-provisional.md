---
title: "Decision 0004: SPCT0007 not enforced while provisional"
description: "SPCT0007 (mandatory testMethod/verifies) is not enforced while recognition is provisional; every other shape, grammar and resolution check stays enforced."
type: decision
---

# Decision 0004: SPCT0007 not enforced while provisional

**Date:** 2026-10-24
**Decided by:** Repository owner, through the coordinator of item 0261

## The call

SPCT0007 (mandatory testMethod/verifies) is not enforced while recognition is provisional; every other shape, grammar and resolution check stays enforced.

## Why

Enforcing the mandatory role would brick a clean checkout, a stale artifact or an IDE view; reconciliation catches an undiscovered verifies method (B-014).

## Rejected

- Require evidence before the first build; recognise by xUnit attribute.

## Affects

B-017, B-018, B-019, B-022, B-023, B-024; OQ-6.

## Reversal

None.
