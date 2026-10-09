---
title: "Specification: Tool cold start"
description: "A benchmark of the packed specht tool as a consumer runs it - installed from the local package output into a temporary local tool manifest, started as a fresh process for each measured check of a one-specification tree - with the tool's start-to-exit time as a recorded finding"
type: feature
id: "F4"
epic: "0109"
spec_status: draft
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

Every call site starts the tool as a new process - the pre-commit hook on every commit, CI on every run, an agent mid-write (brief § 2 Must-1) - so on a small tree the process's start, not the engine, is most of what a person waits for, and nobody knows what it costs. This Feature removes that failure state: the packed tool, installed and invoked the way a consumer's repository does, is measured from process start to exit on the smallest generated tree, one fresh process per measurement, so its fixed cost per invocation is a recorded finding.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-09 -->

| #   | Persona                             | Need                                                                | Pain Point Today                                     |
| --- | ----------------------------------- | ------------------------------------------------------------------- | ---------------------------------------------------- |
| 1   | The maintainer of four repositories | Know what the hook adds to every commit                             | Felt, not measured                                   |
| 2   | `benchmarker`                       | A number for the tool's fixed cost, apart from the engine's work    | The engine benchmarks never start a process          |
| 3   | `implementer`                       | See whether a dependency or a startup change moved the tool's start | A change to the host's composition is never measured |

### Assumptions

| ID  | Assumption                                                                                                                                              |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | The benchmark target packs the tool before any benchmark runs (`0109-F1` B-013), and that package is the one `0055-F7` describes a consumer installing. |
| A-2 | A check of the one-specification generated tree exits `0` (`0109-F1` B-015; `0001-F2` exit codes).                                                      |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-09 -->

| ID    | Claim                                                                                                                                             | Source                      | Status |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------- | ------ |
| B-001 | Given the package the build packed, this Feature installs it from the local package output into a temporary local tool manifest before measuring. | owner, 2026-10-09; C-3; C-4 | Active |
| B-002 | Given the installed tool, this Feature measures one check of the one-specification generated tree from process start to exit.                     | owner, 2026-10-09; C-1      | Active |
| B-003 | Given two measured invocations, each starts its own process.                                                                                      | decision 0001; C-6          | Active |
| B-004 | Given a measured invocation that exits non-zero, this Feature fails instead of reporting a time.                                                  | `0109-F1` OQ-3; C-1         | Active |
| B-005 | Given a run completes, this Feature leaves no tool installed outside its temporary directory.                                                     | `0109-F1` C-10; C-4         | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-09 -->

| ID  | Constraint                                                                                                                                                               | Rules Out                                                                                                                                 |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------- |
| C-1 | The packed tool's start-to-exit time per invocation is a reported finding, never a threshold; it is a fixed cost per invocation, measured on the one-specification tree. | A gate on it; a tree size parameter, which is `0109-F3`'s question; a finding of "fast" or "slow".                                        |
| C-2 | It is measured by the default out-of-process toolchain (`0109-F1` decision 0003).                                                                                        | The in-process toolchain, which cannot measure startup.                                                                                   |
| C-3 | The tool measured is the packed package, run through a local tool manifest as a consumer runs it.                                                                        | Timing the build output's executable, `dotnet run`, or the engine through a project reference.                                            |
| C-4 | The package is installed from the local package output only, with its tool manifest and package cache inside the benchmark's temporary directory.                        | A feed; a global tool install; a write to the user's package cache or to this repository's tool manifest.                                 |
| C-5 | Installing the tool and generating the tree happen before the measurement.                                                                                               | An install, a restore or a tree generation inside a measured invocation.                                                                  |
| C-6 | Cold means a fresh process: no state carries from one measured invocation to the next inside the tool.                                                                   | Reusing a process; a claim about the first run after a reboot or with the operating system's file cache flushed, which no run can repeat. |
| C-7 | The memory column of this Feature's result is the launching process's, and the finding says the tool process's memory is not measured (OQ-1).                            | Reporting that column as the tool's allocation.                                                                                           |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-09 -->

| #   | Item                                                           | Exclusion Reason                                                         |
| --- | -------------------------------------------------------------- | ------------------------------------------------------------------------ |
| 1   | How the check's cost grows with the tree                       | `0109-F3`.                                                               |
| 2   | The first run after a reboot, or with the file cache flushed   | Decision 0001: not repeatable; C-6.                                      |
| 3   | The install's own time                                         | Not asked for; a consumer pays it once per restore, not per call (C-5).  |
| 4   | The tool process's memory                                      | OQ-1.                                                                    |
| 5   | A baseline of the bare .NET host starting with no tool         | OQ-2.                                                                    |
| 6   | Commands other than the check - `init`, `upgrade`, `--explain` | Not asked for; the check is the command every call site runs.            |
| 7   | Native ahead-of-time compilation or trimming of the tool       | A packaging change, not a measurement; an ADR's if a finding prompts it. |

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

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-09 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-09 -->

| ID   | Question                                                                                                                                                                                                                                                                                                                         | Blocks    | Resolution |
| ---- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------- | ---------- |
| OQ-1 | Is the tool process's own memory measured? The library's memory diagnoser sees only the process that launches the tool, so the owner's "memory on every benchmark" yields the launcher's allocation here; the tool's would need its peak working set read outside the library's diagnoser. Default: not measured; C-7 states it. | § 5 row 4 | Open       |
| OQ-2 | Is the bare .NET host's start measured beside the tool's, as the baseline, so the ratio isolates what `specht` adds? Default: no; the finding is the tool's start-to-exit time alone.                                                                                                                                            | § 5 row 5 | Open       |

## 12. Sign-off

<!-- last written by: spec-author, 2026-10-09 -->

| Section | Status | Reviewer      | Note                   |
| ------- | ------ | ------------- | ---------------------- |
| 1-5     | 🟡     | spec-reviewer | Awaiting first review. |

## Tasks

None yet. Cut from § 3 after agreement.

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
