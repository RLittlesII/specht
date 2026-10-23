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
blocks: ["F7"]
spikes: []
created: "2026-10-17"
updated: "2026-10-23"
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

<!-- last written by: spec-author, 2026-10-23 -->

| ID    | Claim                                                                                                                                                                                               | Source                              | Status  |
| ----- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------- | ------- |
| B-001 | Given a consumer selecting a subset of projects and declarations for enforcement, this Feature ensures only that selected scope is required to carry bindings and the scope is visible in evidence. | Epic 0201; decision 0001 in 0201-F1 | Active  |
| B-002 | Given the same valid pinned configuration and specification inputs supplied in editor and build, this Feature ensures the same binding yields the same resolution verdict.                          | Epic 0201; decision 0001 in 0201-F1 | Active  |
| B-003 | Given a selected analysis scope with missing or invalid manifest or specification inputs, this Feature ensures analysis reports the input failure rather than silently reporting clean coverage.    | Epic 0201; decision 0001 in 0201-F1 | Active  |
| B-004 | Given a consumer selecting xUnit and Playwright .NET analysis without the scenario bridge, this Feature ensures adoption requires no Reqnroll installation.                                         | Epic 0201; decision 0001 in 0201-F1 | Active  |
| B-005 | Given a binding defect or missing required binding within the selected enforcement scope with no severity override, this Feature reports it as a warning under decision 0001.                       | Decision 0001                       | Active  |
| B-006 | Given duplicate valid relationships with no severity override, this Feature reports the duplicate diagnostic as information.                                                                        | Decision 0001                       | Active  |
| B-007 | Given a consumer severity override for one rule, this Feature applies the selected severity to that rule, including promotion to error.                                                             | Decision 0001                       | Active  |
| B-008 | Given a consumer using standard Roslyn diagnostic controls, this Feature honors analyzer configuration, pragma and SuppressMessage suppression without a custom suppression protocol.               | Decision 0002                       | Active  |
| B-009 | Given an invalid binding whose diagnostic is suppressed, this Feature keeps that binding invalid for evidence purposes.                                                                             | Decision 0002                       | Active  |
| B-010 | Given a missing required evidence link whose diagnostic is suppressed, this Feature does not treat that link as satisfied.                                                                          | Decision 0002                       | Active  |
| B-011 | Given a suppressed diagnostic, this Feature leaves the declared analysis scope unchanged.                                                                                                           | Decision 0002                       | Active  |
| B-012 | Given an explicit binding in an analyzed project, this Feature validates it even when its declaration is outside the scope requiring authored bindings.                                             | 0201-F6 decision 0003               | Active  |
| B-013 | Given generated code, this Feature does not require authored bindings on that code.                                                                                                                 | 0201-F6 decision 0003               | Active  |
| B-014 | Given shared infrastructure, this Feature requires authored method bindings only when its consumer explicitly selects that infrastructure for enforcement.                                          | 0201-F6 decision 0003               | Amended |
| B-015 | Given a requested coverage exemption without an explicit reason, this Feature rejects that exemption.                                                                                               | 0201-F6 decision 0003               | Active  |
| B-016 | Given enabled analysis, this Feature reads the consumer's actual selected manifest, specification, ADR and configured schema contents supplied through Roslyn AdditionalFiles by build integration. | 0201-F6 decision 0004               | Active  |
| B-017 | Given enabled analysis with missing, invalid or incompatible required inputs and no severity override, this Feature reports error diagnostics and marks analysis unavailable or incomplete.         | 0201-F6 decision 0004               | Active  |
| B-018 | Given changed source, specification or configuration inputs, this Feature invalidates the affected analysis verdicts.                                                                               | 0201-F6 decision 0004               | Active  |
| B-019 | Given a private PackageReference to Specht.CodeAnalysis, this Feature deploys analyzers, code fixes and build integration without an application runtime dependency.                                | Decision 0005                       | Active  |
| B-020 | Given a repository release, this Feature packs and publishes Specht.CodeAnalysis alongside specht.tool at the repository release version and attaches both packages to the GitHub Release.          | Decision 0005                       | Amended |
| B-021 | Given installed package versions, this Feature determines interoperability through schema/evidence contract versions rather than exact package-version equality.                                    | Decision 0005                       | Active  |
| B-022 | Given supplied consumer analysis policy, this Feature reads selected scopes, required links, exemptions and record discovery using the consumer's existing schemas and ID grammars.                 | Decision 0005                       | Amended |
| B-023 | Given build integration, this Feature uses MSBuild repository-root selection and input delivery while standard Roslyn configuration owns severity and suppression.                                  | Decision 0005                       | Active  |
| B-024 | Given consumer-supplied schemas and their version, this Feature consumes those inputs without requiring Specht-owned schemas, a schema upgrade or changes to the consumer's pin.                    | Decision 0005                       | Amended |
| B-025 | Given a consumer declaring a test evidence file through the `SpechtTestEvidence` MSBuild item, this Feature makes that evidence available to the analyzer's test-method recognition.                | Item 0257; lesson 0001              | Active  |

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

<!-- last written by: implementer, 2026-10-18 -->

