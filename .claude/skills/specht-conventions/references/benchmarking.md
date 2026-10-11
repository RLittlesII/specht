---
title: Benchmarking in specht
description: Where benchmarks live, why they sit outside the test tiers, how a finding is recorded in a specification, where each harness decision is recorded, and what is not built yet
type: reference
---

# Benchmarking

The library and its traps are [`benchmarkdotnet`](../../benchmarkdotnet/SKILL.md);
the role that writes benchmarks is [`benchmarker`](../../../agents/benchmarker.md).
This page says where that lands here.

## Where benchmarks live

`.performance/benchmarks` - a console project of its own, outside `test/`,
decided by the owner on 2026-10-09 (item 0108). The project file is
`benchmarks.csproj` and the assembly `specht.benchmarks`, as `tool` and
`tests` are named (owner, 2026-10-11).

- **Not a test project.** It declares no `IsTestProject` and its name does not
  end `tests`, so `UnitTest` and `IntegrationTest` never select it, and the
  tier trait guard
  (`test/Shared/TestTierGovernanceTests.cs`) scans only the assembly it is
  compiled into, so it never sees a benchmark class. A benchmark carries no
  `Tier` trait; a `[Benchmark]` method is not a test and asserts nothing.
- **Not packable.** `IsPackable=false`, like every project but `src/tool`.
- **In `specht.slnx`,** or `Compile` does not build it and it rots unseen.
- **BenchmarkDotNet's version is central,** in `Directory.Packages.props`.

## What it measures, and from what

- The engine through its injected stages (ADR-0005): discovery, the frontmatter
  reader, the manifest and schema load, the Markdig model, each rule, and the
  runner end to end. Until item 0107 lands the stages are reached through
  today's entry points (`0109-F2` A-1); setup stays in `[GlobalSetup]`, so the
  call measured is the stage's.
- The packed tool's cold start, which only the default out-of-process toolchain
  can measure.
- A tree is built in memory by `SpechtTree.Of` and saved by
  `SpechtTreeStore.Save`, into a temporary directory the benchmark creates and
  deletes, with relative paths throughout
  ([`0109-F1` § 7](../../../../.performance/benchmarks/.spec/README.md)). Never a
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
BenchmarkDotNet output does not. It lands where `--artifacts` names -
`.artifacts/benchmarks/` for the target
([`0109-F1` decision 0002](../../../../.performance/benchmarks/.spec/decisions/0002-one-switcher-entry-json-and-markdown-results-under-artifacts.md)) -
and in BenchmarkDotNet's default `BenchmarkDotNet.Artifacts/` when a run by
hand names none. Both are gitignored - never commit a generated report, the
same rule as `format.json`.

## Nothing gates on time

No build target, CI step or test fails on a measurement. A finding that moved is
escalated by the benchmarker; it is not a red build.

## Decided, and where

- The entry point, the exporters and the results folder:
  [`0109-F1` decision 0002](../../../../.performance/benchmarks/.spec/decisions/0002-one-switcher-entry-json-and-markdown-results-under-artifacts.md).
- Whether a finding may come from the in-process toolchain:
  [`0109-F1` decision 0003](../../../../.performance/benchmarks/.spec/decisions/0003-a-finding-comes-from-the-default-toolchain.md).
- Which job a CI run uses: `0109-F1` OQ-1.
- The project, its entry point and its shared configuration as built:
  [`0109-F1` § 7](../../../../.performance/benchmarks/.spec/README.md).

## Not built yet

- the `Benchmark` NUKE target and its parameter names (item 0148);
- the CI step that runs it and publishes the results, and where that step lands in
  the generated `ci.yml` (item 0149; regenerate with `./build.sh Compile`, then
  diff the step order).
