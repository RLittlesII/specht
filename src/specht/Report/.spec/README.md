---
title: "Specification: The report contract"
description: "The agent-facing output: the --json and --report document with a published JSON Schema, every violation carrying what the rule expected, and --explain printing a rule's full text, so a document is repaired from the report alone"
type: feature
id: "F3"
epic: "0001"
spec_status: in-review
status: ready-for-architecture
priority: high
value: 0
risk: 0
rank: 0
scored_by: null
scored_on: null
domain: "Specification governance"
author: "spec-author"
milestone: null
children: []
depends_on: ["F2"]
blocks: ["F7"]
spikes: []
created: "2026-10-07"
updated: "2026-10-08"
github_issue: null
synced_at: null
---

# Specification: The report contract

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-07 -->

A violation today says what is wrong and not what would be right, so the agent that reads it opens the rule catalogue, the template or the schema before it can repair the document, and a tree written before the schema existed fails without saying what it is missing. This Feature removes that failure state: the report is one JSON document with a published schema, every violation carries what the rule expected, and `--explain` prints a rule's full text, so an agent repairs a document, and migrates it between schema versions, from the report alone (Must-6).

## 2. User Needs

<!-- last written by: spec-author, 2026-10-07 -->

| #   | Persona                                  | Need                                                                                             | Pain Point Today                                                                         |
| --- | ---------------------------------------- | ------------------------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------- |
| 1   | The agent repairing a specification      | Each violation says what the rule wanted - the headers, the grammar, the section order - in data | A message in prose; the expectation lives in the rule's code, the template or the schema |
| 2   | The agent migrating a pre-schema tree    | The same, on Transporter's tree, for every rule at once                                          | Nothing says what a pre-schema document is missing, only that it fails                   |
| 3   | A CI step or a script parsing the result | One JSON document on stdout, or at a path, that validates against a schema it can pin            | `hooked` writes an anonymous object; its shape is whatever the code says this week       |
| 4   | The maintainer reading a rule            | The rule's full text from the tool that enforces it, for the version pinned                      | Prose in a skill file, in another repository                                             |
| 5   | `0001-F7`                                | A report that names the schema version it checked against and where the schemas came from        | N/A (internal dependency)                                                                |

### Assumptions

| ID  | Assumption                                                                                                                                                                                                                    |
| --- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | The report's JSON Schema is published at `docs/schema/report.schema.json`. It is the tool's contract, versioned with the tool, so it lives outside `.spec/schema/` and `init` never writes it. The folder name is reversible. |
| A-2 | The exit code for `--explain` with a rule id outside the pinned version's vocabulary is `4`, not found (OQ-1, resolved 2026-10-08; `0001-F2` decision 0003). The proposed default `2` was not chosen.                         |
| A-3 | `--explain` is an option on the check command, as README § 5 writes it (`specht --explain SPEC031`); it prints and exits without running a check.                                                                             |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-08 -->

