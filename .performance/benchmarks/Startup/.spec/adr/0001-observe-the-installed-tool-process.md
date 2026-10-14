---
title: "ADR-0001: Observe the installed tool process without changing the product"
description: "Keep timing through the local tool manifest, isolate installations and require evidence for per-tool lifetime peak memory before selecting an OS observer"
type: adr
---

# ADR-0001: Observe the installed tool process without changing the product

## Status

proposed — 2026-10-13. The timing/session design is concrete; the OS memory
mechanism is not selected until feasibility evidence meets the conditions below.
This is not an accepted portability claim or a completed benchmark finding.

## Context

0109-F4 B-001 to B-009 and C-3 to C-8 require installed-package timings, a
fresh process per invocation, isolated local caches, an empty baseline and the
actual tool process's peak memory. The harness already owns packing, timing,
allocation diagnostics, exporters and main-only CI publication. No product
instrumentation or new process framework is needed for timing.

A local-tool invocation resolves a manifest entry and executes its command.
The process started by the benchmark must not be assumed to be the tool being
measured. A zero-output baseline can exit before polling discovers it. A process
handle's lifetime and memory retrieval semantics differ by OS; .NET documents
PeakWorkingSet64 returning zero on macOS. None of these facts selects a correct
cross-platform memory observer by itself.

## Considered options

1. Read PeakWorkingSet64 from the process started for `dotnet tool run`.
2. Discover descendants by polling and retain the highest sampled working set.
3. Observe native process creation/exec/exit and retain the identified tool's
   OS lifetime resident-memory high-water mark.
4. Inject a startup hook or suspend/connect the runtime, optionally in a separate
   diagnostic pass, to identify and observe the tool.
5. Run the installed assembly directly instead of through the local manifest.

## Decision

Use the existing BenchmarkDotNet toolchain, jobs and exporters, a concrete
isolated session, one fresh manifest-mediated invocation per benchmark call and
one generated empty-tool baseline. The session and argument-list launcher are
shared by the baseline/help/check paths; setup and cleanup remain outside timing.
The allocation column remains benchmark-process managed allocation.

Investigate option 3 before implementing B-006. Native lifecycle mechanisms
(for example process debug/trace events paired with native peak-resident
accounting) are candidates, not a promised API. The investigation must prove:

- Tool identity independently of the launcher; process ancestry alone is not an
  identity check when both executable names are dotnet.
- Lifetime coverage and high-water retrieval even for an immediate exit, with
  no race silently returning zero and no aggregation with launcher/descendants.
- Byte units and resident-memory semantics; private commit, virtual size,
  managed allocation and instantaneous resident samples are different metrics.
- Feasibility on unprivileged Linux/Windows hosted runners, and documented macOS
  behavior; no change to CI privilege policy is implied.
- Per-invocation association with a startup row, bounded resource ownership,
  cleanup on failure, and observer overhead disclosed in the finding.
- A BenchmarkDotNet extension that carries tool peak and observation count into
  both existing exporters without affecting unrelated check benchmarks.

Options 1 and 2 do not satisfy C-7 by assumption: one can attribute the wrong
process, and the other can miss both a short-lived process and its true peak.
Option 5 violates C-3. Option 4 changes the measured program/runtime path; it
requires an explicit design/claim review, not a hidden fallback. If option 3
cannot be made reliable within those constraints, record the negative evidence
and keep the memory task out of implementation until the decision is resolved.
A timing-only delivery does not mark B-006 or the Feature complete.

## Consequences

Timing can be delivered as a useful, independent slice. The memory task has an
honest technical prerequisite rather than an implementation built on a guessed
API. Native observation may require OS-specific code and additional dependencies;
none is selected or added in this design. A successful investigation updates
this proposed record before adoption; after acceptance, a changed choice needs
a new ADR. All raw probes/reports stay under ignored artifacts or temporary
storage and no benchmark tests or product hooks are introduced here.

## Evidence and sources

- Existing declarations: `BenchmarkConfiguration.cs`, `BenchmarkProjectGenerator.cs`
  and `Program.cs` in the benchmark project; `.build/SpechtBuild.cs`.
- [dotnet tool run](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-tool-run)
  and [SDK ToolRunCommand source](https://github.com/dotnet/sdk/blob/main/src/Cli/dotnet/Commands/Tool/Run/ToolRunCommand.cs).
  Source at main establishes the execution shape, not a stable child-process API;
  probe the effective SDK selected by this repository before relying on it.
- [Process platform notes](https://learn.microsoft.com/en-US/dotnet/api/System.Diagnostics.Process)
  and [PeakWorkingSet64](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.process.peakworkingset64?view=net-10.0).
- [BenchmarkDotNet diagnosers](https://benchmarkdotnet.org/articles/configs/diagnosers.html)
  distinguish managed allocation diagnostics from the tool-process observation
  this Feature requires. Built-in allocation diagnostics do not establish B-006.
