---
title: "Decision 0007: the mirror is a Python script beside the queue script"
description: "The mirror is a standard-library Python script driving the gh command, sharing the item reader of the queue script next.py, not a NUKE build target in C#"
type: decision
---

# Decision 0007: the mirror is a Python script beside the queue script

**Date:** 2026-10-24
**Decided by:** the repository owner (2026-10-24), answering OQ-1; recorded by spec-author

## The call

The mirror is a Python script that uses only the standard library and the `gh`
command, and it reads items through the item reader it shares with the queue
script `next.py` (C-7).

## Why

The queue script already reads every work item in Python. Sharing its reader
keeps one parser for both, so the mirror and the queue cannot disagree about
an item. The `renovate` workflow already runs a step that is not a build
target, so a workflow step outside NUKE has precedent.

## Rejected

**A NUKE build target in C#.** Cost of rejecting: the mirror's logic sits
outside the build project, where the rest of the repository's automation is.
Taking it would cost a second item parser in C# beside `next.py`'s, which can
disagree with it.

## Affects

- `0055-F9` C-7; OQ-1 resolved.

## Reversal

None.
