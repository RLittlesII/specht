---
title: "Specification: Traceability reconciliation"
description: "Expose missing links and test outcomes within an explicit project scope."
type: feature
id: "F5"
epic: "0201"
spec_status: approved
status: in-progress
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
updated: "2026-11-13"
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

<!-- last written by: spec-author, 2026-11-13 (B-045 to B-047 added, `0201-F1` decision 0006, item 0436); spec-author, 2026-11-09 (B-042 to B-044 added, item 0429) -->

| ID    | Claim                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    | Source                                                                                       | Status    |
| ----- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------- | --------- |
| B-001 | Given a claim bound to a discovered test through the common runner-evidence contract and a matching result with no scenario policy, this Feature presents the claim-to-method-to-case-to-result chain without requiring Reqnroll.                                                                                                                                                                                                                                                                        | Epic 0201; F3 decision 0001                                                                  | Amended   |
| B-002 | Given a declared repository scope omitting a required test project or input, this Feature ensures the report marks coverage incomplete and identifies the missing input.                                                                                                                                                                                                                                                                                                                                 | Epic 0201; decision 0001 in 0201-F1                                                          | Active    |
| B-003 | Given a resolved binding with a failed, skipped or absent test result, this Feature ensures the report keeps that outcome distinct from a passing result.                                                                                                                                                                                                                                                                                                                                                | Epic 0201; decision 0001 in 0201-F1                                                          | Active    |
| B-004 | Given an explicit policy requiring a link absent from a claim's evidence, this Feature ensures the report identifies that missing link instead of treating reference resolution as complete coverage.                                                                                                                                                                                                                                                                                                    | Epic 0201; decision 0001 in 0201-F1                                                          | Active    |
| B-005 | Given scenario bridge evidence supplied for part of a declared scope, this Feature ensures those scenario links augment the direct chains without inventing links for other tests.                                                                                                                                                                                                                                                                                                                       | Epic 0201; decision 0001 in 0201-F1                                                          | Active    |
| B-006 | Given a declared specification claim requiring test evidence with no test binding at all, this Feature ensures reconciliation reports the missing evidence starting from that claim even when no method names it.                                                                                                                                                                                                                                                                                        | Epic 0201; decision 0001 in 0201-F1                                                          | Active    |
| B-007 | Given a binding to a resolving target classified as ineligible under 0201-F1's lifecycle policy, this Feature excludes that binding from current required evidence while retaining the historical link's target identity and lifecycle state.                                                                                                                                                                                                                                                            | 0201-F1 decision 0004                                                                        | Active    |
| B-008 | Given a non-exempt current claim in the selected Feature scope, this Feature requires at least one valid test binding.                                                                                                                                                                                                                                                                                                                                                                                   | 0201-F6 decision 0003                                                                        | Active    |
| B-009 | Given an implementation or scenario link kind, this Feature requires that kind only when the consumer explicitly enables its requirement.                                                                                                                                                                                                                                                                                                                                                                | 0201-F6 decision 0003                                                                        | Active    |
| B-010 | Given an explicitly reasoned coverage exemption, this Feature presents it as exempt rather than covered.                                                                                                                                                                                                                                                                                                                                                                                                 | 0201-F6 decision 0003                                                                        | Active    |
| B-011 | Given a declared set of project and target-framework combinations, this Feature reports complete analysis only when current required inputs are available for every declared test project's combination.                                                                                                                                                                                                                                                                                                 | 0201-F6 decision 0004, narrowed (§ 10, 2026-11-08); 0201-F5 OQ-5, resolved                   | Amended   |
| B-012 | Given stale or mismatched discovery or results, this Feature retains their stale or mismatched state and excludes them from current discovery or execution evidence.                                                                                                                                                                                                                                                                                                                                     | 0201-F6 decision 0004                                                                        | Active    |
| B-013 | Given absent, stale or mismatched test results, this Feature evaluates binding validity separately from those execution-evidence states.                                                                                                                                                                                                                                                                                                                                                                 | 0201-F6 decision 0004                                                                        | Active    |
| B-014 | Given supplied traceability evidence, this Feature presents reconciliation through `specht evidence` with a human summary or separate versioned JSON output.                                                                                                                                                                                                                                                                                                                                             | Decision 0001 reversal                                                                       | Amended   |
| B-015 | Given the new trace report contract, this Feature preserves the existing checker report contract unchanged.                                                                                                                                                                                                                                                                                                                                                                                              | Decision 0001                                                                                | Active    |
| B-016 | Given supplied discovery and result artifacts, this Feature reconciles them without launching test execution.                                                                                                                                                                                                                                                                                                                                                                                            | Decision 0001                                                                                | Active    |
| B-017 | Given complete required links, this Feature succeeds only when every case key owned by a declared project passed in that project's own run; an owned key that is missing or did not pass fails the default gate.                                                                                                                                                                                                                                                                                         | Decision 0002; decision 0003; consumer request #256                                          | Amended   |
| B-018 | Given a required claim whose outline has planned rows, this Feature reports those rows' cases as planned, neither covering the claim nor failing the default gate.                                                                                                                                                                                                                                                                                                                                       | Consumer request #256; 0201-F7 B-026                                                         | Active    |
| B-019 | Given an outline with planned rows and an active row whose case is missing or skipped, this Feature fails the default gate for that active row.                                                                                                                                                                                                                                                                                                                                                          | Consumer request #256; 0201-F7 B-026                                                         | Active    |
| B-020 | Given a discovered case key its project does not own, this Feature never counts it as passed and never fails the default gate for it.                                                                                                                                                                                                                                                                                                                                                                    | Consumer request #256; decision 0003                                                         | Amended   |
| B-021 | Given two projects whose required-case scopes partition the same claim files, this Feature succeeds when each owned key passes in its own project's run, without merging keys across projects.                                                                                                                                                                                                                                                                                                           | Consumer request #256; decision 0003                                                         | Amended   |
| B-022 | Given a declared test project, one with a required-case scope or named by a `verifies` enforcement entry, with no current evidence, this Feature fails the default gate with incomplete input (exit 2) naming that project, because its owned keys cannot be known without its artifact.                                                                                                                                                                                                                 | Consumer request #256; decision 0003; 0201-F5 OQ-5, resolved                                 | Amended   |
| B-023 | Given evidence paths named as positional arguments, this Feature supplies each file or directory as evidence, repeatable.                                                                                                                                                                                                                                                                                                                                                                                | Decision 0006                                                                                | Amended   |
| B-024 | Given evidence paths named with `--evidence`, this Feature supplies them with the same semantics as positional paths.                                                                                                                                                                                                                                                                                                                                                                                    | Withdrawn 2026-10-25: `--evidence` is removed, not kept as an alias (decision 0006 reversal) | Withdrawn |
| B-025 | Given several evidence paths named positionally, this Feature supplies them in command-line order, before de-duplication, with no error.                                                                                                                                                                                                                                                                                                                                                                 | Decision 0006                                                                                | Amended   |
| B-026 | Given no evidence path, this Feature fails with incomplete input (exit 2) and the no-evidence-supplied finding, as before.                                                                                                                                                                                                                                                                                                                                                                               | Decision 0006                                                                                | Amended   |
| B-027 | Given a request for command help, this Feature shows the optional positional paths in the usage line as the only way to name evidence.                                                                                                                                                                                                                                                                                                                                                                   | Decision 0006                                                                                | Amended   |
| B-028 | Given native Gherkin claim input and no mapped claims section, this Feature reconciles each selected Feature's claims, read through the engine's Gherkin claim reader from the claim files that Feature's claim file set names, as it reconciles Markdown claims.                                                                                                                                                                                                                                        | Decision 0007; 0201-F7 decision 0004                                                         | Active    |
| B-029 | Given a native claim the claim files give the state Withdrawn or Superseded, this Feature does not select it, as it does not select a withdrawn or superseded Markdown claim.                                                                                                                                                                                                                                                                                                                            | Decision 0007; 0201-F7 B-021                                                                 | Active    |
| B-030 | Given a claim file set naming a Feature record whose lifecycle is not current, this Feature selects none of that Feature's native claims, as it selects none from a Markdown record that is not current.                                                                                                                                                                                                                                                                                                 | Decision 0007; 0201-F7 B-022                                                                 | Active    |
| B-031 | Given a native claim, this Feature reports its source location as the claim file's root-relative path and the line of the element that declares the claim.                                                                                                                                                                                                                                                                                                                                               | Decision 0007; 0201-F7 B-008                                                                 | Active    |
| B-032 | Given a native claim whose cases a claim file marks planned, this Feature marks no discovered case planned from those marks.                                                                                                                                                                                                                                                                                                                                                                             | Decision 0008; 0201-F7 B-016                                                                 | Active    |
| B-033 | Given a native claim whose every case a claim file marks planned, this Feature still requires the claim's test evidence.                                                                                                                                                                                                                                                                                                                                                                                 | Decision 0008; 0201-F7 B-024                                                                 | Active    |
| B-034 | Given a selected Feature's claim file that is unreadable, is not valid Gherkin or declares a language other than English, this Feature fails with invalid input (exit 3) naming that claim file.                                                                                                                                                                                                                                                                                                         | Decision 0007; 0201-F7 B-014, B-029                                                          | Active    |
| B-035 | Given a selected Feature with no claim file set, or whose claim file set matches no file, this Feature fails with invalid input (exit 3) naming the manifest.                                                                                                                                                                                                                                                                                                                                            | Decision 0007; 0201-F7 § 7                                                                   | Active    |
| B-036 | Given a declared project with no required-case scope, named by no `verifies` enforcement entry and with no current export, this Feature requires no export from it and reports no missing compilation for it.                                                                                                                                                                                                                                                                                            | 0201-F5 OQ-5, resolved; consumer request #256                                                | Active    |
| B-037 | Given an enabled implementation link requirement and no current export carrying a claim's `implements` bindings, this Feature reports that claim's implementation link missing.                                                                                                                                                                                                                                                                                                                          | 0201-F5 OQ-5, resolved                                                                       | Active    |
| B-038 | Given a claim file set whose glob is spelled with a leading `./`, an empty segment, a backslash, `..` inside the root or a trailing `**`, this Feature reads from it the same claim files the analyzer's native claim catalogue reads, among the `.feature` files the build delivers as scenario inputs outside the manifest's exclusions.                                                                                                                                                               | Decision 0009, narrowed (§ 10, 2026-11-08); 0201-F7 decision 0004                            | Amended   |
| B-039 | Given a claim file that more than one claim file set matches, this Feature fails with invalid input (exit 3) naming that claim file.                                                                                                                                                                                                                                                                                                                                                                     | Decision 0007; 0201-F7 § 7; owner, 2026-11-09 (§ 10)                                         | Active    |
| B-040 | Given a selected Feature whose claim file set matches a claim file and names a Feature record that cannot be read, this Feature fails with invalid input (exit 3) naming that Feature record.                                                                                                                                                                                                                                                                                                            | Decision 0007; § 7; owner, 2026-11-09 (§ 10)                                                 | Active    |
| B-041 | Given a claim file of a current selected Feature whose Feature or epic tag names an identity other than its claim file set's, this Feature fails with invalid input (exit 3) naming that claim file.                                                                                                                                                                                                                                                                                                     | Decision 0007; 0201-F7 B-023, § 7; owner, 2026-11-09 (§ 10)                                  | Active    |
| B-042 | Given a manifest that sets `records.claimSource` with a malformed native claim key (a `claimSource` other than the string `gherkin`, no `/claim` mapping, a claim tag missing, not a string or without exactly one `{claim}` placeholder, a `tagIgnoreCase` that is not a Boolean, a scope tag without its own placeholder, or a planned, withdrawn or superseded tag list that is not an array of `@`-led tags without whitespace), this Feature fails with invalid input (exit 3) naming the manifest. | Decision 0007; § 7; owner, 2026-11-09 (§ 10)                                                 | Active    |
| B-043 | Given a manifest that sets `records.claimSource` with a malformed claim file set entry (a `glob` or `feature` missing, not a string or blank, a `record` that is not a string, a Feature or epic outside its grammar, an epic where the manifest declares no epic grammar or none where it declares one, or two sets for one Feature), this Feature fails with invalid input (exit 3) naming the manifest.                                                                                               | Decision 0007; § 7; owner, 2026-11-09 (§ 10)                                                 | Active    |
| B-044 | Given a manifest that does not set `records.claimSource` and maps no claims section, this Feature fails with invalid input (exit 3) naming the manifest.                                                                                                                                                                                                                                                                                                                                                 | § 7; owner, 2026-11-09 (§ 10)                                                                | Active    |
| B-045 | Given a manifest whose `codeAnalysis` mappings project the `ids` claim record, this Feature reads and validates claim IDs by the grammar `0201-F1` B-023 gives the analyzer.                                                                                                                                                                                                                                                                                                                             | `0201-F1` decision 0006; `0001-F5` A-8                                                       | Active    |
| B-046 | Given a manifest whose `codeAnalysis` mappings project `identifiers.claim` and no `ids` claim record, this Feature reads and validates claim IDs by `identifiers.claim`, with the report it gave before `0201-F1` decision 0006.                                                                                                                                                                                                                                                                         | `0201-F1` decision 0006                                                                      | Active    |
| B-047 | Given a manifest whose `codeAnalysis` mappings project an `ids` claim record `0201-F1` B-025 rejects, this Feature fails with invalid input (exit 3) naming the manifest.                                                                                                                                                                                                                                                                                                                                | `0201-F1` decision 0006                                                                      | Active    |

## 4. Constraints

<!-- last written by: spec-author, 2026-11-08 (OQ-5 resolved in C-3, item 0302) -->

