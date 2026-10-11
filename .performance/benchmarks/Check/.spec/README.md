---
title: "Specification: End-to-end check scaling"
description: "A benchmark of the engine's whole check - manifest load to report, from a root on disk - over generated trees of 1, 10, 100 and 1000 specifications, so how its time and allocation grow with the tree is a recorded finding"
type: feature
id: "F3"
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
depends_on: ["F1"]
blocks: []
spikes: []
created: "2026-10-09"
updated: "2026-10-11"
github_issue: null
synced_at: null
---

# Specification: End-to-end check scaling

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-09 -->

The check runs at three call sites, one a pre-commit hook a person waits on (brief § 2 Must-1), and nobody knows how its cost grows as a repository's specifications multiply: a check that is linear at ten specifications and quadratic at a thousand is discovered by the consumer whose tree reaches a thousand. This Feature removes that failure state: the engine's whole check is measured over trees of four sizes a decade apart, so whether its time and allocation grow linearly or worse is a recorded finding before any consumer's tree is that large.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-09 -->

| #   | Persona                             | Need                                                                      | Pain Point Today                                              |
| --- | ----------------------------------- | ------------------------------------------------------------------------- | ------------------------------------------------------------- |
| 1   | The maintainer of four repositories | Know what a check costs as a repository grows, before one grows           | A slow hook is the first signal                               |
| 2   | `benchmarker`                       | One benchmark whose result reads as a growth shape across tree sizes      | No benchmark exists to cite against a scaling concern         |
| 3   | `implementer`                       | See whether a change moved the whole check, not only the stage it touched | Stage numbers do not add up to the whole when stages interact |

### Assumptions

| ID  | Assumption                                                                                                                                           |
| --- | ---------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | The tree sizes and the generated, clean tree are `0109-F1`'s (`0109-F1` decision 0004, B-015).                                                       |
| A-2 | The whole check is the engine's run from a root - `SpechtRunner`'s entry that takes one, as ADR-0005 § Decision (a) shapes it - and not the command. |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-09 -->

| ID    | Claim                                                                                                                                      | Source                   | Status |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------ | ------ |
| B-001 | Given generated trees of 1, 10, 100 and 1000 specifications, this Feature measures one whole check of each.                                | owner, 2026-10-09; C-1   | Active |
| B-002 | Given a measured check at any size, this Feature's measured call returns a report that counts every specification generated for that size. | C-2                      | Active |
| B-003 | Given a measured check, this Feature measures it over a tree that lays every specification out co-located, beside the code it specifies.   | OQ-1 (owner, 2026-10-09) | Active |
| B-004 | Given a result, this Feature names in it the number of specifications it was measured over.                                                | C-1                      | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-09 -->

| ID  | Constraint                                                                                                                                                                            | Rules Out                                                                                                                 |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------- |
| C-1 | A whole check's time and allocation is a reported finding against the number of specifications in the tree, stated as a growth shape - constant, linear or worse - never a threshold. | A gate on it, or on its growth; a single-size measurement; a finding of "fast" or "slow".                                 |
| C-2 | The measured call is the engine's whole run from a root on disk - manifest load, discovery, model build, evaluation and the report - and nothing of the host.                         | Measuring the command, the process, argument parsing or output formatting (`0109-F4`); a run cut short before the report. |
| C-3 | The tree is on disk, and the finding says the number includes the file system and its cache.                                                                                          | Reading the tree into memory before the measurement; a measured check that skips the disk is not a check.                 |
| C-4 | Each size's tree is generated once, before its measurements, and reused across them.                                                                                                  | Generating or deleting a tree inside the measurement.                                                                     |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-09 -->

| #   | Item                                                        | Exclusion Reason                                                                  |
| --- | ----------------------------------------------------------- | --------------------------------------------------------------------------------- |
| 1   | Where inside the check the time goes                        | `0109-F2`, stage by stage.                                                        |
| 2   | The tool's process start, and the command around the engine | `0109-F4`.                                                                        |
| 3   | A tree with violations                                      | `0109-F1` decision 0004: the generated tree is clean.                             |
| 4   | The legacy `epics/<epic>/<feature>/spec.md` layout          | Owner, 2026-10-09: co-located only (OQ-1).                                        |
| 5   | Writing the report document to a file                       | `--report` is the host's; the measured run returns the report and writes nothing. |
| 6   | A consumer's real tree, measured in place                   | Never: fixtures are generated (`0109-F1` C-7).                                    |

## 6. Concern Separation

<!-- last written by: spec-author, 2026-10-09 -->

Pending: owned by `implementer`, written after agreement.

## 7. Technical Design

<!-- last written by: spec-author, 2026-10-09 -->

Pending: owned by `implementer`, written after agreement.

## 8. Testing Strategy

<!-- last written by: spec-author, 2026-10-09 -->

Pending: owned by `test-writer`, written after agreement.

### Performance

<!-- last written by: benchmarker, 2026-10-11 -->

| Concern                                                                             | Source | Benchmark               | Finding                                                                                                                                                                                                                                                                                   |
| ----------------------------------------------------------------------------------- | ------ | ----------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| How a whole check's time and allocation grow with the tree's size in specifications | C-1    | `CheckBenchmarks.Check` | Both linear from 1 to 1000 specifications. Time: about 2.7 ms fixed, then 0.42 to 0.43 ms per specification. Allocation: about 473 KB fixed, then 310 KB per specification. The fixed part is 86% of the time and 60% of the allocation at 1 specification, and under 1% of both at 1000. |

