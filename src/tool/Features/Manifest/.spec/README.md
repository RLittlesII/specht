---
title: "Specification: The manifest command"
description: "specht manifest prints the effective manifest of a root, the manifest on disk with every key it leaves out filled from the pinned version's default as the engine reads it, or with --defaults the default manifest the tool ships, as text or as one JSON document, and writes nothing"
type: feature
id: "F9"
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
depends_on: ["F2", "F4", "F5", "F7"]
blocks: []
spikes: []
created: "2026-10-11"
updated: "2026-10-11"
github_issue: null
synced_at: null
---

# Specification: The manifest command

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-11 -->

A manifest states only what its author wrote; every key it leaves out is read from a default that lives inside the tool, so nobody can see the configuration a check actually ran under without reading the engine. This Feature removes that failure state: `specht manifest` prints the effective manifest of a root, and with `--defaults` the default manifest the tool ships, as text or as one JSON document.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-11 -->

| #   | Persona                                 | Need                                                                    | Pain Point Today                                                    |
| --- | --------------------------------------- | ----------------------------------------------------------------------- | ------------------------------------------------------------------- |
| 1   | The maintainer configuring a repository | See every value the check reads, the ones the manifest leaves out too   | A left-out key's value is in the engine's embedded default only     |
| 2   | The agent editing a manifest            | The default of every key, without a repository at hand                  | Runs `specht init` in an empty directory to read the files it wrote |
| 3   | A script or an MCP host (epic `0132`)   | The same answer as one JSON document on stdout                          | No command prints schema knowledge                                  |
| 4   | `0001-F5`, `0001-F7`                    | A reader of the loaded manifest that adds no second reading of the file | N/A (internal dependency)                                           |

### Assumptions

| ID  | Assumption                                                                                                                                                                                                                                                                                       |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| A-1 | The command line is `specht manifest [--root <dir>] [--defaults] [--json]`, beside `specht`, `init` and `upgrade`; `--root` defaults to the current directory as it does for the check (owner, 2026-10-11; decision 0001).                                                                       |
| A-2 | "The effective manifest" is the manifest on disk as the engine reads it for a check: each key it declares at its own value, and each key it leaves out that the engine reads from the default manifest at the value the pinned version's default gives it (`0001-F5` B-019, A-4; decision 0001). |
| A-3 | "The default manifest" under `--defaults` is the manifest the newest version the tool ships gives, the one `specht init --epics` writes into a bare root (`0001-F4` B-018, B-003); whether another version can be named is OQ-3.                                                                 |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-11 -->

