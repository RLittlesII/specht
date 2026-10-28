---
title: "Lesson 0015: An action version change needs a from-scratch regeneration"
description: "Rocket.Surgery.Nuke copies an existing workflow's action versions into its regeneration, so ./build.sh Compile never applies a changed action declaration"
type: lesson
kind: process
---

# Lesson 0015: An action version change needs a from-scratch regeneration

**Date:** 2026-10-28
**Kind:** process

## Symptom

While item `0075` was designed, `codecov/codecov-action` and
`renovatebot/github-action` were bumped in the build source. `./build.sh --help`
and `--generate-configuration GitHubActions_ci --host GitHubActions` each
rewrote `ci.yml` and `renovate.yml` unchanged. With both files deleted first, the
same run wrote each bumped version.

## Root cause

Rocket.Surgery.Nuke 2.9.12's `GitHubActionsStepsAttribute.GetConfiguration`
ends with `NormalizeActionVersions` whenever the workflow file exists: every
`uses:` takes the version that file already names. It runs after the
`Enhancements` middleware and is not virtual. The rule in `specht-conventions`
said `./build.sh Compile` regenerates a workflow, which holds for everything but
an action version.

## Spec delta

`0055-F4` § 7 designs the `GenerateWorkflows` target, which deletes each
`[GitHubActionsSteps]` workflow and regenerates it, as the post-upgrade task
C-7 allowlists. No claim changes for this lesson; `0055-F4` § 10 records the
item's other deltas.

## Claim

- `0055-F4 B-013` and `0055-F4 B-014` — § 9

## Skill

`specht-conventions` `references/coding.md` § "Generated and never hand-edited"
and the generated-workflow trap in `AGENTS.md` now say that an action version
change needs `./build.sh GenerateWorkflows`.
