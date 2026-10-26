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
updated: "2026-10-25"
github_issue: null
synced_at: null
---

# Specification: Optional scenario bridge

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-17 -->

Add scenario-to-test links when a consumer opts into Reqnroll. This draft decomposes the owner-selected model; unresolved public contracts block implementation, not the recorded adoption decision.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-25 -->

| #   | Persona             | Need                                                                                                                  | Pain Point Today                                                                                                 |
| --- | ------------------- | --------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------- |
| 1   | Consumer maintainer | Add scenario-to-test links when a consumer opts into Reqnroll.                                                        | References and test names can drift without an inspectable link.                                                 |
| 2   | Consumer maintainer | Have real Reqnroll scenarios and outline rows linked to their discovered and executed cases without writing a bridge. | No shipped producer emits scenario records, so per-row links and planned rows never reach the evidence artifact. |

### Assumptions

| ID  | Assumption                                                                                                                                                                                                                             |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | Originally, area folders were assumed not to select project boundaries. RESOLVED, 2026-10-17: owner review adopts the Analyzers, CodeFixes and deployment CodeAnalysis project homes (ADR-0016); implementation internals remain open. |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-25 -->

| ID    | Claim                                                                                                                                                                                                                           | Source                                             | Status  |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------- | ------- |
| B-001 | Given a scenario carrying a local claim tag and a determined effective scope, this Feature resolves that tag through the same identity contract as a code binding.                                                              | Epic 0201; decision 0001 in 0201-F1; decision 0001 | Amended |
| B-002 | Given an outline producing two discovered test cases, this Feature ensures each case links to the originating scenario and row through generator or runner identity.                                                            | Epic 0201; decision 0001 in 0201-F1                | Active  |
| B-003 | Given two scenarios sharing a handwritten step method, this Feature ensures the scenario-to-test links remain separate and are not inferred from step documentation.                                                            | Epic 0201; decision 0001 in 0201-F1                | Active  |
| B-004 | Given a selected Reqnroll project missing its required generator or discovery input, this Feature ensures the bridge reports an unresolved link.                                                                                | Epic 0201; decision 0001 in 0201-F1                | Active  |
| B-005 | Given a consumer selecting only bound xUnit or Playwright .NET tests, this Feature ensures the absence of Reqnroll scenarios does not invalidate the core test bindings.                                                        | Epic 0201; decision 0001 in 0201-F1                | Active  |
| B-006 | Given no explicit Scenario or Outline scope, this Feature uses the Gherkin Feature-level scope as the scenario default.                                                                                                         | Decision 0001                                      | Active  |
| B-007 | Given an explicit Scenario or Outline scope, this Feature replaces the complete Feature-level scope without merging components.                                                                                                 | Decision 0001                                      | Active  |
| B-008 | Given scenario-to-case metadata from a consumer claim bridge or optional Reqnroll integration, this Feature maps it through the common runner-evidence contract.                                                                | Decision 0001                                      | Active  |
| B-009 | Given an outline whose supplied rows are each marked planned or active, this Feature records a `state` of `planned` or `active` on each row's case link.                                                                        | Consumer request #256; 0201-F7 B-026               | Amended |
| B-010 | Given an outline whose rows differ in planned or active state, this Feature keeps the same claim on every row's case link.                                                                                                      | Consumer request #256; 0201-F7 B-026               | Active  |
| B-011 | Given a planned row with no discovered test case, this Feature emits a planned link with no case, distinct from the unresolved link for missing generator input.                                                                | Consumer request #256; 0201-F7 B-026               | Active  |
| B-012 | Given a selected Reqnroll test project whose cases are discovered and run with the shipped evidence collection, this Feature records one scenario record per Scenario and Scenario Outline in that project's evidence artifact. | Decision 0002; consumer request #256               | Active  |
| B-013 | Given a Reqnroll Scenario Outline whose Examples rows are discovered as separate test cases, this Feature links each row to that row's own discovered case.                                                                     | Decision 0002; consumer request #256               | Active  |
| B-014 | Given a Reqnroll scenario or Examples row carrying a configured planned or ignored tag, this Feature supplies that row to the bridge as planned.                                                                                | Decision 0002; 0201-F7 B-016, B-026                | Active  |
| B-015 | Given a Reqnroll scenario or Examples row with no discovered test case, this Feature records it with an unresolved case link instead of omitting it.                                                                            | Decision 0002; consumer request #256               | Active  |
| B-016 | Given a test project that does not select the Reqnroll producer, this Feature writes no scenario records into its evidence artifact.                                                                                            | Decision 0002; decision 0001                       | Active  |
| B-017 | Given two Reqnroll scenarios sharing one title, this Feature records two scenario records, each linked only to its own discovered cases.                                                                                        | Decision 0002; decision 0001                       | Active  |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-24 -->