| Concern                                              | Classification | Responsibility                                                                                                                  |
| ---------------------------------------------------- | -------------- | ------------------------------------------------------------------------------------------------------------------------------- |
| Consumer schema identity, version and field meanings | Business       | Supplied schemas and explicit mappings remain authoritative; the analyzer does not prescribe a schema upgrade.                  |
| Selected projects, authoring scope and exemptions    | Business       | Policy separates explicit-reference validation from mandatory authoring and keeps reasoned exemptions visible.                  |
| Configuration and record projection                  | Both           | Compiler-compatible readers use supplied JSON Schema, YAML and Markdown inputs without ambient discovery.                       |
| Current input identity and failures                  | Both           | Fresh snapshots carry content identity and unavailable analysis separately from diagnostic presentation.                        |
| AdditionalFiles transport and diagnostics            | Technical      | A Roslyn adapter reads explicit metadata; SPCT0001 reports input failure. Package discovery and build assets remain downstream. |

## 7. Technical Design

<!-- last written by: implementer, 2026-10-20 -->

[ADR-0018](../../../.spec/adr/0018-lowercase-analyzer-project-home.md) adopts
`src/analyzers` (`Specht.Analysis`), `src/CodeFixes` (`Specht.Codefixes`) and
`src/codeanalysis` (`Specht.CodeAnalysis`, deployment). The deployment project
references both non-packable component projects; CodeFixes references Analyzers.
Analyzer loading must not depend on CodeFixes or its Workspaces dependencies.

The implementation follows decisions 0001 through 0005, including decision 0005's
2026-10-18 reversal on consumer schema ownership. The schema identity and version
are supplied by the consumer repository. A consumer version such as `0.1.0` does
not identify a Specht schema, and knowledge of Specht `0.2.0` does not authorize
an upgrade, substitution or rejection of a valid consumer-owned schema.

### Built policy and input interpretation

The implementation lives in [`src/analyzers/Policy`](../../analyzers/Policy).
`AnalysisInputReader` creates a fresh immutable `AnalysisSnapshot` from supplied
texts; it never selects a schema from the tool's version catalog. `AnalysisPolicy`
keeps projects, selected Features and required links visible, and `Select`
separates explicit validation from mandatory authoring. Test recognition and the
generated flag are current compiler/adapter facts, not name heuristics.

`AnalysisConfigurationReader` projects explicit JSON Pointer mappings, reusing
JsonPointer.Net from the existing JsonSchema.Net dependency for pointer validation
and literal evaluation; local code expands corresponding array wildcards.
`AnalysisPolicyReader` validates the normalized analysis meanings, including
reasoned exemptions and consumer identity grammars. It does not validate the
consumer's original field names against a Specht manifest dialect.

`AnalysisRecordReader` reuses the YAML representation and pipe-table readers
already shared by F1, and uses `BindingFeatureReader.ReadMapped` for configured
column/key roles. ADR selectors read the Markdig tree or YAML metadata; they do
not derive identity from filenames. JsonSchema.Net, already used by the engine,
evaluates supplied schemas with an isolated registry and network fetching disabled.
Configuration and ADR schemas are evaluated when supplied; Feature schema input
is required to read selected Feature records. No embedded schema is substituted.

`AnalysisInputAdapter` reads the existing `SpechtInputKind` and
`SpechtRelativePath` AdditionalFiles metadata. The package delivers the input
kinds `configuration`, `mappings`, `feature`, `scenario`, `adr`, `schema:<kind>`
and `test-evidence`, the last for `SpechtTestEvidence` items; mappings may
instead live in `codeAnalysis.mappings`. Consumer-declared items,
`SpechtTestEvidence` and `SpechtAdditionalRecords`, are added to AdditionalFiles
from `Specht.CodeAnalysis.targets`, which MSBuild evaluates after the project
body, so items the project declares are seen; the props are imported before the
body, where those item lists are still empty. A `SpechtAdditionalRecords` item
carries the `SpechtInputKind` the consumer sets. Current source texts also
contribute to content identity.
`AnalysisInputAnalyzer` reports `SPCT0001` with default error severity, a relative
input path and an explicit unavailable property. Normal Roslyn reporting controls
cannot change the snapshot's underlying availability. It registers one compilation
action because no per-declaration actions exist in this input-only slice.

Content identity hashes length-prefixed kind, relative path and current text in
ordinal order; no state survives a compilation. Cancellation propagates. Snapshot
policy, catalog and exposed collections cannot mutate the supplied configuration.
The original F1 three-argument reader keeps its fixed-column behavior.

The standalone checker recognizes `codeAnalysis` on its existing pins and leaves
its checker settings and SPEC vocabulary unchanged. The shipped catalog remains
`0.1.0`, `0.2.0`; consumer-owned schemas consumed by the analyzer are not limited
to that catalog. `src/codeanalysis` packs the private `Specht.CodeAnalysis`
package beside analyzer and code-fix assemblies, their parser dependency closure,
and `build/Specht.CodeAnalysis.props` and `.targets`. Roslyn compiler assemblies
are supplied by the compiler host and are excluded from the package. The package
has no application runtime assets or package dependencies. Its version is the
same computed repository release version as `specht.tool`.
The release workflow carries both packages from the gated build to the publishing
job. It checks that both versioned files are present before pushing either to
GitHub Packages, then attaches both to the GitHub Release.

