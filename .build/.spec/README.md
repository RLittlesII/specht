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
blocks: ["F2", "F3", "F4", "F5", "F6"]
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

| #   | Persona                         | Need                                                                         | Pain Point Today                                                                                                                          |
| --- | ------------------------------- | ---------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | The maintainer, before a commit | One command that says whether a change passes, the same one CI runs          | No build project and no entry script exist                                                                                                |
| 2   | The agent delivering an item    | A gate per concern - format, a test tier, the self-check - it can run alone  | Each check is a hand-typed `dotnet` invocation, differing per session                                                                     |
| 3   | The maintainer's commit         | A hook that refuses a commit whose staged code or specification fails a gate | The hook at `857f00d` calls `dotnet format` and the Markdown formatter directly, not the build's targets, and runs no specification check |
| 4   | `0055-F2`, `0055-F3`, `0055-F6` | Targets to call by name                                                      | N/A (internal dependency)                                                                                                                 |
| 5   | `0001-F2`                       | A `SpecCheck` target and a pre-commit call site for B-010, B-011 and B-014   | N/A (internal dependency)                                                                                                                 |

### Assumptions

| ID  | Assumption                                                                                                                                                                                                                                                                                                                                              |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A-1 | `Test` runs the unit, integration and acceptance tiers; `UnitTest`, `IntegrationTest` and `AcceptanceTest` run one tier each (`specht-conventions` § Build and test).                                                                                                                                                                                   |
| A-2 | Until the first package is published, `SpecCheck` runs `dotnet run --project src/specht.tool -- --root .` (`0001-F2` A-2).                                                                                                                                                                                                                              |
| A-3 | The `.husky/pre-commit` at `857f00d` checks staged C# with `dotnet format --verify-no-changes` and staged Markdown with `prettier --check`, each called directly on the working-tree copy of the staged files, and runs no specification check; this Feature's hook replaces those calls with the build's targets (C-1, B-021).                         |
| A-4 | The hook's formatting checks cover only the staged files, so a commit is never refused for formatting drift in a file it does not touch; its specification check covers the whole tree, because a specification's rules span files and the hook prints what a shell prints (`0001-F2` B-010). CI checks the whole tree (`0055-F2`). Amended 2026-10-08. |

## 3. Acceptance Criteria

<!-- last written by: spec-author, 2026-10-08 -->

