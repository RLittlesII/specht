---
title: "Specification: The build"
description: "One NUKE build drives every gate - compile, the three test tiers, format verification, pack and the self-check - through build.sh and build.cmd, with its tools restored from the local tool manifest and a pre-commit hook that calls the same gates on what is staged"
type: feature
id: "F1"
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
depends_on: []
blocks: ["F2", "F3", "F6"]
spikes: []
created: "2026-10-08"
updated: "2026-10-08"
github_issue: null
synced_at: null
---

# Specification: The build

## 1. Business Goal

<!-- last written by: spec-author, 2026-10-08 -->

The repository has a solution and central package versions but no build project, no entry script, no local tool manifest and a pre-commit hook copied from another repository, so "does this change pass" has no single answer: a contributor runs whatever `dotnet` commands they remember, and CI, once it exists, would run a third set. This Feature removes that failure state: one NUKE build defines every gate as a named target, `./build.sh` and `build.cmd` reach it on every operating system, the tools it calls restore from the committed local tool manifest, and the pre-commit hook calls the same gates on what is staged (Must-1).

## 2. User Needs

<!-- last written by: spec-author, 2026-10-08 -->

| #   | Persona                         | Need                                                                         | Pain Point Today                                                      |
| --- | ------------------------------- | ---------------------------------------------------------------------------- | --------------------------------------------------------------------- |
| 1   | The maintainer, before a commit | One command that says whether a change passes, the same one CI runs          | No build project and no entry script exist                            |
| 2   | The agent delivering an item    | A gate per concern - format, a test tier, the self-check - it can run alone  | Each check is a hand-typed `dotnet` invocation, differing per session |
| 3   | The maintainer's commit         | A hook that refuses a commit whose staged code or specification fails a gate | The hook present names another repository's solution                  |
| 4   | `0055-F2`, `0055-F3`, `0055-F6` | Targets to call by name                                                      | N/A (internal dependency)                                             |
| 5   | `0001-F2`                       | A `SpecCheck` target and a pre-commit call site for B-010, B-011 and B-014   | N/A (internal dependency)                                             |

### Assumptions

| ID  | Assumption                                                                                                                                                                                  |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | `Test` runs the unit, integration and acceptance tiers; `UnitTest`, `IntegrationTest` and `AcceptanceTest` run one tier each (`specht-conventions` § Build and test).                       |
| A-2 | Until the first package is published, `SpecCheck` runs `dotnet run --project src/specht.tool -- --root .` (`0001-F2` A-2).                                                                  |
| A-3 | The `.husky/pre-commit` committed today was copied from another repository - it names another solution and runs a Markdown formatter - and is replaced by this Feature's hook, not amended. |
| A-4 | The hook checks only what is staged, so a commit is never refused for drift in a file it does not touch; CI checks the whole tree (`0055-F2`).                                              |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-08 -->

| ID    | Claim                                                                                                                                                                    | Source                                                           | Status |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ---------------------------------------------------------------- | ------ |
| B-001 | Given a clone with the SDK `global.json` pins, `./build.sh` with no target name runs `Compile` and then `Test`, and exits non-zero when either fails.                    | AGENTS.md § Build and test; README § 8 step 1                    | Active |
| B-002 | Given any target name, `build.cmd` on Windows runs the same target as `./build.sh` given that name.                                                                      | `specht-conventions` § Layout; owner, 2026-10-08                 | Active |
| B-003 | Given a C# file whose formatting differs from the repository's formatting rules, the `Format` target exits non-zero and names that file.                                 | AGENTS.md § Build and test; C-2                                  | Active |
| B-004 | Given any tree, the `Format` target creates, modifies and deletes no file.                                                                                               | AGENTS.md § Build and test; C-2                                  | Active |
| B-005 | Given the `Test` target, the unit, integration and acceptance tiers each run, and a failure in any one fails the target.                                                 | A-1                                                              | Active |
| B-006 | Given the `UnitTest` target, only tests carrying the unit tier trait run.                                                                                                | `specht-conventions` § Testing                                   | Active |
| B-007 | Given the `IntegrationTest` target, only tests carrying the integration tier trait run.                                                                                  | `specht-conventions` § Testing                                   | Active |
| B-008 | Given the `AcceptanceTest` target, the scenarios of every `.feature` file the acceptance project links run.                                                              | `specht-conventions` § Testing                                   | Active |
| B-009 | Given the `Pack` target, exactly one package, `specht.tool.<version>.nupkg`, is written under `.artifacts/nupkg/`, where `<version>` is the version `0055-F5` computes.  | AGENTS.md § Build and test; `0001-F2` C-5                        | Active |
| B-010 | Given a fresh clone, `dotnet tool restore` installs every tool the build and the hook invoke, at the versions the committed local tool manifest pins.                    | README § 8 step 1; AGENTS.md § Setup                             | Active |
| B-011 | Given `dotnet husky install` has run in a clone, a commit runs the pre-commit hook.                                                                                      | AGENTS.md § Setup; owner, 2026-10-08                             | Active |
| B-012 | Given a staged C# file whose formatting differs from the repository's formatting rules, the hook refuses the commit and names that file.                                 | owner, 2026-10-08; A-4                                           | Active |
| B-013 | Given a staged file under a `.spec/` directory, or a staged `.feature` file, and the check reports an error-severity violation on the tree, the hook refuses the commit. | owner, 2026-10-08; Must-1; `specht-conventions` § Build and test | Active |
| B-014 | Given a commit staging no C# file, no file under a `.spec/` directory and no `.feature` file, the hook runs neither check and lets the commit through.                   | `specht-conventions` § Build and test; A-4                       | Active |
| B-015 | Given any commit, the hook modifies no file in the working tree and no entry in the index.                                                                               | C-2; A-3                                                         | Active |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-08 -->

