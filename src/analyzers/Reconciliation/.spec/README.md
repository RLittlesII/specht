---
title: "Specification: Traceability reconciliation"
description: "Expose missing links and test outcomes within an explicit project scope."
type: feature
id: "F5"
epic: "0201"
spec_status: approved
status: done
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
blocks: ["F6"]
spikes: []
created: "2026-10-17"
updated: "2026-10-24"
github_issue: null
synced_at: null
---

# Specification: Traceability reconciliation

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-19 -->

Expose missing links and test outcomes within an explicit project scope. The owner selected the separate `specht evidence` command and a passing-outcome gate by default; the claims, constraints and decisions below define the delivered agreement.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-17 -->

| #   | Persona             | Need                                                                     | Pain Point Today                                                 |
| --- | ------------------- | ------------------------------------------------------------------------ | ---------------------------------------------------------------- |
| 1   | Consumer maintainer | Expose missing links and test outcomes within an explicit project scope. | References and test names can drift without an inspectable link. |

### Assumptions

| ID  | Assumption                                                                                                                                                                                                                             |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | Originally, area folders were assumed not to select project boundaries. RESOLVED, 2026-10-17: owner review adopts the Analyzers, CodeFixes and deployment CodeAnalysis project homes (ADR-0016); implementation internals remain open. |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-24 -->

| ID    | Claim                                                                                                                                                                                                                                         | Source                                              | Status  |
| ----- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------- | ------- |
| B-001 | Given a claim bound to a discovered test through the common runner-evidence contract and a matching result with no scenario policy, this Feature presents the claim-to-method-to-case-to-result chain without requiring Reqnroll.             | Epic 0201; F3 decision 0001                         | Amended |
| B-002 | Given a declared repository scope omitting a required test project or input, this Feature ensures the report marks coverage incomplete and identifies the missing input.                                                                      | Epic 0201; decision 0001 in 0201-F1                 | Active  |
| B-003 | Given a resolved binding with a failed, skipped or absent test result, this Feature ensures the report keeps that outcome distinct from a passing result.                                                                                     | Epic 0201; decision 0001 in 0201-F1                 | Active  |
| B-004 | Given an explicit policy requiring a link absent from a claim's evidence, this Feature ensures the report identifies that missing link instead of treating reference resolution as complete coverage.                                         | Epic 0201; decision 0001 in 0201-F1                 | Active  |
| B-005 | Given scenario bridge evidence supplied for part of a declared scope, this Feature ensures those scenario links augment the direct chains without inventing links for other tests.                                                            | Epic 0201; decision 0001 in 0201-F1                 | Active  |
| B-006 | Given a declared specification claim requiring test evidence with no test binding at all, this Feature ensures reconciliation reports the missing evidence starting from that claim even when no method names it.                             | Epic 0201; decision 0001 in 0201-F1                 | Active  |
| B-007 | Given a binding to a resolving target classified as ineligible under 0201-F1's lifecycle policy, this Feature excludes that binding from current required evidence while retaining the historical link's target identity and lifecycle state. | 0201-F1 decision 0004                               | Active  |
| B-008 | Given a non-exempt current claim in the selected Feature scope, this Feature requires at least one valid test binding.                                                                                                                        | 0201-F6 decision 0003                               | Active  |
| B-009 | Given an implementation or scenario link kind, this Feature requires that kind only when the consumer explicitly enables its requirement.                                                                                                     | 0201-F6 decision 0003                               | Active  |
| B-010 | Given an explicitly reasoned coverage exemption, this Feature presents it as exempt rather than covered.                                                                                                                                      | 0201-F6 decision 0003                               | Active  |
| B-011 | Given a declared set of project and target-framework combinations, this Feature reports complete analysis only when current required inputs are available for every declared combination.                                                     | 0201-F6 decision 0004                               | Active  |
| B-012 | Given stale or mismatched discovery or results, this Feature retains their stale or mismatched state and excludes them from current discovery or execution evidence.                                                                          | 0201-F6 decision 0004                               | Active  |
| B-013 | Given absent, stale or mismatched test results, this Feature evaluates binding validity separately from those execution-evidence states.                                                                                                      | 0201-F6 decision 0004                               | Active  |
| B-014 | Given supplied traceability evidence, this Feature presents reconciliation through `specht evidence` with a human summary or separate versioned JSON output.                                                                                  | Decision 0001 reversal                              | Amended |
| B-015 | Given the new trace report contract, this Feature preserves the existing checker report contract unchanged.                                                                                                                                   | Decision 0001                                       | Active  |
| B-016 | Given supplied discovery and result artifacts, this Feature reconciles them without launching test execution.                                                                                                                                 | Decision 0001                                       | Active  |
| B-017 | Given complete required links, this Feature succeeds only when every case key owned by a declared project passed in that project's own run; an owned key that is missing or did not pass fails the default gate.                              | Decision 0002; decision 0003; consumer request #256 | Amended |
| B-018 | Given a required claim whose outline has planned rows, this Feature reports those rows' cases as planned, neither covering the claim nor failing the default gate.                                                                            | Consumer request #256; 0201-F7 B-026                | Active  |
| B-019 | Given an outline with planned rows and an active row whose case is missing or skipped, this Feature fails the default gate for that active row.                                                                                               | Consumer request #256; 0201-F7 B-026                | Active  |
| B-020 | Given a discovered case key its project does not own, this Feature never counts it as passed and never fails the default gate for it.                                                                                                         | Consumer request #256; decision 0003                | Amended |
| B-021 | Given two projects whose required-case scopes partition the same claim files, this Feature succeeds when each owned key passes in its own project's run, without merging keys across projects.                                                | Consumer request #256; decision 0003                | Amended |
| B-022 | Given a declared test project with no current evidence, this Feature fails the default gate with incomplete input (exit 2) naming that project, because its owned keys cannot be known without its artifact.                                  | Consumer request #256; decision 0003                | Amended |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-24 -->

