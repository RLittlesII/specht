---
title: "Specification: Reading through MCP"
description: "Through the 0132-F3 host, an agent asks for the check's verdict on the whole tree, a rule's text, the rule list, the manifest, the tree's facts, claim coverage, the shipped schema versions and the frontmatter schemas and templates; each answer is byte for byte what its command prints on the same tree and pin, read at the call, and nothing is written"
type: feature
id: "F4"
epic: "0132"
spec_status: approved
status: ready
priority: med
value: 0
risk: 0
rank: 0
scored_by: null
scored_on: null
domain: "Specification queries"
author: "spec-author"
milestone: null
children: []
depends_on:
  ["F2", "F3", "0001/F2", "0001/F3", "0001/F9", "0001/F10", "0001/F13"]
blocks: []
spikes: []
created: "2026-10-31"
updated: "2026-11-01"
github_issue: null
synced_at: null
---

# Specification: Reading through MCP

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-31 -->

An agent that asks about a repository through MCP has no `specht` answer to receive: it runs the command and scrapes the output, follows a copied skill that has drifted from the pinned schema version, or answers from memory, and a guessed rule surfaces at the build or in review. This Feature removes that failure state: through `0132-F3`'s host an agent asks for the check's verdict on the whole tree, what a rule requires, what the pinned schema version demands and what the tree holds, and each answer is the command line's own answer on the same tree and pin, so MCP and the command can never disagree and nothing is written.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-31 -->

| #   | Persona                                 | Need                                                                                  | Pain Point Today                                              |
| --- | --------------------------------------- | ------------------------------------------------------------------------------------- | ------------------------------------------------------------- |
| 1   | A coding agent in a consumer repository | The verdict, the rules and what the pin demands, as data, with no `specht` skill      | Copies skills that drift from the pin, or answers from memory |
| 2   | A role agent in this repository         | The same answers on this repository's own tree                                        | Runs the command and scrapes its output                       |
| 3   | A person in an editor or a chat         | Ask an assistant about the tree with no terminal session (OQ-3)                       | Needs a terminal, or an assistant that guesses                |
| 4   | `0132-F1`, the persistent index         | Answers whose shape it can serve from its index and hold to the command's (its B-006) | N/A (internal dependency; no MCP answer exists)               |

### Assumptions

| ID  | Assumption                                                                                                                                                                                                                                                                  |
| --- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | Every answer is a command that exists or is specified: the verdict `0001-F2` and `0001-F3`, a rule's text `0001-F3`, the rule list `0001-F10`, the manifest `0001-F9`, the tree's facts `0132-F2`, claim coverage `0001-F13` (epic `0132` § "The command line MCP serves"). |
| A-2 | The shipped schema versions as JSON and the printer for the frontmatter schemas and the templates are epic `0001`'s Feature `0001-F18`, reserved and not yet specified (owner, 2026-10-30). B-008 and B-009 cite it; `depends_on` gains it when its specification exists.   |
| A-3 | The host, its transport, its root and its version are `0132-F3`'s; this Feature registers answers with it.                                                                                                                                                                  |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-31 -->