| ID  | Constraint                                                                                                                                                                                                                                                                                         | Rules Out                                                           |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------- |
| C-1 | Resolved reference, required-link completeness and passing execution are separate evidence states.                                                                                                                                                                                                 | Reporting resolved IDs as passing behavior.                         |
| C-2 | Reports are deterministic for the same supplied inputs, offline and read-only with repository-relative paths.                                                                                                                                                                                      | Nonrepeatable or machine-specific reports.                          |
| C-3 | One compilation cannot assert completeness for an unselected or omitted declared test project, one with a required-case scope or named by a `verifies` enforcement entry (OQ-5, resolved). Amended 2026-11-08, was: One compilation cannot assert completeness for unselected or omitted projects. | Complete-coverage claims over a missing declared test project.      |
| C-4 | The gate evaluates a project's cases only in that project's own artifact and selected run, and only those it owns that are not planned.                                                                                                                                                            | Gating an unowned key; one project's pass satisfying another's key. |
| C-5 | A planned case is reported as planned, is never a covering case and is never gated.                                                                                                                                                                                                                | Planned rows covering a claim or failing the gate.                  |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-11-13 (row 14, `0201-F1` decision 0006, item 0436); spec-author, 2026-11-08 (rows 12 and 13 cite § 7, item 0409 review round 3); spec-author, 2026-11-08 (rows 12 and 13, item 0409 review round 2); spec-author, 2026-11-08 (rows 10 and 11, decision 0009, item 0409); spec-author, 2026-11-07 (rows 5 and 6, item 0409); spec-author, 2026-10-25 -->

| #   | Item                                                                                                                                                                                                                     | Exclusion Reason                                                                                                                                                                                                                                                                                                               |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| 1   | Automatically declaring assertion adequacy or architectural compliance.                                                                                                                                                  | This agreement does not authorize it.                                                                                                                                                                                                                                                                                          |
| 2   | Launching test runs or querying remote test services during checking.                                                                                                                                                    | This agreement does not authorize it.                                                                                                                                                                                                                                                                                          |
| 3   | Changing the approved text-level `specht trace` command.                                                                                                                                                                 | `0001-F13` owns its output and exit contract.                                                                                                                                                                                                                                                                                  |
| 4   | Re-ingesting native SPCT diagnostics into the evidence report.                                                                                                                                                           | F3's version-one export carries binding verdicts, not diagnostic records; build diagnostics keep their own stream.                                                                                                                                                                                                             |
| 5   | A second Gherkin reader in the tool. Amended 2026-11-07, was: parsing a consumer's `.feature` files.                                                                                                                     | The tool reads claim files through the engine's Gherkin claim reader (0201-F7 § 5 row 11, decision 0004; B-028); a second reader gives two readings of one claim file. Was: the analyzer exports the native claim catalogue and `specht evidence` reads it there (0201-F7 B-030, decision 0003, both withdrawn or superseded). |
| 6   | Marking a discovered case planned from a claim file's planned marks, and listing a native claim's Gherkin cases in the report. Amended 2026-11-07, was: reading the claim catalogue in the reconciler.                   | A Gherkin case has no join to a discovered case; only scenario records mark a case planned, and the report JSON is unchanged (decision 0008; B-032). Was: not built by item 0262, the gap OQ-3 (resolved by decision 0007).                                                                                                    |
| 7   | A new finding kind for an omitted owning run.                                                                                                                                                                            | B-022 reuses `missingCompilation` (decision 0004).                                                                                                                                                                                                                                                                             |
| 8   | A deprecation alias or a warning for `--evidence`.                                                                                                                                                                       | Decision 0006 reversal removes the option outright; it is an unknown option like any other.                                                                                                                                                                                                                                    |
| 9   | Changing exit codes or the report JSON for positional paths.                                                                                                                                                             | Decision 0006 changes the argument surface only; B-014 and `formatVersion` 1 stand.                                                                                                                                                                                                                                            |
| 10  | Sharing the claim file set normaliser and matcher between the analyzer and `specht evidence`, by source link or reference.                                                                                               | Outside 0201-F7 decision 0004's scope; each side keeps its own copy, held to one reading by the same list of cases in each test project (decision 0009; B-038).                                                                                                                                                                |
| 11  | Applying the manifest's exclusions to the analyzer's native claim catalogue, or reading a claim file under an excluded directory in `specht evidence`.                                                                   | The gate lists files under the manifest's exclusions, such as `bin/` and `obj/`; the analyzer reads the scenario inputs the build delivers and applies none (decision 0009; B-035, B-038).                                                                                                                                     |
| 12  | Reading, in `specht evidence`, a claim file a set matches that is not a `.feature` file, such as one a consumer's `SpechtScenarioInputs` override delivers.                                                              | This specification's boundary: the gate cannot evaluate a consumer's MSBuild override offline and matches a set only against the root's `.feature` files, so the analyzer reads that file and the gate leaves its claims out without a finding (§ 7; B-038). Whether the gate reports one is OQ-6.                             |
| 13  | Reading, in the analyzer's native claim catalogue, a `.feature` file a set matches that the build does not deliver as a scenario input, such as `src/read/claims/read.feature` under the default `SpechtScenarioInputs`. | This specification's boundary: the analyzer matches a set only against the scenario inputs the build delivers, and the gate against the root's `.feature` files, so only the gate reads that file (§ 7; B-038).                                                                                                                |
| 14  | Reading, in `specht evidence`, a claim of a Feature declaring a prefix by the `ids` claim record's `form`, any other `ids` record, or a member of the claim record filled from the shipped record.                       | `0201-F1` § 5 rows 7 and 8: `specht evidence` reads the claim grammar the analyzer reads, and the analyzer reads only that (`0201-F1` decision 0006).                                                                                                                                                                          |

## 6. Concern Separation

<!-- last written by: implementer, 2026-11-08 (item 0409: `RepositoryPaths` moved into the evidence slice); implementer, 2026-11-08 (item 0409: claim file candidates are `.feature` files, B-038); implementer, 2026-11-08 (item 0409: two claim file set path readings, decision 0009); implementer, 2026-11-08 (item 0409); implementer, 2026-11-07 (item 0409); implementer, 2026-10-25 -->

| Concern                                    | Classification | Owner and boundary                                                                                                                                                                                                                                                                                                                                                                                                                                               |
| ------------------------------------------ | -------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Selected current claims and required links | Business       | 0201-F6 defines selection, link policy and reasoned exemptions; F5 starts reconciliation from every selected eligible claim.                                                                                                                                                                                                                                                                                                                                     |
| Binding and case identity                  | Both           | 0201-F2 resolves targets and lifecycle; 0201-F3 exports method, case and run identities. F5 joins those records without re-resolving source symbols.                                                                                                                                                                                                                                                                                                             |
| Completeness, freshness and outcome gate   | Business       | F5 evaluates declared project/TFM inputs, required links and passing current results as independent states, gating only owned cases that are not planned.                                                                                                                                                                                                                                                                                                        |
| Evidence collection and scenario mapping   | Both           | 0201-F3 collects runner evidence; optional 0201-F4 records add scenario links. F5 consumes their exported contracts.                                                                                                                                                                                                                                                                                                                                             |
| Artifact reading and report serialization  | Technical      | The tool's reconciliation slice reads explicit files and emits its own versioned report; it reads native claim files through the engine's Gherkin claim reader, the file the analyzer links, matches claim file sets against the root's `.feature` files with the slice's own `RepositoryPaths`, a second copy of the analyzer's `AnalysisPaths` held to it by tests (decision 0009), does not load the analyzer assembly and does not alter the checker report. |
| Filesystem and process boundary            | Technical      | F5 reads current repository records and supplied artifacts offline. Only the caller-named report path is written.                                                                                                                                                                                                                                                                                                                                                |

## 7. Technical Design

<!-- last written by: implementer, 2026-11-13 (item 0436: B-045 to B-047, the mapped ids claim record); implementer, 2026-11-09 (item 0429: B-039 to B-041 cited for the shared claim file, the unreadable record and the identity tag) -->

The command implements F5 decision 0001, F6's coverage/input decisions and the
owner's decision to require passing outcomes by default. Current CLI usage is in
`docs/cli.md`.

### Command

```text
specht evidence [--root <dir>] [<path>...] [--run <id>] [--json] [--report <path>]
```

`<path>...` is an optional repeatable positional argument, as in
`specht frontmatter`, and the only way to name evidence (decision 0006).
`Execute` passes the paths in command-line order to
`EvidenceReconciler.Reconcile`, which de-duplicates files. No path keeps the
reconciler's no-evidence finding and exit 2. The declaration is
`EvidenceCommand.Settings.Paths`.

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
`SpechtDiscovery` and `SpechtDocument` read manifest layouts and Markdown records,
and the engine's `GherkinClaimTextReader` reads native claim files.
From item `0166` discovery lists root-relative paths: `EvidenceReconciler`
joins the root to each location's `RelativePath` to read the record, and parses
it through the `SpechtDocumentParser` it takes by constructor, which the host's
container supplies (the static `SpechtDocument.Parse` until then).
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
engine's manifest-driven discovery to locate current Feature records (root-relative
from item `0166`, joined to the root for the read), then apply
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
0201-F7) the selected claims come from the claim files, read through the
engine's Gherkin claim reader (0201-F7 decision 0004, decision 0007); the tool
holds no second reader, and the evidence artifact carries no catalogue (item
0411). `EvidencePolicy.Read` needs no mapped claims section on this path. It
reads the keys as the analyzer's `GherkinClaimPolicyReader` does: the mapped
`/claim` object's `tag` and `tagIgnoreCase` into the claim tag form, with the
`claim` identifier grammar; `scenarioScopes.featureTag` and `epicTag`;
`records.plannedTags` (default `["@ignore"]`), `withdrawnTags` and
`supersededTags`; and `records.claimFileSets`, one set per Feature, each a
relative glob, a Feature, an epic exactly when the grammar declares one and an
optional `record`. A malformed key is invalid configuration (exit 3). The policy
holds them as `ClaimTags` and `ClaimFileSets`; `ClaimTags` is `null` on the
Markdown path, which is unchanged.

A set's `glob` and `record` are read by two copies of one reading (decision
0009). The analyzer keeps its own,
[`AnalysisPaths`](../../Policy/AnalysisPaths.cs) with
[`BindingProvenanceGuard`](../../Bindings/BindingProvenanceGuard.cs). The tool's
is [`RepositoryPaths`](../../../tool/Features/Evidence/RepositoryPaths.cs), a
type of the evidence slice, its only caller, which no other assembly compiles.
Neither side links
or references the other's copy: 0201-F7 decision 0004 admits only the Gherkin
claim reader and the four text readers as the analyzer's links (§ 5 row 10).
Both copies drop empty and `.` segments, read a backslash as `/`, resolve `..`
inside the root, and reject a rooted, drive-qualified or escaping path, so
`./src/read/.spec/*.feature` is the same set to the analyzer and to `specht
evidence`, and an escaping one is invalid configuration to both. Both `Matches`
methods normalise each side and read a final `**` as every file beneath, where
the engine's discovery glob matches no file. The tool's copy keeps the
analyzer's error text, which `EvidencePolicy` replaces with its own invalid
configuration message. The two are pinned by parity cases in both test projects,
the same list of cases in each (B-038). `EvidencePolicy` stores each `glob` and
`record` normalised; a project `path` keeps its stricter `IsRelative` check,
which is not a claim file set.

`EvidenceReconciler.ReadNativeClaims` lists the root's `.feature` files once
with `SpechtDiscovery.FindTestFiles` under the manifest's exclusions, the glob
`**/*.feature`, in root-relative ordinal order, and keeps for each set the files
`RepositoryPaths.Matches` matches to its glob. The candidates are `.feature`
files because the analyzer matches a set only against inputs of kind
`scenario`, which `Specht.CodeAnalysis.props` delivers from
`SpechtScenarioInputs`, `.feature` files by default. A Feature record or any
other file under a set's directory is never a claim file to either reading, so
`src/read/.spec/**` selects `src/read/.spec/read.feature` and not
`src/read/.spec/README.md`. The gate does not read the props' three default
patterns: a consumer's `SpechtScenarioInputs` override lives in its own MSBuild
files, which the gate cannot evaluate offline, and a copy of the defaults would
reject a set over a `.feature` tree such an override delivers.

The two readings keep these differences. The exclusions (decision 0009, § 5
row 11): the analyzer matches the scenario inputs the build delivers and applies
no manifest exclusion, so a claim file under an excluded directory is in the
analyzer's catalogue and not in the evidence gate's; the build's default leaves
out only the `SpechtInputExclude` directories. A `.feature` file a set matches
outside the delivered scenario inputs, such as `src/read/claims/read.feature`
under the default `SpechtScenarioInputs`, is a claim file to the gate and not to
the analyzer. A non-`.feature` file a consumer delivers by overriding
`SpechtScenarioInputs` is a claim file to the analyzer and not to the gate. A file two sets match is an
`invalidInput` finding at that file and contributes no claim (B-039). A selected Feature with no set,
or whose set matches no file, is an `invalidInput` finding at
`.specht/manifest.json` (B-035). A set's `record` is parsed with the
`SpechtDocumentParser`, and its `featureLifecycleKey` gates every claim of the
set as a Markdown record's lifecycle does (B-030); a set with no record is
current, and an unreadable record is an `invalidInput` finding at the record (B-040).
Each file is read and passed to `GherkinClaimTextReader`; an `IOException` or
`UnauthorizedAccessException` or a `GherkinClaimTextException` (B-034), or a Feature or
epic tag value differing from the set (B-041), is an `invalidInput` finding at that claim
file, and exit 3. A declaration whose state is `Active` is selected with
the set's epic and Feature, its id, the claim file's root-relative path and the
declaring element's keyword line (B-028, B-031); `Withdrawn` and `Superseded`
declarations are not (B-029). The reader's planned case flags are not read: a
discovered case is planned only through scenario records (B-032, decision
0008), so a claim whose every case is marked planned still needs its test link
(B-033), and the report JSON is unchanged. The F3 export indexes the claim files
with kind `record`, so the record-index check, which looks up each selected
claim's path, accepts them, and a changed claim file makes the artifact stale
through the existing hash check.