| ID  | Constraint                                                                                                                              | Rules Out                                                           |
| --- | --------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------- |
| C-1 | Resolved reference, required-link completeness and passing execution are separate evidence states.                                      | Reporting resolved IDs as passing behavior.                         |
| C-2 | Reports are deterministic for the same supplied inputs, offline and read-only with repository-relative paths.                           | Nonrepeatable or machine-specific reports.                          |
| C-3 | One compilation cannot assert completeness for unselected or omitted projects.                                                          | Complete-coverage claims over missing projects.                     |
| C-4 | The gate evaluates a project's cases only in that project's own artifact and selected run, and only those it owns that are not planned. | Gating an unowned key; one project's pass satisfying another's key. |
| C-5 | A planned case is reported as planned, is never a covering case and is never gated.                                                     | Planned rows covering a claim or failing the gate.                  |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-24 -->

| #   | Item                                                                      | Exclusion Reason                                                                                                                                   |
| --- | ------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | Automatically declaring assertion adequacy or architectural compliance.   | This agreement does not authorize it.                                                                                                              |
| 2   | Launching test runs or querying remote test services during checking.     | This agreement does not authorize it.                                                                                                              |
| 3   | Changing the approved text-level `specht trace` command.                  | `0001-F13` owns its output and exit contract.                                                                                                      |
| 4   | Re-ingesting native SPCT diagnostics into the evidence report.            | F3's version-one export carries binding verdicts, not diagnostic records; build diagnostics keep their own stream.                                 |
| 5   | Parsing a consumer's `.feature` files.                                    | The analyzer exports the native claim catalogue in the test-evidence artifact and `specht evidence` reads it there (0201-F7 B-030, decision 0003). |
| 6   | Reading the claim catalogue (`claimSource: "gherkin"`) in the reconciler. | Not built by item 0262, which takes planned state from scenario records only; the gap is OQ-3.                                                     |
| 7   | A new finding kind for an omitted owning run.                             | B-022 reuses `missingCompilation` (decision 0004).                                                                                                 |

## 6. Concern Separation

<!-- last written by: implementer, 2026-10-24 -->

| Concern                                    | Classification | Owner and boundary                                                                                                                                           |
| ------------------------------------------ | -------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Selected current claims and required links | Business       | 0201-F6 defines selection, link policy and reasoned exemptions; F5 starts reconciliation from every selected eligible claim.                                 |
| Binding and case identity                  | Both           | 0201-F2 resolves targets and lifecycle; 0201-F3 exports method, case and run identities. F5 joins those records without re-resolving source symbols.         |
| Completeness, freshness and outcome gate   | Business       | F5 evaluates declared project/TFM inputs, required links and passing current results as independent states, gating only owned cases that are not planned.    |
| Evidence collection and scenario mapping   | Both           | 0201-F3 collects runner evidence; optional 0201-F4 records add scenario links. F5 consumes their exported contracts.                                         |
| Artifact reading and report serialization  | Technical      | The tool's reconciliation slice reads explicit files and emits its own versioned report; it does not load the analyzer assembly or alter the checker report. |
| Filesystem and process boundary            | Technical      | F5 reads current repository records and supplied artifacts offline. Only the caller-named report path is written.                                            |

## 7. Technical Design

<!-- last written by: implementer, 2026-10-24 -->

The command implements F5 decision 0001, F6's coverage/input decisions and the
owner's decision to require passing outcomes by default. Current CLI usage is in
`docs/cli.md`.

### Command

```text
specht evidence [--root <dir>] --evidence <path>...
             [--run <id>] [--json] [--report <path>]
```

The root defaults consistently with the existing tool command. Evidence paths
are explicit files or caller-named artifact directories; enumeration is bounded
to those supplied paths, never a repository-wide evidence search. `--run` selects
a result invocation. Without it, accept each project/TFM pair's sole matching
run; several runs for one pair require an explicit choice, not a timestamp
heuristic (Reconciliation flow, below).

