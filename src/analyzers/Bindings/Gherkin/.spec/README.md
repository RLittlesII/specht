---
title: "Specification: Native Gherkin claim catalogue"
description: "Read a consumer's claims from tagged Gherkin scenarios so code analysis needs no Markdown claims table."
type: feature
id: "F7"
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
depends_on: ["F1", "F3", "F6"]
blocks: []
spikes: []
created: "2026-10-23"
updated: "2026-10-25"
github_issue: null
synced_at: null
---

# Specification: Native Gherkin claim catalogue

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-23 -->

A consumer whose claims are authored only as tagged Gherkin scenarios cannot use code analysis today: the analyzer builds a Feature's claims from a Markdown claims table, so the consumer must either duplicate every claim into a parallel table or leave `implements` and `verifies` relationships unchecked. This Feature removes that failure state by reading the claim catalogue directly from the consumer's `.feature` files, with the same identities, diagnostics and enforcement a Markdown-declared claim receives.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-23 -->

| #   | Persona                          | Need                                                                                                | Pain Point Today                                                                                   |
| --- | -------------------------------- | --------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------- |
| 1   | Consumer maintainer              | Enforce claim relationships in production code and tests while Gherkin stays the only claim source. | Analysis reports an input failure unless every claim is copied into a Markdown table.              |
| 2   | Consumer maintainer              | Keep a central specification tree shared by several projects, with no epic and no co-located specs. | Claim discovery assumes the claims table of a Feature record found through the configured layouts. |
| 3   | 0201-F4 Optional scenario bridge | Resolve a scenario's claim tag against claims declared in Gherkin.                                  | N/A (internal dependency)                                                                          |

### Assumptions

| ID  | Assumption                                                                                                                                                         |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| A-1 | The package already delivers the consumer's `.feature` files to the analyzer as scenario inputs (0201-F6 § 7); this Feature parses them and adds no new transport. |
| A-2 | A Feature's identity and lifecycle may still be carried by a metadata record that contains no claims table.                                                        |
| A-3 | Diagnostics this Feature introduces take `SPCT####` ids, which follow analyzer package releases and not schema versions (epic 0201 OQ-9; 0201-F2 decision 0009).   |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-23 -->

| ID    | Claim                                                                                                                                                                                                                          | Source                                                                                                              | Status    |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------- | --------- |
| B-001 | Given a manifest setting `codeAnalysis.records.claimSource` to `gherkin`, this Feature builds the claim catalogue from the configured `.feature` files instead of a Markdown claims table.                                     | Consumer request #256; OQ-1                                                                                         | Active    |
| B-002 | Given a manifest with no `codeAnalysis.records.claimSource`, this Feature leaves the Markdown claims-table catalogue unchanged.                                                                                                | Consumer request #256; OQ-1                                                                                         | Active    |
| B-003 | Given a `.feature` file matched by a claim file set glob mapped to a Feature identity, this Feature assigns every claim declared in that file to that Feature identity.                                                        | Consumer request #256; OQ-2                                                                                         | Active    |
| B-004 | Given a tag that matches the consumer's claim tag form and whose captured text satisfies the claim identifier grammar, this Feature records a claim whose identifier is that captured text exactly as authored.                | Consumer request #256; decision 0002                                                                                | Amended   |
| B-005 | Given native Gherkin claim input and a Feature metadata record with no claims table, this Feature reports no missing claims-section input failure.                                                                             | Consumer request #256                                                                                               | Active    |
| B-006 | Given claim files in a central specification tree outside every project directory, this Feature resolves the same claims for each selected project.                                                                            | Consumer request #256                                                                                               | Active    |
| B-007 | Given a consumer identifier grammar that declares no epic, this Feature resolves native claims by Feature identity alone.                                                                                                      | Consumer request #256                                                                                               | Active    |
| B-008 | Given a claim tag on a Scenario, this Feature records one claim carrying the element kind, the repository-relative path and the one-based line of that Scenario.                                                               | Consumer request #256                                                                                               | Active    |
| B-009 | Given a claim tag on a Scenario Outline with several example rows, this Feature records exactly one claim located at that Scenario Outline.                                                                                    | Consumer request #256                                                                                               | Active    |
| B-010 | Given a claim tag on a Rule, this Feature records one claim located at that Rule and does not treat the Rule's scenarios inheriting the tag as further declarations.                                                           | Consumer request #256; OQ-3                                                                                         | Active    |
| B-011 | Given a claim file whose scenarios are reordered or retitled, this Feature keeps each claim's identity unchanged.                                                                                                              | Consumer request #256                                                                                               | Active    |
| B-012 | Given one claim identifier declared on two elements within one Feature identity, this Feature reports a duplicate-claim diagnostic at each declaration's path and line.                                                        | Consumer request #256; OQ-4                                                                                         | Active    |
| B-013 | Given a tag whose text after `@` begins with the configured candidate prefix but fails the claim identifier grammar, this Feature reports a malformed-claim diagnostic at that tag's path and line and records no claim.       | Withdrawn 2026-10-23: there is no candidate prefix, so a tag failing the grammar is ordinary (B-025; decision 0002) | Withdrawn |
| B-014 | Given a configured claim file that is missing or is not valid Gherkin, this Feature reports an input failure naming that file rather than treating its Feature as having no claims.                                            | Consumer request #256                                                                                               | Active    |
| B-015 | Given a Scenario or Scenario Outline in a configured claim file that carries no claim tag of its own or by inheritance, this Feature reports a missing-claim diagnostic at its line with default severity warning.             | Consumer request #256; OQ-5                                                                                         | Active    |
| B-016 | Given a scenario or Examples row carrying a configured planned or ignored tag itself or through its Examples block, this Feature marks that case planned and never counts it as implemented or passing evidence for its claim. | Consumer request #256; OQ-7                                                                                         | Amended   |
| B-017 | Given native claims and a mandatory enforcement scope missing a required `implements` or `verifies` relationship, this Feature reports the same diagnostic a Markdown-declared claim receives.                                 | Consumer request #256                                                                                               | Active    |
| B-018 | Given an `implements` or `verifies` relationship naming an identifier absent from the native catalogue, this Feature reports the same target-resolution failure a Markdown-declared claim receives.                            | Consumer request #256                                                                                               | Active    |
| B-019 | Given native Gherkin claim input, this Feature leaves the consumer's `.feature` files and metadata records unchanged and writes no claims table or other adapter.                                                              | Consumer request #256                                                                                               | Active    |
| B-020 | Given a native claim carrying no lifecycle tag, this Feature treats the claim as Active.                                                                                                                                       | Consumer request #256; OQ-6                                                                                         | Active    |
| B-021 | Given a native claim carrying a tag the consumer configures as withdrawn or superseded, this Feature gives the claim that lifecycle state.                                                                                     | Consumer request #256; OQ-6                                                                                         | Active    |
| B-022 | Given a configured Feature metadata record, this Feature takes the Feature lifecycle from that record, and treats the Feature as current when none is configured.                                                              | Consumer request #256; OQ-6                                                                                         | Active    |
| B-023 | Given a claim file whose Feature-level tag names a Feature identity other than the one its claim file set maps, this Feature reports an input failure naming that file.                                                        | Consumer request #256; OQ-2                                                                                         | Active    |
| B-024 | Given a scenario carrying a configured planned or ignored tag, this Feature still declares its claim, so required-link enforcement on that claim is unchanged.                                                                 | Consumer request #256; OQ-7                                                                                         | Active    |
| B-025 | Given a tag that does not match the consumer's claim tag form, or whose captured text fails the claim identifier grammar, this Feature treats it as an ordinary tag that declares no claim and draws no diagnostic.            | Consumer request #256; decision 0002                                                                                | Amended   |
| B-026 | Given a Scenario Outline whose one Examples block carries a configured planned or ignored tag, this Feature marks only that block's rows planned and keeps the claim and the other rows' requirement unchanged.                | Consumer request #256; OQ-7                                                                                         | Active    |
| B-027 | Given native Gherkin claim input with no candidate prefix configured, this Feature reports an input failure naming the manifest.                                                                                               | Withdrawn 2026-10-23: no candidate prefix is configured, so none can be missing (decision 0002)                     | Withdrawn |
| B-028 | Given a tag matching the claim tag form and the claim identifier grammar on an Examples block, this Feature reports a malformed-claim diagnostic at that tag's path and line instead of recording a claim.                     | Consumer request #256; decision 0002                                                                                | Active    |
| B-029 | Given a configured claim file that declares a Gherkin language other than English, this Feature reports an input failure naming that file.                                                                                     | ADR-0001 (Proposed)                                                                                                 | Active    |
| B-030 | Given native Gherkin claim input, this Feature exports the native claim catalogue in the test-evidence artifact the analyzer writes.                                                                                           | Decision 0003                                                                                                       | Active    |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-23 -->

| ID  | Constraint                                                                                                                                                                                                                                                | Rules Out                                                                                                                             |
| --- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------- |
| C-1 | A tag declares a claim exactly when it matches the consumer's `claim.tag` form, honouring `claim.tagIgnoreCase`, and its captured text matches `identifiers.claim`, as the engine reads companion tags (0001-F5 decision 0013; 0201-F6 C-3; 0201-F4 C-1). | A candidate prefix or other claim-tag key, a second claim grammar, an analyzer regex setting or a competing claim-tag interpretation. |
| C-2 | Native claims enter the same catalogue records that binding resolution, policy and reconciliation already consume (0201-F1).                                                                                                                              | A parallel resolution, enforcement or reporting path for Gherkin-declared claims.                                                     |
| C-3 | A claim's identity is its Feature identity and claim identifier; its source location is provenance only.                                                                                                                                                  | Identity derived from line numbers, scenario titles, step text, example values or file names.                                         |
| C-4 | Analysis reads only supplied inputs, offline, and reports repository-relative paths (epic 0201 § Constraints).                                                                                                                                            | Ambient file discovery, network access or absolute paths in diagnostics.                                                              |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-23 -->

