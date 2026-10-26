---
title: "Specification: Native Gherkin claims"
description: "Under a manifest that selects native claim input, the check builds a Feature's claims from the tags of its .feature files instead of a Markdown claims table, so a repository with no specification document per Feature is checked by the same rules, with the untagged scenario, the misplaced claim tag and the unreadable claim file reported as new rules of the next schema version"
type: feature
id: "F16"
epic: "0001"
spec_status: draft
status: ready
priority: med
value: 0
risk: 0
rank: 0
scored_by: null
scored_on: null
domain: "Specification governance"
author: "spec-author"
milestone: null
children: []
depends_on: ["F5", "F6", "F7", "F12"]
blocks: []
spikes: []
created: "2026-10-26"
updated: "2026-10-26"
github_issue: null
synced_at: null
---

# Specification: Native Gherkin claims

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-26 -->

A repository whose claims are authored only as tagged scenarios cannot be checked today: the check builds a Feature's claims from the § 3 table of a specification document, so a consumer with no such document per Feature, such as an application with no .NET code whose agreement is its `.feature` files, either duplicates every claim into a Markdown table or gets no check at all. The analyzer already reads such a claim catalogue (`0201-F7`) and names the checker-side reading as epic `0001`'s (`0201-F7` § 5 row 7). This Feature removes that failure state: under a manifest that selects native claim input, the check builds the Feature's claims from the tags of the `.feature` files a claim file set names, with the same identities and lifecycle the analyzer gives them, applies the claim rules to that catalogue, and reports a scenario with no claim, a claim tag where none can be declared and a claim file it cannot read, as new rules of the next schema version.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-26 -->

| #   | Persona                                          | Need                                                                                            | Pain Point Today                                                                            |
| --- | ------------------------------------------------ | ----------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------- |
| 1   | The maintainer of a repository with no .NET code | Claims checked for grammar, uniqueness and coverage while `.feature` files stay the only source | Every claim is copied into a Markdown table, or the tree is not checked                     |
| 2   | The maintainer of a central specification tree   | One claim file set per Feature, with no specification document and no epic                      | Discovery assumes a document found through the layouts, with an epic and a Feature id in it |
| 3   | A consumer that also runs the analyzer           | One manifest that both the checker and the analyzer read the same way                           | The analyzer's claim catalogue and the checker's disagree on what a claim is                |
| 4   | `0001-F12` and `0001-F13`                        | A claim catalogue to resolve § 9 rows and the trace against, whatever its source                | N/A (internal dependency)                                                                   |

### Assumptions

| ID  | Assumption                                                                                                                                                                                                                                                                                 |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| A-1 | A claim tag is the manifest's `claim.tag` form, honouring `claim.tagIgnoreCase`, whose captured text matches `identifiers.claim`; the check already reads companion tags that way (`0001-F5` decision 0013), and the analyzer reads native claims that way (`0201-F7` C-1, decision 0002). |
| A-2 | Schema `0.2.0` has shipped in a published package (the `v0.2.x` tags embed `src/tool/manifest/v0.2.0`), so a new `SPEC###` id lands in a new schema version, `0.3.0` (`0001-F7` C-11; brief § 4 already names `0.3.0` for `0001-F7` decision 0015).                                        |
| A-3 | The element, identity, lifecycle and planned semantics of a native claim are the ones `0201-F7` B-003 to B-012 and B-020 to B-029 state and its ADR-0001 fixes; this Feature gives the checker the same reading and decides nothing the analyzer decided differently.                      |
| A-4 | Under schema `0.2.0` the `claim` object has two members and a key a pin does not define exits `3` (`docs/configuration.md`), so a member this Feature adds under `0.3.0` is refused by every earlier pin as today and changes no earlier pin's verdict (B-028).                            |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-26 -->