Default stdout is one human summary with root-relative findings and separate
input, binding, link, exemption and execution counts. `--json` emits only JSON.
`--report` writes the identical JSON to the caller-named path, the only write
performed by this command. Both forms are offline and consume supplied evidence;
they do not build projects, execute tests or launch browsers.

### Exit behavior

| Exit | Meaning                                                                                                                                                         |
| ---- | --------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `0`  | Required current inputs are complete; every non-exempt required link is satisfied; all current cases supplying required test evidence passed.                   |
| `1`  | Inputs can be evaluated, but required links are missing or required current outcomes are failed, skipped, notRun, aborted or absent.                            |
| `2`  | Required compilation/metadata inputs are unavailable, incomplete, stale or incompatible, including unresolved runtime-only enumeration. Never label this clean. |
| `3`  | Invalid manifest/schema/analysis configuration, consistent with the existing tool's configuration-error category.                                               |
| `4`  | An explicitly named evidence path or requested run cannot be found, or the caller-named report cannot be written.                                               |

If failures overlap, select configuration `3`, explicit selection `4`, input
availability `2`, then policy/outcome `1`; still report all findings that can be
established safely. Requiring passing outcomes does not merge evidence states:
a resolved binding with a skipped result remains a valid binding and a
nonpassing execution. No `--require-passing` flag is needed because this is the
default. A links-only success mode is outside this agreement.

Only cases associated with non-exempt required test evidence participate in the
trace outcome gate. Other supplied results remain visible; they do not create
extra required links. Normal test-workflow exit behavior is independent of this
command. Suppression cannot fill required links or turn nonpassing evidence into
passing evidence.

### JSON version one

Use `format: "specht.evidence-report"`, integer `formatVersion: 1`, and these required
properties. This is separate from the existing checker report and the evidence
producer format.

| Property     | Contract                                                                                                                                                                 |
| ------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `scope`      | Declared projects/TFMs, selected Feature identities and actual supplied combinations; no machine-absolute paths.                                                         |
| `inputState` | `complete`, `incomplete`, `unavailable` or `invalid`, with typed input findings and fingerprints.                                                                        |
| `claims`     | Every selected current claim, including claims no method references. Identity, lifecycle, source location, required-link states, explicit exemptions and evidence links. |
| `bindings`   | Valid/invalid/unresolved/ambiguous relationships with independent resolution and eligibility states; historical links retained.                                          |
| `cases`      | Actual discovered cases, associated method/binding identities and current or stale discovery state.                                                                      |
| `runs`       | Explicit run identities and per-case outcomes, preserving absent, skipped and stale states.                                                                              |
| `findings`   | Typed reconciliation gaps and input failures. Native SPCT build diagnostics remain on their existing diagnostic stream, distinct from evidence validity.                 |
| `summary`    | Counts by the independent states above and the computed exit code. Empty Feature selection is reported as no selected claims, never 100% coverage.                       |

Each required link is `satisfied`, `missing`, `exempt` or `unavailable`; each
exemption carries its explicit reason. A claim with an exemption is visibly
exempt and never increments the covered-claim count. Remaining non-exempt links
and their outcomes are still evaluated. `passing` is an execution state, not a
synonym for link completeness or approval.

Reconciliation gap kinds include `missingRequiredLink`, `missingCompilation`,
`deferredCases`, `missingResult`, `staleEvidence`, `ambiguousRun`,
`incompleteRun` and `invalidInput`. These structured facts are not additional SPEC rules. Supplied
analyzer diagnostics retain their stable SPCT IDs and effective presentation
metadata in the build stream. The CLI gate evaluates the underlying required
evidence independently of whether such diagnostics were suppressed.

Sort identities by epic presence/value, Feature, target kind and local ID using
ordinal comparison, then project/TFM/symbol/case/run. Sort findings by relative
path, line, kind and identity. Do not include generated-at time, discovery time
or an ambient latest-run choice. Byte-identical logical inputs produce the same
JSON. Keep escaped text and multiline messages valid JSON.

### Composition and ownership

`src/tool/Features/Evidence/EvidenceCommand.cs` registers a thin Spectre command.
`EvidenceReconciler.cs` reads the explicit artifacts and current specification
inputs, joins the records and builds the report. `EvidenceGate.cs` folds the exit
code; `EvidencePolicy.cs` projects the consumer's mapped configuration;
`EvidenceScenarioJoin.cs` joins explicit claim and case identities. The existing
`SpecDiscovery` and `SpecDocument` read manifest layouts and Markdown records.
The checker DTO and serialization remain unchanged.

The command cannot use the analyzer directly. It does not load the analyzer
assembly, invoke a DiagnosticAnalyzer, create a Roslyn Workspace or compile a
consumer project. Source bindings and their verdicts must already be exported
by build-time collection; test discovery/results come from normal test-workflow
collectors. Absent exports are unavailable inputs, never an empty complete run.
The analyzer's compilation state and syntax objects do not cross this boundary.