| #   | Item                                                                                | Exclusion Reason                                                                                                                                                                                                 |
| --- | ----------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | Generating Markdown claim tables or any other adapter from Gherkin.                 | Gherkin stays the sole authored claim source; a generated copy is the duplication this Feature removes.                                                                                                          |
| 2   | Changing the consumer's authored specification format or test runner.               | The consumer request excludes it.                                                                                                                                                                                |
| 3   | Correlating scenarios and Scenario Outline rows with discovered test cases.         | 0201-F4 owns the scenario bridge; this Feature supplies the claims its tags resolve against.                                                                                                                     |
| 4   | Collecting runner discovery and execution evidence, including xUnit v2 and VSTest.  | 0201-F3 owns runner evidence.                                                                                                                                                                                    |
| 5   | Required-link policy, enforcement scopes, exemptions and diagnostic severities.     | 0201-F6 owns policy; this Feature only supplies the claims it enforces.                                                                                                                                          |
| 6   | The passing-outcome gate and the reconciled report.                                 | 0201-F5 owns reconciliation.                                                                                                                                                                                     |
| 7   | Teaching the standalone `specht` checker to read claims from Gherkin.               | Epic 0001 owns the checker; a new `SPEC###` rule is a new schema version (`0001-F7` C-11).                                                                                                                       |
| 8   | Inferring a claim from a scenario title, step text or file name.                    | Only an explicit claim tag declares a claim.                                                                                                                                                                     |
| 9   | The input kind of `SpechtTestEvidence` files.                                       | Item 0257 owns it.                                                                                                                                                                                               |
| 10  | Claim files declaring a `# language:` other than English.                           | Only the English keywords Feature, Rule, Background, Scenario, Scenario Outline, Scenario Template, Example, Examples and Scenarios are read; another language is an input failure (B-029; ADR-0001 (Proposed)). |
| 11  | Parsing `.feature` files in `specht evidence`.                                      | The tool reads the native claim catalogue from the test-evidence artifact (B-030; decision 0003); 0201-F5 owns that reading.                                                                                     |
| 12  | Reporting a mistyped claim tag, such as `@REQ-FIX-1` against a three-digit grammar. | A tag that fails the claim identifier grammar is ordinary (B-025); the owner accepted that cost (decision 0002).                                                                                                 |

## 6. Concern Separation

<!-- last written by: implementer, 2026-10-23 -->

| Concern                                         | Classification | Responsibility                                                                                                                                                                                         |
| ----------------------------------------------- | -------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Claim identity and tag encoding                 | Business       | The consumer's existing `claim.tag`, `claim.tagIgnoreCase` and `identifiers.claim` decide what a tag means (C-1, decision 0002). The analyzer reads them through a mapping and adds no key of its own. |
| Claim file to Feature mapping and lifecycle     | Business       | A claim file set maps files to a Feature identity and may name a record that supplies lifecycle. Nothing is inferred from file names, titles or steps (C-3).                                           |
| Planned, ignored and lifecycle tags             | Business       | Consumer-configured tag lists. Planned state belongs to a case; lifecycle belongs to a claim.                                                                                                          |
| Gherkin reading and element location            | Technical      | A narrow line reader in the analyzer yields elements, tags, rows and lines for English keywords only (ADR-0001, proposed). No Gherkin package; no I/O.                                                 |
| One reading of a claim tag                      | Technical      | The engine's `FeatureTagParser` and the analyzer's `GherkinTagForm` implement one rule; a shared characterization case list holds them to one answer.                                                  |
| Catalogue assembly, input failures and findings | Both           | The analyzer builds the same `BindingFeature` and `BindingClaim` records Markdown claims produce (C-2). A failure is SPCT0001 with a stated cause; a finding is SPCT0010 to SPCT0012.                  |
| Catalogue export                                | Technical      | The analyzer writes the native catalogue into the test-evidence artifact (decision 0003); `specht evidence` reads it there and parses no Gherkin (0201-F5 § 7).                                        |
| Diagnostic identity and severity                | Technical      | Append-only `SPCT####` ids, defaulted under 0201-F6 decision 0001 and changed only through standard Roslyn severity controls.                                                                          |

## 7. Technical Design

<!-- last written by: implementer, 2026-10-23 -->

Records: [decision 0002](decisions/0002-a-claim-tag-is-the-existing-claim-tag-form.md)
(what a claim tag is), [decision 0003](decisions/0003-the-analyzer-exports-the-native-claim-catalogue.md)
(the analyzer exports the catalogue) and
[ADR-0001](adr/0001-native-claim-source-contract.md) (the keys, the parser, the
element rules, the diagnostic identities and the export), which is proposed and
waits on the owner. [Decision 0001](decisions/0001-claim-candidates-carry-the-encoding-prefix.md)
is superseded. The required-case scope key and its grammar are in
[0201-F6 § 7](../../../../codeanalysis/.spec/README.md). No new architecture is
adopted (ADR-0014) and no project is added (ADR-0017): every type below is an
instance class or record in an existing assembly, the catalogue records are the
existing ones, and nothing is static. The engine does not change.

### Where the code lives

| Project, folder and namespace                                         | Holds                                                                                                                                                                                                                                                                       |
| --------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `src/analyzers/Bindings/Gherkin/`, `Specht.Analysis.Bindings.Gherkin` | New: `GherkinTagForm`, `GherkinClaimPolicy`, `GherkinClaimFileSet`, `GherkinClaimPolicyReader`, `GherkinClaimFileReader`, `GherkinClaimFile`, `GherkinCatalogReader`, `GherkinInputException`, `GherkinDiagnostics`, `GherkinClaimAnalyzer`.                                |
| `src/analyzers/Bindings/`, `Specht.Analysis.Bindings`                 | `BindingClaim` gains optional trailing `RelativePath`, `Element` and `Cases`; new `BindingClaimElement`, `BindingClaimCase` and `BindingFinding`; `BindingCatalog` gains `Findings`; `BindingResolver` takes a claim's own provenance. `BindingFeatureReader` is unchanged. |
| `src/analyzers/Policy/`, `Specht.Analysis.Policy`                     | `AnalysisPolicyReader` reads the mapped `claim` object and calls `GherkinClaimPolicyReader`; `AnalysisPolicy.Gherkin` holds the result; `AnalysisInputReader` reads claim files under it and reports a `GherkinInputException`'s path and message.                          |
| `src/analyzers/TestEvidence/`, `Specht.Analysis.TestEvidence`         | New: `TestEvidenceCatalogFeature`, `TestEvidenceCatalogClaim`, `TestEvidenceCatalogCase`, `TestEvidenceCatalogExporter`. `TestEvidenceCompilationExporter`, `TestEvidenceArtifact`, `TestEvidenceArtifactWriter` and `TestEvidenceArtifactReader` carry `catalog`.          |
| `src/testevidence.mtp/TestEvidenceMtpCollector.cs`                    | Passes the compiler payload's `catalog` into the artifact it writes.                                                                                                                                                                                                        |
| `src/tool/Features/Evidence/`, `Specht.Tool.Features.Evidence`        | The tool-owned `Contract/` copies of the three catalog records and the reader change; `EvidencePolicy` reads `records.claimSource`; `EvidenceReconciler` takes native claims from `catalog` (0201-F5 § 7).                                                                  |

A `BindingClaim` read from a Markdown table leaves the three new members `null`,
so the Markdown path's records, diagnostics and artifacts are unchanged (B-002).
The analyzer's dependency closure gains no assembly, so its closure test is
unchanged.

### One reading of a claim tag (C-1)

`GherkinTagForm(form, placeholder, ignoreCase, grammar)` holds a literal `form`
with exactly one `{placeholder}`, and throws `GherkinInputException` otherwise.
`Read(tag)` returns the captured text, or `null`:

| Condition, tested in this order                                         | `Read` returns |
| ----------------------------------------------------------------------- | -------------- |
| `tag` is shorter than the form's literal prefix and suffix together     | `null`         |
| `tag` does not begin with the prefix or does not end with the suffix    | `null`         |
| `grammar` is set and the captured text does not match `\A(?:grammar)\z` | `null`         |
| otherwise                                                               | captured text  |

Prefix, suffix and grammar are compared ordinally and culture-invariant, and
case-insensitively exactly when `ignoreCase` is true. A captured text of length
zero is tested against the grammar like any other. The claim tag is
`new GherkinTagForm(claim.tag, "claim", claim.tagIgnoreCase, identifiers["claim"])`;
its captured text is the claim identifier, exactly as authored (B-004). A tag for
which it returns `null` is ordinary (B-025).

This is the token rule of `FeatureTagParser.Parse` given a `SpecStructure` that
carries a claim tag form, written a second time. The owner withdrew the shared
library and an MSBuild link is ruled out, so the rule has two
implementations and one test holds them together: a characterization case list
in `test/analysis.tests/`, which references both the engine and the analyzer,
runs every case through `FeatureTagParser.Parse` (one token on one line) and
through `GherkinTagForm.Read`, and asserts the same answer from both. Each case
is a form, a case flag, a grammar, a token and the expected identifier or none.
The list covers a literal prefix, a literal suffix, the form `{claim}` alone, a
case flag folding both literal and identifier, a grammar mismatch, a token equal
to the literal text, and an empty capture. That is the characterization ADR-0017
asks for, and the cost ADR-0001 records: a rule change with no new case is not
caught.