| ID  | Constraint                                                                           | Rules Out                                                                                                                               |
| --- | ------------------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------- |
| C-1 | Every gate is a NUKE target; the hook and every workflow invoke targets by name.     | A `dotnet test`, `dotnet format` or `dotnet run` line in a workflow or the hook that no target runs; a gate CI has and the build lacks. |
| C-2 | Formatting is verified, never fixed, by the build and by the hook.                   | Applying analyzer code fixes in `Format` or the hook; a hook that formats and re-stages files.                                          |
| C-3 | A tool the build or the hook invokes is pinned in the committed local tool manifest. | A global tool install as a prerequisite; an unpinned tool fetched at run time.                                                          |
| C-4 | The build project takes its package versions from `Directory.Packages.props`.        | A version in `.build`'s project file.                                                                                                   |
| C-5 | Everything a target writes lands under `.artifacts/`, which is git-ignored.          | A report, package or coverage file written into a tracked path; a committed generated report.                                           |

## 5. Out of Scope

<!-- last written by: spec-author, 2026-10-08 -->

| #   | Item                                                                                    | Exclusion Reason                                                                   |
| --- | --------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------- |
| 1   | What the `SpecCheck` target's run does, and that the hook, the target and a shell agree | `0001-F2` B-010, B-011, B-014; this Feature provides the target and the call site. |
| 2   | The CI workflow and its generation                                                      | `0055-F2`.                                                                         |
| 3   | Collecting coverage during `Test`                                                       | `0055-F3`.                                                                         |
| 4   | How the package version is computed                                                     | `0055-F5`; `Pack` uses it (B-009).                                                 |
| 5   | Publishing the package                                                                  | `0055-F6`; `Pack` writes a file and pushes nothing.                                |
| 6   | The `specht` entry in the local tool manifest, and the feed it restores from            | `0055-F7`.                                                                         |
| 7   | Generating the API reference                                                            | `0002-F1`.                                                                         |
| 8   | Fixing formatting for the contributor                                                   | C-2; the contributor runs `dotnet format` themselves.                              |

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

| Claim ID | Scenario                                           | Test    | Status  |
| -------- | -------------------------------------------------- | ------- | ------- |
| B-001    | The default build compiles and tests               | Missing | Missing |
| B-002    | The Windows entry runs the same target             | Missing | Missing |
| B-003    | Unformatted code fails the format gate             | Missing | Missing |
| B-004    | The format gate changes nothing                    | Missing | Missing |
| B-005    | The test gate runs every tier                      | Missing | Missing |
| B-006    | The unit tier runs alone                           | Missing | Missing |
| B-007    | The integration tier runs alone                    | Missing | Missing |
| B-008    | The acceptance tier runs the linked scenarios      | Missing | Missing |
| B-009    | Packing writes one tool package                    | Missing | Missing |
| B-010    | A fresh clone restores every tool the build needs  | Missing | Missing |
| B-011    | An installed hook runs on commit                   | Missing | Missing |
| B-012    | The hook refuses unformatted staged code           | Missing | Missing |
| B-013    | The hook refuses a staged specification that fails | Missing | Missing |
| B-014    | The hook lets an unrelated commit through          | Missing | Missing |
| B-015    | The hook changes nothing it checks                 | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-08 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-08 -->

| ID   | Question                                                                                                                                                                                                                                                        | Blocks | Resolution |
| ---- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------ | ---------- |
| OQ-1 | The hook committed today also rewrites every Markdown file with a formatter fetched at commit time. The owner named Format and SpecCheck for the hook. Is Markdown formatting dropped, or a third gate - and if kept, does it verify (C-2) rather than rewrite? | B-015  | Open       |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-08 -->

| Section | Status | Reviewer      | Note                |
| ------- | ------ | ------------- | ------------------- |
| 1-5     | 🟡     | spec-reviewer | Draft; not reviewed |

## Tasks

None cut. Items are cut from § 3 after agreement.

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