| ID    | Claim                                                                                                                                                                                                                                          | Source                                               | Status  |
| ----- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------- | ------- |
| B-001 | Given a clone with the SDK `global.json` pins, `./build.sh` with no target name runs `Compile` and then `Test`.                                                                                                                                | AGENTS.md § Build and test; README § 8 step 1        | Amended |
| B-002 | Given any target name, `build.cmd` on Windows runs the same target as `./build.sh` given that name.                                                                                                                                            | `specht-conventions` § Layout; owner, 2026-10-08     | Active  |
| B-003 | Given a C# file whose formatting differs from the repository's formatting rules, the `Format` target exits non-zero and names that file.                                                                                                       | AGENTS.md § Build and test; C-2                      | Active  |
| B-004 | Given any tree, the `Format` target creates, modifies and deletes no tracked file.                                                                                                                                                             | AGENTS.md § Build and test; C-2; C-5                 | Amended |
| B-005 | Given the `Test` target, the unit, integration and acceptance tiers each run, and a failure in any one fails the target.                                                                                                                       | A-1                                                  | Active  |
| B-006 | Given the `UnitTest` target, only tests carrying the unit tier trait run.                                                                                                                                                                      | `specht-conventions` § Testing                       | Active  |
| B-007 | Given the `IntegrationTest` target, only tests carrying the integration tier trait run.                                                                                                                                                        | `specht-conventions` § Testing                       | Active  |
| B-008 | Given the `AcceptanceTest` target, the scenarios of every `.feature` file the acceptance project links run.                                                                                                                                    | `specht-conventions` § Testing                       | Active  |
| B-009 | Given the `Pack` target, exactly one package, `specht.tool.<version>.nupkg`, is written under `.artifacts/nupkg/`, where `<version>` is the version the build computes for the commit.                                                         | AGENTS.md § Build and test; `0001-F2` C-5            | Amended |
| B-010 | Given a fresh clone, and a token with package read once the local tool manifest names `specht.tool` (`0055-F7` B-003, A-1), `dotnet tool restore` installs every dotnet tool the build and the hook invoke, at the versions the manifest pins. | README § 8 step 1; AGENTS.md § Setup; C-3            | Amended |
| B-011 | Given `dotnet husky install` has run in a clone, a commit runs the pre-commit hook.                                                                                                                                                            | AGENTS.md § Setup; owner, 2026-10-08                 | Active  |
| B-012 | Given a staged C# file whose staged content differs from the repository's formatting rules, the hook refuses the commit and names that file.                                                                                                   | owner, 2026-10-08; A-4; B-021                        | Amended |
| B-013 | Given `0001-F2`'s check command and `0001-F5`'s rule settings both exist, a staged file under a `.spec/` directory or a staged `.feature` file, and an error-severity violation reported on the tree, the hook refuses the commit.             | owner, 2026-10-08; Must-1; A-4; C-7; `0001-F2` B-010 | Amended |
| B-014 | Given a commit staging no C# file, no Markdown file, no file under a `.spec/` directory and no `.feature` file, the hook runs none of its checks and lets the commit through.                                                                  | `specht-conventions` § Build and test; A-4; OQ-1     | Amended |
| B-015 | Given any commit, the hook modifies no file in the working tree and no entry in the index.                                                                                                                                                     | C-2; A-3                                             | Active  |
| B-016 | Given a staged Markdown file whose staged content differs from the repository's Markdown formatting rules, the hook refuses the commit and names that file.                                                                                    | OQ-1, OQ-2 (owner, 2026-10-08); C-2; A-4; B-021      | Amended |
| B-017 | Given a Markdown file whose formatting differs from the repository's Markdown formatting rules, the `Format` target exits non-zero and names that file.                                                                                        | OQ-2 (owner, 2026-10-08); C-1; C-2                   | Active  |
| B-018 | Given `./build.sh` with no target name, and `Compile` or `Test` fails, the build exits non-zero.                                                                                                                                               | AGENTS.md § Build and test; split from B-001         | Active  |
| B-019 | Given two clones on different machines, the `Format` target and the hook run the Markdown formatter at the version committed in the repository.                                                                                                | C-3                                                  | Active  |
| B-020 | Given the `Format` target invoked with a set of files, it checks those files and no other.                                                                                                                                                     | OQ-2 (owner, 2026-10-08); C-1                        | Active  |
| B-021 | Given a commit, the hook runs its formatting checks by invoking the `Format` target with the staged files.                                                                                                                                     | OQ-2 (owner, 2026-10-08); C-1                        | Active  |
| B-022 | Given a staged file whose staged content is unformatted and whose working-tree copy is formatted, the hook refuses the commit.                                                                                                                 | A-4; B-021                                           | Active  |
| B-023 | Given `0001-F2`'s check command does not yet exist, the `SpecCheck` target reports that the check is not yet available and exits successfully.                                                                                                 | owner, 2026-10-08; C-7                               | Active  |
| B-024 | Given `0001-F5`'s rule settings exist and an approved specification in this repository whose traceability matrix has a `Missing` row, the `SpecCheck` target reports `SPEC060` at warning severity and exits successfully.                     | owner, 2026-10-08; AGENTS.md § `specht`; C-6         | Amended |
| B-025 | Given `0001-F2`'s check command exists and `0001-F5`'s rule settings do not yet, the `SpecCheck` target prints the check's violations and exits successfully whatever they are.                                                                | owner, 2026-10-08; C-7                               | Active  |

## 4. Constraints

<!-- last written by: spec-author, 2026-10-08 -->