| ID    | Claim                                                                                                                                                                                                                                                                                                                                                                                                                                              | Source                                                                                                         | Status |
| ----- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------- | ------ |
| B-001 | Given a root whose manifest the check accepts, this Feature prints the effective manifest on stdout.                                                                                                                                                                                                                                                                                                                                               | owner, 2026-10-11; decision 0001                                                                               | Active |
| B-002 | Given a root whose manifest the check accepts, this Feature exits with code `0`.                                                                                                                                                                                                                                                                                                                                                                   | `0001-F2` C-2; C-5                                                                                             | Active |
| B-003 | Given a manifest that declares a key, the effective manifest carries that key at the manifest's value.                                                                                                                                                                                                                                                                                                                                             | decision 0001; `0001-F7` C-4                                                                                   | Active |
| B-004 | Given a manifest that leaves out a key the engine reads from the default manifest (`0001-F5` B-019), the effective manifest carries that key at the value the pinned version's default gives it.                                                                                                                                                                                                                                                   | owner, 2026-10-11; decision 0001; `0001-F5` B-019                                                              | Active |
| B-005 | Given a manifest that sets no level for a rule id of the pinned version's vocabulary, the effective manifest's rule settings carry that id at the rule's own severity.                                                                                                                                                                                                                                                                             | decision 0001; `0001-F5` B-044, decision 0009                                                                  | Active |
| B-006 | Given a manifest with no `schemaVersion` key, the effective manifest carries `schemaVersion` `0.1.0`.                                                                                                                                                                                                                                                                                                                                              | derived from decision 0001 and `0001-F7` A-1, B-002; not stated by the owner                                   | Active |
| B-007 | Given `--json` and the effective manifest it prints written as the manifest of a copy of the root, the check reports for the copy what it reports for the root.                                                                                                                                                                                                                                                                                    | derived by spec-author from the owner's "what the engine reads" (decision 0001); not stated by the owner; OQ-2 | Active |
| B-008 | Given `--defaults`, this Feature prints the default manifest of the newest version the tool ships on stdout.                                                                                                                                                                                                                                                                                                                                       | owner, 2026-10-11; decision 0001; A-3; OQ-3                                                                    | Active |
| B-009 | Given `--defaults`, stdout is the same whatever the root holds: no manifest, a manifest the check rejects, or a `--root` that is not a directory.                                                                                                                                                                                                                                                                                                  | owner, 2026-10-11 ("needs no root or manifest"); decision 0001                                                 | Active |
| B-010 | Given `--defaults`, this Feature exits with code `0` whatever the root holds.                                                                                                                                                                                                                                                                                                                                                                      | owner, 2026-10-11; decision 0001; C-5                                                                          | Active |
| B-011 | Given `--json`, with or without `--defaults`, stdout parses as exactly one JSON document.                                                                                                                                                                                                                                                                                                                                                          | owner, 2026-10-11; decision 0001                                                                               | Active |
| B-012 | Given `--json` and `--defaults`, the document's keys and values equal those of the manifest `specht init --epics` writes into a bare root.                                                                                                                                                                                                                                                                                                         | derived from A-3 and `0001-F4` B-003, B-018; not stated by the owner                                           | Active |
| B-013 | Given `--root` names a path that is not a directory and no `--defaults`, this Feature writes a message naming that path as it was given on stderr, writes nothing on stdout, and exits with code `2`.                                                                                                                                                                                                                                              | `0001-F2` B-005; C-5                                                                                           | Active |
| B-014 | Given a root with no manifest at the manifest path and no `--defaults`, this Feature writes a message naming the manifest path on stderr, writes nothing on stdout, and exits with code `2`.                                                                                                                                                                                                                                                       | `0001-F2` B-006; C-5; C-2                                                                                      | Active |
| B-015 | Given a root whose manifest the check rejects - one that does not parse (`0001-F2` B-007), one `0001-F5` rejects (`0001-F5` B-022), one whose `schemaVersion` the check rejects (`0001-F7` B-003, B-039), or one recording an upstream schema source while explicitly selecting the embedded source (`0001-F7` B-036) - and no `--defaults`, this Feature names on stderr what the check names, writes nothing on stdout, and exits with code `3`. | owner, 2026-10-08 (every command answers a rejected manifest with `3`); `0001-F2` decision 0003; C-5           | Active |
| B-016 | Given any run, stdout carries only the manifest printed, and every message about the tool itself goes to stderr.                                                                                                                                                                                                                                                                                                                                   | `0001-F2` B-009, C-3; C-6                                                                                      | Active |
| B-017 | Given any run, no file is created, modified or deleted, under the root or anywhere else.                                                                                                                                                                                                                                                                                                                                                           | brief § 9; C-1                                                                                                 | Active |
| B-018 | Given any output on stdout or stderr, every path the tool derives is relative to the root with `/` separators, and the only path that may be absolute is one typed on the command line, echoed as given.                                                                                                                                                                                                                                           | brief § 9; `0001-F2` B-013                                                                                     | Active |
| B-019 | Given the same root, manifest and options, two runs print the same stdout and exit with the same code.                                                                                                                                                                                                                                                                                                                                             | brief § 9; C-3                                                                                                 | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-11 -->

| ID  | Constraint                                                                                                                    | Rules Out                                                                                                                                                                    |
| --- | ----------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| C-1 | The command is read-only: it writes nothing, anywhere.                                                                        | An `--output <path>`; writing the filled manifest back into the root; a `--fix`; creating a manifest where none exists.                                                      |
| C-2 | The effective manifest is the one the check's loader produced: the same reader and the same defaults give both.               | A second manifest reader or a second table of defaults in the command; an embedded default used when the root has no manifest; a value printed that the check does not read. |
| C-3 | Deterministic and offline: the output is a function of the manifest, the options and the tool version.                        | A network call, a fetch of a recorded upstream schema source included (`0001-F7` C-8); a clock, a locale or a machine name in the output.                                    |
| C-4 | A command parses, calls the engine and folds the result into an exit code; nothing else lives in the command (`0001-F2` C-4). | Filling a default or ordering keys inside `Features/Manifest/`.                                                                                                              |
| C-5 | Exit codes are `0001-F2` C-2's constants, with the meanings the check gives them.                                             | A sixth exit code; a code that means one thing to this command and another to the check.                                                                                     |
| C-6 | stdout carries the product only, as plain text (`0001-F2` C-3).                                                               | A banner, a heading, colour markup or a note about a filled key on stdout.                                                                                                   |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-11, 2026-10-11 (row 8, `0001-F2` decision 0010) -->

