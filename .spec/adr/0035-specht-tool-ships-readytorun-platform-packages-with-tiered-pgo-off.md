---
title: "ADR-0035: specht.tool ships ReadyToRun platform packages with Tiered PGO off"
description: "specht.tool turns Tiered PGO off in its runtime configuration and packs ReadyToRun code through .NET 10's platform-specific tool packages - a top-level specht.tool package pointing at one framework-dependent package per runtime identifier and a portable fallback - so every process start costs less while the package id, the command and the local tool manifest pin stay as they are. Proposed; native AOT is deferred and a long-lived specht stays excluded."
type: adr
---

# ADR-0035: specht.tool ships ReadyToRun platform packages with Tiered PGO off

## Status

proposed - 2026-11-10, by spec-author. Decider: the repository owner, who
has not ruled on this record. The owner's only word is the choice, in
session on 2026-11-10, of "TieredPGO + ReadyToRun" from the options offered,
to have this lever specified; every detail of the design below is
spec-author's proposal. The claims are `0001-F2` B-049 to B-059, its
constraints C-12 to C-15 and its decision 0015; its open questions OQ-10 to
OQ-12 hold the calls this record leaves open.

## Context

- Every call site starts `specht` as a new process: the pre-commit hook, CI,
  an agent's shell, and `specht-mcp`, which runs the pinned tool once per tool
  call through `dotnet tool run specht` (`0132-F3`;
  [ADR-0031](0031-the-mcp-is-its-own-program-that-runs-the-repositorys-pinned-specht.md)).
  ADR-0031 decision 8 rules out a long-lived `specht`, so the start itself has
  to cost less.
- The recorded finding of the Startup benchmark, `ColdStartBenchmarks`
  (`0109-F4` § 8), is empty tool 199.7 ms, help 339.8 ms and a check of one
  specification 508.6 ms.
- Manual observations, not a benchmark finding: 2026-11-10, this
  repository's own tree of 437 specification files, macOS arm64, the .NET 10
  runtime, a framework-dependent `dotnet publish -f net10.0 -r osx-arm64` of
  `src/tool`, `specht --json`, four runs each on a loaded laptop.

  | Build                               | Wall time                             |
  | ----------------------------------- | ------------------------------------- |
  | Default, JIT                        | about 2.1 to 2.3 s                    |
  | `DOTNET_TieredPGO=0`                | about 1.75 to 2.0 s                   |
  | `PublishReadyToRun=true`            | about 1.7 to 1.9 s, one run at 2.4 s  |
  | ReadyToRun and `DOTNET_TieredPGO=0` | about 1.33 to 1.42 s, 35 to 40 % less |

  `specht --version` on the ReadyToRun build took 142 ms; `dotnet tool list`
  about 80 ms; `dotnet tool run specht -- --version` about 200 ms.

- `<TieredPGO>false</TieredPGO>` in a project writes
  `System.Runtime.TieredPGO: false` into its `runtimeconfig.json`; it changes
  no packaging. Dynamic PGO instruments code before optimising it, a cost a
  short-lived process pays without reaching the benefit.
- ReadyToRun is compiled per runtime identifier and only on a publish that
  names one. A .NET tool carries runtime-identifier-specific code only through
  the platform-specific tool packages of the .NET 10 SDK:
  `ToolPackageRuntimeIdentifiers` makes `dotnet pack` write a top-level
  pointer package, `<id>.<version>.nupkg`, and one `<id>.<rid>.<version>.nupkg`
  per runtime identifier; an `any` entry adds a framework-dependent package
  used on every platform not listed. Installing such a tool needs a .NET 10
  SDK or later. Every package must be on the feed before the pointer, at the
  pointer's exact version.
  [ADR-0028](0028-files-stay-the-reading-path-and-online-allocation-waits.md)
  note 10 names the property.
- The SDK makes each listed runtime identifier's package self-contained by
  default. A manual observation, not a benchmark finding, 2026-11-10: a
  scratch `dotnet pack src/tool -c Release` on SDK 11.0.100 and an osx-arm64
  host, with `ToolPackageRuntimeIdentifiers` `osx-arm64;linux-x64;any`,
  `RuntimeIdentifiers` `osx-arm64;linux-x64`, `PublishReadyToRun=true`,
  `SelfContained=false` and `TieredPGO=false` on the tool project alone:
  - It wrote `specht.tool.osx-arm64` (8.9 MB) and `specht.tool.linux-x64`
    (9.4 MB), the second cross-compiled on the osx-arm64 host.
  - Each holds, under `tools/net10.0/<rid>/` and `tools/net11.0/<rid>/`, the
    apphost `specht.tool`, `specht.tool.dll`, `specht.dll`, `deps.json` and
    `runtimeconfig.json`, and no `coreclr`, `hostfxr` or
    `System.Private.CoreLib`: framework-dependent.
  - `runtimeconfig.json` names the `Microsoft.NETCore.App` framework and sets
    `"System.Runtime.TieredPGO": false`.
  - `DotnetToolSettings.xml` names the command `specht`, entry point
    `specht.tool`, runner `executable`.
  - Not shown: `specht.tool.any` and the top-level package. The pack failed
    with NETSDK1047, the assets file having no target for `net10.0/any`,
    which suggests `RuntimeIdentifiers` must list `any` too.