| ID  | Constraint                                                                                                                                                                                                                                                                         | Rules Out                                                                                                                                                                     |
| --- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| C-1 | Every gate is a NUKE target; the hook and every workflow invoke targets by name.                                                                                                                                                                                                   | A `dotnet test`, `dotnet format` or `dotnet run` line in a workflow or the hook that no target runs; a gate CI has and the build lacks.                                       |
| C-2 | Formatting is verified, never fixed, by the build and by the hook.                                                                                                                                                                                                                 | Applying analyzer code fixes in `Format` or the hook; a hook that formats and re-stages files.                                                                                |
| C-3 | A tool the build or the hook invokes is pinned at a committed version: a dotnet tool in the local tool manifest, the Markdown formatter in a committed version file.                                                                                                               | A global tool install as a prerequisite; an unpinned tool fetched at run time, such as the formatter fetched at its latest version on each commit.                            |
| C-4 | The build project takes its package versions from `Directory.Packages.props`.                                                                                                                                                                                                      | A version in `.build`'s project file.                                                                                                                                         |
| C-5 | A target writes only under `.artifacts/` and the git-ignored `bin/` and `obj/` directories.                                                                                                                                                                                        | A report, package or coverage file written into a tracked path; a committed generated report.                                                                                 |
| C-6 | `SPEC060` is a warning in this repository through its manifest's rule settings (`0001-F5` B-010), so a `Missing` row is not a failure (AGENTS.md § `specht`); this waits on `0001-F5` (owner, 2026-10-08).                                                                         | A change to the engine's `SPEC060`; a filter in the build, the hook or CI that drops `SPEC060` lines; a commit or run failed for a `Missing` row.                             |
| C-7 | `SpecCheck` gates the hook, CI and releases only once both `0001-F2`'s check command and `0001-F5`'s rule settings exist, so it gates from `0001-F5`'s arrival; until then it reports and never fails (B-023, B-025). A dependency on `0001-F2` and `0001-F5` (owner, 2026-10-08). | A `SpecCheck` failure before `0001-F5` lands; any window in which `SPEC060` is an error and `SpecCheck` gates; a gate removed and re-added by hand when either Feature lands. |

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

| Claim ID | Scenario                                                             | Test    | Status  |
| -------- | -------------------------------------------------------------------- | ------- | ------- |
| B-001    | The default build compiles and tests                                 | Missing | Missing |
| B-002    | The Windows entry runs the same target                               | Missing | Missing |
| B-003    | Unformatted code fails the format gate                               | Missing | Missing |
| B-004    | The format gate changes nothing                                      | Missing | Missing |
| B-005    | The test gate runs every tier                                        | Missing | Missing |
| B-006    | The unit tier runs alone                                             | Missing | Missing |
| B-007    | The integration tier runs alone                                      | Missing | Missing |
| B-008    | The acceptance tier runs the linked scenarios                        | Missing | Missing |
| B-009    | Packing writes one tool package                                      | Missing | Missing |
| B-010    | A fresh clone restores every tool the build needs                    | Missing | Missing |
| B-011    | An installed hook runs on commit                                     | Missing | Missing |
| B-012    | The hook refuses unformatted staged code                             | Missing | Missing |
| B-013    | The hook refuses a staged specification that fails                   | Missing | Missing |
| B-014    | The hook lets an unrelated commit through                            | Missing | Missing |
| B-015    | The hook changes nothing it checks                                   | Missing | Missing |
| B-016    | The hook refuses unformatted staged Markdown                         | Missing | Missing |
| B-017    | Unformatted Markdown fails the format gate                           | Missing | Missing |
| B-018    | A failing default build exits non-zero                               | Missing | Missing |
| B-019    | Every clone runs the same Markdown formatter                         | Missing | Missing |
| B-020    | The format gate checks only the files it is given                    | Missing | Missing |
| B-021    | The hook formats through the build                                   | Missing | Missing |
| B-022    | The hook checks what is staged, not the working tree                 | Missing | Missing |
| B-023    | The self-check reports itself unavailable before the command exists  | Missing | Missing |
| B-024    | A missing test is a warning here                                     | Missing | Missing |
| B-025    | The self-check reports without gating before the rule settings exist | Missing | Missing |

## 10. Lessons / Spec Deltas

<!-- last written by: spec-author, 2026-10-08 -->

None.

## 11. Open Questions

<!-- last written by: spec-author, 2026-10-08 -->

| ID   | Question                                                                                                                                                                                                                                                        | Blocks     | Resolution                                                                                                                                                                                                                                                                                                                        |
| ---- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| OQ-1 | The hook committed today also rewrites every Markdown file with a formatter fetched at commit time. The owner named Format and SpecCheck for the hook. Is Markdown formatting dropped, or a third gate - and if kept, does it verify (C-2) rather than rewrite? | B-015      | Resolved 2026-10-08 by the repository owner: kept as a third gate in the hook, verify-only - it fails on unformatted staged Markdown and never rewrites a file. B-016 added; B-014 amended; C-3 amended to pin the formatter.                                                                                                     |
| OQ-2 | C-1 makes every gate a build target that the hook calls by name, but the owner placed the Markdown check (B-016) in the hook only. Is Markdown verification a build target that CI also runs (`0055-F2` B-004), or a hook-only check that C-1 exempts?          | B-016; C-1 | Resolved 2026-10-08 by the repository owner: Markdown verification is part of the `Format` target, which checks Markdown as well as C# without changing either; CI enforces it through `Format` (`0055-F2` B-004), and the hook calls the same gate scoped to the staged files. C-1 stands unamended. B-017 added; B-016 amended. |

