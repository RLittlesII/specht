---
title: "Specification: {{feature_title}}"
description: "{{one_line_summary}}"
type: feature
id: "F{{n}}"
epic: "{{epic_id}}"
spec_status: draft
status: ready
priority: med
value: 0
risk: 0
rank: 0
scored_by: null
scored_on: null
domain: "{{domain}}"
author: "spec-author"
milestone: null
children: []
depends_on: []
blocks: []
spikes: []
created: "{{date}}"
updated: "{{date}}"
github_issue: null
synced_at: null
---

<!-- Copy to <feature>/.spec/README.md. The companion Gherkin file sits beside
     it as <feature>/.spec/<feature-name>.feature, and the three record folders
     decisions/ adr/ lessons/ beside that.

     spec_status is DOCUMENT maturity (draft|in-review|approved|superseded) and
     is owned by this file.

     status is DELIVERY lifecycle — MIRRORED from the issue's status:* label
     once github_issue is set; never hand-edit it after that point.

     priority is authored HERE, permanently. No priority:* label is applied to a
     Feature issue, so there is nothing to mirror it from. priority, rank and
     blocks are DERIVED from value/risk and the dependency graph — recompute,
     never hand-edit.

     Delete this comment and every <!-- Owner: --> guidance comment from the
     copy. Guidance lives in the template; a copy carries content only. -->

# Specification: {{feature_title}}

## 1. Business Goal

<!-- last written by: spec-author, {{date}} -->

{{business_goal}}

<!-- Owner: spec-author. One paragraph. The outcome, not the implementation.
     Name the failure state being removed. -->

## 2. User Needs

<!-- last written by: spec-author, {{date}} -->

| #   | Persona       | Need       | Pain Point Today |
| --- | ------------- | ---------- | ---------------- |
| 1   | {{persona}}   | {{need}}   | {{pain}}         |

<!-- Owner: spec-author. A sibling Feature is a legitimate persona — say so and
     mark its Pain Point "N/A (internal dependency)". -->

### Assumptions

| ID  | Assumption       |
| --- | ---------------- |
| A-1 | {{assumption}}   |

<!-- Owner: spec-author. An assumption that later gets decided is marked
     RESOLVED in place with the date and the OQ it answered, and the decided
     requirement becomes a § 3 claim — the assumption row is never deleted. -->

## 3. Acceptance Criteria

<!-- last written by: spec-author, {{date}} -->

| ID    | Claim        | Source       | Status |
| ----- | ------------ | ------------ | ------ |
| B-001 | {{claim}}    | {{source}}   | Active |

<!-- Owner: spec-author. Each claim is one falsifiable statement in
     Given/this Feature form, scoped to this Feature. Claim ids are permanent
     and never renumbered — a withdrawn claim is marked Withdrawn, not deleted,
     or § 9 and the .feature file lose their anchors. Status is Active,
     Amended, Withdrawn or Superseded. -->

## 4. Constraints

<!-- last written by: spec-author, {{date}} -->

| ID   | Constraint       | Rules Out       |
| ---- | ---------------- | --------------- |
| C-1  | {{constraint}}   | {{what_it_rules_out}} |

<!-- Owner: spec-author. A constraint states what it rules out, or a designer
     cannot act on it. Cited from outside this Feature as `<epic>-F<n> C-<n>`.
     An imported external rule is a decision record, not a constraint row. -->

## 5. Out of Scope

<!-- last written by: spec-author, {{date}} -->

| #   | Item       | Exclusion Reason |
| --- | ---------- | ---------------- |
| 1   | {{item}}   | {{reason}}       |

<!-- Owner: spec-author. A specification that states only the target invites
     over-delivery. Where an exclusion clarifies a scenario, add an
     @boundary-tagged scenario to the .feature file as well. -->

## 6. Concern Separation

<!-- last written by: implementer, {{date}} -->

| #   | Concern       | Classification        |
| --- | ------------- | --------------------- |
| 1   | {{concern}}   | Business \| Technical \| Both |

<!-- Owner: implementer. -->

## 7. Technical Design

<!-- last written by: implementer, {{date}} -->

{{technical_design}}

<!-- Owner: implementer. Design positions, the domain model, interface
     declarations, and diagrams. Once code exists the compiler is the authority:
     reference the file, do not restate a signature. A durable decision made
     here is an ADR in adr/, not a paragraph. -->

## 8. Testing Strategy

<!-- last written by: test-writer, {{date}} -->

{{testing_strategy}}

<!-- Owner: test-writer. Which claims are covered by an acceptance scenario and
     which by unit tests, the testability verdict, and anything the design makes
     hard to test. Reqnroll runs the acceptance tier only — unit tests still
     cover the concerns. -->

## 9. Traceability Matrix

<!-- last written by: test-writer, {{date}} -->

| Claim ID | Scenario       | Test       | Status  |
| -------- | -------------- | ---------- | ------- |
| B-001    | {{scenario}}   | Missing    | Missing |

<!-- Owner: test-writer. Every ID in § 3 appears here exactly once. The row is
     anchored to the scenario's `@B-00n` TAG, not to the title in the Scenario
     column — that title is a human-reading courtesy and a stale one is a
     cleanup, not a gate failure. A `Missing` row blocks the item reaching done;
     it does not block spec_status reaching approved. -->

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, {{date}} -->

| Lesson       | Delta       | Claim       |
| ------------ | ----------- | ----------- |
| {{lesson}}   | {{delta}}   | {{claim_id}} |

<!-- Owner: spec-author. One row per lesson in this Feature's lessons/ folder.
     A repo-wide lesson lives in the root .spec/lessons/ and has no row here,
     because it belongs to no one Feature. "None." is a valid body. -->

## 11. Open Questions

<!-- last written by: spec-author, {{date}} -->

| ID   | Question       | Blocks       | Resolution |
| ---- | -------------- | ------------ | ---------- |
| OQ-1 | {{question}}   | {{claim_id}} | Open       |

<!-- Owner: whoever is blocked. Resolved in place with the date and the
     decision, never deleted. "None." is a valid body. -->

## 12. Sign-off

<!-- last written by: spec-reviewer, {{date}} -->

| Section       | Status | Reviewer       | Note       |
| ------------- | ------ | -------------- | ---------- |
| 1-5           | 🟡     | spec-reviewer  | {{note}}   |

<!-- Owner: spec-reviewer. 🟡 Draft, 🟢 Approved, 🔴 Blocked with a reason.
     Flip the frontmatter spec_status to approved only when every row is 🟢. -->

## Tasks

<!-- Child items cut from § 3 after stakeholder agreement. Each is a GitHub
     issue; this list names them, and the issue carries the delivery state.
     A Feature's specification exists before any task does. -->

## Scoring

| Field  | Value | Basis       |
| ------ | ----- | ----------- |
| value  | 0     | {{basis}}   |
| risk   | 0     | {{basis}}   |

<!-- priority and rank are derived from value and risk — recompute, never
     hand-edit. -->
