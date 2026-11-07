---
title: "Decision 0001: The mint tool joins this Feature and runs the pinned mint command"
description: "On the owner's assignment of 2026-11-06, an MCP tool that mints an id is a claim of 0132-F4, B-023: it relays what the repository's pinned specht's mint command (0001-F19) prints and holds no generator; B-017 is superseded by B-024 so the tool list may include it"
type: decision
---

# Decision 0001: The mint tool joins this Feature and runs the pinned mint command

**Date:** 2026-11-06
**Decided by:** the repository owner, for the tool and its home; `spec-author`, for the claims

## The call

1. **The owner's selection**, in item `0256`'s summary of 2026-11-06: an id is minted through a `specht` CLI command and a `specht-mcp` tool. The owner's assignment, given in session the same day and relayed to `spec-author`: the tool is an amendment of this Feature, the command is `0001-F19`'s, and both only print the id.
2. **B-023.** Given a call for a new id of a type, the answer is the text the pinned `specht`'s mint command writes on stdout in the run that call makes.
3. **B-017 superseded by B-024.** B-017 limits the tools to the answers B-001 to B-009 name, which the mint tool is not; B-024 adds it. B-017's id is retired, not reworded.
4. **C-7 amended.** The mint tool is named for its command when `0001-F19` names it (`0001-F19` OQ-8); its working name is `mint`.

## Why

- ADR-0031: the MCP is its own program that runs the repository's pinned `specht` and relays what the command returns. A tool that minted in the host's process would hold an engine the MCP does not have (`0132-F3`; `0001-F19` C-9). ADR-0031's note that the MCP may one day keep and mint keys with state of its own is not taken up here.
- The mint writes nothing (`0001-F19` C-1), so B-016 and C-3 hold for it, and it is no write of the kind § 5 row 1 defers.
- Each run mints a new id, so the answer is held to the run its call makes, not to a second run compared with it (B-023, § 10).
- Superseding keeps B-017's text and its citations true for what it said; a reworded B-017 would make every past citation of it mean something else.

## Rejected

- **A Feature of its own in epic `0132`.** The owner assigned the tool to this Feature.
- **Rewording B-017 in place.** Call 3.
- **A generator in the host.** ADR-0031; `0001-F19` C-9.

## Affects

- B-017, B-023, B-024; C-7; A-5; § 2 row 5; § 5 rows 7 and 12; § 9 rows B-017, B-023 and B-024; § Tasks.
- `depends_on` gains `0001/F19`, and `0001-F19` declares `blocks` `0132/F4`.
- `spec_status` moves to `in-review` until § 12 is re-signed.

## Reversal

None.
