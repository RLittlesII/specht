---
title: "Specification: Test evidence adapters"
description: "Map explicit test-method bindings to runner evidence across .NET test frameworks without requiring Gherkin."
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

Map explicit test-method bindings to runner evidence across .NET test frameworks without requiring Gherkin. This draft decomposes the owner-selected model; unresolved public contracts block implementation, not the recorded adoption decision.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-17 -->

| #   | Persona             | Need                                                                                                        | Pain Point Today                                                 |
| --- | ------------------- | ----------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------- |
| 1   | Consumer maintainer | Map explicit test-method bindings to runner evidence across .NET test frameworks without requiring Gherkin. | References and test names can drift without an inspectable link. |

### Assumptions

| ID  | Assumption                                                                                                                                                                                                                             |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | Originally, area folders were assumed not to select project boundaries. RESOLVED, 2026-10-17: owner review adopts the Analyzers, CodeFixes and deployment CodeAnalysis project homes (ADR-0016); implementation internals remain open. |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-17 -->

| ID    | Claim                                                                                                                                                                                                                 | Source                                             | Status  |
| ----- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------- | ------- |
| B-001 | Given a bound xUnit fact in a selected test project without Reqnroll, this Feature ensures its discovered test case is linked to that method and claim.                                                               | Epic 0201; decision 0001 in 0201-F1                | Active  |
| B-002 | Given a bound xUnit theory discovering two data rows without Reqnroll, this Feature ensures both test cases link to the same bound method without treating them as two methods.                                       | Epic 0201; decision 0001 in 0201-F1                | Active  |
| B-003 | Given a bound Playwright .NET test with conforming runner evidence and no Reqnroll, this Feature links the discovered case through its CSharp method to the declared claim.                                           | Epic 0201; decision 0001 in 0201-F1; decision 0001 | Amended |
| B-004 | Given a selected test project with missing collection integration or insufficient case/method metadata, this Feature reports unavailable or unresolved evidence rather than empty complete coverage.                  | Epic 0201; decision 0001 in 0201-F1; decision 0001 | Amended |
| B-005 | Given two discovered cases with duplicate display names but distinct runner identities, this Feature ensures they remain distinct and correlate through runner identity and method metadata.                          | Epic 0201; decision 0001 in 0201-F1                | Active  |
| B-006 | Given supplied discovery or results, this Feature checks their source, assembly and configuration identities against the current analysis inputs.                                                                     | 0201-F6 decision 0004                              | Active  |
| B-007 | Given conforming runner evidence from a .NET test framework, this Feature maps case and method identities through the common evidence contract without a framework-name allowlist.                                    | Decision 0001                                      | Active  |
| B-008 | Given runtime-only parameterized test cases, this Feature leaves case enumeration incomplete until the runner supplies the cases and never invents cases or counts.                                                   | Decision 0001                                      | Active  |
| B-009 | Given collected test evidence, this Feature reads versioned JSON carrying runner case IDs, method metadata, project/framework identity, source/assembly/configuration fingerprints, adapter version and run identity. | Decision 0001                                      | Active  |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-17 -->

| ID  | Constraint                                                                                                          | Rules Out                                               |
| --- | ------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------- |
| C-1 | The binding model is framework-independent; adapter-specific recognition does not alter reference semantics.        | Framework-specific reference grammar.                   |
| C-2 | Reqnroll and Gherkin are optional; source bindings, discovered cases and execution results remain distinct records. | Treating a method declaration as an executed test case. |
| C-3 | Discovery and result ingestion use explicit offline inputs; analysis does not execute browsers or fetch providers.  | Running tests or contacting services during analysis.   |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-17 -->

| #   | Item                                                                        | Exclusion Reason                      |
| --- | --------------------------------------------------------------------------- | ------------------------------------- |
| 1   | JavaScript and TypeScript Playwright bindings; Roslyn analyzes CSharp.      | This agreement does not authorize it. |
| 2   | Claim inference from test display names, assertion text or scenario titles. | This agreement does not authorize it. |
| 3   | Guessing case or method identity when runner metadata is insufficient.      | This agreement does not authorize it. |

## 6. Concern Separation

<!-- last written by: implementer, 2026-10-17 -->

Not yet designed; no production implementation is authorized by this draft.

## 7. Technical Design