- **Environment.** BenchmarkDotNet v0.15.8, macOS Tahoe 26.4 (25E246) [Darwin 25.4.0]; Apple M1 Max, 1 CPU, 10 logical and 10 physical cores; .NET SDK 10.0.401; .NET 10.0.12 (10.0.12, 10.0.1226.42308), Arm64 RyuJIT armv8.0-a. Release build, `Toolchain=.NET 10.0`, out of process, no debugger (`0109-F1` C-6).
- **Job.** The library's default job, run twice on 2026-10-11 with `dotnet run --configuration Release --project .performance/benchmarks -- --filter '*CheckBenchmarks*' --artifacts .artifacts/benchmarks`. The table is the second run; the first run's mean is beside it.

| Specifications | Mean       | Error     | StdDev    | Allocated    | Gen2 per 1000 operations | Mean, first run |
| -------------- | ---------- | --------- | --------- | ------------ | ------------------------ | --------------- |
| 1              | 3.127 ms   | 0.0617 ms | 0.0781 ms | 782.95 KB    | -                        | 3.282 ms        |
| 10             | 6.908 ms   | 0.1381 ms | 0.1644 ms | 3574.63 KB   | -                        | 7.399 ms        |
| 100            | 45.635 ms  | 0.8902 ms | 1.5117 ms | 31485.77 KB  | 250                      | 45.287 ms       |
| 1000           | 435.003 ms | 6.5275 ms | 5.7865 ms | 310536.23 KB | 1000                     | 434.373 ms      |

- **The shape, from the differences between adjacent sizes.** A tenfold tree multiplies the mean by 2.21, 6.61 and 9.53, and the allocation by 4.57, 8.81 and 9.86: each ratio approaches 10 as the fixed part stops mattering.

  | Between sizes | Added time per added specification | Added allocation per added specification |
  | ------------- | ---------------------------------- | ---------------------------------------- |
  | 1 and 10      | 0.420 ms                           | 310.19 KB                                |
  | 10 and 100    | 0.430 ms                           | 310.12 KB                                |
  | 100 and 1000  | 0.433 ms                           | 310.06 KB                                |

  The three time slopes differ by less than the `Error` of the means they are taken from, so no growth worse than linear is shown up to 1000 specifications. Mean divided by size is 3.127, 0.691, 0.456 and 0.435 ms; allocation divided by size is 783, 357, 315 and 311 KB.

- **The fixed part** is what a tree of any size pays once: the manifest and schema load among it. Which stage holds it is `0109-F2`'s to measure (§ 5 row 1).
- **Between the two runs** the means at 100 and 1000 specifications agree within `Error`. At 1 and 10 they differ by 5% and 7%, more than `Error`; the first run had 15 outliers removed at 10 specifications and the second none. Allocation agrees within 0.1% at every size. The first run's slopes are 0.457, 0.421 and 0.432 ms.
- **The file system is in the number (C-3).** Every measured call reads the manifest and `2n` files from a temporary directory on disk. The tree is generated once per size and checked once before the measurement (C-4), so every measured call reads through a warm file-system cache; a first check of a tree the cache does not hold is not measured here.
- **Collections.** At 1000 specifications each check ran 50 gen-0, 4 gen-1 and 1 gen-2 collection.
- **B-002, B-003.** `[GlobalSetup]` runs the same call on the same root once and throws, so the size reports no number, unless the report counts exactly the size's specifications and holds no violation. It does not check the layout: the generator writes only `features/feature-<n>/.spec/README.md` (`0109-F1` § 7).

## 9. Traceability Matrix

<!-- last written by: spec-author, 2026-10-09 -->

| Claim ID | Scenario                                     | Test    | Status  |
| -------- | -------------------------------------------- | ------- | ------- |
| B-001    | A whole check is measured at every tree size | Missing | Missing |
| B-002    | The measured check covers the whole tree     | Missing | Missing |
| B-003    | The measured tree is laid out co-located     | Missing | Missing |
| B-004    | Each result names its tree size              | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-09 -->

- 2026-10-09, spec-reviewer finding on commit `4b23e76`: B-002 to B-004 restated with this Feature as subject. C-3's exclusion no longer calls `0109-F2` B-007's evaluation in-memory; it reads `.feature` files from disk too.
- 2026-10-09, the owner resolved OQ-1: co-located only. B-003 stands; § 5 row 4 cites the decision.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-09 -->

| ID   | Question                                                                                                                                                                                                                                                                                          | Blocks | Resolution                                                  |
| ---- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------ | ----------------------------------------------------------- |
| OQ-1 | Is the legacy `epics/<epic>/<feature>/spec.md` layout measured as well as the co-located one? Discovery reads both, and Transporter's tree is legacy (brief § 2 "Done means"); measuring both doubles the scaling run. Default: co-located only, the layout this repository and the template use. | B-003  | Resolved 2026-10-09: co-located only; B-003 stands (owner). |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-09 -->

| Section | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                          |
| ------- | ------ | ------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-5     | 🟡     | spec-reviewer | Awaiting first review.                                                                                                                                                                                                                                                                                        |
| 1-5     | 🟢     | spec-reviewer | Round 1 (2026-10-09) - approved, reviewing the uncommitted OQ answers on `cdb0876`. OQ-1 is resolved in place to the owner's co-located-only answer; B-003 stands and cites it; § 5 row 4 cites the decision; § 9 and every `@B` tag resolve, and the `@B-003 @boundary` scenario states the excluded layout. |

## Tasks

Cut 2026-10-11 into [`../.issue/`](../.issue/): `0150` (the Feature).

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
