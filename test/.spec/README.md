---
title: "Specification: The coverage gate"
description: "The test run writes coverage, CI uploads it to Codecov, new code in a pull request must be at least 80% covered or the patch status fails, and the project total is reported and never blocks"
type: feature
id: "F3"
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
depends_on: ["F1", "F2"]
blocks: ["F8"]
spikes: []
created: "2026-10-08"
updated: "2026-10-08"
github_issue: null
synced_at: null
---

# Specification: The coverage gate

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-08 -->

A `.codecov.yml` exists, but nothing produces coverage, nothing uploads it, and the file it holds would block a pull request on the repository's total while setting no target for the code the pull request adds. So a change can arrive with its new code untested and pass, while an unrelated drop in the total blocks a change that is fully tested. This Feature removes that failure state: every test run writes coverage, CI uploads it, the code a pull request changes must be at least 80% covered, and the total is reported for the reviewer without ever blocking.

## 2. User Needs

<!-- last written by: spec-author, 2026-10-08 -->

| #   | Persona                        | Need                                                         | Pain Point Today                                                |
| --- | ------------------------------ | ------------------------------------------------------------ | --------------------------------------------------------------- |
| 1   | The reviewer of a pull request | To know the new code is tested without reading every test    | Nothing measures coverage                                       |
| 2   | The author of a pull request   | A gate about their change, not about code they did not touch | The configuration present would block on the repository's total |
| 3   | The maintainer                 | The total visible over time                                  | Nothing is uploaded                                             |
| 4   | `0055-F8`                      | A status name to require                                     | N/A (internal dependency)                                       |

### Assumptions

| ID  | Assumption                                                                                                                                                                                           |
| --- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | `.codecov.yml` today sets the project status with no target and therefore blocking, and the patch status with no target; this Feature replaces both settings. Its other keys are not this Feature's. |
| A-2 | Every operating system's coverage is uploaded and Codecov merges the uploads for one commit, so a line covered only on Windows counts as covered.                                                    |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-08 -->

| ID    | Claim                                                                                                                                                             | Source                                          | Status  |
| ----- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------- | ------- |
| B-001 | Given the `Test` target runs, each test project runs once and this Feature writes one Cobertura coverage report per test project under `.artifacts/coverage/`.    | `specht-conventions` § Testing; `0055-F1` B-005 | Amended |
| B-002 | Given a CI run on any operating system, this Feature uploads that run's coverage reports to Codecov for the run's commit.                                         | owner, 2026-10-08; A-2                          | Active  |
| B-003 | Given a pull request whose changed lines of measured code are less than 80% covered, the patch coverage status on its head commit fails.                          | owner, 2026-10-08                               | Active  |
| B-004 | Given a pull request whose changed lines of measured code are 80% covered or more, the patch coverage status on its head commit passes.                           | owner, 2026-10-08                               | Active  |
| B-005 | Given a pull request that lowers the total coverage of measured code, the project coverage status reports the total and the change, and passes.                   | owner, 2026-10-08                               | Active  |
| B-006 | Given a pull request whose only changed lines are outside `src/`, the patch coverage status counts none of them.                                                  | OQ-1 (owner, 2026-10-08); C-4                   | Active  |
| B-007 | Given a CI run whose upload to Codecov fails, that operating system's check does not fail on that account and the run reports a warning naming the failed upload. | OQ-2 (owner, 2026-10-08)                        | Active  |
| B-008 | Given a pull request in which no line of measured code changed, the patch coverage status passes.                                                                 | C-4; `0055-F4` B-005; `0055-F8` B-003           | Active  |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-08 -->

