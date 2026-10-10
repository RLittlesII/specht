---
title: "Decision 0007: the README's default-manifest block omits the discovery keys"
description: "The default-manifest block on the repository-root README.md is the default manifest less its five discovery keys, the keys are listed in the README's table of configurable keys without naming a layout, and the legacy layout stays off that page"
type: decision
---

# Decision 0007: the README's default-manifest block omits the discovery keys

**Date:** 2026-10-09
**Decided by:** the repository owner, asked directly during delivery of item `0005` (§ 11 OQ-11); recorded by spec-author

## The call

Asked: item `0119` holds the `### Default manifest` block of the
repository-root `README.md` byte for byte to
`.spec/schema/spec-structure.schema.json`, and its owner decision keeps the
legacy layout off that page. The default manifest now carries `layouts` with
`legacy` (decision 0004). Which gives?

The block omits the discovery keys.

- The block is `.spec/schema/spec-structure.schema.json` less the five keys
  `layouts`, `exclusions`, `taskFiles`, `epicFiles` and `companionFiles`, and
  the page says they are left out and default.
- Item `0119`'s criterion is relaxed to that, by a dated amendment; the item
  stays `done`.
- The README's "What is configurable" table gains the five keys. Its `layouts`
  row names no layout.
- The legacy layout stays off the page, as item `0119`'s owner decision of
  2026-10-09 has it.

## Why

- A manifest that omits the five keys reads as the default manifest
  (`0001-F5` B-019), so the block is still a manifest a consumer can copy and
  get the default behaviour from.
- Item `0119`'s owner decision stands: the legacy layout is not mentioned on
  the README.

## Rejected

**Showing `legacy` in the README.** The block would stay byte for byte the
schema file. Cost of rejecting: the block is no longer the whole file, and a
reader learns the default layouts and exclusions from the schema file or from
`specht init`, not from the page.

**Dropping `legacy` from the default manifest.** It reverses decision 0004's
default, under which the default manifest reproduces the literals the engine
held. Cost of rejecting: none in this item.

## Affects

- Item `0119`: its default-manifest criterion, amended 2026-10-09.
- The repository-root `README.md`: the `### Default manifest` block and the
  "What is configurable" table.
- § 11 OQ-11: resolved.
- Decision 0004's defaults are unchanged.
- No § 3 claim and no § 4 constraint is added, changed or withdrawn.

## Reversal

None.