| ID    | Claim                                                                                                                                                                                                                        | Source                                                                                        | Status |
| ----- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------- | ------ |
| B-001 | Given a call for the verdict, this Feature answers with text byte-identical to what `specht --json` writes on stdout for the same root.                                                                                      | epic `0132` third and fifth Must criteria; `0001-F3` B-001                                    | Active |
| B-002 | Given the verdict's call, this Feature offers no argument that names a specification or a path.                                                                                                                              | the owner, 2026-10-30 (epic `0132` § Features, candidate C); C-5                              | Active |
| B-003 | Given a call for the text of a rule id the pinned version has, this Feature answers with text byte-identical to what `specht --explain <id>` writes on stdout.                                                               | epic `0132` first Must criterion; `0001-F3` B-019                                             | Active |
| B-004 | Given a call for the rule list, this Feature answers with text byte-identical to what `specht rules --json` writes on stdout for the same root.                                                                              | epic `0132` first Must criterion; `0001-F10`                                                  | Active |
| B-005 | Given a call for the manifest, this Feature answers with text byte-identical to what `specht manifest --json` writes on stdout for the same root.                                                                            | epic `0132` first Must criterion; `0001-F9`                                                   | Active |
| B-006 | Given a call for the tree's facts, this Feature answers with text byte-identical to what `specht facts --json` writes on stdout for the same root.                                                                           | epic `0132` second Must criterion; `0132-F2`, working name                                    | Active |
| B-007 | Given a call for claim coverage, this Feature answers with text byte-identical to what `specht trace --json` writes on stdout for the same root.                                                                             | epic `0132` second Must criterion; the owner, 2026-10-30; `0001-F13`                          | Active |
| B-008 | Given a call for the shipped schema versions, this Feature answers with text byte-identical to what `specht schema list --json` writes on stdout for the same root.                                                          | the owner, 2026-10-30 (epic `0132` § Open questions); `0001-F18`, reserved; A-2               | Active |
| B-009 | Given a call for the frontmatter schemas and the templates of the pinned version, this Feature answers with text byte-identical to what `0001-F18`'s printer writes on stdout for the same root.                             | epic `0132` first Must criterion; the owner, 2026-10-30; `0001-F18`, reserved; A-2            | Active |
| B-010 | Given a call carrying an argument its command takes, other than `--root`, `--report` and `--json`, this Feature answers with what the command writes on stdout given that argument.                                          | spec-author's reading of epic `0132` § Constraints; C-1, C-3                                  | Active |
| B-011 | Given a call for the verdict on a tree the check reports violations for, this Feature's answer is not marked as an error.                                                                                                    | spec-author's reading; OQ-5                                                                   | Active |
| B-012 | Given a call whose command exits `2`, `3` or `4`, this Feature's answer is marked as an error and its text is the message the command writes on stderr.                                                                      | `0001-F2` B-005 to B-007 and decision 0003; `0001-F3` B-020; OQ-5                             | Active |
| B-013 | Given the same tree pinned to two shipped schema versions, each answer under each pin is byte-identical to its command's stdout under that pin.                                                                              | epic `0132` fourth Must criterion ("proven by a test across two schema versions")             | Active |
| B-014 | Given the pin moved between two calls in one session, the second answer is its command's stdout under the new pin.                                                                                                           | epic `0132` fourth Must criterion; C-4                                                        | Active |
| B-015 | Given a specification edited between two calls in one session, the second answer is its command's stdout on the edited tree.                                                                                                 | epic `0132` fifth Must criterion; `0132-F1` decision 0001; C-4                                | Active |
| B-016 | Given any call this Feature answers, no file under the root or elsewhere is created, modified or deleted.                                                                                                                    | brief § 9; epic `0132` § Constraints                                                          | Active |
| B-017 | Given the list of tools the host offers, each one this Feature registers is an answer B-001 to B-009 names.                                                                                                                  | epic `0132` § Constraints and candidate C; candidates D and E deferred and parked, 2026-10-30 | Active |
| B-018 | Given a repository with the manifest `specht init` writes and no specification, a Feature specification and its scenarios built only from this Feature's answers have no violation in the verdict this Feature answers with. | epic `0132` sixth Must criterion; OQ-2                                                        | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-31 -->

| ID  | Constraint                                                                                                                                                  | Rules Out                                                                                                                               |
| --- | ----------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------- |
| C-1 | Every answer is its command's stdout on the same root and pin; MCP adds nothing the command line cannot say (owner, 2026-10-09; epic `0132` § Constraints). | An answer, a member or a field available only through MCP; reformatting, summarising, merging or annotating a command's output.         |
| C-2 | Every answer comes from the installation the host runs from (`0132-F3` C-2; ADR-0015, option 3 rejected).                                                   | Running a `specht` the path or another tool manifest resolves; an answer computed by a second parse that the command does not make.     |
| C-3 | Read-only and offline, with every path relative to the root (brief § 9; AGENTS.md § Invariants).                                                            | Serving `init`, `upgrade`, the pin and set commands, `format` or `--report`; a network call; an absolute path in an answer or an error. |
| C-4 | Each answer is for the tree and the pin as they are when the call is made (`0132-F1` decision 0001; epic `0132` § Success criteria).                        | An answer computed for an earlier state of the tree or an earlier pin, a cached one included.                                           |
| C-5 | The verdict is for the whole tree (owner, 2026-10-30).                                                                                                      | A filter naming a specification, a path or a rule; scoping the verdict's output, which is epic `0228` candidate E's.                    |
| C-6 | The rule vocabulary is fixed per schema version (AGENTS.md § Invariants; `0001-F7` C-11).                                                                   | A rule, or a rule's text, that MCP knows and the pinned schema version does not.                                                        |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-31 -->

