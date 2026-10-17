---
title: "Specification: Binding identity"
description: "Resolve stable claim and ADR identities using consumer grammars without a test-framework dependency."
type: feature
id: "F1"
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
depends_on: []
blocks: ["F2", "F3", "F4", "F5", "F6"]
spikes: []
created: "2026-10-17"
updated: "2026-10-17"
github_issue: null
synced_at: null
---

# Specification: Binding identity

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-17 -->

Resolve stable claim and ADR identities using consumer grammars without a test-framework dependency. This draft decomposes the owner-selected model; unresolved public contracts block implementation, not the recorded adoption decision.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-17 -->

| #   | Persona             | Need                                                                                                 | Pain Point Today                                                 |
| --- | ------------------- | ---------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------- |
| 1   | Consumer maintainer | Resolve stable claim and ADR identities using consumer grammars without a test-framework dependency. | References and test names can drift without an inspectable link. |

### Assumptions

| ID  | Assumption                                                                                                                                                                                                                             |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | Originally, area folders were assumed not to select project boundaries. RESOLVED, 2026-10-17: owner review adopts the Analyzers, CodeFixes and deployment CodeAnalysis project homes (ADR-0016); implementation internals remain open. |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-17 -->

| ID    | Claim                                                                                                                                                                                         | Source                              | Status |
| ----- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------- | ------ |
| B-001 | Given a manifest defining custom Feature and claim grammars and records using them, this Feature ensures references are validated and resolved using those configured grammars.               | Epic 0201; decision 0001 in 0201-F1 | Active |
| B-002 | Given two Features declaring the same local claim ID, this Feature ensures a qualified claim reference resolves only in its declared owning Feature.                                          | Epic 0201; decision 0001 in 0201-F1 | Active |
| B-003 | Given a consumer whose configured model has no epic tier, this Feature ensures a Feature and claim reference resolves without inventing an epic.                                              | Epic 0201; decision 0001 in 0201-F1 | Active |
| B-004 | Given a repository ADR and Feature ADRs sharing a local number, this Feature ensures an ADR reference resolves only in its declared repository or Feature scope.                              | Epic 0201; decision 0001 in 0201-F1 | Active |
| B-005 | Given the same specification inputs and declared references with no Reqnroll or test-framework assemblies, this Feature ensures reference resolution yields the same identities and findings. | Epic 0201; decision 0001 in 0201-F1 | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-17 -->

| ID  | Constraint                                                                                                                | Rules Out                                                 |
| --- | ------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------- |
| C-1 | Reuse manifest identity and discovery semantics (0001-F5 C-2); no parallel regex configuration or first-match resolution. | A second ID parser or implicit first-match lookup.        |
| C-2 | Keep format validation distinct from existence and ambiguity checks.                                                      | Treating a regex match as a resolving reference.          |
| C-3 | Binding identity depends on no Reqnroll, xUnit or Playwright types.                                                       | Mandatory test-framework packages in identity resolution. |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-17 -->

| #   | Item                                                                                                                | Exclusion Reason                      |
| --- | ------------------------------------------------------------------------------------------------------------------- | ------------------------------------- |
| 1   | Public XML encoding and member kinds are owned by F2.                                                               | This agreement does not authorize it. |
| 2   | Test discovery, execution and scenario links belong to adapters; a resolved reference alone is no behavioral proof. | This agreement does not authorize it. |

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

| Claim ID | Scenario               | Test    | Status  |
| -------- | ---------------------- | ------- | ------- |
| B-001    | Consumer grammars      | Missing | Missing |
| B-002    | Owning scope           | Missing | Missing |
| B-003    | No epic tier           | Missing | Missing |
| B-004    | ADR ownership          | Missing | Missing |
| B-005    | Framework independence | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-17 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-17 -->

| ID   | Question                                                                                                   | Blocks                    | Resolution |
| ---- | ---------------------------------------------------------------------------------------------------------- | ------------------------- | ---------- |
| OQ-1 | Which public encoding carries local Feature, optional epic and ADR ownership without assuming a delimiter? | Production implementation | Open       |
| OQ-2 | Which claim and ADR lifecycle states are eligible targets?                                                 | Production implementation | Open       |

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
