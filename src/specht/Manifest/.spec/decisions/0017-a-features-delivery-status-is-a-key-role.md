---
title: "Decision 0017: a Feature's delivery status is a key role"
description: "The owner's ruling of November 2 2026 on item 0016: keys.feature.status, default status, names the Feature key SPEC7001 reads under schema 0.3.0, and B-004's list gains it"
type: decision
---

# Decision 0017: a Feature's delivery status is a key role

**Date:** 2026-11-02
**Decided by:** the repository owner, answering item `0016`'s question 2 with "A add key role (Recommended)"; recorded by spec-author

## The call

- `keys.feature.status`, default `status`, names the Feature frontmatter
  key that holds the delivery status.
- Under schema `0.3.0` and later, `SPEC7001` reads a Feature's delivery
  status through that key (`0001-F7` B-118).
- B-004's list of keys gains `status`.
- Decision 0010's `keys.feature` table gains `status` beside `id`, `epic`,
  `specStatus`, `children`, `spikes`, `dependsOn` and `blocks`.

## Why

- Since item `0321`, `SPEC7001` reads `status` under `0.3.0`. B-004 asks
  for every frontmatter key a rule reads, yet listed only the nine keys
  decision 0010 ratified before that.
- A literal left in a rule is what C-1 and brief § 5 rule out. A consumer
  that renames `status` would otherwise have to fork the engine.

## Rejected

- **(B) `status` stays a literal and B-004 is narrowed to the keys it
  lists.** One literal stays, against brief § 5.
- **(C) The key role in a later version.** That spends a `0.minor` on one
  key.

## Affects

- `0001-F5` B-004 and its scenarios.
- Decision 0010's key table.
- C-8 still holds: a consumer who renames `status` renames it in its
  on-disk Feature schema too, because schema `0.3.0`'s Feature schema
  requires `status`.

## Reversal

None.
