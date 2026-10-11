---
title: "Epic 0142: a GitHub Action that runs specht"
description: "A GitHub Action, shipped from this repository, that runs specht in any repository, writes its violations onto the pull request diff and into the step summary, and exits with the tool's own exit code; this repository's CI adopts it, so one implementation of the annotation remains"
id: "0142"
type: epic
status: needs-decomposition
priority: med
milestone: null
children: []
created: "2026-10-10"
updated: "2026-10-10"
github_issue: null
---

# Epic 0142: a GitHub Action that runs `specht`

## Summary

A GitHub Action, shipped from this repository, that runs `specht` for any
repository and puts its violations on the build: on the pull request's diff,
and in the step summary. The step's verdict is the tool's exit code.

Today only this repository gets its violations on a diff, through
`AnnotateChangedFiles` in `.build/SpechtBuild.GitHubActions.cs`, which the
`Specht` target in `.build/SpechtBuild.cs` calls. That code is part of this
repository's build and cannot be shipped. The action replaces it here and
gives every consumer the same thing.

The need and its scope were decided by the repository owner on 2026-10-10.
This file is the record of that decision. Nothing here is designed: the
action's form, its folder and its inputs' final names are not chosen. The
split into Features is a candidate the owner has not confirmed, and no
Feature specification exists yet.

## Business Value

**The problem.** A consumer's pull request shows no `specht` violation on its
diff. No problem matcher reads the tool's line (`0055-F2` decision 0002), and
the one implementation that writes annotations lives in this repository's
build. A consumer reads the step's log or writes the annotation code again.

**Why now.** `hooked` is about to adopt the tool, and more repositories
follow. Each would otherwise hand-write its own annotation step, which is the
hand-copied implementation `specht` exists to remove (AGENTS.md § Project),
one layer up.

**The solved state.** One `uses:` step gives a repository the run, the
annotations, the summary and the verdict. This repository's own CI uses that
same step, so the annotation exists in one place.

| Persona                                 | Standing  | Goal                                                               |
| --------------------------------------- | --------- | ------------------------------------------------------------------ |
| The maintainer of a consumer repository | Primary   | Violations on the pull request with one step and no code of theirs |
| A pull request's author and reviewer    | Primary   | See each violation at its line, and the whole list in one place    |
| This repository's maintainer            | Secondary | One implementation of the annotation, used here as a consumer does |

## Success criteria

Every row is the owner's answer of 2026-10-10 and is a Must. `R1` to `R8` are
this epic's names for those answers, so that a Feature specification can cite
one.

| #   | Criterion                                                                                                                                                                                      | Notes                                                                  |
| --- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------- |
| R1  | The action serves consumers and this repository. This repository's CI adopts it, and the build-side `AnnotateChangedFiles` is retired, so one implementation remains                           | Amends `0055-F2`; see "Conflicts with recorded specifications"         |
| R2  | A violation is written as a workflow command (`::error file=,line=,title=SPEC###`) so it shows on the diff, and the step summary (`GITHUB_STEP_SUMMARY`) lists every violation with the counts | No check run and no SARIF (Out of this epic)                           |
| R3  | By default only violations in files the pull request changed are annotated; an input widens the annotations to every violation                                                                 | The default is `0055-F2` decision 0002's scope                         |
| R4  | The tool is installed from the consumer's local tool manifest with `dotnet tool restore`. The action pins no tool version, and the consumer supplies the feed authentication                   | The manifest is the one place a consumer pins the tool (`0055-F7` C-2) |
| R5  | The action exits with the tool's exit code: `0`, `1`, `2`, `3` or `4`. Annotating and summarising never fail the step. A `strict` input maps to `--strict`                                     | The exit codes are AGENTS.md § CLI; `--strict` is `0001-F2` B-004      |
| R6  | The action lives in a subfolder of this repository and is used as `uses: RLittlesII/specht/<folder>@<ref>`                                                                                     | The folder's name is OQ-1                                              |
| R7  | The action runs on Linux, macOS and Windows runners                                                                                                                                            | Rules out a form that needs bash alone (OQ-3)                          |
| R8  | The action is versioned with the tool's release: one `v<version>` tag covers the tool and the action                                                                                           | The tag is `0055-F5` B-007's; the release is `0055-F6`                 |

## Features

**A candidate cut, awaiting the owner's confirmation.** Nothing below is
agreed. No Feature here has an id, a specification or a place in `children`:
a candidate takes its Feature id when its specification is written, and joins
`children` then, so that every child resolves to a file.

| Candidate | Name                              | Covers                                                                                                         | Serves     |
| --------- | --------------------------------- | -------------------------------------------------------------------------------------------------------------- | ---------- |
| F1        | Run and verdict                   | The inputs (`root`, `strict`, `command`), the restore from the manifest, the run, and passing the exit code on | R4, R5, R7 |
| F2        | Diff annotations                  | The workflow commands, the changed-file scope and the input that widens it, the cap, and the one-leg rule      | R2, R3     |
| F3        | Step summary                      | The Markdown summary of every violation and the counts                                                         | R2         |
| F4        | This repository adopts the action | Retiring `AnnotateChangedFiles`, amending `0055-F2`, and correcting the two stale statements                   | R1         |
| F5        | Releasing the action              | The tag shared with the tool, and the consumer-facing usage document under `docs/`                             | R6, R8     |