### Reading Gherkin

`GherkinClaimFileReader(GherkinClaimPolicy policy)` has one method,
`Read(relativePath, text)`, returning a `GherkinClaimFile`. It reads the text
line by line, one-based, with no Gherkin package (ADR-0001), and throws
`GherkinInputException(relativePath, message)` for the input failures below. A
line's trimmed text is classified in this order:

| Trimmed line                              | Meaning                                                                                                                             |
| ----------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------- |
| inside a doc string                       | Content, up to the line that begins with the opening delimiter. Never a tag, keyword or row.                                        |
| begins with `"""` or three backticks      | Opens a doc string with that delimiter.                                                                                             |
| empty                                     | Nothing.                                                                                                                            |
| begins with `#`                           | A comment. Before `Feature:`, `# language: <code>` (spaces optional around `:`) declares the language; only `en` reads (B-029).     |
| begins with `@`                           | A tag line: whitespace-separated tokens up to one that begins with `#`. Each token is a tag and must be `@` and one character more. |
| begins with `\|`                          | A table row. Under an `Examples` block the first is the header and each later one is a case; elsewhere it belongs to a step.        |
| `Feature:`                                | The Feature.                                                                                                                        |
| `Rule:`                                   | A Rule; it ends the previous Rule.                                                                                                  |
| `Background:`                             | A Background.                                                                                                                       |
| `Scenario Outline:`, `Scenario Template:` | A Scenario Outline.                                                                                                                 |
| `Scenario:`, `Example:`                   | A Scenario.                                                                                                                         |
| `Examples:`, `Scenarios:`                 | An `Examples` block of the current Scenario or Outline.                                                                             |
| anything else                             | A step or description text; read for nothing.                                                                                       |

Keywords are English, matched ordinally with their colon. A Scenario with an
`Examples` block is an Outline, whichever keyword opened it. Tags collected on
tag lines belong to the next Feature, Rule, Scenario, Outline or `Examples` line.

`GherkinClaimFile` holds:

| Member             | Meaning                                                                                                                       |
| ------------------ | ----------------------------------------------------------------------------------------------------------------------------- |
| `RelativePath`     | The `relativePath` argument.                                                                                                  |
| `Claims`           | One `BindingClaim(Id, State, Line, RelativePath: path, Element, Cases)` per declaration, in document order, then tag order.   |
| `Findings`         | `BindingFinding(DiagnosticId, RelativePath, Line, Text)` for SPCT0011 and SPCT0012.                                           |
| `FeatureTagValues` | The text each Feature-level tag captures through the Feature scope `GherkinTagForm`, when `scenarioScopes.featureTag` is set. |
| `EpicTagValues`    | The same for `scenarioScopes.epicTag`.                                                                                        |

Rules, in the order the reader applies them:

1. **Declaring elements.** A Feature, a Rule, a Scenario and a Scenario Outline
   declare the claims their own tags carry. `Element` is the `BindingClaimElement`
   `Feature`, `Rule`, `Scenario` or `ScenarioOutline`. `Line` is the line of the
   element's keyword, never the tag line (B-008 to B-010).
2. **Tags.** Each tag goes through the claim `GherkinTagForm`. A captured
   identifier declares `Id`, once per element however often the tag repeats. A
   `null` does nothing and draws nothing (B-025). A tag on a Rule that a scenario
   inherits is not a further declaration (B-010).
3. **Placement.** A tag on an `Examples` block that reads as a claim adds an
   SPCT0011 finding at the tag's line with the text
   `Tag <tag> declares a claim where no claim can be declared.` and declares
   nothing (B-028). A Background carries no tags.
4. **State.** `Superseded` when a declaring element's own tags include a
   superseded tag, else `Withdrawn` for a withdrawn tag, else `Active` (B-020,
   B-021). Inherited tags never change a claim's state.
5. **Cases.** A case is one Scenario (`BindingClaimCase.Line` is its keyword line)
   or one row of an Outline's `Examples` table body (the row's line); an Outline
   with no body rows has no case. A claim's cases are the cases of every Scenario
   and Outline at or below its declaring element, in document order, so a Rule's
   claim holds all of its scenarios' cases.
6. **Planned.** `BindingClaimCase.Planned` is true when any tag of the Feature, of
   the enclosing Rule, of the Scenario or Outline, or of the row's own `Examples`
   block is in the planned list (B-016, B-026). A planned case changes neither
   the claim nor its other cases (B-024).
7. **Missing claim.** A Scenario or Outline that no declaration covers, by its own
   tags, its Rule's or the Feature's, adds an SPCT0012 finding at its keyword line
   with the text `<Scenario|Scenario Outline> "<title>" declares no claim.`
   (B-015).

The reader rejects a file as not valid Gherkin (B-014) for these causes only; it
is not otherwise a Gherkin validator (ADR-0001):

- no `Feature:` line, or more than one;
- a line before `Feature:` that is not empty, a comment or a tag line;
- tags followed by a Background, a step, a table row, description text or the
  end of the file;
- a tag token that is not `@` and at least one character more;
- an `Examples:` line with no Scenario or Outline open above it in its Rule or
  Feature;
- an `Examples` body row whose cell count differs from its header;
- a doc string still open at the end of the file.

### Manifest contract

The record keys sit under `codeAnalysis` in the consumer's manifest. A key not
listed here is not read. Under no `claimSource`, every key in this table is
ignored and the Markdown path is unchanged (B-002).

| Key                                    | Type and default                                                                 | Rule                                                                                                                   |
| -------------------------------------- | -------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------- |
| `records.claimSource`                  | string; absent selects the Markdown claims table                                 | The only value is `"gherkin"`; any other value is an input failure (B-001). Repository-wide.                           |
| `records.claimFileSets`                | array of `{ glob, feature, epic?, record? }`; required, nonempty under `gherkin` | See below (B-003, B-006, B-007).                                                                                       |
| `records.plannedTags`                  | array of string; default `["@ignore"]`                                           | Exact tags that mark a case planned. `[]` marks nothing.                                                               |
| `records.withdrawnTags`                | array of string; default `[]`                                                    | Exact tags that give a claim the state `Withdrawn`.                                                                    |
| `records.supersededTags`               | array of string; default `[]`                                                    | Exact tags that give a claim the state `Superseded`.                                                                   |
| `records.featureLifecycleKey`          | string; existing, default `spec_status`                                          | The frontmatter key read from a claim file set's `record`.                                                             |
| `scenarioScopes.featureTag`, `epicTag` | string; existing, optional                                                       | When set, the Feature and epic tags a claim file may carry (B-023).                                                    |
| `mappings`: `/claim`                   | pointer, normally `/claim`; required under `gherkin`                             | The consumer's existing `claim` object, mapped as `/identifiers` is. See below.                                        |
| `mappings`: `/claimsSection`           | existing; optional under `gherkin`                                               | A Gherkin-only consumer has no claims table; `BindingPolicy.ClaimsSection` is then empty and nothing reads it (B-005). |

**Why `/claim` is a mapping.** The analyzer sees the consumer's manifest only
through `codeAnalysis.mappings`: `AnalysisConfigurationReader` projects each
mapped field into the normalized analysis input without interpreting a schema
version (0201-F6 § 7, `mappings` row), and that is how it already reads
`/identifiers`, `/markers` and `/schemaVersion`. `"/claim": "/claim"` is a pointer
to where the existing `claim.tag` and `claim.tagIgnoreCase` live. It restates no
value and its target is the manifest's own name; a consumer whose `claim` object
sits elsewhere points at it there. The mapped object must hold a string `tag`
with exactly one `{claim}`; `tagIgnoreCase`, when present, must be a Boolean,
and when absent reads as `false`. The engine fills an omitted `claim` member from
the pin's default; the analyzer does not read the pin, so a consumer on native
input declares `claim.tag` explicitly. `identifiers.claim` is the grammar, read
through the existing `/identifiers` mapping.

A tag-list entry is `@` followed by one or more characters other than whitespace.
Tag lists are compared ordinally with tag text; only claim tags honour
`tagIgnoreCase`.

A **claim file set** is `{ glob, feature, epic?, record? }`:

- `glob` is root-relative with the checker's segment semantics (`AnalysisPaths`)
  and is matched against the delivered inputs of kind `scenario`.
- `feature` must match `identifiers.feature`. `epic` is required exactly when
  `identifiers.epic` is declared and must then match it; with no epic grammar
  the key is forbidden and a claim resolves by Feature alone (B-007).
- `record` is optional and is the root-relative path of a delivered `feature`
  input. Its `featureLifecycleKey` value is the Feature lifecycle, validated as the
  Markdown path validates it; its claims table, if any, is never read (B-005,
  B-022). With no `record`, the Feature state is the new value `current`, which
  `BindingResolver.Eligible` treats as eligible; the Markdown path rejects that
  value, so no existing record can carry it.
- Two sets may not name one Feature identity, and one file may not match two
  sets. Selected Features (`codeAnalysis.features`) must each be named by a set.

The delivery default for scenario inputs also covers a central
`.spec/features` tree (0201-F6 B-027), so `.spec/features/arithmetic/` is
delivered without a build property. A tree elsewhere still needs the
`SpechtScenarioInputs` build property. A set whose glob matches no delivered
input is an input failure, which is how that omission shows (B-006).

