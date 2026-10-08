---
title: "Specification: Continuous integration"
description: "Every pull request and every push to main runs the build's gates on ubuntu, windows and macos, from a workflow NUKE generates, each operating system a stable check of its own, with spec violations annotated on the diff and nothing published"
type: feature
id: "F2"
epic: "0055"
spec_status: draft
status: needs-decomposition
priority: high
value: 0
risk: 0
rank: 0
scored_by: null
scored_on: null
domain: "Build and release"
author: "spec-author"
milestone: null
children: []
depends_on: ["F1"]
blocks: ["F3", "F4", "F8"]
spikes: []
created: "2026-10-08"
updated: "2026-10-08"
github_issue: null
synced_at: null
---

# Specification: Continuous integration

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-08 -->

No workflow exists, so the first pull request on GitHub would merge on the author's word, and the invariants README § 9 makes non-negotiable - no absolute path, `/` separators in every output - would be proven only on the one operating system a contributor happens to use. This Feature removes that failure state: every pull request and every push to `main` runs the build's gates on ubuntu, windows and macos, each operating system reports as a check of its own under a name that does not change, and a specification violation is annotated on the line of the diff that caused it.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-08 -->

| #   | Persona                              | Need                                                                   | Pain Point Today                                  |
| --- | ------------------------------------ | ---------------------------------------------------------------------- | ------------------------------------------------- |
| 1   | The reviewer of a pull request       | A verdict on the change before reading it                              | No workflow exists                                |
| 2   | The maintainer                       | The path invariants proven on Windows, where the separator differs     | Only the local operating system is ever exercised |
| 3   | The author of a specification change | The violation shown on the line that caused it, in the diff            | A log to search                                   |
| 4   | `0055-F8`                            | Check names stable enough to require in branch protection              | N/A (internal dependency)                         |
| 5   | `0055-F3`, `0055-F4`                 | A run whose coverage can be uploaded, and checks an update can wait on | N/A (internal dependency)                         |

### Assumptions

| ID  | Assumption                                                                             |
| --- | -------------------------------------------------------------------------------------- |
| A-1 | Every operating system runs the same list of targets; no gate runs on one system only. |
| A-2 | A pull request means one targeting `main`; `main` is the only long-lived branch.       |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-08 -->

| ID    | Claim                                                                                                                                                                                      | Source                                     | Status  |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------ | ------- |
| B-001 | Given a pull request targeting `main` is opened or updated, this Feature runs the build against the pull request's head.                                                                   | owner, 2026-10-08; A-2                     | Active  |
| B-002 | Given a push to `main`, this Feature runs the build against the pushed commit.                                                                                                             | owner, 2026-10-08                          | Active  |
| B-003 | Given a run, this Feature runs the build once on each of ubuntu, windows and macos.                                                                                                        | owner, 2026-10-08; README § 9              | Amended |
| B-004 | Given a run on one operating system, this Feature runs the `Format`, `Compile`, `Test`, `SpecCheck` and `Pack` targets through the build's entry script.                                   | README § 8 step 1; `0055-F1` C-1; A-1; C-4 | Active  |
| B-005 | Given any target fails on one operating system, that operating system's check fails.                                                                                                       | README § 8 step 1; C-4                     | Active  |
| B-006 | Given a pull request whose tree has a specification violation, this Feature annotates the violation's file and line in the pull request's diff.                                            | AGENTS.md § `specht`; `0001-F2` B-001      | Active  |
| B-007 | Given two runs on different commits, each operating system's check carries the same name in both.                                                                                          | `0055-F8` B-002                            | Active  |
| B-008 | Given any run of this Feature, no package is pushed to any feed.                                                                                                                           | `0055-F6` C-1                              | Active  |
| B-009 | Given a commit whose committed copy of any generated workflow - integration, release or dependency updates - differs from what the build generates from that commit, the run fails.        | OQ-1 (owner, 2026-10-08); C-1              | Amended |
| B-010 | Given a run, each operating system's build reports as a separate check.                                                                                                                    | owner, 2026-10-08; split from B-003        | Active  |
| B-011 | Given a run after this repository's local tool manifest names `specht.tool` (`0055-F7` B-003), the run restores the tool from the feed with the workflow's own token granted package read. | owner, 2026-10-08; `0055-F7` A-1           | Active  |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-08 -->

| ID  | Constraint                                                                                                                                                                                                                        | Rules Out                                                                                              |
| --- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------ |
| C-1 | Every workflow file - integration, release and dependency updates - is generated by the NUKE build; a change to one is a change to the build project plus the committed regeneration, with its step order diffed.                 | A hand edit to any file under `.github/workflows/`; a committed workflow the build would not generate. |
| C-2 | A run checks out the commit with its full history.                                                                                                                                                                                | A shallow checkout, which changes the git height `0055-F5` computes the version from (`0055-F5` C-2).  |
| C-3 | A run is offline beyond restoring packages and tools and uploading coverage.                                                                                                                                                      | A test or a gate that calls GitHub or another service (AGENTS.md § Invariants).                        |
| C-4 | `SpecCheck` gates a run only from `0001-F2`'s check command landing; until then its step reports itself unavailable and passes, and `SPEC060` is a warning here throughout (`0055-F1` B-023, B-024, C-6, C-7; owner, 2026-10-08). | A run failed because the check command does not exist yet, or for a `Missing` traceability row.        |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-08 -->

