---
title: "Setting up specht"
description: "Install, initialise, configure the manifest and share your frontmatter schemas through GitHub"
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

A repository with no .NET project installs the same way; the SDK is needed for
the tool alone ([Without .NET code](non-dotnet.md)).

## Initialise

```sh
dotnet specht init --all
```

```text
written .specht/manifest.json
written .specht/schema/frontmatter/feature.schema.json
written .specht/schema/frontmatter/task.schema.json
written .specht/templates/adr.md
written .specht/templates/decision.md
written .specht/templates/feature.md
written .specht/templates/lesson.md
```

The manifest, the Feature and item frontmatter schemas and four blanks. Without `--all`,
`init` writes only the manifest; `--schemas` adds the frontmatter schemas and
`--templates` adds the blanks. A file in the selection that exists is listed as
`skipped` and left as it was, so `init` is safe to run again. `--root <dir>`
writes under another directory.

Beside an existing manifest that declares the epic grammar, `--schemas` and
`--all` also select the epic frontmatter schema.

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

## Schemas maintained in your repository

Start with `dotnet specht init --schemas`, then edit the frontmatter schemas under
`.specht/schema/frontmatter/`. Select them when checking:

```sh
dotnet specht --schema-source disk
```

To keep using them, add `"schemaSource": "disk"` to `.specht/manifest.json` with
a `schemaVersion` of `0.2.0` or later. Feature and task schema files are required;
epic is required when the manifest explicitly declares `identifiers.epic`.
Use `frontmatterSchemas` to change filenames within the same folder.
[Configuration](configuration.md) describes source selection and upstream hashes.

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

## Host your schemas on GitHub

Publish your frontmatter JSON Schemas in a **public** GitHub repository:

```text
schema/frontmatter/feature.schema.json
schema/frontmatter/task.schema.json
```

Start from the files `init --schemas` writes under `.specht/schema/frontmatter/`
and adapt them. Only frontmatter schemas come from your source: the rule
vocabulary, manifest schema and templates stay the pinned embedded version's.
Private sources are unsupported in the first release; the tool reads no
credential from the environment or keychain.

In the schema repository's checkout, after publishing the commit, replace
`OWNER/REPOSITORY` below with its GitHub owner and repository name. These POSIX
shell commands hash the committed bytes, which the raw address serves:

```sh
schema_repo='OWNER/REPOSITORY'
schema_ref=$(git rev-parse HEAD)
schema_url="https://raw.githubusercontent.com/$schema_repo/$schema_ref/schema/frontmatter/"
feature_hash=$(git show "$schema_ref:schema/frontmatter/feature.schema.json" | shasum -a 256 | cut -d ' ' -f 1)
task_hash=$(git show "$schema_ref:schema/frontmatter/task.schema.json" | shasum -a 256 | cut -d ' ' -f 1)
```

The raw HTTPS base address ends at the folder; `init` appends each schema's
file name. A [commit address](https://docs.github.com/en/repositories/working-with-files/using-files/getting-permanent-links-to-files)
keeps the content fixed. The version is a label, independent of `schemaVersion`;
the tool does not add it to the address. Each hash is `sha256:` followed by
64 lowercase hexadecimal digits.

In the same shell, change to a new consumer repository with the local tool
installed and no `.specht/manifest.json`, then run:

```sh
dotnet specht init --all \
  --upstream "$schema_url" --upstream-version "$schema_ref" \
  --upstream-hash "feature=sha256:$feature_hash" \
  --upstream-hash "task=sha256:$task_hash"
dotnet specht
```

`init` records `location`, `version` and `hashes` in the manifest's `spechtSchema`,
fetches the two schemas into `.specht/schema/frontmatter/` and writes the four
embedded templates. Every fetch and hash check succeeds before any file is
written; a failure exits `3` and writes nothing. Existing files are preserved.
Source options alone write only the manifest; fetching requires `--schemas`
or `--all`.

Commit the manifest and fetched files. The check verifies their hashes and uses
them offline; a missing or changed schema exits `3`. `dotnet specht init --schemas`
fetches missing files from the recorded source. To adopt a source beside an
existing manifest, edit its `spechtSchema` by hand as [Configuration](configuration.md)
describes; source options beside that manifest are refused. A manifest declaring
`identifiers.epic` also requires an epic schema and hash.
