---
title: "Specification: Row ordering rules"
description: "Schema version 2 rules that warn when the claims, the constraints or the open questions of a specification are not in ascending id order, or when the traceability matrix does not follow the claims"
type: feature
id: "F1"
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
blocks: ["F5"]
spikes: []
created: "2026-10-08"
updated: "2026-10-09"
github_issue: null
synced_at: null
---

# Specification: Row ordering rules

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-08 -->

A specification's claims, constraints and open questions are appended as they are decided, and nothing holds them in id order: a late `B-004` lands under `B-011`, and § 9 drifts from § 3 as rows are added to one and not moved in the other. Every reader then scans instead of reading, and every reviewer reads a diff of moved rows. This Feature removes that failure state: in schema version 2 the check warns, with the row and the id it expected there, whenever one of those tables is out of ascending id order or § 9 does not follow § 3.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-08 -->

| #   | Persona                             | Need                                                          | Pain Point Today                                              |
| --- | ----------------------------------- | ------------------------------------------------------------- | ------------------------------------------------------------- |
| 1   | The reader of a specification       | Find `B-007` by position, and read § 9 beside § 3 row for row | Rows sit in the order they were written, not the order of ids |
| 2   | The agent authoring a specification | Be told where a row belongs, from the report alone (Must-5)   | Nothing says the order is wrong until a reviewer does         |
| 3   | The maintainer of four repositories | Choose whether ordering is enforced, per repository           | A new rule could break a tree that is otherwise complete      |
| 4   | `0101-F5`                           | One definition of the order, which the command applies        | N/A (internal dependency)                                     |

### Assumptions

| ID  | Assumption                                                                                                                                                                    |
| --- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | A warning is printed and counted, and fails the run only under `--strict`; a disabled rule reports nothing. Both are `0001-F5` B-010 and B-011, which this Feature relies on. |
| A-2 | § 3, § 4, § 9 and § 11 are found by their role in the manifest once `0001-F5` B-001 lands; this specification names them by their default titles.                             |
| A-3 | An id's number and letter are read through the manifest's grammar for that table (`claim`, `constraint`, `openQuestion`), as `0001-F5` C-2 requires of every rule.            |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-08 -->

| ID    | Claim                                                                                                                                                                            | Source                         | Status |
| ----- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------ | ------ |
| B-001 | Given § 3 rows whose claim ids are not in ascending order, this Feature reports one warning, at the first row out of place, naming the claim id expected there.                  | decision 0002                  | Active |
| B-002 | Given § 4 rows whose constraint ids are not in ascending order, this Feature reports one warning, at the first row out of place, naming the constraint id expected there.        | decision 0002                  | Active |
| B-003 | Given § 11 rows whose open-question ids are not in ascending order, this Feature reports one warning, at the first row out of place, naming the open-question id expected there. | decision 0002                  | Active |
| B-004 | Given § 3 in ascending order and § 9 rows in another order, this Feature reports one warning, at the first § 9 row out of place, naming the claim id expected there.             | decision 0002                  | Active |
| B-005 | Given ids whose numbers have different digit counts, such as `C-2` and `C-10`, this Feature orders them by number, so `C-2` before `C-10` is in order.                           | decision 0002; C-1             | Active |
| B-006 | Given a lettered claim id such as `B-006b`, this Feature orders it directly after `B-006` and before `B-007`.                                                                    | decision 0002; C-1             | Active |
| B-007 | Given a withdrawn or superseded row in its numeric slot, this Feature reports nothing for it.                                                                                    | decision 0002; C-2             | Active |
| B-008 | Given § 3, § 4, § 9 and § 11 each in order, this Feature reports nothing.                                                                                                        | decision 0002                  | Active |
| B-009 | Given an ordering warning, its `expected` carries that table's ids in the order this Feature wants them.                                                                         | Must-5; `0001-F3`              | Active |
| B-010 | Given a manifest pinning schema version 2 that sets no severity for this Feature's rule, its violations are warnings.                                                            | decision 0001                  | Active |
| B-011 | Given a manifest pinning schema version 2 that disables this Feature's rule, this Feature reports nothing.                                                                       | decision 0001; `0001-F5` B-011 | Active |
| B-012 | Given a manifest pinning schema version 1, this Feature reports nothing.                                                                                                         | decision 0001; C-5             | Active |
| B-013 | Given ids out of order in a bulleted list or a sentence, this Feature reports nothing.                                                                                           | decision 0002                  | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-09 (C-6 amended for ADR-0004) -->