The F5 agreement stays at its adopted specification home; this does not require
putting the CLI reconciler into the compiler-loaded assembly. Core CodeAnalysis
implementation and package delivery do not depend on delivering this command.
This adds no mediator, CQRS pipeline or new shared-core assembly.

### Reconciliation flow

The reconciliation slice owns the report model, artifact input reader and
reconciliation operation. The command calls that operation and renders its result.
It does not reference `Specht.Analysis`: loading that assembly would cross the
compiler boundary. `src/tool/Features/Evidence/Contract/` holds a tool-owned
F3 reader for versions one and two in a distinct namespace. Parity tests compare its fields
and rejection behavior with the producer reader and writer. A malformed or
unsupported envelope remains an input finding with its relative evidence path.

Read the declared `codeAnalysis` project/TFM set, Feature selection, required-link
switches and reasoned exemptions from current configuration. Use the existing
engine's manifest-driven discovery to locate current Feature records, then apply
F6's explicit consumer mappings and lifecycle policy to enumerate every selected
claim, including claims absent from every exported binding. The evidence reader
implements only the normalized selection and record projection it needs;
fixtures shared with F6's input reader establish identical claim identities and
eligibility. Treat an empty explicit Feature selection as zero selected claims,
not complete coverage. The configuration reader must reject invalid normalized
policy before reconciliation; it cannot infer missing project or Feature
selection from the artifacts. Current records, schema files and manifest content
form the policy identity against which F3's `inputs` and `policyFingerprint` are
compared.

Under native Gherkin claim input (`codeAnalysis.records.claimSource: "gherkin"`,
0201-F7) the selected claims come from the supplied artifacts, not from engine
discovery, and the command parses no `.feature` file (decision 0003 of 0201-F7).
`EvidencePolicy` reads `claimSource`; `EvidenceReconciler` takes each selected
Feature's claims, lifecycle, provenance and cases from the F3 `catalog` member of
the current artifacts. Every current artifact must carry a `catalog`, and all of
them must be equal, because they derive from the same hashed claim files; a
missing or differing catalogue is an `invalidInput` finding at the artifact's
evidence path. With no current artifact the selected claims cannot be
enumerated, and each selected Feature is reported as missing, as a missing
Markdown record is. A selected Feature absent from the catalogue is the same
finding. A `planned` case keeps its claim required and never counts as passing
evidence (0201-F7 B-016, B-024); the gate treats it as planned only through
scenario records (decision 0004). Under the
Markdown path `catalog` is not read. The claim files are `record` inputs, so a
changed claim file makes the artifact stale through the existing hash check, and
the stale catalogue is excluded with the rest of the artifact.

For every declared project/TFM pair, require current compiler and runner evidence
in the supplied F3 envelope. Group by the pair and their source, assembly,
configuration and policy identities. Duplicate compatible exports with conflicting contents are
invalid; a missing pair or a pair without current identity is incomplete. Check
repository files named by the input index against their recorded hashes. A
generated compiler input or assembly whose current bytes cannot be established
from a supplied current export is unavailable, never presumed current from an
old artifact's self-reported hash. Preserve stale and mismatched artifacts in the
report but exclude their links and outcomes from the current gate. Distinct run
IDs stay distinct. Without `--run`, a run is chosen for each declared
project/TFM pair: a pair whose current artifacts hold one available run uses
it, and a pair holding several reports `ambiguousRun` for that pair only, so
several projects' runs combine without a selection (item 0260, 0201-F3 OQ-5).
`--run <id>` selects that run wherever it appears; a pair without it has no
selected run. A claim's case references are keyed by project, target framework
and case ID, so keys from two projects never merge (0201-F3 B-021);
`claims[].cases` lists one case-ID string per key, ordered by project, target
framework and case ID. The gate (decision 0004) takes, for each required test
link, the cases of its bound methods in each project/TFM artifact's own selected
run. A case is gated when `EvidenceGate.Gated` holds: its `owned` is not `false`
and it is not planned; version-1 and version-2 artifacts carry no ownership and
count as owned. A gated case that is unselected, has no result or has a
non-passing outcome is a `missingResult` naming the case. A gated, selected case
is listed in `claims[].cases`. An unowned case is never gated, never listed in
`claims[].cases` and never counted in the summary's passed, failed or execution
counts. A case is planned (`EvidenceGate.Planned`) when every current resolved
scenario-record link to its `caseId` has state `planned`; one active link makes
it active, and a link without a case names none. Case rows carry `owned` and
`planned`; a planned case is not in `claims[].cases`, covers nothing and is not
gated, while an active row of the same outline still is. A bound method no
discovered case names gets its own `missingResult` naming its symbol ID
(0201-F3 B-014, OQ-4), and so does a bound method with no owned case that is not
planned, in every artifact ("has no owned case"). A claim whose only cases are
planned therefore has no covering case. A declared project/TFM with no current
export stays `missingCompilation` (exit 2), and its keys are not reported as
passed.

