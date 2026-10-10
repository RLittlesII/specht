---
title: "Decision 0001: the record number rule is an error by default"
description: "The rule of 0101-F7 - a number two files in one decisions, adr or lessons folder open with - defaults to error severity, as SPEC030, SPEC012 and SPEC044 do and as 0101-F6 decision 0001 and 0101-F8 decision 0001 record for their own rules; warning by default was turned down"
type: decision
---

# Decision 0001: the record number rule is an error by default

**Date:** 2026-10-09
**Decided by:** the repository owner (2026-10-09, recorded on work item `0128`); recorded by spec-author

## The call

The rule this Feature adds - a number that two or more files in one record
folder open with - has error severity unless the manifest says otherwise. An
error is printed, counted and fails the run. A repository lowers or disables
the rule through `0001-F5`'s per-rule settings; this rule adds no switch of
its own.

`0101-F6` decision 0001 records the same default for that Feature's rules and
says each later uniqueness Feature records it when it is written; `0101-F8`
decision 0001 is that record for work-item ids. This is that record for
`0101-F7`, and it binds only `0101-F7`. Nothing is decided again here.

## Why

Stated by the owner:

- The new rules are errors by default "as `SPEC030`, `SPEC012` and `SPEC044`
  are": a claim id declared twice, a Feature identity declared twice and an
  item id declared twice.

The author's inference, not stated by the owner:

- The owner's answer covered all five cases of item `0128` at once; record
  numbers are its case (a). A doubled record number is read here as the same
  fault as those three: a citation of it names two files.

## Rejected

**Warning by default, as the epic first worded it.** Rejected by the owner.
The cost is the author's assessment, not the owner's: a repository that
already holds a doubled record number fails on the day it pins the version
that adds the rule, until a record is renumbered or the repository lowers or
disables the rule. Under the proposal that the rule is on for every
repository (OQ-7), that reaches a repository that never declared anything
about records, which `0101-F8` decision 0004 spares for work items. Taken.

## Affects

- `0101-F7` B-023, B-024; C-12.
- Epic `0101`: unchanged in wording; its summary already says the uniqueness
  Features error.
- `0101-F6` decision 0001 and `0101-F8` decision 0001: relied on, unchanged.
- `0001-F5` B-010, B-011: relied on, unchanged.

## Reversal

None.
