---
title: "Decision 0001: The deferral is lifted, and the site is published to GitHub Pages"
description: "The owner lifted the site's deferral, so it is specified and cut beside 0002-F1's remaining claims; DocFX builds it and GitHub Pages hosts it, published from CI"
type: decision
---

# Decision 0001: The deferral is lifted, and the site is published to GitHub Pages

**Date:** 2026-11-06
**Decided by:** the repository owner; what is marked spec-author's below was not put to the owner

## The call

The repository owner ruled, 2026-11-06:

1. **The deferral is lifted now.** Site work may be specified and cut beside
   `0002-F1`'s remaining claims. B-003, which held every piece of work until
   `0002-F1` shipped or the deferral was lifted, is withdrawn.
2. **DocFX builds the site** from the existing `docs/*.md` pages and the API
   reference it generates for `0002-F1`. This resolves OQ-2. The ruling, its
   rationale and the alternative turned down are `0002-F1` decision 0002, and
   are not repeated here.
3. **GitHub Pages hosts the site, published from CI.** This resolves OQ-1.

The repository is private today, so whether a GitHub Pages site from it can be
read at a public URL depends on the owner's GitHub plan. B-002 asks for a
public URL; OQ-3 records the question and blocks B-002.

Spec-author's, on those rulings:

- **B-003 is withdrawn, not amended.** Its only assertion was the deferral.
  Its id stays retired and its `@B-003` scenario is removed, as `0002-F1`
  removed the scenarios of its withdrawn B-009 and B-011.
- **C-1 keeps the content dependency and drops the wait.** The site is still
  built from `0002-F1`'s generated reference; the work that builds it waits
  on the work that generates it, not on the whole of `0002-F1`.
- **C-2 keeps the ADR.** The owner chose the framework; the ADR that records
  the alternatives and the trade-off is still written before design starts,
  by the work that delivers B-001.
- **B-001's content is the owner's `docs/*.md` and the API reference.** It
  read "the commands' usage docs", which `0002-F1` decision 0001 made the one
  page `docs/cli.md`, one of those pages.
- **When the site publishes - each push to `main`, or each release - is not
  ruled.** OQ-4 records it.

## Why

The owner recorded no reason for lifting the deferral or for GitHub Pages
beyond the rulings above. For the framework, `0002-F1` decision 0002.

## Rejected

- **Holding the site until `0002-F1` ships**, the deferral carried from
  2026-09-29 (B-003). The owner lifted it.
- **Astro Starlight** as the framework: `0002-F1` decision 0002.

## Affects

B-001 (amended), B-002, B-003 (withdrawn), C-1 and C-2 (amended), A-1 to A-3
(amended), § 1, § 5 row 2, OQ-1 and OQ-2 (resolved), OQ-3 and OQ-4 (opened),
the `.feature` file.

## Reversal

None.
