---
title: "Specification: The report contract"
description: "The agent-facing output: the --json and --report document with a published JSON Schema, every violation carrying what the rule expected, and --explain printing a rule's full text, so a document is repaired from the report alone"
type: feature
id: "F3"
epic: "0001"
spec_status: draft
status: needs-decomposition
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
blocks: []
spikes: []
created: "2026-10-07"
updated: "2026-10-07"
github_issue: null
synced_at: null
---

# Specification: The report contract

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-07 -->

A violation today says what is wrong and not what would be right, so the agent that reads it opens the rule catalogue, the template or the schema before it can repair the document, and a tree written before the schema existed fails without saying what it is missing. This Feature removes that failure state: the report is one JSON document with a published schema, every violation carries what the rule expected, and `--explain` prints a rule's full text, so an agent repairs a document, and migrates it between schema versions, from the report alone (Must-6).

## 2. User Needs

<!-- last written by: spec-author, 2026-10-07 -->

| #   | Persona                                       | Need                                                                                                  | Pain Point Today                                                                          |
| --- | --------------------------------------------- | ----------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------- |
| 1   | The agent repairing a specification           | Each violation says what the rule wanted - the headers, the grammar, the section order - in data       | A message in prose; the expectation lives in the rule's code, the template or the schema  |
| 2   | The agent migrating a pre-schema tree         | The same, on Transporter's tree, for every rule at once                                               | Nothing says what a pre-schema document is missing, only that it fails                     |
| 3   | A CI step or a script parsing the result      | One JSON document on stdout, or at a path, that validates against a schema it can pin                  | `hooked` writes an anonymous object; its shape is whatever the code says this week          |
| 4   | The maintainer reading a rule                 | The rule's full text from the tool that enforces it, for the version pinned                           | Prose in a skill file, in another repository                                                |
| 5   | `0001-F7`                                     | A report that names the schema version it checked against and where the schemas came from              | N/A (internal dependency)                                                                 |

### Assumptions

| ID  | Assumption                                                                                                                                                                                                                       |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | The report's JSON Schema is published at `docs/schema/report.schema.json`. It is the tool's contract, versioned with the tool, so it lives outside `.spec/schema/` and `init` never writes it. The folder name is reversible.    |
| A-2 | `--explain` with a rule id outside the pinned version's vocabulary is a missing-input failure, like a missing root: exit code `2`. See OQ-1.                                                                                     |
| A-3 | `--explain` is an option on the check command, as README § 5 writes it (`specht --explain SPEC031`); it prints and exits without running a check.                                                                                 |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-07 -->

| ID    | Claim                                                                                                                                                                                                                                      | Source                       | Status |
| ----- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------- | ------ |
| B-001 | Given `--json`, this Feature writes the report document to stdout in place of the violation lines and the summary, so stdout parses as one JSON document.                                                                                 | README § 5                   | Active |
| B-002 | Given `--report <path>`, this Feature writes the same document to that path, creating the directories above it, and stdout keeps the line stream.                                                                                         | README § 5                   | Active |
| B-003 | Given both `--json` and `--report`, the document on stdout and the document at the path are identical.                                                                                                                                     | README § 5                   | Active |
| B-004 | Given `--json` or `--report`, the exit code is the one the same run gives without them.                                                                                                                                                   | `dotnet-tool` § Exit codes   | Active |
| B-005 | Given the document, it carries the schema version checked against, each layout's name and specification count, the item count, the count of rules evaluated, the error and warning counts, and the violations.                            | README § 6                   | Active |
| B-006 | Given the document, it carries no timestamp and no field derived from the clock, the machine or the environment.                                                                                                                          | decision 0001                | Active |
| B-007 | Given a violation in the document, it carries the rule id, the severity, the file, the line, the identifier where the rule has one, the message, and `expected`.                                                                          | README § 6; Must-6           | Active |
| B-008 | Given any document the tool writes, it validates against the report schema published in this repository, and a test proves it.                                                                                                             | README § 6; C-1              | Active |
| B-009 | Given a frontmatter violation (`SPEC001`-`SPEC004`), `expected` names the schema file, the key at fault and the schema's constraint on that key - the required list, the enum, the pattern or the format.                                  | Must-6                       | Active |
| B-010 | Given a section violation (`SPEC010`), `expected` carries the ordered list of section titles of the pinned schema version.                                                                                                                | Must-6                       | Active |
| B-011 | Given a table violation (`SPEC013`), `expected` carries the ordered header list for that section.                                                                                                                                         | Must-6                       | Active |
| B-012 | Given an identity violation (`SPEC011`, `SPEC012`, `SPEC043`), `expected` carries the identity form and the value the path, the parent or the first declaration implies.                                                                  | Must-6                       | Active |
| B-013 | Given a companion violation (`SPEC020`, `SPEC021`), `expected` carries the companion count the rule requires or the claim ids the specification declares.                                                                                | Must-6                       | Active |
| B-014 | Given a claim violation (`SPEC030`), `expected` carries the claim grammar and, for a duplicate, the line of the first declaration.                                                                                                        | Must-6                       | Active |
| B-015 | Given a matrix violation (`SPEC031`), `expected` carries the claim ids that need exactly one row and the number of rows each has.                                                                                                         | Must-6                       | Active |
| B-016 | Given a child-item violation (`SPEC040`, `SPEC041`, `SPEC044`), `expected` carries the file-name shape the item must have and, for a gap, the next number in the epic's sequence.                                                          | Must-6                       | Active |
| B-017 | Given a dependency violation (`SPEC050`-`SPEC052`), `expected` carries the edge the other Feature must declare, or the cycle as the ordered list of identities.                                                                            | Must-6                       | Active |
| B-018 | Given an approval violation (`SPEC060`, `SPEC061`), `expected` carries the cell or sign-off row that must change before the approved status is honest.                                                                                    | Must-6                       | Active |
| B-019 | Given `--explain <SPEC###>` with an id in the pinned version's vocabulary, this Feature prints on stdout the rule's full text - what it checks, what it expects, and the schema version it belongs to - and exits with code `0` without running a check. | README § 5; A-3      | Active |
| B-020 | Given `--explain` with an id outside the pinned version's vocabulary, this Feature names the id on stderr, writes nothing on stdout, and exits with code `2`.                                                                             | A-2; OQ-1                    | Active |
| B-021 | Given the document, every path in it is relative to the root with `/` separators.                                                                                                                                                         | README § 9                   | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-07 -->