| ID  | Constraint                                                                                           | Rules Out                                                      |
| --- | ---------------------------------------------------------------------------------------------------- | -------------------------------------------------------------- |
| C-1 | Reuse existing claim-tag parsing and grammar semantics; do not create a competing Gherkin validator. | A competing interpretation of claim tags.                      |
| C-2 | A scenario, generated test case and shared step method are different records.                        | Step annotations standing in for scenario links.               |
| C-3 | The bridge is optional and cannot impose Reqnroll dependencies on the binding model.                 | A mandatory scenario framework for direct tests.               |
| C-4 | A case link's `state` comes only from the planned flag the producer supplies for that row.           | Inferring state from titles, tags or test results.             |
| C-5 | A case link's `state` is independent of its resolution status and never changes its claim.           | Treating planned as unresolved, or splitting a claim by state. |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-25 -->

| #   | Item                                                             | Exclusion Reason                                                                           |
| --- | ---------------------------------------------------------------- | ------------------------------------------------------------------------------------------ |
| 1   | Changing this repository's own Gherkin-first development policy. | This agreement does not authorize it.                                                      |
| 2   | Using scenario titles or step method names as identity.          | This agreement does not authorize it.                                                      |
| 3   | Taking ownership of or replacing consumer convention analyzers.  | Decision 0001 reuses the consumer bridge through the common contract.                      |
| 4   | Deciding whether a planned or active link fails the gate.        | 0201-F5 owns coverage and the default gate (B-018, B-019).                                 |
| 5   | Setting a row's planned flag inside the bridge mapping.          | The producer supplies it (B-014; 0201-F7 B-016, B-026); the mapping only carries it (C-4). |
| 6   | A shipped producer for a scenario framework other than Reqnroll. | Decision 0002 names Reqnroll; other producers use the common contract (B-008).             |

## 6. Concern Separation

<!-- last written by: implementer, 2026-10-24 -->

| Concern                                      | Classification | Owner and boundary                                                                                                          |
| -------------------------------------------- | -------------- | --------------------------------------------------------------------------------------------------------------------------- |
| Effective scenario scope and local claim IDs | Business       | `ScenarioEvidenceBridge` applies the complete-override rule; the consumer supplies existing claim-tag text unparsed.        |
| Claim target identity and lifecycle          | Both           | The bridge resolves each tag with `BindingResolver`, the F2 identity contract used for code bindings.                       |
| Scenario-to-case correlation                 | Both           | The producer supplies stable scenario, row and F3 case identities; no title or step-method inference occurs.                |
| Planned or active row state                  | Business       | The producer's per-row planned flag becomes the link `state`; it never alters status or claims (C-4, C-5).                  |
| Missing producer metadata                    | Business       | The bridge emits one unresolved active link, distinct from a planned link, while preserving the direct F3 binding evidence. |
| Scenario evidence serialization              | Technical      | F3's optional `scenarios` property carries the closed record shape in section 7; both evidence readers validate it.         |

## 7. Technical Design

<!-- last written by: implementer, 2026-10-24 -->

The consumer claim bridge is the producer boundary. It reads its own Gherkin and
generator output and hands this Feature normalized `ScenarioMetadata`; the
analyzer has no Reqnroll dependency, parses no `.feature` text, and reads no
scenario titles or step methods. `ScenarioEvidenceBridge.Map` turns that metadata
into scenario records for F3's optional `scenarios` array. A producer ID identifies
an integration; it does not gate which framework's evidence the contract accepts.