**The Arithmetic fixture**, configured as a Gherkin-only consumer with no epic:

```json
{
  "schemaVersion": "0.2.0",
  "identifiers": { "feature": "^[a-z]+$", "claim": "^REQ-FIX-[0-9]{3}$" },
  "markers": {
    "approved": "approved",
    "withdrawn": "Withdrawn",
    "superseded": "Superseded"
  },
  "claim": { "tag": "@{claim}", "tagIgnoreCase": false },
  "codeAnalysis": {
    "mappings": {
      "/schemaVersion": "/schemaVersion",
      "/identifiers": "/identifiers",
      "/markers": "/markers",
      "/claim": "/claim"
    },
    "projects": [
      {
        "path": "test/Arithmetic.Tests/Arithmetic.Tests.csproj",
        "targetFrameworks": ["net10.0"]
      }
    ],
    "features": [{ "feature": "arithmetic" }],
    "enforcement": [
      {
        "project": "test/Arithmetic.Tests/Arithmetic.Tests.csproj",
        "include": ["test/Arithmetic.Tests/**/*.cs"],
        "exclude": [],
        "role": "verifies",
        "declarationKinds": ["testMethod"]
      }
    ],
    "records": {
      "claimSource": "gherkin",
      "claimFileSets": [
        {
          "glob": ".spec/features/arithmetic/*.feature",
          "feature": "arithmetic"
        }
      ],
      "plannedTags": ["@ignore"],
      "withdrawnTags": ["@withdrawn"],
      "supersededTags": ["@superseded"]
    },
    "scenarioScopes": { "featureTag": "@spec.feature:{feature}" }
  }
}
```

With `.spec/features/arithmetic/arithmetic.feature` reading:

```gherkin
@spec.feature:arithmetic
Feature: Arithmetic

  Rule: Addition returns the sum

    @REQ-FIX-001
    Scenario Outline: Add two operands
      Given <a> and <b>
      Then the sum is <sum>

      Examples:
        | a | b | sum |
        | 1 | 2 | 3   |
        | 2 | 2 | 4   |
```

the catalogue holds one Feature, identity `arithmetic` with no epic and the state
`current`, with one claim: `REQ-FIX-001`, `ScenarioOutline`, path
`.spec/features/arithmetic/arithmetic.feature`, line 7, state `Active`, two
cases at lines 13 and 14, neither planned. Feature tag `arithmetic` agrees with
the set.

### Building the catalogue

`AnalysisPolicyReader` reads the `records` keys and the mapped `claim` object
through `GherkinClaimPolicyReader` into a `GherkinClaimPolicy` (the claim
`GherkinTagForm`, the scope forms, the tag lists and the `GherkinClaimFileSet`s),
held by `AnalysisPolicy.Gherkin` (`null` for the Markdown path). Under it,
`claimsSection` need not be mapped. `AnalysisInputReader`, when the policy has
one, replaces the loop over `feature` inputs with `GherkinCatalogReader`, which
runs in this order and stops at the first failure (the reader reports one
failure, as today):

1. the keys and the mapped `claim` object, in the order of the failure table below;
2. every delivered `scenario` input that a glob matches, in ordinal path order:
   a file matched by two sets fails; a file whose text is `null` fails; a file
   `GherkinClaimFileReader` rejects fails; a captured Feature or epic tag
   differing from its set's identity fails;
3. a set matching no delivered file fails;
4. each `record`, validated by the Feature schema as a Markdown record is;
5. each selected Feature without a set fails.

It then builds one `BindingFeature` per set: identity from the set, state from
the record or `current`, `RelativePath` the record's path or else the ordinal-first
claim file, and the `BindingClaim`s of its files. `BindingResolver` uses
`claim.RelativePath ?? feature.RelativePath` and `claim.Line` as the target's
provenance (B-008, C-2), and `BindingCatalog` validates every claim path with the
existing `BindingProvenanceGuard`. Everything after the catalogue is the existing
code: `AnalysisPolicy.Select`, `RequiredLinks` and `BindingResolver` are
unchanged, so B-017 and B-018 give the Markdown path's diagnostics (SPCT0007,
SPCT0004). Planned cases are visible through `Cases` to the scenario bridge
(0201-F4), and through the export below to reconciliation (0201-F5); this
Feature counts nothing.

A claim keeps its identity through reordering or retitling because the identity
is the Feature identity and `Id` (C-3, B-011); path and line are provenance.
Duplicates are not removed: after all files are read, an `Id` declared twice in
one Feature adds an SPCT0010 finding at each declaration (B-012), and the
catalogue keeps both, so a reference to it is ambiguous as for a Markdown
duplicate. The findings of every file and the duplicates go to
`BindingCatalog.Findings`, sorted by path (ordinal), line, id; the Markdown path
gives none. No member reads the file system.

### Export in the test-evidence artifact

Under native input the catalogue leaves the build in the artifact the analyzer
already writes, and nowhere else (decision 0003, B-030):

1. `TestEvidenceCompilationExporter` already reads the snapshot. When
   `snapshot.Policy.Gherkin` is set, `TestEvidenceCatalogExporter` projects
   `snapshot.Catalog.Features` into `TestEvidenceCatalogFeature` records, and the
   compiler payload (`CompilationEvidence`) carries them as `catalog`. Under the
   Markdown path the payload has no `catalog` member.
2. `TestEvidenceMtpCollector` reads `catalog` from the payload when present and
   passes it to `TestEvidenceArtifact.Catalog`. The payload member is an object
   with a `features` array, as in the artifact. `TestEvidenceVsTestLogger` makes
   the same pass-through for the identity artifact it writes.
3. `TestEvidenceArtifactWriter` writes a top-level `catalog` and
   `formatVersion: 2` when `Catalog` is set, and otherwise the version-1 artifact
   it writes today, byte for byte.

The member's contract is 0201-F3 § 7's; for the fixture it is:

```json
{
  "catalog": {
    "features": [
      {
        "epic": null,
        "feature": "arithmetic",
        "state": "current",
        "path": ".spec/features/arithmetic/arithmetic.feature",
        "claims": [
          {
            "id": "REQ-FIX-001",
            "state": "Active",
            "element": "scenarioOutline",
            "path": ".spec/features/arithmetic/arithmetic.feature",
            "line": 7,
            "cases": [
              { "line": 13, "planned": false },
              { "line": 14, "planned": false }
            ]
          }
        ]
      }
    ]
  }
}
```

Findings are not exported: they are build diagnostics, which reconciliation does
not re-ingest (0201-F5 § 5 row 4). The claim files are already `record` entries
of the artifact's `inputs`, so `policyFingerprint` covers them, and the tool's
hash check of `inputs` is what makes an exported catalogue current or stale. The
tool reads `catalog` with its own `Contract/` reader and never references the
analyzer (0201-F5 § 7).

### Input failures

A failure is `GherkinInputException(relativePath, message)`, caught by
`AnalysisInputReader` into the existing `AnalysisInputFailure` with its path and
message, and reported as `SPCT0001` (error, 0201-F6 B-017) with the message as its
argument. A `null` path is the manifest.

| Cause                                                           | Path            | Message                                                                   | Claim |
| --------------------------------------------------------------- | --------------- | ------------------------------------------------------------------------- | ----- |
| `claimSource` is not `gherkin`                                  | manifest        | `codeAnalysis.records.claimSource must be "gherkin".`                     |       |
| `/claim` unmapped, or `tag` absent or without one `{claim}`     | manifest        | `Native claim input requires claim.tag, mapped as /claim.`                |       |
| `tagIgnoreCase` present and not a Boolean                       | manifest        | `claim.tagIgnoreCase must be a Boolean.`                                  |       |
| a tag list holds a malformed entry                              | manifest        | `Claim tag lists must hold @-led tags without whitespace.`                |       |
| `claimFileSets` absent, empty or of the wrong shape or identity | manifest        | `codeAnalysis.records.claimFileSets is missing or invalid.`               |       |
| two sets name one Feature identity                              | manifest        | `Two claim file sets name one Feature.`                                   |       |
| a set's glob matches no delivered input                         | manifest        | `A claim file set matches no supplied scenario input.`                    |       |
| one file matched by two sets                                    | the file        | `A claim file matches more than one claim file set.`                      |       |
| claim file delivered without readable text                      | the file        | `Claim file is missing or unreadable.`                                    | B-014 |
| claim file declares a language other than `en`                  | the file        | `Claim file declares a Gherkin language other than English.`              | B-029 |
| claim file rejected by the reader for a listed cause            | the file        | `Claim file is not valid Gherkin.`                                        | B-014 |
| Feature or epic tag differs from the set                        | the file        | `Feature or epic tag names a different identity than its claim file set.` | B-023 |
| `record` is not a delivered `feature` input                     | the record path | `Feature record is not a supplied feature input.`                         |       |
| record lifecycle value unknown                                  | the record path | `A supplied Feature has an unknown lifecycle value.`                      |       |
| a selected Feature has no set                                   | manifest        | `A selected consumer Feature has no claim file set.`                      |       |

The language row is tested before the reader's other causes, so a file in
another language reports the language and not a parse failure. A Feature is
never reported as having no claims when its file failed (B-014).

### Diagnostics

