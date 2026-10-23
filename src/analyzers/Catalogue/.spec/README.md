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
depends_on: ["F1", "F6"]
blocks: []
spikes: []
created: "2026-10-23"
updated: "2026-10-23"
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

| ID    | Claim                                                                                                                                                                                                                          | Source                                     | Status  |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------ | ------- |
| B-001 | Given a manifest setting `codeAnalysis.records.claimSource` to `gherkin`, this Feature builds the claim catalogue from the configured `.feature` files instead of a Markdown claims table.                                     | Consumer request #256; OQ-1                | Active  |
| B-002 | Given a manifest with no `codeAnalysis.records.claimSource`, this Feature leaves the Markdown claims-table catalogue unchanged.                                                                                                | Consumer request #256; OQ-1                | Active  |
| B-003 | Given a `.feature` file matched by a claim file set glob mapped to a Feature identity, this Feature assigns every claim declared in that file to that Feature identity.                                                        | Consumer request #256; OQ-2                | Active  |
| B-004 | Given a tag whose text after `@` begins with the configured candidate prefix and satisfies the claim identifier grammar, this Feature records a claim whose identifier is that whole text exactly as authored.                 | Consumer request #256; decision 0001       | Amended |
| B-005 | Given native Gherkin claim input and a Feature metadata record with no claims table, this Feature reports no missing claims-section input failure.                                                                             | Consumer request #256                      | Active  |
| B-006 | Given claim files in a central specification tree outside every project directory, this Feature resolves the same claims for each selected project.                                                                            | Consumer request #256                      | Active  |
| B-007 | Given a consumer identifier grammar that declares no epic, this Feature resolves native claims by Feature identity alone.                                                                                                      | Consumer request #256                      | Active  |
| B-008 | Given a claim tag on a Scenario, this Feature records one claim carrying the element kind, the repository-relative path and the one-based line of that Scenario.                                                               | Consumer request #256                      | Active  |
| B-009 | Given a claim tag on a Scenario Outline with several example rows, this Feature records exactly one claim located at that Scenario Outline.                                                                                    | Consumer request #256                      | Active  |
| B-010 | Given a claim tag on a Rule, this Feature records one claim located at that Rule and does not treat the Rule's scenarios inheriting the tag as further declarations.                                                           | Consumer request #256; OQ-3                | Active  |
| B-011 | Given a claim file whose scenarios are reordered or retitled, this Feature keeps each claim's identity unchanged.                                                                                                              | Consumer request #256                      | Active  |
| B-012 | Given one claim identifier declared on two elements within one Feature identity, this Feature reports a duplicate-claim diagnostic at each declaration's path and line.                                                        | Consumer request #256; OQ-4                | Active  |
| B-013 | Given a tag whose text after `@` begins with the configured candidate prefix but fails the claim identifier grammar, this Feature reports a malformed-claim diagnostic at that tag's path and line and records no claim.       | Consumer request #256; decision 0001       | Amended |
| B-014 | Given a configured claim file that is missing or is not valid Gherkin, this Feature reports an input failure naming that file rather than treating its Feature as having no claims.                                            | Consumer request #256                      | Active  |
| B-015 | Given a Scenario or Scenario Outline in a configured claim file that carries no claim tag of its own or by inheritance, this Feature reports a missing-claim diagnostic at its line with default severity warning.             | Consumer request #256; OQ-5                | Active  |
| B-016 | Given a scenario or Examples row carrying a configured planned or ignored tag itself or through its Examples block, this Feature marks that case planned and never counts it as implemented or passing evidence for its claim. | Consumer request #256; OQ-7                | Amended |
| B-017 | Given native claims and a mandatory enforcement scope missing a required `implements` or `verifies` relationship, this Feature reports the same diagnostic a Markdown-declared claim receives.                                 | Consumer request #256                      | Active  |
| B-018 | Given an `implements` or `verifies` relationship naming an identifier absent from the native catalogue, this Feature reports the same target-resolution failure a Markdown-declared claim receives.                            | Consumer request #256                      | Active  |
| B-019 | Given native Gherkin claim input, this Feature leaves the consumer's `.feature` files and metadata records unchanged and writes no claims table or other adapter.                                                              | Consumer request #256                      | Active  |
| B-020 | Given a native claim carrying no lifecycle tag, this Feature treats the claim as Active.                                                                                                                                       | Consumer request #256; OQ-6                | Active  |
| B-021 | Given a native claim carrying a tag the consumer configures as withdrawn or superseded, this Feature gives the claim that lifecycle state.                                                                                     | Consumer request #256; OQ-6                | Active  |
| B-022 | Given a configured Feature metadata record, this Feature takes the Feature lifecycle from that record, and treats the Feature as current when none is configured.                                                              | Consumer request #256; OQ-6                | Active  |
| B-023 | Given a claim file whose Feature-level tag names a Feature identity other than the one its claim file set maps, this Feature reports an input failure naming that file.                                                        | Consumer request #256; OQ-2                | Active  |
| B-024 | Given a scenario carrying a configured planned or ignored tag, this Feature still declares its claim, so required-link enforcement on that claim is unchanged.                                                                 | Consumer request #256; OQ-7                | Active  |
| B-025 | Given a tag whose text after `@` does not begin with the configured candidate prefix, this Feature treats it as an ordinary tag that declares no claim and draws no diagnostic.                                                | Consumer request #256; decision 0001       | Active  |
| B-026 | Given a Scenario Outline whose one Examples block carries a configured planned or ignored tag, this Feature marks only that block's rows planned and keeps the claim and the other rows' requirement unchanged.                | Consumer request #256; OQ-7                | Active  |
| B-027 | Given native Gherkin claim input with no candidate prefix configured, this Feature reports an input failure naming the manifest.                                                                                               | Consumer request #256; decision 0001; OQ-9 | Active  |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-23 -->

