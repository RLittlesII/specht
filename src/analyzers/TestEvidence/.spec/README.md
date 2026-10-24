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
blocks: ["F4", "F5", "F6", "F7"]
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

| ID    | Claim                                                                                                                                                                                                                                                                                                                                                                           | Source                                             | Status  |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------- | ------- |
| B-001 | Given a bound xUnit fact in a selected test project without Reqnroll, this Feature ensures its discovered test case is linked to that method and claim.                                                                                                                                                                                                                         | Epic 0201; decision 0001 in 0201-F1                | Active  |
| B-002 | Given a bound xUnit theory discovering two data rows without Reqnroll, this Feature ensures both test cases link to the same bound method without treating them as two methods.                                                                                                                                                                                                 | Epic 0201; decision 0001 in 0201-F1                | Active  |
| B-003 | Given a bound Playwright .NET test with conforming runner evidence and no Reqnroll, this Feature links the discovered case through its CSharp method to the declared claim.                                                                                                                                                                                                     | Epic 0201; decision 0001 in 0201-F1; decision 0001 | Amended |
| B-004 | Given a selected test project with missing collection integration or insufficient case/method metadata, this Feature reports unavailable or unresolved evidence rather than empty complete coverage.                                                                                                                                                                            | Epic 0201; decision 0001 in 0201-F1; decision 0001 | Amended |
| B-005 | Given two discovered cases with duplicate display names but distinct runner identities, this Feature ensures they remain distinct and correlate through runner identity and method metadata.                                                                                                                                                                                    | Epic 0201; decision 0001 in 0201-F1                | Active  |
| B-006 | Given supplied discovery or results, this Feature checks their source, assembly and configuration identities against the current analysis inputs.                                                                                                                                                                                                                               | 0201-F6 decision 0004                              | Active  |
| B-007 | Given conforming runner evidence from a .NET test framework, this Feature maps case and method identities through the common evidence contract without a framework-name allowlist.                                                                                                                                                                                              | Decision 0001                                      | Active  |
| B-008 | Given runtime-only parameterized test cases, this Feature leaves case enumeration incomplete until the runner supplies the cases and never invents cases or counts.                                                                                                                                                                                                             | Decision 0001                                      | Active  |
| B-009 | Given collected test evidence, this Feature reads versioned JSON carrying runner case IDs, method metadata, project/framework identity, source/assembly/configuration fingerprints, adapter version and run identity.                                                                                                                                                           | Decision 0001                                      | Active  |
| B-010 | Given a bound xUnit v2 theory discovered under VSTest, this Feature records each theory case with its own stable runner case identity linked to the bound method.                                                                                                                                                                                                               | Consumer request #256; decision 0001; OQ-3         | Active  |
| B-011 | Given a VSTest run of a bound xUnit v2 theory, this Feature records each case's execution outcome against that case's own runner identity.                                                                                                                                                                                                                                      | Consumer request #256; decision 0001; OQ-3         | Active  |
| B-012 | Given a bound test method compiled from a source file outside its test project's directory, this Feature resolves its method identity and repository-relative source location.                                                                                                                                                                                                  | Consumer request #256                              | Active  |
| B-013 | Given a VSTest case that failed, was skipped or was not executed, this Feature records that outcome and never records the case as passed.                                                                                                                                                                                                                                       | Consumer request #256; 0201-F5 decision 0002       | Active  |
| B-014 | Given a bound method for which VSTest discovered no case, this Feature reports its discovery as unavailable or incomplete rather than as a passing or complete empty set.                                                                                                                                                                                                       | Consumer request #256; decision 0001               | Active  |
| B-015 | Given a bound theory with one passing case and another failed, skipped or missing case, this Feature keeps the outcomes per case and never reduces them to one method-level outcome.                                                                                                                                                                                            | Consumer request #256; 0201-F5 decision 0002       | Active  |
| B-016 | Given Reqnroll-generated tests executed under VSTest, this Feature records their discovered cases and outcomes in the common evidence contract without bindings on generated code.                                                                                                                                                                                              | Consumer request #256; 0201-F4 B-008               | Active  |
| B-017 | Given a clean checkout with a selected xUnit v2/VSTest project holding an authored `verifies` relationship, mandatory `testMethod`/`verifies` enforcement and warnings-as-errors, this Feature lets the initial build succeed without evidence or a diagnostic suppression.                                                                                                     | Consumer request #256                              | Amended |
| B-018 | Given an evidence artifact whose recorded inputs no longer match the current analysis inputs, this Feature reports it as stale with a diagnosis naming the step that regenerates it.                                                                                                                                                                                            | Consumer request #256; 0201-F6 decision 0004       | Active  |
| B-019 | Given a clean checkout with a selected xUnit v2/VSTest project holding an authored `verifies` relationship, mandatory `testMethod`/`verifies` enforcement and warnings-as-errors, this Feature supplies current passing evidence for that relationship after the documented build, discovery, run and analysis sequence, without hand-written evidence or a pre-existing cache. | Consumer request #256                              | Amended |
| B-020 | Given an evidence artifact from one run, this Feature records for each discovered case its categories (Reqnroll scenario tags mapped to `Category` traits, or xUnit `Trait("Category", ...)` values) and, separately, whether that run selected it, each with its project, assembly and source provenance.                                                                      | Consumer request #256; decision 0002               | Amended |
| B-021 | Given evidence artifacts from several projects' runs over the same claim files, this Feature combines them while keeping every project's case keys distinct and never merging identities across projects.                                                                                                                                                                       | Consumer request #256; decision 0002               | Amended |
| B-022 | Given a clean checkout with a selected xUnit v2/VSTest project holding an authored `verifies` relationship, mandatory `testMethod`/`verifies` enforcement and warnings-as-errors and no evidence collected yet, this Feature reports reconciliation incomplete with a diagnosis naming the step that produces the evidence.                                                     | Consumer request #256                              | Active  |
| B-023 | Given a global package reference and a Release configuration that excludes the test sources and test packages, this Feature lets the Release build succeed without evidence or a diagnostic suppression.                                                                                                                                                                        | Consumer request #256                              | Active  |
| B-024 | Given a Release build beside an evidence artifact left by a Debug build, this Feature never uses that artifact as Release evidence.                                                                                                                                                                                                                                             | Consumer request #256; B-006                       | Active  |
| B-025 | Given a project's declared required-case scope, this Feature marks the discovered case keys whose categories match its tag expression as owned by that project, independently of which cases its run selected.                                                                                                                                                                  | Consumer request #256; decision 0002               | Active  |
| B-026 | Given a run whose recorded selection does not cover every case key its project owns, this Feature reports an incomplete-run diagnosis for that run.                                                                                                                                                                                                                             | Consumer request #256; decision 0002               | Active  |

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

