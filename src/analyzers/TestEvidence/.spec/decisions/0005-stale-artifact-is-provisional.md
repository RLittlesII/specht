---
title: "Decision 0005: Stale artifact is provisional"
description: "An artifact of the same configuration whose fingerprints differ is provisional recognition, not authoritative."
type: decision
---

# Decision 0005: Stale artifact is provisional

**Date:** 2026-10-24
**Decided by:** Repository owner, through the coordinator of item 0261

## The call

An artifact of the same configuration whose fingerprints differ is provisional recognition, not authoritative.

## Why

Provisional is never stricter than authoritative, so a source edit cannot brick the build.

## Rejected

- Treat a stale artifact as authoritative.

## Affects

B-017, B-018, B-019, B-022, B-023, B-024; OQ-7.

## Reversal

None.