| #   | Item                                                                  | Exclusion Reason                                                                                                                                                                                                                                                                                    |
| --- | --------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | Editing the manifest: a rule's level, or any other key                | `0001-F11` sets one rule's level; `schemaVersion` is the pin command's (`0001-F7` B-043); every other key is a hand edit. C-1.                                                                                                                                                                      |
| 2   | Listing the rules with their family and default severity              | `0001-F10`. This command prints the rule settings as the manifest holds them and nothing about a rule.                                                                                                                                                                                              |
| 3   | Printing the frontmatter schemas or the templates                     | `init` writes them as files (`0001-F4`); whether they need a printing command is epic `0132`'s open question and the owner's.                                                                                                                                                                       |
| 4   | Printing the report schema                                            | `0001-F3` A-1: it is a published file, the tool's contract.                                                                                                                                                                                                                                         |
| 5   | A diff of the manifest against the default                            | Not asked for; a caller compares the `--json` outputs of the two forms.                                                                                                                                                                                                                             |
| 6   | Checking the tree, or any rule                                        | `0001-F2`. The command stops at the loaded manifest.                                                                                                                                                                                                                                                |
| 7   | The default manifest of a version other than the newest               | Not decided (OQ-3).                                                                                                                                                                                                                                                                                 |
| 8   | A `--version` option that prints the schema version the manifest pins | Considered by the owner and not chosen, 2026-10-11 (`0001-F2` decision 0010): the pinned version is marked in `specht schema list` (`0001-F7` B-084), and the effective manifest carries `schemaVersion` (B-006). A boolean `--version` would also take the name OQ-3 may want for a version value. |

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

| Claim ID | Scenario                                                 | Test    | Status  |
| -------- | -------------------------------------------------------- | ------- | ------- |
| B-001    | The effective manifest is printed                        | Missing | Missing |
| B-002    | Printing the effective manifest succeeds                 | Missing | Missing |
| B-003    | A declared value is printed as declared                  | Missing | Missing |
| B-004    | A value left out is printed at its default               | Missing | Missing |
| B-005    | A rule with no level is printed at its own severity      | Missing | Missing |
| B-006    | A manifest without a version is printed as version 0.1.0 | Missing | Missing |
| B-007    | The effective manifest gives the check's verdict         | Missing | Missing |
| B-008    | The default manifest is printed                          | Missing | Missing |
| B-009    | The default manifest does not depend on the root         | Missing | Missing |
| B-010    | Printing the default manifest succeeds without a root    | Missing | Missing |
| B-011    | The JSON form is one document                            | Missing | Missing |
| B-012    | The default manifest is the one init writes              | Missing | Missing |
| B-013    | A missing root is a missing-input failure                | Missing | Missing |
| B-014    | A root without a manifest is a missing-input failure     | Missing | Missing |
| B-015    | A rejected manifest is an invalid-manifest failure       | Missing | Missing |
| B-016    | Messages about the tool go to standard error             | Missing | Missing |
| B-017    | A run writes nothing                                     | Missing | Missing |
| B-018    | No derived path in the output is absolute                | Missing | Missing |
| B-019    | The same input gives the same output                     | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-11, 2026-10-11 (§ 5 row 8) -->

- 2026-10-11, § 5 row 8 added: the owner considered a `--version` option on this command for the pinned schema version and did not choose it (`0001-F2` decision 0010). No claim, constraint or scenario changed.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-11 -->

| ID   | Question                                                                                                                                                                                                                                                                                                                                                                                                                                                                            | Blocks                       | Resolution                                                                                                                |
| ---- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------- | ------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | What does the output look like without `--json`, and in what order are the keys printed, in either form? The manifest is itself JSON, so the text form may be that JSON, a key-per-line listing, or something else; the order may be the on-disk manifest's, the default manifest's, or alphabetical. Owner not asked.                                                                                                                                                              | B-001, B-008 (the form only) | Open. No claim fixes the text form or a key order; B-011 fixes only that `--json` is one document.                        |
| OQ-2 | The owner's decision reads "every omitted key filled from the pinned version's default" and "what the engine reads". They part on a key whose absence is itself a setting: the epic grammar (`0001-F5` C-10, decision 0004), which the default declares and the engine does not fill; what declares work-item files, which is never filled from a default (`0101-F8` C-12). Does the effective manifest leave such a key out, as the engine reads it, or print the default's value? | B-004, B-007                 | Open. B-004 claims only the keys `0001-F5` B-019 fills, and B-007 is written on "what the engine reads". Owner not asked. |
| OQ-3 | Does `--defaults` take a version, so the default manifest of an older shipped version can be printed, and beside a manifest does it follow that manifest's pin?                                                                                                                                                                                                                                                                                                                     | B-008, B-012; § 5 row 7      | Open. Proceeds on the default: the newest version the tool ships, and no root read (B-009). Owner not asked.              |
| OQ-4 | Does the JSON document of this command get a published schema, as the report has (`0001-F3` C-1), and is it bound by an additive-change rule as `0001-F3` C-4 binds the report? Epic `0132` would read it.                                                                                                                                                                                                                                                                          | -                            | Open. No claim is written. Owner not asked.                                                                               |
| OQ-5 | Does the effective manifest say which values came from the manifest and which from the default?                                                                                                                                                                                                                                                                                                                                                                                     | -                            | Open. No claim is written; C-6 rules out a note on stdout until it is answered. Owner not asked.                          |

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