<!-- last written by: implementer, 2026-10-23 -->

| Concern                                              | Classification | Owner and boundary                                                                                                      |
| ---------------------------------------------------- | -------------- | ----------------------------------------------------------------------------------------------------------------------- |
| Declared method-to-claim/ADR binding                 | Business       | 0201-F2 owns syntax, target validation, resolution and lifecycle eligibility.                                           |
| Discovered method and case identity                  | Both           | F3 exports compiler method identity and runner case identity without inferring either from display text.                |
| Runner outcomes and run separation                   | Business       | F3 retains each supplied run and outcome; F5 decides how selected evidence contributes to reconciliation.               |
| Source, assembly, configuration and policy freshness | Both           | F3 records identities; 0201-F6 owns current-input scope and 0201-F5 excludes stale artifacts from current evidence.     |
| Scenario-to-case metadata                            | Both           | F3 carries the common case identity; 0201-F4 defines scenario scope and producer metadata mapping.                      |
| Evidence serialization and versioning                | Technical      | F3 owns the closed version-one to version-three JSON envelopes and their deterministic identity encoding.               |
| Case categories, selection and ownership             | Both           | F3 records runner categories and selection and evaluates 0201-F6's `CaseScope`; 0201-F5 reports and gates on them.      |
| Runner collection and producer compatibility         | Technical      | First-party adapters normalize runner metadata; custom producers may submit the same contract without framework gating. |
| Diagnostic and filesystem boundary                   | Technical      | DiagnosticAnalyzer remains read-only; explicit build/runner collection writes artifacts to the selected output folder.  |

