---
title: "Specification: CSharp binding authoring"
description: "Use type defaults to author explicit member bindings that remain stable across moves."
type: feature
id: "F2"
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
depends_on: ["F1"]
blocks: ["F3", "F6"]
spikes: []
created: "2026-10-17"
updated: "2026-10-17"
github_issue: null
synced_at: null
---

# Specification: CSharp binding authoring

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-17 -->

Use type defaults to author explicit member bindings that remain stable across moves. This draft decomposes the owner-selected model; unresolved public contracts block implementation, not the recorded adoption decision.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-17 -->

| #   | Persona             | Need                                                                                  | Pain Point Today                                                 |
| --- | ------------------- | ------------------------------------------------------------------------------------- | ---------------------------------------------------------------- |
| 1   | Consumer maintainer | Use type defaults to author explicit member bindings that remain stable across moves. | References and test names can drift without an inspectable link. |

### Assumptions

| ID  | Assumption                                                                                                         |
| --- | ------------------------------------------------------------------------------------------------------------------ |
| A-1 | Area folders organize the agreement; no assembly, package or runtime architecture is committed by their placement. |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-17 -->

| ID    | Claim                                                                                                                                                                                                | Source                              | Status |
| ----- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------- | ------ |
| B-001 | Given a member carrying a complete explicit Feature and claim binding, this Feature ensures checked traceability uses that member identity regardless of its containing type default.                | Epic 0201; decision 0001 in 0201-F1 | Active |
| B-002 | Given a member carrying only a claim under a type default, this Feature ensures analysis reports that the member lacks explicit identity and does not count it as checked evidence.                  | Epic 0201; decision 0001 in 0201-F1 | Active |
| B-003 | Given a claim-only member with one unambiguous type default and an invoked repair, this Feature ensures the repair persists its complete target identity while preserving other XML documentation.   | Epic 0201; decision 0001 in 0201-F1 | Active |
| B-004 | Given missing, conflicting or ambiguous default targets, this Feature ensures no repair silently selects a target.                                                                                   | Epic 0201; decision 0001 in 0201-F1 | Active |
| B-005 | Given a malformed, unresolved or ambiguous member reference, this Feature ensures analysis reports a diagnostic at that declaration with its target reference.                                       | Epic 0201; decision 0001 in 0201-F1 | Active |
| B-006 | Given one member explicitly verifying two distinct declared claims, this Feature ensures both bindings are retained independently.                                                                   | Epic 0201; decision 0001 in 0201-F1 | Active |
| B-007 | Given a selected test method with no binding documentation, this Feature ensures analysis reports the missing binding and an invoked fix offers existing claim choices without inventing the target. | Epic 0201; decision 0001 in 0201-F1 | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-17 -->

| ID  | Constraint                                                                                                                             | Rules Out                                                      |
| --- | -------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------- |
| C-1 | Type defaults supply authoring context only; checked evidence never inherits identity.                                                 | Implicit inheritance supplying checked evidence.               |
| C-2 | An explicit member target supplies its complete identity; no partial merge with type defaults.                                         | Combining an explicit target with a different enclosing scope. |
| C-3 | Analysis is read-only. Only an invoked code fix edits existing documentation; it cannot create claims or change specification records. | Unsolicited writes or invented specification content.          |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-17 -->

| #   | Item                                                                                                   | Exclusion Reason                      |
| --- | ------------------------------------------------------------------------------------------------------ | ------------------------------------- |
| 1   | Selecting assertion adequacy or proving a method implements a claim.                                   | This agreement does not authorize it. |
| 2   | Shipped source generation and automatic specification editing.                                         | This agreement does not authorize it. |
| 3   | A package name, diagnostic vocabulary or broad CSharp declaration support is not chosen by this draft. | This agreement does not authorize it. |

## 6. Concern Separation

<!-- last written by: implementer, 2026-10-17 -->

Not yet designed; no production implementation is authorized by this draft.

## 7. Technical Design

<!-- last written by: implementer, 2026-10-17 -->

Deferred until the public contract questions in § 11 are resolved. The isolated spike informs those decisions but is not production architecture.

## 8. Testing Strategy

<!-- last written by: test-writer, 2026-10-17 -->

The companion scenarios state the acceptance obligations. [Decision 0001](decisions/0001-analyzer-testing.md) selects the production Roslyn test harness and direct assertions without Verify. Production step definitions and mechanism tests are not written; each claim remains Missing in § 9. Spike results do not promote these rows to Covered.

## 9. Traceability Matrix

<!-- last written by: test-writer, 2026-10-17 -->

| Claim ID | Scenario               | Test    | Status  |
| -------- | ---------------------- | ------- | ------- |
| B-001    | Explicit evidence      | Missing | Missing |
| B-002    | Inherited binding      | Missing | Missing |
| B-003    | Materialize default    | Missing | Missing |
| B-004    | Unsafe repair          | Missing | Missing |
| B-005    | Invalid binding        | Missing | Missing |
| B-006    | Multiple targets       | Missing | Missing |
| B-007    | Missing member binding | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-17 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-17 -->

| ID   | Question                                                                                                                            | Blocks                    | Resolution |
| ---- | ----------------------------------------------------------------------------------------------------------------------------------- | ------------------------- | ---------- |
| OQ-1 | Which exact XML tags, role names and CSharp declaration kinds are public?                                                           | Production implementation | Open       |
| OQ-2 | Which diagnostic namespace and schema/version policy govern these findings?                                                         | Production implementation | Open       |
| OQ-3 | What are the nested and partial type default conflict rules? The spike nearest-scope behavior is evidence, not an adopted contract. | Production implementation | Open       |

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
