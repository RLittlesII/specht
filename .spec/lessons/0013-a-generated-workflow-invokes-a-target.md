---
title: "Lesson 0013: A generated workflow invokes a target"
description: "Workflow behaviour hand-built as middleware steps belongs in a NUKE target the workflow invokes"
type: lesson
kind: process
---

# Lesson 0013: A generated workflow invokes a target

**Date:** 2026-10-25
**Kind:** process

## Symptom

In the review of pull request #281 (item `0295`), the mirror workflow's steps
were written by hand in an `Enhancements` middleware method of
`.build/SpechtBuild.Mirror.cs`. The owner's review: "This didn't have to be
middleware. It could have been a target. Targets should be the default."

## Root cause

No rule said that a generated workflow invokes a NUKE target. The CI generation
rules in `specht-conventions` covered regeneration, step order and the matrix,
but not where a job's work lives, so middleware that replaced the generated
steps passed every check.

## Spec delta

No claim changes. The workflow now invokes the `Mirror` target, which depends
on `MirrorTest`, through `InvokedTargets`; middleware adds only the dispatch
input, concurrency, permissions, checkout depth, SDK setup and step
environment (commit `07c14e85`). `0055-F9` OQ-5's default now names the .NET
SDKs the job sets up, and OQ-11 records its runner.

## Claim

- `0055-F9 B-036` — `MirrorWorkflowIntegrationTests.B036_PushToMainOnAMirroredPath_WhenGenerated_ShouldTriggerTheMirror` asserts the generated step is `./build.cmd --target Mirror --skip MirrorTest`.

## Skill

`specht-conventions` `references/coding.md` § "Generated and never hand-edited"
now states that a generated workflow invokes a NUKE target through
`InvokedTargets`, and that `Enhancements` middleware only adds what the
attribute cannot express.
