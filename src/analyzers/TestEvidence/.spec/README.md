---
title: "Specification: Test evidence adapters"
description: "Map explicit test-method bindings to runner evidence across .NET test frameworks without requiring Gherkin."
type: feature
id: "F3"
epic: "0201"
spec_status: in-review
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
updated: "2026-10-23"
github_issue: null
synced_at: null
---

# Specification: Test evidence adapters

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-18 -->

Map explicit test-method bindings to discovered and executed test cases across .NET runners. Maintainers can inspect which cases a method represents, which runner outcomes were observed, and whether those artifacts still describe the current build inputs.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-18 -->

| #   | Persona             | Need                                                                                               | Pain Point Today                                                 |
| --- | ------------------- | -------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------- |
| 1   | Consumer maintainer | Inspect current test-case and execution evidence for explicitly bound methods across .NET runners. | References and test names can drift without an inspectable link. |

### Assumptions

| ID  | Assumption                                                                                                                                                            |
| --- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | The owner-selected project homes are Analyzers, CodeFixes and deployment CodeAnalysis (ADR-0016); implementation internals are defined by the owning Feature designs. |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-23 -->

| ID    | Claim                                                                                                                                                                                                                                     | Source                                             | Status  |
| ----- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------- | ------- |
| B-001 | Given a bound xUnit fact in a selected test project without Reqnroll, this Feature ensures its discovered test case is linked to that method and claim.                                                                                   | Epic 0201; decision 0001 in 0201-F1                | Active  |
| B-002 | Given a bound xUnit theory discovering two data rows without Reqnroll, this Feature ensures both test cases link to the same bound method without treating them as two methods.                                                           | Epic 0201; decision 0001 in 0201-F1                | Active  |
| B-003 | Given a bound Playwright .NET test with conforming runner evidence and no Reqnroll, this Feature links the discovered case through its CSharp method to the declared claim.                                                               | Epic 0201; decision 0001 in 0201-F1; decision 0001 | Amended |
| B-004 | Given a selected test project with missing collection integration or insufficient case/method metadata, this Feature reports unavailable or unresolved evidence rather than empty complete coverage.                                      | Epic 0201; decision 0001 in 0201-F1; decision 0001 | Amended |
| B-005 | Given two discovered cases with duplicate display names but distinct runner identities, this Feature ensures they remain distinct and correlate through runner identity and method metadata.                                              | Epic 0201; decision 0001 in 0201-F1                | Active  |
| B-006 | Given supplied discovery or results, this Feature checks their source, assembly and configuration identities against the current analysis inputs.                                                                                         | 0201-F6 decision 0004                              | Active  |
| B-007 | Given conforming runner evidence from a .NET test framework, this Feature maps case and method identities through the common evidence contract without a framework-name allowlist.                                                        | Decision 0001                                      | Active  |
| B-008 | Given runtime-only parameterized test cases, this Feature leaves case enumeration incomplete until the runner supplies the cases and never invents cases or counts.                                                                       | Decision 0001                                      | Active  |
| B-009 | Given collected test evidence, this Feature reads versioned JSON carrying runner case IDs, method metadata, project/framework identity, source/assembly/configuration fingerprints, adapter version and run identity.                     | Decision 0001                                      | Active  |
| B-010 | Given a bound xUnit v2 theory discovered under VSTest, this Feature records each theory case with its own stable runner case identity linked to the bound method.                                                                         | Consumer request #256; decision 0001; OQ-3         | Active  |
| B-011 | Given a VSTest run of a bound xUnit v2 theory, this Feature records each case's execution outcome against that case's own runner identity.                                                                                                | Consumer request #256; decision 0001; OQ-3         | Active  |
| B-012 | Given a bound test method compiled from a source file outside its test project's directory, this Feature resolves its method identity and repository-relative source location.                                                            | Consumer request #256                              | Active  |
| B-013 | Given a VSTest case that failed, was skipped or was not executed, this Feature records that outcome and never records the case as passed.                                                                                                 | Consumer request #256; 0201-F5 decision 0002       | Active  |
| B-014 | Given a bound method for which VSTest discovered no case, this Feature reports its discovery as unavailable or incomplete rather than as a passing or complete empty set.                                                                 | Consumer request #256; decision 0001               | Active  |
| B-015 | Given a bound theory with one passing case and another failed, skipped or missing case, this Feature keeps the outcomes per case and never reduces them to one method-level outcome.                                                      | Consumer request #256; 0201-F5 decision 0002       | Active  |
| B-016 | Given Reqnroll-generated tests executed under VSTest, this Feature records their discovered cases and outcomes in the common evidence contract without bindings on generated code.                                                        | Consumer request #256; 0201-F4 B-008               | Active  |
| B-017 | Given a clean checkout with no evidence artifact, this Feature lets the build succeed and the analysis report the evidence as missing with a diagnosis naming the step that produces it.                                                  | Consumer request #256                              | Active  |
| B-018 | Given an evidence artifact whose recorded inputs no longer match the current analysis inputs, this Feature reports it as stale with a diagnosis naming the step that regenerates it.                                                      | Consumer request #256; 0201-F6 decision 0004       | Active  |
| B-019 | Given a clean checkout following the documented build, discovery-and-run and analysis sequence, this Feature supplies the analysis with current evidence without hand-written evidence, a pre-existing cache or a diagnostic suppression. | Consumer request #256                              | Active  |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-18 -->

