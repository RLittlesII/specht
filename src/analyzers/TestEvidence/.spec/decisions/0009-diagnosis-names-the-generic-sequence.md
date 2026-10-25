---
title: "Decision 0009: Diagnosis names the generic sequence"
description: "Diagnosis text for B-018 and B-022 names the build, dotnet test --list-tests, dotnet test sequence; exact wording is the implementer's, but the message must name the producing or regenerating step."
type: decision
---

# Decision 0009: Diagnosis names the generic sequence

**Date:** 2026-10-24
**Decided by:** Repository owner, through the coordinator of item 0261

## The call

Diagnosis text for B-018 and B-022 names the build, dotnet test --list-tests, dotnet test sequence; exact wording is the implementer's, but the message must name the producing or regenerating step.

## Why

Only message text changes; formatVersion and exit codes stand.

## Rejected

- Reusing incompleteRun, which means owned unselected keys (B-026).

## Affects

B-017, B-018, B-019, B-022, B-023, B-024; OQ-11.

## Reversal

None.
