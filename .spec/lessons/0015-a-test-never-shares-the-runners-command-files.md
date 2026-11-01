---
title: "Lesson 0015: A test never writes to the runner's command files"
description: "A build a test starts writes no version variables, and a test that simulates a runner gives it private command files"
type: lesson
kind: process
---

# Lesson 0015: A test never writes to the runner's command files

**Date:** 2026-11-01
**Kind:** process

## Symptom

Pull request #648's `build` job failed twice after the NUKE target reported
Succeeded with 0 failed tests: run 41274511771 attempt 1 on vitruvius-2, step
"💨 Acceptance Test", and run 41277685527 attempt 1 on vitruvius-3, step
"💨 Integration Test". The runner then printed "Unable to process file command
'env' successfully." and "Value cannot be null. (Parameter 'name')". A rerun of
the first passed, and `main` at the same code passed (item `0326`).

## Root cause

No rule said what a test's child process may inherit. Every project references
Nerdbank.GitVersioning 3.10.94, whose default cloud-build settings make a build
under `GITHUB_ACTIONS=true` append `GitAssemblyInformationalVersion`,
`GitBuildVersion` and `GitBuildVersionSimple` to the file `GITHUB_ENV` names.
Tests start child `dotnet` and build processes that inherit the step's
environment, so in CI each of them appended to the runner's real `GITHUB_ENV`.
`CodeAnalysisReleaseIntegrationTests` ran the NUKE build as a simulated runner
and left that real file in place as well. Nothing reads those variables.

Why the file became unreadable is a working hypothesis, not an observation:
parallel child builds appended to the same file at once and corrupted a line.
The runner deletes the file at the end of the step, so the bad line was never
seen.

## Spec delta

No claim changes. `version.json` sets `cloudBuild.setVersionVariables` to
`false`, so no build writes the variables. Nerdbank.GitVersioning reads that
setting from the committed `version.json`, not the working copy: an
uncommitted edit changes nothing. `CodeAnalysisReleaseIntegrationTests` gives
its simulated runner private `GITHUB_ENV`, `GITHUB_OUTPUT`,
`GITHUB_STEP_SUMMARY` and `GITHUB_PATH` files under its own scratch directory,
which it deletes.

## Claim

- None changes. `RunnerCommandFilesIntegrationTests.RepositoryBuild_WhenRunAsARunnerStep_ShouldWriteNothingToTheRunnerEnvironmentFile` proves a repository build under `GITHUB_ACTIONS=true` leaves a private `GITHUB_ENV` empty; at `7ee35dbf` it found the three lines.

## Skill

`specht-conventions` `references/testing.md` § "Fixtures" now states that a
build a test starts writes nothing to the runner's command files, held by
`version.json` and proved by `RunnerCommandFilesIntegrationTests`, and that a
test simulating a runner gives it private ones and deletes them; § "Never add"
lists a simulated runner left with the real files.