| ID  | Constraint                                                                                                          | Rules Out                                                          |
| --- | ------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------ |
| C-1 | The binding model is framework-independent; adapter-specific recognition does not alter reference semantics.        | Framework-specific reference grammar.                              |
| C-2 | Reqnroll and Gherkin are optional; source bindings, discovered cases and execution results remain distinct records. | Treating a method declaration as an executed test case.            |
| C-3 | Analysis consumes supplied runner artifacts and never launches tests, browsers or external providers.               | Collecting evidence during analysis or contacting providers.       |
| C-4 | Evidence identity is derived from supplied logical inputs and opaque caller-provided run identities.                | Ambient paths, timestamps or analyzer clocks in evidence identity. |
| C-5 | Runner display names are presentation only; cases correlate by stable runner case and method identities.            | Merging cases by display name or guessing method identity.         |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-18 -->

| #   | Item                                                                                        | Exclusion Reason                                                           |
| --- | ------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------- |
| 1   | JavaScript and TypeScript Playwright bindings; compiler bindings cover CSharp declarations. | This agreement does not authorize those language integrations.             |
| 2   | Claim inference from display names, assertion text or scenario titles.                      | This agreement requires explicit source bindings.                          |
| 3   | Guessing case or method identity when runner metadata is insufficient.                      | Missing metadata remains unavailable or unresolved.                        |
| 4   | Scenario-to-case mapping and scenario scope policy.                                         | 0201-F4 owns the optional scenario bridge.                                 |
| 5   | Required-link policy, coverage exemptions and reconciliation presentation.                  | 0201-F6 owns policy; 0201-F5 owns reconciliation and reporting.            |
| 6   | Starting or scheduling a build, test run or browser session to collect evidence.            | Collection consumes normal, explicitly invoked build and runner workflows. |

## 6. Concern Separation

<!-- last written by: implementer, 2026-10-18 -->

| Concern                                              | Classification | Owner and boundary                                                                                                      |
| ---------------------------------------------------- | -------------- | ----------------------------------------------------------------------------------------------------------------------- |
| Declared method-to-claim/ADR binding                 | Business       | 0201-F2 owns syntax, target validation, resolution and lifecycle eligibility.                                           |
| Discovered method and case identity                  | Both           | F3 exports compiler method identity and runner case identity without inferring either from display text.                |
| Runner outcomes and run separation                   | Business       | F3 retains each supplied run and outcome; F5 decides how selected evidence contributes to reconciliation.               |
| Source, assembly, configuration and policy freshness | Both           | F3 records identities; 0201-F6 owns current-input scope and 0201-F5 excludes stale artifacts from current evidence.     |
| Scenario-to-case metadata                            | Both           | F3 carries the common case identity; 0201-F4 defines scenario scope and producer metadata mapping.                      |
| Evidence serialization and versioning                | Technical      | F3 owns the closed version-one JSON envelope and its deterministic identity encoding.                                   |
| Runner collection and producer compatibility         | Technical      | First-party adapters normalize runner metadata; custom producers may submit the same contract without framework gating. |
| Diagnostic and filesystem boundary                   | Technical      | DiagnosticAnalyzer remains read-only; explicit build/runner collection writes artifacts to the selected output folder.  |

## 7. Technical Design

<!-- last written by: implementer, 2026-10-19 -->

