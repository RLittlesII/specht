---
title: "Specification: Traceability reconciliation"
description: "Expose missing links and test outcomes within an explicit project scope."
type: feature
id: "F5"
epic: "0201"
spec_status: draft
status: ready-for-architecture
priority: med
value: 0
risk: 0
rank: 0
scored_by: null
scored_on: null
domain: "Code traceability"
author: "spec-author"
milestone: null
children: []
depends_on: ["F1", "F3"]
blocks: ["F6"]
spikes: []
created: "2026-10-17"
updated: "2026-10-17"
github_issue: null
synced_at: null
---

# Specification: Traceability reconciliation

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-17 -->

Expose missing links and test outcomes within an explicit project scope. This draft decomposes the owner-selected model; unresolved public contracts block implementation, not the recorded adoption decision.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-17 -->

| #   | Persona             | Need                                                                     | Pain Point Today                                                 |
| --- | ------------------- | ------------------------------------------------------------------------ | ---------------------------------------------------------------- |
| 1   | Consumer maintainer | Expose missing links and test outcomes within an explicit project scope. | References and test names can drift without an inspectable link. |

### Assumptions

| ID  | Assumption                                                                                                                                                                                                                             |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | Originally, area folders were assumed not to select project boundaries. RESOLVED, 2026-10-17: owner review adopts the Analyzers, CodeFixes and deployment CodeAnalysis project homes (ADR-0016); implementation internals remain open. |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-17 -->

| ID    | Claim                                                                                                                                                                                                                                 | Source                              | Status |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------- | ------ |
| B-001 | Given a claim bound to a discovered xUnit or Playwright .NET test and a matching result with no scenario policy, this Feature ensures reconciliation presents the claim-to-method-to-case-to-result chain without requiring Reqnroll. | Epic 0201; decision 0001 in 0201-F1 | Active |
| B-002 | Given a declared repository scope omitting a required test project or input, this Feature ensures the report marks coverage incomplete and identifies the missing input.                                                              | Epic 0201; decision 0001 in 0201-F1 | Active |
| B-003 | Given a resolved binding with a failed, skipped or absent test result, this Feature ensures the report keeps that outcome distinct from a passing result.                                                                             | Epic 0201; decision 0001 in 0201-F1 | Active |
| B-004 | Given an explicit policy requiring a link absent from a claim's evidence, this Feature ensures the report identifies that missing link instead of treating reference resolution as complete coverage.                                 | Epic 0201; decision 0001 in 0201-F1 | Active |
| B-005 | Given scenario bridge evidence supplied for part of a declared scope, this Feature ensures those scenario links augment the direct chains without inventing links for other tests.                                                    | Epic 0201; decision 0001 in 0201-F1 | Active |
| B-006 | Given a declared specification claim requiring test evidence with no test binding at all, this Feature ensures reconciliation reports the missing evidence starting from that claim even when no method names it.                     | Epic 0201; decision 0001 in 0201-F1 | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-17 -->

| ID  | Constraint                                                                                                    | Rules Out                                       |
| --- | ------------------------------------------------------------------------------------------------------------- | ----------------------------------------------- |
| C-1 | Resolved reference, required-link completeness and passing execution are separate evidence states.            | Reporting resolved IDs as passing behavior.     |
| C-2 | Reports are deterministic for the same supplied inputs, offline and read-only with repository-relative paths. | Nonrepeatable or machine-specific reports.      |
| C-3 | One compilation cannot assert completeness for unselected or omitted projects.                                | Complete-coverage claims over missing projects. |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-17 -->

| #   | Item                                                                    | Exclusion Reason                      |
| --- | ----------------------------------------------------------------------- | ------------------------------------- |
| 1   | Automatically declaring assertion adequacy or architectural compliance. | This agreement does not authorize it. |
| 2   | Launching test runs or querying remote test services during checking.   | This agreement does not authorize it. |

## 6. Concern Separation

<!-- last written by: implementer, 2026-10-17 -->

Not yet designed; no production implementation is authorized by this draft.

## 7. Technical Design

<!-- last written by: implementer, 2026-10-17 -->

Deferred until the public contract questions in § 11 are resolved. The isolated spike informs those decisions but is not production architecture.

## 8. Testing Strategy

<!-- last written by: test-writer, 2026-10-17 -->

The companion scenarios state the acceptance obligations. Production step definitions and mechanism tests are not written; each claim remains Missing in § 9. Spike results do not promote these rows to Covered.

## 9. Traceability Matrix

<!-- last written by: test-writer, 2026-10-17 -->

| Claim ID | Scenario            | Test    | Status  |
| -------- | ------------------- | ------- | ------- |
| B-001    | Direct test chain   | Missing | Missing |
| B-002    | Incomplete scope    | Missing | Missing |
| B-003    | Outcome distinction | Missing | Missing |
| B-004    | Policy gaps         | Missing | Missing |
| B-005    | Optional scenarios  | Missing | Missing |
| B-006    | Unbound claim       | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-17 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-17 -->

| ID   | Question                                                                                        | Blocks                    | Resolution |
| ---- | ----------------------------------------------------------------------------------------------- | ------------------------- | ---------- |
| OQ-1 | Which links are required by each selectable consumer policy, and what exemptions are supported? | Production implementation | Open       |
| OQ-2 | What report surface and stale-result policy are public?                                         | Production implementation | Open       |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-17 -->

| Section | Status | Reviewer | Note                                                                                                              |
| ------- | ------ | -------- | ----------------------------------------------------------------------------------------------------------------- |
| 1-12    | 🟡     | Pending  | Owner selected the model; this decomposed Feature has not received detailed agreement or implementation sign-off. |

## Tasks

No delivery items cut; the agreement precedes implementation planning.

## Scoring

| Field | Value | Basis           |
| ----- | ----- | --------------- |
| value | 0     | Not yet scored. |
| risk  | 0     | Not yet scored. |
