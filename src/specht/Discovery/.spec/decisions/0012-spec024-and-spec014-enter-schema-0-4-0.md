---
title: "Decision 0012: SPEC024 and SPEC014 enter schema 0.4.0"
description: "The orphan-companion and required-specification rules enter schema 0.4.0, the open minor, because 0.2.0 to 0.3.1 were published without them; B-013 and B-014 read 0.4.0 or later"
type: decision
---

# Decision 0012: SPEC024 and SPEC014 enter schema 0.4.0

**Date:** 2026-11-14
**Decided by:** the repository owner (2026-11-14), answering OQ-22 by selecting "0.4.0 (Recommended)", relayed in session by the coordinating session; recorded by spec-author. The option's wording was the coordinating session's and the default spec-author's; only the selection is the owner's.

## The call

`SPEC024` (B-013) and `SPEC014` (B-014) enter schema `0.4.0`. B-013 and
B-014 read "a manifest that pins schema `0.4.0` or later", was `0.2.0`.

Spec-author's readings, not stated by the owner:

1. Each id is added to `0.4.0`'s vocabulary in place, with its rule page
   (`0001-F3` B-031) and a relock by `0001-F7` decision 0014's procedure
   (`0001-F7` C-17), in the change that builds the rule, while no published
   package embeds `0.4.0`. Its four-digit id is the next free one in its
   ADR-0029 group, on the owner's approval in that change.
2. If a published package embeds `0.4.0` before a rule lands, that rule's
   item stops and the version goes to the owner, as `0001-F12` decision
   0005 call 3 has it.
3. The `@B-013`, `@B-014` and `@B-015` scenarios pin `0.4.0`.

## Why

- `0.2.0`, `0.2.1`, `0.3.0` and `0.3.1` are published without either id and
  frozen (`0001-F7` C-1); `0001-F7` B-115, which put them in `0.2.0`, was
  withdrawn on 2026-10-30.
- Neither rule needs a new key, so it is not an opt-in addition and takes a
  `0.minor` step (`0001-F7` C-11). `0.4.0` was cut by item `0436`, and on
  2026-11-14 no tag carries it.
- `0001-F12` decision 0005 put `SPEC4008` and `SPEC4010` in `0.4.0` for the
  same reason.

## Rejected

Which other options were put to the owner is not in this record. Ruled out
by the constraints, as spec-author reads them: adding either id to a
published version (`0001-F7` C-1).

## Affects

- B-013 and B-014, amended. B-015 is unchanged.
- OQ-22, resolved.
- Item `0441`, ready. B-014 and B-015 have no item.

## Reversal

None.