| ID  | Constraint                                                                                                                   | Rules Out                                                                                     |
| --- | ---------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------- |
| C-1 | Claim tags are read through the consumer's existing claim tag encoding and identifier grammar (0201-F6 C-3; 0201-F4 C-1).    | A second claim grammar, an analyzer regex setting or a competing claim-tag interpretation.    |
| C-2 | Native claims enter the same catalogue records that binding resolution, policy and reconciliation already consume (0201-F1). | A parallel resolution, enforcement or reporting path for Gherkin-declared claims.             |
| C-3 | A claim's identity is its Feature identity and claim identifier; its source location is provenance only.                     | Identity derived from line numbers, scenario titles, step text, example values or file names. |
| C-4 | Analysis reads only supplied inputs, offline, and reports repository-relative paths (epic 0201 § Constraints).               | Ambient file discovery, network access or absolute paths in diagnostics.                      |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-23 -->

| #   | Item                                                                               | Exclusion Reason                                                                                        |
| --- | ---------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------- |
| 1   | Generating Markdown claim tables or any other adapter from Gherkin.                | Gherkin stays the sole authored claim source; a generated copy is the duplication this Feature removes. |
| 2   | Changing the consumer's authored specification format or test runner.              | The consumer request excludes it.                                                                       |
| 3   | Correlating scenarios and Scenario Outline rows with discovered test cases.        | 0201-F4 owns the scenario bridge; this Feature supplies the claims its tags resolve against.            |
| 4   | Collecting runner discovery and execution evidence, including xUnit v2 and VSTest. | 0201-F3 owns runner evidence.                                                                           |
| 5   | Required-link policy, enforcement scopes, exemptions and diagnostic severities.    | 0201-F6 owns policy; this Feature only supplies the claims it enforces.                                 |
| 6   | The passing-outcome gate and the reconciled report.                                | 0201-F5 owns reconciliation.                                                                            |
| 7   | Teaching the standalone `specht` checker to read claims from Gherkin.              | Epic 0001 owns the checker; a new `SPEC###` rule is a new schema version (`0001-F7` C-11).              |
| 8   | Inferring a claim from a scenario title, step text or file name.                   | Only an explicit claim tag declares a claim.                                                            |
| 9   | The input kind of `SpechtTestEvidence` files.                                      | Item 0257 owns it.                                                                                      |

## 6. Concern Separation

<!-- last written by: implementer, 2026-10-23 -->

| Concern                                           | Classification | Responsibility                                                                                                                                                                              |
| ------------------------------------------------- | -------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Claim identity, tag encoding and candidate prefix | Business       | The consumer's `claim.tag` encoding and `identifiers.claim` grammar, with the configured prefix, decide what a tag means. One reader gives that meaning to engine, analyzer and tool (C-1). |
| Claim file to Feature mapping and lifecycle       | Business       | A claim file set maps files to a Feature identity and may name a record that supplies lifecycle. Nothing is inferred from file names, titles or steps (C-3).                                |
| Planned, ignored and lifecycle tags               | Business       | Consumer-configured tag lists. Planned state belongs to a case; lifecycle belongs to a claim.                                                                                               |
| Gherkin parsing and element location              | Technical      | The `Gherkin` package yields tags, elements and lines. `Specht.Claims` turns them into declarations and findings without I/O.                                                               |
| Catalogue assembly, input failures and findings   | Both           | The analyzer builds the same catalogue records Markdown claims produce (C-2). A failure is SPCT0001 with a stated cause; a finding is SPCT0010 to SPCT0012.                                 |
| Diagnostic identity and severity                  | Technical      | Append-only `SPCT####` ids, defaulted under 0201-F6 decision 0001 and changed only through standard Roslyn severity controls.                                                               |

## 7. Technical Design

<!-- last written by: implementer, 2026-10-23 -->

