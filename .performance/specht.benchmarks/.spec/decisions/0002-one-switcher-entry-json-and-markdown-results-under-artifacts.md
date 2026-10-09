---
title: "Decision 0002: one switcher entry, JSON and Markdown results under .artifacts/benchmarks"
description: "The benchmark project has one BenchmarkSwitcher entry point; the Benchmark target passes a filter and a job through to it; results are a GitHub-flavoured Markdown summary and a full JSON result per class, written to .artifacts/benchmarks; a runner per class, the library's default folder and the HTML, CSV and plot exporters were turned down"
type: decision
---

# Decision 0002: one switcher entry, JSON and Markdown results under `.artifacts/benchmarks`

**Date:** 2026-10-09
**Decided by:** spec-author, settling points `specht-conventions` § Benchmarking
listed as not decided; the owner may reverse it

## The call

- **Entry point.** One `BenchmarkSwitcher` over the benchmark assembly (C-5).
- **The target's arguments.** `Benchmark` takes two optional parameters and
  passes them through: a filter, as the library's `--filter` glob, every
  benchmark when absent (B-003, B-004); and a job, as the library's `--job`,
  the library's default job when absent (B-005, B-006).
- **Packing.** The target packs the tool only when the selection includes a
  cold-start benchmark (`0109-F4`), before any benchmark runs (B-013); a
  run that selects none packs nothing (B-021).
- **Exporters.** The GitHub-flavoured Markdown summary for a person (B-009) and
  the full JSON result for a machine (B-010), one each per benchmark class.
- **Results folder.** `.artifacts/benchmarks/`, passed as the library's
  `--artifacts` (B-008).

## Why

- Three Features add benchmark classes; the switcher is the library's entry
  point for more than one, and the only one that gives `--filter`.
- A pass-through keeps the target from growing its own selection language; the
  library's documented arguments stay the interface.
- Markdown renders in a pull request and in the published artifact as it
  stands. JSON is the form a later comparison would read (OQ-4) and the one
  that carries every statistic, so nothing needs re-running to get a number
  the summary omits.
- `.artifacts/` is already where the build writes everything it generates
  (`.artifacts/nupkg`, `.artifacts/spec-check`) and is already gitignored, so
  nothing new can be committed by mistake.

## Rejected

**Packing on every run.** Simple, and it makes a filtered stage-only run pay
for a package nothing in it uses. Cost of rejecting: the target reads its
own selection to decide.

**`BenchmarkRunner.Run<T>()` per class.** No selection from the command line;
each new class edits the entry point. Cost of rejecting: none.

**The library's default `BenchmarkDotNet.Artifacts/`** at the working
directory. A second output root beside `.artifacts/`, ignored by a separate
`.gitignore` line. Cost of rejecting: the target passes one argument.

**HTML, CSV and the plot exporters.** HTML and CSV restate the Markdown and the
JSON; plots need R on the runner. Cost of rejecting: a reader who wants a
chart builds it from the JSON.

## Affects

- `0109-F1` B-003 to B-006, B-008 to B-010, B-013, B-021; C-5.

## Reversal

None.