| ID    | Claim                                                                                                                                                                                                                                                                                                                           | Source                                                      | Status |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------- | ------ |
| B-001 | Given a manifest that pins schema `0.3.0` or later and sets `claim.source` to `gherkin`, for each entry of `claim.files`, a claim file set, this Feature builds the claims of the Feature identity the entry names from the `.feature` files its glob matches outside `exclusions`, and reads no claims table for that Feature. | `0201-F7` B-001; ADR-0023; OQ-1, OQ-2, OQ-9                 | Active |
| B-002 | Given a manifest with no `claim.source`, or `claim.source` of `markdown`, this Feature changes no verdict: every claim comes from the § 3 table as today, and no rule reads the manifest's `codeAnalysis` section.                                                                                                              | `0201-F7` B-002; `0001-F5` C-4; ADR-0023                    | Active |
| B-003 | Given a claim file set with a `record`, this Feature reads the Feature's frontmatter, lifecycle and dependency edges from that document, and its frontmatter is held to the Feature schema by `SPEC001` to `SPEC003`; given a set with no `record`, the Feature's lifecycle is current and it declares no edge.                 | `0201-F7` B-005, B-022, ADR-0001 item 8; OQ-4               | Active |
| B-004 | Given a tag on a Feature, a Rule, a Scenario or a Scenario Outline that matches the claim tag form and whose captured text matches the claim grammar, this Feature records one claim whose identifier is that captured text exactly as authored, located at the element's keyword line.                                         | `0201-F7` B-004, B-008; A-1                                 | Active |
| B-005 | Given a tag that does not match the claim tag form, or whose captured text fails the claim grammar, this Feature treats it as an ordinary tag that declares no claim and draws no violation.                                                                                                                                    | `0201-F7` B-025, decision 0002                              | Active |
| B-006 | Given a claim tag on a Scenario Outline with several Examples rows, this Feature records exactly one claim, located at the Scenario Outline.                                                                                                                                                                                    | `0201-F7` B-009                                             | Active |
| B-007 | Given a claim tag on a Rule, this Feature records one claim located at the Rule, and the Rule's scenarios inheriting the tag are not further declarations.                                                                                                                                                                      | `0201-F7` B-010, OQ-3                                       | Active |
| B-008 | Given one claim identifier declared on two elements within one Feature identity, this Feature reports `SPEC030` at each declaration's path and line, its identifier the claim id.                                                                                                                                               | `0201-F7` B-012, OQ-4; brief § 4 `SPEC030`                  | Active |
| B-009 | Given a tag matching the claim tag form and the claim grammar on an Examples block, this Feature reports `SPEC026` at that tag's path and line and records no claim.                                                                                                                                                            | `0201-F7` B-028; OQ-5                                       | Active |
| B-010 | Given a Scenario or Scenario Outline in a claim file that carries no claim tag of its own, from its Rule or from its Feature, this Feature reports `SPEC025` at its keyword line, at warning by default.                                                                                                                        | `0201-F7` B-015, OQ-5; OQ-5                                 | Active |
| B-011 | Given a claim file whose scenarios are reordered or retitled, this Feature builds a catalogue equal in every claim's identifier, Feature identity and lifecycle to the one built before.                                                                                                                                        | `0201-F7` B-011                                             | Active |
| B-012 | Given a native claim whose declaring element carries no tag listed in `claim.withdrawnTags` or `claim.supersededTags`, this Feature gives it the status Active; given a withdrawn tag, Withdrawn; given a superseded tag, or both, Superseded.                                                                                  | `0201-F7` B-020, B-021, OQ-6                                | Active |
| B-013 | Given a Scenario, or an Examples block, carrying a tag listed in `claim.plannedTags`, this Feature keeps the claim it declares or inherits declared and marks that scenario, or that block's rows, planned in the catalogue.                                                                                                    | `0201-F7` B-016, B-024, B-026, OQ-7                         | Active |
| B-014 | Given a claim file that declares a `# language:` other than `en`, this Feature reports `SPEC027` at that line and records no claim from the file.                                                                                                                                                                               | `0201-F7` B-029, § 5 row 10                                 | Active |
| B-015 | Given a claim file with no `Feature:` line, this Feature reports `SPEC027` at line `0` and records no claim from the file.                                                                                                                                                                                                      | `0201-F7` B-014                                             | Active |
| B-016 | Given a claim file set whose glob matches no file outside `exclusions`, this Feature reports `SPEC027` at line `0`, the violation's path the set's glob as written.                                                                                                                                                             | `0201-F7` B-014; OQ-8                                       | Active |
| B-017 | Given two claim file sets naming one Feature identity, or a set naming the identity of a specification a layout discovers, this Feature reports `SPEC012` at each place the identity is declared.                                                                                                                               | `0201-F7` ADR-0001 item 7; brief § 4 `SPEC012`              | Active |
| B-018 | Given one `.feature` file matched by the globs of two claim file sets, this Feature reports `SPEC027` at that file, line `0`, and records its claims under neither set.                                                                                                                                                         | `0201-F7` ADR-0001 item 7                                   | Active |
| B-019 | Given `claim.scopes.featureTag` set and a claim file whose Feature-level tag captures a Feature identity other than the one its set names, this Feature reports `SPEC027` at that tag's line; a file with no Feature-level tag, or one that agrees, is not reported.                                                            | `0201-F7` B-023, OQ-2                                       | Active |
| B-020 | Given a claim file set with a `record`, `SPEC010` and `SPEC013` hold the record to every contracted section and table but the section `roles.claims` names, which is neither required nor read.                                                                                                                                 | OQ-4                                                        | Active |
| B-021 | Given a Feature a claim file set names, `SPEC020` and `SPEC021` report nothing for it: its claim files are its companions, and every claim tag in them declares.                                                                                                                                                                | brief § 4 `SPEC020`, `SPEC021`; B-005                       | Active |
| B-022 | Given a `.feature` file a claim file set's glob matches, `SPEC024` does not report it as a companion beside no specification.                                                                                                                                                                                                   | `0001-F6` B-013                                             | Active |
| B-023 | Given a claim file set with a `record` holding the section `roles.matrix` names, `SPEC031` holds that section to exactly one row per native claim, and the rules of `0001-F12` resolve each row's scenario against the set's claim files as they resolve a companion.                                                           | brief § 4 `SPEC031`; `0001-F12` A-1, B-002 to B-019; OQ-3   | Active |
| B-024 | Given a claim file set with no `record`, or a `record` without the section `roles.matrix` names, no rule of `0001-F12` and no `SPEC031` reports for that Feature, and `0001-F13` lists each of its claims with the status `none`.                                                                                               | `0001-F13` B-001; OQ-3                                      | Active |
| B-025 | Given a native claim, the scenarios carrying its tag are the declaring Scenario or Scenario Outline, or every Scenario and Scenario Outline under a declaring Rule, or every one in the file for a claim the Feature declares, and that set is the one `0001-F12` A-1 and `0001-F13` B-001 count.                               | `0201-F7` ADR-0001 item 6; `0001-F12` A-1; `0001-F13` B-001 | Active |
| B-026 | Given a Feature a claim file set names, the summary counts it among `features`, and the report lists it with its `record` path when the set has one and otherwise with the path of its first claim file in ordinal order.                                                                                                       | `0001-F3`; `0001-F6` B-008, B-009; OQ-7                     | Active |
| B-027 | Given a manifest that declares `identifiers.epic`, a claim file set's identity is its `epic` and `feature`, both required; given a manifest that declares no epic grammar, the identity is `feature` alone and a set with an `epic` is an invalid manifest, exit `3`.                                                           | `0201-F7` B-007; `0001-F5` B-042, C-5                       | Active |
| B-028 | Given a manifest that pins schema `0.2.0` or earlier, this Feature changes nothing: a `claim` member this Feature adds is a key that pin does not define and exits `3` as today, and a tree that passed under that pin passes unchanged.                                                                                        | `0001-F7` C-1, C-11; A-2, A-4                               | Active |
| B-029 | Given a manifest with `claim.files` absent, empty, or holding an entry without a `glob` or a `feature` while `claim.source` is `gherkin`, or with `claim.files` beside any other `claim.source`, the run exits `3` before any rule runs, naming the key.                                                                        | `0001-F5` C-5; `0201-F7` § 7 failure table                  | Active |
| B-030 | Given the same tree, this Feature gives byte-identical output: claim files in ordinal path order, claims in document order within a file, and tags in line order within an element.                                                                                                                                             | brief § 9; `0001-F6` B-008                                  | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-26 -->