| ID    | Claim                                                                                                                                                                                                                                                                                                                                                                                          | Source                                             | Status    |
| ----- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------- | --------- |
| B-001 | Given `--json`, this Feature writes the report document to stdout in place of the violation lines and the summary, so stdout parses as one JSON document.                                                                                                                                                                                                                                      | README § 5                                         | Active    |
| B-002 | Given `--report <path>`, this Feature writes the same document to that path, creating the directories above it, and stdout keeps the line stream.                                                                                                                                                                                                                                              | README § 5; split into B-022-B-025 (decision 0002) | Withdrawn |
| B-003 | Given both `--json` and `--report`, the document on stdout and the document at the path are identical.                                                                                                                                                                                                                                                                                         | README § 5                                         | Active    |
| B-004 | Given `--json` or `--report`, the exit code is the one the same run gives without them.                                                                                                                                                                                                                                                                                                        | `dotnet-tool` § Exit codes                         | Active    |
| B-005 | Given the document, it carries the schema version checked against, whether the schemas came from the embedded set, from the on-disk files, or from the on-disk copy of a recorded upstream source (named with its recorded source and version), each layout's name and specification count, the item count, the count of rule ids evaluated, the error and warning counts, and the violations. | README § 6; Should-8; `0001-F7` decision 0004      | Amended   |
| B-006 | Given the document, it carries no timestamp and no field derived from the clock, the machine or the environment.                                                                                                                                                                                                                                                                               | decision 0001                                      | Active    |
| B-007 | Given a violation in the document, it carries the rule id, the severity, the file, the line, the identifier where the rule has one, the message, and `expected`.                                                                                                                                                                                                                               | README § 6; Must-6                                 | Active    |
| B-008 | Given any document the tool writes, it validates against the report schema published in this repository, and a test proves it.                                                                                                                                                                                                                                                                 | README § 6; C-1                                    | Active    |
| B-009 | Given a frontmatter schema violation (`SPEC002`-`SPEC004`), `expected` names the schema file, the key at fault and the schema's constraint on that key - the required list, the enum, the pattern or the format.                                                                                                                                                                               | Must-6                                             | Active    |
| B-010 | Given a section violation (`SPEC010`), `expected` carries the ordered list of section titles of the pinned schema version.                                                                                                                                                                                                                                                                     | Must-6                                             | Active    |
| B-011 | Given a table violation (`SPEC013`), `expected` carries the ordered header list for that section.                                                                                                                                                                                                                                                                                              | Must-6                                             | Active    |
| B-012 | Given an identity violation (`SPEC011`, `SPEC012`, `SPEC043`), `expected` carries the identity form and the value the path, the parent or the first declaration implies.                                                                                                                                                                                                                       | Must-6                                             | Active    |
| B-013 | Given a companion violation (`SPEC020`, `SPEC021`), `expected` carries the companion count the rule requires or the claim ids the specification declares.                                                                                                                                                                                                                                      | Must-6                                             | Active    |
| B-014 | Given a claim violation (`SPEC030`), `expected` carries the claim grammar and, for a duplicate, the line of the first declaration.                                                                                                                                                                                                                                                             | Must-6                                             | Active    |
| B-015 | Given a matrix violation (`SPEC031`), `expected` carries the claim ids that need exactly one row and the number of rows each has.                                                                                                                                                                                                                                                              | Must-6                                             | Active    |
| B-016 | Given a child-item violation (`SPEC040`, `SPEC041`, `SPEC044`), `expected` carries the file-name shape the item must have and, for a gap, the next number in the epic's sequence.                                                                                                                                                                                                              | Must-6                                             | Active    |
| B-017 | Given an edge or cycle violation (`SPEC051`, `SPEC052`), `expected` carries the edge the other Feature must declare, or the cycle as the ordered list of identities.                                                                                                                                                                                                                           | Must-6                                             | Active    |
| B-018 | Given an approval violation (`SPEC060`, `SPEC061`), `expected` carries the cell or sign-off row that must change before the approved status is honest.                                                                                                                                                                                                                                         | Must-6                                             | Active    |
| B-019 | Given a manifest that pins a schema version the tool ships and `--explain <SPEC###>` with an id in that version's vocabulary, this Feature prints on stdout the rule's full text - what it checks, what it expects, and the schema version it belongs to - and exits with code `0` without running a check.                                                                                    | README § 5; A-3                                    | Active    |
| B-020 | Given a manifest that pins a schema version the tool ships and `--explain` with an id outside that version's vocabulary, this Feature names the id on stderr, writes nothing on stdout, and exits with code `4`.                                                                                                                                                                               | README § 5; `0001-F2` decision 0003 (OQ-1)         | Active    |
| B-021 | Given the document, every path in it is relative to the root with `/` separators.                                                                                                                                                                                                                                                                                                              | README § 9                                         | Active    |
| B-022 | Given `--report <path>`, this Feature writes the report document to that path.                                                                                                                                                                                                                                                                                                                 | README § 5; decision 0002                          | Active    |
| B-023 | Given `--report <path>` whose directories do not exist, this Feature creates them.                                                                                                                                                                                                                                                                                                             | README § 5; decision 0002                          | Active    |
| B-024 | Given `--report <path>` naming a file that exists, this Feature replaces its content with this run's document.                                                                                                                                                                                                                                                                                 | decision 0002                                      | Active    |
| B-025 | Given `--report <path>` without `--json`, stdout carries the same violation lines and summary the run prints without `--report`.                                                                                                                                                                                                                                                               | README § 5                                         | Active    |
| B-026 | Given a missing-frontmatter violation (`SPEC001`), `expected` names the schema file for that document's kind and that schema's required key list.                                                                                                                                                                                                                                              | Must-6                                             | Active    |
| B-027 | Given a dependency that names no Feature (`SPEC050`), `expected` carries the Feature identities the dependency could name.                                                                                                                                                                                                                                                                     | Must-6                                             | Active    |
| B-028 | Given a reused item id (`SPEC044`), `expected` carries the file of the item that first holds the id.                                                                                                                                                                                                                                                                                           | Must-6                                             | Active    |
| B-029 | Given `--explain` under a root with no manifest, this Feature prints no rule text, writes nothing on stdout, and exits with code `2`, as the check does (`0001-F2` B-006).                                                                                                                                                                                                                     | OQ-2                                               | Active    |
| B-030 | Given `--explain` under a root whose manifest the check rejects - one that does not parse (`0001-F2` B-007), one `0001-F5` rejects (`0001-F5` B-022), or one pinning a version the tool does not ship (`0001-F7` B-003) - this Feature prints no rule text, writes nothing on stdout, and exits with code `3`, as the check does.                                                              | OQ-2; `0001-F2` decision 0003                      | Active    |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-07 -->

