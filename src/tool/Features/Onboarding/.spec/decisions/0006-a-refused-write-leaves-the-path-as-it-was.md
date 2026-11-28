---
title: "Decision 0006: a refused write leaves the path as it was"
description: "When the file system refuses the write to --output, specht onboard keeps any earlier file exactly as it was and never leaves a partial report"
type: decision
---

# Decision 0006: a refused write leaves the path as it was

**Date:** 2026-11-27
**Decided by:** the repository owner, answering "Do you accept reading (a) for OQ-6?" with "Accept (a)" (2026-11-27); recorded by `spec-author`

## The call

The owner accepted OQ-6's reading (a). The coordinator's summary, which the
owner answered, read: "The recommended reading for OQ-6 keeps any earlier
file exactly as it was, and never leaves a partial one, so the report is
written in full or not at all. It's recommended because it asks nothing more
of a path the file system just refused."

When the file system refuses the write to `--output` (B-010, exit `4`), the
path holds what it held before the request: an earlier run's file, unchanged,
or no file. B-019 states it.

## Why

The owner accepted the reading and gave no reason. The reading's own, as the
coordinator's summary put it: it asks nothing more of a path the file system
just refused.

## Rejected

Not chosen: OQ-6's reading (b), no file left, removing an earlier one as
B-017 does for a request that exits `2` or `3`; and reading (c), whatever the
file system leaves, with no promise. The summary put to the owner named
reading (a) only.

## Affects

- `0001-F15` OQ-6, resolved; B-019, added with its scenario and § 9 row;
  items `0384` and `0386`, which carry B-019.

## Reversal

None.
