---
title: Installation & Distribution
description: How specht.tool is packed, installed through a local tool manifest, published to NuGet.org, and restored by a consumer's CI.
type: reference
---

# Installation & Distribution

## This repository's path

`specht` ships as `specht.tool` on NuGet.org, command `specht`. A consumer
installs it **per repository** through a local tool manifest: a committed
`.config/dotnet-tools.json` pins the tool version beside the schema version the
repository has chosen, and `dotnet tool restore` in CI brings it back. No build
depends on a global install, and no consumer carries the engine as a project
reference (brief § 2 Must-4, § 6).

This repository's build and pre-commit self-check follow
[`specht-conventions` § Build and test](../../specht-conventions/SKILL.md#build-and-test).

## Pack

```bash
./build.sh Pack
# Output: .artifacts/nupkg/specht.tool.<version>.nupkg
```

`dotnet pack -c Release` does the same thing without the build's version
inference.

## Local Development Install

Install directly from the local nupkg directory for rapid iteration:

```bash
dotnet tool install -g specht.tool --add-source .artifacts/nupkg
specht --help

# Uninstall when done iterating
dotnet tool uninstall -g specht.tool
```

## Global Install (Workstation)

```bash
dotnet tool install -g specht.tool
```

Installs to `~/.dotnet/tools` (Linux/macOS) or `%USERPROFILE%\.dotnet\tools` (Windows).
The command is on `PATH` immediately — invoke as `specht`.

**When to use**: a shell that wants bare `specht`, or a developer trying the
tool on a repository that has not adopted it. Documented as a second path
(`0001-F2` decision 0009). Not how a consumer repository pins it.

## Local Install (Version-Pinned, Per-Repo) — the consumer path

```bash
# Initialize the manifest (once per repo, at the root)
dotnet new tool-manifest

# Install into the manifest
dotnet tool install specht.tool --version 1.0.0

# Restore from manifest (CI, teammates)
dotnet tool restore
```

The manifest lives at `.config/dotnet-tools.json`. **Commit it to the repository.**

```json
{
  "version": 1,
  "isRoot": true,
  "tools": {
    "specht.tool": {
      "version": "1.0.0",
      "commands": ["specht"]
    }
  }
}
```

With a local install, invoke via `dotnet tool run specht` or `dotnet specht`.
Then `dotnet specht init` writes the manifest, the frontmatter schemas and the templates into `<root>/.specht/`
from the embedded copies, never overwriting an existing file.

**When to use**: every consumer repository; this repository retains its committed
manifest for consumer installation and restore coverage. A pinned tool version
beside a pinned `schemaVersion` is what makes lagging deliberate rather than drift.

## Publish to NuGet.org

Every version below `1.0.0` is an explicit SemVer prerelease: `version.json`
carries `-preview` (`0.3-preview`), so a tag build packs `0.3.<height>-preview`
and a consumer opts in with `--prerelease`. NuGet treats a bare `0.x` as stable.
`VerifyTag` fails a major-`0` version with no label (`0055-F6` decision 0007).

Only a `v*` tag publishes. A nuget.org version is permanent - it can be unlisted,
never deleted - so a branch build (`-preview-g<commit>`) is never pushed.

## CI Workflow - GitHub Actions

`release.yml` is NUKE-generated from `.build/SpechtBuild.Release.cs`. On a `v*`
tag the `release` job logs in with NuGet trusted publishing - `NuGet/login@v1`
exchanges the job's OIDC token (`id-token: write`) for a short-lived key, the
nuget.org profile name coming from the repository variable `NUGET_USER` - then
runs the `PushReleasePackages` target with that key. No API key is stored.

Tag and release:

```bash
dotnet nbgv tag
git push origin v0.3.<height>-preview
```

A consumer's CI needs only:

```yaml
- run: dotnet tool restore
- run: dotnet specht --strict
```

## Verify Installed Tools

```bash
dotnet tool list -g           # global tools
dotnet tool list              # local tools (from manifest)
```