| ID  | Constraint                                                                                                                                                                                   | Rules Out                                                                                                                               |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------- |
| C-1 | A claim tag has one reading in the check: the form-and-grammar rule the companion reader applies (`0001-F5` decision 0013), and the analyzer's reader gives the same answer (`0201-F7` C-1). | A second tag grammar for native claims; a candidate prefix; a claim read from a scenario title, a step or a file name.                  |
| C-2 | Claim files are read as text through `IFileSystem`, offline, and every path in a violation, the report or a fixture is relative to the root (brief § 9).                                     | A Gherkin runner started to list scenarios; a network call; an absolute path anywhere.                                                  |
| C-3 | A verdict this Feature adds is a new `SPEC###` id in a new schema version, and no shipped id changes meaning (`0001-F7` C-11; A-2).                                                          | A new rule under `0.2.0`; `SPEC021` or `SPEC030` reporting something under `0.3.0` it did not report under `0.2.0` for a Markdown tree. |
| C-4 | Every key, tag list and marker this Feature reads comes from the manifest, and a rule holds no such literal (`0001-F5` C-1).                                                                 | `"@ignore"` or `"gherkin"` written in a rule; a tag list compared to text the manifest did not supply.                                  |
| C-5 | One catalogue serves the claim rules, `0001-F12` and `0001-F13`: a native claim is the same record a § 3 claim becomes, with its source as provenance (`0001-F12` C-3).                      | A second parse of a claim file in the trace command; a rule that asks where a claim came from before judging it.                        |
| C-6 | No rule writes a file, and none proposes a rewrite beyond its `expected` (brief § 9).                                                                                                        | Writing a claims table, a record or a `.feature` file; a generated adapter of any kind.                                                 |
| C-7 | Every key this Feature reads is a top-level manifest key the checker owns, and no rule reads a key under `codeAnalysis` (ADR-0023).                                                          | Selecting native claim input under `codeAnalysis.records`; a Gherkin setting placed in the analyzer's section for its convenience.      |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-26 -->

