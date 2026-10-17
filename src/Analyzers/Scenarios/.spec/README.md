---
title: "Specification: Optional scenario bridge"
description: "Add scenario-to-test links when a consumer opts into Reqnroll."
type: feature
id: "F4"
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
blocks: []
spikes: []
created: "2026-10-17"
updated: "2026-10-17"
github_issue: null
synced_at: null
---

# Specification: Optional scenario bridge

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-17 -->

Add scenario-to-test links when a consumer opts into Reqnroll. This draft decomposes the owner-selected model; unresolved public contracts block implementation, not the recorded adoption decision.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-17 -->

| #   | Persona             | Need                                                           | Pain Point Today                                                 |
| --- | ------------------- | -------------------------------------------------------------- | ---------------------------------------------------------------- |
| 1   | Consumer maintainer | Add scenario-to-test links when a consumer opts into Reqnroll. | References and test names can drift without an inspectable link. |

### Assumptions

| ID  | Assumption                                                                                                                                                                                                                             |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | Originally, area folders were assumed not to select project boundaries. RESOLVED, 2026-10-17: owner review adopts the Analyzers, CodeFixes and deployment CodeAnalysis project homes (ADR-0016); implementation internals remain open. |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-17 -->

| ID    | Claim                                                                                                                                                                    | Source                              | Status |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ----------------------------------- | ------ |
| B-001 | Given a scenario carrying a claim tag in its owning Feature scope, this Feature ensures the tag resolves through the same identity contract as a code binding.           | Epic 0201; decision 0001 in 0201-F1 | Active |
| B-002 | Given an outline producing two discovered test cases, this Feature ensures each case links to the originating scenario and row through generator or runner identity.     | Epic 0201; decision 0001 in 0201-F1 | Active |
| B-003 | Given two scenarios sharing a handwritten step method, this Feature ensures the scenario-to-test links remain separate and are not inferred from step documentation.     | Epic 0201; decision 0001 in 0201-F1 | Active |
| B-004 | Given a selected Reqnroll project missing its required generator or discovery input, this Feature ensures the bridge reports an unresolved link.                         | Epic 0201; decision 0001 in 0201-F1 | Active |
| B-005 | Given a consumer selecting only bound xUnit or Playwright .NET tests, this Feature ensures the absence of Reqnroll scenarios does not invalidate the core test bindings. | Epic 0201; decision 0001 in 0201-F1 | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-17 -->

| ID  | Constraint                                                                                           | Rules Out                                        |
| --- | ---------------------------------------------------------------------------------------------------- | ------------------------------------------------ |
| C-1 | Reuse existing claim-tag parsing and grammar semantics; do not create a competing Gherkin validator. | A competing interpretation of claim tags.        |
| C-2 | A scenario, generated test case and shared step method are different records.                        | Step annotations standing in for scenario links. |
| C-3 | The bridge is optional and cannot impose Reqnroll dependencies on the binding model.                 | A mandatory scenario framework for direct tests. |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-17 -->

| #   | Item                                                             | Exclusion Reason                      |
| --- | ---------------------------------------------------------------- | ------------------------------------- |
| 1   | Changing this repository's own Gherkin-first development policy. | This agreement does not authorize it. |
| 2   | Using scenario titles or step method names as identity.          | This agreement does not authorize it. |

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

| Claim ID | Scenario                | Test    | Status  |
| -------- | ----------------------- | ------- | ------- |
| B-001    | Scenario claim          | Missing | Missing |
| B-002    | Scenario outline        | Missing | Missing |
| B-003    | Shared steps            | Missing | Missing |
| B-004    | Missing generated input | Missing | Missing |
| B-005    | Bridge disabled         | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-17 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-17 -->

| ID   | Question                                                                           | Blocks                    | Resolution |
| ---- | ---------------------------------------------------------------------------------- | ------------------------- | ---------- |
| OQ-1 | What is Feature-level versus Scenario-level scope precedence?                      | Production implementation | Open       |
| OQ-2 | Which Reqnroll generator artifacts and versions form a supported adapter contract? | Production implementation | Open       |

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
