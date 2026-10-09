---
title: "Specification: Frontmatter key order"
description: "A schema version 2 rule that warns when a Feature specification's or an epic file's frontmatter keys are not in the order the pinned version declares for that kind"
type: feature
id: "F4"
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
updated: "2026-10-08"
github_issue: null
synced_at: null
---

# Specification: Frontmatter key order

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-08 -->

A specification's frontmatter is 22 keys, written from a template and then edited by hand, by agents and by scripts that append what they set. The schema checks each key and ignores their order, so the order drifts: `status` lands below `updated`, `blocks` above `depends_on`, and two specifications of the same kind stop reading alike. This Feature removes that failure state: in schema version 2 the check warns when a Feature specification's or an epic file's keys are not in the order the pinned version declares for that kind, naming the first key out of place.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-08 -->

| #   | Persona                             | Need                                                                  | Pain Point Today                                           |
| --- | ----------------------------------- | --------------------------------------------------------------------- | ---------------------------------------------------------- |
| 1   | The reader of a specification       | The same key at the same place in every specification of a kind       | Keys drift as each editor appends what it sets             |
| 2   | The agent authoring a specification | The expected order in the report, so it can write keys in it (Must-5) | The order exists only in a template the agent may not read |
| 3   | `0101-F5`                           | One declared order, which the command applies                         | N/A (internal dependency)                                  |

### Assumptions

| ID  | Assumption                                                                                                                           |
| --- | ------------------------------------------------------------------------------------------------------------------------------------ |
| A-1 | A warning is printed and counted, and fails the run only under `--strict`; a disabled rule reports nothing (`0001-F5` B-010, B-011). |
| A-2 | The order is the pinned version's, one per frontmatter kind; where it is declared is OQ-1.                                           |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-08 -->

| ID    | Claim                                                                                                                                                                                             | Source                       | Status |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------- | ------ |
| B-001 | Given a Feature specification whose frontmatter keys are not in the order declared for a Feature, this Feature reports one warning, at the first key out of place, naming the key expected there. | `0101-F1` decision 0002      | Active |
| B-002 | Given an epic file whose frontmatter keys are not in the order declared for an epic, this Feature reports one warning, at the first key out of place, naming the key expected there.              | `0101-F1` decision 0002      | Active |
| B-003 | Given frontmatter whose keys follow the declared order with some optional keys absent, this Feature reports nothing.                                                                              | `0101-F1` decision 0002      | Active |
| B-004 | Given a key-order warning, its `expected` carries that kind's declared order.                                                                                                                     | Must-5; `0001-F3`            | Active |
| B-005 | Given a manifest pinning schema version 2 that sets no severity for this Feature's rule, its violations are warnings.                                                                             | `0101-F1` decision 0001      | Active |
| B-006 | Given a manifest pinning schema version 2 that disables this Feature's rule, this Feature reports nothing.                                                                                        | `0101-F1` decision 0001      | Active |
| B-007 | Given a manifest pinning schema version 1, this Feature reports nothing.                                                                                                                          | `0101-F1` decision 0001; C-3 | Active |
| B-008 | Given frontmatter keys in the declared order whose values are written in another style - quoted or bare, flow or block - this Feature reports nothing.                                            | C-1                          | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-08 -->

| ID  | Constraint                                                                                        | Rules Out                                                                                            |
| --- | ------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------- |
| C-1 | The rule judges the order of keys only; a value's content and style are the frontmatter schemas'. | A finding about quoting, flow style or a value; a second schema check beside `SPEC002` to `SPEC004`. |
| C-2 | The rule reports; it never rewrites a file.                                                       | A fix applied by the check (`0101-F5` decision 0001).                                                |
| C-3 | The rule belongs to schema version 2 and later, at warning severity by default.                   | Reporting under version 1; error by default (`0101-F1` decision 0001).                               |
| C-4 | The declared order is data the pinned version ships, read by the rule, not a list in C#.          | A key list hardcoded in a rule (`0001-F5` C-1).                                                      |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-08 -->

| #   | Item                                                   | Exclusion Reason                                      |
| --- | ------------------------------------------------------ | ----------------------------------------------------- |
| 1   | Putting the keys in order                              | `0101-F5`.                                            |
| 2   | Which keys are required, and their values              | The frontmatter schemas, `SPEC002` to `SPEC004`.      |
| 3   | Item frontmatter, and decision, lesson and ADR records | OQ-3; no version 1 rule reads a record's frontmatter. |
| 4   | The rule id                                            | `0101-F1` OQ-1.                                       |

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

| Claim ID | Scenario                                                 | Test    | Status  |
| -------- | -------------------------------------------------------- | ------- | ------- |
| B-001    | A Feature specification's keys out of order are reported | Missing | Missing |
| B-002    | An epic file's keys out of order are reported            | Missing | Missing |
| B-003    | Absent optional keys do not break the order              | Missing | Missing |
| B-004    | The warning carries the declared order                   | Missing | Missing |
| B-005    | Key-order findings are warnings by default               | Missing | Missing |
| B-006    | A disabled key-order rule reports nothing                | Missing | Missing |
| B-007    | A version 1 repository is not checked for key order      | Missing | Missing |
| B-008    | The style of a value is not judged                       | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-08 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-08 -->

| ID   | Question                                                                                                                                                                                                                                                                                                                                                                                      | Blocks       | Resolution |
| ---- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------ | ---------- |
| OQ-1 | Where is the declared order read from? The decided answer is "template order", but the sources disagree: the Feature template writes `type` before `id`, while the Feature frontmatter schema's `required` lists `id`, `epic`, `type`; and version 1 ships no epic template at all. Candidates: the kind's template, the schema's `properties` order, or a per-kind key list in the manifest. | B-001, B-002 | Open       |
| OQ-2 | Where does a key the declared order does not list go - such as the optional `alignment_rejections` on a Feature, or `synced_at` on an epic, which the epic schema allows and no epic here writes? After every listed key, or anywhere?                                                                                                                                                        | B-001, B-002 | Open       |
| OQ-3 | Is a work item's frontmatter in scope? Version 1 ships a `task` frontmatter schema for items, while this repository's own items are `.issue/*.yml` files the tool does not read.                                                                                                                                                                                                              | —            | Open       |

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