| #   | Item                                                                                             | Exclusion Reason                                                                                                                                                      |
| --- | ------------------------------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | Any change to the analyzer's reading, diagnostics, policy or export of native claims.            | `0201-F7` owns them; this Feature gives the checker the same reading (A-3) and changes nothing the analyzer does.                                                     |
| 2   | How the analyzer and the engine share one reader: a project reference, a source link or a copy.  | A code-structure decision for an ADR under ADR-0014 and ADR-0017; `0201-F7` ADR-0001 item 3 holds the two readers to one answer by test until it is made (OQ-6).      |
| 3   | Test evidence from a runner that is not a .NET test framework, and `specht evidence` reading it. | Epic `0201` owns evidence; a language-neutral evidence adapter has no need recorded.                                                                                  |
| 4   | Binding a claim to code in a language the analyzer does not read.                                | Epic `0201` owns code bindings; the checker resolves text (C-2).                                                                                                      |
| 5   | Generating a Markdown claims table, a record or any adapter from Gherkin.                        | Gherkin stays the sole authored source; a generated copy is the duplication this Feature removes (C-6; `0201-F7` § 5 row 1).                                          |
| 6   | Inferring a claim from a scenario title, step text or file name.                                 | Only an explicit claim tag declares (C-1; `0201-F7` § 5 row 8).                                                                                                       |
| 7   | Reporting a mistyped claim tag, such as `@B-01` against a three-digit grammar.                   | An ordinary tag (B-005); the owner accepted that cost for the analyzer (`0201-F7` decision 0002) and the checker reads the same way (A-3).                            |
| 8   | Claim files in a Gherkin language other than English.                                            | `0201-F7` § 5 row 10; B-014 reports the header.                                                                                                                       |
| 9   | Carrying test names on scenarios, as tags or otherwise, so a record-less Feature has coverage.   | Not asked for; a `record` carries § 9 (B-023), and a Feature with none has no coverage to judge (B-024). Raised as OQ-3 for the owner.                                |
| 10  | How a § 9 row's scenario and test names resolve.                                                 | `0001-F12`; this Feature supplies the claims and the companions it resolves against (B-023, B-025).                                                                   |
| 11  | What `specht trace` prints for a native claim beyond its status `none` and its scenario count.   | `0001-F13`.                                                                                                                                                           |
| 12  | A template or `init` option for a native layout.                                                 | `0001-F4`; a native Feature needs no file `init` would write.                                                                                                         |
| 13  | Items, children and spikes for a native Feature.                                                 | `SPEC040` to `SPEC044` run on a `record` that lists them as on any specification; a record-less Feature lists none.                                                   |
| 14  | Adopting native claim input in this repository's own tree.                                       | Its specifications are Markdown by brief § 7; the self-check stays on the § 3 table.                                                                                  |
| 15  | Moving the analyzer's `codeAnalysis.records.*` claim settings to the key this Feature reads.     | `0201-F7` takes the specification delta ADR-0023 names; until it lands the analyzer reads its own keys and the two readers agree by its characterization test (OQ-6). |

