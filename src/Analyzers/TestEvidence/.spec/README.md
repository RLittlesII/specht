---
title: "Specification: Test evidence adapters"
description: "Map explicit test-method bindings to discovered test cases without requiring Gherkin."
type: feature
id: "F3"
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
depends_on: ["F1", "F2"]
blocks: ["F4", "F5", "F6"]
spikes: []
created: "2026-10-17"
updated: "2026-10-17"
github_issue: null
synced_at: null
---

# Specification: Test evidence adapters

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-17 -->

Map explicit test-method bindings to discovered test cases without requiring Gherkin. This draft decomposes the owner-selected model; unresolved public contracts block implementation, not the recorded adoption decision.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-17 -->

| #   | Persona             | Need                                                                                  | Pain Point Today                                                 |
| --- | ------------------- | ------------------------------------------------------------------------------------- | ---------------------------------------------------------------- |
| 1   | Consumer maintainer | Map explicit test-method bindings to discovered test cases without requiring Gherkin. | References and test names can drift without an inspectable link. |

### Assumptions

| ID  | Assumption                                                                                                                                                                                                                             |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | Originally, area folders were assumed not to select project boundaries. RESOLVED, 2026-10-17: owner review adopts the Analyzers, CodeFixes and deployment CodeAnalysis project homes (ADR-0016); implementation internals remain open. |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-17 -->

| ID    | Claim                                                                                                                                                                                        | Source                              | Status |
| ----- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------- | ------ |
| B-001 | Given a bound xUnit fact in a selected test project without Reqnroll, this Feature ensures its discovered test case is linked to that method and claim.                                      | Epic 0201; decision 0001 in 0201-F1 | Active |
| B-002 | Given a bound xUnit theory discovering two data rows without Reqnroll, this Feature ensures both test cases link to the same bound method without treating them as two methods.              | Epic 0201; decision 0001 in 0201-F1 | Active |
| B-003 | Given a bound Playwright .NET test hosted by the supported xUnit adapter without Reqnroll, this Feature ensures the discovered case links through its CSharp method to the declared claim.   | Epic 0201; decision 0001 in 0201-F1 | Active |
| B-004 | Given a selected test project with no supported discovery adapter, this Feature ensures the project is reported as unsupported rather than empty complete coverage.                          | Epic 0201; decision 0001 in 0201-F1 | Active |
| B-005 | Given two discovered cases with duplicate display names but distinct runner identities, this Feature ensures they remain distinct and correlate through runner identity and method metadata. | Epic 0201; decision 0001 in 0201-F1 | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-17 -->

| ID  | Constraint                                                                                                          | Rules Out                                               |
| --- | ------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------- |
| C-1 | The binding model is framework-independent; adapter-specific recognition does not alter reference semantics.        | Framework-specific reference grammar.                   |
| C-2 | Reqnroll and Gherkin are optional; source bindings, discovered cases and execution results remain distinct records. | Treating a method declaration as an executed test case. |
| C-3 | Discovery and result ingestion use explicit offline inputs; analysis does not execute browsers or fetch providers.  | Running tests or contacting services during analysis.   |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-17 -->

| #   | Item                                                                                  | Exclusion Reason                      |
| --- | ------------------------------------------------------------------------------------- | ------------------------------------- |
| 1   | JavaScript and TypeScript Playwright bindings; Roslyn analyzes CSharp.                | This agreement does not authorize it. |
| 2   | Claim inference from test display names, assertion text or scenario titles.           | This agreement does not authorize it. |
| 3   | Supporting every xUnit version, NUnit or MSTest is not implied by Playwright support. | This agreement does not authorize it. |

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
| B-001    | xUnit fact          | Missing | Missing |
| B-002    | xUnit theory        | Missing | Missing |
| B-003    | Playwright dotnet   | Missing | Missing |
| B-004    | Unsupported adapter | Missing | Missing |
| B-005    | Stable correlation  | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-17 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-17 -->

| ID   | Question                                                                            | Blocks                    | Resolution |
| ---- | ----------------------------------------------------------------------------------- | ------------------------- | ---------- |
| OQ-1 | Which xUnit versions, dynamic theory discovery modes and output formats ship first? | Production implementation | Open       |
| OQ-2 | What runner metadata and source identity reject stale discovery and results?        | Production implementation | Open       |

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
