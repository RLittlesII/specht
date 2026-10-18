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
| B-005 | Given a binding defect or missing required binding within the selected enforcement scope with no severity override, this Feature reports it as a warning under decision 0001.                       | Decision 0001                       | Active |
| B-006 | Given duplicate valid relationships with no severity override, this Feature reports the duplicate diagnostic as information.                                                                        | Decision 0001                       | Active |
| B-007 | Given a consumer severity override for one rule, this Feature applies the selected severity to that rule, including promotion to error.                                                             | Decision 0001                       | Active |
| B-008 | Given a consumer using standard Roslyn diagnostic controls, this Feature honors analyzer configuration, pragma and SuppressMessage suppression without a custom suppression protocol.               | Decision 0002                       | Active |
| B-009 | Given an invalid binding whose diagnostic is suppressed, this Feature keeps that binding invalid for evidence purposes.                                                                             | Decision 0002                       | Active |
| B-010 | Given a missing required evidence link whose diagnostic is suppressed, this Feature does not treat that link as satisfied.                                                                          | Decision 0002                       | Active |
| B-011 | Given a suppressed diagnostic, this Feature leaves the declared analysis scope unchanged.                                                                                                           | Decision 0002                       | Active |
| B-012 | Given an explicit binding in an analyzed project, this Feature validates it even when its declaration is outside the scope requiring authored bindings.                                             | 0201-F6 decision 0003               | Active |
| B-013 | Given generated code, this Feature does not require authored bindings on that code.                                                                                                                 | 0201-F6 decision 0003               | Active |
| B-014 | Given shared infrastructure, this Feature imposes no blanket requirement to bind every method; required authoring follows explicitly selected enforcement scopes.                                   | 0201-F6 decision 0003               | Active |
| B-015 | Given a requested coverage exemption without an explicit reason, this Feature rejects that exemption.                                                                                               | 0201-F6 decision 0003               | Active |
| B-016 | Given enabled analysis, this Feature reads the consumer's actual selected manifest, specification, ADR and configured schema contents supplied through Roslyn AdditionalFiles by build integration. | 0201-F6 decision 0004               | Active |
| B-017 | Given enabled analysis with missing, invalid or incompatible required inputs and no severity override, this Feature reports error diagnostics and marks analysis unavailable or incomplete.         | 0201-F6 decision 0004               | Active |
| B-018 | Given changed source, specification or configuration inputs, this Feature invalidates the affected analysis verdicts.                                                                               | 0201-F6 decision 0004               | Active |
| B-019 | Given a private PackageReference to Specht.CodeAnalysis, this Feature deploys analyzers, code fixes and build integration without an application runtime dependency.                                | Decision 0005                       | Active |
| B-020 | Given a repository release, this Feature versions the analysis package alongside specht.tool using the repository release version.                                                                  | Decision 0005                       | Active |
| B-021 | Given installed package versions, this Feature determines interoperability through schema/evidence contract versions rather than exact package-version equality.                                    | Decision 0005                       | Active |
| B-022 | Given consumer analysis policy, this Feature reads selected scopes, required links, exemptions and record discovery from the versioned codeAnalysis manifest section using existing ID grammars.    | Decision 0005                       | Active |
| B-023 | Given build integration, this Feature uses MSBuild repository-root selection and input delivery while standard Roslyn configuration owns severity and suppression.                                  | Decision 0005                       | Active |
| B-024 | Given new codeAnalysis manifest settings, this Feature introduces them through a new schema version while preserving existing pins and checker behavior.                                            | Decision 0005                       | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-17 -->

| ID  | Constraint                                                                                                           | Rules Out                                                    |
| --- | -------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------ |
| C-1 | No production delivery before the public diagnostic/version contract and input contract are agreed.                  | Shipping an unspecified public diagnostic contract.          |
| C-2 | The repository-wide Markdown checker remains independently usable without compiling consumer code.                   | Requiring Roslyn compilation to check Markdown.              |
| C-3 | Keep installation and policy explicit; reuse consumer grammar configuration rather than add analyzer regex settings. | Implicit enforcement scope or duplicate regex configuration. |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-17 -->

| #   | Item                                                                                        | Exclusion Reason                                |
| --- | ------------------------------------------------------------------------------------------- | ----------------------------------------------- |
| 1   | Replacing the existing specification checker or silently widening a pinned SPEC vocabulary. | This agreement does not authorize it.           |
| 2   | Generic convention analyzers or a mandatory test-framework migration.                       | This agreement does not authorize it.           |
| 3   | Custom suppression XML elements or a parallel suppression system.                           | Decision 0002 selects standard Roslyn controls. |

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