`GherkinClaimAnalyzer` reads the same snapshot as `AnalysisInputAnalyzer`,
through `AnalysisInputAdapter`, and reports `BindingCatalog.Findings` when the
snapshot is available. Each diagnostic is at the supplied file's line, with the
properties `SpechtRelativePath` and `SpechtLine`, message `{0}` being the
finding's text, category `Traceability` and a help link under
`src/codeanalysis/docs/`.

| Id       | Title                                | Default severity | Reported                                                                                                | Claims |
| -------- | ------------------------------------ | ---------------- | ------------------------------------------------------------------------------------------------------- | ------ |
| SPCT0010 | Duplicate claim declaration          | Warning          | At each declaration's path and line; text `Claim <id> is declared more than once in Feature <feature>.` | B-012  |
| SPCT0011 | Claim tag where no claim is declared | Warning          | At the `Examples` block's tag line; the text of rule 3 above                                            | B-028  |
| SPCT0012 | Scenario declares no claim           | Warning          | At the Scenario or Outline keyword line; the text of rule 7                                             | B-015  |

The ids follow SPCT0009, the last published (SPCT0099 is a test fixture);
SPCT0001 to SPCT0009 are unchanged. A duplicate claim declaration is a defect
that makes references ambiguous, so it is a warning and not the information of
SPCT0009, which is a duplicate of a valid relationship. `GherkinDiagnostics` is
an append-only registry of its own, with the same guard that rejects a removed,
reassigned or revived id as `DocumentationDiagnostics`; a test unions the two so
no id collides. A page for each id under `src/codeanalysis/docs/`, the table in
its README and the SPCT0001 page's list of causes are 0259's. Severity and
suppression are the standard Roslyn controls (0201-F6 B-007, B-008); suppressing
a finding leaves the catalogue unchanged.

### What the later items follow

- **0259** builds everything above, the version-2 artifact and both readers of
  it included, and gives each claim of § 3 the assertion in the table below.
- **0245** (the scenario bridge) reads claims only through the claim
  `GherkinTagForm` and the Feature and epic scope forms, and joins a runner case
  to a catalogue case by path and line; the producer's own row identity stays
  0201-F4's.
- **0260** evaluates `CaseScope` (0201-F6 § 7, declared in the analyzer) in the
  collector, which references the analyzer, over a case's categories, written without `@`, and records the result in the
  artifact; the incomplete-run diagnosis is a typed finding in the evidence
  artifact and report (0201-F5 `findings`), not an SPCT diagnostic, because it
  exists only after a run. The case record's new fields are 0260's.
- **0262** gates on the `planned` flag of `catalog` cases and on owned keys, both
  read from the artifact. The tool takes no new reference.

| Claim | The test asserts, at `AnalysisInputReader.Read` unless stated                                                                                                                                                          |
| ----- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| B-001 | With `claimSource: "gherkin"`, the snapshot catalogue holds `REQ-FIX-001` for Feature `arithmetic` and a `feature` input's claims table is not read.                                                                   |
| B-002 | With no `claimSource` and a record with a claims table, the catalogue is the table's claims, and a present `claimFileSets` changes nothing.                                                                            |
| B-003 | A claim in a file matched by the set belongs to that set's identity.                                                                                                                                                   |
| B-004 | The claim's `Id` is `REQ-FIX-001`; with `tagIgnoreCase` true, `@req-fix-001` reads as `req-fix-001`, as authored.                                                                                                      |
| B-005 | A set whose `record` has no claims table yields no failure; so does a set with no `record`.                                                                                                                            |
| B-006 | The same inputs with two different selected project paths give equal catalogues; a glob outside every project directory resolves.                                                                                      |
| B-007 | With no `identifiers.epic`, a claim resolves from a reference with no epic, and an `epic` in a set is a failure.                                                                                                       |
| B-008 | A Scenario's claim has `Element` `Scenario`, its path, and the keyword line, not the tag line.                                                                                                                         |
| B-009 | An Outline with two rows gives one claim and two cases.                                                                                                                                                                |
| B-010 | A Rule tag gives one claim at the Rule line, its scenarios' cases, and no SPCT0010.                                                                                                                                    |
| B-011 | Reordered and retitled elements give claims equal in `Id` and Feature identity.                                                                                                                                        |
| B-012 | The same `Id` on two elements gives two SPCT0010 findings, one per path and line.                                                                                                                                      |
| B-014 | A `null` text and a text with no `Feature:` line each give one failure whose path is the file and whose message is the stated one.                                                                                     |
| B-015 | A Scenario with no claim from itself, its Rule or the Feature gives SPCT0012 at its keyword line; with a Rule claim it gives none.                                                                                     |
| B-016 | A case under a planned tag, itself or through its `Examples` block, has `Planned` true.                                                                                                                                |
| B-017 | A native claim with no `verifies` method gives the same SPCT0007 as a Markdown claim; the existing resolver and policy are used unchanged.                                                                             |
| B-018 | A `verifies` naming `REQ-FIX-999` gives the same SPCT0004 as a Markdown claim.                                                                                                                                         |
| B-019 | `Read` takes texts and returns records; no member writes. The test supplies no file system.                                                                                                                            |
| B-020 | A claim with no lifecycle tag has `State` `Active`.                                                                                                                                                                    |
| B-021 | A configured withdrawn tag gives `Withdrawn`, a superseded tag `Superseded`; both together give `Superseded`.                                                                                                          |
| B-022 | A `record` with `spec_status: draft` gives Feature state `draft`; none gives `current`; both are eligible targets.                                                                                                     |
| B-023 | `@spec.feature:billing` on a file mapped to `arithmetic` gives one failure at that file; `@spec.feature:arithmetic`, or no Feature tag, gives none.                                                                    |
| B-024 | An `@ignore` scenario still declares its claim, and the claim's required links are as for an unplanned one.                                                                                                            |
| B-025 | `@visual`, `@ignore`, `@boundary`, `@issue-42` and `@REQ-FIX-5` give no claim and no finding; under the form `@req:{claim}`, `@REQ-FIX-001` gives none.                                                                |
| B-026 | An Outline with one ignored Examples block has `Planned` true only for that block's rows; the claim and the other rows are unchanged.                                                                                  |
| B-028 | An `Examples` block tagged `@REQ-FIX-006` gives one SPCT0011 at that tag's line and no claim `REQ-FIX-006`.                                                                                                            |
| B-029 | `# language: fr` gives one failure at that file with the stated message; `# language: en`, and no header, give none.                                                                                                   |
| B-030 | At `TestEvidenceArtifactWriter` and the tool's `Contract/` reader: the fixture's artifact has `formatVersion` 2 and the `catalog` above and reads back equal; a Markdown-path artifact is version 1 with no `catalog`. |

## 8. Testing Strategy

<!-- last written by: test-writer, 2026-10-24 -->

Two tiers. Reqnroll covers acceptance only: every scenario in
`gherkin.feature` is bound by `test/acceptance/Bindings/GherkinSteps.cs`
(scope tag `0201-F7`), running the real `AnalysisInputReader`,
`DocumentationBindingAnalyzer` and `TestEvidenceCatalogExporter` on synthetic
inputs built in the steps. The mechanism beneath each claim is pinned in
`test/analysis.tests`, under `Gherkin/` and `TestEvidence/`, all
`Tier=Unit`. The feature file is linked by a literal `ReqnrollFeatureFile`
entry in `acceptance.csproj`; the discovered scenario count for tag
`0201-F7` is 33.

| Claim               | Mechanism beneath it                                                                                                                                                                           | Unit test                                                                                                                       |
| ------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------- |
| B-001, B-002        | `AnalysisInputReader` choosing the catalogue source by `records.claimSource`                                                                                                                   | `AnalysisInputReaderGherkinUnitTests` source and no-source facts                                                                |
| B-003, B-004        | `claimFileSets` to Feature mapping; `GherkinTagForm.Read`                                                                                                                                      | file-set fact, `GherkinTagFormUnitTests`, `ClaimTagParityUnitTests` over `ClaimTagCases` against `FeatureTagParser`             |
| B-005, B-006, B-007 | Feature record optional; projects do not enter the catalogue; no epic tier                                                                                                                     | record, project and no-epic facts                                                                                               |
| B-008 to B-010      | `GherkinClaimFileReader` line reader: keyword lines, Outline, Rule and Feature elements                                                                                                        | `GherkinClaimFileReaderUnitTests`, including doc strings, comments, CRLF and Examples                                           |
| B-011, B-012        | identity is Feature plus claim id; duplicate finding per declaration                                                                                                                           | reorder and duplicate facts; `GherkinClaimAnalyzerUnitTests` for the SPCT0010 warning                                           |
| B-014, B-029        | `GherkinInputException` messages mapped to `AnalysisInputFailure`                                                                                                                              | message theories over `InvalidClaimFiles` and `InvalidGherkinPolicies`                                                          |
| B-015, B-025, B-028 | SPCT0012 and SPCT0011 findings; ordinary tags ignored                                                                                                                                          | reader facts; analyzer finding theory over `GherkinFindingCases`                                                                |
| B-016, B-024, B-026 | per-case `Planned` from `plannedTags`; claim stays declared                                                                                                                                    | reader planned-case facts; `IgnoredScenario_...`                                                                                |
| B-017, B-018        | `DocumentationBindingAnalyzer` unchanged over the native catalogue                                                                                                                             | `GherkinEnforcementUnitTests` against a Markdown twin                                                                           |
| B-019               | the reader reads only the inputs it is handed and adds none                                                                                                                                    | `ClaimInputsGiven_...`                                                                                                          |
| B-020 to B-022      | lifecycle tag strength; `record` `spec_status`                                                                                                                                                 | lifecycle theory; feature lifecycle theory                                                                                      |
| B-023               | Feature-level tag compared with its file set                                                                                                                                                   | feature-tag theory                                                                                                              |
| B-030               | `TestEvidenceCatalogExporter.Project`, writer version selection, reader, the tool's `Contract/` reader, source-generator payload, `TestEvidenceMtpCollector.Write`, `TestEvidenceVsTestLogger` | exporter, writer, reader, `EvidenceWireParityUnitTests`, `TestEvidenceCompilationExporterUnitTests`, collector and logger facts |

