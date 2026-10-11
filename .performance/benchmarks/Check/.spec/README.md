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

<!-- last written by: implementer, 2026-10-11 -->

| #   | Concern                                                                                 | Classification | Where                                                                                              |
| --- | --------------------------------------------------------------------------------------- | -------------- | -------------------------------------------------------------------------------------------------- |
| 1   | Which tree sizes are measured, and the size named in each result (B-001, B-004; C-1)    | Business       | `CheckBenchmarks.Specifications`, a `[Params(1, 10, 100, 1000)]` property                          |
| 2   | What the measured call is: the engine's whole run from a root, and no host (C-2)        | Business       | `CheckBenchmarks.Check`, which returns `SpechtRunner.Run` of the root                              |
| 3   | The tree is on disk, built once per size and outside the measured call (C-3, C-4)       | Both           | `CheckBenchmarks.GenerateTree`, the `[GlobalSetup]`; `DeleteTree`, the `[GlobalCleanup]`           |
| 4   | A size is measured only when the check counts its whole tree and finds it clean (B-002) | Business       | `CheckBenchmarks.GenerateTree`, which throws otherwise                                             |
| 5   | What the tree holds and how it is laid out (B-003)                                      | Business       | Not this Feature's: `SpechtTree.Of` and `SpechtTreeStore` (`0109-F1` § 7 `### The tree generator`) |
| 6   | Allocated memory in the result, and which result files a run writes                     | Technical      | Not this Feature's: the shared `BenchmarkConfiguration` (`0109-F1` § 7)                            |

## 7. Technical Design

<!-- last written by: implementer, 2026-10-11 -->

Design for item `0150` (B-001 to B-004; C-1 to C-4), as built. The numbers are § 8's.

