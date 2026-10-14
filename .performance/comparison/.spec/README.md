---
title: "Specification: Release baseline comparison"
description: "Automatically compare each published release with the previous release, publish reproducible performance evidence, and flag meaningful regressions for review without failing the build"
type: feature
id: "F5"
epic: "0109"
spec_status: approved
status: ready-for-architecture
priority: med
value: 0
risk: 0
rank: 0
scored_by: null
scored_on: null
domain: "Performance measurement"
author: "spec-author"
milestone: null
children: []
depends_on: ["F1", "0055/F6"]
blocks: []
spikes: []
created: "2026-10-13"
updated: "2026-10-13"
github_issue: null
synced_at: null
---

# Specification: Release baseline comparison

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-13 -->

Published benchmark files do not tell the maintainer whether a release became materially more expensive than its predecessor. This Feature gives every release an automatic comparison report with identifiable evidence and explicit review flags, so performance regressions receive attention without making a measurement a build or publication gate.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-13 -->

| #   | Persona            | Need                                                                    | Pain Point Today                                 |
| --- | ------------------ | ----------------------------------------------------------------------- | ------------------------------------------------ |
| 1   | Release maintainer | See changes in time and managed allocation against the previous release | Separate run artifacts require manual comparison |
| 2   | Reviewer           | Know which measured changes deserve investigation and why               | No automatic review signal exists                |
| 3   | Future maintainer  | Recover the release pair, policy and raw evidence behind a finding      | Transient build artifacts can expire             |

### Assumptions

| ID  | Assumption                                                                                                                                                                                                                                          |
| --- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | `0109-F1` B-010 supplies machine-readable BenchmarkDotNet results; this Feature evaluates them and does not replace the benchmark harness.                                                                                                          |
| A-2 | `0055-F6` supplies the successful publication event and release identity; this Feature owns the performance report added to that release run.                                                                                                       |
| A-3 | Measurement coverage is the release comparison inventory of available engine-stage, whole-check and tool-startup workloads, retaining each input size as a separate case; this Feature does not wait for every measuring Feature to be implemented. |

### Measurement equivalence

Measurement conditions comprise operating system, runtime version, architecture, toolchain, garbage-collection mode, benchmark job and timing confidence level. Workload identity covers the fixture definition and measured operation, independently of its display name. A changed product implementation is the object of comparison, not a workload change. Unknown workload or configuration equivalence is not comparable. Metric validity is checked first: absent or invalid timing uncertainty, including an unknown confidence level, receives `Insufficient evidence` rather than an equivalence judgment. Compatible time or byte scales are normalised; incompatible dimensions or observer boundaries cannot be paired.

### Comparison policy

Decision 0002 records the owner's 2026-10-13 answers. Eligible releases are successfully published stable releases and prereleases; drafts and bare tags are excluded. The baseline is the most recent eligible publication before the current release's first publication, and that pair stays fixed on reruns. Equal publication timestamps are ordered by immutable release identifier, with the larger identifier later in the sequence.

A timing flag requires a mean increase strictly greater than 10% and a current confidence interval whose lower bound is strictly greater than the previous interval's upper bound, using the same confidence level. Overlapping or touching intervals do not flag. An allocation flag requires an increase strictly greater than 10% in managed bytes per operation. Increases exactly at 10% do not flag. These are review screens, not claims of statistical significance or build thresholds.

Percentage change is `100 × (current − previous) / previous` for a positive previous value, computed without display rounding. With zero allocation in both releases, the allocation is unchanged and its percentage is undefined. A zero-to-positive allocation has no defined percentage and receives `Insufficient evidence`, making the overall comparison incomplete unless another metric already requests review. A non-positive timing baseline is invalid evidence. A known review flag takes precedence over coverage gaps in the comparison judgment. If a later reporting stage fails, available run-summary output retains any already-computed flags alongside the incomplete reporting state; it does not invent a successful attachment or a regression-free result.

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-13 -->

