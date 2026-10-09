---
title: "Lesson 0003: The unit tier had no home"
description: "Epic 0055 shipped its build and CI with acceptance scenarios alone, because the test-writer contract defined a unit test by the engine, gave other code nowhere to put one, and let a scenario mark a claim Covered."
type: lesson
kind: process
---

# Lesson 0003: The unit tier had no home

**Date:** 2026-10-08
**Kind:** process

## Symptom

Epic `0055`'s first two Features, the build and CI, were delivered with
acceptance scenarios and no unit test: `BuildSteps` binds 38 steps,
`ContinuousIntegrationSteps` 15, and no `*.Unit.Tests.cs` file tests either. The
build's § 8 recorded "There is no unit tier for this Feature yet: the build is
target wiring with no logic of its own to isolate". `Build.cs` splits `Files` by
extension, skips a formatter with nothing to check, runs both formatters before
it fails, builds `--include` arguments and reads the Prettier version from
`package.json`. In the same period the engine, copied with its tests, had about
fifty unit tests. The owner noticed the imbalance.

## Root cause

The `test-writer` contract made acceptance-only coverage the easiest legal outcome:

1. **The unit tier was defined by the engine.** The role owned unit tests
   "under `test/specht.tests`, built over a `SpecTree`", and the testing
   reference defined a unit test as "one rule or one reader". No test project
   referenced the build, so the role had nowhere to put a unit test for one, and
   "no unit tier" was the only verdict available.
2. **§ 9 could not show a gap.** One Test cell and one Status meant a step
   class made a claim `Covered`, whether or not the mechanism beneath it was
   tested.
3. **"The fewest tests" read as one per claim.** The scenario came first and
   took the slot. "Both tiers" was advice, and the role judged for itself what
   counted as a mechanism.
4. **Nobody checked the verdict.** `spec-reviewer` checked § 9 only for one row
   per claim.

## Spec delta

- `test-writer`: unit tests live in the `*.Tests` project for the code under
  test. § 8 names each claim's mechanism and its unit test, or why it has none,
  naming the code read. "No project to put it in" is a gap, not a verdict.
  Fewest tests is counted per tier.
- `.spec/templates/feature.md`: the § 9 Test cell names every test. `Partial`
  marks a scenario over an untested mechanism; `Covered` needs both. The § 9
  header is pinned in the manifest and did not change.
- `spec-reviewer`: an unpinned mechanism, or a "no unit tier" verdict that names
  no code, is a finding.
- `0055-F1` B-003, B-017, B-019 and B-020, and `0055-F2` B-004, are now
  `Partial`. Item `0098` gives the build a unit tier. Amended 2026-10-09: the
  owner closed `0098` without delivery, keeping `0055-F1` § 8's decision that
  the build has no tests; those claims stay `Missing`, not `Partial`.

## Claim

- `0055-F1` B-020 — the unit test `0098` adds for `Include`, `Quote` and the
  extension split, cited in § 9 beside `BuildSteps`. Amended 2026-10-09: no
  test proves it; the owner closed `0098` without delivery.

## Skill

[`specht-conventions` § Testing](../../.claude/skills/specht-conventions/references/testing.md):
tiers are defined by what is under test, not where it lives. Code outside `src/`
has a unit tier in its own `*.Tests` project. Acceptance-only coverage is named
as the failure mode, and Never add gains "a claim `Covered` by a scenario alone".