| ID  | Constraint                                                                                                                                     | Rules Out                                                                                               |
| --- | ---------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------- |
| C-1 | The report's shape is written once, as a JSON Schema in this repository, and a test validates a produced report against it.                    | An anonymous object as the only definition; a field the schema does not name.                           |
| C-2 | `expected` is structured data an agent can act on without the catalogue: an object with named members per rule family, never a sentence alone. | `expected: "the right headers"`; the message repeated under another name.                               |
| C-3 | The document is a function of the tree, the manifest and the tool version.                                                                     | A timestamp; a machine name; an absolute path; a locale-dependent string.                               |
| C-4 | Within a tool major version the document changes only by addition.                                                                             | Renaming or removing a field; changing a field's type.                                                  |
| C-5 | `--json` and `--report` change what is written, never the verdict.                                                                             | A different exit code under `--json`; a violation that appears in one form and not the other.           |
| C-6 | A rule's full text lives once, in the engine, versioned with the schema; `--explain` prints it and README § 4 only summarises it.              | A second catalogue in a skill or a wiki that `--explain` does not read.                                 |
| C-7 | The file `--report` names is the only file a check run writes, and it holds exactly one report document.                                       | A temporary or backup file left beside it; appending a second document; any other write under the root. |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-07 -->

| #   | Item                                                         | Exclusion Reason                                                                    |
| --- | ------------------------------------------------------------ | ----------------------------------------------------------------------------------- |
| 1   | Autofix: applying `expected` to the document                 | README § 2; the agent repairs, the tool never writes a specification.               |
| 2   | The verdict, the line stream, the exit codes                 | `0001-F2`.                                                                          |
| 3   | A second output format (SARIF, HTML, JUnit)                  | One document with one schema; a converter can be written over it.                   |
| 4   | Output from `init` and `upgrade`                             | Each prints its own file list (`0001-F4`, `0001-F7`); neither writes this document. |
| 5   | Changing a rule's text between schema versions               | The text is part of the version (C-6); a changed text is `0001-F7`'s next version.  |
| 6   | Colour, markup or a table rendering of the report            | stdout is plain text (`0001-F2` C-3).                                               |
| 7   | Refusing, appending to or backing up an existing report file | Decision 0002: the report file is replaced on every run.                            |