The props file finds the nearest `.specht/manifest.json` above the project unless
`SpechtRoot` is supplied. It passes the manifest, co-located and legacy Features,
scenarios, ADRs, frontmatter schemas, and explicitly supplied records/evidence as
AdditionalFiles. Each file carries explicit kind and root-relative path metadata.
Build properties can narrow the Feature, scenario and ADR input globs for a
selected project; generated artifacts, nested checkouts and build outputs are
excluded from the default globs. Empty item metadata on unrelated compiler
AdditionalFiles is ignored. The project path, target framework and repository
root are compiler-visible so declarations can be selected, and rooted compiler
source paths are converted to repository-relative provenance before matching.
Selected projects enable XML documentation parsing to make checked bindings
visible to Roslyn.
For this repository's own adoption, decision 0006 adds the engine and CLI to
the required package-fed build gate. Their entire compilations are selected;
every eligible type, constructor and method in both projects requires an
authored implementation binding. The local package is built
first without a package version property, then all selected projects restore
and compile with that exact version. This exercises the consumer package
without creating a source-project dependency cycle.

The same item construction applies to design-time and normal builds. The targets
file reports a missing root as an MSBuild warning. Standard compiler controls
remain responsible for diagnostic severity and suppression; presentation does
not change whether the analysis snapshot is available.

### Supplied analysis policy

The fields below describe analysis policy, not an extension imposed on the
consumer's schema. Presence of the package in a selected project activates input
validation against the actual supplied consumer schemas and configuration.
Neither the manifest's version alone nor absence of a Specht-owned schema makes
those inputs incompatible. The consumer manifest gains a `codeAnalysis` section
for analyzer settings without advancing its existing schema version. Explicit
mappings read consumer-specific schema metadata and record roles from the existing
configuration. The consumer's schemas and field meanings remain authoritative.
The standalone checker remains independently usable without the package.

| Field            | Type and meaning                                                                                                                                                                                                                                                                          |
| ---------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `projects`       | Required array of `{ path, targetFrameworks }`; root-relative project path and nonempty distinct TFM strings. This is the declared aggregation set.                                                                                                                                       |
| `features`       | Required array of `{ feature, epic? }`; explicit selection, including an explicitly empty selection for ADR-only analysis. No inference from referenced methods.                                                                                                                          |
| `requiredLinks`  | Optional `{ test: true, implementation: false, scenario: false }` defaults; test is always required for non-exempt selected current claims. Implementation/scenario may be enabled.                                                                                                       |
| `enforcement`    | Optional array of `{ project, include, exclude, role, declarationKinds }`. Project must be declared; root-relative source globs select required authoring. Roles are `verifies`, `implements`, `follows`; kinds follow F2's declaration matrix. Default is no mandatory-authoring scopes. |
| `exemptions`     | Optional array of `{ target, links, reason }`. Target is a claim identity or `{ kind: "declaration", project, symbolId }`; links name the requirements exempted. Nonempty reason required. No wildcard or implicit exemptions.                                                            |
| `records`        | Optional record-reading settings below; Feature discovery continues to use existing `layouts`, `exclusions`, `roles`, `tables`, `markers` and identifier settings.                                                                                                                        |
| `mappings`       | JSON Pointer mappings from normalized analysis fields to existing consumer configuration; array wildcards preserve corresponding entries.                                                                                                                                                 |
| `scenarioScopes` | Optional `{ featureTag: "@spec.feature:{feature}", epicTag: "@spec.epic:{epic}" }`; literal forms with exactly their one placeholder. Existing `claim.tag` remains authoritative for local claim tags. These are encodings, not replacement ID grammars.                                  |

Enforcement selection does not validate a declaration as a test merely because
its name looks like a test. Test-method identity comes from current runner or
collector metadata. Missing metadata is unavailable/unresolved evidence. Explicit
relationships are still parsed and validated; no assertion adequacy is inferred.
Generated declarations require no authored binding; explicit bindings remain
subject to validation. Shared infrastructure has no automatic every-method rule.

An exemption never creates a link. Report it and its reason as exempt; it cannot
increment covered counts. Other non-exempt links on the same claim remain visible
and evaluated. Suppression changes presentation, not any of these verdicts.

The following shape illustrates the new manifest section. It does not introduce
or require schema `0.3.0`; consumer-specific metadata is read through mappings.

```json
{
  "codeAnalysis": {
    "mappings": {
      "/schemaVersion": "/schemaVersion",
      "/identifiers": "/identifiers",
      "/markers": "/markers",
      "/claimsSection": "/roles/claims"
    },
    "projects": [
      {
        "path": "test/Example.Tests/Example.Tests.csproj",
        "targetFrameworks": ["net10.0"]
      }
    ],
    "features": [{ "epic": "0201", "feature": "F1" }],
    "requiredLinks": {
      "test": true,
      "implementation": false,
      "scenario": false
    },
    "enforcement": [
      {
        "project": "test/Example.Tests/Example.Tests.csproj",
        "include": ["test/Example.Tests/**/*.cs"],
        "exclude": [],
        "role": "verifies",
        "declarationKinds": ["testMethod"]
      }
    ],
    "exemptions": [],
    "records": {
      "adrProfiles": [
        {
          "glob": ".spec/adr/*.md",
          "owner": "repository",
          "identity": { "headingLevel": 1, "separator": ": " },
          "lifecycle": { "section": "Status", "firstWord": true }
        }
      ]
    }
  }
}
```

