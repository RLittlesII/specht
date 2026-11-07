---
title: "Specification: Id generation"
description: "A specht command mints a new id for a record type the manifest opts in, through a Nano ID or Snowflake strategy with its own settings, wrapped in formatting, a collision guard and validation against the type's identifiers grammar; it prints the id and writes nothing, and a type not opted in gets no id and keeps its own numbering"
type: feature
id: "F19"
epic: "0001"
spec_status: draft
status: blocked
priority: med
value: 0
risk: 0
rank: 0
scored_by: null
scored_on: null
domain: "Record identity"
author: "spec-author"
milestone: null
children: []
depends_on: ["F5", "F7"]
blocks: ["0132/F4"]
spikes: []
created: "2026-11-06"
updated: "2026-11-06"
github_issue: null
synced_at: null
---

# Specification: Id generation

## 1. Business Goal

<!-- last written by: spec-author, 2026-11-06 -->

An author on a branch takes the next free number from their own copy of the tree, and a branch cut from the same `main` takes the same one; git reports nothing, and lesson 0005 records decisions, claims, constraints, questions and work items claimed two and three times and one duplicate item id reaching `main`. This Feature removes that failure state for every record type a consumer opts in: the pinned `specht` mints a new id that no other branch, worktree or agent can take, and prints it, while every type the consumer does not opt in keeps its own numbering untouched.

## 2. User Needs

<!-- last written by: spec-author, 2026-11-06 -->

| #   | Persona                                                   | Need                                                                     | Pain Point Today                                                                    |
| --- | --------------------------------------------------------- | ------------------------------------------------------------------------ | ----------------------------------------------------------------------------------- |
| 1   | An author on a branch cut in parallel with others         | A new id for a record that no other branch can also take                 | Takes the next number by hand, collides, and renumbers before merging (lesson 0005) |
| 2   | An agent session in one of several worktrees on a machine | An id from the pinned tool rather than one read off its copy of the tree | Reads the highest id on its branch and takes the next, as every other session does  |
| 3   | A consumer with numbering of its own                      | Every type it does not opt in left exactly as it is                      | N/A (a contract this Feature must not break)                                        |
| 4   | `0132-F4`, reading through MCP                            | A command whose standard output is the minted id                         | N/A (internal dependency; no command mints an id)                                   |

### Assumptions

| ID  | Assumption                                                                                                                                     |
| --- | ---------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | "The id settings", "a type" and "a strategy's settings" in these claims are whatever OQ-6 and OQ-7 settle; no claim depends on their spelling. |
| A-2 | "A request to mint an id" is the command OQ-8 names; its working name is `specht mint <type> [--root <dir>]`.                                  |
| A-3 | Item `0256` stays open as a spike; its proposed ADR answers OQ-1 to OQ-10 and OQ-12, and no claim waits on its trial branches.                 |
| A-4 | "The tree holds an id of the type" (B-010) means wherever OQ-9 settles that the collision guard looks for that type.                           |
| A-5 | A "candidate" is the id a type's strategy produces with its formatting applied, before the collision guard and validation accept it.           |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-11-06 -->