Records: [ADR-0021](../../../../.spec/adr/0021-gherkin-claim-semantics-live-in-a-netstandard-library.md)
(where the reader lives), [ADR-0001](adr/0001-native-claim-source-contract.md)
(the keys, the element rules and the diagnostic identities) and
[decision 0001](decisions/0001-claim-candidates-carry-the-encoding-prefix.md).
Both ADRs are proposed and wait on the owner. The required-case scope key and its
grammar are in [0201-F6 § 7](../../../codeanalysis/.spec/README.md). No new
architecture is adopted (ADR-0014): the reader is an instance class with a
constructor, the catalogue records are the existing ones, and nothing is static.

### Where the code lives

| Project and namespace                                         | Holds                                                                                                                                                                                                    |
| ------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `src/claims/claims.csproj`, `Specht.Claims`, `netstandard2.0` | `TagForm`, `ClaimTagEncoding`, `ClaimTagRead`, `GherkinClaimReader`, `ClaimFile`, `ClaimDeclaration`, `ClaimCase`, `ClaimElement`, `ClaimFinding`, `InvalidGherkinException`, and 0201-F6's `CaseScope`. |
| `src/analyzers/Catalogue/`, `Specht.Analysis.Catalogue`       | `ClaimSourcePolicy` (the parsed keys), `NativeClaimCatalogue` (files to `BindingFeature` records and findings), `CatalogueFinding`, `CatalogueDiagnostics`, `CatalogueAnalyzer`.                         |
| `src/analyzers/Policy/` (existing)                            | `AnalysisInputException`, and the changes below to `AnalysisPolicyReader`, `AnalysisInputReader`, `AnalysisPolicy`, `AnalysisProject` and `AnalysisSnapshot`.                                            |
| `src/specht/Model/FeatureTagParser.cs` (existing)             | Delegates each token to `ClaimTagEncoding`; its method, its legacy scanner and its reports are unchanged.                                                                                                |

`claims.csproj` clears the inherited TFM list as the analyzer does, references
only the `Gherkin` package (`35.0.0`, declared once in `Directory.Packages.props`
under a `Gherkin` label), and is listed in `specht.slnx` under `/src/`.
`analyzers.csproj` and `specht.csproj` take `ProjectReference`s to it. The
existing `*.dll` pack glob carries `Specht.Claims.dll`, `Gherkin.dll` and
`Cucumber.Messages.dll` into the package; 0259 adds a closure test naming them.
The change to `FeatureTagParser` is a refactor: No `.feature` change needed.
Constraints preserved: 0001-F1 B-004 and the golden baseline report.

### One reading of a claim tag (C-1)

`TagForm(form, placeholder, ignoreCase)` holds a literal form with exactly one
`{placeholder}` and throws `ArgumentException` otherwise. `Capture(tag)` returns
the text between the form's literal prefix and suffix, or `null` when the tag
lacks either (compared ordinally, or ordinal-ignore-case, as constructed). A
captured text of length zero is returned.

`ClaimTagEncoding(TagForm form, string grammar, string? candidatePrefix = null)`
holds the claim grammar and `Read(tag)` returns a `ClaimTagRead(Outcome, Text)`:

| Condition, tested in this order                                       | `Outcome`       | `Text`        |
| --------------------------------------------------------------------- | --------------- | ------------- |
| `form.Capture` is `null`                                              | `NotACandidate` | `null`        |
| `candidatePrefix` is set and the captured text does not begin with it | `NotACandidate` | `null`        |
| the captured text matches `\A(?:grammar)\z`                           | `Claim`         | captured text |
| otherwise                                                             | `Malformed`     | captured text |

The prefix and the grammar use the encoding's case rule. The engine builds
`new ClaimTagEncoding(new TagForm(form, "claim", ignoreCase), identifiers["claim"])`
with no prefix and keeps only `Claim`; the analyzer adds the configured prefix and
uses all three outcomes. The captured text is the claim identifier, exactly as
authored (B-004).

### Reading Gherkin

`GherkinClaimReader(ClaimTagEncoding, GherkinClaimSettings)` has one method,
`Read(relativePath, text)`, returning a `ClaimFile`. `GherkinClaimSettings` holds
the planned, withdrawn and superseded tag lists (strings, `@`-led) and optional
`TagForm`s for the Feature and epic tags. It parses with the `Gherkin` package's
parser, one per call, and throws `InvalidGherkinException` when the package
reports a parse error or when the text holds no `Feature:`. Element kinds come
from the parsed structure, not from keyword text, so any Gherkin language reads.

| `ClaimFile` member | Meaning                                                                                      |
| ------------------ | -------------------------------------------------------------------------------------------- |
| `Path`             | The `relativePath` argument.                                                                 |
| `Declarations`     | `ClaimDeclaration(Id, Element, Path, Line, State, Cases)` in document order, then tag order. |
| `Findings`         | `ClaimFinding(Kind, Path, Line, Text)`; `Kind` is `Malformed` or `MissingClaim`.             |
| `FeatureTagValues` | The text each Feature-level tag captures through the Feature `TagForm`.                      |
| `EpicTagValues`    | The same for the epic `TagForm`.                                                             |

