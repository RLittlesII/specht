---
title: "Decision 0001: isolate package consumption from development tool restore"
description: "Pin the published checker, map its package to GitHub Packages and restore development prerequisites independently so builds need no feed token"
type: decision
---

# Decision 0001: isolate package consumption from development tool restore

**Date:** 2026-10-14
**Decision:** implementation design for B-002, B-003 and B-007, within OQ-2's
accepted token-free build boundary.

The root tool manifest pins `specht.tool` at the published `0.1.90`, command
`specht`, beside the existing development tools. Root `nuget.config` maps
exactly `specht.tool` to `https://nuget.pkg.github.com/rlittlesii/index.json`
and everything else to nuget.org. Audit data comes from nuget.org alone.
The source is named `specht`; NuGet reads its credential from the native
`NuGetPackageSourceCredentials_specht` environment variable. No credential
is written to configuration or passed on a command line.

`dotnet tool restore` from the root restores the complete committed manifest
and requires a feed read credential. The build's `Restore` target instead
derives a development-only manifest under `.artifacts/tool-restore/`,
excluding `specht.tool` and preserving every other pin and setting. It
restores that derived manifest before the solution. The committed manifest
remains the one source of tool versions; it is never rewritten by the build.

The derived manifest is necessary because tool restore has no per-package
selection option. A second committed manifest would duplicate development
pins. Ignoring restore failures would hide genuine build-tool failures.
Credential-dependent skip logic would make the build's prerequisites vary
with the caller's environment. None is adopted.

Automated proofs use synthetic packages on local folder feeds and fresh
caches, never GitHub or a real token. A separate hand run verifies the actual
published package restore using the existing authenticated login.
The private feed is unavailable in the token-free build proof. The package
pin is checked against the recorded first-release version, whose publication
is proven by `0055-F6` section 8.

This item does not change the self-check invocation (0032), workflow feed
authentication (0067 and 0085), Renovate feed access (0077) or the install
document (0088). Those remain separate delivery work.

Native behavior is documented in [package source mapping](https://learn.microsoft.com/en-us/nuget/consume-packages/package-source-mapping),
[environment credentials](https://learn.microsoft.com/en-us/nuget/consume-packages/consuming-packages-authenticated-feeds)
and [tool restore](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-tool-restore).

Tool and schema versions have independent number sequences (`0001-F7` C-1).
This package pin does not change `.specht/manifest.json` or couple either
version to the other. Separately, the `v0.1.90` tag ships only the schema
`v0.1.0` directory. Item 0032 owns selecting a published tool that ships the
repository's exact schema pin when it switches the self-check invocation.
