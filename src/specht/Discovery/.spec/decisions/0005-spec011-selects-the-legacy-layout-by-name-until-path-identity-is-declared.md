---
title: "Decision 0005: SPEC011 selects the legacy layout by name until path identity is declared"
description: "Between item 0005 and item 0006, SPEC011 applies to the layout named legacy, so a manifest that renames that layout turns SPEC011 off for it until a layout can declare its path identity; the two items stay separate pull requests"
type: decision
---

# Decision 0005: SPEC011 selects the legacy layout by name until path identity is declared

**Date:** 2026-10-09
**Decided by:** the repository owner, asked directly during delivery of item `0005` (§ 11 OQ-5); recorded by spec-author

## The call

Item `0005` lands before item `0006`, as its own pull request. Between the two,
`SPEC011` applies to the layout whose manifest name is `legacy` and to no
other, so a manifest that renames that layout turns `SPEC011` off for it. Item
`0006` ends the interim: a layout then declares its path identity and `SPEC011`
follows the declaration (B-004).

## Why

- Nothing ships between the two items: the first release waits on the whole
  Feature.
- The default manifest keeps the name `legacy`, so a repository on the default
  sees no change.

## Rejected

**Landing items `0005` and `0006` in one pull request.** No tree would ever
hold the interim. Cost of rejecting: for as long as `0006` is unbuilt, a
manifest that renames the legacy layout loses `SPEC011` with no diagnostic.

## Affects

- B-004: unchanged; it is the claim that ends the interim.
- B-009: its scenario renames both layouts, and under the interim `SPEC011`
  reports nothing for either.
- § 11 OQ-5: resolved.
- Items `0005` and `0006`.

## Reversal

**Amended 2026-10-09 by the repository owner ([decision 0008](0008-the-layouts-are-named-epics-and-features-and-the-file-shape-keys-are-lists.md)).**
The default manifest's layout `legacy` is renamed `epics`. The interim stands,
and the name `SPEC011` selects is `epics`: "the layout named `legacy`" above
reads as "the layout named `epics`". The sections above stand as written.
