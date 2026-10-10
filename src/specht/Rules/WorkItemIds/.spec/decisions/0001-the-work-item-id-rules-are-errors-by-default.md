---
title: "Decision 0001: the work-item id rules are errors by default"
description: "The rules of 0101-F8 - a work-item id claimed twice, a sequence file that disagrees with the ids present - default to error severity, as SPEC030, SPEC012 and SPEC044 do and as 0101-F6 decision 0001 records for its own rules; warning by default was turned down"
type: decision
---

# Decision 0001: the work-item id rules are errors by default

**Date:** 2026-10-09
**Decided by:** the repository owner (2026-10-09, recorded on work item `0128`); recorded by spec-author

## The call

The rules this Feature adds - a work-item id claimed twice, a sequence file
that disagrees with the ids present - have error severity unless the manifest
says otherwise. An error is printed, counted and fails the run. A repository
lowers or disables a rule through `0001-F5`'s per-rule settings; these rules
add no switch of their own.

`0101-F6` decision 0001 records the same default for that Feature's rules and
says each later uniqueness Feature records it when it is written. This is
that record for `0101-F8`, and it binds only `0101-F8`.

## Why

Stated by the owner:

- The new rules are errors by default "as `SPEC030`, `SPEC012` and `SPEC044`
  are": a claim id declared twice, a Feature identity declared twice and an
  item id declared twice.

The author's inference, not stated by the owner:

- The owner's answer covered all five cases of item `0128` at once and did
  not name the sequence file apart. A sequence file behind the ids present
  hands the next claimant an id that is taken, so it is read here as the same
  fault one step earlier.
- A stale shared-id entry (B-013) takes the severity of the rule it is
  reported under; which rule that is, is OQ-1.

## Rejected

**Warning by default, as the epic first worded it.** Rejected by the owner.
The cost is the author's assessment, not the owner's: a repository that
declares its work items and already holds a doubled id fails on the day it
pins the version that adds the rules, until it records the pair, renumbers
an item, or lowers or disables the rule. This repository is one: its two
items numbered `0118` fail its own check unless its manifest records them in
the change that delivers the rules (A-7). Taken.

## Affects

- `0101-F8` B-030, B-031; C-11.
- Epic `0101`: unchanged in wording; its summary already says the uniqueness
  Features error.
- `0101-F6` decision 0001: relied on, unchanged.
- `0001-F5` B-010, B-011: relied on, unchanged.

## Reversal

None.