Rules, in the order the reader applies them:

1. **Declaring elements.** A `Feature`, a `Rule`, a `Scenario` and a
   `Scenario Outline` declare the claims their own tags carry. `Element` is
   `Feature`, `Rule`, `Scenario` or `ScenarioOutline`; an element is an Outline
   when it has at least one `Examples` block. `Line` is the line of the element's
   keyword, never the tag line (B-008 to B-010).
2. **Tags.** Each tag name, `@` included, goes through `Read`. `Claim` declares
   `Id`, once per element however often the tag repeats. `Malformed` adds a
   `Malformed` finding at the tag's line with the text
   `Tag <tag> begins with the claim prefix but is not a valid claim identifier.`
   (B-013). `NotACandidate` does nothing and draws nothing (B-025). A tag on a
   `Rule` that a scenario inherits is not a further declaration (B-010).
3. **Placement.** A tag that reads as `Claim` or `Malformed` on an `Examples`
   block adds a `Malformed` finding with the text
   `Tag <tag> begins with the claim prefix but sits where no claim can be declared.`
   and declares nothing. A `Background` has no tags.
4. **State.** `Superseded` when a declaring element's own tags include a
   superseded tag, else `Withdrawn` for a withdrawn tag, else `Active` (B-020,
   B-021). Inherited tags never change a claim's state.
5. **Cases.** A case is one `Scenario` (`ClaimCase.Line` is its keyword line) or
   one row of an Outline's `Examples` table body (the row's line); an Outline with
   no body rows has no case. A claim's cases are the cases of every Scenario and
   Outline at or below its declaring element, in document order, so a Rule's
   claim holds all of its scenarios' cases.
6. **Planned.** `ClaimCase.Planned` is true when any tag of the Feature, of the
   enclosing Rule, of the Scenario or Outline, or of the row's own `Examples`
   block is in the planned list (B-016, B-026). A planned case changes neither
   the claim nor its other cases (B-024).
7. **Missing claim.** A Scenario or Outline that no declaration covers, by its own
   tags, its Rule's or the Feature's, adds a `MissingClaim` finding at its keyword
   line with the text `<Scenario|Scenario Outline> "<title>" declares no claim.`
   A malformed tag does not cover it (B-015).

### Manifest contract

All keys sit under `codeAnalysis` in the consumer's manifest. A key not listed
here is not read. Under no `claimSource`, every key in this table is ignored and
the Markdown path is unchanged (B-002).

| Key                                    | Type and default                                                                 | Rule                                                                                                                   |
| -------------------------------------- | -------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------- |
| `records.claimSource`                  | string; absent selects the Markdown claims table                                 | The only value is `"gherkin"`; any other value is an input failure (B-001). Repository-wide.                           |
| `records.claimFileSets`                | array of `{ glob, feature, epic?, record? }`; required, nonempty under `gherkin` | See below (B-003, B-006, B-007).                                                                                       |
| `records.claimTagPrefix`               | string; required under `gherkin`                                                 | Nonempty, no whitespace, no `@`. The text after `@` of every claim identifier begins with it (decision 0001, B-027).   |
| `records.plannedTags`                  | array of string; default `["@ignore"]`                                           | Exact tags that mark a case planned. `[]` marks nothing.                                                               |
| `records.withdrawnTags`                | array of string; default `[]`                                                    | Exact tags that give a claim the state `Withdrawn`.                                                                    |
| `records.supersededTags`               | array of string; default `[]`                                                    | Exact tags that give a claim the state `Superseded`.                                                                   |
| `records.featureLifecycleKey`          | string; existing, default `spec_status`                                          | The frontmatter key read from a claim file set's `record`.                                                             |
| `scenarioScopes.featureTag`, `epicTag` | string; existing, optional                                                       | When set, the Feature and epic tags a claim file may carry (B-023).                                                    |
| `mappings`: `/claimTag`                | pointer, normally `/claim/tag`; required under `gherkin`                         | The claim tag encoding, mapped like `/identifiers`. It must hold `{claim}` exactly once.                               |
| `mappings`: `/claimTagIgnoreCase`      | pointer, normally `/claim/tagIgnoreCase`; optional, default `false`              | Map it only when the consumer's manifest sets it, because a mapping to an absent field is an existing input failure.   |
| `mappings`: `/claimsSection`           | existing; optional under `gherkin`                                               | A Gherkin-only consumer has no claims table; `BindingPolicy.ClaimsSection` is then empty and nothing reads it (B-005). |

A tag-list entry is `@` followed by one or more characters other than whitespace.
Tag lists are compared ordinally with Gherkin tag names; only claim tags honour
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