| ID    | Claim                                                                                                                                                                                                                             | Source                                              | Status  |
| ----- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------- | ------- |
| B-001 | Given a successfully published stable release or prerelease, this Feature automatically starts its baseline comparison in the release run.                                                                                        | owner, 2026-10-13; OQ-2                             | Active  |
| B-002 | Given earlier releases, this Feature selects the immediately preceding published stable release or prerelease by publication order, fixing that predecessor for reruns and excluding drafts and unpublished tags.                 | owner, 2026-10-13; OQ-2                             | Active  |
| B-003 | Given no earlier eligible release, this Feature publishes a report marked `No baseline` without a regression judgment.                                                                                                            | B-002 boundary                                      | Active  |
| B-004 | Given a selected release pair, this Feature collects both revisions' measurements in the same comparison session on the same runner.                                                                                              | owner-approved direction, 2026-10-13; decision 0001 | Active  |
| B-005 | Given matching workload identities, inputs, measurement boundaries and measurement conditions, this Feature pairs the results as comparable.                                                                                      | decision 0001                                       | Amended |
| B-006 | Given otherwise valid evidence with a mismatch or unknown equivalence in a comparison condition of B-005, this Feature marks the pair `Not comparable` with the reason instead of judging it.                                     | B-005 boundary                                      | Amended |
| B-007 | Given a workload found only in the current measurement inventory, this Feature reports it as `Added` without a regression judgment.                                                                                               | comparison coverage boundary                        | Active  |
| B-008 | Given a workload found only in the previous measurement inventory, this Feature reports it as `Removed` without a regression judgment.                                                                                            | comparison coverage boundary                        | Active  |
| B-009 | Given comparable metric values with a positive previous value, this Feature normalises compatible units and reports both values, their shared unit, absolute change and percentage change.                                        | owner, 2026-10-13                                   | Amended |
| B-010 | Given a zero previous allocation, this Feature reports its percentage as `Not defined`, judging zero-to-zero as unchanged and zero-to-positive as `Insufficient evidence`.                                                        | arithmetic boundary                                 | Active  |
| B-011 | Given comparable timings whose mean rises strictly more than 10% and whose current confidence interval lies strictly above the previous interval, this Feature flags the timing `Review required` with its reason.                | owner, 2026-10-13; OQ-1                             | Active  |
| B-012 | Given comparable managed allocation rising strictly more than 10% above a positive previous allocation, this Feature flags the allocation `Review required` with its reason.                                                      | owner, 2026-10-13; OQ-1                             | Active  |
| B-013 | Given invalid timing values or absent or invalid timing uncertainty, this Feature marks the timing judgment `Insufficient evidence`.                                                                                              | evidence boundary                                   | Amended |
| B-014 | Given any result flagged `Review required`, this Feature identifies it in the release run summary with a link to the report.                                                                                                      | owner, 2026-10-13                                   | Active  |
| B-015 | Given a completed comparison, this Feature publishes human-readable and machine-readable reports with the same findings.                                                                                                          | owner, 2026-10-13; `0109-F1` B-009, B-010           | Active  |
| B-016 | Given a published report, this Feature identifies the release tags and commits, environments, workload cases, comparison policy and raw measurement evidence behind its findings.                                                 | reproducibility boundary                            | Active  |
| B-017 | Given passing correctness gates and a published release, this Feature leaves the build successful when it flags a regression.                                                                                                     | owner, 2026-10-13; `0109-F1` C-1                    | Active  |
| B-018 | Given a collection, parsing, evaluation, rendering or attachment failure after publication, this Feature exposes an `Incomplete` advisory outcome with an actionable reason in the release run summary without failing the build. | advisory reporting boundary; decision 0001          | Amended |
| B-019 | Given at least one flagged metric, this Feature gives the report an overall judgment of `Review required`, retaining any coverage gaps.                                                                                           | owner, 2026-10-13                                   | Active  |
| B-020 | Given no flagged metric but an empty inventory or a required metric missing, not comparable or lacking sufficient evidence, this Feature gives the report an overall judgment of `Incomplete`.                                    | incomplete evidence boundary                        | Active  |
| B-021 | Given at least one required workload case, valid comparable results for every required metric and no flag, this Feature gives the report an overall judgment of `No regression flagged`.                                          | complete evidence boundary                          | Active  |
| B-022 | Given a published comparison, this Feature keeps its reports and referenced raw evidence with the release independently of transient workflow artifact retention.                                                                 | reproducibility boundary; decision 0001             | Active  |
| B-023 | Given a pull request, main push or release dry run that publishes no release, this Feature starts no automatic release comparison.                                                                                                | `0109-F1` OQ-2; `0055-F6` B-008                     | Active  |
| B-024 | Given the selected immediate predecessor cannot be resolved or measured, this Feature reports `Incomplete` with the reason rather than substituting an older release.                                                             | B-002 boundary                                      | Active  |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-13 -->

