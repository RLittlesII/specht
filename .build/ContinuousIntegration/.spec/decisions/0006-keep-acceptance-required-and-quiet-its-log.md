---
title: "Decision 0006: Keep acceptance required and quiet its log"
description: "The acceptance tier remains a required pull request gate and CI shows the test summary and failure details."
type: decision
---

# Decision 0006: Keep acceptance required and quiet its log

**Date:** 2026-11-09
**Decided by:** the repository owner

## The call

- Keep the acceptance tier in the required pull request build (B-004, B-005), with coverage measured on the pull request's head (`0055-F3` B-001, B-002).
- Show the acceptance test summary and failure details in CI and omit routine output from passing and skipped scenarios (B-025, B-026). Keep local diagnostic output available.
- Measure runtime before reconsidering when the acceptance tier runs.

## Why

Passing scenarios and skips fill the build log with step output, debug messages and skip details. The summary reports their counts. A failure still needs its own details so a contributor can investigate it.

## Rejected

**Run acceptance only after the merge.** The October 26 proposal at commit `ee607438` would find an acceptance failure after the pull request merged and carry coverage forward from an earlier main commit.

**Move acceptance to a separate required check.** Deferred until there is a measured benefit. The check would still have to pass before merge, and it would need its own branch protection rule.

## Affects

B-025, B-026, their scenarios and § 5 row 31.

## Reversal

None.
