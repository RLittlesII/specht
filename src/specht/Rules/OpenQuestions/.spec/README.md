---
title: "Specification: Open-question gate"
description: "Under a schema version carrying its rules, the check reports a § 11 Blocks cell citing a claim the specification does not declare and an approved specification with an open question, and, before approval, warns on each open question that blocks a live claim"
type: feature
id: "F1"
epic: "0228"
spec_status: draft
status: ready
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
depends_on: ["F2", "0001/F5", "0001/F7"]
blocks: []
spikes: []
created: "2026-10-28"
updated: "2026-10-28"
github_issue: null
synced_at: null
---

# Specification: Open-question gate

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-28 -->

A consumer ran ten review rounds on one specification and the check reported 0 errors every round, because it checks a specification's structure and not whether its open questions are answered or block claims that exist; most rounds came from questions answered late. Today a § 11 row may block a claim § 3 never declared, a specification may be approved with a question still open - 23 rows in four approved specifications of this repository on 2026-10-28 - and nothing lists the unanswered questions before a round starts. This Feature removes that failure state: under a schema version carrying its rules, the check reports a Blocks cell citing a claim the specification does not declare and an approved specification with an open question, and, before approval, warns on each open question that blocks a live claim.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-28 -->

| #   | Persona                                | Need                                                            | Pain Point Today                                                 |
| --- | -------------------------------------- | --------------------------------------------------------------- | ---------------------------------------------------------------- |
| 1   | The reviewer starting a review round   | The open questions that block live claims, listed by the check  | Reads § 11 by hand, or meets an unanswered question in round six |
| 2   | The spec-author answering a question   | A Blocks cell that cites only claims the specification declares | A Blocks cell citing a claim never written passes                |
| 3   | The owner approving a specification    | Approval refused while a question is open                       | That no question is open is a hand check (epic `0228`)           |
| 4   | A consumer repository pinning the tool | The same gate from the tool it pins, with no checker of its own | Writes its own check, or has none                                |

### Assumptions

| ID  | Assumption                                                                                                                                                              |
| --- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | A claim is live unless its § 3 Status is the withdrawn or the superseded marker; the owner's "Active claim" is read as live (spec-author's reading; `0001-F12` A-3).    |
| A-2 | A warning is printed and counted and fails the run only under `--strict`, and a rule set off reports nothing (`0001-F5` B-010, B-011).                                  |
| A-3 | A Blocks cell's citations are read as `0228-F2` reads a citation: an unqualified id is this specification's, and a range cites its two ends (`0228-F2` B-011 to B-013). |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-28 -->

