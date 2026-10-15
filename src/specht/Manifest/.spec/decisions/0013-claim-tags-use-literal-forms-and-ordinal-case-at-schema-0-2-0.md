---
title: Claim tags use literal forms and ordinal case at schema 0.2.0
description: The owner ratifies the tag keys, case scope, diagnostic spelling and version boundary
type: decision
---

# Decision 0013

Accepted by the repository owner on 2026-10-13 in the 0134 delivery chat.

- `claimTagForm` is literal text around exactly one `{claim}`; default `@{claim}`.
- `claimTagIgnoreCase` is a Boolean, default `false`. When true it applies to the entire tag, including literal text. Matching and resolution are independent of culture; claim and matrix ids remain exact.
- SPEC021 quotes the original tag text and carries the captured claim id alone as its identifier.
- This contract first ships in a new working schema `0.2.0`. Pins `0.1.0` and `0.1.1` retain their scanner, messages and unknown-key rejection. No locked resource changes.

Rejected: folding only the claim substring, which makes literal text follow a different case policy; reconstructing the tag in diagnostics, which discards its original spelling; adding keys or scanner semantics to an older pin, which changes that pin's verdict. The cost is a retained compatibility path and a new versioned shipping set. The separate `0.2.0` frontmatter key/form work remains outside this change.

Resolves 0001-F5 OQ-1's remaining tag names, OQ-8 and OQ-9. Amends B-007 and B-036 to B-038's version boundary and B-036/B-037's diagnostic and case contract; preserves C-2, C-4, C-5, C-12 and 0001-F7 C-4/C-11.

## Review refinement on 2026-10-13

The owner requested a `claim` object in [review 5512818512](https://github.com/RLittlesII/specht/pull/177#pullrequestreview-5512818512). The two accepted settings now live at `claim.claimTagForm` and `claim.claimTagIgnoreCase`; their names, defaults, case scope and diagnostic behavior are unchanged. Omission fills each member from the exact pin's default, including when the consumer declares only one member.

This supersedes the original top-level placement, which is not retained as an alias in the unreleased `0.2.0` working set. Older pins reject `claim` as well as the two original flat keys. The claim grammar remains solely at `identifiers.claim`; it is not copied into the new object.

## Member names refined on 2026-10-13

In [review 5512940197](https://github.com/RLittlesII/specht/pull/177#pullrequestreview-5512940197), the owner named the members `tag` and `tagIgnoreCase`. The final paths are `claim.tag` and `claim.tagIgnoreCase`. This supersedes the repeated claim prefix in the preceding placement refinement; defaults, per-member fill and tag behavior are unchanged.

## Schema 0.1.1 withdrawn on 2026-10-14

Schema `0.1.1` stops shipping before it is published
([`0001-F7` decision 0013](../../../../tool/.spec/decisions/0013-schema-0-1-1-stops-shipping.md)).
The older pin that keeps the original scanner, messages and unknown-key
rejection is `0.1.0` alone. Nothing else here changes.