| ID  | Constraint                                                                                                      | Rules Out                                                                                        |
| --- | --------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------ |
| C-1 | The patch target, 80%, is written once, in the Codecov configuration (owner, 2026-10-08).                       | A second copy of the number in the build, a workflow or a skill; a target per operating system.  |
| C-2 | The Codecov upload token is a repository secret.                                                                | A token in `.codecov.yml`, a workflow, the build project or any tracked file.                    |
| C-3 | Coverage is measured from the test tiers `0055-F1` runs, and from nothing else.                                 | A separate coverage-only test run; coverage from a test that runs outside the build.             |
| C-4 | Measured code is `src/**` only (owner, 2026-10-08).                                                             | Test projects, `.build/` or any other path raising or lowering the patch or the project number.  |
| C-5 | The patch gate is Codecov's patch status as Codecov reports it; nothing in CI recomputes or substitutes for it. | A CI step that fails a run on a coverage number; a run failed because the upload failed (B-007). |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-08 -->

| #   | Item                                                  | Exclusion Reason                                              |
| --- | ----------------------------------------------------- | ------------------------------------------------------------- |
| 1   | A target for the total that blocks                    | Owner, 2026-10-08: the total is reported, not a gate (B-005). |
| 2   | Requiring the patch status before a merge             | `0055-F8`; this Feature produces the status.                  |
| 3   | Running the tests                                     | `0055-F1`.                                                    |
| 4   | Codecov's pull-request comment and its other settings | Not a gate; left as configured (A-1).                         |
| 5   | Coverage of mutation, branch or path kinds as a gate  | Not asked for; the gate is line coverage of changed lines.    |

## 6. Concern Separation

<!-- last written by: implementer, 2026-10-08 -->

| #   | Concern                                                            | Classification |
| --- | ------------------------------------------------------------------ | -------------- |
| 1   | Coverage written by the run that tests, and by no other run        | Both           |
| 2   | Which code is measured: `src/**`, never tests or the build         | Both           |
| 3   | The coverage collector, its instrumentation and the report format  | Technical      |
| 4   | Where the reports land, and that no earlier run's report survives  | Technical      |
| 5   | Uploading the reports to Codecov, and a failed upload only warning | Both           |
| 6   | The patch and project statuses, and the 80% target                 | Business       |
| 7   | The upload's token held as a repository secret, never in a file    | Technical      |
| 8   | Which commit the upload is recorded against                        | Both           |

## 7. Technical Design

<!-- last written by: implementer, 2026-10-08 -->

Delivered so far: the reports (item `0069`, B-001) and the upload (item `0070`, B-002, B-007). `0071` adds the patch and project statuses (B-003 to B-006, B-008).