| ID    | Claim                                                                                                                                                                                                                                | Source                                                                                                     | Status |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------------- | ------ |
| B-001 | Given a § 11 row whose Blocks cell cites an unqualified claim id that no § 3 row of the same specification declares, this Feature reports the Blocks rule at that row, its identifier the claim id.                                  | owner, 2026-10-28; decision 0001 call 2; epic `0228` candidate D; as `SPEC021` resolves a tag              | Active |
| B-002 | Given a Blocks cell citing several claim ids, this Feature reports the Blocks rule once for each id no § 3 row declares and not for the others.                                                                                      | owner, 2026-10-28 (asked); spec-author's reading                                                           | Active |
| B-003 | Given a Blocks cell citing a claim id qualified with another Feature's key, such as `0001-F7` B-115, the Blocks rule does not resolve it against this specification.                                                                 | owner, 2026-10-28 (asked); A-3; `0228-F2` B-011                                                            | Active |
| B-004 | Given a Blocks cell citing a constraint, an assumption or an open-question id, or no id, such as `None` or `§ 5 row 9`, the Blocks rule reports nothing for that text.                                                               | owner, 2026-10-28 (asked); spec-author's reading: this tree's Blocks cells cite 103 such ids on 2026-10-28 | Active |
| B-005 | Given a Blocks cell citing a claim whose § 3 Status is the withdrawn or the superseded marker, the Blocks rule reports nothing for it.                                                                                               | spec-author's reading: the row is declared (AGENTS.md § "Stable IDs")                                      | Active |
| B-006 | Given a specification whose `spec_status` is the approved marker and a § 11 row whose Resolution is open, this Feature reports the approval rule at that row, its identifier the open-question id.                                   | owner, 2026-10-28; decision 0001 call 2; epic `0228` R6; beside `SPEC061`; OQ-3                            | Active |
| B-007 | Given an approved specification and an open § 11 row whose Blocks cell cites no claim, this Feature reports the approval rule at that row.                                                                                           | owner, 2026-10-28: every row whose Resolution is open                                                      | Active |
| B-008 | Given a specification whose `spec_status` is not the approved marker, the approval rule reports nothing.                                                                                                                             | owner, 2026-10-28                                                                                          | Active |
| B-009 | Given a Resolution cell that is empty, or whose first word is the open marker in exact case, such as `Open` or `Open. Proposed default: …`, this Feature reads the row as open, and reads any other Resolution as answered.          | OQ-3 proposed default; epic `0228` R6                                                                      | Active |
| B-010 | Given a specification that is not approved, each § 11 row whose Resolution is open and whose Blocks cell cites at least one live claim of § 3 is reported by the readiness warning at that row, its identifier the open-question id. | owner, 2026-10-28; decision 0001 call 2; A-1; OQ-5                                                         | Active |
| B-011 | Given a specification that is not approved, an open § 11 row whose Blocks cell cites no live claim, only a withdrawn claim, a constraint or no id, is not reported by the readiness warning.                                         | owner, 2026-10-28; A-1                                                                                     | Active |
| B-012 | Given an approved specification, the readiness warning reports nothing, so an open row is reported by the approval rule alone.                                                                                                       | spec-author's reading: one finding per fault, as `0101-F6` C-8                                             | Active |
| B-013 | Given a § 11 that holds no table, such as `None.`, none of this Feature's rules reports anything for that specification.                                                                                                             | the feature template's § 11 guidance; as `0101-F6` B-010                                                   | Active |
| B-014 | Given a specification discovered under the legacy `epics/<epic>/<feature>/spec.md` layout, this Feature judges it as it judges a co-located one.                                                                                     | owner, 2026-10-28 (asked); AGENTS.md § "Where a specification lives"; spec-author's reading                | Active |
| B-015 | Given a manifest pinning a schema version that carries this Feature's rules and setting no level for them, the Blocks rule and the approval rule report errors.                                                                      | owner, 2026-10-28; decision 0001 call 2                                                                    | Active |
| B-016 | Given a manifest pinning a schema version that carries this Feature's rules and setting no level for the readiness warning, it reports warnings.                                                                                     | owner, 2026-10-28; decision 0001 call 2                                                                    | Active |
| B-017 | Given a manifest pinning schema `0.2.1` or earlier, none of this Feature's rules runs, and the report is byte-identical to the one the tool gives that tree without this Feature.                                                    | `0001-F7` C-1, C-11; as `0001-F12` B-021                                                                   | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-28 -->

| ID  | Constraint                                                                                                                                              | Rules Out                                                                                                                             |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------- |
| C-1 | The rules read one specification: its frontmatter, its claims section and its open-questions section.                                                   | Reading a sibling specification, an epic, an item, a decision record or git history; a verdict that depends on anything but the tree. |
| C-2 | Every section, column position and marker the rules compare comes from the manifest (brief § 5; `0001-F5` C-2).                                         | `"Open"`, `"11. Open Questions"`, a column header or an id grammar written in a rule.                                                 |
| C-3 | A Blocks cell's citations are read by `0228-F2`'s citation reader.                                                                                      | A second grammar for qualifiers or ranges; a Blocks rule and the dangling-citation rule disagreeing on what one cell cites.           |
| C-4 | The rules join the vocabulary at a `0.minor` schema step after `0.2.1`, `0.3.0` at the earliest, and stay in every later version (`0001-F7` C-1, C-11). | Adding them to `0.1.0`, `0.2.0` or `0.2.1`; reporting under those pins; a patch step that adds them.                                  |
| C-5 | A Resolution is judged by its own cell.                                                                                                                 | Reading "OQ-3 is answered" in § 10, a decision record or a commit; checking a Resolution's date against history.                      |
| C-6 | The rules report; they never rewrite a file (brief § 9).                                                                                                | Resolving a question, changing `spec_status` or editing a Blocks cell from the check.                                                 |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-28 -->

