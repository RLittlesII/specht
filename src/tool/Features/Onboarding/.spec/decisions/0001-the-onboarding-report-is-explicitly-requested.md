---
title: "Decision 0001: the onboarding report is explicitly requested"
description: "The onboarding report is opt-in and available in the terminal and as a standalone file; ordinary check output stays unchanged"
type: decision
---

# Decision 0001: the onboarding report is explicitly requested

**Date:** 2026-10-23
**Decided by:** the repository owner; recorded by `spec-author`

## The call

The onboarding report is a separate, explicitly requested output. It is available in the terminal and as a standalone file. An ordinary check does not add onboarding explanations.

## Why

The established check output is used by shells, build targets, hooks, and CI. Keeping onboarding explanations opt-in preserves that output contract while still giving a repository owner a complete explanation when adopting Specht.

## Rejected

Automatically adding explanations to the first ordinary check was rejected. Its cost is changing the check's output for users and integrations that rely on its existing diagnostic stream.

## Affects

- `0001-F15` B-003 and B-006; § 5 rows 1.
- The exact command syntax and standalone file format remain open in `0001-F15` OQ-1.

## Reversal

None.