### Configuration mappings

The new `codeAnalysis.mappings` object maps normalized analysis field paths to
JSON Pointers in the supplied consumer configuration. Build integration can also
supply the same mappings as an explicit input. Field names in the normalization are analyzer implementation data,
not required consumer keys. Wildcards map corresponding array entries; do not
infer mappings from a schema version or a matching field name. Supplied schemas validate the records before projection. A supplied configuration
schema is also honored; no configuration schema is invented when none is supplied. Schema versions
are opaque consumer values. No shipped Specht schema or manifest defaults are
substituted for supplied inputs.

### Actual consumer record data

Treat the supplied schema version as part of that consumer's contract, not as an
index into Specht's shipped schema catalog. Evaluate the supplied schemas and
mappings; support consumer-owned schemas without substituting embedded ones.
Keep the consumer pin, schemas and record contents unchanged. The new analysis
section is configuration, not a request to version or replace those schemas.

Build discovery enumerates actual files using the configured layouts and ADR
profiles. It does not invent records from schemas. The analyzer receives and
parses each supplied record's text and resolves its declared values.

- Feature identity comes from existing frontmatter roles (`id`, optional `epic`);
  Feature lifecycle uses `spec_status`. Claims come from the manifest's `claims`
  section role and its ID column, preserving the original declaration line.
- `records.featureIdentityKey`, `featureEpicKey` and `claimIdColumn` select consumer
  metadata/column names; their conventional defaults are `id`, `epic` and `ID`.
  `records.claimStatusColumn` defaults to `Status`. Active/Amended and historical
  state meanings are F1 decision 0004. `records.featureLifecycleKey` may replace
  the default frontmatter key, not the lifecycle semantics.
- `records.adrProfiles` entries have `glob`, `owner`, `identity`, `lifecycle`.
  Owner is `repository` or `containingFeature`. The latter selects the unique
  discovered containing Feature; absence or ambiguity is an input failure.
- Identity selects exactly one of `{ frontmatterKey }` or
  `{ headingLevel, separator }`; the heading form takes the text before the
  configured separator. This reads this repository's `# ADR-0016: ...` without
  guessing identity from the filename. The result is validated by `identifiers.adr`.
- Lifecycle selects exactly one of `{ frontmatterKey }` or
  `{ section, firstWord: true }`. The latter reads the first word of the first
  paragraph in that Markdown section, matching this repository's ADR Status
  section. Case is normalized for lifecycle vocabulary, not for record IDs.
- Use the ADR identity grammar when the supplied consumer configuration defines
  one; do not add it to or version the consumer's schema. F1 retains its explicit
  local-identity behavior when no ADR grammar is supplied. Unknown lifecycle
  values, conflicting records or invalid selected metadata are input findings.

Frontmatter and Markdown parsing reuse the existing YamlDotNet representation
model and Markdig pipe-table behavior. No regex Markdown parser or second schema
checker is introduced. A mixed-target solution is expected: the compiler-loaded
components target netstandard2.0 while the engine and tool keep their existing
targets. Do not add a project reference from the analyzer to the modern-only
engine assembly. Choose the smallest compatible reuse of existing text-reading
semantics during F1 implementation; no engine retargeting is required.

### Build delivery

| Surface                                 | Contract                                                                                                                                                                                                 |
| --------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `SpechtRoot` MSBuild property           | Explicit override; otherwise build integration finds the nearest ancestor manifest from the project directory. Never use process working directory. Normalize internally; emit only root-relative paths. |
| `SpechtAdditionalRecords` MSBuild items | Explicit extra consumer files, subject to the same identity/owner validation. No ambient reads inside analyzer callbacks.                                                                                |
| `SpechtTestEvidence` MSBuild items      | Explicit collector JSON files for the current compilation/run. Missing data remains missing; never fetch or run tests to fill it.                                                                        |
| `AdditionalFiles`                       | Actual manifest, selected Feature/ADR/schema/scenario text and current metadata inputs. Build creates the same logical input set in normal and design-time builds.                                       |
| Compiler-visible metadata               | `SpechtInputKind` and `SpechtRelativePath` identify input role and root-relative source. Expose these through `CompilerVisibleItemMetadata`; root selection is compiler-visible build metadata.          |
| Normal build artifacts                  | Collector inputs/outputs use caller/build-selected intermediate paths. DiagnosticAnalyzer itself writes no files; `specht trace` writes only an explicit `--report` destination.                         |

A compilation-start input catalog is immutable and content-keyed. Rebuild affected
state when source, manifest, selected record, schema or collector metadata changes.
Never retain a mutable cross-compilation dictionary. Cancellation propagates;
malformed or missing required input yields SPCT0001 and unavailable analysis.
A genuinely nonexistent referenced record in a complete catalog is SPCT0004,
not an input-delivery error. Normal Roslyn suppression cannot make unavailable
analysis or invalid evidence valid.

