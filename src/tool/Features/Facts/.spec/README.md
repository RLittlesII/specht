---
title: "Specification: The facts command"
description: "specht facts gives what the specification tree holds - each Feature's identity, statuses, dependencies, claims, constraints, out-of-scope rows, open questions, deltas, sign-off rows and the records beside it - for the whole tree or a path, as text or one JSON document with a published schema, read through the pinned manifest, writing nothing and exiting 0 whatever the verdict"
type: feature
id: "F2"
epic: "0132"
spec_status: draft
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
depends_on: []
blocks: []
spikes: []
created: "2026-10-30"
updated: "2026-10-30"
github_issue: null
synced_at: null
---

# Specification: The facts command

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-30 -->

The check reports what is wrong with a specification tree, and nothing reports what the tree holds. An agent or a maintainer who needs a Feature's claims, constraints, open questions or dependencies reads and searches the Markdown, scrapes a command's output, or answers from memory, and two readers get two pictures. This Feature removes that failure state: `specht facts` gives the tree's facts as one structured answer, read through the same parse and the same pinned manifest the check uses, so that epic `0132`'s MCP host serves it and epic `0228`'s review surfaces build on it.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-30 -->

| #   | Persona                                      | Need                                                       | Pain Point Today                                       |
| --- | -------------------------------------------- | ---------------------------------------------------------- | ------------------------------------------------------ |
| 1   | A coding agent in a consumer repository      | A Feature's claims, constraints and open questions as data | Reads and searches Markdown, or answers from memory    |
| 2   | The maintainer planning or reviewing work    | Every Feature's statuses and dependencies in one answer    | Opens each specification's frontmatter by hand         |
| 3   | Epic `0132` candidate C, reading through MCP | The command-line answer MCP serves unchanged               | N/A (internal dependency; no tree-facts answer exists) |
| 4   | Epic `0228` candidate B, the facts delta     | Two documents of one stable, published shape to compare    | N/A (internal dependency)                              |

### Assumptions

| ID  | Assumption                                                                                                                                                                                                               |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| A-1 | `specht facts` is a working name, recorded as one (the owner, 2026-10-30; decision 0001 call 3).                                                                                                                         |
| A-2 | The command has no rule id and judges nothing; its document has one shape under every shipped pin, and its values are read through the pinned manifest (the owner, 2026-10-30; decision 0001 call 5; as `0001-F13` A-2). |
| A-3 | The root, the manifest, discovery and the exit codes `2` and `3` are the check's (`0001-F2`, `0001-F5`, `0001-F6`). `depends_on` stays empty until those Features list this one in `blocks`.                             |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-30 -->