For each current version-3 artifact (0201-F3 § 7) with available results and at
least one case that is owned and not selected, report one `incompleteRun`
finding at the evidence label, identified by its project and target framework,
whose message counts the owned case keys the run did not select (0201-F3
B-026). The finding is informational and does not change the exit code; the
`missingResult` the gate raises for each such owned case does (decision 0004).
Version-1 and version-2 artifacts record no ownership and
get no such finding.

Join eligible resolved `verifies` bindings to current discovered methods by
`symbolId`, then to cases by `methodSymbolId` and outcomes by the F3 case key
and selected run. A resolved ineligible target stays in the binding list with
its lifecycle identity and supplies no current link. Invalid, unresolved and
ambiguous bindings likewise remain visible. Optional F4 scenario records join
only through their explicit resolved case links and effective claim identities;
they neither create cases nor add scenario links to unrelated tests. Never join
on display name, method spelling or a guessed scenario title.

Evaluate each current claim's link kinds separately. `test` is required unless
the exact claim/link has a reasoned exemption; `implementation` and `scenario`
are required only when enabled by policy. A valid current binding satisfies a
test link, while its discovered cases and selected results determine execution
state independently. A required test link with no current case or no passing
required outcome fails the gate even if the binding resolves. An exemption is
recorded with its reason and excluded from covered counts; it does not suppress
other required links or their outcomes. Include all current cases that supply
non-exempt required test evidence, and gate on every such case passing. Keep
other supplied cases and results visible without adding requirements.

Build the trace report from normalized, sorted identities and findings, then
compute the exit code from configuration, explicit selection, input availability,
link and outcome findings in the precedence table above. Serialize once for
`--json` and `--report` so both are byte-identical. The human renderer consumes
the same report object. The existing checker report DTO and serializer are not
modified.

## 8. Testing Strategy

<!-- last written by: test-writer, 2026-10-24 -->

`test/acceptance/Reconciliation/ReconciliationSteps.cs` runs B-001 to B-022
through the actual CLI over an isolated synthetic consumer; `EvidenceTree.cs`
builds one or two projects' version-three artifacts with per-case ownership,
selection and scenario links. Unit tests pin the independent link and exit
decisions, exact scenario joins, normalized policy and parity with the F3
producer reader/writer and F6 mapping projector. Integration tests over
`PartitionedEvidenceRoot` pin the reconciler's gate on owned and planned case
keys, since it reads artifacts with `File` and `Directory` statics and has no
seam for a unit test. No test fetches or executes a consumer test run.

Testability verdict for the owned-key gate (decisions 0003 and 0004), one line
per claim:

- B-017: mechanism is the gated set (`owned != false`, not planned) and the
  `Passed` decision, applied per project artifact in its own run. Unit:
  `EvidenceGateUnitTests.AnOwnershipAndPlannedState_WhenEvaluated_ShouldGateOnlyAnOwnedCaseThatIsNotPlanned` and
  `AnExecutionOutcome_WhenEvaluated_ShouldPassOnlyIfPassed`. Integration:
  `EvidenceReconcilerIntegrationTests.AnOwnedKeyNotPassingInItsOwnRun_*`,
  `AnOwnedCaseItsRunDidNotSelect_*` and `AnArtifactCarryingNoOwnership_*`
  (version-one and version-two artifacts count as owned).
- B-018: mechanism is the planned derivation, a case is planned when every
  current resolved scenario link to it is planned. Unit:
  `EvidenceGateUnitTests.ScenarioLinkStatesOfOneCase_WhenEvaluated_ShouldBePlannedOnlyWhenEveryLinkIsPlanned`.
  Integration: `AnOutlineWithAPlannedAndAPassingActiveRow_*`,
  `AClaimWhoseOnlyCasesArePlanned_*`, `ACaseLinkedByAPlannedAndAnActiveScenario_*`
  and `APlannedLinkNamingNoCase_*`.
- B-019: mechanism is the same derivation leaving an active row gated.
  Integration: `AnActiveRowBesideAPlannedBlock_*` and
  `AnOutlineWithAPlannedAndAnActiveRow_*`; unit as B-018.
- B-020: mechanism is the unowned exclusion from gating, `claims[].cases` and the
  summary counts, and the no-owned-case finding. Unit: the gating decision as
  B-017. Integration: `AnUnownedCaseWithAnyOutcome_*` and
  `ABoundMethodWhoseEveryCaseIsUnownedInEveryProject_*`.
- B-021: mechanism is the per-project artifact and run with keys never merged.
  No unit seam beneath it; code read: `EvidenceReconciler.Reconcile` selects a
  run per project/TFM pair and keys cases by project, framework and case id.
  Integration: `TwoProjectsEachPassingTheKeysTheyOwn_*` and
  `TwoProjectsRunsOverTheSameClaim_*`.
- B-022: mechanism is the `missingCompilation` finding for a declared pair with
  no current export, folded to exit `2` by `EvidenceGate.ExitCode`. Unit:
  `EvidenceGateUnitTests.OverlappingFailures_*`. Integration:
  `ADeclaredProjectWithNoEvidence_*`. This behaviour exists before item 0262,
  so its tests pass on arrival.

