---
title: "Specification: The rules command"
description: "specht rules lists every rule id of the pinned schema version with its family, its default severity and the level it has under the root's manifest, as text or as one JSON document, read from the version's vocabulary and its rule pages, and writes nothing"
type: feature
id: "F10"
epic: "0001"
spec_status: draft
status: blocked
priority: med
value: 0
risk: 0
rank: 0
scored_by: null
scored_on: null
domain: "Specification governance"
author: "spec-author"
milestone: null
children: []
depends_on: ["F2", "F3", "F5", "F7"]
blocks: []
spikes: []
created: "2026-10-11"
updated: "2026-10-11"
github_issue: null
synced_at: null
---

# Specification: The rules command

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-11 -->

A rule can be lowered, raised or switched off from the manifest, but nothing lists the rules a pinned version has: a maintainer learns a rule id from the violation that names it, and its level only by reading the manifest beside the rule's page. This Feature removes that failure state: `specht rules` lists every rule id of the pinned version with its family, its default severity and the level it has under this manifest, as text or as one JSON document.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-11 -->

| #   | Persona                                         | Need                                                                                  | Pain Point Today                                                             |
| --- | ----------------------------------------------- | ------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------- |
| 1   | The maintainer adopting the tool on an old tree | Every rule of the pinned version and what each is set to, in one listing              | Rule ids are met one violation at a time; brief § 4 is in another repository |
| 2   | The agent about to change a rule's level        | The ids the manifest may name and the level each has now                              | Reads the manifest, then `--explain` for each id it already knows            |
| 3   | A script or an MCP host (epic `0132`)           | The same listing as one JSON document on stdout                                       | No command prints the rule vocabulary                                        |
| 4   | `0001-F3`, `0001-F5`                            | A listing that reads the vocabulary and the rule settings and keeps no copy of either | N/A (internal dependency)                                                    |

### Assumptions

| ID  | Assumption                                                                                                                                                                                                           |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | The command line is `specht rules [--root <dir>] [--json]`, beside `specht`, `init` and `upgrade`; `--root` defaults to the current directory as it does for the check (owner, 2026-10-11).                          |
| A-2 | A rule's family and default severity are the ones its rule page gives (`0001-F3` B-032, A-5): one of the eight families, and `error` or `warning`.                                                                   |
| A-3 | A rule's effective level is `error`, `warning` or `off` (`0001-F5` decision 0008): the level the manifest sets for its id, or, where the manifest sets none, the level of the rule's own severity (`0001-F5` B-044). |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-11 -->

