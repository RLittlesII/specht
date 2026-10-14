---
title: "ADR-0001: Observe the installed tool process without changing the product"
description: "Keep timing through the local tool manifest, isolate installations and require evidence for per-tool lifetime peak memory before selecting an OS observer"
type: adr
---

# ADR-0001: Observe the installed tool process without changing the product

## Status

proposed — 2026-10-13. The timing/session design is concrete; the OS memory
mechanism is not selected until feasibility evidence meets the conditions below.
The investigation below concluded with a no-go for implementation. This is not
an accepted portability claim or a completed benchmark finding.

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

## Feasibility result

Investigated 2026-10-13; **no observer adopted**. Native lifecycle observation
can identify rapid tools on both hosted systems, but the probes do not establish
an acceptable complete-lifetime measurement for the approved invocation. The
memory implementation remains on hold; B-006 is not delivered. This is a negative
design result, not a claim that native observation is impossible.

The reproducible probe source is retained at
[commit 825ef4ca](https://github.com/RLittlesII/specht/commit/825ef4ca), under
`.artifacts/0205/probe/`. The temporary source and branch-only probe workflow
were removed from the delivered tree. The
[successful hosted run](https://github.com/RLittlesII/specht/actions/runs/38498985256)
contains the Linux and Windows artifacts. Its timestamps are 2026-10-14 UTC,
2026-10-13 America/Chicago. Local macOS observations below were made separately.

### Reproduction and identity

Fetch the cited commit into a separate disposable checkout, then run
`python .artifacts/0205/probe/run.py` (`python3` where required). It needs Python 3
and SDK 10.0.401 selected by the repository's global.json; framework runtime
roll-forward remains enabled. The temporary workflow names ubuntu-latest and
windows-latest and installs that SDK. The script creates a private local source,
NuGet cache, CLI home, temp directories and root tool manifest; it restores and
packs a reference-free Release tool and installs version 1.0.0 from that source.
No product assembly, startup hook, diagnostic port or real specification is used.

The synthetic modes are `noop` (return immediately without output), `pid` (print
PID and return), and `held 0` / `held 128` (touch each 4096-byte page of the named
MiB allocation, print PID, retain the array and wait for a release file). The
observer releases held tools only after acquiring their handles and live peaks.
The later lifecycle probes reuse the existing release file, so those allocations
exit without an observer handshake. Each platform first runs 30 PID-returning
launches and 10 no-output launches, then the hosted observers run ten invocations
of each of noop, zero MiB and 128 MiB. These are mechanism probes, not benchmark
measurements or tests of benchmark code.

The live probes distinguish tool PID from the directly started launcher PID.
Native observers independently identify the installed Probe.dll in exec/create
command lines. Linux captures fork/vfork, exec and exit with ptrace; Windows uses
CreateProcess DEBUG_PROCESS and CREATE_PROCESS/EXIT_PROCESS_DEBUG_EVENT, querying
the command line with NtQueryInformationProcess information class 60. That native
information class is an internal compatibility risk, not an approved product
identity API. Matching a basename suffices only for this controlled synthetic
session; an implementation must match the resolved installed assembly and the
specific invocation, reject multiple matches and retain the process lifetime.

### Observations

All byte values below are synthetic probe evidence, not cold-start benchmark
findings. Peaks include shared and private resident pages; the 128 MiB array is
an input contrast, not an expectation that the OS peak equals the array size.

| Environment                                 | Effective SDK / runtime | Live tool / launcher peak, zero MiB | Live tool / launcher peak, 128 MiB |
| ------------------------------------------- | ----------------------- | ----------------------------------- | ---------------------------------- |
| Ubuntu 24.04.5 x64, Linux 6.17.0-1022-azure | 10.0.401 / 10.0.13      | 34,230,272 / 66,633,728 bytes       | 173,035,520 / 66,703,360 bytes     |
| Windows Server 2025 x64, build 26100        | 10.0.401 / 10.0.13      | 24,342,528 / 45,662,208 bytes       | 160,673,792 / 45,666,304 bytes     |
| macOS 26.4 Arm64, Apple M1 Max              | 10.0.401 / 10.0.12      | 0 / 0 from the .NET API             | 0 / 0 from the .NET API            |

For example, Linux launcher/tool PIDs were 2679/2688 and 2705/2713; Windows
6880/9004 and 6512/9952. Local macOS initially observed 72956/72957 and
72958/72960. The low-allocation launcher does not acquire the tool's 128 MiB
contrast. Thirty immediate PID-returning tools could be opened on each platform
in these runs; this is not a discovery guarantee for the silent baseline. The
unobserved silent runs carry an explicit missing identity, never a zero peak.
After exit and Refresh, PeakWorkingSet64 threw InvalidOperationException on all
three platforms despite retaining the Process object/handle. Windows native
GetProcessMemoryInfo on the retained handle still succeeded, including
160,813,056 bytes for the high-allocation tool; .NET failure does not prove
native handle failure. macOS returns no usable peak even while the tool is live;
[runtime 10.0.12's implementation](https://github.com/dotnet/runtime/blob/v10.0.12/src/libraries/System.Diagnostics.Process/src/System/Diagnostics/ProcessManager.OSX.cs)
sets the current resident size without supplying a peak.

| Lifecycle observer                                        | No-output immediate tools   | Zero MiB tools              | 128 MiB tools                 | Coverage                                                  |
| --------------------------------------------------------- | --------------------------- | --------------------------- | ----------------------------- | --------------------------------------------------------- |
| Linux ptrace, VmHWM at exit stop                          | 27,357,184–27,590,656 bytes | 34,054,144–34,631,680 bytes | 172,920,832–173,215,744 bytes | 30 identities and 30 exit observations for 30 invocations |
| Windows debugger, native PeakWorkingSetSize at exit event | 24,281,088–24,412,160 bytes | 25,751,552–25,845,760 bytes | 161,996,800–162,045,952 bytes | 30 identities and 30 exit observations for 30 invocations |

Windows PeakWorkingSetSize is a per-process OS peak in bytes, retrieved before
ContinueDebugEvent allows shutdown to complete. No descendant sum is used.
[Microsoft's event contract](https://learn.microsoft.com/en-us/windows/win32/debug/debugging-events)
keeps the process handle and address space available at that event;
[the counter definition](https://learn.microsoft.com/en-us/windows/win32/api/psapi/ns-psapi-process_memory_counters)
distinguishes this field from current working set and pagefile/commit counters.
The synthetic high-allocation contrast and immediate exits support this
candidate's attribution and retrieval lifetime on the tested hosted image.

Linux VmHWM is the kernel's resident high-water counter, not a maximum of sampled
current RSS; its kB value was multiplied by 1024. However, an exit stop occurs
before the thread completes exit, not necessarily after every thread has ceased
allocating. The final wait4 ru_maxrss (also multiplied by 1024) was
65,945,600–66,338,816 bytes for the noop tool, materially different from its
post-exec VmHWM. No equality or subtraction is assumed. Linux wait accounting
can also include reaped descendants: kernel
[wait handling](https://github.com/torvalds/linux/blob/v6.11/kernel/exit.c) calls
[getrusage with RUSAGE_BOTH](https://github.com/torvalds/linux/blob/v6.11/kernel/sys.c).
These sources explain scope risks; they are not a diagnosis of the exact
pre-exec/inheritance contribution on the tested 6.17 kernel. Thus neither an
early exit snapshot nor wait4 is adopted as the tool-only complete-lifetime
metric. The [proc documentation](https://man7.org/linux/man-pages/man5/proc_pid_status.5.html)
also explicitly qualifies VmHWM accounting accuracy.

Local macOS wait4 on the launcher returned 83,312,640 bytes for noop,
83,083,264 for zero MiB and 183,648,256 for 128 MiB. macOS getrusage units are
bytes (the local system manual), unlike Linux KiB. The high-allocation contrast
in a launcher wait demonstrates why that value cannot be attributed to the tool
alone. No per-tool macOS native observer was proven. Apple's
[resource definitions](https://github.com/apple-oss-distributions/xnu/blob/main/bsd/sys/resource.h)
expose current resident size and a lifetime maximum physical footprint; a
footprint must not silently replace resident peak. Unsupported macOS may fail
explicitly under the existing design; it need not pretend to produce a value.

### Permissions, perturbation and failures

The hosted probes used the runners' default accounts, own-child tracing/debugging
and OS-provided libraries. No sudo, capability changes, SeDebugPrivilege enable,
service, driver, startup injection or CI privilege-policy change was used.
A restricted Windows user token was not separately exercised, and Linux policy
can deny ptrace; success here is evidence for these hosted images, not all user
or container policies. Dependencies were Python 3, .NET and native OS APIs; no
additional NuGet or system package was installed.

Both mechanisms perturb scheduling. Linux stops at fork/vfork, exec and exit;
Windows suspends threads while servicing debugger events, including module and
thread creation. Windows also launches under a debugger. This is stronger than
an observer merely reading a resident counter. Their cost was not timed and no
performance comparison is claimed; these values cannot enter the existing
unattached cold-start timing findings. A separate diagnostic pass or accepting
suspension in measured invocations requires explicit design/claim review.

The first Linux probe failed on ESRCH when continuing an exiting traced thread.
The second also encountered ESRCH retrieving a clone-event message. Both failed
runs remain linked by the probe commit history and
[first](https://github.com/RLittlesII/specht/actions/runs/38498591426) and
[second](https://github.com/RLittlesII/specht/actions/runs/38498743157) run records.
Removing unnecessary thread-clone tracing produced the final successful 30-run
process-only observation; it does not prove arbitrary multithreaded exit
correctness. The Python driver limits subprocess duration to 180 seconds and
removes its private session in finally; Linux EXITKILL and Windows default
debugger-exit behavior bound debuggee lifetime. The prototypes are not hardened
observers: Windows records API and tool-exit failures but its return value checks
identity count only. Fault forwarding, cancellation, denied permissions, handle
ownership and lost-event behavior require explicit fail-closed implementation
proof. No missing or failed native result was accepted as a valid zero finding.

### Remaining design gate and exports

Before implementing memory, resolve the Linux metric's full-lifetime boundary
and exclude descendant accounting; demonstrate terminal coverage for all tool
threads, immediate exits and failures. Review whether native stop/debug behavior
is compatible with per-invocation timing, or amend the agreement before choosing
a diagnostic pass or startup synchronization. Validate permissions for the
intended runner policy and fail explicitly on unsupported platforms. No claim,
scenario or timing benchmark is changed by this investigation.

The existing proposed export contract remains maximum tool peak per startup
row, observation count, bytes, scope and observer identity, separately from
benchmark-process managed allocation. BenchmarkDotNet 0.15.8's
[Full JSON exporter](https://github.com/dotnet/BenchmarkDotNet/blob/v0.15.8/src/BenchmarkDotNet/Exporters/Json/JsonExporterBase.cs)
serializes report Metrics, but this source inspection does not prove transport
from each benchmark subprocess or a Markdown column. Once an observer is agreed,
prove both exporters carry the same scoped values and counts before adoption.
No exporter extension, fabricated measurement or new report schema is delivered
by this spike.