### Implementation boundary

Keep analysis in the compiler-loaded component and package only its compatible
dependencies. Reuse the current YamlDotNet representation model, Markdig table
semantics and manifest grammar behavior. Characterize any reused or adapted
text-reading logic against the existing engine's verdicts. Do not retarget the
engine, move its pipeline, create a shared-core assembly or prescribe a
source-sharing mechanism before a concrete implementation needs it.

The portability experiment attempted to compile the whole engine for
netstandard2.0. It did not exercise analyzer loading and establishes no loading
failure. Compiler-host and package tests must instead exercise the actual
analyzer dependency closure.

The read-only analyzer reports diagnostics; it does not write evidence artifacts.
A build-time collection step can run the same analysis over explicit compilation
inputs and export bindings, independently of diagnostic suppression. Normal test
collectors add case/result metadata. `specht trace` consumes those exports; it
does not instantiate a DiagnosticAnalyzer, load a Roslyn Workspace or depend on
IDE analyzer execution. Core binding diagnostics and code fixes do not wait for
that downstream command or runner collection.

This is new capability wiring under ADR-0016/0017, not adoption of ADR-0010's
model-reader/mediator architecture. Dependency versions are central; package
contents include the complete compatible parser dependency closure.

## 8. Testing Strategy

<!-- last written by: test-writer, 2026-10-23 -->

`AdoptionSteps` executes the input/policy scenarios through real in-memory
AdditionalFiles and the production adapter. `AnalysisInputAnalyzerUnitTests` uses
the agreed Rocket Surgery context with direct AwesomeAssertions for diagnostic
count, ID, severity and location, and independently checks compiler errors.
There are no Verify calls or snapshot files.

| Claim | Mechanism and unit coverage                                                                                                                                                                                                                                                                                                                                                                                                                                                       |
| ----- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| B-001 | Project/TFM, source glob and declaration selection: `DeclarationFacts_WhenSelected_ShouldSeparateValidationFromRequiredAuthoring`.                                                                                                                                                                                                                                                                                                                                                |
| B-003 | Explicit unavailable state for missing/invalid inputs: `RequiredInputDefects_WhenRead_ShouldMakeAnalysisUnavailable`.                                                                                                                                                                                                                                                                                                                                                             |
| B-012 | Explicit-reference validation remains enabled outside mandatory authoring: `DeclarationFacts_WhenSelected_ShouldSeparateValidationFromRequiredAuthoring`.                                                                                                                                                                                                                                                                                                                         |
| B-013 | Generated authoring exemption preserves explicit validation: the generated case in `DeclarationFacts_WhenSelected_ShouldSeparateValidationFromRequiredAuthoring`.                                                                                                                                                                                                                                                                                                                 |
| B-014 | Shared infrastructure follows explicit selection: the excluded/outside cases in `DeclarationFacts_WhenSelected_ShouldSeparateValidationFromRequiredAuthoring`, `SelectedRoleAndDeclarationKind_WhenApplied_ShouldRequireOnlyTheConfiguredRelationships`, and the repository package gate.                                                                                                                                                                                         |
| B-015 | Reasonless exemptions fail; reasoned exemptions remain visible: `InvalidPolicy_WhenRead_ShouldReportUnavailableAnalysis` and `ReasonedExemption_WhenSelected_ShouldRemainVisibleWithoutCreatingRequiredLinks`.                                                                                                                                                                                                                                                                    |
| B-016 | Actual consumer record/schema contents and mapped columns: `SuppliedRecords_WhenRead_ShouldUseActualIdentityAndPreserveSelectedScope`, `ConfiguredColumns_WhenRead_ShouldUseTheirNamesInsteadOfColumnPositions`, `ActualAdditionalFiles_WhenAnalyzed_ShouldReadConfiguredConsumerRecordsWithoutInputErrors`, `SuppliedSchemaReference_WhenValidated_ShouldUseTheSuppliedDependencyWithoutFetching` and `ConsumerSchemaConstraint_WhenNotSatisfied_ShouldMakeAnalysisUnavailable`. |
| B-017 | Error diagnostics with unavailable analysis: `MissingInputs_WhenAnalyzed_ShouldReportAnErrorWithoutCompilerErrors`, `MissingSuppliedSchema_WhenRead_ShouldFailInsteadOfUsingAnEmbeddedSchema`.                                                                                                                                                                                                                                                                                    |
| B-018 | Content identity and current catalog replacement: `ChangedInput_WhenReadAgain_ShouldInvalidateContentIdentityAndRetainOriginalSnapshot`, `ChangedClaim_WhenReadAgain_ShouldUseNewContentsAndRetainTheOriginalCatalog`.                                                                                                                                                                                                                                                            |
| B-022 | Explicit consumer mappings and section policy: `ExplicitMappings_WhenProjectingDifferentConsumerKeys_ShouldMapArraysAndLeaveTheOriginalUntouched`, `ManifestSectionMappings_WhenSupplied_ShouldReadExistingMetadataWithoutChangingItsSchemaVersion`.                                                                                                                                                                                                                              |
| B-024 | Consumer-owned versions and unchanged checker pins: `ConsumerSchemaVersion_WhenSupplied_ShouldPreserveItWithoutRequiringASpechtPin`, `AnalysisSection_WhenAddedToAnExistingPin_ShouldPreserveThePinAndCheckerSettings`.                                                                                                                                                                                                                                                           |
| B-025 | The package's `SpechtTestEvidence` delivery kind matches the kind test-method recognition reads: the design-time inventory in `DeploymentProject_WhenPacked_ShouldCarryCompilerAssetsAndNoRuntimeDependencies`; recognition from a supplied artifact: `RunnerSelectedMethods_WhenAnalyzed_ShouldSeparateExplicitValidationFromRequiredAuthoring`. The kind mapping is MSBuild evaluation in `Specht.CodeAnalysis.props`, so its finest tier is that package integration test.     |

