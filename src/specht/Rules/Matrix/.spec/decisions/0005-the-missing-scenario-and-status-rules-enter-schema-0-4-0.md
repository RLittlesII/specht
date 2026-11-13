---
title: "Decision 0005: the missing-scenario and status rules enter schema 0.4.0"
description: "SPEC4008 and SPEC4010 enter schema 0.4.0, the next minor, because a published package froze 0.3.0 before either was built; SPEC4007 and SPEC4009 stay in 0.3.0 and 0.3.1 as built"
type: decision
---

# Decision 0005: the missing-scenario and status rules enter schema 0.4.0

**Date:** 2026-11-12
**Decided by:** the repository owner (2026-11-12), answering OQ-4; recorded by spec-author

## The call

The owner selected "0.4.0", the next minor, with item `0336` waiting on
item `0436`, which cuts `0.4.0`. That selection is the whole of the
owner's call.

Spec-author's readings, not stated by the owner:

1. `SPEC4008` (item `0336`) and `SPEC4010` (item `0337`) enter schema
   `0.4.0`. `0337` and `0338` also wait on `0436`: `0337` adds its id to
   `0.4.0`, and `0338` proves the guard at `0.3.0` and `0.3.1`, which needs
   `0.4.0` to exist.
2. Each of the two adds its id to `0.4.0`'s vocabulary with its rule page
   (`0001-F3` B-031) and relocks `0.4.0` by `0001-F7` decision 0014's
   procedure (`0001-F7` C-17), in the change that builds it, while no
   published package embeds `0.4.0`. `0436` cuts `0.4.0` with no rule id
   of its own.
3. If a published package embeds `0.4.0` before either rule lands, that
   rule's item stops and the version is put to the owner. This is OQ-4's
   default carried to `0.4.0`, with "published" read as lesson 0016 reads
   it: a package on a feed consumers install from.
4. `SPEC4007` and `SPEC4009` stay in `0.3.0` and `0.3.1` as built, and are
   in `0.4.0`, which is cut from `0.3.1`.
5. B-021 holds each rule silent below the version it enters: below `0.3.0`
   none of the four runs, and at `0.3.0` and `0.3.1` neither `SPEC4008` nor
   `SPEC4010` runs.
6. Decision 0002 call 3 stands: `0436` moves this repository's pin to
   `0.4.0` in its own change (`0001-F4` B-004), so each rule runs on this
   tree in the change that builds it, as item `0339`'s `0.3.0` pin did for
   `SPEC4009`.

## Why

- OQ-4's case occurred. `specht.tool` `0.4.2-preview` (tag
  `v0.4.2-preview`, 2026-11-03) is on nuget.org and embeds schema `0.3.0`,
  whose rule pages at that tag include `SPEC4007` and `SPEC4009` and not
  `SPEC4008` or `SPEC4010`. Every later package embeds `0.3.0`, and from
  `0.5.35-preview` on `0.3.1` too. Both versions are published (lesson 0016) and frozen (`0001-F7` C-1); `0001-F7` § 10 already says published
  `0.3.0` stays frozen.
- Decision 0003 call 3 allowed a relock of `0.3.0` only while it was
  unpublished, so the two rules not yet built can no longer enter it.

## Rejected

Which other options were put to the owner is not in this record. Ruled out
by the constraints, as spec-author reads them:

- **Adding the two ids to `0.3.0` or `0.3.1`.** Both are published
  (`0001-F7` C-1).
- **A `0.3.2` patch.** Both rules report at their default level with no
  opt-in key, so the addition can fail a tree `0.3.1` accepts, which a
  patch may not (`0001-F7` C-11, decision 0022).

## Affects

- A-4, B-021, § 1, the frontmatter `description`, § 5 rows 8 and 11, OQ-4;
  the scenarios of B-011 to B-017, which now pin `0.4.0`; B-021's
  scenarios. No other claim's text changes.
- Decision 0003 calls 1 and 2, reversed for `SPEC4008` and `SPEC4010` (its
  Reversal).
- `0001-F7` C-1, C-11, C-17, decision 0014, decision 0022; `0001-F5` A-8
  and decision 0024, which cut `0.4.0`; `0001-F4` B-004; `0001-F3` B-031.
  No claim of another Feature changes.
- Items `0224`, `0336`, `0337` and `0338`.

## Reversal

None.
