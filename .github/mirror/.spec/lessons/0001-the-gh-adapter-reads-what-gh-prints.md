---
title: "Lesson 0001: The gh adapter reads what gh prints"
description: "The first live mirror run stopped at the first sub-issue link because the real gh adapter parsed a raw --jq string as JSON, and no test read gh's output"
type: lesson
kind: product
---

# Lesson 0001: The gh adapter reads what gh prints

**Date:** 2026-10-25
**Kind:** product

## Symptom

The first live mirror run, Actions run 40174035328 after #281 merged,
created all 279 issues and closed the done ones, then failed at its first
sub-issue link with `json.decoder.JSONDecodeError: Expecting value: line 1
column 1 (char 0)`. No issue was linked under its parent.

## Root cause

`Gh.node()` looked up an issue's node id with
`--jq .data.repository.issue.id`. For a string result `gh` prints the value
raw, not JSON-quoted, and `values()` reads its input as JSON, so the id
could not be parsed. `issues()` caches the node id of every issue it reads,
so `node()` is reached only for an issue the same run created, which no run
before the first live one did.

The tests did not catch it because C-11 named only the plan computation:
every test drove a fake `gh`, the real adapter's parsing of `gh`'s output
had no test, and § 7 and § 8 recorded the adapter as reached only by a live
run. B-014 and B-015 were Covered through the fake, which links by issue
number and needs no node id.

## Spec delta

B-054 added: an issue the run created, or whose parent's issue it created,
is linked under the parent in that run, with its `@B-054` scenario. C-11
amended: the real `gh` adapter's reading of `gh`'s output is tested against
that output as `gh` prints it, with the process replaced. § 7 records that
`node()` parses the whole JSON document; § 8 names `test_gh.py` and narrows
what no test reaches to the `gh` process and the adapter's writes.

## Claim

- B-054 - `test_B054_TaskAndParentWithNoIssue_WhenMirroredThroughGh_ShouldLinkTheTaskUnderItsParentInTheSameRun`
  and `test_B054_GhPrintingAnIssueNodeQuery_WhenLookedUp_ShouldReturnTheIssuesNodeId`
  in `.github/mirror/tests/test_gh.py`