- **The collector** is `Microsoft.Testing.Extensions.CodeCoverage`, its version in [`Directory.Packages.props`](../../Directory.Packages.props). It runs on the Microsoft.Testing.Platform runner that `global.json` selects. [`test/Directory.Build.props`](../Directory.Build.props) references it from every test project, because `Test` asks every project for coverage and a project without the extension rejects `--coverage`.
- **The run** is `0055-F1`'s `Test` target in [`.build/Build.cs`](../../.build/Build.cs), extended rather than duplicated (C-3). It empties `CoverageDirectory`, `.artifacts/coverage/`, then runs the same single `dotnet test --solution` with `--coverage --coverage-output-format cobertura --results-directory` at that folder. Each test project writes one `<project>.coverage.cobertura.xml` (B-001). Emptying the folder first means no report from an earlier run is counted or uploaded.
- **Measured code is `src/**` with no filter** (C-4). The extension skips test assemblies, and `.build/` is never loaded into a test process, so both reports name only files under `src/specht/` and `src/specht.tool/`.
- **Instrumentation is static.** Dynamic-only instrumentation (`EnableStaticManagedInstrumentation=False`) wrote an empty report on macOS arm64. Static instrumentation rewrites the test project's `bin/` copy of each `src` assembly during the run and restores it afterward. `src/*/bin/` is never touched, so `Pack`, which runs with `--no-build`, ships uninstrumented assemblies.
- **Each report is named for its test project**: `specht.tests.coverage.cobertura.xml` and `specht.acceptance.coverage.cobertura.xml`, directly in `.artifacts/coverage/`. One `TestingPlatformCommandLineArguments` property in [`test/Directory.Build.props`](../Directory.Build.props) appends `--coverage-output $(MSBuildProjectName).coverage.cobertura.xml` for every test project. Without `--coverage` the extension ignores it, so running a test project on its own is unaffected. This supersedes `0069`'s choice to keep the extension's GUID names. The upload needs the name: Codecov's uploader finds a report by searching a folder for names that match its fixed patterns. `*coverage*.*` matches the name above, but no pattern matches `<guid>.cobertura.xml`; the only Cobertura pattern is the exact name `cobertura.xml`.
- **The upload** is a step of `0055-F2`'s `ci` workflow, added by `AddCodecovUpload`, which `ContinuousIntegrationMiddleware` calls in [`.build/Build.GitHubActions.cs`](../../.build/Build.GitHubActions.cs). It is generated into [`.github/workflows/ci.yml`](../../.github/workflows/ci.yml) directly after the `test` step, so it runs on every leg of the operating-system matrix and each leg uploads its own reports (B-002, A-2). It uses `codecov/codecov-action@v5` with `directory: .artifacts/coverage`, so the uploader searches only the folder `Test` empties and refills, and only that run's reports are sent. Naming the reports in `files` with a glob was rejected. The action passes `files` to the uploader unquoted, so the runner's shell expands the glob first. On PR #9's run 37865464297, the Windows leg's shell expanded `.artifacts/coverage/**/*.cobertura.xml` into two paths after one `--file`, and the uploader stopped with "Got unexpected extra argument". Ubuntu's bash left the glob unexpanded only because no subfolder matched `**/`, so that leg's upload worked by chance.
- **The token** is `${{ secrets.CODECOV_TOKEN }}`, a repository secret (C-2). No tracked file holds its value; `0091` names the secret in `0055-F8`'s checklist.
- **The commit** is passed as `override_commit: ${{ github.event.pull_request.head.sha || github.sha }}`, the same expression the checkout uses through `0055-F2` C-2. On a pull request GitHub's `GITHUB_SHA` is the merge commit, and the uploader recognises the merge commit only from a checked-out merge, which this workflow does not check out. Without the override the upload would land on the merge commit, not the pull request's head, where the patch status (`0071`) has to appear. On a push the expression is the pushed commit.
- **A failed upload warns and passes** (B-007, C-5). The upload step sets `fail_ci_if_error: true`, so any failure (an outage, a missing token on a fork's pull request) fails the step and sets its outcome to `failure`; `continue-on-error: true` stops that outcome failing the job. The next step, `if: ${{ !cancelled() && steps.codecov.outcome == 'failure' }}`, writes a `::warning` annotation titled "Codecov upload failed" onto the run. `fail_ci_if_error: false` was rejected: the action then swallows the error, the step succeeds, and nothing warns.
- **The upload runs unless the run is cancelled** (`if: ${{ !cancelled() }}`), so a leg whose tests fail still sends the coverage it wrote. That leg's check fails on the test failure, never on the upload.
- **Not adopted:** Rocket.Surgery.Nuke's `ICanTestWithDotNetCore` and `ITriggerCodeCoverageReports`. `Build` never used RSN's test target, and RSN collects coverage the VSTest way, while `global.json` runs Microsoft.Testing.Platform.

## 8. Testing Strategy

<!-- last written by: test-writer, 2026-10-08 -->

- **No claim here gets a test (owner, 2026-10-08).** "I don't think we need ReqnRoll (or any tests) for the build. Keep what we have, in case I change my mind, don't wire them into CI." Every claim in § 3 is about the build's `Test` target, the CI workflow or Codecov's configuration. So `coverage-gate.feature` is not linked into `test/specht.acceptance`, no step class binds it, and every § 9 row stays `Missing`.
- **B-001, verified by hand.** A full `./build.sh` on macOS arm64 on 2026-10-08 left exactly two files in `.artifacts/coverage/`: `6a4b2f98-8600-482b-ae92-c4419c869313.cobertura.xml` and `33b4d035-3586-431f-a846-c50206f46250.cobertura.xml`. That is one report for each of `test/specht.tests` and `test/specht.acceptance`. Each is Cobertura version 1.9 and holds two packages, `specht` and `specht.tool`. Each names 69 source files, 63 under `src/specht/` and 6 under `src/specht.tool/`, and no file under `test/` or `.build/`. Both count 1147 valid lines: one reports 97.2% line coverage, the other 54.2%. Re-running `test/specht.acceptance` alone with the same options wrote one report with the same 63 and 6 files. That was `0069`'s run, with the extension's GUID names.
- **B-001, verified by hand again for `0070`.** A clean `./build.sh` on macOS arm64 on 2026-10-08 left exactly `specht.acceptance.coverage.cobertura.xml` (54.2% line coverage) and `specht.tests.coverage.cobertura.xml` (97.2%) in `.artifacts/coverage/`. Both are Cobertura version 1.9 with 1147 valid lines and the same two packages. Each holds 69 `class` entries over 29 distinct `filename` values, 25 under `src/specht/` and 4 under `src/specht.tool/`, and none elsewhere. The 69 counted in `0069`'s run above were those class entries, not files. A check by hand pins nothing for the next change, so the row stays `Missing`.
- **Coverage instruments the library in place, and one scenario had to change.** Microsoft code coverage instruments `specht.dll` statically: dynamic-only instrumentation produced an empty report on macOS arm64. While the run lasts, the copy in the test project's `bin/` grows from 103936 to 144384 bytes and gains the type `Microsoft.CodeCoverage.Instrumentation.Static.Tracker`. The original is restored afterward. The library's own output in `src/specht/bin/` is not touched. `0001-F1` B-005 checks the namespaces of the built library, so `EngineSteps` now reads those type definitions from `src/specht/bin/<configuration>/<framework>/specht.dll` with `System.Reflection.Metadata`, not from the loaded assembly. The assertion did not change. With and without `--coverage`, the scenario passes.
- **Mechanisms, per claim.** Read in [`Build.cs`](../../.build/Build.cs), [`Build.GitHubActions.cs`](../../.build/Build.GitHubActions.cs), [`ci.yml`](../../.github/workflows/ci.yml), [`test/Directory.Build.props`](../Directory.Build.props) and [`.codecov.yml`](../../.codecov.yml). None has a test, by the owner's decision above.
  - B-001: `Test` empties `.artifacts/coverage/` with `CreateOrCleanDirectory`, then runs one `dotnet test --solution` with `--coverage --coverage-output-format cobertura --results-directory` at that folder. `test/Directory.Build.props` references `Microsoft.Testing.Extensions.CodeCoverage` in every test project, because a project without the extension rejects the option, and appends `--coverage-output $(MSBuildProjectName).coverage.cobertura.xml` through `TestingPlatformCommandLineArguments`, so each report is named for its project. This is a declaration and an ordering: the folder is cleaned before the run.
  - B-002, B-007: `AddCodecovUpload` inserts two steps into the generated `ci.yml` directly after `test`, on every matrix leg. The upload step, `id: codecov`, runs `codecov/codecov-action@v5` unless the run is cancelled, with `directory: .artifacts/coverage` and `override_commit` set to the pull request's head or the pushed commit. `fail_ci_if_error: true` turns any upload failure into a step outcome of `failure`, and `continue-on-error: true` keeps that outcome off the job. The warning step runs only when `steps.codecov.outcome == 'failure'` and writes the `::warning` "Codecov upload failed". This is a declaration in the generated workflow, observable only on a live GitHub Actions run.
  - B-002, B-007, observed on PR #9's run 37865464297, before the `directory` fix. Ubuntu uploaded both reports to commit `8abfe18`, and the `codecov/patch` and `codecov/project` statuses appeared on it. Windows failed on the shell-expanded `files` glob, the warning step fired, and the check passed: B-007 observed. The next run is what shows B-002 on both legs.
  - B-003 to B-006, B-008: the patch and project statuses and the `src/**` path set belong to `.codecov.yml` and Codecov (`0071`, C-4, C-5). Not built yet. A Codecov status can be observed only on a live pull request.
- **Verdict.** The B-001 mechanism is a literal argument string inside a NUKE target. It would need a seam to be unit tested, for example a static member that builds the arguments. The owner's decision makes that seam unnecessary for now.

## 9. Traceability Matrix

<!-- last written by: test-writer, 2026-10-08 -->

| Claim ID | Scenario                                                | Test    | Status  |
| -------- | ------------------------------------------------------- | ------- | ------- |
| B-001    | A test run writes coverage                              | Missing | Missing |
| B-002    | Integration uploads coverage                            | Missing | Missing |
| B-003    | Under-tested new code fails the patch status            | Missing | Missing |
| B-004    | Tested new code passes the patch status                 | Missing | Missing |
| B-005    | A lower total is reported and does not block            | Missing | Missing |
| B-006    | Changes outside the product are not measured            | Missing | Missing |
| B-007    | A failed upload warns and does not fail the run         | Missing | Missing |
| B-008    | A change with no measured lines passes the patch status | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-08 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-08 -->

| ID   | Question                                                                                                                                                                                                        | Blocks              | Resolution                                                                                                                                                                           |
| ---- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| OQ-1 | What is "measured code"? Proposed default: `src/**` only, so test projects and `.build/` neither raise nor lower either number.                                                                                 | B-003, B-004, B-005 | Resolved 2026-10-08: proposed default accepted by the repository owner. C-4 and B-006 added.                                                                                         |
| OQ-2 | When the upload to Codecov fails (an outage, a missing token on a fork's pull request), does the operating system's CI check fail, or does the missing coverage status alone block the merge through `0055-F8`? | B-002               | Resolved 2026-10-08 by the repository owner: a failed upload does not fail CI and is reported as a warning; the patch gate is Codecov's status when it reports. B-007 and C-5 added. |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-08 -->

| Section | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          |
| ------- | ------ | ------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-5     | 🟡     | spec-reviewer | Draft; not reviewed                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           |
| 1-5     | 🟡     | spec-reviewer | Round 1 (2026-10-08) - reviewed in draft, reviewing commit `04fdc28`; no blocking findings. The claims state the owner's decisions - an 80% patch target written once, an informational total, `src/**` measured, a failed upload warning without failing - one each and testably, and § 9 and the tags are complete. Non-blocking (spec-author): B-006 says lines outside `src/` count for nothing but not the patch status's verdict when no measured line changed, which a dependency update (`0055-F4` B-005) needs to pass because `0055-F8` B-003 requires the status; B-001's one report per test project assumes `Test` runs each project once, which `0055-F1` B-005 does not say. Left `draft`: the spec-author has not submitted it, and epic `0055` says its eight-Feature split awaits the owner's confirmation. |
| 1-5     | 🟢     | spec-reviewer | Round 2 (2026-10-08) - approved, reviewing `acf3cff`..`f32a31d`. B-008 closes the round-1 gap: a pull request with no measured line changed passes the patch status, which `0055-F4` B-005 and `0055-F8` B-003 need. Non-blocking (spec-author): B-001's one report per test project still assumes `Test` runs each project once.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             |
| 1-5     | 🟢     | spec-reviewer | Round 3 (2026-10-08) - approved, reviewing `01f1f73`..`9a379ce`. B-001 now says each test project runs once, consistent with `0055-F1` B-005; the round-2 non-blocking finding is fixed.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      |
| Tasks   | 🟢     | spec-reviewer | Item cut (2026-10-08) - approved, reviewing `6113181`..`0657b68`. 0069-0071 carry B-001-B-008 once, sliced by where each runs; 0069 on 0057 and 0070 on 0064 are the target and the workflow each extends.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |

## Tasks

Cut 2026-10-08 into [`../.issue/`](../.issue/): `0068` (the Feature), with `0069` to `0071`.

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