Bridge input, in `Specht.Analysis.Scenarios`:

| Type                     | Members                                                                                                                       |
| ------------------------ | ----------------------------------------------------------------------------------------------------------------------------- |
| `ScenarioScope`          | `Epic?`, `Feature?`; a whole scope, never merged.                                                                             |
| `ScenarioRow`            | `RowId?`, `CaseId?`, `Planned`; the producer's flag for one outline row, or the single implicit row of a plain scenario.      |
| `ScenarioMetadata`       | `Path`, `ScenarioId?`, `Line`, `FeatureScope`, `ScopeOverride?`, `ClaimTags`, `Rows`, `GeneratorInputPresent`.                |
| `ScenarioEvidenceBridge` | Constructed with a `BindingResolver`; `Map(IReadOnlyList<ScenarioMetadata>)` returns one record per scenario, in input order. |

The effective scope is `ScopeOverride` when present, else `FeatureScope`, taken
whole. Each claim tag resolves through `BindingResolver.Resolve` with that scope,
so a replacement without an epic does not inherit the Feature-level epic.

Each record represents one Gherkin scenario and contains:

| Property     | Shape and meaning                                                                                             |
| ------------ | ------------------------------------------------------------------------------------------------------------- |
| `path`       | Repository-relative Gherkin file path.                                                                        |
| `scenarioId` | Stable producer scenario ID, or null when the producer cannot supply one.                                     |
| `line`       | One-based source line used to locate the scenario, including when its stable ID is unavailable.               |
| `scope`      | Effective `{ epic, feature }`; `epic` is null when the selected grammar has no epic role.                     |
| `claims`     | Local claim records `{ id, status, isEligible, target, matches }` resolved with F2's binding identity rules.  |
| `cases`      | One or more `{ rowId, caseId, status, state }` links to F3 discovery cases; `rowId` and `caseId` may be null. |

Claim `status` uses F2's `incomplete`, `invalid`, `unresolved`, `ambiguous` or
`resolved` vocabulary. `target` is null unless resolved and otherwise uses the
F3 target shape with kind `claim`; `matches` carries the F2 matches, and
`isEligible` follows F2's resolved-target rule. `id` retains the local claim ID.
The target's epic and feature equal the effective scenario scope. Claims are
computed from the tags and scope alone, so row state never changes them.

Case-link `status` is `resolved` or `unresolved`: whether a discovered case
exists. A resolved link has a nonempty `caseId` that exists in `discovery.cases`;
an unresolved link has a null `caseId`. Case-link `state` is `planned` or
`active` and is required; it is the producer's flag for that row and is
independent of `status`, so a planned link may be resolved or unresolved. Outline
rows carry the producer's opaque `rowId`, which matches the linked F3 case row ID.

`Map` emits one link per row: `resolved` when the row has a `CaseId`, else
`unresolved` with the row's `RowId` kept, and `state` from `Planned`. When
`GeneratorInputPresent` is false the active rows are dropped, planned rows stay as
planned links, and one `{ null, null, unresolved, active }` link is appended. That
link is the missing-input marker, distinct from a planned link. A planned-only
outline with missing input therefore keeps its planned links and gains the marker.
`Rows` must be non-empty when `GeneratorInputPresent` is true; otherwise `Map` emits
an empty `cases` array that the readers reject (OQ-3).
A missing `scenarioId` is never synthesized from mutable text. Scenario identity is
`(path, scenarioId)` when the ID is present and `(path, line)` otherwise. Case
identity remains F3's case key.

When the bridge is not selected, the producer omits `scenarios`; an empty array
means it was selected and found no scenario records. Both forms leave direct
method bindings valid. Both evidence readers validate the closed scenario shape,
`state` included, and the case references; the writer preserves the validated
records. These boundaries are implemented in `TestEvidenceArtifactReader` (the
analyzer's and the tool's contract copy), `TestEvidenceArtifactWriter` and the
`TestEvidenceArtifact.Scenarios` model. A consumer bridge producing
`ScenarioMetadata` from its generator output is outside this repository.