## 6. Concern Separation

<!-- last written by: spec-author, 2026-10-07 -->

Pending: owned by `implementer`, written after agreement.

## 7. Technical Design

<!-- last written by: spec-author, 2026-10-07 -->

Pending: owned by `implementer`, written after agreement.

## 8. Testing Strategy

<!-- last written by: spec-author, 2026-10-07 -->

Pending: owned by `test-writer`.

## 9. Traceability Matrix

<!-- last written by: spec-author, 2026-10-07 -->

| Claim ID | Scenario                                                                                                                                                                                               | Test    | Status  |
| -------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------- | ------- |
| B-001    | JSON output replaces the diagnostic lines                                                                                                                                                              | Missing | Missing |
| B-002    | A report path receives the document                                                                                                                                                                    | Missing | Missing |
| B-003    | Both outputs are the same document                                                                                                                                                                     | Missing | Missing |
| B-004    | The document form does not change the verdict                                                                                                                                                          | Missing | Missing |
| B-005    | The document carries the counts and the version                                                                                                                                                        | Missing | Missing |
| B-006    | The document carries nothing from the clock; The document carries nothing from the machine                                                                                                             | Missing | Missing |
| B-007    | A violation carries what the rule expected                                                                                                                                                             | Missing | Missing |
| B-008    | Every document validates against the published schema                                                                                                                                                  | Missing | Missing |
| B-009    | A frontmatter violation says what the schema requires                                                                                                                                                  | Missing | Missing |
| B-010    | A section violation says the section order                                                                                                                                                             | Missing | Missing |
| B-011    | A table violation says the headers                                                                                                                                                                     | Missing | Missing |
| B-012    | An identity violation says the identity implied                                                                                                                                                        | Missing | Missing |
| B-013    | A companion violation says what must resolve                                                                                                                                                           | Missing | Missing |
| B-014    | A claim violation says the grammar                                                                                                                                                                     | Missing | Missing |
| B-015    | A matrix violation says which claims need a row                                                                                                                                                        | Missing | Missing |
| B-016    | A child-item violation says the file shape or the next number                                                                                                                                          | Missing | Missing |
| B-017    | A dependency violation says the missing edge or the cycle                                                                                                                                              | Missing | Missing |
| B-018    | An approval violation says what must change first                                                                                                                                                      | Missing | Missing |
| B-019    | Explain prints a rule's full text                                                                                                                                                                      | Missing | Missing |
| B-020    | Explain of an unknown rule names it and fails                                                                                                                                                          | Missing | Missing |
| B-021    | No path in the document is absolute                                                                                                                                                                    | Missing | Missing |
| B-022    | A report path receives the document                                                                                                                                                                    | Missing | Missing |
| B-023    | A report path in a new directory is created                                                                                                                                                            | Missing | Missing |
| B-024    | A report path that exists is replaced                                                                                                                                                                  | Missing | Missing |
| B-025    | A report path keeps the diagnostic lines on the output                                                                                                                                                 | Missing | Missing |
| B-026    | A missing frontmatter says what the schema requires                                                                                                                                                    | Missing | Missing |
| B-027    | A dependency on nothing says what it could name                                                                                                                                                        | Missing | Missing |
| B-028    | A reused item id names its first holder                                                                                                                                                                | Missing | Missing |
| B-029    | Explain without a manifest fails as the check does                                                                                                                                                     | Missing | Missing |
| B-030    | Explain beside a manifest that does not parse fails as the check does; Explain beside a rejected manifest fails as the check does; Explain beside a pin the tool does not ship fails as the check does | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-07 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-07 -->

