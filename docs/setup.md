---
title: "Setting up specht"
description: "From an empty repository to a checked one: install, initialise, the first check and the manifest"
type: reference
---

# Setup

From an empty repository to a checked one.

## Install

> Not published yet. Install steps land with the first release.

```sh
dotnet new tool-manifest          # once, if the repository has none
dotnet tool install specht.tool   # records the version in .config/dotnet-tools.json
```

The local tool manifest pins the tool's version in the repository, so CI and
every clone run the same release after `dotnet tool restore`. A local tool runs
as `dotnet specht`.

`dotnet tool install -g specht.tool` puts `specht` on PATH for a shell. It is
not pinned per repository, so a build still uses the manifest.

## Initialise

```sh
dotnet specht init
```

```text
written .specht/manifest.json
written .specht/schema/frontmatter/epic.schema.json
written .specht/schema/frontmatter/feature.schema.json
written .specht/schema/frontmatter/task.schema.json
written .specht/templates/adr.md
written .specht/templates/decision.md
written .specht/templates/feature.md
written .specht/templates/lesson.md
```

The manifest, the three frontmatter schemas and four blanks. A file that exists
is listed as `skipped` and left as it was, so `init` is safe to run again.
`--root <dir>` writes under another directory.

## First check

```sh
dotnet specht
```

```text
specifications: epics 0, features 0
items: 0
rules evaluated: 21
errors: 0, warnings: 0
```

Clean, because there is nothing to check yet. Copy `.specht/templates/feature.md`
to `<area>/.spec/README.md`, fill it in, put its `.feature` file beside it and
run the check again: each violation is one line with a file, a line and a rule
id. [What it checks](what-it-checks.md) says where a specification is found and
which rules run.

`--strict` fails on a warning too. `--json` prints the report document in place
of the lines. [Command line](cli.md) lists every option, and
[Exit codes](exit-codes.md) what the build reads; before `init`, the check
exits `2` for want of a manifest.

## Manifest

One file configures the check: `.specht/manifest.json`. Change
a setting by editing it.

```json
{
  "schemaVersion": "0.1.0",
  "rules": { "SPEC060": "warning" }
}
```

- `schemaVersion` pins the schema the tree is checked with. The pin is exact.
  `init` writes no pin, and a manifest without one is read as `0.1.0`. A version
  the installed tool does not ship exits `3` and names the versions it does.
- `rules` sets a rule's level: `error`, `warning` or `off`.
- A key left out takes its default, so the two above are a whole manifest.

[Configuration](configuration.md) has every key and the default manifest.