- The ReadyToRun compiler on a macOS or Linux SDK targets Linux and macOS
  only; Windows targets need a Windows x64 SDK. The release `build` job runs
  on macOS arm64 (`0055-F6` C-9).
- The tool targets `net10.0` and `net11.0` (`0001-F2` decision 0011, B-045),
  and ReadyToRun applies to each.
- The consumer contract: package id `specht.tool`, command `specht`, a local
  tool manifest pin (`0001-F2` C-5, C-6), a global install as the second path
  (`0001-F2` decision 0009).

## Decision drivers

- A cheaper start on every call, with no long-lived process.
- No change a consumer makes: the package id, the command, the pin, the
  install paths and `dotnet tool run specht`, which `specht-mcp` uses, stay.
- Framework-dependent, as today: the tool runs on the .NET runtime the
  machine has.
- No behaviour of the check changes; only how its code reaches the CPU.
- Evidence a reviewer can read beside the change.

## Considered options

1. **Status quo.** Costs nothing; every call keeps paying the JIT start.
2. **Tiered PGO off only.** One property, no packaging change; the smaller
   saving, about 10 to 20 % in the observations above.
3. **ReadyToRun only.** Platform-specific packages and their release cost;
   about the same saving as option 2 alone, with Tiered PGO's start cost
   left in.
4. **ReadyToRun and Tiered PGO off.** Recommended. Both costs of options 2
   and 3, and the largest observed saving, 35 to 40 %.
5. **Native AOT.** Deferred, not rejected. YamlDotNet, JsonSchema.Net and
   Spectre.Console.Cli rely on reflection, AOT is not measured here, Native
   AOT packs only for the build host's operating system, and `0109-F4` § 5
   row 7 gives AOT and trimming an ADR of their own.
6. **A long-lived `specht`.** Excluded by ADR-0031 decision 8.

## Decision

Proposed: option 4.

1. `src/tool` sets `TieredPGO` to `false`, so every package that carries the
   tool states it in its runtime configuration (`0001-F2` B-049).
2. `src/tool` lists its runtime identifiers in `ToolPackageRuntimeIdentifiers`
   and `RuntimeIdentifiers`, with `any` last, and publishes ReadyToRun; the one
   `dotnet pack` of `Pack` writes the pointer `specht.tool`, one
   `specht.tool.<rid>` per listed runtime identifier and `specht.tool.any`
   (B-050 to B-053). The set is `0001-F2` OQ-10.
3. Every package is framework-dependent: none carries the .NET runtime
   (B-054; C-13). Composite ReadyToRun, which needs self-contained, is not used.
4. Nothing a consumer types changes (B-055 to B-059; C-12).
5. Tiered PGO stays on in `specht.mcp`, a long-lived process.
6. The delivering pull request runs `ColdStartBenchmarks` on its head and on
   the `main` commit it is cut from, on one host, back to back, and records
   the empty-tool, help and check rows of both in the pull request and in
   `0109-F4` § 8. Neither ADR-0014 nor ADR-0027 binds a packaging change by its
   terms; this record borrows their posture: a finding the owner judges, no
   threshold and no automated gate (C-15).

## Consequences

Gained:

- Each start is cheaper for the hook, CI, an agent's shell and every
  `specht-mcp` tool call, with nothing to change on their side.
- A platform the list leaves out still runs the tool, at today's cost, with
  Tiered PGO off.

Paid:

- Packages per release: one for each listed runtime identifier, plus the
  pointer and `specht.tool.any`, where one is pushed today. The release must
  push every runtime identifier's package and `specht.tool.any` before the
  pointer; `0055-F6` B-003 names three packages and is owed an amendment, and
  `PushReleasePackages` (`.build/SpechtBuild.Release.cs`) finds one file per
  project id today.
- ReadyToRun assemblies are two to three times the size of IL, per runtime
  identifier and per target framework; the pack takes longer.
- A consumer needs a .NET 10 SDK or later to install the tool (`0001-F2`
  OQ-11).
- The release host cannot compile Windows ReadyToRun code (`0001-F2` OQ-10).
- Tests and the self-check that read "the one `specht.tool` package" find
  several; which package each existing package claim holds of is
  `0001-F2` OQ-12.
- The Startup benchmark's baseline tool stays a JIT tool, so its ratios move
  as well as its times.

## What would reverse it

- The delivering pull request's `ColdStartBenchmarks` rows show no saving
  against `main` on the check row: drop ReadyToRun and keep option 2.
- `dotnet pack` cannot produce `specht.tool.any` and the top-level package
  beside the framework-dependent runtime-identifier packages it was observed
  to write: return to the owner; a self-contained package is `0001-F2` § 5
  row 33.
- `dotnet tool restore` or `dotnet tool run specht` behaves differently for a
  consumer on a supported SDK, or `dotnet tool list` stops naming
  `specht.tool`: the contract of C-12 wins and this record is superseded.
- Native AOT measured and adopted by a later ADR supersedes decision 2.