`CodeAnalysisPackageIntegrationTests.DeploymentProject_WhenPacked_ShouldCarryCompilerAssetsAndNoRuntimeDependencies`
packs both packages, inspects the analyzer dependency closure and package
metadata, restores a private package reference in a synthetic consumer, checks
its normal and design-time input inventory, and confirms compiler suppression
does not stop input delivery. `CodeAnalysisPackageSteps` binds the package,
version, compatibility and reporting-configuration scenarios to the artifacts
produced by the solution pack. Contract compatibility also remains covered by
`AnalysisInputReaderUnitTests.ConsumerSchemaVersion_WhenSupplied_ShouldPreserveItWithoutRequiringASpechtPin`
and `CodeAnalysisSectionUnitTests.AnalysisSection_WhenAddedToAnExistingPin_ShouldPreserveThePinAndCheckerSettings`.
Compiler dependency loading and parser compatibility remain exercised by the
existing `CompilerClosureIntegrationTests` and `EngineTextCompatibilityIntegrationTests`.
The standard build's `CodeAnalysis` target packs the current analyzer and
builds the analyzer, engine and CLI through private package references before
the test tiers. Generated CI runs the same gate. A manual invalid binding in
the engine produced `SPCT0004` and `SPCT0007` on both target frameworks and
failed the gate; restoring it returned the build to clean. The package
integration and analyzer tests cover input delivery, package assets and
diagnostic behavior without a separate repository dogfood script.

### Performance

<!-- last written by: benchmarker, 2026-10-19 -->

Measured at revision `bd384ed6` on Apple M1 Max, macOS Tahoe 26.4, .NET 10.0.12 Arm64, Release, with BenchmarkDotNet 0.15.8's default isolated job and memory diagnoser. The synthetic inputs hold one source, scenario and ADR fixed while varying distinct Feature records. Input creation and availability checks occur outside the timed operation.

| Concern                                  | Source                                           | Benchmark                                   | Finding                                                                                                                                                                                |
| ---------------------------------------- | ------------------------------------------------ | ------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Fresh content-keyed input reconstruction | § 7 Build delivery and compilation-start catalog | `InputReconstructionBenchmarks.Reconstruct` | At 1/40/160 Feature records, reconstruction took 0.110/1.701/6.833 ms and allocated 179 KB/3.52 MB/13.81 MB per operation. From 40 to 160 records, time grew 4.0× and allocation 3.9×. |

## 9. Traceability Matrix

<!-- last written by: test-writer, 2026-10-23 -->

