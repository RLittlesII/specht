---
title: "Decision 0009: the GitHub Release carries no package"
description: "The owner removed the release job's package download: the GitHub Release no longer attaches the two packages, which nuget.org already holds, and a Release already published at the run's commit is kept without any asset check"
type: decision
---

# Decision 0009: the GitHub Release carries no package

**Date:** 2026-11-01
**Decided by:** the repository owner, 2026-11-01, in a review comment on
`release.yml` in pull request #668; recorded by spec-author

## The question

The owner asked why the `release` job downloads the package. The answer
given on the pull request: the download exists only because "Create GitHub
Release" attaches both `.nupkg` files to the Release. `CreateGitHubRelease` in
`.build/SpechtBuild.Release.cs` passes both files to `gh release create`,
`ReleasePackages()` fails if either is missing, and `IsPublishedAtThisCommit`
checks that the Release's assets carry both packages. The answer offered:

> nuget.org already has the packages, so the Release could stop carrying them.
> Then `release` would need no download. That changes 0055-F6 C-4 and B-004, so
> the specification would change first.

The owner answered, verbatim:

> If the release doesn't need to download ... lets remove the download.

## The call

1. **The GitHub Release this Feature creates carries no package file.** Both
   packages are on nuget.org, the one feed (C-10). B-007 is withdrawn, its
   number retired.
2. **The `release` job downloads no package and reads none.** It keeps its
   checkout and SDK steps: "Create GitHub Release" is a NUKE target run through
   `./build.cmd`, and `0109-F5`'s comparison steps run the comparator with
   `dotnet`.
3. **The `publish` job is unchanged.** It downloads this run's `nupkg` artifact
   and pushes both packages (C-4, C-9).
4. **A Release already published at this commit** (`0109-F5` A-6) is judged
   without assets: published, not a draft, under this tag, and the tag at this
   run's commit. Derived by spec-author from call 1: an asset check on a
   Release that carries no package never keeps one.

## Why

The download served the attachment and nothing else. nuget.org already holds
both packages, and a second copy on the Release is a file the `release` job
must fetch, verify and keep in step with what `publish` pushed. Without it the
job that holds contents write reads no package, and a rerun of the whole run
can no longer attach a file `Pack` wrote again (OQ-14).

## Rejected

**Keep attaching the packages**, OQ-3's answer of 2026-10-08 and B-007. Cost of
rejecting: a Release no longer carries the files, so a reader takes a package
from nuget.org; Releases published before this decision keep the files they
carry, and none is rewritten.

## Affects

- `0055-F6`: B-007 withdrawn and its scenario removed; a boundary scenario
  under B-004 states that the Release carries no package; C-4 amended; § 5
  row 9 amended, rows 14 and 15 added; OQ-3 and OQ-14 gain dated notes; § 10
  entry; Tasks. Decision 0003's call 4 and decision 0008's Rejected cost gain
  Reversals pointing here.
- `0109-F5`: A-6 amended: the Release is kept without an asset check; § 10
  entry.
- `0201-F6`: B-020 amended: the release publishes both packages and attaches
  neither; its scenario amended; § 10 entry.
- Item `0333` delivers it. `0055-F6` § 6 rows 8 and 11 and the § 7 of
  `0055-F6`, `0109-F5` and `0201-F6` (`implementer`), and § 8 and § 9 of
  `0055-F6` and `0201-F6` (`test-writer`) follow in that item.

## Reversal

None.