| ID  | Constraint                                                                                                                                                                            | Rules Out                                                                                                                                      |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------- |
| C-1 | Order is ascending by the id's number, then by its letter, with no letter first.                                                                                                      | A text sort that puts `C-10` before `C-2`; ordering by any other column.                                                                       |
| C-2 | A row's status never moves it: a withdrawn or superseded row keeps its numeric slot.                                                                                                  | Grouping retired rows at the end; a separate order for active rows.                                                                            |
| C-3 | Order is judged only over first cells that match the manifest's grammar for the table; a cell that does not is `0101-F3`'s finding.                                                   | Guessing a number out of a malformed id; a second id grammar in C# (`0001-F5` C-2).                                                            |
| C-4 | The rule reports; it never rewrites a file.                                                                                                                                           | A fix applied by the check; a `--fix` option; any write from a check run (`0101-F5` decision 0001).                                            |
| C-5 | The rule belongs to schema version 2 and later; version 1's vocabulary is frozen.                                                                                                     | Reporting under a version 1 manifest; adding the rule to version 1 (brief § 9; `0001-F1` C-5).                                                 |
| C-6 | The default severity is warning, declared by the rule's own `DefaultSeverity`, an `ISpecRule` member that lands with epic `0101`'s first item; `0001-F5`'s rule settings override it. | Error by default; the default kept in the manifest instead of on the rule (ADR-0004); a lint-only switch outside the manifest's rule settings. |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-08 -->

| #   | Item                                                           | Exclusion Reason                                                   |
| --- | -------------------------------------------------------------- | ------------------------------------------------------------------ |
| 1   | The order of ids inside prose - bullets and sentences          | Decision 0002; B-013.                                              |
| 2   | The order of § 2 needs and assumptions, § 5 rows and § 12 rows | Decision 0002: only the three id tables and § 9.                   |
| 3   | A missing id - a gap in a sequence                             | `0101-F2`; this Feature judges the order of rows that are present. |
| 4   | Putting the rows in order                                      | `0101-F5`; the check stays read-only (C-4).                        |
| 5   | A first cell that is empty, or not exactly one id              | `0101-F3` (C-3); a duplicate claim id is `SPEC030`.                |
| 6   | Per-rule severity and disable                                  | `0001-F5` B-010, B-011; this Feature sets a default (C-6).         |
| 7   | The rule ids of epic `0101`                                    | OQ-1.                                                              |

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

| Claim ID | Scenario                                                    | Test    | Status  |
| -------- | ----------------------------------------------------------- | ------- | ------- |
| B-001    | Claims out of order are reported at the first misplaced row | Missing | Missing |
| B-002    | Constraints out of order are reported                       | Missing | Missing |
| B-003    | Open questions out of order are reported                    | Missing | Missing |
| B-004    | A matrix that does not follow the claims is reported        | Missing | Missing |
| B-005    | Ids are ordered by number, not by text                      | Missing | Missing |
| B-006    | A lettered claim follows its number                         | Missing | Missing |
| B-007    | A withdrawn row in its slot is in order                     | Missing | Missing |
| B-008    | Tables in order report nothing                              | Missing | Missing |
| B-009    | The warning carries the order expected                      | Missing | Missing |
| B-010    | Ordering findings are warnings by default                   | Missing | Missing |
| B-011    | A disabled ordering rule reports nothing                    | Missing | Missing |
| B-012    | A version 1 repository is not checked for order             | Missing | Missing |
| B-013    | Ids in prose are not checked for order                      | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-09 (C-6) -->

- 2026-10-09, C-6 amended in place on the owner's acceptance of [ADR-0004](../../../../../.spec/adr/0004-per-rule-settings-are-selection-and-a-map.md): it read "set through `0001-F5`'s rule settings", and the default is now the rule's own `DefaultSeverity`, which the manifest's rule settings override. Decision 0001 is preserved: a member is a fact about the rule, not a switch, and this rule adds no switch of its own. C-6 keeps its id; no claim or scenario changed.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-08 -->

| ID   | Question                                                                                                                                                                                                                                                                                                                      | Blocks       | Resolution |
| ---- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------ | ---------- |
| OQ-1 | Which rule ids does epic `0101` take? `SPEC070` is already named for constraint-citation resolution (`0001-F1` § 5 row 7; `specht-conventions` specs reference), so the proposal is a new `SPEC08x` block: one id per Feature (`0101-F1` to `0101-F4`) or one per finding. The epic's other rule Features cite this question. | B-010, B-011 | Open       |
| OQ-2 | When § 3 is itself out of order, is § 9 judged against § 3 as written, or against the claim ids in ascending order? The first reports § 9 for a fault that is § 3's; the second reports § 9 even where it mirrors § 3 row for row. `format` puts both in ascending order either way (`0101-F5` B-004).                        | B-004        | Open       |

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