| #   | Item                                                                                  | Exclusion Reason                                                                                                        |
| --- | ------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------- |
| 1   | Writes through MCP: `init`, `upgrade`, the pin and set commands, `format`, `--report` | Epic `0132` candidate D, deferred, 2026-10-30; C-3; B-017.                                                              |
| 2   | The work queue                                                                        | Epic `0132` candidate E, parked, 2026-10-30; B-017.                                                                     |
| 3   | A check of one specification, or a verdict scoped to named files                      | The owner, 2026-10-30: none; scoping the output is epic `0228` candidate E; B-002, C-5.                                 |
| 4   | Authoring guidance, method skills and role contracts                                  | Epic `0132` § Out of this epic: `specht` serves facts and does not distribute skills.                                   |
| 5   | Feedback while a document is being typed                                              | Epic `0132` § Out of this epic: the verdict on request is enough.                                                       |
| 6   | The host: its transport, its root, its lifecycle and its version                      | `0132-F3`.                                                                                                              |
| 7   | The answers' content and each command that gives it                                   | `0001-F2`, `0001-F3`, `0001-F9`, `0001-F10`, `0001-F13`, `0132-F2` and `0001-F18`; this Feature only serves them (C-1). |
| 8   | A form shaped for a person, such as a command's text output instead of its JSON       | Not decided (OQ-3); B-001 to B-009 proceed on the default.                                                              |
| 9   | Protocol prompts, resources or any feature other than tools                           | Not decided (OQ-4); B-017 proceeds on the default.                                                                      |
| 10  | Serving an answer from a persistent index                                             | `0132-F1`, held to these answers by its B-006.                                                                          |

## 6. Concern Separation

<!-- last written by: spec-author, 2026-10-31 -->

Pending: owned by `implementer`, written after agreement.

## 7. Technical Design

<!-- last written by: spec-author, 2026-10-31 -->

Pending: owned by `implementer`, written after agreement.

## 8. Testing Strategy

<!-- last written by: spec-author, 2026-10-31 -->

Pending: owned by `test-writer`, written after agreement.

## 9. Traceability Matrix

<!-- last written by: spec-author, 2026-10-31 -->

| Claim ID | Scenario                                                            | Test    | Status  |
| -------- | ------------------------------------------------------------------- | ------- | ------- |
| B-001    | The verdict is the check's JSON report                              | Missing | Missing |
| B-002    | The verdict is for the whole tree only                              | Missing | Missing |
| B-003    | A rule's text is what explaining it prints                          | Missing | Missing |
| B-004    | The rule list is what listing the rules prints                      | Missing | Missing |
| B-005    | The manifest is what printing the manifest prints                   | Missing | Missing |
| B-006    | The tree's facts are what the facts command prints                  | Missing | Missing |
| B-007    | Claim coverage is what the trace prints                             | Missing | Missing |
| B-008    | The shipped schema versions are what listing them prints            | Missing | Missing |
| B-009    | The frontmatter schemas and templates are what printing them prints | Missing | Missing |
| B-010    | An argument the command takes is passed to it                       | Missing | Missing |
| B-011    | A tree with violations is an answer, not an error                   | Missing | Missing |
| B-012    | A failure the command reports is an error answer                    | Missing | Missing |
| B-013    | Every answer is for the version the repository pins                 | Missing | Missing |
| B-014    | Moving the pin between two calls changes the second answer          | Missing | Missing |
| B-015    | An edit between two calls is in the second answer                   | Missing | Missing |
| B-016    | Every answer writes nothing                                         | Missing | Missing |
| B-017    | The server offers only answers the command line gives               | Missing | Missing |
| B-018    | A Feature specification built from the answers alone passes         | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-31 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-31 -->

