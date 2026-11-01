---
title: "ADR-0026: The source-generated mediator measured no benefit; direct calls stay"
description: "Item 0216's evidence decision. martinothamar/Mediator 3.0.2 was tried in front of the check on an unmerged branch and measured against direct calls: whole-check time and allocation unchanged, a cost per send and per composition, and a slower packed-tool cold start. The owner ruled (a), do not adopt: ADR-0014's gate is not met for the mediator, hosts keep direct calls through the existing composition, and ADR-0010's mediator clauses are superseded. ADR-0010's other stages stay gated by ADR-0014."
type: adr
---

# ADR-0026: The source-generated mediator measured no benefit; direct calls stay

## Status

accepted - 2026-10-31. Decider: the repository owner, who ruled on the
proposal the same day in these words: "(a) Don't adopt". Item `0216`
([`.issue/0216-evaluate-source-generated-mediator.yml`](../../.issue/0216-evaluate-source-generated-mediator.yml)),
the comparison
[ADR-0014](0014-architecture-rollout-requires-benchmark-evidence.md) assigns
it, records the ruling. This is the evidence decision ADR-0014 waits for, for
the mediator.

Supersedes in part
[ADR-0010](0010-layered-engine-boundaries-with-io-at-the-edge-and-a-mediator.md):
its mediator clauses, named under Decision, "What this supersedes". ADR-0010's
body is unedited and its Status gains one line pointing here, as ADR-0014's
supersession did; ADR-0014's Status gains one line as well. The ADR template
says a new record supersedes an accepted one entirely; this partial
supersession is deliberate, as ADR-0010's and ADR-0014's were. The record
changes no code, amends no claim and edits no item but `0216`.

## Context

### How to read the evidence

- **Read from a commit**: code, package contents and sizes, build target
  source.
- **Measured by a trial**: a BenchmarkDotNet result or a build result,
  produced by a command run on 2026-10-31 on the commits below and not re-run
  for this record. The trial branch is never merged (ADR-0014) and may be
  deleted once the owner rules, so the numbers that matter are quoted here.
- **Judgement** or **inference**: labelled where it stands.

| Run      | Branch                     | Commit     | Content                                                                 |
| -------- | -------------------------- | ---------- | ----------------------------------------------------------------------- |
| Base     | `main`                     | `c1584735` | release v0.3.0; what both runs were cut from                            |
| Baseline | `0216/mediator-evaluation` | `0f860700` | `c1584735` plus item `0216`'s status only; no code change               |
| Trial    | `0216/trial-mediator`      | `66e82c43` | `c1584735` plus the candidate in front of the check, and the benchmarks |

The baseline branch was later rebased onto `dd2e710d`; the measured baseline
binary was built from `0f860700`, whose code equals `c1584735`. The trial
branch's head has moved past `66e82c43` to `b3d305a9`, which adds
XML documentation tags only (see Correctness); `66e82c43` is the measured
commit.

### The candidate

