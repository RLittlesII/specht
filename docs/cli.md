---
title: "specht command line"
description: "Every command and option the specht tool accepts, what each prints and which exit codes it returns"
type: reference
---

# Command line

Two commands: `specht` checks a tree, `specht init` writes the files a check
needs. Installed as a local tool, each runs as `dotnet specht`.

## `specht`

```text
specht [--root <DIR>] [--strict] [--json]
specht --version
```

Checks the specifications under a root against the schema version its manifest
pins. [What it checks](what-it-checks.md) lists the rules, and
[Configuration](configuration.md) the manifest.

| Option         | Value   | Default               | Does                                                                              |
| -------------- | ------- | --------------------- | --------------------------------------------------------------------------------- |
| `--root <DIR>` | `<DIR>` | The working directory | Names the repository root to check.                                               |
| `--strict`     | none    | Off                   | Fails on a violation of any severity, not only an error.                          |
| `--json`       | none    | Off                   | Writes the report document to stdout in place of the violation lines and summary. |
| `-h, --help`   | none    | Off                   | Prints the command's help and checks nothing.                                     |
| `--version`    | none    | Off                   | As the only argument, prints the tool's version and checks nothing.               |

**Stdout.** One line per violation, with a file, a line in parentheses when it
has one, a severity, a rule id, a message and, in square brackets, the
identifier it is about when it has one; then four summary lines. Each rule id
has a page under [`rules/v0.1.0/`](rules/v0.1.0/). Under `--json`, the report document
alone, as [`schema/report.schema.json`](schema/report.schema.json) describes it.
Every path is relative to the root. A run that exits `2` or `3` says why on
stderr. As the only argument, `--version` prints one line instead: the tool's
version and the commit it was built from, such as `0.1.66+28d0ba692e`. It needs
no manifest and writes nothing.

**Exit codes.** `0`, `1`, `2` or `3`; [Exit codes](exit-codes.md) says what each
means. `--version` as the only argument exits `0`.

```sh
specht --root . --strict
```

```text
src/specht/.spec/README.md(214): error SPEC031: claim 'B-007' has no row in § 9 - every § 3 claim appears there exactly once, with 'Missing' as its test until one exists [B-007]
specifications: epics 0, features 1
items: 0
rules evaluated: 21
errors: 1, warnings: 0
```

## `specht init`

```text
specht init [--root <DIR>] [--schemas] [--templates] [--all]
            [--upstream <ADDRESS>] [--upstream-version <LABEL>]
            [--upstream-hash <KIND=HASH>]...
```

Writes the newest manifest under `<DIR>/.specht/`; the part flags add the
frontmatter schemas and templates. It never overwrites a file that exists.
[Setup](setup.md) walks through the first run.

Beside an existing manifest, missing embedded schemas and templates come from its exact
`schemaVersion`, with an omitted pin meaning `0.1.0`. Rerun `specht init --schemas`
to restore deleted schemas, or `specht init --all` to fill missing schemas and
templates. Existing files are skipped byte-for-byte, even if edited or truncated;
init does not validate or repair their contents. An unshipped pin exits `3`,
lists the shipped versions on stderr and writes nothing, whatever parts are selected.

| Option                        | Value                   | Default               | Does                                                        |
| ----------------------------- | ----------------------- | --------------------- | ----------------------------------------------------------- |
| `--root <DIR>`                | `<DIR>`                 | The working directory | Names the repository root to write under.                   |
| `--schemas`                   | none                    | Off                   | Adds the selected frontmatter schemas.                      |
| `--templates`                 | none                    | Off                   | Adds the four templates.                                    |
| `--all`                       | none                    | Off                   | Adds the frontmatter schemas and templates.                 |
| `--upstream <ADDRESS>`        | HTTPS base address      | None                  | Records a public schema source in a new manifest.           |
| `--upstream-version <LABEL>`  | Nonempty label          | None                  | Records the upstream version label; required with a source. |
| `--upstream-hash <KIND=HASH>` | Repeated kind/hash pair | None                  | Supplies a SHA-256 hash for every declared schema kind.     |
| `-h, --help`                  | none                    | Off                   | Prints the command's help and writes nothing.               |

**Stdout.** One line per file, relative to the root: `written` for a file it
created, `skipped` for one that was already there. A run that exits `2` or `3` says
why on stderr.

Into a bare root, the selected schemas are Feature and item. Beside an existing
manifest that declares the epic grammar, the selection includes the epic schema.

When the existing manifest records `spechtSchema`, `--schemas` or `--all`
fetches each missing schema from that source and verifies its recorded SHA-256
before writing any file. Present files are preserved. A failed fetch or hash
check writes nothing and exits `3`. With every schema present, or without the
schemas selected, no fetch occurs. Sources must be public HTTPS addresses;
the tool sends no credentials and follows no redirects.

To record a source in a new manifest, supply all three source options. Repeat
`--upstream-hash` for `feature` and `task`, with values such as
`feature=sha256:<64 lowercase hexadecimal digits>`. Supply an epic hash when
the manifest declares that kind. The upstream version is a label; it does not
change the schema version that pins the rule vocabulary.

Source options alone write the manifest without fetching. Add `--schemas` or
`--all` to fetch missing schemas. Existing files are preserved without fetching
them; the check can reject them if they differ from their recorded hashes.
Invalid source options, including missing hashes, or any source option beside
an existing manifest, write and fetch nothing and exit `3`. Edit an existing
manifest by hand to configure its source.

**Exit codes.** `0`, `2` or `3`; [Exit codes](exit-codes.md) says what each means.

```sh
specht init
```

```text
written .specht/manifest.json
```

```sh
specht init --all
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
