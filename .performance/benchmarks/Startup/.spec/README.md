---
title: "Specification: Tool cold start"
description: "A benchmark of the packed specht tool as a consumer runs it - installed from the local package output into a temporary local tool manifest, started as a fresh process for each measured invocation - timing a generated tool that does nothing as the baseline, specht --help, and a check of a one-specification tree, each as a ratio of the baseline, with the tool process's peak memory as a recorded finding"
type: feature
id: "F4"
epic: "0109"
spec_status: approved
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
depends_on: ["F1"]
blocks: []
spikes: []
created: "2026-10-09"
updated: "2026-10-13"
github_issue: null
synced_at: null
---

# Specification: Tool cold start

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-09 -->

Every call site starts the tool as a new process - the pre-commit hook on every commit, CI on every run, an agent mid-write (brief § 2 Must-1) - so on a small tree the process's start, not the engine, is most of what a person waits for, and nobody knows what it costs. This Feature removes that failure state: the packed tool, installed and invoked the way a consumer's repository does, is measured from process start to exit on the smallest generated tree, one fresh process per measurement, beside a generated tool that does nothing and the tool composing its host without a check, so its fixed cost per invocation, the share of it that launch, host and engine each take, and the tool process's own memory are recorded findings.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-09 -->

| #   | Persona                             | Need                                                                | Pain Point Today                                     |
| --- | ----------------------------------- | ------------------------------------------------------------------- | ---------------------------------------------------- |
| 1   | The maintainer of four repositories | Know what the hook adds to every commit                             | Felt, not measured                                   |
| 2   | `benchmarker`                       | A number for the tool's fixed cost, apart from the engine's work    | The engine benchmarks never start a process          |
| 3   | `implementer`                       | See whether a dependency or a startup change moved the tool's start | A change to the host's composition is never measured |

### Assumptions

| ID  | Assumption                                                                                                                                                                                        |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | The benchmark target packs the tool before any benchmark runs when this Feature's benchmark is selected (`0109-F1` B-013), and that package is the one `0055-F7` describes a consumer installing. |
| A-2 | A check of the one-specification generated tree exits `0` (`0109-F1` B-015; `0001-F2` exit codes).                                                                                                |
| A-3 | `specht --help` composes the host, prints usage and exits `0` without reading a tree (`0001-F2` B-008).                                                                                           |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-09 -->

| ID    | Claim                                                                                                                                                                                | Source                                           | Status |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------ | ------ |
| B-001 | Given the package the build packed, this Feature installs it from the local package output into a temporary local tool manifest before measuring.                                    | owner, 2026-10-09; C-3; C-4                      | Active |
| B-002 | Given the installed tool, this Feature measures one check of the one-specification generated tree from process start to exit.                                                        | owner, 2026-10-09; C-1                           | Active |
| B-003 | Given two measured invocations, this Feature starts each in a process of its own.                                                                                                    | decision 0001; C-6                               | Active |
| B-004 | Given a measured invocation that exits non-zero, this Feature fails instead of reporting a time.                                                                                     | `0109-F1` OQ-3 (owner, 2026-10-09); C-1          | Active |
| B-005 | Given a run completes, this Feature leaves no tool installed outside its temporary directory.                                                                                        | `0109-F1` C-10; C-4                              | Active |
| B-006 | Given a measured invocation, this Feature reports the peak memory of the measured tool's own process, not that of the `dotnet` command that launches it or of the benchmark process. | owner, 2026-10-09; OQ-1; C-7; decision 0002      | Active |
| B-007 | Given a minimal tool generated by the benchmark code, packed and installed exactly as B-001 installs `specht`, this Feature measures its start to exit as the baseline.              | owner, 2026-10-09; OQ-2; C-8; decision 0002      | Active |
| B-008 | Given the installed tool, this Feature measures `specht --help`, an invocation that composes the host and runs no check, from process start to exit.                                 | owner, 2026-10-09; OQ-2; A-3; C-1; decision 0002 | Active |
| B-009 | Given the three measurements, this Feature reports each as a ratio of the baseline.                                                                                                  | owner, 2026-10-09; OQ-2; C-1; decision 0002      | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-09 -->

