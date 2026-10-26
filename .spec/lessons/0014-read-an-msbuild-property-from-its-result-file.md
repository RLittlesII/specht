---
title: "Lesson 0014: Read an MSBuild property from its result file"
description: "A test reads an MSBuild property from the -getResultOutputFile file, never from dotnet msbuild console output"
type: lesson
kind: process
---

# Lesson 0014: Read an MSBuild property from its result file

**Date:** 2026-10-25
**Kind:** process

## Symptom

The `Main` workflow on `main` failed from `d1b34d48` (Actions run 40233517873) and in pull request #565's run 40257484584, only on the
self-hosted macOS ARM64 runner: the VSTest evidence fixture test expected the
reconciliation report's `inputState.state` to be `complete` and read
`incomplete`. Other runs on the same code passed (item `0301`).

## Root cause

No rule said how a test reads an MSBuild property. The fixture ran
`dotnet msbuild -getProperty:BundledNETCoreAppTargetFrameworkVersion` and took
the last line of the console output, which the .NET CLI shares with its own
diagnostics. Under load the MSBuild server timed out and printed "MSBuild
server unavailable: ... Falling back to an in-process build." after the
value, so the generated manifest declared that sentence as a framework, no
export was current for it, and the reconciler correctly reported the inputs
incomplete.

## Spec delta

No claim changes; the reconciler and the logger were right. The two tests
that read a `-getProperty` value now pass `-getResultOutputFile:<path>` and
read the file: `test/build.tests/TestEvidenceVsTest.Integration.Tests.cs`
(`VsTestRun.Arrange`) and `test/acceptance/Build/BuildSteps.cs`
(`ThenItsNameCarriesTheVersionComputedForTheCommit`), which had the same
latent defect.

## Claim

- None changes. `TestEvidenceVsTestIntegrationTests.DocumentedSequence_WhenFollowedOnACleanCheckout_ShouldHoldCurrentPassingEvidenceForTheAuthoredVerifies` now holds when the CLI prints a diagnostic after the value.

## Skill

`specht-conventions` `references/testing.md` § "Fixtures" now states that a
test reads an MSBuild property from the file `-getResultOutputFile` writes and
never parses `dotnet msbuild` console output; § "Never add" lists the parse.
