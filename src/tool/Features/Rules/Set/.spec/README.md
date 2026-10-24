---
title: "Specification: Setting a rule's level"
description: "The set command sets one rule id of the pinned schema version to error, warning or off in the manifest's rules key and changes nothing else in the manifest or under the root, as the pin command changes only schemaVersion"
type: feature
id: "F11"
epic: "0001"
spec_status: draft
status: ready-for-architecture
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
depends_on: ["F2", "F5", "F7"]
blocks: []
spikes: []
created: "2026-10-11"
updated: "2026-10-24"
github_issue: null
synced_at: null
---

# Specification: Setting a rule's level

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-11 -->

Lowering, raising or switching off a rule is a hand edit to a JSON file: a mistyped rule id or level is found only when the next check rejects the manifest, and an agent making the edit rewrites more of the file than the one value it meant to change. This Feature removes that failure state: the set command sets one rule id to `error`, `warning` or `off` in the manifest, refuses an id or a level the pinned version does not have, and changes nothing else.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-11, 2026-10-24 -->

| #   | Persona                              | Need                                                                                                 | Pain Point Today                                                                              |
| --- | ------------------------------------ | ---------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------- |
| 1   | The maintainer repairing an old tree | Lower a rule now and raise it again later, one command each                                          | Opens the manifest, finds the entry, types the level, runs the check to see whether it parsed |
| 2   | The agent changing a rule's level    | A write that touches one value and is refused when the id or level is wrong                          | Rewrites the whole manifest and may reorder or drop a key                                     |
| 3   | A script preparing a repository      | An exit code that says whether the level was set                                                     | Parses and rewrites JSON itself                                                               |
| 4   | `0001-F5`, `0001-F7`                 | A writer that keeps the manifest one the check accepts and leaves `schemaVersion` to the pin command | N/A (internal dependency)                                                                     |

### Assumptions

| ID  | Assumption                                                                                                                                                                                                                                                                                |
| --- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | The working command line is `specht rules set <id> <level> [--root <dir>]`; the name is not decided (OQ-1), and the claims say "the set command" until it is, as `0001-F7` says "the pin command" (`0001-F7` OQ-11). `--root` defaults to the current directory as it does for the check. |
| A-2 | A level is one of `error`, `warning` and `off`, with the meaning `0001-F5` decision 0008 gives each; a rule id is one of the pinned version's vocabulary (`0001-F5` B-013).                                                                                                               |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-11, 2026-10-24 -->

| ID    | Claim                                                                                                                                                                                                                                               | Source                                                                                                                                                                                                            | Status |
| ----- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------ |
| B-001 | Given the set command naming a rule id of the pinned version's vocabulary and a level, this Feature sets that rule id to that level in the manifest's `rules`.                                                                                      | owner, 2026-10-11; decision 0001; OQ-1                                                                                                                                                                            | Active |
| B-002 | Given the set command sets a level, this Feature exits with code `0`.                                                                                                                                                                               | `0001-F7` B-043; C-6                                                                                                                                                                                              | Active |
| B-003 | Given the set command under a manifest whose `rules` carries an entry for the named rule id, the manifest's bytes differ from before only in that entry's value.                                                                                    | owner, 2026-10-11; decision 0001; C-1; `0001-F7` B-045                                                                                                                                                            | Active |
| B-004 | Given the set command under a manifest whose `rules` carries no entry for the named rule id, this Feature adds the entry and every other key and every other `rules` entry keeps its value.                                                         | owner, 2026-10-11; decision 0001; C-1; `0001-F7` B-047                                                                                                                                                            | Active |
| B-005 | Given the set command under a manifest with no `rules` key, this Feature adds the key holding that one entry and every other key keeps its value.                                                                                                   | owner, 2026-10-11; decision 0001; C-1; `0001-F7` B-047                                                                                                                                                            | Active |
| B-006 | Given the set command, no file under the root other than the manifest is created, modified or deleted.                                                                                                                                              | owner, 2026-10-11; decision 0001; C-1; brief § 9; `0001-F7` B-046                                                                                                                                                 | Active |
| B-007 | Given the set command naming a rule id outside the pinned version's vocabulary under a root and a manifest that B-009 to B-011 accept, this Feature names the id on stderr, writes nothing, and exits with code `4`.                                | derived from `0001-F2` decision 0003 (`4` is for what the command line names), `0001-F3` B-020 and `0001-F7` B-048, B-111; not stated by the owner; decision 0002; OQ-2                                           | Active |
| B-008 | Given the set command naming a level other than `error`, `warning` and `off` under a root and a manifest that B-009 to B-011 accept, this Feature names the level as typed and the three levels on stderr, writes nothing, and exits with code `4`. | derived from `0001-F2` decision 0003 (`4` is for what the command line names), `0001-F8` B-020 and `0001-F7` B-111; the refusal is the owner's, 2026-10-11, the code is not; `0001-F5` OQ-15; decision 0002; OQ-2 | Active |
| B-009 | Given the set command with `--root` naming a path that is not a directory, this Feature writes nothing and exits with code `2`.                                                                                                                     | brief § 5; `0001-F7` B-049                                                                                                                                                                                        | Active |
| B-010 | Given the set command under a root with no manifest, this Feature writes nothing and exits with code `2`.                                                                                                                                           | brief § 5; `0001-F7` B-050; C-2; OQ-2                                                                                                                                                                             | Active |
| B-011 | Given the set command under a root whose manifest the check rejects, this Feature writes nothing and exits with code `3`.                                                                                                                           | `0001-F2` decision 0003; `0001-F7` B-051; OQ-2; OQ-3                                                                                                                                                              | Active |
| B-012 | Given the set command sets a level in a manifest the check accepts, the check accepts the manifest it leaves.                                                                                                                                       | derived from C-3 and `0001-F5` B-013, B-045; not stated by the owner                                                                                                                                              | Active |
| B-013 | Given any output on stdout or stderr, every path the tool derives is relative to the root with `/` separators, and the only path that may be absolute is one typed on the command line, echoed as given.                                            | brief § 9; `0001-F2` B-013                                                                                                                                                                                        | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-11 -->