The version-one artifact implements decision 0001 and 0201-F6 decision 0004.
Adapters produce it during normal build, discovery or execution workflows. The
analyzer and trace command consume supplied artifacts and never launch a runner.
Framework names are metadata and never determine whether a conforming artifact is
accepted.

### Envelope

Use UTF-8 JSON with `format: "specht.test-evidence"` and integer
`formatVersion: 1`. Every top-level property is required except `scenarios`.
Objects are closed: unknown properties and enum values are invalid. An unknown
format version is `unsupportedContract`. This contract version is independent of
the package and manifest schema versions.
Object property names are camelCase and property order is insignificant. Enum
values use the lowercase spellings shown here; they are not serialized as
language-specific enum names or numeric values. `formatVersion` versions the
artifact, while `adapter.contractVersion` versions the producer integration.

| Property            | Contract                                                                                                                                        |
| ------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------- |
| `format`            | Exact string `specht.test-evidence`.                                                                                                            |
| `formatVersion`     | Integer `1`.                                                                                                                                    |
| `project`           | Repository-relative project file path and exact declared target framework.                                                                      |
| `configuration`     | Build configuration name and digest of compiler options and resolved reference identities.                                                      |
| `assembly`          | Assembly name and SHA-256 content hash; optional assembly path is repository-relative.                                                          |
| `sourceFingerprint` | SHA-256 over sorted repository-relative source paths and exact compiler-input content hashes, including generated compilation inputs.           |
| `policyFingerprint` | SHA-256 over the manifest, selected schemas, records and policy input index with their exact content hashes.                                    |
| `adapter`           | Stable producer ID, producer version and contract version. The producer ID names an integration, not a permitted framework.                     |
| `discovery`         | Discovery state, opaque session ID, method records and case records. States are `complete`, `deferred`, `unavailable` or `unsupportedContract`. |
| `bindings`          | Explicit method relationships, raw target components, F2 resolution and eligibility findings, and source locations.                             |
| `results`           | Either `absent` or an opaque run ID and outcomes keyed by stable case ID. Absence is not a passing result.                                      |
| `inputs`            | Sorted `{ path, sha256, kind }` entries for the source, manifest, schema and record inputs represented by the artifact.                         |
| `scenarios`         | Optional F4 scenario records. When present, they follow the F4 contract and do not alter F3 case identity.                                      |

The nested shapes are fixed: `project` is `{ path, targetFramework }`;
`configuration` is `{ name, fingerprint }`; `assembly` is
`{ name, sha256, path? }`; and `adapter` is `{ id, version, contractVersion }`.
Every digest is 64 lowercase hexadecimal characters. A method is
`{ symbolId, declaringType, name, genericArity, parameterTypes, location }`;
a case is `{ caseId, methodSymbolId, rowId, displayName }`. `rowId` and
`displayName` may be null, while IDs are nonempty strings. A source location is
`{ path, line, column }` or null. Lines and columns are one-based.

`discovery` is `{ state, sessionId, methods, cases }`. `sessionId` is a
nonempty opaque string for `complete` or `deferred`, and null for `unavailable`
or `unsupportedContract`; both arrays are always present. `results` is either
`{ state: "absent" }` or
`{ state: "available", runId, cases: [{ caseId, outcome, detail }] }`;
`runId` is a nonempty opaque string and `detail` is a nullable string. `bindings`
is an array of `{ symbolId, role, attributes, shapeValid, status, isEligible,
target, matches, location }`. `attributes` preserves the raw explicit XML
attributes, `shapeValid` is boolean, `role` uses the F2 role vocabulary, and
`status` is null when `shapeValid` is false and otherwise is `incomplete`,
`invalid`, `unresolved`, `ambiguous` or `resolved`, matching F2's
`BindingResolution`. `isEligible` is true or false only for resolved targets
and otherwise null. `target` is null unless resolved; `matches` is an array of F2
target records shaped as `{ kind, epic, feature, id, state, featureState,
relativePath, line }`. `inputs` is an array of `{ path, sha256, kind }`; `kind`
is `source`, `generated-source`, `manifest`, `schema` or `record`. `scenarios`,
when present, is an array in the F4-defined shape.

Hash input contents as SHA-256 over the exact bytes supplied to compilation or
collection. Build each fingerprint by sorting entries by ordinal path then kind
and appending each UTF-8 value (`path`, `kind`, lowercase hexadecimal content
hash) preceded by its unsigned 32-bit big-endian byte length, then SHA-256 the
concatenation. Paths use repository-relative `/` separators. No machine path,
timestamp or analyzer clock participates; identical logical inputs produce the
same fingerprints.