| ID  | Constraint                                                                                                                                                                                                                                                                                                   | Rules Out                                                                                                                                                                                                                                                                                       |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| C-1 | The packed tool's fixed cost per invocation is a reported finding, never a threshold, decomposed into three rows - the launch floor of a tool that does nothing, the tool composing its host and running no check, and the tool checking the one-specification tree - each reported as a ratio of the first. | A gate on any row or ratio; a tree size parameter, which is `0109-F3`'s question; a finding of "fast" or "slow".                                                                                                                                                                                |
| C-2 | It is measured by the default out-of-process toolchain (`0109-F1` decision 0003).                                                                                                                                                                                                                            | The in-process toolchain, which cannot measure startup.                                                                                                                                                                                                                                         |
| C-3 | The tool measured is the packed package, run through a local tool manifest as a consumer runs it.                                                                                                                                                                                                            | Timing the build output's executable, `dotnet run`, or the engine through a project reference.                                                                                                                                                                                                  |
| C-4 | The package is installed from the local package output only, with its tool manifest and package cache inside the benchmark's temporary directory.                                                                                                                                                            | A feed; a global tool install; a write to the user's package cache or to this repository's tool manifest.                                                                                                                                                                                       |
| C-5 | Installing the tool and generating the tree happen before the measurement.                                                                                                                                                                                                                                   | An install, a restore or a tree generation inside a measured invocation.                                                                                                                                                                                                                        |
| C-6 | Cold means a fresh process: no state carries from one measured invocation to the next inside the tool.                                                                                                                                                                                                       | Reusing a process; a claim about the first run after a reboot or with the operating system's file cache flushed, which no run can repeat.                                                                                                                                                       |
| C-7 | The peak memory of the measured tool's own process - not the `dotnet` command that launches it, nor the benchmark process - is a reported finding, never a threshold; the library's allocation column is the benchmark process's, and the finding labels it so.                                              | Reporting the launcher's or the benchmark process's memory as the tool's; presenting the allocation column as the tool's; a threshold on either number.                                                                                                                                         |
| C-8 | The baseline tool is generated by the benchmark code, does nothing and exits `0`, and the benchmark code generates, packs and installs it inside the benchmark's temporary directory before any measurement, as C-4 and C-5 hold for `specht`.                                                               | A baseline copied from another package, a template or `specht`; a baseline that reads a tree, prints or loads `specht`'s assemblies; the benchmark target or any build target packing it; generating, packing or installing it inside a measured invocation or outside the temporary directory. |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-09, 2026-10-11 (row 8 annotated, `0001-F2` decision 0010) -->

| #   | Item                                                                  | Exclusion Reason                                                                                                                                                                                                                                                                                                        |
| --- | --------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | How the check's cost grows with the tree                              | `0109-F3`.                                                                                                                                                                                                                                                                                                              |
| 2   | The first run after a reboot, or with the file cache flushed          | Decision 0001: not repeatable; C-6.                                                                                                                                                                                                                                                                                     |
| 3   | The install's own time                                                | Not asked for; a consumer pays it once per restore, not per call (C-5).                                                                                                                                                                                                                                                 |
| 4   | The tool process's memory                                             | Withdrawn 2026-10-09: now measured, B-006.                                                                                                                                                                                                                                                                              |
| 5   | A baseline of the bare .NET host starting with no tool                | Withdrawn 2026-10-09: now measured, B-007.                                                                                                                                                                                                                                                                              |
| 6   | Commands other than the check - `init`, `upgrade`, `--explain`        | Not asked for; the check is the command every call site runs, and `--help` is measured only as B-008's host row.                                                                                                                                                                                                        |
| 7   | Native ahead-of-time compilation or trimming of the tool              | A packaging change, not a measurement; an ADR's if a finding prompts it.                                                                                                                                                                                                                                                |
| 8   | Adding `--version`, or any other no-op, to the tool for the benchmark | A product change outside this epic; B-008 measures `--help`, which exists (`0001-F2` B-008). Noted 2026-10-11: the owner has since asked for `--version` as a product change of epic `0001` (`0001-F2` B-040, decision 0010), not for the benchmark. B-008 still measures `--help`; whether the host row moves is OQ-3. |