What the owner is asked to confirm:

- **F2 and F3 are cut apart.** The annotations are scoped to the pull
  request's files and capped by GitHub; the summary lists everything and has
  no cap. Each can fail with the other correct.
- **F4 is cut from F1 to F3.** It changes this repository's build and three
  recorded specifications, and delivers nothing to a consumer.
- **F1 names `command`**, an input that exists only on OQ-2's default. If
  OQ-2 is answered otherwise, F1 loses it.
- **The AGENTS.md "&" heuristic.** "Run and verdict" joins two nouns. They are
  kept together because the verdict is the run's exit code and cannot fail
  with the run correct; the owner may still cut them.

## What this epic waits on

None of these stops a Feature specification being written. Each stops a
consumer using the action.

| Waits on                                                                                                                               | Recorded in                      | Blocks                                            |
| -------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------- | ------------------------------------------------- |
| A first release. The repository has no tag, so there is no `<ref>` a consumer can pin                                                  | `0055-F6`; `0055-F5` B-007       | R6, R8                                            |
| The package id. `src/tool/tool.csproj` sets `PackageId` to `tool`; `README.md`, `0055-F6` and `0055-F7` name the package `specht.tool` | `0055-F6` B-003; `0055-F7` B-001 | R4: a consumer's manifest needs the id that ships |
| The feed token scope a consumer's CI supplies                                                                                          | `0055-F7` B-005                  | R4                                                |
| Access to the action from another repository while this one is private                                                                 | Nowhere yet (OQ-10)              | R6                                                |

## Constraints

Every constraint is hard. None is this epic's invention: each is recorded
elsewhere and still holds, and is listed so that a Feature specification cut
from this epic carries it forward.

| Constraint                                                                                                                   | Source                                   | What it rules out                                                                                          |
| ---------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------- | ---------------------------------------------------------------------------------------------------------- |
| The `specht` tool learns nothing about GitHub. The action reads the tool's output                                            | `0055-F2` C-6; `0055-F2` decision 0002   | A GitHub flag, option or output format in `src/`                                                           |
| The tool is given no base, diff or pull request input                                                                        | `0055-F2` C-7                            | The action passing the changed files to the tool; the action filters what the tool reported                |
| GitHub keeps 10 error and 10 warning annotations per step and 50 per job. The step summary is the surface with no cap        | `0055-F2` decision 0002                  | Relying on an annotation to show every violation                                                           |
| No output carries an absolute path                                                                                           | AGENTS.md § Invariants; brief § 9        | A path in an annotation or the summary that is not relative to the root the tool was given                 |
| The check stays deterministic and offline                                                                                    | AGENTS.md § Invariants                   | The action calling GitHub, or any network, to decide what the check reports                                |
| Only one job leg annotates                                                                                                   | `0055-F2` B-006; `0055-F2` decision 0002 | The same violation annotated twice                                                                         |
| A silently skipped annotation is a defect: when the changed files cannot be determined, the action says so with a `::notice` | `0055-F2` lesson 0001; owner, 2026-10-10 | Returning with no output when the scope is unknown. The notice reports the skip and annotates no violation |
| Annotating and summarising never decide the verdict                                                                          | `0055-F2` C-6; R5                        | A step failed because an annotation or the summary could not be written                                    |
| A workflow file is generated by the build, never edited by hand                                                              | `0055-F2` C-1                            | A hand-written `uses:` step in this repository's workflows (OQ-6)                                          |

## Conflicts with recorded specifications

R1 to R8 contradict three records. Each is named here with the record it
amends. This file amends none of them: the amendment is the work of the
Feature that owns the change.

| #   | Record                                                                                                                                                       | What it says                                                                             | What changes                                                                                                                                                             |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| 1   | `0055-F7` C-3 (`.config/.spec/README.md`)                                                                                                                    | Rules out consumer build wiring written here as if it were this repository's to maintain | A decision narrows C-3: the action is a product surface this repository ships; a consumer's own build target stays the consumer's                                        |
| 2   | `0055-F2` B-006, C-6 and § 7 (`.build/ContinuousIntegration/.spec/README.md`)                                                                                | The build's `Specht` target writes the annotations                                       | R1 moves the writing to the action. `0055-F2` is amended, and decision 0002 is superseded for who writes only; its scope, its single leg and its never-gating rule stand |
| 3   | AGENTS.md § `specht` ("diagnostics are MSBuild-shaped, so GitHub annotates them on the diff") and the summary comment in `src/specht/Model/SpecViolation.cs` | The MSBuild shape makes GitHub annotate with no plumbing                                 | `0055-F2` decision 0002 found that false: no problem matcher reads the line. Both statements are corrected                                                               |

