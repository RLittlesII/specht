---
title: "Decision 0003: onboard exits 0 once the report is written, and 2 or 3 on an input failure"
description: "specht onboard returns the check's input-failure codes, 2 and 3, and 0 once the report is written whatever the check found; --output is required"
type: decision
---

# Decision 0003: `onboard` exits `0` once the report is written, and `2` or `3` on an input failure

**Date:** 2026-11-27
**Decided by:** the repository owner, answering "Do you accept reading (a) for OQ-2?" with "Accept (a)" (2026-11-27); recorded by `spec-author`

## The call

The owner accepted OQ-2's reading (a), as written:

1. On an input failure, `specht onboard` exits with the check's codes, `2` or
   `3`: `2` for a missing root or manifest, `3` for an invalid manifest.
2. It exits `0` once the report is written, whatever the check found, since
   the report explains and does not gate (C-2).
3. `--output` is required.

Derived by `spec-author`, not the owner's call: an absent `--output` exits
`2`. A missing required option is an input failure, call 1 gives an input
failure the check's codes, and `0001-F2` C-2 returns a missing input as `2`
and never as the command-line library's `-1`; `0001-F8` B-026 does the same
for `frontmatter`. What the command prints on these failures, and what it
does when the file cannot be written, reading (a) does not say; that is OQ-3.

## Why

The owner accepted the reading and gave no reason. The reading's own: the
report explains a check and does not gate on it (C-2), so a run that wrote
its report succeeded whatever the check found.

## Rejected

- **Reading (b), the check's own exit code, `1` when it reports findings.**
  spec-author's reading of the cost: a repository adopting Specht expects
  findings, so every onboarding run would read as a failure.

## Affects

- `0001-F15` OQ-2, resolved; B-007, added with its scenario and § 9 row;
  OQ-3, raised; items `0384` and `0385`, which carry B-007.

## Reversal

None.