What the design makes hard to test: `EvidenceReconciler.Reconcile` is one
method that reads files, joins records and gates in a single pass, so the
planned and owned decisions are reachable only through `EvidenceGate.Gated` and
`EvidenceGate.Planned`, which the implementer must expose, or through an
artifact on disk.

## 9. Traceability Matrix

<!-- last written by: test-writer, 2026-10-24 -->

| Claim ID | Scenario                                                                                                                                     | Test                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                | Status  |
| -------- | -------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------- |
| B-001    | Direct test chain                                                                                                                            | `Direct test chain`; `ReconciliationSteps.ThenDirectChain`; `EvidenceWireParityUnitTests.ProducerArtifact_WhenReadByCli_ShouldRetainTheSameVersionOneFields`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        | Covered |
| B-002    | Incomplete scope                                                                                                                             | `Incomplete scope`; `ReconciliationSteps.ThenIncomplete`; `EvidenceGateUnitTests.OverlappingFailures_WhenFolded_ShouldUseDocumentedPrecedence`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      | Covered |
| B-003    | Outcome distinction                                                                                                                          | `Outcome distinction`; `ReconciliationSteps.ThenOutcomeDistinct`; `EvidenceGateUnitTests.AnExecutionOutcome_WhenEvaluated_ShouldPassOnlyIfPassed`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   | Covered |
| B-004    | Policy gaps                                                                                                                                  | `Policy gaps`; `ReconciliationSteps.ThenMissingPolicyLink`; `EvidenceGateUnitTests.LinkInputs_WhenEvaluated_ShouldRetainTheirIndependentState`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      | Covered |
| B-005    | Optional scenarios                                                                                                                           | `Optional scenarios`; `ReconciliationSteps.ThenOnlyExplicitScenario`; `EvidenceScenarioJoinUnitTests.ExplicitCaseAndClaim_WhenJoined_ShouldExcludeUnrelatedCasesAndClaims`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          | Covered |
| B-006    | Unbound claim                                                                                                                                | `Unbound claim`; `ReconciliationSteps.ThenUnboundClaim`; `SpecDocumentUnitTests.ASection_WhenParsed_ShouldCarryTheHeadersRowsAndOneBasedRowLinesOfTheTableItOpensWith`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              | Covered |
| B-007    | Ineligible target evidence                                                                                                                   | `Ineligible target evidence`; `ReconciliationSteps.ThenIneligibleDoesNotCover`; `BindingLifecycleUnitTests.ClaimAndFeatureLifecycle_WhenEvaluated_ShouldSeparateResolutionFromEligibility`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          | Covered |
| B-008    | Default test requirement                                                                                                                     | `Default test requirement`; `ReconciliationSteps.ThenUnboundClaim`; `EvidencePolicyUnitTests.SelectedPolicy_WhenProjected_ShouldRequireTestByDefaultAndOptionalKindsOnlyWhenEnabled`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                | Covered |
| B-009    | Optional link requirements                                                                                                                   | `Optional link requirements`; `ReconciliationSteps.ThenOptionalRequirement`; `EvidencePolicyUnitTests.SelectedPolicy_WhenProjected_ShouldRequireTestByDefaultAndOptionalKindsOnlyWhenEnabled`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       | Covered |
| B-010    | Visible coverage exemption; An exempt test link leaves the scenario requirement active                                                       | `ReconciliationSteps.ThenExemptNotCovered`; `ReconciliationSteps.ThenScenarioRemainsRequired`; `EvidencePolicyUnitTests.ClaimExemption_WhenProjected_ShouldKeepItsExactReasonAndLinkKind`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           | Covered |
| B-011    | Declared compilation completeness; Older export beside current export; Selected record absent from the input index                           | `ReconciliationSteps.ThenAnalysisState`; `ReconciliationSteps.ThenStaleExtraDoesNotDisplaceCurrent`; `ReconciliationSteps.ThenMissingIndexedRecordIsReported`; `EvidenceGateUnitTests.OverlappingFailures_WhenFolded_ShouldUseDocumentedPrecedence`; `EvidencePolicyUnitTests.EscapingProjectPath_WhenProjected_ShouldBeRejected`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   | Covered |
| B-012    | Stale evidence state                                                                                                                         | `Stale evidence state`; `ReconciliationSteps.ThenEvidenceState`; `TestEvidenceFingerprintUnitTests.ChangedLogicalInput_WhenHashed_ShouldProduceDifferentFingerprint`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                | Covered |
| B-013    | Binding and execution independence                                                                                                           | `Binding and execution independence`; `ReconciliationSteps.ThenBindingIndependent`; `EvidenceGateUnitTests.LinkInputs_WhenEvaluated_ShouldRetainTheirIndependentState`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              | Covered |
| B-014    | Trace report surface; Caller-named JSON report; Unwritable caller-named report; External evidence directory; Overlapping evidence selections | `ReconciliationSteps.ThenFormat`; `ReconciliationSteps.ThenReportMatchesStdout`; `ReconciliationSteps.ThenReportFailureIsRelative`; `ReconciliationSteps.ThenExternalLabelsAreDistinct`; `ReconciliationSteps.ThenOverlappingFileIsDeduplicated`; `EvidenceGateUnitTests.OverlappingFailures_WhenFolded_ShouldUseDocumentedPrecedence`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              | Covered |
| B-015    | Checker report isolation                                                                                                                     | `Checker report isolation`; `ReconciliationSteps.ThenCheckerUnchanged`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              | Covered |
| B-016    | Consume supplied evidence                                                                                                                    | `Consume supplied evidence`; `ReconciliationSteps.ThenNoTestLaunch`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 | Covered |
| B-017    | Passing outcomes required by default; Owned key missing or not passing                                                                       | `Passing outcomes required by default`; `Owned key missing or not passing`; `ReconciliationSteps.ThenDefaultGate`; `ReconciliationSteps.ThenOwnedKeyFails`; `EvidenceGateUnitTests.AnExecutionOutcome_WhenEvaluated_ShouldPassOnlyIfPassed`; `EvidenceGateUnitTests.AnOwnershipAndPlannedState_WhenEvaluated_ShouldGateOnlyAnOwnedCaseThatIsNotPlanned`; `EvidenceReconcilerIntegrationTests.AnOwnedKeyNotPassingInItsOwnRun_WhenAnotherProjectPassedTheSameKeyUnowned_ShouldFailForTheOwnedKeyOnly`; `EvidenceReconcilerIntegrationTests.AnOwnedCaseItsRunDidNotSelect_WhenReconciled_ShouldFailWithAMissingResultNamingTheCase`; `EvidenceReconcilerIntegrationTests.AnArtifactCarryingNoOwnership_WhenItsCaseDidNotPass_ShouldGateItAsOwned`                                                                                                                     | Partial |
| B-018    | Planned rows; Claim whose only cases are planned                                                                                             | `Planned rows`; `Claim whose only cases are planned`; `ReconciliationSteps.ThenPlannedRowsReported`; `ReconciliationSteps.ThenPlannedRowsNeitherCoverNorFail`; `ReconciliationSteps.ThenNoCoveringCase`; `ReconciliationSteps.ThenPlannedOnlyClaimFails`; `EvidenceGateUnitTests.ScenarioLinkStatesOfOneCase_WhenEvaluated_ShouldBePlannedOnlyWhenEveryLinkIsPlanned`; `EvidenceReconcilerIntegrationTests.AnOutlineWithAPlannedAndAPassingActiveRow_WhenReconciled_ShouldSucceedCoveringTheClaimWithTheActiveRowOnly`; `EvidenceReconcilerIntegrationTests.AClaimWhoseOnlyCasesArePlanned_WhenReconciled_ShouldHaveNoCoveringCaseAndFail`; `EvidenceReconcilerIntegrationTests.ACaseLinkedByAPlannedAndAnActiveScenario_WhenItFailed_ShouldBeActiveAndFailTheGate`; `EvidenceReconcilerIntegrationTests.APlannedLinkNamingNoCase_WhenReconciled_ShouldGateNothing` | Partial |
| B-019    | Active row beside planned rows                                                                                                               | `Active row beside planned rows`; `ReconciliationSteps.ThenActiveRowFails`; `EvidenceReconcilerIntegrationTests.AnActiveRowBesideAPlannedBlock_WhenItsCaseDidNotPass_ShouldFailTheGateForThatRow`; `EvidenceReconcilerIntegrationTests.AnOutlineWithAPlannedAndAnActiveRow_WhenBothAreSkipped_ShouldReportThePlannedRowAndFailOnlyForTheActiveRow`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  | Partial |
| B-020    | Partitioned runs; Bound test with no owned case                                                                                              | `Partitioned runs`; `Bound test with no owned case`; `ReconciliationSteps.ThenUnownedKeysNotCounted`; `ReconciliationSteps.ThenNoOwnedCaseFails`; `EvidenceGateUnitTests.AnOwnershipAndPlannedState_WhenEvaluated_ShouldGateOnlyAnOwnedCaseThatIsNotPlanned`; `EvidenceReconcilerIntegrationTests.AnUnownedCaseWithAnyOutcome_WhenReconciled_ShouldNeitherCountNorFailForIt`; `EvidenceReconcilerIntegrationTests.ABoundMethodWhoseEveryCaseIsUnownedInEveryProject_WhenReconciled_ShouldFailAsHavingNoOwnedCase`                                                                                                                                                                                                                                                                                                                                                   | Partial |
| B-021    | Partitioned runs; Owned key missing or not passing                                                                                           | `Partitioned runs`; `Owned key missing or not passing`; `ReconciliationSteps.ThenKeysNotMerged`; `ReconciliationSteps.ThenNoStandIn`; `EvidenceReconcilerIntegrationTests.TwoProjectsEachPassingTheKeysTheyOwn_WhenReconciled_ShouldSucceedWithoutCountingOrListingUnownedKeys`; `EvidenceReconcilerIntegrationTests.TwoProjectsRunsOverTheSameClaim_WhenReconciledWithoutARun_ShouldUseEachProjectsOwnRunAndKeepTheirCaseKeysApart`                                                                                                                                                                                                                                                                                                                                                                                                                                | Partial |
| B-022    | Omitted owning run                                                                                                                           | `Omitted owning run`; `ReconciliationSteps.ThenOmittedRunFails`; `ReconciliationSteps.ThenOmittedKeysNotPassed`; `EvidenceGateUnitTests.OverlappingFailures_WhenFolded_ShouldUseDocumentedPrecedence`; `EvidenceReconcilerIntegrationTests.ADeclaredProjectWithNoEvidence_WhenReconciled_ShouldFailAsIncompleteInputNamingTheProjectAndReportNoPassForItsKeys`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      | Covered |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-24 -->

