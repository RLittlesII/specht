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

| ID    | Claim                                                                                                                                                                    | Source                                             | Status  |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | -------------------------------------------------- | ------- |
| B-001 | Given a scenario carrying a local claim tag and a determined effective scope, this Feature resolves that tag through the same identity contract as a code binding.       | Epic 0201; decision 0001 in 0201-F1; decision 0001 | Amended |
| B-002 | Given an outline producing two discovered test cases, this Feature ensures each case links to the originating scenario and row through generator or runner identity.     | Epic 0201; decision 0001 in 0201-F1                | Active  |
| B-003 | Given two scenarios sharing a handwritten step method, this Feature ensures the scenario-to-test links remain separate and are not inferred from step documentation.     | Epic 0201; decision 0001 in 0201-F1                | Active  |
| B-004 | Given a selected Reqnroll project missing its required generator or discovery input, this Feature ensures the bridge reports an unresolved link.                         | Epic 0201; decision 0001 in 0201-F1                | Active  |
| B-005 | Given a consumer selecting only bound xUnit or Playwright .NET tests, this Feature ensures the absence of Reqnroll scenarios does not invalidate the core test bindings. | Epic 0201; decision 0001 in 0201-F1                | Active  |
| B-006 | Given no explicit Scenario or Outline scope, this Feature uses the Gherkin Feature-level scope as the scenario default.                                                  | Decision 0001                                      | Active  |
| B-007 | Given an explicit Scenario or Outline scope, this Feature replaces the complete Feature-level scope without merging components.                                          | Decision 0001                                      | Active  |
| B-008 | Given scenario-to-case metadata from a consumer claim bridge or optional Reqnroll integration, this Feature maps it through the common runner-evidence contract.         | Decision 0001                                      | Active  |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-17 -->

| ID  | Constraint                                                                                           | Rules Out                                        |
| --- | ---------------------------------------------------------------------------------------------------- | ------------------------------------------------ |
| C-1 | Reuse existing claim-tag parsing and grammar semantics; do not create a competing Gherkin validator. | A competing interpretation of claim tags.        |
| C-2 | A scenario, generated test case and shared step method are different records.                        | Step annotations standing in for scenario links. |
| C-3 | The bridge is optional and cannot impose Reqnroll dependencies on the binding model.                 | A mandatory scenario framework for direct tests. |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-17 -->

| #   | Item                                                             | Exclusion Reason                                                      |
| --- | ---------------------------------------------------------------- | --------------------------------------------------------------------- |
| 1   | Changing this repository's own Gherkin-first development policy. | This agreement does not authorize it.                                 |
| 2   | Using scenario titles or step method names as identity.          | This agreement does not authorize it.                                 |
| 3   | Taking ownership of or replacing consumer convention analyzers.  | Decision 0001 reuses the consumer bridge through the common contract. |

## 6. Concern Separation

<!-- last written by: implementer, 2026-10-17 -->

Not yet designed; no production implementation is authorized by this draft.

## 7. Technical Design

<!-- last written by: implementer, 2026-10-17 -->

Decision 0001 settles scope precedence and bridge ownership, using 0201-F3's
common evidence contract. Exact tag/configuration encoding and generator/runner
metadata extraction remain technical-design work; the isolated spike is not
production architecture.

## 8. Testing Strategy

<!-- last written by: test-writer, 2026-10-17 -->

The companion scenarios state the acceptance obligations. Production step definitions and mechanism tests are not written; each claim remains Missing in § 9. Spike results do not promote these rows to Covered.

## 9. Traceability Matrix

<!-- last written by: test-writer, 2026-10-17 -->

| Claim ID | Scenario                         | Test    | Status  |
| -------- | -------------------------------- | ------- | ------- |
| B-001    | Scenario claim                   | Missing | Missing |
| B-002    | Scenario outline                 | Missing | Missing |
| B-003    | Shared steps                     | Missing | Missing |
| B-004    | Missing generated input          | Missing | Missing |
| B-005    | Bridge disabled                  | Missing | Missing |
| B-006    | Feature scope default            | Missing | Missing |
| B-007    | Complete scenario scope override | Missing | Missing |
| B-008    | Common scenario bridge contract  | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-17 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-17 -->

| ID   | Question                                                                           | Blocks                                                | Resolution                                                                                                                                                                                                                                                                                                                                 |
| ---- | ---------------------------------------------------------------------------------- | ----------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| OQ-1 | What is Feature-level versus Scenario-level scope precedence?                      | Tag/configuration encoding design                     | RESOLVED for behavior, 2026-10-17: decision 0001 uses Feature-level scope by default; explicit Scenario/Outline scope replaces it completely without field merging. Local claim tags resolve in effective scope.                                                                                                                           |
| OQ-2 | Which Reqnroll generator artifacts and versions form a supported adapter contract? | Concrete metadata extraction and compatibility design | RESOLVED for integration policy, 2026-10-17: decision 0001 maps generator/runner scenario-to-case metadata through the common evidence contract, reuses consumer claim bridges, retains consumer ownership of convention analyzers and keeps Reqnroll optional. Concrete extraction/compatibility and wire fields remain technical design. |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-17 -->

| Section | Status | Reviewer | Note                                                                                                              |
| ------- | ------ | -------- | ----------------------------------------------------------------------------------------------------------------- |
| 1-12    | 🟡     | Pending  | Owner selected the model; this decomposed Feature has not received detailed agreement or implementation sign-off. |

## Tasks

- [0245: Implement the optional scenario evidence bridge](../.issue/0245-optional-scenario-bridge.yml)

## Scoring

| Field | Value | Basis           |
| ----- | ----- | --------------- |
| value | 0     | Not yet scored. |
| risk  | 0     | Not yet scored. |