## 6. Concern Separation

<!-- last written by: implementer, 2026-10-13 -->

| Concern                                                         | Kind      | Owner / boundary                                                                                            |
| --------------------------------------------------------------- | --------- | ----------------------------------------------------------------------------------------------------------- |
| Consumer-visible launch, host and one-specification check costs | Business  | Three rows, fixed input, ratios to the empty tool; C-1                                                      |
| Package identity and isolated installation                      | Both      | Setup reads the packed package and installs both tools in one temporary local manifest; B-001, B-005, B-007 |
| Synthetic clean input                                           | Both      | Reuse the harness tree and store; no consumer tree; A-2                                                     |
| Fresh processes, argument forwarding and exit failures          | Technical | One launch per benchmark call, async draining and waiting; B-002 to B-004                                   |
| Statistical timing, allocations and exporters                   | Technical | Existing BenchmarkDotNet configuration and target; no second timing loop                                    |
| Tool-process identity and lifetime peak memory                  | Both      | External observation of the actual tool process; B-006, C-7; mechanism must be proven before implementation |
| Main-only execution and result publication                      | Technical | Existing harness integration, unchanged; 0109-F1 B-017 to B-020                                             |

## 7. Technical Design

<!-- last written by: implementer, 2026-10-13 -->

### Implemented timing slice

The timing slice is implemented by [ColdStartBenchmarks](../ColdStartBenchmarks.cs)
and [ColdStartSession](../ColdStartSession.cs). The memory mechanism remains
proposed in [ADR-0001](adr/0001-observe-the-installed-tool-process.md); B-006 is
not implemented. The existing project, BenchmarkConfiguration,
BenchmarkProjectGenerator and Program remain the harness. No product code,
workflow, target, dependency or test-project reference was added.

The three methods share the session launcher: EmptyTool is BenchmarkDotNet's
baseline, Help forwards --help, and Check forwards --root tree. Each async call
starts and awaits a fresh `dotnet tool run` invocation. The caller-selected job
and existing out-of-process toolchain are unchanged. GlobalSetup and
GlobalCleanup own session installation and disposal; no IterationSetup,
SimpleJob override or second timing loop is used. The method descriptions label
Allocated as benchmark-process allocation in both exporters. Tool-process peak
memory is unmeasured, not a zero or an allocation proxy.

The measured boundary includes process creation, local-manifest resolution,
child tool execution and output draining. It does not isolate the tool child's
internal timestamps. The common empty baseline includes the same launch wrapper;
ratios are not a subtraction of host and engine costs. The OS file cache is warm
from setup. Redirected output, SDK/runtime and harness allocation are named in
the finding.

### Installation and ownership

ColdStartSession finds the repository by walking ancestors of its assembly
location until build.sh and the benchmark project exist, following the existing
project generator's approach. It uses DOTNET_HOST_PATH, the current dotnet host,
or PATH to resolve one executable shared by setup and measured calls. The
repository's global.json is copied into the session so SDK roll-forward policy
also applies outside the repository.

The target already packs Release when it selects the Startup namespace. Setup
reads each package's root nuspec and selects exactly one specht.tool identity,
then copies that package into a private source directory. Missing or ambiguous
packages fail setup; versions are read from metadata, not inferred from names.
The test-only PackedTool helper is not referenced: unlike that helper this code
never packs the real tool or asserts through a test library.