| ID   | Constraint                                                                                                                                                                                                | Rules Out                                                                                                                       |
| ---- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------- |
| C-1  | A performance measurement or review flag never changes the correctness verdict, prevents publication, or rolls back a release.                                                                            | Timing or allocation gates; treating an advisory flag as a failed check.                                                        |
| C-2  | Comparisons use the exact published product revisions and the same measurement conditions; historical shared-runner timings alone cannot establish a regression.                                          | Comparing unrelated runs or treating an unchanged benchmark name as proof of an unchanged workload.                             |
| C-3  | BenchmarkDotNet's out-of-process Release measurements supply timing and managed allocation; fixture construction and composition keep the boundaries of the measuring Features.                           | Debug or in-process findings, stopwatch substitutes, changed workloads hidden in a release delta.                               |
| C-4  | Each workload and parameter combination is judged independently, with no average hiding a flagged result.                                                                                                 | Combining small and large trees or trading a timing regression against an allocation improvement.                               |
| C-5  | An unknown, duplicated, failed, missing, non-finite, negative, added, removed or incompatible required metric is visible and cannot count as evidence of no regression.                                   | Empty reports presented as clean; dropping unmatched cases from the summary.                                                    |
| C-6  | A report distinguishes mean time, managed bytes per operation and any tool-process peak-memory observation, with explicit units and observer boundaries.                                                  | Presenting benchmark-harness allocation as child-tool memory; treating missing child-process memory as zero.                    |
| C-7  | The comparison inventory is the union of workloads and parameter cases available for the two releases; time and managed allocation are required for each case where the measuring contract supplies them. | A hand-picked subset that omits regressions or silently discards added and removed cases.                                       |
| C-8  | All local outputs stay under the comparison's temporary workspace or `.artifacts/benchmarks/`, and every reported path is repository-relative.                                                            | Consumer-tree writes, global tool installs and absolute paths in reports or fixtures.                                           |
| C-9  | Performance reporting is advisory even when a comparison-owned stage fails; existing benchmark targets retain their broken-benchmark failure semantics outside this release reporting boundary.           | Weakening `0109-F1` B-012 or reporting a failed measurement as a successful comparison.                                         |
| C-10 | Measurement execution grants no publishing credentials; report publication follows `0055-F6` C-5 and C-7 in its existing release job.                                                                     | Running either measured revision with package-write or contents-write credentials; a third job with contents-write permissions. |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-13 -->

| #   | Item                                                                                                           | Exclusion Reason                                                                                                        |
| --- | -------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------- |
| 1   | Failing a build or blocking publication on performance                                                         | Owner, 2026-10-13; C-1.                                                                                                 |
| 2   | Benchmarks on every pull request, branch push or release dry run                                               | This Feature compares published releases; `0109-F1` keeps its main-only automatic measurement policy.                   |
| 3   | Choosing when to release, publishing the package, or rewriting release versions                                | `0055-F5` and `0055-F6` own versioning and publication.                                                                 |
| 4   | Implementing missing stage or startup benchmarks                                                               | `0109-F2` to `0109-F4` own the measurements; unavailable evidence remains visible.                                      |
| 5   | Profiling, attribution to individual commits, or automatic optimisation                                        | Review flags prompt investigation, not a remedy.                                                                        |
| 6   | A new public `specht` command, manifest option, schema version or rule id                                      | This is repository release tooling, not consumer checker behaviour.                                                     |
| 7   | Email, chat messages, automatic issues or automatic reviewer assignment                                        | The review signal lives in the release run summary and report; local work items remain the delivery tracker.            |
| 8   | Predicting performance from a single size, extrapolating to unseen sizes, or claiming statistical significance | Findings cover the measured cases and the agreed screening policy only.                                                 |
| 9   | Review thresholds for tool-process peak memory                                                                 | Report that observation when available; this Feature's initial review policy covers timing and managed allocation only. |

## 6. Concern Separation

<!-- last written by: spec-author, 2026-10-13 -->

Pending: owned by `implementer`, written after agreement.

## 7. Technical Design

<!-- last written by: spec-author, 2026-10-13 -->

Pending: owned by `implementer`, written after agreement. This specification does not choose a workflow layout, comparator library or storage encoding.

## 8. Testing Strategy

<!-- last written by: spec-author, 2026-10-13 -->

Pending: owned by `test-writer`, written after agreement. No executable coverage is claimed.

## 9. Traceability Matrix

<!-- last written by: spec-author, 2026-10-13 (initial unimplemented inventory for test-writer) -->

