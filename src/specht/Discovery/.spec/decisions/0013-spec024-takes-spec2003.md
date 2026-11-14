---
title: "Decision 0013: SPEC024 takes SPEC2003"
description: "The orphan-companion rule, working name SPEC024, takes the four-digit id SPEC2003 in the Structure group when it enters schema 0.4.0; B-013 names SPEC2003"
type: decision
---

# Decision 0013: SPEC024 takes SPEC2003

**Date:** 2026-11-14
**Decided by:** the repository owner (2026-11-14), asked which four-digit id `SPEC024` (B-013) takes in schema `0.4.0` under ADR-0029, by selecting "(c) SPEC2003, Structure group", relayed in session by the coordinating session; recorded by spec-author. The options' wording was the coordinating session's and the delivering session's; only the selection is the owner's.

## The call

The orphan-companion rule, working name `SPEC024`, takes `SPEC2003` when it
enters schema `0.4.0` (decision 0012). B-013 names `SPEC2003`, was `SPEC024`.

Spec-author's readings, not stated by the owner:

1. B-013 holds only under a pin at `0.4.0` or later, where every id has four
   digits, so it names the four-digit id (ADR-0029 § "How a specification
   names a rule"). It keeps its id.
2. The working name `SPEC024` stays the rule's key, mapped to `SPEC2003`, as
   `SPEC023` is mapped to `SPEC4008` (`0001-F12`).
3. `0101-F10` OQ-6, open, proposes `SPEC2003` for the comment size rule. That
   proposal now falls on the next free Structure id.

## Why

- ADR-0029 gives a working-name rule a four-digit id in its group when it
  enters a version, on the owner's approval; item `0441`'s criterion asked for
  it in its pull request.
- `SPEC2003` is free: on 2026-11-14 schema `0.4.0` holds `SPEC2001` and
  `SPEC2002` in the Structure group, and no open pull request adds an id
  there.

## Rejected

The other options put to the owner, in the sessions' wording:

- (a) `SPEC4011`, with `SPEC4012` to `SPEC4014` reserved for `0001-F16`'s
  `SPEC025` to `SPEC027`.
- (b) `SPEC4011` for whichever rule lands first.

## Affects

- B-013, amended; its scenarios name no id and are unchanged.
- Item `0441`: title, summary and the id criterion.
- Not amended here, each naming `SPEC024` and owned by its Feature:
  `0001-F16` B-022 and its scenario, `0001-F3` B-038, `0001-F7` B-115
  (a `0.2.0` claim), and ADR-0029's list of working names.

## Reversal

None.
