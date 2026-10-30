---
title: "Decision 0014: A locked, unpublished version may be relocked"
description: "A digest in the version lock is never rewritten once its version is published; a locked version no package has carried may be relocked, and schema 0.2.0 is relocked when the matrix-resolution, discovery and section-owner rules land in it"
type: decision
---

# Decision 0014: A locked, unpublished version may be relocked

**Date:** 2026-10-14
**Decided by:** the repository owner (2026-10-14); recorded by spec-author

## The call

1. A digest the version lock records is never rewritten once its version is
   published. A locked version that no published package has carried may be
   relocked.
2. Schema `0.2.0` has never shipped in a published package: tag `v0.1.90`
   carries `0.1.0` alone. It is relocked when the rules of `0001-F12`, the
   two discovery rules of `0001-F6` and `0101-F9`'s `SPEC015` land in it.
3. `0.2.0`'s vocabulary becomes the 21 rule ids of `0.1.0` and `SPEC014`,
   `SPEC015`, `SPEC022`, `SPEC023`, `SPEC024`, `SPEC032`, `SPEC033`,
   `SPEC034` and `SPEC035` (B-115). `0.1.0` keeps its 21 ids and its digest.
4. `0.2.0`'s feature template says, in its § 9 guidance, that "`SPEC060`
   reports it once spec_status is approved, unless the manifest lowers the
   rule", where `0.1.0`'s says "it does not block spec_status reaching
   approved" (B-117). The wording was part of the proposal the owner approved
   on 2026-10-14, and the owner confirmed it as an owner call on 2026-10-15.

C-17 is amended in place to say so, and OQ-54 is resolved.

How a relock is made is spec-author's reading, not stated by the owner: the
unpublished version's lock entry is removed by hand in a reviewed change, as
decision 0013 deleted `0.1.1`, and the lock target then records the version
again as the newest one absent from the lock (B-092). B-093 is unchanged: the
lock target never rewrites an entry it finds.

## Why

- No consumer can pin a version no package carried, so changing its bytes
  moves no consumer's verdict; the freeze protects published versions
  (C-1; B-013, B-015).
- The alternative, a `0.3.0` for the new rules, ships a `0.2.0` no package
  carried and no consumer needs, and spends a number for nothing.

## Rejected

Recorded by spec-author as what the call rules out:

- OQ-54's proposed default, that a version on `main` is treated as
  published: the owner chose the relock. Cost: a reviewer must check the tag
  history, not the lock, to know whether a version may still be relocked.
- A `0.3.0` for the new rules: as under Why.
- A lock target that rewrites an entry: B-093 stands, so a relock is a
  reviewed hand edit, never a side effect of a build.

## Affects

- C-17 amended in place; OQ-54 resolved; OQ-18 answered in part for
  `0101-F9`'s `SPEC015`.
- Added: B-115 to B-117; B-117 from call 4.
- `0001-F12` A-4, B-021; `0001-F6` B-013, B-014; `0101-F9` B-024 to B-026;
  `0001-F5` B-055 to B-061.
- The shipping `0.2.0` manifest, templates and rule pages and the version
  lock change in the delivering pull request.

## Reversal

2026-10-30, calls 2 to 4, on the repository owner's approval of item
`0041`'s close-out; recorded by spec-author. A release went before the
nine rules were built. Two `0.2.0`s were carried by packages, and OQ-60
asks which is published: tag `v0.1.131`'s adds no rule id to `0.1.0`'s
21, and from `v0.2.4` on it adds `SPEC034` and `SPEC035`. Neither holds
the other seven ids, and neither feature template changed `0.1.0`'s § 9
guidance. Call 1 and C-1 forbid relocking whichever is published, so call
3's vocabulary and call 4's wording never held, under either answer to
OQ-60. B-115 is withdrawn by spec-author and B-117 by the owner, their
numbers retired; OQ-58 is resolved; C-17 is amended. Call 1 stands.

Each of the seven ids enters with its own rule, at least a
`0.minor` step above the newest published version (C-11); which version
is not decided. B-124 gives `0.3.0`'s feature template new § 9 guidance,
after decision 0015.

Considered and not chosen, by spec-author:

- B-115 retargeted to `0.3.0`: no owner call names it, and it would hold
  B-118 to B-124 back for rules not yet built or specified (`SPEC015`'s
  specification is a draft; `SPEC014` and `SPEC024` have no item).
- B-115 retargeted to "the version that ships the seven ids": it names no
  version, so it says no more than B-014.