| #   | Item                                                                                        | Exclusion Reason                                                                                           |
| --- | ------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------- |
| 1   | Constraint and open-question ids in a Blocks cell, and every citation outside a Blocks cell | `0228-F2` (B-004).                                                                                         |
| 2   | A qualified claim id in a Blocks cell resolved against the Feature it names                 | Epic `0228` candidate C, item `0307` (B-003; `0228-F2` OQ-3).                                              |
| 3   | A § 12 review note checked against repository history                                       | Declined by the owner, 2026-10-28: it breaks "same tree, same report" (brief § 9; decision 0001 call 3).   |
| 4   | Whether a Then step is implied by another claim's refusal                                   | Declined by the owner, 2026-10-28: semantic, the reviewer's work (decision 0001 call 3).                   |
| 5   | Constraint tracing                                                                          | Spike `0316` (decision 0001 call 4).                                                                       |
| 6   | Then-step wording lint                                                                      | Spike `0315` (decision 0001 call 4).                                                                       |
| 7   | Term variants across a specification                                                        | Spike `0317` (decision 0001 call 4).                                                                       |
| 8   | A baseline or suppression of accepted warnings                                              | Spike `0318`; per-rule levels and `--strict` are the only way today (epic `0228` § Constraints).           |
| 9   | An undated Resolution read as open                                                          | Epic `0228` R6 names it; not adopted by default (OQ-3).                                                    |
| 10  | The manifest keys for the open-questions section, its columns and the open marker           | `0001-F5` (OQ-4).                                                                                          |
| 11  | The rule ids and the schema version that carries them                                       | OQ-1, OQ-2; `0001-F7`.                                                                                     |
| 12  | What each rule's `expected` carries                                                         | `0001-F3`.                                                                                                 |
| 13  | A `Missing` § 9 cell or an unapproved § 12 row in an approved specification                 | `SPEC060`, `SPEC061`; from schema `0.3.0` `SPEC060` follows the delivery status (`0001-F7` decision 0015). |
| 14  | Per-rule levels and disabling a rule                                                        | `0001-F5` B-010, B-011.                                                                                    |

## 6. Concern Separation

<!-- last written by: spec-author, 2026-10-28 -->

Pending: owned by `implementer`, written after agreement.

## 7. Technical Design

<!-- last written by: spec-author, 2026-10-28 -->

Pending: owned by `implementer`, written after agreement.

## 8. Testing Strategy

<!-- last written by: spec-author, 2026-10-28 -->

Pending: owned by `test-writer`, written after agreement.

## 9. Traceability Matrix

<!-- last written by: spec-author, 2026-10-28 -->

| Claim ID | Scenario                                                              | Test    | Status  |
| -------- | --------------------------------------------------------------------- | ------- | ------- |
| B-001    | A Blocks cell citing an undeclared claim is reported                  | Missing | Missing |
| B-002    | Each undeclared claim in a Blocks cell is reported once               | Missing | Missing |
| B-003    | A claim qualified with another Feature is not resolved here           | Missing | Missing |
| B-004    | A Blocks cell citing no claim is not reported                         | Missing | Missing |
| B-005    | A Blocks cell citing a withdrawn claim is not reported                | Missing | Missing |
| B-006    | An approved specification with an open question is reported           | Missing | Missing |
| B-007    | An open question that blocks no claim still stops approval            | Missing | Missing |
| B-008    | A specification that is not approved is not held to the approval gate | Missing | Missing |
| B-009    | What reads as an open question                                        | Missing | Missing |
| B-010    | An open question blocking a live claim is listed before approval      | Missing | Missing |
| B-011    | An open question blocking no live claim is not listed                 | Missing | Missing |
| B-012    | An approved specification's open question is reported once            | Missing | Missing |
| B-013    | A specification with no open-question table is not judged             | Missing | Missing |
| B-014    | A legacy-layout specification is judged the same                      | Missing | Missing |
| B-015    | The Blocks rule and the approval rule are errors by default           | Missing | Missing |
| B-016    | The readiness warning is a warning by default                         | Missing | Missing |
| B-017    | Under schema 0.2.1 none of the open-question rules runs               | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-28 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-28 -->

