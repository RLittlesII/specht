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

| #   | Concern                                                                                                                 | Classification | Where                                                                                                            |
| --- | ----------------------------------------------------------------------------------------------------------------------- | -------------- | ---------------------------------------------------------------------------------------------------------------- |
| 1   | Which tree sizes are measured, and the size named in each result (B-001, B-004; C-1)                                    | Business       | `CheckBenchmarks.Specifications`, a `[Params(1, 10, 100, 1000)]` property                                        |
| 2   | What the measured call is: the engine's whole run from a root, and no host (C-2)                                        | Business       | `CheckBenchmarks.Check`, which returns `_runner.Run(_root)` on the runner the setup resolved                     |
| 3   | The tree is on disk, built once per size and outside the measured call (C-3, C-4)                                       | Both           | `CheckBenchmarks.GenerateTree`, the `[GlobalSetup]`; `DeleteTree`, the `[GlobalCleanup]`                         |
| 4   | A size is measured only when the check counts its whole tree and finds it clean (B-002)                                 | Business       | `CheckBenchmarks.GenerateTree`, which throws otherwise                                                           |
| 5   | What the tree holds and how it is laid out (B-003)                                                                      | Business       | Not this Feature's: `SpechtTree.Of` and `SpechtTreeStore` (`0109-F1` § 7 `### The tree generator`)               |
| 6   | Allocated memory in the result, and which result files a run writes                                                     | Technical      | Not this Feature's: the shared `BenchmarkConfiguration` (`0109-F1` § 7)                                          |
| 7   | Where the measured runner comes from: the engine's own registration, composed outside the measured call (C-2; ADR-0003) | Technical      | `CheckBenchmarks.GenerateTree`, through `TypeRegistrar.Build()`; the `src/tool` reference in `benchmarks.csproj` |

## 7. Technical Design

<!-- last written by: implementer, 2026-10-11 -->

Design for item `0150` (B-001 to B-004; C-1 to C-4), as built. The numbers are § 8's.

