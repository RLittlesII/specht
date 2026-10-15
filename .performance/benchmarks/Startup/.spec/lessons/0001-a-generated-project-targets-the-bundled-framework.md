---
title: "Lesson 0001: A generated project targets the bundled framework"
description: "The cold-start session generated its baseline tool for a literal net10.0 and restored it from an isolated feed; once global.json pinned the .NET 11 SDK, which carries no net10.0 packs, the restore failed on main on both images, and the pull-request build, which runs no benchmark, never saw it."
type: lesson
kind: process
---

# Lesson 0001: A generated project targets the bundled framework

**Date:** 2026-10-14
**Kind:** process

## Symptom

The `Benchmark` step of the `main` workflow failed on `ubuntu-latest` and
`windows-latest` on every push from `73ff7ab` (item `0217`, pull request #192).
Run 38665634106 is one such failure:

```
System.InvalidOperationException: Cold-start setup failed: Cold-start baseline restore exited 1:
<session>\baseline\empty.csproj : error NU1101: Unable to find package Microsoft.NETCore.App.Ref.
No packages exist with this id in source(s): C:\Program Files\dotnet\library-packs, local
```

The ASP.NET Core and Windows Desktop reference packs and the apphost pack
failed the same way. Every pull-request check had passed.

## Root cause

[`ColdStartSession`](../../ColdStartSession.cs) generated the baseline tool
(B-007, C-8) with a literal `<TargetFramework>net10.0</TargetFramework>` and
restored it with every inherited source cleared, one session source holding
only packages the session packed, and a private package folder (C-4).

Item `0217` pinned SDK `11.0.100-rc.2.26504.105`. That SDK resolves `net10.0`
to targeting and apphost packs at `10.0.13`. It carries none; the SDK 10
installed beside it carries `10.0.12`. With no source able to supply
`10.0.13`, the restore failed. It fails the same way on a developer machine
with SDK 10.0.401 beside SDK 11.

Item `0217` fixed this failure in the consumption fixture (`0055-F7` § 7) and
did not search for the other projects generated at run time.

It reached `main` because the `Benchmark` step runs on a push to `main` only
(`0109-F1` A-4); no pull-request check builds the cold-start session.

## Spec delta

No delta. B-007 and C-8 hold as written, and the generated project was wrong.
It now targets `net$(BundledNETCoreAppTargetFrameworkVersion)`, the framework of
the SDK that builds it, whose packs that SDK carries. [§ 7](../README.md)
records why, and which framework `specht` runs on beside it. Item
[`0220`](../../.issue/0220-cold-start-baseline-bundled-framework.yml) records
the fix.

## Claim

- B-007 - `Missing`, under the owner's no-tests ruling for benchmark code
  (§ 9). The cold-start benchmark under the Short job completes its setup and
  reports the three rows; the next push to `main` shows it on both images.

## Skill

[`specht-conventions` § Testing](../../../../../.agents/skills/specht-conventions/references/testing.md)
§ Fixtures gains **"A project generated at run time targets the bundled
framework"**: a project a test or benchmark writes and restores from an
isolated feed targets `net$(BundledNETCoreAppTargetFrameworkVersion)`, never a
literal framework, and a change to `global.json` or `TargetFrameworks`
searches for a literal `<TargetFramework>` in generated project text. Its
`Never add` gains the literal framework.
