---
title: "Specification: Continuous integration"
description: "Every push to main, and every pull request that changes more than documentation, runs the build's gates on ubuntu and windows, from a workflow NUKE generates, each operating system a stable check of its own, with spec violations annotated on the diff and nothing published"
type: feature
id: "F2"
epic: "0055"
spec_status: approved
status: ready-for-architecture
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

No workflow exists, so the first pull request on GitHub would merge on the author's word, and the invariants brief § 9 makes non-negotiable - no absolute path, `/` separators in every output - would be proven only on the one operating system a contributor happens to use. This Feature removes that failure state: every push to `main`, and every pull request that changes more than documentation, runs the build's gates on ubuntu and windows, each operating system reports as a check of its own under a name that does not change, and a specification violation is annotated on the line of the diff that caused it.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-08 -->

| #   | Persona                              | Need                                                                   | Pain Point Today                                  |
| --- | ------------------------------------ | ---------------------------------------------------------------------- | ------------------------------------------------- |
| 1   | The reviewer of a pull request       | A verdict on the change before reading it                              | No workflow exists                                |
| 2   | The maintainer                       | The path invariants proven on Windows, where the separator differs     | Only the local operating system is ever exercised |
| 3   | The author of a specification change | The violation shown on the line that caused it, in the diff            | A log to search                                   |
| 4   | `0055-F8`                            | Check names stable enough to require in branch protection              | N/A (internal dependency)                         |
| 5   | `0055-F3`, `0055-F4`                 | A run whose coverage can be uploaded, and checks an update can wait on | N/A (internal dependency)                         |
| 6   | The author of a documentation change | Green checks without a full build on two runners                       | Every Markdown edit runs every gate twice         |

### Assumptions

| ID  | Assumption                                                                             |
| --- | -------------------------------------------------------------------------------------- |
| A-1 | Every operating system runs the same list of targets; no gate runs on one system only. |
| A-2 | A pull request means one targeting `main`; `main` is the only long-lived branch.       |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-08 -->