These are agreed project boundaries, not built projects. Decisions 0001 and
0002 define severity defaults and standard Roslyn reporting controls. Suppression
does not alter evidence validity, required-link completeness or declared scope.
Decision 0003 defines coverage requirements, selected authoring scope and
reasoned exemptions. Decision 0004 defines consumer input delivery through AdditionalFiles, required-input
failures, invalidation and completeness boundaries. Configuration encoding and
detailed build-discovery design remain open in § 11. The isolated spike does not select production targets or
waive the engine/host architecture gate.

## 8. Testing Strategy

<!-- last written by: test-writer, 2026-10-17 -->

The companion scenarios state the acceptance obligations. Production step definitions and mechanism tests are not written; each claim remains Missing in § 9. Spike results do not promote these rows to Covered.

## 9. Traceability Matrix

<!-- last written by: test-writer, 2026-10-17 -->

| Claim ID | Scenario                             | Test    | Status  |
| -------- | ------------------------------------ | ------- | ------- |
| B-001    | Selected scope                       | Missing | Missing |
| B-002    | Editor build parity                  | Missing | Missing |
| B-003    | Invalid inputs                       | Missing | Missing |
| B-004    | Optional Reqnroll                    | Missing | Missing |
| B-005    | Default warning findings             | Missing | Missing |
| B-006    | Informational duplicate findings     | Missing | Missing |
| B-007    | Per-rule severity override           | Missing | Missing |
| B-008    | Standard Roslyn suppression          | Missing | Missing |
| B-009    | Suppressed invalid binding           | Missing | Missing |
| B-010    | Suppressed missing evidence          | Missing | Missing |
| B-011    | Suppression preserves scope          | Missing | Missing |
| B-012    | Explicit binding validation          | Missing | Missing |
| B-013    | Generated authoring boundary         | Missing | Missing |
| B-014    | Shared infrastructure boundary       | Missing | Missing |
| B-015    | Reasoned exemptions                  | Missing | Missing |
| B-016    | Consumer record inputs               | Missing | Missing |
| B-017    | Required input failure               | Missing | Missing |
| B-018    | Input change invalidation            | Missing | Missing |
| B-019    | Private package installation         | Missing | Missing |
| B-020    | Aligned package release version      | Missing | Missing |
| B-021    | Contract-based compatibility         | Missing | Missing |
| B-022    | Manifest analysis policy             | Missing | Missing |
| B-023    | Configuration responsibility         | Missing | Missing |
| B-024    | Versioned configuration introduction | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-17 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-17 -->

| ID   | Question                                                                                   | Blocks                                   | Resolution                                                                                                                                                                                                                                                                                                                                                                                                          |
| ---- | ------------------------------------------------------------------------------------------ | ---------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | Which deployment identity, packages, input transport and configuration surface ship first? | None for deployment/configuration policy | RESOLVED, 2026-10-17: decision 0005 selects private Specht.CodeAnalysis installation, aligned repository release versions, contract-based compatibility and versioned codeAnalysis policy in the manifest. MSBuild owns root/input delivery; Roslyn controls reporting. Decision 0004 supplies actual consumer record data through AdditionalFiles. Exact contracts are defined in § 7 and its companion reference. |
| OQ-2 | What severities, exclusions and suppressions allow gradual adoption?                       | None for policy                          | RESOLVED, 2026-10-17: decisions 0001 through 0003 settle reporting and coverage; decision 0005 assigns policy to codeAnalysis in the manifest, root/input delivery to MSBuild and severity/suppression to standard Roslyn configuration.                                                                                                                                                                            |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-17 -->

| Section | Status | Reviewer | Note                                                                                                              |
| ------- | ------ | -------- | ----------------------------------------------------------------------------------------------------------------- |
| 1-12    | 🟡     | Pending  | Owner selected the model; this decomposed Feature has not received detailed agreement or implementation sign-off. |

## Tasks

- [0242: Implement analysis configuration and consumer input policy](../.issue/0242-analysis-policy-inputs.yml)
- [0243: Package CodeAnalysis and deliver editor and build inputs](../.issue/0243-codeanalysis-package-delivery.yml)

## Scoring

| Field | Value | Basis           |
| ----- | ----- | --------------- |
| value | 0     | Not yet scored. |
| risk  | 0     | Not yet scored. |
