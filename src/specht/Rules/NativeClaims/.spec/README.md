---
title: "Specification: Native Gherkin claims"
description: "Under a manifest that selects native claim input, the check builds a Feature's claims from the tags of its .feature files instead of a Markdown claims table, so a repository with no specification document per Feature is checked by the same rules, with the untagged scenario, the misplaced claim tag and the unreadable claim file reported as new rules of the next schema version"
type: feature
id: "F16"
epic: "0001"
spec_status: in-review
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
updated: "2026-11-14"
github_issue: null
synced_at: null
---

# Specification: Native Gherkin claims

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-26 -->

A repository whose claims are authored only as tagged scenarios cannot be checked today: the check builds a Feature's claims from the § 3 table of a specification document, so a consumer with no such document per Feature, such as an application with no .NET code whose agreement is its `.feature` files, either duplicates every claim into a Markdown table or gets no check at all. The analyzer already reads such a claim catalogue (`0201-F7`) and names the checker-side reading as epic `0001`'s (`0201-F7` § 5 row 7). This Feature removes that failure state: under a manifest that selects native claim input, the check builds the Feature's claims from the tags of the `.feature` files a claim file set names, with the same identities and lifecycle the analyzer gives them, applies the claim rules to that catalogue, and reports a scenario with no claim, a claim tag where none can be declared and a claim file it cannot read, as new rules of the next schema version.

## 2. User Needs

<!-- last written by: spec-author, 2026-11-13 (decision 0002: A-4 amended, the claim members unread below schema 0.4.0) -->

| #   | Persona                                          | Need                                                                                            | Pain Point Today                                                                            |
| --- | ------------------------------------------------ | ----------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------- |
| 1   | The maintainer of a repository with no .NET code | Claims checked for grammar, uniqueness and coverage while `.feature` files stay the only source | Every claim is copied into a Markdown table, or the tree is not checked                     |
| 2   | The maintainer of a central specification tree   | One claim file set per Feature, with no specification document and no epic                      | Discovery assumes a document found through the layouts, with an epic and a Feature id in it |
| 3   | A consumer that also runs the analyzer           | One manifest that both the checker and the analyzer read the same way                           | The analyzer's claim catalogue and the checker's disagree on what a claim is                |
| 4   | `0001-F12` and `0001-F13`                        | A claim catalogue to resolve § 9 rows and the trace against, whatever its source                | N/A (internal dependency)                                                                   |

### Assumptions

| ID  | Assumption                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| A-1 | A claim tag is the manifest's `claim.tag` form, honouring `claim.tagIgnoreCase`, whose captured text matches the claim grammar, which from schema `0.4.0` the `ids.claim` record gives (`0001-F5` C-14); the check already reads companion tags that way (`0001-F5` decision 0013), and the analyzer reads native claims that way (`0201-F7` C-1, decision 0002). Amended 2026-11-12 (spec-author, on the owner's selection "Fold into 0.4.0"; [decision 0001](decisions/0001-native-claims-fold-into-schema-0-4-0.md)): `0.4.0` rejects `identifiers.claim` (`0001-F5` B-092). Was: whose captured text matches `identifiers.claim`.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  |
| A-2 | Schema `0.3.1` has gone out in NuGet packages (`specht.tool` `0.5.35-preview`, `0.6.1-preview` and `0.6.4-preview`, checked 2026-11-12), so this Feature's `claim` members and verdicts land in schema `0.4.0`, cut by item `0436`, on the owner's selection "Fold into 0.4.0" ([decision 0001](decisions/0001-native-claims-fold-into-schema-0-4-0.md)). Spec-author's reading: `0.4.0` is a `0.minor` step, which before `1.0.0` may add a rule id (`0001-F7` C-11), so neither C-11's opt-in clause nor `0001-F7` decision 0022 is the basis, and `0001-F7` B-154 does not compare `0.3.1` and `0.4.0` (`0001-F7` OQ-78). Amended 2026-11-12 (the owner's selection; decision 0001). Was: schema `0.3.0` is published (the owner, 2026-11-06), so this Feature's `claim` members and verdicts land in a new schema version, `0.3.1`; it is a patch, because each verdict reports only when a manifest sets `claim.source` to `gherkin`, a key no earlier version defines (`0001-F7` C-11, decision 0022). Amended 2026-11-06 (the owner, OQ-5). Was: schema `0.2.0` has shipped in a published package, so a new `SPEC###` id lands in a new schema version, `0.3.0` (`0001-F7` C-11; brief § 4 already names `0.3.0` for `0001-F7` decision 0015). |
| A-3 | The element, identity, lifecycle and planned semantics of a native claim are the ones `0201-F7` B-003 to B-012 and B-020 to B-029 state and its ADR-0001 fixes; this Feature gives the checker the same reading and decides nothing the analyzer decided differently.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  |
| A-4 | Under every schema version up to `0.3.1` the check reads two `claim` members, `tag` and `tagIgnoreCase`, and leaves every other `claim` member unread at every pin. So a member this Feature adds under `0.4.0` goes unread under every earlier pin, as today, and changes no earlier pin's verdict (B-028). Amended 2026-11-06 (OQ-5). Was: under schema `0.2.0`, with a member added under `0.3.0`. Amended 2026-11-12 (the owner's selection "Fold into 0.4.0"; [decision 0001](decisions/0001-native-claims-fold-into-schema-0-4-0.md)). Was: up to `0.3.0`, with a member added under `0.3.1`. Amended 2026-11-13 (the owner's selection "Ignore below 0.4.0"; [decision 0002](decisions/0002-claim-members-go-unread-below-schema-0-4-0.md); that no pin refuses such a member today is spec-author's reading of the parser). Was: the `claim` object has two members, and a key a pin does not define exits `3` (`docs/configuration.md`), so a member this Feature adds is refused by every earlier pin as today.                                                                                                                                                                                                                              |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-11-14 (B-015 widened, on the owner's selection for OQ-19); before: spec-author, 2026-11-14 (item `0394`: B-009 names its identifier, the claim id); before: spec-author, 2026-11-14 (decision 0004: B-009, B-010, B-014 to B-016, B-018, B-019, B-033 and B-034 amended to name `SPEC4011` to `SPEC4013`); before: spec-author, 2026-11-13 (item 0391: OQ-11 to OQ-18 resolved by the owner's selections; B-017 and B-026 amended; B-033 to B-038 added); before: spec-author, 2026-11-13 (item 0391: B-032 added) -->