| ID   | Question                                                                                                                                                                                                                | Blocks               | Resolution                                                                                                                                                                                                                                                                                                                                                                       |
| ---- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | README § 5 fixes four exit codes and none for "the thing named does not exist". A-2 reads an unknown `--explain` id as missing input (`2`). Is that the rule, or does the code set grow a `NotFound`?                   | B-020                | Resolved 2026-10-08 by the repository owner: the code set grows `4`, not found (`0001-F2` decision 0003). Rejected: `2`, A-2's proposed default; `3`. B-020 now names `4`.                                                                                                                                                                                                       |
| OQ-2 | `--explain` reads the pinned schema version from the manifest. With no manifest, or one `0001-F5` rejects, does `--explain` fail as the check does (`0001-F2` B-006, B-007), or print from the newest embedded version? | B-019, B-020 (scope) | Resolved 2026-10-08 by the repository owner: fail as the check does - `2` with no manifest, `3` with a rejected one - and print no rule text. Rejected: printing the newest embedded version's text, which gives a lagging repository the wrong version's rule. Claimed by B-029 (no manifest, `2`) and B-030 (any manifest the check rejects, `3`, including an unshipped pin). |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-08 -->

| Section | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       |
| ------- | ------ | ------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-5     | 🟢     | spec-reviewer | Approved (round 2). Round-1 blocks cleared: B-002 withdrawn into B-022-B-025 under decision 0002 and C-7; B-020 leaves the code to OQ-1; B-026 (`SPEC001`) and B-027 (`SPEC050`) define `expected`; B-005 carries the schema source. Non-blocking: § 9 B-002 row should read Withdrawn, not Missing (test-writer); B-024's scenario also asserts B-004's exit code (spec-author); decision 0001 Affects still describes `0001-F1` B-004 as naming the field (spec-author).                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 |
| 1-5     | 🔴     | spec-reviewer | Round 3 (2026-10-08) - blocked, reviewing commit `e7cbd17`. A-2, B-020 and OQ-1 agree with `0001-F2` decision 0003; OQ-2's resolution is claimed by B-029; § 9 carries B-029 once and both its scenarios are tagged. Blocking: (1) B-029's Given covers only no manifest and a manifest `0001-F5` rejects, so `--explain` beside a manifest that does not parse (`0001-F2` B-007) or that pins a version the tool does not ship (`0001-F7` B-003) is unclaimed, and B-020's Given (an id outside "that version's vocabulary") reads as exit `4` for an unshipped pin, against decision 0003's rule that such a pin is `3` from every command; widen B-029 to every manifest the check rejects with `3` and narrow B-020 to a version the tool ships (spec-author). (2) Both `@B-029` scenarios inherit the Background "a repository root holding a manifest", so "a root directory with no manifest" contradicts its own setup; give them a setup outside that Background (spec-author). Non-blocking: B-029 joins two Givens with two codes where `0001-F7` B-019/B-020 split the same pair; A-2 now restates B-020 and is no longer an assumption; the § 2, § 3, § 9 and § 11 stamps still read 2026-10-07. `spec_status: in-review` is not honest while this row is 🔴. |
| 1-5     | 🟢     | spec-reviewer | Round 4 (2026-10-08) - approved, reviewing commit `d7ce118`. Round-3 blockers cleared: B-019 and B-020 are scoped to a version the tool ships, so `4` never meets an unshipped pin; B-029 claims no manifest (`2`) and B-030 claims every manifest the check rejects with `3` (`0001-F2` B-007, `0001-F5` B-022, `0001-F7` B-003), agreeing with `0001-F2` decision 0003, `0001-F7` B-021 and `0001-F4` B-013; the `@B-029` and `@B-030` scenarios delete or replace the Background's manifest and match their claims; § 9 carries both ids once; OQ-2's resolution names both. The plain `specht` line in the conventions keeps `0/1/2/3` correctly, since the check never returns `4`. Non-blocking (spec-author): A-2 restates B-020 and is no longer an assumption; the § 2, § 3, § 9 and § 11 stamps still read 2026-10-07.                                                                                                                                                                                                                                                                                                                                                                                                                                           |

## Tasks

Cut 2026-10-08 into [`../.issue/`](../.issue/): `0033` (the Feature), with `0034` to `0040`.

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
