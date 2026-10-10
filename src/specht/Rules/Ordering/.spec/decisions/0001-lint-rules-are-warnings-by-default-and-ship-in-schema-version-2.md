---
title: "Decision 0001: lint rules are warnings by default and ship in schema version 2"
description: "Every rule of epic 0101 defaults to warning severity, fails a run only under --strict, can be disabled in the manifest, and exists only from schema version 2; error by default, a separate lint switch and adding the rules to version 1 were turned down"
type: decision
---

# Decision 0001: lint rules are warnings by default and ship in schema version 2

**Date:** 2026-10-08
**Decided by:** the repository owner, in the epic `0101` requirements session (2026-10-08); recorded by spec-author

## The call

Every rule epic `0101` adds - row order (`0101-F1`), retired ids
(`0101-F2`), heading and table form (`0101-F3`) and frontmatter key order
(`0101-F4`) - has warning severity unless the manifest says otherwise. A
warning is printed and counted, and fails the run only under `--strict`.
Each rule can be disabled in the manifest. Both switches are `0001-F5`'s
per-rule severity and disable; these rules add no switch of their own.

The rules are part of schema version 2, after `0001-F7`. A repository pinned
to version 1 is never checked by them.

## Why

- A lint finding leaves the document complete and the verdict sound; it is
  a cost to the reader, not a broken contract. An error would fail every
  existing tree on the day it upgrades, for findings the tree's owner may not
  care about yet.
- `--strict` already exists for a repository that wants warnings to fail, and
  `0001-F5`'s settings already let a repository promote or switch off a rule.
  One mechanism, not two.
- The rule vocabulary is fixed per schema version (brief § 9; `0001-F1` C-5).
  A new rule in version 1 would change the verdict for a repository that
  chose to lag, which is the drift versioning exists to prevent.

## Rejected

**Error by default.** Every consumer's tree fails on upgrade until it is
reordered by hand or by `format`. Cost of rejecting: a repository that wants
ordering enforced sets the severity or runs `--strict`. Taken.

**A separate lint switch** - a `--lint` option or a `lint` command that runs
only these rules. A second way to select rules beside the manifest's
settings, and a fourth call site that can disagree with the other three
(Must-1). Cost of rejecting: none stated.

**Adding the rules to version 1.** Breaks the fixed vocabulary and `0001-F7`
decision 0002 (version 1 freezes at first publish). Cost of rejecting: the
rules wait for `0001-F7`. Taken.

## Affects

- `0101-F1` B-010 to B-012, C-5, C-6.
- `0101-F2`, `0101-F3`, `0101-F4`: their severity, disable and version 1
  claims, and their constraints of the same kind.
- `0001-F5` B-010, B-011: relied on, unchanged.

## Reversal

**Narrowed 2026-10-09 by the repository owner (`0101-F6` decision 0001).**
"Every rule epic `0101` adds" now reads as the rules of the four Features
this record names, `0101-F1` to `0101-F4`. The uniqueness rules the owner
added to the epic on 2026-10-09 are errors by default, and "Error by
default" under Rejected does not bind them. Considered and not chosen:
warning by default for the uniqueness rules too (`0101-F6` decision 0001,
Rejected).
