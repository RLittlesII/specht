---
title: "Lesson 0002: Recognition reads the runner's discovered cases"
description: "Authoritative recognition required an eligible binding, so a discovered test method with no verifies was never a recognised test and mandatory enforcement could not report it"
type: lesson
kind: process
---

# Lesson 0002: Recognition reads the runner's discovered cases

**Date:** 2026-11-06
**Kind:** process

## Symptom

Item 0302's packaged consumer fixture stripped `<verifies>` from a test
method, re-ran discovery and the run, and rebuilt with complete, current
evidence under mandatory `testMethod`/`verifies` enforcement. No SPCT0007 was
reported (branch `0302/packaged-gherkin-vstest-fixture`, commit `38670c0d`,
the `missing-verifies-rebuild` step).

## Root cause

Authoritative recognition intersected the artifact's discovery `methods` with
the methods carrying an eligible resolved binding. Discovery `methods` is the
compiled method inventory, helpers included, not the set the runner
discovered, so the intersection was the bound methods alone. A method with no
`verifies` has no binding, so it could never be a recognised test, and the
mandatory role could not report on the one method it exists for.

0201-F3 § 7, "Recognition has two modes", stated that rule. The analyzer tests
built their artifact with an eligible binding the source did not carry, so no
test drove recognition from an artifact a runner could write.

## Spec delta

0201-F3 § 7's authoritative bullet now states that recognition is the methods
a discovery case names, together with the eligible resolved bound methods, and
that discovery `methods` alone recognises nothing. The analyzer tests build
their artifact the way the runner's logger does, from the compilation export.

## Claim

- [0201-F6 B-001 and B-005](../README.md#9-traceability-matrix) — § 9 names
  the tests that drive recognition from a faithful artifact in mandatory
  scope; B-005 is Partial until its scenario's steps are bound.

## Skill

`test-from-scenarios` § "Fixtures are synthetic" gains the rule that a fixture
standing in for another component's output is one that component could
produce from the same input, derived from the producer where it exists, never
with a field the input would not yield.
