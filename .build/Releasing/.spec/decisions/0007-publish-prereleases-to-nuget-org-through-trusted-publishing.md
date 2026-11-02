---
title: "Decision 0007: publish prereleases to nuget.org through trusted publishing"
description: "The owner reversed the GitHub Packages feed: both packages publish to nuget.org only, every version below 1.0.0 carries the -preview label, a short-lived key from NuGet Trusted Publishing pushes them, and a prerelease version's GitHub Release is marked prerelease"
type: decision
---

# Decision 0007: publish prereleases to nuget.org through trusted publishing

**Date:** 2026-11-01
**Decided by:** the repository owner, 2026-11-01, in chat; recorded by spec-author

## The call

1. **nuget.org is the only feed.** `specht.tool` and `Specht.CodeAnalysis`
   publish to nuget.org and nowhere else. This is the reversal brief § 2 names
   for its Soft feed constraint. C-3 is retired and C-10 replaces it; B-003
   names nuget.org.
2. **Every version below `1.0.0` is an explicit prerelease.** A package
   version whose major is `0` carries the SemVer prerelease label `-preview`,
   and a consumer opts in with `--prerelease`. From `1.0.0` the label goes
   away. `version.json` carries the label, so `0055-F5` computes it
   (`0055-F5` B-009); `VerifyTag` refuses a major-`0` version that has none
   (B-014). Only a pushed `v*` tag publishes (C-1), so a branch build's
   `-g<commit>` version never reaches nuget.org.
3. **The credential is NuGet Trusted Publishing.** The `release` job holds
   `id-token: write`. A login step exchanges the workflow's OIDC token for a
   short-lived nuget.org API key for the profile the repository variable
   `NUGET_USER` names, and the push uses that key. No API key is stored, and
   no job holds `packages: write`. The GitHub Release is still created with
   the workflow's own token, granted contents write. C-5 and C-7 amended.
4. **A prerelease version's GitHub Release is marked prerelease** (B-015), so
   the Release mirrors the package.

## Why

- GitHub Packages requires an authenticated token even to read a public
  package (`0055-F7` A-1, retired by this decision). Every consumer needed a
  personal access token and a package source entry, a global install outside
  a repository had only the workaround `0055-F7` decision 0002 documents
  (`0055-F7` OQ-3), and the package did not appear in NuGet search.
- nuget.org serves a public package to an anonymous read, is a default source
  of every SDK, and is searchable. The package ids `specht.tool`,
  `specht.codeanalysis` and `specht` were free there on 2026-11-01 (owner).
- NuGet orders a bare `0.3.12` as stable, so it would offer a `0.x` package to
  every consumer by default. The label marks the pre-`1.0.0` line in the
  version itself, and `--prerelease` makes adopting it an explicit act.
- A nuget.org version is permanent: it can be unlisted, never deleted. Only a
  tag that passed every gate may publish (C-1, B-009, B-011, B-013), and § 5
  row 4 stands.
- A stored API key is a long-lived secret to rotate; trusted publishing issues
  a key per run that expires.

The mechanism was probed by the owner on 2026-11-01 in a throwaway clone with
`nbgv` 3.10.94. `version.json` `"version": "0.3-preview"` computes
`0.3.<height>-preview` on `main` and `0.3.<height>-preview-g<commit>` on any
other ref; `dotnet nbgv tag` creates `v0.3.<height>-preview`. The tag pattern
`^refs/tags/v\d+(?:\.\d+)*$` in `publicReleaseRefSpec` does not match that tag,
so a tag build stayed non-public until the pattern was widened to
`^refs/tags/v\d+(?:\.\d+)*(?:-preview)?$`. `VerifyTag`'s comparison of the tag
with `v` and the computed version holds for the labelled tag unchanged.

## Rejected

- **Keep GitHub Packages and document the token better.** Every consumer, a
  global install included, still needs a token and a source entry, and the
  package stays out of search.
- **Publish to both feeds.** The owner named nuget.org only; C-10 rules out a
  second feed.
- **A bare `0.x` version on nuget.org.** NuGet treats it as stable.
- **A stored nuget.org API key as a repository secret.** A long-lived
  credential; C-5 as amended rules it out.
- **Publishing a branch build's `-g<commit>` version.** A nuget.org version
  cannot be deleted; C-1 stands.

## Affects

- `0055-F6`: C-3 retired; C-10 added; C-5 and C-7 amended; B-003 and B-010
  amended; B-014 and B-015 added; § 1 and § 5 rows 3 and 8 amended.
  [Decision 0003](0003-the-release-names-the-ancestry-check-and-the-release-step.md)'s
  package-write grant is superseded, recorded in its Reversal.
- `0055-F5`: B-003 amended; B-009 added.
- `0055-F7`: A-1 retired; B-001, B-002, B-004 and B-005 amended; B-009 to
  B-011 added; C-5 amended; C-6 added; OQ-3 resolved; § 5 row 6 amended;
  its decision 0002 reversed.
- brief § 2, the feed constraint, and brief § 6, the install-path row.
- Epic `0055`'s summary and its out-of-epic row for NuGet.org.

## Transition

This repository's own `.config/dotnet-tools.json` pins `specht.tool`
`0.1.131`, which exists only on GitHub Packages. Its `specht` package source
and the `NuGetPackageSourceCredentials_specht` credential therefore stay until
the first nuget.org release. Then the pin moves to that version, and the source
and the credential go (`0055-F7` B-010, B-011). Until then `0055-F6` B-010 and
`0055-F7` C-5 hold as amended.

## Reversal - 2026-11-01

[Decision 0008](0008-the-push-runs-in-its-own-publish-job.md) moves
`id-token: write`, the login and the push from the `release` job into a
`publish` job of their own; call 3's "The `release` job holds
`id-token: write`" is superseded. The feed, the label, the trusted-publishing
credential and the prerelease Release stand.