Registry: `GherkinDiagnosticsUnitTests` pins SPCT0010 to SPCT0012 as warnings,
the union with `DocumentationDiagnostics`, and the published-history guard.

Red for the right reason: on the shells under the planned names, the reader
throws `NotImplementedException`, the input reader returns an unavailable
snapshot for a native manifest, and the catalogue projection throws. The
acceptance steps fail in the same way; none fails to compile or to bind.

Gaps reported to `implementer`:

- `TestEvidenceMtpCollector` gained a public instance method `Write(outputPath,
exportAssembly, exportJson)` as the seam for the catalogue pass-through test;
  `Export` reflects the payload and calls it. The logger needed no seam.
- Three acceptance rows pass on the shells and so cannot show red: the
  Markdown scenario (B-002) and the two B-014 rows, which already fail the
  input with the claim file path. Their unit tests fail on the exact message.
- `GherkinClaimPolicyFixture` is written by hand: the AutoFixture generator
  emits a duplicate `WithList` for a record with three
  `IReadOnlyList<string>` properties.
- `BindingCatalog` gained a second constructor rather than an optional
  parameter, because an existing integration test activates the two-argument
  constructor by reflection.
- The new documentation pages for SPCT0010 to SPCT0012 are the implementer's.

## 9. Traceability Matrix

<!-- last written by: test-writer, 2026-10-24 -->