| Claim ID | Scenario                         | Test                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     | Status  |
| -------- | -------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------- |
| B-001    | Selected scope                   | AdoptionSteps.ThenScopeIsExplicit; AnalysisInputReaderUnitTests.DeclarationFacts_WhenSelected_ShouldSeparateValidationFromRequiredAuthoring; AnalysisPolicyUnitTests.SelectedRoleAndDeclarationKind_WhenApplied_ShouldRequireOnlyTheConfiguredRelationships                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              | Covered |
| B-002    | Editor build parity              | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  | Missing |
| B-003    | Invalid inputs                   | AdoptionSteps.ThenUnavailable; AnalysisInputReaderUnitTests.RequiredInputDefects_WhenRead_ShouldMakeAnalysisUnavailable; AnalysisInputReaderUnitTests.InvalidPolicy_WhenRead_ShouldReportUnavailableAnalysis; AnalysisConfigurationReaderUnitTests.InvalidExplicitMapping_WhenProjected_ShouldRejectIt                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   | Covered |
| B-004    | Optional Reqnroll                | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  | Missing |
| B-005    | Default warning findings         | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  | Missing |
| B-006    | Informational duplicate findings | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  | Missing |
| B-007    | Per-rule severity override       | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  | Missing |
| B-008    | Standard Roslyn suppression      | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  | Missing |
| B-009    | Suppressed invalid binding       | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  | Missing |
| B-010    | Suppressed missing evidence      | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  | Missing |
| B-011    | Suppression preserves scope      | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  | Missing |
| B-012    | Explicit binding validation      | AdoptionSteps.ThenExplicitValidation; AnalysisInputReaderUnitTests.DeclarationFacts_WhenSelected_ShouldSeparateValidationFromRequiredAuthoring                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           | Covered |
| B-013    | Generated authoring boundary     | AdoptionSteps.ThenNoMandatoryAuthoring; AnalysisInputReaderUnitTests.DeclarationFacts_WhenSelected_ShouldSeparateValidationFromRequiredAuthoring                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         | Covered |
| B-014    | Shared infrastructure boundary   | AdoptionSteps.ThenNoMandatoryAuthoring; AdoptionSteps.ThenSharedMethodsRequireBindings; AnalysisInputReaderUnitTests.DeclarationFacts_WhenSelected_ShouldSeparateValidationFromRequiredAuthoring                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         | Covered |
| B-015    | Reasoned exemptions              | AdoptionSteps.ThenUnavailable; AnalysisInputReaderUnitTests.InvalidPolicy_WhenRead_ShouldReportUnavailableAnalysis; AnalysisInputReaderUnitTests.ReasonedExemption_WhenSelected_ShouldRemainVisibleWithoutCreatingRequiredLinks; AnalysisPolicyUnitTests.ReasonedDeclarationExemption_WhenApplied_ShouldPreserveValidationAndExposeItsReason; AnalysisPolicyUnitTests.AdditionalRequiredLinks_WhenATestLinkIsExempt_ShouldRetainTheOtherRequirements                                                                                                                                                                                                                                                                                                                                                                     | Covered |
| B-016    | Consumer record inputs           | AdoptionSteps.ThenReadsRecords; AnalysisInputReaderUnitTests.SuppliedRecords_WhenRead_ShouldUseActualIdentityAndPreserveSelectedScope; AnalysisInputAnalyzerUnitTests.ActualAdditionalFiles_WhenAnalyzed_ShouldReadConfiguredConsumerRecordsWithoutInputErrors; AnalysisInputReaderUnitTests.ConfiguredColumns_WhenRead_ShouldUseTheirNamesInsteadOfColumnPositions; ConsumerSchemasUnitTests.SuppliedSchemaReference_WhenValidated_ShouldUseTheSuppliedDependencyWithoutFetching; ConsumerSchemasUnitTests.ConsumerSchemaConstraint_WhenNotSatisfied_ShouldMakeAnalysisUnavailable; ConsumerRecordMappingsUnitTests.ConsumerFrontmatterKeys_WhenExplicitlyMapped_ShouldUseTheSuppliedIdentityAndLifecycle; ConsumerRecordMappingsUnitTests.FrontmatterAdrProfile_WhenRead_ShouldUseItsDeclaredIdentityLifecycleAndOwner | Covered |
| B-017    | Required input failure           | AdoptionSteps.ThenInputError; AdoptionSteps.ThenUnavailable; AnalysisInputAnalyzerUnitTests.MissingInputs_WhenAnalyzed_ShouldReportAnErrorWithoutCompilerErrors; AnalysisInputReaderUnitTests.MissingSuppliedSchema_WhenRead_ShouldFailInsteadOfUsingAnEmbeddedSchema                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    | Covered |
| B-018    | Input change invalidation        | AdoptionSteps.ThenInvalidated; AnalysisInputReaderUnitTests.ChangedInput_WhenReadAgain_ShouldInvalidateContentIdentityAndRetainOriginalSnapshot; AnalysisInputReaderUnitTests.ChangedClaim_WhenReadAgain_ShouldUseNewContentsAndRetainTheOriginalCatalog                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 | Covered |
| B-019    | Private package installation     | CodeAnalysisPackageSteps.ThenPackageAssetsAreAvailable; CodeAnalysisPackageSteps.ThenNoApplicationRuntimeDependency; CodeAnalysisPackageIntegrationTests.DeploymentProject_WhenPacked_ShouldCarryCompilerAssetsAndNoRuntimeDependencies                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  | Covered |
| B-020    | Aligned package release version  | CodeAnalysisPackageSteps.ThenPackageVersionsMatch; CodeAnalysisPackageSteps.ThenBothPackagesAreAttached; CodeAnalysisReleaseIntegrationTests.ReleaseWorkflow_WhenPackagesArePresent_ShouldPushAndAttachBothAndFailClosedIfOneIsMissing; CodeAnalysisPackageIntegrationTests.DeploymentProject_WhenPacked_ShouldCarryCompilerAssetsAndNoRuntimeDependencies                                                                                                                                                                                                                                                                                                                                                                                                                                                               | Covered |
| B-021    | Contract-based compatibility     | CodeAnalysisPackageSteps.ThenPackageVersionEqualityIsNotRequired; AnalysisInputReaderUnitTests.ConsumerSchemaVersion_WhenSupplied_ShouldPreserveItWithoutRequiringASpechtPin; CodeAnalysisSectionUnitTests.AnalysisSection_WhenAddedToAnExistingPin_ShouldPreserveThePinAndCheckerSettings                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               | Covered |
| B-022    | Supplied analysis policy         | AdoptionSteps.ThenMappedPolicy; AnalysisConfigurationReaderUnitTests.ExplicitMappings_WhenProjectingDifferentConsumerKeys_ShouldMapArraysAndLeaveTheOriginalUntouched; AnalysisInputReaderUnitTests.ManifestSectionMappings_WhenSupplied_ShouldReadExistingMetadataWithoutChangingItsSchemaVersion; AnalysisConfigurationReaderUnitTests.LiteralArrayIndexAndEmptySelection_WhenMapped_ShouldHonorJsonPointersAndPreserveExplicitEmptyArrays                                                                                                                                                                                                                                                                                                                                                                             | Covered |
| B-023    | Configuration responsibility     | CodeAnalysisPackageSteps.ThenMsBuildSuppliesInputs; CodeAnalysisPackageSteps.ThenSeverityAndSuppressionAreStandard; CodeAnalysisPackageIntegrationTests.DeploymentProject_WhenPacked_ShouldCarryCompilerAssetsAndNoRuntimeDependencies; AnalysisInputAnalyzerUnitTests.MissingInputs_WhenAnalyzed_ShouldReportAnErrorWithoutCompilerErrors                                                                                                                                                                                                                                                                                                                                                                                                                                                                               | Covered |
| B-024    | Consumer schema ownership        | AdoptionSteps.ThenConsumerSchema; AdoptionSteps.ThenNoUpgrade; AnalysisInputReaderUnitTests.ConsumerSchemaVersion_WhenSupplied_ShouldPreserveItWithoutRequiringASpechtPin; CodeAnalysisSectionUnitTests.AnalysisSection_WhenAddedToAnExistingPin_ShouldPreserveThePinAndCheckerSettings                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  | Covered |
| B-025    | Package-delivered test evidence  | CodeAnalysisPackageSteps.ThenEvidenceMethodsAreRecognisedAsTests; CodeAnalysisPackageIntegrationTests.DeploymentProject_WhenPacked_ShouldCarryCompilerAssetsAndNoRuntimeDependencies; DocumentationBindingAnalyzerUnitTests.RunnerSelectedMethods_WhenAnalyzed_ShouldSeparateExplicitValidationFromRequiredAuthoring                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     | Covered |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-23 -->

