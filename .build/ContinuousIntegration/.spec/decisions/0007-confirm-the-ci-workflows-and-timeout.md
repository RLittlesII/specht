---
title: "Decision 0007 — Confirm the CI workflows and timeout"
description: "Pull requests run on macOS, pushes to main run on macOS and Windows, and each build job has a 45-minute limit."
type: decision
---

# Decision 0007 — Confirm the CI workflows and timeout

**Date:** 2026-11-17
**Decided by:** The repository owner

## The call

- Confirm `ci` as one macOS build check for pull requests targeting `main`.
  Confirm `main` as separate macOS and Windows checks on pushes to `main`.
- Ratify the 45-minute limit on each `build` job in both workflows.
- Keep the CI Feature open until the coverage gaps and the merge-tree check
  are resolved.

## Why

The separate workflows already run this way. The macOS PR job provides the
pre-merge result. The Windows leg on `main` exercises Windows-specific
behavior after merge. The timeout frees runners held by hung jobs.

## Affects

OQ-5 and OQ-6 are resolved. B-003, B-004, B-006, B-007, B-010, B-013 and
B-015 follow the workflow split. B-016 is withdrawn. C-5 applies to the
documentation skip inside the macOS job. B-027 retains the implemented limit.

## Reversal

Supersedes the PR Windows check and inside-the-Windows-job skip in
[decision 0003](0003-run-the-windows-gates-on-main-only.md).
