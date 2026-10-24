---
title: "Decision 0002: a root or manifest fault is reported before an unknown rule or level"
description: "When the set command meets a root or manifest fault and an unknown rule id or level together, the root or manifest fault is reported, so exit 2 or 3 wins over 4, as for the pin command; the Feature moves from blocked to ready-for-architecture"
type: decision
---

# Decision 0002: a root or manifest fault is reported before an unknown rule or level

**Date:** 2026-10-24
**Decided by:** the repository owner for the pin command (`0001-F7` OQ-23 resolved 2026-10-14, decision 0012, B-111); applied here by `spec-author` because OQ-2 asked that one answer serve both.

## The call

1. **Order.** The root and the manifest are validated before the rule id and
   the level. An unknown rule id or level (`4`) is reported only under a root
   and a manifest that B-009 to B-011 accept; otherwise `2` or `3` is.
   B-007 and B-008 say so; two `@boundary` scenarios state the order.
2. **Not carried over.** `0001-F7` OQ-20 (the pin command repairs a bad pin
   only) does not carry to OQ-3 here. B-011 refuses every rejected manifest,
   and OQ-3 stays open.
3. **Status.** The Feature was drafted `blocked` on `0001-F2`, `0001-F5` and
   `0001-F7`. `0001-F2` and `0001-F5` are built; `0001-F7`'s list and pin
   commands are built (items `0121` to `0125`) and only `upgrade` (item
   `0047`) is outstanding, which no claim here depends on. The status is
   `ready-for-architecture`; `spec_status` stays `draft`.

## Why

- A rule or level can only be judged against a manifest that exists and is
  accepted; the pin command already works in this order.
- One answer for two commands: a code that means one thing here and another
  to the pin command is what `0001-F11` C-6 rules out.

## Rejected

**The argument first, so `4` wins.** Cost of rejecting: a typo in a rule id is
not reported until the root and manifest are sound.

## Affects

- `0001-F11` B-007, B-008, OQ-2, two scenarios.
- B-009 to B-011 are unchanged.

## Reversal

None.
