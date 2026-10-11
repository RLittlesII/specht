---
title: "Lesson 0001: The package id had no claim"
description: "A rename changed the published package id from specht.tool to tool with every gate green, because the id was held only by a constraint, by a claim whose scenario does not run, and by a literal inside the test of another claim that the same commit edited"
type: lesson
kind: product
---

# Lesson 0001: The package id had no claim

**Date:** 2026-10-11
**Kind:** product

## Symptom

`./build.sh Pack` writes `tool.<version>.nupkg`. `src/tool/tool.csproj` sets
`<PackageId>tool</PackageId>`, and the release workflow's push line names
`tool.<version>.nupkg`. C-5, `0055-F1` B-009, `0055-F6` B-003, `AGENTS.md` and
`README.md` all give `specht.tool`. Every gate was green. No tag had been
pushed, so nothing was published under the wrong name.

## Root cause

Commit `a62689f`, "chore: clean up namespaces (#96)", renamed the project
`specht.tool.csproj` to `tool.csproj`. The id had come from the project's file
name, so the commit wrote one in, and wrote `tool`. Nothing held it:

- **C-5 names the id, and a constraint has no § 9 row.** No test cites it.
- **`0055-F1` B-009 names the file, and its scenario does not run.** The
  build has no tests by the owner's decision (`0055-F1` § 8); the row is
  `Missing`, and the unlinked step class packs a made-up project that carries
  its own id.
- **The one executed test that read the real package belongs to another
  claim.** `PackageVersioningSteps` proves `0055-F5` B-005, the package's
  version, and held the file name as a literal on the way to it. The same
  commit changed that literal from `specht.tool` to `tool`, and the release
  workflow's push line with it, so product and proof moved together.
- **The commit cited no claim and claimed no exemption.** A rename that
  preserves C-5 says so in the form `AGENTS.md` § Exemptions fixes; this one
  did not, and C-5 did not hold.

The specification said the id once, where nothing checks, and never as a
claim.

## Spec delta

B-017 added: the packed package's id is `specht.tool`. C-5 is unchanged. No
skill changes: the exemption rule that would have stopped the rename already
exists.

## Claim

- B-017 — `Missing` until the item that fixes the id lands; § 9 names the
  test.