| ID    | Claim                                                                                                                                                                                      | Source                                                                                                   | Status  |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | -------------------------------------------------------------------------------------------------------- | ------- |
| B-001 | Given a pull request targeting `main` is opened or updated, and B-012 does not apply to it, this Feature runs the build against the pull request's head.                                   | owner, 2026-10-08; A-2; B-012 (owner, 2026-10-08)                                                        | Amended |
| B-002 | Given a push to `main`, this Feature runs the build against the pushed commit.                                                                                                             | owner, 2026-10-08                                                                                        | Active  |
| B-003 | Given a run, this Feature runs once on each of ubuntu and windows.                                                                                                                         | owner, 2026-10-08; brief § 9; macOS dropped by the owner, 2026-10-08 (§ 5 #8); B-012 (owner, 2026-10-08) | Amended |
| B-004 | Given a run on one operating system that B-012 does not apply to, this Feature runs the `Format`, `Compile`, `Test`, `SpecCheck` and `Pack` targets through the build's entry script.      | brief § 8 step 1; `0055-F1` C-1; A-1; C-4; B-012 (owner, 2026-10-08)                                     | Amended |
| B-005 | Given any target fails on one operating system, that operating system's check fails.                                                                                                       | brief § 8 step 1; C-4                                                                                    | Active  |
| B-006 | Given a pull request whose tree has a specification violation, this Feature annotates the violation's file and line in the pull request's diff.                                            | AGENTS.md § `specht`; `0001-F2` B-001                                                                    | Active  |
| B-007 | Given two runs on different commits, each operating system's check carries the same name in both.                                                                                          | `0055-F8` B-002                                                                                          | Active  |
| B-008 | Given any run of this Feature, no package is pushed to any feed.                                                                                                                           | `0055-F6` C-1                                                                                            | Active  |
| B-009 | Given a commit whose committed copy of any generated workflow - integration, release or dependency updates - differs from what the build generates from that commit, the run fails.        | OQ-1 (owner, 2026-10-08); C-1                                                                            | Amended |
| B-010 | Given a run, each operating system's build reports as a separate check.                                                                                                                    | owner, 2026-10-08; split from B-003                                                                      | Active  |
| B-011 | Given a run after this repository's local tool manifest names `specht.tool` (`0055-F7` B-003), the run restores the tool from the feed with the workflow's own token granted package read. | owner, 2026-10-08; `0055-F7` A-1                                                                         | Active  |
| B-012 | Given a pull request targeting `main` with one or more changed files, each a `*.md` file outside any `.spec/` folder, this Feature runs none of the build's targets.                       | owner, 2026-10-08; C-5                                                                                   | Active  |
| B-013 | Given a pull request B-012 applies to, each operating system's check passes.                                                                                                               | owner, 2026-10-08; `0055-F8` B-002, B-011; B-007                                                         | Active  |
| B-014 | Given a pull request targeting `main` whose changed files are none or cannot be determined, this Feature runs the build against the pull request's head.                                   | owner, 2026-10-08                                                                                        | Active  |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-08 -->

| ID  | Constraint                                                                                                                                                                                                                                                                            | Rules Out                                                                                                                                                                        |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| C-1 | Every workflow file - integration, release and dependency updates - is generated by the NUKE build; a change to one is a change to the build project plus the committed regeneration, with its step order diffed.                                                                     | A hand edit to any file under `.github/workflows/`; a committed workflow the build would not generate.                                                                           |
| C-2 | A run checks out the commit with its full history.                                                                                                                                                                                                                                    | A shallow checkout, which changes the git height `0055-F5` computes the version from (`0055-F5` C-2).                                                                            |
| C-3 | A run is offline beyond restoring packages and tools and uploading coverage.                                                                                                                                                                                                          | A test or a gate that calls GitHub or another service (AGENTS.md § Invariants).                                                                                                  |
| C-4 | `SpecCheck` gates a run only once both `0001-F2`'s check command and `0001-F5`'s rule settings exist - from `0001-F5`'s arrival; until then its step reports and passes, so no run is gated while `SPEC060` is an error (`0055-F1` B-023, B-024, B-025, C-6, C-7; owner, 2026-10-08). | A run failed before `0001-F5` lands, or for a `Missing` traceability row.                                                                                                        |
| C-5 | Whether B-012 applies is decided inside each operating system's run, so both checks still report under the names B-007 keeps (owner, 2026-10-08).                                                                                                                                     | A workflow-level path filter or a job-level condition, either of which leaves the per-operating-system checks `0055-F8` B-002 requires pending and the pull request unmergeable. |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-08 -->

| #   | Item                                                            | Exclusion Reason                                                                                                                                                                                |
| --- | --------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | Uploading coverage and the coverage statuses                    | `0055-F3`.                                                                                                                                                                                      |
| 2   | Building and publishing on a version tag                        | `0055-F6`; B-008 is the boundary.                                                                                                                                                               |
| 3   | Which checks branch protection requires                         | `0055-F8`; this Feature makes the names stable (B-007).                                                                                                                                         |
| 4   | Opening update pull requests                                    | `0055-F4`.                                                                                                                                                                                      |
| 5   | What each target does                                           | `0055-F1`; this Feature only runs them.                                                                                                                                                         |
| 6   | Scheduled or nightly runs                                       | Not asked for; a run follows a pull request or a push.                                                                                                                                          |
| 7   | Keeping the packed package as a run artifact                    | Not asked for; only a release keeps a package (`0055-F6`).                                                                                                                                      |
| 8   | Running the build on macOS                                      | Owner, 2026-10-08: a macOS runner is billed at ten times the Linux rate on a private repository, and its Unix behaviour is Linux's; Windows is the leg that proves the path invariants (B-003). |
| 9   | Skipping the build on a push to `main`                          | Owner, 2026-10-08: every push builds (B-002), so Codecov's project baseline on `main` has no gap.                                                                                               |
| 10  | Checking Markdown formatting on a pull request B-012 applies to | Owner, 2026-10-08: B-012 skips every target, `Format` included; the pre-commit hook (`0055-F1` B-016) is the guard.                                                                             |
| 11  | A configurable list of paths or file kinds that skip the build  | Not asked for; B-012 fixes the one rule, and a `.feature`, `.yml`, `.json` or anything under `.spec/` changes what the build checks.                                                            |

## 6. Concern Separation

<!-- last written by: implementer, 2026-10-08 -->

| #   | Concern                                                        | Classification |
| --- | -------------------------------------------------------------- | -------------- |
| 1   | Which events run the build, and which commit they build        | Both           |
| 2   | Which operating systems run it, and how each reports           | Both           |
| 3   | Which gates a run executes, and through what entry             | Both           |
| 4   | Generating the workflow from the build rather than by hand     | Technical      |
| 5   | What a run may write or push: no feed, no artifact, read token | Both           |

## 7. Technical Design

<!-- last written by: implementer, 2026-10-08 -->

Delivered so far by item 0064.

- **The declaration** is [`.build/Build.GitHubActions.cs`](../../Build.GitHubActions.cs): Rocket.Surgery.Nuke's `[GitHubActionsSteps]` generator, which the owner chose (`487e5d8`), named `ci`, writing [`.github/workflows/ci.yml`](../../../.github/workflows/ci.yml) with `AutoGenerate` on, so any build run regenerates it and the regeneration is committed with the build change (C-1). Its `ContinuousIntegrationMiddleware` enhancement shapes what the generator emits; the rest is attribute settings. B-009's check that the committed copy is current is `0065`.
- **Triggers.** `OnPullRequestBranches` and `OnPushBranches`, each `main` (B-001, B-002, A-2).
- **A matrix of two images** - `ubuntu-latest` and `windows-latest` - in one `build` job. Each leg is its own check, `build (ubuntu-latest)` and `build (windows-latest)`, named by literals in the committed file and nothing computed from the commit (B-003, B-007, B-010). The middleware turns `fail-fast` off, so a failing leg never cancels the other and each operating system reports its own result (B-005).
- **One step per gate**, in dependency order: `Restore`, `Format`, `Compile`, `Test`, `SpecCheck`, `Pack`, each skipping what an earlier step already ran. The generator would run the first through a global `nuke` and the rest through the compiled build assembly; the middleware drops the global install (`0055-F1` C-3) and rewrites every step to `./build.cmd --target ...`, the entry script on every system (B-004). A failing target exits it non-zero and fails that step and that leg (B-005, `0055-F1` B-018). The middleware also keeps `Format` ahead of `Compile`, so a formatting failure is the first one reported.
- **The checkout** is the generator's `actions/checkout` step, which the owner's shared `Middleware` already gives `fetch-depth: 0` (C-2), followed by a `git fetch --prune` and `actions/setup-dotnet`. The CI middleware sets `ref: ${{ github.event.pull_request.head.sha }}`: a pull request builds its head commit rather than the merge GitHub synthesises (B-001), and a push, where the expression is empty, builds the pushed commit (B-002).
- **Nothing leaves a run** (B-008, C-3): no target in the list pushes, no step uploads an artifact, so `Pack`'s output is not kept (§ 5 #7), and the middleware replaces the generator's default token - which writes issues, pull requests and statuses - with `contents: read` and nothing else.
- **Not in this workflow:** the owner's Codecov upload, which is `0070`'s (it uploads `0069`'s reports), the `artifacts/logs/` upload, a path this build never writes, and `DeployMiddleware`'s registry and Azure logins, which belong to a release workflow, not to integration.

## 8. Testing Strategy

<!-- last written by: test-writer, 2026-10-08 -->

- **CI has no tests (owner, 2026-10-08).** "I don't think we need ReqnRoll (or any tests) for the build. Keep what we have, in case I change my mind, don't wire them into CI." `continuous-integration.feature` is no longer linked into `test/specht.acceptance`, so none of its scenarios is discovered or run. [`ContinuousIntegration/ContinuousIntegrationSteps.cs`](../../../test/specht.acceptance/ContinuousIntegration/ContinuousIntegrationSteps.cs) stays in the tree and compiles, bound to no linked feature. Every § 9 row is therefore `Missing`: a step class that never runs pins nothing.
- **To restore the tier**, relink `continuous-integration.feature` in `test/specht.acceptance/specht.acceptance.csproj` with its literal `ReqnrollFeatureFile` entry, then check that the discovered test count rose.
- **What the unlinked scenarios did, while they ran.** `ContinuousIntegrationSteps` bound B-001 to B-005, B-007, B-008 and B-010. It read the committed `ci.yml` with YamlDotNet and never called GitHub (C-3). Moving the `push` trigger to another branch turned B-002 red. Dropping `Pack` from the gate steps turned B-004 red. Those results describe the workflow as it was then. Nothing re-checks them now. A runner was never under test: the first live run is the one observed on the pull request that delivered 0064.
- **Mechanisms, per claim.** Read in [`Build.GitHubActions.cs`](../../Build.GitHubActions.cs). None has a test, by the owner's decision above.
  - B-004: `ContinuousIntegrationMiddleware` removes the generator's tool-install and tool-restore steps. It then rewrites every run step that names `--target ` into `./build.cmd`, followed by the text from `--target ` onward. This is a derivation.
  - B-001, B-002, B-003, B-005, B-007, B-008, B-010: no mechanism of their own. The trigger branches, the two images, `FailFast = false`, the job name and the read-only permissions are each a literal setting in `ci.yml`.
  - B-006 (`0066`), B-009 (`0065`), B-011 (`0067`): not built yet.
- **Verdict.** B-004's rewrite is testable as it stands, given the generated step list. It has no test because the owner decided CI needs none, not because the design prevents it.

## 9. Traceability Matrix

<!-- last written by: test-writer, 2026-10-08 -->

| Claim ID | Scenario                                            | Test    | Status  |
| -------- | --------------------------------------------------- | ------- | ------- |
| B-001    | A pull request is built                             | Missing | Missing |
| B-002    | A push to main is built                             | Missing | Missing |
| B-003    | Every run covers Linux and Windows                  | Missing | Missing |
| B-004    | Every operating system runs every gate              | Missing | Missing |
| B-005    | A failing gate fails its operating system's check   | Missing | Missing |
| B-006    | A specification violation is shown on the diff      | Missing | Missing |
| B-007    | Check names do not change between runs              | Missing | Missing |
| B-008    | Integration never publishes                         | Missing | Missing |
| B-009    | A stale workflow fails the run                      | Missing | Missing |
| B-010    | Each operating system is its own check              | Missing | Missing |
| B-011    | A run reads the feed with its own token             | Missing | Missing |
| B-012    | A documentation-only pull request skips the gates   | Missing | Missing |
| B-013    | A documentation-only pull request passes its checks | Missing | Missing |
| B-014    | A pull request with no known changed files is built | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-08 -->

- 2026-10-08, B-003 amended from ubuntu, windows and macos to ubuntu and windows by the owner while 0064 was in review: a run proves the tests on each runtime, and macOS added cost without a failure mode Linux and Windows do not already exercise. § 5 #8 records the boundary.
- 2026-10-08, the first run of 0064's workflow failed on Windows only: the runner's `core.autocrlf` checked every file out with CRLF, and `Format` rejected each C# file against `.editorconfig`'s `end_of_line = lf`. A root `.gitattributes` (`* text=auto eol=lf`) now checks out LF on every system. The Windows leg found what Linux and macOS cannot - the reason B-003 keeps it.
- 2026-10-08, 0064 first replaced the owner's Rocket.Surgery.Nuke declaration with NUKE's own generator, without asking; the owner had it redone on Rocket.Surgery.Nuke, whose attribute settings and middleware cover every claim. No claim changed. Process lesson: [`.spec/lessons/0002`](../../../.spec/lessons/0002-extend-what-the-owner-put-in-place.md).
- 2026-10-08, the owner decided a pull request that changes only Markdown outside any `.spec/` folder runs no gates. B-012 to B-014 added; B-001, B-003 and B-004 amended to except it; C-5 and § 5 #9 to #11 added; OQ-2 raised on the coverage status such a pull request no longer gets, and resolved the same day: the patch status is not required (`0055-F8` B-003). § 7's trigger bullet describes the workflow before item 0102.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-08 -->

| ID   | Question                                                                                                                                                                                                                                                                                                                                                                                                                        | Blocks                                  | Resolution                                                                                                                                                                                                                                                           |
| ---- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | C-1 says the workflow is generated, but nothing yet fails when the committed copy is stale. Does a run fail when the committed workflow differs from what the build generates? Proposed default: yes, as a claim.                                                                                                                                                                                                               | C-1                                     | Resolved 2026-10-08: proposed default accepted by the repository owner. B-009 added.                                                                                                                                                                                 |
| OQ-2 | A run B-012 applies to uploads no coverage, so Codecov posts no patch status on that pull request: `0055-F3` B-002 and B-008 do not hold for it, and once `0055-F8` B-003 requires the patch status, it can never merge. Which gives way - the patch status stays unrequired, B-012 is withdrawn, or `0055-F3` names another source of that status? No default is proposed: each option gives up something the owner asked for. | `0055-F3` B-002, B-008; `0055-F8` B-003 | Resolved 2026-10-08 by the repository owner: the patch status stays reported but is not a required check, so a pull request B-012 applies to merges on the two build checks. `0055-F8` B-003 amended; `0055-F3` B-002 and B-008 narrowed to runs that run the build. |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-08 -->

| Section | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  |
| ------- | ------ | ------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-5     | 🟡     | spec-reviewer | Draft; not reviewed                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |
| 1-5     | 🔴     | spec-reviewer | Round 1 (2026-10-08) - blocked, reviewing commit `04fdc28`. B-009 states OQ-1 and agrees with `0055-F4` B-013 and C-7 once `0055-F4` OQ-6 is answered; B-007 is what `0055-F8` B-002 needs. Blocking (spec-author, escalate to the owner): B-004 runs `SpecCheck` on every run and B-005 fails the check on any failing target, but `src/specht.tool` has no check command yet, so the first push the epic plans is red; and once it exists, `SPEC060` (brief § 4, Error) fails on this repository's approved specifications that carry `Missing` rows, so every pull request is red and `0055-F4` B-005 and `0055-F8` B-002 never pass. State what `SpecCheck` contributes before `0001-F2` lands, and get the owner's call on `SPEC060` against `Missing` (a `0001-F5` severity override is one route); `0055-F1` B-013 and `0055-F6` B-001 and B-008 take the same answer. Non-blocking (spec-author): B-009 and C-1 speak of one workflow file, `ci.yml`, but the release workflow is generated too (`0055-F6` A-1) and nothing fails a stale copy of it; after `0055-F7` B-003, B-004's `SpecCheck` restores the tool from the authenticated feed and no claim says the run supplies a read token; B-003 carries two assertions. |
| 1-5     | 🔴     | spec-reviewer | Round 2 (2026-10-08) - blocked, reviewing `acf3cff`..`f32a31d`. B-009 and C-1 now cover every generated workflow; B-010 splits B-003; B-011 states the owner's read token, matching `0055-F7` C-5. Blocking: C-4 says `SPEC060` is a warning here throughout, but `0001-F2`'s check command lands before `0001-F5` (`0001-F5` depends on `0001-F2`), and `0001-F1` C-9 holds every verdict, severity included, at the baseline until brief § 8 step 5, so between those two landings `SpecCheck` gates with `SPEC060` still an error on the six approved specifications that carry `Missing` rows, and fails every commit staging a `.spec/` file and every run; it takes `0055-F1`'s answer (spec-author, owner).                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |
| 1-5     | 🟢     | spec-reviewer | Round 3 (2026-10-08) - approved, reviewing `01f1f73`..`9a379ce`. C-4 takes `0055-F1` C-7's timing: no run is gated before `0001-F5` lands.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            |
| Tasks   | 🟢     | spec-reviewer | Item cut (2026-10-08) - approved, reviewing `6113181`..`0657b68`. 0064-0067 carry B-001-B-011 once, and 0064 is the CI item 0021 and 0051 wait on. 0066 on 0031 is real: B-006 annotates the lines the `SpecCheck` step prints, and before 0031 that step is `0055-F1` B-023's placeholder, which prints none. 0067 on 0087 matches B-011's Given. 0064 runs `SpecCheck` (B-004) and so meets the `0001-F2` contradiction recorded in that Feature's § 12; C-4 is already right.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      |
| Tasks   | 🟢     | spec-reviewer | Re-cut (2026-10-08) - approved, reviewing `f9d0ce1`. 0066 on 0061 replaces the ruling on 0066 on 0031 and is the real edge: 0061 replaces `0055-F1` B-023's placeholder with the call that prints the MSBuild-shaped lines B-006 annotates, and 0031 now wires nothing.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               |

## Tasks

Cut 2026-10-08 into [`../.issue/`](../.issue/): `0063` (the Feature), with `0064` to `0067`. `0102` cut 2026-10-08 from B-012 to B-014.

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