| ID    | Claim                                                                                                                                                                                                                                                                                                                                                                                                                         | Source                                                                                                           | Status |
| ----- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------- | ------ |
| B-001 | Given a root whose manifest the check accepts, this Feature prints on stdout one entry for each rule id in the pinned version's vocabulary and no other entry.                                                                                                                                                                                                                                                                | owner, 2026-10-11; decision 0001; `0001-F3` B-031                                                                | Active |
| B-002 | Given an entry, it carries its rule's family.                                                                                                                                                                                                                                                                                                                                                                                 | owner, 2026-10-11; A-2                                                                                           | Active |
| B-003 | Given an entry, it carries its rule's default severity.                                                                                                                                                                                                                                                                                                                                                                       | owner, 2026-10-11; A-2                                                                                           | Active |
| B-004 | Given a manifest that sets a level for a rule id, that rule's entry carries that level as its effective level.                                                                                                                                                                                                                                                                                                                | owner, 2026-10-11; A-3; `0001-F5` B-010, B-011, B-046                                                            | Active |
| B-005 | Given a manifest that sets no level for a rule id, that rule's entry carries the level of the rule's own severity as its effective level.                                                                                                                                                                                                                                                                                     | A-3; `0001-F5` B-044                                                                                             | Active |
| B-006 | Given a manifest that sets a rule id to `off`, the listing still carries that rule's entry.                                                                                                                                                                                                                                                                                                                                   | derived from B-001 and `0001-F5` B-011; not stated by the owner                                                  | Active |
| B-007 | Given an entry, the family and the default severity it carries are the ones `--explain` prints for that rule id under the same root.                                                                                                                                                                                                                                                                                          | decision 0001; `0001-F3` B-019, B-032, C-6                                                                       | Active |
| B-008 | Given the listing, its entries are in ascending ordinal order of rule id.                                                                                                                                                                                                                                                                                                                                                     | derived by spec-author (the order the vocabulary is written in, brief § 4); not stated by the owner; OQ-5        | Active |
| B-009 | Given `--json`, stdout parses as exactly one JSON document.                                                                                                                                                                                                                                                                                                                                                                   | owner, 2026-10-11                                                                                                | Active |
| B-010 | Given `--json`, the document carries the entries the text form prints, each with the same rule id, family, default severity and effective level, in the same order.                                                                                                                                                                                                                                                           | owner, 2026-10-11; C-2                                                                                           | Active |
| B-011 | Given a root whose manifest the check accepts, this Feature exits with code `0`, whatever levels the manifest sets.                                                                                                                                                                                                                                                                                                           | `0001-F2` C-2; C-5                                                                                               | Active |
| B-012 | Given `--root` names a path that is not a directory, this Feature writes a message naming that path as it was given on stderr, writes nothing on stdout, and exits with code `2`.                                                                                                                                                                                                                                             | `0001-F2` B-005; C-5                                                                                             | Active |
| B-013 | Given a root with no manifest at the manifest path, this Feature writes a message naming the manifest path on stderr, writes nothing on stdout, and exits with code `2`.                                                                                                                                                                                                                                                      | `0001-F2` B-006; `0001-F3` B-029, OQ-2; OQ-1                                                                     | Active |
| B-014 | Given a root whose manifest the check rejects - one that does not parse (`0001-F2` B-007), one `0001-F5` rejects (`0001-F5` B-022), one whose `schemaVersion` the check rejects (`0001-F7` B-003, B-039), or one recording an upstream schema source while explicitly selecting the embedded source (`0001-F7` B-036) - this Feature names on stderr what the check names, writes nothing on stdout, and exits with code `3`. | owner, 2026-10-08 (every command answers a rejected manifest with `3`); `0001-F3` B-030; `0001-F2` decision 0003 | Active |
| B-015 | Given any run, stdout carries only the listing, and every message about the tool itself goes to stderr.                                                                                                                                                                                                                                                                                                                       | `0001-F2` B-009, C-3; C-6                                                                                        | Active |
| B-016 | Given any run, no file is created, modified or deleted, under the root or anywhere else.                                                                                                                                                                                                                                                                                                                                      | brief § 9; C-1                                                                                                   | Active |
| B-017 | Given any output on stdout or stderr, every path the tool derives is relative to the root with `/` separators, and the only path that may be absolute is one typed on the command line, echoed as given.                                                                                                                                                                                                                      | brief § 9; `0001-F2` B-013                                                                                       | Active |
| B-018 | Given the same root, manifest and options, two runs print the same stdout and exit with the same code.                                                                                                                                                                                                                                                                                                                        | brief § 9; C-3                                                                                                   | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-11 -->

| ID  | Constraint                                                                                                                                                     | Rules Out                                                                                                                                                                                       |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| C-1 | The command is read-only: it writes nothing, anywhere.                                                                                                         | An `--output <path>`; writing a rules index file under the root or under `docs/`; setting a level, which is `0001-F11`'s.                                                                       |
| C-2 | The listing is read from the pinned version's vocabulary and its rule pages, and from the manifest the check's loader produced (decision 0001; `0001-F3` C-6). | A second catalogue of rule ids, families or severities in the command or in a file; a hand-kept or generated index (`0001-F3` § 5 row 10); a rule listed that the pinned version does not hold. |
| C-3 | Deterministic and offline: the output is a function of the manifest, the pinned version and the tool version.                                                  | A network call; a clock, a locale or a machine name in the output; an order that follows a culture.                                                                                             |
| C-4 | A command parses, calls the engine and folds the result into an exit code; nothing else lives in the command (`0001-F2` C-4).                                  | Reading a rule page or grading a level inside `Features/Rules/`.                                                                                                                                |
| C-5 | Exit codes are `0001-F2` C-2's constants, with the meanings the check gives them.                                                                              | A sixth exit code; exit `1` because a rule is set to `off` or `warning`.                                                                                                                        |
| C-6 | stdout carries the product only, as plain text (`0001-F2` C-3).                                                                                                | A banner, a count line, colour markup or a rendered table border on stdout.                                                                                                                     |
| C-7 | The command evaluates no rule and reads no specification.                                                                                                      | Violation counts per rule; a listing that changes with the tree's content.                                                                                                                      |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-11 -->