The delivery default for scenario inputs is `**/.spec/*.feature`. A central tree
such as `.spec/features/arithmetic/` is not matched, so the consumer sets the
`SpechtScenarioInputs` build property to include it. A set whose glob matches no
delivered input is an input failure, which is how that omission shows (B-006).

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
      "/claimTag": "/claim/tag",
      "/claimTagIgnoreCase": "/claim/tagIgnoreCase"
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
      "claimTagPrefix": "REQ-",
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

`AnalysisPolicyReader` parses the keys into a `ClaimSourcePolicy` held by
`AnalysisPolicy` (`null` for the Markdown path). `AnalysisInputReader`, when the
policy has one, replaces the loop over `feature` inputs with
`NativeClaimCatalogue`, which runs in this order and stops at the first failure
(the reader reports one failure, as today):

1. the keys and the mapped encoding, in the order of the failure table below;
2. every delivered `scenario` input that a glob matches, in ordinal path order:
   a file matched by two sets fails; a file whose text is `null` fails; a file
   `GherkinClaimReader` rejects fails; a captured Feature or epic tag differing
   from its set's identity fails;
3. a set matching no delivered file fails;
4. each `record`, validated by the Feature schema as a Markdown record is;
5. each selected Feature without a set fails.

It then builds one `BindingFeature` per set: identity from the set, state from
the record or `current`, `RelativePath` the record's path or else the ordinal-first
claim file, and a `BindingClaim` per declaration. `BindingClaim` gains optional
trailing members `RelativePath`, `Element` and `Cases`; `BindingResolver` uses
`claim.RelativePath ?? feature.RelativePath` and `claim.Line` as the target's
provenance (B-008, C-2), and `BindingCatalog` validates every claim path with the
existing provenance guard. Everything after the catalogue is the existing code:
`AnalysisPolicy.Select`, `RequiredLinks` and `BindingResolver` are unchanged, so
B-017 and B-018 give the Markdown path's diagnostics (SPCT0007, SPCT0004).
Planned cases are visible through `Cases` to the scenario bridge (0201-F4) and to
reconciliation (0201-F5); this Feature counts nothing.

A claim keeps its identity through reordering or retitling because the identity
is the Feature identity and `Id` (C-3, B-011); path and line are provenance.
Duplicates are not removed: after all files are read, an `Id` declared by two
declarations of one Feature adds a `SPCT0010` finding at each (B-012), and the
catalogue keeps both, so a reference to it is ambiguous as for a Markdown
duplicate. No catalogue member reads the file system.

### Input failures

A failure is `AnalysisInputException(relativePath, message)`, caught by
`AnalysisInputReader` into the existing `AnalysisInputFailure` and reported as
`SPCT0001` (error, 0201-F6 B-017) with the message as its argument. The path is the
manifest unless the row says otherwise.

| Cause                                                           | Path            | Message                                                                              | Claim |
| --------------------------------------------------------------- | --------------- | ------------------------------------------------------------------------------------ | ----- |
| `claimSource` is not `gherkin`                                  | manifest        | `codeAnalysis.records.claimSource must be "gherkin".`                                |       |
| `claimTagPrefix` absent                                         | manifest        | `Native claim input requires codeAnalysis.records.claimTagPrefix.`                   | B-027 |
| `claimTagPrefix` empty, spaced or holding `@`                   | manifest        | `codeAnalysis.records.claimTagPrefix must be nonempty text without whitespace or @.` |       |
| `/claimTag` unmapped or without one `{claim}`                   | manifest        | `Native claim input requires the claim tag encoding mapped as /claimTag.`            |       |
| a tag list holds a malformed entry                              | manifest        | `Claim tag lists must hold @-led tags without whitespace.`                           |       |
| `claimFileSets` absent, empty or of the wrong shape or identity | manifest        | `codeAnalysis.records.claimFileSets is missing or invalid.`                          |       |
| two sets name one Feature identity                              | manifest        | `Two claim file sets name one Feature.`                                              |       |
| a set's glob matches no delivered input                         | manifest        | `A claim file set matches no supplied scenario input.`                               |       |
| one file matched by two sets                                    | the file        | `A claim file matches more than one claim file set.`                                 |       |
| claim file delivered without readable text                      | the file        | `Claim file is missing or unreadable.`                                               | B-014 |
| claim file rejected by the reader                               | the file        | `Claim file is not valid Gherkin.`                                                   | B-014 |
| Feature or epic tag differs from the set                        | the file        | `Feature or epic tag names a different identity than its claim file set.`            | B-023 |
| `record` is not a delivered `feature` input                     | the record path | `Feature record is not a supplied feature input.`                                    |       |
| record lifecycle value unknown                                  | the record path | `A supplied Feature has an unknown lifecycle value.`                                 |       |
| a selected Feature has no set                                   | manifest        | `A selected consumer Feature has no claim file set.`                                 |       |

