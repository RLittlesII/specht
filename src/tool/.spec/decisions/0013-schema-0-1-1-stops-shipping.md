---
title: "Decision 0013: Schema 0.1.1 stops shipping"
description: "Schema 0.1.1, unlocked and never published, is deleted from every tree so that 0.2.0 is locked and becomes the first version published after 0.1.0."
type: decision
---

# Decision 0013: Schema 0.1.1 stops shipping

**Date:** 2026-10-14
**Decided by:** repository owner

## The call

Schema `0.1.1` stops shipping. Its folder is deleted from every tree a
version's files sit in (B-086): the frontmatter schemas, the shipping
manifest, the templates and the rule pages. It is deleted by hand, in a
reviewed change; no target is built for it.

The tool then ships `0.1.0` and `0.2.0`. `0.1.0` stays locked and unchanged.
`0.2.0` is locked by the lock target (B-092) and becomes the first schema
version published after `0.1.0`. No byte of `0.1.0` or `0.2.0` changes.

No scenario names `0.1.1` as a version the tool ships, the scenarios whose
version set is built for the test included. Their claims are unchanged.

## Why

The next tool pre-release cannot be tagged while a shipped version is absent
from the version lock (`0055-F6` B-013). Two versions are unlocked today,
`0.1.1` and `0.2.0`, which B-090 already forbids, and the lock target locks
only the newest (B-092), so `0.1.1` could never be locked.

`0.1.1` was never published: the only release, tag `v0.1.90` at commit
`b213a05`, embeds `0.1.0` alone. Before `1.0.0` a later tool may stop
shipping a `0.x.y` (C-1; decision 0007), and an unpublished one has no
consumer pinned to it.

`0.1.1` existed to give init an explicit pin to write (decision 0010).
`0.2.0`'s shipping manifest carries `schemaVersion: "0.2.0"`, so B-012 holds
with `0.2.0` as the newest version.

Once `0.2.0` ships, a cut to `0.1.1` is refused (B-095), so the number is not
taken again.

## Rejected

The owner chose deletion. What it rules out, as spec-author records it:

- Locking `0.1.1` beside `0.2.0`: the lock target locks only the newest
  version (B-092), and a hand-written lock entry is ruled out (C-17).
- Publishing `0.1.1` first, then cutting and publishing `0.2.0`: a release
  for a version nothing needs, which a later tool may stop shipping anyway.
- Folding `0.1.1` into `0.2.0` by renaming: `0.2.0` is already its own set,
  and renaming an unlocked version stays open (OQ-53).

## Affects

- OQ-53 resolved in part: an unlocked, unpublished version may be deleted by
  hand in a reviewed change. Renaming one stays open.
- B-090 and B-091 hold for the shipped set again once `0.1.1` is deleted.
- The scenarios of B-053, B-055, B-090, B-092 to B-094, B-097 to B-100 and
  B-102 now use versions other than `0.1.1`; their claims are unchanged.
- Decision 0010 gains a reversal; B-012 is unchanged.
- `0001-F5` B-049 amended to `0.1.0` alone; `0001-F5` decisions 0010 and 0013
  gain a dated note.
- `0055-F6` B-013: the guard passes once `0.2.0` is locked.
- § 7 and § 9 of this Feature and of `0001-F5`, which name `0.1.1`, and the
  reference pages under `docs/`, follow in the delivering change.