[martinothamar/Mediator](https://github.com/martinothamar/Mediator), the one
ADR-0014 names: NuGet `Mediator.Abstractions` 3.0.2 and
`Mediator.SourceGenerator` 3.0.2, the latest stable on nuget.org at
2026-10-31, packaged 2026-03-22, source commit
`31583e11289e1bc76e2d49e4f2a990389107fb00`, MIT.

- Runtime: `Mediator.dll`, 39,424 bytes (net8.0). It depends on
  `Microsoft.Extensions.DependencyInjection.Abstractions` >= 8.0.0, already in
  the graph at 10.0.12.
- Compile time only: `Mediator.SourceGenerator.dll`, 1,181,184 bytes, plus two
  polyfill assemblies (`PrivateAssets=all`).
- Configuration: `services.AddMediator()` with the defaults, singleton
  lifetime (the v3 default), handlers discovered in `src/tool`.

### What the trial changed

Read from `c1584735..66e82c43`, 11 files, +236 -9:

| File                                                 | +/-    |
| ---------------------------------------------------- | ------ |
| `Directory.Packages.props`, two versions             | +5     |
| `src/tool/tool.csproj`, two package references       | +2     |
| `src/tool/Program.cs`, `services.AddMediator();`     | +1     |
| `src/tool/Features/Check/CheckCommand.cs`            | +10 -9 |
| `src/tool/Features/Check/CheckRequest.cs`, new       | +9     |
| `src/tool/Features/Check/CheckHandler.cs`, new       | +16    |
| `test/tests/Check/CheckCommand.Integration.Tests.cs` | +1     |
| `test/acceptance/Check/CheckSteps.cs`                | +1     |
| `.performance/benchmarks/Dispatch/*.cs`, three new   | +191   |

- `CheckCommand` takes `ISender` in place of
  `Func<string, SchemaSourceSelection?, SpechtReport>` and awaits
  `sender.Send(new CheckRequest(root, source), cancellationToken)`; its
  `ExecuteAsync` became `async`.
- `CheckHandler` takes the same runner delegate `main` registers and returns
  `ValueTask.FromResult(run(...))`. Both paths therefore do identical engine
  work; the only difference measured is the dispatch path.
- Only the default check command is dispatched. Init, upgrade, schema, rules,
  manifest, frontmatter and evidence still call their services directly.
- The two test helpers that compose their own provider each needed
  `services.AddMediator();`.

Correctness, measured on the trial:

- `dotnet build` of `src/tool` and of the benchmark project in Release passed
  with `TreatWarningsAsErrors`; the generated code raised no Rocket.Surgery
  analyzer finding.
- `./build.sh Test` on `66e82c43`, 21:18: Format and Compile succeeded;
  CodeAnalysis failed with ten SPCT0007 errors per target framework, and the
  tests did not run. `Specht.CodeAnalysis` requires an
  `<implements epic feature claim />` tag on every public type and member of
  `src/tool`: `CheckRequest`, its constructor, `CheckHandler`, its
  constructor and `Handle`.
- The follow-up commit `b3d305a9` adds those tags only, as XML
  documentation citing `0001-F2` B-001 as `CheckCommand` does, so the
  measured IL is unchanged. The full `./build.sh` on that commit, 21:20 to
  21:31, passed every target - Format, Compile, CodeAnalysis, UnitTest,
  IntegrationTest and AcceptanceTest: 13 test runs over both target
  frameworks, 7,698 tests, 0 failed, 424 skipped.

### Conditions

- Apple M1 Max, 10 cores, 64 GB, macOS Tahoe 26.4 (25E246); .NET SDK
  `11.0.100-rc.2.26504.105`; runtime .NET 10.0.12 Arm64 RyuJIT;
  BenchmarkDotNet 0.15.8 with the repository's `BenchmarkConfiguration`
  (default job, MemoryDiagnoser, out-of-process toolchain, Release).
- The host was shared with other sessions' builds: a 1-minute load average of
  6.6 to 16.4 on 10 cores during the runs.
- The in-process comparisons are within one benchmark class, so both methods
  ran under the same conditions. The cold-start runs were alternated -
  baseline, trial, baseline, trial - to expose drift.

```sh
# trial at 66e82c43
./build.sh Benchmark --benchmark-filter '*Dispatch.*'
# each tree, alternated: baseline 0f860700, trial 66e82c43, baseline, trial
dotnet build src/tool/tool.csproj -c Release
./build.sh Benchmark --benchmark-filter '*ColdStart*'
```

### The measurements

Measured by the trial. ADR-0014 asks for whole-check and start-up costs with
dispatch and registration exercised; 1 and 4 are those, 2 and 3 isolate the
two parts.

**1. Whole check, in process** (`CheckDispatchBenchmarks`, 21:07:55 to
21:10:55, load 9.47 to 6.65). The provider is composed as `Program.cs`
composes it and resolved through `TypeRegistrar`; the tree is synthetic, from
`SpechtTree.Of`, and setup refuses a tree either path does not count whole and
clean.

| Method   | Specs | Mean      | StdDev    | Ratio | Allocated    | Alloc ratio |
| -------- | ----- | --------- | --------- | ----- | ------------ | ----------- |
| Direct   | 1     | 2.897 ms  | 0.0361 ms | 1.00  | 857.88 KB    | 1.00        |
| Mediator | 1     | 2.872 ms  | 0.0454 ms | 0.99  | 857.91 KB    | 1.00        |
| Direct   | 100   | 48.573 ms | 1.2925 ms | 1.00  | 34,207.5 KB  | 1.00        |
| Mediator | 100   | 48.982 ms | 1.8559 ms | 1.01  | 34,207.53 KB | 1.00        |

The difference is within one standard deviation at both sizes; +0.03 KB per
check. BenchmarkDotNet warned MultimodalDistribution on Direct at 1 spec
(mValue 3.45).

**2. Dispatch alone** (`DispatchOverheadBenchmarks`). The delegate returns a
report computed once in setup, so neither path does the check's work.

| Method   | Mean       | StdDev    | Allocated |
| -------- | ---------- | --------- | --------- |
| Direct   | 0.0353 ns  | 0.0147 ns | 0 B       |
| Mediator | 12.1205 ns | 0.0691 ns | 32 B      |

BenchmarkDotNet warned ZeroMeasurement on Direct: indistinguishable from an
empty method. The mediator costs about 12 ns and 32 B per send. The 32 B is
the size of the `CheckRequest` record, a header and two references, so it is
the request's allocation rather than the mediator's bookkeeping (inference
from object size).

**3. Composition and first dispatch** (`CompositionBenchmarks`). A new
`ServiceCollection().AddSpechtEngine()` with the delegate, plus
`AddMediator()` on the trial path; build the provider, resolve the entry,
dispatch once, dispose. Steady state with the JIT warm, so this is the
per-process registration cost, not a cold start.

| Method   | Mean      | StdDev    | Ratio | Allocated | Alloc ratio |
| -------- | --------- | --------- | ----- | --------- | ----------- |
| Direct   | 8.788 us  | 0.2046 us | 1.00  | 17.24 KB  | 1.00        |
| Mediator | 14.441 us | 0.2391 us | 1.64  | 29.36 KB  | 1.70        |

+5.65 us and +12.1 KB per process.

**4. Packed-tool cold start** (`ColdStartBenchmarks`, unchanged from `main`).
`dotnet tool run` of the packed tool from an isolated local manifest, on a tree
of one clean specification. Empty tool is the generated no-op tool baseline;
Allocated is the benchmark process, not the tool, and is not compared.

| Run        | Commit     | Window            | Load start to end | Empty tool | Help      | Check     |
| ---------- | ---------- | ----------------- | ----------------- | ---------- | --------- | --------- |
| baseline-1 | `0f860700` | 21:12:14-21:13:37 | 11.96 to 10.52    | 86.29 ms   | 226.00 ms | 389.30 ms |
| trial-1    | `66e82c43` | 21:13:37-21:15:05 | 10.52 to 11.87    | 88.37 ms   | 231.47 ms | 405.57 ms |
| baseline-2 | `0f860700` | 21:15:05-21:16:28 | 11.87 to 8.48     | 72.03 ms   | 223.17 ms | 396.96 ms |
| trial-2    | `66e82c43` | 21:16:28-21:17:54 | 8.48 to 10.70     | 88.18 ms   | 238.64 ms | 423.98 ms |

StdDev: Check 3.2 to 9.4 ms, Help 2.6 to 5.0 ms, Empty 0.8 to 1.4 ms.

- Check: the trial is slower in both pairs, +16.3 ms (+4.2%) and +27.0 ms
  (+6.8%); mean of the pairs 393.1 to 414.8 ms, +21.6 ms (+5.5%).
- Help, which dispatches nothing and exercises registration and Spectre
  only: +5.5 ms and +15.5 ms; mean 224.6 to 235.1 ms, +10.5 ms (+4.7%).
- The empty tool, which runs no specht code, moved between 72 and 88 ms across
  runs. Host noise is of the same order as the Help delta, so the cold-start
  magnitude is indicative and its direction consistent: the trial was never
  faster.
- The cold-start delta, about 10 to 20 ms, is three orders above the warm
  composition delta of 5.65 us. The remainder is plausibly loading
  `Mediator.dll` and the JIT of the generated mediator and the async command
  path at first use (inference; not decomposed).

The packed `specht.tool` package, measured from the two packs: baseline
4,161,113 bytes, trial 4,260,320 bytes, +99,207 bytes (+2.4%).

### A harness defect, recorded and not acted on

Read from `.build/SpechtBuild.cs`, unchanged from `c1584735` to `dd2e710d`:
the `Benchmark` target builds `.performance/benchmarks/benchmarks.csproj` for
`net10.0` only, then, when a `Startup` benchmark is selected, packs
`src/tool/tool.csproj` with `--no-build`. On a tree with no Release `net11.0`
build of the tool, the pack fails with MSB3030; measured on all four first
cold-start attempts, 21:10 to 21:11. The prebuild in the commands above works
around it. The defect is on `main` and not specific to the trial. Whether the
CI job, which runs `Benchmark` after its own build, meets it was not checked.
No item is cut here.

### Complexity and maintenance cost

Read from the trial diff and the packages.

- Two new packages, one of them a source generator in the compile of
  `src/tool`, which runs inside the compiler of a preview SDK
  (`11.0.100-rc.2`).
- Per dispatched command: one request record, one handler class, and the
  command rewritten to send. With ten commands - the check, `init`,
  `upgrade`, `schema list`, `schema pin`, `frontmatter`, `rules`,
  `rules set`, `manifest` and `evidence` - that is 20 new types, each with one
  caller. Today's seam is one registration line, the
  delegate in `src/tool/Program.cs`.
- Each of those types, and each public member, must carry an `<implements>`
  claim citation or the build fails with SPCT0007, as the trial's did: five
  tags for the check alone.
- The generated `AddMediator()` must be called by every provider that
  resolves a dispatching command: the production host and two test helpers
  today. A missed call fails at resolve time, not at compile time.
- `ISender.Send` returns `ValueTask<T>`; a command becomes an async state
  machine where its body was synchronous.
- Upstream release line on nuget.org: 2.1.x to 2.1.7, then 3.0.1 and 3.0.2, a
  major in between. A pin here would follow that line through Renovate like
  every other central version. Upstream maintenance cadence was not assessed.

### What was not measured

- ADR-0010's handwritten `ICqrs`: ADR-0014 superseded it and does not
  authorize a handwritten substitute.
- Linux and Windows, the CI runners, and a quiet host.
- Any command but the check, the ahead-of-time spike (ADR-0010 stage P), and
  ADR-0010's structural stages, none of which is dispatch.

## Decision drivers

- ADR-0014: rollout waits for a repository comparison "that the owner judges
  worth its complexity and maintenance cost"; no numerical threshold.
- ADR-0010 owner decision 7: start-up time is a driver.
- `coding-conventions` § "Design": no seam without a second real caller or a
  substitution that exists today.

## Considered options

### (a) Do not adopt martinothamar/Mediator

What it looks like: nothing changes in `src/`. The check keeps the delegate
`src/tool/Program.cs` registers, and every other command keeps its direct
call through the existing composition. The trial branch is not merged.

Cost: none measured; the numbers above are what it avoids.

Effect on the items that depend on `0216`:

- `0167` is redesigned without mediator handler contracts, as ADR-0014
  already requires it be redesigned from this evidence. `0144`'s stage-8
  scope in ADR-0010 names a send through `ICqrs`, and its `depends_on`
  names `0167` for the mediator; that clause has nothing to send through
  under (a). Named, not decided.
- ADR-0010's other stages do not depend on a mediator: `0100` (stage 1),
  `0156` to `0163` (stage 3), `0164` (stage 4), `0153` and `0166` (stage 6).
  ADR-0014 gates the rollout as a whole, so under (a) each still needs its own
  evidence. Whether those stages proceed is for the owner to rule; this
  record does not decide it.

### (b) Adopt for structure, not speed

What it looks like: the trial's shape for every command - one request and one
handler per command, ten commands; `AddMediator()` in every
composing provider, the test helpers included; the commands async. The
trial placed the request and handler in `src/tool`; ADR-0010 owner decision
3 places them in the engine, and where the generator then runs was not tried.

Cost: the measured cost accepted as the price - +12 ns and 32 B per send,
+5.65 us and +12.1 KB per composition, a slower cold start (Check +21.6 ms and
Help +10.5 ms on the mean of the pairs, indicative), +99,207 bytes of
package, and the maintenance listed above.

Effect: the owner's ruling meets ADR-0014's gate on grounds other than
performance. `0167` is still redesigned from this evidence before
implementation - its handler contracts and return types become the
candidate's `IRequest<T>` and `ValueTask<T>` in place of `ICqrs` and
`IQuery<T>` - and a migration design names the per-command requests,
handlers and registrations. Whether the ruling also releases the stages that
are not dispatch is the owner's to say in it.

### (c) Re-measure before ruling

What it looks like: the same benchmarks on a quiet host, or on the CI Linux
and Windows runners, from the same two commits.

Cost: another run; the harness defect above has to be worked around or fixed
first.

Effect: every item that depends on `0216` stays blocked until the re-run is
recorded and the owner rules. In-process results 1 to 3 compare methods
within one class and would be expected to hold (judgement); the cold start is
the measurement a quieter host would sharpen.

## Decision

Accepted: **(a)**, do not adopt martinothamar/Mediator. This is the
repository owner's decision of 2026-10-31, in the owner's words "(a) Don't
adopt"; it is no longer a judgement of this record. (b) and (c) are not
chosen.

The proposal recommended (a) as a **judgement**, resting on the measurements
above, and left the decision to the owner. Its reasoning, unchanged:

Performance supplies no benefit to weigh against the cost. The whole check's
time and allocation are unchanged within noise. Dispatch adds about 12 ns and
32 B per send, on a path taken once per invocation. Composition adds 5.65 us
and 12.1 KB. The packed tool's cold start is slower in both alternated pairs -
Check +16.3 and +27.0 ms, Help +5.5 and +15.5 ms - with the magnitude
indicative on a loaded host and the direction consistent, and the package
grows by 99,207 bytes. ADR-0014's gate, benchmark evidence that the benefit is
worth the complexity and maintenance cost, is therefore not met on
performance grounds.

The proposal held (b) open as a structural choice these measurements do not
support, and (c) for an owner who read the loaded host as reason to doubt the
cold-start direction. The owner chose neither.

ADR-0014's gate is not met for the mediator. The comparison it requires is
recorded above, and the owner's ruling is the unfavourable result ADR-0014's
Consequences anticipate.

### The revised migration design

- Hosts reach the engine by direct calls through the existing composition:
  `AddSpechtEngine()` and the registrations `src/tool/Program.cs` and the
  command slices make. A command takes what it calls by constructor, as every
  command does today.
- No mediator, no dispatcher, no `Specht.Cqrs`, and no request or handler
  contracts. No handwritten substitute either: ADR-0014 already forbids one,
  and this record authorizes none.
- The engine's check entry point that replaces `Check.Query` and
  `Check.Handler` is not designed here. It is item `0167`'s redesign, which
  ADR-0014 already requires from this evidence before implementation. Whether
  the runner delegate `src/tool/Program.cs` registers - stage 8's
  `Func<string, SpechtReport>` seam - goes, and what a command takes in its
  place, is decided with that redesign and item `0144`.
- ADR-0010's other stages are unchanged as target: stages 1 to 6, the parts
  of stages 7 and 8 the table below leaves standing, and tracks T, R and P.
  They stay gated by ADR-0014. This record measured dispatch only and
  supplies no evidence for them; whether they proceed is for the owner.

### What this supersedes

In
[ADR-0010](0010-layered-engine-boundaries-with-io-at-the-edge-and-a-mediator.md),
its body unedited:

| ADR-0010                              | Superseded                                                                                                                                                                         | Stands                                                                                                                                                                                                            |
| ------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Owner decision 2                      | The whole decision: a mediator; requests and handlers in the shape of the owner's cqrsz library; dispatch through a minimal `ICqrs`; replacement by a source-generated port.       | Option 3's rejection of a use-case object is not reopened; this record adopts none.                                                                                                                               |
| § "The layout"                        | `Specht.Cqrs`, and the bullet that stands it outside the order; `Check.Query` and `Check.Handler` as what `Specht.Check` holds.                                                    | Every other namespace, the downward order, and `Specht.Composition` outside it. What `Specht.Check` holds is item `0167`'s redesign.                                                                              |
| § "A host sends a request"            | The whole section: the `Check` sketch, cqrsz's conventions, `ICqrs` and its dispatcher, and "The departure from cqrsz".                                                            | Owner decision 4 as the section applies it: the engine returns a plain result, and the four input-failure exceptions cross to the catch ladder in `CheckCommand`.                                                 |
| § "The patterns, present and absent"  | The Mediator row.                                                                                                                                                                  | Strategy, Builder and Decorator, and what is deliberately absent.                                                                                                                                                 |
| § "Each interface, and what earns it" | The row for `IQuery<T>`, `ICommand<T>` and the two handler contracts; the `ICqrs` row; the paragraph that takes `ICqrs` as the one exception to `coding-conventions` § Design.     | `ISpecRule` and `IFileSystem`; everything else a sealed class; `ISpecDiscovery`'s timing. The § Design bar holds with no exception.                                                                               |
| § "What 'no static' covers"           | The bullet keeping "a cqrsz operation class such as `Check`". It exists only because cqrsz nests a message and its handler in one static class; with no mediator nothing needs it. | The `static readonly` constant and `AddSpechtEngine()` bullets, and the judgement on private static helpers and `static` lambdas.                                                                                 |
| § "The order of work"                 | Stage 7: `Specht.Cqrs`, and `Check.Query` and `Check.Handler` in cqrsz's shape. Stage 8: "`CheckCommand` sends `Check.Query` through `ICqrs`".                                     | Stage 7's report mapper, serializer and store, `SpechtRunner` deleted, and the guard; stage 8's ADR-0007 factory and `ShippingCopy` as an instance; stages 1 to 6; tracks T, R and P. All stay gated by ADR-0014. |

Owner decisions 3 and 4 are not superseded. Each names requests and handlers;
after this record they apply to the engine's check entry point that item
`0167`'s redesign gives, so the tool, the benchmarks and any later host call
the same one.

Further mentions in ADR-0010 rest only on the clauses above and are read as
superseded with them: option 1's "a mediator in front" as part of the chosen
option; the Buys bullet's "sends a message and composes nothing"; the Costs
bullets on "a dispatcher this repository owns and tests" and on handlers that
depart from cqrsz's return type; open point 2's "dispatched through the same
`ICqrs`"; `Check.Handler` and `Check.Query` as the measured call in § "The
whole-check benchmark", which item `0167`'s redesign re-points; and, in
ADR-0010's own § "What this supersedes", "driver 3, but for `ICqrs`" in the
ADR-0001 row and "with `ICqrs` the one stated exception" in the ADR-0005 row.
ADR-0001's driver 3 and ADR-0005 (d) stand without that exception.

## Consequences

- No code changes. The check keeps the delegate `src/tool/Program.cs`
  registers, and every other command keeps its direct call. Trial branch
  `0216/trial-mediator` is not merged; the numbers that matter are quoted
  under Context.
- ADR-0014's gate is answered for the mediator, unfavourably. It stands for
  the rest of ADR-0010's rollout.
- Two items need rescoping by `spec-author` before either is implemented,
  because their mediator clauses are gone:
  - `0167`: its title, the summary's `Specht.Cqrs` bullet, criterion 3
    (`Specht.Cqrs`), criterion 4 (the cqrsz `Check` class), criterion 7's
    exemption for `Specht.Cqrs`, and criterion 8 (the benchmark calling
    `Check.Handler` with a `Check.Query`). Its report types, the runner's
    deletion and the guard are not mediator clauses and stay gated by
    ADR-0014.
  - `0144`: the stage-8 criterion that `CheckCommand` takes `ICqrs` and sends
    `Check.Query`, the open-point-2 criterion on `InitCommand` sending
    through `ICqrs`, and its `depends_on` reason, "the mediator and
    `Check.Query` from 0167". The factory, the test wrap and `ShippingCopy`
    are not, and stay gated.
- Item `0170`'s criterion 6 weighs "the hand-written dispatcher of item
  0167", which will not be built. Named, not edited.
- `0100`, `0153`, `0156` to `0164` and `0166` are not mediator-dependent.
  Closing `0216` meets their `depends_on` edge on it but not ADR-0014's gate:
  they remain gated, and `blocked`, pending the owner.
- This record edits none of those items.
- Guidance that stated the mediator as the direction, or as still to be
  evaluated, is revised with this record: `AGENTS.md` § Architecture,
  `dotnet-tool` and its vertical-slice reference, the `specht-conventions`
  coding reference, and the roadmap's 0.4.0 milestone. No Feature
  specification is edited; their historical review rows stand.
- The harness defect stays recorded; no item is cut for it.
- Item `0216`'s fourth acceptance criterion holds: the owner's evidence
  decision is recorded here and in the item, and the migration design is
  revised above, before any adoption.