A Feature is never reported as having no claims when its file failed (B-014).

### Diagnostics

`CatalogueAnalyzer` reads the same snapshot as `AnalysisInputAnalyzer`, through
`AnalysisInputAdapter`, and reports `AnalysisSnapshot.Findings` when the snapshot
is available. `CatalogueFinding(DiagnosticId, RelativePath, Line, Text)` is
sorted by path (ordinal), line, id. Each diagnostic is at the supplied file's
line, with the properties `SpechtRelativePath` and `SpechtLine`, message `{0}`
being `Text`, category `Traceability` and a help link under
`src/codeanalysis/docs/`.

| Id       | Title                       | Default severity | Reported                                                                                                | Claims |
| -------- | --------------------------- | ---------------- | ------------------------------------------------------------------------------------------------------- | ------ |
| SPCT0010 | Duplicate claim declaration | Warning          | At each declaration's path and line; text `Claim <id> is declared more than once in Feature <feature>.` | B-012  |
| SPCT0011 | Malformed claim tag         | Warning          | At the tag's line; the two texts of rules 2 and 3 above                                                 | B-013  |
| SPCT0012 | Scenario declares no claim  | Warning          | At the Scenario or Outline keyword line; the text of rule 7                                             | B-015  |

The ids follow SPCT0009, the last published (SPCT0099 is a test fixture);
SPCT0001 to SPCT0009 are unchanged. A duplicate claim declaration is a defect
that makes references ambiguous, so it is a warning and not the information of
SPCT0009, which is a duplicate of a valid relationship. `CatalogueDiagnostics` is
an append-only registry of its own, with the same guard that rejects a removed,
reassigned or revived id as `DocumentationDiagnostics`; a test unions the two so
no id collides. A page for each id under `src/codeanalysis/docs/`, the table in
its README and the SPCT0001 page's list of causes are 0259's. Severity and
suppression are the standard Roslyn controls (0201-F6 B-007, B-008); suppressing
a finding leaves the catalogue unchanged.

### What the later items follow

- **0259** builds everything above except `CaseScope`, and gives each claim of
  § 3 the assertion in the table below.
- **0245** (the scenario bridge) reads claims only through `ClaimTagEncoding` and
  the Feature and epic `TagForm`s, and joins a runner case to a catalogue case by
  path and line; the producer's own row identity stays 0201-F4's.
- **0260** evaluates `CaseScope` over a case's categories, written without `@`;
  the incomplete-run diagnosis is a typed finding in the evidence artifact and
  report (0201-F5 `findings`), not an SPCT diagnostic, because it exists only
  after a run. The artifact's new fields and any format version are 0260's § 7.
- **0262** gates on `ClaimCase.Planned` from the catalogue and on owned keys. The
  tool takes a `ProjectReference` to `Specht.Claims` and never to the analyzer.

| Claim | The test asserts, at `AnalysisInputReader.Read` unless stated                                                                                                            |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| B-001 | With `claimSource: "gherkin"`, the snapshot catalogue holds `REQ-FIX-001` for Feature `arithmetic` and a `feature` input's claims table is not read.                     |
| B-002 | With no `claimSource` and a record with a claims table, the catalogue is the table's claims, and a present `claimFileSets` changes nothing.                              |
| B-003 | A claim in a file matched by the set belongs to that set's identity.                                                                                                     |
| B-004 | `Declarations[0].Id` is `REQ-FIX-001`; with `tagIgnoreCase` true, `@req-fix-001` reads as `req-fix-001`, as authored.                                                    |
| B-005 | A set whose `record` has no claims table yields no failure; so does a set with no `record`.                                                                              |
| B-006 | The same inputs with two different selected project paths give equal catalogues; a glob outside every project directory resolves.                                        |
| B-007 | With no `identifiers.epic`, a claim resolves from a reference with no epic, and an `epic` in a set is a failure.                                                         |
| B-008 | A Scenario's declaration has `Element` `Scenario`, its path, and the keyword line, not the tag line.                                                                     |
| B-009 | An Outline with two rows gives one declaration and two cases.                                                                                                            |
| B-010 | A Rule tag gives one declaration at the Rule line, its scenarios' cases, and no `SPCT0010`.                                                                              |
| B-011 | Reordered and retitled elements give declarations equal in `Id` and Feature identity.                                                                                    |
| B-012 | The same `Id` on two elements gives two `SPCT0010` findings, one per path and line.                                                                                      |
| B-013 | The scenario tagged `@REQ-FIX-004 @visual @ignore @boundary @issue-42 @REQ-FIX-5` gives one declaration, one `SPCT0011` at the `@REQ-FIX-5` line, and none for that tag. |
| B-014 | A `null` text and a non-Gherkin text each give one failure whose path is the file and whose message is the stated one.                                                   |
| B-015 | A Scenario with no claim from itself, its Rule or the Feature gives `SPCT0012` at its keyword line; with a Rule claim it gives none.                                     |
| B-016 | A case under a planned tag, itself or through its `Examples` block, has `Planned` true.                                                                                  |
| B-017 | A native claim with no `verifies` method gives the same SPCT0007 as a Markdown claim; the existing resolver and policy are used unchanged.                               |
| B-018 | A `verifies` naming `REQ-FIX-999` gives the same SPCT0004 as a Markdown claim.                                                                                           |
| B-019 | `Read` takes texts and returns records; no member writes. The test supplies no file system.                                                                              |
| B-020 | A claim with no lifecycle tag has `State` `Active`.                                                                                                                      |
| B-021 | A configured withdrawn tag gives `Withdrawn`, a superseded tag `Superseded`; both together give `Superseded`.                                                            |
| B-022 | A `record` with `spec_status: draft` gives Feature state `draft`; none gives `current`; both are eligible targets.                                                       |
| B-023 | `@spec.feature:billing` on a file mapped to `arithmetic` gives one failure at that file; `@spec.feature:arithmetic`, or no Feature tag, gives none.                      |
| B-024 | An `@ignore` scenario still declares its claim, and the claim's required links are as for an unplanned one.                                                              |
| B-025 | `@visual`, `@ignore`, `@boundary` and `@issue-42` give no declaration and no finding.                                                                                    |
| B-026 | An Outline with one ignored Examples block has `Planned` true only for that block's rows; the claim and the other rows are unchanged.                                    |
| B-027 | No `claimTagPrefix` gives one failure at the manifest with the stated message.                                                                                           |

