---
title: "Specification: Benchmark harness"
description: "The benchmark project, its Benchmark build target and CI step, the shared configuration that measures memory on every benchmark and writes JSON and Markdown results under .artifacts/benchmarks, and the synthetic tree generator the measuring Features share - reporting only, never a gate"
type: feature
id: "F1"
epic: "0109"
spec_status: draft
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
depends_on: []
blocks: ["F2", "F3", "F4"]
spikes: []
created: "2026-10-09"
updated: "2026-10-09"
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

| ID  | Assumption                                                                                                                                                                                                                                           |
| --- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | `0055-F2` is amended to admit the benchmark step: its B-004 lists the gates each run runs and its C-3 allows only package restore and the coverage upload to leave the runner. The amendment is owed by `spec-author` before B-017 to B-020 are cut. |
| A-2 | `0055-F2` B-012 holds unchanged: a pull request that changes only Markdown outside a specification folder runs no target, the benchmark target included.                                                                                             |
| A-3 | The tool package `0055-F1`'s `Pack` target writes is the one `0109-F4` installs; this Feature only orders `Pack` first (B-013).                                                                                                                      |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-09 -->

| ID    | Claim                                                                                                                               | Source                                   | Status |
| ----- | ----------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------- | ------ |
| B-001 | Given the solution is compiled, this Feature's benchmark project is compiled with it.                                               | owner, 2026-10-09; C-2                   | Active |
| B-002 | Given the default build runs, this Feature runs no benchmark.                                                                       | owner, 2026-10-09; decision 0001         | Active |
| B-003 | Given the benchmark target runs with no filter, this Feature runs every benchmark in the project.                                   | decision 0002                            | Active |
| B-004 | Given the benchmark target runs with a filter, this Feature runs only the benchmarks whose full names match it.                     | decision 0002; C-5                       | Active |
| B-005 | Given the benchmark target runs with a job named, this Feature runs every selected benchmark under that job.                        | decision 0002                            | Active |
| B-006 | Given the benchmark target runs with no job named, this Feature runs every selected benchmark under the library's default job.      | decision 0002                            | Active |
| B-007 | Given any benchmark runs, this Feature reports the managed memory it allocates per operation.                                       | owner, 2026-10-09; C-4                   | Active |
| B-008 | Given a run, this Feature writes its results under `.artifacts/benchmarks/` at the repository root.                                 | decision 0002                            | Active |
| B-009 | Given a run, this Feature writes a human-readable summary for each benchmark class it ran.                                          | decision 0002                            | Active |
| B-010 | Given a run, this Feature writes a machine-readable result for each benchmark class it ran.                                         | decision 0002; OQ-4                      | Active |
| B-011 | Given every selected benchmark completes, the benchmark target succeeds whatever times and allocations it measured.                 | owner, 2026-10-09; decision 0001; C-1    | Active |
| B-012 | Given a selected benchmark throws, the benchmark target fails.                                                                      | OQ-3                                     | Active |
| B-013 | Given the benchmark target runs, the tool package is packed before any benchmark runs.                                              | `0109-F4` B-001; A-3                     | Active |
| B-014 | Given a run, this Feature leaves the repository's working tree unchanged outside `.artifacts/`.                                     | brief § 9; C-10                          | Active |
| B-015 | Given a generated tree of any size, the check reports no violation against it.                                                      | decision 0004                            | Active |
| B-016 | Given the same size twice, this Feature generates the same files at the same relative paths with the same contents.                 | brief § 9; decision 0004; C-8            | Active |
| B-017 | Given a CI run that builds, this Feature runs the benchmark target on each operating system the run covers.                         | owner, 2026-10-09; OQ-2; `0055-F2` B-003 | Active |
| B-018 | Given a CI run, this Feature runs the benchmarks under the short job.                                                               | OQ-1                                     | Active |
| B-019 | Given a CI run's benchmark target completes, this Feature publishes its results as a build artifact named for the operating system. | owner, 2026-10-09; C-13                  | Active |
| B-020 | Given a CI run, the benchmark step runs after the format, compile, test, self-check and pack steps.                                 | decision 0001; C-12                      | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-09 -->

| ID   | Constraint                                                                                                                                                                     | Rules Out                                                                                                                                                         |
| ---- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| C-1  | No build target, CI step or test fails on a measured time or allocation (owner, 2026-10-09).                                                                                   | A threshold, an assertion or a regression check on any number; a CI step whose outcome reads a result; a `Ratio` compared against a limit.                        |
| C-2  | The benchmark project is `.performance/specht.benchmarks`, listed in `specht.slnx`, not packable, and not a test project.                                                      | A project under `test/`; a `.Tests` name the tier filters select; a `Tier` trait on a benchmark; packing it; a project `Compile` does not build.                  |
| C-3  | The benchmark library's version is written once, in `Directory.Packages.props`.                                                                                                | A version in the benchmark `.csproj`.                                                                                                                             |
| C-4  | Allocated managed memory per operation is a reported finding of every benchmark, switched on by the shared configuration; it scales with whatever input that benchmark varies. | A benchmark without an allocation column; memory as a per-class opt-in; an allocation threshold.                                                                  |
| C-5  | One entry point selects benchmarks by full name from the command line.                                                                                                         | A `Main` or a runner per benchmark class; a benchmark reachable only by editing code.                                                                             |
| C-6  | A recorded finding comes from a Release build, run by the default out-of-process toolchain, with no debugger attached.                                                         | Recording an in-process, Debug or debugger-attached number in a § 8 `### Performance` row; the benchmark target passing the in-process toolchain (decision 0003). |
| C-7  | Every specification a fixture holds is generated by the benchmark code; only the shipped schema and manifest are copied.                                                       | A fixture copied from this repository's `.spec/` tree or any consumer's; reading `hooked` source.                                                                 |
| C-8  | A generated tree is a function of its size alone.                                                                                                                              | Random content, a clock, a GUID or a machine name in any generated file; a size chosen at run time.                                                               |
| C-9  | Every path a benchmark writes into a fixture, a parameter, a benchmark name or a finding is relative to the tree's root or the repository root.                                | An absolute path in any of those; a path as a parameter value.                                                                                                    |
| C-10 | A run writes only into its own temporary directory and `.artifacts/benchmarks/`.                                                                                               | A write into the repository's tree, a consumer's tree, the user profile or a shared cache.                                                                        |
| C-11 | A run is offline beyond the build's package and tool restore.                                                                                                                  | A benchmark that touches the network, a feed or a live provider.                                                                                                  |
| C-12 | The CI benchmark step is generated by the NUKE build, and its regeneration is committed with its step order diffed.                                                            | A hand edit to `.github/workflows/ci.yml`; a step order nobody checked (`0055-F2` C-1).                                                                           |
| C-13 | The published build artifact holds the exporters' result files only.                                                                                                           | Publishing the library's run log, which carries absolute paths and machine detail; publishing the generated benchmark projects.                                   |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-09 -->