The 2026-10-19 owner decision moved the evidence gate from `specht trace` to
`specht evidence` after the approved `0001-F13` command was found to own the
former name with an informational exit contract. B-014 and its scenario changed;
decision 0001 records the reversal. The independent passing-outcome gate of
B-017 and decision 0002 remains in force. The version-one F3 export has no
native diagnostic field, so § 5 and § 7 leave SPCT diagnostics in their existing
build stream while this command reports evidence gaps.

2026-10-23: owner decision for consumer request #256 makes planned or ignored
state per case. B-018 and B-019 keep planned rows out of coverage and the gate
while an active row still fails when missing or skipped; B-017 is unchanged.

2026-10-23: owner decision for consumer request #256 gates on run selection.
B-017 is amended so a required case passes only on a pass from a run that
selected it; B-020 and B-021 add the unselected and partitioned cases.

2026-10-23: decision 0003 gates on per-project ownership instead of run
selection, under 0201-F3 decision 0002. B-017, B-020 and B-021 are amended to
owned keys; B-022 fails a declared project with no current evidence.

2026-10-24: decision 0004 settles the mechanics of the owned-key gate. B-022 is
amended to say the omitted owning run fails with exit 2 through the existing
`missingCompilation` finding, not exit 1, since an absent artifact leaves
ownership unknowable. The gate amends the rule in § 7 that a case with
`selected: false` is not gated: an owned unselected case is now a `missingResult`;
the implementer restates § 7 and item 0262's sentence in it. TestEvidence § 7 is
read-only here and keeps `incompleteRun` as an informational finding.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-24 -->

