---
title: Benchmarking in specht
description: Where benchmarks live, why they sit outside the test tiers, how a finding is recorded in a specification, and what is not decided until the benchmark epic is specified
type: reference
---

# Benchmarking

The library and its traps are [`benchmarkdotnet`](../../benchmarkdotnet/SKILL.md);
the role that writes benchmarks is [`benchmarker`](../../../agents/benchmarker.md).
This page says where that lands here.

## Where benchmarks live

`.performance/specht.benchmarks` - a console project of its own, outside `test/`,
decided by the owner on 2026-10-09 (item 0108).

- **Not a test project.** Its name does not end `.Tests`, so `UnitTest` and
  `IntegrationTest` never select it, and the tier trait guard
  (`test/Shared/TestTierGovernanceTests.cs`) scans only the assembly it is
  compiled into, so it never sees a benchmark class. A benchmark carries no
  `Tier` trait; a `[Benchmark]` method is not a test and asserts nothing.
- **Not packable.** `IsPackable=false`, like every project but `src/specht.tool`.
- **In `specht.slnx`,** or `Compile` does not build it and it rots unseen.
- **BenchmarkDotNet's version is central,** in `Directory.Packages.props`.

## What it measures, and from what

- The engine through its injected stages (ADR-0005): discovery, the frontmatter
  reader, the manifest and schema load, the Markdig model, each rule, and the
  runner end to end. Resolve the graph from the container in `[GlobalSetup]`;
  the call measured is the stage's, not the container's.
- The packed tool's cold start, which only the default out-of-process toolchain
  can measure.
- A tree is built with the same synthetic `SpecTree` approach the tests use,
  under a temporary directory, with repository-relative paths throughout. Never a
  copy of this repository's `.spec/` tree, and never `hooked`'s.

## Recording a finding

A finding goes in the Feature's § 8, in a `### Performance` subsection owned by
`benchmarker` (§ "Section ownership" in [SKILL.md](../SKILL.md)). The first
finding for a Feature adds the subsection; `.spec/templates/feature.md` does not
carry it, because the templates are the embedded v1 copy `specht init` writes
into a consumer (`0001-F4` B-004). The subsection carries its own
`<!-- last written by: benchmarker, <date> -->` stamp under its heading; the
benchmarker never rewrites the stamp `test-writer` keeps under § 8's.

```markdown
### Performance

| Concern               | Source   | Benchmark                  | Finding                                 |
| --------------------- | -------- | -------------------------- | --------------------------------------- |
| allocation per spec   | C-n      | `DiscoveryBenchmarks.Walk` | 2.1 KB per spec, linear in tree size    |
| stage resolution cost | ADR-0005 | `RunnerBenchmarks.Resolve` | Not measured - harness has no container |
```

Numbers go in the specification as a finding with its environment; the raw
BenchmarkDotNet output does not. It lands in BenchmarkDotNet's default
`BenchmarkDotNet.Artifacts/` unless the epic's target names another directory;
that directory is gitignored - never commit a generated report, the same rule as
`format.json`.

## Nothing gates on time

No build target, CI step or test fails on a measurement. A finding that moved is
escalated by the benchmarker; it is not a red build.

## Not decided yet

The benchmark epic specifies these; until it does, none of them exists:

- the `Benchmark` NUKE target and its arguments;
- the CI step that runs it and publishes the results, and where that step lands in
  the generated `ci.yml` (regenerate with `./build.sh Compile`, then diff the step
  order);
- which job a CI run uses;
- the entry point (`BenchmarkSwitcher` is the library's choice for more than one
  class);
- which exporters run, and the artifacts directory;
- whether a finding may come from the in-process toolchain.
