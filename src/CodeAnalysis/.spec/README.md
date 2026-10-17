---
title: "Specification: Consumer analysis adoption"
description: "Apply binding enforcement consistently to an explicitly selected consumer scope."
type: feature
id: "F6"
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
depends_on: ["F1", "F2", "F3", "F5"]
blocks: []
spikes: []
created: "2026-10-17"
updated: "2026-10-17"
github_issue: null
synced_at: null
---

# Specification: Consumer analysis adoption

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-17 -->

Apply binding enforcement consistently to an explicitly selected consumer scope. This draft decomposes the owner-selected model; unresolved public contracts block implementation, not the recorded adoption decision.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-17 -->

| #   | Persona             | Need                                                                             | Pain Point Today                                                 |
| --- | ------------------- | -------------------------------------------------------------------------------- | ---------------------------------------------------------------- |
| 1   | Consumer maintainer | Apply binding enforcement consistently to an explicitly selected consumer scope. | References and test names can drift without an inspectable link. |

### Assumptions

| ID  | Assumption                                                                                                                                                                                                                             |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | Originally, area folders were assumed not to select project boundaries. RESOLVED, 2026-10-17: owner review adopts the Analyzers, CodeFixes and deployment CodeAnalysis project homes (ADR-0016); implementation internals remain open. |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-17 -->

| ID    | Claim                                                                                                                                                                                               | Source                              | Status |
| ----- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------- | ------ |
| B-001 | Given a consumer selecting a subset of projects and declarations for enforcement, this Feature ensures only that selected scope is required to carry bindings and the scope is visible in evidence. | Epic 0201; decision 0001 in 0201-F1 | Active |
| B-002 | Given the same valid pinned configuration and specification inputs supplied in editor and build, this Feature ensures the same binding yields the same resolution verdict.                          | Epic 0201; decision 0001 in 0201-F1 | Active |
| B-003 | Given a selected analysis scope with missing or invalid manifest or specification inputs, this Feature ensures analysis reports the input failure rather than silently reporting clean coverage.    | Epic 0201; decision 0001 in 0201-F1 | Active |
| B-004 | Given a consumer selecting xUnit and Playwright .NET analysis without the scenario bridge, this Feature ensures adoption requires no Reqnroll installation.                                         | Epic 0201; decision 0001 in 0201-F1 | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-17 -->

| ID  | Constraint                                                                                                           | Rules Out                                                    |
| --- | -------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------ |
| C-1 | No production delivery before the public diagnostic/version contract and input contract are agreed.                  | Shipping an unspecified public diagnostic contract.          |
| C-2 | The repository-wide Markdown checker remains independently usable without compiling consumer code.                   | Requiring Roslyn compilation to check Markdown.              |
| C-3 | Keep installation and policy explicit; reuse consumer grammar configuration rather than add analyzer regex settings. | Implicit enforcement scope or duplicate regex configuration. |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-17 -->

| #   | Item                                                                                        | Exclusion Reason                      |
| --- | ------------------------------------------------------------------------------------------- | ------------------------------------- |
| 1   | Replacing the existing specification checker or silently widening a pinned SPEC vocabulary. | This agreement does not authorize it. |
| 2   | Generic convention analyzers or a mandatory test-framework migration.                       | This agreement does not authorize it. |

## 6. Concern Separation

<!-- last written by: implementer, 2026-10-17 -->

Not yet designed; no production implementation is authorized by this draft.

## 7. Technical Design

<!-- last written by: implementer, 2026-10-17 -->

[ADR-0016](../../../.spec/adr/0016-analyzer-codefix-deployment-projects.md) adopts
`src/Analyzers` (`Specht.Analysis`), `src/CodeFixes` (`Specht.Codefixes`) and
`src/CodeAnalysis` (`Specht.CodeAnalysis`, deployment). The deployment project
references both non-packable component projects; CodeFixes references Analyzers.
Analyzer loading must not depend on CodeFixes or its Workspaces dependencies.

These are agreed project boundaries, not built projects. Remaining implementation
design waits for § 11. The isolated spike does not select production targets or
waive the engine/host architecture gate.

## 8. Testing Strategy

<!-- last written by: test-writer, 2026-10-17 -->

The companion scenarios state the acceptance obligations. Production step definitions and mechanism tests are not written; each claim remains Missing in § 9. Spike results do not promote these rows to Covered.

## 9. Traceability Matrix

<!-- last written by: test-writer, 2026-10-17 -->

| Claim ID | Scenario            | Test    | Status  |
| -------- | ------------------- | ------- | ------- |
| B-001    | Selected scope      | Missing | Missing |
| B-002    | Editor build parity | Missing | Missing |
| B-003    | Invalid inputs      | Missing | Missing |
| B-004    | Optional Reqnroll   | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-17 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-17 -->

| ID   | Question                                                                                                                                                                                                                                                                                                                         | Blocks                    | Resolution |
| ---- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------- | ---------- |
| OQ-1 | Which deployment identity, packages, input transport and configuration surface ship first? RESOLVED IN PART, 2026-10-17: Specht.CodeAnalysis owns deployment; Specht.Analysis and Specht.Codefixes are separate components (ADR-0016). Installation/versioning, public package ID and input/configuration contracts remain open. | Production implementation | Open       |
| OQ-2 | What severities, exclusions and suppressions allow gradual adoption?                                                                                                                                                                                                                                                             | Production implementation | Open       |

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
