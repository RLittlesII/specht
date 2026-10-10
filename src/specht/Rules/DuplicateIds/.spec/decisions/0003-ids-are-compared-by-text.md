---
title: "Decision 0003: ids are compared by text"
description: "Two constraint or open-question ids are the same only when their text is the same, as SPEC030 compares claim ids, so C-1 and C-01 are different ids; comparing by number was turned down"
type: decision
---

# Decision 0003: ids are compared by text

**Date:** 2026-10-09
**Decided by:** the repository owner (2026-10-09, answering `0101-F6` OQ-3); recorded by spec-author

## The call

Two ids are the same when their text is the same. `C-1` and `C-01` are
different ids, and a table that declares each once has no duplicate.

## Why

Stated by the owner:

- Ids are compared as `SPEC030` compares claim ids.

## Rejected

**Comparing by number**, so that `C-1` and `C-01` are one id declared twice.
Rejected by the owner. The cost is the author's assessment, not the
owner's: a table can hold `C-1` and `C-01` with no finding from this
Feature, and `0101-F1` C-1 gives the two one position in the order.

## Affects

- `0101-F6` B-015 and C-11 (added); B-001 and B-002 (Source); § 5 row 13
  (added); OQ-3 (resolved).

## Reversal

None.