| ID   | Constraint                                                                                                                                                | Rules Out                                                                                               |
| ---- | --------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------- |
| C-1  | The report's shape is written once, as a JSON Schema in this repository, and a test validates a produced report against it.                              | An anonymous object as the only definition; a field the schema does not name.                            |
| C-2  | `expected` is structured data an agent can act on without the catalogue: an object with named members per rule family, never a sentence alone.           | `expected: "the right headers"`; the message repeated under another name.                                |
| C-3  | The document is a function of the tree, the manifest and the tool version.                                                                               | A timestamp; a machine name; an absolute path; a locale-dependent string.                                |
| C-4  | Within a tool major version the document changes only by addition.                                                                                        | Renaming or removing a field; changing a field's type.                                                   |
| C-5  | `--json` and `--report` change what is written, never the verdict.                                                                                        | A different exit code under `--json`; a violation that appears in one form and not the other.             |
| C-6  | A rule's full text lives once, in the engine, versioned with the schema; `--explain` prints it and README § 4 only summarises it.                          | A second catalogue in a skill or a wiki that `--explain` does not read.                                   |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-07 -->

| #   | Item                                                                     | Exclusion Reason                                                                                     |
| --- | ------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------- |
| 1   | Autofix: applying `expected` to the document                             | README § 2; the agent repairs, the tool never writes a specification.                                 |
| 2   | The verdict, the line stream, the exit codes                              | `0001-F2`.                                                                                           |
| 3   | A second output format (SARIF, HTML, JUnit)                               | One document with one schema; a converter can be written over it.                                     |
| 4   | Output from `init` and `upgrade`                                          | Each prints its own file list (`0001-F4`, `0001-F7`); neither writes this document.                    |
| 5   | Changing a rule's text between schema versions                            | The text is part of the version (C-6); a changed text is `0001-F7`'s next version.                    |
| 6   | Colour, markup or a table rendering of the report                          | stdout is plain text (`0001-F2` C-3).                                                                 |

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

| Claim ID | Scenario                                                             | Test    | Status  |
| -------- | -------------------------------------------------------------------- | ------- | ------- |
| B-001    | JSON output replaces the diagnostic lines                            | Missing | Missing |
| B-002    | A report path receives the document                                  | Missing | Missing |
| B-003    | Both outputs are the same document                                   | Missing | Missing |
| B-004    | The document form does not change the verdict                        | Missing | Missing |
| B-005    | The document carries the counts and the version                      | Missing | Missing |
| B-006    | The document carries nothing from the clock                          | Missing | Missing |
| B-007    | A violation carries what the rule expected                           | Missing | Missing |
| B-008    | Every document validates against the published schema                | Missing | Missing |
| B-009    | A frontmatter violation says what the schema requires                | Missing | Missing |
| B-010    | A section violation says the section order                           | Missing | Missing |
| B-011    | A table violation says the headers                                   | Missing | Missing |
| B-012    | An identity violation says the identity implied                      | Missing | Missing |
| B-013    | A companion violation says what must resolve                         | Missing | Missing |
| B-014    | A claim violation says the grammar                                   | Missing | Missing |
| B-015    | A matrix violation says which claims need a row                      | Missing | Missing |
| B-016    | A child-item violation says the file shape or the next number        | Missing | Missing |
| B-017    | A dependency violation says the missing edge or the cycle            | Missing | Missing |
| B-018    | An approval violation says what must change first                    | Missing | Missing |
| B-019    | Explain prints a rule's full text                                    | Missing | Missing |
| B-020    | Explain of an unknown rule is a missing-input failure                | Missing | Missing |
| B-021    | No path in the document is absolute                                  | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-07 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-07 -->

| ID   | Question                                                                                                                                                                                                          | Blocks | Resolution |
| ---- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------ | ---------- |
| OQ-1 | README § 5 fixes four exit codes and none for "the thing named does not exist". A-2 reads an unknown `--explain` id as missing input (`2`). Is that the rule, or does the code set grow a `NotFound`?             | B-020  | Open       |

## 12. Sign-off

<!-- last written by: spec-author, 2026-10-07 -->

| Section | Status | Reviewer      | Note                  |
| ------- | ------ | ------------- | --------------------- |
| 1-5     | 🟡     | spec-reviewer | Awaiting first review |

## Tasks

None yet. Cut from § 3 after agreement.

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