## 8. Testing Strategy

<!-- last written by: test-writer, 2026-10-23 -->

Not yet written.

## 9. Traceability Matrix

<!-- last written by: test-writer, 2026-10-23 -->

| Claim ID | Scenario                            | Test    | Status  |
| -------- | ----------------------------------- | ------- | ------- |
| B-001    | Native claim input selected         | Missing | Missing |
| B-002    | Markdown catalogue unchanged        | Missing | Missing |
| B-003    | File mapped to its Feature identity | Missing | Missing |
| B-004    | Claim identifier kept as authored   | Missing | Missing |
| B-005    | No claims table required            | Missing | Missing |
| B-006    | Central specification tree          | Missing | Missing |
| B-007    | No epic identity                    | Missing | Missing |
| B-008    | Scenario claim location             | Missing | Missing |
| B-009    | Scenario Outline declares one claim | Missing | Missing |
| B-010    | Rule claim                          | Missing | Missing |
| B-011    | Stable claim identity               | Missing | Missing |
| B-012    | Duplicate claim                     | Missing | Missing |
| B-013    | Claim candidates and ordinary tags  | Missing | Missing |
| B-014    | Missing or invalid claim file       | Missing | Missing |
| B-015    | Scenario without a claim            | Missing | Missing |
| B-016    | Planned or ignored scenario         | Missing | Missing |
| B-017    | Required relationship missing       | Missing | Missing |
| B-018    | Unknown claim                       | Missing | Missing |
| B-019    | No adapter written                  | Missing | Missing |
| B-020    | Untagged claim is Active            | Missing | Missing |
| B-021    | Configured lifecycle tag            | Missing | Missing |
| B-022    | Feature lifecycle source            | Missing | Missing |
| B-023    | Feature tag disagrees               | Missing | Missing |
| B-024    | Ignored scenario keeps its claim    | Missing | Missing |
| B-025    | Claim candidates and ordinary tags  | Missing | Missing |
| B-026    | Mixed Examples blocks               | Missing | Missing |
| B-027    | No candidate prefix configured      | Missing | Missing |

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

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-23 -->