The nine reader types in `src/specht/Model/` (`GherkinClaimTextReader`,
`GherkinClaimTagForm`, `GherkinClaimTags`, `GherkinClaimText`,
`GherkinClaimDeclaration`, `GherkinClaimElement`, `GherkinClaimCaseLine`,
`GherkinClaimTextFinding`, `GherkinClaimTextException`) are `public` from item
0409, so the tool reaches them through its existing reference. `InternalsVisibleTo`
on the engine is rejected (ADR-0001), and the reader's public members expose the
result types, so all nine change together. The analyzer's `Text/` links compile
the same files, so its copies are public in `Specht.Analysis` too.
`RepositoryPaths` is not an engine type: it is `public` in the tool, and
`test/analysis.tests` reaches it through its existing project reference to the
tool, not a source link. Neither
assembly is a compile-time library to anyone outside this repository:
`specht.dll` ships in the tool package and `Specht.Analysis.dll` under
`analyzers/` and `build/` of `Specht.CodeAnalysis`, and no public API file or
dependency closure test changes. The one cost: a project referencing both
assemblies, `test/analysis.tests` and `test/acceptance`, gets CS0433 if it names
one of these nine types; none does, and the analyzer's tests reach them through its
adapters. Rejected: per-assembly accessibility through a preprocessor symbol or
partial declarations, which puts conditional compilation in nine files and cannot
cover the enum, and a public engine facade, which would need a second set of
result types.

For every declared project/TFM pair of a declared test project, require current
compiler and runner evidence in the supplied F3 envelope (B-011, B-022, B-036,
OQ-5 as resolved). `EvidencePolicy.ProjectEntry.RequiresExport` holds for
a project with a `requiredCaseScope` or named, by `project`, in a
`codeAnalysis.enforcement` entry whose `role` is `verifies`. Another declared
project needs no export: with none it raises no `missingCompilation`, and a
supplied export of it is read and checked like any other. An enabled
implementation link with no current export carrying `implements` bindings is
therefore `missing` (exit 1), not `unavailable` (B-037). Group by the pair and their source, assembly,
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
`claims[].cases` is a multiplicity projection: one bare case-ID string per
key, ordered by project, target framework and case ID, so a case ID shared by
two projects appears twice as the same string; identity is on the matching
`cases` rows, which carry project and target framework (decision 0005).
`formatVersion` stays 1. The gate (decision 0004) takes, for each required test
link, the cases of its bound methods in each project/TFM artifact's own selected
run. A case is gated when `EvidenceGate.Gated` holds: its `owned` is not `false`
and it is not planned; version-1 and version-2 artifacts carry no ownership and
count as owned. A gated case that is unselected, has no result or has a
non-passing outcome is a `missingResult` naming the case. A gated, selected case
is listed in `claims[].cases`. An unowned case is never gated, never listed in
`claims[].cases` and never counted in the summary's passed, failed or execution
counts; only unowned rows are excluded from those counts, so an owned planned
row still counts. A case is planned (`EvidenceGate.Planned`) when every current resolved
scenario-record link to its `caseId` has state `planned`; one active link makes
it active, and a link without a `caseId` names no case and gates nothing. Case rows carry `owned` and
`planned`; a planned case is not in `claims[].cases`, covers nothing and is not
gated, while an active row of the same outline still is. A bound method no
discovered case names gets its own `missingResult` naming its symbol ID
(0201-F3 B-014, OQ-4), and so does a bound method with no owned case that is not
planned, in every artifact ("has no owned case"). A claim whose only cases are
planned therefore has no covering case. A declared test project/TFM with no
current export stays `missingCompilation` (exit 2), and its keys are not
reported as passed.