| #   | Item                                         | Exclusion Reason                                           |
| --- | -------------------------------------------- | ---------------------------------------------------------- |
| 1   | Uploading coverage and the coverage statuses | `0055-F3`.                                                 |
| 2   | Building and publishing on a version tag     | `0055-F6`; B-008 is the boundary.                          |
| 3   | Which checks branch protection requires      | `0055-F8`; this Feature makes the names stable (B-007).    |
| 4   | Opening update pull requests                 | `0055-F4`.                                                 |
| 5   | What each target does                        | `0055-F1`; this Feature only runs them.                    |
| 6   | Scheduled or nightly runs                    | Not asked for; a run follows a pull request or a push.     |
| 7   | Keeping the packed package as a run artifact | Not asked for; only a release keeps a package (`0055-F6`). |

## 6. Concern Separation

<!-- last written by: spec-author, 2026-10-08 -->

Pending: owned by `implementer`, written after agreement.

## 7. Technical Design

<!-- last written by: spec-author, 2026-10-08 -->

Pending: owned by `implementer`, written after agreement.

## 8. Testing Strategy

<!-- last written by: spec-author, 2026-10-08 -->

Pending: owned by `test-writer`.

## 9. Traceability Matrix

<!-- last written by: spec-author, 2026-10-08 -->

| Claim ID | Scenario                                          | Test    | Status  |
| -------- | ------------------------------------------------- | ------- | ------- |
| B-001    | A pull request is built                           | Missing | Missing |
| B-002    | A push to main is built                           | Missing | Missing |
| B-003    | Every run covers three operating systems          | Missing | Missing |
| B-004    | Every operating system runs every gate            | Missing | Missing |
| B-005    | A failing gate fails its operating system's check | Missing | Missing |
| B-006    | A specification violation is shown on the diff    | Missing | Missing |
| B-007    | Check names do not change between runs            | Missing | Missing |
| B-008    | Integration never publishes                       | Missing | Missing |
| B-009    | A stale workflow fails the run                    | Missing | Missing |
| B-010    | Each operating system is its own check            | Missing | Missing |
| B-011    | A run reads the feed with its own token           | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-08 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-08 -->

| ID   | Question                                                                                                                                                                                                          | Blocks | Resolution                                                                           |
| ---- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------ | ------------------------------------------------------------------------------------ |
| OQ-1 | C-1 says the workflow is generated, but nothing yet fails when the committed copy is stale. Does a run fail when the committed workflow differs from what the build generates? Proposed default: yes, as a claim. | C-1    | Resolved 2026-10-08: proposed default accepted by the repository owner. B-009 added. |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-08 -->

| Section | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |
| ------- | ------ | ------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-5     | 🟡     | spec-reviewer | Draft; not reviewed                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |
| 1-5     | 🔴     | spec-reviewer | Round 1 (2026-10-08) - blocked, reviewing commit `04fdc28`. B-009 states OQ-1 and agrees with `0055-F4` B-013 and C-7 once `0055-F4` OQ-6 is answered; B-007 is what `0055-F8` B-002 needs. Blocking (spec-author, escalate to the owner): B-004 runs `SpecCheck` on every run and B-005 fails the check on any failing target, but `src/specht.tool` has no check command yet, so the first push the epic plans is red; and once it exists, `SPEC060` (README § 4, Error) fails on this repository's approved specifications that carry `Missing` rows, so every pull request is red and `0055-F4` B-005 and `0055-F8` B-002 never pass. State what `SpecCheck` contributes before `0001-F2` lands, and get the owner's call on `SPEC060` against `Missing` (a `0001-F5` severity override is one route); `0055-F1` B-013 and `0055-F6` B-001 and B-008 take the same answer. Non-blocking (spec-author): B-009 and C-1 speak of one workflow file, `ci.yml`, but the release workflow is generated too (`0055-F6` A-1) and nothing fails a stale copy of it; after `0055-F7` B-003, B-004's `SpecCheck` restores the tool from the authenticated feed and no claim says the run supplies a read token; B-003 carries two assertions. |
| 1-5     | 🔴     | spec-reviewer | Round 2 (2026-10-08) - blocked, reviewing `acf3cff`..`f32a31d`. B-009 and C-1 now cover every generated workflow; B-010 splits B-003; B-011 states the owner's read token, matching `0055-F7` C-5. Blocking: C-4 says `SPEC060` is a warning here throughout, but `0001-F2`'s check command lands before `0001-F5` (`0001-F5` depends on `0001-F2`), and `0001-F1` C-9 holds every verdict, severity included, at the baseline until README § 8 step 5, so between those two landings `SpecCheck` gates with `SPEC060` still an error on the six approved specifications that carry `Missing` rows, and fails every commit staging a `.spec/` file and every run; it takes `0055-F1`'s answer (spec-author, owner).                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |

## Tasks

None cut. Items are cut from § 3 after agreement.

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