### First-party build and runner collection

The `Specht.CodeAnalysis` package exports compiler methods, explicit F2 bindings,
repository-relative input hashes and fingerprints through a Roslyn incremental
generator. Its MSBuild targets place the MTP collector beside the test output,
register the collector for MTP test applications, and write the requested
artifact path to a runner configuration file. The xUnit v3 MTP collector maps
runner method metadata to the compiler documentation-comment IDs and writes
case IDs, outcomes, session identity and the compiler payload at session end.
The fixture under `test/testevidence.mtp.tests/` exercises xUnit facts, theory
rows and a real Playwright .NET xUnit test class; its Playwright test is skipped
so evidence collection verifies discovery and skip reporting without launching
a browser. The collector preserves deferred discovery when MTP does not provide
a complete-enumeration signal.

### Methods and cases

A method record carries `symbolId` (Roslyn documentation-comment ID), declaring
type metadata name, method metadata name, generic arity, parameter type identities
and source location. A source location is `{ path, line, column }`, one-based and
repository-relative. Generated or unsourceable locations are explicitly absent.
First-party CSharp collection uses the Roslyn documentation-comment ID for
`symbolId`; the build binding exporter and runner adapter must emit the same ID.
Other producers supply their stable method ID as that correlation value.

A case record carries a nonempty stable `caseId`, `methodSymbolId`, optional
opaque row identity and display name. The case key is
`(project, targetFramework, assembly hash, adapter ID, caseId)`; it never uses
the display name. An unstable per-run result ID cannot substitute for a stable
case ID; a producer must correlate it before export. The ID must remain stable
between discovery and execution for the same project, TFM, assembly and adapter.

A method can have zero, one or many known cases. Complete discovery means the
producer enumerated cases available through its supported discovery surface; it
does not claim runtime-only data was enumerated. Runtime-only rows remain
`deferred` until runner events supply them. Do not create synthetic cases or
counts. Metadata that cannot uniquely map a method is unresolved evidence.

A result carries `caseId`, `runId`, `outcome` and optional runner detail.
Outcomes are `passed`, `failed`, `skipped`, `notRun` or `aborted`.
Conflicting results for one case/run invalidate the artifact; identical repeats
do not add executions. Different run IDs remain separate; no ambient-time
selection occurs.

### Bindings and collection

A binding record preserves the method symbol, role, raw explicit target
components, source location and separate syntax validity, resolution and
eligibility. F2 remains the authority for target parsing, identity, resolution and
lifecycle. Invalid or unresolved relationships remain visible but do not become
case evidence. Suppressing a diagnostic does not change the binding result.

Collection combines current compiler symbol records with runner discovery and
result events. Compiler-produced XML member IDs are the first-party CSharp method
identity; display names and assertion text are never identity inputs. Export is an
explicit build/collector step to its selected intermediate/output destination.
`TestEvidenceRunnerCollector` normalizes producer-supplied case and outcome
records before they enter the version-one artifact. Runner records carry opaque
case IDs and either a compiler symbol ID or method metadata that uniquely matches
the compiler-exported methods. Missing or unmatched identity produces unavailable
discovery; conflicting outcomes invalidate collection. The collector retains the
producer ID and version as metadata and does not select adapters by framework name.
Its implementation and input/result records are declared in
`src/analyzers/TestEvidence/`.
The `SpechtTestEvidence` MSBuild item from 0243 supplies those explicit artifact
files to Roslyn as AdditionalFiles. DiagnosticAnalyzer writes no files. Compiler-loaded components retain
`netstandard2.0`; collection and test hosts retain `net10.0;net11.0` under
ADR-0017. Runner-specific packages stay outside the compiler-loaded dependency
closure.

### Scenarios, freshness and compatibility

When present, `scenarios` records carry the scenario path, stable generator
scenario ID, effective target scope, local claim IDs, optional outline row ID and
associated runner case key as defined by 0201-F4. Consumer bridges produce them
without requiring Reqnroll. Version one has no arbitrary extension bag; a new
checked meaning requires a new contract version.

Discovery and results are current only when project, target framework, source
fingerprint, assembly hash, configuration identity and policy fingerprint match
the declared analysis inputs. Discovery and a later execution may have different
session/run IDs but must agree on the stable case key and current input identities.
A changed policy requires revalidating bindings against the current record
catalog. Stale artifacts remain visible and are excluded from current evidence.
Missing a declared project/TFM combination makes scope incomplete; unavailable or
deferred cases and absent results never become passing evidence.