## 8. Testing Strategy

<!-- last written by: test-writer, 2026-10-24 -->

Two mechanisms carry the claims. The producer-neutral mapping,
`ScenarioEvidenceBridge.Map` in `Specht.Analysis.Scenarios`, turns normalized
`ScenarioMetadata` (path, scenario ID, line, Feature-level scope, an optional
whole-replacement scope override, local claim tags, and rows of
`(RowId, CaseId, Planned)` plus a generator-input flag) into F3 scenario records,
resolving each tag with `BindingResolver`. `TestEvidenceArtifactReader` validates
those records as a closed shape, `state` included. Both have unit tests in
`test/analysis.tests`; the Reqnroll steps in `test/acceptance/Scenarios` drive the
same bridge over synthetic metadata. The bridge is a mapping from metadata, not a
Gherkin parser, so no unit test parses `.feature` text.

| Claim | Mechanism                                                                                                                                                                                          | Unit test or reason none                                                                                                                                                                                                                                     |
| ----- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| B-001 | `Map` resolves each tag through `BindingResolver.Resolve(ClaimReference(effective epic, feature, tag))`                                                                                            | `ScenarioEvidenceBridgeUnitTests`: `LocalClaimTag_WhenMapped_ShouldResolveThroughTheCodeBindingIdentityContract`, `LocalClaimTagThatDoesNotResolve_WhenMapped_ShouldKeepTheResolverStatusWithoutATarget`                                                     |
| B-002 | `Map` copies each row's `RowId` and `CaseId` into one case link                                                                                                                                    | `ScenarioEvidenceBridgeUnitTests`: `OutlineWithTwoDiscoveredCases_WhenMapped_ShouldLinkEachCaseToItsOwnRow`; reader row/case agreement in `TestEvidenceScenarioReaderUnitTests`                                                                              |
| B-003 | `Map` builds links from metadata rows only; the input has no step-method member, so nothing can be inferred from one                                                                               | `ScenarioEvidenceBridgeUnitTests`: `TwoScenariosOverOneStepMethod_WhenMapped_ShouldKeepEachScenarioLinkedToItsOwnCase`; reader identity uniqueness in `TestEvidenceScenarioReaderUnitTests`                                                                  |
| B-004 | `Map` replaces active rows with one unresolved link, null `rowId` and `caseId`, when `GeneratorInputPresent` is false                                                                              | `ScenarioEvidenceBridgeUnitTests`: `ScenarioWithoutGeneratorInput_WhenMapped_ShouldEmitOneUnresolvedLinkAndKeepItsClaims`                                                                                                                                    |
| B-005 | `Map` emits nothing for no metadata; the reader accepts an artifact with no `scenarios`                                                                                                            | `ScenarioEvidenceBridgeUnitTests`: `NoScenarioMetadata_WhenMapped_ShouldProduceNoRecords`; `TestEvidenceScenarioReaderUnitTests`: `ScenarioEvidence_WhenBridgeIsOmitted_ShouldPreserveDirectBindings`                                                        |
| B-006 | `Map` takes `FeatureScope` when `ScopeOverride` is null                                                                                                                                            | `ScenarioEvidenceBridgeUnitTests`: `ScenarioWithoutScopeOverride_WhenMapped_ShouldUseTheFeatureLevelScope` (Scenario and Outline)                                                                                                                            |
| B-007 | `Map` takes `ScopeOverride` whole, never merging a component from `FeatureScope`                                                                                                                   | `ScenarioEvidenceBridgeUnitTests`: `ScopeOverrideWithoutEpic_WhenMapped_ShouldNotMergeTheFeatureLevelEpic`, `CompleteScopeOverride_WhenMapped_ShouldReplaceEveryFeatureLevelComponent`                                                                       |
| B-008 | The mapped records satisfy the reader's closed shape and case references; the bridge takes no producer identity                                                                                    | `ScenarioEvidenceBridgeUnitTests`: `MappedRecords_WhenCarriedInAnArtifact_ShouldBeAcceptedByTheCommonReader`; the malformed-record cases in `TestEvidenceScenarioReaderUnitTests`; `OptionalScenarioRecords_WhenWritten_ShouldBePreserved`                   |
| B-009 | `Map` sets `state` from `ScenarioRow.Planned`; the reader accepts only `planned` or `active`                                                                                                       | `ScenarioEvidenceBridgeUnitTests`: `OutlineRowsMarkedActiveAndPlanned_WhenMapped_ShouldRecordTheProducerStateOnEachCaseLink`; `TestEvidenceScenarioReaderUnitTests`: `ScenarioEvidence_WhenCaseLinkStateIsOmittedOrUnrecognized_ShouldBeInvalid`             |
| B-010 | The claims array belongs to the scenario record and is computed without reading row state                                                                                                          | `ScenarioEvidenceBridgeUnitTests`: `OutlineWithPlannedRows_WhenMapped_ShouldKeepTheSameClaimsAsWithoutThem`                                                                                                                                                  |
| B-011 | `Map` emits a planned link (`rowId` kept, null `caseId`, status `unresolved`) for a planned row without a case, apart from the missing-input link; the reader accepts either status with `planned` | `ScenarioEvidenceBridgeUnitTests`: `PlannedRowWithoutDiscoveredCase_WhenMapped_ShouldBePlannedAndDistinctFromTheMissingInputLink`; `TestEvidenceScenarioReaderUnitTests`: `ScenarioEvidence_WhenPlannedLinkHasEitherResolution_ShouldBeValidAndKeepItsState` |