| Claim ID | Scenario                                                                           | Test    | Status  |
| -------- | ---------------------------------------------------------------------------------- | ------- | ------- |
| B-001    | A published release starts an advisory comparison                                  | Missing | Missing |
| B-002    | The previous eligible release is selected                                          | Missing | Missing |
| B-003    | The first release has no baseline                                                  | Missing | Missing |
| B-004    | Both releases are measured together                                                | Missing | Missing |
| B-005    | Matching workloads produce comparable rows                                         | Missing | Missing |
| B-006    | Changed measurement conditions are visible                                         | Missing | Missing |
| B-007    | A new benchmark remains visible                                                    | Missing | Missing |
| B-008    | A removed benchmark remains visible                                                | Missing | Missing |
| B-009    | Comparable measurements show their change                                          | Missing | Missing |
| B-010    | Zero allocation does not produce an invented percentage                            | Missing | Missing |
| B-011    | Timing review requires both a material increase and separated confidence intervals | Missing | Missing |
| B-012    | Allocation review uses a strict percentage increase                                | Missing | Missing |
| B-013    | Uncertain or invalid timings cannot establish a clean comparison                   | Missing | Missing |
| B-014    | Review flags remain visible in the release run                                     | Missing | Missing |
| B-015    | People and automation receive the same findings                                    | Missing | Missing |
| B-016    | A report identifies the evidence behind its judgment                               | Missing | Missing |
| B-017    | A meaningful regression leaves the build successful                                | Missing | Missing |
| B-018    | A reporting failure leaves an incomplete advisory outcome                          | Missing | Missing |
| B-019    | One review flag determines the overall review status                               | Missing | Missing |
| B-020    | Comparison gaps prevent an overall clean judgment                                  | Missing | Missing |
| B-021    | A complete comparison can report no review flags                                   | Missing | Missing |
| B-022    | An expired historical run does not remove a release report                         | Missing | Missing |
| B-023    | Ordinary builds do not start release comparisons                                   | Missing | Missing |
| B-024    | A missing immediate baseline is not silently replaced                              | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-13 -->

- 2026-10-13, owner requested the deferred comparison capability from `0109-F1` OQ-4: compare every release with its predecessor, automate the report and flag meaningful regressions without failing the build. This Feature takes `0109-F5`; the epic and harness scope now link to it. No implementation was included in that definition.

- 2026-10-13, review rework: B-005, B-006, B-009, B-013 and B-018 now make equivalence, unit conversion, invalid timing evidence and failures beyond benchmark collection explicit. C-5 covers duplicate evidence. Decision 0003 records these refinements; the owner's numeric thresholds and inclusion of prereleases are unchanged.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-13 -->

| ID   | Question                                                                                                                                                                                                                                                                              | Blocks       | Resolution                                                                                                                                                                                                 |
| ---- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | What makes a regression meaningful? Proposed timing screen: mean increase strictly above 10% and non-overlapping confidence intervals; proposed allocation screen: increase strictly above 10%. Equality with the limit does not flag.                                                | B-011, B-012 | Resolved 2026-10-13 by owner: use the proposed initial 10% screens and non-overlapping timing intervals. Decision 0002 records the boundaries; zero baselines are reported without inventing a percentage. |
| OQ-2 | Which releases qualify? Proposed: published stable releases only, excluding drafts and prereleases; choose the most recently published eligible release before the current release's first publication, and preserve that pair on reruns. The first eligible release has no baseline. | B-001, B-002 | Resolved 2026-10-13 by owner: include prereleases. Published stable releases and prereleases qualify; drafts do not. Decision 0002 records selection and reruns.                                           |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-13 -->

| Section | Status | Reviewer             | Note                                                                                                                                                                                                                                                                                                                                                                      |
| ------- | ------ | -------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-5     | 🟢     | Codex, spec-reviewer | Round 2 self-review: decision 0003 and the amended scenarios resolve the equivalence, unit/invalid-evidence and advisory-failure gaps. Owner decisions 0001-0002 stand. All 24 claims have scenario tags and one Missing traceability row each. Agreement approved for decomposition; § 6-8 remain pending, and neither implementation nor independent review is claimed. |

### Review history

| Section | Status | Reviewer             | Note                                                                                                                                                                                                                                                                                   |
| ------- | ------ | -------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-5     | 🟡     | spec-reviewer        | Original draft: owner answered OQ-1 and OQ-2; specification review pending.                                                                                                                                                                                                            |
| 1-5     | 🔴     | Codex, spec-reviewer | Round 1: B-005/B-006 leave unknown measurement equivalence implicit; B-009/B-013 leave unit conversion and invalid timing values implicit; B-018 covers collection failures but not subsequent reporting failures. Routed to spec-author and resolved by decision 0003 in this review. |

## Tasks

Cut 2026-10-13 into [`../.issue/`](../.issue/): `0208` is the Feature container.

| Item | Scope                                               | Claims                                                 |
| ---- | --------------------------------------------------- | ------------------------------------------------------ |
| 0209 | Reviewed execution, evidence and testability design | No behavioural claim; establishes § 6-8                |
| 0210 | Release selection and normalised evidence           | B-002, B-005, B-006, B-007, B-008                      |
| 0211 | Regression judgments                                | B-009, B-010, B-011, B-012, B-013, B-019, B-020, B-021 |
| 0212 | Paired measurement collection                       | B-004                                                  |
| 0213 | Report forms, evidence and durable publication      | B-015, B-016, B-022                                    |
| 0214 | Release workflow and advisory outcomes              | B-001, B-003, B-014, B-017, B-018, B-023, B-024        |

Items own their dependencies and delivery state. No item is taken by this cut.

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
