---
title: "Decision 0005: the mirror's plan has unit tests"
description: "The mirror's plan computation is tested with the Python standard library's unittest against a fake gh and a synthetic git log, run by a build target, so its traceability rows can reach Covered"
type: decision
---

# Decision 0005: the mirror's plan has unit tests

**Date:** 2026-10-24
**Decided by:** the repository owner (2026-10-24), answering OQ-2; recorded by spec-author

## The call

The mirror's plan computation - what it would create, edit, close, reopen,
link, comment and mark stale for a given tree, history and set of issues - is
tested with the Python standard library's `unittest`. The tests replace `gh`
with a fake and git history with a synthetic log, and a build target runs
them. Which target runs them is OQ-8.

## Why

Without tests every row of § 9 stays `Missing`, and a `Missing` row blocks an
item reaching done, so no item cut from this Feature could close. A fake `gh`
and a synthetic log keep the tests offline and deterministic, as every other
test in the repository is.

## Rejected

**A reviewed dry run only.** Cost of rejecting: a test suite to keep beside a
script, where `next.py` has none. Taking it would leave the claims proven by a
reviewer reading a plan once, and § 9 unable to reach Covered.

## Affects

- `0055-F9` C-11 (new); OQ-2 resolved; OQ-8 (new); § 9.

## Reversal

None.