First-party adapters translate supported runner discovery and result metadata
to the common records. An explicit exporter accepts those records from other
producers. The compatibility matrix records tested runner, adapter and platform
versions and the metadata each can supply; it documents evidence and never acts
as a framework allowlist. Conformance covers xUnit facts and multi-row theories,
NUnit, MSTest, Playwright .NET, a custom producer, duplicate display names,
overloads, runtime-only data, missing metadata and stale artifacts. A framework
name alone is not evidence of compatibility.

## 8. Testing Strategy

<!-- last written by: test-writer, 2026-10-19 -->

The acceptance scenarios exercise the version-one reader and common runner
normalizer with synthetic xUnit fact/theory, Playwright .NET and custom producer
records. Mechanism tests cover closed-contract validation, freshness comparison,
case and outcome normalization, overload matching, stable identity, deferred
discovery, deterministic fingerprints and round-trip serialization. These fixtures do not
launch test runners; concrete runner event adapters still need end-to-end
conformance evidence.

## 9. Traceability Matrix

<!-- last written by: test-writer, 2026-10-19 -->

| Claim ID | Scenario                                 | Test                                                                                                                                                                                                                                                                                                                                                                                                                                        | Status  |
| -------- | ---------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------- |
| B-001    | xUnit fact                               | `Xunit fact`; `TestEvidenceSteps.ThenCaseIsLinkedToMethodAndClaim`; `TestEvidenceRunnerCollectorUnitTests.XunitFact_WhenRunnerSuppliesStableCaseAndMethodIds_ShouldLinkOneCaseToOneMethod`; `TestEvidenceRunnerCollectorUnitTests.RunnerMethodMetadata_WhenItUniquelyMatchesAnOverload_ShouldResolveToCompilerSymbolId`; `test/testevidence.mtp.tests/RunnerCases.FactCase`                                                                 | Covered |
| B-002    | xUnit theory                             | `xUnit theory`; `TestEvidenceSteps.ThenTheoryRowsShareOneMethod`; `TestEvidenceRunnerCollectorUnitTests.XunitTheory_WhenRunnerSuppliesTwoRows_ShouldKeepOneMethodAndTwoDistinctCases`; `test/testevidence.mtp.tests/RunnerCases.TheoryCase`                                                                                                                                                                                                 | Covered |
| B-003    | Playwright dotnet                        | `Playwright dotnet`; `TestEvidenceSteps.ThenPlaywrightCaseLinksToClaim`; `TestEvidenceRunnerCollectorUnitTests.PlaywrightDotNet_WhenRunnerSuppliesCSharpMethodIdentity_ShouldRetainTheMethodLink`; `test/testevidence.mtp.tests/PlaywrightRunnerCase.PlaywrightDotNetCase`                                                                                                                                                                  | Covered |
| B-004    | Unavailable evidence                     | `Unavailable evidence`; `TestEvidenceSteps.ThenEvidenceIsNotComplete`                                                                                                                                                                                                                                                                                                                                                                       | Covered |
| B-005    | Stable correlation                       | `Stable correlation`; `TestEvidenceSteps.ThenDuplicateNamesRemainDistinct`; `TestEvidenceArtifactReaderUnitTests.DuplicateDisplayNames_WhenRead_ShouldPreserveDistinctRunnerCaseIdentities`; `TestEvidenceRunnerCollectorUnitTests.DuplicateDisplayNames_WhenRunnerSuppliesDistinctCaseIds_ShouldRemainDistinct`; `TestEvidenceCaseCorrelationUnitTests.DiscoveryAndResultsWithDifferentSessionIds_WhenCorrelated_ShouldMatchStableCaseIds` | Covered |
| B-006    | Evidence identity matching               | `Evidence identity matching`; `TestEvidenceSteps.ThenIdentityMismatchIsStale`; `TestEvidenceArtifactReaderUnitTests.ArtifactIdentity_WhenComparedWithCurrentInputs_ShouldReportFreshness`; `TestEvidenceCaseCorrelationUnitTests.ResultsWithChangedAssemblyIdentity_WhenCorrelated_ShouldBeExcludedAsStale`; `TestEvidenceFingerprintUnitTests.ChangedLogicalInput_WhenHashed_ShouldProduceDifferentFingerprint`                            | Covered |
| B-007    | Framework-independent runner evidence    | `Framework-independent runner evidence`; `TestEvidenceSteps.ThenFrameworkUsesCommonContract`; `TestEvidenceRunnerCollectorUnitTests.CustomProducer_WhenItSuppliesCommonRecords_ShouldBeAcceptedWithoutFrameworkAllowlist`                                                                                                                                                                                                                   | Covered |
| B-008    | Runtime-only case enumeration            | `Runtime-only case enumeration`; `TestEvidenceSteps.ThenEnumerationIsDeferred`; `TestEvidenceSteps.ThenNoCasesInvented`; `TestEvidenceRunnerCollectorUnitTests.RuntimeOnlyRows_WhenRunnerHasNotEnumeratedThem_ShouldRemainDeferredWithoutInventedCases`; `TestEvidenceCaseCorrelationUnitTests.DeferredCasesSuppliedByRunnerExecution_WhenCorrelated_ShouldUseRunnerCases`                                                                  | Covered |
| B-009    | Versioned runner evidence                | `Versioned runner evidence`; `TestEvidenceSteps.ThenRunnerIdsAvailable`; `TestEvidenceSteps.ThenAllIdentityMetadataIsAvailable`; `TestEvidenceRunnerCollectorUnitTests.RepeatedIdenticalOutcomes_WhenCollected_ShouldRemainOneExecution`; `TestEvidenceArtifactWriterUnitTests.ReadArtifact_WhenWrittenAndReadAgain_ShouldPreserveEvidenceIdentities`                                                                                       | Covered |
| B-010    | VSTest theory discovery                  | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                     | Missing |
| B-011    | VSTest theory execution                  | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                     | Missing |
| B-012    | Method compiled from outside the project | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                     | Missing |
| B-013    | Nonpassing VSTest outcomes               | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                     | Missing |
| B-014    | Undiscovered VSTest cases                | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                     | Missing |
| B-015    | One case never masks another             | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                     | Missing |
| B-016    | Reqnroll-generated VSTest cases          | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                     | Missing |
| B-017    | Clean checkout without evidence          | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                     | Missing |
| B-018    | Stale evidence                           | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                     | Missing |
| B-019    | Clean-checkout bootstrap                 | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                     | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-23 -->