Within the private session, setup generates a console tool with no dependency or
project references, fixed synthetic package identity/version and a body of only
`return 0;`. The generated project targets net10.0, matching the benchmark
project. It restores from installed SDK framework assets using the sole local
package source, disables audit for that restore, and packs Release with no
restore and shared compilation disabled. The build target does not pack this
empty tool.

The local manifest is explicit and isRoot, and the NuGet configuration clears
all inherited sources in favor of the session source alone. Both installs name
an exact version, --local, the explicit manifest/config and disabled manifest
auto-creation. DOTNET_CLI_HOME and NuGet package/HTTP/plugin/scratch caches and
process temporary directories belong to the session; environment changes apply
only to child processes. Resolver-cache isolation was observed in the manual
probe. No feed, global install, user-cache fallback or repository manifest edit
is part of setup.

The session resolves the registered SpechtResources and SpecManifestParser
outside measurement, supplying the shipped manifest bytes, parsed structure and
pinned version to SpechtTree, then saves it through SpechtTreeStore. This follows
CheckBenchmarks after the manifest-resource refactor; no consumer tree or
generator type is copied or moved. Setup smoke
runs the empty tool and help, then checks JSON from the installed tool for
exactly one specification and no violations. Timed checks use normal text
output. The production-tree refactor remains separate work.

### Process and cleanup behavior

ColdStartSession passes an ArgumentList without a shell, runs from the private
root manifest, closes stdin and concurrently drains stdout/stderr while awaiting
exit. A nonzero exit throws with the row/stage and exit code. Session/repository
path prefixes in command diagnostics are sanitized. Successful output is returned
to the benchmark without being logged. Every process is disposed.

CreateAsync owns cleanup when setup throws; GlobalCleanup disposes a successful
session. All manifests, installed packages, resolver caches, generated source,
compiled baseline and synthetic input are removed with that session. Retained
benchmark results use the existing .artifacts/benchmarks exporters and uploads.

BenchmarkDotNet's Markdown summary reports the ratios against EmptyTool. Its
Full JSON exporter supplies raw measurements, statistics and managed-allocation
metrics, with the allocation scope in MethodTitle; it does not export the
formatted ratio column. No ratio formula or replacement exporter is introduced.
Machine consumers can read the raw results; stored-baseline comparison remains
the later Feature the epic names.

### Tool-process memory

The existing MemoryDiagnoser remains enabled and reports **benchmark-process
managed allocation**. It cannot satisfy B-006. An observer must identify the
actual installed tool process for each invocation and obtain that process's
lifetime resident-memory high-water mark, normalize it to bytes, and associate it
with the row. A maximum of periodic working-set samples, the launcher's peak,
process-tree aggregate, managed heap size or zero for an unavailable reading is
not an acceptable substitute.

The OS mechanism is deliberately not assumed. The local-tool command and tool
can be distinct processes; rapid exits can make discovery and post-exit reads
race. In particular, the .NET Process documentation reports PeakWorkingSet64 as
zero on macOS. The feasibility investigation must prove identity, complete
lifetime coverage and retrieval after a very short invocation on Linux and
Windows CI and document macOS behavior. See ADR-0001 for candidate mechanisms,
rejected shortcuts and the go/no-go evidence.

Once proven, the observer belongs to this Feature, outside `src/`. Prefer a
concrete observer used by the three launch paths, with OS-specific operations
only where the evidence requires them; do not introduce a generic process
framework. Capture raw per-invocation peaks outside the timed result processing.
Publish the maximum observed tool peak per row, the observation count, unit and
scope through BenchmarkDotNet metrics/columns and its Full JSON and Markdown
exporters. Prove the chosen extension actually reaches both exports before
relying on the existing main upload glob. It must not replace the allocation
column or add a second report with incompatible units silently.

The observer's overhead and any change to the launched process must be recorded.
If only an injected startup hook, a suspended runtime or a separate diagnostic
pass makes the measurement possible, document the perturbation and revisit
B-006's per-invocation meaning before adopting it; this design grants no silent
exception to the approved claim. An unsupported platform or missing observation
fails the memory-enabled run with an actionable reason; it reports no invented
peak. The timing-only slice may be delivered first with B-006 explicitly still
Missing. The Feature is not complete until the memory claim is delivered.