| ID    | Claim                                                                                                                                                                                                                                      | Source                                                                                                  | Status |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------- | ------ |
| B-001 | Given `specht facts [--root <dir>]`, this Feature gives the facts of every Feature specification the check discovers, in discovery order.                                                                                                  | the owner, 2026-10-30; decision 0001 call 3; `0001-F6`                                                  | Active |
| B-002 | Given `specht facts --json`, this Feature writes one document to stdout, and it validates against the published facts schema (OQ-1).                                                                                                       | the owner, 2026-10-30; decision 0001 call 4                                                             | Active |
| B-003 | Given `--json`, each specification carries its root-relative path, its qualified Feature id, its `spec_status` and its `status`.                                                                                                           | the owner, 2026-10-30; decision 0001 call 1; epic `0228` R1                                             | Active |
| B-004 | Given `--json`, each specification carries its `depends_on`, `blocks` and `children` as its frontmatter lists them.                                                                                                                        | the owner, 2026-10-30; decision 0001 call 1; epic `0228` R1; `children` is spec-author's reading        | Active |
| B-005 | Given `--json`, each § 3 claim carries its id, its text, its status and its line.                                                                                                                                                          | the owner, 2026-10-30; decision 0001 call 1; epic `0228` R1                                             | Active |
| B-006 | Given `--json`, each § 4 constraint carries its id, its text, what it rules out and its line.                                                                                                                                              | the owner, 2026-10-30; decision 0001 call 1; epic `0228` R1                                             | Active |
| B-007 | Given `--json`, each § 5 row carries its number, its item, its exclusion reason and its line.                                                                                                                                              | the owner, 2026-10-30; decision 0001 call 1; epic `0228` R1                                             | Active |
| B-008 | Given `--json`, each § 11 row carries its id, its question, its Blocks cell, its resolution, its line, and whether it is resolved as OQ-2 reads it.                                                                                        | the owner, 2026-10-30; decision 0001 call 1; epic `0228` R1; OQ-2                                       | Active |
| B-009 | Given `--json`, each § 10 entry carries its line and its text: a list item's text, or a table row's cells.                                                                                                                                 | the owner, 2026-10-30; decision 0001 call 1; epic `0228` R1; the entry's shape is spec-author's reading | Active |
| B-010 | Given `--json`, each § 12 row carries its section, its status, its reviewer, its note and its line.                                                                                                                                        | the owner, 2026-10-30; decision 0001 call 1; epic `0228` R1                                             | Active |
| B-011 | Given decision, ADR or lesson records in the `decisions/`, `adr/` and `lessons/` folders beside a specification, this Feature gives each with its kind, its number from its file name, its frontmatter `title` and its root-relative path. | the owner, 2026-10-30; decision 0001 call 1; epic `0228` R1; a record's status is OQ-3                  | Active |
| B-012 | Given a claim whose § 3 Status is the withdrawn or superseded marker, this Feature still gives the claim, with that status.                                                                                                                | AGENTS.md § "Stable IDs"; spec-author's reading                                                         | Active |
| B-013 | Given a tree the check reports violations for, this Feature gives every fact that parses, two claims sharing an id included, and exits `0`.                                                                                                | the owner, 2026-10-30; decision 0001 call 5; as `0001-F13` B-006                                        | Active |
| B-014 | Given `--json`, the document carries no violation, no count and no verdict.                                                                                                                                                                | the owner, 2026-10-30; decision 0001 call 5; C-5                                                        | Active |
| B-015 | Given a specification missing a section, or with a table the parse cannot read, this Feature gives no facts for that section and every other fact of that specification.                                                                   | the owner, 2026-10-30; decision 0001 call 5 ("give what parses")                                        | Active |
| B-016 | Given a manifest naming a section or a table header other than the default, this Feature reads that section's facts from what the manifest names.                                                                                          | the owner, 2026-10-30; decision 0001 call 5; `0001-F5`                                                  | Active |
| B-017 | Given the same specifications under two shipped pins, this Feature's two `--json` documents carry the same members.                                                                                                                        | the owner, 2026-10-30; decision 0001 call 5; epic `0132` "two schema versions" criterion                | Active |
| B-018 | Given `specht facts <path>` naming a discovered specification's root-relative path, this Feature gives that specification's facts only.                                                                                                    | the owner, 2026-10-30; decision 0001 call 3                                                             | Active |
| B-019 | Given `specht facts <path>` naming a root-relative directory, this Feature gives the facts of every discovered specification under it, in discovery order.                                                                                 | spec-author's reading; OQ-4                                                                             | Active |
| B-020 | Given `specht facts <path>` selecting no discovered specification, this Feature writes nothing on stdout and exits `4`.                                                                                                                    | the owner, 2026-10-30; decision 0001 call 3; `0001-F2` decision 0003                                    | Active |
| B-021 | Given a missing root or manifest this Feature exits `2`, and given a manifest the check rejects it exits `3`, as the check does.                                                                                                           | `0001-F2` B-006, B-007; as `0001-F13` B-006                                                             | Active |
| B-022 | Given the same tree and arguments, this Feature gives byte-identical output.                                                                                                                                                               | brief § 9; as `0001-F13` B-007                                                                          | Active |
| B-023 | Given `specht facts`, no file under the root or elsewhere is created, modified or deleted.                                                                                                                                                 | brief § 9; AGENTS.md § Invariants                                                                       | Active |
| B-024 | Given `specht facts` without `--json`, this Feature prints one line per claim, constraint, § 5 row and open question, each beginning with its specification's root-relative path.                                                          | spec-author's reading; OQ-5                                                                             | Active |
| B-025 | Given `--json`, a claim carries no § 9 row, no scenario and no test.                                                                                                                                                                       | the owner, 2026-10-30; decision 0001 call 2                                                             | Active |
| B-026 | Given an epic file the check discovers, this Feature gives no facts for it.                                                                                                                                                                | spec-author's reading; OQ-6                                                                             | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-30 -->

