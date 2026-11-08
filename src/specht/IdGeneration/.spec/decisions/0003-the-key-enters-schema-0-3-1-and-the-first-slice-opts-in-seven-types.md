---
title: "Decision 0003: The mint key enters schema 0.3.1, and the first slice opts in seven types"
description: "The options the owner selected on 2026-11-07 for the two questions ADR-0033 left as alternatives - the mint key and four optional identifiers keys enter schema 0.3.1, and the first slice opts in adr, lesson, decision, claim, constraint, openQuestion and scenario, not epic, feature or task - and for OQ-11, kept open on its default"
type: decision
---

# Decision 0003: The `mint` key enters schema `0.3.1`, and the first slice opts in seven types

**Date:** 2026-11-07
**Decided by:** the repository owner, in session, by selecting options; recorded by `spec-author`

## The call

On 2026-11-07 the owner accepted [ADR-0033](../../../../../.spec/adr/0033-hand-cited-ids-are-minted-by-nano-id-and-snowflake-takes-random-fields.md) by selecting the option "Accept as proposed", and was then asked the two questions ADR-0033 left as alternatives and `0001-F19` OQ-11. The owner selected one option for each; no words of the owner are recorded beyond the option labels.

1. **ADR-0033 decision 4, the schema patch: "0.3.1".** The top-level `mint` key and the four optional `identifiers` keys, `adr`, `lesson`, `decision` and `scenario`, enter schema `0.3.1`, amended in place before any package carries it, not `0.3.2`. C-17; B-044, B-045.
2. **ADR-0033 decision 5, the first slice: "Seven, no epic/feature/task".** A manifest can opt in `adr`, `lesson`, `decision`, `claim`, `constraint`, `openQuestion` and `scenario`. `epic`, `feature` and `task` are left for a later slice, and no frontmatter schema id pattern changes. C-13; B-018; § 5 row 10.
3. **`0001-F19` OQ-11: "Keep open on default".** OQ-11 stays open and proceeds on C-10: a request that prints no id exits `2`, `3` or `4`, with the reason on standard error and no absolute path. B-002, B-012, B-017.

## Why

The reasons are ADR-0033's, not the owner's.

- `0.3.1`: an optional key is a patch (`0001-F7` decision 0022), and a version no published package carries is changed in place (`0001-F7` C-17; lesson 0016). Item `0388` cuts `0.3.1` and is `ready`, not delivered.
- Seven types: `epic`, `feature` and `task` also need the frontmatter schemas' id patterns widened and, for `task`, `ItemSequenceRule`'s literal (ADR-0033 finding 5); the seven need neither.

## Rejected

- **`0.3.2`.** Call 1.
- **`epic`, `feature` and `task` in the first slice.** Call 2.
- **One exit code per refusal now.** Call 3 keeps OQ-11 open.

## Affects

- `0001-F19` B-017, B-018, B-044, B-045; C-13, C-17; § 5 row 10; OQ-6, OQ-7, OQ-11.
- Item `0388`: its summary says `0.3.1` adds no key at the cut.

## Reversal

None.