## 12. Sign-off

<!-- last written by: spec-reviewer, 2026-10-08 -->

| Section | Status | Reviewer      | Note                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            |
| ------- | ------ | ------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1-5     | 🟡     | spec-reviewer | Draft; not reviewed                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             |
| 1-5     | 🔴     | spec-reviewer | Round 1 (2026-10-08) - blocked, reviewing commit `04fdc28`. Structure, frontmatter, ids, tags, § 9 and the `depends_on`/`blocks` pairs across `0055-F1`..`F8` are sound; OQ-1 and OQ-2 are claimed. Blocking (spec-author): (1) B-010 says `dotnet tool restore` installs every tool the build and the hook invoke, but C-3 pins the Markdown formatter B-016 and B-017 need in a committed version file that restore does not read, and after `0055-F7` B-003 the restore needs a read token (`0055-F7` A-1); scope B-010 to the dotnet tools and the token, and add a claim that proves C-3's formatter pin. (2) C-1 and OQ-2's resolution have the hook call `Format` scoped to the staged files, but no claim says `Format` accepts a file set and checks only it, B-016 names "the Format gate's Markdown check", which is not a target, and B-012 names none, so a hook calling `dotnet format` and `prettier` directly - as `.husky/pre-commit` does at `857f00d` - meets B-012 and B-016 while breaking C-1; add that claim and state B-012 and B-016 as the hook invoking `Format` with the staged files. (3) B-013 refuses a commit for an error on the whole tree, contradicting A-4 (never refused for drift in an untouched file); and with `SPEC060` an error (README § 4) on the six approved specifications here that carry `Missing` rows, every commit staging a `.spec/` file is refused until all their tests exist. Reconcile A-4 with B-013 and `0001-F2` B-010, and take `SPEC060` against `Missing` to the owner (the same finding blocks `0055-F2`). Non-blocking (spec-author): B-004's "creates no file" is false for `dotnet format`'s restore, which writes `obj/`, and C-5 puts every write under `.artifacts/` though `Compile` writes `bin/` and `obj/` - scope both to tracked paths or adopt artifacts output; A-3 and § 2 row 3 describe the hook as it was before `857f00d`; B-012 and B-016 do not say whether a partially staged file is checked as staged or as in the working tree (the hook checks the working tree); B-009 uses `0055-F5`'s version and `0055-F5` B-005 uses `Pack`, yet neither lists the other in `depends_on`; B-001 and B-016 each carry more than one assertion. |
| 1-5     | 🔴     | spec-reviewer | Round 2 (2026-10-08) - blocked, reviewing `acf3cff`..`f32a31d`. Round-1 blockers (1) and (2) cleared: B-010 is scoped to dotnet tools and the read token and B-019 proves C-3's formatter pin; B-020 and B-021 make the hook call `Format` with the staged files, B-022 checks staged content, and B-012 and B-016 follow. Of (3), A-4 now agrees with B-013 and `0001-F2` B-010, and B-023 and C-7 state gate-on-arrival. Blocking (spec-author, escalate to the owner): C-7 makes `SpecCheck` gate when `0001-F2` lands and C-6 makes `SPEC060` a warning only through `0001-F5`, but `0001-F2`'s check command lands before `0001-F5` (`0001-F5` depends on `0001-F2`), and `0001-F1` C-9 holds every verdict, severity included, at the baseline until README § 8 step 5, so between those two landings `SpecCheck` gates with `SPEC060` still an error on the six approved specifications that carry `Missing` rows, and fails every commit staging a `.spec/` file and every run - against C-6's own Rules Out and B-024. Gate on `0001-F5`'s arrival instead, or have the owner accept the window in C-6 and C-7; `0055-F2` C-4 and `0055-F6` C-6 take the same answer. Round-1 non-blocking findings are fixed.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         |

## Tasks

None cut. Items are cut from § 3 after agreement.

## Scoring

| Field | Value | Basis          |
| ----- | ----- | -------------- |
| value | 0     | Not yet scored |
| risk  | 0     | Not yet scored |
