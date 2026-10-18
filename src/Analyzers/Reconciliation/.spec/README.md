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

| ID    | Claim                                                                                                                                                                                                                                         | Source                              | Status  |
| ----- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------- | ------- |
| B-001 | Given a claim bound to a discovered test through the common runner-evidence contract and a matching result with no scenario policy, this Feature presents the claim-to-method-to-case-to-result chain without requiring Reqnroll.             | Epic 0201; F3 decision 0001         | Amended |
| B-002 | Given a declared repository scope omitting a required test project or input, this Feature ensures the report marks coverage incomplete and identifies the missing input.                                                                      | Epic 0201; decision 0001 in 0201-F1 | Active  |
| B-003 | Given a resolved binding with a failed, skipped or absent test result, this Feature ensures the report keeps that outcome distinct from a passing result.                                                                                     | Epic 0201; decision 0001 in 0201-F1 | Active  |
| B-004 | Given an explicit policy requiring a link absent from a claim's evidence, this Feature ensures the report identifies that missing link instead of treating reference resolution as complete coverage.                                         | Epic 0201; decision 0001 in 0201-F1 | Active  |
| B-005 | Given scenario bridge evidence supplied for part of a declared scope, this Feature ensures those scenario links augment the direct chains without inventing links for other tests.                                                            | Epic 0201; decision 0001 in 0201-F1 | Active  |
| B-006 | Given a declared specification claim requiring test evidence with no test binding at all, this Feature ensures reconciliation reports the missing evidence starting from that claim even when no method names it.                             | Epic 0201; decision 0001 in 0201-F1 | Active  |
| B-007 | Given a binding to a resolving target classified as ineligible under 0201-F1's lifecycle policy, this Feature excludes that binding from current required evidence while retaining the historical link's target identity and lifecycle state. | 0201-F1 decision 0004               | Active  |
| B-008 | Given a non-exempt current claim in the selected Feature scope, this Feature requires at least one valid test binding.                                                                                                                        | 0201-F6 decision 0003               | Active  |
| B-009 | Given an implementation or scenario link kind, this Feature requires that kind only when the consumer explicitly enables its requirement.                                                                                                     | 0201-F6 decision 0003               | Active  |
| B-010 | Given an explicitly reasoned coverage exemption, this Feature presents it as exempt rather than covered.                                                                                                                                      | 0201-F6 decision 0003               | Active  |
| B-011 | Given a declared set of project and target-framework combinations, this Feature reports complete analysis only when current required inputs are available for every declared combination.                                                     | 0201-F6 decision 0004               | Active  |
| B-012 | Given stale or mismatched discovery or results, this Feature retains their stale or mismatched state and excludes them from current discovery or execution evidence.                                                                          | 0201-F6 decision 0004               | Active  |
| B-013 | Given absent, stale or mismatched test results, this Feature evaluates binding validity separately from those execution-evidence states.                                                                                                      | 0201-F6 decision 0004               | Active  |
| B-014 | Given supplied traceability evidence, this Feature presents reconciliation through `specht trace` with a human summary or separate versioned JSON output.                                                                                     | Decision 0001                       | Active  |
| B-015 | Given the new trace report contract, this Feature preserves the existing checker report contract unchanged.                                                                                                                                   | Decision 0001                       | Active  |
| B-016 | Given supplied discovery and result artifacts, this Feature reconciles them without launching test execution.                                                                                                                                 | Decision 0001                       | Active  |
| B-017 | Given complete required links, this Feature succeeds only when all current cases supplying non-exempt required test evidence have passing outcomes; missing or nonpassing outcomes fail the default gate.                                     | Decision 0002                       | Active  |

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

0201-F6 decision 0003 defines required-link and exemption policy. Detailed
production design still waits for the report/input contracts in § 11; the
isolated spike is not production architecture.

## 8. Testing Strategy

<!-- last written by: test-writer, 2026-10-17 -->

The companion scenarios state the acceptance obligations. Production step definitions and mechanism tests are not written; each claim remains Missing in § 9. Spike results do not promote these rows to Covered.

## 9. Traceability Matrix

<!-- last written by: test-writer, 2026-10-17 -->

| Claim ID | Scenario                             | Test    | Status  |
| -------- | ------------------------------------ | ------- | ------- |
| B-001    | Direct test chain                    | Missing | Missing |
| B-002    | Incomplete scope                     | Missing | Missing |
| B-003    | Outcome distinction                  | Missing | Missing |
| B-004    | Policy gaps                          | Missing | Missing |
| B-005    | Optional scenarios                   | Missing | Missing |
| B-006    | Unbound claim                        | Missing | Missing |
| B-007    | Ineligible target evidence           | Missing | Missing |
| B-008    | Default test requirement             | Missing | Missing |
| B-009    | Optional link requirements           | Missing | Missing |
| B-010    | Visible coverage exemption           | Missing | Missing |
| B-011    | Declared compilation completeness    | Missing | Missing |
| B-012    | Stale evidence state                 | Missing | Missing |
| B-013    | Binding and execution independence   | Missing | Missing |
| B-014    | Trace report surface                 | Missing | Missing |
| B-015    | Checker report isolation             | Missing | Missing |
| B-016    | Consume supplied evidence            | Missing | Missing |
| B-017    | Passing outcomes required by default | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-17 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-17 -->

| ID   | Question                                                                                        | Blocks                                | Resolution                                                                                                                                                                                                                                                                                                                                     |
| ---- | ----------------------------------------------------------------------------------------------- | ------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | Which links are required by each selectable consumer policy, and what exemptions are supported? | None for coverage policy              | RESOLVED, 2026-10-17: 0201-F6 decision 0003 requires a valid test binding for each non-exempt current claim in selected Feature scope. Implementation and scenario requirements are opt-in. Exemptions require explicit reasons and remain visible as exempt, never covered. Configuration encoding remains with 0201-F6 OQ-1.                 |
| OQ-2 | What report surface and stale-result policy are public?                                         | Report/evidence schema and CLI design | RESOLVED for behavior, 2026-10-17: F6 decision 0004 defines freshness and completeness. Decision 0001 adopts specht trace with human summary and separate versioned JSON, preserves the checker report and consumes normal workflow artifacts without launching tests. Detailed schema, options and exit-code mapping remain technical design. |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-17 -->

| Section | Status | Reviewer | Note                                                                                                              |
| ------- | ------ | -------- | ----------------------------------------------------------------------------------------------------------------- |
| 1-12    | 🟡     | Pending  | Owner selected the model; this decomposed Feature has not received detailed agreement or implementation sign-off. |

## Tasks

- [0246: Reconcile exported trace evidence in the CLI](../.issue/0246-trace-artifact-reconciliation.yml)

## Scoring

| Field | Value | Basis           |
| ----- | ----- | --------------- |
| value | 0     | Not yet scored. |
| risk  | 0     | Not yet scored. |
