---
title: "Specification: Benchmark harness"
description: "The benchmark project, its Benchmark build target and CI step, the shared configuration that measures memory on every benchmark and writes JSON and Markdown results under .artifacts/benchmarks, and the synthetic tree generator the measuring Features share - reporting only, never a gate"
type: feature
id: "F1"
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
depends_on: ["0055/F1", "0055/F2"]
blocks: ["F2", "F3", "F4"]
spikes: []
created: "2026-10-09"
updated: "2026-10-11"
github_issue: null
synced_at: null
---

# Specification: Benchmark harness

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-09 -->

No cost the tool incurs is measured, so every specification constraint about time, scaling or allocation is unverifiable, and a design chosen for its cost is never checked against it. This Feature removes that failure state: the repository has one benchmark project that any developer runs with one build target and CI runs on every build, with memory measured on every benchmark and the results kept as files a person and a machine can read - and nothing it measures ever fails a build.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-09 -->

| #   | Persona                             | Need                                                                       | Pain Point Today                                               |
| --- | ----------------------------------- | -------------------------------------------------------------------------- | -------------------------------------------------------------- |
| 1   | The maintainer of four repositories | See what the tool costs, run to run, without running anything by hand      | No number exists; cost is noticed only when a hook feels slow  |
| 2   | `benchmarker`                       | One project, one configuration and one fixture to measure a concern with   | Nowhere to put a benchmark, and no synthetic tree beyond tests |
| 3   | The developer changing the engine   | Run only the benchmarks that touch the change, locally, with one command   | No target exists                                               |
| 4   | The reviewer of a pull request      | Read the run's results beside its checks, without the results failing it   | A build artifact holds only what a gate produced               |
| 5   | `0109-F2`, `0109-F3`, `0109-F4`     | A generated tree of a known size, and the packed tool, before they measure | N/A (internal dependency)                                      |

### Assumptions

| ID  | Assumption                                                                                                                                                                                                                                                                                                                             |
| --- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | Satisfied 2026-10-09: `0055-F2` B-004 runs the `Benchmark` target on every run that builds, and its C-3 admits uploading the benchmark results as a build artifact; B-017 to B-020 rest on both.                                                                                                                                       |
| A-2 | `0055-F2` B-012 holds unchanged: a pull request that changes only Markdown outside a specification folder runs no target, the benchmark target included.                                                                                                                                                                               |
| A-3 | The tool package `0055-F1`'s `Pack` target writes is the one `0109-F4` installs; this Feature only packs it first, and only when `0109-F4`'s benchmark is selected (B-013, B-021). The baseline tool `0109-F4` B-007 measures is generated, packed and installed by the benchmark code, never by the benchmark target (`0109-F4` C-8). |
| A-4 | `0055-F2` B-015 holds unchanged: a pull request runs no target on windows, the benchmark target included. On a pull request the windows leg is not a run that builds (`0055-F2` B-004), so the run B-017 names covers ubuntu alone there.                                                                                              |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-09 -->

