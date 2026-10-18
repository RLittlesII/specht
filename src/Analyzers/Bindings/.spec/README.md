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

| ID    | Claim                                                                                                                                                                                                                             | Source                              | Status  |
| ----- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------- | ------- |
| B-001 | Given a manifest defining custom Feature and claim grammars and records using them, this Feature ensures references are validated and resolved using those configured grammars.                                                   | Epic 0201; decision 0001 in 0201-F1 | Active  |
| B-002 | Given two Features declaring the same local claim ID, this Feature ensures a qualified claim reference resolves only in its declared owning Feature.                                                                              | Epic 0201; decision 0001 in 0201-F1 | Active  |
| B-003 | Given a consumer whose configured model has no epic tier, this Feature ensures a Feature and claim reference resolves without inventing an epic.                                                                                  | Epic 0201; decision 0001 in 0201-F1 | Active  |
| B-004 | Given an ADR reference, this Feature selects only repository scope when `feature` is absent or the explicitly named Feature scope when `feature` is present, with `epic` optional.                                                | Decision 0003                       | Amended |
| B-005 | Given the same specification inputs and declared references with no Reqnroll or test-framework assemblies, this Feature ensures reference resolution yields the same identities and findings.                                     | Epic 0201; decision 0001 in 0201-F1 | Active  |
| B-006 | Given a claim reference, this Feature represents its identity in separate `epic`, `feature` and `claim` fields, with `epic` optional.                                                                                             | Decision 0002                       | Active  |
| B-007 | Given an ADR missing from its selected owner but declared under another owner, this Feature reports the reference as unresolved without searching another scope.                                                                  | Decision 0003                       | Active  |
| B-008 | Given an Active or Amended claim in a draft, in-review or approved Feature, this Feature reports a resolving reference to that claim as eligible for current evidence.                                                            | Decision 0004                       | Active  |
| B-009 | Given a resolving reference to a Withdrawn or Superseded claim or a claim in a superseded Feature, this Feature reports the target as ineligible for current evidence while preserving its resolved identity and lifecycle state. | Decision 0004                       | Active  |
| B-010 | Given an accepted ADR, this Feature reports a resolving reference to that decision as eligible for current evidence.                                                                                                              | Decision 0004                       | Active  |
| B-011 | Given a resolving reference to a proposed, rejected, deprecated or superseded ADR, this Feature reports the target as ineligible for current evidence while preserving its resolved identity and lifecycle state.                 | Decision 0004                       | Active  |
| B-012 | Given a checked relationship, this Feature permits claim or ADR targets only; Feature and epic identities provide scope rather than additional checked target kinds.                                                              | Decision 0005                       | Active  |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-17 -->

| ID  | Constraint                                                                                                                | Rules Out                                                 |
| --- | ------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------- |
| C-1 | Reuse manifest identity and discovery semantics (0001-F5 C-2); no parallel regex configuration or first-match resolution. | A second ID parser or implicit first-match lookup.        |
| C-2 | Keep format validation distinct from existence and ambiguity checks.                                                      | Treating a regex match as a resolving reference.          |
| C-3 | Binding identity depends on no Reqnroll, xUnit or Playwright types.                                                       | Mandatory test-framework packages in identity resolution. |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-17 -->

| #   | Item                                                                                                                                                                                                                               | Exclusion Reason                                     |
| --- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------- |
| 1   | XML roles, supported declarations and repeated single-target elements are defined in 0201-F2 decisions 0002, 0003 and 0005; strict XML validation is defined by F2 decision 0006. Claim identity field names are defined by B-006. | This agreement does not authorize it.                |
| 2   | Test discovery, execution and scenario links belong to adapters; a resolved reference alone is no behavioral proof.                                                                                                                | This agreement does not authorize it.                |
| 3   | Additional checked record kinds beyond claims and ADRs.                                                                                                                                                                            | Decision 0005 limits the initial checked target set. |

## 6. Concern Separation

<!-- last written by: implementer, 2026-10-17 -->

Not yet designed; no production implementation is authorized by this draft.

## 7. Technical Design

<!-- last written by: implementer, 2026-10-17 -->

Decisions 0002 through 0005 settle identity components, ADR ownership, lifecycle eligibility and checked target kinds. Detailed implementation design remains to be written; the isolated spike is not production architecture.

## 8. Testing Strategy

<!-- last written by: test-writer, 2026-10-17 -->

The companion scenarios state the acceptance obligations. Production step definitions and mechanism tests are not written; each claim remains Missing in § 9. Spike results do not promote these rows to Covered.

## 9. Traceability Matrix

<!-- last written by: test-writer, 2026-10-17 -->

| Claim ID | Scenario                               | Test    | Status  |
| -------- | -------------------------------------- | ------- | ------- |
| B-001    | Consumer grammars                      | Missing | Missing |
| B-002    | Owning scope                           | Missing | Missing |
| B-003    | No epic tier                           | Missing | Missing |
| B-004    | ADR ownership                          | Missing | Missing |
| B-005    | Framework independence                 | Missing | Missing |
| B-006    | Separate claim identity components     | Missing | Missing |
| B-007    | No ADR scope fallback                  | Missing | Missing |
| B-008    | Current claim eligibility              | Missing | Missing |
| B-009    | Historical claim eligibility           | Missing | Missing |
| B-010    | Accepted ADR eligibility               | Missing | Missing |
| B-011    | Historical or proposed ADR eligibility | Missing | Missing |
| B-012    | Checked target boundary                | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-17 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-17 -->

| ID   | Question                                                                                                   | Blocks                        | Resolution                                                                                                                                                                                                                                                                                                                                                                                                      |
| ---- | ---------------------------------------------------------------------------------------------------------- | ----------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | Which public encoding carries local Feature, optional epic and ADR ownership without assuming a delimiter? | None for reference components | RESOLVED, 2026-10-17: decision 0002 defines separate claim identity fields; decision 0003 defines `adr` plus explicit optional owner components. No `feature` means repository ownership; a named Feature owns the ADR otherwise. XML roles, declaration kinds and repeated single-target elements are defined in 0201-F2 decisions 0002, 0003 and 0005; strict XML validation is defined by its decision 0006. |
| OQ-2 | Which claim and ADR lifecycle states are eligible targets?                                                 | None for lifecycle policy     | RESOLVED, 2026-10-17: decision 0004 defines current claim and accepted-ADR eligibility while preserving resolving historical links as ineligible. Target diagnostic severity is defined in 0201-F6 decision 0001; standard suppression is defined in 0201-F6 decision 0002.                                                                                                                                     |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-17 -->

| Section | Status | Reviewer | Note                                                                                                              |
| ------- | ------ | -------- | ----------------------------------------------------------------------------------------------------------------- |
| 1-12    | 🟡     | Pending  | Owner selected the model; this decomposed Feature has not received detailed agreement or implementation sign-off. |

## Tasks

- [0238: Implement binding identity and record resolution](../.issue/0238-binding-identity.yml)

## Scoring

| Field | Value | Basis           |
| ----- | ----- | --------------- |
| value | 0     | Not yet scored. |
| risk  | 0     | Not yet scored. |
