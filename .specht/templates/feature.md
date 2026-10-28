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

<!-- Copy to <feature>/.spec/README.md with one sibling .feature file and
     decisions/, adr/ and lessons/ records. Delete guidance from the copy.
     Use the fewest words preserving meaning; amend content in place.
     Cite siblings by qualified keys (0001-F2, 0001-F2 B-044, 0001-F2 C-3),
     including external Feature-local decisions and ADRs; no sibling-spec links.
     Own scenarios and supporting records may be linked. Keep required stamps.
     Delivery status mirrors the configured tracker; priority is authored here;
     rank and blocks are derived. spec_status records document maturity. -->

# Specification: {{feature_title}}

## 1. Business Goal

<!-- last written by: spec-author, {{date}} -->

{{business_goal}}

<!-- Outcome and failure state, one paragraph. -->

## 2. User Needs

<!-- last written by: spec-author, {{date}} -->

| #   | Persona     | Need     | Pain Point Today |
| --- | ----------- | -------- | ---------------- |
| 1   | {{persona}} | {{need}} | {{pain}}         |

### Assumptions

| ID  | Assumption     |
| --- | -------------- |
| A-1 | {{assumption}} |

<!-- Resolve in place with date and decision; retain IDs. -->

## 3. Acceptance Criteria

<!-- last written by: spec-author, {{date}} -->

| ID    | Claim     | Source     | Status |
| ----- | --------- | ---------- | ------ |
| B-001 | {{claim}} | {{source}} | Active |

<!-- One falsifiable Given/this Feature statement per permanent ID.
     Withdraw rather than delete or renumber. -->

## 4. Constraints

<!-- last written by: spec-author, {{date}} -->

| ID  | Constraint     | Rules Out             |
| --- | -------------- | --------------------- |
| C-1 | {{constraint}} | {{what_it_rules_out}} |

<!-- State what each constraint rules out; adopted external rules are decisions. -->

## 5. Out of Scope

<!-- last written by: spec-author, {{date}} -->

| #   | Item     | Exclusion Reason |
| --- | -------- | ---------------- |
| 1   | {{item}} | {{reason}}       |

## 6. Concern Separation

<!-- last written by: implementer, {{date}} -->

| #   | Concern     | Classification                |
| --- | ----------- | ----------------------------- |
| 1   | {{concern}} | Business \| Technical \| Both |

## 7. Technical Design

<!-- last written by: implementer, {{date}} -->

{{technical_design}}

<!-- Architectural intent, boundaries and necessary contracts. After implementation,
     remove copied declarations and code pointers. Durable choices belong in ADRs. -->

## 8. Testing Strategy

<!-- last written by: test-writer, {{date}} -->

{{testing_strategy}}

<!-- Strategy, testability and coverage requirements. Require acceptance and
     mechanism coverage where applicable; explain absent mechanisms. No concrete
     test names or locators here. -->

## 9. Traceability Matrix

<!-- last written by: test-writer, {{date}} -->

| Claim ID | Scenario     | Test    | Status  |
| -------- | ------------ | ------- | ------- |
| B-001    | {{scenario}} | Missing | Missing |

<!-- Each § 3 ID appears once, anchored by its scenario tag. All concrete test
     names and locators belong here; verify targets exist. Covered requires
     mechanism coverage; Partial lacks it. Missing blocks delivery, not agreement. -->

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, {{date}} -->

| Trigger     | Change     | Affected IDs     |
| ----------- | ---------- | ---------------- |
| {{trigger}} | {{change}} | {{affected_ids}} |

<!-- Compact trigger/change/affected-ID rows; amend owning sections in place.
     Link local lessons when applicable. None. is valid. -->

## 11. Open Questions

<!-- last written by: spec-author, {{date}} -->

| ID   | Question     | Blocks       | Resolution |
| ---- | ------------ | ------------ | ---------- |
| OQ-1 | {{question}} | {{claim_id}} | Open       |

<!-- Resolve in place with date and decision. None. is valid. -->

## 12. Sign-off

<!-- last written by: spec-reviewer, {{date}} -->

| Section | Status | Reviewer      | Note     |
| ------- | ------ | ------------- | -------- |
| 1-5     | 🟡     | spec-reviewer | {{note}} |

<!-- Record returned independent verdicts with unit, scope and recoverable subject
     revision in Note. Current approval requires applicable green coverage;
     preserve superseded evidence as labeled history, not current approval. -->

## Tasks

<!-- Child tracker paths only; delivery state stays in the tracker. -->

## Scoring

| Field | Value | Basis     |
| ----- | ----- | --------- |
| value | 0     | {{basis}} |
| risk  | 0     | {{basis}} |