| ID    | Claim                                                                                                                                         | Source                                                                                                                       | Status |
| ----- | --------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------- | ------ |
| B-001 | Given the solution is compiled, this Feature's benchmark project is compiled with it.                                                         | owner, 2026-10-09; C-2                                                                                                       | Active |
| B-002 | Given the default build runs, this Feature runs no benchmark.                                                                                 | owner, 2026-10-09; decision 0001                                                                                             | Active |
| B-003 | Given the benchmark target runs with no filter, this Feature runs every benchmark in the project.                                             | decision 0002                                                                                                                | Active |
| B-004 | Given the benchmark target runs with a filter, this Feature runs only the benchmarks whose full names match it.                               | decision 0002; C-5                                                                                                           | Active |
| B-005 | Given the benchmark target runs with a job named, this Feature runs every selected benchmark under that job.                                  | decision 0002                                                                                                                | Active |
| B-006 | Given the benchmark target runs with no job named, this Feature runs every selected benchmark under the library's default job.                | decision 0002                                                                                                                | Active |
| B-007 | Given any benchmark runs, this Feature reports the managed memory it allocates per operation.                                                 | owner, 2026-10-09; C-4                                                                                                       | Active |
| B-008 | Given a run, this Feature writes its results under `.artifacts/benchmarks/` at the repository root.                                           | decision 0002                                                                                                                | Active |
| B-009 | Given a run, this Feature writes a human-readable summary for each benchmark class it ran.                                                    | decision 0002                                                                                                                | Active |
| B-010 | Given a run, this Feature writes a machine-readable result for each benchmark class it ran.                                                   | decision 0002; OQ-4 (owner, 2026-10-09)                                                                                      | Active |
| B-011 | Given every selected benchmark completes, this Feature reports the benchmark target as succeeded, whatever times and allocations it measured. | owner, 2026-10-09; decision 0001; C-1                                                                                        | Active |
| B-012 | Given a selected benchmark throws, this Feature reports the benchmark target as failed.                                                       | OQ-3 (owner, 2026-10-09)                                                                                                     | Active |
| B-013 | Given the benchmark target runs with a cold-start benchmark selected, this Feature packs the tool package before any benchmark runs.          | `0109-F4` B-001; A-3; decision 0002                                                                                          | Active |
| B-014 | Given a run, this Feature leaves the repository's working tree unchanged outside `.artifacts/`.                                               | brief § 9; C-10                                                                                                              | Active |
| B-015 | Given a size of 1, 10, 100 or 1000 specifications, this Feature generates a tree on which the check reports no violation.                     | decision 0004                                                                                                                | Active |
| B-016 | Given the same size twice, this Feature generates the same files at the same relative paths with the same contents.                           | owner, 2026-10-09 (item `0108`; `specht-conventions` references/benchmarking.md); AGENTS.md § Invariants; decision 0004; C-8 | Active |
| B-017 | Given a CI run that builds, this Feature runs the benchmark target on each operating system the run covers.                                   | owner, 2026-10-09; OQ-2 (owner, 2026-10-09); `0055-F2` B-003, B-004, B-015; A-2; A-4                                         | Active |
| B-018 | Given a CI run, this Feature runs the benchmarks under the short job.                                                                         | OQ-1 (owner, 2026-10-09)                                                                                                     | Active |
| B-019 | Given a CI run's benchmark target completes, this Feature publishes its results as a build artifact named for the operating system.           | owner, 2026-10-09; C-13                                                                                                      | Active |
| B-020 | Given a CI run, this Feature runs the benchmark step after the format, compile, test and self-check steps.                                    | decision 0001; C-12                                                                                                          | Active |
| B-021 | Given the benchmark target runs with no cold-start benchmark selected, this Feature packs nothing.                                            | decision 0002; B-013                                                                                                         | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-09 -->