2026-10-18: completed the version-one envelope, deterministic identity encoding,
producer boundary and stale-evidence behavior under decision 0001. No claim IDs
or scenario tags changed.

2026-10-23: consumer request #256 adds B-010 to B-019 for genuine xUnit v2 and
VSTest evidence, nonpassing and masked cases, and the clean-checkout sequence.
OQ-3 records the open collector mechanism. No existing claim changed.

2026-10-23: the owner resolved OQ-3 with a VSTest test logger registered by
the package build integration; TRX ingestion is rejected. No claim wording
changed.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-23 -->

| ID   | Question                                                                                                                                    | Blocks                            | Resolution                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     |
| ---- | ------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | Which runner integrations and versions produce supported evidence?                                                                          | Concrete adapter compatibility    | RESOLVED, 2026-10-18: decision 0001 rejects a closed framework/version allowlist. Version one accepts producers that emit this Feature's contract; the compatibility matrix names only runner, adapter and platform versions actually tested and the metadata they supply. Runtime-only cases remain deferred until supplied by the runner.                                                                                                                                                                                                                                                    |
| OQ-2 | Which identities determine whether discovery and result artifacts are current?                                                              | Evidence envelope and freshness   | RESOLVED, 2026-10-18: artifacts match project, TFM, source fingerprint, assembly hash, configuration identity and policy fingerprint. Discovery and execution retain separate opaque session/run IDs; cases correlate by the stable case key in § 7.                                                                                                                                                                                                                                                                                                                                           |
| OQ-3 | Which collection mechanism produces xUnit v2 and VSTest evidence: a VSTest data collector, a VSTest test logger, TRX ingestion, or another? | B-010, B-011, B-013, B-014, B-016 | Open. Proposed default: a VSTest test logger registered by the package build integration, which receives discovery and result events with managed method metadata and writes the version-one artifact as another producer under decision 0001, with no framework allowlist; TRX ingestion is not the default because a TRX file carries no discovery-only cases and no compiler method identity. RESOLVED, 2026-10-23: owner decision takes the proposed default: a VSTest test logger registered by the package build integration writes the version-one artifact; TRX ingestion is rejected. |

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