<!-- last written by: implementer, 2026-10-17 -->

This planned version-one contract implements decision 0001 and F6 decision 0004.
Collection adapters are producers; the checker never launches a runner to obtain
missing data. Framework names are descriptive metadata, never an allowlist.

### Envelope

Use UTF-8 JSON with `format: "specht.test-evidence"` and integer `formatVersion: 1`.
Reject unsupported versions explicitly. Required properties are below; nested
objects are closed for version one. Additions requiring different interpretation
need a new format version, independently of package and manifest schema versions.

| Property            | Contract                                                                                                                                                                                                |
| ------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `project`           | Root-relative `.csproj` path from the declared project set.                                                                                                                                             |
| `targetFramework`   | Exact declared TFM string.                                                                                                                                                                              |
| `configuration`     | Build configuration string and compiler-option/reference content digest.                                                                                                                                |
| `assembly`          | Assembly name and SHA-256 content hash; path, if present, is root-relative.                                                                                                                             |
| `sourceFingerprint` | SHA-256 over sorted root-relative source paths and exact compiler-input content hashes, including generated inputs used by the collector. No mtime or machine path.                                     |
| `policyFingerprint` | SHA-256 over the manifest/schema/record input index and their exact content hashes.                                                                                                                     |
| `adapter`           | Producer ID, producer version and contract version. IDs identify integration producers, not permitted frameworks.                                                                                       |
| `runId`             | Required opaque runner invocation identity for result evidence; discovery may use its own discovery-session ID. Correlation is explicitly within the declared assembly/source/configuration identities. |
| `discovery`         | `{ state, methods, cases }`; state is `complete`, `deferred`, `unavailable` or `unsupportedContract`.                                                                                                   |
| `bindings`          | Explicit code relationships, their validity and source locations, including invalid/unresolved references.                                                                                              |
| `results`           | Runner outcomes keyed by case identity and run, or an explicitly absent result set.                                                                                                                     |
| `inputs`            | Sorted `{ path, sha256, kind }` entries proving the supplied source/manifest/record/schema scope.                                                                                                       |

Hash the length-prefixed UTF-8 path, kind and hash values in ordinal path/kind
order. Content hashes use the bytes supplied to the compiler/collector, not a
later ambient reread. Identical logical inputs on different machines produce the
same fingerprints. Run identity is supplied execution data, not an analyzer clock.

### Methods and cases

A method record carries `symbolId` (Roslyn documentation-comment ID), declaring
type metadata name, method metadata name, generic arity and parameter type
identities. Source location is `{ path, line, column }`, one-based and relative
to the root; generated/unsourceable locations are explicitly absent, never guessed.
Use these fields to disambiguate overloads and generic methods.

A case record carries `caseId`, `methodSymbolId`, optional opaque row identity,
and a display name used only for presentation. The case key is
`(project, targetFramework, assembly hash, adapter ID, caseId)`; it is not the
display name. A runner's unstable per-run result ID cannot substitute for its
stable case ID. The producer must correlate such IDs before exporting evidence.

A method can have zero, one or many known cases. Runtime-only rows have
`discovery.state: "deferred"` until actual runner events enumerate them. Do not
create a synthetic passing row or claim a known case count. Metadata that cannot
uniquely map a method is unresolved evidence, not a guessed method-name match.

A result carries `caseId`, `runId`, `outcome` and optional runner detail. Outcomes
are `passed`, `failed`, `skipped`, `notRun` or `aborted`; absence remains distinct.
Duplicate conflicting results for one case/run make that input invalid; identical
repeats do not create additional executions. Different run IDs remain separate;
never select the latest by ambient time. The CLI can select a run explicitly.

### Binding records

A binding record carries `symbolId`, `role`, its own target components, source
location, and separate `syntaxValidity`, `resolution`, `eligibility` fields.
Target kinds are `claim` and `adr`; authoring defaults are context, not bindings.
Preserve invalid/raw target fields for findings without treating them as evidence.
Deduplicate valid relationships by symbol, role and resolved target under F2's
rules. No enclosing type context or neighboring binding completes checked IDs.

Collection uses current compiler/documentation metadata during the normal build
and runner integration during discovery/execution. Compiler-produced XML member
IDs are suitable identity inputs; display names and assertion text are not.
The shared binding resolver validates records even if diagnostic presentation is
suppressed. Export through an explicitly invoked build/collector step into its
selected intermediate/output destination; DiagnosticAnalyzer writes no files.

