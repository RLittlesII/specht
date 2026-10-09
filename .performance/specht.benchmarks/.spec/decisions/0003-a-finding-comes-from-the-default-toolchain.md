---
title: "Decision 0003: a finding comes from the default toolchain"
description: "Only a Release build run by BenchmarkDotNet's default out-of-process toolchain produces a recorded finding, and the Benchmark target never selects the in-process toolchain; in-process findings and an in-process CI run were turned down"
type: decision
---

# Decision 0003: a finding comes from the default toolchain

**Date:** 2026-10-09
**Decided by:** spec-author, settling a point `specht-conventions` § Benchmarking
listed as not decided; the owner may reverse it

## The call

A number recorded in a § 8 `### Performance` row comes from a Release build,
run by the library's default toolchain - a generated project in its own
process - with no debugger attached (C-6). The `Benchmark` target never
passes the in-process toolchain. A developer may run the project in process by
hand to iterate; that number is never a finding.

## Why

- The in-process toolchains share JIT state and the GC heap between
  benchmarks, so one benchmark's warm-up moves the next one's number, and they
  cannot measure startup at all (`benchmarkdotnet` skill, "Toolchains"). The
  library's own guidance keeps them for iteration and the default toolchain
  for published results.
- `0109-F4` measures the tool's start, which only the default toolchain can.

## Rejected

**In-process findings.** Cheaper and faster, and not isolated. Cost of
rejecting: a full run takes longer.

**In-process in CI only,** to save runner minutes. A CI number that is not
comparable to a local one, and a cold-start benchmark that cannot run there.
Cost of rejecting: CI pays for process isolation; OQ-1 bounds it by job.

## Affects

- `0109-F1` C-6.
- `0109-F4` C-2.

## Reversal

None.
