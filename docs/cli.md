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
has a page under [`rules/v1/`](rules/v1/). Under `--json`, the report document
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
specht init [--root <DIR>]
```

Writes the newest schema set and templates under `<DIR>/.spec/`, and never
overwrites a file that exists. [Setup](setup.md) walks through the first run.

| Option         | Value   | Default               | Does                                          |
| -------------- | ------- | --------------------- | --------------------------------------------- |
| `--root <DIR>` | `<DIR>` | The working directory | Names the repository root to write under.     |
| `-h, --help`   | none    | Off                   | Prints the command's help and writes nothing. |

**Stdout.** One line per file, relative to the root: `written` for a file it
created, `skipped` for one that was already there. A run that exits `2` says
why on stderr.

**Exit codes.** `0` or `2`; [Exit codes](exit-codes.md) says what each means.

```sh
specht init
```

```text
written .spec/schema/epic.frontmatter.schema.json
written .spec/schema/feature-spec.frontmatter.schema.json
written .spec/schema/spec-structure.schema.json
written .spec/schema/task.frontmatter.schema.json
written .spec/templates/adr.md
written .spec/templates/decision.md
written .spec/templates/feature.md
written .spec/templates/lesson.md
```
