---
title: "Specification: Retired-id rule"
description: "A schema version 2 rule that warns when a claim, constraint or open-question sequence skips a number, because a retired id keeps its row, marked, and is never deleted"
type: feature
id: "F2"
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

# Specification: Retired-id rule

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-08 -->

Ids are never reused and never renumbered, and a withdrawn claim is marked, not deleted (AGENTS.md § "Stable IDs"). Nothing checks it. A deleted row leaves a hole: every past commit, review and sibling specification that cites the id now points at nothing, and the next author may take the free number and reuse it. This Feature removes that failure state: in schema version 2 the check warns when a claim, constraint or open-question sequence skips a number, naming the id whose row is gone, so the row is restored and marked instead of left missing.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-08 -->

| #   | Persona                                  | Need                                                                       | Pain Point Today                                       |
| --- | ---------------------------------------- | -------------------------------------------------------------------------- | ------------------------------------------------------ |
| 1   | Whoever follows a citation such as `C-3` | The row the citation names still exists, marked if it was retired          | A deleted row breaks every citation to it, silently    |
| 2   | The agent authoring a specification      | Be told which id lost its row and how to restore it, from the report alone | The stable-id rule lives in prose; nothing enforces it |
| 3   | The maintainer of four repositories      | Choose whether the rule is enforced, per repository                        | A tree written before the rule may carry old gaps      |

### Assumptions

| ID  | Assumption                                                                                                                                                               |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| A-1 | A warning is printed and counted, and fails the run only under `--strict`; a disabled rule reports nothing (`0001-F5` B-010, B-011).                                     |
| A-2 | A sequence starts at 1: the first claim is `B-001`, the first constraint `C-1`, the first open question `OQ-1`, as the template and every specification here write them. |
| A-3 | An id's number is read through the manifest's grammar for its table, as `0001-F5` C-2 requires of every rule.                                                            |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-08 -->

| ID    | Claim                                                                                                                                                       | Source                                | Status |
| ----- | ----------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------- | ------ |
| B-001 | Given § 3 claim ids that skip a number, such as `B-001` and `B-003` with no `B-002`, this Feature reports a warning naming `B-002` as an id with no row.    | `0101-F1` decision 0002; AGENTS.md    | Active |
| B-002 | Given § 4 constraint ids that skip a number, this Feature reports a warning naming each skipped constraint id.                                              | `0101-F1` decision 0002; AGENTS.md    | Active |
| B-003 | Given § 11 open-question ids that skip a number, this Feature reports a warning naming each skipped open-question id.                                       | `0101-F1` decision 0002; AGENTS.md    | Active |
| B-004 | Given a sequence whose lowest id is above 1, such as claims starting at `B-002`, this Feature reports each id below it as an id with no row.                | A-2                                   | Active |
| B-005 | Given `B-006` and `B-006b` both present, this Feature reports no gap for either.                                                                            | A-3                                   | Active |
| B-006 | Given every number from 1 to the highest present, each with a row, withdrawn or not, this Feature reports nothing.                                          | `0101-F1` decision 0002               | Active |
| B-007 | Given a gap, the warning's `expected` names the missing id and the marker a restored row carries, as the manifest declares it, and proposes no renumbering. | Must-5; AGENTS.md § "Stable IDs"; C-2 | Active |
| B-008 | Given a manifest pinning schema version 2 that sets no severity for this Feature's rule, its violations are warnings.                                       | `0101-F1` decision 0001               | Active |
| B-009 | Given a manifest pinning schema version 2 that disables this Feature's rule, this Feature reports nothing.                                                  | `0101-F1` decision 0001               | Active |
| B-010 | Given a manifest pinning schema version 1, this Feature reports nothing.                                                                                    | `0101-F1` decision 0001; C-4          | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-08 -->

| ID  | Constraint                                                                             | Rules Out                                                                                                            |
| --- | -------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------- |
| C-1 | A gap is judged per table, from 1 to the highest number present in it.                 | Judging a gap across Features; reporting a number above the highest present, which nothing can know was issued.      |
| C-2 | A finding never proposes a renumbering, and the expected fix is always a restored row. | "Renumber `B-003` to `B-002`" in a message or an `expected`; closing a gap by moving ids (AGENTS.md § "Stable IDs"). |
| C-3 | The rule reports; it never rewrites a file.                                            | Restoring a row from the check; `format` inserting one (`0101-F5` decision 0001).                                    |
| C-4 | The rule belongs to schema version 2 and later, at warning severity by default.        | Reporting under version 1; error by default (`0101-F1` decision 0001).                                               |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-08 -->

| #   | Item                                            | Exclusion Reason                                                                                                  |
| --- | ----------------------------------------------- | ----------------------------------------------------------------------------------------------------------------- |
| 1   | The order of rows that are present              | `0101-F1`.                                                                                                        |
| 2   | Restoring a missing row                         | Never automatic: `format` moves rows and inserts none (`0101-F5` decision 0001).                                  |
| 3   | A § 9 row for every claim, withdrawn or not     | `SPEC031` already.                                                                                                |
| 4   | Whether a present row's retirement is marked    | A present row fills its slot, marked or not; nothing can tell a retired row from a live one but its marker. OQ-1. |
| 5   | Gaps in item ids, Feature ids or assumption ids | Item ids are `SPEC044`; Feature and assumption ids are not in the decided scope (`0101-F1` decision 0002).        |
| 6   | The rule id                                     | `0101-F1` OQ-1.                                                                                                   |

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

| Claim ID | Scenario                                                | Test    | Status  |
| -------- | ------------------------------------------------------- | ------- | ------- |
| B-001    | A skipped claim number is reported                      | Missing | Missing |
| B-002    | A skipped constraint number is reported                 | Missing | Missing |
| B-003    | A skipped open-question number is reported              | Missing | Missing |
| B-004    | A sequence that starts above one is reported            | Missing | Missing |
| B-005    | A lettered claim is not a gap                           | Missing | Missing |
| B-006    | A complete sequence with withdrawn rows reports nothing | Missing | Missing |
| B-007    | The finding asks for the row back, never a renumbering  | Missing | Missing |
| B-008    | Retired-id findings are warnings by default             | Missing | Missing |
| B-009    | A disabled retired-id rule reports nothing              | Missing | Missing |
| B-010    | A version 1 repository is not checked for gaps          | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-08 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-08 -->

| ID   | Question                                                                                                                                                                                                                                                                                                                                                                                                              | Blocks | Resolution |
| ---- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------ | ---------- |
| OQ-1 | What is the retired marker, and where is it declared? The specifications here mark a claim with a Status of `Withdrawn` or `Superseded`, and a constraint by opening its text with `Withdrawn <date>:`; the decided scope also names `superseded by B-0nn`. Is the vocabulary a manifest marker role (brief § 5, beside `Missing` and the sign-off markers), and does a marker on a § 4 row need a column of its own? | B-007  | Open       |

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