## Open questions

None is resolved here. Each carries its default where one was given, and
travels to the Feature specification it binds, where it is resolved in place.
OQ-1 to OQ-7 were recorded on 2026-10-10 with their defaults. OQ-8 to OQ-11
were raised while writing this file, were not asked on 2026-10-10, and carry
no default.

| OQ    | Question                                                                                                                                                                                                                                                                              | Default                                                                                                                       | Who answers |
| ----- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------- | ----------- |
| OQ-1  | What is the action's folder named?                                                                                                                                                                                                                                                    | `.github/actions/specht/`: epic `0055` § Placement puts hand-written GitHub configuration under `.github/`                    | The owner   |
| OQ-2  | This repository runs the tool from source (`dotnet run --project src/tool`), and its local tool manifest does not name the tool. How does it use the action under R4?                                                                                                                 | A `command` input that defaults to `dotnet specht`; this repository passes its from-source command                            | The owner   |
| OQ-3  | What form does the action take, given R7 rules out bash alone?                                                                                                                                                                                                                        | None: left to the design. The candidates are a JavaScript action and a composite action over `pwsh`                           | The design  |
| OQ-4  | Does the action read the tool's stdout lines or the report document?                                                                                                                                                                                                                  | The report document from `--json` (`docs/schema/report.schema.json`); `--report` is specified and not built (`0001-F3` B-002) | The design  |
| OQ-5  | What does the action do on a push event?                                                                                                                                                                                                                                              | The step summary only, and no diff annotation (`0055-F2` decision 0002)                                                       | The owner   |
| OQ-6  | How does this repository's generated workflow gain the `uses:` step?                                                                                                                                                                                                                  | Declared in `.build/` and regenerated; never a hand edit (`0055-F2` C-1)                                                      | The design  |
| OQ-7  | When R3 is widened, does the action annotate past GitHub's cap, or stop?                                                                                                                                                                                                              | Stop at the cap and write one `::notice` naming the summary                                                                   | The owner   |
| OQ-8  | R5 exits with the tool's exit code, and `0055-F2` C-4 says this repository's `Specht` step reports and passes until `0001-F5`'s rule settings exist. When this repository adopts the action before then, which holds?                                                                 | None                                                                                                                          | The owner   |
| OQ-9  | R7 runs the action on three operating systems, and only one leg may annotate. In a consumer's matrix, which leg annotates, and who chooses it?                                                                                                                                        | None                                                                                                                          | The owner   |
| OQ-10 | A consumer can use an action from a private repository only when that repository grants access in its settings. `0055-F8` B-010 sets this repository's visibility to public, and no checklist row names that access. Does the checklist gain the row, or does the wait end at public? | None                                                                                                                          | The owner   |
| OQ-11 | R7 names macOS, and this repository's CI runs on Linux and Windows only (`0055-F2` B-003). What proves the action on macOS?                                                                                                                                                           | None                                                                                                                          | The owner   |

## Placement

No Feature specification exists, so none is placed. A specification sits
beside what it specifies: F1 to F3 and F5 beside the action, in the folder
OQ-1 names, and F4 beside the build it changes, under `.build/`.

This epic file lives at `epics/0142-github-action/epic.md`, where schema
version 1's epic glob `epics/**/epic.md` discovers it (`0001-F6` decision
0001).

## Status

`needs-decomposition`: the candidate cut is not confirmed and no child
exists. The epic is not `blocked`: nothing in "What this epic waits on" stops
a Feature specification being written, and the owner did not decide that the
epic waits. The next step is the owner's: confirm or change the candidate
cut, and each confirmed Feature then begins with its own specification.

## Out of this epic

| Item                                                              | Where it lives instead                                                                     |
| ----------------------------------------------------------------- | ------------------------------------------------------------------------------------------ |
| A check run through the GitHub Checks API                         | Not built: workflow commands and the step summary are the two surfaces (R2)                |
| SARIF and code scanning                                           | Not built (R2)                                                                             |
| A GitHub option, flag or output format in the tool                | Never: the tool learns nothing about GitHub (`0055-F2` C-6)                                |
| A tool version pinned by the action                               | The consumer's local tool manifest (R4; `0055-F7` C-2)                                     |
| Feed authentication supplied by the action                        | The consumer's workflow (R4); what it needs is `0055-F7`'s to document                     |
| A problem matcher over the tool's lines                           | Rejected in `0055-F2` decision 0002                                                        |
| A tag or a version line for the action apart from the tool's      | Never: one `v<version>` tag covers both (R8)                                               |
| What the check reports, its exit codes and the report document    | `0001-F2`, `0001-F3`; the action only runs the tool and reads its output                   |
| Publishing the package and creating the release                   | `0055-F6`                                                                                  |
| The amendments to `0055-F2`, `0055-F7`, AGENTS.md and the comment | The Feature that owns each change (candidate F4, and the decision narrowing `0055-F7` C-3) |