| ID  | Constraint                                                                                                                                                        | Rules Out                                                                                                                                                                               |
| --- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| C-1 | The set command writes the manifest only, and in it only the named rule id's entry under `rules` (decision 0001).                                                 | Reformatting or reordering the manifest; writing `schemaVersion` or any other key; filling in an entry for a rule that was not named; touching a schema file, a template or a document. |
| C-2 | The set command never creates a manifest.                                                                                                                         | Writing a manifest into a root that has none, which is `init`'s (`0001-F4`); falling back to an embedded default.                                                                       |
| C-3 | The set command writes only a rule id of the pinned version's vocabulary and one of the three levels, so it never turns an accepted manifest into a rejected one. | An unknown id written "for a later version"; an object or a number as the value; a fourth level; a write followed by a failure.                                                         |
| C-4 | A level a manifest sets changes only through the set command or a hand edit.                                                                                      | The check, `upgrade`, the pin command, `specht rules` or `specht manifest` changing a level.                                                                                            |
| C-5 | Deterministic and offline: the manifest it leaves is a function of the manifest it found, the rule id and the level.                                              | A network call; a timestamp or a comment written into the manifest; a prompt.                                                                                                           |
| C-6 | Exit codes are `0001-F2` C-2's constants, with the meanings the check gives them.                                                                                 | A sixth exit code; a code that means one thing to this command and another to the pin command.                                                                                          |
| C-7 | A command parses, calls the engine and folds the result into an exit code; nothing else lives in the command (`0001-F2` C-4).                                     | Editing JSON or checking the vocabulary inside `Features/Rules/Set/`.                                                                                                                   |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-11 -->

| #   | Item                                                                   | Exclusion Reason                                                                                                                                           |
| --- | ---------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | Setting any other manifest key                                         | The owner's decision is the rule level only (decision 0001). `schemaVersion` is the pin command's (`0001-F7` B-043, C-13); every other key is a hand edit. |
| 2   | A severity per path or per Feature                                     | `0001-F5` § 5 row 14; item `0141` puts it to the owner.                                                                                                    |
| 3   | An interactive editor, a prompt or a selection list                    | Not asked for; C-5.                                                                                                                                        |
| 4   | What `init` writes into `rules`, and what `upgrade` adds to it         | `0001-F4` B-024 to B-026; `0001-F7` B-016 and OQ-31.                                                                                                       |
| 5   | Setting several rules, a family or every rule in one run               | Not asked for: the owner's decision is one rule id.                                                                                                        |
| 6   | Removing a rule's entry so that it follows the version's default again | Not decided (OQ-5).                                                                                                                                        |
| 7   | Adding a rule, or a setting of a rule other than its level             | `0001-F5` C-3 and § 5 row 14.                                                                                                                              |
| 8   | Listing the rules and their levels                                     | `0001-F10`.                                                                                                                                                |

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

| Claim ID | Scenario                                                        | Test    | Status  |
| -------- | --------------------------------------------------------------- | ------- | ------- |
| B-001    | Setting a level writes it into the manifest                     | Missing | Missing |
| B-002    | Setting a level succeeds                                        | Missing | Missing |
| B-003    | Setting a level changes only that rule's entry                  | Missing | Missing |
| B-004    | Setting a rule the manifest does not name adds its entry        | Missing | Missing |
| B-005    | Setting a rule in a manifest with no rule settings adds them    | Missing | Missing |
| B-006    | Setting a level touches no other file                           | Missing | Missing |
| B-007    | A rule the pinned version does not hold is a not-found failure  | Missing | Missing |
| B-008    | A level that is not one of the three is a not-found failure     | Missing | Missing |
| B-009    | Setting a level under a missing root is a missing-input failure | Missing | Missing |
| B-010    | Setting a level without a manifest is a missing-input failure   | Missing | Missing |
| B-011    | Setting a level in an invalid manifest is invalid configuration | Missing | Missing |
| B-012    | The manifest left behind is one the check accepts               | Missing | Missing |
| B-013    | No derived path in the output is absolute                       | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-11, 2026-10-24 -->