2026-10-23: B-025 and its scenario require evidence declared through the
`SpechtTestEvidence` item to reach the analyzer's test-method recognition. The
package and the analyzer named the evidence input differently, and no claim
covered the route between them. [Lesson 0001](lessons/0001-package-evidence-reaches-the-analyzer.md)
records the finding.

2026-10-20: B-020 and its scenario now require the release pipeline to publish
and attach both packages, closing the gap between the pack test and a release.

2026-10-18: amended B-022 and B-024 and their scenarios following the owner's
correction: consume the consumer repository's supplied schemas and schema version,
including bring-your-own schemas. Analyzer adoption neither introduces a Specht
schema version nor requires a consumer upgrade. Decision 0005 records the reversal.
OQ-1 records explicit mappings over existing consumer configuration as the policy input.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-18 -->

| ID   | Question                                                                                   | Blocks                    | Resolution                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            |
| ---- | ------------------------------------------------------------------------------------------ | ------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | Which deployment identity, packages, input transport and configuration surface ship first? | None for policy transport | RESOLVED, 2026-10-17: decision 0005 selects private Specht.CodeAnalysis installation, aligned releases and manifest-owned policy. REOPENED IN PART, 2026-10-18: the owner requires consumer-supplied schemas and their versions, including bring-your-own schemas, with no imposed schema adoption, upgrade or pin change. Installation, aligned releases, MSBuild input delivery and Roslyn reporting remain settled. RESOLVED, 2026-10-18: the owner selected existing consumer configuration through explicit mappings. The owner subsequently confirmed a new codeAnalysis manifest section, without a schema 0.3.0 bump. Build integration supplies configuration and explicit mappings; consumer schema metadata retains its existing meanings. |
| OQ-2 | What severities, exclusions and suppressions allow gradual adoption?                       | None for policy           | RESOLVED, 2026-10-17: decisions 0001 through 0003 settle reporting and coverage. Decision 0005 retains MSBuild root/input delivery and standard Roslyn severity/suppression. Its 2026-10-18 reversal retains the new analysis section while removing a schema bump or consumer pin change; OQ-1 records the owner-selected explicit mappings over existing consumer configuration.                                                                                                                                                                                                                                                                                                                                                                    |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-17 -->

| Section | Status | Reviewer | Note                                                                                                              |
| ------- | ------ | -------- | ----------------------------------------------------------------------------------------------------------------- |
| 1-12    | 🟡     | Pending  | Owner selected the model; this decomposed Feature has not received detailed agreement or implementation sign-off. |

## Tasks

- [0242: Implement analysis configuration and consumer input policy](../.issue/0242-analysis-policy-inputs.yml)
- [0243: Package CodeAnalysis and deliver editor and build inputs](../.issue/0243-codeanalysis-package-delivery.yml)
- [0250: Publish Specht.CodeAnalysis with the repository release](../.issue/0250-publish-codeanalysis-package.yml)
- [0251: Adopt Specht.CodeAnalysis in the engine and CLI](../.issue/0251-analyze-engine-cli.yml)

## Scoring

| Field | Value | Basis           |
| ----- | ----- | --------------- |
| value | 0     | Not yet scored. |
| risk  | 0     | Not yet scored. |