| Claim ID | Scenario                                               | Test                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        | Status    |
| -------- | ------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------- |
| B-001    | Native claim input selected                            | `GherkinSteps.GivenNative`; `GherkinSteps.ThenHolds`; `AnalysisInputReaderGherkinUnitTests.GherkinClaimSource_WhenRead_ShouldBuildTheCatalogueFromClaimFilesAndNotReadAClaimsTable`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         | Covered   |
| B-002    | Markdown catalogue unchanged                           | `GherkinSteps.ThenTableClaims`; `AnalysisInputReaderGherkinUnitTests.NoClaimSource_WhenRead_ShouldKeepTheMarkdownCatalogueAndIgnoreClaimFileSets`; `AnalysisInputReaderGherkinUnitTests.MarkdownRecordWithTheCurrentLifecycleValue_WhenRead_ShouldFailNamingTheRecord`; `GherkinClaimAnalyzerUnitTests.MarkdownClaimInput_WhenAnalyzed_ShouldReportNothing`; `TestEvidenceCompilationExporterUnitTests.MarkdownClaimInput_WhenExported_ShouldCarryNoCatalogMember`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          | Covered   |
| B-003    | File mapped to its Feature identity                    | `GherkinSteps.ThenBelongs`; `AnalysisInputReaderGherkinUnitTests.TwoClaimFileSets_WhenRead_ShouldAssignEachFilesClaimsToItsSetsFeature`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     | Covered   |
| B-004    | Claim identifier kept as authored                      | `GherkinSteps.ThenAsTagged`; `AnalysisInputReaderGherkinUnitTests.IgnoredTagCase_WhenAClaimTagIsRead_ShouldKeepTheIdentifierAsAuthored`; `GherkinTagFormUnitTests.IgnoredCaseUnderDifferentCultures_WhenRead_ShouldFoldOrdinallyAndKeepTheAuthoredText`; `ClaimTagParityUnitTests.ClaimTagToken_WhenReadByTheEngineAndTheAnalyzer_ShouldGiveTheSameAnswerFromBoth`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          | Covered   |
| B-005    | No claims table required                               | `GherkinSteps.ThenNoFailure`; `AnalysisInputReaderGherkinUnitTests.FeatureRecordWithoutAClaimsTable_WhenRead_ShouldReportNoInputFailure`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    | Covered   |
| B-006    | Central specification tree                             | `GherkinSteps.ThenBothResolve`; `AnalysisInputReaderGherkinUnitTests.SameInputsUnderDifferentSelectedProjects_WhenRead_ShouldGiveEqualCatalogues`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           | Covered   |
| B-007    | No epic identity                                       | `GherkinSteps.ThenNoEpic`; `AnalysisInputReaderGherkinUnitTests.ConsumerDeclaringNoEpic_WhenAReferenceNamesNoEpic_ShouldResolveByFeatureAlone`; `GherkinClaimFileReaderUnitTests.PolicyWithoutScopeForms_WhenFeatureTagsAreRead_ShouldCaptureNoScopeValues`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 | Covered   |
| B-008    | Scenario claim location                                | `GherkinSteps.ThenLocated`; `GherkinClaimFileReaderUnitTests.ScenarioTaggedWithAClaim_WhenRead_ShouldLocateTheClaimAtTheScenarioKeywordLine`; `GherkinClaimFileReaderUnitTests.ClaimFileWithWindowsLineEndings_WhenRead_ShouldGiveTheSameClaimsAndLines`; `GherkinClaimFileReaderUnitTests.CommentOnATagLine_WhenRead_ShouldEndTheTagsAtTheComment`; `GherkinClaimFileReaderUnitTests.TagsAndKeywordsInsideADocString_WhenRead_ShouldBeContentOnly`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         | Covered   |
| B-009    | Scenario Outline declares one claim                    | `GherkinSteps.ThenOneClaim`; `GherkinSteps.ThenAtOutline`; `GherkinClaimFileReaderUnitTests.OutlineWithTwoExampleRows_WhenRead_ShouldDeclareOneClaimWithOneCasePerRow`; `GherkinClaimFileReaderUnitTests.OutlineWithHeaderOnlyExamples_WhenRead_ShouldHaveNoCases`; `GherkinClaimFileReaderUnitTests.ScenarioKeywordsWithAnExamplesBlock_WhenRead_ShouldDeclareAnOutline`; `GherkinClaimFileReaderUnitTests.ExampleKeywordWithoutExamples_WhenRead_ShouldDeclareAScenario`; `GherkinClaimFileReaderUnitTests.TablesOutsideExamples_WhenRead_ShouldAddNoCases`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               | Covered   |
| B-010    | Rule claim                                             | `GherkinSteps.ThenAtRule`; `GherkinSteps.ThenNoDuplicate`; `GherkinClaimFileReaderUnitTests.RuleTaggedWithAClaim_WhenRead_ShouldDeclareOneClaimAtTheRuleHoldingEveryScenarioCase`; `GherkinClaimFileReaderUnitTests.FeatureTaggedWithAClaim_WhenRead_ShouldDeclareItAtTheFeatureAndCoverItsScenarios`; `AnalysisInputReaderGherkinUnitTests.RuleClaimInheritedByItsScenarios_WhenRead_ShouldReportNoDuplicate`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              | Covered   |
| B-011    | Stable claim identity                                  | `GherkinSteps.ThenIdentity`; `AnalysisInputReaderGherkinUnitTests.ReorderedAndRetitledScenarios_WhenRead_ShouldResolveTheSameClaimIdentity`; `GherkinClaimFileReaderUnitTests.RetitledAndReorderedScenarios_WhenRead_ShouldKeepEveryClaimIdentity`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          | Covered   |
| B-012    | Duplicate claim                                        | `GherkinSteps.ThenDuplicate`; `AnalysisInputReaderGherkinUnitTests.ClaimIdentifierDeclaredOnTwoScenarios_WhenRead_ShouldReportOneDuplicateFindingPerDeclarationAndKeepBoth`; `AnalysisInputReaderGherkinUnitTests.FindingsOfSeveralKinds_WhenRead_ShouldBeOrderedByPathThenLineThenId`; `GherkinClaimFileReaderUnitTests.OneClaimTaggedTwiceOnOneElement_WhenRead_ShouldDeclareItOnce`; `GherkinClaimFileReaderUnitTests.ElementsCarryingSeveralClaims_WhenRead_ShouldListClaimsInDocumentOrderThenTagOrder`; `GherkinClaimAnalyzerUnitTests.ClaimFileFinding_WhenAnalyzed_ShouldBeReportedAsAWarningAtTheSuppliedFilesLine`; `GherkinDiagnosticsUnitTests.AgreedRegistry_WhenLoaded_ShouldHoldSpct0010ToSpct0012AsWarnings`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                | Covered   |
| B-013    | Claim candidates and ordinary tags                     | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     | Withdrawn |
| B-014    | Missing or invalid claim file                          | `GherkinSteps.ThenFailureNamesFile`; `GherkinSteps.ThenNotNoClaims`; `AnalysisInputReaderGherkinUnitTests.ClaimFileThatCannotBeRead_WhenRead_ShouldFailNamingTheFileAndReportNoCatalogue`; `AnalysisInputReaderGherkinUnitTests.ClaimFileMatchedByTwoSets_WhenRead_ShouldFailNamingTheFile`; `AnalysisInputReaderGherkinUnitTests.RecordThatIsNotADeliveredFeatureInput_WhenRead_ShouldFailNamingTheRecordPath`; `AnalysisInputReaderGherkinUnitTests.InvalidNativeClaimConfiguration_WhenRead_ShouldFailNamingTheManifestWithTheStatedMessage`; `GherkinClaimFileReaderUnitTests.ClaimFileBreakingAListedCause_WhenRead_ShouldFailNamingTheFileAsNotValidGherkin`; `GherkinTagFormUnitTests.FormWithoutExactlyOnePlaceholder_WhenConstructed_ShouldFailTheManifestWithTheClaimTagMessage`; `GherkinClaimAnalyzerUnitTests.UnavailableSnapshot_WhenAnalyzed_ShouldLeaveTheInputFailureToTheInputAnalyzer`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   | Covered   |
| B-015    | Scenario without a claim                               | `GherkinSteps.ThenMissingClaim`; `GherkinClaimFileReaderUnitTests.ScenarioWithoutAnyClaim_WhenRead_ShouldReportAMissingClaimAtItsKeywordLine`; `GherkinClaimFileReaderUnitTests.OutlineWithoutAnyClaim_WhenRead_ShouldReportAMissingClaimNamingTheOutline`; `GherkinClaimFileReaderUnitTests.ScenarioCoveredByItsRuleOrFeatureClaim_WhenRead_ShouldReportNoMissingClaim`; `GherkinClaimAnalyzerUnitTests.ClaimFileFinding_WhenAnalyzed_ShouldBeReportedAsAWarningAtTheSuppliedFilesLine`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    | Covered   |
| B-016    | Planned or ignored scenario                            | `GherkinSteps.ThenPlanned`; `AnalysisInputReaderGherkinUnitTests.PlannedTagList_WhenAbsentOrConfigured_ShouldPlanCasesAccordingly`; `GherkinClaimFileReaderUnitTests.ScenarioTaggedIgnored_WhenRead_ShouldStillDeclareItsClaimWithPlannedCases`; `GherkinClaimFileReaderUnitTests.CaseUnderAnIgnoredFeatureOrRule_WhenRead_ShouldBePlanned`; `GherkinClaimFileReaderUnitTests.EmptyPlannedTagList_WhenAnIgnoredTagIsRead_ShouldPlanNothing`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 | Covered   |
| B-017    | Required relationship missing                          | `GherkinSteps.ThenRequiredLink`; `GherkinEnforcementUnitTests.NativeClaimsWithAMissingRequiredRelationship_WhenAnalyzed_ShouldReportTheDiagnosticAMarkdownClaimReceives`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    | Covered   |
| B-018    | Unknown claim                                          | `GherkinSteps.ThenUnknown`; `GherkinEnforcementUnitTests.NativeVerifiesNamingAnAbsentClaim_WhenAnalyzed_ShouldReportTheTargetResolutionFailureAMarkdownClaimReceives`; `GherkinEnforcementUnitTests.NativeVerifiesNamingADeclaredClaim_WhenAnalyzed_ShouldReportNoResolutionFailure`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        | Covered   |
| B-019    | No adapter written                                     | `GherkinSteps.ThenUnchanged`; `GherkinSteps.ThenNoAdapter`; `AnalysisInputReaderGherkinUnitTests.ClaimInputsGiven_WhenRead_ShouldLeaveEveryInputUnchangedAndAddNone`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        | Covered   |
| B-020    | Untagged claim is Active                               | `GherkinSteps.ThenActive`; `GherkinClaimFileReaderUnitTests.ClaimWithConfiguredLifecycleTags_WhenRead_ShouldGiveTheStrongestState`; `GherkinClaimFileReaderUnitTests.LifecycleTagOnARule_WhenAScenarioInsideDeclaresItsOwnClaim_ShouldNotChangeTheScenarioClaimState`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       | Covered   |
| B-021    | Configured lifecycle tag                               | `GherkinSteps.ThenWithdrawn`; `GherkinSteps.ThenSuperseded`; `GherkinClaimFileReaderUnitTests.ClaimWithConfiguredLifecycleTags_WhenRead_ShouldGiveTheStrongestState`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        | Covered   |
| B-022    | Feature lifecycle source                               | `GherkinSteps.ThenFeatureSuperseded`; `GherkinSteps.ThenFeatureCurrent`; `AnalysisInputReaderGherkinUnitTests.FeatureLifecycleSource_WhenRead_ShouldTakeTheRecordsStateOrCurrentAndRemainAnEligibleTarget`; `AnalysisInputReaderGherkinUnitTests.FeatureRecordWithAnUnknownLifecycleValue_WhenRead_ShouldFailNamingTheRecord`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               | Covered   |
| B-023    | Feature tag disagrees                                  | `GherkinSteps.ThenFailureNamesFile`; `AnalysisInputReaderGherkinUnitTests.FeatureLevelTag_WhenComparedWithItsClaimFileSet_ShouldFailOnlyWhenItNamesAnotherFeature`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          | Covered   |
| B-024    | Ignored scenario keeps its claim                       | `GherkinSteps.ThenStillDeclared`; `GherkinSteps.ThenStillReported`; `AnalysisInputReaderGherkinUnitTests.IgnoredScenario_WhenReadBesideAnUnplannedOne_ShouldKeepTheClaimAndItsRequiredLinks`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                | Covered   |
| B-025    | Ordinary tags; A mistyped claim tag is an ordinary tag | `GherkinSteps.ThenOrdinary`; `GherkinSteps.ThenNoClaim`; `GherkinSteps.ThenNoMalformed`; `GherkinClaimFileReaderUnitTests.MixedOrdinaryAndClaimTags_WhenRead_ShouldDeclareOnlyTheClaimAndDrawNoFinding`; `GherkinClaimFileReaderUnitTests.TagFailingTheClaimGrammar_WhenRead_ShouldDeclareNoClaimAndNameNoTagInAFinding`; `ClaimTagParityUnitTests.ClaimTagToken_WhenReadByTheEngineAndTheAnalyzer_ShouldGiveTheSameAnswerFromBoth`; `GherkinTagFormUnitTests.FormWithoutExactlyOnePlaceholder_WhenConstructed_ShouldFailTheManifestWithTheClaimTagMessage`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 | Covered   |
| B-026    | Mixed Examples blocks                                  | `GherkinSteps.ThenFirstRequired`; `GherkinSteps.ThenOnlySecondPlanned`; `GherkinClaimFileReaderUnitTests.OutlineWithOneIgnoredExamplesBlock_WhenRead_ShouldPlanOnlyThatBlocksRows`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          | Covered   |
| B-027    | No candidate prefix configured                         | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     | Withdrawn |
| B-028    | Claim tag on an Examples block                         | `GherkinSteps.ThenMalformed`; `GherkinSteps.ThenNoClaim`; `GherkinClaimFileReaderUnitTests.ClaimTagOnAnExamplesBlock_WhenRead_ShouldReportItAtTheTagLineAndDeclareNothing`; `GherkinClaimAnalyzerUnitTests.ClaimFileFinding_WhenAnalyzed_ShouldBeReportedAsAWarningAtTheSuppliedFilesLine`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  | Covered   |
| B-029    | Claim file in another language                         | `GherkinSteps.ThenFailureNamesFile`; `AnalysisInputReaderGherkinUnitTests.ClaimFileInAnotherLanguage_WhenRead_ShouldFailNamingTheFile`; `GherkinClaimFileReaderUnitTests.ClaimFileDeclaringAnotherLanguage_WhenRead_ShouldFailNamingTheFileAndTheLanguage`; `GherkinClaimFileReaderUnitTests.ClaimFileInEnglishOrWithoutALanguageHeader_WhenRead_ShouldNotFail`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             | Covered   |
| B-030    | Catalogue exported with the evidence                   | `GherkinSteps.ThenEvidence`; `TestEvidenceCatalogExporterUnitTests.NativeCatalog_WhenProjected_ShouldCarryEachFeatureClaimAndCase`; `TestEvidenceCatalogExporterUnitTests.ClaimElement_WhenProjected_ShouldBeWrittenInTheContractSpelling`; `TestEvidenceCatalogExporterUnitTests.FeaturesInAnyOrder_WhenProjected_ShouldBeSortedByEpicNullFirstThenFeatureOrdinally`; `TestEvidenceCatalogExporterUnitTests.ClaimsInDocumentOrder_WhenProjected_ShouldKeepThatOrder`; `TestEvidenceArtifactWriterUnitTests.NativeCatalog_WhenWritten_ShouldBeVersionTwoCarryingTheCatalogMember`; `TestEvidenceArtifactWriterUnitTests.NativeCatalogWithCaseFacts_WhenWritten_ShouldBeVersionThreeCarryingTheCatalogMember`; `TestEvidenceArtifactWriterUnitTests.NativeCatalogArtifact_WhenWrittenAndReadAgain_ShouldReadBackEqual`; `TestEvidenceArtifactWriterUnitTests.ArtifactWithoutACatalog_WhenWritten_ShouldCarryNoCatalogMember`; `TestEvidenceArtifactReaderUnitTests.VersionTwoArtifactWithACatalog_WhenRead_ShouldExposeTheCatalog`; `TestEvidenceArtifactReaderUnitTests.VersionThreeArtifactWithACatalog_WhenRead_ShouldExposeTheCatalogBesideTheCaseFacts`; `TestEvidenceArtifactReaderUnitTests.VersionTwoArtifactWithoutACatalog_WhenRead_ShouldBeValidWithNoCatalog`; `TestEvidenceArtifactReaderUnitTests.MalformedCatalog_WhenRead_ShouldReturnContractFailure`; `EvidenceWireParityUnitTests.ProducerVersionTwoArtifact_WhenReadByCli_ShouldRetainTheCatalog`; `EvidenceWireParityUnitTests.ProducerVersionOneArtifact_WhenReadByCli_ShouldHaveNoCatalog`; `EvidenceWireParityUnitTests.MalformedCatalog_WhenReadByCli_ShouldGiveTheSameContractFailure`; `TestEvidenceCompilationExporterUnitTests.NativeClaimInput_WhenExported_ShouldCarryTheCatalogMember`; `TestEvidenceMtpCollectorCatalogTests.PayloadWithACatalog_WhenWritten_ShouldCarryTheCatalogInTheArtifact`; `TestEvidenceVsTestLoggerUnitTests.PayloadWithACatalog_WhenDiscovered_ShouldCarryTheCatalogInTheArtifact` | Covered   |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-23 -->

