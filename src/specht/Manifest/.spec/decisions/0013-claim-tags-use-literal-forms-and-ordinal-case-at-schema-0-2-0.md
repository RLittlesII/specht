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
