---
title: "Lesson 0001: Every command-line shape runs against the real git"
description: "From item 0008 on, git mode listed no tracked epic file under the default manifest and listed a tracked file under an excluded obj for a test-file glob, because git ls-files mis-applies an exclude pathspec beside glob pathspecs that share a leading directory, and the real-git tests ran only globs that begin with **."
type: lesson
kind: process
---

# Lesson 0001: Every command-line shape runs against the real git

**Date:** 2026-11-10
**Kind:** process

## Symptom

Found by item [`0009`](../../.issue/0009-discovery-mode-parity.yml)'s proof of
`0001-F6` B-007, red at `5c5fba46`. On `main` since item `0008` (`782c3850`,
pull request #815), git mode in a work tree under the default manifest:

- **lists no tracked epic file.** `FindEpics` gives git
  `:(glob)epics/**/epic.md` beside `:(exclude,literal).spec`, from the default
  `/.spec` exclusion; git lists nothing and exits `0`. The walk lists every
  epic file, so a check in a git work tree reads no epic file, and B-007's
  scenario, on a clone of this repository, failed on its epics. An untracked
  epic file was still listed.
- **lists a tracked file under an excluded directory.** `FindTestFiles` with
  `test/**/*.cs` beside `:(exclude,glob)**/obj/**` listed a tracked
  `test/obj/b.cs`, which B-010 keeps out. B-010 had been `Covered` in § 9 since
  `20c8287d`.

`FindSpecifications` was not affected: `**/.spec/README.md` has no leading
directory.

## Root cause

A defect in `git ls-files`, observed with git 2.50.1 on macOS: beside an exclude
pathspec, glob pathspecs that share a non-empty leading directory list too few
tracked files or too many. A reading of git's `v2.50.1` source, not run, places
it in the common prefix `ls-files` computes from the positive pathspecs, by
whose length every item is then advanced, exclude items included. One observed
row does not follow from that reading, so the trigger is stated from
observation. The evidence is in the [specification](../README.md), § 7 "The
epic listing".

Why it reached `main` is the process gap. Item `0008`'s design probed the
exclude pathspec beside `**/`-leading globs only, the `features` layout's shape,
and its real-git tests did the same: the exclusions integration test's globs
all begin with `**`. The command-line theory pinned the epic and test-file
finders' arguments against a recorded double, never against git, so the two
shapes the code builds with a leading directory never ran. A pass on one shape
was read as a pass for every finder.

The classification is spec-author's: process, because a rule that a test runs
each shape the code hands a real dependency would have had item `0008`'s
test-writer run `FindEpics` and `FindTestFiles` against git and see both
failures; with a product remedy, because no claim was wrong and the remedy that
proves the behaviour is B-007's scenario and the tests beneath it.

## Spec delta

No claim, constraint, open question or scenario changes. B-007 and B-010 hold
as written; the code did not meet them in git mode.

The fix is the implementer's, at `8a8b7572`, and the owner was not asked:
`HeadEscaped` writes `\` before each glob pathspec's literal first character, so
no glob pathspec leaves git a leading directory to read as a common prefix, and
the exclusions stay exclude pathspecs and `--exclude` patterns (B-010, C-1).
§ 7 "Design for item `0008`" is corrected and "The epic listing" records the
evidence and the rejected options; § 6 rows 13 and 18 follow. Whether the
respelt glob is still drawn from the manifest (C-2) is for `spec-reviewer`. The
untracked scan now starts at the root for every finder, not measured, a concern
for `benchmarker`.

## Claim

`0001-F6` B-007 and B-010 — § 9. B-007's scenario compares both modes on a clone
of this repository; beneath it a real-git test of `FindEpics` and one of
`FindTestFiles` beside an excluded `obj` (B-010), each red with
`SpechtDiscovery.cs` at `5f2470cb` and green at `8a8b7572` (§ 8).

## Skill

[`test-from-scenarios`](../../../../../.agents/skills/test-from-scenarios/SKILL.md)
gains **"Each shape that crosses a real boundary"**: a test against a real
dependency runs every distinct shape of input the code builds for it, not one
representative. Its `Never add` gains the real-dependency test that runs one
shape where the code builds several.