- **One class**, [`Specht.Benchmarks.Check.CheckBenchmarks`](../CheckBenchmarks.cs), holds the Feature: one parameter, one setup, one cleanup and one measured method.
- **The size is a parameter.** `Specifications` carries `[Params(1, 10, 100, 1000)]`, so each size is its own measurement and the parameter names the size in its result (B-001, B-004).
- **The setup is outside the measured call**, so building a tree is never measured (C-4). `[GlobalSetup]` runs once per size: it creates a temporary directory and calls `new SpechtTreeStore(new FileSystem()).Save(SpechtTree.Of(Specifications), _root)`. `[GlobalCleanup]` deletes that directory.
- **The tree is on disk** because C-3 requires it: the store writes through the real file system, and the measured call reads from that root.
- **The guard.** The setup then runs the check once on the same root and throws, after deleting the directory, unless the report counts exactly `Specifications` specifications and holds no violation. It exists so a benchmark never measures a failing or miscounted check (B-002).
- **The measured method** is `Check`, which returns `SpechtRunner.Run(_root)` and does nothing else (C-2). The report is returned so the call is not eliminated.
- **The layout is the tree's.** This class lays nothing out; that every specification is co-located (B-003) is what `SpechtTree.Of` generates (`0109-F1` § 7).
- **The class opts into nothing.** The memory diagnoser and the exporters come from the shared `BenchmarkConfiguration` of `0109-F1`; the class carries no diagnoser, job or exporter attribute.
- **The tree types are due to move.** Refactor `0153` (owner, pull request #114) moves `SpechtTree` and `SpechtTreeStore` into the engine; this class's setup call follows them.

## 8. Testing Strategy

<!-- last written by: test-writer, 2026-10-11 (item 0150: the benchmark class read as built; no test, by the owner's rulings) -->

- **No test proves these claims, and none is owed.** Three owner rulings decide it. On 2026-10-08: the build carries no tests. On 2026-10-11, the proof decision that cut item `0145` (`0109-F1` § 10): a build or benchmark-run claim is `Missing` in § 9, with its mechanism and a hand-run record here. On 2026-10-11, pull request #114 (`0109-F1` § 10, verbatim): "Bechmarks don't get tests. They should benchmark production code!" So no test of `CheckBenchmarks` is written or proposed, `check-scaling.feature` is not linked into `test/acceptance`, and it has no step class. All four rows of § 9 are `Missing`.
- **Mechanisms, per claim.** Read on 2026-10-11 in [`CheckBenchmarks.cs`](../CheckBenchmarks.cs), as item `0150` built it. Each is a declaration or a guard in that one class; none has a test.
  - B-001: `Specifications` carries `[Params(1, 10, 100, 1000)]`, and `Check`, the one `[Benchmark]` method, returns `SpechtRunner.Run(_root)`. The library measures the method once for each parameter value, after a `[GlobalSetup]` that builds that value's tree. Four sizes and one whole check of each are those two attributes.
  - B-002: `Check` returns the report. Before any measurement of a size, `GenerateTree` runs the same call on the same root and throws unless `report.SpecificationCount == Specifications` and the report holds no violation. The guard reads the setup's report, not a measured call's: the two are one expression over a root nothing writes to between them, and the check is deterministic (`AGENTS.md` § Invariants), so the measured call returns the same count.
  - B-003: no mechanism in this class. `GenerateTree` saves `SpechtTree.Of(Specifications)`, and nothing else, into a temporary directory it has just created, so the root holds only that tree. Where the tree puts a specification is `SpechtTree.Of`'s, which writes each at `features/feature-<n>/.spec/README.md` (`0109-F1` § 7 `### The tree generator`). The guard does not read the layout.
  - B-004: the parameter is a public property named `Specifications`. The library prints a column for each parameter, headed by the property's name, with the value each row was measured at.
- **Evidence: the hand run in `### Performance`.** `benchmarker`'s run of 2026-10-11, under the default job, with the command recorded there. Its result table has one row for each of 1, 10, 100 and 1000 specifications, each with a mean and an allocated figure. The record names no exit code.
  - B-001 is in that table: four rows, one per size, each a measured whole check.
  - B-004 is in that table: its first column is `Specifications`, and each row carries its size.
  - B-002 is shown by the same rows, through the guard: a size whose setup threw would report no number, and all four report one. `### Performance` says so under "B-002, B-003".
  - B-003 is not shown by the run. It holds by reading `SpechtTree.Of`, as `### Performance` also says.
  - A run by hand pins nothing for the next change, so the rows stay `Missing`.
- **What one measured call reads from disk.** `SpechtRunner.Run(root)` (`src/specht/SpechtRunner.cs` line 15) passes `SchemaVersions.Embedded` to `SpecModel.Load`, which calls the `SpecSchemas.Load` overload that takes a version set (`src/specht/Model/SpecModel.cs` line 61; `src/specht/Model/SpecSchemas.cs` lines 100 to 105). That overload reads the manifest from the tree (`src/specht/Manifest/SpecManifest.cs` line 57) and takes the three frontmatter schemas from the embedded version. `SchemaVersions.Embedded` is a static property, filled once for the process from the assembly's resources (`src/specht/Versioning/SchemaVersions.cs` line 15). The overload that reads the schema files from `.spec/schema/` (`SpecSchemas.cs` lines 72 to 86) is not on the check's path. So a measured call never opens the three schema files `SpechtTree.Of` writes. The files it reads are the manifest, each specification (`SpecModel.cs` line 71) and each Gherkin file (`src/specht/Rules/FeatureFileReader.cs` line 17): the manifest and `2n` files, as `### Performance` says under C-3.
- **Testability verdict.**
  - B-001: mechanism, the `[Params]` and `[Benchmark]` attributes. No unit test: a declaration the library reads, visible only in a run.
  - B-002: mechanism, the guard in `GenerateTree`. No unit test: it is a decision, the only one in the class, and it is reachable only by calling a benchmark's setup over a real root; a benchmark is not tested (owner, pull request #114).
  - B-003: no mechanism in this class; the layout is `SpechtTree.Of`'s.
  - B-004: mechanism, the parameter's name. No unit test: a declaration, visible only in a result.
  - Reported to `implementer`, not acted on: the report carries a count for each layout (`SpechtRunner.Evaluate`), and the guard reads only the total and the violations. A guard that also read the layout counts would make B-003 something the run shows, not only something read.

### Performance

<!-- last written by: benchmarker, 2026-10-11 -->

| Concern                                                                             | Source | Benchmark               | Finding                                                                                                                                                                                                                                                                                   |
| ----------------------------------------------------------------------------------- | ------ | ----------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| How a whole check's time and allocation grow with the tree's size in specifications | C-1    | `CheckBenchmarks.Check` | Both linear from 1 to 1000 specifications. Time: about 2.5 ms fixed, then 0.42 to 0.43 ms per specification. Allocation: about 473 KB fixed, then 310 KB per specification. The fixed part is 86% of the time and 60% of the allocation at 1 specification, and under 1% of both at 1000. |

- **Environment.** BenchmarkDotNet v0.15.8, macOS Tahoe 26.4 (25E246) [Darwin 25.4.0]; Apple M1 Max, 1 CPU, 10 logical and 10 physical cores; .NET SDK 10.0.401; .NET 10.0.12 (10.0.12, 10.0.1226.42308), Arm64 RyuJIT armv8.0-a. Release build, `Toolchain=.NET 10.0`, out of process, no debugger (`0109-F1` C-6).
- **Job.** The library's default job, run on 2026-10-11 with `dotnet run -c Release --project .performance/benchmarks -- --filter '*CheckBenchmarks*' --artifacts .artifacts/benchmarks`. The table is that run; the previous finding's mean and allocation are beside it.

| Specifications | Mean       | Error     | StdDev    | Allocated    | Gen2 per 1000 operations | Mean, previous finding | Allocated, previous finding |
| -------------- | ---------- | --------- | --------- | ------------ | ------------------------ | ---------------------- | --------------------------- |
| 1              | 2.947 ms   | 0.0339 ms | 0.0301 ms | 783.01 KB    | -                        | 3.127 ms               | 782.95 KB                   |
| 10             | 6.754 ms   | 0.1338 ms | 0.2199 ms | 3574.63 KB   | -                        | 6.908 ms               | 3574.63 KB                  |
| 100            | 45.775 ms  | 0.5534 ms | 0.4621 ms | 31485.83 KB  | 250                      | 45.635 ms              | 31485.77 KB                 |
| 1000           | 425.070 ms | 3.3122 ms | 2.9362 ms | 310536.23 KB | 1000                     | 435.003 ms             | 310536.23 KB                |

- **The shape, from the differences between adjacent sizes.** A tenfold tree multiplies the mean by 2.29, 6.78 and 9.29, and the allocation by 4.57, 8.81 and 9.86: each ratio approaches 10 as the fixed part stops mattering.

  | Between sizes | Added time per added specification | Added allocation per added specification |
  | ------------- | ---------------------------------- | ---------------------------------------- |
  | 1 and 10      | 0.423 ms                           | 310.18 KB                                |
  | 10 and 100    | 0.434 ms                           | 310.12 KB                                |
  | 100 and 1000  | 0.421 ms                           | 310.06 KB                                |

  The three time slopes lie within 3% of each other and do not rise with size - the slope to 1000 specifications is the lowest - so no growth worse than linear is shown up to 1000 specifications. Mean divided by size is 2.947, 0.675, 0.458 and 0.425 ms; allocation divided by size is 783, 357, 315 and 311 KB.

- **The fixed part** is what a tree of any size pays once: the manifest and schema load among it. Which stage holds it is `0109-F2`'s to measure (§ 5 row 1).
- **Against the previous finding** the shape did not move: linear in both, with the same allocation slope (310 KB) and time slopes of 0.42 to 0.43 ms in both. Allocation agrees within 0.06 KB at every size. The means at 10 and 100 specifications agree within `Error`. At 1 specification the mean is 5.8% lower and at 1000 it is 2.3% lower, each just beyond the two runs' `Error` taken together; two earlier runs of the same code on this machine differed by 5% at 1 specification, so run-to-run noise of that size is recorded here, and nothing in the measured call changed between the findings. This run removed 1, 4, 2 and 1 outliers at the four sizes.
- **The file system is in the number (C-3).** Every measured call reads the manifest and `2n` files from a temporary directory on disk. Each size's tree is built in memory by `SpechtTree.Of`, saved once through `SpechtTreeStore` over the real file system, and checked once before the measurement (C-4), so every measured call reads through a warm file-system cache; a first check of a tree the cache does not hold is not measured here.
- **Collections.** At 1000 specifications each check ran 50 gen-0, 4 gen-1 and 1 gen-2 collection.
- **B-002, B-003.** `[GlobalSetup]` runs the same call on the same root once and throws, so the size reports no number, unless the report counts exactly the size's specifications and holds no violation. It does not check the layout: `SpechtTree.Of` holds specifications only at `features/feature-<n>/.spec/README.md` (`0109-F1` § 7).

## 9. Traceability Matrix

<!-- last written by: test-writer, 2026-10-11 (item 0150: no test, by the owner's rulings; each row names its mechanism and the hand run) -->

| Claim ID | Scenario                                     | Test                                                                                                                                    | Status  |
| -------- | -------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------- | ------- |
| B-001    | A whole check is measured at every tree size | No test. `CheckBenchmarks.Specifications`' `[Params(1, 10, 100, 1000)]` and the `[Benchmark]` method `Check`; hand run, § 8 Performance | Missing |
| B-002    | The measured check covers the whole tree     | No test. The guard in `CheckBenchmarks.GenerateTree`; hand run, § 8 Performance                                                         | Missing |
| B-003    | The measured tree is laid out co-located     | No test. `SpechtTree.Of`'s layout (`0109-F1` § 7), by reading; the hand run does not show it                                            | Missing |
| B-004    | Each result names its tree size              | No test. The parameter column `Specifications`; hand run, § 8 Performance                                                               | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-11 -->

- 2026-10-09, spec-reviewer finding on commit `4b23e76`: B-002 to B-004 restated with this Feature as subject. C-3's exclusion no longer calls `0109-F2` B-007's evaluation in-memory; it reads `.feature` files from disk too.
- 2026-10-09, the owner resolved OQ-1: co-located only. B-003 stands; § 5 row 4 cites the decision.
- 2026-10-11, item `0150` delivered and the finding recorded in § 8 `### Performance`. No claim or constraint changed. `benchmarker` escalated two things the first measurement showed; neither has an item, and neither is a claim:
  - To `implementer`: a check allocates about 310 KB for each added specification, and `benchmarker` reported a generated specification as about 2.4 KB on disk. Whether the engine's load allocates more than it needs is `implementer`'s judgement; which stage holds the allocation is `0109-F2`'s to measure (§ 5 row 1).
  - To `spec-author`: no constraint states what a first check costs on a tree the file-system cache does not hold. C-3 requires only that the finding say the cache is in the number, and § 8 measures a warm cache. No constraint is added: whether that cost is a concern is the owner's to decide.

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
