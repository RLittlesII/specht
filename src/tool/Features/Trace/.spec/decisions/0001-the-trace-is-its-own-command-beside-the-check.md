---
title: "Decision 0001: the trace is its own command beside the check"
description: "specht trace lists every claim's status, scenarios and test resolution through the matrix resolver, as lines or one JSON document, writes nothing and exits 0 whatever the gaps; the check's report carries no trace block"
type: decision
---

# Decision 0001: the trace is its own command beside the check

**Date:** 2026-10-14
**Decided by:** the repository owner (2026-10-14); recorded by spec-author

## The call

1. `specht trace` prints every claim of every specification: its § 9
   status, the scenarios carrying its tag and the resolution of each test it
   names. `--json` writes the same as one document with a published schema.
2. It resolves through `0001-F12`'s resolver and judges nothing: it exits
   `0` whatever the gaps, and `2` or `3` as the check does.
3. It has no rule id and is not schema-versioned. Its document changes only
   by addition within a tool major version.
4. The check's `--json` report carries no trace block.

## Why

- The check reports the rows that fail; a reader also needs the rows that
  pass, and the scenarios and tests behind them, in one listing.
- One resolver keeps the trace and the check in agreement (`0001-F12` C-3).
- Epic `0201` candidate D needs a text-level surface to extend with code
  bindings; this command is that surface, and answers epic `0201` OQ-11 as
  far as text-level resolution goes.

## Rejected

**A trace block inside the check's `--json` report.** The owner rejected it:
the check's report stays the verdict. Cost: a consumer that wants both runs
the tool twice.

Recorded by spec-author as what the call rules out:

- A rendered graph or an HTML page: not asked for.
- A non-zero exit for a gap: the trace is not a gate; the gate is
  `0001-F12`'s rules.

## Affects

- B-001 to B-009; A-1, A-2; C-1 to C-4; § 5 rows 1 to 5; OQ-1, OQ-2.
- `0001-F12` C-3; epic `0201` candidate D and OQ-11.

## Reversal

None.
