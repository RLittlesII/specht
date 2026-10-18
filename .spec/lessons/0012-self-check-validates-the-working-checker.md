---
title: "Lesson 0012: Self-check validates the working checker"
description: "A published checker cannot gate development of schema capabilities it does not yet support"
type: lesson
kind: process
---

# Lesson 0012: Self-check validates the working checker

**Date:** 2026-10-17
**Kind:** process

## Symptom

After 0227, the published checker 0.1.131 rejected the required `tests` and `owners`
settings before the build compiled the working checker. Three installed-tool
acceptance scenarios failed for the same reason, while the working checker
accepted the tree. The investigation incorrectly requested a newer published
version as a prerequisite.

## Root cause

Check B-014 and the build's tool-pin constraint required an older published
artifact to judge development of its successor. The self-hosted gate and the
consumer installation contract shared one pin, leaving no path for unpublished
schema development.

## Spec delta

Check decision 0014 retains installed-package validation but prepares that package
from the working checkout in an isolated local manifest. B-014 is amended; B-046
requires preparation failures to fail the gate, and B-047 prevents stale bytes at
an unchanged package version. Consumer installation remains separately pinned.

## Claim

- `0001-F2 B-014` — `CheckSteps.ThenTheWorkingPackageIsInstalled` and the fresh-package unit case.
- `0001-F2 B-046` — `CheckSteps.ThenPreparationFails` and the preparation-failure unit theory.
- `0001-F2 B-047` — `CheckSteps.ThenTheChangedCheckerRuns` and the fresh-package unit case.

## Skill

The working-package scenarios and unit tests prove these boundaries.
`coding-conventions` now requires a self-hosted validation gate to exercise the
tool built from the change; the repository companion states the execution path.