## 6. Concern Separation

<!-- last written by: spec-author, 2026-10-26 -->

Pending: owned by `implementer`, written after agreement.

## 7. Technical Design

<!-- last written by: spec-author, 2026-10-26 -->

Pending: owned by `implementer`, written after agreement.

## 8. Testing Strategy

<!-- last written by: spec-author, 2026-10-26 -->

Pending: owned by `test-writer`, written after agreement.

## 9. Traceability Matrix

<!-- last written by: spec-author, 2026-10-26 -->

| Claim ID | Scenario                                                    | Test    | Status  |
| -------- | ----------------------------------------------------------- | ------- | ------- |
| B-001    | Claims come from the claim files a set names                | Missing | Missing |
| B-002    | Without the selection nothing changes                       | Missing | Missing |
| B-003    | A record supplies lifecycle and edges; none means current   | Missing | Missing |
| B-004    | A claim tag declares a claim at its element                 | Missing | Missing |
| B-005    | An ordinary tag declares nothing                            | Missing | Missing |
| B-006    | An outline declares one claim                               | Missing | Missing |
| B-007    | A Rule declares once for its scenarios                      | Missing | Missing |
| B-008    | One identifier on two elements is reported at both          | Missing | Missing |
| B-009    | A claim tag on an Examples block is reported                | Missing | Missing |
| B-010    | A scenario with no claim is reported                        | Missing | Missing |
| B-011    | Reordering and retitling keep every claim                   | Missing | Missing |
| B-012    | Lifecycle tags give the claim its status                    | Missing | Missing |
| B-013    | A planned scenario keeps its claim                          | Missing | Missing |
| B-014    | A claim file in another language is reported                | Missing | Missing |
| B-015    | A claim file with no Feature line is reported               | Missing | Missing |
| B-016    | A set matching no file is reported                          | Missing | Missing |
| B-017    | One identity declared twice is reported at both             | Missing | Missing |
| B-018    | A file two sets match is reported                           | Missing | Missing |
| B-019    | A Feature-level tag that disagrees with its set is reported | Missing | Missing |
| B-020    | A record needs every section but the claims table           | Missing | Missing |
| B-021    | The companion rules leave a native Feature alone            | Missing | Missing |
| B-022    | A claim file is not a stray companion                       | Missing | Missing |
| B-023    | A record's matrix is held to the native claims              | Missing | Missing |
| B-024    | Without a matrix the trace says none                        | Missing | Missing |
| B-025    | The scenarios carrying a claim                              | Missing | Missing |
| B-026    | A native Feature is counted and listed                      | Missing | Missing |
| B-027    | A set's identity follows the epic grammar                   | Missing | Missing |
| B-028    | An earlier pin is untouched                                 | Missing | Missing |
| B-029    | An invalid claim file set exits before any rule             | Missing | Missing |
| B-030    | The same tree gives the same output                         | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-26 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-26 -->