For each current version-3 artifact (0201-F3 § 7) with available results and at
least one case that is owned, not planned and not selected, report one
`incompleteRun` finding at the evidence label, identified by its project and
target framework, whose message counts the owned case keys the run did not
select, excluding planned cases, which are expected not to run (0201-F3
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

### The ids claim record (item 0436)

[`EvidencePolicy`](../../../tool/Features/Evidence/EvidencePolicy.cs) reads the
`codeAnalysis` mappings' projection as the analyzer does (`0201-F1` § 7 "The ids
claim record"). When the projection carries a record at `/ids/claim` it rejects
one beside a mapped `identifiers.claim` and one `0201-F1` B-025 rejects, by
`EvidenceConfigurationException`, which the reconciler reports as `invalidInput`
at `.specht/manifest.json`, exit 3 (B-047). Otherwise it writes the record's
grammar, [`SpechtIdRecord`](../../../specht/Manifest/SpechtIdRecord.cs)`.Grammar(null)`,
into the projected identifiers under `claim`, so `Matches` and the native
Gherkin claim tags read it (B-045). Without the record `identifiers.claim` is
read as before (B-046).

## 8. Testing Strategy

<!-- last written by: test-writer, 2026-11-13 (item 0436: B-045 to B-047 green for the right reason); test-writer, 2026-11-13 (item 0436: B-045 to B-047, the mapped ids claim record, stated in tests); test-writer, 2026-11-09 (item 0429: B-039 to B-044 pinned; the malformed-key, set-entry and Markdown claims-section rejections re-cited from B-028 and B-035 to B-042 to B-044) -->

`test/acceptance/Reconciliation/ReconciliationSteps.cs` runs B-001 to B-044
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
  `AnOwnedCaseItsRunDidNotSelect_*`, `AnArtifactCarryingNoOwnership_*`
  (version-one and version-two artifacts count as owned) and
  `ARequiredClaimsBoundMethodWithAnOwnedCaseTheRunDidNotSelect_*`, which pairs the
  `incompleteRun` finding with the `missingResult` it causes.
- B-018: mechanism is the planned derivation, a case is planned when every
  current resolved scenario link to it is planned. Unit:
  `EvidenceGateUnitTests.ScenarioLinkStatesOfOneCase_WhenEvaluated_ShouldBePlannedOnlyWhenEveryLinkIsPlanned`.
  Integration: `AnOutlineWithAPlannedAndAPassingActiveRow_*`,
  `AClaimWhoseOnlyCasesArePlanned_*`, `ACaseLinkedByAPlannedAndAnActiveScenario_*`
  `APlannedLinkNamingNoCase_*`, `APlannedOwnedCaseTheRunDidNotSelect_*` (planned
  keys are not counted in `incompleteRun`) and `AResolvedScenarioLinkWithNoCaseId_*`
  (the artifact reader rejects it, so it is an `invalidInput` finding, exit 2, nothing gated;
  `APlannedLinkNamingNoCase_*` guards the unresolved case).
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

Testability verdict for positional evidence paths (decision 0006 and its
reversal), one line per claim. `--evidence` is removed; the tests name evidence
positionally only.

- B-023: mechanism is the command's argument binding, `EvidenceCommand.Settings`
  carrying an optional repeatable positional argument that reaches
  `EvidenceReconciler.Reconcile` as evidence paths. Integration (a command
  through `CommandAppTester` is Integration): `TwoPositionalExports_*`.
  Acceptance: `Positional evidence paths`.
- B-024: withdrawn with the option (decision 0006 reversal); no test, and its
  scenario is removed.
- B-025: mechanism is the positional order feeding the reconciler's index-based
  external labels and its file de-duplication. The report sorts
  `scope.supplied` by label, so the order is observable only through the
  `external-evidence-<n>` numbering; the integration test pins it with three
  directories outside the root named out of creation order. Integration:
  `SeveralPositionalDirectories_*` and `TheSameExportNamedTwice_*`. Acceptance:
  `Evidence paths in command-line order` and `Same export named twice`.
- B-026: mechanism is the reconciler's empty-path finding, unchanged. Code read:
  `EvidenceReconciler.Reconcile` adds `missingCompilation` when no file was
  named and none was missing. Integration: `NoPath_*`. It guards that the
  positional argument does not turn the empty case into a usage error.
- B-027: mechanism is Spectre's rendered usage and option list from the
  attributes on `Settings`. Integration: `TheEvidenceHelp_*`, which asserts the
  optional positional usage and that the help names no `--evidence`; it is red
  until the option is removed. `docs/cli.md` matches by the 0002-F1 check,
  which reads options only, so the positional usage line in the page is held
  by review, not by that test.

Testability verdict for native Gherkin claims and the gate's project set
(decisions 0007 and 0008, OQ-5 as resolved), one line per claim. Native claims
are reconciled over `NativeClaimEvidenceRoot` in the integration tier and over
`EvidenceTree.UseNativeClaims` in the acceptance tier; the claim files are
indexed with kind `record`, as the F3 export indexes them.

- B-028: mechanism is the claim source switch: with `records.claimSource`
  `gherkin` and no mapped claims section, `EvidencePolicy.Read` accepts the
  manifest and the reconciler reads each selected Feature's claim file set
  through the engine's `GherkinClaimTextReader` instead of Markdown tables.
  Unit: `EvidencePolicyUnitTests.ANativeClaimManifestWithNoMappedClaimsSection_*`
  (red: `Read` throws "Claims section is missing"); the reader is pinned by
  `GherkinClaimFileReaderUnitTests`, which drive it through its adapter. A set
  names the files the analyzer reads for it: `EvidencePolicy.Read` stores each
  `glob` and `record` through `RepositoryPaths.Normalize`, pinned by
  `EvidencePolicyUnitTests.AClaimFileSetSpelledAsTheAnalyzerReadsIt_*` over a
  `./`, an empty and a backslash segment (red before item 0409's review fix:
  `IsRelative` rejected all three), and the match is `RepositoryPaths.Matches`,
  pinned by `RepositoryPathsUnitTests.ARootRelativePathAndGlob_*` (a final `**`
  taking every file beneath, an inner `**` spanning directories, `*` within one
  segment, an escaped `.`, and either spelling of path and glob; the final-`**`
  row fails when it is read as the engine's discovery glob). Listing the root's
  files and keeping each set's matches has no unit seam: code read,
  `EvidenceReconciler.ReadNativeClaims` lists them with
  `SpechtDiscovery.FindTestFiles` over a real `FileSystem` under the manifest's
  exclusions and filters them with `RepositoryPaths.Matches`, inside
  `Reconcile`. Integration: `NativeClaimsWithAPassingTestBoundToEach_*`,
  `AFeatureFileTheClaimFileSetDoesNotName_*` and
  `AClaimFileSetGlobTheAnalyzerReadsAsNamingAClaimFile_*`, a `./` glob and a
  final-`**` glob over a claim file outside the record's directory, each red
  against the reconciler and policy before the fix. Acceptance: `Native claims reconciled`.
  The policy read of the native keys (§ 7) is pinned by
  `EvidencePolicyUnitTests.ANativeClaimManifestNamingEveryTagKey_*`, a
  case-insensitive claim tag, both scope tags and the three tag lists as named,
  and `ANativeClaimManifestNamingNoOptionalTagKey_*`, a case-sensitive claim
  tag, no scope tag and `@ignore` alone planned. The rejection of a malformed
  native claim key is B-042's, of a malformed claim file set entry B-043's, and
  of a Markdown manifest with no mapped claims section, the switch's other
  side, B-044's.
- B-029: mechanism is the reader's lifecycle state from the configured
  withdrawn and superseded tags, then the existing selection of current claims.
  Unit: `GherkinClaimFileReaderUnitTests.ClaimWithConfiguredLifecycleTags_WhenRead_ShouldGiveTheStrongestState`.
  Integration: `ANativeClaimTaggedWithdrawnOrSuperseded_*`. Acceptance:
  `Withdrawn or superseded native claim`.
- B-030: mechanism is the Feature record's lifecycle, read from the record a
  claim file set names, gating every claim of that set. Unit:
  `BindingLifecycleUnitTests.ClaimAndFeatureLifecycle_WhenEvaluated_ShouldSeparateResolutionFromEligibility`
  for the lifecycle decision; joining a set to its record has no seam beyond
  `ReadNativeClaims`. Integration: `AClaimFileSetWhoseFeatureRecordIsNotCurrent_*`.
  It and the acceptance step assert exit `0`, so a rejected manifest, which also
  selects no claim, cannot pass them. Acceptance:
  `Native claims of a Feature that is not current`. The other side of the
  decision is `AClaimFileSetWithNoRecordOrACurrentRecord_*`: a set with no
  `record`, current by § 7, and a record whose lifecycle is `draft` or
  `in-review` each select the set's claim with no `invalidInput` finding. An
  unreadable `record` is B-040's.
- B-031: mechanism is the reader's declaration line, the keyword line of the
  element the claim tag sits on, carried as the claim's path and line. Unit:
  `GherkinClaimFileReaderUnitTests.ScenarioTaggedWithAClaim_WhenRead_ShouldLocateTheClaimAtTheScenarioKeywordLine`.
  Integration: `ANativeClaimDeclaredOnAScenario_*`, after a `Background`, so the
  line is neither the first element's nor the tag's. Acceptance: `Native claim location`.
- B-032: mechanism is the planned derivation, which reads only scenario-record
  case links; a claim file's planned case flag must not reach it. Unit:
  `EvidenceGateUnitTests.ScenarioLinkStatesOfOneCase_WhenEvaluated_ShouldBePlannedOnlyWhenEveryLinkIsPlanned`;
  the reader's planned flag is
  `GherkinClaimFileReaderUnitTests.ScenarioTaggedIgnored_WhenRead_ShouldStillDeclareItsClaimWithPlannedCases`.
  Integration: `APlannedMarkInAClaimFile_*`. Acceptance: `Planned mark in a claim file`.
- B-033: mechanism is the test requirement, applied to a native claim whatever
  its cases' planned flags. Unit:
  `EvidencePolicyUnitTests.SelectedPolicy_WhenProjected_ShouldRequireTestByDefaultAndOptionalKindsOnlyWhenEnabled`.
  Integration: `ANativeClaimWhoseEveryCaseIsMarkedPlanned_*`. Acceptance:
  `Native claim whose every case is marked planned`.
- B-034: mechanism is the reader's `GherkinClaimTextException` and an
  unreadable file, each mapped to an `invalidInput` finding at the claim file's
  path instead of the manifest catch-all. Unit:
  `GherkinClaimFileReaderUnitTests.ClaimFileBreakingAListedCause_WhenRead_ShouldFailNamingTheFileAsNotValidGherkin`
  and `ClaimFileDeclaringAnotherLanguage_WhenRead_ShouldFailNamingTheFileAndTheLanguage`;
  the mapping has no seam outside `Reconcile`. Integration:
  `AClaimFileTheReaderRejects_*`, the unreadable row holding the file open with
  `FileShare.None`. Acceptance: `Claim file the reader rejects`. A claim file
  whose Feature or epic scope tag names another identity than its set is
  B-041's. The `exception.RelativePath ?? file` fallback in the
  `GherkinClaimTextException` catch has no test.
  Code read: every exception `GherkinClaimTextReader.Read` throws carries its
  path (`Invalid()` and `CheckLanguage`), and the one `null` path, from
  `GherkinClaimTagForm`, is thrown in `EvidencePolicy.Read` before any claim
  file is read, so the fallback is unreachable through the reader.
- B-035: mechanism is the check that every selected Feature has a claim file
  set matching at least one file, reported at the manifest. The check has no
  unit seam; code read as B-028, in `ReadNativeClaims`. A set whose `glob` or
  `record` is rooted, drive-qualified, escapes the root or names nothing is
  invalid configuration, which `Reconcile` reports at the manifest with exit
  `3`. Unit: `EvidencePolicyUnitTests.AClaimFileSetPathRootedEscapingOrNamingNothing_*`,
  which passed on arrival, since `IsRelative` rejected each row before
  `RepositoryPaths` replaced it; its rooted and drive rows fail when
  `Normalize` skips `Validate`. `Validate`'s rejections are also pinned through
  the analyzer by `BindingProvenanceUnitTests` in `test/analysis.tests`.
  Integration: `ASelectedFeatureWithoutClaimFiles_*`, whose `obj/*.feature` row
  sets a claim file only under a directory the manifest excludes; it passed on
  arrival and fails when `ReadNativeClaims` drops the exclusions. Acceptance:
  `Selected Feature without claim files`. Both passed on arrival, because
  `EvidencePolicy.Read` then rejected any manifest without a claims section at
  the manifest. B-028 is built and `Read` accepts the native manifest
  (`ANativeClaimManifestWithNoMappedClaimsSection_*`), so the only manifest
  finding on this path is the one `EvidenceReconciler.ReadNativeClaims` adds for
  the gap. Code read: without it the gap Feature contributes no claim, the other
  Feature's claim passes, both rows exit `0` and both tests fail on the exit
  code. The reconciler's catch-all for a configuration exception in `Reconcile`
  also names the manifest with exit `3`, and the claim does not tell the two
  apart; doing so needs the claim to say more than "naming the manifest", an
  amendment for `spec-author`, not an assertion here.
  `EvidencePolicyUnitTests.ANativeClaimManifestWithNoClaimFileSet_*`, over
  `AbsentClaimFileSets`, pins `EvidencePolicy.FileSets` rejecting
  `claimFileSets` missing, empty or not an array, the manifest holding no claim
  file set for a selected Feature. Its rows were the first three of
  `AMissingOrMalformedClaimFileSet_*`, split by item 0429; the entry rows are
  B-043's. A claim file two sets match is B-039's.
- B-036: mechanism is the derivation of the declared test projects,
  `EvidencePolicy.ProjectEntry.RequiresExport`, true for a project with a
  `requiredCaseScope` or named by a `verifies` enforcement entry, and
  `missingCompilation` raised for those alone. Unit:
  `EvidencePolicyUnitTests.ADeclaredProject_WhenProjected_ShouldRequireAnExportOnlyWithARequiredCaseScopeOrAVerifiesEntryNamingIt`,
  over a required case scope, a `verifies` entry naming the project, neither, an
  `implements` entry naming it and a `verifies` entry naming another project.
  Integration:
  `ADeclaredProjectThatIsNotATestProject_*`, with no role, `implements` and
  `follows`. Acceptance: `Declared production project without evidence`.
  The unit rows also hold an enforcement entry naming the project with no
  `role` and a `verifies` entry naming no project; neither requires an export.
- B-037: mechanism is the implementation link, missing when no current export
  carries `implements` bindings, not unavailable for want of a production
  export. Unit: `EvidenceGateUnitTests.LinkInputs_WhenEvaluated_ShouldRetainTheirIndependentState`.
  Integration: `AnEnabledImplementationLinkWithNoExportCarryingItsImplementsBindings_*`.
  Acceptance: `Implementation link required without production evidence`.
- B-038: mechanism is two copies of one reading (decision 0009) and the
  gate's candidate set. The tool's copy is the evidence slice's
  `RepositoryPaths.Normalize` and `Matches`
  (`src/tool/Features/Evidence/RepositoryPaths.cs`), which `EvidencePolicy`
  stores each glob through and `ReadNativeClaims` matches with. The
  analyzer's copy is its internal `AnalysisPaths` with
  `BindingProvenanceGuard`, reached through the public
  `AnalysisInputReader.Read`. The candidate set is `ReadNativeClaims`'
  listing of the root's `.feature` files, the glob `**/*.feature`, beside the
  analyzer's `scenario` inputs. Unit: one list of eight rows,
  `ClaimFileSetParityCases`, a copy in each test project and never linked. In
  `test/tests/Evidence/`,
  `RepositoryPathsUnitTests.AClaimFileSetGlobSpelling_WhenNormalisedAndMatched_*`
  matches the claim file to the normalised glob, as the gate stores and reads
  it. In `test/analysis.tests`,
  `ClaimFileSetParityUnitTests.AClaimFileSetGlobSpelling_WhenReadByTheAnalyzerAndTheEngine_*`
  reads each glob as a Gherkin manifest's `claimFileSets` through
  `AnalysisInputReader.Read`, asserts the catalogued claim file or the
  manifest's invalid claim file set message, and asserts `RepositoryPaths`'
  answer in the same theory, as `ClaimTagParityUnitTests` does. Five rows read
  a leading `./`, an empty segment, a backslash, `..` inside the root and a
  final `**` over a nested file; three reject a rooted, a drive-qualified and
  an escaping glob. Both passed on arrival. Mutating either copy's backslash,
  `.` and empty segment, `..`, final `**` and root checks fails every row of
  each theory that runs that copy. `RepositoryGlobMatches` no longer carries
  its `./`-and-empty-segment row or its final-`**` row, since the cases hold
  them. The listing has no unit seam, since `ReadNativeClaims` lists files
  over a real `FileSystem`. Integration:
  `EvidenceReconcilerIntegrationTests.AClaimFileSetGlobTheAnalyzerReadsAsNamingAClaimFile_*`,
  whose `AnalyzerClaimFileGlobs` row `src/read/.spec/**` sets a glob over the
  native root's record `src/read/.spec/README.md` and claim file
  `src/read/.spec/read.feature`. It asserts that file's claim and no
  `invalidInput` finding. It passed on arrival, after 4306a6bc, and it fails
  on that assertion when the listing glob goes back to `**/*`, which reads the
  README as a claim file. Acceptance, six examples:
  `Claim file set read alike by the analyzer and specht evidence`,
  `ReconciliationSteps.ThenBothReadTheSameClaimFiles`.
  It reads one tree's manifest, record and every `.feature` file, which the
  default `SpechtScenarioInputs` delivers, through `AnalysisInputReader.Read`,
  and runs `specht evidence` over the same tree. For the first five
  spellings the set is under `.spec/features/read/` and holds a nested file,
  with a claim file outside it. The step asserts no analyzer failure, a
  non-empty catalogue, the evidence report's claim paths equal to the
  catalogue's, and no `invalidInput` finding at a file the catalogue does not
  hold. Drifting either copy's final `**` alone fails the `**` example. The
  tree is under the temporary directory, so no claim file is under `bin/` or
  `obj/`. The sixth example, `ending in ** over the Feature record's
directory`, maps to `src/read/.spec/**`, over the record
  `src/read/.spec/README.md` and the claim file `src/read/.spec/read.feature`.
  It failed on the last assertion, naming the README, while the gate listed
  every file and read the record as a claim file. It passes now that
  `ReadNativeClaims` lists only `.feature` files (§ 7), as the analyzer
  matches only `scenario` inputs. B-038 claims the same reading among the
  `.feature` files the build delivers as scenario inputs, outside the
  manifest's exclusions. Not covered, by design (decision 0009, § 7, § 5 rows
  11 to 13): the differences the two readings keep. Row 11 is the manifest's
  exclusions, which only the gate applies; B-035's excluded-directory row pins
  it. Row 12 is a non-`.feature` file a consumer delivers by overriding
  `SpechtScenarioInputs`, which only the analyzer reads. Row 13 is a
  `.feature` file a set matches that the build does not deliver as a scenario
  input, which only the gate reads. `RepositoryPathsUnitTests.AnEmptyRootedOrDriveQualifiedPath_WhenNormalised_*`
  pins `Validate`'s first check over an empty path, a backslash-rooted path
  and a lower-case drive, which the parity cases do not hold. A `:` at index 1
  after a character that is not a letter has no test: no claim and no § 7
  sentence says whether such a path is drive-qualified.
- B-039: mechanism is `ReadNativeClaims`' `shared` set, the claim files more
  than one set's match list holds, each an `invalidInput` finding at the file
  and read by no set. The match is `RepositoryPaths.Matches`, pinned by
  `RepositoryPathsUnitTests` as B-038; the grouping across sets has no seam
  outside `ReadNativeClaims`, a private method over a real `FileSystem`.
  Integration:
  `EvidenceReconcilerIntegrationTests.AClaimFileTwoClaimFileSetsMatch_WhenReconciled_ShouldFailAsInvalidInputNamingThatClaimFile`,
  a second selected Feature, `cap:list`, whose set has the first set's glob.
  Acceptance: `Claim file two claim file sets match`,
  `ReconciliationSteps.GivenClaimFileOfTwoSets` and `ThenClaimFileRejected`.
  Both passed on arrival, since B-039 states built behaviour. Code read: the
  claim file is valid Gherkin and every other `invalidInput` branch in
  `ReadNativeClaims` names the manifest or a record, so without the `shared`
  check no finding names the file and both fail. That the file contributes no
  claim stays § 7's and is not asserted.
- B-040: mechanism is the `IOException` and `UnauthorizedAccessException`
  catch around the record read in `ReadNativeClaims`, an `invalidInput`
  finding at the set's `record`. No unit seam; the read is a `File` static in
  the same private method. Integration:
  `EvidenceReconcilerIntegrationTests.AClaimFileSetNamingAFeatureRecordThatCannotBeRead_WhenReconciled_ShouldFailAsInvalidInputNamingThatRecord`,
  a `record` naming a missing file, `src/read/.spec/missing.md`. Acceptance:
  `Claim file set whose Feature record cannot be read`,
  `ReconciliationSteps.GivenSelectedFeatureWithClaimFile`,
  `GivenUnreadableFeatureRecord` and `ThenFeatureRecordRejected`. Both passed
  on arrival. A record held open with `FileShare.None`, as B-034's unreadable
  row is, is the same catch and has no row of its own.
- B-041: mechanism is the reader's Feature and epic scope tag values, compared
  in `ReadNativeClaims` with the set's `feature` and `epic`, a mismatch an
  `invalidInput` finding at the claim file. Unit: the capture is the engine's
  `GherkinClaimTextReader`, which the analyzer's reader wraps, pinned by
  `GherkinClaimFileReaderUnitTests.FeatureTaggedWithAClaim_WhenRead_ShouldDeclareItAtTheFeatureAndCoverItsScenarios`
  and `PolicyWithoutScopeForms_WhenFeatureTagsAreRead_ShouldCaptureNoScopeValues`
  in `test/analysis.tests`; the comparison has no seam outside
  `ReadNativeClaims`. Integration:
  `EvidenceReconcilerIntegrationTests.AClaimFileWhoseFeatureOrEpicTagNamesAnotherIdentity_WhenReconciled_ShouldFailAsInvalidInputNamingThatClaimFile`
  over `ForeignScopeTags`, `@feature:cap:list` and `@epic:ep2`, under an epic
  grammar with the Feature and its set in epic `ep1`, so each row names an
  identity the set holds another of. Acceptance, two examples:
  `Claim file tagged for another identity`,
  `ReconciliationSteps.GivenClaimFileTaggedForAnotherIdentity` and
  `ThenClaimFileRejected`. Both passed on arrival. Code read: a Feature-line
  tag is valid Gherkin to the reader, so the only finding at the file is the
  comparison's, and without its Feature or its epic half that row's finding
  is gone.
- B-042: mechanism is `EvidencePolicy.NativeTags`, with `Scope` and `Tags`:
  `claimSource` read as the string `gherkin`, `claim.tag` required as a string
  through the `/claim` mapping, `tagIgnoreCase` as a Boolean, and the claim and
  scope tag forms built as `GherkinClaimTagForm`, whose
  `GherkinClaimTextException` is mapped to `EvidenceConfigurationException`.
  Unit: `EvidencePolicyUnitTests.AMalformedNativeClaimKey_WhenProjected_ShouldBeRejectedAsInvalidConfiguration`
  over `MalformedNativeClaimKeys`, re-cited from B-028: a `claimSource` of
  `markdown` or `1`, no `/claim` mapping, a `tag` missing, `7`, with no or two
  `{claim}` placeholders, a `tagIgnoreCase` of `"yes"`, a Feature or epic
  scope tag without its own placeholder, and a planned, withdrawn or superseded
  list that is not an array, holds a non-string, `@` alone, an entry with no
  leading `@` or one with whitespace. Integration:
  `EvidenceReconcilerIntegrationTests.AManifestWhoseClaimInputTheEvidencePolicyRejects_WhenReconciled_ShouldFailAsInvalidInputNamingTheManifest`
  over `RejectedClaimInputManifests`, whose `claimSource` and claim tag rows are
  B-042's: `Reconcile`'s catch maps the policy's exception to exit `3` and an
  `invalidInput` finding at `.specht/manifest.json`. Code read: the read throws
  before `ReadNativeClaims` runs, so that catch is the only finding at the
  manifest on this path. Acceptance, four examples: `Malformed native claim
key`, `ReconciliationSteps.GivenMalformedNativeClaimKey` and
  `ThenManifestRejected`. All passed on arrival, since B-042 states built
  behaviour. Not tested: a non-string `featureTag` or `epicTag` is ignored, not
  rejected, by `EvidencePolicy.Scope` and by the analyzer's
  `GherkinClaimPolicyReader.Scope` alike, which B-042 and § 7's malformed-key
  sentence do not allow; a defect reported to `implementer`, for its own bug
  item.
- B-043: mechanism is `EvidencePolicy.FileSets` and `Text`, which require each
  entry's `glob` and `feature` as non-blank strings and a present `record` or
  `epic` as one, then the policy's check of each set's Feature and epic against
  the identifier grammars and of one set per Feature. Unit:
  `EvidencePolicyUnitTests.AMalformedClaimFileSetEntry_WhenProjected_ShouldBeRejectedAsInvalidConfiguration`
  over `MalformedClaimFileSets`, the entry rows of the split
  `AMissingOrMalformedClaimFileSet_*`: a `glob` missing, `7` or blank, a
  `feature` missing, `7` or blank, a `record` of `7`, a Feature outside its
  grammar, an epic with no epic grammar, two sets for one Feature, and, under an
  epic grammar, a set with no epic or one outside it. The `feature` of `7` and
  the blank `feature` are new rows, cases B-043 states that no row held; both
  passed on arrival. Integration: `AManifestWhoseClaimInputTheEvidencePolicyRejects_*`,
  its no-glob and two-sets rows, as B-042. Acceptance, four examples:
  `Malformed claim file set entry`,
  `ReconciliationSteps.GivenMalformedClaimFileSetEntry` and
  `ThenManifestRejected`. The analyzer's `GherkinClaimPolicyReader.Sets`
  rejects the entries alike.
- B-044: mechanism is `EvidencePolicy`'s constructor: with no `claimSource`,
  `ClaimTags` stays null and a missing `claimsSection` projection throws
  `EvidenceConfigurationException`. Unit:
  `EvidencePolicyUnitTests.AMarkdownClaimManifestWithNoMappedClaimsSection_WhenProjected_ShouldBeRejected`,
  re-cited from B-028. Integration: `AManifestWhoseClaimInputTheEvidencePolicyRejects_*`,
  its row removing `claimSource` from a manifest that maps no claims section,
  as B-042. Acceptance: `Markdown claim input without a claims section`,
  `ReconciliationSteps.GivenMarkdownClaimsWithoutClaimsSection` and
  `ThenManifestRejected`. Both passed on arrival.
- B-011, amended: the derivation as B-036, beneath the complete-analysis
  state. Unit: `EvidencePolicyUnitTests.ADeclaredProject_*`, as B-036. A
  declared project path keeps § 7's stricter `IsRelative`, pinned by
  `AProjectPathThatIsNotRepositoryRelative_*` over an empty, a rooted, a
  backslash, a drive-qualified, a `.`-segment, an empty-segment and an escaping
  path; it was `EscapingProjectPath_*`, the escaping row alone.
  Integration: `ADeclaredProjectThatIsNotATestProject_*`.
- B-022, amended: the derivation as B-036, beneath the existing incomplete-input
  finding. Unit: `EvidencePolicyUnitTests.ADeclaredProject_*`, as B-036.
  Integration: `ADeclaredProjectWithARequiredCaseScopeAndNoEvidence_*`, which
  passed on arrival, since before item 0409 every declared project needed an
  export.
  `ADeclaredProjectWithNoEvidence_*` and the acceptance tree now name each test
  project in a `verifies` enforcement entry, so they keep their assertions under
  OQ-5 as resolved.

What the design makes hard to test: `EvidenceReconciler.Reconcile` is one
method that reads files, joins records and gates in a single pass, so the
planned and owned decisions are reachable only through `EvidenceGate.Gated` and
`EvidenceGate.Planned`, which the implementer must expose, or through an
artifact on disk. The declared test-project derivation of B-011, B-022 and
B-036 is exposed as `EvidencePolicy.ProjectEntry.RequiresExport` and pinned by
`EvidencePolicyUnitTests.ADeclaredProject_*`. The claim file set glob match of
B-028 is `RepositoryPaths.Matches`, pinned by `RepositoryPathsUnitTests`; the
file listing under the exclusions and record join of B-028 and B-030, the
reader-failure mapping of B-034, the claim file set check of B-035, the
shared claim file of B-039, the unreadable record of B-040 and the scope tag
comparison of B-041 stay inside `EvidenceReconciler.ReadNativeClaims`, a
private method over a real `FileSystem`; they are pinned at the integration
tier, and a seam for them stays reported to `implementer`.

From item `0166` the reconciler takes its `SpechtDocumentParser` by constructor,
so `PartitionedEvidenceRoot`, `VsTestEvidenceTree` and the build test construct it
over a `SpechtDocumentParser` and a `FrontmatterReader`; no assertion changed, and
B-006's record parse is pinned by `SpechtDocumentParserUnitTests`
(`SpechtDocumentUnitTests` until then).

The mapped `ids` claim record (item `0436`) is pinned by
`EvidencePolicyClaimRecordUnitTests` over `EvidencePolicy.Read` and
`EvidencePolicy.Matches`, the member `EvidenceReconciler` asks of each selected
claim: the synthetic manifest keeps its record under `idRecords.claim` and maps
it to `/ids/claim`, as it maps `idPatterns` to `/identifiers`. Members read:
`EvidencePolicy.Read`, which rejects a projection without `identifiers.claim`,
and `EvidencePolicy.Matches`, which matches `identifiers.claim` alone. B-045 is
`EvidencePolicyClaimRecordUnitTests.AClaimUnderAMappedClaimRecord_WhenMatched_ShouldMatchExactlyWhenItIsThePlainHeadThenAWholeMatchOfTheId`,
B-046 `EvidencePolicyClaimRecordUnitTests.AClaimUnderTheOlderClaimGrammarAndNoClaimRecord_WhenMatched_ShouldMatchExactlyWhenTheOlderGrammarAdmitsIt`
and B-047 `EvidencePolicyClaimRecordUnitTests.AMappedClaimRecordTheAnalyzerRejects_WhenProjected_ShouldBeRejected`,
over `EvidenceClaimRecordMatches`, `EvidenceOlderClaimGrammarMatches` and
`EvidenceUnreadableClaimRecords`. The step bindings are
`ReconciliationSteps.ClaimRecord.cs`. Red for the right reason on `net11.0` at
`03e103aa`, with the tests in the working tree: 21 unit cases, 9 fail on their
assertion - every B-045 row, since the policy is rejected without
`identifiers.claim`, and the B-047 row whose record sits beside the older
grammar - and both B-045 examples and that B-047 example fail. Green before the
build: B-046's rows and scenario, a guard that holds today, and B-047's other
rows and examples, green by accident, since a mapping without
`identifiers.claim` is rejected today whatever its record holds. Not asserted:
the rejection's message, and a report frozen before the record was read; B-046's
second step asserts the passing gate and an empty finding list this tree gives
today. Reported to `implementer`: B-045 does not name the policy member the
record is mapped to; the tests assume `/ids/claim`.

Green for the right reason on `net11.0` after `63a3422c`, where
`EvidencePolicy.Read` reads a mapped record through its `ClaimGrammar` and
`RecordText` before `identifiers.claim` is required: all 21 unit cases and the
B-045 to B-047 examples pass. The B-045 rows pass on the record's grammar,
`SpechtIdRecord.Grammar(null)`. B-046's rows and scenario are the guard they
were. B-047's rows that were green by accident now hold on the record's own
checks - beside a mapped `identifiers.claim`, a `plain` or `id` absent or
empty, a `plain` not ending with its only `{id}`, an `id` that does not
compile - since a valid record with no `identifiers.claim` is accepted, as the
B-045 rows show, so a defect row is rejected for its record. B-047 states no
message beyond the manifest named, so the tests do not tell those checks
apart; that each defect is caught by its own check was read in the code. § 7
"The ids claim record" names `/ids/claim` (item `0436`), which closes the gap
reported above.

## 9. Traceability Matrix

<!-- last written by: test-writer, 2026-11-13 (item 0436: B-045 to B-047 `Covered`); test-writer, 2026-11-13 (item 0436: B-045 to B-047 named with their tests, `Partial` until built); test-writer, 2026-11-09 (item 0429: B-042 to B-044 Covered, their unit tests re-cited from B-028 and B-035) -->

| Claim ID | Scenario                                                                                                                                     | Test                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           | Status    |
| -------- | -------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------- |
| B-001    | Direct test chain                                                                                                                            | Direct test chain; `ReconciliationSteps.ThenDirectChain`; `EvidenceWireParityUnitTests.ProducerArtifact_WhenReadByCli_ShouldRetainTheSameVersionOneFields`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     | Covered   |
| B-002    | Incomplete scope                                                                                                                             | Incomplete scope; `ReconciliationSteps.ThenIncomplete`; `EvidenceGateUnitTests.OverlappingFailures_WhenFolded_ShouldUseDocumentedPrecedence`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   | Covered   |
| B-003    | Outcome distinction                                                                                                                          | Outcome distinction; `ReconciliationSteps.ThenOutcomeDistinct`; `EvidenceGateUnitTests.AnExecutionOutcome_WhenEvaluated_ShouldPassOnlyIfPassed`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                | Covered   |
| B-004    | Policy gaps                                                                                                                                  | Policy gaps; `ReconciliationSteps.ThenMissingPolicyLink`; `EvidenceGateUnitTests.LinkInputs_WhenEvaluated_ShouldRetainTheirIndependentState`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   | Covered   |
| B-005    | Optional scenarios                                                                                                                           | Optional scenarios; `ReconciliationSteps.ThenOnlyExplicitScenario`; `EvidenceScenarioJoinUnitTests.ExplicitCaseAndClaim_WhenJoined_ShouldExcludeUnrelatedCasesAndClaims`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       | Covered   |
| B-006    | Unbound claim                                                                                                                                | Unbound claim; `ReconciliationSteps.ThenUnboundClaim`; `SpechtDocumentParserUnitTests.ASection_WhenParsed_ShouldCarryTheHeadersRowsAndOneBasedRowLinesOfTheTableItOpensWith`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   | Covered   |
| B-007    | Ineligible target evidence                                                                                                                   | Ineligible target evidence; `ReconciliationSteps.ThenIneligibleDoesNotCover`; `BindingLifecycleUnitTests.ClaimAndFeatureLifecycle_WhenEvaluated_ShouldSeparateResolutionFromEligibility`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       | Covered   |
| B-008    | Default test requirement                                                                                                                     | Default test requirement; `ReconciliationSteps.ThenUnboundClaim`; `EvidencePolicyUnitTests.SelectedPolicy_WhenProjected_ShouldRequireTestByDefaultAndOptionalKindsOnlyWhenEnabled`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             | Covered   |
| B-009    | Optional link requirements                                                                                                                   | Optional link requirements; `ReconciliationSteps.ThenOptionalRequirement`; `EvidencePolicyUnitTests.SelectedPolicy_WhenProjected_ShouldRequireTestByDefaultAndOptionalKindsOnlyWhenEnabled`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    | Covered   |
| B-010    | Visible coverage exemption; An exempt test link leaves the scenario requirement active                                                       | `ReconciliationSteps.ThenExemptNotCovered`; `ReconciliationSteps.ThenScenarioRemainsRequired`; `EvidencePolicyUnitTests.ClaimExemption_WhenProjected_ShouldKeepItsExactReasonAndLinkKind`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      | Covered   |
| B-011    | Declared compilation completeness; Older export beside current export; Selected record absent from the input index                           | `ReconciliationSteps.ThenAnalysisState`; `ReconciliationSteps.ThenStaleExtraDoesNotDisplaceCurrent`; `ReconciliationSteps.ThenMissingIndexedRecordIsReported`; `EvidenceGateUnitTests.OverlappingFailures_WhenFolded_ShouldUseDocumentedPrecedence`; `EvidencePolicyUnitTests.AProjectPathThatIsNotRepositoryRelative_WhenProjected_ShouldBeRejected`; `EvidencePolicyUnitTests.ADeclaredProject_WhenProjected_ShouldRequireAnExportOnlyWithARequiredCaseScopeOrAVerifiesEntryNamingIt`; `EvidenceReconcilerIntegrationTests.ADeclaredProjectThatIsNotATestProject_WhenItHasNoExport_ShouldNeedNoneAndLetTheTestProjectsEvidenceSucceed`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       | Covered   |
| B-012    | Stale evidence state                                                                                                                         | Stale evidence state; `ReconciliationSteps.ThenEvidenceState`; `TestEvidenceFingerprintUnitTests.ChangedLogicalInput_WhenHashed_ShouldProduceDifferentFingerprint`; `GherkinVsTestConsumerIntegrationTests.SourceEditedAfterTheRun_WhenReconciledAndRebuilt_ShouldReportStaleEvidenceExcludedFromTheGateAndStillBuild`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         | Covered   |
| B-013    | Binding and execution independence                                                                                                           | Binding and execution independence; `ReconciliationSteps.ThenBindingIndependent`; `EvidenceGateUnitTests.LinkInputs_WhenEvaluated_ShouldRetainTheirIndependentState`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           | Covered   |
| B-014    | Trace report surface; Caller-named JSON report; Unwritable caller-named report; External evidence directory; Overlapping evidence selections | `ReconciliationSteps.ThenFormat`; `ReconciliationSteps.ThenReportMatchesStdout`; `ReconciliationSteps.ThenReportFailureIsRelative`; `ReconciliationSteps.ThenExternalLabelsAreDistinct`; `ReconciliationSteps.ThenOverlappingFileIsDeduplicated`; `EvidenceGateUnitTests.OverlappingFailures_WhenFolded_ShouldUseDocumentedPrecedence`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         | Covered   |
| B-015    | Checker report isolation                                                                                                                     | Checker report isolation; `ReconciliationSteps.ThenCheckerUnchanged`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           | Covered   |
| B-016    | Consume supplied evidence                                                                                                                    | Consume supplied evidence; `ReconciliationSteps.ThenNoTestLaunch`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              | Covered   |
| B-017    | Passing outcomes required by default; Owned key missing or not passing                                                                       | Passing outcomes required by default; Owned key missing or not passing; `ReconciliationSteps.ThenDefaultGate`; `ReconciliationSteps.ThenOwnedKeyFails`; `EvidenceGateUnitTests.AnExecutionOutcome_WhenEvaluated_ShouldPassOnlyIfPassed`; `EvidenceGateUnitTests.AnOwnershipAndPlannedState_WhenEvaluated_ShouldGateOnlyAnOwnedCaseThatIsNotPlanned`; `EvidenceReconcilerIntegrationTests.AnOwnedKeyNotPassingInItsOwnRun_WhenAnotherProjectPassedTheSameKeyUnowned_ShouldFailForTheOwnedKeyOnly`; `EvidenceReconcilerIntegrationTests.AnOwnedCaseItsRunDidNotSelect_WhenReconciled_ShouldFailWithAMissingResultNamingTheCase`; `EvidenceReconcilerIntegrationTests.AnArtifactCarryingNoOwnership_WhenItsCaseDidNotPass_ShouldGateItAsOwned` ; `EvidenceReconcilerIntegrationTests.ARequiredClaimsBoundMethodWithAnOwnedCaseTheRunDidNotSelect_WhenReconciled_ShouldReportBothTheIncompleteRunAndTheMissingResultAndExitOne`; `GherkinVsTestConsumerIntegrationTests.DocumentedSequence_WhenFollowedOnACleanCheckout_ShouldPassTheDefaultGateWithCurrentEvidence`; `GherkinVsTestConsumerIntegrationTests.FixtureWithoutItsRequiredCaseScopeAndWithItsPendingMethodsBound_WhenReconciled_ShouldFailTheDefaultGateForTheFailedAndSkippedCases`                                                                   | Covered   |
| B-018    | Planned rows; Claim whose only cases are planned                                                                                             | Planned rows; Claim whose only cases are planned; `ReconciliationSteps.ThenPlannedRowsReported`; `ReconciliationSteps.ThenPlannedRowsNeitherCoverNorFail`; `ReconciliationSteps.ThenNoCoveringCase`; `ReconciliationSteps.ThenPlannedOnlyClaimFails`; `EvidenceGateUnitTests.ScenarioLinkStatesOfOneCase_WhenEvaluated_ShouldBePlannedOnlyWhenEveryLinkIsPlanned`; `EvidenceReconcilerIntegrationTests.AnOutlineWithAPlannedAndAPassingActiveRow_WhenReconciled_ShouldSucceedCoveringTheClaimWithTheActiveRowOnly`; `EvidenceReconcilerIntegrationTests.AClaimWhoseOnlyCasesArePlanned_WhenReconciled_ShouldHaveNoCoveringCaseAndFail`; `EvidenceReconcilerIntegrationTests.ACaseLinkedByAPlannedAndAnActiveScenario_WhenItFailed_ShouldBeActiveAndFailTheGate`; `EvidenceReconcilerIntegrationTests.APlannedLinkNamingNoCase_WhenReconciled_ShouldGateNothing`; `EvidenceReconcilerIntegrationTests.APlannedOwnedCaseTheRunDidNotSelect_WhenReconciled_ShouldNotBeCountedInTheIncompleteRunMessage`; `EvidenceReconcilerIntegrationTests.AResolvedScenarioLinkWithNoCaseId_WhenReconciled_ShouldRejectTheArtifactAsInvalidInputAndGateNothing`                                                                                                                                                                | Covered   |
| B-019    | Active row beside planned rows                                                                                                               | Active row beside planned rows; `ReconciliationSteps.ThenActiveRowFails`; `EvidenceReconcilerIntegrationTests.AnActiveRowBesideAPlannedBlock_WhenItsCaseDidNotPass_ShouldFailTheGateForThatRow`; `EvidenceReconcilerIntegrationTests.AnOutlineWithAPlannedAndAnActiveRow_WhenBothAreSkipped_ShouldReportThePlannedRowAndFailOnlyForTheActiveRow`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               | Covered   |
| B-020    | Partitioned runs; Bound test with no owned case                                                                                              | Partitioned runs; Bound test with no owned case; `ReconciliationSteps.ThenUnownedKeysNotCounted`; `ReconciliationSteps.ThenNoOwnedCaseFails`; `EvidenceGateUnitTests.AnOwnershipAndPlannedState_WhenEvaluated_ShouldGateOnlyAnOwnedCaseThatIsNotPlanned`; `EvidenceReconcilerIntegrationTests.AnUnownedCaseWithAnyOutcome_WhenReconciled_ShouldNeitherCountNorFailForIt`; `EvidenceReconcilerIntegrationTests.ABoundMethodWhoseEveryCaseIsUnownedInEveryProject_WhenReconciled_ShouldFailAsHavingNoOwnedCase`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  | Covered   |
| B-021    | Partitioned runs; Owned key missing or not passing                                                                                           | Partitioned runs; Owned key missing or not passing; `ReconciliationSteps.ThenKeysNotMerged`; `ReconciliationSteps.ThenNoStandIn`; `EvidenceReconcilerIntegrationTests.TwoProjectsEachPassingTheKeysTheyOwn_WhenReconciled_ShouldSucceedWithoutCountingOrListingUnownedKeys`; `EvidenceReconcilerIntegrationTests.TwoProjectsRunsOverTheSameClaim_WhenReconciledWithoutARun_ShouldUseEachProjectsOwnRunAndKeepTheirCaseKeysApart`; `EvidenceReconcilerIntegrationTests.TwoProjectsArtifactsHoldingTheSameCaseIdForOneClaim_WhenReconciled_ShouldListTheBareCaseIdTwiceAndKeepIdentityOnTheCaseRows`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             | Covered   |
| B-022    | Omitted owning run                                                                                                                           | Omitted owning run; `ReconciliationSteps.ThenOmittedRunFails`; `ReconciliationSteps.ThenOmittedKeysNotPassed`; `EvidenceGateUnitTests.OverlappingFailures_WhenFolded_ShouldUseDocumentedPrecedence`; `EvidencePolicyUnitTests.ADeclaredProject_WhenProjected_ShouldRequireAnExportOnlyWithARequiredCaseScopeOrAVerifiesEntryNamingIt`; `EvidenceReconcilerIntegrationTests.ADeclaredProjectWithNoEvidence_WhenReconciled_ShouldFailAsIncompleteInputNamingTheProjectAndReportNoPassForItsKeys`; `EvidenceReconcilerIntegrationTests.ADeclaredProjectWithARequiredCaseScopeAndNoEvidence_WhenReconciled_ShouldFailAsIncompleteInputNamingIt`; `GherkinVsTestConsumerIntegrationTests.CleanCheckout_WhenReconciledBeforeAnyEvidence_ShouldBeIncompleteNamingTheTestProjectAndTheProducingSteps`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  | Covered   |
| B-023    | Positional evidence paths                                                                                                                    | Positional evidence paths; `ReconciliationSteps.ThenBothLocationsAreSupplied`; `EvidenceCommandIntegrationTests.TwoPositionalExports_WhenReconciled_ShouldSupplyBothWithoutError`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              | Covered   |
| B-024    | Evidence option alias                                                                                                                        | None: withdrawn (decision 0006 reversal, 2026-10-25)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           | Withdrawn |
| B-025    | Evidence paths in command-line order                                                                                                         | Evidence paths in command-line order; Same export named twice; `ReconciliationSteps.ThenBothSuppliedInCommandLineOrder`; `ReconciliationSteps.ThenOverlappingFileIsDeduplicated`; `EvidenceCommandIntegrationTests.SeveralPositionalDirectories_WhenReconciled_ShouldNumberExternalDirectoriesInCommandLineOrder`; `EvidenceCommandIntegrationTests.TheSameExportNamedTwice_WhenReconciled_ShouldSupplyItOnce`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 | Covered   |
| B-026    | No evidence path                                                                                                                             | No evidence path; `ReconciliationSteps.ThenNoEvidenceReported`; `EvidenceCommandIntegrationTests.NoPath_WhenReconciled_ShouldFailWithIncompleteInputAndTheNoEvidenceFinding`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   | Covered   |
| B-027    | Evidence help shows positional paths                                                                                                         | Evidence help shows positional paths; `ReconciliationSteps.ThenHelpShowsPositionalPaths`; `EvidenceCommandIntegrationTests.TheEvidenceHelp_WhenRequested_ShouldShowOptionalPositionalPathsAndNoEvidenceOption`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 | Covered   |
| B-028    | Native claims reconciled                                                                                                                     | Native claims reconciled; `ReconciliationSteps.ThenNativeChain`; `EvidencePolicyUnitTests.ANativeClaimManifestWithNoMappedClaimsSection_WhenProjected_ShouldSelectItsFeatures`; `EvidencePolicyUnitTests.AClaimFileSetSpelledAsTheAnalyzerReadsIt_WhenProjected_ShouldHoldItsGlobAndRecordNormalised`; `EvidencePolicyUnitTests.ANativeClaimManifestNamingEveryTagKey_WhenProjected_ShouldReadTheTagFormsAndTagListsItNames`; `EvidencePolicyUnitTests.ANativeClaimManifestNamingNoOptionalTagKey_WhenProjected_ShouldReadACaseSensitiveClaimTagNoScopeTagsAndOnlyIgnoreAsPlanned`; `RepositoryPathsUnitTests.ARootRelativePathAndGlob_WhenMatched_ShouldMatchByNormalisedSegmentsWithAFinalDoubleStarTakingEveryFileBeneath`; `EvidenceReconcilerIntegrationTests.NativeClaimsWithAPassingTestBoundToEach_WhenReconciled_ShouldReportEveryDeclaredClaimWithItsTestChainAndSucceed`; `EvidenceReconcilerIntegrationTests.AFeatureFileTheClaimFileSetDoesNotName_WhenReconciled_ShouldContributeNoClaim`; `EvidenceReconcilerIntegrationTests.AClaimFileSetGlobTheAnalyzerReadsAsNamingAClaimFile_WhenReconciled_ShouldSelectTheClaimsOfThatFile`; `GherkinVsTestConsumerIntegrationTests.NativeClaimsInTheClaimFileSet_WhenTheGateReconcilesAnArtifactCarryingNoCatalogue_ShouldReadEachClaimFromItsClaimFile` | Covered   |
| B-029    | Withdrawn or superseded native claim                                                                                                         | Withdrawn or superseded native claim; `ReconciliationSteps.ThenRetiredNativeClaimNotSelected`; `GherkinClaimFileReaderUnitTests.ClaimWithConfiguredLifecycleTags_WhenRead_ShouldGiveTheStrongestState`; `EvidenceReconcilerIntegrationTests.ANativeClaimTaggedWithdrawnOrSuperseded_WhenNoTestIsBoundToIt_ShouldNotBeSelectedOrRequireEvidence`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                | Covered   |
| B-030    | Native claims of a Feature that is not current                                                                                               | Native claims of a Feature that is not current; `ReconciliationSteps.ThenNoNativeClaimSelected`; `BindingLifecycleUnitTests.ClaimAndFeatureLifecycle_WhenEvaluated_ShouldSeparateResolutionFromEligibility`; `EvidenceReconcilerIntegrationTests.AClaimFileSetWhoseFeatureRecordIsNotCurrent_WhenReconciled_ShouldSelectNoneOfItsNativeClaims`; `EvidenceReconcilerIntegrationTests.AClaimFileSetWithNoRecordOrACurrentRecord_WhenReconciled_ShouldSelectItsNativeClaims`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      | Covered   |
| B-031    | Native claim location                                                                                                                        | Native claim location; `ReconciliationSteps.ThenNativeClaimLocation`; `GherkinClaimFileReaderUnitTests.ScenarioTaggedWithAClaim_WhenRead_ShouldLocateTheClaimAtTheScenarioKeywordLine`; `EvidenceReconcilerIntegrationTests.ANativeClaimDeclaredOnAScenario_WhenReconciled_ShouldLocateItAtTheClaimFileAndTheScenarioKeywordLine`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              | Covered   |
| B-032    | Planned mark in a claim file                                                                                                                 | Planned mark in a claim file; `ReconciliationSteps.ThenNativeCaseNotPlanned`; `ReconciliationSteps.ThenNativeCaseFails`; `EvidenceGateUnitTests.ScenarioLinkStatesOfOneCase_WhenEvaluated_ShouldBePlannedOnlyWhenEveryLinkIsPlanned`; `GherkinClaimFileReaderUnitTests.ScenarioTaggedIgnored_WhenRead_ShouldStillDeclareItsClaimWithPlannedCases`; `EvidenceReconcilerIntegrationTests.APlannedMarkInAClaimFile_WhenItsBoundCaseWasSkippedAndNoScenarioRecordNamesIt_ShouldMarkNoCasePlannedAndFailForTheCase`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 | Covered   |
| B-033    | Native claim whose every case is marked planned                                                                                              | Native claim whose every case is marked planned; `ReconciliationSteps.ThenUnboundClaim`; `EvidencePolicyUnitTests.SelectedPolicy_WhenProjected_ShouldRequireTestByDefaultAndOptionalKindsOnlyWhenEnabled`; `EvidenceReconcilerIntegrationTests.ANativeClaimWhoseEveryCaseIsMarkedPlanned_WhenNoTestIsBoundToIt_ShouldHaveMissingRequiredTestEvidence`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          | Covered   |
| B-034    | Claim file the reader rejects                                                                                                                | Claim file the reader rejects; `ReconciliationSteps.ThenClaimFileRejected`; `GherkinClaimFileReaderUnitTests.ClaimFileBreakingAListedCause_WhenRead_ShouldFailNamingTheFileAsNotValidGherkin`; `GherkinClaimFileReaderUnitTests.ClaimFileDeclaringAnotherLanguage_WhenRead_ShouldFailNamingTheFileAndTheLanguage`; `EvidenceReconcilerIntegrationTests.AClaimFileTheReaderRejects_WhenReconciled_ShouldFailAsInvalidInputNamingThatClaimFile`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  | Covered   |
| B-035    | Selected Feature without claim files                                                                                                         | Selected Feature without claim files; `ReconciliationSteps.ThenManifestRejected`; `EvidencePolicyUnitTests.AClaimFileSetPathRootedEscapingOrNamingNothing_WhenProjected_ShouldBeRejected`; `EvidencePolicyUnitTests.ANativeClaimManifestWithNoClaimFileSet_WhenProjected_ShouldBeRejectedAsInvalidConfiguration`; `EvidenceReconcilerIntegrationTests.ASelectedFeatureWithoutClaimFiles_WhenReconciled_ShouldFailAsInvalidInputNamingTheManifest`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              | Covered   |
| B-036    | Declared production project without evidence                                                                                                 | Declared production project without evidence; `ReconciliationSteps.ThenGateSucceeds`; `ReconciliationSteps.ThenProductionProjectNotMissing`; `EvidencePolicyUnitTests.ADeclaredProject_WhenProjected_ShouldRequireAnExportOnlyWithARequiredCaseScopeOrAVerifiesEntryNamingIt`; `EvidenceReconcilerIntegrationTests.ADeclaredProjectThatIsNotATestProject_WhenItHasNoExport_ShouldNeedNoneAndLetTheTestProjectsEvidenceSucceed`; `GherkinVsTestConsumerIntegrationTests.ProductionProjectNamedOnlyByImplementsEnforcement_WhenReconciledWithAndWithoutTestEvidence_ShouldNeedNoExportAndNeverBeMissing`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         | Covered   |
| B-037    | Implementation link required without production evidence                                                                                     | Implementation link required without production evidence; `ReconciliationSteps.ThenImplementationLinkGap`; `EvidenceGateUnitTests.LinkInputs_WhenEvaluated_ShouldRetainTheirIndependentState`; `EvidenceReconcilerIntegrationTests.AnEnabledImplementationLinkWithNoExportCarryingItsImplementsBindings_WhenReconciled_ShouldReportTheImplementationLinkMissing`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               | Covered   |
| B-038    | Claim file set read alike by the analyzer and specht evidence                                                                                | Claim file set read alike by the analyzer and specht evidence, six examples including `ending in ** over the Feature record's directory`; `ReconciliationSteps.ThenBothReadTheSameClaimFiles`; `RepositoryPathsUnitTests.AClaimFileSetGlobSpelling_WhenNormalisedAndMatched_ShouldReadTheClaimFileOrRejectTheGlobAsTheAnalyzerDoes`; `RepositoryPathsUnitTests.AnEmptyRootedOrDriveQualifiedPath_WhenNormalised_ShouldBeRejected`; `ClaimFileSetParityUnitTests.AClaimFileSetGlobSpelling_WhenReadByTheAnalyzerAndTheEngine_ShouldCatalogueTheClaimFileOrRejectTheGlobInBoth`; `EvidenceReconcilerIntegrationTests.AClaimFileSetGlobTheAnalyzerReadsAsNamingAClaimFile_WhenReconciled_ShouldSelectTheClaimsOfThatFile`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         | Covered   |
| B-039    | Claim file two claim file sets match                                                                                                         | Claim file two claim file sets match; `ReconciliationSteps.GivenClaimFileOfTwoSets`; `ReconciliationSteps.ThenClaimFileRejected`; `RepositoryPathsUnitTests.AClaimFileSetGlobSpelling_WhenNormalisedAndMatched_ShouldReadTheClaimFileOrRejectTheGlobAsTheAnalyzerDoes`; `EvidenceReconcilerIntegrationTests.AClaimFileTwoClaimFileSetsMatch_WhenReconciled_ShouldFailAsInvalidInputNamingThatClaimFile`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        | Covered   |
| B-040    | Claim file set whose Feature record cannot be read                                                                                           | Claim file set whose Feature record cannot be read; `ReconciliationSteps.GivenSelectedFeatureWithClaimFile`; `ReconciliationSteps.GivenUnreadableFeatureRecord`; `ReconciliationSteps.ThenFeatureRecordRejected`; `EvidenceReconcilerIntegrationTests.AClaimFileSetNamingAFeatureRecordThatCannotBeRead_WhenReconciled_ShouldFailAsInvalidInputNamingThatRecord`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               | Covered   |
| B-041    | Claim file tagged for another identity                                                                                                       | Claim file tagged for another identity, two examples; `ReconciliationSteps.GivenClaimFileTaggedForAnotherIdentity`; `ReconciliationSteps.ThenClaimFileRejected`; `GherkinClaimFileReaderUnitTests.FeatureTaggedWithAClaim_WhenRead_ShouldDeclareItAtTheFeatureAndCoverItsScenarios`; `GherkinClaimFileReaderUnitTests.PolicyWithoutScopeForms_WhenFeatureTagsAreRead_ShouldCaptureNoScopeValues`; `EvidenceReconcilerIntegrationTests.AClaimFileWhoseFeatureOrEpicTagNamesAnotherIdentity_WhenReconciled_ShouldFailAsInvalidInputNamingThatClaimFile`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          | Covered   |
| B-042    | Malformed native claim key                                                                                                                   | Malformed native claim key, four examples; `ReconciliationSteps.GivenMalformedNativeClaimKey`; `ReconciliationSteps.ThenManifestRejected`; `EvidencePolicyUnitTests.AMalformedNativeClaimKey_WhenProjected_ShouldBeRejectedAsInvalidConfiguration`; `EvidenceReconcilerIntegrationTests.AManifestWhoseClaimInputTheEvidencePolicyRejects_WhenReconciled_ShouldFailAsInvalidInputNamingTheManifest`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             | Covered   |
| B-043    | Malformed claim file set entry                                                                                                               | Malformed claim file set entry, four examples; `ReconciliationSteps.GivenMalformedClaimFileSetEntry`; `ReconciliationSteps.ThenManifestRejected`; `EvidencePolicyUnitTests.AMalformedClaimFileSetEntry_WhenProjected_ShouldBeRejectedAsInvalidConfiguration`; `EvidenceReconcilerIntegrationTests.AManifestWhoseClaimInputTheEvidencePolicyRejects_WhenReconciled_ShouldFailAsInvalidInputNamingTheManifest`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   | Covered   |
| B-044    | Markdown claim input without a claims section                                                                                                | Markdown claim input without a claims section; `ReconciliationSteps.GivenMarkdownClaimsWithoutClaimsSection`; `ReconciliationSteps.ThenManifestRejected`; `EvidencePolicyUnitTests.AMarkdownClaimManifestWithNoMappedClaimsSection_WhenProjected_ShouldBeRejected`; `EvidenceReconcilerIntegrationTests.AManifestWhoseClaimInputTheEvidencePolicyRejects_WhenReconciled_ShouldFailAsInvalidInputNamingTheManifest`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             | Covered   |
| B-045    | Claim grammar from the ids claim record                                                                                                      | Claim grammar from the ids claim record, two examples; `ReconciliationSteps.GivenAConsumerWhoseManifestMapsItsIdsClaimRecord`; `ReconciliationSteps.ThenTheClaimIsReconciled`; `ReconciliationSteps.ThenTheClaimIsReportedAsViolatingItsGrammar`; `EvidencePolicyClaimRecordUnitTests.AClaimUnderAMappedClaimRecord_WhenMatched_ShouldMatchExactlyWhenItIsThePlainHeadThenAWholeMatchOfTheId`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  | Covered   |
| B-046    | Older claim grammar without an ids claim record                                                                                              | Older claim grammar without an ids claim record; `ReconciliationSteps.GivenAConsumerWhoseManifestMapsTheOlderClaimGrammar`; `ReconciliationSteps.ThenClaimIdsAreReadAndValidatedByTheOlderClaimGrammar`; `EvidencePolicyClaimRecordUnitTests.AClaimUnderTheOlderClaimGrammarAndNoClaimRecord_WhenMatched_ShouldMatchExactlyWhenTheOlderGrammarAdmitsIt`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        | Covered   |
| B-047    | Ids claim record the analyzer rejects                                                                                                        | Ids claim record the analyzer rejects, five examples; `ReconciliationSteps.GivenAConsumerWhoseManifestMapsAnIdsClaimRecordThat`; `ReconciliationSteps.ThenManifestRejected`; `EvidencePolicyClaimRecordUnitTests.AMappedClaimRecordTheAnalyzerRejects_WhenProjected_ShouldBeRejected`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          | Covered   |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-11-13 (B-045 to B-047, item 0436); spec-author, 2026-11-09 (B-042 to B-044, item 0429) -->

The 2026-10-19 owner decision moved the evidence gate from `specht trace` to
`specht evidence` after the approved `0001-F13` command was found to own the
former name with an informational exit contract. B-014 and its scenario changed;
decision 0001 records the reversal. The independent passing-outcome gate of
B-017 and decision 0002 remains in force. The version-one F3 export has no
native diagnostic field, so § 5 and § 7 leave SPCT diagnostics in their existing
build stream while this command reports evidence gaps.

2026-10-24: item 0265 finding M4 records `claims[].cases` as a multiplicity
projection with identity on the `cases` rows and `formatVersion` unchanged
(decision 0005). No claim or scenario changed.

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
the implementer restates § 7 and item 0262's sentence in it. `incompleteRun`
stays an informational finding.

Hand-off to the TestEvidence owner: the B-026 paragraph of its § 7 says
"In this item it is reported and does not change the exit code; gating on owned
keys is item 0262 (0201-F5 decision 0003)." That sentence is superseded for the
gate, since an owned unselected case bound to a required claim now exits 1 as a
`missingResult`. This repository cannot edit that README in this pull request;
the owner must reword it (OQ-4). The TestEvidence acceptance step
`ThenIncompleteRun` already expects exit 1.

2026-10-25: decision 0006 makes the evidence paths positional and keeps
`--evidence` as an alias. B-023 to B-027 are added instead of amending B-014,
which stays the report-surface claim at one assertion; no existing claim changed.
§ 7 (command block), § 8 and § 9 evidence for B-023 to B-027 are for the
implementer and test-writer.

2026-10-25: the owner reversed the alias: "So what I don't like about
`--evidence` we are calling it on `specht evidence`. Would you have
`dotnet test --test`?", then "drop it". Decision 0006 gains a reversal;
`--evidence` is removed with no alias and no deprecation period. B-024 is
withdrawn and its scenario removed; B-023, B-025, B-026 and B-027 are amended to
the positional form only, and § 5 row 8 now excludes a deprecation alias or
warning. § 7, § 8 and § 9 follow for the implementer and test-writer.

2026-11-07: item 0409 writes decision 0007's agreement. B-028 to B-031 read
native claims through the engine's Gherkin claim reader and reconcile them as
Markdown claims; B-034 and B-035 make an unreadable claim file or a missing or
empty claim file set an `invalidInput` finding, exit 3. The owner selected
option A of decision 0008, so B-032 and B-033 keep a claim-file planned mark on
the claim and off every discovered case, and the report JSON is unchanged.
B-036 and B-037 follow OQ-5's proposed default, which stays open: only a
declared test project needs a current export, and B-011 and B-022 are amended
to agree. § 5 rows 5 and 6 are amended to agree with 0201-F7 § 5 row 11 and
decision 0008. § 7's paragraph on the `catalog` member, which item 0411
withdrew, and § 8 and § 9 for B-028 to B-037 are for the implementer and
test-writer.

2026-11-08: item 0409 review round 1, finding 1. C-3 is amended to the declared
test projects, so it agrees with B-011 and B-036 under OQ-5's proposed default.
B-011 departs from its source, 0201-F6 decision 0004, which requires current
inputs for every declared project/target-framework combination; B-011 counts
only the declared test projects' combinations, and its source cell says so.
0201-F6 is unchanged. OQ-5 now records the consequence for the owner's ruling
and stays open. No claim or scenario changed.

2026-11-08: item 0409, the path source link. Commit `e2acacf0` linked
`src/specht/Model/RepositoryPaths.cs` into the analyzer, outside 0201-F7
decision 0004's scope. The owner selected option A of decision 0009, "A: two
copies, tested", put as a shared list of cases run against both. In the
recorder's reading the link is reverted, the analyzer keeps `AnalysisPaths` and
`BindingProvenanceGuard`, and `specht evidence` reads a claim file set with its
own normaliser and matcher, held to the analyzer's reading by one copy of the
list of cases in `test/tests` and one in `test/analysis.tests`, since fixtures
are not shared across test projects. B-038 states the parity and its
scenario runs one claim file set through both; § 5 row 10 excludes shared path
code and row 11 the exclusions difference, which B-035's excluded-directory
case keeps pinned. B-028 is unchanged. Repo-wide lesson 0017 records the cause.
§ 7's `RepositoryPaths` paragraph, § 8, and § 9 for B-038, whose row is
Missing, are for the implementer and test-writer.

2026-11-08: item 0409 review round 2, findings 1 and 4. B-038 is amended: it
claimed the gate and the analyzer read the same claim files outside the
exclusions, but § 7 states two more differences, so it now holds only among the
`.feature` files the build delivers as scenario inputs outside the exclusions.
§ 5 rows 12 and 13 exclude the two: a non-`.feature` file a
`SpechtScenarioInputs` override delivers, which only the analyzer reads and
whose claims the gate leaves out without a finding, and a `.feature` file the
build does not deliver, which only the gate reads (§ 7; B-038). The B-038
scenario is unchanged; its examples lie within the narrowed claim. Decision
0009's description, lesson 0017's Spec delta and the entry above now keep the
owner's selection to option A as it was put, a shared list of cases run against
both; one copy in each test project is the recorder's reading.

2026-11-08: item 0409 review round 3. § 5 rows 12 and 13, and the round 2 entry
above, cited decision 0009 for the two differences it does not cover: decision
0009 records only the exclusions, and the `.feature`-only candidate set came
later, in commit `4306a6bc`. Both rows now cite § 7 and B-038 and state the
boundary as this specification's own. OQ-6 asks the owner whether the gate
should report a finding for a set matching files it does not read; its proposed
default is no finding, as built. No claim or scenario changed.

2026-11-09: item 0429. Its tests found three branches of `ReadNativeClaims`
that § 7 describes and no claim states, so the test-writer left them untested
(§ 8). Asked "Should spec-author amend 0201-F5 so the three unstated branches
can be tested?", the owner answered "Amend the spec". B-039 makes a claim file
two claim file sets match an `invalidInput` finding at that file, B-040 a
selected Feature's unreadable Feature record one at the record, and B-041 a
Feature or epic tag naming another identity than the set one at the claim file;
each exits 3. They are new claims, not amendments: B-034 is the claim file the
reader rejects and B-035 the gap reported at the manifest, and widening either
would join a second subject, the set match, the record or the tag, to one id
and one test. As for B-034, that the rejected file or set contributes no claim
stays § 7's and is not claimed. § 7 is unchanged; it already cites B-034 for
the tag check, a citation for the implementer to move to B-041. § 8 and § 9
for B-039 to B-041, whose rows are Missing, are for the test-writer. No
decision record: the behaviour is decision 0007's, already in § 7.

2026-11-09: item 0429 review. Its unit tests reject malformed native claim
keys, malformed claim file set entries and a Markdown manifest with no mapped
claims section, and no claim stated those rejections; the review blocked on
them. Asked "Add a claim for the malformed-key rejections, or drop those test
rows?", the owner answered "Add a claim". B-042 states the malformed native
claim keys, B-043 the malformed claim file set entries and B-044 the Markdown
manifest with no claims section; each fails with invalid input (exit 3) naming
the manifest, as § 7's "A malformed key is invalid configuration (exit 3)" and
its rejection of invalid normalized policy say. Three claims, not one: the keys
are the claim tag form and the set entries the claim file sets, each pinned by
its own unit theory, and B-044 is the Markdown path, not native claim input.
B-035 keeps a missing or empty `claimFileSets` and its existing row the rooted
and escaping paths, and B-038 the spellings the analyzer reads alike. A
non-string `featureTag` or `epicTag`, which `EvidencePolicy.Scope` ignores, is
not claimed: it is a defect against § 7, for its own bug item. § 8 and § 9 for B-042 to B-044, whose rows are Missing, are
for the test-writer. No decision record: the behaviour is § 7's.

2026-11-13: Schema `0.4.0` moves the claim grammar into the `ids` claim
record and rejects `identifiers.claim` (`0001-F5` B-092). B-045 to B-047 hold
`specht evidence` to the reading `0201-F1` B-023 to B-025 give the analyzer;
§ 5 row 14 bounds it. `0201-F1` decision 0006 records the owner's selection.
§ 8 and § 9 for B-045 to B-047, whose rows are Missing, are for the
test-writer.

## 11. Open Questions

<!-- last written by: spec-author, 2026-11-08 (OQ-5 resolved, item 0302) -->

| ID   | Question                                                                                                                                                                                                                                                                                       | Blocks                   | Resolution                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |
| ---- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | Which links are required by each selectable consumer policy, and what exemptions are supported?                                                                                                                                                                                                | None for coverage policy | RESOLVED, 2026-10-17: 0201-F6 decision 0003 requires a valid test binding for each non-exempt current claim in selected Feature scope. Implementation and scenario requirements are opt-in. Exemptions require explicit reasons and remain visible as exempt, never covered. Configuration encoding remains with 0201-F6 OQ-1.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               |
| OQ-2 | What report surface and stale-result policy are public?                                                                                                                                                                                                                                        | None                     | RESOLVED for behavior, 2026-10-17; command name revised 2026-10-19: F6 decision 0004 defines freshness and completeness. Decision 0001 adopts `specht evidence` with human summary and separate versioned JSON, preserves the checker report and consumes normal workflow artifacts without launching tests. Section 7 records the delivered schema, options and exit-code mapping.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          |
| OQ-3 | `claimSource: "gherkin"` is described in § 7 but the reconciler does not read the claim catalogue, so planned state has no source outside scenario records. Which input supplies it for a consumer without scenario records?                                                                   | B-018, B-019             | RESOLVED, 2026-11-06: decision 0007. The owner rules that `specht evidence` reconciles native Gherkin claims, "Yes, in scope": the reconciler reads them and their planned marks from the claim files through the engine's Gherkin claim reader (0201-F7 decision 0004, B-026), not from the artifact's `catalog`. Item 0409 writes the § 3 claims and amends § 5 rows 5 and 6.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              |
| OQ-4 | TestEvidence § 7 (B-026 paragraph) still says `incompleteRun` "does not change the exit code; gating on owned keys is item 0262 (0201-F5 decision 0003)". Who rewords it?                                                                                                                      | None                     | Open. The sentence is superseded for the gate by decision 0004: an owned unselected case bound to a required claim is a `missingResult`, exit 1; `incompleteRun` stays informational. The TestEvidence owner rewords it; this pull request cannot edit that README.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          |
| OQ-5 | Which declared projects does the default gate require a current export from? Enforcement requires a production project to be declared in `codeAnalysis.projects`, no collector writes an export for it, and B-011 counts every declared combination while B-022 names a declared test project. | B-011, B-022, B-036, C-3 | RESOLVED, 2026-11-08: the owner, "Accept OQ-5 default". The proposed default below is the rule: the gate requires a current export only from a declared test project, one with a `requiredCaseScope` or named by a `verifies` enforcement entry; another declared project needs none, and an enabled `requiredLinks.implementation` with no export carrying its `implements` bindings stays a missing link (B-037). Consequence: a real test project with neither key needs no export, and when its export is missing the run exits 1, or 0 when another project's export covers the claims, instead of the earlier 2. B-011, B-022, B-036, B-037 and C-3 cite it as resolved; no claim and no behaviour changes. Recorded before the ruling: Open. Proposed default: the gate requires a current export only from a declared test project, one with a `requiredCaseScope` or named by a `verifies` enforcement entry; another declared project needs none, and an enabled `requiredLinks.implementation` with no export carrying its `implements` bindings stays a missing link. Item 0409 proceeds on it. 2026-11-07: still open; B-011, B-022, B-036 and B-037 follow this default and cite it. 2026-11-08, consequence for the owner to rule on: under 0201-F6's default configuration, where `enforcement` is optional and sets no mandatory-authoring scope, a declared test project with no `requiredCaseScope` and named by no `verifies` entry needs no export, so omitting its export moves from exit 2 (`missingCompilation`) to exit 1 (a missing test link), or exit 0 when another project's export covers its claims. 0201-F6 decision 0004's wording counts every declared project/target-framework combination toward completeness; B-011 and C-3 count only the declared test projects. Still open; no ruling is recorded. |
| OQ-6 | Should the gate report a finding when a claim file set's glob matches files that are not `.feature` files, which it does not read, such as files a consumer's `SpechtScenarioInputs` override delivers to the analyzer?                                                                        | § 5 row 12               | Open. Proposed default: no finding, as built (§ 7, B-038); the gate leaves those files' claims out. Raised 2026-11-08 by item 0409's review, round 3. No ruling is recorded.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-11-13 (item 0436, round 1: approved); spec-reviewer, 2026-11-09 (item 0429, round 2) -->

| Section                   | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             |
| ------------------------- | ------ | ------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-12                      | 🟢     | spec-reviewer | Item 0262 re-review, 2026-10-24: B-001-B-016 keep the earlier sign-off. B-017 to B-022 are re-approved after amendment: section 3, decisions 0003 and 0004, section 7 and the code agree, section 9 reads Covered for each with passing scenario, unit and integration tests, and the planned-set, `incompleteRun` and caseId-less-link edges are pinned. Evidence unit and integration tests (1368) and the Reconciliation acceptance scenarios (49) pass on net10.0; the earlier full acceptance run passed. The superseded TestEvidence section 7 B-026 sentence is recorded as OQ-4 and in decision 0004 for that README's owner; it does not block this Feature. The owner selected the separate command and passing gate. B-001–B-017 each have passing acceptance and mechanism evidence; the review found and corrected scenario identity, stale-extra and record-index gaps, and the report-path boundary. The direct implementation preserves ADR-0014, ADR-0017 and ADR-0018. Local full build passed with Format skipped because unrelated shared-checkout files require formatting; targeted format and final focused tests passed on both frameworks. CI and PR review remain separate delivery checks.                                                                                                                                                                                                                                                                                                                                            |
| 7, 9                      | 🟢     | spec-reviewer | Item 0265 review (M4), 2026-10-24: decision 0005 and § 7 agree, the new integration test pins the repeated bare case ID and the two `cases` rows, and B-021 stays Covered; no claim changed.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     |
| 3, 5, 7, 9                | 🟢     | spec-reviewer | Item 0298 re-review after the alias reversal, 2026-10-25 (sections 8 and 10 also read): B-024 is marked Withdrawn in sections 3 and 9 and its scenario removed, as the repository's other withdrawn claims are; B-023 and B-025 to B-027 agree with decision 0006's reversal, section 7, the code, the rendered help, `docs/cli.md` and the three usage lines; no `--evidence` remains outside historical text and the tests' negative help assertions. Evidence integration tests (5) and Reconciliation acceptance scenarios (54) pass on net10.0; the self-check reports no errors. Open for their owners: item 0298 still lists B-024 under `claims`, and decision 0006's Callers and "What the test must prove" sections are not marked superseded by the reversal. Supersedes the earlier item 0298 row, which approved the alias.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         |
| 3-11 (item 0409, round 4) | 🟢     | spec-reviewer | Item 0409, round 4, 2026-11-08 - approved, reviewing `02f0ebae..0ee706cf`. The round 3 finding is resolved: section 5 rows 12 and 13 and the section 10 round 2 entry cite section 7 and B-038, not decision 0009, and state the boundary as this specification's own; OQ-6 asks whether the gate reports a finding for a set matching files it does not read, open with its proposed default, no finding, as built, and no ruling recorded. Item 0409 is `done`, closed 2026-11-08, and the Feature `status` returns to `done`, its value at `4b28928c`, with no section 9 row `Missing` or `Partial`. `src/analyzers/analyzers.csproj` is unchanged from `4b28928c`. The test counts of round 3 stand (Evidence 121, `test/analysis.tests` 704, Reconciliation acceptance 74, net10.0); this round changes no code or test and ran none, since the full build ran in parallel. Supersedes the blocked rows of rounds 1 to 3, read at commits `c38d7073`, `63bbb443` and `02f0ebae`: round 1's four findings (C-3 and OQ-5, `docs/cli.md`, the claim file set path reading, section 8's method name), round 2's four (B-038's remaining differences, the `.feature`-only candidate pin, `RepositoryPaths`' slice, decision 0009's attribution) and round 3's one (the decision 0009 citation in section 5 rows 12 and 13) are each resolved on this branch, and the `RepositoryPaths` source link `e2acacf0` added, which 0201-F7 decision 0004 does not permit, is reverted under decision 0009. `spec_status` returns to `approved`, its value at `4b28928c`. |
| 3-4, 7-9, 11 (item 0302)  | 🟢     | spec-reviewer | Item 0302, 2026-11-08, approved at `316a38b4`. OQ-5 is resolved in place on the owner's words, and the sources and § 7 to § 9 cite it as resolved. Open: the 0201-F6 decision 0004 follow-up in 0201-F6 § 12.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |
| 3, 7-10 (item 0429)       | 🟢     | spec-reviewer | Item 0429, round 2, 2026-11-09, approved at `f1d7240a`. The round 1 finding is resolved: B-042, B-043 and B-044 state the malformed native claim keys, the malformed claim file set entries and the Markdown manifest with no claims section, each agreeing with `EvidencePolicy`'s `NativeTags`, `FileSets`, `Text` and constructor, the `Reconcile` catch naming `.specht/manifest.json` and `EvidenceGate.ExitCode`'s 3; one subject each. Section 10 and the item quote the owner's question and "Add a claim" and attribute the three-way split to the specification, not the owner. Every unit, integration and acceptance row asserts only its claim; § 9 has one row per § 3 claim and § 8 names the code read for each Covered row. The `{}` `claimFileSets` row stays B-035's: an empty object holds no claim file set for the selected Feature, as B-035 states, and § 10 keeps an empty `claimFileSets` there. The item's `claims` now lists B-011 and drops B-029 and B-034, and no production file changed. Not blocking, for `implementer` in a later item: the non-string `featureTag` or `epicTag` that `EvidencePolicy.Scope` ignores against § 7 is placed in the item's `out_of_scope` and § 8 and still needs its bug item; B-042 to B-044 carry no `<implements>` tag, `NativeTags` and `FileSets` citing B-028 and B-035; and `ReadNativeClaims`' single B-028 tag and the unreachable `RelativePath ?? file` fallback remain. Evidence tests 177 and Reconciliation acceptance 87 pass on net11.0; the self-check reports 0 errors.      |
| 3, 5, 7-10 (item 0436)    | 🟢     | spec-reviewer | Item 0436, round 1, 2026-11-13 - approved, reviewing `origin/main`...`763955f2`; read only. B-045 to B-047 are built as worded in `EvidencePolicy`: a projected `/ids/claim` record is read through `SpechtIdRecord.Grammar(null)`, `identifiers.claim` is read as before without it, and one beside `identifiers.claim` or one `0201-F1` B-025 rejects fails as `invalidInput` at `.specht/manifest.json`, exit 3. Each is `Covered` by its scenario and `EvidencePolicyClaimRecordUnitTests`; § 5 row 14 bounds it to the record's `plain` and `id`. No finding.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               |

## Tasks

- [0246: Reconcile exported trace evidence in the CLI](../.issue/0246-trace-artifact-reconciliation.yml)
- [0298: Take evidence paths as positional arguments on specht evidence](../.issue/0298-evidence-positional-paths.yml)
- [0409: Reconcile native Gherkin claims and state the gate's project set](../.issue/0409-reconcile-native-gherkin-claims.yml)
- [0436: Cut schema 0.4.0 with the ids key](../../../specht/Manifest/.issue/0436-cut-schema-0-4-0-with-the-ids-key.yml)

## Scoring

| Field | Value | Basis           |
| ----- | ----- | --------------- |
| value | 0     | Not yet scored. |
| risk  | 0     | Not yet scored. |
