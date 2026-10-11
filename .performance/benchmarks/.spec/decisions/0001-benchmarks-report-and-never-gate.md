---
title: "Decision 0001: benchmarks report and never gate"
description: "The owner's 2026-10-09 scope for epic 0109 - BenchmarkDotNet, run locally and in CI with results published as a build artifact, memory on every benchmark, the project at .performance/specht.benchmarks - and the rule that no measurement fails a build; a regression gate, a dry-run-only CI step and a benchmark in the default build were turned down"
type: decision
---

# Decision 0001: benchmarks report and never gate

**Date:** 2026-10-09
**Decided by:** the repository owner (item `0108`, 2026-10-09); recorded by spec-author

## The call

- Benchmarks use BenchmarkDotNet.
- They run locally, through a build target, and in CI, which publishes the
  results as a build artifact.
- Nothing fails a build on a measurement: no threshold, no assertion, no
  comparison against an earlier run.
- They measure the engine's stages, a full check as the tree grows, and the
  packed tool's cold start, and they measure memory on every benchmark.
- The project is `.performance/specht.benchmarks`, outside `test/` and its tier
  governance.

From the owner's call, spec-author derived two placements:

- The default build (`Compile` + `Test`) runs no benchmark (B-002). A benchmark
  run takes minutes and gates nothing, so it does not belong in the command a
  developer runs before every commit; compiling the project there (B-001) is
  what keeps it from rotting.
- In CI the benchmark step runs after every gating step (B-020), so a
  benchmark never delays a gate's verdict or runs against a tree a gate has
  already failed.

## Why

- A timing from a shared CI runner moves with the runner's other load; a gate
  on it fails pull requests that changed nothing (`benchmarkdotnet` skill,
  "Noise"). A finding is evidence for the `benchmarker` to report, not a verdict.
- Allocation is the cost that transfers between machines best, and the one the
  engine's design choices move (ADR-0005's materialised list); measuring it
  everywhere costs one configuration line.
- Outside `test/`, the tier filters never select the project and the tier trait
  guard never scans it (`specht-conventions` § Benchmarking).

## Rejected

**A regression gate** - fail the build when a mean or an allocation moves past
a margin. Turned down by the owner. Cost of rejecting: a regression is found by
reading the published results or by the `benchmarker`, not by a red check.

**A dry run only in CI** - prove each benchmark runs, publish nothing usable.
Contradicts the owner's call that CI publishes results. Cost of rejecting: CI
pays for real iterations (OQ-1 picks how many).

**Benchmarks in the default build.** Every local build would pay the benchmark
time for a result nobody reads. Cost of rejecting: a developer runs the target
by name.

## Affects

- `0109-F1` B-001, B-002, B-011, B-017 to B-020; C-1, C-2, C-4.
- `0109-F2`, `0109-F3`, `0109-F4`: every constraint that names a reported
  finding.
- `0055-F2` B-004 and C-3, which owe an amendment (`0109-F1` A-1).

## Reversal

**Amended 2026-10-11 by the repository owner; recorded by spec-author.** One
part of the call above is redirected, and the sections above stand as written.

- The project is `.performance/benchmarks`, not
  `.performance/specht.benchmarks`. The owner: "instead of `specht.benchmarks`
  it should follos the same convention as `tools` and `tests`" - the folder and
  the project file carry the short name and the assembly carries the `specht.`
  prefix. "`.performance/specht.benchmarks`" in the description and the call
  reads as "`.performance/benchmarks`"; `0109-F1` C-2 is amended to match.
- The Feature's specifications, records and items moved with the project, so
  this record now sits under `.performance/benchmarks/`.

Every other part of the call stands: the project is outside `test/` and its
tier governance, and nothing gates on a measurement.
