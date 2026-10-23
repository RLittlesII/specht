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

| ID    | Claim                                                                                                                                                                                                                    | Source                                     | Status  |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------ | ------- |
| B-001 | Given a manifest setting `codeAnalysis.records.claimSource` to `gherkin`, this Feature builds the claim catalogue from the configured `.feature` files instead of a Markdown claims table.                               | Consumer request #256; OQ-1                | Active  |
| B-002 | Given a manifest with no `codeAnalysis.records.claimSource`, this Feature leaves the Markdown claims-table catalogue unchanged.                                                                                          | Consumer request #256; OQ-1                | Active  |
| B-003 | Given a `.feature` file matched by a claim file set glob mapped to a Feature identity, this Feature assigns every claim declared in that file to that Feature identity.                                                  | Consumer request #256; OQ-2                | Active  |
| B-004 | Given a tag whose text after `@` begins with the configured candidate prefix and satisfies the claim identifier grammar, this Feature records a claim whose identifier is that whole text exactly as authored.           | Consumer request #256; decision 0001       | Amended |
| B-005 | Given native Gherkin claim input and a Feature metadata record with no claims table, this Feature reports no missing claims-section input failure.                                                                       | Consumer request #256                      | Active  |
| B-006 | Given claim files in a central specification tree outside every project directory, this Feature resolves the same claims for each selected project.                                                                      | Consumer request #256                      | Active  |
| B-007 | Given a consumer identifier grammar that declares no epic, this Feature resolves native claims by Feature identity alone.                                                                                                | Consumer request #256                      | Active  |
| B-008 | Given a claim tag on a Scenario, this Feature records one claim carrying the element kind, the repository-relative path and the one-based line of that Scenario.                                                         | Consumer request #256                      | Active  |
| B-009 | Given a claim tag on a Scenario Outline with several example rows, this Feature records exactly one claim located at that Scenario Outline.                                                                              | Consumer request #256                      | Active  |
| B-010 | Given a claim tag on a Rule, this Feature records one claim located at that Rule and does not treat the Rule's scenarios inheriting the tag as further declarations.                                                     | Consumer request #256; OQ-3                | Active  |
| B-011 | Given a claim file whose scenarios are reordered or retitled, this Feature keeps each claim's identity unchanged.                                                                                                        | Consumer request #256                      | Active  |
| B-012 | Given one claim identifier declared on two elements within one Feature identity, this Feature reports a duplicate-claim diagnostic at each declaration's path and line.                                                  | Consumer request #256; OQ-4                | Active  |
| B-013 | Given a tag whose text after `@` begins with the configured candidate prefix but fails the claim identifier grammar, this Feature reports a malformed-claim diagnostic at that tag's path and line and records no claim. | Consumer request #256; decision 0001       | Amended |
| B-014 | Given a configured claim file that is missing or is not valid Gherkin, this Feature reports an input failure naming that file rather than treating its Feature as having no claims.                                      | Consumer request #256                      | Active  |
| B-015 | Given a Scenario or Scenario Outline in a configured claim file that carries no claim tag of its own or by inheritance, this Feature reports a missing-claim diagnostic at its line with default severity warning.       | Consumer request #256; OQ-5                | Active  |
| B-016 | Given a scenario carrying a tag the consumer configures as planned or ignored, this Feature never counts that scenario as implemented or passing evidence for its claim.                                                 | Consumer request #256; OQ-7                | Active  |
| B-017 | Given native claims and a mandatory enforcement scope missing a required `implements` or `verifies` relationship, this Feature reports the same diagnostic a Markdown-declared claim receives.                           | Consumer request #256                      | Active  |
| B-018 | Given an `implements` or `verifies` relationship naming an identifier absent from the native catalogue, this Feature reports the same target-resolution failure a Markdown-declared claim receives.                      | Consumer request #256                      | Active  |
| B-019 | Given native Gherkin claim input, this Feature leaves the consumer's `.feature` files and metadata records unchanged and writes no claims table or other adapter.                                                        | Consumer request #256                      | Active  |
| B-020 | Given a native claim carrying no lifecycle tag, this Feature treats the claim as Active.                                                                                                                                 | Consumer request #256; OQ-6                | Active  |
| B-021 | Given a native claim carrying a tag the consumer configures as withdrawn or superseded, this Feature gives the claim that lifecycle state.                                                                               | Consumer request #256; OQ-6                | Active  |
| B-022 | Given a configured Feature metadata record, this Feature takes the Feature lifecycle from that record, and treats the Feature as current when none is configured.                                                        | Consumer request #256; OQ-6                | Active  |
| B-023 | Given a claim file whose Feature-level tag names a Feature identity other than the one its claim file set maps, this Feature reports an input failure naming that file.                                                  | Consumer request #256; OQ-2                | Active  |
| B-024 | Given a scenario carrying a configured planned or ignored tag, this Feature still declares its claim, so required-link enforcement on that claim is unchanged.                                                           | Consumer request #256; OQ-7                | Active  |
| B-025 | Given a tag whose text after `@` does not begin with the configured candidate prefix, this Feature treats it as an ordinary tag that declares no claim and draws no diagnostic.                                          | Consumer request #256; decision 0001       | Active  |
| B-027 | Given native Gherkin claim input with no candidate prefix configured, this Feature reports an input failure naming the manifest.                                                                                         | Consumer request #256; decision 0001; OQ-9 | Active  |

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

Not yet designed.

## 7. Technical Design

<!-- last written by: implementer, 2026-10-23 -->

Not yet designed.

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