| ID    | Claim                                                                                                                                                                                                  | Source                                                               | Status |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | -------------------------------------------------------------------- | ------ |
| B-001 | Given a request to mint an id of a type the manifest does not opt in, this Feature writes nothing on standard output.                                                                                  | the owner, 2026-11-06 (item `0256`); decision 0001; C-2              | Active |
| B-002 | Given a request to mint an id of a type the manifest does not opt in, this Feature exits `2`, `3` or `4`.                                                                                              | decision 0001; C-10; OQ-11                                           | Active |
| B-003 | Given a request to mint an id of a type the manifest opts in, this Feature writes exactly one line on standard output, holding the minted id and nothing else.                                         | the owner's selection, 2026-11-06 (item `0256`); decision 0001; C-1  | Active |
| B-004 | Given a request that prints a minted id, this Feature exits `0`.                                                                                                                                       | AGENTS.md § CLI; C-10                                                | Active |
| B-005 | Given a type opted in to Nano ID with a size, this Feature mints an id whose part after the type's prefix is exactly that many characters long.                                                        | the owner, 2026-11-06 (item `0256`); OQ-3                            | Active |
| B-006 | Given a type opted in to Nano ID with an alphabet, this Feature mints an id in which every character after the type's prefix is in that alphabet.                                                      | the owner, 2026-11-06 (item `0256`); OQ-3                            | Active |
| B-007 | Given a type opted in to Snowflake with an epoch, this Feature mints an id that, decoded under the type's encoding and field widths, holds the milliseconds from that epoch to the time it was minted. | the owner, 2026-10-23 and 2026-11-06 (item `0256`); OQ-1, OQ-2, OQ-4 | Active |
| B-008 | Given two types opted in with different strategy settings, this Feature mints each type's id with that type's own settings.                                                                            | the owner, 2026-11-06 (item `0256`); C-3                             | Active |
| B-009 | Given a type whose formatting names a prefix, this Feature mints an id that starts with that prefix.                                                                                                   | the owner's selection, 2026-11-06 (item `0256`); OQ-4                | Active |
| B-010 | Given a tree that already holds, as an id of the type, the first candidate the type's strategy produces, this Feature prints an id the tree does not hold.                                             | the owner's selection, 2026-11-06 (item `0256`); A-4; OQ-9           | Active |
| B-011 | Given a candidate that the manifest's `identifiers` grammar for its type does not match, this Feature writes nothing on standard output.                                                               | the owner's selection, 2026-11-06 (item `0256`); C-4; OQ-10          | Active |
| B-012 | Given a candidate that the manifest's `identifiers` grammar for its type does not match, this Feature exits `2`, `3` or `4`.                                                                           | C-10; OQ-11                                                          | Active |
| B-013 | Given any request to mint an id, this Feature creates, modifies and deletes no file under the root or elsewhere.                                                                                       | the owner, 2026-11-06 (decision 0001); C-1; C-11                     | Active |
| B-014 | Given a valid manifest that opts types in, this Feature leaves the check reporting the violations the same tree gets with the id settings removed.                                                     | the owner, 2026-11-06 (item `0256`); C-6; C-8                        | Active |
| B-015 | Given a manifest whose id settings its pinned schema version does not accept, this Feature exits `3` on a request to mint an id.                                                                       | AGENTS.md § CLI; `0001-F5` B-012; C-6; OQ-7                          | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-11-06 -->

