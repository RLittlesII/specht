---
title: "Decision 0001: Markdown remains authoritative and indexed answers stay current"
description: "The database is a disposable local index of Markdown; every MCP answer reflects the current repository tree, including direct edits and branch switches."
type: decision
---

# Decision 0001: Markdown remains authoritative and indexed answers stay current

**Date:** 2026-10-19
**Decided by:** the repository owner

## The call

Markdown in the repository is the source of truth. The database is a rebuildable index of its files and facts. An indexed MCP answer must reflect the current tree, including after a direct Markdown edit or branch switch. The index may validate and read source files as needed to establish that answer; the optimization target is to avoid reparsing unchanged Markdown for repeated keyed lookups.

## Why

The owner wants key traversal and lower repeated lookup cost while keeping repository specifications authoritative. The MCP epic already requires the answer on the same tree to agree with the command line. An explicitly stale answer would break that agreement and could send an agent to a removed or amended claim.

## Rejected

- **Database records as the authoritative documents:** this would change authoring, Git review, offline checking and the CLI input contract. Rejecting it retains synchronization work between files and the index.
- **A named database snapshot until explicit refresh:** this avoids freshness checks on every lookup but can answer from an earlier tree after a direct edit or branch switch. Rejecting it means validation may still reread source bytes, and the performance benefit must be measured rather than assumed.

## Affects

`0132-F1` B-001, B-003, B-004, B-006, C-1, C-8 and OQ-1; epic `0132` same-tree parity and write boundary.
