---
title: "Decision 0001: use native package and tool restore"
description: "Configure the published checker in NuGet and the tool manifest; keep solution restore independent of tool-feed authentication"
type: decision
---

# Decision 0001: use native package and tool restore

**Date:** 2026-10-14
**Decision:** implementation of B-002, B-003 and B-007 within OQ-2.

The root tool manifest pins published `specht.tool` 0.1.90, command `specht`.
Root `nuget.config` maps exactly that package to GitHub Packages and all other
packages to nuget.org. Audit data comes from nuget.org. The source key is
`specht`; NuGet reads `NuGetPackageSourceCredentials_specht` from the process
environment. No credential is committed or passed in a command argument.

Use native restore commands. `dotnet tool restore` restores the complete tool
manifest with a package read credential. `dotnet restore specht.slnx` and
`dotnet build specht.slnx` restore and build the solution without that credential.
The existing NUKE Restore target still restores tools, then the solution; a
complete NUKE invocation therefore needs the tool-feed credential. B-007 is
about solution restore and build, not credential-free tool restore. No filtered
manifest, custom tool selection or NUKE change is needed.

Automated scenarios use synthetic tools on local folder feeds and fresh caches.
B-007 omits the checker package and removes credential environment variables.
A separate hand run restores the real published package using the existing
login's credential; `dotnet specht --version` identifies the published commit.

Tool and schema versions have independent number sequences (`0001-F7` C-1).
The package pin does not change the repository's schema pin. The generated CI, main and release build jobs supply the same native environment
credential from their workflow token, granted `packages: read` (0067 and 0085).
This is required when the existing tool restore starts reading the new pin.
Self-check adoption (0032), Renovate (0077) and installation documentation
(0088) remain separate work.

Native behavior: [package source mapping](https://learn.microsoft.com/en-us/nuget/consume-packages/package-source-mapping),
[environment credentials](https://learn.microsoft.com/en-us/nuget/consume-packages/consuming-packages-authenticated-feeds),
[solution restore](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-restore)
and [tool restore](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-tool-restore).