| ID   | Question                                                                                                                                                                                                                     | Blocks                   | Resolution                                                                                                                                                                                                                                                                                                                                                                          |
| ---- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | Which links are required by each selectable consumer policy, and what exemptions are supported?                                                                                                                              | None for coverage policy | RESOLVED, 2026-10-17: 0201-F6 decision 0003 requires a valid test binding for each non-exempt current claim in selected Feature scope. Implementation and scenario requirements are opt-in. Exemptions require explicit reasons and remain visible as exempt, never covered. Configuration encoding remains with 0201-F6 OQ-1.                                                      |
| OQ-2 | What report surface and stale-result policy are public?                                                                                                                                                                      | None                     | RESOLVED for behavior, 2026-10-17; command name revised 2026-10-19: F6 decision 0004 defines freshness and completeness. Decision 0001 adopts `specht evidence` with human summary and separate versioned JSON, preserves the checker report and consumes normal workflow artifacts without launching tests. Section 7 records the delivered schema, options and exit-code mapping. |
| OQ-3 | `claimSource: "gherkin"` is described in § 7 but the reconciler does not read the claim catalogue, so planned state has no source outside scenario records. Which input supplies it for a consumer without scenario records? | B-018, B-019             | Open. Proposed default: scenario records only, as decision 0004 states; a planned Examples block with no scenario record gates as active. 0201-F7 B-026 and 0201-F3 version 2 (`catalog`, item 0259) own the catalogue source.                                                                                                                                                      |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-19 -->

| Section | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            |
| ------- | ------ | ------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-12    | 🟢     | spec-reviewer | The owner selected the separate command and passing gate. B-001–B-017 each have passing acceptance and mechanism evidence; the review found and corrected scenario identity, stale-extra and record-index gaps, and the report-path boundary. The direct implementation preserves ADR-0014, ADR-0017 and ADR-0018. Local full build passed with Format skipped because unrelated shared-checkout files require formatting; targeted format and final focused tests passed on both frameworks. CI and PR review remain separate delivery checks. |

## Tasks

- [0246: Reconcile exported trace evidence in the CLI](../.issue/0246-trace-artifact-reconciliation.yml)

## Scoring

| Field | Value | Basis           |
| ----- | ----- | --------------- |
| value | 0     | Not yet scored. |
| risk  | 0     | Not yet scored. |
