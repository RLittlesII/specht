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
updated: "2026-10-18"
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

<!-- last written by: implementer, 2026-10-18 -->

| Concern                                      | Classification | Owner and boundary                                                                                           |
| -------------------------------------------- | -------------- | ------------------------------------------------------------------------------------------------------------ |
| Effective scenario scope and local claim IDs | Business       | The consumer claim bridge applies this Feature's complete-override rule and existing claim-tag grammar.      |
| Claim target identity and lifecycle          | Both           | The bridge resolves each local claim with the same F2 identity contract used for code bindings.              |
| Scenario-to-case correlation                 | Both           | The producer supplies stable scenario, row and F3 case identities; no title or step-method inference occurs. |
| Missing producer metadata                    | Business       | The bridge emits an unresolved scenario link while preserving the direct F3 binding evidence.                |
| Scenario evidence serialization              | Technical      | F3's optional `scenarios` property carries the closed record shape defined in § 7.                           |

## 7. Technical Design

<!-- last written by: implementer, 2026-10-18 -->

The consumer claim bridge is the producer boundary. It parses the consumer's
existing local claim tags and Gherkin metadata, applies decision 0001, and emits
scenario records in F3's optional `scenarios` array. The analyzer has no
Reqnroll dependency and does not parse scenario titles or step methods. A
producer ID identifies an integration; it does not gate which framework's
evidence the common contract accepts.

Each array entry represents one Gherkin scenario and contains:

| Property     | Shape and meaning                                                                                            |
| ------------ | ------------------------------------------------------------------------------------------------------------ |
| `path`       | Repository-relative Gherkin file path.                                                                       |
| `scenarioId` | Stable producer scenario ID, or null when the producer cannot supply one.                                    |
| `line`       | One-based source line used to locate the scenario, including when its stable ID is unavailable.              |
| `scope`      | Effective `{ epic, feature }`; `epic` is null when the selected grammar has no epic role.                    |
| `claims`     | Local claim records `{ id, status, isEligible, target, matches }` resolved with F2's binding identity rules. |
| `cases`      | One or more `{ rowId, caseId, status }` links to F3 discovery cases; `rowId` and `caseId` may be null.       |

Claim `status` uses F2's `incomplete`, `invalid`, `unresolved`, `ambiguous` or
`resolved` vocabulary. `target` is null unless resolved and otherwise uses the
F3 target shape with kind `claim`; `matches` carries the F2 matches, and
`isEligible` follows F2's resolved-target rule. `id` retains the local claim ID.
The target's epic and feature equal the effective scenario scope.

Case-link `status` is `resolved` or `unresolved`. A resolved link has a
nonempty `caseId` that exists in `discovery.cases`; an unresolved link has a
null `caseId`. Outline rows carry the producer's opaque `rowId`, which matches
the linked F3 case row ID. If the bridge cannot produce any case link for a
scenario because its required generator or discovery input is missing, it emits
one unresolved link with null `rowId` and `caseId`. A missing `scenarioId` is
also unresolved and never synthesized from mutable text. Scenario identity is
`(path, scenarioId)` when the ID is present and `(path, line)` otherwise. Case
identity remains F3's case key.

When the bridge is not selected, the producer omits `scenarios`; an empty array
means it was selected and found no scenario records. Both forms leave direct
method bindings valid. The F3 reader validates the closed scenario shape and
case references; its writer preserves the validated records. These boundaries
are implemented in `TestEvidenceArtifactReader`, `TestEvidenceArtifactWriter`
and the `TestEvidenceArtifact.Scenarios` model.

## 8. Testing Strategy

<!-- last written by: test-writer, 2026-10-18 -->

`TestEvidenceArtifactReader` validates normalized scenario records; it does not
parse Gherkin or produce records from consumer generator/runner metadata. The
unit tests cover the reader's contract checks and preservation of direct
bindings. They do not prove the consumer bridge behavior in B-001 through
B-007. No acceptance step definitions can exercise that producer because no
production bridge is present in this repository. Those claims remain Partial
or Missing in § 9 until the producer boundary has an executable implementation.

## 9. Traceability Matrix

<!-- last written by: test-writer, 2026-10-17 -->

| Claim ID | Scenario                         | Test                                                                                                                                                                                       | Status  |
| -------- | -------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------- |
| B-001    | Scenario claim                   | `ScenarioEvidence_WhenResolvedClaimMatchesEffectiveScope_ShouldBeValid`; `ScenarioEvidence_WhenClaimTargetBelongsToAnotherFeature_ShouldBeInvalid`                                         | Partial |
| B-002    | Scenario outline                 | `ScenarioEvidence_WhenOutlineRowDoesNotMatchDiscoveredCase_ShouldBeInvalid`                                                                                                                | Partial |
| B-003    | Shared steps                     | `ScenarioEvidence_WhenTwoScenariosShareOneStepMethod_ShouldKeepScenarioLinksSeparate`; `ScenarioEvidence_WhenTwoOutlinesShareOneScenarioId_ShouldBeInvalid`                                | Partial |
| B-004    | Missing generated input          | `ScenarioEvidence_WhenProducerCannotResolveCase_ShouldRetainUnresolvedLinkAndDirectBinding`                                                                                                | Partial |
| B-005    | Bridge disabled                  | `ScenarioEvidence_WhenBridgeIsOmitted_ShouldPreserveDirectBindings`                                                                                                                        | Partial |
| B-006    | Feature scope default            | Missing                                                                                                                                                                                    | Missing |
| B-007    | Complete scenario scope override | Missing                                                                                                                                                                                    | Missing |
| B-008    | Common scenario bridge contract  | `ScenarioEvidence_WhenCaseIdIsUnknown_ShouldBeInvalid`; `OptionalScenarioRecords_WhenWritten_ShouldBePreserved`; remaining malformed-record cases in `TestEvidenceScenarioReaderUnitTests` | Partial |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-17 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-17 -->

| ID   | Question                                                                           | Blocks                                                | Resolution                                                                                                                                                                                                                                                                                              |
| ---- | ---------------------------------------------------------------------------------- | ----------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | What is Feature-level versus Scenario-level scope precedence?                      | Tag/configuration encoding design                     | RESOLVED for behavior, 2026-10-17: decision 0001 uses Feature-level scope by default; explicit Scenario/Outline scope replaces it completely without field merging. Local claim tags resolve in effective scope.                                                                                        |
| OQ-2 | Which Reqnroll generator artifacts and versions form a supported adapter contract? | Concrete metadata extraction and compatibility design | RESOLVED, 2026-10-18: decision 0001 leaves Reqnroll optional and accepts scenario-to-case data through the producer-neutral contract. Section 7 defines the wire fields and unresolved-link representation; the compatibility matrix records only integrations actually tested and is not an allowlist. |

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
