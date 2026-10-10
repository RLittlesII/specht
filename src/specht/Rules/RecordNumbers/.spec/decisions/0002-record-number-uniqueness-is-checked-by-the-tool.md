---
title: "Decision 0002: record number uniqueness is checked by the tool"
description: "Two files in one decisions, adr or lessons folder sharing a number are reported by specht as a rule of a new major schema version, over the one tree it is given; a build-target check in one repository and a base or diff input were turned down"
type: decision
---

# Decision 0002: record number uniqueness is checked by the tool

**Date:** 2026-10-09
**Decided by:** the repository owner (2026-10-09, recorded on work item `0128`); recorded by spec-author

## The call

`specht` reports two files in one `decisions/`, `adr/` or `lessons/` folder
that share a number (item `0128`, case (a)), both paths named, as a
`SPEC###` rule. A new rule is a new major schema version (`0001-F7` C-11).

The check stays a deterministic, offline read of one tree. A record number
a pull request adds that its base already holds is found by running the
check a second time over the merge tree, where it is two claimants like any
other; that run is `0055-F2`'s (`0055-F2` B-017, C-7).

The author's reading, not stated by the owner:

- **Per folder.** Item `0128` words case (a) as two files "in one" folder,
  and `specht-conventions` § Specs numbers each record folder on its own.
  The same number in two folders is not reported (B-007). A Feature's
  `adr/` against the repository-wide `.spec/adr/` is OQ-2.
- **Which folders.** The three beside a specification and the two
  repository-wide ones, `.spec/adr` and `.spec/lessons`. Item `0128` names
  the three folder names and not where they sit; lesson 0005 and
  `specht-conventions` § Delivery name the repository-wide pair as the
  numbers every branch shares. Reading them under the excluded root `.spec`
  is OQ-5.
- **What a record file is.** A file directly in the folder, named by the
  manifest's record file shape: by default four digits, `-`, any text and
  `.md`, as the shipped templates name a record. Any other file claims no
  number and is not reported (B-008, B-009). OQ-5.
- **The number is the file name's.** No record content is read. The owner
  answered the file name only for work-item ids (`0101-F8` decision 0005)
  and was not asked here; it is OQ-4.
- **How numbers compare.** By text. The owner answered by text for
  `0101-F6` (its decision 0003) and was not asked here; it is OQ-6.
- **Who is checked.** Every repository that pins the version, with the
  record values filled from the default manifest. The owner answered
  opt-in for work items (`0101-F8` decision 0004) and was not asked for
  record folders; it is OQ-7.
- **Scope of the version.** The rule ships in the same major schema version
  as the rest of epic `0101`. The number is `0001-F7` OQ-18, and whether
  this Feature shares it is OQ-3.

## Why

- Lesson 0005: `0001-F5` decision 0003 was written by three pull requests at
  once. The three files had different names, so git would have merged all
  three as 0003 with no conflict; they were found by reading.
- No rule of schema version `0.1.0` reads a record folder, and a record
  folder has no sequence file (`specht-conventions` § Delivery), so the
  stable-id rule (AGENTS.md § "Stable IDs") is enforced for record numbers
  by review alone.

## Rejected

**A build-target check in this repository, run in CI beside the
self-check.** It was the first proposal of item `0128`. Rejected by the
owner. Cost of rejecting: the rule waits for a new major schema version and
for `0001-F5`'s rule settings, and until then review enforces it. Taken.

**A base, diff or pull-request input to `specht`.** Rejected by the owner.
The check would compare two revisions and stop being a read of one tree.
Cost of rejecting: CI runs the check twice on a pull request. Taken.

Turned down by the author in reading case (a), not put to the owner:

**Reporting a skipped number.** A withdrawn branch leaves one. Cost of
rejecting: a number skipped by mistake is never reported.

**One record number space across folders.** Each folder is numbered from
`0001`, so every repository would fail. Cost of rejecting: a bare
`ADR-0002` can name a Feature's record and the repository's (OQ-2).

**Every folder named `decisions`, `adr` or `lessons` anywhere in the tree.**
It would read folders that are not part of a specification, such as a
documentation site's `adr/`. Cost of rejecting: a record folder kept
somewhere else is checked only once the manifest names it (B-021).

## Affects

- `0101-F7` B-001 to B-022, B-025; C-1 to C-11, C-13 to C-15; § 5 rows 1
  to 10.
- `0055-F2`: the merge-tree run is that Feature's; relied on, unchanged.
- `0101-F6` decision 0002 and `0101-F8` decision 0002: the same call for
  ids inside a specification and for work-item ids; unchanged.

## Reversal

**Amended 2026-10-09 by the repository owner (decisions [0003](0003-a-features-adr-numbers-are-apart-from-the-repository-wide-ones.md), [0004](0004-a-named-root-record-folder-is-read-under-an-exclusion.md) and [0005](0005-the-record-number-rule-is-on-by-default.md)).**
Three of the author's readings above are now the owner's answers, and the
sections above stand as written.

- **Per folder.** A Feature's `adr/` and the repository-wide `.spec/adr` are
  apart: `0101-F7` OQ-2 is resolved. "One ADR space" was put to the owner
  and rejected by the owner; one record number space across every folder
  stays turned down by the author.
- **Which folders.** A named root record folder is read under an exclusion:
  `0101-F7` OQ-5 is resolved as to its part (b). How record folders are
  found is OQ-8.
- **Who is checked.** On by default: `0101-F7` OQ-7 is resolved.

"What a record file is" cites OQ-5; that part was not asked and is now OQ-8.
"The number is the file name's", "How numbers compare" and "Scope of the
version" stay the author's readings (OQ-4, OQ-6, OQ-3).

Nothing is reversed.

**Noted 2026-10-09, after review round 1.** The sections above stand as
written and nothing is reversed; this says whose words they are, as
decisions 0001 and 0003 to 0005 do in place.

- **Why.** Both bullets are the author's reasoning, taken from lesson 0005
  and from item `0128`'s summary. The owner's decisions on item `0128` give
  the call and no reason beside it.
- **Rejected.** The two rejections above "Turned down by the author" are the
  owner's (item `0128`, decisions 1 and 4). Each "Cost of rejecting"
  sentence, and "Taken" after it, is the author's assessment: item `0128`
  records the rejections and no cost. "The check would compare two revisions
  and stop being a read of one tree" restates the owner's "stays a
  deterministic, offline check over one tree" (item `0128`, decision 4).
- **Affects.** `0101-F7` B-026 to B-028 were added in that round and rest on
  the "What a record file is" reading above: the default number grammar,
  split from B-020, and a declared grammar and file shape being read. They
  are the author's, not asked (OQ-8).
