---
title: "Specification: Heading and table form"
description: "Schema version 2 rules that warn, report-only, when a contracted heading's source line is not exactly the manifest's title, when a pipe table lacks its delimiter row or ends a row in empty cells, or when an id table's first cell is not exactly one id"
type: feature
id: "F3"
epic: "0101"
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
depends_on: ["0001/F5", "0001/F7"]
blocks: []
spikes: []
created: "2026-10-08"
updated: "2026-10-08"
github_issue: null
synced_at: null
---

# Specification: Heading and table form

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-08 -->

Version 1 reads a specification through Markdig, which forgives what a reader and a diff do not: a heading wrapped in emphasis reads as the contracted title, a row padded with empty trailing cells reads as the same row, and a § 4 or § 11 row whose first cell is not an id is read by no rule at all. The document passes and still drifts from the form every other specification has. This Feature removes that failure state for the cases version 1 leaves unchecked: in schema version 2 the check warns, report-only, where a heading's or a table's written form departs from the contract.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-08 -->

| #   | Persona                             | Need                                                                    | Pain Point Today                                              |
| --- | ----------------------------------- | ----------------------------------------------------------------------- | ------------------------------------------------------------- |
| 1   | The reader of a specification       | Every heading and table written the same way in every specification     | Markup and stray cells pass the check and differ file to file |
| 2   | The agent authoring a specification | Be told which line departs from the form and what it should be (Must-5) | Only a reviewer notices                                       |
| 3   | Whoever cites a constraint          | A § 4 and § 11 whose every row carries exactly one id                   | No rule reads § 4 or § 11, so a malformed id passes           |

### Assumptions

| ID  | Assumption                                                                                                                                                                                                                                   |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | A warning is printed and counted, and fails the run only under `--strict`; a disabled rule reports nothing (`0001-F5` B-010, B-011).                                                                                                         |
| A-2 | Version 1 already compares a contracted heading's text to the manifest, ordinal, after Markdig drops its markup and trims it (`SectionStructureRule`, `SpecDocument`): a misspelt, renumbered or wrong-level heading is `SPEC010` "missing". |
| A-3 | Version 1 already reports a § 3 first cell that breaks the claim grammar or repeats (`SPEC030`) and a § 9 first cell naming no claim (`SPEC031`); both skip a row whose first cell is empty (`ClaimRule`).                                   |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-08 -->

| ID    | Claim                                                                                                                                                                                                                                                                                | Source                       | Status |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ---------------------------- | ------ |
| B-001 | Given a contracted heading whose text matches the manifest but whose source line is not exactly `## ` followed by that title - emphasis, inline code, a closing `#` run, or extra spaces - this Feature reports a warning on that line naming the heading as the manifest writes it. | `0101-F1` decision 0002; A-2 | Active |
| B-002 | Given § 3, § 4, § 9 or § 11 written as pipe rows with no delimiter row beneath the header, this Feature reports a warning on that section's heading line that its table has no delimiter row.                                                                                        | `0101-F1` decision 0002      | Active |
| B-003 | Given a pipe-table body row that ends in one or more empty cells beyond the header's column count, this Feature reports a warning on that row's line.                                                                                                                                | `0101-F1` decision 0002      | Active |
| B-004 | Given a § 4 row whose first cell is not exactly one id matching the manifest's constraint grammar, this Feature reports a warning on that row's line.                                                                                                                                | `0101-F1` decision 0002; A-3 | Active |
| B-005 | Given a § 11 row whose first cell is not exactly one id matching the manifest's open-question grammar, this Feature reports a warning on that row's line.                                                                                                                            | `0101-F1` decision 0002; A-3 | Active |
| B-006 | Given a § 3 or § 9 row whose first cell is empty, this Feature reports a warning on that row's line.                                                                                                                                                                                 | A-3                          | Active |
| B-007 | Given a manifest pinning schema version 2 that sets no severity for this Feature's rules, their violations are warnings.                                                                                                                                                             | `0101-F1` decision 0001      | Active |
| B-008 | Given a manifest pinning schema version 2 that disables this Feature's rules, this Feature reports nothing.                                                                                                                                                                          | `0101-F1` decision 0001      | Active |
| B-009 | Given a manifest pinning schema version 1, this Feature reports nothing.                                                                                                                                                                                                             | `0101-F1` decision 0001; C-4 | Active |
| B-010 | Given a contracted heading whose text is misspelt, this Feature reports nothing for it, leaving the one finding to `SPEC010`.                                                                                                                                                        | A-2; C-2                     | Active |
| B-011 | Given a § 3 row whose first cell holds two claim ids, this Feature reports nothing for it, leaving the one finding to `SPEC030`.                                                                                                                                                     | A-3; C-2                     | Active |
| B-012 | Given a pipe table whose columns are not padded to one width, this Feature reports nothing.                                                                                                                                                                                          | § 5 row 2                    | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-08 -->