## 7. Technical Design

<!-- last written by: implementer, 2026-10-23 -->

The version-one artifact implements decision 0001 and 0201-F6 decision 0004.
Adapters produce it during normal build, discovery or execution workflows. The
analyzer and trace command consume supplied artifacts and never launch a runner.
Framework names are metadata and never determine whether a conforming artifact is
accepted.

### Envelope

Use UTF-8 JSON with `format: "specht.test-evidence"` and integer
`formatVersion` 1, 2 or 3. Every top-level property is required except `scenarios`
and `catalog`; `catalog` is permitted only at versions 2 and 3, and version 2 differs
from version 1 by that member alone (0201-F7 ADR-0001, proposed). Version 3
differs from version 2 only by three required members on every case record
(below; [ADR-0001](adr/0001-vstest-logger-and-artifact-version-three.md), proposed).
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
| `formatVersion`     | Integer `3` when case records carry categories, ownership and selection; otherwise `2` when `catalog` is present, otherwise `1`.                |
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
| `catalog`           | Versions 2 and 3. The native claim catalogue the analyzer read from Gherkin claim files (0201-F7 B-030); absent under Markdown claim input.     |

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

`catalog` is `{ features }`. A feature is
`{ epic, feature, state, path, claims }`, sorted by `epic` (null first) then
`feature`, ordinal; `epic` is null when the consumer declares no epic grammar,
`state` is the Feature lifecycle or `current`, and `path` is the record path or
the ordinal-first claim file. A claim is
`{ id, state, element, path, line, cases }` in the reader's document order;
`element` is `feature`, `rule`, `scenario` or `scenarioOutline`, and `state` is
the claim lifecycle. A case is `{ line, planned }` with a Boolean `planned`.
Paths are repository-relative and lines one-based. Every claim file is already a
`record` entry of `inputs`, so `policyFingerprint` covers the catalogue's
sources. `TestEvidenceCatalogFeature`, `TestEvidenceCatalogClaim` and
`TestEvidenceCatalogCase` declare the records, and `TestEvidenceCatalogExporter`
projects them from the analyzer's `BindingCatalog` (0201-F7 § 7).

Hash input contents as SHA-256 over the exact bytes supplied to compilation or
collection. Build each fingerprint by sorting entries by ordinal path then kind
and appending each UTF-8 value (`path`, `kind`, lowercase hexadecimal content
hash) preceded by its unsigned 32-bit big-endian byte length, then SHA-256 the
concatenation. Paths use repository-relative `/` separators. No machine path,
timestamp or analyzer clock participates; identical logical inputs produce the
same fingerprints.

### Version three

At version 3 a case is `{ caseId, methodSymbolId, rowId, displayName,
categories, owned, selected }`; at versions 1 and 2 the three new members are
invalid, as any unknown member is. Every other object, and `catalog`, is as at
version 2.

| Member       | Contract                                                                                                                                        |
| ------------ | ----------------------------------------------------------------------------------------------------------------------------------------------- |
| `categories` | Array of nonempty strings, distinct and in ordinal order; empty when the case has none. A category is written as the runner reports it, no `@`. |
| `owned`      | Boolean. Whether the producing project's required-case scope owns this case (B-025); `true` when the project declares no scope.                 |
| `selected`   | Boolean. Whether this artifact's run selected the case (B-020). `false` for every case when `results` is `absent`.                              |