| ID   | Constraint                                                                                                                                                                            | Rules Out                                                                                                                                                         |
| ---- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| C-1  | No build target, CI step or test fails on a measured time or allocation (owner, 2026-10-09).                                                                                          | A threshold, an assertion or a regression check on any number; a CI step whose outcome reads a result; a `Ratio` compared against a limit.                        |
| C-2  | The benchmark project is `.performance/specht.benchmarks`, listed in `specht.slnx`, not packable, and not a test project.                                                             | A project under `test/`; a `.Tests` name the tier filters select; a `Tier` trait on a benchmark; packing it; a project `Compile` does not build.                  |
| C-3  | The benchmark library's version is written once, in `Directory.Packages.props`.                                                                                                       | A version in the benchmark `.csproj`.                                                                                                                             |
| C-4  | Allocated managed memory per operation is a reported finding of every benchmark, switched on by the shared configuration, and reported at every parameter value the benchmark varies. | A benchmark without an allocation column; memory as a per-class opt-in; an allocation threshold.                                                                  |
| C-5  | One entry point selects benchmarks by full name from the command line.                                                                                                                | A `Main` or a runner per benchmark class; a benchmark reachable only by editing code.                                                                             |
| C-6  | A recorded finding comes from a Release build, run by the default out-of-process toolchain, with no debugger attached.                                                                | Recording an in-process, Debug or debugger-attached number in a § 8 `### Performance` row; the benchmark target passing the in-process toolchain (decision 0003). |
| C-7  | Every specification a fixture holds is generated by the benchmark code; only the shipped schema and manifest are copied.                                                              | A fixture copied from this repository's `.spec/` tree or any consumer's; reading `hooked` source.                                                                 |
| C-8  | A generated tree is a function of its size alone.                                                                                                                                     | Random content, a clock, a GUID or a machine name in any generated file; a size chosen at run time.                                                               |
| C-9  | Every path a benchmark writes into a fixture, a parameter, a benchmark name or a finding is relative to the tree's root or the repository root.                                       | An absolute path in any of those; a path as a parameter value.                                                                                                    |
| C-10 | A run writes only into its own temporary directory, `.artifacts/benchmarks/`, and the package output `Pack` writes under `.artifacts/nupkg/` (B-013).                                 | A write into the repository's tree outside `.artifacts/`, a consumer's tree, the user profile or a shared cache.                                                  |
| C-11 | A run is offline beyond the build's package and tool restore.                                                                                                                         | A benchmark that touches the network, a feed or a live provider.                                                                                                  |
| C-12 | The CI benchmark step is generated by the NUKE build, and its regeneration is committed with its step order diffed.                                                                   | A hand edit to `.github/workflows/ci.yml`; a step order nobody checked (`0055-F2` C-1).                                                                           |
| C-13 | The published build artifact holds the exporters' result files only.                                                                                                                  | Publishing the library's run log, which carries absolute paths and machine detail; publishing the generated benchmark projects.                                   |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-09 -->

| #   | Item                                                              | Exclusion Reason                                                                                                                |
| --- | ----------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------- |
| 1   | Any gate on a time or an allocation                               | Owner, 2026-10-09; decision 0001; C-1.                                                                                          |
| 2   | Comparing a run with a stored baseline                            | A later Feature of epic `0109`, to be defined (owner, 2026-10-09; OQ-4); B-010's machine-readable result is the input it reads. |
| 3   | What any one benchmark measures                                   | `0109-F2`, `0109-F3`, `0109-F4`; this Feature runs and reports them.                                                            |
| 4   | Profiling and tracing                                             | Not asked for (epic `0109` § Out of this epic).                                                                                 |
| 5   | Committing a result, or a finding's raw output, to the repository | `.artifacts/` is gitignored; a finding goes in a § 8 `### Performance` row, owned by `benchmarker`.                             |
| 6   | Scheduled or nightly benchmark runs                               | Not asked for; a benchmark run follows a CI run (`0055-F2` § 5 row 6).                                                          |
| 7   | A tree with violations                                            | Decision 0004: the generated tree is clean; a violation-heavy tree is a later amendment if a constraint asks for one.           |

## 6. Concern Separation

<!-- last written by: spec-author, 2026-10-09 -->

Pending: owned by `implementer`, written after agreement.

## 7. Technical Design

<!-- last written by: spec-author, 2026-10-09 -->

Pending: owned by `implementer`, written after agreement.

## 8. Testing Strategy

<!-- last written by: spec-author, 2026-10-09 -->

Pending: owned by `test-writer`, written after agreement.

## 9. Traceability Matrix

<!-- last written by: spec-author, 2026-10-09 -->

| Claim ID | Scenario                                               | Test    | Status  |
| -------- | ------------------------------------------------------ | ------- | ------- |
| B-001    | The benchmarks are compiled with the solution          | Missing | Missing |
| B-002    | The default build runs no benchmark                    | Missing | Missing |
| B-003    | Every benchmark runs when none is named                | Missing | Missing |
| B-004    | A filter runs only the benchmarks it names             | Missing | Missing |
| B-005    | A named job is the job every benchmark runs under      | Missing | Missing |
| B-006    | With no job named, the default job runs                | Missing | Missing |
| B-007    | Every benchmark reports the memory it allocates        | Missing | Missing |
| B-008    | Results land in the benchmark results folder           | Missing | Missing |
| B-009    | A person can read each class's results                 | Missing | Missing |
| B-010    | A machine can read each class's results                | Missing | Missing |
| B-011    | Whatever a benchmark measures, the build does not fail | Missing | Missing |
| B-012    | A benchmark that breaks fails the target               | Missing | Missing |
| B-013    | The tool is packed before the benchmarks run           | Missing | Missing |
| B-014    | A run leaves the repository as it found it             | Missing | Missing |
| B-015    | A generated tree passes the check                      | Missing | Missing |
| B-016    | The same size generates the same tree                  | Missing | Missing |
| B-017    | CI runs the benchmarks on every operating system       | Missing | Missing |
| B-018    | CI uses the short job                                  | Missing | Missing |
| B-019    | CI publishes the results for each operating system     | Missing | Missing |
| B-020    | The benchmarks run after every gate                    | Missing | Missing |
| B-021    | A run without the cold-start benchmark packs nothing   | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-11 -->

