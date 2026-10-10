---
title: "Epic 0109: Benchmarks"
description: "BenchmarkDotNet measurements of the engine's stages, of a full check as the tree grows, and of the packed tool's cold start, with memory measured on every benchmark - run locally and in CI, published as a build artifact, and never a gate"
id: "0109"
type: epic
status: ready-for-architecture
priority: med
milestone: null
children: ["0109-F1", "0109-F2", "0109-F3", "0109-F4"]
created: "2026-10-09"
updated: "2026-10-09"
github_issue: null
---

# Epic 0109: Benchmarks

## Summary

Nothing measures what the tool costs. ADR-0005 redraws the engine as typed
stages, ADR-0004 trades a decoration for a map lookup, and the tool runs at
three call sites, one of them a pre-commit hook a person waits on (brief § 2
Must-1) - and every claim about cost, scaling or allocation is an opinion,
because no number exists to cite.

This epic adds a benchmark project, `.performance/specht.benchmarks`, built
on [BenchmarkDotNet](https://benchmarkdotnet.org/). It measures each engine
stage, a full check as the tree grows, and the packed tool's cold start, and
it measures memory on every benchmark. It runs locally through a build target
and in CI, where the results are published as a build artifact. Nothing it
measures fails a build.

The need and its scope were decided by the repository owner on 2026-10-09
(item `0108`, which added the `benchmarker` role and the `benchmarkdotnet`
skill); the decisions are recorded beside the Feature they bind (`0109-F1`
decision 0001).

## Business Value

Without this epic, a design choice made for cost - a stage boundary, a map in
place of a decorator, a materialised list - is never checked against the cost
it was made for, and a change that doubles the time of a hook run is found by
the person waiting on it. With it delivered, every cost concern a
specification states has a benchmark and a recorded finding, findings are
comparable run to run on one machine, and a regression is something the
`benchmarker` reports from a number, not something a user feels.

## Features

Decomposed by capability dimension. Each Feature measures a different cost on
a different input, and the cut is made where the invariant changes.

| Feature   | Name                     | Specification                                            | Invariant                                                                     |
| --------- | ------------------------ | -------------------------------------------------------- | ----------------------------------------------------------------------------- |
| `0109-F1` | Benchmark harness        | `.performance/specht.benchmarks/.spec/README.md`         | Benchmarks run, report memory and publish results; nothing gates on a number  |
| `0109-F2` | Engine stage benchmarks  | `.performance/specht.benchmarks/Stages/.spec/README.md`  | Each stage's cost is measured alone, its setup outside the measurement        |
| `0109-F3` | End-to-end check scaling | `.performance/specht.benchmarks/Check/.spec/README.md`   | A whole check is measured against tree size, so its growth shape is a finding |
| `0109-F4` | Tool cold start          | `.performance/specht.benchmarks/Startup/.spec/README.md` | The packed tool is measured as a consumer runs it, one fresh process each     |

The starting hypothesis was confirmed unchanged. Where it was tested:

- **Allocation is not a Feature.** The owner put memory on every benchmark;
  it is a property of the harness's shared configuration (`0109-F1` B-007,
  C-4), which every other Feature inherits. A Feature of its own would have no
  input and no benchmark that is not already another Feature's.
- **Stages and the end-to-end check: split.** A stage benchmark isolates one
  stage's call, its setup outside the measurement; the scaling benchmark
  measures the engine's whole run from a root on disk. Each survives
  the other being cut, and they answer different questions - where the time
  goes, and how it grows.
- **Cold start: split from the end-to-end check.** It needs the packed
  package, a process boundary and the out-of-process toolchain, and its cost
  is fixed per invocation rather than a function of the tree.
- **The harness and its CI step: kept together.** The CI step is a call site
  of the harness's build target with no behaviour of its own; cut from the
  harness it has nothing to run.
- **The "&" heuristic** found no conjunction in any Feature name.

Baseline comparison - a later Feature of this epic, to be defined (owner,
2026-10-09; `0109-F1` OQ-4), comparing a run against a stored result; not
`0109-F4`'s in-run baseline row. Like every Feature here it reports and never
gates (`0109-F1` decision 0001), and it reads `0109-F1` B-010's
machine-readable result. It takes a Feature id when it is specified, and
joins `children` then, so that every child resolves to a file.

`0109-F2`, `0109-F3` and `0109-F4` depend on `0109-F1`. `0109-F1` depends on
`0055-F1` (the build it adds a target to, whose `Pack` `0109-F4` measures) and
`0055-F2` (the CI workflow it adds a step to). `0109-F2` measures the stages
ADR-0005 names through whatever entry points `0001-F1`'s items have landed
(`0109-F2` A-1), so it waits on no item. The two cross-epic edges landed on
2026-10-09, on the owner's word: `0109-F1`'s `depends_on` names `0055/F1` and
`0055/F2`, and each of those names `0109/F1` in its `blocks`, so `SPEC051`'s
symmetry holds. The same change amended `0055-F2` B-004 and C-3 to admit the
benchmark step (`0109-F1` A-1).

## Placement

Every specification sits in the benchmark project the owner placed at
`.performance/specht.benchmarks`, outside `test/` and its tier governance
(`specht-conventions` § Benchmarking). The harness's specification is the
project's root; each measuring Feature has the folder its benchmark classes
will occupy. The `implementer` may move a specification with its code
(`git mv`): a specification's identity is its frontmatter `epic` and `id`
(`SPEC012`).

This epic file lives at `epics/0109-benchmarks/epic.md`, where schema version
1's epic glob `epics/**/epic.md` discovers it (`0001-F6` decision 0001).

## Status

`ready-for-architecture`: every Feature has its agreement half - § 1-5 and its
`.feature` - and the next owner is the `implementer`, for § 6 and § 7. The
owner answered every open question on 2026-10-09, and none remains.

## Out of this epic

| Item                                                      | Where it lives instead                                                             |
| --------------------------------------------------------- | ---------------------------------------------------------------------------------- |
| A CI gate that fails on a regression or a threshold       | Never: nothing gates on a measurement (owner, 2026-10-09; `0109-F1` decision 0001) |
| Profiling and tracing - where inside a call the time goes | Not asked for; a finding names a cost, not its cause                               |
| The optimisation work a finding prompts                   | The `implementer`, through a claim or an ADR; never inside a benchmark             |
| Benchmarking a consumer's tree, or any captured tree      | Never: fixtures are synthetic (`0109-F1` C-7; owner, item `0108`)                  |
| What each existing build target and CI step does          | `0055-F1`, `0055-F2`; this epic adds one target and one step                       |