A case with an entry in `results.cases` has `selected: true`; an entry for an
unselected case invalidates the artifact. A selected case may have no entry,
which is never a passing result. Ownership and selection are facts about the
case and the run; neither changes the case key, and the artifact records no
diagnosis derived from them. The reader returns all three; at versions 1 and 2
they are unknown, not empty, false or owned. `TestEvidenceCase` carries them as
nullable members, and the writer emits version 3 only for a producer that
states them.

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
a complete-enumeration signal. It writes version 1 and records no categories,
ownership or selection; the VSTest logger below is the version-3 producer.

### VSTest collection

[ADR-0001](adr/0001-vstest-logger-and-artifact-version-three.md) (proposed)
records the choices; OQ-3 fixes the mechanism.

**Project.** `src/testevidence.vstest/testevidence.vstest.csproj`, beside
`src/testevidence.mtp`: assembly `Specht.TestEvidence.VsTest.TestLogger`,
namespace `Specht.TestEvidence.VsTest`, `net10.0;net11.0` (ADR-0017),
`IsPackable=false`, in `specht.slnx`. It references `analyzers.csproj` as the
MTP collector does, and `Microsoft.TestPlatform.ObjectModel` with
`ExcludeAssets="runtime"` and `PrivateAssets="all"`: `vstest.console` supplies
that assembly. One class, `TestEvidenceVsTestLogger`, implements
`ITestLoggerWithParameters` with `[FriendlyName("specht")]` and
`[ExtensionUri("logger://specht/test-evidence")]`. Adapter ID `vstest`,
adapter version `1.0.0`, contract version `1`.

**Registration.** For a project with `IsTestProject` true,
`IsTestingPlatformApplication` not true and `SpechtRoot` set, the package
targets copy `build/vstest/<tfm>/*.dll` into the output directory, where
`<tfm>` is `net$(BundledNETCoreAppTargetFrameworkVersion)` when the package
ships it and `net10.0` otherwise: the logger runs in `vstest.console`, on the
SDK's runtime, not the test project's. VSTest loads a logger whose file name
ends in `TestLogger.dll` from the test source's directory. The targets append
`specht` to `VSTestLogger`, which `dotnet test` in VSTest mode passes for both
`--list-tests` and a run. `SpechtTestEvidenceWriteRunnerConfig` runs for these
projects too, and the runner config gains `assemblyPath`, `$(TargetPath)`
relative to `SpechtRoot` with `/` separators, which both collectors may record.
A command-line `--logger` sets `VSTestLogger` as a global property and replaces
the appended name; such a run writes no evidence unless it also names
`--logger specht`.

**Packaging.** `codeanalysis.csproj` references the project with
`ReferenceOutputAssembly="false"` and `PrivateAssets="all"`, builds both target
frameworks as it does the MTP collector, and packs
`bin/<configuration>/<tfm>/*.dll` to `build/vstest/<tfm>/`. No TestPlatform
assembly is packed, and `analyzers/dotnet/cs/` is unchanged, so the
compiler-loaded closure is unchanged (ADR-0017). The package integration test
asserts both.

**Per source.** Events are grouped by `TestCase.Source`, so one invocation over
several test assemblies writes one artifact for each. For a source the logger
reads `specht.test-evidence.config.json` beside it, and reads the constant
`Specht.Generated.TestEvidenceCompilationExport.Json` from the assembly's
metadata with `System.Reflection.Metadata`, without loading the assembly. The
assembly name comes from the same metadata and `sha256` from the file's bytes.
With no config or no export it writes nothing, as the MTP collector does.

