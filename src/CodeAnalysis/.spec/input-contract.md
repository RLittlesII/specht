---
title: "0201-F6: Analysis configuration and input contract"
description: "Concrete manifest, build-input and consumer-record metadata contracts for implementation"
type: design
---

# Analysis configuration and input contract

This is the planned contract, not a claim that the package or schema is built.
It implements decisions 0001 through 0005. The shipping schema task selects the
next unused schema minor after the current catalog; it must not modify an already
published version. Old pins retain their manifests, schemas and SPEC vocabulary.

## Policy in the manifest

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

## Actual consumer record data

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

## Build delivery

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

## Implementation boundary

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
