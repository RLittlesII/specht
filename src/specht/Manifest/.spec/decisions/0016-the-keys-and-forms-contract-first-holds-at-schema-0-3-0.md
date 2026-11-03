---
title: "Decision 0016: the keys and forms contract first holds at schema 0.3.0, in place"
description: "The owner's ruling of November 2 2026 on item 0016: keys, forms, named task groups, grammars checked at parse, a malformed edge's original text and the captured gap width enter unpublished schema 0.3.0 in place, with a relock; 0.1.0, 0.2.0 and 0.2.1 keep their verdicts and bytes"
type: decision
---

# Decision 0016: the keys and forms contract first holds at schema 0.3.0, in place

**Date:** 2026-11-02
**Decided by:** the repository owner, answering item `0016`'s question 1 with "A 0.3.0 in place (Recommended)"; recorded by spec-author

## The call

1. The contract of item `0016` first holds at schema `0.3.0`: `keys`,
   `forms`, the task grammar's named groups, every grammar compiled at
   parse, the original text of a malformed edge, and the captured gap
   width (A-6; B-004 to B-006, B-014, B-019's omitted form, B-049 to
   B-054, B-068).
2. It is written into `0.3.0` in place, and `0.3.0` is relocked
   (`0001-F7` C-17, `0001-F7` decision 0014), as item `0169` wrote its
   rule ids ([ADR-0029](../../../../../.spec/adr/0029-rule-ids-take-four-digits-in-schema-0-3-0-and-published-versions-keep-theirs.md)
   ruling 2).
3. `0.1.0`, `0.2.0` and `0.2.1` keep today's interpretation, verdicts,
   bytes and lock digests (B-049). Under them `keys` and `forms` stay
   unknown keys (B-012).
4. `0.3.0`'s default task grammar becomes
   `^(?<epic>[0-9]{4})-(?<sequence>[0-9]{2})$`, decision 0010's default,
   so the default manifest passes B-050 and B-051 (B-068). This
   repository's live copy follows in the same change (A-1).
5. B-053 and B-054 take the same boundary.
   [ADR-0030](../../../../../.spec/adr/0030-schema-0-3-0-first-carries-the-gap-width-and-the-malformed-edge-text.md)
   narrows ADR-0029 ruling 6 for them.
6. No `v*` tag is pushed before item `0016` merges: a tag runs the
   release, and a package carrying `0.3.0` freezes it (`0001-F7` C-1).

## Why

- Decision 0010 put the contract at `0.2.0` on 2026-10-13, while `0.2.0`
  was unpublished. `0.2.0` and `0.2.1` are now in a published package and
  `0.3.0` is in none (ADR-0029 ruling 2; lesson 0016).
- A published version never changes in place (`0001-F7` C-1). An older pin
  never takes a newer interpretation (`0001-F7` C-4). A patch changes no
  verdict (`0001-F7` C-11). The verdict includes the identifier and the
  message (`0001-F1` C-9).
- None of the contract fits `0.2.0`, `0.2.1` or a patch of either. B-050
  to B-052 reject the group-less task grammar every `0.2.x` manifest
  declares. B-054 changes a reported identifier. `keys` and `forms` turn a
  rejection into a pass.
- `0.3.0` can take the contract in place. A new version would leave one
  that no consumer can pin (lesson 0016).

## Rejected

- **(B) `0.3.0` for the keys, forms and rejections, with B-053 and B-054
  from `0.2.0` as ADR-0029 ruling 6 reads.** A tree with a malformed edge
  would change its `0.2.0` and `0.2.1` verdict, which `0001-F7` C-1 and C-4
  rule out.
- **(C) A new schema `0.4.0` beside `0.3.0`.** It spends a version no
  consumer can pin. It is right only if a package carries `0.3.0` before
  item `0016` merges.

## Affects

- `0001-F5` A-6, B-014, B-049 to B-054, B-068 and OQ-16; the scenarios'
  pins for B-004 to B-006, B-014, B-019, B-049 to B-054.
- Decision 0010's "Version boundary, OQ-16" row, superseded on the version
  it names.
- Schema `0.3.0`'s shipped manifest and its entry in the version lock.

## Reversal

None. The call stands only while no published package carries `0.3.0`. If
one does before item `0016` merges, option (C) applies and this decision is
reversed by a new one.