| ID   | Question                                                                                                                                                                                                                                                                      | Blocks                     | Resolution                                                                                                                                                                                                                                                                                                                                                                                                                                                     |
| ---- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | Which rule ids carry the three verdicts, and in which groups?                                                                                                                                                                                                                 | B-001, B-006, B-010        | Open. Proposed default: the Blocks rule in the claims and traceability group `SPEC03x`, beside `SPEC031`, which reports § 9 citing a non-claim; the approval rule and the readiness warning in the approval group `SPEC06x`, beside `SPEC060` and `SPEC061`; three ids, since a rule page states one default severity. The ids are the vocabulary's at the version that ships them (item `0169`); none is reserved here.                                       |
| OQ-2 | Which schema version carries the rules?                                                                                                                                                                                                                                       | B-015 to B-017             | Open. Proposed default: `0.3.0`, the next `0.minor` step, which `0001-F7` C-11 lets fail a tree that passed and which `0001-F7` decision 0015 and `0001-F16` OQ-5 already aim at. Rejected by the default: adding to `0.2.1`, which has shipped (`0001-F7` C-1).                                                                                                                                                                                               |
| OQ-3 | What makes a Resolution open: the cell exactly the open marker, its first word the open marker, or epic `0228` R6's "empty or undated"? And is case exact?                                                                                                                    | B-006, B-009, B-010        | Open. Proposed default: B-009, an empty cell or a first word equal to the open marker in exact case, as `SPEC060` compares markers. This tree's § 11 Resolutions begin `Open` 159 times, `Resolved` 94 and `RESOLVED` 20, and `0001-F16`'s read `Open. Proposed default: …` (2026-10-28). Rejected by the default: the exact cell, which misses those rows; "undated", which needs a date grammar and would flag an answer written without a date (§ 5 row 9). |
| OQ-4 | Which manifest keys name the open-questions section, the Blocks and Resolution columns and the open marker?                                                                                                                                                                   | B-001, B-006, B-009, B-010 | Open. Proposed default: a `0001-F5` delta adding `roles.openQuestions`, default `11. Open Questions`, as `0101-F6` OQ-2 proposes for the same section; `tables.openQuestions`, default `ID`, `Question`, `Blocks`, `Resolution`, read by position as `tables.signOff` is (`0001-F5` decision 0015); and `markers.open`, default `Open`. Whether `SPEC013` then holds § 11 to those headers is `0001-F5`'s.                                                     |
| OQ-5 | Does the readiness warning report a specification whose `spec_status` is `superseded`?                                                                                                                                                                                        | B-010                      | Open. Proposed default: yes, as any status that is not approved: no manifest marker names superseded, and a superseded specification's open questions still mislead its reader. Rejected by the default: a new marker read only to silence the warning.                                                                                                                                                                                                        |
| OQ-6 | Four approved specifications here, `0001-F6`, `0001-F9`, `0201-F3` and `0201-F5`, carry 23 open § 11 rows (2026-10-28). When this repository's pin reaches the version carrying the approval rule, each is an error. How does the pull request that moves the pin clear them? | None                       | Open. Proposed default: each row is resolved, or its specification returns to `in-review`, in that pull request; the manifest does not lower the rule. Rejected by the default: lowering the rule here, which `0001-F7` decision 0015 found hides the defect for `SPEC060`.                                                                                                                                                                                    |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-28 -->

| Section | Status | Reviewer      | Note                                                                         |
| ------- | ------ | ------------- | ---------------------------------------------------------------------------- |
| 1-5, 11 | 🟡     | spec-reviewer | Draft awaiting the owner's answers to OQ-1 to OQ-6 and a first review round. |

## Tasks

None cut; the agreement is awaiting review and the owner's answers to § 11.

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
