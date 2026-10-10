---
title: "Decision 0003: the two items numbered 0118 are a recorded exception"
description: "Both work items numbered 0118 stay and neither is renumbered; the check carries that one pair as a recorded exception, proposed as a manifest record of the exact paths allowed to share an id; renumbering the item that merged second was turned down"
type: decision
---

# Decision 0003: the two items numbered 0118 are a recorded exception

**Date:** 2026-10-09
**Decided by:** the repository owner (2026-10-09, recorded on work item `0128`); recorded by spec-author

## The call

This repository holds two work items numbered `0118`:

- `src/specht/Manifest/.issue/0118-configurable-claim-tags.yml` (`9f01e20`)
- `.build/ContinuousIntegration/.issue/0118-windows-gates-on-main-only.yml`
  (`5fa5964`)

Both stay. Neither is renumbered. The check carries that one pair as a
recorded exception.

The mechanism is the author's proposal, not put to the owner (OQ-8):

- The manifest holds a shared-id record. An entry is one work-item id and
  the exact set of root-relative paths allowed to share it.
- A file that claims the id at a listed path carries no finding (B-011).
- A claimant the entry does not list is still reported (B-012).
- An entry listing a path at which nothing claims the id is itself a
  finding, so a record cannot outlive the pair it was written for (B-013,
  B-014).
- An entry of the wrong shape is an invalid manifest, exit `3` (B-015,
  B-016).
- The record is consumer configuration: no shipping manifest carries it,
  and `init` and `upgrade` never write it (C-12, B-029).

## Why

Stated by the owner:

- Both items numbered `0118` stay and neither is renumbered.
- The check carries that one pair as a recorded exception.

From item `0128`'s acceptance criteria, which the owner's decision was
recorded beside:

- A third item numbered `0118` is still reported.

The author's inference, not stated by the owner:

- "That one pair" and "a third is still reported" together require the
  record to name the two files, not the id alone; exact paths are the
  smallest thing that does.
- The pair is a fact about this repository, so it is recorded in this
  repository's configuration and not in the tool every repository installs.

## Rejected

**Renumbering the item that merged second, as an exception to AGENTS.md
§ "Stable IDs".** Rejected by the owner. The cost is the author's
assessment, not the owner's: a bare `0118` keeps naming two items, and a
citation that could mean either has to name the path.

Turned down by the author in proposing the mechanism, not put to the owner:

**Disabling the claimed-twice rule in this repository.** Every other
duplicate would pass with the pair.

**An entry that exempts the id, whoever claims it.** A third item numbered
`0118` would pass, against item `0128`'s acceptance criteria.

**A marker inside the two item files.** The tool would have to read a file
whose schema it does not ship (C-1), and a new item copied from one of the
two would carry the marker with it.

**Naming the pair in the tool.** One repository's history would ship to
every consumer.

Cost of rejecting all four: one more manifest record, with a shape to
validate and a way to go stale.

## Affects

- `0101-F8` B-011 to B-016, B-029; C-6, C-12; § 5 row 19; OQ-8.
- This repository's manifest, which records the pair in the change that
  delivers the rules (A-7); not changed by this record.
- Item `0128`: its second decision, and its acceptance criterion for the
  pair.

## Reversal

**Amended 2026-10-09 by the repository owner ([decision 0007](0007-the-recorded-exception-is-a-manifest-key-from-id-to-paths.md)).**
The mechanism above, proposed by the author, is now the owner's choice: a
manifest key from an id to the exact paths allowed to share it. "A marker
inside the two item files" was put to the owner and rejected by the owner.
The key's name, an entry's exact shape and where a stale-entry finding sits
stay the author's proposal (`0101-F8` OQ-5, OQ-9). The sections above stand
as written, and nothing is reversed.