- **One class**, [`Specht.Benchmarks.Check.CheckBenchmarks`](../CheckBenchmarks.cs), holds the Feature: one parameter, one setup, one cleanup and one measured method.
- **The size is a parameter.** `Specifications` carries `[Params(1, 10, 100, 1000)]`, so each size is its own measurement and the parameter names the size in its result (B-001, B-004).
- **The setup is outside the measured call**, so building a tree is never measured (C-4). `[GlobalSetup]` runs once per size: it creates a temporary directory and calls `new SpechtTreeStore(new FileSystem()).Save(SpechtTree.Of(Specifications), _root)`. `[GlobalCleanup]`, `DeleteTree`, disposes the provider the runner came from and then deletes that directory.
- **The runner is resolved once per size, in the setup.** Since item `0107` (pull request #116) `SpechtRunner` is an instance the container builds, with no static entry. After saving the tree the setup calls `new TypeRegistrar(new ServiceCollection().AddSpechtEngine()).Build()`, keeps the resolver, and resolves `SpechtRunner` from it; it throws if the registration holds none. `AddSpechtEngine()` registers the runner, the loaders, the readers and the rules as singletons ([`SpechtEngineServiceCollectionExtensions.cs`](../../../../src/specht/Composition/SpechtEngineServiceCollectionExtensions.cs)), so what is measured is the engine as the host composes it, not a second hand-built one.
- **Composition is not measured.** C-2 defines the measured call as the engine's whole run from a root on disk, manifest load to report. Building the provider and resolving the runner are not part of that run, so they sit in `[GlobalSetup]` with the tree (C-4), and every measured call of a size runs over the same runner and its singletons.
- **The tree is on disk** because C-3 requires it: the store writes through the real file system, and the measured call reads from that root.
- **The guard.** The setup then runs the check once on the same root, with the same runner the measured call uses, and throws unless the report counts exactly `Specifications` specifications and holds no violation. Before it throws it calls `DeleteTree()`, the cleanup method, so a refused size leaves neither a provider nor a directory behind. It exists so a benchmark never measures a failing or miscounted check (B-002).
- **The measured method** is `Check`, which returns `_runner.Run(_root)` and does nothing else (C-2): an instance call on the runner the setup resolved, touching no host type. The report is returned so the call is not eliminated.
- **`benchmarks.csproj` references `src/tool`, and why.** [`benchmarks.csproj`](../../benchmarks.csproj) references `tool.csproj` beside the engine, for one type: `TypeRegistrar`. ADR-0003 § Decision ([record](../../../../.spec/adr/0003-microsoft-extensions-dependency-injection-is-the-container.md)) says "Only `specht.tool` references the implementation package" and "There is exactly one call site of `BuildServiceProvider()`: inside `TypeRegistrar.Build`", and "nothing else builds a provider". The engine references the container's abstractions only, so a provider can be built here in two ways: reference `Microsoft.Extensions.DependencyInjection` from the benchmark project and call `BuildServiceProvider()` in the setup, which breaks both sentences, or go through the host's `TypeRegistrar`, which keeps both. The second was built. **What it costs.** The benchmark project depends on the host project, so `specht.tool`, Spectre.Console.Cli and the container implementation package arrive transitively, and the class names two host-side types in its setup and cleanup: `TypeRegistrar` and Spectre's `ITypeResolver`. No command, settings type or `CommandApp` is constructed, and the measured call names no host type.
- **Read against C-2.** C-2's wording is "The measured call is the engine's whole run from a root on disk - manifest load, discovery, model build, evaluation and the report - and nothing of the host." It constrains the measured call, and `Check` runs engine code only - the runner holds no provider and no host type - so the reference is consistent with the wording as written. C-2 says nothing either way about a host type in the setup or a host project in the benchmark's references, and no constraint of `0109-F1` (C-1 to C-13) names the host. **Question for the owner:** is "nothing of the host" meant to reach the setup and the project graph too? If it is, this design breaks it and the remedy is ADR-0003's to give, not this class's. The constraint is not amended here.
- **The layout is the tree's.** This class lays nothing out; that every specification is co-located (B-003) is what `SpechtTree.Of` generates (`0109-F1` § 7).
- **The class opts into nothing.** The memory diagnoser and the exporters come from the shared `BenchmarkConfiguration` of `0109-F1`; the class carries no diagnoser, job or exporter attribute.
- **The tree types are due to move.** Refactor `0153` (owner, pull request #114) moves `SpechtTree` and `SpechtTreeStore` into the engine; this class's setup call follows them.

## 8. Testing Strategy

<!-- last written by: test-writer, 2026-10-11 (item 0150: the benchmark class read as built; no test, by the owner's rulings) -->

- **No test proves these claims, and none is owed.** Three owner rulings decide it. On 2026-10-08: the build carries no tests. On 2026-10-11, the proof decision that cut item `0145` (`0109-F1` § 10): a build or benchmark-run claim is `Missing` in § 9, with its mechanism and a hand-run record here. On 2026-10-11, pull request #114 (`0109-F1` § 10, verbatim): "Bechmarks don't get tests. They should benchmark production code!" So no test of `CheckBenchmarks` is written or proposed, `check-scaling.feature` is not linked into `test/acceptance`, and it has no step class. All four rows of § 9 are `Missing`.
- **Mechanisms, per claim.** Read on 2026-10-11 in [`CheckBenchmarks.cs`](../CheckBenchmarks.cs), as item `0150` built it. Each is a declaration or a guard in that one class; none has a test.
  - B-001: `Specifications` carries `[Params(1, 10, 100, 1000)]`, and `Check`, the one `[Benchmark]` method, returns `_runner.Run(_root)` on the `SpechtRunner` the setup resolved. The library measures the method once for each parameter value, after a `[GlobalSetup]` that builds that value's tree. Four sizes and one whole check of each are those two attributes.
  - B-002: `Check` returns the report. Before any measurement of a size, `GenerateTree` resolves the runner and calls `_runner.Run(_root)` once, then throws unless `report.SpecificationCount == Specifications` and the report holds no violation. The guard reads the setup's report, not a measured call's: the two are the same call on the same runner instance, over a root nothing writes to between them, and the check is deterministic (`AGENTS.md` § Invariants), so the measured call returns the same count.
  - B-003: no mechanism in this class. `GenerateTree` saves `SpechtTree.Of(Specifications)`, and nothing else, into a temporary directory it has just created, so the root holds only that tree. Where the tree puts a specification is `SpechtTree.Of`'s, which writes each at `features/feature-<n>/.spec/README.md` (`0109-F1` § 7 `### The tree generator`). The guard does not read the layout.
  - B-004: the parameter is a public property named `Specifications`. The library prints a column for each parameter, headed by the property's name, with the value each row was measured at.
- **Evidence: the hand run in `### Performance`.** `benchmarker`'s run of 2026-10-11, under the default job, with the command recorded there. Its result table has one row for each of 1, 10, 100 and 1000 specifications, each with a mean and an allocated figure. The record says the run exited 0.
  - B-001 is in that table: four rows, one per size, each a measured whole check.
  - B-004 is in that table: its first column is `Specifications`, and each row carries its size.
  - B-002 is shown by the same rows, through the guard: a size whose setup threw would report no number, and all four report one. `### Performance` says so under "B-002, B-003".
  - B-003 is not shown by the run. It holds by reading `SpechtTree.Of`, as `### Performance` also says.
  - A run by hand pins nothing for the next change, so the rows stay `Missing`.
- **What one measured call reads from disk.** Traced on 2026-10-11, after item `0107`. `SpechtRunner.Run(root)` (`src/specht/SpechtRunner.cs` line 69) passes `SchemaVersions.Embedded` to its two-argument overload (line 76), which calls `SpecModelLoader.Load(root, versions)` (`src/specht/Model/SpecModelLoader.cs` line 41). That calls `SpecSchemasLoader.Load(root, versions)` (line 43; `src/specht/Model/SpecSchemasLoader.cs` line 31), which hands on to the `SpecSchemas.Load` overload that takes a version set (`src/specht/Model/SpecSchemas.cs` lines 100 to 105). That overload reads the manifest from the tree (`src/specht/Manifest/SpecManifest.cs` line 57) and takes the three frontmatter schemas from the embedded version. `SchemaVersions.Embedded` is a static property, filled once for the process from the assembly's resources (`src/specht/Versioning/SchemaVersions.cs` line 15). The overload that reads the schema files from `.spec/schema/` (`SpecSchemas.cs` lines 72 to 86) is not on the check's path. So a measured call never opens the three schema files `SpechtTree.Of` writes. The files it reads are the manifest, each specification (`SpecModelLoader.cs` line 52) and each Gherkin file (`src/specht/Rules/FeatureFileReader.cs` line 19): the manifest and `2n` files, as `### Performance` says under C-3.
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
| How a whole check's time and allocation grow with the tree's size in specifications | C-1    | `CheckBenchmarks.Check` | Both linear from 1 to 1000 specifications. Time: about 2.6 ms fixed, then 0.41 to 0.43 ms per specification. Allocation: about 472 KB fixed, then 310 KB per specification. The fixed part is 86% of the time and 60% of the allocation at 1 specification, and under 1% of both at 1000. |

- **Environment.** BenchmarkDotNet v0.15.8, macOS Tahoe 26.4 (25E246) [Darwin 25.4.0]; Apple M1 Max, 1 CPU, 10 logical and 10 physical cores; .NET SDK 10.0.401; .NET 10.0.12 (10.0.12, 10.0.1226.42308), Arm64 RyuJIT armv8.0-a. Release build, `Toolchain=.NET 10.0`, out of process, no debugger (`0109-F1` C-6).
- **Job.** The library's default job, run on 2026-10-11 with `dotnet run -c Release --project .performance/benchmarks -- --filter '*CheckBenchmarks*' --artifacts .artifacts/benchmarks`. The run exited 0. The table is that run; the previous finding's mean and allocation are beside it.
- **What is measured, since item `0107`.** The measured call is `Run(root)` on one `SpechtRunner` instance. `[GlobalSetup]` resolves it once per size from the provider `TypeRegistrar.Build()` builds over `AddSpechtEngine()`, so building the provider and resolving the engine are outside the measured call, and every measured call of a size runs over the same singleton loader, readers and rules. The previous finding measured the static `SpechtRunner.Run(root)`, which built no container either: it ran the same stages through static entry points over the static rule list. The tree, the sizes and the job are unchanged.

| Specifications | Mean       | Error     | StdDev    | Allocated    | Gen2 per 1000 operations | Mean, previous finding | Allocated, previous finding |
| -------------- | ---------- | --------- | --------- | ------------ | ------------------------ | ---------------------- | --------------------------- |
| 1              | 3.034 ms   | 0.0527 ms | 0.0493 ms | 782.33 KB    | -                        | 2.947 ms               | 783.01 KB                   |
| 10             | 6.747 ms   | 0.1335 ms | 0.1538 ms | 3573.32 KB   | -                        | 6.754 ms               | 3574.63 KB                  |
| 100            | 44.963 ms  | 0.8957 ms | 1.7471 ms | 31485.69 KB  | 250                      | 45.775 ms              | 31485.83 KB                 |
| 1000           | 431.466 ms | 5.9679 ms | 5.5824 ms | 310535.55 KB | 1000                     | 425.070 ms             | 310536.23 KB                |

- **The shape, from the differences between adjacent sizes.** A tenfold tree multiplies the mean by 2.22, 6.66 and 9.60, and the allocation by 4.57, 8.81 and 9.86: each ratio approaches 10 as the fixed part stops mattering.

  | Between sizes | Added time per added specification | Added allocation per added specification |
  | ------------- | ---------------------------------- | ---------------------------------------- |
  | 1 and 10      | 0.413 ms                           | 310.11 KB                                |
  | 10 and 100    | 0.425 ms                           | 310.14 KB                                |
  | 100 and 1000  | 0.429 ms                           | 310.06 KB                                |

  The three time slopes lie within 4% of each other. They rise by 0.012 ms and then 0.004 ms from one interval to the next, each rise within what the `Error` of the means it is computed from allows (about 0.010 ms and 0.007 ms per specification for the second and third slopes), so no growth worse than linear is shown up to 1000 specifications. Mean divided by size is 3.034, 0.675, 0.450 and 0.431 ms; allocation divided by size is 782, 357, 315 and 311 KB.

- **The fixed part** is what a tree of any size pays once: the manifest and schema load among it. Which stage holds it is `0109-F2`'s to measure (§ 5 row 1).
- **Against the previous finding** the shape did not move: linear in both, with the same allocation slope (310 KB) and time slopes of 0.41 to 0.43 ms here against 0.42 to 0.43 ms there. The previous finding is the run of the static entry point on 2026-10-11, before item `0107` merged. Allocation is lower at every size, by 0.68, 1.31, 0.14 and 0.68 KB, at most 0.09% of a call. The means at 10, 100 and 1000 specifications agree within the two runs' `Error` taken together: 0.1% lower, 1.8% lower and 1.5% higher. At 1 specification the mean is 3.0% higher, 0.087 ms against a combined `Error` of 0.087 ms; earlier runs of unchanged code on this machine differed by 5% to 6% at that size, so the move from the static entry point to the resolved instance shows no change in time beyond run-to-run noise. This run removed 10 outliers at 100 specifications and none at the other sizes.
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
