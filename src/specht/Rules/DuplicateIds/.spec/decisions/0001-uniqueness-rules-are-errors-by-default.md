---
title: "Decision 0001: uniqueness rules are errors by default"
description: "The duplicate-id rules of 0101-F6 default to error severity, as SPEC030, SPEC012 and SPEC044 do, departing from epic 0101's warnings by default for its style and ordering rules; warning by default was turned down"
type: decision
---

# Decision 0001: uniqueness rules are errors by default

**Date:** 2026-10-09
**Decided by:** the repository owner (2026-10-09, recorded on work item `0128`); recorded by spec-author

## The call

The rules this Feature adds - a constraint id declared twice, an
open-question id declared twice - have error severity unless the manifest
says otherwise. An error is printed, counted and fails the run. A repository
lowers or disables a rule through `0001-F5`'s per-rule settings; these rules
add no switch of their own.

Epic `0101` now holds two kinds of rule: its style and ordering Features
(`0101-F1` to `0101-F4`) warn by default, as `0101-F1` decision 0001 says of
the four it names, and its uniqueness Features error by default. The epic's
wording is amended in the pull request that records this decision.

The same default is decided for the two uniqueness Features not yet
specified (record numbers, work-item ids). Each records it when it is
written; this record binds only `0101-F6`.

## Why

- The existing uniqueness rules are errors: `SPEC030` (a claim id declared
  twice), `SPEC012` (a Feature identity declared twice) and `SPEC044` (an
  item id declared twice). A doubled constraint or open-question id is the
  same fault in a table no rule read.
- `0101-F1` decision 0001 gave its reason for warnings: a lint finding
  "leaves the document complete and the verdict sound". A doubled id does
  not: a citation of it names two rows.

## Rejected

**Warning by default, as the epic worded it.** One default for every rule of
the epic, and no tree fails on moving to schema version 2. Cost of
rejecting: a tree that already carries a doubled id fails on the day it pins
version 2, until a row is renumbered or the repository lowers or disables
the rule. Taken.

## Affects

- `0101-F6` B-012, B-013; C-7.
- Epic `0101`: its description and summary, which said every rule warns.
- `0101-F1` decision 0001: unchanged. It names `0101-F1` to `0101-F4`, and
  its opening words, "every rule epic `0101` adds", are now read by that
  list.
- `0001-F5` B-010, B-011: relied on, unchanged.

## Reversal

None.
