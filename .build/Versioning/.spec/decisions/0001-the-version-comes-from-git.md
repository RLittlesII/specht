---
title: "Decision 0001: the version comes from git"
description: "Nerdbank.GitVersioning computes the package version from version.json and the git height; the release tag is created from it with nbgv tag; the package version and the schema version are independent"
type: decision
---

# Decision 0001: the version comes from git

**Date:** 2026-10-08
**Decided by:** the repository owner (2026-10-08); recorded by spec-author

## The call

The package version is computed by Nerdbank.GitVersioning (NBGV) from
`version.json` and the git height. `publicReleaseRefSpec` names `main` and `v*`
tags; every other ref builds a prerelease. A release is cut with `nbgv tag`,
which creates the `v*` tag from the computed version. The package version is
independent of the schema version `0001-F7` owns.

## Why

The owner named NBGV explicitly. A version that is a function of the commit
gives every `Pack` - local, CI or release - a version that names one commit,
and a tag made from the computed version cannot disagree with what the release
builds. Schema versions have their own lifecycle (`0001-F7` decision 0002:
version 1 freezes at the first publish, and a shipped version never changes);
tying the two would force a tool release to move a schema or the reverse.

## Rejected

The owner gave the choice without a list of alternatives. The two it displaces:

**A version typed into `Directory.Build.props`.** Cost of rejecting: none
beyond adopting NBGV; keeping it costs a forgotten bump that publishes a
duplicate version, and every branch build carrying a release number.

**The version read from the tag name at release time.** Cost of rejecting:
none beyond the above; keeping it leaves local and CI packs without a
meaningful version, and nothing stops a tag that disagrees with the commit.

## Affects

- `0055-F5` § 2 A-1, A-2; § 3 B-001 to B-007; § 4 C-1, C-3.
- `0055-F6`: the tag it publishes on is created this way.

## Reversal

None.