### Delivery sequence

The Tasks table owns claim allocation. Timing/package work can be completed and
verified independently of the memory investigation. The memory implementation
requires both the working timing session and the proven observer decision. They
share this specification and may edit the same Startup files, so only one item
is taken in this Feature at a time; no parallel-write claim is made here.

The finished Feature uses the existing main-only Short runs on Linux and Windows;
no automatic benchmark is added to pull requests. Local Dry establishes that the
three cases run, and the default job provides a recorded finding. Neither Dry nor
shared-runner Short results establish a regression threshold.

### References checked for this design

- [BenchmarkDotNet setup/cleanup](https://benchmarkdotnet.org/articles/features/setup-and-cleanup.html)
  and [baselines](https://benchmarkdotnet.org/articles/features/baselines.html):
  setup stays outside measurement and the baseline method supplies ratios.
- [.NET local tool installation](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-tool-install)
  and [CLI environment](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-environment-variables):
  explicit manifest, exact version, sole local source and private CLI/cache paths.
- [Process platform notes](https://learn.microsoft.com/en-US/dotnet/api/System.Diagnostics.Process):
  peak-working-set portability cannot be presumed.

## 8. Testing Strategy

<!-- last written by: test-writer, 2026-10-13 -->

The owner's benchmark-code proof ruling recorded in 0109-F1 § 10 applies:
no tests of benchmark code or acceptance wiring were added. The timing claims
are recorded as inspected mechanisms and manual observations below, with Missing
retained in § 9 Status. B-006 remains unimplemented. The Feature is unfinished.

### Manual verification

- The real target `./build.sh Benchmark --benchmark-filter '*ColdStartBenchmarks*'
--benchmark-job Dry` selected all three cases, packed specht first, installed
  both tools, completed all cases and wrote Markdown and Full JSON. Dry verifies
  execution only; the default-job finding below is separate evidence. The same
  three cases also passed the Short job main CI selects.
- A temporary manual probe called the same session and launcher. It inspected
  both installed package identities and the private resolver cache, ran all
  three invocations, and observed unchanged user resolver-cache contents and no
  empty-tool installation in the user's NuGet cache after cleanup.
- The probe changed the synthetic specification to invalid frontmatter: the
  installed check exited 1 and the launcher threw with sanitized diagnostics.
  A temporary replacement of the installed synthetic tool exited 7 and likewise
  threw. A temporary benchmark using the same session corrupted its clean input
  after setup; the real Benchmark target failed with exit 1, and that probe class
  was removed before delivery. An immediate-exit variant printing its PID produced distinct tool PIDs
  on repeated calls. A variant writing 1 MiB each to stdout and stderr completed
  with its output intact. These variants are manual probes only; the shipped
  empty tool body remains `return 0;`.
- Normal disposal deleted the session. With package output temporarily absent,
  setup failed and left no new session directory. Probe variants were removed
  with the private session; no probe or functional test ships.
- A default-job run records timing and allocation below. Markdown ratios and
  Full JSON's raw measurements/statistics/allocation scope are inspected, without
  claiming a formatted ratio field the library does not export.

The manual probes also passed with network access restricted. The full local
build passed Format, Compile and all three test tiers: 601 Unit, 238 Integration,
and acceptance 200 passed / 210 skipped. Specht reported zero errors and the same
141 existing warnings. PR checks are recorded in the delivery report. The existing main-only Short run will publish these cases on Linux and
Windows after merge; platform evidence is not inferred from a local macOS run.
The PR workflow remains free of automatic benchmark and upload steps.

### Performance

<!-- last written by: benchmarker, 2026-10-13 -->

| Concern                                                 | Source             | Benchmark                                  | Finding                                                                                                                                                |
| ------------------------------------------------------- | ------------------ | ------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Fixed consumer invocation cost against the launch floor | C-1, C-3, C-6      | ColdStartBenchmarks.EmptyTool, Help, Check | Empty tool 203.6 ms; help 338.0 ms (1.66 ratio); one-specification check 522.9 ms (2.57 ratio). Includes manifest-mediated launch and output draining. |
| Benchmark-process managed allocation per invocation     | C-7; 0109-F1 B-007 | ColdStartBenchmarks.EmptyTool, Help, Check | 75.59 KB, 81.64 KB and 76.66 KB respectively; these are wrapper allocations, not tool-process memory.                                                  |
| Tool-process peak memory                                | B-006, C-7         | Not implemented                            | Not measured. Observer feasibility and reporting are separate work; no substitute figure is reported.                                                  |

Measured 2026-10-13 using the library's default job, Release, out of process,
without an attached debugger: `./build.sh Benchmark --benchmark-filter
'*ColdStartBenchmarks*'`. All three cases succeeded. BenchmarkDotNet 0.15.8;
macOS Tahoe 26.4 (25E246), Darwin 25.4.0; Apple M1 Max, 10 logical/physical cores;
.NET SDK 10.0.401; .NET 10.0.12 (10.0.12, 10.0.1226.42308), Arm64 RyuJIT;
Concurrent Workstation GC. The runner could not raise process priority.

| Invocation               | Mean     | Error (99.9% CI half-width) | StdDev   | Ratio | RatioSD | Benchmark allocated |
| ------------------------ | -------- | --------------------------- | -------- | ----- | ------- | ------------------- |
| Empty tool               | 203.6 ms | 2.40 ms                     | 2.01 ms  | 1.00  | 0.01    | 75.59 KB            |
| Help                     | 338.0 ms | 6.70 ms                     | 9.39 ms  | 1.66  | 0.05    | 81.64 KB            |
| Check, one specification | 522.9 ms | 7.33 ms                     | 11.83 ms | 2.57  | 0.06    | 76.66 KB            |

Setup packs/installs the empty tool, installs the real package, saves the tree
and smoke-runs each path before timing. Every measured call starts a new process;
OS file caches, SDK/tool-resolution state and package files are warm. Stdout and
stderr are redirected and drained; help includes formatting its usage output and
check includes its ordinary text report. The allocation column covers the
benchmark's launcher/tasks/output buffers, not either child's managed heap or
resident memory. No tool-memory observer is installed.

The library retained 13, 27 and 34 measurements respectively and removed 2, 1
and 11 outliers. The check's removed outliers ranged from about 599 ms to 1.70 s,
so this is one environment's finding, not a latency guarantee. Ratios are the
library's ratio-distribution means, not a separately computed ratio of means.
The host and check rows exceed the empty tool in this run; subtraction does not
isolate composition or engine time. No previous cold-start finding is available
for a regression comparison and no number gates the build.

## 9. Traceability Matrix

<!-- last written by: test-writer, 2026-10-13 -->

| Claim ID | Scenario                                                    | Test                                                                                            | Status  |
| -------- | ----------------------------------------------------------- | ----------------------------------------------------------------------------------------------- | ------- |
| B-001    | The packed tool is installed the way a consumer installs it | No test. ColdStartSession.InstallAsync selects metadata and installs locally; § 8 manual probe. | Missing |
| B-002    | A check by the packed tool is measured start to exit        | No test. ColdStartBenchmarks.Check and RunAsync; § 8 Dry/default runs.                          | Missing |
| B-003    | Every measured invocation is a fresh process                | No test. DotNetAsync starts a process per call; § 8 distinct-PID probe.                         | Missing |
| B-004    | A tool that fails is not timed                              | No test. DotNetAsync exit guard; § 8 broken-tree and exit-7 probes.                             | Missing |
| B-005    | A run leaves no tool installed behind it                    | No test. CreateAsync failure cleanup and Dispose; § 8 cache/cleanup probes.                     | Missing |
| B-006    | Every measured invocation reports the tool's own memory     | Missing                                                                                         | Missing |
| B-007    | A tool that does nothing is measured as the baseline        | No test. InstallAsync generated no-op and EmptyTool baseline; § 8 runs/probe.                   | Missing |
| B-008    | The tool's help is measured start to exit                   | No test. ColdStartBenchmarks.Help; § 8 Dry/default runs.                                        | Missing |
| B-009    | Each measurement is reported against the baseline           | No test. EmptyTool baseline and library Markdown ratio; § 8 default finding.                    | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-13 -->

- 2026-10-09, spec-reviewer finding on commit `4b23e76`: B-003 restated with this Feature as subject. A-1 follows `0109-F1` B-013, which now packs only when this Feature's benchmark is selected.
- 2026-10-09, the owner resolved OQ-1 and OQ-2. OQ-1 reverses its default: the tool process's memory is measured; B-006 added, C-7 restated, § 5 row 4 withdrawn. OQ-2: the fixed cost is measured in three rows - a generated tool that does nothing as the baseline, `specht --help`, and the check; B-007 to B-009 and C-8 added, C-1 restated, A-3 added, § 5 row 5 withdrawn and row 8 added. `--help` is the host row because the tool has no `--version`, and adding one is a product change (§ 5 row 8). B-004's source cites the owner's answer to `0109-F1` OQ-3. `0109-F1` C-4 holds unchanged: the allocation column stays, and B-006 reports beside it. Decision 0001 is unchanged: the checked invocation B-002 measures is still the one a call site runs, and B-008 is a row beside it, not a replacement; decision 0002 records the three-row call. On spec-reviewer findings before commit: B-006 and C-7 name the measured tool's own process, apart from the `dotnet` launcher and the benchmark process, and leave how and when it is read to § 7; C-8 gives packing the baseline tool to the benchmark code, not the benchmark target (orchestrator default, 2026-10-09, not the owner's), so A-1 and `0109-F1` B-013 are unchanged.

- 2026-10-11, the owner asked for `specht --version` (`0001-F2` decision 0010; `0001-F2` B-040 to B-044), so "the tool has no `--version`" in the entry above and in decision 0002 is true of 2026-10-09 only. § 5 row 8 and decision 0002 annotated. No claim, constraint or scenario changed: B-008 measures `--help`. OQ-3 raised.

- 2026-10-13, decomposition and design added for the approved agreement. No claim,
  constraint or scenario changed. The design retains OQ-3's --help reading and
  requires evidence of a tool-process memory observer before its implementation;
  it does not treat launcher allocation as B-006.

- 2026-10-13, timing implementation added without changing any claim, constraint
  or scenario. Inspection corrected the design's assumption that Full JSON
  exports the formatted ratio column: Markdown carries ratios and JSON carries
  raw measurements and allocation metrics. B-009 remains satisfied by the
  summary; B-006 remains separate, unimplemented memory work.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-09, 2026-10-11 (OQ-3 raised) -->

| ID   | Question                                                                                                                                                                                                                                                                                                                                                                                                         | Blocks    | Resolution                                                                                                                                                                                              |
| ---- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | Is the tool process's own memory measured? The library's memory diagnoser sees only the process that launches the tool, so the owner's "memory on every benchmark" yields the launcher's allocation here; the tool's would need its peak working set read outside the library's diagnoser. Default: not measured; C-7 states it.                                                                                 | § 5 row 4 | Resolved 2026-10-09: measured, reversing the default; B-006 added and C-7 restated (owner).                                                                                                             |
| OQ-2 | Is the bare .NET host's start measured beside the tool's, as the baseline, so the ratio isolates what `specht` adds? Default: no; the finding is the tool's start-to-exit time alone.                                                                                                                                                                                                                            | § 5 row 5 | Resolved 2026-10-09: yes, three rows - a generated tool that does nothing as the baseline, `specht --help`, and the check of the one-specification tree; B-007 to B-009 added and C-1 restated (owner). |
| OQ-3 | The tool gains `--version` (`0001-F2` B-040), which reads no tree and prints one line. Does the host row move from `specht --help` to `specht --version`? Decision 0002 chose `--help` because no `--version` existed, at the cost of a host row that prints usage. Readings: (a) B-008 stays on `--help`; (b) B-008 is amended to measure `--version`; (c) both are measured. Raised 2026-10-11 by spec-author. | B-008     | Open. Owner not asked. Proceeds on (a): B-008 is unchanged.                                                                                                                                             |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-13 (historical rows preserved in the history table) -->

| Section | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         |
| ------- | ------ | ------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| 1-5     | 🟢     | spec-reviewer | Round 2 (2026-10-09) - approved, reviewing the revised uncommitted diff on `cdb0876`. Round 1's blocker is met: A-1 has the target pack only `specht`, and C-8 gives the baseline tool's generation, packing and install to the benchmark code and rules out a build target packing it. B-006, C-7 and the `@B-006` scenario name the measured tool's own process, apart from the `dotnet` launcher and the benchmark process, and leave the read point to § 7. Decision 0002 records the three rows, the memory reversal, the bare host and `dotnet --version` rejected, and the packing split as the orchestrator's default. Non-blocking (spec-author): decision 0001 still describes a no-op invocation as turned down, and its Rejected `--help` entry still says OQ-2 is pending, with no pointer to decision 0002; add one under Affects or Reversal. |
| 1-5     | 🟢     | spec-reviewer | Round 3 (2026-10-11) - approved, reviewing `origin/main` (`5c22c84`)...`28d0ba6`. § 5 row 8 is annotated and not reworded; decision 0002 gains a dated note under its text, marked as no reversal; OQ-3 is new and clashes with no open pull request. B-008 still measures `--help`, so no claim, constraint or scenario changed and `spec_status` stays `approved`. Non-blocking (spec-author): OQ-3 is the owner's, and the owner was not asked.                                                                                                                                                                                                                                                                                                                                                                                                           |

### Review history

Earlier review rows are preserved verbatim; later approvals and the current record supersede them.

| Section | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       |
| ------- | ------ | ------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-5     | 🟡     | spec-reviewer | Awaiting first review.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     |
| 1-5     | 🔴     | spec-reviewer | Round 1 (2026-10-09) - blocked, reviewing the uncommitted OQ answers on `cdb0876`. OQ-1 and OQ-2 are resolved in place; § 5 rows 4 and 5 are kept and marked withdrawn; B-006 to B-009 and C-8 are new ids, none reused; § 9 rows and `@B-006` to `@B-009` match; A-3 cites `0001-F2` B-008, which holds. Blocking (spec-author): A-1 says `0109-F1` B-013's target packs the baseline package, C-8 says the benchmark generates, packs and installs it inside its temporary directory - one owner, as `0109-F1`'s round 1 states. Non-blocking (spec-author, escalate to the owner): B-006 and C-7 read the peak working set of "the child process" at its exit, but a tool run through a local tool manifest is launched by the `dotnet` CLI, which is likely the direct child, with the tool its child - verify which process and name it in B-006 rather than pinning a read point in C-7. (spec-author) decision 0001's description, Rejected `--help` entry, Affects and Reversal predate OQ-2's answer; record the three-row call, and the synthetic tool chosen over the bare host, in a decision. |

## Tasks

<!-- last written by: spec-author, 2026-10-13 -->

Cut from the approved agreement into [`../.issue/`](../.issue/):

| Item | Scope                                                               | Claims                            |
| ---- | ------------------------------------------------------------------- | --------------------------------- |
| 0203 | Feature container                                                   | B-001 to B-009                    |
| 0204 | Installed tools, three timing rows, ratios and cleanup              | B-001 to B-005, B-007 to B-009    |
| 0205 | Feasibility investigation of tool identity and lifetime peak memory | No delivered claim; informs B-006 |
| 0206 | Proven tool-process memory observation and exports                  | B-006                             |

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