| ID   | Question                                                                                                                                                                                                                                                                                                                                                                      | Blocks                         | Resolution                                                                                                                                                                                                                                                           |
| ---- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | Which MCP clients must the epic be proven against?                                                                                                                                                                                                                                                                                                                            | B-001 to B-018; `0132-F3` OQ-5 | Open. Travels here from epic `0132` § Open questions (owner, 2026-10-30). Proceeds on the protocol alone: tests drive the host through a protocol client over its standard input and output, and no named client is tested until the owner names one.                |
| OQ-2 | How is the sixth Must criterion proven - an agent with no `specht`-specific skill authoring a Feature specification that passes the check - and what is a fresh repository? Options: a deterministic test that builds a specification from the answers alone; a recorded run of an agent outside the build, since a model call is neither offline nor deterministic; or both. | B-018                          | Open. Travels here from epic `0132` § Open questions (owner, 2026-10-30). Proceeds on the deterministic test alone, with a fresh repository being one whose local tool manifest lists `specht.tool`, holding the manifest `specht init` writes and no specification. |
| OQ-3 | Does a person in an editor need anything an answer shaped for an agent lacks, such as a command's text output instead of its JSON document?                                                                                                                                                                                                                                   | B-001 to B-009; § 5 row 8      | Open. Deferred here by the owner, 2026-10-30 (epic `0132` § Open questions). Proceeds on no: every answer is its command's JSON form, or the rule page for a rule's text, and a person's assistant reads it.                                                         |
| OQ-4 | Which protocol features carry the answers: tools, resources, prompts, or more than one?                                                                                                                                                                                                                                                                                       | B-017; § 5 row 9               | Open. Proceeds on tools only, since each answer is computed when it is asked for. Owner not asked.                                                                                                                                                                   |
| OQ-5 | How does an answer carry its command's exit code?                                                                                                                                                                                                                                                                                                                             | B-011, B-012                   | Open. Proceeds on: an answer is marked as an error exactly when its command exits `2`, `3` or `4`; a verdict's pass or fail is read from the report's counts, and no code is carried. Owner not asked.                                                               |
| OQ-6 | Does an answer also carry its command's JSON document as structured content, declared by the command's published schema?                                                                                                                                                                                                                                                      | B-001 to B-009                 | Open. Proceeds on text only, byte-identical to the command's stdout. Owner not asked.                                                                                                                                                                                |
| OQ-7 | What are the tools' names?                                                                                                                                                                                                                                                                                                                                                    | Implementation                 | Open. Proceeds on one tool per answer, named after the command it serves, recorded as working names. Owner not asked.                                                                                                                                                |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-11-01 (round 2; round 1 moved to Review history) -->

| Section   | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |
| --------- | ------ | ------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-5, 9-11 | 🟢     | spec-reviewer | Round 2 (2026-11-01) - approved, reviewing the uncommitted working tree on `a189f53d`. Both round-1 findings are closed: the B-010 Outline no longer asks for the tree's facts, and the B-015 scenario asks for the verdict before and after the Background's violation is fixed, so no claim epic `0132` § Status ships in milestone 0.5.0 (B-001 to B-005, B-010 to B-017) waits on `0132-F2`, `0001-F13` or `0001-F18`. § 9 has one row per claim and every tag resolves; the `@boundary` scenarios match § 5 rows 1 to 3; B-012's exit codes agree with `0001-F3` B-020, B-029, B-030, `0001-F10` B-013, B-014 and `0001-F2`; B-013's pins `0.1.0` and `0.2.1` both ship; A-2 records the `0001-F18` edge B-008 and B-009 wait on; OQ-1 to OQ-7 stay open on recorded defaults, which do not block agreement. §§ 6-8 are pending after agreement and are reviewed when written; § 9 `Missing` rows block items, not this agreement. |

### Review history

| Section   | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |
| --------- | ------ | ------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-5, 9-11 | 🟡     | spec-reviewer | Round 1 (2026-11-01) - draft, reviewing the uncommitted working tree on `a189f53d`. Two findings, both routed to `spec-author`, both binding a claim epic `0132` § Status ships in milestone 0.5.0 to the tree's facts (B-006, `0132-F2`), which it defers: the B-010 Outline's first Examples row asks for the tree's facts with a specification's path, and the B-015 scenario edits a claim and asks for the tree's facts twice. Rewrite each against an answer B-001 to B-005 names (the verdict before and after an edit for B-015), or keep the facts form in a scenario that B-006's milestone delivers. Otherwise sound: § 9 has one row per claim and every tag resolves; the `@boundary` scenarios match § 5 rows 1 to 3; B-012's exit codes agree with `0001-F3` B-020, B-029, B-030, `0001-F10` B-013, B-014 and `0001-F2`; B-013's pins `0.1.0` and `0.2.1` both ship; A-2 records the `0001-F18` edge B-008 and B-009 wait on; OQ-1 to OQ-7 stay open on recorded defaults, which do not block agreement. |

## Tasks

- `0330`: the verdict and the rules every answer keeps (B-001, B-002, B-011, B-013, B-015 to B-017), delivered with `0132-F3`'s `0329`.
- `0331`: a rule's text, the rule list and the manifest, arguments and failures (B-003 to B-005, B-010, B-012, B-014).

B-006 to B-009 and B-018 are not cut: they wait on `0132-F2`, `0001-F13` and `0001-F18` (the owner, 2026-11-01).

## Scoring

| Field | Value | Basis                                    |
| ----- | ----- | ---------------------------------------- |
| value | 0     | Not scored while the agreement is draft. |
| risk  | 0     | Not scored while the agreement is draft. |