| ID   | Question                                                                                                                                                                      | Blocks                                          | Resolution                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         |
| ---- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | Which manifest key selects native claim input for the check: the analyzer's `codeAnalysis.records.claimSource` and `claimFileSets`, or a checker-level key beside `layouts`?  | B-001, B-002, B-029                             | RESOLVED, 2026-10-26, by the owner, in session: `codeAnalysis` is the manifest's one C#-specific section and holds nothing else; the tool serves every technology its text rules can read, so a setting the checker reads is a top-level key the checker owns, and the analyzer reads it there ([ADR-0023](../../../../../.spec/adr/0023-codeanalysis-is-the-only-csharp-specific-manifest-section.md), C-7). The key's name and shape are OQ-9.                                                                                                                                                                                                   |
| OQ-2 | Is the selection additive - specifications the layouts discover stay Markdown-checked beside the native Features the sets name - or does `gherkin` make every Feature native? | B-001, B-002, B-017                             | Open. Proposed default: additive. A layout-discovered specification is checked as today; a set's `record` is the one document whose claims table is not read (B-001, B-020); the same identity from both is `SPEC012` (B-017). Rejected by the default: a repository-wide switch that leaves a mixed tree unable to adopt one Feature at a time.                                                                                                                                                                                                                                                                                                   |
| OQ-3 | Where does § 9 live for a Feature with no `record`, and does a record-less Feature have coverage the check judges?                                                            | B-023, B-024; § 5 row 9                         | Open. Proposed default: a `record` carries § 9 and `SPEC031` and `0001-F12` judge it against the native claims (B-023); with no record there is no matrix, no matrix rule reports, and the trace lists each claim as `none` (B-024). Rejected by the default: test names carried as scenario tags, which would make the companion also the matrix and would need `0001-F12` to read a tag as a Test cell; recorded in § 5 row 9.                                                                                                                                                                                                                   |
| OQ-4 | Which section rules does a `record` obey: every contracted section but the claims table, every section, or none beyond frontmatter?                                           | B-003, B-020                                    | Open. Proposed default: every contracted section and table but the one `roles.claims` names (B-020), so a record is a specification whose § 3 moved into Gherkin and nothing else moved. Rejected by the default: a frontmatter-only record, which would leave § 9 and § 12 without a home; requiring an empty claims table, which is the duplication removed.                                                                                                                                                                                                                                                                                     |
| OQ-5 | Which ids and version carry the three new verdicts, and at what default level?                                                                                                | B-009, B-010, B-014 to B-019, B-028             | Open. Proposed default: `SPEC025` a scenario with no claim, at warning (B-010, as `0201-F7` OQ-5 chose for SPCT0012); `SPEC026` a claim tag where no claim can be declared, at error (B-009); `SPEC027` a claim file or set the check cannot read, at error (B-014 to B-016, B-018, B-019); all three in schema `0.3.0` (A-2, C-3), relocking nothing. Rejected by the default: adding to `0.2.0`, which has shipped.                                                                                                                                                                                                                              |
| OQ-6 | How does the analyzer come to share the checker's reader once it exists: a reference to the engine, a source link, or a kept copy held equal by the characterization test?    | None here; § 5 row 2                            | Open. Proposed default: no change in this Feature; `0201-F7` ADR-0001 item 3's parity test holds the two readers to one answer, and the implementer records the dependency form as an ADR when the engine's reader is built, under ADR-0014's gate. Rejected by the default: a source link, which PR 114 ruled out; a package reference the analyzer's dependency closure test would have to admit.                                                                                                                                                                                                                                                |
| OQ-7 | How is a native Feature named in the summary and the report: under which layout name, and by which path?                                                                      | B-026                                           | Open. Proposed default: counted among `features`, listed under a layout named `claims`, by its `record` path when it has one and otherwise by its first claim file in ordinal order (B-026). Rejected by the default: listing the Feature by its glob, which is not a path the reader can open.                                                                                                                                                                                                                                                                                                                                                    |
| OQ-8 | When a claim file set's glob matches no file, what path does the violation carry, or is it an invalid manifest?                                                               | B-016, B-029                                    | Open. Proposed default: a violation, `SPEC027` at line `0` with the glob as written for its path (B-016), since the manifest is well-formed and the tree is what is missing; an entry without `glob` or `feature` stays exit `3` (B-029). Rejected by the default: exit `3`, which would end the run on one Feature's missing files and report nothing else.                                                                                                                                                                                                                                                                                       |
| OQ-9 | What are the name and shape of the top-level key that carries the settings `0201-F7` put under `codeAnalysis.records`?                                                        | B-001, B-002, B-012, B-013, B-019, B-027, B-029 | Open. Proposed default: the existing `claim` object grows the members `source` (`markdown`, the default, or `gherkin`), `files` (a list of `{glob, feature, epic?, record?}`), `plannedTags` (default `["@ignore"]`), `withdrawnTags` and `supersededTags` (default empty), `lifecycleKey` (default `spec_status`) and `scopes` with `featureTag` and `epicTag`, each missing member filled from the pinned manifest as `claim.tag` is today; `0201-F7`'s delta renames its keys to these. Rejected by the default: a new top-level `claims` one letter from `claim`; keeping the analyzer's names under `codeAnalysis`, which ADR-0023 rules out. |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-26 -->

| Section | Status | Reviewer      | Note                                                                         |
| ------- | ------ | ------------- | ---------------------------------------------------------------------------- |
| 1-5, 11 | 🟡     | spec-reviewer | Draft awaiting the owner's answers to OQ-2 to OQ-9 and a first review round. |

## Tasks

None cut; the agreement is awaiting review and the owner's answers to § 11.

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