2026-10-23: the owner resolved OQ-1 to OQ-7 with their proposed defaults.
B-001, B-002, B-003 and B-015 now state the decided key, mapping and severity;
B-020 to B-024 add the decided lifecycle, Feature-tag agreement and
ignored-scenario behaviour. No claim id or scenario tag changed.

2026-10-23: owner decision 0001 makes a configured candidate prefix, which
begins the claim identifier, the test of a claim candidate. B-004 and B-013 are
amended to it, B-025 adds the ordinary-tag case and B-027 the missing-prefix
input failure, and one companion scenario documents B-013 and B-025 together.
OQ-8 and OQ-9 are resolved by the same decision.

2026-10-23: owner decision makes planned or ignored state per case. B-016 is
amended from scenario to case, and B-026 adds an Outline whose second Examples
block alone is ignored. 0201-F4 B-009 and 0201-F5 B-018 and B-019 carry the
same per-case state through the bridge and the gate.

2026-10-23: owner decision 0002 supersedes decision 0001: there is no
candidate prefix, and a tag declares a claim exactly when it matches the claim
tag form and the claim identifier grammar, as the engine reads it. B-004 and
B-025 are amended, B-013 and B-027 are withdrawn, and B-028 keeps the
malformed-claim diagnostic for a claim tag on an Examples block. Owner decision
0003 makes the analyzer the only reader of claim files, and B-030 adds the
export of the catalogue in the test-evidence artifact; B-029 adds the
non-English input failure under ADR-0001 (Proposed). The Feature moved from `src/analyzers/Catalogue/` to
`src/analyzers/Bindings/Gherkin/`; its id and every claim id are unchanged.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-23 -->

| ID   | Question                                                                                                                                                                                                                                                                                               | Blocks       | Resolution                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                |
| ---- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | Which manifest key selects native Gherkin claim input, and is the selection repository-wide or per claim file set?                                                                                                                                                                                     | B-001, B-002 | Open. Proposed default: a `claimSource` value of `gherkin` under `codeAnalysis.records`, beside a list of claim file sets; it extends the existing `codeAnalysis` section without a schema version change (0201-F6 OQ-1), and its absence means the Markdown table. RESOLVED, 2026-10-23: owner decision takes the proposed default: `codeAnalysis.records.claimSource: "gherkin"` with a list of claim file sets selects native input; its absence means the Markdown claims table. B-001 and B-002 state it.                                                                                                                                                                                                                                            |
| OQ-2 | How does a `.feature` file map to its Feature identity: a configured path mapping, a Feature-level tag, or both?                                                                                                                                                                                       | B-003        | Open. Proposed default: each claim file set is a glob with its configured Feature identity (and epic only where the grammar declares one), as the consumer fixture configures `arithmetic`; a Feature-level scope tag (`scenarioScopes.featureTag`), when present, must agree or it is an input failure. RESOLVED, 2026-10-23: owner decision takes the proposed default: each claim file set is a glob mapped to a configured Feature identity; a Feature-level tag, when present, must agree or it is an input failure. B-003 and B-023 state it.                                                                                                                                                                                                       |
| OQ-3 | When a claim tag sits on a Rule, which element declares the claim?                                                                                                                                                                                                                                     | B-010, B-012 | Open. Proposed default: the Rule declares it at the Rule's line; its scenarios inherit the tag under Gherkin semantics and are neither further declarations nor duplicates. RESOLVED, 2026-10-23: owner decision takes the proposed default: the Rule declares the claim at its line; scenarios inheriting the tag are neither declarations nor duplicates. B-010 states it.                                                                                                                                                                                                                                                                                                                                                                              |
| OQ-4 | Is one claim identifier tagged on two scenarios of one Feature a duplicate declaration, or one claim documented by two scenarios?                                                                                                                                                                      | B-012        | Open. Proposed default: a duplicate declaration, reported on both; one claim is declared by exactly one element, and several scenarios that document one claim sit under one Rule that declares it. RESOLVED, 2026-10-23: owner decision takes the proposed default: one identifier on two scenarios is a duplicate, reported on both; scenarios that document one claim are grouped under a Rule that declares it. B-012 states it.                                                                                                                                                                                                                                                                                                                      |
| OQ-5 | Does a Scenario with no claim tag in a configured claim file get a diagnostic, and at what default severity?                                                                                                                                                                                           | B-015        | Open. Proposed default: yes, at warning, the default for a binding defect under 0201-F6 decision 0001, adjustable through standard Roslyn severity configuration. RESOLVED, 2026-10-23: owner decision takes the proposed default: an untagged Scenario gets a diagnostic at warning, adjustable through standard Roslyn severity configuration. B-015 states it.                                                                                                                                                                                                                                                                                                                                                                                         |
| OQ-6 | How are claim status (Active, Withdrawn, Superseded) and Feature lifecycle expressed when claims live in Gherkin?                                                                                                                                                                                      | B-001, B-003 | Open. Proposed default: a claim with no lifecycle tag is Active; consumer-configured tags map to the existing `withdrawn` and `superseded` markers; Feature lifecycle comes from the configured Feature metadata record's lifecycle key when one is configured, and is current otherwise. RESOLVED, 2026-10-23: owner decision takes the proposed default: a claim with no lifecycle tag is Active; configured tags map to the existing withdrawn and superseded markers; Feature lifecycle comes from the configured metadata record, and is current otherwise. B-020 to B-022 state it.                                                                                                                                                                 |
| OQ-7 | Which tags mark a scenario as planned or ignored?                                                                                                                                                                                                                                                      | B-016        | Open. Proposed default: a consumer-configured tag list whose default is `@ignore`; a planned or ignored scenario still declares its claim, so required-link enforcement on that claim is unchanged. RESOLVED, 2026-10-23: owner decision takes the proposed default: a configurable tag list, default `@ignore`; the claim stays declared and the scenario never counts as passing. B-016 and B-024 state it.                                                                                                                                                                                                                                                                                                                                             |
| OQ-8 | Under an encoding with a literal prefix such as `@REQ-{claim}`, is a claim's identifier the part the placeholder captures (`FIX-001` from `@REQ-FIX-001`, as the existing claim-tag parser reads it) or the whole tag text after `@` (`REQ-FIX-001`, as the consumer fixture's relationships name it)? | B-004        | Open. Proposed default: the captured part, so the existing claim-tag parsing stays the one interpretation (C-1). RESOLVED, 2026-10-23: owner decision: the candidate prefix is part of the identifier. The encoding stays `@{claim}`, so the identifier is the whole tag text after `@` (`@REQ-FIX-001` declares `REQ-FIX-001`); the candidate prefix (`REQ-`) is configured beside the encoding and must begin the identifier. Decision 0001 and B-004, B-013 and B-025 state it. AMENDED, 2026-10-23: owner decision 0002 supersedes decision 0001: there is no candidate prefix, and the identifier is the text the placeholder captures, as the engine reads it; under the encoding `@{claim}` that is the whole tag text after `@`. B-004 states it. |
| OQ-9 | Under native Gherkin claim input, what happens when no candidate prefix is configured?                                                                                                                                                                                                                 | B-013, B-025 | Open. Proposed default: an input failure naming the manifest, because without a prefix no tag can be told apart from an ordinary one (decision 0001). RESOLVED, 2026-10-23: owner decision takes the proposed default. B-027 states it. SUPERSEDED, 2026-10-23: owner decision 0002 removes the candidate prefix, so the question no longer arises; B-027 is withdrawn.                                                                                                                                                                                                                                                                                                                                                                                   |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-24 -->

| Section | Status | Reviewer      | Note                                                                                                                                                                                                                                                                       |
| ------- | ------ | ------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-12    | 🟡     | Pending       | From consumer request #256; OQ-1 to OQ-7 resolved 2026-10-23; agreement not yet reviewed.                                                                                                                                                                                  |
| 7-9     | 🟢     | spec-reviewer | Item 0259 re-review, 2026-10-24: all 33 scenarios run; § 9 rows B-001 to B-030 are Covered or Withdrawn and cite the mechanism beneath each claim, including the MTP collector and VSTest logger pass-through. ADR-0001 stays Proposed pending the owner (open at the PR). |

## Tasks

- [0258: Design the native claim catalogue manifest contract and tag semantics](../.issue/0258-claim-catalogue-manifest-contract-design.yml)
- [0259: Read the claim catalogue from tagged Gherkin](../.issue/0259-native-gherkin-claim-catalogue-reader.yml)
- [0304: Document native Gherkin claim input for consumers](../.issue/0304-document-native-gherkin-claim-input.yml)

## Scoring

| Field | Value | Basis           |
| ----- | ----- | --------------- |
| value | 0     | Not yet scored. |
| risk  | 0     | Not yet scored. |