| ID  | Constraint                                                                                                                          | Rules Out                                                                                                      |
| --- | ----------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------- |
| C-1 | Every finding of this Feature is report-only: no command of the tool rewrites a heading or a cell to clear it.                      | `format` editing a heading, a cell or a delimiter row (`0101-F5` decision 0001); a `--fix` on the check.       |
| C-2 | A fault a version 1 rule already reports is not reported again here.                                                                | Two findings for one misspelt heading; a second id-grammar check on § 3 or § 9 beside `SPEC030` and `SPEC031`. |
| C-3 | Tables are read from Markdig's pipe-table model, and the raw line only where the model cannot say, such as a missing delimiter row. | A second Markdown parser; splitting every line on `\|` (`0001-F1` C-6).                                        |
| C-4 | The rules belong to schema version 2 and later, at warning severity by default.                                                     | Reporting under version 1; error by default (`0101-F1` decision 0001).                                         |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-08 -->

| #   | Item                                                                    | Exclusion Reason                                                     |
| --- | ----------------------------------------------------------------------- | -------------------------------------------------------------------- |
| 1   | A heading misspelt, renumbered, missing, repeated or at the wrong level | `SPEC010` (A-2; B-010).                                              |
| 2   | Cell padding and column alignment                                       | The repository's Markdown formatter; not a structural fault (B-012). |
| 3   | A § 3 or § 9 first cell that breaks its grammar or repeats              | `SPEC030`, `SPEC031` (A-3; B-011).                                   |
| 4   | A contracted table with the wrong headers                               | `SPEC013`; its headers per role are `0001-F5` B-002.                 |
| 5   | Fixing any finding of this Feature                                      | Report-only (C-1).                                                   |
| 6   | The order of rows, and missing ids                                      | `0101-F1`, `0101-F2`.                                                |
| 7   | The rule ids                                                            | `0101-F1` OQ-1.                                                      |

## 6. Concern Separation

<!-- last written by: spec-author, 2026-10-08 -->

Pending: owned by `implementer`, written after agreement.

## 7. Technical Design

<!-- last written by: spec-author, 2026-10-08 -->

Pending: owned by `implementer`, written after agreement.

## 8. Testing Strategy

<!-- last written by: spec-author, 2026-10-08 -->

Pending: owned by `test-writer`, written after agreement.

## 9. Traceability Matrix

<!-- last written by: spec-author, 2026-10-08 -->

| Claim ID | Scenario                                                   | Test    | Status  |
| -------- | ---------------------------------------------------------- | ------- | ------- |
| B-001    | A contracted heading carrying markup is reported           | Missing | Missing |
| B-002    | A table with no delimiter row is reported                  | Missing | Missing |
| B-003    | A row ending in empty cells is reported                    | Missing | Missing |
| B-004    | A constraint row without exactly one id is reported        | Missing | Missing |
| B-005    | An open-question row without exactly one id is reported    | Missing | Missing |
| B-006    | A claim or matrix row with an empty first cell is reported | Missing | Missing |
| B-007    | Form findings are warnings by default                      | Missing | Missing |
| B-008    | Disabled form rules report nothing                         | Missing | Missing |
| B-009    | A version 1 repository is not checked for form             | Missing | Missing |
| B-010    | A misspelt heading is left to the section rule             | Missing | Missing |
| B-011    | A claim cell with two ids is left to the claim rule        | Missing | Missing |
| B-012    | Column padding is not checked                              | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-08 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-08 -->

| ID   | Question                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        | Blocks | Resolution |
| ---- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------ | ---------- |
| OQ-1 | Is the heading half of this Feature still wanted? Reading `SectionStructureRule` and `SpecDocument`: `SPEC010` already requires each contracted heading's text to equal the manifest's title exactly (ordinal), after Markdig drops emphasis and code markup and trims the ends. A misspelt, renumbered or `###` heading is already reported, as "missing". What is left is B-001 - markup, a closing `#` run, or extra spaces that the flattening hides. If that is not worth a rule, B-001 is withdrawn and the Feature becomes "Table form". | B-001  | Open       |
| OQ-2 | How wide is the row-shape check? B-003 covers a row with empty cells beyond the header's count, as decided. Is a row with fewer cells than the header, or with non-empty extra cells, also a finding - and does the check cover every pipe table in a specification, or only § 3, § 4, § 9 and § 11?                                                                                                                                                                                                                                            | B-003  | Open       |

## 12. Sign-off

<!-- last written by: spec-author, 2026-10-08 -->

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