## 9. Traceability Matrix

<!-- last written by: test-writer, 2026-10-25 -->

| Claim ID | Scenario                                         | Test                                                                                                                                                                                                                                                                                                                                                                                                                        | Status  |
| -------- | ------------------------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------- |
| B-001    | Scenario claim                                   | `ScenarioEvidenceBridgeUnitTests.LocalClaimTag_WhenMapped_ShouldResolveThroughTheCodeBindingIdentityContract`; `ScenarioEvidenceBridgeUnitTests.LocalClaimTagThatDoesNotResolve_WhenMapped_ShouldKeepTheResolverStatusWithoutATarget`; `ScenarioEvidence_WhenResolvedClaimMatchesEffectiveScope_ShouldBeValid`; `ScenarioEvidence_WhenClaimTargetBelongsToAnotherFeature_ShouldBeInvalid`; step class `ScenarioBridgeSteps` | Partial |
| B-002    | Scenario outline                                 | `ScenarioEvidenceBridgeUnitTests.OutlineWithTwoDiscoveredCases_WhenMapped_ShouldLinkEachCaseToItsOwnRow`; `ScenarioEvidence_WhenOutlineRowDoesNotMatchDiscoveredCase_ShouldBeInvalid`; step class `ScenarioBridgeSteps`                                                                                                                                                                                                     | Partial |
| B-003    | Shared steps                                     | `ScenarioEvidenceBridgeUnitTests.TwoScenariosOverOneStepMethod_WhenMapped_ShouldKeepEachScenarioLinkedToItsOwnCase`; `ScenarioEvidence_WhenTwoScenariosShareOneStepMethod_ShouldKeepScenarioLinksSeparate`; `ScenarioEvidence_WhenTwoOutlinesShareOneScenarioId_ShouldBeInvalid`; step class `ScenarioBridgeSteps`                                                                                                          | Partial |
| B-004    | Missing generated input                          | `ScenarioEvidenceBridgeUnitTests.ScenarioWithoutGeneratorInput_WhenMapped_ShouldEmitOneUnresolvedLinkAndKeepItsClaims`; `ScenarioEvidence_WhenProducerCannotResolveCase_ShouldRetainUnresolvedLinkAndDirectBinding`; step class `ScenarioBridgeSteps`                                                                                                                                                                       | Partial |
| B-005    | Bridge disabled                                  | `ScenarioEvidenceBridgeUnitTests.NoScenarioMetadata_WhenMapped_ShouldProduceNoRecords`; `ScenarioEvidence_WhenBridgeIsOmitted_ShouldPreserveDirectBindings`; step class `ScenarioBridgeSteps`                                                                                                                                                                                                                               | Partial |
| B-006    | Feature scope default                            | `ScenarioEvidenceBridgeUnitTests.ScenarioWithoutScopeOverride_WhenMapped_ShouldUseTheFeatureLevelScope`; step class `ScenarioBridgeSteps`                                                                                                                                                                                                                                                                                   | Covered |
| B-007    | Complete scenario scope override                 | `ScenarioEvidenceBridgeUnitTests.ScopeOverrideWithoutEpic_WhenMapped_ShouldNotMergeTheFeatureLevelEpic`; `ScenarioEvidenceBridgeUnitTests.CompleteScopeOverride_WhenMapped_ShouldReplaceEveryFeatureLevelComponent`; step class `ScenarioBridgeSteps`                                                                                                                                                                       | Covered |
| B-008    | Common scenario bridge contract                  | `ScenarioEvidenceBridgeUnitTests.MappedRecords_WhenCarriedInAnArtifact_ShouldBeAcceptedByTheCommonReader`; `ScenarioEvidence_WhenCaseIdIsUnknown_ShouldBeInvalid`; `OptionalScenarioRecords_WhenWritten_ShouldBePreserved`; remaining malformed-record cases in `TestEvidenceScenarioReaderUnitTests`; step class `ScenarioBridgeSteps`                                                                                     | Partial |
| B-009    | Planned and active rows                          | `ScenarioEvidenceBridgeUnitTests.OutlineRowsMarkedActiveAndPlanned_WhenMapped_ShouldRecordTheProducerStateOnEachCaseLink`; `ScenarioEvidence_WhenCaseLinkStateIsOmittedOrUnrecognized_ShouldBeInvalid`; step class `ScenarioBridgeSteps`                                                                                                                                                                                    | Partial |
| B-010    | Planned and active rows keep one claim           | `ScenarioEvidenceBridgeUnitTests.OutlineWithPlannedRows_WhenMapped_ShouldKeepTheSameClaimsAsWithoutThem`; step class `ScenarioBridgeSteps`                                                                                                                                                                                                                                                                                  | Covered |
| B-011    | Planned row without a discovered case            | `ScenarioEvidenceBridgeUnitTests.PlannedRowWithoutDiscoveredCase_WhenMapped_ShouldBePlannedAndDistinctFromTheMissingInputLink`; `ScenarioEvidence_WhenPlannedLinkHasEitherResolution_ShouldBeValidAndKeepItsState`; step class `ScenarioBridgeSteps`                                                                                                                                                                        | Partial |
| B-012    | Reqnroll scenarios recorded from a real run      | Missing                                                                                                                                                                                                                                                                                                                                                                                                                     | Missing |
| B-013    | Each Reqnroll outline row linked to its own case | Missing                                                                                                                                                                                                                                                                                                                                                                                                                     | Missing |
| B-014    | Planned Reqnroll rows                            | Missing                                                                                                                                                                                                                                                                                                                                                                                                                     | Missing |
| B-015    | Reqnroll row with no discovered case             | Missing                                                                                                                                                                                                                                                                                                                                                                                                                     | Missing |
| B-016    | Reqnroll producer not selected                   | Missing                                                                                                                                                                                                                                                                                                                                                                                                                     | Missing |
| B-017    | Two Reqnroll scenarios sharing a title           | Missing                                                                                                                                                                                                                                                                                                                                                                                                                     | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-25 -->

