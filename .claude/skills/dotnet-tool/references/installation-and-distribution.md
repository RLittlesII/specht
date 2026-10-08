---
title: Installation & Distribution
description: How Specht.Tool is packed, installed through a local tool manifest, published to NuGet.org, and restored by a consumer's CI.
type: reference
---

# Installation & Distribution

## This repository's path

`specht` ships as `Specht.Tool` on NuGet.org, command `specht`. A consumer
installs it **per repository** through a local tool manifest: a committed
`.config/dotnet-tools.json` pins the tool version beside the schema version the
repository has chosen, and `dotnet tool restore` in CI brings it back. Nothing
is installed globally, and no consumer carries the engine as a project reference
(README § 2 Must-4, § 6).

This repository is its own first consumer. Its build and its pre-commit hook run
`dotnet specht` from its own manifest — the same call site every other
repository uses (README § 7). Until the first package is published, the
self-check runs the project directly (`dotnet run --project src/Specht.Tool --
--root .`); the moment a package exists, the manifest replaces that.

## Pack

```bash
./build.sh Pack
# Output: .artifacts/nupkg/Specht.Tool.<version>.nupkg
```

`dotnet pack -c Release` does the same thing without the build's version
inference.

## Local Development Install

Install directly from the local nupkg directory for rapid iteration:

```bash
dotnet tool install -g Specht.Tool --add-source .artifacts/nupkg
specht --help

# Uninstall when done iterating
dotnet tool uninstall -g Specht.Tool
```

## Global Install (Workstation)

```bash
dotnet tool install -g Specht.Tool
```

Installs to `~/.dotnet/tools` (Linux/macOS) or `%USERPROFILE%\.dotnet\tools` (Windows).
The command is on `PATH` immediately — invoke as `specht`.

**When to use**: a developer trying the tool on a repository that has not
adopted it. Not how a consumer repository pins it.

## Local Install (Version-Pinned, Per-Repo) — the consumer path

```bash
# Initialize the manifest (once per repo, at the root)
dotnet new tool-manifest

# Install into the manifest
dotnet tool install Specht.Tool --version 1.0.0

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
Then `dotnet specht init` writes the schema set and templates into `<root>/.spec/`
from the embedded copies, never overwriting an existing file.

**When to use**: every consumer repository, and this one. A pinned tool version
beside a pinned `schemaVersion` is what makes lagging deliberate rather than
drift.

## Publish to NuGet.org

```bash
dotnet nuget push .artifacts/nupkg/Specht.Tool.1.0.0.nupkg \
  --api-key $NUGET_API_KEY \
  --source https://api.nuget.org/v3/index.json
```

## Publish to a Private Feed (GitHub Packages)

Not the chosen channel — the four consumer repositories span organisations and
NuGet.org needs no feed configuration on their side. Kept because the command is
the same shape:

```bash
dotnet nuget push .artifacts/nupkg/Specht.Tool.1.0.0.nupkg \
  --api-key $GITHUB_TOKEN \
  --source https://nuget.pkg.github.com/YOUR_ORG/index.json
```

A private feed needs a `nuget.config` at the consumer's repository root so
teammates don't pass `--source` by hand:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="github" value="https://nuget.pkg.github.com/YOUR_ORG/index.json" />
  </packageSources>
</configuration>
```

## CI Workflow — GitHub Actions

This repository's `ci.yml` is NUKE-generated and builds, tests and packs on every
push. Publishing is a separate, tag-triggered workflow of this shape:

```yaml
name: publish

on:
  push:
    tags: ["v*"]

jobs:
  publish:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - uses: actions/setup-dotnet@v4
        with:
          global-json-file: global.json

      - run: dotnet pack -c Release -p:Version=${GITHUB_REF_NAME#v}

      - run: |
          dotnet nuget push .artifacts/nupkg/*.nupkg \
            --api-key ${{ secrets.NUGET_API_KEY }} \
            --source https://api.nuget.org/v3/index.json
```

Tag and release:

```bash
git tag v1.2.0
git push --tags
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
