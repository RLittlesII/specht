---
title: "Decision 0002: Id generation is 0001-F19, specified at src/specht/IdGeneration"
description: "The owner assigned id generation to epic 0001 as F19; spec-author places its one specification beside the engine folder that will hold the strategies and decorators, with the command a thin folder under src/tool/Features and the MCP tool a claim in 0132-F4"
type: decision
---

# Decision 0002: Id generation is `0001-F19`, specified at `src/specht/IdGeneration`

**Date:** 2026-11-06
**Decided by:** the repository owner, for the epic and the id; `spec-author`, for the folder

## The call

1. **`0001-F19`, in epic `0001`.** The owner's assignment, given in session on 2026-11-06 and relayed to `spec-author`. `F18` stays reserved for the printer `0132-F4` A-2 names; no specification, open pull request or `main` held `F19` when it was taken.
2. **One specification, at `src/specht/IdGeneration/.spec/`.** The engine folder `src/specht/IdGeneration/` holds the strategies, the decorators and the settings they read (C-9). The command is a folder under `src/tool/Features/`, its working name `Mint/` (OQ-8), which parses, calls the engine and prints; it carries no `.spec/` of its own.
3. **The MCP tool is `0132-F4` B-023**, not a claim here (§ 5 row 5).

## Why

- Most of this Feature's claims are about what a strategy mints and what a decorator refuses (B-005 to B-012); the command only prints the result. A specification sits beside the code it specifies (AGENTS.md § "Where a specification lives"), and that code is the engine's.
- [`dotnet-tool` § Vertical Slice](../../../../../.agents/skills/dotnet-tool/references/vertical-slice.md): a command parses, calls the engine and folds the result; a file one Feature owns goes in that Feature's engine folder.
- The MCP holds no engine (ADR-0031; `0132-F3`), so its tool is an answer of the Feature that serves the command line through MCP.

## Rejected

- **`src/tool/Features/Mint/.spec/`.** The command folder would hold a specification whose claims are almost all the engine's.
- **A Feature in epic `0132`.** The MCP holds no strategy (C-9).
- **Epic `0324`.** Its allocation needs an authority and durable state; this Feature is offline, needs no coordinator and keeps no state (C-1, C-7).
- **Claims in `0001-F5`.** It reads the manifest's `identifiers` grammars; minting ids is a capability of its own, with its own invariant, that validation reads those grammars for (C-4).
- **Claims in item `0256`.** A spike changes no shipped behaviour.

## Affects

- `0001-F19` frontmatter and C-9; § 5 row 5.
- Epic `0001`: `children` and § Features gain `0001-F19`.
- `0001-F5` and `0001-F7`: `blocks` gains `F19`. `0132-F4`: `depends_on` gains `0001/F19`.

## Reversal

None.