**Identity.** `caseId` is `TestCase.Id` in lowercase `D` form; the xUnit adapter
derives it from xUnit's test-case unique ID, which covers the assembly, class,
method and serialized row arguments, so each pre-enumerated theory row has its
own (B-010) and discovery and execution agree on it. `rowId` is null and
`displayName` is `TestCase.DisplayName`. Method metadata comes only from the
VSTest managed-name properties `TestCase.ManagedType` and
`TestCase.ManagedMethod`, which `xunit.runner.visualstudio` 3.x sets and 2.x
does not. `ManagedType` is the declaring type; `ManagedMethod` gives the name,
the arity after a backtick and the parameter list, each parameter prefixed
`global::` to match the compiler exporter's form. A nested type (`+`), a generic
parameter (`!`) or a generic instantiation (`` ` ``, `<`) is not translated: the
case keeps no method metadata and stays unresolved (B-004), as does a case from
an adapter that sets neither property. `FullyQualifiedName`, `DisplayName`,
`CodeFilePath` and `LineNumber` are never identity inputs; `CodeFilePath` is an
absolute machine path and is never written. A method's location is the
compiler's: the syntax tree path relative to `SpechtRoot`, which for a file
linked from outside the project directory is that file's own path (B-012).
Reqnroll-generated tests are compiled methods like any other; their generated
source has no location and needs no binding (B-016).

**Discovery.** A discovery request (`dotnet test --list-tests`) supplies
`DiscoveredTests` and `DiscoveryComplete`. The state is `complete` when
discovery was not aborted and the source is in none of the partially, skipped
or not-discovered source lists, `unavailable` when it was aborted, and
`deferred` otherwise. The artifact it writes has `results` `absent` and every
case `selected: false`. A bound method with no case gets none (B-014): no case
is synthesized.

**Run.** A run request supplies `TestRunStart`, `TestResult` and
`TestRunComplete`, and no discovered cases. A case is selected when the run
reported a result for it, or when `TestRunCriteria.HasSpecificTests` named it;
a named case with no result is `notRun`. Outcomes map `Passed` to `passed`,
`Failed` to `failed`, `Skipped` to `skipped`, and `None` and `NotFound` to
`notRun` (B-013); `detail` is `ErrorMessage`. Each case keeps its own outcome
(B-011, B-015). Several results for one `TestCase.Id` are runtime-only rows under
one case (B-008): that case gets no outcome and discovery is `deferred`. The
logger never evaluates `TestRunCriteria.TestCaseFilter`.

**Discovery then run.** At run end the logger reads the artifact already at the
output path. When it reads as valid and `TestEvidenceCaseCorrelation` finds the
same project, target framework, assembly hash, adapter and fingerprints, the
logger keeps that discovery, session ID and case set and adds the run's
selection and outcomes; a run case missing from it makes discovery `deferred`.
Otherwise the run's own cases form the discovery, `deferred`. So one project and
target framework keeps one artifact. VSTest gives a logger no session identity:
each request's session or run ID is a new GUID, opaque and never a time (C-4).

**Categories and ownership.** A case's categories are the values of its
`TestCase.Traits` named exactly `Category`: xUnit `Trait("Category", ...)`, and
the `Category` traits Reqnroll.xUnit generates from scenario tags. The compiler
export payload gains `requiredCaseScope`, the expression text of the
`codeAnalysis.projects` entry whose path is the compiling project, or null. The
logger builds the same `CaseScope` from it and records
`owned = Owns(categories)`, `true` when null (B-025). A malformed expression
never reaches the logger: the build reports SPCT0001 and generates no export.

**Fixture.** `test/testevidence.vstest.fixture/` is a consumer-shaped tree that
no solution builds: two xUnit v2 VSTest projects compiling one shared Reqnroll
claim file and one shared test file linked from outside both project
directories, with passing, failing, skipped and categorized cases. A
`build.tests` integration test copies it to a temporary root with the
repository's `Directory.Packages.props`, a `global.json` carrying the
repository's SDK and `"test": { "runner": "VSTest" }`, packs Specht.CodeAnalysis,
restores from the pack output and the local package cache, and runs
`dotnet test --list-tests` and then filtered `dotnet test` per project. The
logger's event handling is also driven in process from
`test/testevidence.vstest.tests/` with synthetic `TestLoggerEvents`.

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

### Required-case scope

A project's `requiredCaseScope` is a tag expression over case categories,
specified in [0201-F6 § 7](../../../codeanalysis/.spec/README.md): key, grammar and
the A/B partition table. `CaseScope.Owns(categories)` in the analyzer
(`src/analyzers/Policy/`) is the one evaluation: the collector, which
references the analyzer, evaluates it for each case and records the result in the artifact, and `specht evidence` reads
that recorded result and never parses the expression. A case's categories are its Reqnroll scenario
tags or `Category` trait values without `@`. Ownership and selection are the
version-3 case members `owned` and `selected`; the expression reaches the
collector through the compiler export (VSTest collection above). Neither they
nor the incomplete-run finding are SPCT diagnostics.

The incomplete-run diagnosis (B-026) is derived, not stored: `specht evidence`
reports one finding of kind `incompleteRun` for each current version-3 artifact
with `results` `available` and at least one case with `owned: true` and
`selected: false`. Its path is the evidence label, its identity the project and
target framework, and its message the number of owned case keys the run did not
select. In this item it is reported and does not change the exit code; gating on
owned keys is item 0262 (0201-F5 decision 0003). A version-1 or version-2
artifact records no ownership, so it gets no such finding.

### Several projects' evidence

Each artifact is one project and target framework, and its case key is
`(project, targetFramework, assembly hash, adapter ID, caseId)`. `specht
evidence` combines artifacts in `EvidenceReconciler`, which already keys case
rows by evidence label, project and target framework. Two changes keep every
project's keys distinct when several projects compile the same claim files
(B-021):

- A claim's case references carry the project and target framework beside the
  case ID, so equal runner IDs from two projects never collapse into one entry.
- Without `--run`, a run is chosen per declared project and target framework:
  each pair with one available run uses it, and `ambiguousRun` is reported only
  for a pair with several. Today one run is chosen across all artifacts, so two
  projects' runs are always ambiguous. `--run <id>` keeps its meaning.

The tool-owned reader in `src/tool/Features/Evidence/Contract/` reads version 3
with the producer reader's rules; the parity tests cover the new members.
0201-F5 § 7 states the run choice and the `incompleteRun` gap kind once these
are built; whether the per-pair run choice belongs here or to item 0262 is OQ-5.

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

| Claim ID | Scenario                                   | Test                                                                                                                                                                                                                                                                                                                                                                                                                                        | Status  |
| -------- | ------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------- |
| B-001    | xUnit fact                                 | `Xunit fact`; `TestEvidenceSteps.ThenCaseIsLinkedToMethodAndClaim`; `TestEvidenceRunnerCollectorUnitTests.XunitFact_WhenRunnerSuppliesStableCaseAndMethodIds_ShouldLinkOneCaseToOneMethod`; `TestEvidenceRunnerCollectorUnitTests.RunnerMethodMetadata_WhenItUniquelyMatchesAnOverload_ShouldResolveToCompilerSymbolId`; `test/testevidence.mtp.tests/RunnerCases.FactCase`                                                                 | Covered |
| B-002    | xUnit theory                               | `xUnit theory`; `TestEvidenceSteps.ThenTheoryRowsShareOneMethod`; `TestEvidenceRunnerCollectorUnitTests.XunitTheory_WhenRunnerSuppliesTwoRows_ShouldKeepOneMethodAndTwoDistinctCases`; `test/testevidence.mtp.tests/RunnerCases.TheoryCase`                                                                                                                                                                                                 | Covered |
| B-003    | Playwright dotnet                          | `Playwright dotnet`; `TestEvidenceSteps.ThenPlaywrightCaseLinksToClaim`; `TestEvidenceRunnerCollectorUnitTests.PlaywrightDotNet_WhenRunnerSuppliesCSharpMethodIdentity_ShouldRetainTheMethodLink`; `test/testevidence.mtp.tests/PlaywrightRunnerCase.PlaywrightDotNetCase`                                                                                                                                                                  | Covered |
| B-004    | Unavailable evidence                       | `Unavailable evidence`; `TestEvidenceSteps.ThenEvidenceIsNotComplete`                                                                                                                                                                                                                                                                                                                                                                       | Covered |
| B-005    | Stable correlation                         | `Stable correlation`; `TestEvidenceSteps.ThenDuplicateNamesRemainDistinct`; `TestEvidenceArtifactReaderUnitTests.DuplicateDisplayNames_WhenRead_ShouldPreserveDistinctRunnerCaseIdentities`; `TestEvidenceRunnerCollectorUnitTests.DuplicateDisplayNames_WhenRunnerSuppliesDistinctCaseIds_ShouldRemainDistinct`; `TestEvidenceCaseCorrelationUnitTests.DiscoveryAndResultsWithDifferentSessionIds_WhenCorrelated_ShouldMatchStableCaseIds` | Covered |
| B-006    | Evidence identity matching                 | `Evidence identity matching`; `TestEvidenceSteps.ThenIdentityMismatchIsStale`; `TestEvidenceArtifactReaderUnitTests.ArtifactIdentity_WhenComparedWithCurrentInputs_ShouldReportFreshness`; `TestEvidenceCaseCorrelationUnitTests.ResultsWithChangedAssemblyIdentity_WhenCorrelated_ShouldBeExcludedAsStale`; `TestEvidenceFingerprintUnitTests.ChangedLogicalInput_WhenHashed_ShouldProduceDifferentFingerprint`                            | Covered |
| B-007    | Framework-independent runner evidence      | `Framework-independent runner evidence`; `TestEvidenceSteps.ThenFrameworkUsesCommonContract`; `TestEvidenceRunnerCollectorUnitTests.CustomProducer_WhenItSuppliesCommonRecords_ShouldBeAcceptedWithoutFrameworkAllowlist`                                                                                                                                                                                                                   | Covered |
| B-008    | Runtime-only case enumeration              | `Runtime-only case enumeration`; `TestEvidenceSteps.ThenEnumerationIsDeferred`; `TestEvidenceSteps.ThenNoCasesInvented`; `TestEvidenceRunnerCollectorUnitTests.RuntimeOnlyRows_WhenRunnerHasNotEnumeratedThem_ShouldRemainDeferredWithoutInventedCases`; `TestEvidenceCaseCorrelationUnitTests.DeferredCasesSuppliedByRunnerExecution_WhenCorrelated_ShouldUseRunnerCases`                                                                  | Covered |
| B-009    | Versioned runner evidence                  | `Versioned runner evidence`; `TestEvidenceSteps.ThenRunnerIdsAvailable`; `TestEvidenceSteps.ThenAllIdentityMetadataIsAvailable`; `TestEvidenceRunnerCollectorUnitTests.RepeatedIdenticalOutcomes_WhenCollected_ShouldRemainOneExecution`; `TestEvidenceArtifactWriterUnitTests.ReadArtifact_WhenWrittenAndReadAgain_ShouldPreserveEvidenceIdentities`                                                                                       | Covered |
| B-010    | VSTest theory discovery                    | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                     | Missing |
| B-011    | VSTest theory execution                    | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                     | Missing |
| B-012    | Method compiled from outside the project   | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                     | Missing |
| B-013    | Nonpassing VSTest outcomes                 | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                     | Missing |
| B-014    | Undiscovered VSTest cases                  | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                     | Missing |
| B-015    | One case never masks another               | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                     | Missing |
| B-016    | Reqnroll-generated VSTest cases            | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                     | Missing |
| B-017    | Clean checkout builds before evidence      | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                     | Missing |
| B-018    | Stale evidence                             | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                     | Missing |
| B-019    | Clean-checkout bootstrap                   | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                     | Missing |
| B-020    | Partitioned runs over shared claim files   | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                     | Missing |
| B-021    | Partitioned runs over shared claim files   | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                     | Missing |
| B-022    | Clean checkout builds before evidence      | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                     | Missing |
| B-023    | Release excludes the test sources          | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                     | Missing |
| B-024    | Release excludes the test sources          | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                     | Missing |
| B-025    | Partitioned runs over shared claim files   | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                     | Missing |
| B-026    | Selection narrower than the required scope | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                     | Missing |

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

2026-10-23: owner decision for consumer request #256 records run selection
beside discovery. B-020 and B-021 keep each run's selected cases and their
provenance and combine every run's evidence; 0201-F5 B-017 is amended and
B-020 and B-021 there gate on the selecting run.

2026-10-23: review of consumer request #256 made the clean-checkout claims
non-vacuous. B-017 and B-019 are amended to a selected VSTest project with an
authored relationship, mandatory enforcement and warnings-as-errors; B-022 adds
incomplete reconciliation before collection, and B-023 and B-024 the Release
configuration that excludes the test sources.

2026-10-23: decision 0002 separates a project's owned case keys, set by its
declared required-case scope, from discovery and run selection. B-020 and B-021
are amended to record case categories and keep project identities apart; B-025
adds ownership and B-026 the incomplete-run diagnosis.

## 11. Open Questions

<!-- last written by: implementer, 2026-10-23 -->

| ID   | Question                                                                                                                                                                                     | Blocks                            | Resolution                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     |
| ---- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | Which runner integrations and versions produce supported evidence?                                                                                                                           | Concrete adapter compatibility    | RESOLVED, 2026-10-18: decision 0001 rejects a closed framework/version allowlist. Version one accepts producers that emit this Feature's contract; the compatibility matrix names only runner, adapter and platform versions actually tested and the metadata they supply. Runtime-only cases remain deferred until supplied by the runner.                                                                                                                                                                                                                                                    |
| OQ-2 | Which identities determine whether discovery and result artifacts are current?                                                                                                               | Evidence envelope and freshness   | RESOLVED, 2026-10-18: artifacts match project, TFM, source fingerprint, assembly hash, configuration identity and policy fingerprint. Discovery and execution retain separate opaque session/run IDs; cases correlate by the stable case key in § 7.                                                                                                                                                                                                                                                                                                                                           |
| OQ-3 | Which collection mechanism produces xUnit v2 and VSTest evidence: a VSTest data collector, a VSTest test logger, TRX ingestion, or another?                                                  | B-010, B-011, B-013, B-014, B-016 | Open. Proposed default: a VSTest test logger registered by the package build integration, which receives discovery and result events with managed method metadata and writes the version-one artifact as another producer under decision 0001, with no framework allowlist; TRX ingestion is not the default because a TRX file carries no discovery-only cases and no compiler method identity. RESOLVED, 2026-10-23: owner decision takes the proposed default: a VSTest test logger registered by the package build integration writes the version-one artifact; TRX ingestion is rejected. |
| OQ-4 | When discovery is complete and a bound method required for a claim has no discovered case, is that reported as incomplete input (exit `2`) or as today's `missingResult` failure (exit `1`)? | B-014                             | Open. Proposed default: keep `missingResult` (exit `1`) and name the method in its message; the artifact never synthesizes a case and the claim never passes, which meets "neither passing nor a complete empty set". If B-014's "unavailable or incomplete" means the input state, the finding moves to `deferredCases` and exit `2` instead, a 0201-F5 change.                                                                                                                                                                                                                               |
| OQ-5 | Does choosing a run per project and target framework, which combining several projects' runs needs, belong to item 0260 (B-021) or to item 0262 (0201-F5 B-021)?                             | B-021                             | Open. Proposed default: item 0260, because without it two projects' runs are reported `ambiguousRun` and B-021 here cannot be shown; item 0262 then gates on the chosen runs.                                                                                                                                                                                                                                                                                                                                                                                                                  |

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