- 2026-10-09, spec-reviewer finding on commit `4b23e76`: B-020 ordered the CI step after a pack step CI does not run; dropped. B-013 packed on every run; it now packs only when a cold-start benchmark is selected, and B-021 states the other case (decision 0002). B-011, B-012, B-013, B-015 and B-020 restated with this Feature as subject; B-015 names decision 0004's sizes; B-016's source corrected from brief § 9 to the owner's decision and AGENTS.md § Invariants; A-1 quotes `0055-F2` C-3 exactly; C-4 restated as an exclusion.
- 2026-10-09, the owner resolved OQ-1 to OQ-4. OQ-1 to OQ-3 keep their defaults, so B-018, B-017 and B-012 stand. OQ-4 places baseline comparison in this epic as a later Feature, to be defined; § 5 row 2 names it. A-1 is satisfied: `0055-F2` B-004 and C-3 are amended in the same change, and `depends_on` gains `0055/F1` and `0055/F2`. B-013 was briefly amended to pack the baseline package `0109-F4` B-007 measures, and that amendment is withdrawn before commit on a spec-reviewer finding (one owner for packing): the benchmark code generates, packs and installs the baseline tool in its own temporary directory (`0109-F4` C-8), and the benchmark target packs only the tool package (orchestrator default, 2026-10-09, not the owner's). B-013, B-021 and their scenarios read as before; A-3 names the split. B-020 stands: `0055-F2` B-004 listed a `Pack` step that CI does not run, and is corrected there.
- 2026-10-09, spec-reviewer round 2: C-10 widened to the package output `Pack` writes under `.artifacts/nupkg/`, which B-013 already caused; every write stays inside `.artifacts/`, as B-014 holds.
- 2026-10-09, spec-reviewer finding against `0055-F2` B-015 (owner, 2026-10-09; `0055-F2` decision 0003; item 0118): a pull request runs no target on windows, and the benchmark target is a target. A-4 records B-015 beside A-2, and B-017's Source cites it. B-017 and the owner's OQ-2 answer are unchanged: the windows leg of a pull request is not a run that builds, so neither asks for a benchmark there. The `@B-017` scenario, which ran a pull request's benchmarks on Linux and on Windows, is now a push to `main`, with a `@boundary` scenario for a pull request.
- 2026-10-11, the owner decided how this Feature is delivered, and items `0145` to `0149` are cut. No claim changed. Scope: the check measured end to end (`0109-F3`, item `0150`) is the first thing covered, because it is the first feature shipping, and work stops once it is covered; `0146`, `0147` and `0150` are delivered now, and `0148` and `0149` are cut and left in the queue. Order: `0109-F3`'s benchmark is the first measuring item and is delivered before the `Benchmark` target (`0148`), because with no benchmark class the harness selects nothing and cannot be observed, and `benchmarker` refuses a placeholder. Proof: B-015 and B-016 are tested; the build-target and CI claims are proven as `0055-F1` and `0055-F2` are under the owner's 2026-10-08 ruling that the build and CI carry no tests - `Missing` in § 9, with the mechanism recorded in § 8 - and `benchmark-harness.feature` is not linked into `test/acceptance`. Score: this Feature carries value 3, authored on `0145`, and is not on the `.issue/.goal` path.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-09 -->

| ID   | Question                                                                                                                                                                                                                                                                                                                                                               | Blocks    | Resolution                                                                                                                                                        |
| ---- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | Which job does a CI run use? The default job gives the most stable numbers and costs the most runner minutes per leg; the short job is a few iterations and a usable record; the dry job only proves a benchmark runs and publishes no usable number. A shared runner's timings are a record, not a comparison, whichever is chosen. Default: the short job.           | B-018     | Resolved 2026-10-09: the short job; B-018 stands (owner).                                                                                                         |
| OQ-2 | Which CI runs run the benchmarks - every run that builds, on both operating systems, or only a push to `main`? The first gives every pull request its numbers and adds the benchmark time to every check; the second keeps pull requests fast and records `main` only. Default: every run that builds, on both operating systems, following `0055-F2` B-003 and B-004. | B-017     | Resolved 2026-10-09: every run that builds, on both operating systems; B-017 stands (owner).                                                                      |
| OQ-3 | Does a benchmark that throws fail the benchmark target, and so the CI check? A throw is broken code, not a measurement, so C-1 does not cover it; failing it keeps a benchmark from rotting unseen, at the cost of a red check that no gate target caused. Default: it fails.                                                                                          | B-012     | Resolved 2026-10-09: a throw fails the benchmark target, and so the check; B-012 and `0109-F4` B-004 stand (owner).                                               |
| OQ-4 | Is comparing a run against a stored baseline a later epic, or part of this one? Comparison needs a stored result per environment, and a number from a shared runner does not transfer between runs. Default: a later epic, if asked for; B-010's machine-readable result is what it would read.                                                                        | § 5 row 2 | Resolved 2026-10-09: part of this epic, as a later Feature to be defined that reports and never gates (decision 0001), reading B-010's result; § 5 row 2 (owner). |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-09 -->

| Section | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     |
| ------- | ------ | ------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-5     | 🟡     | spec-reviewer | Awaiting first review.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |
| 1-5     | 🔴     | spec-reviewer | Round 1 (2026-10-09) - blocked, reviewing the uncommitted OQ answers on `cdb0876`. OQ-1 to OQ-4 are resolved in place, none deleted; B-017, B-018 and B-012 stand on the owner's answers; § 5 row 2 and the epic place baseline comparison in this epic with no Feature id claimed; `depends_on` and `0055-F1`/`0055-F2` `blocks` are symmetric (no new `SPEC051`); § 9 and every `@B` tag resolve. Blocking (spec-author): amended B-013 has this Feature's target pack the baseline package before any benchmark runs, but A-3 says the benchmark code generates it, not `Pack`, and `0109-F4` C-8 has the benchmark generate, pack and install it inside its temporary directory - where the target cannot reach it before a benchmark runs, and where C-10 confines it anyway; state one owner of the baseline's packing and align B-013, B-021 and their scenarios, A-3, § 2 row 5, decision 0002's Packing bullet and `0109-F4` A-1 and C-8 to it. |
| 1-5     | 🟢     | spec-reviewer | Round 2 (2026-10-09) - approved, reviewing the revised uncommitted diff on `cdb0876`. Round 1's blocker is met: B-013, B-021, their scenarios, § 2 row 5 and decision 0002 read as on `cdb0876`, packing only the tool package; A-3 gives the baseline tool to the benchmark code, matching `0109-F4` C-8, and § 10 marks the split as the orchestrator's default, not the owner's. `0055-F2` B-004 and C-3 carry the amendment A-1 names, and the item note under Tasks ties B-017 to B-004's `Benchmark` half. Non-blocking (spec-author, existing before this diff): C-10 confines a run's writes to its temporary directory and `.artifacts/benchmarks/`, but B-013 runs `Pack`, which writes `.artifacts/nupkg/`, and B-014 allows anything under `.artifacts/`; widen C-10 to the package output, or say B-013's `Pack` is outside it.                                                                                                             |

## Tasks

Cut 2026-10-11 into [`../.issue/`](../.issue/): `0145` (the Feature), with `0146` to `0149`. `0149`, which delivers B-017, also closes the `Benchmark` half of `0055-F2` B-004.

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