| ID   | Constraint                                                                                                                                                                                                          | Rules Out                                                                                                                                                                                         |
| ---- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| C-1  | Print only: a request writes the minted id on standard output and nothing anywhere else (the owner, 2026-11-06; decision 0001).                                                                                     | Writing the id into a document, a file name or `.issue/.sequence`; any state file, a Snowflake last-time or sequence file included; a new entry in the write invariant of AGENTS.md § Invariants. |
| C-2  | Opt in means opt in: a type is minted for only when the manifest names it with a strategy (the owner, 2026-11-06; item `0256`).                                                                                     | A default strategy for a type the manifest does not name; a sequence default; a fallback to the sequence when a type is absent or its strategy cannot mint.                                       |
| C-3  | Two built-in strategies, Nano ID and Snowflake, and each reads only its own settings (the owner, 2026-11-06; item `0256`).                                                                                          | A consumer-supplied strategy (§ 5 row 1); a sequence strategy; one strategy reading another's settings; a third strategy without a specification delta.                                           |
| C-4  | Formatting, the collision guard and validation are the decorators around a strategy, and validation reads a type's grammar only from the manifest's `identifiers` (the owner's selection, 2026-11-06).              | A second grammar for a type held in the id settings or in code; a decorator that rewrites or renumbers an id already in the tree.                                                                 |
| C-5  | The id settings are one top-level manifest key, which the checker owns ([ADR-0023](../../../../.spec/adr/0023-codeanalysis-is-the-only-csharp-specific-manifest-section.md)).                                       | Settings under `codeAnalysis`; a configuration file beside the manifest; settings spread over several top-level keys.                                                                             |
| C-6  | The key is an opt-in addition and enters a schema patch (`0001-F7` C-11 and decision 0022).                                                                                                                         | A `0.minor` step for it; a required key; a rule id or a verdict reached by a tree whose manifest does not set it.                                                                                 |
| C-7  | Minting is offline and needs no coordinator (AGENTS.md § Invariants; [ADR-0028](../../../../.spec/adr/0028-files-stay-the-reading-path-and-online-allocation-waits.md)).                                            | A network call; a lock, allocation or reservation service; a call to GitHub; epic `0324`'s allocation authority.                                                                                  |
| C-8  | The check never mints, and stays deterministic (AGENTS.md § Invariants).                                                                                                                                            | The check calling a strategy or a decorator; a verdict that depends on the clock or on a random source.                                                                                           |
| C-9  | The strategies and decorators live in the engine; the command and the MCP tool hold none ([ADR-0031](../../../../.spec/adr/0031-the-mcp-is-its-own-program-that-runs-the-repositorys-pinned-specht.md); `0132-F3`). | A strategy or decorator in the command's folder or in `src/mcp`; the MCP minting in its own process.                                                                                              |
| C-10 | A request that prints no id exits with a failure code the check already gives, `2`, `3` or `4` (AGENTS.md § CLI).                                                                                                   | Exit `0` or `1` with nothing printed, which `0132-F4` B-020 reads as a tool no longer restored; a new exit code.                                                                                  |
| C-11 | An id already in the tree is never renumbered or rewritten (AGENTS.md § Stable IDs).                                                                                                                                | `init`, `upgrade` or `format` rewriting ids under a strategy; a migration of existing sequence ids to minted ones.                                                                                |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-11-06 -->

| #   | Item                                                                                                                | Exclusion Reason                                                                          |
| --- | ------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------- |
| 1   | A consumer-supplied, pluggable strategy                                                                             | Kept open as a future option (the owner, 2026-11-06; item `0256`); C-3.                   |
| 2   | A sequence strategy, a sequence default or a fallback to the sequence                                               | This replaces the sequence (the owner, 2026-11-06; item `0256`); C-2.                     |
| 3   | Writing a minted id into a document, a file name or a counter                                                       | Print only (the owner, 2026-11-06; decision 0001); C-1.                                   |
| 4   | Renumbering or rewriting an id already in the tree, under any strategy                                              | AGENTS.md § Stable IDs; C-11.                                                             |
| 5   | The MCP tool that mints an id                                                                                       | `0132-F4` B-023; C-9.                                                                     |
| 6   | A check that fails on an id claimed twice across `main` and open pull requests, or any rule over minted ids         | Item `0128`; B-014.                                                                       |
| 7   | Allocation from a shared sequence, online or across machines                                                        | Epic `0324`; ADR-0028; C-7.                                                               |
| 8   | Moving this repository's own records - the queue script, `.issue/.sequence`, ADR and lesson numbers - to minted ids | A follow-up item after the owner rules on item `0256`'s ADR (item `0256` § out of scope). |
| 9   | A key a record keeps for its whole lifecycle in a database                                                          | Epic `0324` § "Service mode".                                                             |

## 6. Concern Separation

Pending implementer authorship after agreement.

## 7. Technical Design

Pending implementer authorship after agreement.

## 8. Testing Strategy

Pending test-writer authorship after agreement.

## 9. Traceability Matrix

<!-- last written by: spec-author, 2026-11-06 (rows added, Missing until bound) -->

| Claim ID | Scenario                                           | Test    | Status  |
| -------- | -------------------------------------------------- | ------- | ------- |
| B-001    | A type not opted in gets no id                     | Missing | Missing |
| B-002    | A type not opted in is refused                     | Missing | Missing |
| B-003    | A minted id is the whole output                    | Missing | Missing |
| B-004    | A printed id ends in success                       | Missing | Missing |
| B-005    | A Nano ID has the configured size                  | Missing | Missing |
| B-006    | A Nano ID draws only from the configured alphabet  | Missing | Missing |
| B-007    | A Snowflake id carries the time since its epoch    | Missing | Missing |
| B-008    | Each type follows its own settings                 | Missing | Missing |
| B-009    | A minted id starts with its type's prefix          | Missing | Missing |
| B-010    | An id the tree already holds is never handed out   | Missing | Missing |
| B-011    | An id its type's grammar rejects is not printed    | Missing | Missing |
| B-012    | An id its type's grammar rejects is refused        | Missing | Missing |
| B-013    | A request writes nothing                           | Missing | Missing |
| B-014    | Opting types in changes no verdict                 | Missing | Missing |
| B-015    | Id settings the pinned version rejects are invalid | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-11-06 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-11-06 -->

| ID    | Question                                                                                                                                                                                                                                             | Blocks                       | Resolution                                                                                                                                                                                                   |
| ----- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| OQ-1  | How is a Snowflake datacenter and worker id assigned with no coordinator, across clones, worktrees and agents on one machine, and what collides when two of them share one?                                                                          | B-007; a claim on worker ids | Open. Item `0256` answers it (question 1, criterion 2). No claim states how a worker id is assigned until then.                                                                                              |
| OQ-2  | What does Snowflake do when the clock reads earlier than an id it minted before - within a run, and across runs, given that C-1 keeps no state between them?                                                                                         | B-007                        | Open. Item `0256` answers it (question 1, criterion 2).                                                                                                                                                      |
| OQ-3  | What are Nano ID's default alphabet and size, or must a type name both, and what is the collision probability at that size against the expected id volume?                                                                                           | B-005, B-006                 | Open. Item `0256` answers it (its Nano ID criterion, with the arithmetic). B-005 and B-006 hold for the settings a type names.                                                                               |
| OQ-4  | Which encodings does formatting offer - decimal, base32 or another - and does a shorter one keep the collision property where ids are cited by hand, in file names, branch names and commit subjects?                                                | B-007, B-009                 | Open. Item `0256` answers it (question 2, criterion 3).                                                                                                                                                      |
| OQ-5  | What is a scenario id: where is it written, where does the collision guard look for one, and how does it relate to epic `0324`'s scenario key?                                                                                                       | B-010, for scenarios         | Open. Item `0256` answers it (its design, opt-in per id type; epic `0324` § "Service mode").                                                                                                                 |
| OQ-6  | Which id types can be opted in, and what is each one's name in the key? Is a work item one, when the check reads only a consumer's task files?                                                                                                       | B-001 to B-015               | Open. Item `0256` answers it (question 6). Proceeds on item `0256`'s list: items and epics, Features, ADRs, lessons, claims, constraints, open questions, decisions and scenarios.                           |
| OQ-7  | What are the key's name and shape, and which patch carries it: `0.3.1` while it is open beside `0001-F16`'s item `0388`, or the next patch?                                                                                                          | B-014, B-015; C-5, C-6       | Open. Item `0256` answers it (its ADR names the manifest key and the schema version it needs).                                                                                                               |
| OQ-8  | What is the command's name, which arguments does it take, and does one request mint one id or several?                                                                                                                                               | B-003                        | Open. Item `0256` answers it (its ADR names the command and MCP tool shape). Proceeds on the working name `specht mint <type> [--root <dir>]`, one id per request; `0132-F4` C-7 names the MCP tool from it. |
| OQ-9  | Where does the collision guard look for each type's existing ids, how many candidates does it try, and what does a request do when every one is held?                                                                                                | B-010                        | Open. Item `0256` answers it (criterion 1 runs the guard per id type).                                                                                                                                       |
| OQ-10 | What does validation check for a type the manifest's `identifiers` holds no grammar for - ADRs, lessons, decisions, scenarios - and must a type opted in have a grammar that admits its ids, since the shipped claim grammar rejects a Snowflake id? | B-011, B-012                 | Open. Item `0256` answers it (question 4).                                                                                                                                                                   |
| OQ-11 | Which of `2`, `3` and `4` does each refusal exit with - a type not opted in, a candidate its grammar rejects, a guard that finds no free id - and what does each write on standard error?                                                            | B-002, B-012                 | Open. Owner not asked. Proceeds on C-10: one of `2`, `3` and `4`, with the reason on standard error and no absolute path.                                                                                    |
| OQ-12 | Does any shipped default grammar in `identifiers` change, or does a consumer that opts a type in widen its own?                                                                                                                                      | B-011                        | Open. Item `0256` answers it (question 4). Proceeds on no shipped grammar changing.                                                                                                                          |

## 12. Sign-off

| Section | Status | Reviewer      | Note                     |
| ------- | ------ | ------------- | ------------------------ |
| 1-5     | 🟡     | spec-reviewer | Agreement review pending |

## Tasks

None. Work items are cut from the claims after agreement.

## Scoring

| Field | Value | Basis                       |
| ----- | ----- | --------------------------- |
| value | 0     | Not scored before agreement |
| risk  | 0     | Not scored before agreement |