| #   | Item                                                                          | Exclusion Reason                                                                                               |
| --- | ----------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------- |
| 1   | Setting or removing a rule's level                                            | `0001-F11`. C-1.                                                                                               |
| 2   | A rule's cause, examples and fix                                              | `--explain` prints the rule page (`0001-F3` B-019).                                                            |
| 3   | The rest of the manifest                                                      | `0001-F9`.                                                                                                     |
| 4   | How many violations each rule reports on this tree                            | The check and its report (`0001-F2`, `0001-F3`); C-7.                                                          |
| 5   | The rules of a version other than the pinned one, or of every shipped version | Not asked for; the shipped versions are the list command's (`0001-F7` B-041). OQ-1 holds the no-manifest case. |
| 6   | Filtering by family, by level or by default severity                          | Not asked for; a caller filters the `--json` document.                                                         |
| 7   | A link or address for a rule's page                                           | No page has a published address (`0001-F3` § 5 row 9).                                                         |

## 6. Concern Separation

<!-- last written by: spec-author, 2026-10-11 -->

Pending: owned by `implementer`, written after agreement.

## 7. Technical Design

<!-- last written by: spec-author, 2026-10-11 -->

Pending: owned by `implementer`, written after agreement.

## 8. Testing Strategy

<!-- last written by: spec-author, 2026-10-11 -->

Pending: owned by `test-writer`, written after agreement.

## 9. Traceability Matrix

<!-- last written by: spec-author, 2026-10-11 -->

| Claim ID | Scenario                                                       | Test    | Status  |
| -------- | -------------------------------------------------------------- | ------- | ------- |
| B-001    | Every rule of the pinned version is listed once                | Missing | Missing |
| B-002    | A rule is listed with its family                               | Missing | Missing |
| B-003    | A rule is listed with its default severity                     | Missing | Missing |
| B-004    | A rule the manifest sets is listed at that level               | Missing | Missing |
| B-005    | A rule the manifest does not set is listed at its own severity | Missing | Missing |
| B-006    | A rule switched off is still listed                            | Missing | Missing |
| B-007    | The listing agrees with the rule's page                        | Missing | Missing |
| B-008    | Rules are listed in order of rule id                           | Missing | Missing |
| B-009    | The JSON form is one document                                  | Missing | Missing |
| B-010    | The JSON form carries what the text form prints                | Missing | Missing |
| B-011    | Listing succeeds whatever the levels are                       | Missing | Missing |
| B-012    | A missing root is a missing-input failure                      | Missing | Missing |
| B-013    | A root without a manifest is a missing-input failure           | Missing | Missing |
| B-014    | A rejected manifest is an invalid-manifest failure             | Missing | Missing |
| B-015    | Messages about the tool go to standard error                   | Missing | Missing |
| B-016    | A run writes nothing                                           | Missing | Missing |
| B-017    | No derived path in the output is absolute                      | Missing | Missing |
| B-018    | The same input gives the same output                           | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-11 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-11 -->

| ID   | Question                                                                                                                                                                                                                                                        | Blocks                | Resolution                                                                                              |
| ---- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------- | ------------------------------------------------------------------------------------------------------- |
| OQ-1 | With no manifest, does `specht rules` fail as the check and `--explain` do (`0001-F3` OQ-2, B-029), or list the newest shipped version's rules with each at its own severity? The effective level needs a manifest; the id, family and default severity do not. | B-013; § 5 row 5      | Open. Proceeds on the default: it fails with `2`, as `--explain` does. Owner not asked.                 |
| OQ-2 | What does an entry look like without `--json`: one line per rule with its four values, an aligned table, or something else?                                                                                                                                     | B-001 (the form only) | Open. No claim fixes the text form. Owner not asked.                                                    |
| OQ-3 | Does an entry also carry the rule's title, which its page holds (`0001-F3` B-032)? The owner named the family, the default severity and the effective level.                                                                                                    | B-001, B-010          | Open. No claim is written for a title. Owner not asked.                                                 |
| OQ-4 | Does the JSON document of this command get a published schema, as the report has (`0001-F3` C-1), with its member names fixed and bound to change by addition only? Epic `0132` would read it.                                                                  | B-010                 | Open. B-010 fixes what the document carries and no member name. Owner not asked.                        |
| OQ-5 | Item `0169` renumbers the rule ids to four digits whose thousands digit names the group. Does the listing then order and group by that digit, and is "family" (`0001-F3` A-5) still the word, or the group ADR-0010 names?                                      | B-002, B-008          | Open. B-008's ordinal order holds under either numbering; B-002 follows the rule page. Owner not asked. |

## 12. Sign-off

<!-- last written by: spec-author, 2026-10-11 -->

| Section | Status | Reviewer      | Note                   |
| ------- | ------ | ------------- | ---------------------- |
| 1-5     | 🟡     | spec-reviewer | Awaiting first review. |

## Tasks

None yet. Cut from § 3 after agreement.

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