2026-10-25: decision 0002 ships a Reqnroll producer feeding the bridge, after the
#256 readiness review found the bridge had no production caller. B-012 to B-017,
§ 2 row 2 and § 5 row 6 are new; § 5 row 5 now names the producer as the source
of the planned flag. OQ-2 gains a note and OQ-4 to OQ-6 are open. No existing
claim changed. § 7's sentence placing every producer outside this repository is
the implementer's to update, and § 9 rows for B-012 to B-017 are the
test-writer's.

2026-10-24: B-009 amended. Case-link `resolved`/`unresolved` status says whether a
discovered case exists; it cannot say planned or active. Each row's case link
carries a separate `state` of `planned` or `active`. The same-claim assertion
moved to B-010, and B-011 covers a planned row with no discovered case. The
design in section 7 is the implementer's to update.

2026-10-23: owner decision for consumer request #256 makes planned or ignored
state per case. B-009 carries that state on each row's case link; no existing
claim changed.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-25 -->

| ID   | Question                                                                                                                                                                                                                                                                                                                                                            | Blocks                                                            | Resolution                                                                                                                                                                                                                                                                                                                                                                                                              |
| ---- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | What is Feature-level versus Scenario-level scope precedence?                                                                                                                                                                                                                                                                                                       | Tag/configuration encoding design                                 | RESOLVED for behavior, 2026-10-17: decision 0001 uses Feature-level scope by default; explicit Scenario/Outline scope replaces it completely without field merging. Local claim tags resolve in effective scope.                                                                                                                                                                                                        |
| OQ-2 | Which Reqnroll generator artifacts and versions form a supported adapter contract?                                                                                                                                                                                                                                                                                  | Concrete metadata extraction and compatibility design             | RESOLVED, 2026-10-18: decision 0001 leaves Reqnroll optional and accepts scenario-to-case data through the producer-neutral contract. Section 7 defines the wire fields and unresolved-link representation; the compatibility matrix records only integrations actually tested and is not an allowlist. Note, 2026-10-25: decision 0002 adds a shipped Reqnroll producer; the artifacts and versions it reads are OQ-4. |
| OQ-3 | When scenario metadata has generator input present and empty rows, should the bridge throw or emit the missing-input marker link? `Map` currently emits `cases: []`, which both evidence readers reject ("Each scenario must retain at least one case link or unresolved link").                                                                                    | None for 0245: producer contract violation; no test until decided | Open.                                                                                                                                                                                                                                                                                                                                                                                                                   |
| OQ-4 | Which metadata does the Reqnroll producer read to identify a scenario, its line, each Examples row and the case key it links to: the runner's discovery and result events, the generated test code, the native claim catalogue the analyzer exports (0201-F7 B-030), or a combination? Where does it run so that C-3 holds, and which Reqnroll versions are tested? | B-012, B-013, B-015, B-017; producer design                       | Open. Proposed default: the VSTest logger, which already holds each case key and its `Category` traits and reads the exported catalogue, runs the producer; it adds no Reqnroll dependency, so C-3 holds. How a discovered case is matched to its Examples row is not proposed. Tested versions go in the compatibility matrix and are not an allowlist (OQ-2).                                                         |
| OQ-5 | How does a consumer select the Reqnroll producer for a test project: a `codeAnalysis` manifest key, an MSBuild property, or automatically when the project's cases are Reqnroll-generated?                                                                                                                                                                          | B-012, B-016; a public contract                                   | Open. Proposed default: an explicit per-project opt-in that is off by default, since decision 0001 keeps Reqnroll optional (B-005, C-3).                                                                                                                                                                                                                                                                                |
| OQ-6 | Does the Reqnroll producer cover the Microsoft.Testing.Platform collector as well as the VSTest logger?                                                                                                                                                                                                                                                             | B-012                                                             | Open. Proposed default: the VSTest logger only, the runner consumer request #256 names; the other collector would need a later decision.                                                                                                                                                                                                                                                                                |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-24 -->

| Section | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                      |
| ------- | ------ | ------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-12    | 🟡     | spec-reviewer | Code and tests satisfy B-001 to B-011 and section 7 matches the built code. Held at Draft: section 9 statuses for B-006, B-007 and B-010 are stale, item 0245 does not cite B-010 and B-011, and the empty-rows precondition and the planned-only missing-input behaviour are not pinned. |

## Tasks

- [0245: Implement the optional scenario evidence bridge](../.issue/0245-optional-scenario-bridge.yml)
- [0302: Produce scenario records from a consumer's Reqnroll runs](../.issue/0302-reqnroll-scenario-producer.yml)

## Scoring

| Field | Value | Basis           |
| ----- | ----- | --------------- |
| value | 0     | Not yet scored. |
| risk  | 0     | Not yet scored. |