| ID  | Constraint                                                                                                                                        | Rules Out                                                                                                                     |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------- |
| C-1 | The facts are read by the discovery and the parse the check uses (`0001-F6`; AGENTS.md § Architecture).                                           | A second Markdown, YAML or table parser; a fact disagreeing with a violation about a claim's id, line or section.             |
| C-2 | Offline and read-only; every path is relative to the root with `/` separators (brief § 9).                                                        | A network call; any write, a cache file included; an absolute path in a line or in the document.                              |
| C-3 | Within a tool major version the document changes only by addition (the owner, 2026-10-30; decision 0001 call 4; as `0001-F3` C-4).                | Renaming or removing a member, or changing a member's type, inside a major version.                                           |
| C-4 | A command parses, calls the engine and folds the result into an exit code (`0001-F2` C-4).                                                        | Reading Markdown, YAML or record files inside `Features/Facts/`.                                                              |
| C-5 | A fact is given, never judged: the command has no rule id and its shape does not vary with the pin (the owner, 2026-10-30; decision 0001 call 5). | A severity, a violation or a verdict in the output; a new `SPEC###`; a member present under one pin and absent under another. |
| C-6 | Every fact MCP gives about the tree is one this Feature gives (epic `0132` § Constraints).                                                        | A tree fact, or a member of one, available through MCP and not here.                                                          |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-30 -->

| #   | Item                                                                | Exclusion Reason                                                                                        |
| --- | ------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------- |
| 1   | Claim coverage: § 9 rows, scenarios carrying a tag, test resolution | `0001-F13` (`specht trace`); the owner, 2026-10-30; decision 0001 call 2; B-025.                        |
| 2   | The verdict, a violation or a count                                 | The check (`0001-F2`, `0001-F3`); B-014.                                                                |
| 3   | Selecting a specification by id, such as `0001-F3`                  | Qualified-key resolution is epic `0228` candidate C and item `0307`; decision 0001 call 3.              |
| 4   | Serving the facts through MCP                                       | Epic `0132` candidates B and C; this Feature is the command-line answer they serve.                     |
| 5   | Any write, `--report <path>` included                               | Not asked for; C-2.                                                                                     |
| 6   | Work items and the work queue                                       | Epic `0132` candidate E, parked; the item schema differs between this repository and its consumers.     |
| 7   | Comparing two facts documents                                       | Epic `0228` candidate B.                                                                                |
| 8   | A record's body, or a specification's §§ 1, 2 and 6 to 8 as text    | Not asked for: epic `0228` R1 names the sections given; complete documents are spike `0252`'s question. |
| 9   | A persistent index or cache of the facts                            | `0132-F1`; C-2.                                                                                         |
| 10  | Epic files                                                          | Not decided (OQ-6); B-026 proceeds on the default.                                                      |

## 6. Concern Separation

<!-- last written by: spec-author, 2026-10-30 -->

Pending: owned by `implementer`, written after agreement.

## 7. Technical Design

<!-- last written by: spec-author, 2026-10-30 -->

Pending: owned by `implementer`, written after agreement.

## 8. Testing Strategy

<!-- last written by: spec-author, 2026-10-30 -->

Pending: owned by `test-writer`, written after agreement.

## 9. Traceability Matrix

<!-- last written by: spec-author, 2026-10-30 -->

