---
title: "Decision 0001: the specifications sit in src/mcp, and the Reading Feature beside its slice"
description: "0132-F3's specification and items moved from src/tool/Features/Mcp to src/mcp with the project's first items, and 0132-F4's to src/mcp/Features/Reading, where its code is; no claim, constraint or item id changed"
type: decision
---

# Decision 0001: the specifications sit in src/mcp, and the Reading Feature beside its slice

**Date:** 2026-11-04
**Decided by:** spec-author, carrying out OQ-2's recorded call (co-location,
decided by the repository owner, 2026-11-04); the `0132-F4` placement is
spec-author's, under the same rule

## The call

1. `0132-F3`'s `.spec/` and `.issue/` are at `src/mcp/`, beside `mcp.csproj`.
2. `0132-F4`'s `.spec/` and `.issue/` are at `src/mcp/Features/Reading/`, beside
   the `Reading` slice that holds its code (`0132-F3` § 7).
3. The move is `git mv`; every claim, constraint, open question and item id is
   unchanged. The acceptance project links the two `.feature` files from the new
   paths.
4. Accepted ADRs and recorded review rows that cite the old path are history and
   are not edited.

## Why

A specification lives beside the code it specifies. The project exists, and the
Reading slice is a folder of that project, as `specht rules set` sits beneath
`specht rules`.

## Rejected

- `src/mcp/Features/Mcp/Reading/`: no such folder holds code; the slice is
  `src/mcp/Features/Reading/`.
- Leaving `0132-F4` under `src/tool/Features/Mcp/Reading/`: the path would name
  a project that holds neither the Feature nor its code.

## Affects

OQ-2, § Tasks, epic `0132` § Features and § Placement, items `0329`, `0332`,
`0361` (`0132-F3`) and `0330`, `0331` (`0132-F4`), `test/acceptance`.

## Reversal

None.
