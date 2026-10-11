---
title: "Decision 0007: The package describes itself"
description: "The packed specht.tool names its id, repository, commit, project page, licence, author, copyright, description, tags and release notes, and carries an icon and a readme written for the package; this Feature owns those claims"
type: decision
---

# Decision 0007: The package describes itself

**Date:** 2026-10-11
**Decided by:** the repository owner asked for it and delegated the list; the list, the owning Feature and the readme's form are spec-author's proposal on that delegation

## The call

The owner, 2026-10-11:

> fix package id and repository. Can we make tool "release ready" so when it
> hits a package manager it has all the README.md and stuff a good package
> should have?

Two things are the owner's words: the package id is fixed, and the package
names its repository (`0055-F6` OQ-11, resolved by this ruling). Everything
below is spec-author's proposal. The owner was not asked about any line of it.

- **What the package says.** Its id `specht.tool` (B-017); the git repository
  `https://github.com/RLittlesII/specht` (B-018) and the commit it was packed
  from (B-019); that address as its project page (B-020); the licence
  expression `MIT` (B-021); `Rodney Littles, II` as author (B-022) and in the
  copyright notice (B-023); a one-sentence description (B-024); five tags
  (B-028); and the repository's releases as its release notes (B-029).
- **What the package carries.** A readme it names (B-025) and an icon it names
  (B-027).
- **The readme is a file written for the package** (C-9), and every link and
  image in it is an absolute `https` address (B-026). It gives the commands
  and links to the install document; it does not restate the feed, the source
  entry or the token (C-10).
- **This Feature owns the claims.** They are claims about the package
  `src/tool` packs.
- **Not claimed:** source link and a runner-deterministic build (OQ-3), a
  symbol package, signing (§ 5 rows 14, 16 and 18).

## Why

- **This Feature, not `0055-F6` or `0055-F7`.** C-5 already states the
  package's identity here: only `src/tool` packs, as `specht.tool`. The
  settings are written in that project. `0055-F6` is about what a tag's run
  does and `0055-F7` about what a consumer commits and supplies; neither is
  about what the package says. `0055-F1` and `0055-F6` also carry the owner's
  decision that the build has no tests, so a claim placed there stays
  `Missing` by decision, which is how the id went wrong unseen (lesson 0001).
  This Feature's tiers run on every build.
- **A readme written for the package.** A package page resolves no
  repository-relative link or image. The repository's `README.md` is read in
  the repository, on any branch, fork or clone, where relative links are the
  ones that keep working; it opens with centred HTML and badges and holds the
  directory structure, none of which a package page needs. Making it serve
  both would turn every one of its links into an address fixed to one branch.
- **MIT as an expression.** `LICENSE.md` is the MIT licence, and an
  expression is what a feed reads without opening a file.
- **The author and the copyright** are `LICENSE.md`'s.
- **The project page is the repository** because no documentation site exists
  (`0002-F2` is deferred).
- **The icon is the avatar** (OQ-1): it is the one square image of the mark in
  `.assets/logos/`. Nothing was drawn or resized for this.

## Rejected

**`0055-F6` owns the metadata.** OQ-11 was raised there. Rejected for the
reasons above. Cost of rejecting: a reader of the release Feature follows a
citation to learn what ties the package to the repository.

**A new Feature for the package.** The cleanest name. Rejected: C-5 is already
here, and thirteen claims over one project file do not need their own epic
entry. Cost of rejecting: a Feature titled "The check command" now holds
claims that are about the host's package, not about the check.

**Packing the repository's `README.md` with absolute links.** One file, no
drift. Rejected: see "A readme written for the package". Cost of rejecting:
two files say what the tool is and how to run it, and nothing compares them.

**The logo with the wordmark as the icon.** It is the image `README.md` shows.
Rejected: it is 2400 by 3040 pixels, and a feed draws an icon in a square.

**Claims for source link and a runner-deterministic build.** Rejected for now:
OQ-3. Cost of rejecting: nothing proves the debug information in the package
maps to the repository.

## Affects

- B-017 to B-029, added.
- C-9 and C-10, added.
- § 2 row 6; § 5 rows 13 to 19; OQ-1 to OQ-4.
- `check.feature`: thirteen scenarios, `@B-026` a `@boundary`.
- `0055-F6` OQ-11, resolved.

## Reversal

None.
