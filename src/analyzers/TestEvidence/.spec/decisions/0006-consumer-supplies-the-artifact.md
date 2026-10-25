---
title: "Decision 0006: Consumer supplies the artifact"
description: "The project's own artifact is not auto-included; the documented sequence has the consumer supply it as SpechtTestEvidence."
type: decision
---

# Decision 0006: Consumer supplies the artifact

**Date:** 2026-10-24
**Decided by:** Repository owner, through the coordinator of item 0261

## The call

The project's own artifact is not auto-included; the documented sequence has the consumer supply it as SpechtTestEvidence.

## Why

The artifact is not a build input of the export, so supplying it starts no cycle; automatic inclusion would be a hidden coupling to 0260's path.

## Rejected

- Auto-include obj/specht-test-evidence.

## Affects

B-017, B-018, B-019, B-022, B-023, B-024; OQ-8.

## Reversal

None.
