---
title: "Decision 0001: the set command changes one rule's level and nothing else"
description: "A command sets one rule id to error, warning or off in the manifest's rules key, changing only that entry, as the pin command changes only schemaVersion; it is its own Feature and a new writer into the consumer's tree"
type: decision
---

# Decision 0001: the set command changes one rule's level and nothing else

**Date:** 2026-10-11
**Decided by:** the repository owner, answering the third of three questions on the need ("Maybe even an edit function?"); recorded by `spec-author`

## The call

1. **Rule level only.** The command sets one rule id to `error`, `warning` or
   `off`. It edits no other manifest key.
2. **Only that entry changes.** In the manifest only the named rule's entry
   under `rules` changes, as the pin command changes only `schemaVersion`
   (`0001-F7` decision 0005, C-13). No other file under the root is touched.
3. **Its own Feature, and a new writer.** It is `0001-F11`, and it joins
   `init`, `upgrade`, the pin command, the `--report` file and `format` as a
   writer into the consumer's tree (brief § 9; AGENTS.md § Invariants).

## Why

- The owner's "edit function" was a question. A general editor of the
  manifest would have to know every key's shape and re-validate the whole
  file; the level is the one value a consumer changes repeatedly, lowering a
  rule on an old tree and raising it as the tree is repaired (`0001-F5`
  need 2).
- `init` now writes a level for every rule (`0001-F4` decision 0002), so the
  entry to change always exists in a manifest `init` wrote.
- A write that is refused for an id or a level the pinned version lacks
  keeps the manifest one the check accepts (`0001-F5` B-013, B-045).

## Rejected

**A general `set <key> <value>` over the manifest.** Cost of rejecting: every
other key stays a hand edit.

**An interactive editor.** Cost of rejecting: none recorded; an agent and a
script cannot drive one.

**No writer: `specht rules` and `specht manifest` only.** It keeps the list
of writers as it was. Cost of rejecting: one more command that writes into a
consumer's tree, with its own never-writes-elsewhere claims (B-003 to B-006).

**The set command inside `0001-F10`.** The invariant changes from "writes
nothing" to "writes one entry", and a Feature is cut where the invariant
changes (AGENTS.md § "Feature naming heuristic"). Cost of rejecting: a third
specification.

## Affects

- `0001-F11`: every claim and constraint.
- brief § 9 and AGENTS.md § Invariants: the set command joins the writers.
- `0001-F8` § 5 row 3, which lists the writers: amended.
- Not decided here: the command's name (OQ-1); the exit code for a level that
  is not one of the three, which B-008 derives from `0001-F2` decision 0003;
  removing an entry (OQ-5).

## Reversal

None.
