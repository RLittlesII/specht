---
title: "Specification: Tool cold start"
description: "A benchmark of the packed specht tool as a consumer runs it - installed from the local package output into a temporary local tool manifest, started as a fresh process for each measured invocation - timing a generated tool that does nothing as the baseline, specht --help, and a check of a one-specification tree, each as a ratio of the baseline, with the tool process's peak memory as a recorded finding"
type: feature
id: "F4"
epic: "0109"
spec_status: approved
status: blocked
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
updated: "2026-10-09"
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

<!-- last written by: spec-author, 2026-10-09 -->

| #   | Item                                                                  | Exclusion Reason                                                                                                 |
| --- | --------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------- |
| 1   | How the check's cost grows with the tree                              | `0109-F3`.                                                                                                       |
| 2   | The first run after a reboot, or with the file cache flushed          | Decision 0001: not repeatable; C-6.                                                                              |
| 3   | The install's own time                                                | Not asked for; a consumer pays it once per restore, not per call (C-5).                                          |
| 4   | The tool process's memory                                             | Withdrawn 2026-10-09: now measured, B-006.                                                                       |
| 5   | A baseline of the bare .NET host starting with no tool                | Withdrawn 2026-10-09: now measured, B-007.                                                                       |
| 6   | Commands other than the check - `init`, `upgrade`, `--explain`        | Not asked for; the check is the command every call site runs, and `--help` is measured only as B-008's host row. |
| 7   | Native ahead-of-time compilation or trimming of the tool              | A packaging change, not a measurement; an ADR's if a finding prompts it.                                         |
| 8   | Adding `--version`, or any other no-op, to the tool for the benchmark | A product change outside this epic; B-008 measures `--help`, which exists (`0001-F2` B-008).                     |

## 6. Concern Separation

<!-- last written by: spec-author, 2026-10-09 -->

Pending: owned by `implementer`, written after agreement.

## 7. Technical Design

<!-- last written by: spec-author, 2026-10-09 -->

Pending: owned by `implementer`, written after agreement.

## 8. Testing Strategy

<!-- last written by: spec-author, 2026-10-09 -->

Pending: owned by `test-writer`, written after agreement.

## 9. Traceability Matrix

<!-- last written by: spec-author, 2026-10-09 -->

| Claim ID | Scenario                                                    | Test    | Status  |
| -------- | ----------------------------------------------------------- | ------- | ------- |
| B-001    | The packed tool is installed the way a consumer installs it | Missing | Missing |
| B-002    | A check by the packed tool is measured start to exit        | Missing | Missing |
| B-003    | Every measured invocation is a fresh process                | Missing | Missing |
| B-004    | A tool that fails is not timed                              | Missing | Missing |
| B-005    | A run leaves no tool installed behind it                    | Missing | Missing |
| B-006    | Every measured invocation reports the tool's own memory     | Missing | Missing |
| B-007    | A tool that does nothing is measured as the baseline        | Missing | Missing |
| B-008    | The tool's help is measured start to exit                   | Missing | Missing |
| B-009    | Each measurement is reported against the baseline           | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-09 -->

- 2026-10-09, spec-reviewer finding on commit `4b23e76`: B-003 restated with this Feature as subject. A-1 follows `0109-F1` B-013, which now packs only when this Feature's benchmark is selected.
- 2026-10-09, the owner resolved OQ-1 and OQ-2. OQ-1 reverses its default: the tool process's memory is measured; B-006 added, C-7 restated, § 5 row 4 withdrawn. OQ-2: the fixed cost is measured in three rows - a generated tool that does nothing as the baseline, `specht --help`, and the check; B-007 to B-009 and C-8 added, C-1 restated, A-3 added, § 5 row 5 withdrawn and row 8 added. `--help` is the host row because the tool has no `--version`, and adding one is a product change (§ 5 row 8). B-004's source cites the owner's answer to `0109-F1` OQ-3. `0109-F1` C-4 holds unchanged: the allocation column stays, and B-006 reports beside it. Decision 0001 is unchanged: the checked invocation B-002 measures is still the one a call site runs, and B-008 is a row beside it, not a replacement; decision 0002 records the three-row call. On spec-reviewer findings before commit: B-006 and C-7 name the measured tool's own process, apart from the `dotnet` launcher and the benchmark process, and leave how and when it is read to § 7; C-8 gives packing the baseline tool to the benchmark code, not the benchmark target (orchestrator default, 2026-10-09, not the owner's), so A-1 and `0109-F1` B-013 are unchanged.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-09 -->

| ID   | Question                                                                                                                                                                                                                                                                                                                         | Blocks    | Resolution                                                                                                                                                                                              |
| ---- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | Is the tool process's own memory measured? The library's memory diagnoser sees only the process that launches the tool, so the owner's "memory on every benchmark" yields the launcher's allocation here; the tool's would need its peak working set read outside the library's diagnoser. Default: not measured; C-7 states it. | § 5 row 4 | Resolved 2026-10-09: measured, reversing the default; B-006 added and C-7 restated (owner).                                                                                                             |
| OQ-2 | Is the bare .NET host's start measured beside the tool's, as the baseline, so the ratio isolates what `specht` adds? Default: no; the finding is the tool's start-to-exit time alone.                                                                                                                                            | § 5 row 5 | Resolved 2026-10-09: yes, three rows - a generated tool that does nothing as the baseline, `specht --help`, and the check of the one-specification tree; B-007 to B-009 added and C-1 restated (owner). |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-09 -->

| Section | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       |
| ------- | ------ | ------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-5     | 🟡     | spec-reviewer | Awaiting first review.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     |
| 1-5     | 🔴     | spec-reviewer | Round 1 (2026-10-09) - blocked, reviewing the uncommitted OQ answers on `cdb0876`. OQ-1 and OQ-2 are resolved in place; § 5 rows 4 and 5 are kept and marked withdrawn; B-006 to B-009 and C-8 are new ids, none reused; § 9 rows and `@B-006` to `@B-009` match; A-3 cites `0001-F2` B-008, which holds. Blocking (spec-author): A-1 says `0109-F1` B-013's target packs the baseline package, C-8 says the benchmark generates, packs and installs it inside its temporary directory - one owner, as `0109-F1`'s round 1 states. Non-blocking (spec-author, escalate to the owner): B-006 and C-7 read the peak working set of "the child process" at its exit, but a tool run through a local tool manifest is launched by the `dotnet` CLI, which is likely the direct child, with the tool its child - verify which process and name it in B-006 rather than pinning a read point in C-7. (spec-author) decision 0001's description, Rejected `--help` entry, Affects and Reversal predate OQ-2's answer; record the three-row call, and the synthetic tool chosen over the bare host, in a decision. |
| 1-5     | 🟢     | spec-reviewer | Round 2 (2026-10-09) - approved, reviewing the revised uncommitted diff on `cdb0876`. Round 1's blocker is met: A-1 has the target pack only `specht`, and C-8 gives the baseline tool's generation, packing and install to the benchmark code and rules out a build target packing it. B-006, C-7 and the `@B-006` scenario name the measured tool's own process, apart from the `dotnet` launcher and the benchmark process, and leave the read point to § 7. Decision 0002 records the three rows, the memory reversal, the bare host and `dotnet --version` rejected, and the packing split as the orchestrator's default. Non-blocking (spec-author): decision 0001 still describes a no-op invocation as turned down, and its Rejected `--help` entry still says OQ-2 is pending, with no pointer to decision 0002; add one under Affects or Reversal.                                                                                                                                                                                                                                               |

## Tasks

None yet. Cut from § 3 after agreement.

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
