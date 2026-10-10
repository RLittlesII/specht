---
title: "Decision 0002: version 1 freezes at first publish"
description: "Schema version 1 is fixed by the first published package, which waits for brief § 8 step 5, so the manifest keys 0001-F5 and 0001-F6 add are version 1's and a shipped version never changes from then on"
type: decision
---

# Decision 0002: version 1 freezes at first publish

**Date:** 2026-10-07
**Decided by:** the repository owner (2026-10-07); recorded by spec-author

## The call

"Shipped" means published as a package. Schema version 1 is whatever the first
published package embeds. No package is published until brief § 8 step 5
(`0001-F5`, `0001-F6`) has landed, so the manifest keys those Features add are
part of version 1. From the first publish on, C-1 and B-013 hold without
exception.

## Why

- brief § 8 step 3 packs the tool and installs it into this repository's own
  tool manifest; step 5 then adds manifest keys. If step 3's package counted
  as shipping, step 5 would change a shipped version, contradicting C-1, or
  make every manifest role a version 2 change before any consumer exists.
- Steps 3 and 4 consume the tool locally and in `hooked`'s branch; neither
  needs a published package to pin.

## Rejected

**Version 1 is what step 3 packs; the roles are version 2.** Every consumer's
first upgrade would be a roles migration nobody asked for. Cost of rejecting:
publishing waits for step 5. Taken.

**Allow additive changes to a shipped version.** Breaks B-013's test and makes
"pinned to 1" mean different things in different tool releases. Cost of
rejecting: none beyond the wait above.

## Affects

- `0001-F7` § 2 A-2; § 3 B-013, B-015; § 4 C-1.
- `0001-F5`, `0001-F6`: their manifest keys are version 1.

## Reversal

**Amended 2026-10-09 by the repository owner (decision 0005).** The integer
version `1` is now `0.1.0`: "version 1" above reads as `0.1.0`, the version the
first published package embeds, and the manifest keys `0001-F5` and `0001-F6`
add are `0.1.0`'s. "A change is the next version" now means a new version
numbered by `0001-F7` C-11. Whether a published `x.y.z` is frozen forever,
`0.x` included, is reopened as `0001-F7` OQ-10, behind OQ-9; until it is
answered, C-1, B-013 and B-015 hold as written. Considered and not chosen:
deciding the freeze for `0.x` now (the owner deferred it behind where the
schemas are hosted).
