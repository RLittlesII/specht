---
title: "Decision 0001: Ids are minted by a strategy with decorators, opted in per type, and only printed"
description: "The owner's design of 2026-11-06 for collision-free ids: Nano ID and Snowflake strategies with their own settings, formatting, a collision guard and validation around them, opt-in per id type with no default and no sequence fallback, a specht command and a specht-mcp tool, and print only"
type: decision
---

# Decision 0001: Ids are minted by a strategy with decorators, opted in per type, and only printed

**Date:** 2026-11-06
**Decided by:** the repository owner, in session; recorded by `spec-author`

## The call

The owner's words and selections are quoted from item `0256`'s summary, its 2026-11-06 paragraphs. Calls 6 and 7 were given in session on 2026-11-06 and relayed to `spec-author`; no words of the owner are recorded for them.

1. **A strategy with decorators.** The owner: "I want this done as a Strategy with a Decorator. I want to support nanoid AND Snowflake and allow them both their own configuration for the bits they need to configure." C-3, B-005 to B-008.
2. **Opt in per id type.** The owner: "I want it to have an option to opt in for each id type (spec, adr, lessons, claims, scenarios, etc.)" C-2, OQ-6.
3. **No default for a type not opted in.** The owner: "If they bring their own strategy, we want them to keep it. Opt in means opt in, not opt in AND get a default ..." and, on what their own strategy means, "Existing numbering, keep pluggable strategy open for a future option." B-001, B-002, C-2, C-11; § 5 row 1.
4. **No sequence fallback.** The owner: "We are creating this to replace the current system because it's flawed." The fallback decorator the owner first selected was withdrawn by this answer. C-2; § 5 row 2.
5. **The decorators and the surface, selected by the owner.** Formatting, collision guard and validation are the decorators (B-009 to B-012, C-4); an id is minted through a `specht` command and a `specht-mcp` tool (B-003; `0132-F4` B-023).
6. **The Feature is specified now.** It is `0001-F19`, authored before item `0256`'s ADR; the spike stays open and answers this Feature's open questions (A-3). This reverses the order in item `0256`'s summary, where a Feature followed the owner's ruling on the ADR.
7. **Print only.** The command and the MCP tool return the minted id and write no file; the write invariant of AGENTS.md § Invariants does not change. C-1, B-013.

## Why

The owner's reason for the design is in call 4: the sequence collides across branches (lesson 0005). The reasons below are `spec-author`'s reading, not the owner's.

- Print only keeps every write a consumer's tree can receive on the list AGENTS.md § Invariants names, so the author puts the id where it belongs, as today.
- Print only also means no state survives between two requests, so a Snowflake strategy cannot remember the last time it minted. That is why OQ-1 and OQ-2 are open, not a reason to keep state.
- A request that prints no id exits `2`, `3` or `4` (C-10), because `0132-F4` B-020 reads exit `0` or `1` with nothing printed as a tool that is no longer restored.

## Rejected

- **A sequence strategy as a default or a fallback.** Calls 3 and 4.
- **A consumer-supplied strategy now.** Call 3 keeps it open for later; § 5 row 1.
- **Writing the minted id into the tree.** Call 7.
- **Waiting for item `0256`'s ADR before writing the agreement.** Call 6.

## Affects

- `0001-F19` B-001 to B-015, C-1 to C-4, C-10, C-11; § 5 rows 1 to 3; A-3.
- `0132-F4` B-023 (its decision 0001).
- Item `0256`: a decision line naming the open questions it answers.

## Reversal

None.