| ID   | Question                                                                                                                                                                                                                                                                                               | Blocks       | Resolution                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                |
| ---- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | Which manifest key selects native Gherkin claim input, and is the selection repository-wide or per claim file set?                                                                                                                                                                                     | B-001, B-002 | Open. Proposed default: a `claimSource` value of `gherkin` under `codeAnalysis.records`, beside a list of claim file sets; it extends the existing `codeAnalysis` section without a schema version change (0201-F6 OQ-1), and its absence means the Markdown table. RESOLVED, 2026-10-23: owner decision takes the proposed default: `codeAnalysis.records.claimSource: "gherkin"` with a list of claim file sets selects native input; its absence means the Markdown claims table. B-001 and B-002 state it.                                                                            |
| OQ-2 | How does a `.feature` file map to its Feature identity: a configured path mapping, a Feature-level tag, or both?                                                                                                                                                                                       | B-003        | Open. Proposed default: each claim file set is a glob with its configured Feature identity (and epic only where the grammar declares one), as the consumer fixture configures `arithmetic`; a Feature-level scope tag (`scenarioScopes.featureTag`), when present, must agree or it is an input failure. RESOLVED, 2026-10-23: owner decision takes the proposed default: each claim file set is a glob mapped to a configured Feature identity; a Feature-level tag, when present, must agree or it is an input failure. B-003 and B-023 state it.                                       |
| OQ-3 | When a claim tag sits on a Rule, which element declares the claim?                                                                                                                                                                                                                                     | B-010, B-012 | Open. Proposed default: the Rule declares it at the Rule's line; its scenarios inherit the tag under Gherkin semantics and are neither further declarations nor duplicates. RESOLVED, 2026-10-23: owner decision takes the proposed default: the Rule declares the claim at its line; scenarios inheriting the tag are neither declarations nor duplicates. B-010 states it.                                                                                                                                                                                                              |
| OQ-4 | Is one claim identifier tagged on two scenarios of one Feature a duplicate declaration, or one claim documented by two scenarios?                                                                                                                                                                      | B-012        | Open. Proposed default: a duplicate declaration, reported on both; one claim is declared by exactly one element, and several scenarios that document one claim sit under one Rule that declares it. RESOLVED, 2026-10-23: owner decision takes the proposed default: one identifier on two scenarios is a duplicate, reported on both; scenarios that document one claim are grouped under a Rule that declares it. B-012 states it.                                                                                                                                                      |
| OQ-5 | Does a Scenario with no claim tag in a configured claim file get a diagnostic, and at what default severity?                                                                                                                                                                                           | B-015        | Open. Proposed default: yes, at warning, the default for a binding defect under 0201-F6 decision 0001, adjustable through standard Roslyn severity configuration. RESOLVED, 2026-10-23: owner decision takes the proposed default: an untagged Scenario gets a diagnostic at warning, adjustable through standard Roslyn severity configuration. B-015 states it.                                                                                                                                                                                                                         |
| OQ-6 | How are claim status (Active, Withdrawn, Superseded) and Feature lifecycle expressed when claims live in Gherkin?                                                                                                                                                                                      | B-001, B-003 | Open. Proposed default: a claim with no lifecycle tag is Active; consumer-configured tags map to the existing `withdrawn` and `superseded` markers; Feature lifecycle comes from the configured Feature metadata record's lifecycle key when one is configured, and is current otherwise. RESOLVED, 2026-10-23: owner decision takes the proposed default: a claim with no lifecycle tag is Active; configured tags map to the existing withdrawn and superseded markers; Feature lifecycle comes from the configured metadata record, and is current otherwise. B-020 to B-022 state it. |
| OQ-7 | Which tags mark a scenario as planned or ignored?                                                                                                                                                                                                                                                      | B-016        | Open. Proposed default: a consumer-configured tag list whose default is `@ignore`; a planned or ignored scenario still declares its claim, so required-link enforcement on that claim is unchanged. RESOLVED, 2026-10-23: owner decision takes the proposed default: a configurable tag list, default `@ignore`; the claim stays declared and the scenario never counts as passing. B-016 and B-024 state it.                                                                                                                                                                             |
| OQ-8 | Under an encoding with a literal prefix such as `@REQ-{claim}`, is a claim's identifier the part the placeholder captures (`FIX-001` from `@REQ-FIX-001`, as the existing claim-tag parser reads it) or the whole tag text after `@` (`REQ-FIX-001`, as the consumer fixture's relationships name it)? | B-004        | Open. Proposed default: the captured part, so the existing claim-tag parsing stays the one interpretation (C-1). RESOLVED, 2026-10-23: owner decision: the candidate prefix is part of the identifier. The encoding stays `@{claim}`, so the identifier is the whole tag text after `@` (`@REQ-FIX-001` declares `REQ-FIX-001`); the candidate prefix (`REQ-`) is configured beside the encoding and must begin the identifier. Decision 0001 and B-004, B-013 and B-025 state it.                                                                                                        |
| OQ-9 | Under native Gherkin claim input, what happens when no candidate prefix is configured?                                                                                                                                                                                                                 | B-013, B-025 | Open. Proposed default: an input failure naming the manifest, because without a prefix no tag can be told apart from an ordinary one (decision 0001). RESOLVED, 2026-10-23: owner decision takes the proposed default. B-027 states it.                                                                                                                                                                                                                                                                                                                                                   |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-23 -->

| Section | Status | Reviewer | Note                                                                                      |
| ------- | ------ | -------- | ----------------------------------------------------------------------------------------- |
| 1-12    | 🟡     | Pending  | From consumer request #256; OQ-1 to OQ-7 resolved 2026-10-23; agreement not yet reviewed. |

## Tasks

None yet. Items are cut from § 3 after agreement.

## Scoring

| Field | Value | Basis           |
| ----- | ----- | --------------- |
| value | 0     | Not yet scored. |
| risk  | 0     | Not yet scored. |