| #   | Item                                                              | Exclusion Reason                                                                                                      |
| --- | ----------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------- |
| 1   | Any gate on a time or an allocation                               | Owner, 2026-10-09; decision 0001; C-1.                                                                                |
| 2   | Comparing a run with a stored baseline                            | OQ-4: a later epic if asked for; B-010's machine-readable result is what it would read.                               |
| 3   | What any one benchmark measures                                   | `0109-F2`, `0109-F3`, `0109-F4`; this Feature runs and reports them.                                                  |
| 4   | Profiling and tracing                                             | Not asked for (epic `0109` § Out of this epic).                                                                       |
| 5   | Committing a result, or a finding's raw output, to the repository | `.artifacts/` is gitignored; a finding goes in a § 8 `### Performance` row, owned by `benchmarker`.                   |
| 6   | Scheduled or nightly benchmark runs                               | Not asked for; a benchmark run follows a CI run (`0055-F2` § 5 row 6).                                                |
| 7   | A tree with violations                                            | Decision 0004: the generated tree is clean; a violation-heavy tree is a later amendment if a constraint asks for one. |

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

| Claim ID | Scenario                                           | Test    | Status  |
| -------- | -------------------------------------------------- | ------- | ------- |
| B-001    | The benchmarks are compiled with the solution      | Missing | Missing |
| B-002    | The default build runs no benchmark                | Missing | Missing |
| B-003    | Every benchmark runs when none is named            | Missing | Missing |
| B-004    | A filter runs only the benchmarks it names         | Missing | Missing |
| B-005    | A named job is the job every benchmark runs under  | Missing | Missing |
| B-006    | With no job named, the default job runs            | Missing | Missing |
| B-007    | Every benchmark reports the memory it allocates    | Missing | Missing |
| B-008    | Results land in the benchmark results folder       | Missing | Missing |
| B-009    | A person can read each class's results             | Missing | Missing |
| B-010    | A machine can read each class's results            | Missing | Missing |
| B-011    | A slower result does not fail the build            | Missing | Missing |
| B-012    | A benchmark that breaks fails the target           | Missing | Missing |
| B-013    | The tool is packed before the benchmarks run       | Missing | Missing |
| B-014    | A run leaves the repository as it found it         | Missing | Missing |
| B-015    | A generated tree passes the check                  | Missing | Missing |
| B-016    | The same size generates the same tree              | Missing | Missing |
| B-017    | CI runs the benchmarks on every operating system   | Missing | Missing |
| B-018    | CI uses the short job                              | Missing | Missing |
| B-019    | CI publishes the results for each operating system | Missing | Missing |
| B-020    | The benchmarks run after every gate                | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-09 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-09 -->

| ID   | Question                                                                                                                                                                                                                                                                                                                                                               | Blocks    | Resolution |
| ---- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------- | ---------- |
| OQ-1 | Which job does a CI run use? The default job gives the most stable numbers and costs the most runner minutes per leg; the short job is a few iterations and a usable record; the dry job only proves a benchmark runs and publishes no usable number. A shared runner's timings are a record, not a comparison, whichever is chosen. Default: the short job.           | B-018     | Open       |
| OQ-2 | Which CI runs run the benchmarks - every run that builds, on both operating systems, or only a push to `main`? The first gives every pull request its numbers and adds the benchmark time to every check; the second keeps pull requests fast and records `main` only. Default: every run that builds, on both operating systems, following `0055-F2` B-003 and B-004. | B-017     | Open       |
| OQ-3 | Does a benchmark that throws fail the benchmark target, and so the CI check? A throw is broken code, not a measurement, so C-1 does not cover it; failing it keeps a benchmark from rotting unseen, at the cost of a red check that no gate target caused. Default: it fails.                                                                                          | B-012     | Open       |
| OQ-4 | Is comparing a run against a stored baseline a later epic, or part of this one? Comparison needs a stored result per environment, and a number from a shared runner does not transfer between runs. Default: a later epic, if asked for; B-010's machine-readable result is what it would read.                                                                        | § 5 row 2 | Open       |

## 12. Sign-off

<!-- last written by: spec-author, 2026-10-09 -->

| Section | Status | Reviewer      | Note                   |
| ------- | ------ | ------------- | ---------------------- |
| 1-5     | 🟡     | spec-reviewer | Awaiting first review. |

## Tasks

None yet. Cut from § 3 after agreement.

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