### Scenarios and producer extensions

Optional `scenarios` records carry the scenario source path, a stable generator
scenario ID, explicit effective target scope, local claim IDs, optional outline
row ID and the associated runner case key. F4 defines scope and metadata mapping.
A consumer bridge can produce these records without Reqnroll. Arbitrary producer
extensions cannot add new checked target kinds or silently change version-one
semantics.

### Freshness and aggregation

Join only evidence with matching project/TFM, source, assembly and configuration
identities. Discovery and a later execution can have different session IDs but
must agree on stable case keys and relevant current identities. A policy change
requires revalidating bindings against the current record catalog; an old binding
verdict cannot be carried forward as current evidence without that evaluation.

Retain stale/mismatched artifacts visibly and exclude their discovery/results
from current evidence. A valid current source binding remains independently
resolvable. Missing a declared compilation makes repository scope incomplete;
missing/deferred cases or results never become passing evidence.

### Collection boundary and conformance

Implement adapters around test-platform/runner discovery and execution surfaces,
with an exporter entry point that accepts the common records for other producers.
Do not add framework-specific reference grammars to the core. The compatibility
matrix records tested producer/platform versions and metadata capabilities.
Conformance cases include xUnit facts/theories, NUnit, MSTest, Playwright .NET,
custom producer records, identical display names, overloads, deferred data,
missing metadata and stale artifacts. No compatibility claim is made merely
because a framework name appears in a fixture.

## 8. Testing Strategy

<!-- last written by: test-writer, 2026-10-17 -->

The companion scenarios state the acceptance obligations. Production step definitions and mechanism tests are not written; each claim remains Missing in § 9. Spike results do not promote these rows to Covered.

## 9. Traceability Matrix

<!-- last written by: test-writer, 2026-10-17 -->

| Claim ID | Scenario                              | Test    | Status  |
| -------- | ------------------------------------- | ------- | ------- |
| B-001    | xUnit fact                            | Missing | Missing |
| B-002    | xUnit theory                          | Missing | Missing |
| B-003    | Playwright dotnet                     | Missing | Missing |
| B-004    | Unavailable evidence                  | Missing | Missing |
| B-005    | Stable correlation                    | Missing | Missing |
| B-006    | Evidence identity matching            | Missing | Missing |
| B-007    | Framework-independent runner evidence | Missing | Missing |
| B-008    | Runtime-only case enumeration         | Missing | Missing |
| B-009    | Versioned runner evidence             | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-17 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-17 -->

| ID   | Question                                                                            | Blocks                                            | Resolution                                                                                                                                                                                                                                                                                                                                                                                               |
| ---- | ----------------------------------------------------------------------------------- | ------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | Which xUnit versions, dynamic theory discovery modes and output formats ship first? | Concrete collection integration and schema design | RESOLVED for behavior, 2026-10-17: decision 0001 replaces a framework/version allowlist with a common runner-evidence contract and extensible collection adapters. xUnit, NUnit, MSTest and Playwright are validation cases. Runtime-only cases remain unenumerated until supplied; collection produces versioned JSON. Concrete integration compatibility and wire schema remain design/test-plan work. |
| OQ-2 | What runner metadata and source identity reject stale discovery and results?        | Evidence schema design                            | RESOLVED for metadata semantics, 2026-10-17: F6 decision 0004 requires source/assembly/configuration matching. Decision 0001 adds case IDs, method metadata, project/framework identity, adapter version and run identity in versioned JSON. Exact fields and encoding remain technical design.                                                                                                          |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-17 -->

| Section | Status | Reviewer | Note                                                                                                              |
| ------- | ------ | -------- | ----------------------------------------------------------------------------------------------------------------- |
| 1-12    | 🟡     | Pending  | Owner selected the model; this decomposed Feature has not received detailed agreement or implementation sign-off. |

## Tasks

- [0244: Collect framework-independent test evidence](../.issue/0244-test-evidence-collection.yml)

## Scoring

| Field | Value | Basis           |
| ----- | ----- | --------------- |
| value | 0     | Not yet scored. |
| risk  | 0     | Not yet scored. |