| ID    | Claim                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     | Source                                                                                                                                                         | Status  |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------- |
| B-001 | Given a manifest that pins schema `0.4.0` or later and sets `claim.source` to `gherkin`, for each entry of `claim.files`, a claim file set, this Feature builds the claims of the Feature identity the entry names from the `.feature` files its glob matches outside `exclusions`, and reads no claims table for that Feature. Amended 2026-11-06 (the owner, OQ-5): was `0.3.0` or later. Amended 2026-11-12 (the owner's selection "Fold into 0.4.0"; [decision 0001](decisions/0001-native-claims-fold-into-schema-0-4-0.md)): was `0.3.1` or later.                                                                                                                                                                                                                                                                                                                  | `0201-F7` B-001; ADR-0023; OQ-1, OQ-2, OQ-5, OQ-9                                                                                                              | Amended |
| B-002 | Given a manifest with no `claim.source`, or `claim.source` of `markdown`, and no `claim.files`, this Feature changes no verdict: every claim comes from the § 3 table as today, and no rule reads the manifest's `codeAnalysis` section. Amended 2026-11-13 (spec-author's reading, on test-writer's question; not an owner answer): was without "and no `claim.files`", which B-029 already makes exit `3`.                                                                                                                                                                                                                                                                                                                                                                                                                                                              | `0201-F7` B-002; `0001-F5` C-4; ADR-0023; B-029                                                                                                                | Amended |
| B-003 | Given a claim file set with a `record`, this Feature reads the Feature's frontmatter, lifecycle and dependency edges from that document, and its frontmatter is held to the Feature schema by `SPEC1001` to `SPEC1003`; given a set with no `record`, the Feature's lifecycle is current and it declares no edge.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         | `0201-F7` B-005, B-022, ADR-0001 item 8; OQ-4                                                                                                                  | Active  |
| B-004 | Given a tag on a Feature, a Rule, a Scenario or a Scenario Outline that matches the claim tag form and whose captured text matches the claim grammar, this Feature records one claim whose identifier is that captured text exactly as authored, located at the element's keyword line.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   | `0201-F7` B-004, B-008; A-1                                                                                                                                    | Active  |
| B-005 | Given a tag that does not match the claim tag form, or whose captured text fails the claim grammar, this Feature treats it as an ordinary tag that declares no claim and draws no violation.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              | `0201-F7` B-025, decision 0002                                                                                                                                 | Active  |
| B-006 | Given a claim tag on a Scenario Outline with several Examples rows, this Feature records exactly one claim, located at the Scenario Outline.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              | `0201-F7` B-009                                                                                                                                                | Active  |
| B-007 | Given a claim tag on a Rule, this Feature records one claim located at the Rule, and the Rule's scenarios inheriting the tag are not further declarations.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                | `0201-F7` B-010, OQ-3                                                                                                                                          | Active  |
| B-008 | Given one claim identifier declared on two elements within one Feature identity, this Feature reports `SPEC4003` at each declaration's path and line, its identifier the claim id.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        | `0201-F7` B-012, OQ-4; brief § 4 `SPEC4003`                                                                                                                    | Active  |
| B-009 | Given a tag matching the claim tag form and the claim grammar on an Examples block, this Feature reports `SPEC4012` at that tag's path and line, its identifier the claim id, and records no claim. Amended 2026-11-14 (decision 0004): was `SPEC026`. Amended 2026-11-14 (item `0394`, its scenario corrected): the identifier added, as the scenario named the claim; was "at that tag's path and line and records no claim".                                                                                                                                                                                                                                                                                                                                                                                                                                           | `0201-F7` B-028; OQ-5; decision 0004; the identifier spec-author's reading of the scenario, not an owner answer                                                | Amended |
| B-010 | Given a Scenario or Scenario Outline in a claim file that carries no claim tag of its own, from its Rule or from its Feature, this Feature reports `SPEC4011` at its keyword line, at warning by default. Amended 2026-11-14 (decision 0004): was `SPEC025`.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              | `0201-F7` B-015, OQ-5; OQ-5; decision 0004                                                                                                                     | Amended |
| B-011 | Given a claim file whose scenarios are reordered or retitled, this Feature builds a catalogue equal in every claim's identifier, Feature identity and lifecycle to the one built before.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  | `0201-F7` B-011                                                                                                                                                | Active  |
| B-012 | Given a native claim whose declaring element carries no tag listed in `claim.withdrawnTags` or `claim.supersededTags`, this Feature gives it the status Active; given a withdrawn tag, Withdrawn; given a superseded tag, or both, Superseded.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            | `0201-F7` B-020, B-021, OQ-6                                                                                                                                   | Active  |
| B-013 | Given a Scenario, or an Examples block, carrying a tag listed in `claim.plannedTags`, this Feature keeps the claim it declares or inherits declared and marks that scenario, or that block's rows, planned in the catalogue.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              | `0201-F7` B-016, B-024, B-026, OQ-7                                                                                                                            | Active  |
| B-014 | Given a claim file that declares a `# language:` other than `en`, this Feature reports `SPEC4013` at that line and records no claim from the file. Amended 2026-11-14 (decision 0004): was `SPEC027`.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     | `0201-F7` B-029, § 5 row 10; decision 0004                                                                                                                     | Amended |
| B-015 | Given a claim file whose text the reader refuses for any cause but its `# language:` header (B-014): no `Feature:` line, a second `Feature:` line, a ragged table, a malformed tag token, an unterminated doc string, tags followed by no element, or an `Examples:` with no outline above it, this Feature reports `SPEC4013` at line `0` and records no claim from the file. Amended 2026-11-14 (decision 0004): was `SPEC027`. Amended 2026-11-14 (the owner's selection "(a) All as SPEC4013 (Recommended)", OQ-19; the list of causes spec-author's, from the reader): was "Given a claim file with no `Feature:` line".                                                                                                                                                                                                                                             | `0201-F7` B-014; decision 0004; OQ-19                                                                                                                          | Amended |
| B-016 | Given a claim file set whose glob matches no file outside `exclusions`, this Feature reports `SPEC4013` at line `0`, the violation's path the set's glob as written. Amended 2026-11-14 (decision 0004): was `SPEC027`.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   | `0201-F7` B-014; OQ-8; decision 0004                                                                                                                           | Amended |
| B-017 | Given two claim file sets naming one Feature identity, or a set naming the identity of a specification a layout discovers, this Feature reports `SPEC3002` at each place the identity is declared: a layout's specification where `SPEC3002` reports one today, and a set at its native Feature's path (B-026), line `0`. Amended 2026-11-13 (the owner's selection "(a) Feature path, line 0 (Recommended)", OQ-12; the wording spec-author's): was without the clause after the colon.                                                                                                                                                                                                                                                                                                                                                                                  | `0201-F7` ADR-0001 item 7; brief § 4 `SPEC3002`; OQ-12                                                                                                         | Amended |
| B-018 | Given one `.feature` file matched by the globs of two claim file sets, this Feature reports `SPEC4013` at that file, line `0`, and records its claims under neither set. Amended 2026-11-14 (decision 0004): was `SPEC027`.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               | `0201-F7` ADR-0001 item 7; decision 0004                                                                                                                       | Amended |
| B-019 | Given `claim.scopes.featureTag` set and a claim file whose Feature-level tag captures a Feature identity other than the one its set names, this Feature reports `SPEC4013` at that tag's line; a file with no Feature-level tag, or one that agrees, is not reported. Amended 2026-11-14 (decision 0004): was `SPEC027`.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  | `0201-F7` B-023, OQ-2; decision 0004                                                                                                                           | Amended |
| B-020 | Given a claim file set with a `record`, `SPEC2001` and `SPEC2002` hold the record to every contracted section and table but the section `roles.claims` names, which is neither required nor read.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         | OQ-4                                                                                                                                                           | Active  |
| B-021 | Given a Feature a claim file set names, `SPEC4001` and `SPEC4002` report nothing for it: its claim files are its companions, and every claim tag in them declares.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        | brief § 4 `SPEC4001`, `SPEC4002`; B-005                                                                                                                        | Active  |
| B-022 | Given a `.feature` file a claim file set's glob matches, `SPEC024` does not report it as a companion beside no specification.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             | `0001-F6` B-013                                                                                                                                                | Active  |
| B-023 | Given a claim file set with a `record` holding the section `roles.matrix` names, `SPEC4004` holds that section to exactly one row per native claim, and the rules of `0001-F12` resolve each row's scenario against the set's claim files as they resolve a companion.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    | brief § 4 `SPEC4004`; `0001-F12` A-1, B-002 to B-019; OQ-3                                                                                                     | Active  |
| B-024 | Given a claim file set with no `record`, or a `record` without the section `roles.matrix` names, no rule of `0001-F12` and no `SPEC4004` reports for that Feature, and `0001-F13` lists each of its claims with the status `none`.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        | `0001-F13` B-001; OQ-3                                                                                                                                         | Active  |
| B-025 | Given a native claim, the scenarios carrying its tag are the declaring Scenario or Scenario Outline, or every Scenario and Scenario Outline under a declaring Rule, or every one in the file for a claim the Feature declares, and that set is the one `0001-F12` A-1 and `0001-F13` B-001 count.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         | `0201-F7` ADR-0001 item 6; `0001-F12` A-1; `0001-F13` B-001                                                                                                    | Active  |
| B-026 | Given a Feature a claim file set names, the summary and the report's `layouts` count it in one entry named `claims`, after the manifest's layouts and beside any manifest layout of that name, and every violation on it carries its `record` path when the set has one and otherwise the path of its first claim file in ordinal order. Amended 2026-11-13 (the owner's selections "(a) claims layout entry (Recommended)", OQ-11, and "(a) Two claims entries (Recommended)", OQ-16; the wording spec-author's): was the summary counts it among `features`, and the report lists it with its `record` path when the set has one and otherwise with the path of its first claim file in ordinal order.                                                                                                                                                                  | `0001-F3`; `0001-F6` B-008, B-009; OQ-7, OQ-11, OQ-16                                                                                                          | Amended |
| B-027 | Given a manifest that declares `identifiers.epic`, a claim file set's identity is its `epic` and `feature`, both required, and a set without an `epic` is an invalid manifest, exit `3`; given a manifest that declares no epic grammar, the identity is `feature` alone and a set with an `epic` is an invalid manifest, exit `3`. Amended 2026-11-13 (spec-author's reading of "both required", on test-writer's question; not an owner answer): was without "and a set without an `epic` is an invalid manifest, exit `3`".                                                                                                                                                                                                                                                                                                                                            | `0201-F7` B-007; `0001-F5` B-042, C-5                                                                                                                          | Amended |
| B-028 | Given a manifest that pins a schema version below `0.4.0`, this Feature changes nothing: a `claim` member this Feature adds goes unread, as today, and the check gives the same output with it as without it. Amended 2026-11-06 (the owner, OQ-5): was `0.2.0` or earlier. Amended 2026-11-12 (the owner's selection "Fold into 0.4.0"; [decision 0001](decisions/0001-native-claims-fold-into-schema-0-4-0.md)): was `0.3.0` or earlier; below `0.4.0` is `0.3.1` and every earlier version. Amended 2026-11-13 (the owner's selection "Ignore below 0.4.0"; [decision 0002](decisions/0002-claim-members-go-unread-below-schema-0-4-0.md)): was a `claim` member this Feature adds is a key that pin does not define and exits `3` as today, and a tree that passed under that pin passes unchanged; that no pin refuses such a member today is spec-author's reading. | `0001-F7` C-1, C-4, C-11; A-2, A-4; OQ-5; decision 0002                                                                                                        | Amended |
| B-029 | Given a manifest with `claim.files` absent, empty, or holding an entry without a `glob` or a `feature` while `claim.source` is `gherkin`, or with `claim.files` beside any other `claim.source`, an omitted one included, the run exits `3` before any rule runs, naming the key. Amended 2026-11-13 (spec-author's reading, on test-writer's question; not an owner answer): was without "an omitted one included"; an omitted `claim.source` reads as `markdown` (OQ-9).                                                                                                                                                                                                                                                                                                                                                                                                | `0001-F5` C-5; `0201-F7` § 7 failure table; OQ-9                                                                                                               | Amended |
| B-030 | Given the same tree, this Feature gives byte-identical output: claim files in ordinal path order, claims in document order within a file, and tags in line order within an element.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       | brief § 9; `0001-F6` B-008                                                                                                                                     | Active  |
| B-031 | Given a manifest that pins schema `0.4.0` or later and sets `claim.source` to any value but the text `markdown` or `gherkin`, the run exits `3` before any rule runs, naming `claim.source`.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              | Owner, 2026-11-13; [decision 0003](decisions/0003-a-claim-source-outside-markdown-and-gherkin-is-an-invalid-manifest.md); `0001-F7` B-056; `0001-F5` C-5; OQ-9 | Active  |
| B-032 | Given a claim file set whose `record` declares a `claim_prefix`, this Feature reads the Feature's claim tags with the claim grammar `0001-F5` A-8 gives a Feature that declares no prefix, not the grammar that prefix gives.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             | The owner's selection "(b) No, record is lifecycle", 2026-11-13, relayed; § 10; B-003; C-1; `0001-F5` A-8, B-086                                               | Active  |
| B-033 | Given a claim file set whose `record` is a relative path that names no file, names a file the check cannot read, or leads outside the root, this Feature reports `SPEC4013` at that path as written, line `0`. Amended 2026-11-14 (decision 0004): was `SPEC027`.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         | The owner's selection "(a) SPEC027, record-less (Recommended)", 2026-11-13; OQ-13; B-014; C-2; decision 0004                                                   | Amended |
| B-034 | Given a claim file set whose `record` is an absolute path, this Feature reports `SPEC4013` at the manifest's path, `.specht/manifest.json`, line `0`. Amended 2026-11-14 (decision 0004): was `SPEC027`.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  | The owner's selection "At manifest, line 0 (Recommended)", 2026-11-13; OQ-17; OQ-13; C-2; brief § 9; decision 0004                                             | Amended |
| B-035 | Given a claim file set whose `record` names no file, names a file the check cannot read, is an absolute path or leads outside the root, this Feature reads the set as it reads one with no `record` (B-003, B-026).                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       | The owner's selections "(a) SPEC027, record-less (Recommended)" and "At manifest, line 0 (Recommended)", 2026-11-13; OQ-13, OQ-17; B-003; B-026                | Active  |
| B-036 | Given a claim file set with no `record` whose glob matches no `.feature` file outside `exclusions`, this Feature makes no Feature of it, so the `claims` entry does not count it and `SPEC3002` does not compare its identity.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            | The owner's selection "(a) Not a Feature (Recommended)", 2026-11-13; OQ-14; B-016; B-026; OQ-7                                                                 | Active  |
| B-037 | Given a claim file set whose `record` declares an `id` or an `epic` other than the set's `feature` or `epic`, the native Feature's identity is the set's.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 | The owner's selection "(a) Silent, set wins (Recommended)", 2026-11-13; OQ-15; B-001; B-027; § 5 row 16                                                        | Active  |
| B-038 | Given a manifest that pins schema `0.4.0` or later and a claim file set whose `feature` fails the manifest's feature grammar, or whose `epic` fails its epic grammar, the run exits `3` before any rule runs, naming `claim.files`.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       | The owner's selection "Must match; exit 3 (Recommended)", 2026-11-13; OQ-18; B-027; B-029; `0001-F5` C-5                                                       | Active  |

## 4. Constraints

<!-- last written by: spec-author, 2026-11-12 (decision 0001: C-3 amended to schema 0.4.0) -->

| ID  | Constraint                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   | Rules Out                                                                                                                                                                                                                                                           |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| C-1 | A claim tag has one reading in the check: the form-and-grammar rule the companion reader applies (`0001-F5` decision 0013), and the analyzer's reader gives the same answer (`0201-F7` C-1).                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 | A second tag grammar for native claims; a candidate prefix; a claim read from a scenario title, a step or a file name.                                                                                                                                              |
| C-2 | Claim files are read as text through `IFileSystem`, offline, and every path in a violation, the report or a fixture is relative to the root (brief § 9).                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     | A Gherkin runner started to list scenarios; a network call; an absolute path anywhere.                                                                                                                                                                              |
| C-3 | A verdict this Feature adds is a new rule id, four digits in its group (ADR-0029), in schema `0.4.0`, and no shipped id changes meaning (`0001-F7` C-11; A-2). Amended 2026-11-02 (ADR-0029): was a new `SPEC###` id; only the id's form changed, and which version takes the verdicts stays OQ-5's. Amended 2026-11-06 (the owner, OQ-5; `0001-F7` decision 0022): was in a new schema version. Amended 2026-11-12 (the owner's selection "Fold into 0.4.0"; [decision 0001](decisions/0001-native-claims-fold-into-schema-0-4-0.md)): was in schema `0.3.1`; Rules Out was a new rule or `claim` member under `0.3.0` or earlier, and `SPEC4002` or `SPEC4003` reporting something under `0.3.1` that it did not report under `0.3.0` for a Markdown tree. | A new rule or `claim` member under a version below `0.4.0`; a verdict of this Feature reached without `claim.source` set to `gherkin`; `SPEC4002` or `SPEC4003` reporting for a Markdown tree under `0.4.0` anything they did not report there before this Feature. |
| C-4 | Every key, tag list and marker this Feature reads comes from the manifest, and a rule holds no such literal (`0001-F5` C-1).                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 | `"@ignore"` or `"gherkin"` written in a rule; a tag list compared to text the manifest did not supply.                                                                                                                                                              |
| C-5 | One catalogue serves the claim rules, `0001-F12` and `0001-F13`: a native claim is the same record a § 3 claim becomes, with its source as provenance (`0001-F12` C-3).                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      | A second parse of a claim file in the trace command; a rule that asks where a claim came from before judging it.                                                                                                                                                    |
| C-6 | No rule writes a file, and none proposes a rewrite beyond its `expected` (brief § 9).                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        | Writing a claims table, a record or a `.feature` file; a generated adapter of any kind.                                                                                                                                                                             |
| C-7 | Every key this Feature reads is a top-level manifest key the checker owns, and no rule reads a key under `codeAnalysis` (ADR-0023).                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          | Selecting native claim input under `codeAnalysis.records`; a Gherkin setting placed in the analyzer's section for its convenience.                                                                                                                                  |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-11-13 (row 16, on the owner's selection for OQ-15) -->

| #   | Item                                                                                             | Exclusion Reason                                                                                                                                                       |
| --- | ------------------------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | Any change to the analyzer's reading, diagnostics, policy or export of native claims.            | `0201-F7` owns them; this Feature gives the checker the same reading (A-3) and changes nothing the analyzer does.                                                      |
| 2   | How the analyzer and the engine share one reader: a project reference, a source link or a copy.  | A code-structure decision for an ADR under ADR-0014 and ADR-0017; `0201-F7` ADR-0001 item 3 holds the two readers to one answer by test until it is made (OQ-6).       |
| 3   | Test evidence from a runner that is not a .NET test framework, and `specht evidence` reading it. | Epic `0201` owns evidence; a language-neutral evidence adapter has no need recorded.                                                                                   |
| 4   | Binding a claim to code in a language the analyzer does not read.                                | Epic `0201` owns code bindings; the checker resolves text (C-2).                                                                                                       |
| 5   | Generating a Markdown claims table, a record or any adapter from Gherkin.                        | Gherkin stays the sole authored source; a generated copy is the duplication this Feature removes (C-6; `0201-F7` § 5 row 1).                                           |
| 6   | Inferring a claim from a scenario title, step text or file name.                                 | Only an explicit claim tag declares (C-1; `0201-F7` § 5 row 8).                                                                                                        |
| 7   | Reporting a mistyped claim tag, such as `@B-01` against a three-digit grammar.                   | An ordinary tag (B-005); the owner accepted that cost for the analyzer (`0201-F7` decision 0002) and the checker reads the same way (A-3).                             |
| 8   | Claim files in a Gherkin language other than English.                                            | `0201-F7` § 5 row 10; B-014 reports the header.                                                                                                                        |
| 9   | Carrying test names on scenarios, as tags or otherwise, so a record-less Feature has coverage.   | Not asked for; a `record` carries § 9 (B-023), and a Feature with none has no coverage to judge (B-024). Raised as OQ-3 for the owner.                                 |
| 10  | How a § 9 row's scenario and test names resolve.                                                 | `0001-F12`; this Feature supplies the claims and the companions it resolves against (B-023, B-025).                                                                    |
| 11  | What `specht trace` prints for a native claim beyond its status `none` and its scenario count.   | `0001-F13`.                                                                                                                                                            |
| 12  | A template or `init` option for a native layout.                                                 | `0001-F4`; a native Feature needs no file `init` would write.                                                                                                          |
| 13  | Items, children and spikes for a native Feature.                                                 | `SPEC040` to `SPEC044` run on a `record` that lists them as on any specification; a record-less Feature lists none.                                                    |
| 14  | Adopting native claim input in this repository's own tree.                                       | Its specifications are Markdown by brief § 7; the self-check stays on the § 3 table.                                                                                   |
| 15  | Moving the analyzer's `codeAnalysis.records.*` claim settings to the key this Feature reads.     | `0201-F7` takes the specification delta ADR-0023 names; until it lands the analyzer reads its own keys and the two readers agree by its characterization test (OQ-6).  |
| 16  | Reporting a `record` whose own `id` or `epic` differs from its set's.                            | The owner selected "(a) Silent, set wins (Recommended)" (OQ-15): the set's identity holds (B-037), and `SPEC1002` holds the record's keys to the Feature schema alone. |

## 6. Concern Separation

<!-- last written by: implementer, 2026-11-14 (item 0395 review round 1: the record faults reach `Build` as an argument); before: implementer, 2026-11-14 (item 0395 designed: SPEC4013, an unreadable claim file, set or record); before: implementer, 2026-11-14 (item 0394 built as designed); before: implementer, 2026-11-14 (item 0394 designed: SPEC4012, a claim tag on an Examples block); before: implementer, 2026-11-14 (item 0393 built as designed); before: implementer, 2026-11-14 (item 0393 designed: SPEC4011, a scenario in a claim file with no claim); before: implementer, 2026-11-14 (item 0440: a set's claim files listed by `FindClaimFiles`); before: implementer, 2026-11-14 (item 0391 review round 2: SPEC4010 skipped; ten rules); before: implementer, 2026-11-14 (item 0391 review round 1: the matrix resolver's guard and the facts record skip); before: implementer, 2026-11-14 (item 0391 built: the record, identity and report rows); before: implementer, 2026-11-13 (item 0391 designed, round 2: OQ-11 to OQ-18, B-035's reading, B-036 to B-038) -->

Item `0389` designs the `claim` members and their validation. Items `0390` to `0396` each add their rows below.

| Concern                                                                                                                                                             | Classification | Where                                                                                                                                                                                                                              |
| ------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Which `claim` members a pin reads: the seven of OQ-9 from schema `0.4.0`; below it `tag` and `tagIgnoreCase` alone, every other member unread (B-028; § 7)          | Business       | `SpechtManifestParser.Parse`, gated by `ClaimMembersVersion`                                                                                                                                                                       |
| A member left out reads as the pinned version's shipped manifest's (OQ-9; `0001-F5` B-019)                                                                          | Business       | `SpechtManifestParser.Selected` over the shipped `claim` object, the way `claim.tag` is filled                                                                                                                                     |
| No `claim.source`, or `markdown`, changes no verdict (B-002)                                                                                                        | Business       | `SpechtClaimInput.Source` is `Markdown`; no rule reads the record until item `0390`                                                                                                                                                |
| A claim file set's members and its identity under the epic grammar (B-027, B-029)                                                                                   | Business       | `SpechtManifestParser.ClaimInput`, over the `DeclaresEpicGrammar` reading                                                                                                                                                          |
| An invalid claim file set exits `3`, naming the key, before any rule runs (B-029; `0001-F5` B-018, C-5)                                                             | Both           | The faults join `SpechtManifestParser.Check`'s list; `SpechtModelReader.Read` reads the manifest before discovery; each command folds `SpechtManifestException` to exit `3`                                                        |
| A `claim.source` outside `markdown` and `gherkin` exits `3`, naming `claim.source` (B-031; decision 0003)                                                           | Both           | `SpechtManifestParser.ClaimInput` adds the fault to the same list                                                                                                                                                                  |
| Every member is a top-level key the checker owns; nothing is read under `codeAnalysis` (C-4, C-7; ADR-0023)                                                         | Business       | The parser reads `manifest["claim"]` alone; `codeAnalysis` stays in `KnownKeys` and unread                                                                                                                                         |
| A member of the wrong JSON type outside a claim file set takes the path every key of the wrong type takes                                                           | Technical      | `SpechtManifestUnreadableException`, exit `3` (`0001-F2` B-007)                                                                                                                                                                    |
| `specht manifest` prints the filled members (`0001-F9` B-004)                                                                                                       | Technical      | The existing `claimDocument` projection                                                                                                                                                                                            |
| The shipped defaults join `0.4.0` with a relock while no published package carries it (`0001-F7` C-1, C-17; decision 0001)                                          | Technical      | `src/tool/manifest/v0.4.0/manifest.json`, `./build.sh LockSchemaVersion`                                                                                                                                                           |
| The native reading runs only under `claim.source` `gherkin`; otherwise the listing, the texts and the model are today's (B-001, B-002, C-3)                         | Business       | `SpechtModelReader.Read` and `SpechtModelBuilder.Build`, on `ClaimInput.Source`                                                                                                                                                    |
| A set's claim files: the `.feature` files its glob matches outside `exclusions`, root-relative, in ordinal order (B-001, B-030)                                     | Both           | `ISpecDiscovery.FindClaimFiles` once per set, onto `SpechtListing.ClaimFiles`                                                                                                                                                      |
| Claim files are read as text through `IFileSystem`, each once (C-2)                                                                                                 | Technical      | `SpechtTreeStore.Load`, with every other listed path                                                                                                                                                                               |
| One tag reading: `claim.tag`, `tagIgnoreCase`, the Feature's claim grammar and `0001-F5` B-095's scenario head (C-1, A-1, B-004, B-005)                             | Business       | The `GherkinClaimTagForm` `SpechtModelBuilder` composes                                                                                                                                                                            |
| What declares, an outline once, a Rule once, inheritance (B-004, B-006, B-007; A-3)                                                                                 | Business       | `GherkinClaimTextReader`, the reader the analyzer compiles, unchanged in what it reads                                                                                                                                             |
| Lifecycle from the declaring element's tags, planned cases from the tags in effect, every list from the manifest (B-012, B-013, C-4)                                | Business       | The reader's `State` and `Cases`, over `ClaimInput`'s three tag lists                                                                                                                                                              |
| The scenarios carrying a claim (B-025)                                                                                                                              | Business       | `GherkinClaimDeclaration.Scenarios`                                                                                                                                                                                                |
| A native claim is the record a § 3 row becomes, its source as provenance (C-5)                                                                                      | Both           | `SpechtClaim.Path` and `SpechtClaim.Declaration`; `SpechtClaims`' set constructor; `SpechtModel.ClaimSets`                                                                                                                         |
| A claim file the reader cannot read declares nothing, and the run goes on                                                                                           | Technical      | `GherkinClaimTextException` caught per file in `SpechtModelBuilder.Build`; item `0395` reports it                                                                                                                                  |
| The same tree gives the same catalogue (B-030, B-011)                                                                                                               | Technical      | Manifest order, ordinal path order, document order; nothing ordered by a hash                                                                                                                                                      |
| A set becomes one native Feature: a `FeatureSpec` keyed into `Claims` with the set's `SpechtClaims` (B-001, B-003, C-5)                                             | Business       | Item `0391`: `SpechtModelBuilder.Build`; `FeatureSpec.ClaimFileSet`; `SpechtModel`'s `Claims`, by reference to the set                                                                                                             |
| The Feature's identity is the set's `epic` and `feature`, never the record's keys, and a difference is not reported (B-001, B-027, B-037; OQ-15)                    | Business       | Item `0391`: `FeatureSpec.Epic` and `FeatureSpec.Id` read `ClaimFileSet` first                                                                                                                                                     |
| A record supplies frontmatter, lifecycle through `claim.lifecycleKey`, and edges; none means `current` and no edge (B-003; A-3)                                     | Business       | Item `0391`: the record parsed as the Feature's document; `FeatureSpec.Lifecycle`; the existing dependency graph                                                                                                                   |
| A record is held by `SPEC1001` and `SPEC1002`, and its items by `SPEC1003`; a record-less Feature has nothing to hold (B-003)                                       | Business       | Item `0391`: the two Feature frontmatter rules skip a native Feature whose `FeatureSpec.Record` is `null`; record locations join `FindChildItems`' input                                                                           |
| A record's `claim_prefix` does not choose the grammar (B-032)                                                                                                       | Business       | Item `0391`: the native `FeatureSpec` takes `ClaimGrammar(null)`, as item `0390`'s reader does                                                                                                                                     |
| Discovery does not also read a record as a layout's Feature (B-017, B-026)                                                                                          | Technical      | Item `0391`: `SpechtModelReader.Read` drops a discovered specification whose path is a set's `record`                                                                                                                              |
| A set with no `record` and no claim file makes no Feature (B-036; OQ-14)                                                                                            | Business       | Item `0391`: a `null` entry in `SpechtListing.ClaimLocations`; `SpechtModelBuilder` makes no `FeatureSpec` for it                                                                                                                  |
| A record that is rooted, leaves the root, names no file or cannot be read leaves the set record-less, and nothing throws (B-035's reading; OQ-13, OQ-17)            | Both           | Item `0391`: `SpechtTreeStore.TryLoad`; reporting it is item `0395`'s (B-033, B-034)                                                                                                                                               |
| A set's `feature` and `epic` match the manifest's grammars, else exit `3` naming `claim.files` (B-038; OQ-18)                                                       | Business       | Item `0391`: `SpechtManifestParser.FileSets`, beside B-027 and B-029, over the filled `identifiers`                                                                                                                                |
| One identity in two sets, or a set and a layout's specification, is `SPEC3002` at each, a set at its Feature's path, line `0` (B-017; OQ-12)                        | Business       | Item `0391`: `DuplicateIdentityRule` over `FeaturesWithIdentity`, natives included, line `0` for a native Feature                                                                                                                  |
| Counted in a `claims` entry after the manifest's layouts, even beside one of that name, and named by the record or the first claim file (B-026; OQ-7, OQ-11, OQ-16) | Business       | Item `0391`: `SpechtEvaluator.Evaluate` appends the entry under `claim.source` `gherkin`; `SpechtLocation.RelativePath`                                                                                                            |
| No verdict items `0392` and `0396` own appears early on a native Feature (B-008, B-020, B-021, B-023, B-024; C-3)                                                   | Technical      | Item `0391`: ten rules, `MatrixResolver.ResolveClaims` (for `SPEC4006` and `SPEC4008`), `FactsDocument.From` and the facts command's record read skip a Feature with a `ClaimFileSet`, each lifted by the item that owns its claim |
| A tree without `claim.source` `gherkin` keeps every verdict, count and listing (B-002; C-3)                                                                         | Business       | No native Feature is built, the `claims` entry is not added and every skip is vacuous                                                                                                                                              |
| Every native path is root-relative, and nothing is written (C-2, C-6)                                                                                               | Technical      | `SpechtTreeStore.TryLoad`'s resolved root-relative record path, never a rooted one, and discovery's relative paths; no rule writes                                                                                                 |
| A scenario carrying no claim, of its own, its Rule's or its Feature's, found once by the shared reader (B-010; A-3, C-1)                                            | Business       | Item `0393`: `GherkinClaimTextReader`'s `SPCT0012` finding, unchanged                                                                                                                                                              |
| Each claim file's read is kept, so no rule parses a claim file again (C-5)                                                                                          | Technical      | Item `0393`: `SpechtClaims.Files`, the list `SpechtModelBuilder.ClaimSets` already passes; empty for a claims table                                                                                                                |
| `SPEC4011` reports that scenario at its claim file's path and keyword line, once per file (B-010, C-2)                                                              | Business       | Item `0393`: `ScenarioWithoutClaimRule`, keyed `SPEC025`, over `SpechtModel.ClaimSets`                                                                                                                                             |
| Warning by default, raised or silenced by the manifest's `rules` (B-010; OQ-5)                                                                                      | Business       | Item `0393`: the rule's `DefaultLevel`; the shipped `0.4.0` manifest's `rules`; the evaluator's stamp                                                                                                                              |
| The id joins `0.4.0` alone, with its page and a relock; `0.1.0` to `0.3.1` are unchanged (C-3; `0001-F7` B-014, C-1, C-17; `0001-F3` B-031)                         | Technical      | Item `0393`: `SchemaVersions.Shipped`, `docs/rules/v0.4.0/SPEC4011.md`, `./build.sh LockSchemaVersion`                                                                                                                             |
| Nothing reported without `claim.source` `gherkin` (B-002, C-3)                                                                                                      | Business       | Item `0393`: `SpechtModel.ClaimSets` is empty unless the source is `gherkin`; below `0.4.0` no vocabulary holds the id                                                                                                             |
| A claim tag on an Examples block, found once by the shared reader at the tag's line, its claim id kept (B-009; A-3, C-1)                                            | Business       | Item `0394`: `GherkinClaimTextReader`'s `SPCT0011` finding, which gains the optional `ClaimId` the reader already reads; the analyzer does not read it                                                                             |
| The tag records no claim (B-009)                                                                                                                                    | Business       | Item `0394`: by construction; an Examples block's tags are on no `Element`, so no `GherkinClaimDeclaration` holds them                                                                                                             |
| `SPEC4012` reports that tag at its claim file's path and line, naming the claim id, once per file (B-009, C-2)                                                      | Business       | Item `0394`: `ExamplesClaimTagRule`, keyed `SPEC026`, over `SpechtClaims.Files`                                                                                                                                                    |
| Error by default, lowered or silenced by the manifest's `rules` (B-009; OQ-5)                                                                                       | Business       | Item `0394`: `ISpechtRule.DefaultLevel`; the shipped `0.4.0` manifest's `rules`; the evaluator's stamp                                                                                                                             |
| The id joins `0.4.0` alone, with its page and a relock; `0.1.0` to `0.3.1` are unchanged (B-028, C-3; `0001-F7` B-014, C-1, C-17; `0001-F3` B-031)                  | Technical      | Item `0394`: `SchemaVersions.Shipped`, `docs/rules/v0.4.0/SPEC4012.md`, `./build.sh LockSchemaVersion`                                                                                                                             |
| Nothing reported without `claim.source` `gherkin`, nor below `0.4.0` (B-002, B-028, C-3)                                                                            | Business       | Item `0394`: `SpechtModel.ClaimSets` is empty unless the source is `gherkin`; below `0.4.0` no vocabulary holds the id                                                                                                             |
| What the check could not take from claim input is recorded once, where it is dropped, as one list the rule reads (B-014 to B-016, B-018, B-019, B-033, B-034)       | Technical      | Item `0395`: `SpechtClaimFault` and `SpechtClaimFaultKind`; the model reader's record faults as `Build`'s `recordFaults`, each set's `SpechtClaims.Faults` from the builder, joined as `SpechtModel.ClaimFaults`                   |
| A claim file the reader cannot read declares nothing and is reported at its `# language:` line, or line `0` (B-014, B-015; A-3)                                     | Business       | Item `0395`: `GherkinClaimTextException.Line`, the header's line for a language other than `en`; `SpechtModelBuilder.ClaimSets`' per-file catch                                                                                    |
| A set whose glob matches no file is reported at the glob as written, line `0` (B-016, C-2)                                                                          | Business       | Item `0395`: `SpechtModelBuilder.ClaimSets`, over the set's listed files                                                                                                                                                           |
| A file two sets match is reported once, line `0`, its claims under neither; its read stays for `SPEC4011` and `SPEC4012` (B-018)                                    | Business       | Item `0395`: `SpechtModelBuilder.ClaimSets` finds the path in two sets' listings; `SpechtClaims`' set constructor skips its declarations                                                                                           |
| Under `claim.scopes.featureTag`, a Feature-level tag capturing another identity is reported at the tag's line, naming what it captured (B-019; A-3)                 | Business       | Item `0395`: the reader's scope form from `FeatureScopeTag`; `GherkinClaimText.FeatureTagLines`; `SpechtModelBuilder.ClaimSets` compares each value with the set's `feature`                                                       |
| A record that cannot be read is reported at its path as written, an absolute one at the manifest, line `0`, never as an absolute path (B-033, B-034, B-035; C-2)    | Both           | Item `0395`: `SpechtTreeStore.IsRooted`, which `TryLoad` also calls; `SpechtModelReader.Read` beside its `TryLoad`                                                                                                                 |
| `SPEC4013` reports each fault once, error by default (B-014 to B-016, B-018, B-019, B-033, B-034; OQ-5)                                                             | Business       | Item `0395`: `ClaimInputFaultRule`, keyed `SPEC027`, over `SpechtModel.ClaimFaults`; `ISpechtRule.DefaultLevel`; the shipped `0.4.0` manifest's `rules`                                                                            |
| The id joins `0.4.0` alone, with its page and a relock; `0.1.0` to `0.3.1` are unchanged (B-028, C-3; `0001-F7` B-014, C-1, C-17; `0001-F3` B-031)                  | Technical      | Item `0395`: `SchemaVersions.Shipped`, `docs/rules/v0.4.0/SPEC4013.md`, `./build.sh LockSchemaVersion`                                                                                                                             |
| Nothing reported without `claim.source` `gherkin`, nor below `0.4.0` (B-002, B-028, C-3)                                                                            | Business       | Item `0395`: no set is read and no fault recorded unless the source is `gherkin`; below `0.4.0` no vocabulary holds the id                                                                                                         |

## 7. Technical Design

<!-- last written by: implementer, 2026-11-14 (item 0395 review round 1: the record faults reach `Build` as an argument, not on `SpechtListing`); before: implementer, 2026-11-14 (item 0395 built as designed; the open points stated as OQ-19 and OQ-20 resolve them); before: implementer, 2026-11-14 (item 0395 designed: SPEC4013 over one list of claim input faults the model reader and the builder record where they drop what they cannot take); before: implementer, 2026-11-14 (item 0394 review round 1: the claim id paragraph cites where the reader lives, not an uncited escalation); before: implementer, 2026-11-14 (item 0394 built as designed); before: implementer, 2026-11-14 (item 0394 designed: SPEC4012 over the reader's SPCT0011 finding, which gains the claim id); before: implementer, 2026-11-14 (item 0393 built as designed); before: implementer, 2026-11-14 (item 0393 designed: SPEC4011 over the reader's SPCT0012 finding; the working names SPEC025 to SPEC027 renamed SPEC4011 to SPEC4013, decision 0004); before: implementer, 2026-11-14 (item 0440, B-078 amended: the guard keeps a set by identity or by a `record` `TryLoad` resolves to the README, as the check reads it; item `0391` is built); before: implementer, 2026-11-14 (item 0440 review round 1: the guard's own match of a set to a Feature, and item `0391`'s dependency on it); before: implementer, 2026-11-14 (item 0440: the set listing named `FindClaimFiles`, `ClaimSets` internal and shared with `0001-F19`'s collision guard); before: implementer, 2026-11-14 (item 0391 review round 2: StatusRule (SPEC4010) skips a native Feature, its matrix read through MatrixResolver.Resolve; the resolver guard cites B-003); before: implementer, 2026-11-14 (item 0391 review round 1: SPEC4008 and SPEC4006 silenced by one guard in MatrixResolver.ResolveClaims; the facts command reads no record for a native Feature); before: implementer, 2026-11-14 (item 0391 built: the record, identity and report as designed, with the listing's ClaimRecords, discovery's DirectoryOf and the record-directory filter); before: implementer, 2026-11-13 (item 0391 designed, round 2: the questions answered as OQ-11 to OQ-18; B-035's reading, B-036 to B-038) -->

Items `0390` to `0396` each add their subsection after this one.

### The claim members at schema 0.4.0 (item `0389`)

Built. Schema `0.4.0` is cut by item `0436`, as `0001-F5` B-081 to B-097 and
decision 0024 specify it. `identifiers.epic` stays a grammar under `0.4.0`
(`0001-F5` B-092), so the epic reading below holds there.

**The gate.** One constant beside the parser's others,
`ClaimMembersVersion`, in
[`SpechtManifestParser.cs`](../../../Manifest/SpechtManifestParser.cs).
`Parse` sets `claimMembers` as it sets `keysAndForms`: the pin reads `claim`
at all, is at least `ClaimMembersVersion`, and its shipped `claim` object
holds `source`. The last test is the `keysAndForms` precedent, which asks the
pinned default for `keys` and `forms`.

**Below the gate.** Nothing changes. The parser reads `tag` and
`tagIgnoreCase` and leaves every other `claim` member unread, as it does at
every pin today, so a `0.3.1` manifest setting `claim.source` checks as it
does without it. That is the repository owner's answer of 2026-11-13, relayed
through the coordinating session, "Ignore below 0.4.0": B-028's second
scenario becomes "an earlier pin ignores `claim.source`", which spec-author
amends in § 2, § 3, § 10 and the `.feature` file. Both B-028 scenarios are the
parser as built, green when written, as `0001-F5` B-072's are. `ClaimInput` is
null below the gate.

**The record.** `SpechtStructure.ClaimInput`, declared in
[`SpechtStructure.cs`](../../../Manifest/SpechtStructure.cs), holds a
`SpechtClaimInput`, null below the gate:

| Member                                         | Read from                                         | Shipped `0.4.0` default |
| ---------------------------------------------- | ------------------------------------------------- | ----------------------- |
| `Source`, a `SpechtClaimSource`                | `claim.source`                                    | `"markdown"`            |
| `Files`, a list of `SpechtClaimFileSet`        | `claim.files`                                     | none                    |
| `PlannedTags`                                  | `claim.plannedTags`                               | `["@ignore"]`           |
| `WithdrawnTags`                                | `claim.withdrawnTags`                             | `[]`                    |
| `SupersededTags`                               | `claim.supersededTags`                            | `[]`                    |
| `LifecycleKey`                                 | `claim.lifecycleKey`                              | `"spec_status"`         |
| `FeatureScopeTag`, `EpicScopeTag`, each `null` | `claim.scopes.featureTag`, `claim.scopes.epicTag` | none                    |

`SpechtClaimSource` is an enum, `Markdown` and `Gherkin`, as
`SchemaSourceSelection` is for `schemaSource`; the text `"gherkin"` is then
written in the parser alone, and item `0390` branches on the value (C-4).
`SpechtClaimFileSet` holds one entry, `Epic` and `Record` null when the entry
has none. `Files` is empty under `Markdown`. Each type has its own file, as
`SchemaSourceSelection` does:
[`SpechtClaimInput.cs`](../../../Manifest/SpechtClaimInput.cs),
[`SpechtClaimSource.cs`](../../../Manifest/SpechtClaimSource.cs) and
[`SpechtClaimFileSet.cs`](../../../Manifest/SpechtClaimFileSet.cs).

**The fill.** `Parse` already holds `claim`, `claimDefaults` and
`claimDocument`. `source`, the three tag lists and `lifecycleKey` are read
through `Selected`, the way `tag` and `tagIgnoreCase` are, so a member left
out is the shipped default's, a partial `claim` object included, and the
projection copies the filled value into the document. A declared `source` is
read as written, JSON `null` included, so a `null` source is refused rather
than filled. `files` and `scopes` have no shipped default and are read only as
declared: `Selected` requires one. The tag lists read through `Strings`,
`lifecycleKey` and the scope forms through `GetValue<string>`.

**The faults.** The private static `ClaimInput` reads the record, with
`FileSets` reading `claim.files` under `gherkin` and `NoFileSets` refusing it
under `markdown`, and adds each fault to the list `SourceSelection` and
`Policy` already fill, so they reach `Check`
with every other fault and throw one `SpechtManifestException` before a schema
is read or a rule runs; each command already folds that to exit `3`, stderr
only. `epicGrammar` is the `identifiers.epic` test, hoisted to a local that
the `DeclaresEpicGrammar` initializer reads too, so the record and the
structure read one answer. Each fault names its key first:

| Manifest                                                | Fault text                                                                             | Claim                                               |
| ------------------------------------------------------- | -------------------------------------------------------------------------------------- | --------------------------------------------------- |
| `files` declared while the source is not `gherkin`      | `claim.files is set, and only claim.source 'gherkin' reads claim files`                | B-029                                               |
| `gherkin`, `files` absent                               | `claim.files is missing, and claim.source 'gherkin' needs at least one claim file set` | B-029                                               |
| `gherkin`, `files` not an array, `null` included        | `claim.files is not a list of claim file sets`                                         | unclaimed; `0001-F2` B-007                          |
| `gherkin`, `files` empty                                | `claim.files is empty, and claim.source 'gherkin' needs at least one claim file set`   | B-029                                               |
| an entry that is not an object                          | `claim.files[<n>] is not an object`                                                    | unclaimed; `0001-F2` B-007                          |
| an entry whose `glob` is absent or not text             | `claim.files[<n>] has no text 'glob'`                                                  | B-029 (absent); not text unclaimed, `0001-F2` B-007 |
| an entry whose `feature` is absent or not text          | `claim.files[<n>] has no text 'feature'`                                               | B-029 (absent); not text unclaimed, `0001-F2` B-007 |
| an entry with `epic` and no `identifiers.epic`          | `claim.files[<n>] has an 'epic', and the manifest declares no epic grammar`            | B-027                                               |
| an entry without a text `epic` under `identifiers.epic` | `claim.files[<n>] has no text 'epic', which identifiers.epic requires`                 | B-027                                               |
| an entry whose `record` is present and not text         | `claim.files[<n>] has a 'record' that is not text`                                     | unclaimed; `0001-F2` B-007                          |
| `source` that is neither `"markdown"` nor `"gherkin"`   | `claim.source must be 'markdown' or 'gherkin'`                                         | B-031                                               |

`<n>` is the entry's zero-based index. No fault repeats a value: a glob or a
record may be rooted, and an absolute path is in no output (brief § 9; the
`spechtSchema` precedent in `0001-F7` § 7). A fault on one entry does not stop
the others, so one run names every faulty entry. `files` declared beside the
default source is a fault whether or not `source` is written, which also
catches a misspelt `source` member: unread, it leaves the source `markdown`.

**B-027 under each grammar.** A manifest that declares `identifiers.epic`
requires `epic` on every entry; one that declares none refuses it. The
grammar's own match against `epic` and `feature` is not this item's: no claim
of this Feature states it, and a set's identity is item `0391`'s (B-017).

**A source value outside the two.** B-031 (decision 0003): `"yaml"`,
`"Gherkin"`, a number or `null` is refused, as `schemaSource` and `pinPolicy`
refuse theirs, because a misread value would check the tree as Markdown in
silence. A refused source reads as `Markdown`, so `claim.files` beside it adds
its own fault as well.

**Type faults outside a set.** A tag list that is not an array, an entry in
one that is not text, a `lifecycleKey`, `featureTag` or `epicTag` that is not
text, and a `scopes` that is not an object take the path every key of the
wrong JSON type takes: `Strings`, `GetValue<string>` or `AsObject` throws, the
model reader raises `SpechtManifestUnreadableException`, exit `3`, without the
key (`0001-F2` B-007), as `claim.tag` does today. Inside a set every fault
names `claim.files`: B-029 asks its faults to name the key, and the type faults
inside a set, which § 10 leaves unclaimed because each exits `3` either way
(`0001-F2` B-007), name it by this design.
Whether a tag list entry must be an `@`-led tag, and whether a scope form must
hold its placeholder once, no claim of this item states; item `0390` reads the
forms and item `0395` reports B-019.

**No `codeAnalysis`.** The parser reads `manifest["claim"]` and nothing under
`codeAnalysis`, which stays in `KnownKeys` so the analyzer's section is
accepted and unread (B-002, C-7). `EvidencePolicy` in `src/tool` reads
`codeAnalysis.records.claimSource` and `claimFileSets` for `specht evidence`,
which is `0201-F5`'s and not a rule; moving it is § 5 row 15.

**The effective manifest.** `claimDocument` already receives every value
`Selected` fills, so under `0.4.0` `specht manifest` prints `claim` with its
declared members in written order, then each filled member in the order the
parser reads it. `files` and `scopes` print as declared, and are absent when
not declared. `--defaults` prints the shipped `0.4.0` `claim` object. The MCP
`manifest` tool runs the same command.

**The files.** The shipped
[`0.4.0` manifest](../../../../tool/manifest/v0.4.0/manifest.json)'s `claim`
carries `source`, `plannedTags`, `withdrawnTags`, `supersededTags` and
`lifecycleKey` with the defaults above, and `./build.sh LockSchemaVersion`
relocked `0.4.0` after its entry was removed by hand, as item `0352` relocked
`0.3.1`; `0.1.0` to `0.3.1` and their lock entries are byte-identical
(`0001-F7` C-1, C-17). The live `.specht/manifest.json` carries the five
members, because `0001-F4` B-004's test compares `claim` whole; `upgrade` adds
only missing top-level keys. `docs/configuration.md` names the `0.4.0`
members.

**Reuse.** `Selected`, `Strings`, `Text`, the fault list and `Check` carry the
reading; no new helper is needed. Three records of the same shape already
exist out of the engine's reach: `EvidencePolicy.ClaimFileSet` in `src/tool`,
which the engine cannot reference, and `GherkinClaimFileSet` in
`src/analyzers`, a separate assembly. `SpechtClaimFileSet` is a third, and
how the engine and the analyzer share one reader is § 5 row 2's (OQ-6). The
engine's `GherkinClaimTags` is not held by the record: it needs the claim
grammar, which is chosen per Feature (A-1; `0001-F5` B-086), so item `0390`
composes it from `SpechtClaimInput` and the Feature.

**Rejected.**

- Rejecting the members below `0.4.0`: the owner's answer of 2026-11-13; it
  would also fail, under published `0.3.1`, a manifest that `0.3.1` accepts
  (`0001-F7` C-1).
- Rejecting an unknown `claim` member under `0.4.0`, as `0001-F5` B-083 does
  for an `ids` record: no claim asks for it, and `claim` members have never
  been held to a list.
- A nested `scopes` record: two nullable texts, with no second caller.
- A Boolean for the source: the enum names the two values the manifest names,
  as `SchemaSourceSelection` does.
- `GherkinClaimTags` on the structure: see Reuse.
- Validating the scope forms and the tag-list entries at parse time, as the
  analyzer does: no claim of this item states it.

### The native claim reader (item `0390`)

Built. The engine already holds the reader:
[`GherkinClaimTextReader`](../../../Model/GherkinClaimTextReader.cs), which the
analyzer compiles through its source link (`0201-F7` decision 0004) and
`specht evidence` calls. This item calls it from the model build, so an
element, a lifecycle and a planned case mean what the analyzer reads them to
mean by construction (A-3), and the check gains no second Gherkin reading.

**When.** Only when `SpechtStructure.ClaimInput` is set and its `Source` is
`Gherkin`, which is schema `0.4.0` or later with `claim.source` `gherkin`
(B-001). Otherwise the listing, the texts and the model are today's, and no
rule reads what this item adds, so no verdict moves (B-002, C-3).

**Expanding a set.** `SpechtModelReader.Read` calls
`ISpecDiscovery.FindTestFiles(root, [set.Glob], structure.Discovery.Exclusions)`
once per entry of `ClaimInput.Files`, in the manifest's order, and keeps the
paths ending `.feature`, B-001's words. That method already lists each file a
glob matches once, outside `exclusions`, root-relative, in ordinal order,
through the check's one listing: git, or a walk through `IFileSystem`
(`0001-F6` B-001, B-005, B-008). The lists go on `SpechtListing.ClaimFiles`,
a new init member at the index of `ClaimInput.Files`, empty by default, as
`Companions` sits beside `Specifications`. Their paths join the distinct list
`SpechtTreeStore.Load` reads through `IFileSystem` (C-2), so a file that two
sets match, or that is also a companion, is read once. The builder takes each
text from that dictionary and touches no file system.

From item `0440` (`0001-F19` B-078) that listing is
`ISpecDiscovery.FindClaimFiles(root, set.Glob, exclusions)`, the same
`FindTestFiles` call and `.feature` filter in one place, and
`SpechtModelBuilder.ClaimSets` is `internal static`, taking the sets and each
one's listed files, so `0001-F19`'s collision guard reads one Feature's sets
through the same reader. `Build` hands it every set and
`SpechtListing.ClaimFiles`, so the check reads as described here.

Which sets the guard reads is matched in `0001-F19`, not through anything this
Feature builds. `HeldIds.NativeClaimsOf` reads the Feature's `.spec/README.md`
frontmatter itself, the text under the manifest's `FeatureKeys.Id` and
`FeatureKeys.Epic`, and keeps a set whose `feature` equals that id and, where
the set has an `epic`, whose `epic` equals that epic, both compared ordinally.
It also keeps a set whose `record`, resolved through `SpechtTreeStore.TryLoad`
as `SpechtModelReader.Read` resolves it, gives the README's root-relative path,
compared ordinally, whatever identity the set declares (B-037; `0001-F19`
B-078, § 7, item `0440`). A `record` `TryLoad` reads as none leaves the set
matched by identity alone (B-035). Item `0391` built the check's side: a native
Feature is a `FeatureSpec` at its set's loaded record, or else its first claim
file, under the set's identity, and a layout's specification at a loaded
record's path is dropped ("Where a native Feature sits" and "The Feature",
below). So the guard and the check resolve a `record` by one reading,
`TryLoad`'s ("Reading a record").

**The tag forms.** `SpechtModelBuilder.ClaimSets`, a step of `Build`
declared in [`SpechtModelBuilder.cs`](../../../Model/SpechtModelBuilder.cs),
composes one `GherkinClaimTags` and reads every set with it, since every set
reads the tree's grammar:

| Member                                           | Composed from                                                                                                                                          |
| ------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `Claim`                                          | `new GherkinClaimTagForm(structure.ClaimTagForm, "claim", structure.ClaimTagIgnoreCase, grammar)`                                                      |
| `grammar`                                        | `structure.ClaimGrammar(null).Pattern`, grouped as `(?:…)` after a negative lookahead for the escaped `ScenarioHead` when `LongerScenarioHead` is true |
| `FeatureScope`, `EpicScope`                      | `null`                                                                                                                                                 |
| `PlannedTags`, `WithdrawnTags`, `SupersededTags` | `ClaimInput`'s, each the shipped `0.4.0` default when the manifest leaves it out (item `0389`)                                                         |

That is the reading `FeatureTagParser` gives a companion token: the text
between `claim.tag`'s prefix and suffix, compared under `tagIgnoreCase`, and a
whole match of the Feature's claim grammar under the same case setting
(`0001-F5` decision 0013, B-086). The lookahead is `0001-F5` B-095: a captured
text that begins with a longer scenario head is not a claim tag.
`FeatureTagParser` tests that with `StartsWith`; the composed grammar tests it
with `(?!…)` under the same case option, so a token has one reading (C-1). No
tag is written in code; the form and every list come from the manifest (C-4).
The parser already refuses a `claim.tag` without `{claim}` exactly once, so the
form's constructor does not throw. The scope forms stay `null`: they fill
`FeatureTagValues` and `EpicTagValues` alone, which only B-019 reads, and a
scope form without its placeholder throws in that constructor; both are item
`0395`'s.

The grammar is `ClaimGrammar(null)`, the tree's, and stays so when item `0391`
reads a set's `record`: the owner selected "(b) No, record is lifecycle" for
`0391`, which § 10 reads as a record's claim prefix not selecting a native
Feature's grammar (B-032). The group around the pattern keeps an alternation in a
declared grammar under the lookahead.

**Reading a file.** `GherkinClaimTextReader.Read(path, text)` per claim file,
in ordinal order. What it already does answers the claims:

| Claim | The reader as built                                                                                                                                                                     |
| ----- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| B-004 | A Feature, a Rule, a Scenario or `Example`, or a Scenario Outline or `Scenario Template`, declares each claim id its own tags give, once per element, at its keyword line, as captured  |
| B-005 | A tag the form or the grammar rejects declares nothing, and the reader makes no finding of it                                                                                           |
| B-006 | An outline declares once, at the outline; each Examples body row is one of its cases                                                                                                    |
| B-007 | A Rule declares once, at the Rule; its scenarios are its cases and declare nothing of it                                                                                                |
| B-012 | `State` from the declaring element's own tags: `Superseded` when one is in `supersededTags`, else `Withdrawn` when one is in `withdrawnTags`, else `Active`                             |
| B-013 | `Cases`: a Scenario's keyword line, or each Examples body row of an outline, `Planned` when the Feature, the Rule, the scenario or, for a row, its Examples block carries a planned tag |

A claim file the reader cannot read, a `GherkinClaimTextException` for a
language other than `en`, no `Feature:` line or text it does not parse,
declares no claim, and the run goes on: `SpechtModelBuilder.ClaimSets` catches
it per file. Reporting it is item `0395`'s (B-014, B-015). Nothing in this item
reads the reader's `SPCT0011` and `SPCT0012` findings.

**B-025's scenario set.** `GherkinClaimDeclaration`, declared in
[`GherkinClaimDeclaration.cs`](../../../Model/GherkinClaimDeclaration.cs),
gains one positional member, `Scenarios`: the keyword lines of the Scenarios
and Scenario Outlines carrying the claim, in document order. For a declaring
Scenario or outline that is its own line, for a Rule every scenario under it,
and for the Feature every scenario in the file. The reader already holds that
set, as `covered`, when it builds `Cases`; it is kept as keyword lines because
an outline's cases are its rows, not the outline. The reader's one constructor
call is the only one. The analyzer and `specht evidence` read neither the
member nor its value, so their reading, diagnostics and export do not change
(§ 5 row 1), and the analyzer's link list is unchanged. `0001-F13` B-001 counts
the set; `0001-F12` A-1 resolves a row's scenario against it, which needs each
scenario's title as well, and adding titles is item `0396`'s.

**The catalogue record (C-5).** A native claim is the record a § 3 row
becomes, [`SpechtClaim`](../../../Model/SpechtClaim.cs), with its source as
provenance:

| `SpechtClaim` member             | A § 3 row                   | A native claim                                                          |
| -------------------------------- | --------------------------- | ----------------------------------------------------------------------- |
| `Id`                             | its trimmed first cell      | the declaration's id, as authored                                       |
| `Line`                           | its line                    | the declaring element's keyword line                                    |
| `IsValid`                        | the grammar's verdict       | `true`: the reader declares only what the grammar matches               |
| `FirstDeclarationLine`           | an earlier row's line       | `null`; B-008 reports each declaration, item `0392`                     |
| `Path`, a new init member        | `null`, the specification's | the claim file's root-relative path                                     |
| `Declaration`, a new init member | `null`                      | the `GherkinClaimDeclaration`: `Element`, `State`, `Cases`, `Scenarios` |

[`SpechtClaims`](../../../Model/SpechtClaims.cs) gains a second constructor,
from a `SpechtClaimFileSet`, the files read from it in ordinal order and the
structure: `Rows` holds every declaration, in file then document order; `Ids`
and `TagIds` are built as the table's are, an id's first declaration keeping
its line; `Section` is `null`, since no claims table is read (B-001); and a new
`FileSet` names the set, `null` for a table. The Feature identity is the set's
`Epic` and `Feature` (B-001, B-027).

`SpechtModel.ClaimSets`, new, holds one such `SpechtClaims` per entry of
`ClaimInput.Files`, in the manifest's order, empty unless the source is
`gherkin`; the builder passes it to a new optional constructor parameter,
`claimSets`, which the tests that build a model by hand leave out. It
sits beside `SpechtModel.Claims`, not in it: that dictionary is keyed by a
`FeatureSpec`, and a native Feature has none until item `0391` gives it a
document, an identity and a report entry. `0391` keys the same `SpechtClaims`
under that Feature in `Claims`, in place of the one the record's table would
give; no claim file is read twice. Until then `Features` holds no native
Feature, so no rule, summary count or report line changes (B-002; B-026 is
`0391`'s).

**Determinism (B-030, B-011).** Sets in manifest order, files in ordinal path
order from discovery, declarations in document order, and within an element
its ids in tag order, line by line. Nothing orders output by a hash, reads a
clock or holds an absolute path, and this item prints nothing new, so the
output of the same tree is byte-identical. Reordering or retitling a file
moves `Line`, `Cases`, `Scenarios` and row order, and leaves each claim's id,
set identity and `State` (B-011): none is read from a title or a position.

**Testability.** Each mechanism is reachable in memory:
`SpechtModelBuilder.Build` over a `SpechtListing` with `ClaimFiles` and a text
dictionary, or `SpechtModelReader.Read` over a `MockFileSystem` tree through
`EngineServices`, reading `model.ClaimSets`. The item `0389` review asked that
this item's § 9 rows for B-012 and B-013 name a test of the shipped default
fill of `plannedTags`, `withdrawnTags`, `supersededTags` and `lifecycleKey`.
That fill is two observations: `SpechtManifestParser.Parse` fills all four from
the shipped `0.4.0` manifest when the manifest leaves them out, and the
composition reads the filled lists, so a scenario tagged `@ignore` under a
manifest with no `plannedTags` is planned, and one tagged with a lifecycle tag
the manifest does not list is `Active`. `lifecycleKey` takes effect where the
record is read, item `0391` (B-003).

**What stays.**

| Item   | Takes                                                                                                                                                                                        |
| ------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `0391` | A native Feature as a `FeatureSpec` from the set's `record` or its first claim file; the record's lifecycle and edges; its identity check and report entry; keying `ClaimSets` into `Claims` |
| `0392` | `SPEC4003` over native rows across files (B-008); a record's sections (B-020); `SPEC4001` and `SPEC4002` silent (B-021)                                                                      |
| `0393` | `SPEC4011` from the reader's scenario-without-claim finding (B-010)                                                                                                                          |
| `0394` | `SPEC4012` from the reader's Examples-tag finding (B-009)                                                                                                                                    |
| `0395` | `SPEC4013`: an unreadable file, an empty set, a file two sets match with its claims under neither, and the scope forms (B-014 to B-016, B-018, B-019)                                        |
| `0396` | Scenario titles, the matrix and the trace over `ClaimSets` (B-022 to B-024)                                                                                                                  |

The record-prefix question this design raised for item `0391` is answered: the
owner selected "(b) No, record is lifecycle", which § 10 reads as a set's
`record` supplying frontmatter, lifecycle and edges only, the tree's grammar
reading the tags, so `0391` keeps the one `GherkinClaimTags` this item composes. B-032
says so.

**Reuse.** `GherkinClaimTextReader` and its records, `GherkinClaimTagForm`,
`ISpecDiscovery.FindTestFiles`, `SpechtTreeStore.Load`,
`SpechtStructure.ClaimGrammar`, `SpechtClaim` and `SpechtClaims` carry the
reading. No type is added.

**Rejected.**

- Reading claim files with `FeatureTagParser`: a second Gherkin reading, which
  differs from the analyzer's on `Example:`, `Scenario Template:` and an
  Examples block's tags (A-3, C-1).
- A native Feature in `Features` now: every rule would run on a Feature with no
  document before items `0391` and `0392` say what it obeys, and the summary
  would count it before B-026.
- A per-file scenario record with titles now: a new type, and a new source link
  into the analyzer, which decision 0004 of `0201-F7` does not cover; no claim
  of this item reads a title.
- One listing of every `.feature` file, each glob matched after it: the glob's
  reading, `SpechtDiscovery.Pattern`, is discovery's own; one listing per set
  reuses its public method.
- Dropping a repeated id from the rows: B-008 reports every declaration.

### A claim file set's record, identity and report (item `0391`)

Built. Item `0390` reads each set into `SpechtModel.ClaimSets`. This item
makes each set one
native Feature, a [`FeatureSpec`](../../../Model/FeatureSpec.cs) in
`SpechtModel.Features`, so the rules that should see it do (B-003, B-017,
B-026, B-036, B-037). The rules whose claims later items own skip it until
those items land. The parser holds a set's identity to the manifest's grammars
(B-038). Nothing here runs unless `ClaimInput.Source` is `Gherkin`, so a tree
without the selection keeps every verdict, count and listing (B-002, C-3).

**A set's identity against its grammars (B-038; OQ-18).**
`SpechtManifestParser.FileSets` already checks each entry for B-027 and B-029.
It also tests the entry's `feature` against the feature grammar, and its
`epic`, when it has one, against the epic grammar. Each test is a whole match,
case-sensitive, as `PathIdentityRule` tests a path's identity. A failure adds
`claim.files[i] has feature '<value>', which the feature grammar '<grammar>'
does not match`, or the same for `epic`, to the fault list that already exits
`3` before any rule runs. To make the grammars available there, `Parse` reads
the filled `sections` and then the filled `identifiers` into locals before it
builds the structure, in the order the constructor call read them, so the
projected manifest's keys keep their order. `Parse` passes `identifiers`
through `ClaimInput` to `FileSets`.

A grammar that does not compile is already `GrammarFaults`' fault, and
`FileSets` skips its test, so no grammar throws. The fault is in the entry's
own loop, so it names the index as written even when an earlier entry is
invalid.

**Reading a record (B-003; B-035's reading).** `SpechtTreeStore` gains
`TryLoad(root, relativePath)`, which returns a `SpechtTreeFile` or `null`. It
returns `null` in four cases:

- the path is rooted;
- resolving its `.` and `..` segments in turn leaves the root;
- no file is there;
- reading it throws an `IOException` or an `UnauthorizedAccessException`.

Otherwise it returns the file under its resolved root-relative path, with
forward slashes. A segment splits at `/` and at the platform's separator.
`SpechtModelReader.Read` calls it once per set with a `record`, before
discovery. A `null` is the set read as having no `record` (B-035), so
`SpechtTreeStore.Load` never sees a record path and nothing throws.

Reporting a `null`, `SPEC4013` at the record's path or at the manifest, is item
`0395`'s (B-033, B-034; OQ-13, OQ-17). Until then the set is read as
record-less with nothing reported. That is B-035's reading with its report
still to come, not a third behaviour.

**Where a native Feature sits.** For each set, `SpechtModelReader.Read`, after
item `0390`'s set listing:

| Step                  | What it does                                                                                                                                                                                                                                                                                                                                                                                             |
| --------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Location              | The loaded record's resolved path; else the first of `ClaimFiles[i]`, which discovery lists in ordinal order (B-026; OQ-7); else none, and the set makes no Feature (B-036; OQ-14). The layout is `new SpechtLayout("claims", set.Glob)`, with no path identity                                                                                                                                          |
| No second reading     | A specification the layouts discovered at a loaded record's resolved path, compared ordinally, is dropped before companions and items are found, so the record is read once, as the native Feature's document, and never as a layout's Feature too                                                                                                                                                       |
| Items beside a record | Each loaded record's location joins the specifications passed to `ISpecDiscovery.FindChildItems`, so `SPEC1003` and `SPEC5001` to `SPEC5004` hold the record's items and children as they hold any specification's (B-003; § 5 row 13). A record whose directory a specification or an earlier record already holds is not passed again, so no item is listed twice. A record-less Feature lists no item |

Two init members of [`SpechtListing`](../../../Discovery/SpechtListing.cs),
each at the index of `ClaimFiles`, carry the result: `ClaimLocations`, `null`
for a set that makes no Feature, and `ClaimRecords`, the loaded record's
resolved path or `null`. Each loaded record's text joins the text dictionary
the builder reads. A location's directory comes from
`SpechtDiscovery.DirectoryOf`, now `internal`, so the directory of a location
has one implementation. A second `SpechtLocation` constructor was rejected:
the test project's generated fixture builder fails on a record with two
constructors.

**The Feature.** `SpechtModelBuilder.Build` makes one `FeatureSpec` for each
location that is not `null`, after the layouts' Features, in its private
`NativeFeatures`:

| `FeatureSpec` member              | A layout's Feature      | A native Feature                                                                                                                   |
| --------------------------------- | ----------------------- | ---------------------------------------------------------------------------------------------------------------------------------- |
| `Document`                        | the specification       | the record, parsed by `SpechtDocumentParser`; with no record, the parse of empty text at the first claim file's path               |
| `FeatureFiles`                    | its companions          | none; B-021 makes the claim files its companions, item `0392`                                                                      |
| `ClaimGrammar`                    | from its `claim_prefix` | `ClaimGrammar(null)`, whatever the record's `claim_prefix` says (B-032), the grammar item `0390`'s reader uses                     |
| `ClaimFileSet`, a new init member | `null`                  | the set, the same instance as `ClaimInput.Files[i]`                                                                                |
| `Record`, a new init member       | `null`                  | `ClaimRecords[i]`: the loaded record's resolved path; `null` with no `record` or when `TryLoad` gives `null`                       |
| `Epic`, `Id`                      | the frontmatter's       | the set's `Epic` and `Feature` (B-001, B-027), even when the record declares others (B-037; OQ-15); the difference is not reported |
| `Lifecycle`, a new init member    | `null`                  | the record's frontmatter text at `ClaimInput.LifecycleKey`, `null` when absent; `current` when the set has no record               |

`Lifecycle` reads as the analyzer's catalogue reads it,
[`GherkinCatalogReader`](../../../../analyzers/Bindings/Gherkin/GherkinCatalogReader.cs):
the text at the lifecycle key, or `current` with no record (A-3). The key
comes from the manifest (C-4). `SpecStatus` and `Status` still read
`keys.feature.specStatus` and `keys.feature.status` from the document, so the
approval rules read a record as they read any specification. A record's own
`id` and `epic` are held by `SPEC1002`'s schema alone (§ 5 row 16).

Edges come from the document as for any Feature: `FeatureSpec.Strings` over
`keys.feature.dependsOn` and `keys.feature.blocks`. A record-less Feature's
empty document declares none (B-003). Because `Epic` and `Id` are set, every
native Feature is in `FeaturesWithIdentity`. The dependency graph resolves an
edge to it and from it, and `SPEC6001` to `SPEC6003` hold it.

**Its claims.** `SpechtModel` keys each native Feature in `Claims` to the
`SpechtClaims` of `ClaimSets` whose `FileSet` is the same instance as the
Feature's `ClaimFileSet`. It compares by reference because two manifest
entries can hold equal values. A layout Feature's claims are still derived
from its table. No claim file is read twice, and no claims table is read for a
native Feature (B-001). `MatrixSections` reads the document as before: a
record's matrix, or `null`. A set that makes no Feature keeps its empty entry
in `ClaimSets`, and nothing keys it.

**Frontmatter (B-003).** `SPEC1001` and `SPEC1002` hold a record as any
specification. `FrontmatterPresentRule` and `FeatureFrontmatterRule` skip a
native Feature whose document is no record: a set with no `record`, or one
whose `TryLoad` gave `null`. Nothing is authored to hold, and the empty
document would otherwise be reported as missing frontmatter. That skip is
permanent; it is B-003, not a deferral. The skip reads `ClaimFileSet` set and
`Record` `null`, since a layout's Feature also has no `Record`.

**One identity (B-017; OQ-12).** `DuplicateIdentityRule` already groups
`FeaturesWithIdentity` by `Identity`. A set beside another set, or beside a
layout's specification, with the same identity is therefore reported at each,
under the identity form the manifest declares. A layout's specification is
reported where it is today: its path, at the frontmatter's id line. A native
Feature is reported at its path, line `0`: the rule takes line `0` when
`ClaimFileSet` is set. A set that makes no Feature is compared with none
(B-036).

**Count and listing (B-026; OQ-11, OQ-16).** `SpechtEvaluator.Evaluate`
appends one `SpechtReportLayout` named `claims` after the manifest's layouts.
It counts the Features with a `ClaimFileSet`, and it is appended only under
`claim.source` `gherkin`, so the summary and `layouts` of any other tree are
unchanged. A manifest layout that is itself named `claims` keeps its entry,
and this one follows it. A manifest layout's entry counts only Features with
no `ClaimFileSet`, so a native Feature is never counted twice, whatever its
set's glob. `SpecificationCount`, `model.Features.Count`,
includes native Features. Every violation on a native Feature carries its
location: the record, or the first claim file. No other report member changes
(`0001-F3` B-005).

**What stays silent until its item.** With native Features in `Features`,
these rules would report on them before the items that own their claims say
what holds. Each one skips a Feature whose `ClaimFileSet` is not `null` until
that item replaces the skip. The two rules that read claims only through
`MatrixResolver.ResolveClaims` are silenced there instead, by one guard:

| Rule                                           | Type                                                                              | Would report today                                                                                                        | Lifted by            |
| ---------------------------------------------- | --------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------- | -------------------- |
| `SPEC2001`                                     | `SectionContractRule`                                                             | a record's missing claims section; every section of a record-less Feature's empty document                                | `0392`, B-020        |
| `SPEC2002`                                     | `TableHeaderRule`                                                                 | a record's tables, before B-020 says which are held                                                                       | `0392`, B-020        |
| `SPEC4001`                                     | `CompanionFileRule`                                                               | `found 0 '.feature' files` on every native Feature                                                                        | `0392`, B-021        |
| `SPEC4002`                                     | `ClaimTagRule`                                                                    | nothing today, with no companions; skipped so B-021 decides it                                                            | `0392`, B-021        |
| `SPEC4003`                                     | `ClaimIdRule`                                                                     | no row, since a native `Section` is `null`; the shared-head finding at the Feature's path                                 | `0392`, B-008        |
| `SPEC4004`                                     | `ClaimRowRule`                                                                    | nothing, since a native `Section` is `null`; skipped so B-023 and B-024 decide it                                         | `0396`, B-023, B-024 |
| `SPEC4005`, `SPEC4007`, `SPEC4009`, `SPEC4010` | the `0001-F12` rules                                                              | a record's § 9 rows resolved against no companion, through `MatrixResolver.Resolve`, which the claim guard does not cover | `0396`, B-023, B-024 |
| `SPEC4006` (`SPEC035`), `SPEC4008`             | `RetiredClaimRule`, `MissingScenarioRule`, through `MatrixResolver.ResolveClaims` | a crash: the resolver reads `claims.Section!`, and a set's claims have no section                                         | `0396`, B-023, B-024 |

`SPEC3001` needs no skip. The `claims` layout has no path identity, and B-027
makes a set's `epic` present wherever the epic grammar is declared, so
`PathIdentityRule` passes over every native Feature. `SPEC7001` and
`SPEC7002` need none either: they read a record as any specification (OQ-4),
and a record-less Feature's `SpecStatus` is `null`. `specht facts` lists
`model.Features` (`0132-F2`). No claim says what it carries for a native
Feature, so `FactsDocument.From` skips one too, and its output is unchanged.
`FactsCommand` passes only the layouts' Features to
`SpechtModelReader.ReadRecords` (item `0368`), so an unreadable file beside a
native Feature's location cannot fail `specht facts` for a Feature whose facts
are dropped.

Each rule skip is a `Where` on `ClaimFileSet` over the rule's loop.
`MatrixResolver.ResolveClaims` returns no claim for a Feature whose
`ClaimFileSet` is set, so every rule that resolves claims through it, those
of later items included, is silent on a native Feature without a skip of its
own. There is no model member and no base class: the skips are lifted one at
a time, and item `0396` removes the last, the resolver's guard among them.
`AMatrixRuleOverANativeFeatureWithAClaim_WhenEvaluated_ShouldReportNothingAndNotFail`
pins the matrix rules' silence; no other skip has a test (§ 8).

**Determinism and paths (B-030, C-2, C-6).** Native Features follow the
layouts' Features in manifest order. A record's path is its resolved
root-relative form, which is its text as written for a path with no `.` or
`..` segment. A rooted record is never a path in the output (B-035; OQ-17).
The first claim file is discovery's relative path. No rule writes.

**Testability.** Every step runs in memory:

- `SpechtManifestParser.Parse` over manifest text gives B-038's faults.
- `SpechtTreeStore.TryLoad` over a `MockFileSystem` gives each `null` case.
- `SpechtModelReader.Read` over a `MockFileSystem` tree through
  `EngineServices` reads `Features`, `Claims` and the listing.
- `SpechtModelBuilder.Build` takes a hand-built listing with
  `ClaimLocations` and `ClaimRecords`.
- `SpechtEvaluator.Evaluate` gives the summary and `layouts`.

**Reuse.** `FeatureSpec`, `SpechtDocumentParser`, `DuplicateIdentityRule`,
`SpechtDependencyGraph`, `ISpecDiscovery.FindChildItems`, `SpechtTreeStore`,
`SpechtReportLayout`, `SpechtDiscovery.DirectoryOf`, the parser's fault list
and item `0390`'s `ClaimSets` carry the work. No type is added.

**Rejected.**

- A second list, `NativeFeatures`, read by the rules that should see native
  Features. `SPEC1001`, `SPEC1002`, `SPEC3002`, `SPEC5001` to `SPEC5004` and
  `SPEC6001` to `SPEC6003` would each need a second loop, and a dependency
  edge between the two lists would need both. One list with ten rule skips
  and one resolver guard is the smaller change, and the skips shrink.
- Leaving a record to its layout and adding the set's claims to that Feature.
  A record outside every layout glob would be no Feature at all, and a record
  inside one would be held to the claims table B-020 removes.
- A record's `claim_prefix` choosing the grammar: B-032, the owner's
  selection "(b) No, record is lifecycle".
- Loading a record with the rest of the tree through `SpechtTreeStore.Load`
  and catching its failure. `Load` reads every listed path in one call, and
  one bad record would lose the whole tree's text.
- B-038 as a rule over native Features. OQ-18 chose an invalid manifest, and
  the parser already holds B-027's and B-029's faults on the same entry.

### A scenario in a claim file with no claim, `SPEC4011` (item `0393`)

Built. B-010: a Scenario or Scenario Outline in a claim file
that carries no claim tag of its own, from its Rule or from its Feature is
reported at its keyword line, at warning by default. The id is `SPEC4011`
(decision 0004). The shared reader already finds such a scenario; this item
keeps what it found and reports it.

**What the reader finds.** `GherkinClaimTextReader`'s `Complete` adds one
`SPCT0012` finding for each case whose own claim ids, its Rule's and the
Feature's are all empty (`0201-F7` B-015). A case is a `Scenario:` or
`Example:`, or a `Scenario Outline:` or `Scenario Template:`. Each id is one
the composed `GherkinClaimTagForm` reads, so inheritance and the tag reading
are the ones B-004 to B-007 already use (A-3, C-1). The finding's line is the
case's keyword line, the line `Open` records, never a tag line above it. A
`Background:` is not a case. A file the reader cannot read throws before
`Complete` returns, so it has no finding; reporting that file is `SPEC4013`'s
(B-014, B-015, item `0395`). This item adds no second test of what a scenario
inherits.

**Keeping the reads (C-5).** `SpechtModelBuilder.ClaimSets` already holds
each file's `GherkinClaimText` and passes the list to `SpechtClaims`' set
constructor, which keeps only the declarations. The constructor also keeps
the list, as a new read-only member, `SpechtClaims.Files`, in listing order.
The table constructor leaves it empty. No claim file is parsed again. Item
`0394` can read `SPCT0011` from the same member (B-009).

**The rule.** `ScenarioWithoutClaimRule`, in
`src/specht/Rules/NativeClaims/ScenarioWithoutClaimRule.cs`, namespace
`Specht.Rules.NativeClaims`, beside this specification, as
`MissingScenarioRule` sits beside `0001-F12`'s. `Evaluate` walks
`model.ClaimSets`, then each set's `Files`, keeping the first read of each
`RelativePath` under ordinal comparison. Each finding whose `DiagnosticId` is
`SPCT0012` becomes one `SpechtFinding`:

| Member       | Value                                                                                                                  |
| ------------ | ---------------------------------------------------------------------------------------------------------------------- |
| `File`       | the claim file's root-relative path, from discovery (C-2), not the native Feature's location: B-010's `add.feature(9)` |
| `Line`       | the finding's line, the scenario's keyword line                                                                        |
| `Identifier` | `null`: the scenario has no claim to name, so the printed line carries no brackets                                     |
| `Message`    | `scenario declares no claim and inherits none from its Rule or its Feature`                                            |

The message names no section, marker or manifest key, so `0001-F5` B-009's
message theories do not take it. `SPCT0012` is the shared reader's finding id,
not a manifest literal (C-4).

**A file two sets match.** Until item `0395`, such a file is listed under
both sets and read by the one reader, so both reads carry the same findings.
The first-read-per-path filter reports each scenario once. B-018 then records
the file's claims under neither set. This design reads the file as still a
claim file, so its scenarios are still reported, once. Item `0395` keeps the
file's read where this rule finds it, or moves this filter with it.

**Readings B-010 does not spell out.** Each follows from the reader as built,
and the analyzer reports the same (A-3):

- A scenario carrying a `claim.plannedTags` tag and no claim is reported. B-013
  keeps a planned scenario's claim, and this scenario has none to keep.
- A scenario whose only tags are ordinary, a mistyped claim tag included, is
  reported (B-005; § 5 row 7).
- An outline whose only claim tag is on an Examples block is reported. That tag
  declares nothing (B-009), and `SPEC4012` reports the tag itself.
- A scenario whose title holds a claim id is reported, and no claim is
  inferred (C-1; the `@boundary` scenario).

**The id and the vocabulary.** The rule's key is `SPEC025`, its working name,
as `MissingScenarioRule`'s is `SPEC023` and `StatusRule`'s `SPEC033`
(ADR-0029 § "The four `0001-F12` rules"; decision 0004's table).
`SchemaVersions.Shipped`'s `0.4.0` row gains `["SPEC025"] = "SPEC4011"`.
No lower row holds `SPEC025`. `RuleId` returns a key it does not map
unchanged, no lower vocabulary holds that key, and the evaluator neither runs
nor counts the rule below `0.4.0` (`0001-F7` B-014, C-1). `AddSpechtEngine`
registers it in the ordinal order of its key, after `MissingScenarioRule`
(`SPEC023`) and before `ClaimIdRule` (`SPEC030`). `FaultIsolatedRule` wraps it
as it wraps every rule. `0.4.0` is a minor above `0.3.1`, so `0001-F7` B-066
holds with no `opt_in` mark on the page (`0001-F7` OQ-78).

**The level.** Warning by default: OQ-5's level, as `0201-F7` OQ-5 chose for
`SPCT0012`. It is written in two places, each read by its own path, as
`WaiverRule`'s `Off` is:

| Where                                                                               | Read by                                                                                                                                                                                   |
| ----------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `DefaultLevel => SpechtRuleLevel.Warning` on the rule                               | the evaluator, when the manifest's `rules` does not name `SPEC4011`; `SpechtManifestParser` does not fill `rules` from the shipped manifest, so this is every manifest that leaves it out |
| `"SPEC4011": "warning"` in the shipped `0.4.0` manifest's `rules`, after `SPEC4010` | `init`, which writes the pinned rules at their levels; `InitRuleDefaultsUnitTests` holds each level to its page's Default severity                                                        |

The evaluator stamps a warning, so the check exits `0` on it, and `1` under
`--strict`. `rules` or `specht rules set SPEC4011 error` raises it, and `off`
stops it running.

**The rule page.** `docs/rules/v0.4.0/SPEC4011.md`, in the shape of
`SPEC4008.md`:

- Frontmatter: a `title`, a `description` and `type: rule`.
- Metadata: Rule ID `SPEC4011`, Family `Feature file` (as `SPEC4001`,
  `SPEC4002`, `SPEC4007` and `SPEC4008`), Default severity `Warning`, Schema
  version `0.4.0`.
- Cause: the message above in its table.
- Rule description: it reads only under `claim.source` `gherkin`, and only the
  `.feature` files a set's glob matches. It covers inheritance from a Rule and
  the Feature, `Example:` and `Scenario Template:`, and the four readings above.
- An example violation, the corrected file and how to fix.

`0001-F3` B-031 holds `0.4.0`'s pages to its vocabulary, one each, so the page
and the vocabulary row land in one commit. `specht --explain SPEC4011` prints
it under a `0.4.0` pin.

**The relock.** No published package carries `0.4.0`; `0.6.4-preview` locks
`0.1.0` to `0.3.1`, as item `0336` recorded. The digest covers the four
shipping trees, so the page and the manifest line change it. By `0001-F7`
decision 0014's procedure, `0.4.0`'s entry in
`src/tool/schema/versions.lock.json` is removed by hand, then
`./build.sh LockSchemaVersion` records the newest version again (`0001-F7`
C-17, B-092). The `0.1.0` to `0.3.1` entries and their files stay
byte-identical (`0001-F7` C-1).

**The docs.** `docs/what-it-checks.md` gains a `SPEC4011` row after
`SPEC4010`'s. In `docs/configuration.md`, "No rule reads Gherkin claims yet."
becomes a sentence naming `SPEC4011`.

**Silent without `gherkin` (B-002, C-3).** Two gates, either one enough:

1. Below `0.4.0` no vocabulary holds the id, so the rule is neither run nor
   counted (B-028).
2. Under `0.4.0`, without `claim.source` `gherkin`, `ClaimSets` returns no set,
   so the rule runs and finds nothing. A companion `.feature` beside a Markdown
   specification is no claim file: its untagged scenarios are not reported.

Under `0.4.0` every tree counts one more rule evaluated, as it did for
`SPEC4008` and `SPEC4010`. That is the vocabulary growing, not a verdict.
`0001-F7` B-154 compares patch neighbours only, and `0.3.1` and `0.4.0` are not
(`0001-F7` OQ-75, OQ-78). No golden report pins `0.4.0`: the two are the
`0.1.0` baseline and `engine-v0.3.0.json`. This repository's tree is Markdown,
so its self-check gains the count and no violation.

**Determinism (B-030).** Sets in manifest order, files in ordinal path order,
findings in line order, and the evaluator's order sorts the report.

**Testability.** The mechanisms that prove B-010, for `test-writer`'s § 8 and
§ 9:

- The rule from the composed rule set, through `SpechtEvaluator`, over a model
  `SpechtModelReader.Read` builds from a `MockFileSystem` tree through
  `EngineServices`, as `SpechtModelReader.NativeClaims.Unit.Tests` does. The
  stamped id, level, path, line and identifier are all observable:

  | Case                                                                             | Expected                                                                   |
  | -------------------------------------------------------------------------------- | -------------------------------------------------------------------------- |
  | an untagged `Scenario:`, `Example:`, `Scenario Outline:` or `Scenario Template:` | one `SPEC4011` warning at its keyword line, the file's path, no identifier |
  | its tags on lines above the keyword                                              | the keyword's line, not a tag's                                            |
  | a claim tag on the scenario, on its Rule, or on the Feature                      | none                                                                       |
  | a Rule with a claim, then a later Rule without                                   | only the later Rule's scenarios                                            |
  | a planned tag, or only ordinary or mistyped tags                                 | reported                                                                   |
  | a claim tag on an Examples block only                                            | the outline reported                                                       |
  | a claim id in the title only                                                     | reported, and no claim                                                     |
  | a file in another language, or with no `Feature:` line                           | none                                                                       |
  | one file two sets match                                                          | each scenario once                                                         |

- The level: `DefaultLevel` is `Warning`; a manifest `rules` entry of `error`
  stamps an error and `off` does not run it; `--strict` exits `1` on the
  warning alone.
- Silence: a `0.4.0` Markdown tree with an untagged companion scenario gives
  no `SPEC4011` (B-002); a `0.3.1` pin with `claim.source` `gherkin` does not
  evaluate the id, and its rule count is unchanged (B-028, C-3).
- The vocabulary: `ShippedVocabularies`' `0.4.0` row gains `SPEC4011`;
  `ZeroFourZeroRuleNames` gains `SPEC025` to `SPEC4011`; the composition id
  list gains `SPEC025`. The rule-page tests (`0001-F3` B-031),
  `InitRuleDefaultsUnitTests`, the real-tree numbering test (`0001-F7` B-066)
  and the lock tests take the page, the manifest line and the relock without
  a new test.
- Acceptance: the two `@B-010` scenarios and the `@boundary` title scenario,
  in `NativeClaimsSteps`.

**Reuse.** The reader's `SPCT0012` finding, `SpechtModelBuilder.ClaimSets`,
`SpechtClaims`, `SpechtModel.ClaimSets`, the evaluator's selection and stamp,
`SchemaVersions.Shipped` and `LockSchemaVersion` carry the work. Added: one
rule and one member.

**Rejected.**

- Recomputing inheritance in the rule from `Declaration.Scenarios`. That
  member holds only the scenarios that carry a claim, and the rest are not in
  the model, so the rule would need a second Gherkin reading (A-3, C-1).
- Reading the claim files again in the rule: C-5's second parse.
- Walking `model.Features` with a `ClaimFileSet` through `model.Claims`. These
  are the same reads, reached through a Feature the rule does not use, and a
  set that makes no Feature has no claim file (B-036).
- The reader's text as the message. It is the analyzer's wording, capitalised
  and ending with a period, where an engine message is neither. Getting the
  title out of it would be a second reading.
- A title member on `GherkinClaimTextFinding`. It would change a record the
  analyzer compiles, for a message no claim asks for (§ 5 row 1).
- A member holding only the `SPCT0012` findings. The whole read is already in
  hand, and item `0394` reads `SPCT0011` from it.
- The key `SPEC4011` with a map entry to itself. The rules that entered
  `0.4.0` are keyed by their working names.
- A constant for `SPCT0012` in the shared reader: an edit to code the analyzer
  compiles, for one engine caller.

### A claim tag on an Examples block, `SPEC4012` (item `0394`)

Built. B-009: a tag on an Examples block that the claim tag form and the
claim grammar read is reported at its claim file's path and the tag's line,
at error by default, and declares no claim. The id is `SPEC4012` (decision
0004). The shared reader already finds such a tag and already records no
claim for it; this item reports what it found, as item `0393` reports
`SPCT0012`.

**What the reader finds.** On an `Examples:` or `Scenarios:` line,
`GherkinClaimTextReader`'s `Keyword` gives the tags pending above it to the
block, not to an `Element`, and adds one `SPCT0011` finding for each of
those tags the composed claim form reads (`0201-F7` B-028). The finding's
line is the tag's line. The claim form is the one B-004 to B-007 use, with
`claim.tagIgnoreCase`, so what counts as a claim tag is the reading every
other native rule has (A-3, C-1). Two claim tags on one block are two
findings, in tag order. A file the reader cannot read throws, so it has no
finding; reporting that file is `SPEC4013`'s (B-014, B-015, item `0395`).

**No claim recorded, by construction.** Only an `Element`'s tags declare a
claim, and an Examples block's tags are on no `Element`. The tag is in no
`GherkinClaimDeclaration`, so `SpechtClaims` holds no claim for it and no
claims rule sees it: it is no duplicate of a scenario's claim (`SPEC4003`),
and the § 9 rules have no claim to match a row to. B-009's "records no
claim" holds before this item, and this item adds no code for it.

**The claim id the finding carries.** B-009's scenario names `B-005` on the
reported line. The finding's `Text` holds the tag inside the analyzer's
sentence, and the finding has no member holding it. The reader already reads
the claim id, in the test that selects the tag, and dropped it.
`GherkinClaimTextFinding` has a trailing optional member,
`string? ClaimId = null`, and the reader's `Keyword` passes the id it read to
the `SPCT0011` finding; `SPCT0012`'s leaves it null. The analyzer compiles the
record and the reader and reads neither the member nor anything new:
`GherkinClaimFileReader` copies `DiagnosticId`, the path, `Line` and `Text`
into its `BindingFinding`. The analyzer's findings, lines, texts and export
are unchanged (§ 5 row 1), and the two readers stay one reader (§ 5 row 2).
Item `0393` rejected a title member on the same record because no claim asks
for a title; B-009's scenario asks for the claim id. The reader and the
record are the engine's own files in `src/specht/Model/`, where `0201-F7`
places them; the analyzer links them as source.

**The rule.** `ExamplesClaimTagRule`, in
`src/specht/Rules/NativeClaims/ExamplesClaimTagRule.cs`, namespace
`Specht.Rules.NativeClaims`, beside `ScenarioWithoutClaimRule`. `Evaluate`
walks `model.ClaimSets`, then each set's `SpechtClaims.Files`, keeping the
first read of each `RelativePath` under ordinal comparison, as item `0393`'s
rule does. Each finding whose `DiagnosticId` is `SPCT0011` becomes one
`SpechtFinding`:

| Member       | Value                                                                                        |
| ------------ | -------------------------------------------------------------------------------------------- |
| `File`       | the claim file's root-relative path, from discovery (C-2): B-009's `add.feature(14)`         |
| `Line`       | the finding's line, the tag's line, never the `Examples:` keyword line below it              |
| `Identifier` | the finding's `ClaimId`, the claim id the tag would declare, so the line ends with `[B-005]` |
| `Message`    | `claim tag on an Examples block declares no claim`                                           |

The bracketed identifier is how `SPEC4003` names `[B-001]`. The message
names no section, marker or manifest key, so `0001-F5` B-009's message
theories do not take it. `SPCT0011` is the shared reader's finding id, not a
manifest literal (C-4).

**A file two sets match.** Until item `0395`, such a file is listed under
both sets and read by the one reader, so both reads carry the same findings.
The first-read-per-path filter reports each tag once, as for `SPEC4011`.
Item `0395` keeps the file's read where this rule finds it, or moves this
filter with it. The filter is then written in two rules, a few lines each; a
shared helper waits for a third caller.

**Readings B-009 does not spell out.** Each follows from the reader as built,
and the analyzer reports the same (A-3):

- A tag the claim form does not read, an ordinary tag, a mistyped claim tag
  or a `claim.plannedTags` tag, is not reported. A planned tag on an Examples
  block marks its rows planned (B-013), which is item `0390`'s.
- A claim tag on each of several Examples blocks of one outline is reported
  once per tag, each at its own line.
- An outline whose only claim tag is on an Examples block is also reported by
  `SPEC4011`: the tag declares nothing, so the outline has no claim. An
  outline that carries its own claim, its Rule's or its Feature's is not, and
  `SPEC4012` still reports the tag.
- A claim tag above an `Examples:` line with no outline above it is no
  finding: the reader throws, and the file is `SPEC4013`'s.

**The id and the vocabulary.** The rule's key is `SPEC026`, its working name,
as `ScenarioWithoutClaimRule`'s is `SPEC025` (decision 0004's table).
`SchemaVersions.Shipped`'s `0.4.0` row gains `["SPEC026"] = "SPEC4012"`. No
lower row holds `SPEC026`, so the evaluator neither runs nor counts the rule
below `0.4.0` (`0001-F7` B-014, C-1). `AddSpechtEngine` registers it in the
ordinal order of its key, after `ScenarioWithoutClaimRule` (`SPEC025`) and
before `ClaimIdRule` (`SPEC030`). `FaultIsolatedRule` wraps it as it wraps
every rule. `0001-F7` B-066 holds with no `opt_in` mark, `0.4.0` being a
minor above `0.3.1` (`0001-F7` OQ-78).

**The level.** Error by default, OQ-5's level. `ISpechtRule.DefaultLevel` is
already `Error`, so the rule declares none. The shipped `0.4.0` manifest's
`rules` gains `"SPEC4012": "error"` after `SPEC4011`, for `init` and
`InitRuleDefaultsUnitTests`. The check exits `1` on it; `rules` or
`specht rules set SPEC4012 warning` lowers it, and `off` stops it running.

**The rule page.** `docs/rules/v0.4.0/SPEC4012.md`, in the shape of
`SPEC4011.md`:

- Frontmatter: a `title`, a `description` and `type: rule`.
- Metadata: Rule ID `SPEC4012`, Family `Feature file`, Default severity
  `Error`, Schema version `0.4.0`.
- Cause: the message above in its table.
- Rule description: it reads only under `claim.source` `gherkin`, and only
  the `.feature` files a set's glob matches, and it covers the readings
  above.
- An example violation, the corrected file with the tag moved to the outline,
  and how to fix.

`0001-F3` B-031 holds the page and the vocabulary row to one commit.
`specht --explain SPEC4012` prints it under a `0.4.0` pin.

**The relock.** As for `SPEC4011`: `0.4.0`'s entry in
`src/tool/schema/versions.lock.json` is removed by hand, then
`./build.sh LockSchemaVersion` records it again (`0001-F7` decision 0014,
C-17, B-092). The page and the manifest line change the digest. The `0.1.0`
to `0.3.1` entries and their files stay byte-identical (`0001-F7` C-1).

**The docs.** `docs/what-it-checks.md` gains a `SPEC4012` row after
`SPEC4011`'s. `docs/configuration.md`'s sentence naming `SPEC4011` gains
`SPEC4012`, at error, for a claim tag on an Examples block.

**Silent without `gherkin` (B-002, B-028, C-3).** The two gates of
`SPEC4011`, either one enough:

1. Below `0.4.0` no vocabulary holds the id, so the rule is neither run nor
   counted, and a `0.3.1` pin with `claim.source` `gherkin` keeps its rule
   count (B-028).
2. Under `0.4.0`, without `claim.source` `gherkin`, `ClaimSets` returns no
   set, so the rule runs and finds nothing. A companion `.feature` beside a
   Markdown specification is no claim file: a claim tag on its Examples block
   is not reported.

Under `0.4.0` every tree counts one more rule evaluated: the vocabulary
growing, not a verdict, and `0001-F7` B-154 compares patch neighbours only
(`0001-F7` OQ-75, OQ-78). No golden report pins `0.4.0`. This repository's
tree is Markdown, so its self-check gains the count and no violation.

**Determinism (B-030).** Sets in manifest order, files in ordinal path order,
findings in line order, and the evaluator's order sorts the report.

**Testability.** The mechanisms that prove B-009, for `test-writer`'s § 8 and
§ 9:

- The rule from the composed rule set, through `SpechtEvaluator`, over a
  model `SpechtModelReader.Read` builds from a `MockFileSystem` tree through
  `EngineServices`, as `ScenarioWithoutClaimRule.Unit.Tests` does:

  | Case                                                        | Expected                                                              |
  | ----------------------------------------------------------- | --------------------------------------------------------------------- |
  | a claim tag on an `Examples:` or `Scenarios:` block         | one `SPEC4012` error at the tag's line, the file's path, the claim id |
  | the tag on a line above other tags, or above the keyword    | the tag's own line                                                    |
  | two claim tags on one block, or one on each of two blocks   | one each                                                              |
  | an ordinary, mistyped or planned tag on the block           | none                                                                  |
  | a claim tag in another case under `claim.tagIgnoreCase`     | reported, with the id as written                                      |
  | the outline's only claim tag on its block                   | the tag reported, the outline under `SPEC4011`, and no claim          |
  | the outline claimed by its own tag, its Rule or its Feature | the tag reported, no `SPEC4011`                                       |
  | the block's claim id declared nowhere else                  | the Feature has no such claim, and no `SPEC4003`                      |
  | one file two sets match                                     | each tag once                                                         |
  | a file the reader cannot read                               | none                                                                  |

- The reader: the `SPCT0011` finding carries the claim id, and `SPCT0012`'s
  carries none; the analyzer's findings are unchanged, under its existing
  tests.
- The level: the rule's level is `Error`; a manifest `rules` entry of
  `warning` stamps a warning and `off` does not run it.
- Silence: a `0.4.0` Markdown tree with a claim tag on a companion's Examples
  block gives no `SPEC4012` (B-002); a `0.3.1` pin with `claim.source`
  `gherkin` does not evaluate the id, and its rule count is unchanged (B-028,
  C-3).
- The vocabulary: `ShippedVocabularies`' `0.4.0` row gains `SPEC4012`;
  `ZeroFourZeroRuleNames` gains `SPEC026` to `SPEC4012`; the composition id
  list gains `SPEC026`. The rule-page tests (`0001-F3` B-031),
  `InitRuleDefaultsUnitTests`, the real-tree numbering test (`0001-F7`
  B-066) and the lock tests take the page, the manifest line and the relock
  without a new test.
- Acceptance: the `@B-009` scenario, in `NativeClaimsSteps`. Its line names
  `SPEC4012` and the claim id as the identifier, `[B-005]` at the line's end.

**Reuse.** The reader's `SPCT0011` finding, `SpechtClaims.Files`,
`SpechtModel.ClaimSets`, the evaluator's selection and stamp,
`SchemaVersions.Shipped` and `LockSchemaVersion` carry the work. Added: one
rule and one optional member on the shared finding.

**Rejected.**

- The tag taken back out of the finding's `Text`, between `Tag ` and
  ` declares`, and read again by the claim form. It ties an engine rule to
  the analyzer's sentence, so a rewording there breaks a check here, and it
  is a second reading of a tag the reader has already read (A-3).
- A null identifier and a message naming no claim. It needs no shared edit,
  but B-009's line names `B-005`, and the output could not say which of
  several tags on one line is meant.
- The tag, `@B-005`, as the identifier. Every other claims rule names the
  claim id, and the tag form is the consumer's.
- Reading the claim files again in the rule: C-5's second parse.
- A list of Examples tags on `GherkinClaimText`. The finding is already the
  reader's account of the tag; a second list says it twice.
- Recording the tag's claim and marking it misplaced. B-009 records no claim,
  and the reader declares only from an `Element`.
- The key `SPEC4012` with a map entry to itself: the rules that entered
  `0.4.0` are keyed by their working names.
- A `DefaultLevel` override returning `Error`: it restates the interface's
  default.

### An unreadable claim file, set or record, `SPEC4013` (item `0395`)

Built. One rule for claim input the check cannot take claims from: a claim
file in another language (B-014) or any other text the reader refuses (B-015), a set
whose glob matches no file (B-016), a file two sets match (B-018), a
Feature-level tag naming another identity (B-019), and a `record` that cannot
be read (B-033) or is absolute (B-034). The id is `SPEC4013` (decision 0004),
at error by default. B-035's reading, the set read as record-less, is item
`0391`'s and stays as built; this item reports it.

**Where each fault is found.** Four of the seven conditions already change
what the model holds: an unreadable file declares nothing, a record that
cannot be read is dropped, and B-018 drops a shared file's claims. The place
that drops each one records why, once, and the rule reads that record. The
two that change nothing, B-016 and B-019, are found where their data is
already in hand, so no verdict is split between a store and a rule.

| Claim | Found in                       | From                                                                            |
| ----- | ------------------------------ | ------------------------------------------------------------------------------- |
| B-014 | `SpechtModelBuilder.ClaimSets` | the per-file catch of `GherkinClaimTextException`, at the header's line         |
| B-015 | `SpechtModelBuilder.ClaimSets` | the same catch, at line `0`                                                     |
| B-016 | `SpechtModelBuilder.ClaimSets` | a set whose listed files are none                                               |
| B-018 | `SpechtModelBuilder.ClaimSets` | a path in the listings of two or more of the sets it is given                   |
| B-019 | `SpechtModelBuilder.ClaimSets` | each Feature-level scope value the reader captured, against the set's `feature` |
| B-033 | `SpechtModelReader.Read`       | a `record` `TryLoad` returns `null` for, not rooted                             |
| B-034 | `SpechtModelReader.Read`       | a `record` `TryLoad` returns `null` for, rooted                                 |

**The fault.** One record, `SpechtClaimFault(string Path, int Line,
SpechtClaimFaultKind Kind, string? Identifier)`, in `src/specht/Model/`, and
its kind, an enum with one value per row of the message table below. It holds
no message: the wording is the rule's. `SpechtModelBuilder.Build`'s optional
`recordFaults` argument carries the model reader's faults, `SpechtClaims.Faults`
each set's from the builder, and `Build` joins them, the reader's first, as
`SpechtModel.ClaimFaults`, distinct under record equality, so a file two sets
both read and fail on is one fault. Both lists are empty unless `claim.source`
is `gherkin`.

**An unreadable claim file (B-014, B-015).** `GherkinClaimTextException`
gains `int Line`, an optional constructor argument, default `0`. The reader's
`CheckLanguage` passes the one-based line of the `# language:` header it
refuses; every other throw leaves `0`. The analyzer links the exception as
source, and `GherkinClaimFileReader` copies only `RelativePath` and `Message`
into its `GherkinInputException`, so its failures are unchanged (§ 5 rows 1
and 2). The builder's catch, today an empty block, adds one `UnreadableFile`
fault at the file's path and the exception's line. The file stays out of
`Files`, so `SPEC4011` and `SPEC4012` see nothing of it, and it declares no
claim, as today. The reader throws one exception, with one message, for every
text it does not parse, and B-015 names each of them (OQ-19).

**A set matching no file (B-016).** A set whose listed files are none gets one
`EmptySet` fault: `Path` the set's `glob` as the manifest writes it, line `0`,
`Identifier` the set's `feature`, which tells two such sets apart. The glob
is relative by B-001, so the path is root-relative as written (C-2). A set
with a `record` and no file is still a Feature (B-036 is for a record-less
set), and it is reported the same way.

**A file two sets match (B-018).** `ClaimSets` counts each path across the
listings of the sets it is given, ordinally. A path in two or more is
`shared`:

- each set reading it gets a `SharedFile` fault at that path, line `0`, one
  after the join;
- `SpechtClaims`' set constructor takes an optional `unclaimed` path set and
  skips the declarations of those files, so the claims are under neither set
  (B-018), and no rule of `0001-F12` or `0001-F13` sees them;
- the read stays in `Files`. `SPEC4011` and `SPEC4012` report what the reader
  found in the file's text, which does not depend on which set owns it, so the
  first-read-per-path filter of items `0393` and `0394` keeps reporting each
  scenario and each Examples tag once. The test that pins the once-report
  holds.

B-018 does not touch identity. Two sets naming one identity are `SPEC3002`'s
at each set (B-017), whether or not they share a file. A set whose only files
are shared is still a Feature, at its first claim file (B-026), with no
claim; a § 9 row in its record naming a dropped claim is what `SPEC4004` and
`0001-F12` say of a row with no claim (B-023). `MatrixResolver.ResolveClaims`
reads `model.Claims[feature]`, so it sees the set without them.

`0001-F19`'s collision guard calls `ClaimSets` with one Feature's sets alone,
so a file it shares with another Feature's set is not shared among the sets it
passes, and its claims stay held. The guard then refuses to mint an id the
file uses, which is the cautious answer; no claim of `0001-F19` asks it to
change.

**A Feature-level tag naming another identity (B-019).** `ClaimSets` builds the
reader's `FeatureScope` from `ClaimInput.FeatureScopeTag` when it is set and
holds exactly one `{feature}`: `GherkinClaimTagForm(form, "feature", false,
null)`, the placeholder, case and grammar the analyzer's
`GherkinClaimPolicyReader` gives it (A-3). Any other form is read as no form
(OQ-20), so the form's constructor never sees one it refuses. The reader
already reads each Feature-level tag through it into `FeatureTagValues`, with
no line. `GherkinClaimText` gains a trailing optional member,
`IReadOnlyList<int>? FeatureTagLines = null`, the line of each value at the same
index, filled by the reader beside the values. The analyzer reads only the
values, so its findings are unchanged. Each value that differs ordinally from
the set's `feature` gives one `FeatureTag` fault at the file's path and that
tag's line, `Identifier` the value captured. A file with no Feature-level tag,
or one that agrees, gives none. B-019, unlike B-014, B-015 and B-018, says
nothing of the file's claims, so they are kept. `claim.scopes.epicTag` stays
unread: no claim of this Feature reads it, so the reader's `EpicScope` stays
`null`.

**A record that cannot be read (B-033, B-034).** `SpechtTreeStore` gains
`IsRooted(path)`, the call `TryLoad`'s first guard makes, which `TryLoad` now
makes through it, so the two never disagree. For each set with a `record`
that `TryLoad` returns `null` for, `SpechtModelReader.Read` passes one fault to
`Build`:

| Case     | Fault                                                                                                                       |
| -------- | --------------------------------------------------------------------------------------------------------------------------- |
| rooted   | `AbsoluteRecord` at `SpechtManifestStore.RelativePath`, `.specht/manifest.json`, line `0`, `Identifier` the set's `feature` |
| any else | `UnreadableRecord` at the `record` as the manifest writes it, line `0`, no identifier                                       |

A rooted path never reaches a violation (C-2; OQ-17). "Any else" is
`TryLoad`'s other three `null` cases: a path that leaves the root, names no
file, or cannot be read. Its path as written is relative, `..` segments
included. The set is read as record-less, as item `0391` built (B-035), so a
record-less set with no claim file is no Feature (B-036) and is reported
twice, once for its record and once for its glob.

**The rule.** `ClaimInputFaultRule`, in
`src/specht/Rules/NativeClaims/ClaimInputFaultRule.cs`, key `SPEC027`. Its
`Evaluate` maps each of `model.ClaimFaults` to one `SpechtFinding`, with
`File` from `Path`, `Line` and `Identifier` as they stand, and `Message` from
the kind:

| Kind               | Message                                                                    |
| ------------------ | -------------------------------------------------------------------------- |
| `UnreadableFile`   | `claim file cannot be read as English Gherkin, so it declares no claim`    |
| `EmptySet`         | `claim file set matches no claim file`                                     |
| `SharedFile`       | `claim file matches more than one claim file set, so it declares no claim` |
| `FeatureTag`       | `Feature-level tag names a Feature other than its claim file set's`        |
| `UnreadableRecord` | `claim file set's Feature document cannot be read under the root`          |
| `AbsoluteRecord`   | `claim file set's Feature document is an absolute path`                    |

No message names a manifest key, so `0001-F5` B-009's message theories do not
take them (C-4). A line-`0` violation prints with no position, as
`SpechtViolation.ToString` prints one today.

**The id, the level, the page and the relock.** As for `SPEC4011` and
`SPEC4012`:

- `SchemaVersions.Shipped`'s `0.4.0` row gains `["SPEC027"] = "SPEC4013"`, and
  no lower row holds `SPEC027` (`0001-F7` B-014, C-1).
- `AddSpechtEngine` registers it after `ExamplesClaimTagRule` (`SPEC026`) and
  before `ClaimIdRule` (`SPEC030`).
- Error by default: `ISpechtRule.DefaultLevel`, no override, and
  `"SPEC4013": "error"` after `SPEC4012` in the shipped `0.4.0` manifest.
- `docs/rules/v0.4.0/SPEC4013.md`, Family `Feature file`, Default severity
  `Error`, Schema version `0.4.0`, the message table above, an example and the
  fixes. `0001-F3` B-031 holds it to the vocabulary row.
- The relock: `0.4.0`'s lock entry removed by hand, then
  `./build.sh LockSchemaVersion`; `0.1.0` to `0.3.1` byte-identical
  (`0001-F7` decision 0014, C-1, C-17).
- `docs/what-it-checks.md` gains a `SPEC4013` row; `docs/configuration.md`'s
  sentence naming `SPEC4011` and `SPEC4012` gains it.
- `0001-F7` B-066 holds with no `opt_in` mark, `0.4.0` being a minor above
  `0.3.1` (`0001-F7` OQ-78).

**Silent without `gherkin` (B-002, B-028, C-3).** Below `0.4.0` no vocabulary
holds the id, so the rule is neither run nor counted. Under `0.4.0` without
`claim.source` `gherkin`, `SpechtModelReader.Read` lists no set and
`ClaimSets` returns none, so `ClaimFaults` is empty. The reader gains a scope
form only under `gherkin`. Every `0.4.0` tree counts one more rule evaluated,
and this repository's self-check gains the count and no violation.

**Determinism (B-030).** Faults in set order and ordinal path order, and the
evaluator's order sorts the report.

**Resolved points.** The design left two points open, as behaviour no claim
stated; § 11 resolves both, and the build follows:

1. **Other text the reader refuses** (OQ-19, resolved (a)). Every text the
   reader refuses but a `# language:` header is reported at line `0`, and B-015
   names each cause: no `Feature:` line, a second one, a ragged table, a
   malformed tag token, an unterminated doc string, tags followed by no
   element, and an `Examples:` with no outline above it. The catch needs no
   way to tell them apart. Item `0394`'s reading of an `Examples:` with no
   outline as `SPEC4013`'s holds.
2. **A `featureTag` without exactly one `{feature}`** (OQ-20, resolved (a)).
   It is read as no form: B-019 reports nothing and nothing throws. Item
   `0390`'s parser still accepts the form, and no exit `3` is added.

**Testability.** The mechanisms that prove each claim, for `test-writer`'s § 8
and § 9:

- The rule from the composed rule set, through `SpechtEvaluator`, over a model
  `SpechtModelReader.Read` builds from a `MockFileSystem` tree, as
  `ExamplesClaimTagRule.Unit.Tests` does:

  | Case                                                              | Expected                                                              |
  | ----------------------------------------------------------------- | --------------------------------------------------------------------- |
  | `# language: fr` on line 2                                        | one `SPEC4013` at the file, line 2; no claim from it                  |
  | `# language: en`, or no header                                    | none                                                                  |
  | no `Feature:` line, or an empty file                              | one at the file, line `0`; no claim from it                           |
  | a set's glob matching no file, with and without a `record`        | one at the glob as written, line `0`, the set's `feature`             |
  | one file two sets match                                           | one at the file, line `0`; neither set's claims hold its ids          |
  | that file's untagged scenario and Examples claim tag              | `SPEC4011` and `SPEC4012` once each, as before                        |
  | `featureTag` set and a Feature tag capturing another id           | one at the tag's line, the captured id; the file's claims kept        |
  | the Feature tag agreeing, or absent, or `featureTag` unset        | none                                                                  |
  | a `record` naming no file, a directory, or `../outside/README.md` | one at the path as written, line `0`; the set record-less             |
  | a `record` that is rooted                                         | one at `.specht/manifest.json`, line `0`; no rooted path in any field |

- `SpechtTreeStore.TryLoad`'s four `null` cases, rooted, leaving the root, no
  file and a read that throws, and `IsRooted` agreeing with the first; item
  `0391`'s review left them to this item.
- The reader: a refused `# language:` header's exception carries its line,
  every other carries `0`; `FeatureTagLines` holds each Feature-level scope
  value's line; the analyzer's tests stand unchanged.
- The level: the rule's level is `Error`; a manifest `rules` entry lowers or
  silences it.
- Silence: a `0.4.0` Markdown tree, and a `0.3.1` pin with `claim.source`
  `gherkin`, with each condition above, give no `SPEC4013`, and the `0.3.1`
  rule count is unchanged (B-002, B-028, C-3).
- The vocabulary: `ShippedVocabularies`' `0.4.0` row gains `SPEC4013`,
  `ZeroFourZeroRuleNames` gains `SPEC027` to `SPEC4013`, and the composition id
  list gains `SPEC027`.
- Acceptance: the `@B-014` to `@B-016`, `@B-018`, `@B-019` and `@B-033` to
  `@B-035` scenarios. Their lines are error lines, and the step "the output has
  a line" is bound to a warning line alone since item `0393`, so the step is
  widened to any line starting with the text given.

**Reuse.** `SpechtModelBuilder.ClaimSets`' per-file catch,
`SpechtListing.ClaimFiles`, `SpechtTreeStore.TryLoad`, the reader's scope
form and `FeatureTagValues`, `SpechtClaims`, the evaluator's selection and
stamp, `SchemaVersions.Shipped` and `LockSchemaVersion` carry the work. Added:
one rule, one fault record and its kind, one store method, one optional member
each on the shared exception and the shared read.

**Rejected.**

- Each condition found in the rule. B-018's shared paths and B-035's dropped
  record are found where the model drops them; finding them again in the rule
  would be the same verdict in two places, and the rule has no file system to
  tell a rooted path from another.
- A message in the fault. The wording is a rule's, as every other rule's is.
- The exception's `Message` as the violation's message: the analyzer's
  sentence, capitalised and ending with a period, and the same text for every
  refused parse.
- Dropping a shared file's read from `Files` as well as its claims. It would
  silence `SPEC4011` and `SPEC4012` on the file, which B-018 does not ask, and
  reverse the once-report item `0393` pins.
- `TryLoad` returning why it gave `null`. It changes a signature `0001-F19`'s
  guard also calls, for the one case, rooted, the caller needs.
- A list of `(value, line)` pairs in place of `FeatureTagValues`. The analyzer
  reads the values; a second list beside them leaves its reading as it is.
- Reading `claim.scopes.epicTag`: no claim of this Feature reads it.
- The record faults on `SpechtListing`, as first built. `Specht.Discovery`
  sits below `Specht.Model` in ADR-0010's layout, so the listing cannot hold a
  model type without the two depending on each other (review round 1). Moving
  the fault into `Specht.Discovery` would put the builder's and the rule's
  type below them for one caller. The reader finds the record faults and hands
  them to `Build` directly.

## 8. Testing Strategy

<!-- last written by: test-writer, 2026-11-14 (item 0395: SPEC4013's tests, red at `4a441831`) -->

Items `0390` to `0396` each add their subsection after this one.

### The claim members at schema 0.4.0 (item `0389`)

**Unit.** `SpechtManifestParserClaimMembersUnitTests`, in
`test/tests/Manifest/SpechtManifestParser.ClaimMembers.Unit.Tests.cs`, parses in
memory, with no file system, through `EngineServices.Parser()`, over the shipped
catalog, whose `0.4.0` manifest carries § 7's `claim` defaults and `ids`. Each
row's manifest members come from a `[ClassData]` type beside the class.
Test-writer added the declarations of the types § 7 names, with no reading, so
the tests compiled before the parser read them: `SpechtClaimSource`,
`SpechtClaimFileSet` and `SpechtClaimInput` under `src/specht/Manifest/`, and
`SpechtStructure.ClaimInput`.

- **B-002** - mechanism: the source `SpechtManifestParser.Parse` reads under
  `ClaimMembersVersion`, `Markdown` when the member is left out, with nothing read
  under `codeAnalysis` (C-7). Test:
  `AManifestWithoutAGherkinClaimSource_WhenParsedUnderZeroFourZero_ShouldReadMarkdownClaimsAndNoClaimFileSet`,
  over `MarkdownClaimSources`: no `claim`, an empty `claim`, `claim.tag` alone,
  `source` `markdown`, and a `codeAnalysis.records` that selects Gherkin claim
  file sets. Red: `ClaimInput` is null.
- **B-027** - mechanism: the identity `ClaimInput` reads from each entry over the
  `DeclaresEpicGrammar` reading. Tests:
  `AClaimFileSet_WhenParsedUnderEitherEpicGrammar_ShouldTakeTheIdentityThatGrammarGives`,
  over `ClaimFileSetIdentities`: epic `0001` and Feature `arithmetic` under a
  declared `identifiers.epic`, and Feature `arithmetic` with no epic under none;
  red, `ClaimInput` is null. And two rows of
  `AnInvalidClaimFileSet_WhenParsedUnderZeroFourZero_ShouldBeRejectedNamingClaimFiles`:
  an `epic` under no epic grammar, and no `epic` under a declared one; red, no
  exception is thrown.
- **B-028** - mechanism: the `ClaimMembersVersion` gate. Guard:
  `AClaimMemberUnderAPinBelowZeroFourZero_WhenParsed_ShouldReadTheManifestAsWithoutIt`,
  over `ClaimMembersBelowZeroFourZero`: each shipped pin that reads `claim`
  (`0.2.0`, `0.2.1`, `0.3.0`, `0.3.1`) with a valid Gherkin selection, with
  `source` `gherkin` and no `claim.files` (a B-029 fault at `0.4.0`), and with
  every other member; the structure equals, member by member, the one parsed
  without the members. Green when written: it is the parser as built (decision
  0002), as `0001-F5` B-072's are. `0.1.0` is not a row: it refuses `claim`
  whole, as today, which
  `ClaimTagManifestUnitTests.ANewTagKeyUnderAnOlderPin_WhenParsed_ShouldRemainUnknown`
  pins; and the tag reading the first B-028 scenario keeps is
  `ClaimTagManifestUnitTests.OmittedTagSettings_WhenParsed_ShouldUseTheExactPinsDefaults`'s.
- **B-029** - mechanism: the faults `ClaimInput` adds to the list `Check` throws
  as one `SpechtManifestException`, inside `Parse`, so before any schema is read
  or rule runs. Test:
  `AnInvalidClaimFileSet_WhenParsedUnderZeroFourZero_ShouldBeRejectedNamingClaimFiles`,
  over `InvalidClaimFileSets`: under `gherkin`, `claim.files` absent, empty, an
  entry with no `glob`, an entry with no `feature`; and a set beside `source`
  `markdown` and beside no `claim.source`. Each asserts the exact exception type and that its message names
  `claim.files`. Red: no exception is thrown. Exit `3` with stderr alone is the
  existing fold of `SpechtManifestException` (`0001-F5` B-018), not tested again
  here.
- **B-031** - mechanism: the source `SpechtManifestParser.Parse` reads, adding a
  fault to the same list for any value but the text `markdown` or `gherkin`.
  Test:
  `AClaimSourceOutsideTheTwo_WhenParsedUnderZeroFourZero_ShouldBeRejectedNamingClaimSource`,
  over `UndefinedClaimSources`: `"yaml"`, `"Gherkin"` and the number `1`, and a
  declared JSON `null`, which `ClaimInput` reads through `TryGetPropertyValue`
  rather than `Selected`, so it is refused, not filled with the default; each
  with no `claim.files`, so no B-029 fault stands in. The outline names no
  `null`, so `null` has no acceptance row. Each asserts the exact exception
  type and that its message names `claim.source`. Red: no exception is thrown.

**Acceptance.** `native-claims.feature` is linked into
`test/acceptance/acceptance.csproj` with a literal `ReqnrollFeatureFile`, and
`NativeClaimsSteps`, in `test/acceptance/NativeClaims/NativeClaimsSteps.cs`,
scoped to this Feature, binds the Background and the B-002, B-027, B-028, B-029
and B-031 scenarios. The other scenarios bind no step, and
`missingOrPendingStepsOutcome` `Ignore` leaves them out until their items bind
them. A root is a `SpechtTree` holding one co-located Markdown specification,
so a claim read from its claims table is observable, and its manifest declares
no epic grammar, because the Background's claim file set carries no epic and
B-027 makes such a set invalid under one; a scenario that needs the grammar
declares it. The manifest is edited in memory and written when the check runs,
which launches the built tool on the root: an exit code and its streams are
seen nowhere else. "The output of the same tree before this Feature" (B-028)
is written into the step rather than produced by the tool under test: the
synthetic tree's clean output under `0.3.1`, exit `0`, an empty stderr and the
four summary lines `specifications: epics 0, features 1`, `items: 0`,
`rules evaluated: 23` and `errors: 0, warnings: 0`, which `specht.tool`
`0.5.35-preview`, a package that predates this Feature and carries `0.3.1`,
prints for the same tree. Changing the reference turns the scenario red. "The
output without `claim.source`" is a run recorded before the
member is set. "No violation is printed" is stdout holding no `SPEC` diagnostic
line.

**Not asserted**, as no claim of this item states it:

- The type faults § 7 lists: `claim.files` not a list, an entry not an object, a
  `glob`, `feature` or `record` that is not text.
- The defaults `plannedTags`, `withdrawnTags`, `supersededTags` and
  `lifecycleKey` fill (OQ-9), which item `0390` reads; the fault wording beyond
  naming `claim.files`; and one run naming every faulty entry.

**Testability.** The design leaves each mechanism in
`SpechtManifestParser.Parse`, reachable in memory through the shipped catalog,
and each outcome reachable through the built tool. Nothing in this item's scope
is hard to test.

### The native claim reader (item `0390`)

**Unit.** `SpechtModelReaderNativeClaimsUnitTests`, in
`test/tests/Model/SpechtModelReader.NativeClaims.Unit.Tests.cs`, reads a
`MockFileSystem` tree through `EngineServices.Reader(fileSystem)` and observes
`model.ClaimSets`. Each tree pins `0.4.0` and sets `claim.source` `gherkin` with
one claim file set, `.spec/features/arithmetic/*.feature` for Feature
`F1` unless a row names its own. The trees named Feature `arithmetic` until
item `0391`, when B-038 held a set's `feature` to the feature grammar. Each replaces `exclusions` with `obj`:
under B-001 as written a set honours `exclusions`, and the shipped `/.spec`
would exclude every path the Feature's scenarios use (finding 2 below, OQ-10).
A row's manifest members and file text come from a
`[ClassData]` type beside the class. `GherkinClaimTextReaderUnitTests`, in
`test/tests/Model/GherkinClaimTextReader.Unit.Tests.cs`, calls the reader
directly.

The tests were written red at `1782f082`. Test-writer added the declarations
§ 7 names that a test reads, with no behaviour then:

- `GherkinClaimDeclaration.Scenarios`, which the reader filled with an empty
  list;
- `SpechtClaim.Path` and `SpechtClaim.Declaration`;
- `SpechtClaims.FileSet`, then always null;
- `SpechtModel.ClaimSets`, then always empty.

`SpechtListing.ClaimFiles` and the second `SpechtClaims` constructor were left
to implementer, because no test reads them. Implementer built all of them at
`0bd82fb3`, as § 7 records. Each "Red" below is the failure a test gave at
`1782f082`. Every test of this item passes from `0bd82fb3` on.

- **B-001** - mechanism: `SpechtModelReader.Read` expands each set through
  `ISpecDiscovery.FindTestFiles` outside `exclusions`, keeps the `.feature`
  paths, and builds one `SpechtClaims` per set with no claims table. Test:
  `AClaimFileSet_WhenReadUnderAGherkinClaimSource_ShouldBuildItsClaimsFromTheFeatureFilesItsGlobMatchesOutsideExclusionsWithNoClaimsTable`.
  The fixture is a glob `.spec/features/**/*` over `add.feature`, a `notes.md`
  and an excluded `obj/old.feature`. It expects one set, with its `FileSet`, no
  `Section`, and only the rows of `add.feature`. Red: `ClaimSets` is empty.
- **B-004, B-006, B-007** - mechanism: the reader's element walk. It makes one
  declaration per element, at the element's keyword line, with the id captured
  as authored, and joins it to the claim with its path. Test:
  `AClaimTagOnAnElement_WhenRead_ShouldDeclareOneClaimAsAuthoredAtTheElementsKeywordLine`,
  over `ClaimDeclaringElements`. Its rows are:
  - a tagged Feature;
  - a tagged Scenario;
  - a Scenario Outline with three Examples rows (B-006);
  - a Rule holding two untagged scenarios (B-007);
  - a custom `claim.tag` form;
  - `tagIgnoreCase`, keeping `b-001` as authored.

  Each row expects exactly one claim and checks its id, line, path and element.
  Red: `ClaimSets` is empty.

- **B-005** - mechanism: the reader matches each tag against the tag form and
  the claim grammar. Test:
  `ATagTheClaimTagFormOrTheClaimGrammarRejects_WhenRead_ShouldDeclareNoClaim`,
  over `OrdinaryClaimTags`. Its rows are:
  - `@smoke` and `@B-01` beside `@B-001`;
  - a case the form rejects;
  - a bare `@B-002` under a custom form;
  - an `ids.scenario` marker under an `ids.claim` grammar wide enough to match
    it, with and without `tagIgnoreCase`.

  Each row checks the declared ids exactly. Red: `ClaimSets` is empty.

- **B-012** - mechanism: `State` comes from the declaring element's own tags,
  matched against `withdrawnTags` and `supersededTags`. Test:
  `ANativeClaim_WhenRead_ShouldTakeItsStatusFromTheLifecycleTagsTheManifestListsOnItsDeclaringElement`,
  over `ClaimLifecycleTags`. Its rows are:
  - no lifecycle tag, Active;
  - a withdrawn tag, Withdrawn;
  - a superseded tag, Superseded;
  - both tags, Superseded;
  - a lifecycle tag on a scenario under a declaring Feature, Active;
  - both tags under a manifest that leaves both lists out, Active.

  Red: `ClaimSets` is empty.

- **B-013** - mechanism: `Cases` and `Planned` come from `plannedTags` on the
  scenario or on the Examples block. Test:
  `APlannedTagTheManifestLists_WhenRead_ShouldKeepTheClaimAndMarkTheCasesItCoversPlanned`,
  over `PlannedClaimCases`. Its rows are:
  - `@ignore` under the default fill, planned;
  - `@ignore` under `plannedTags` `@wip`, not planned;
  - `@wip` under `plannedTags` `@wip`, planned;
  - an outline whose `@ignore` Examples block has planned rows, beside an
    untagged block whose rows are not.

  Red: `ClaimSets` is empty.

- **B-025** - mechanism: the reader fills `GherkinClaimDeclaration.Scenarios`
  with the keyword lines an element reaches. Test:
  `GherkinClaimTextReaderUnitTests.AClaimTagOnAnElement_WhenRead_ShouldCarryTheKeywordLinesOfEveryScenarioTheElementReachesInDocumentOrder`,
  over `ClaimScenarioSets`. The fixture is one file with a Scenario, an Outline,
  and a Rule with two children. The tag goes on each element in turn and on the
  Feature. Red: `Scenarios` is empty.
- **B-011** - mechanism: a claim's id, set identity and `State` are read from
  tags and the set, never from a title or a position. Test:
  `AClaimFileReorderedAndRetitled_WhenReadAgain_ShouldKeepEveryClaimsIdentifierFeatureIdentityAndStatus`.
  It reads two claims, one of them withdrawn, before and after the scenarios
  are swapped and retitled. Red: the first catalogue is empty.
- **B-030** - mechanism: sets in manifest order, files in ordinal path order,
  claims in document order and tags in line order. Test:
  `ClaimFileSetsAndTheirFiles_WhenRead_ShouldOrderClaimsBySetInManifestOrderThenFileInOrdinalPathOrderThenDocumentAndTagOrder`.
  It uses two sets, the second matching `add.feature` and `subtract.feature`,
  with `@B-002 @B-001` on one line. Red: `ClaimSets` is empty.

The item `0389` review asked that the B-012 and B-013 rows name a test of the
shipped default fill, which § 7 states as two observations:

1. The parser fills the members. Test:
   `SpechtManifestParserClaimMembersUnitTests.AGherkinClaimSourceLeavingOutTheTagListsAndLifecycleKey_WhenParsedUnderZeroFourZero_ShouldFillThemFromTheShippedDefaults`.
   It checks `plannedTags` `@ignore`, empty `withdrawnTags` and `supersededTags`,
   and `lifecycleKey` `spec_status`. It was green when written, because the
   `0389` parser already fills them.
2. The composition reads the filled lists. Tests:
   - the `ClaimLifecycleTags` row with no lists;
   - the `PlannedClaimCases` default row;
   - the acceptance scenario that leaves `claim.plannedTags` at its default.

`lifecycleKey` takes effect in item `0391`. This item pins only its fill.

**Acceptance.** `NativeClaimsSteps` binds the B-001, B-004, B-005, B-006, B-007,
B-011, B-012, B-013, B-025 and B-030 scenarios.

- The check runs the built tool, as for item `0389`.
- No output of this item prints a claim. So a step that asserts on claims reads
  the same tree's model in-process through `EngineServices.Reader()`, after
  first asserting that the run did not exit `3`.
- The Background's step `the manifest's "exclusions" does not hold "/.spec"`
  removes that entry from the manifest's list, and nothing else does (finding
  2). The root step only writes the tree and the pin. The Background's step
  `the manifest declares no epic grammar` removes `identifiers.epic`.
- A tag sits on the line before its element's keyword. A Rule's scenarios and an
  outline's Examples are written at fixed lines.
- "The scenario is marked planned" and "that block's rows are marked planned"
  read `Cases`.
- "Byte-identical" compares the stdout, stderr and exit code of two runs.
- "The claims of one file precede another's" compares the positions of their
  rows.

Red, at `4949c8b8`: every scenario of this item failed on an empty `ClaimSets`
or catalogue, B-004's `Feature | 2` row included. They pass from `0bd82fb3` on.

**Not asserted**, as no claim of this item states it:

- a violation of any kind, items `0392` to `0395`;
- the record's lifecycle and how `lifecycleKey` is read, item `0391`;
- anything printed, counted or listed for a native Feature, item `0396`.

**Testability.** The design leaves every mechanism reachable in memory through
`SpechtModelReader.Read`. Two findings:

1. Resolved. B-004's Examples row put a tagged Feature at line 1, which no file
   can hold, because a claim tag sits on a line before the keyword it declares
   on. Spec-author amended the row to line 2 (§ 10), and the step builds it.
   The step still refuses a keyword line with no line above it for the tag.
2. Resolved as OQ-10, for option (a) (§ 10, § 11). Every shipped manifest's
   `exclusions` holds `/.spec`, so the Feature's example glob,
   `.spec/features/arithmetic/*.feature`, matches nothing under the default
   exclusions. B-001 stands as written: a set reads its files outside
   `exclusions`. The tests hold it so. The Background states the precondition,
   and the unit trees replace `exclusions`.

### A claim file set's record, identity and report (item `0391`)

**Unit.** Three classes hold this item's tests:

- `SpechtModelReaderNativeClaimsUnitTests`, item `0390`'s class, reads a
  `MockFileSystem` tree and observes the native Feature, the one Feature of
  `model.Features` whose `ClaimFileSet` is set.
- `SpechtEvaluatorNativeFeaturesUnitTests`, in
  `test/tests/Evaluation/SpechtEvaluator.NativeFeatures.Unit.Tests.cs`, reads
  such a tree and evaluates it with the composed rules through
  `SpechtEvaluator.Evaluate`. It observes the violations and `Layouts`. Its
  trees pin `0.4.0`, replace `exclusions` with `obj`, declare no epic grammar,
  and set `claim.source` `gherkin` unless a row sets it.
- `SpechtManifestParserClaimMembersUnitTests`, item `0389`'s class, parses
  manifest text.

A row's manifest members and file text come from a `[ClassData]` type beside
its class. Test-writer added the three `FeatureSpec` members § 7 names that a
test reads, `ClaimFileSet`, `Record` and `Lifecycle`, as init properties with
no behaviour: nothing sets them. Each "Red" below is the failure a test gives
against the production code at `cc1c2146` with those declarations.

- **B-003** - mechanism: for a set with a `record`, the reader parses the
  record as the native Feature's document and reads `Lifecycle` at
  `ClaimInput.LifecycleKey`; with none, the document is empty and `Lifecycle`
  is `current`. Edges come from the document through `FeatureSpec.Strings`.
  Test:
  `AClaimFileSet_WhenRead_ShouldMakeOneNativeFeatureWithItsRecordsLifecycleAndEdgesOrCurrentAndNoEdgeWithoutARecord`,
  over `ClaimFileSetRecords`. Its rows are:
  - a record setting `spec_status` `approved` and `depends_on` `[F2]`;
  - no record, `current` with no edge;
  - `claim.lifecycleKey` `maturity`, read from the record's `maturity`.

  Each checks `Record`, `Lifecycle`, `depends_on` and an empty `blocks`. Red:
  no Feature has a `ClaimFileSet`.

  The second mechanism holds a record's frontmatter: `SPEC1001` and `SPEC1002`
  read the native Feature's document, the record's items join
  `FindChildItems` for `SPEC1003`, and both frontmatter rules skip a Feature
  whose `Record` is null. Test:
  `ARecordOrItsAbsence_WhenEvaluated_ShouldBeHeldToTheFeatureAndItemSchemasOnlyWhereARecordIsAuthored`,
  over `RecordFrontmatterFindings`. Each tree holds `add.feature` and an item,
  `0001-01-add.md`, whose frontmatter the task schema rejects. Its rows are:
  - a record with no frontmatter, `SPEC1001` at the record;
  - a record with only `id`, `SPEC1002` at the record;
  - the same record, `SPEC1003` at the item beside it;
  - no record, and for each of the three rules no file.

  Red: the three record rows find nothing, since no record is read. The three
  record-less rows pass when written; they pin the permanent skip, which the
  implementation must not break.

  The third mechanism keeps a native Feature evaluable by the rules that
  read claims or a matrix through `MatrixResolver`. `ResolveClaims` reads a
  claim's matrix status from `SpechtClaims.Section`, `null` for a native
  Feature. A rule that throws there is reported by `FaultIsolatedRule` as a
  failure at `.specht/manifest.json`, line `0`, so a native Feature with a
  claim would fail the check. `Resolve` reads the matrix of the native
  Feature's document, which is its record, so a rule that walks those rows
  reports verdicts that B-023 and B-024 leave to item `0396`. Test:
  `AMatrixRuleOverANativeFeatureWithAClaim_WhenEvaluated_ShouldReportNothingAndNotFail`,
  over `NativeMatrixRuleSilences`. Its tree holds `add.feature` and a record
  whose § 9 matrix has a `Covered` row for `B-001`, and each set names that
  record. Its rows are `SPEC4008` and `SPEC4010` at their default level and
  `SPEC4006`, off by default, at `error`. Each checks that the rule reports
  nothing. Red, from review round 1: the `SPEC4008` row reads
  `Rule SPEC4008 failed with NullReferenceException.` The `SPEC4006` row
  passes when written, since `RetiredClaimRule` then skipped a native Feature;
  with that skip removed it fails the same way. Red, after the rebase onto
  item `0337`: the `SPEC4010` row reads
  `.spec/features/arithmetic/README.md(9): error SPEC4010: § 9 status 'Covered' overclaims: its scenario does not resolve [B-001]`,
  since `StatusRule` walks `Resolve`'s rows with no skip.

- **B-017** - mechanism: `DuplicateIdentityRule` groups `FeaturesWithIdentity`,
  which holds a native Feature because its `Epic` and `Id` are set, and reports
  line `0` when `ClaimFileSet` is set. Test:
  `OneIdentityDeclaredBySetsOrBySetAndSpecification_WhenEvaluated_ShouldReportSpec3002AtEachASetAtItsNativeFeaturesPathLineZero`,
  over `NativeIdentityDuplicates`. Its rows are a set beside
  `src/arithmetic/.spec/README.md`, which is reported at line `2`, its id line,
  and two sets naming `F1`. Each checks every `SPEC3002` place and the
  identifier `F1`. Red: no `SPEC3002` is reported.

  The second mechanism is "no second reading": a specification the layouts
  discover at a record's path is dropped, so the record is not its own
  duplicate. Test:
  `ARecordALayoutAlsoDiscovers_WhenRead_ShouldBeReadOnceAsTheNativeFeatureAndNotAsTheLayoutsFeature`.
  Red: the one Feature at that path is the layout's, with no `ClaimFileSet`.

- **B-026** - mechanism: the reader locates the native Feature at its record,
  else at the first of `ClaimFiles[i]`, in the layout `claims` with the set's
  glob. Test:
  `ANativeFeature_WhenRead_ShouldBeLocatedInTheClaimsLayoutAtItsRecordOrElseItsFirstClaimFileInOrdinalOrder`,
  over `NativeFeatureLocations`. Its rows are a set with a record, and a
  record-less set over `subtract.feature` and `add.feature`, located at
  `add.feature`. Red: no Feature has a `ClaimFileSet`.

  The second mechanism is `SpechtEvaluator.Evaluate` appending the `claims`
  entry. Test:
  `NativeFeatures_WhenEvaluated_ShouldBeCountedUnderClaimsAfterTheManifestsLayoutsOnlyUnderAGherkinClaimSource`,
  over `NativeReportLayouts`. Its rows are:
  - the shipped layouts, then `claims 1`;
  - a manifest whose first layout is named `claims`, which keeps that entry
    first and `claims 1` last;
  - a `markdown` claim source, with no `claims` entry (C-3, B-002).

  Red: the first two rows have no trailing `claims` entry. The `markdown` row
  passes when written.

- **B-032** - mechanism: the native Feature's `ClaimGrammar` is
  `ClaimGrammar(null)`, and `model.Claims` keys it to its set's claims. Test:
  `ARecordDeclaringAClaimPrefix_WhenRead_ShouldLeaveTheNativeFeatureTheGrammarOfAFeatureWithNoPrefix`.
  The record declares `claim_prefix: ADD-`, and `add.feature` tags `@B-001` and
  `@ADD-002`. It checks the grammar and that the Feature's claims are
  `B-001` alone. Red: no Feature has a `ClaimFileSet`.
- **B-036** - mechanism: a set with no record and no claim file has no
  location, so it makes no Feature, and its empty `ClaimSets` entry stays.
  Tests:
  - `AClaimFileSetWithNoRecordWhoseGlobMatchesNoFeatureFile_WhenRead_ShouldMakeNoFeatureAndKeepItsEmptyClaimSet`,
    with two sets, `F1` matching nothing and `F2` matching one file. It expects
    native Features `F2` alone, and both sets in `ClaimSets`. Red: there is no
    native Feature.
  - `AClaimFileSetWithNoRecordAndNoClaimFile_WhenEvaluated_ShouldBeCountedUnderClaimsAsNoneAndComparedWithNoIdentity`,
    beside a specification `F1`. It expects the last entry to be `claims 0`
    and no `SPEC3002`. Red: the last entry is `features 1`.
- **B-037** - mechanism: a native Feature's `Epic` and `Id` are the set's,
  whatever the record declares. Test:
  `ARecordDeclaringAnIdentityOtherThanItsSets_WhenRead_ShouldLeaveTheNativeFeatureTheSetsIdentity`,
  over `RecordIdentities`. Its rows are:
  - a record `id` `F9`;
  - a record `epic` under no epic grammar;
  - a record `id` and `epic` under the epic grammar.

  Each checks the native Feature's epic and id and that no Feature has id
  `F9`. Red: no Feature has a `ClaimFileSet`.

- **B-038** - mechanism: `SpechtManifestParser.FileSets` matches a set's
  `feature` and `epic` against the grammars and adds a `claim.files` fault.
  Test: `AnInvalidClaimFileSet_WhenParsedUnderZeroFourZero_ShouldBeRejectedNamingClaimFiles`,
  over `InvalidClaimFileSets`. It gains three rows:
  - feature `arithmetic` under no epic grammar;
  - feature `arithmetic`, epic `0001`, under the epic grammar;
  - feature `F1`, epic `1`, under the epic grammar.

  The other rows' `feature` became `F1`, so each still fails only for its own
  fault. Red: the three rows parse. `ClaimFileSetIdentities` and the default
  fill test use `F1` for the same reason.

`SpechtTreeStore.TryLoad`'s four `null` cases are the reading half of B-035,
which § 7 builds here. B-035 is item `0395`'s claim, so their tests are left
to it, with B-033 and B-034.

**Acceptance.** `NativeClaimsSteps` binds the B-003, B-017, B-026, B-032,
B-036, B-037 and B-038 scenarios. It keeps every step of items `0389` and
`0390` as it was, so their scenarios hold.

- The root's specification is `F5`, so no identity a scenario names meets it.
- "Names the record" sets `record` and writes the record with a full Feature
  frontmatter, `id` `F1`, unless the path leads outside the root. Each
  "frontmatter sets" step rewrites it with the key set. A list key, such as
  `depends_on`, is written as a one-item list.
- "The layouts discover" writes a specification at the path with the id.
- A native Feature's lifecycle, edges and identity are read from the same
  tree's model in-process. The step finds the Feature whose `ClaimFileSet`
  names the scenario's feature, after asserting that the run did not exit `3`.
- "Held to the Feature schema" asserts that the run reported no `SPEC1001` or
  `SPEC1002` at the record. It then rewrites the record with `spec_status`
  `unknown`, runs again, and asserts a `SPEC1002` line at the record.
- "A layout named claims" inserts that layout first in the manifest's layouts,
  matching `docs/claims/**/spec.md`, and writes one specification there. So
  its entry's place shows it is the manifest's.
- The summary steps read the `specifications:` line. The report steps parse
  the `--json` document's `layouts`. Each asserts that the entries open with
  the manifest's layouts by name, in order, and that one entry follows.
- "No file matches" asserts the glob's directory holds no matching file.

Red, at this item's test commit: every bound scenario but B-032's fails. A
native Feature step finds none. A `SPEC3002` line is missing. The summary or
`layouts` has no entry after the manifest's. B-038's run does not exit `3`.
B-032's scenario passes when written: its Then reads the claim set, which
item `0390` already reads with the default grammar. Its unit test is the red
one.

**Not asserted**, as no claim of this item states it:

- what a rule that § 7 skips reports for a native Feature. The skips defer
  B-008, B-020, B-021, B-023 and B-024 to items `0392` and `0396`. A test
  that a skipped rule is silent would assert a deferral, not a claim. The one
  exception is the matrix rules, `SPEC4008`, `SPEC4006` and `SPEC4010`:
  without a skip they fail on a native Feature, or report on its record's
  matrix, which B-003's third mechanism pins. Item `0396` replaces that test's silence with the verdicts
  B-023 and B-024 state;
- a record that `TryLoad` cannot read, items `0395` (B-033 to B-035);
- the claim files as a native Feature's companions, item `0392` (B-021).

**Testability.** Every mechanism is reachable in memory through
`SpechtModelReader.Read`, `SpechtEvaluator.Evaluate` and
`SpechtManifestParser.Parse`. Two findings, both resolved by spec-author
before binding (§ 10):

1. B-003's "No record means a current Feature with no edges" named no claim
   file. Under B-036 such a set makes no Feature, so its Thens had nothing to
   read. The scenario now adds `the glob matches
".spec/features/arithmetic/add.feature"`.
2. B-026's and B-036's summary scenarios read "the summary" of a run with
   `--json`, which prints the report document and no summary lines. Each
   count scenario is now split into a summary scenario, run without `--json`,
   and a report scenario, run with it; B-036 runs without it.

### A scenario in a claim file with no claim, `SPEC4011` (item `0393`)

**Unit.** `ScenarioWithoutClaimRuleUnitTests`, in
`test/tests/Rules/NativeClaims/ScenarioWithoutClaimRule.Unit.Tests.cs`, the
folder of the rule § 7 names, reads a `MockFileSystem` tree through
`EngineServices.Reader(fileSystem)` and evaluates it with the composed rules
through `SpechtEvaluator.Evaluate`. So the id the evaluator stamps at the pin,
the severity, the path and the line are observed together. Its trees pin
`0.4.0`, replace `exclusions` with `obj`, declare no epic grammar, and set
`claim.source` `gherkin` with one set, `.spec/features/arithmetic/*.feature`
for Feature `F1`, unless a test sets its own. A row's manifest members and
file texts come from `UnclaimedScenarios`, a `[ClassData]` type beside the
class. No test reads `SpechtClaims.Files`, so test-writer added no declaration
to `src/`; the member is implementer's, with the rule.

- **B-010** - mechanism: `ScenarioWithoutClaimRule` turns each `SPCT0012`
  finding of a claim file's read into a finding at the file's root-relative
  path and the finding's line, once per path. The shared reader's finding
  already holds inheritance from a Rule and the Feature (A-3, C-1), and the
  rule's `DefaultLevel` gives the warning. Test:
  `AScenarioInAClaimFile_WhenEvaluated_ShouldBeReportedAsASpec4011WarningAtItsKeywordLineOnlyWhenItCarriesNoClaimOfItsOwnItsRulesOrItsFeatures`,
  over `UnclaimedScenarios`. Its rows are § 7's cases:
  - an untagged `Scenario:`, `Example:`, `Scenario Outline:` and
    `Scenario Template:` in one file, each at its keyword line;
  - `@smoke` and `@B-01` on two lines above the keyword, at the keyword's line;
  - `@ignore`, a planned tag, alone;
  - a claim on one scenario beside an untagged one, the untagged one alone;
  - a claimed Rule, then an unclaimed Rule, the later Rule's scenario alone;
  - a claimed Feature beside an untagged second file, the second file alone;
  - a claim tag on an Examples block alone, at the outline;
  - a claim id in a title alone;
  - a file declaring `# language: fr`, and a file with no `Feature:` line,
    each beside an untagged second file, the second file alone;
  - one file two sets match, each scenario once.

  Each row asserts the exact multiset of severity and `path(line)`, so the
  level, the path, the line and the count are all held. Every row expects at
  least one place: a file that must give nothing sits beside a file that
  gives one, so a rule that reports nothing fails every row. Before the rows
  were handed on, each row's places were checked against the `SPCT0012` lines
  the shared reader gives its text, and the two unreadable files were checked
  to throw `GherkinClaimTextException`. Red at `f81d195c`: every row, on
  "found an empty collection", since no rule reports `SPEC4011`. Every row
  passes from `719148ae`.

- **B-002, C-3** - mechanism: under `0.4.0` without `claim.source` `gherkin`,
  `SpechtModel.ClaimSets` holds no set, so the rule has nothing to read.
  Guard:
  `AMarkdownTreeUnderZeroFourZero_WhenItsCompanionHoldsAnUntaggedScenario_ShouldReportNoSpec4011`,
  a co-located specification whose companion holds an untagged scenario.
  Green when written: nothing reports `SPEC4011` yet. It holds the second
  gate once the rule does.
- **B-028, C-3** - mechanism: below `0.4.0` no vocabulary holds the id, so the
  evaluator neither runs nor counts the rule. Guard:
  `AGherkinClaimSourceUnderZeroThreeOne_WhenAClaimFileHoldsAnUntaggedScenario_ShouldNeitherReportNorCountSpec4011`.
  It asserts no `SPEC4011` and `RulesEvaluated` `23`, the count the same tree
  gives at `a57f4bae`, the base before this item. Green when written.
- **`0001-F7` B-014, C-1** - mechanism: `SchemaVersions.Shipped`'s `0.4.0` row
  maps `SPEC025` to `SPEC4011`, and `AddSpechtEngine` registers the rule. Rows
  added to existing theories and lists:
  - `ShippedVocabularies`' `0.4.0` row gains `SPEC4011`.
    `SchemaVersionsUnitTests.AnEmbeddedVersion_WhenItsVocabularyIsRead_ShouldNameEachRuleByThatVersionsId`
    fails on that row at `f81d195c`: 29 ids expected, 28 found, the row
    holding `SPEC2003` from `a57f4bae`. Its `0.1.0` to `0.3.1` rows
    are unchanged (C-1). The integration theory over the same data,
    `SpechtCheckVersionsIntegrationTests.TheBaselineTreePinnedToAShippedVersion_WhenChecked_ShouldReportEveryViolationUnderAnIdOfThatVersionsVocabulary`,
    stays green: it asks only that each reported id belongs.
  - `ZeroFourZeroRuleNames` gains `SPEC025` to `SPEC4011`.
    `SchemaVersionsUnitTests.ARuleKeyEnteringVersionZeroFourZero_WhenNamedUnderIt_ShouldTakeTheFourDigitIdTheMapPairsItWith`
    fails on that row at `f81d195c`: `RuleId` returns `SPEC025` unmapped.
  - The composition id list gains `SPEC025`.
    `SpechtEngineServiceCollectionExtensionsUnitTests.EveryRuleInTheEngine_WhenItsIdIsRead_ShouldNameEachShippedRuleIdExactlyOnce`
    fails at `f81d195c`: 29 ids expected, 28 found.

  All three pass from `719148ae`.

  As § 7 says, the rule-page tests (`0001-F3` B-031),
  `InitRuleDefaultsUnitTests`, the real-tree numbering test (`0001-F7` B-066)
  and the lock tests take the page, the manifest line and the relock with no
  new test.

**Acceptance.** `NativeClaimsSteps` binds the two `@B-010` scenarios and the
`@boundary` scenario "A claim is never inferred from a title or a step".

- "A scenario at line 9 with no claim tag of its own, its Rule or its Feature"
  writes an untagged Feature, a scenario tagged `@B-001`, and an untagged Rule
  whose untagged scenario is at the named line.
- "The output has a line" binds a warning line alone, matched as the start of
  a diagnostic line. Item `0395`'s scenarios name error lines in the same
  words, and its rule is not built: bound to any line, the step made B-016's
  scenario run, and fail, here. Item `0395` widens it.
- "The exit code is 0 without "--strict"" asserts that the run was launched
  without the flag, and its exit code.
- "No violation is reported for the scenario" asserts that the run did not
  exit `3` and that no diagnostic line starts at the scenario's path and
  keyword line.
- "The scenario is reported under" asserts a diagnostic line at the scenario's
  path and keyword line naming the id. "Feature "F1" has no claim" reads the
  model, as item `0390`'s claim steps do.

Red at `f81d195c`: "A scenario with no claim is reported" fails on no
diagnostic line, and the boundary scenario on its last step, after "has no
claim" passes. "An inherited claim satisfies the untagged scenario rule"
passes: a guard that holds once the rule reports. All three pass from
`719148ae`.

**Not asserted**, as no claim of this item states it:

- the message and the `null` identifier § 7 fixes. B-010 names the id, the
  level and the line;
- a manifest `rules` entry raising or silencing `SPEC4011`, and `--strict`
  exiting `1` on the warning. § 7's testability list names both. They are the
  evaluator's and the check command's for every id, pinned by
  `SpechtEvaluatorUnitTests.ARuleIdSetToError_WhenEvaluated_ShouldReportTheFindingOfARuleWhoseOwnLevelIsWarningAtErrorSeverity`,
  `SpechtEvaluatorUnitTests.ARuleWhoseIdIsSetOff_WhenEvaluated_ShouldNotRunAndShouldLeaveTheOtherRulesViolationsAndCount`
  and the warning rows of
  `CheckCommandIntegrationTests.AReportOfSeverities_WhenChecked_ShouldExitWithTheClaimedCode`.
  A `SPEC4011` copy of either would assert the same behaviour through another
  arrangement;
- that no claim file is parsed twice (C-5). It shows in no output; the two-set
  row observes only that each scenario is reported once;
- what else a file two sets match gives, item `0395` (B-018).

**Testability.** Every mechanism is reachable in memory through
`SpechtModelReader.Read` and `SpechtEvaluator.Evaluate`, and every outcome
through the built tool. One verdict per claim:

- B-010: the `SPCT0012` mapping, the per-path filter and the default level,
  pinned by the theory above.

Nothing in this item's scope is hard to test.

### A claim tag on an Examples block, `SPEC4012` (item `0394`)

**Unit.** `ExamplesClaimTagRuleUnitTests`, in
`test/tests/Rules/NativeClaims/ExamplesClaimTagRule.Unit.Tests.cs`, beside
item `0393`'s class, reads a `MockFileSystem` tree through
`EngineServices.Reader(fileSystem)` and evaluates it with the composed rules
through `SpechtEvaluator.Evaluate`. So the id the evaluator stamps at the
pin, the severity, the path, the line and the identifier are observed
together. Its trees take item `0393`'s defaults: `0.4.0`, `exclusions` `obj`
alone, no epic grammar, and `claim.source` `gherkin` with one set,
`.spec/features/arithmetic/*.feature` for Feature `F1`, unless a test sets
its own. A row's manifest members and file texts come from
`ExamplesClaimTags`, a `[ClassData]` type beside the class. Test-writer
declared one member in `src/`, for the reader test to compile: the optional
`string? ClaimId = null` on `GherkinClaimTextFinding`, as § 7 designs it. At
`a311b77d` the reader did not set it; from `013adb9f`, as implementer built
it, the reader sets it on each `SPCT0011` finding.

- **B-009** - mechanism: `ExamplesClaimTagRule` turns each `SPCT0011`
  finding of a claim file's read into a finding at the file's root-relative
  path and the finding's line, the tag's, with the finding's `ClaimId` as its
  identifier, once per path, at the interface's default level, error. Test:
  `AClaimTagOnAnExamplesBlock_WhenEvaluated_ShouldBeReportedAsASpec4012ErrorAtTheTagsLineNamingTheClaimIdItWouldDeclare`,
  over `ExamplesClaimTags`. Its rows are § 7's cases:
  - a claim tag on an `Examples:` block, and on a `Scenarios:` block;
  - the tag above the keyword, and between two ordinary tags above it, at the
    tag's own line;
  - two claim tags on one block, and one on each of two blocks, one each;
  - `@smoke`, `@B-05` and `@ignore`, ordinary, mistyped and planned, on one
    block, none;
  - `@b-005` under `claim.tagIgnoreCase`, reported with the id as written;
  - the outline's only claim tag on its block: the tag at error, and the
    outline under `SPEC4011` at its keyword line;
  - the outline claimed by its Rule, and by its Feature: the tag, and no
    `SPEC4011`;
  - the block naming the outline's own claim, `@B-001`: the tag, and no
    `SPEC4003`, since the tag declares nothing;
  - one file two sets match, each tag once;
  - a file the reader cannot read, a claim tag above an `Examples:` line with
    no outline, beside a claimed block: the second file alone.

  Each row asserts the exact multiset of `SPEC4012`, `SPEC4011` and
  `SPEC4003` violations as id, severity, `path(line)` and identifier, so the
  level, the line, the identifier, the count and the two neighbouring rules'
  verdicts are held at once. Every row expects at least one `SPEC4012`, so a
  rule that reports nothing fails every row. Before the rows were handed on,
  each row's places and ids were checked against the `SPCT0011` and
  `SPCT0012` findings the shared reader gives its text, and the unreadable
  file was checked to throw `GherkinClaimTextException`.

- **B-009, the claim id** - mechanism: `GherkinClaimTextReader` passes the
  claim id the claim form read to the `SPCT0011` finding and none to
  `SPCT0012`'s. Test:
  `GherkinClaimTextReaderUnitTests.AClaimTagOnAnExamplesBlock_WhenRead_ShouldGiveAPlacementFindingCarryingTheClaimIdAndAMissingClaimFindingCarryingNone`,
  an untagged outline whose block carries `@smoke @B-005`. The analyzer's
  findings stay under their existing tests, as § 7 says.
- **B-009, no claim** - mechanism: only an `Element`'s tags declare a claim.
  Guard:
  `AClaimTagOnAnExamplesBlockAlone_WhenRead_ShouldDeclareNoClaimForItsId`:
  the set holds the outline's `B-001` and no `B-005`. Green when written: § 7
  adds no code for it.
- **B-002, C-3** - mechanism: under `0.4.0` without `claim.source` `gherkin`,
  `SpechtModel.ClaimSets` holds no set. Guard:
  `AMarkdownTreeUnderZeroFourZero_WhenItsCompanionHasAClaimTagOnAnExamplesBlock_ShouldReportNoSpec4012`.
  Green when written.
- **B-028, C-3** - mechanism: below `0.4.0` no vocabulary holds the id. Guard:
  `AGherkinClaimSourceUnderZeroThreeOne_WhenAClaimFileHasAClaimTagOnAnExamplesBlock_ShouldNeitherReportNorCountSpec4012`.
  It asserts no `SPEC4012` and `RulesEvaluated` `23`, the count the same tree
  gives at `a311b77d`, where no rule reports `SPEC4012`. Green when written.
- **`0001-F7` B-014, C-1** - mechanism: `SchemaVersions.Shipped`'s `0.4.0` row
  maps `SPEC026` to `SPEC4012`, and `AddSpechtEngine` registers the rule.
  Rows added to existing theories and lists:
  - `ShippedVocabularies`' `0.4.0` row gains `SPEC4012`.
    `SchemaVersionsUnitTests.AnEmbeddedVersion_WhenItsVocabularyIsRead_ShouldNameEachRuleByThatVersionsId`
    fails on that row: 30 ids expected, 29 found. Its `0.1.0` to `0.3.1` rows
    are unchanged (C-1).
  - `ZeroFourZeroRuleNames` gains `SPEC026` to `SPEC4012`.
    `SchemaVersionsUnitTests.ARuleKeyEnteringVersionZeroFourZero_WhenNamedUnderIt_ShouldTakeTheFourDigitIdTheMapPairsItWith`
    fails on that row: `RuleId` returns `SPEC026` unmapped.
  - The composition id list gains `SPEC026`.
    `SpechtEngineServiceCollectionExtensionsUnitTests.EveryRuleInTheEngine_WhenItsIdIsRead_ShouldNameEachShippedRuleIdExactlyOnce`
    fails: 30 ids expected, 29 found.

  As § 7 says, the rule-page tests (`0001-F3` B-031),
  `InitRuleDefaultsUnitTests`, the real-tree numbering test (`0001-F7` B-066)
  and the lock tests take the page, the manifest line and the relock with no
  new test.

**Acceptance.** `NativeClaimsSteps` binds the `@B-009` scenario. "An Examples
block tagged "@B-005" at line 14" writes an outline claimed by `@B-001`, so
the line the scenario names is the only violation, and puts the block's tag
at the named line. "The output has a line … naming … with the identifier"
and "Feature "F1" has no claim" are item `0391`'s and item `0393`'s steps,
unchanged: the first asserts a diagnostic line that starts with the place
and severity, names the id, and ends `[B-005]`.

**Red at `a311b77d`**, on `net11.0`, and passing from `013adb9f`:

- every row of the theory, on "found an empty collection", but the outline
  row, which finds its `SPEC4011` line alone: no rule reports `SPEC4012`;
- the reader test, on "to be "B-005", but found <null>";
- the three vocabulary tests, on the counts and the unmapped key above;
- the `@B-009` scenario, on its "naming" step: no line names `SPEC4012`.

The guards pass, and so does item `0393`'s class with the new member on the
shared finding. From `013adb9f` every test above passes, and so does the
`@B-009` scenario.

**Not asserted**, as no claim of this item states it:

- the message § 7 fixes. B-009 names the id, the line and the identifier;
- a manifest `rules` entry lowering or silencing `SPEC4012`, and the exit
  code. § 7's testability list names the level. They are the evaluator's and
  the check command's for every id, pinned by
  `SpechtEvaluatorUnitTests.ARuleWhoseIdIsSetOff_WhenEvaluated_ShouldNotRunAndShouldLeaveTheOtherRulesViolationsAndCount`,
  the `OwnLevelSeverityData` rows of `SpechtEvaluatorUnitTests` and
  `CheckCommandIntegrationTests.AReportOfSeverities_WhenChecked_ShouldExitWithTheClaimedCode`.
  A `SPEC4012` copy would assert the same behaviour through another
  arrangement;
- that no claim file is parsed twice (C-5), and what else a file two sets
  match gives, item `0395` (B-018).

**Testability.** Every mechanism is reachable in memory through
`GherkinClaimTextReader.Read`, `SpechtModelReader.Read` and
`SpechtEvaluator.Evaluate`, and every outcome through the built tool. One
verdict per claim:

- B-009: the `SPCT0011` mapping, the claim id on the finding, the per-path
  filter and the default level, pinned by the theory and the reader test
  above; no claim recorded, held by the guard.

Nothing in this item's scope is hard to test.

### An unreadable claim file, set or record, `SPEC4013` (item `0395`)

**Unit.** `ClaimInputFaultRuleUnitTests`, in
`test/tests/Rules/NativeClaims/ClaimInputFaultRule.Unit.Tests.cs`, beside the
classes of items `0393` and `0394`, reads a `MockFileSystem` tree through
`EngineServices.Reader(fileSystem)` and evaluates it with the composed rules
through `SpechtEvaluator.Evaluate`. So the id the evaluator stamps at the pin,
the severity, the path and the line are observed together. Its trees take item
`0393`'s defaults: `0.4.0`, `exclusions` `obj` alone, no epic grammar, and
`claim.source` `gherkin` with one set, `.spec/features/arithmetic/*.feature`
for Feature `F1`, unless a test sets its own. The root is `repo`, so
`../outside/README.md` is a file outside it, and `/outside/README.md` is the
same file by a rooted path. That rooted path is synthetic, held only in the
in-memory file system, and it is B-034's subject; no fixture holds a path of
the machine the test runs on. Test-writer declared three members in `src/`
for the tests to compile, as § 7 designs them: the optional `int line = 0` and
`Line` on `GherkinClaimTextException`, the optional
`IReadOnlyList<int>? FeatureTagLines = null` on `GherkinClaimText`, and
`SpechtTreeStore.IsRooted`, a stub that answers `false`. At `4a441831` the
reader passes no line and fills no `FeatureTagLines`, and `TryLoad` does not
call `IsRooted`.

- **B-014, B-015, B-016, B-018, B-019, B-033, B-034** - mechanism: the
  builder's per-file catch, its set and shared-path counts, its Feature-level
  scope form, and the model reader's `TryLoad` of each `record` record a fault
  where they drop or find what they cannot take, and `ClaimInputFaultRule`
  turns each into a finding at the fault's path and line, at the interface's
  default level, error. Test:
  `ClaimInputTheCheckCannotTakeClaimsFrom_WhenEvaluated_ShouldBeReportedAsASpec4013ErrorAtItsPlace`,
  over `UnreadableClaimInputs`. Its rows are § 7's cases and B-015's causes:
  - `# language: fr` on line 2, beside a file whose `# language: en` header
    is not reported: line 2 (B-014);
  - each text B-015 names, no `Feature:` line, a second one, a ragged table,
    a malformed tag token, an unterminated doc string, tags followed by no
    element and an `Examples:` with no outline, and an empty file: line `0`
    (B-015);
  - a second set whose glob matches no file, with and without a `record`: the
    glob as written, line `0` (B-016);
  - one file two sets match: the file, line `0`, once (B-018);
  - under `claim.scopes.featureTag` `@spec.feature:{feature}`, a file tagged
    `@spec.feature:F2` below `@smoke`, beside a file that agrees and a file
    with no Feature-level tag: the tag's line, 2, alone (B-019);
  - a `record` naming no file, a directory, and `../outside/README.md`, a file
    that exists outside the root: the path as written, line `0` (B-033);
  - a `record` `/outside/README.md`, a file that exists:
    `.specht/manifest.json`, line `0` (B-034).

  Each row asserts the exact multiset of `SPEC4013` violations as id,
  severity and `path(line)`, so the level, the place and the count are held at
  once. Every row expects one, so a rule that reports nothing fails every row.
  Before the rows were handed on, every text B-014 and B-015 name was checked
  to throw `GherkinClaimTextException` from the shared reader, by the reader
  theory below, and the in-memory file system was checked to hold
  `repo/../outside/README.md` at `/outside/README.md` and to throw an
  `IOException` reading a file shared with no one.

- **B-014, B-015, B-018, no claim** - mechanism: a file the reader refuses
  declares nothing, and `SpechtClaims` skips the declarations of a shared
  path. Test:
  `AClaimFileTheCheckCannotTakeClaimsFrom_WhenRead_ShouldRecordNoClaimFromItUnderAnySet`,
  over `UnclaimableClaimFiles`: a `# language: de` file, a file with no
  `Feature:` line, and a file a second set also matches, each beside a file
  only the first set matches, whose `B-002` alone is recorded.
- **B-014, B-015, the line** - mechanism: `CheckLanguage` passes the refused
  header's line to the exception, and every other refusal leaves `0`. Test:
  `GherkinClaimTextReaderUnitTests.AClaimFileTextTheReaderRefuses_WhenRead_ShouldThrowCarryingTheRefusedLanguageHeadersLineOrZeroForEveryOtherCause`,
  over `RefusedClaimTexts`: a `# language: de` header on line 1 and a
  `# language: fr` header on line 2, and each text of the rule theory's B-015
  rows at `0`.
- **B-019, the line** - mechanism: the reader fills `FeatureTagLines` beside
  `FeatureTagValues`. Test:
  `GherkinClaimTextReaderUnitTests.FeatureLevelTagsAFeatureScopeFormReads_WhenRead_ShouldGiveEachValueWithTheLineOfItsTag`:
  `@smoke @spec.feature:F2` on line 1, a comment, `@spec.feature:F3` on line
  3, and `@spec.feature:F4` on a scenario, which is no Feature-level tag. The
  analyzer reads only the values, and its findings stay under their existing
  tests, as § 7 says.
- **B-019, no form** - mechanism: the builder gives the reader a Feature
  scope form only when `claim.scopes.featureTag` is set and the form's
  constructor takes it; OQ-20 (a) reads a form without exactly one
  `{feature}` as none. Guard:
  `AFeatureLevelTagNoFeatureTagFormReads_WhenEvaluated_ShouldReportNoSpec4013`,
  over `FeatureTagFormsReadingNoTag`: no `featureTag`, `@spec.feature` and
  `@{feature}:{feature}`, each over a file tagged `@spec.feature:F2`. Green
  when written: the builder reads no `featureTag` at `4a441831`, and the
  `0.4.0` parser accepts both malformed forms. § 7's open point 2 designs this
  answer; no claim states that nothing throws, so the guard asserts only that
  nothing is reported.
- **B-034, C-2** - mechanism: a rooted `record` is reported at the manifest,
  and its path is in no field. Test:
  `ARootedRecord_WhenEvaluated_ShouldPutNoRootedPathInAnyViolation`: a
  `SPEC4013` is reported, and no violation's path starts with `/` or holds
  `/outside` in its message or identifier. The first assertion keeps it from
  passing on a tree that reports nothing.
- **B-033 to B-035, the store** - mechanism: `TryLoad` gives `null`, and
  throws nothing, for a rooted path, a path leaving the root, a path naming no
  file and a file whose read throws; `IsRooted` answers what its first guard
  tests. Tests, in `SpechtTreeStoreUnitTests`:
  `APathTheStoreCannotLoadUnderTheRoot_WhenTriedToLoad_ShouldGiveNothingAndThrowNothing`,
  over `UnloadableRelativePaths`, the four cases and the root itself, `.`,
  with a file at each path that leads to one; and
  `APathAManifestWrites_WhenAskedWhetherItIsRooted_ShouldGiveTheAnswerOfTheGuardTryLoadRefusesFirst`,
  over `RootedAndRelativePaths`: `/outside/README.md` rooted,
  `../outside/README.md` and `README.md` not. Item `0391`'s review left them
  to this item.
- **B-035** - mechanism: a `record` `TryLoad` gives `null` for is read as no
  `record`. Test:
  `SpechtModelReaderNativeClaimsUnitTests.AClaimFileSetWhoseRecordTheCheckCannotRead_WhenRead_ShouldMakeTheNativeFeatureItMakesWithNoRecordAndThrowNothing`,
  over `UnreadableRecords`: a path naming no file, a file shared with no one,
  `/outside/README.md` and `../outside/README.md`. The file that cannot be
  read and the file outside are each an approved record depending on `F2`, so
  a read of either would change the lifecycle and the edges. The Feature has
  no record, sits at its first claim file, is `current` and depends on
  nothing. Green when written: item `0391` built the reading.
- **B-002, C-3** - mechanism: under `0.4.0` without `claim.source` `gherkin`,
  the model holds no set, so there is no fault. Guard:
  `AMarkdownTreeUnderZeroFourZero_WhenItsCompanionCannotBeReadAsClaimInput_ShouldReportNoSpec4013`,
  a companion with a `# language: fr` header. Green when written.
- **B-028, C-3** - mechanism: below `0.4.0` no vocabulary holds the id. Guard:
  `AGherkinClaimSourceUnderZeroThreeOne_WhenEachClaimInputFaultIsPresent_ShouldNeitherReportNorCountSpec4013`:
  under `0.3.1`, a file in another language, a set matching no file with a
  rooted record, a record naming no file, a shared file and a disagreeing
  Feature-level tag. It asserts no `SPEC4013` and `RulesEvaluated` `23`, the
  count the same tree gives at `4a441831`, where no rule reports `SPEC4013`.
  Green when written.
- **`0001-F7` B-014, C-1** - mechanism: `SchemaVersions.Shipped`'s `0.4.0` row
  maps `SPEC027` to `SPEC4013`, and `AddSpechtEngine` registers the rule.
  Rows added to existing theories and lists:
  - `ShippedVocabularies`' `0.4.0` row gains `SPEC4013`.
    `SchemaVersionsUnitTests.AnEmbeddedVersion_WhenItsVocabularyIsRead_ShouldNameEachRuleByThatVersionsId`
    fails on that row: 31 ids expected, 30 found. Its `0.1.0` to `0.3.1` rows
    are unchanged (C-1).
  - `ZeroFourZeroRuleNames` gains `SPEC027` to `SPEC4013`.
    `SchemaVersionsUnitTests.ARuleKeyEnteringVersionZeroFourZero_WhenNamedUnderIt_ShouldTakeTheFourDigitIdTheMapPairsItWith`
    fails on that row: `RuleId` returns `SPEC027` unmapped.
  - The composition id list gains `SPEC027`.
    `SpechtEngineServiceCollectionExtensionsUnitTests.EveryRuleInTheEngine_WhenItsIdIsRead_ShouldNameEachShippedRuleIdExactlyOnce`
    fails: 31 ids expected, 30 found.

  As § 7 says, the rule-page tests (`0001-F3` B-031),
  `InitRuleDefaultsUnitTests`, the real-tree numbering test (`0001-F7` B-066)
  and the lock tests take the page, the manifest line and the relock with no
  new test.

**Acceptance.** `NativeClaimsSteps` binds the `@B-014`, `@B-015`, `@B-016`,
`@B-018`, both `@B-019`, `@B-033`, `@B-034` and `@B-035` scenarios. "The
output has a line" now binds any diagnostic line that starts with the text
given, so a line-`0` line, printed as `<path>:` with no position, is matched as
the scenarios write it; item `0393`'s warning lines still match it. A claim
file the steps write declares `@B-001`, so "has no claim from that file"
fails if the file's claim is kept. "The file at that path cannot be read"
holds the record open with no sharing until the scenario ends. "A record at an
absolute path" names the root's own record by its full path, so a check that
read it would find it, and "no output line holds an absolute path" asserts
that neither the root nor that path is in stdout or stderr. B-035's record is
approved and depends on `F2` wherever a file is written, so a read of it would
change the lifecycle and the edges the scenario checks. "The path leads
outside the root" writes no file outside the temporary root; the store test
above holds a file there.

**Red at `4a441831`**, on `net11.0`:

- every row of the rule theory, on "a collection with 1 item(s), but found an
  empty collection": no rule reports `SPEC4013`;
- the shared-file row of the no-claim theory, on `{"F1 B-001", "F1 B-002",
"F2 B-001"}` against `{"F1 B-002"}`; its other two rows pass;
- the rooted-record test, on its first assertion, no `SPEC4013`;
- the reader theory's two `# language:` rows, on `Item2` `0` against `1`
  and `2`; its eight line-`0` rows pass;
- the Feature tag line test, on "but found <null>";
- the `IsRooted` row for `/outside/README.md`, on "to be True, but found
  False"; its two relative rows pass;
- the three vocabulary tests, on the counts and the unmapped key above;
- nine acceptance scenarios, `@B-014`, `@B-015`, `@B-016`, `@B-018`, the
  first `@B-019`, the three `@B-033` rows and `@B-034`, each on "the output
  has a line": no line names `SPEC4013`.

Twenty-six unit tests fail across the six classes, and nothing else. The
guards, the `TryLoad` theory, the B-035 theory, the four `@B-035` rows and the
`@B-019` boundary "A Feature-level tag that agrees, or none, is not reported"
pass, and so do the classes of items `0393` and `0394`, with the new members
on the shared exception and read.

**Not asserted**, as no claim of this item states it:

- the message § 7 fixes, and every identifier: B-016's set `feature`, B-019's
  captured id and B-034's `feature`. B-014 to B-019, B-033 and B-034 name the
  id and the place;
- that B-019 keeps the file's claims. B-019, unlike B-014, B-015 and B-018,
  says nothing of them;
- a manifest `rules` entry lowering or silencing `SPEC4013`, and the exit
  code beyond B-016's scenario. They are the evaluator's and the check
  command's for every id, pinned as item `0394`'s § 8 names;
- `SPEC4011` and `SPEC4012` once on a shared file (B-018's reading, § 10):
  item `0393`'s and item `0394`'s shared-file rows pin it and stand
  unchanged;
- a set with no record and no claim file reported twice (§ 7): no claim
  states the pair.

**Testability.** Every mechanism is reachable in memory through
`GherkinClaimTextReader.Read`, `SpechtTreeStore.TryLoad`,
`SpechtModelReader.Read` and `SpechtEvaluator.Evaluate`, and every outcome
through the built tool. One verdict per claim:

- B-014: the language refusal's line, pinned by the reader theory; its fault
  and finding, and no claim, by the rule and no-claim theories.
- B-015: one refusal at line `0` for every cause, pinned by the reader
  theory; its fault and finding by the rule theory; no claim by the no-claim
  theory.
- B-016: the empty-set count, pinned by the rule theory's two glob rows.
- B-018: the shared-path count and the skipped declarations, pinned by the
  rule theory's shared row and the no-claim theory's.
- B-019: the scope form, the value's line and the ordinal compare, pinned by
  the line test and the rule theory's tag row; no form, by the guard.
- B-033: `TryLoad`'s three relative `null` cases, pinned by the store theory;
  their fault and finding by the rule theory's three record rows. "Cannot be
  read" reaches the rule tier only through the store, since the rule theory's
  rows are text; the acceptance row holds the whole path.
- B-034: `IsRooted` and the manifest place, pinned by the store theory, the
  rule theory's rooted row and the rooted-record test.
- B-035: the record-less reading, pinned by the model reader theory.

Nothing in this item's scope is hard to test.

## 9. Traceability Matrix

<!-- last written by: test-writer, 2026-11-14 (item 0395: the B-014 to B-016, B-018, B-019 and B-033 to B-035 rows) -->

| Claim ID | Scenario                                                                                                                                                                                                                                                                                                                                                                              | Test                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              | Status  |
| -------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------- |
| B-001    | Claims come from the claim files a set names                                                                                                                                                                                                                                                                                                                                          | `NativeClaimsSteps`; `SpechtModelReaderNativeClaimsUnitTests.AClaimFileSet_WhenReadUnderAGherkinClaimSource_ShouldBuildItsClaimsFromTheFeatureFilesItsGlobMatchesOutsideExclusionsWithNoClaimsTable`                                                                                                                                                                                                                                                                                              | Covered |
| B-002    | Without the selection nothing changes                                                                                                                                                                                                                                                                                                                                                 | `NativeClaimsSteps`; `SpechtManifestParserClaimMembersUnitTests.AManifestWithoutAGherkinClaimSource_WhenParsedUnderZeroFourZero_ShouldReadMarkdownClaimsAndNoClaimFileSet`                                                                                                                                                                                                                                                                                                                        | Covered |
| B-003    | A record supplies lifecycle and edges; No record means a current Feature with no edges                                                                                                                                                                                                                                                                                                | `NativeClaimsSteps`; `SpechtModelReaderNativeClaimsUnitTests.AClaimFileSet_WhenRead_ShouldMakeOneNativeFeatureWithItsRecordsLifecycleAndEdgesOrCurrentAndNoEdgeWithoutARecord`; `SpechtEvaluatorNativeFeaturesUnitTests.ARecordOrItsAbsence_WhenEvaluated_ShouldBeHeldToTheFeatureAndItemSchemasOnlyWhereARecordIsAuthored`; `SpechtEvaluatorNativeFeaturesUnitTests.AMatrixRuleOverANativeFeatureWithAClaim_WhenEvaluated_ShouldReportNothingAndNotFail`                                         | Covered |
| B-004    | A claim tag declares a claim at its element                                                                                                                                                                                                                                                                                                                                           | `NativeClaimsSteps`; `SpechtModelReaderNativeClaimsUnitTests.AClaimTagOnAnElement_WhenRead_ShouldDeclareOneClaimAsAuthoredAtTheElementsKeywordLine`                                                                                                                                                                                                                                                                                                                                               | Covered |
| B-005    | An ordinary tag declares nothing                                                                                                                                                                                                                                                                                                                                                      | `NativeClaimsSteps`; `SpechtModelReaderNativeClaimsUnitTests.ATagTheClaimTagFormOrTheClaimGrammarRejects_WhenRead_ShouldDeclareNoClaim`                                                                                                                                                                                                                                                                                                                                                           | Covered |
| B-006    | An outline declares one claim                                                                                                                                                                                                                                                                                                                                                         | `NativeClaimsSteps`; `SpechtModelReaderNativeClaimsUnitTests.AClaimTagOnAnElement_WhenRead_ShouldDeclareOneClaimAsAuthoredAtTheElementsKeywordLine`, its Scenario Outline row                                                                                                                                                                                                                                                                                                                     | Covered |
| B-007    | A Rule declares once for its scenarios                                                                                                                                                                                                                                                                                                                                                | `NativeClaimsSteps`; `SpechtModelReaderNativeClaimsUnitTests.AClaimTagOnAnElement_WhenRead_ShouldDeclareOneClaimAsAuthoredAtTheElementsKeywordLine`, its Rule row                                                                                                                                                                                                                                                                                                                                 | Covered |
| B-008    | One identifier on two elements is reported at both                                                                                                                                                                                                                                                                                                                                    | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           | Missing |
| B-009    | A claim tag on an Examples block is reported                                                                                                                                                                                                                                                                                                                                          | `NativeClaimsSteps`; `ExamplesClaimTagRuleUnitTests.AClaimTagOnAnExamplesBlock_WhenEvaluated_ShouldBeReportedAsASpec4012ErrorAtTheTagsLineNamingTheClaimIdItWouldDeclare`; `GherkinClaimTextReaderUnitTests.AClaimTagOnAnExamplesBlock_WhenRead_ShouldGiveAPlacementFindingCarryingTheClaimIdAndAMissingClaimFindingCarryingNone`                                                                                                                                                                 | Covered |
| B-010    | A scenario with no claim is reported; An inherited claim satisfies the untagged scenario rule                                                                                                                                                                                                                                                                                         | `NativeClaimsSteps`; `ScenarioWithoutClaimRuleUnitTests.AScenarioInAClaimFile_WhenEvaluated_ShouldBeReportedAsASpec4011WarningAtItsKeywordLineOnlyWhenItCarriesNoClaimOfItsOwnItsRulesOrItsFeatures`                                                                                                                                                                                                                                                                                              | Covered |
| B-011    | Reordering and retitling keep every claim                                                                                                                                                                                                                                                                                                                                             | `NativeClaimsSteps`; `SpechtModelReaderNativeClaimsUnitTests.AClaimFileReorderedAndRetitled_WhenReadAgain_ShouldKeepEveryClaimsIdentifierFeatureIdentityAndStatus`                                                                                                                                                                                                                                                                                                                                | Covered |
| B-012    | Lifecycle tags give the claim its status                                                                                                                                                                                                                                                                                                                                              | `NativeClaimsSteps`; `SpechtModelReaderNativeClaimsUnitTests.ANativeClaim_WhenRead_ShouldTakeItsStatusFromTheLifecycleTagsTheManifestListsOnItsDeclaringElement`; `SpechtManifestParserClaimMembersUnitTests.AGherkinClaimSourceLeavingOutTheTagListsAndLifecycleKey_WhenParsedUnderZeroFourZero_ShouldFillThemFromTheShippedDefaults`                                                                                                                                                            | Covered |
| B-013    | A planned scenario keeps its claim; A planned Examples block keeps its outline's claim                                                                                                                                                                                                                                                                                                | `NativeClaimsSteps`; `SpechtModelReaderNativeClaimsUnitTests.APlannedTagTheManifestLists_WhenRead_ShouldKeepTheClaimAndMarkTheCasesItCoversPlanned`; `SpechtManifestParserClaimMembersUnitTests.AGherkinClaimSourceLeavingOutTheTagListsAndLifecycleKey_WhenParsedUnderZeroFourZero_ShouldFillThemFromTheShippedDefaults`                                                                                                                                                                         | Covered |
| B-014    | A claim file in another language is reported                                                                                                                                                                                                                                                                                                                                          | `NativeClaimsSteps`; `ClaimInputFaultRuleUnitTests.ClaimInputTheCheckCannotTakeClaimsFrom_WhenEvaluated_ShouldBeReportedAsASpec4013ErrorAtItsPlace`; `ClaimInputFaultRuleUnitTests.AClaimFileTheCheckCannotTakeClaimsFrom_WhenRead_ShouldRecordNoClaimFromItUnderAnySet`; `GherkinClaimTextReaderUnitTests.AClaimFileTextTheReaderRefuses_WhenRead_ShouldThrowCarryingTheRefusedLanguageHeadersLineOrZeroForEveryOtherCause`                                                                      | Covered |
| B-015    | A claim file with no Feature line is reported                                                                                                                                                                                                                                                                                                                                         | `NativeClaimsSteps`; `ClaimInputFaultRuleUnitTests.ClaimInputTheCheckCannotTakeClaimsFrom_WhenEvaluated_ShouldBeReportedAsASpec4013ErrorAtItsPlace`; `ClaimInputFaultRuleUnitTests.AClaimFileTheCheckCannotTakeClaimsFrom_WhenRead_ShouldRecordNoClaimFromItUnderAnySet`; `GherkinClaimTextReaderUnitTests.AClaimFileTextTheReaderRefuses_WhenRead_ShouldThrowCarryingTheRefusedLanguageHeadersLineOrZeroForEveryOtherCause`                                                                      | Covered |
| B-016    | A set matching no file is reported                                                                                                                                                                                                                                                                                                                                                    | `NativeClaimsSteps`; `ClaimInputFaultRuleUnitTests.ClaimInputTheCheckCannotTakeClaimsFrom_WhenEvaluated_ShouldBeReportedAsASpec4013ErrorAtItsPlace`                                                                                                                                                                                                                                                                                                                                               | Covered |
| B-017    | One identity declared by a set and a discovered specification; One identity declared by two sets                                                                                                                                                                                                                                                                                      | `NativeClaimsSteps`; `SpechtEvaluatorNativeFeaturesUnitTests.OneIdentityDeclaredBySetsOrBySetAndSpecification_WhenEvaluated_ShouldReportSpec3002AtEachASetAtItsNativeFeaturesPathLineZero`; `SpechtModelReaderNativeClaimsUnitTests.ARecordALayoutAlsoDiscovers_WhenRead_ShouldBeReadOnceAsTheNativeFeatureAndNotAsTheLayoutsFeature`                                                                                                                                                             | Covered |
| B-018    | A file two sets match is reported                                                                                                                                                                                                                                                                                                                                                     | `NativeClaimsSteps`; `ClaimInputFaultRuleUnitTests.ClaimInputTheCheckCannotTakeClaimsFrom_WhenEvaluated_ShouldBeReportedAsASpec4013ErrorAtItsPlace`; `ClaimInputFaultRuleUnitTests.AClaimFileTheCheckCannotTakeClaimsFrom_WhenRead_ShouldRecordNoClaimFromItUnderAnySet`                                                                                                                                                                                                                          | Covered |
| B-019    | A Feature-level tag that disagrees with its set is reported; A Feature-level tag that agrees, or none, is not reported                                                                                                                                                                                                                                                                | `NativeClaimsSteps`; `ClaimInputFaultRuleUnitTests.ClaimInputTheCheckCannotTakeClaimsFrom_WhenEvaluated_ShouldBeReportedAsASpec4013ErrorAtItsPlace`; `GherkinClaimTextReaderUnitTests.FeatureLevelTagsAFeatureScopeFormReads_WhenRead_ShouldGiveEachValueWithTheLineOfItsTag`; `ClaimInputFaultRuleUnitTests.AFeatureLevelTagNoFeatureTagFormReads_WhenEvaluated_ShouldReportNoSpec4013`                                                                                                          | Covered |
| B-020    | A record needs every section but the claims table                                                                                                                                                                                                                                                                                                                                     | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           | Missing |
| B-021    | The companion rules leave a native Feature alone                                                                                                                                                                                                                                                                                                                                      | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           | Missing |
| B-022    | A claim file is not a stray companion                                                                                                                                                                                                                                                                                                                                                 | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           | Missing |
| B-023    | A record's matrix is held to the native claims                                                                                                                                                                                                                                                                                                                                        | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           | Missing |
| B-024    | Without a matrix the trace says none                                                                                                                                                                                                                                                                                                                                                  | Missing                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           | Missing |
| B-025    | The scenarios carrying a claim                                                                                                                                                                                                                                                                                                                                                        | `NativeClaimsSteps`; `GherkinClaimTextReaderUnitTests.AClaimTagOnAnElement_WhenRead_ShouldCarryTheKeywordLinesOfEveryScenarioTheElementReachesInDocumentOrder`                                                                                                                                                                                                                                                                                                                                    | Covered |
| B-026    | A native Feature is counted under a claims entry in the summary; A native Feature is counted under a claims entry in the report; A manifest layout named claims keeps its own entry in the summary; A manifest layout named claims keeps its own entry in the report; A native Feature is reported at its record; A native Feature with no record is reported at its first claim file | `NativeClaimsSteps`; `SpechtModelReaderNativeClaimsUnitTests.ANativeFeature_WhenRead_ShouldBeLocatedInTheClaimsLayoutAtItsRecordOrElseItsFirstClaimFileInOrdinalOrder`; `SpechtEvaluatorNativeFeaturesUnitTests.NativeFeatures_WhenEvaluated_ShouldBeCountedUnderClaimsAfterTheManifestsLayoutsOnlyUnderAGherkinClaimSource`                                                                                                                                                                      | Covered |
| B-027    | A set's identity carries an epic under the epic grammar; A set without an epic under the epic grammar is an invalid manifest; A set with an epic under no epic grammar is an invalid manifest                                                                                                                                                                                         | `NativeClaimsSteps`; `SpechtManifestParserClaimMembersUnitTests.AClaimFileSet_WhenParsedUnderEitherEpicGrammar_ShouldTakeTheIdentityThatGrammarGives`; `SpechtManifestParserClaimMembersUnitTests.AnInvalidClaimFileSet_WhenParsedUnderZeroFourZero_ShouldBeRejectedNamingClaimFiles`, its two `epic` rows                                                                                                                                                                                        | Covered |
| B-028    | An earlier pin is untouched; An earlier pin ignores claim.source                                                                                                                                                                                                                                                                                                                      | `NativeClaimsSteps`; `SpechtManifestParserClaimMembersUnitTests.AClaimMemberUnderAPinBelowZeroFourZero_WhenParsed_ShouldReadTheManifestAsWithoutIt`                                                                                                                                                                                                                                                                                                                                               | Covered |
| B-029    | An invalid claim file set exits before any rule                                                                                                                                                                                                                                                                                                                                       | `NativeClaimsSteps`; `SpechtManifestParserClaimMembersUnitTests.AnInvalidClaimFileSet_WhenParsedUnderZeroFourZero_ShouldBeRejectedNamingClaimFiles`                                                                                                                                                                                                                                                                                                                                               | Covered |
| B-030    | The same tree gives the same output                                                                                                                                                                                                                                                                                                                                                   | `NativeClaimsSteps`; `SpechtModelReaderNativeClaimsUnitTests.ClaimFileSetsAndTheirFiles_WhenRead_ShouldOrderClaimsBySetInManifestOrderThenFileInOrdinalPathOrderThenDocumentAndTagOrder`                                                                                                                                                                                                                                                                                                          | Covered |
| B-031    | A claim source outside the two is an invalid manifest                                                                                                                                                                                                                                                                                                                                 | `NativeClaimsSteps`; `SpechtManifestParserClaimMembersUnitTests.AClaimSourceOutsideTheTwo_WhenParsedUnderZeroFourZero_ShouldBeRejectedNamingClaimSource`                                                                                                                                                                                                                                                                                                                                          | Covered |
| B-032    | A record's claim prefix does not choose the claim grammar                                                                                                                                                                                                                                                                                                                             | `NativeClaimsSteps`; `SpechtModelReaderNativeClaimsUnitTests.ARecordDeclaringAClaimPrefix_WhenRead_ShouldLeaveTheNativeFeatureTheGrammarOfAFeatureWithNoPrefix`                                                                                                                                                                                                                                                                                                                                   | Covered |
| B-033    | A record the check cannot read is reported                                                                                                                                                                                                                                                                                                                                            | `NativeClaimsSteps`; `ClaimInputFaultRuleUnitTests.ClaimInputTheCheckCannotTakeClaimsFrom_WhenEvaluated_ShouldBeReportedAsASpec4013ErrorAtItsPlace`; `SpechtTreeStoreUnitTests.APathTheStoreCannotLoadUnderTheRoot_WhenTriedToLoad_ShouldGiveNothingAndThrowNothing`                                                                                                                                                                                                                              | Covered |
| B-034    | An absolute record is reported at the manifest                                                                                                                                                                                                                                                                                                                                        | `NativeClaimsSteps`; `ClaimInputFaultRuleUnitTests.ClaimInputTheCheckCannotTakeClaimsFrom_WhenEvaluated_ShouldBeReportedAsASpec4013ErrorAtItsPlace`; `ClaimInputFaultRuleUnitTests.ARootedRecord_WhenEvaluated_ShouldPutNoRootedPathInAnyViolation`; `SpechtTreeStoreUnitTests.APathTheStoreCannotLoadUnderTheRoot_WhenTriedToLoad_ShouldGiveNothingAndThrowNothing`; `SpechtTreeStoreUnitTests.APathAManifestWrites_WhenAskedWhetherItIsRooted_ShouldGiveTheAnswerOfTheGuardTryLoadRefusesFirst` | Covered |
| B-035    | A record the check cannot read leaves a record-less Feature                                                                                                                                                                                                                                                                                                                           | `NativeClaimsSteps`; `SpechtModelReaderNativeClaimsUnitTests.AClaimFileSetWhoseRecordTheCheckCannotRead_WhenRead_ShouldMakeTheNativeFeatureItMakesWithNoRecordAndThrowNothing`; `SpechtTreeStoreUnitTests.APathTheStoreCannotLoadUnderTheRoot_WhenTriedToLoad_ShouldGiveNothingAndThrowNothing`                                                                                                                                                                                                   | Covered |
| B-036    | A set with no record and no claim file is no Feature                                                                                                                                                                                                                                                                                                                                  | `NativeClaimsSteps`; `SpechtModelReaderNativeClaimsUnitTests.AClaimFileSetWithNoRecordWhoseGlobMatchesNoFeatureFile_WhenRead_ShouldMakeNoFeatureAndKeepItsEmptyClaimSet`; `SpechtEvaluatorNativeFeaturesUnitTests.AClaimFileSetWithNoRecordAndNoClaimFile_WhenEvaluated_ShouldBeCountedUnderClaimsAsNoneAndComparedWithNoIdentity`                                                                                                                                                                | Covered |
| B-037    | A record's own identity does not override its set's                                                                                                                                                                                                                                                                                                                                   | `NativeClaimsSteps`; `SpechtModelReaderNativeClaimsUnitTests.ARecordDeclaringAnIdentityOtherThanItsSets_WhenRead_ShouldLeaveTheNativeFeatureTheSetsIdentity`                                                                                                                                                                                                                                                                                                                                      | Covered |
| B-038    | A set identity outside its grammar is an invalid manifest                                                                                                                                                                                                                                                                                                                             | `NativeClaimsSteps`; `SpechtManifestParserClaimMembersUnitTests.AnInvalidClaimFileSet_WhenParsedUnderZeroFourZero_ShouldBeRejectedNamingClaimFiles`, its three grammar rows                                                                                                                                                                                                                                                                                                                       | Covered |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-11-14 (item `0395`: OQ-19 and OQ-20 resolved; B-015 widened); before: spec-author, 2026-11-14 (item `0395`: OQ-19 and OQ-20 raised; readings on `SPEC4013`'s identifiers and B-018); before: spec-author, 2026-11-14 (item `0394`: the B-009 scenario corrected, B-009 amended); before: spec-author, 2026-11-14 (item `0393` review round 1: the decision 0004 entry's § 7 note stated as of its writing); before: spec-author, 2026-11-14 (decision 0004: `SPEC4011` to `SPEC4013`); before: spec-author, 2026-11-14 (`0001-F7` OQ-75 resolved, moot under `0.4.0`); before: spec-author, 2026-11-14 (item 0391: the scenarios' line-0 diagnostic lines corrected to the printed form without a position) -->

- 2026-11-14, item `0395`, the owner's selections, given directly on a question the coordinating session put, its options in that session's wording and built from spec-author's proposed defaults: OQ-19, "(a) All as SPEC4013 (Recommended)"; OQ-20, "(a) Read as no form (Recommended)".
  - Amended, keeping its id: B-015, from a file with no `Feature:` line to every text the reader refuses but a `# language:` header (B-014), each cause named; was "Given a claim file with no `Feature:` line". The list of causes is spec-author's, from the reader's refusals as § 7 names them.
  - Unchanged: the `@B-015` scenario "A claim file with no Feature line is reported", one of the causes, and its title; no scenario states the other causes. § 9's B-015 row is test-writer's.
  - OQ-20 changes no claim: under its reading no verdict is added, and no claim yet states that a `featureTag` without exactly one `{feature}` is read as no form.
  - Item `0395`'s B-015 criterion and summary follow B-015.

- 2026-11-14, item `0395`, spec-author, on the implementer's design (§ 7, "An unreadable claim file, set or record"), not an owner answer.
  - Raised: OQ-19, whether every claim file text the reader refuses is `SPEC4013`, proposed default (a), each at line `0` as B-015's is; OQ-20, a `claim.scopes.featureTag` without exactly one `{feature}`, proposed default (a), read as no form. No claim changes until the owner answers. Item `0394`'s § 7 reading, that an `Examples:` with no outline is `SPEC4013`'s, holds under OQ-19 (a).
  - Spec-author's reading, the identifiers: no claim states the identifier of a `SPEC4013` line, and no `SPEC4013` scenario asserts one, so it is § 7's and the rule page's, as the message is. B-008 and B-009 state theirs, B-009 because its scenario names the claim; a scenario that asserts a `SPEC4013` identifier needs its claim amended first.
  - Spec-author's reading, B-018: a file two sets match is still a claim file, so B-009 and B-010 still report its text, by `SPEC4012` and `SPEC4011`; B-018 takes away only its claims. Each scenario and each Examples tag is reported once, the file being one file whichever set reads it (B-030). No claim changes.

- 2026-11-14, item `0394`, spec-author's correction, not an owner answer: the B-009 scenario "A claim tag on an Examples block is reported" ended `naming "B-005"`, and that step takes a rule id, so it looked for ` B-005:`, which no diagnostic line carries. It now reads `Then the output has a line ".spec/features/arithmetic/add.feature(14): error" naming "SPEC4012" with the identifier "B-005"`: the tag's line, the rule, and the claim as the identifier, as the `SPEC3002` scenarios name theirs. B-009 is amended to state the identifier, the claim id, as B-008 states it for `SPEC4003`; was "at that tag's path and line and records no claim". The scenario title and § 9 are unchanged.
- 2026-11-14, [decision 0004](decisions/0004-the-native-claim-rules-take-spec4011-to-spec4013.md): the owner selected "Fixed 4011, 4012, 4013 (Recommended)", an option in the coordinating session's wording. The working names take ids in the claims group, in old-id order, whatever order their items merge in: `SPEC025` is `SPEC4011`, `SPEC026` is `SPEC4012` and `SPEC027` is `SPEC4013`. It is the owner's approval the 2026-11-02 entry below names, for these three. Amended, each keeping its id: B-009, B-010, B-014 to B-016, B-018, B-019, B-033 and B-034, and their scenarios. Tasks names the ids. § 7 is the implementer's and was not edited here; it named the working names when this entry was written, and the implementer has since edited it for item `0393`. No id enters `0.4.0`'s vocabulary here; each enters with the item that builds its rule (C-3).
- 2026-11-14, `0001-F7` OQ-75 (`src/tool/.spec/README.md` § 11) is resolved on the owner's selection "(c) Moot under 0.4.0", relayed through the coordinating session, whose wording the option is. This supersedes the 2026-11-13 entry below, which read OQ-75 as no longer bearing on this Feature and left it open in `0001-F7` for the owner; that entry is kept as written. Items `0393` to `0395` no longer wait on OQ-75. No claim, constraint or scenario of this Feature changed, and decision 0001 is unchanged.
- 2026-11-14, spec-author's correction, not an owner answer: ten scenario steps expected a line-`0` finding printed with a position, `<path>(0):` or `<path>(`, but the diagnostic line carries `(<line>)` only for a line above `0` (`SpechtViolation.ToString`), so a line-`0` finding prints `<path>:`. The steps of B-015, B-016, B-017 (both scenarios), B-018, B-026 ("A native Feature is reported at its record" and "A native Feature with no record is reported at its first claim file"), B-033 and B-034 now expect `<path>:`. Every claim still says line `0` and is unchanged; the printed form is not changed, since it governs every line-`0` diagnostic in every tree.

- 2026-11-13, spec-author's reading on test-writer's finding 1 (§ 8, item `0390`), not an owner answer: B-004's scenario outline put a tagged Feature at line 1, but a claim tag sits on a line before the keyword it declares on, so a tagged Feature's keyword is at line 2 at the earliest. The Examples row now says line 2. The Rule, Scenario and Scenario Outline rows, at lines 4, 7 and 12, leave a line for the tag below the Feature line and stand. B-004 is unchanged.
- 2026-11-13, spec-author's reading on test-writer's finding 2 (§ 8, item `0390`), not an owner answer: every shipped manifest's `exclusions` holds `/.spec`, so under B-001 as written, which reads a set's files outside `exclusions`, the Background's glob `.spec/features/arithmetic/*.feature` matches nothing, and the acceptance root removed the entry with no step saying so. The Background now states the precondition: the manifest's `exclusions` does not hold `/.spec`. Whether a claim file set honours `exclusions` went to the owner as OQ-10. Resolved 2026-11-13: the owner selected "(a) Keep B-001 (Recommended)"; B-001 and B-016 stand as written.
- 2026-11-13, the owner's selection for item `0391`, relayed 2026-11-13. The question, in the coordinating session's wording, not the owner's: "does a claim file set's `record` claim prefix choose the native Feature's claim grammar?" The owner selected "(b) No, record is lifecycle". Spec-author's reading of that label: a set's `record` supplies frontmatter, lifecycle and edges only, and the tree's claim grammar reads the tags. Added by item `0391`: B-032 and its scenario "A record's claim prefix does not choose the claim grammar"; the claim's and the scenario's wording, and naming the record's `claim_prefix` as the prefix in question, are spec-author's. B-003 and its two scenarios stand unchanged: they state what a record supplies and say nothing of the grammar, so nothing in them contradicts the selection (spec-author's reading).
- 2026-11-13, the owner's selections on implementer's six questions for item `0391` (§ 7, "A claim file set's record, identity and report"), relayed 2026-11-13 by the coordinating session and confirmed by the owner directly in session ("Yes, all six are mine"); the questions were implementer's, put in the coordinating session's wording, and the owner's words are the labels alone, recorded as OQ-11 to OQ-16. Amended, each keeping its id: B-017 (OQ-12, "(a) Feature path, line 0 (Recommended)"): a set is reported at its native Feature's path, line `0`; B-026 (OQ-11, "(a) claims layout entry (Recommended)", and OQ-16, "(a) Two claims entries (Recommended)"): a native Feature is counted in one `claims` entry after the manifest's layouts, beside any layout of that name, and "lists" is the path its violations carry. Added: B-033 and B-035 (OQ-13, "(a) SPEC027, record-less (Recommended)"), split because the report and the record-less reading are two assertions, cut to item `0395`; B-036 (OQ-14, "(a) Not a Feature (Recommended)") and B-037 (OQ-15, "(a) Silent, set wins (Recommended)"), cut to item `0391`; § 5 row 16 holds the silence OQ-15 selected. Scenarios: B-017's two and B-026's two are restated, B-026 gains "A manifest layout named claims keeps its own entry", and B-033 to B-037 each have one, with the `@boundary` scenario "A record's differing identity is not reported". Every claim, scenario and row wording, and each reading of a label, is spec-author's.
- 2026-11-13, OQ-17, raised by spec-author on OQ-13: its label reports a bad `record` at the record's path, but C-2 and brief § 9 keep an absolute path out of every output. The owner selected "At manifest, line 0 (Recommended)", 2026-11-13, in session, relayed by the coordinating session; the question was in the coordinating session's wording. Added: B-034, cut to item `0395`; B-035 reads an absolute record as record-less too. The wording is spec-author's.
- 2026-11-13, spec-author, not an owner answer: the Background's root is pinned to `0.4.0` with the defaults, and the shipped `0.4.0` manifest declares `identifiers.epic`, so under B-027 the Background's set, which has no `epic`, would be an invalid manifest; the acceptance root drops the grammar with no step saying so. The Background now states the precondition, "And the manifest declares no epic grammar", as it states the exclusions one. No claim changes.
- 2026-11-13, spec-author, not an owner answer: no claim of this Feature states whether a claim file set's `feature`, or its `epic`, must match the manifest's feature or epic grammar (§ 7, item `0389`, "B-027 under each grammar"); the scenarios' identity `arithmetic` does not match the shipped `0.4.0` feature grammar `^F[0-9]+[a-z]?$`. Not decided here; put to the coordinating session as a question for the owner, and to be raised in § 11 if the owner does not rule on it. Raised and resolved as OQ-18 on the same day, in the next entry.
- 2026-11-13, OQ-18: the owner selected "Must match; exit 3 (Recommended)", in session, relayed by the coordinating session; the question was in the coordinating session's wording, drawn from spec-author's options. Added: B-038, cut to item `0391`, and its scenario outline "A set identity outside its grammar is an invalid manifest". Every scenario's Feature identity is now a grammar id: `arithmetic` is `F1`, `sums` is `F2`, in step text and in the `@spec.feature:` tags of B-019's scenarios; paths keep their folder names. B-001 and B-027 are unchanged. The wording is spec-author's.
- 2026-11-13, spec-author's reading on test-writer's two scenario findings (item `0391`), not an owner answer. (1) B-003's "No record means a current Feature with no edges" named no claim file, so under B-036 its set was no Feature; it now states that the glob matches `.spec/features/arithmetic/add.feature`. (2) B-026's two count scenarios and B-036's scenario ran the check with `--json` and read "the summary", but `--json` replaces the summary lines with the report document. Each B-026 scenario is split into one that runs without `--json` and reads the summary lines, and one that runs with it and reads the report's `layouts`, the two places B-026 names; B-036's scenario runs without `--json`, reads the summary lines, and now says they count no Feature under `claims`, which B-036 states, rather than a count of 0, which no claim states. No claim changes.

- 2026-11-13, [decision 0003](decisions/0003-a-claim-source-outside-markdown-and-gherkin-is-an-invalid-manifest.md): no claim stated what a `claim.source` other than `markdown` or `gherkin` does under `0.4.0`, and § 7 rejected it with a row it kept out of the build. The owner selected "Exit 3". Added: B-031 and its scenario "A claim source outside the two is an invalid manifest". The wording, the case-exact and non-text reading, and the `schemaSource` and `pinPolicy` precedent (`0001-F7` B-056) are spec-author's reading. Below `0.4.0` the member stays unread (B-028; decision 0002).
- 2026-11-13, spec-author's reading on test-writer's two questions (§ 8 "Not asserted"), not an owner answer. B-027's "both required" already made an entry with no `epic` under `identifiers.epic` invalid; B-027 now says so, with the scenario "A set without an epic under the epic grammar is an invalid manifest". An omitted `claim.source` reads as `markdown` (OQ-9), so `claim.files` with no `claim.source` is `claim.files` beside another source; B-029 now says so, with an Examples row, and B-002 is narrowed to a manifest with no `claim.files`, removing its conflict with B-029, which made `markdown` beside `claim.files` exit `3` already. Each keeps its id. The type faults § 7 lists, a `claim.files` that is not a list, an entry that is not an object and a `glob`, `feature` or `record` that is not text, stay unclaimed: each exits `3` either way (`0001-F2` B-007), and whether stderr names the key is § 7's design, not a verdict.

- 2026-11-13, [decision 0002](decisions/0002-claim-members-go-unread-below-schema-0-4-0.md): B-028, A-4, B-028's second scenario and item `0389`'s B-028 criterion said a pin below `0.4.0` refuses a new `claim` member with exit `3` as today, but no pin does: the parser reads `claim.tag` and `claim.tagIgnoreCase` alone and leaves every other `claim` member unread, so a `0.3.1` manifest setting `claim.source` checks as it does without it. The owner selected "Ignore below 0.4.0". Amended, each keeping its id: A-4 and B-028. B-028's second scenario is now "An earlier pin ignores claim.source"; § 9 is unchanged, its B-028 row naming the first scenario. The basis, `0001-F7` C-1 and C-4 with `0001-F5` decision 0021 call 7 as precedent, and every wording, are spec-author's reading.
- 2026-11-13, spec-author's reading, not an owner answer: `0001-F7` OQ-75 (`src/tool/.spec/README.md` § 11), which names items `0393` to `0395` among what it blocks, no longer bears on this Feature. It asks whether B-154's same report sets aside the rule count once `0.3.1` holds a rule id `0.3.0` lacks, but this Feature's rules enter `0.4.0` (decision 0001), and `0001-F7` B-154 compares patch neighbours only, which `0.3.1` and `0.4.0` are not (`0001-F7` OQ-78). OQ-75 stays open in `0001-F7` for the owner; this Feature does not edit it.

- 2026-11-12, [decision 0001](decisions/0001-native-claims-fold-into-schema-0-4-0.md): schema `0.3.1` has gone out in NuGet packages (`specht.tool` `0.5.35-preview`, `0.6.1-preview` and `0.6.4-preview`; tags `v0.5.35-preview`, `v0.6.1-preview` and `v0.6.4-preview` each hold `src/tool/manifest/v0.3.1/manifest.json`), so `0001-F7` C-1 and C-17 forbid the relock OQ-5 planned. Of the two options put to the owner, "0.3.2 patch" and "Fold into 0.4.0", the owner selected "Fold into 0.4.0": the `claim` members and the three rules enter schema `0.4.0`, cut by item `0436`. Amended, each keeping its id: A-1, A-2, A-4, B-001, B-028 and C-3. The `.feature` Background pins `0.4.0` and names `ids.claim`, B-028's scenarios pin `0.3.1`, and Tasks names `0436`. OQ-5's resolution stands as written, with a note. Item `0389` waits on `0436` as well as `0388`. The basis, `0001-F7` C-11's `0.minor` clause, is spec-author's reading.
- 2026-11-06, the owner answered § 11 in session. OQ-2, OQ-3, OQ-4 and OQ-6 to OQ-9 take their proposed defaults ("Accept all defaults"). OQ-5 is ruled: this Feature's `claim` members and its three rules enter a new patch schema version, `0.3.1`, under `0001-F7` C-11 as amended ([`0001-F7` decision 0022](../../../../tool/.spec/decisions/0022-an-opt-in-addition-below-1-0-0-is-a-patch.md)), at the proposed levels. Each rule takes its four-digit id when it enters `0.3.1`. Amended, each keeping its id: A-2, A-4, B-001, B-028 and C-3. The `.feature` file's Background pins `0.3.1`, and B-028's scenarios pin `0.3.0`. OQ-2 and OQ-3 name `SPEC3002` and `SPEC4004` where they named `SPEC012` and `SPEC031` (ADR-0029). The owner approved the claims for cutting; `spec_status` moves to `in-review`, and Tasks lists `0387` to `0396`.
- 2026-11-02, item `0169` ([ADR-0029](../../../../../.spec/adr/0029-rule-ids-take-four-digits-in-schema-0-3-0-and-published-versions-keep-theirs.md)): the shipped rules this Feature's claims and scenarios name take schema `0.3.0`'s four-digit ids, each claim keeping its id. C-3 amended: a verdict this Feature adds takes a four-digit id. `SPEC024` to `SPEC027` are working names no shipped version holds; each takes a four-digit id in its group when it enters a version, on the owner's approval.

## 11. Open Questions

<!-- last written by: spec-author, 2026-11-14 (OQ-19 and OQ-20 resolved by the owner's selections); before: spec-author, 2026-11-14 (item `0395`: OQ-19 and OQ-20 raised on the implementer's open points); before: spec-author, 2026-11-14 (item `0393` review round 1: OQ-5 notes decision 0004); before: spec-author, 2026-11-13 (OQ-11 to OQ-16 recorded, resolved by the owner's selections on implementer's questions for item 0391; OQ-17 and OQ-18 raised by spec-author and resolved by the owner's selections) -->

| ID    | Question                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             | Blocks                                                                          | Resolution                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |
| ----- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1  | Which manifest key selects native claim input for the check: the analyzer's `codeAnalysis.records.claimSource` and `claimFileSets`, or a checker-level key beside `layouts`?                                                                                                                                                                                                                                                                                                                                                                         | B-001, B-002, B-029                                                             | RESOLVED, 2026-10-26, by the owner, in session: `codeAnalysis` is the manifest's one C#-specific section and holds nothing else; the tool serves every technology its text rules can read, so a setting the checker reads is a top-level key the checker owns, and the analyzer reads it there ([ADR-0023](../../../../../.spec/adr/0023-codeanalysis-is-the-only-csharp-specific-manifest-section.md), C-7). The key's name and shape are OQ-9.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              |
| OQ-2  | Is the selection additive - specifications the layouts discover stay Markdown-checked beside the native Features the sets name - or does `gherkin` make every Feature native?                                                                                                                                                                                                                                                                                                                                                                        | B-001, B-002, B-017                                                             | RESOLVED, 2026-11-06, by the owner, in session ("Accept all defaults"): the proposed default is accepted. Additive: a layout-discovered specification is checked as today; a set's `record` is the one document whose claims table is not read (B-001, B-020); the same identity from both is `SPEC3002` (B-017; `SPEC012` below `0.3.0`, ADR-0029). Rejected: a repository-wide switch that leaves a mixed tree unable to adopt one Feature at a time.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       |
| OQ-3  | Where does § 9 live for a Feature with no `record`, and does a record-less Feature have coverage the check judges?                                                                                                                                                                                                                                                                                                                                                                                                                                   | B-023, B-024; § 5 row 9                                                         | RESOLVED, 2026-11-06, by the owner, in session ("Accept all defaults"): the proposed default is accepted. A `record` carries § 9 and `SPEC4004` (`SPEC031` below `0.3.0`, ADR-0029) and `0001-F12` judge it against the native claims (B-023); with no record there is no matrix, no matrix rule reports, and the trace lists each claim as `none` (B-024). Rejected: test names carried as scenario tags, which would make the companion also the matrix and would need `0001-F12` to read a tag as a Test cell; recorded in § 5 row 9.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      |
| OQ-4  | Which section rules does a `record` obey: every contracted section but the claims table, every section, or none beyond frontmatter?                                                                                                                                                                                                                                                                                                                                                                                                                  | B-003, B-020                                                                    | RESOLVED, 2026-11-06, by the owner, in session ("Accept all defaults"): the proposed default is accepted. Every contracted section and table but the one `roles.claims` names (B-020), so a record is a specification whose § 3 moved into Gherkin and nothing else moved. Rejected: a frontmatter-only record, which would leave § 9 and § 12 without a home; requiring an empty claims table, which is the duplication removed.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             |
| OQ-5  | Which ids and version carry the three new verdicts, and at what default level?                                                                                                                                                                                                                                                                                                                                                                                                                                                                       | B-001, B-009, B-010, B-014 to B-019, B-028                                      | RESOLVED, 2026-11-06, by the owner, in session. Version, verbatim: "Only move to 0.4.0 is we are making BREAKING CHANGES to the schema. Additions are not breaking as long as they are not required." Then the owner chose "0.3.1 patch, amend C-11". A new patch schema version, `0.3.1`, carries this Feature's `claim` members and its three rules. They report only when `claim.source` is `gherkin`, so no existing tree's verdict changes. `0001-F7` C-11 is amended so that below `1.0.0` an opt-in addition is a patch (`0001-F7` decision 0022). Published `0.3.0` stays frozen (`0001-F7` C-1). Levels: the proposed defaults stand - a scenario with no claim at warning, a claim tag on an Examples block at error, an unreadable claim file or set at error. Ids: not decided. `SPEC025`, `SPEC026` and `SPEC027` stay working names, and each rule's item assigns the next free id in its ADR-0029 group when the rule enters `0.3.1`, for the owner's approval in that pull request. Rejected: changing `0.3.0` in place; `0.3.1` without amending C-11; a new `0.4.0`, which the owner keeps for a breaking change. A-2, A-4, B-001, B-028 and C-3 amended. As raised: proposed default: `SPEC025` a scenario with no claim, at warning (B-010, as `0201-F7` OQ-5 chose for SPCT0012); `SPEC026` a claim tag where no claim can be declared, at error (B-009); `SPEC027` a claim file or set the check cannot read, at error (B-014 to B-016, B-018, B-019); all three in schema `0.3.0` (A-2, C-3), relocking nothing. Rejected by the default: adding to `0.2.0`, which has shipped. Noted 2026-11-02 by spec-author (item `0169`): under ADR-0029 each verdict takes a four-digit id in its group, so the three names above are working names; `0.3.0` has since been cut (item `0339`), and whether a verdict may still enter it in place while no published package carries it (lesson 0016; `0001-F7` C-17) is part of this question, for the owner. Noted 2026-11-12 by spec-author ([decision 0001](decisions/0001-native-claims-fold-into-schema-0-4-0.md)): the version moved to `0.4.0`. `0.3.1` has gone out in NuGet packages, so the relock this resolution names is forbidden (`0001-F7` C-1, C-17); of the options "0.3.2 patch" and "Fold into 0.4.0", the owner selected "Fold into 0.4.0", and the members and rules enter schema `0.4.0`, cut by item `0436`. The levels and the undecided ids stand; each rule's item assigns its id when the rule enters `0.4.0`. A-2, A-4, B-001, B-028 and C-3 are amended again. Noted 2026-11-14 by spec-author ([decision 0004](decisions/0004-the-native-claim-rules-take-spec4011-to-spec4013.md)): the ids are decided. On the owner's selection "Fixed 4011, 4012, 4013 (Recommended)", `SPEC025` is `SPEC4011`, `SPEC026` is `SPEC4012` and `SPEC027` is `SPEC4013`, in the claims group, whatever order their items merge in; no item assigns the next free id. Each id enters `0.4.0` with the item that builds its rule. The levels stand. |
| OQ-6  | How does the analyzer come to share the checker's reader once it exists: a reference to the engine, a source link, or a kept copy held equal by the characterization test?                                                                                                                                                                                                                                                                                                                                                                           | None here; § 5 row 2                                                            | RESOLVED, 2026-11-06, by the owner, in session ("Accept all defaults"): the proposed default is accepted. No change in this Feature; `0201-F7` ADR-0001 item 3's parity test holds the two readers to one answer, and the implementer records the dependency form as an ADR when the engine's reader is built, under ADR-0014's gate. Rejected: a source link, which PR 114 ruled out; a package reference the analyzer's dependency closure test would have to admit.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        |
| OQ-7  | How is a native Feature named in the summary and the report: under which layout name, and by which path?                                                                                                                                                                                                                                                                                                                                                                                                                                             | B-026                                                                           | RESOLVED, 2026-11-06, by the owner, in session ("Accept all defaults"): the proposed default is accepted. Counted among `features`, listed under a layout named `claims`, by its `record` path when it has one and otherwise by its first claim file in ordinal order (B-026). Rejected: listing the Feature by its glob, which is not a path the reader can open.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            |
| OQ-8  | When a claim file set's glob matches no file, what path does the violation carry, or is it an invalid manifest?                                                                                                                                                                                                                                                                                                                                                                                                                                      | B-016, B-029                                                                    | RESOLVED, 2026-11-06, by the owner, in session ("Accept all defaults"): the proposed default is accepted. A violation, `SPEC027` at line `0` with the glob as written for its path (B-016), since the manifest is well-formed and the tree is what is missing; an entry without `glob` or `feature` stays exit `3` (B-029). Rejected: exit `3`, which would end the run on one Feature's missing files and report nothing else.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               |
| OQ-9  | What are the name and shape of the top-level key that carries the settings `0201-F7` put under `codeAnalysis.records`?                                                                                                                                                                                                                                                                                                                                                                                                                               | B-001, B-002, B-012, B-013, B-019, B-027, B-029                                 | RESOLVED, 2026-11-06, by the owner, in session ("Accept all defaults"): the proposed default is accepted. The existing `claim` object grows the members `source` (`markdown`, the default, or `gherkin`), `files` (a list of `{glob, feature, epic?, record?}`), `plannedTags` (default `["@ignore"]`), `withdrawnTags` and `supersededTags` (default empty), `lifecycleKey` (default `spec_status`) and `scopes` with `featureTag` and `epicTag`, each missing member filled from the pinned manifest as `claim.tag` is today; `0201-F7`'s delta renames its keys to these. Rejected: a new top-level `claims` one letter from `claim`; keeping the analyzer's names under `codeAnalysis`, which ADR-0023 rules out.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         |
| OQ-10 | Does a claim file set read the files its glob matches outside the manifest's `exclusions`, when every shipped manifest excludes `/.spec`, so a glob under a `.spec/` directory matches nothing?                                                                                                                                                                                                                                                                                                                                                      | B-001, B-016                                                                    | RESOLVED, 2026-11-13: the owner selected "(a) Keep B-001 (Recommended)", relayed 2026-11-13 by the coordinating session; the question put to the owner was in that session's wording, not the owner's. Spec-author's reading of that label: a claim file set honours `exclusions`; no claim changes, B-001 and B-016 stand as written, and a consumer whose claim files sit under `/.spec` removes that entry, as the Background states. Raised 2026-11-13 by test-writer's finding 2 (§ 8, item `0390`); the question, the default and the options are spec-author's wording. Rejected: (b) a set's glob ignores exclusions; (c) the shipped `0.4.0` default drops `/.spec` from `exclusions`.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               |
| OQ-11 | What do B-026's "counted among `features`" and "lists it" observe for a native Feature?                                                                                                                                                                                                                                                                                                                                                                                                                                                              | B-026                                                                           | RESOLVED, 2026-11-13: the owner selected "(a) claims layout entry (Recommended)", relayed 2026-11-13 by the coordinating session and confirmed by the owner directly in session; the question was implementer's (§ 7, item `0391`), in the coordinating session's wording, not the owner's. Spec-author's reading of that label: the summary and the report's `layouts` count a native Feature in an entry named `claims`, after the manifest's layouts, and "lists" is the path its violations carry; no other report member changes (B-026). Rejected: (b) a new report member listing each Feature, which would change `0001-F3` B-005; (c) counting it under the manifest's layout named `features`.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      |
| OQ-12 | Where does `SPEC3002` report a claim file set that declares an identity declared elsewhere (B-017)?                                                                                                                                                                                                                                                                                                                                                                                                                                                  | B-017                                                                           | RESOLVED, 2026-11-13: the owner selected "(a) Feature path, line 0 (Recommended)", relayed 2026-11-13 by the coordinating session and confirmed by the owner directly in session; the question was implementer's (§ 7, item `0391`), in the coordinating session's wording, not the owner's. Spec-author's reading of that label: at the native Feature's path, its record or its first claim file (B-026), line `0` (B-017). Rejected: (b) at `.specht/manifest.json`, line `0`; (c) at a record's frontmatter id line, else line `0`.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       |
| OQ-13 | What does the check do with a claim file set whose `record` names no file, names a file it cannot read, is absolute or leads outside the root?                                                                                                                                                                                                                                                                                                                                                                                                       | B-033, B-035                                                                    | RESOLVED, 2026-11-13: the owner selected "(a) SPEC027, record-less (Recommended)", relayed 2026-11-13 by the coordinating session and confirmed by the owner directly in session; the question was implementer's (§ 7, item `0391`), in the coordinating session's wording, not the owner's. Spec-author's reading of that label: `SPEC027` at the record's path, line `0`, as B-014 reports an unreadable claim file (B-033), and the set read as having no `record` (B-035); cut to item `0395`. An absolute record's path is OQ-17's. Rejected: (b) an invalid manifest, exit `3`, naming the entry's `record`; (c) read as record-less with nothing reported.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             |
| OQ-14 | Is a claim file set with no `record` and no claim file a Feature?                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    | B-036                                                                           | RESOLVED, 2026-11-13: the owner selected "(a) Not a Feature (Recommended)", relayed 2026-11-13 by the coordinating session and confirmed by the owner directly in session; the question was implementer's (§ 7, item `0391`), in the coordinating session's wording, not the owner's. Spec-author's reading of that label: it is no Feature, not counted and its identity compared with none (B-036); B-016 still reports its glob. Rejected: (b) a Feature listed by its glob, which OQ-7 rejected as a path.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                |
| OQ-15 | When a set's `record` declares an `id` or an `epic` other than the set's, which identity holds, and is the difference reported?                                                                                                                                                                                                                                                                                                                                                                                                                      | B-037; § 5 row 16                                                               | RESOLVED, 2026-11-13: the owner selected "(a) Silent, set wins (Recommended)", relayed 2026-11-13 by the coordinating session and confirmed by the owner directly in session; the question was implementer's (§ 7, item `0391`), in the coordinating session's wording, not the owner's. Spec-author's reading of that label: the set's identity holds (B-037), the record's keys are held by `SPEC1002`'s schema alone, and the difference is not reported (§ 5 row 16). Rejected: (b) reporting the difference, which needs a rule; (c) the record's identity holding.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      |
| OQ-16 | What does the summary carry when a manifest layout is already named `claims`?                                                                                                                                                                                                                                                                                                                                                                                                                                                                        | B-026                                                                           | RESOLVED, 2026-11-13: the owner selected "(a) Two claims entries (Recommended)", relayed 2026-11-13 by the coordinating session and confirmed by the owner directly in session; the question was implementer's (§ 7, item `0391`), in the coordinating session's wording, not the owner's. Spec-author's reading of that label: the manifest's layout keeps its entry and the native Features' `claims` entry follows it, so two entries carry the name (B-026). Rejected: (b) the native count merged into the layout's; (c) an invalid manifest, exit `3`.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  |
| OQ-17 | Under OQ-13, where does `SPEC027` report a claim file set whose `record` is an absolute path, when C-2 and brief § 9 keep an absolute path out of every output?                                                                                                                                                                                                                                                                                                                                                                                      | B-034, B-035                                                                    | RESOLVED, 2026-11-13: the owner selected "At manifest, line 0 (Recommended)", in session, relayed by the coordinating session; the question was in the coordinating session's wording, raised by spec-author on OQ-13. Spec-author's reading of that label: `SPEC027` at `.specht/manifest.json`, line `0`, where the set declares the record (B-034), and the set read as record-less (B-035); cut to item `0395`. Rejected: the native Feature's first claim file, line `0`; an invalid manifest, exit `3`, for this case alone.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            |
| OQ-18 | Must a claim file set's `feature`, and its `epic`, match the manifest's feature and epic grammars, and how is one that does not reported?                                                                                                                                                                                                                                                                                                                                                                                                            | B-038                                                                           | RESOLVED, 2026-11-13: the owner selected "Must match; exit 3 (Recommended)", in session, relayed by the coordinating session; the question was in the coordinating session's wording, drawn from options spec-author raised. Spec-author's reading of that label: a set whose `feature` or `epic` fails its grammar is an invalid manifest, exit `3` before any rule runs, naming `claim.files`, beside B-027 and B-029 (B-038); cut to item `0391`. Rejected: a violation at the native Feature's path, line `0`, as `SPEC3001` holds a layout's path identity; no check, a set's identity being any text.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |
| OQ-19 | Is every claim file text the reader refuses reported as `SPEC4013`, or only the cases B-014 and B-015 name? Beside a `# language:` other than `en` (B-014) and no `Feature:` line (B-015), the reader refuses a ragged table, a malformed tag token, an unterminated doc string, a second `Feature:` line, tags before nothing, and an `Examples:` with no outline above it. Each such file declares no claim, and no claim says whether it is reported. Raised 2026-11-14 by spec-author, on the implementer's first open point (§ 7, item `0395`). | B-015; item `0395`; item `0394`'s § 7 reading of an `Examples:` with no outline | RESOLVED, 2026-11-14: the owner selected "(a) All as SPEC4013 (Recommended)", directly, on a question the coordinating session put; the option's wording was the coordinating session's, built from spec-author's proposed default. Spec-author's reading of that label: reading (a); every text the reader refuses but a `# language:` header is `SPEC4013` at line `0`, with no claim from the file, and B-015 is amended to say so. Rejected: (b), only a file with no `Feature:` line reported; (c), each refused text at the line the reader stops on. As raised: Owner not asked. Proposed default, spec-author's: (a). Readings: (a) every refused text is reported as B-015's is, `SPEC4013` at line `0`, with no claim from the file, and B-015 is widened to say so; the analyzer reports each such file as an input failure (`0201-F7` B-014, "not valid Gherkin"), and OQ-5's accepted default describes the rule as "a claim file or set the check cannot read"; (b) only a file with no `Feature:` line is reported, and the other refused texts declare nothing, silently, as before item `0395`, which needs a member on the reader's shared exception to tell them apart; (c) each refused text is reported at the line the reader stops on, which needs that member and a line for each cause.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              |
| OQ-20 | What does a `claim.scopes.featureTag` without exactly one `{feature}` placeholder do? No claim states it. Item `0390`'s parser does not check the form, and the reader's tag form throws on it when built, outside the per-file read. The analyzer fails the manifest for such a form, with the `claim.tag` message (`0201-F7` § 9, B-014 row). Raised 2026-11-14 by spec-author, on the implementer's second open point (§ 7, item `0395`).                                                                                                         | B-019; B-029; item `0395`                                                       | RESOLVED, 2026-11-14: the owner selected "(a) Read as no form (Recommended)", directly, on a question the coordinating session put; the option's wording was the coordinating session's, built from spec-author's proposed default. Spec-author's reading of that label: reading (a); a `claim.scopes.featureTag` without exactly one `{feature}` is read as no form, so B-019 reports nothing and the run continues. No claim states it yet. Rejected: (b), an invalid manifest, exit `3`, naming `claim.scopes.featureTag`. As raised: Owner not asked. Proposed default, spec-author's: (a), which adds no verdict, so item `0395` proceeds without a claim the owner has not approved. Readings: (a) the form is read as no form: B-019 reports nothing and the run continues; (b) an invalid manifest, exit `3` before any rule runs, naming `claim.scopes.featureTag`, as B-029 names `claim.files`, which agrees with the analyzer (A-3) and needs a claim beside B-029 and a change to item `0389`'s parser.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-11-14 (item `0395`, round 1: 3, 6-11 blocked on one implementer finding, the namespace cycle; spec_status in-review); spec-reviewer, 2026-11-14 (item `0394`, round 2: 3, 6-10 approved, the round-1 findings cleared; spec_status approved); spec-reviewer, 2026-11-14 (item `0394`, round 1: 3, 6-10 blocked on two test-writer findings in § 8 and one implementer finding in § 7; spec_status in-review); spec-reviewer, 2026-11-14 (item `0393`, round 2: 3, 6-11 approved, the round-1 findings cleared; spec_status approved); spec-reviewer, 2026-11-14 (item `0393`, round 1: 3, 6-11 blocked on record text in § 8, § 10 and § 11; spec_status in-review); spec-reviewer, 2026-11-14 (item `0391`, round 3: 6-9 approved, the round-1 and round-2 findings cleared; spec_status approved); spec-reviewer, 2026-11-14 (item `0391`, round 2: blocked on one mis-cited implements line; spec_status in-review); spec-reviewer, 2026-11-14 (item `0391`, round 1: blocked on the unskipped SPEC4008; spec_status in-review); spec-reviewer, 2026-11-13 (item `0390`, round 2: 6-9 approved, the round-1 findings cleared; spec_status approved); spec-reviewer, 2026-11-13 (item `0390`, round 1: blocked on two test-writer findings in § 8); spec-reviewer, 2026-11-13 (item `0389`, round 2: 6-9 approved, the round-1 blocks cleared); spec-reviewer, 2026-11-13 (item `0389`, round 1: 1-5, 10, 11 approved; 6-9 blocked); spec-reviewer, 2026-10-26 (1-5, 11: draft awaiting the owner's answers to OQ-2 to OQ-9) -->

| Section                              | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           |
| ------------------------------------ | ------ | ------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-5, 10, 11                          | 🟢     | spec-reviewer | Item `0389`, round 1: Approved. OQ-1 to OQ-9 are resolved. Attribution holds: only "Fold into 0.4.0" (decision 0001), "Ignore below 0.4.0" (decision 0002) and "Exit 3" (decision 0003) are the owner's, and every basis, wording and consequence beside them is labelled spec-author's reading in A-4, B-002, B-027 to B-029, B-031, § 10, both decision records and item `0389`. B-002's narrowing to "no `claim.files`" adds nothing new. It removes a contradiction with B-029, which the owner accepted on 2026-11-06 and which already made `markdown` beside `claim.files` exit `3`. B-031 and the B-027 and B-029 amendments keep their ids, and each § 3 id has one § 9 row. The type faults left unclaimed in § 10 exit `3` either way (`0001-F2` B-007).                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            |
| 6-9 (item `0389`)                    | 🟢     | spec-reviewer | Round 2: Approved. Round 1 (commit `f6872891`) blocked on two test-writer findings, with one minor finding for the implementer, all cleared. (1) `73bb1790`: the first B-028 scenario now compares the run against `OutputBeforeThisFeature`, a reference written into `NativeClaimsSteps.cs` and not produced by the tool under test: exit `0`, an empty stderr and the four summary lines of the tree under `0.3.1`. A change this Feature made under `0.3.1` now turns the scenario red. The reference is said to match what `specht.tool` `0.5.35-preview` prints for the tree; this review did not rerun that package. (2) `73bb1790`: `UndefinedClaimSources` now has a `null` row, so B-031 pins that a declared `null` is refused, not filled. (3) `a467be80`: the § 7 fault table now cites no claim for the type faults, `0001-F2` B-007 instead, and cites B-029 for an absent `glob` or `feature` only, which agrees with § 10. Still open, accepted: the fill of `plannedTags`, `withdrawnTags`, `supersededTags` and `lifecycleKey` from the shipped `0.4.0` defaults has no unit test. No rule reads those members before item `0390`, and the lock digest and the live-copy test pin the default bytes, so § 8 deferring the fill to `0390` is enough, provided the `0390` § 9 rows name it. Checked: every commit since round 1, the merge `46afa0bf` included, opens with `Delivers 0389`. 31 of 31 claim-member unit rows and the 15 bound scenarios pass, `test/tests` passes 3434 of 3434, and the self-check reports 0 errors and 0 warnings.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            |
| 6-9 (item `0390`)                    | 🟢     | spec-reviewer | Round 2: Approved. Commit `96e431b6` clears both round-1 findings and touches § 8 alone. (1) Finding 2 now reads "Resolved as OQ-10, for option (a)", agreeing with § 10 and § 11, and the conditional B-001 row is gone. (2) The compile-only declarations and each "Red" are stated as the state at `1782f082` and `4949c8b8`. § 8 now says implementer built them at `0bd82fb3`, as § 7 records. Nothing else changed since round 1, so its checks stand. Ran: `./build.sh Specht`, 0 errors and 0 warnings. Round 1 (commit `2dce46af`, over `8cf6d533` to `0bd82fb3`): Blocked, on § 8 text alone. (1) test-writer: § 8, item `0390`, finding 2 still reads "Open as OQ-10" and promises a B-001 row "if the owner rules for option (b) or (c)", but § 10 and § 11 record OQ-10 resolved on 2026-11-13 for (a). State it resolved and drop the conditional. (2) test-writer: § 8 says in the present tense that the declarations it lists carry no behaviour, that `ClaimSets` is "always empty", and that `SpechtListing.ClaimFiles` and the second `SpechtClaims` constructor "are not declared". After `0bd82fb3` each statement is false. Mark them as the red state at `1782f082`, or restate them as built. No other finding. Claims: B-001, B-004 to B-007, B-011 to B-013, B-025 and B-030 are each satisfied as § 7 states. A unit test pins each mechanism, and each would fail without the code: with `ClaimSets` empty, with an empty `Scenarios`, or, for B-005's `ids.scenario` rows, without `0001-F5` B-095's lookahead. The B-002 criterion holds: without a `gherkin` source the listing gains no path and `ClaimSets` stays empty, and no rule reads it. C-1: the tag form, the tree's grammar and the lookahead match `FeatureTagParser`'s reading under one case option. C-2: texts are read once through `SpechtTreeStore` and paths are root-relative. C-4: every tag list comes from `ClaimInput`. C-5: a native claim is a `SpechtClaim` with `Path` and `Declaration`. A-3: the analyzer compiles the changed `GherkinClaimDeclaration` and reader through its source links and reads neither new member. The item `0389` condition is met: the B-012 and B-013 rows name the parser's fill test and the default rows of `ClaimLifecycleTags` and `PlannedClaimCases`. Attribution holds: only "(a) Keep B-001 (Recommended)" and "(b) No, record is lifecycle" are the owner's, each relayed and labelled, and every reading beside them is spec-author's. Every commit opens with `Delivers 0390`. No commit carries a co-author trailer or a vendor name, and the diff adds no inline comment. Each test class has one `Tier` trait and synthetic fixtures. Ran: the three unit classes, 60 of 60 on `net10.0` and `net11.0`; `NativeGherkinClaimsFeature`, 35 passed and 27 pending for later items; `./build.sh Specht`, 0 errors and 0 warnings; and the full `./build.sh`, every target green, with unit 2786 and 35, integration 777, 140, 131, 5, 4 and 1, and acceptance 1644 passed, 163 pending and 0 failed.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       |
| 6-9 (item `0391`)                    | 🟢     | spec-reviewer | Round 3: Approved, reviewing `b032a372` and `18e819b6` after the rebase onto `c473370b` (#886, #887, #888). Round 2's finding is cleared: the `ResolveClaims` guard cites B-003, the mechanism § 8 and § 9 name, and no longer cites B-023 or B-024. `StatusRule` (`SPEC4010`, #887) reads a record's matrix through `MatrixResolver.Resolve`, which the claim guard does not cover, and now skips a native Feature. The skip table's `0001-F12` row lists it, lifted by `0396`. Ten rules carry a `Where` skip, as § 6 and § 7 now count. `NativeMatrixRuleSilences` gains a `SPEC4010` row, and every row's set names a record whose § 9 matrix holds a `Covered` row. The `Resolve`-walking rule is therefore reached, and § 8 records the row red without the skip. The `FactsCommand` merge keeps main's `--path` selection and passes only the layouts' Features to `ReadRecords`. No other rule on `c473370b` that loops Features reads a native Feature's claims or matrix unguarded, and `SpechtModelReader.Read` is still the only producer of `SpechtListing`. Every commit since round 2 opens with `Delivers 0391`. Read only; the implementer reports `./build.sh` green, 0 failed, and the self-check at 0 and 0. Round 2 (commit `1b30e634`): Blocked, on the guard citing B-023 and B-024, which it defers. Round 1 (commit `d7ec5a07`): Blocked, on `SPEC4008` unskipped, with two minor findings (the `Under` citation, the facts record read), all cleared in round 2. Round 1's check of B-003, B-017, B-026, B-032 and B-036 to B-038, § 9 parity, attribution and the invariants stands. B-033 to B-035 stay `Missing` for `0395`, which must also pin `TryLoad`'s `null` cases.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        |
| 3, 6-11 (item `0393`; decision 0004) | 🟢     | spec-reviewer | Round 2: Approved, reviewing `a24175c5` and `8af0c826` after the rebase onto `0087a757` (#903, #904). `git range-diff` shows the nine earlier commits unchanged by that rebase. All five round-1 findings are cleared. (1) § 11 OQ-5 carries a 2026-11-14 note naming decision 0004, which says the ids are decided, and decision 0004's Affects lists OQ-5. (2) Both § 10 entries now speak as of their writing and say that the owning role has since edited the section. (3) § 8 records 29 expected and 28 found at `f81d195c`, and names `SPEC2003` from `a57f4bae`. (4) The `0.3.1` count of 23 is pinned to `a57f4bae`, which `main` holds. (5) The red states are named at `f81d195c`, and passing from `719148ae`. Those two hashes stand. Both were pushed with round 1's head, `e33fa8e7`, and the rebase onto `0087a757` changed neither commit's content, so they are now `f5373108` and `85493bbc`. `0442` adds no rule id, so the counts hold at both. The rule here is the one item `0391`, round 3, applied to `cc1c2146`: a pushed pre-rebase hash stands as history, while round 1's `59d7fca9` was never pushed. The nine § 3 claims amended to ids, B-009, B-010, B-014 to B-016, B-018, B-019, B-033 and B-034, change only the working name to decision 0004's id. Each adds "Amended 2026-11-14 (decision 0004): was `SPEC02n`", cites decision 0004 under Source and reads `Amended`, and their scenarios name the ids, with no `SPEC025` to `SPEC027` left in `native-claims.feature`. B-010's repeated "OQ-5" in its Source cell predates this branch. Only B-010's § 9 row moves, to `Covered`; the other eight stay `Missing` for `0394` and `0395`, which blocks those items, not this approval. Both new commits open with "Delivers 0393.". Read only; the coordinating session reports `./build.sh` green, with unit 3164 and acceptance 1781 passed, 113 skipped and 0 failed, and the self-check at 27 rules, 0 errors and the 2 `SPEC2003` warnings from main's `0236` spike files. Round 1 (commit `dede2609`): Blocked, on record text alone, reviewing `origin/main` (`a57f4bae`)...`e33fa8e7`, eight commits; read only, no build. (1) spec-author: § 11 OQ-5 still says "The levels and the undecided ids stand; each rule's item assigns its id when the rule enters `0.4.0`", which decision 0004 and the nine § 3 amendments contradict. Add a dated note naming decision 0004, as the 2026-11-12 note names decision 0001, and list OQ-5 under decision 0004's Affects. (2) spec-author: two § 10 entries state in the present tense what this branch later changed. Here, "§ 7 still names the working names" is false from `736c3aff`. In `0001-F7` § 10 (`src/tool/.spec/README.md`), "§ 8's "Open, item `0388`" note still reads OQ-75 as unanswered" is false from `f81d195c`. State each as of its commit, or say that the owning role has since edited it. (3) test-writer: § 8 records the red counts as "28 ids expected, 27 found", for the `0.4.0` vocabulary row and the composition list. Since the rebase onto `a57f4bae`, which brought `SPEC2003`, both expect 29 and find 28 at `f81d195c`. (4) test-writer: § 8 pins the `0.3.1` count of 23 to `59d7fca9`, a pre-rebase commit that neither the pushed branch nor `main` holds; cite `a57f4bae`. (5) test-writer: § 8 says "Red: every row ... since no rule reports `SPEC4011`" and "Red, at this commit" without naming the commit. State the red at `f81d195c` and passing from `719148ae`, as item `0390`'s subsection does. Holds: B-010 is satisfied as § 7 states. `ScenarioWithoutClaimRule` maps each `SPCT0012` finding to a warning at the claim file's path and keyword line, with no identifier, once per path, and the theory's rows would fail without the rule, without the per-path filter (the two-set row) or at a tag's line. The criteria hold. `docs/rules/v0.4.0/SPEC4011.md` meets `0001-F3` B-031. `SPEC025` maps to `SPEC4011` in the `0.4.0` row alone (`0001-F7` B-014), no file under `v0.1.0` to `v0.3.1` changes and only the `0.4.0` lock entry moves (C-1). Two guards hold the silence without `gherkin` and below `0.4.0` (B-002, B-028, C-3). The merge with `0441` holds: `OrphanCompanionRule` and `ScenarioWithoutClaimRule` are both registered in key order, and `SPEC024` and `SPEC025` are both mapped in `0.4.0`. The vocabulary row, the name map and the composition list carry both, the `0.4.0` manifest sets both to warning, and the relock is the only lock change. The implementer's three readings follow from B-005, B-009 and B-013 through the shared reader (A-3), and none reports the tag that § 5 row 7 excludes. The two-set row is in B-010's scope, so no open question is needed: B-010 names no set, the file is a claim file of both, one scenario is one violation, and B-018 says only where the file's claims are recorded. Item `0395` must keep the report, as § 7 says. The warning-only "the output has a line" binding leaves item `0395`'s error-line scenarios pending, not passing, and `0395` widens it. Attribution holds: only "(c) Moot under 0.4.0" and "Fixed 4011, 4012, 4013 (Recommended)" are the owner's, each labelled, with the wording the coordinating session's and every consequence spec-author's. Every commit opens with "Delivers 0393.". No commit carries a co-author trailer or a vendor name, and the diff adds no inline comment and no absolute path. The test class has one `Tier` trait, `MockFileSystem` and synthetic fixtures. The implementer reports `./build.sh` green, acceptance 1861 with 0 failed and 95 skipped, and the self-check at 0 errors, its 2 `SPEC2003` warnings on main's `0236` spike files. Item `0393` is still `in-progress`, and `done` and `closed` go in before the pull request opens. |
| 3, 6-10 (item `0394`)                | 🟢     | spec-reviewer | Round 2: Approved, reviewing `ba974347` and `b3e29a4d` on `ef0f90de`, `main` unmoved. Both commits touch this file alone, and the code is the `013adb9f` the build results below cover. All three round-1 findings are cleared. (1) § 8 states `ClaimId` unset at `a311b77d` and set on each `SPCT0011` finding from `013adb9f`, as built. (2) The red state at `a311b77d` now names `013adb9f` as the commit from which every B-009 test and the `@B-009` scenario pass. (3) The uncited "raised to the owning role" is gone. § 7 now says that the reader and the record are the engine's own files in `src/specht/Model/`, where `0201-F7` places them, and that the analyzer links them as source, which agrees with the Gherkin binding specification's § 6. Nothing else changed, so round 1's checks stand. Both commits open with "Delivers 0394.". Every § 12 row is now 🟢. B-014 to B-016, B-018, B-019 and B-033 to B-035 stay `Missing` for `0395`, which blocks that item, not this approval. Round 1 (commit `ce16cc25`): Blocked, on record text alone, reviewing `origin/main` (`ef0f90de`)...`013adb9f`, six commits; read only, no build. (1) test-writer: § 8 says "The reader does not set it yet." of `GherkinClaimTextFinding.ClaimId`, which is false from `013adb9f`, where the reader sets it for `SPCT0011`. State it as the state at `a311b77d`, or restate it as built. (2) test-writer: § 8's "Red at `a311b77d`" names no commit from which the tests pass. Add one, as item `0393`'s subsection names `719148ae`: `013adb9f` on the rebased branch, or `3efd9492`, its pushed pre-rebase twin. `a311b77d` itself stands. It was pushed, as the parent of `0e1c26d6`, and `git range-diff` shows it content-identical to `ac0f850b`; that is the rule item `0393`'s round 2 applied. (3) implementer: § 7's "This edit to a shared file was raised to the owning role before it was built." names no role and no record. Cite where it was raised, or drop the sentence. The reader is the engine's file: `0201-F7`'s Gherkin binding specification places it in `src/specht/Model/`, and the analyzer only links it. Holds: B-009 as amended by `57c87ca4` keeps its id, and the identifier is labelled spec-author's reading of the scenario, not an owner answer. The approved scenario already named `B-005` on the reported line, the step it used could not match a rule line, and B-008 states the claim id as `SPEC4003`'s identifier, so the amendment adds no behaviour the agreement did not carry. The § 10 entry records it. B-009 is satisfied as § 7 states. `ExamplesClaimTagRule` maps each `SPCT0011` finding to an error at the claim file's path and the tag's line, naming the claim id, once per path. The theory's rows would fail without the rule, without the per-path filter, or at the `Examples:` line, and the reader test fails without `ClaimId`. "Records no claim" holds by construction and is guarded. The analyzer is unaffected. `GherkinClaimFileReader` copies `DiagnosticId`, the path, `Line` and `Text` into `BindingFinding` by name, so the trailing optional `ClaimId` changes no analyzer diagnostic, line, text or export (§ 5 row 1), and the one reader serves both (§ 5 row 2, A-3). C-5: the rule reads `SpechtClaims.Files` and parses nothing again. `0001-F7`: `SPEC026` maps to `SPEC4012` in the `0.4.0` row alone (B-014), B-066 holds with `0.4.0` a minor above `0.3.1`, and only the `0.4.0` lock entry moves, with no file under `v0.1.0` to `v0.3.1` changed (C-1, C-17). Two guards hold the silence without `gherkin` and below `0.4.0` (B-002, B-028, C-3). The level is the interface's default `Error`, and the shipped `0.4.0` manifest sets `SPEC4012` to `error`. The rule page and the docs land with the vocabulary row (`0001-F3` B-031). The vocabulary counts in § 8, 30 expected and 29 found, are right on this base. Every commit opens with "Delivers 0394.". No commit carries a co-author trailer or a vendor name, and the diff adds no inline comment and no absolute path. The test class has one `Tier` trait, `MockFileSystem` and synthetic fixtures. The coordinating session reports the results on `013adb9f`, after the rebase onto `ef0f90de`. `./build.sh` exits `0`. The unit runs of 735, 66, 395, 1, 35, 3186 and 5 and the integration runs of 131, 140, 4 with 1 skipped, 1 and 842 have 0 failed, and acceptance passes 1782 of 1894 with 112 skipped and 0 failed. `./build.sh Specht` exits `0` with 57 features, 28 rules, 0 errors and 2 warnings. Item `0394` is still `in-progress`, and `done` and `closed` go in before the pull request opens.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     |
| 3, 6-11 (item `0395`)                | 🔴     | spec-reviewer | Round 1: Blocked on one implementer finding. Reviewed: the seven commits `3b4a1f4e` to `9020fd97` on `ee80cc7a`. (1) Implementer, § 6 and § 7, `src/specht/Discovery/SpechtListing.cs:1` and `:49`: `SpechtListing.ClaimFaults` holds `Specht.Model`'s `SpechtClaimFault`. At `ee80cc7a` `Specht.Discovery` referenced no engine namespace, and `Specht.Model` already references it, so this adds a cycle. ADR-0010's layout lets a namespace reference only those below it, and `dotnet-tool` § Vertical Slice says a new upward `using` is fixed by moving the type, not by adding the `using`. § 6's row for `SpechtClaimFault` and § 7's "The fault" design this. Fix: carry the model reader's record faults to `SpechtModelBuilder.Build` without `SpechtListing`, or move the record and its kind where `Specht.Discovery` may reference them, and state the place in § 6 and § 7. Judged, not findings. (a) The rooted `/outside/README.md` in the unit fixtures is B-034's subject, on the owner's selection for OQ-17. It is synthetic, lives only in the in-memory file system, names no path on the machine, and § 8 says so. Every expected path is root-relative, and `ARootedRecord_WhenEvaluated_ShouldPutNoRootedPathInAnyViolation` holds C-2 for the violation. `SpechtManifestParserSchemaFileNamesUnitTests` on `main` already uses `/feature.json` and `C:\\feature.json` as refused input in the same way. The `@B-034` step builds its absolute path from the temporary root at run time, stores it nowhere, and asserts that neither the path nor the root appears in stdout or stderr. (b) The analyzer: `GherkinClaimFileReader` copies only `RelativePath` and `Message` from the exception, and only the values from the read. Its own feature-tag check compares ordinally, as the builder's does (A-3), so its findings are unchanged (§ 5 rows 1 and 2). Checked and holding. B-014 and B-015: the refused header's line, `0` for every other cause B-015 names, and no claim from the file. B-016: the glob as written, at line `0`. B-018: one fault after the join; the file's claims are under neither set, and its read stays in `Files` for `SPEC4011` and `SPEC4012`. B-019: a scope form only with exactly one `{feature}`, and nothing reported when the form is malformed (OQ-20). B-033 to B-035: `TryLoad` makes its rooted check through `IsRooted`, so the two agree. B-002, B-028 and C-3: the Markdown guard, and the `0.3.1` guard at 23 rules evaluated. `0001-F7` B-014, B-066, C-1 and C-17: only `0.4.0` gains `SPEC027` to `SPEC4013`, its manifest line and its rule page, and only its lock entry moves. § 9 changes only this item's nine rows, each `Covered`, with a unit test over the mechanism. B-015 is amended on the owner's OQ-19 label with its id kept, and the labels are attributed as the coordinating session's wording, in § 3, § 10, § 11 and the item. Every commit opens with "Delivers 0395.". The full build was reported by the implementer, not observed here: unit 4472, integration 1123, acceptance 1796 passed and 98 skipped, 0 failed.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  |

## Tasks

Cut 2026-11-06 into [`../.issue/`](../.issue/), after the owner answered § 11 and approved the claims for cutting: `0387` (the Feature), with:

- `0389`, the `claim` members and their validation under `0.4.0` (B-002, B-027 to B-029, B-031).
- `0390`, the native claim reader and the claim file sets it reads (B-001, B-004 to B-007, B-011 to B-013, B-025, B-030).
- `0391`, a set's record, its identity and the report (B-003, B-017, B-026, B-032, B-036 to B-038).
- `0392`, the existing claim and section rules on a native Feature (B-008, B-020, B-021).
- `0393`, `SPEC4011`, a scenario with no claim (B-010).
- `0394`, `SPEC4012`, a claim tag on an Examples block (B-009).
- `0395`, `SPEC4013`, an unreadable claim file, set or record (B-014 to B-016, B-018, B-019, B-033 to B-035).
- `0396`, native claims under the rules and the command still to be built: `SPEC024`, the matrix rules and the trace (B-022 to B-024). It waits on `0441`, which builds `SPEC024` (`0001-F6` B-013), and `0442`, which builds the trace lines (`0001-F13` B-001). Amended 2026-11-14, on the repository owner's instruction "Cut items for SPEC024 and trace": was blocked, with no item building either, and waiting on `0336` and `0337` (`0001-F12`), now done.

Each rule item adds its rule page, its four-digit id in `0.4.0`'s vocabulary and a relock, which `0001-F7` C-17 allows while no published package carries `0.4.0`. Every item waits, through `0389`, on `0436` (`0001-F5`), which cuts schema `0.4.0`, a `0.minor` step, and on `0388` (`0001-F7`), which cut `0.3.1`. Amended 2026-11-12 ([decision 0001](decisions/0001-native-claims-fold-into-schema-0-4-0.md)): was `0.3.1`'s vocabulary, and every item waiting on `0388` alone, which cuts schema `0.3.1` under `0001-F7` decision 0022.

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
