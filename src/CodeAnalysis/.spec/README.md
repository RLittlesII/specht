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

This is the planned contract, not a claim that the package or schema is built.
It implements decisions 0001 through 0005. The shipping schema task selects the
next unused schema minor after the current catalog; it must not modify an already
published version. Old pins retain their manifests, schemas and SPEC vocabulary.

### Policy in the manifest

`codeAnalysis` is a closed object. Unknown keys or invalid values are input
failures. Presence of the package in a selected project activates input validation;
installing it with a pin that predates this section reports incompatible input.
The standalone checker remains independently usable without the package.

| Field            | Type and meaning                                                                                                                                                                                                                                                                          |
| ---------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `projects`       | Required array of `{ path, targetFrameworks }`; root-relative project path and nonempty distinct TFM strings. This is the declared aggregation set.                                                                                                                                       |
| `features`       | Required array of `{ feature, epic? }`; explicit selection, including an explicitly empty selection for ADR-only analysis. No inference from referenced methods.                                                                                                                          |
| `requiredLinks`  | Optional `{ test: true, implementation: false, scenario: false }` defaults; test is always required for non-exempt selected current claims. Implementation/scenario may be enabled.                                                                                                       |
| `enforcement`    | Optional array of `{ project, include, exclude, role, declarationKinds }`. Project must be declared; root-relative source globs select required authoring. Roles are `verifies`, `implements`, `follows`; kinds follow F2's declaration matrix. Default is no mandatory-authoring scopes. |
| `exemptions`     | Optional array of `{ target, links, reason }`. Target is a claim identity or `{ kind: "declaration", project, symbolId }`; links name the requirements exempted. Nonempty reason required. No wildcard or implicit exemptions.                                                            |
| `records`        | Optional record-reading settings below; Feature discovery continues to use existing `layouts`, `exclusions`, `roles`, `tables`, `markers` and identifier settings.                                                                                                                        |
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

```json
{
  "codeAnalysis": {
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

### Actual consumer record data

Build discovery enumerates actual files using the configured layouts and ADR
profiles. It does not invent records from schemas. The analyzer receives and
parses each supplied record's text and resolves its declared values.

- Feature identity comes from existing frontmatter roles (`id`, optional `epic`);
  Feature lifecycle uses `spec_status`. Claims come from the manifest's `claims`
  section role and its ID column, preserving the original declaration line.
- `records.claimStatusColumn` defaults to `Status`. Active/Amended and historical
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
- The new manifest version defines `identifiers.adr` alongside the existing ID
  grammars; it is not a separate analyzer regex. Unknown lifecycle values,
  conflicting records or invalid selected metadata are explicit input findings.

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

| ID   | Question                                                                                   | Blocks                                   | Resolution                                                                                                                                                                                                                                                                                                                                                                              |
| ---- | ------------------------------------------------------------------------------------------ | ---------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | Which deployment identity, packages, input transport and configuration surface ship first? | None for deployment/configuration policy | RESOLVED, 2026-10-17: decision 0005 selects private Specht.CodeAnalysis installation, aligned repository release versions, contract-based compatibility and versioned codeAnalysis policy in the manifest. MSBuild owns root/input delivery; Roslyn controls reporting. Decision 0004 supplies actual consumer record data through AdditionalFiles. Exact contracts are defined in § 7. |
| OQ-2 | What severities, exclusions and suppressions allow gradual adoption?                       | None for policy                          | RESOLVED, 2026-10-17: decisions 0001 through 0003 settle reporting and coverage; decision 0005 assigns policy to codeAnalysis in the manifest, root/input delivery to MSBuild and severity/suppression to standard Roslyn configuration.                                                                                                                                                |

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
