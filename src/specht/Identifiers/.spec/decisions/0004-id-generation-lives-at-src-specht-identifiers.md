---
title: "Decision 0004: Id generation lives at src/specht/Identifiers"
description: "On the owner's review of PR 789, 0001-F19's engine folder, its specification and its items move from src/specht/IdGeneration to src/specht/Identifiers, namespace Specht.Identifiers; supersedes the location half of decision 0002"
type: decision
---

# Decision 0004: Id generation lives at `src/specht/Identifiers`

**Date:** 2026-11-08
**Decided by:** Repository owner, in
[review 5819141960](https://github.com/RLittlesII/specht/pull/789#pullrequestreview-5819141960);
recorded by spec-author

## The call

The owner's words, verbatim: "I'd prefer the `IdGeneration` to be `Identifiers`".

The recorder reads those words as follows. The placement below is that
reading, not the owner's wording.

1. `0001-F19`'s engine folder is `src/specht/Identifiers/`, and its types are
   in the namespace `Specht.Identifiers`, as each engine folder names its
   namespace.
2. The specification moves with it, to `src/specht/Identifiers/.spec/`, and the
   items to `src/specht/Identifiers/.issue/`.
3. This supersedes the location in [decision 0002](0002-id-generation-is-0001-f19-at-src-specht-idgeneration.md)
   call 2. Its other calls stand: the id `0001-F19`, one specification beside
   the engine folder, the command a folder under `src/tool/Features/`, and the
   MCP tool as `0132-F4` B-023.
4. No claim, constraint, open question, scenario, id or title changes. The
   Feature is still "Id generation", and its `.feature` file keeps its name.

## Why

A specification sits beside the code it specifies (AGENTS.md § "Where a
specification lives"), so the folder the owner named for the code is the
folder the specification moves to.

## Rejected

- **Moving the code alone.** The specification would no longer sit beside the
  engine folder it specifies, which decision 0002 call 2 keeps.

## Affects

Decision 0002 call 2 (superseded); § 6 and § 7 paths; items `0412` to `0421`
`spec:`; epic `0001` § Features; ADR-0033 links to this specification.

## Reversal

None.