| Claim ID | Scenario                                                         | Test    | Status  |
| -------- | ---------------------------------------------------------------- | ------- | ------- |
| B-001    | The facts cover every specification the check discovers          | Missing | Missing |
| B-002    | The JSON facts validate against the published schema             | Missing | Missing |
| B-003    | A specification is given with its path, identity and statuses    | Missing | Missing |
| B-004    | A specification is given with its dependencies                   | Missing | Missing |
| B-005    | A claim is given with its text, status and line                  | Missing | Missing |
| B-006    | A constraint is given with what it rules out                     | Missing | Missing |
| B-007    | An out-of-scope row is given with its reason                     | Missing | Missing |
| B-008    | An open question is given with whether it is resolved            | Missing | Missing |
| B-009    | A lessons and deltas entry is given with its text and line       | Missing | Missing |
| B-010    | A sign-off row is given with its reviewer and note               | Missing | Missing |
| B-011    | The records beside a specification are given by number and title | Missing | Missing |
| B-012    | A withdrawn claim is still given                                 | Missing | Missing |
| B-013    | The facts succeed on a tree the check fails                      | Missing | Missing |
| B-014    | The facts carry no verdict                                       | Missing | Missing |
| B-015    | A missing section leaves only its own facts out                  | Missing | Missing |
| B-016    | A section the manifest renames is read under its new name        | Missing | Missing |
| B-017    | The facts have one shape under every pin                         | Missing | Missing |
| B-018    | A path selects one specification                                 | Missing | Missing |
| B-019    | A directory selects every specification under it                 | Missing | Missing |
| B-020    | A path that selects nothing fails                                | Missing | Missing |
| B-021    | The facts fail on a broken root as the check does                | Missing | Missing |
| B-022    | The same tree gives the same facts                               | Missing | Missing |
| B-023    | The facts write nothing                                          | Missing | Missing |
| B-024    | The text form gives one line per fact                            | Missing | Missing |
| B-025    | The facts carry no claim coverage                                | Missing | Missing |
| B-026    | An epic file is not given                                        | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-30 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-30 -->

| ID   | Question                                                                                                                                                                                                                                                                                     | Blocks | Resolution                                                                                                                                   |
| ---- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------ | -------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | What is the published facts schema's path and name?                                                                                                                                                                                                                                          | B-002  | Open. Proceeds on `docs/schema/facts.schema.json`, beside the report's and the trace's. Owner not asked.                                     |
| OQ-2 | How is an open question read as resolved? `0228-F1` B-009 reads a Resolution as open when it is empty or its first word is the open marker; schema `0.2.1` and earlier declare no open marker.                                                                                               | B-008  | Open. Proceeds on `0228-F1` B-009's reading, with `Open` in exact case wherever the pinned manifest declares no marker. Owner not asked.     |
| OQ-3 | What is a record's status? A decision and a lesson carry none in their frontmatter, and an ADR's status is the first line of its `## Status` section, which is body, not frontmatter. Options: no status; the ADR's status read from its body; a `status` key added to the record templates. | B-011  | Open. B-011 gives no status until answered. Adding a key is a template change under a new schema version (`0001-F7` C-11).                   |
| OQ-4 | Does the path selector take a directory, selecting every specification under it, or only a specification's own path?                                                                                                                                                                         | B-019  | Open. Proceeds on a directory selecting by whole path segments. Owner not asked.                                                             |
| OQ-5 | What is the text form's exact line shape: which values, in what order, and separated how?                                                                                                                                                                                                    | B-024  | Open. Proceeds on one tab-separated line per fact: path, kind, id, line, text, as `0001-F10` decision 0002 lists the rules. Owner not asked. |
| OQ-6 | Does the document carry epic files? An epic has sections of its own, not the twelve a Feature specification has.                                                                                                                                                                             | B-026  | Open. Proceeds on no epic facts. Owner not asked.                                                                                            |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-30 -->

| Section | Status | Reviewer      | Note                                        |
| ------- | ------ | ------------- | ------------------------------------------- |
| 1-5     | 🟡     | spec-reviewer | Draft, not yet reviewed; OQ-1 to OQ-6 open. |

## Tasks

None yet. Cut from § 3 after agreement.

## Scoring

| Field | Value | Basis                                    |
| ----- | ----- | ---------------------------------------- |
| value | 0     | Not scored while the agreement is draft. |
| risk  | 0     | Not scored while the agreement is draft. |