- 2026-10-24, groomed from `blocked` to `ready-for-architecture` ([decision 0002](decisions/0002-a-root-or-manifest-fault-is-reported-before-an-unknown-rule-or-level.md)). The block named `0001-F2`, `0001-F5` and `0001-F7`: `0001-F2` and `0001-F5` are built; `0001-F7`'s list and pin commands are built (items `0121` to `0125`) and only `upgrade` (item `0047`) is outstanding, which no claim here depends on.
- 2026-10-24, OQ-2 resolved by `0001-F7` OQ-23 (decision 0012, B-111): B-007 and B-008 now apply under a root and manifest that B-009 to B-011 accept, and two boundary scenarios state the order. OQ-3 is not carried over from `0001-F7` OQ-20 and stays open.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-11, 2026-10-24 -->

| ID   | Question                                                                                                                                                                                                                                                                                                                                                                                                   | Blocks                         | Resolution                                                                                                                                                                                                                                              |
| ---- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | The name of the set command. The owner asked "maybe even an edit function?" and decided its scope, not its name. Readings: (a) `specht rules set <id> <level>`, a branch of `0001-F10`'s command, which is its default command (`0001-F10` A-1), as `schema` is a branch with `list` and `pin`; (b) `specht manifest set`; (c) a top-level command. The claims say "the set command" until it is answered. | B-001 to B-011 (the name only) | Open. A-1 carries (a) as the working default. Owner not asked.                                                                                                                                                                                          |
| OQ-2 | Which failure is reported when two apply: an unknown rule id or level (B-007, B-008, `4`) under a root with no manifest (B-010, `2`) or a manifest the check rejects (B-011, `3`). `0001-F7` OQ-23 asks the same of the pin command; one answer should serve both.                                                                                                                                         | B-007, B-008, B-010, B-011     | Resolved 2026-10-24: the owner decided it for the pin command (`0001-F7` OQ-23, decision 0012, B-111) and OQ-2 asked for one answer for both. The root and the manifest are validated first, so `2` or `3` wins over `4` (B-007, B-008; decision 0002). |
| OQ-3 | Does the set command repair a manifest the check rejects only for the entry being set: a level that is not one of the three (`0001-F5` B-045), or an id the pinned version does not hold (`0001-F5` B-013)? B-011 refuses every rejected manifest. `0001-F7` OQ-20 asks the same of the pin command.                                                                                                       | B-011                          | Open. Proceeds on B-011 as written. Owner not asked.                                                                                                                                                                                                    |
| OQ-4 | What does the set command print on success: nothing, the rule id and its new level, or the level it replaced as well?                                                                                                                                                                                                                                                                                      | B-002                          | Open. Proceeds with no claim on stdout. Owner not asked.                                                                                                                                                                                                |
| OQ-5 | `init` writes a level for every rule (`0001-F4` B-024), so a rule follows a later version's default again only when its entry is removed. Does the set command remove an entry, by a fourth word such as `default` or by a second command, or is that a hand edit?                                                                                                                                         | § 5 row 6                      | Open. Proceeds with no claim: an entry is not removed. Owner not asked.                                                                                                                                                                                 |
| OQ-6 | Where does an added entry or an added `rules` key go, and how is it written: last in `rules`, in rule id order, in the file's own indentation? B-004 and B-005 fix that every other key and entry keeps its value, not the bytes around the addition. `0001-F7` B-047 leaves the same open for the pin command.                                                                                            | B-004, B-005                   | Open. Proceeds on B-004 and B-005 as written: no claim on the position or the bytes of an addition. Owner not asked.                                                                                                                                    |
| OQ-7 | Is a level typed in another letter case, such as `Warning`, refused by B-008 or written in lower case? `0001-F5` OQ-15 (a) asks the same of a manifest.                                                                                                                                                                                                                                                    | B-008                          | Open. Proceeds on B-008 as written: refused. Owner not asked.                                                                                                                                                                                           |

## 12. Sign-off

<!-- last written by: spec-author, 2026-10-11 -->

| Section | Status | Reviewer      | Note                   |
| ------- | ------ | ------------- | ---------------------- |
| 1-5     | 🟡     | spec-reviewer | Awaiting first review. |

## Tasks

- `0287`: the engine edits one rule's level in the manifest (B-001, B-003, B-004, B-005, B-012).
- `0288`: the set command, refusals and exit codes (B-002, B-006 to B-011, B-013).

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
