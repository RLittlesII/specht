---
title: "Decision 0002: migrating the tracker to GitHub issues is rejected"
description: "Moving delivery state into GitHub issues and deleting the .issue/ work items was considered and rejected; the work items stay the tracker"
type: decision
---

# Decision 0002: migrating the tracker to GitHub issues is rejected

**Date:** 2026-10-24
**Decided by:** the repository owner (2026-10-24); recorded by spec-author

## The call

The tracker is not migrated to GitHub issues, and the work items are not
deleted. The option was considered earlier on 2026-10-24 and rejected before
decision 0001 was taken.

## Why

- **The tracker stays offline, deterministic and branch-local.** An item is a
  file: a branch changes it, the change is reviewed in the diff, and the same
  tree gives the same queue with no network.
- **Delivery state stays in the pull-request diff.** The pull request that
  delivers an item writes `done` and `closed:` into it before it opens
  (AGENTS.md § SDLC); an issue's state would change outside any diff.
- **`0101-F8` keeps its dogfooding ground.** Its rules check work-item id
  allocation in a repository that declares its work-item files; this
  repository is the first tree they run on.
- **No specification link goes dead.** Specifications, items and commit
  messages cite item ids and item paths; deleting the files breaks every one.
- **The queue script keeps reading files.** `next.py` reads the items; a
  migration would rewrite it against the GitHub API, which the check never
  calls.

## Rejected

**Migrate and delete** is the option this record rejects. Cost of rejecting:
GitHub's own issue features (assignment, notifications, cross-repository
references) are not the tracker; decision 0001's mirror shows the state
without them.

## Affects

- `0055-F9` § 4 C-2, C-3, C-8; § 5 row 1.

## Reversal

None.
