---
title: "Lesson 0001: A workflow with one image has no matrix"
description: "Splitting the generated workflow in two left the pull request's workflow with one image and so no matrix; two conditions that read matrix.os became false on every pull request, and annotations and the coverage upload stopped without any check failing."
type: lesson
kind: process
---

# Lesson 0001: A workflow with one image has no matrix

**Date:** 2026-10-10
**Kind:** process

## Symptom

From pull request 97 (`dc59b81`) on, no pull request carried a specification
annotation on its diff and none sent coverage to Codecov. Every check stayed
green. Seen on pull request 99, run 38102606537: 316 `error SPEC###` lines in
the log, 76 of them in files the pull request changed, and no `::error file=`
command.

## Root cause

Pull request 97 split the one generated workflow in two: `ci`, a pull request
on `ubuntu-latest` alone, and `main`, a push on both images. The generator
emits a `matrix` only for a workflow with more than one image, so in `ci.yml`
`${{ matrix.os }}` is the empty string. Two conditions still read it:

- `AnnotateChangedFiles` returned unless `MATRIX_OS` was `ubuntu-latest` (B-006).
- The Codecov upload step ran only when `matrix.os == 'ubuntu-latest'`
  (`0055-F3` B-002).

An expression that names a context which is not there is not an error on
GitHub Actions: it is empty, the comparison is false, and the step is skipped
or the method returns. A skipped step fails nothing.

[§ 7](../README.md) already gave the reason to read `RUNNER_OS` in the
`changes` step - the runner sets it in every step, matrix or not. The reason
was written for one step and not applied to the other two, and nothing sent a
change of images back to the conditions that depended on them.

## Spec delta

No claim, constraint or scenario changes: B-006 and `0055-F3` B-002 hold as
written, and the workflows were wrong. Item
[`0140`](../../.issue/0140-annotate-without-a-matrix.yml) corrects § 7 and § 8
of this specification and of [`0055-F3`](../../../../test/.spec/README.md)
where they name `MATRIX_OS` or `matrix.os` for these two conditions; each has a
§ 10 entry.

Not reconciled here: § 3 and § 7 still describe one workflow with a matrix of
two images on a pull request (OQ-5).

## Claim

- B-006 - `Missing`, under the owner's no-tests decision (§ 8). The guard in
  `AnnotateChangedFiles` reads `RUNNER_OS` and returns unless it is `Linux`.
- `0055-F3` B-002 - `Missing`, under the same decision. The upload step's
  condition reads `runner.os == 'Linux'`.

Both are first shown live by the run on the pull request that delivers 0140.
No test pins either, so the rule under § Skill and review are what keep them.

## Skill

[`specht-conventions` § Coding](../../../../.claude/skills/specht-conventions/references/coding.md)
§ "Generated and never hand-edited" gains the trap: a workflow with one image
is generated with no matrix, so a decision reads the runner (`runner.os`,
`RUNNER_OS`) and never `matrix.*`, and a change to a workflow's images starts
with a search of the generator for `matrix.`. Its `Never add` gains the
`matrix.*` condition.
