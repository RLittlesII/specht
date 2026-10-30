---
title: "specht command line"
description: "Every command and option the specht tool accepts, what each prints and which exit codes it returns"
type: reference
---

# Command line

`specht` checks a tree, `frontmatter` checks one document, `specht init` writes the files a check needs, and
`schema` groups commands for the repository's schema version. `specht upgrade` brings its schemas and templates to the pin, skipping the schemas under `schemaSource` `disk` with no `spechtSchema` and the templates named in `consumerTemplates`.
`evidence` reconciles supplied build and runner exports. `rules` lists the pinned rules and their effective levels.
`manifest` prints the effective configuration or the newest shipped default.
Installed as a local tool, each runs as `dotnet specht`.

## `specht`

```text
specht [--root <DIR>] [--strict] [--json] [--schema-source <embedded|disk>] [--explain <SPEC###>]
specht --version
```

Checks the specifications under a root against the schema version its manifest
pins. [What it checks](what-it-checks.md) lists the rules, and
[Configuration](configuration.md) the manifest.

| Option                             | Value                | Default                            | Does                                                                                                            |
| ---------------------------------- | -------------------- | ---------------------------------- | --------------------------------------------------------------------------------------------------------------- |
| `--root <DIR>`                     | `<DIR>`              | The working directory              | Names the repository root to check.                                                                             |
| `--schema-source <embedded\|disk>` | `embedded` or `disk` | Manifest selection, then automatic | Selects frontmatter schemas for this run; overrides a valid manifest selection.                                 |
| `--strict`                         | none                 | Off                                | Fails on a violation of any severity, not only an error.                                                        |
| `--json`                           | none                 | Off                                | Writes the report document to stdout in place of the violation lines and summary.                               |
| `--explain <SPEC###>`              | A rule id            | None                               | Prints the entire rule page for the pinned schema version, including YAML frontmatter, without running a check. |
| `-h, --help`                       | none                 | Off                                | Prints the command's help and checks nothing.                                                                   |
| `--version`                        | none                 | Off                                | As the only argument, prints the tool's version and checks nothing.                                             |

Use `--schema-source disk` to read schemas maintained in the consumer repository under
`.specht/schema/frontmatter/`, with filenames from `frontmatterSchemas`. Feature and
task schemas are required; epic is required only when the manifest declares
`identifiers.epic`. The option works with every shipped pin. To persist the choice,
set `"schemaSource": "disk"` in a manifest pinned to schema `0.2.0` or later.
Missing or invalid selected files exit `3`, with their relative path on stderr
and no stdout. A recorded `spechtSchema` still requires hash verification under
`disk`; selecting `embedded` beside that record is an invalid input.

Use `specht --explain SPEC031` to print the complete embedded rule page for the
manifest's schema pin, including its YAML frontmatter. A known rule exits `0`;
an id outside that version's vocabulary is named on stderr and exits `4` with
no stdout. A missing manifest exits `2`; an invalid manifest or unshipped pin
exits `3`, with no page printed. Explain does not run the check.

**Stdout.** One line per violation, with a file, a line in parentheses when it
has one, a severity, a rule id, a message and, in square brackets, the
identifier it is about when it has one; then four summary lines. Each rule id
has a page under [`rules/v0.1.0/`](rules/v0.1.0/). Under `--json`, the report document
alone, as [`schema/report.schema.json`](schema/report.schema.json) describes it.
Every path is relative to the root. A run that exits `2` or `3` says why on
stderr. As the only argument, `--version` prints one line instead: the tool's
version and the commit it was built from, such as `0.1.66+28d0ba692e`. It needs
no manifest and writes nothing.

**Exit codes.** A check returns `0`, `1`, `2` or `3`; `--explain` can also return
`4` for an unknown rule. [Exit codes](exit-codes.md) says what each means.
`--version` as the only argument exits `0`.

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

## `specht manifest`

```text
specht manifest [--root <DIR>] [--defaults] [--json]
```

Prints the manifest as the check reads it: declared values are preserved,
omitted settings the engine fills come from the exact pin's defaults, and
omitted rule levels come from the pinned rule pages. Optional capabilities
that depend on a declaration remain absent. With no pin, the effective version
is `0.1.0`.

`--defaults` prints the newest shipped version's embedded manifest directly.
It ignores the root and its pin, including a missing root or rejected manifest;
it does not expand effective settings into that embedded document.

| Option         | Value   | Default               | Does                                                      |
| -------------- | ------- | --------------------- | --------------------------------------------------------- |
| `--root <DIR>` | `<DIR>` | The working directory | Selects the root whose effective manifest is printed.     |
| `--defaults`   | none    | false                 | Prints the newest shipped default without reading a root. |
| `--json`       | none    | false                 | Prints one JSON document for either selection.            |
| `-h, --help`   | none    | none                  | Prints the command's help.                                |

**Stdout.** Only the manifest document. Without `--json`, JSON is indented;
root keys follow the selected embedded default, followed by other declared
keys in ordinal order. Rule entries follow ordinal rule id order. `--json`
prints the same keys and values as one compact JSON document.

The command is read-only and offline. It neither checks specification files
nor fetches a recorded upstream source or loads local frontmatter schemas.

**Exit codes.** `0` on success; `2` for a missing root or manifest; `3` for a
rejected manifest or pin. Failures go to stderr with empty stdout, using the
check's diagnostics. `--defaults` succeeds without a valid root or manifest.

```sh
specht manifest --root .
specht manifest --defaults --json
```

## `specht rules`

```text
specht rules [--root <DIR>] [--json]
```

Lists every rule in the manifest's exact schema version, including rules set to
`off`, in ascending ordinal order of rule id. Each stdout line contains the rule
id, family, default severity and effective level, separated by single tabs.
Severities and levels use lower-case `error`, `warning` or `off`; families retain
the rule page's spelling. A manifest override supplies the effective level;
otherwise the rule's default severity applies.

With `--json`, stdout is one array with the same entries and order. Each object
has `id`, `family`, `defaultSeverity` and `effectiveLevel` string members; severity
and level retain the text form's lower-case spelling.

| Option         | Value   | Default               | Does                                                               |
| -------------- | ------- | --------------------- | ------------------------------------------------------------------ |
| `--root <DIR>` | `<DIR>` | The working directory | Selects the repository whose manifest supplies the pin and levels. |
| `--json`       | none    | Off                   | Prints the listing as one JSON document.                           |
| `-h, --help`   | none    | Off                   | Prints command help.                                               |

The command reads only the manifest and embedded rule catalogue. It evaluates no
rule, reads no specification, writes no files and makes no network requests.
Stdout contains only the listing, with no header or summary.

**Exit codes.** `0` for a valid listing regardless of rule levels; `2` for a
missing root or manifest; `3` for an invalid manifest or schema pin. Failures go
to stderr with empty stdout.

```sh
specht rules --root .
specht rules --root . --json
```

## `specht rules set`

```text
specht rules set <id> <level> [--root <DIR>]
```

Sets one rule of the manifest's exact pinned schema version to lower-case
`error`, `warning` or `off`. `--root` defaults to the working directory and
may appear before or after `set`. It is inherited from `specht rules`.

| Option       | Value | Default | Does                 |
| ------------ | ----- | ------- | -------------------- |
| `-h, --help` | none  | Off     | Prints command help. |

An existing entry changes only in its value; a missing entry or `rules` object
is added. Other manifest values and every other file are preserved, including
the schema pin. Existing encoding and BOM are retained. Setting the same level
again leaves the bytes unchanged. The command is offline and prints nothing
on success.

**Exit codes.** `0` on success; `2` for a missing root or manifest; `3` for an
invalid manifest or pin; `4` for an unknown rule id or a level other than the
three exact lower-case words. Root and manifest validation precede argument
validation. Failures go to stderr with empty stdout and leave files unchanged.
An invalid manifest is refused even when the requested edit could repair it.

```sh
specht rules set SPEC010 warning --root .
```

## `specht evidence`

```text
specht evidence [--root <DIR>] [<PATH>...] [--run <ID>] [--json] [--report <PATH>]
```

Reconciles the current claims selected by `.specht/manifest.json`'s `codeAnalysis`
policy against explicitly supplied `specht.test-evidence` exports of `formatVersion` 1, 2 or 3. It
checks declared project and target-framework combinations, current input hashes,
resolved bindings, discovered cases and their selected run outcomes. It is offline
and does not build a project or start tests. The command reads each named file or
JSON files beneath each named directory; it does not search elsewhere for evidence.

| Option            | Value                | Default               | Does                                                    |
| ----------------- | -------------------- | --------------------- | ------------------------------------------------------- |
| `--root <DIR>`    | Repository directory | The working directory | Selects the current consumer configuration and files.   |
| `<PATH>...`       | File or directory    | None                  | Supplies an export; repeat to cover the declared scope. |
| `--run <ID>`      | Exact run identity   | One run per project   | Selects that run wherever it appears.                   |
| `--json`          | none                 | Off                   | Writes only the versioned evidence report to stdout.    |
| `--report <PATH>` | Output file          | None                  | Writes that same JSON report to a caller-named path.    |
| `-h, --help`      | none                 | Off                   | Prints command help.                                    |

The human output separates input, binding, required-link, exemption and execution
counts and lists typed findings with root-relative paths. JSON uses
`format: "specht.evidence-report"` and `formatVersion: 1`; it is separate from the
checker's report. Required test links apply to every selected current claim unless
an exact claim/link has a reasoned exemption. Implementation and scenario links
are required only when enabled in the analysis policy. A link can be satisfied
while its associated case failed or did not run; the gate requires every current
case supplying non-exempt required test evidence to pass. Stale or mismatched
exports remain visible and cannot establish current coverage.

Without `--run`, a run is chosen for each declared project and target framework:
one available run is used, and `ambiguousRun` is reported only for a pair holding
several. `incompleteRun` is informational and leaves the exit code unchanged; it
counts owned, non-planned cases the selected run did not select. An owned
unselected case bound to a required claim is also a `missingResult`, which exits
`1`.
Native SPCT build diagnostics remain in the build's diagnostic stream; the
evidence export does not carry them into this report.

On a clean checkout the sequence that produces evidence is: build, then `dotnet test
--list-tests`, then `dotnet test`, which write `obj/specht-test-evidence/<project>-<framework>.json`.
With none supplied the report is incomplete (exit `2`), and each missing or stale
finding names the step that regenerates it.

Exit `0` means complete current inputs, required links and passing outcomes;
`1` means a required link or outcome failed; `2` means required input is missing,
stale or incompatible; `3` means configuration is invalid; and `4` means a named
evidence path or requested run was not found, or the caller-named report could not
be written. Configuration, explicit selection,
input and policy failures take precedence in that order. Findings still show
other facts that can be established safely.

```sh
specht evidence --root . .artifacts/test-evidence --json --report .artifacts/evidence-report.json
```

## `specht frontmatter`

```text
specht frontmatter --kind <KIND> (<path> | --stdin) [--root <DIR>]
```

Checks only one document's YAML frontmatter against the named kind's schema,
using the version and schema source selected by the root's manifest. The kinds
are `feature`, `task` and `epic`. The root's other documents and the document's
sections, companions and dependencies are not checked. The command is offline
and writes nothing; it has no JSON, report-file or fix option.

| Option          | Value                       | Default               | Does                                                                      |
| --------------- | --------------------------- | --------------------- | ------------------------------------------------------------------------- |
| `--kind <KIND>` | `feature`, `task` or `epic` | Required              | Selects the frontmatter schema kind.                                      |
| `--stdin`       | none                        | Off                   | Reads the complete document from standard input instead of a path.        |
| `--root <DIR>`  | `<DIR>`                     | The working directory | Selects the root whose manifest determines the schema version and source. |
| `-h, --help`    | none                        | Off                   | Prints the command's help and checks nothing.                             |

Give exactly one document path or `--stdin`; a lone `-` is not a stdin alias.
A relative document path is resolved from the working directory, independently
of the root, and may name a file outside the root.

**Stdout.** One MSBuild-shaped line per violation, followed by
`errors: <count>, warnings: <count>`. A path is printed exactly as typed;
standard input is named `<stdin>`. Only SPEC001 to SPEC004 can be reported.
Rule-level overrides are the root's: warnings alone succeed and off rules
report nothing.

**Exit codes.** `0` for no errors, `1` for errors, `2` for usage or missing
root/manifest, `3` for invalid configuration, and `4` for an unknown kind or
missing document. Failures go to stderr with empty stdout. When several apply,
usage is reported first, then kind, then document path, then root and manifest.

```sh
specht frontmatter --kind task drafts/issue.md --root .
cat drafts/issue.md | specht frontmatter --kind task --stdin --root .
```

## `specht init`

```text
specht init [--root <DIR>] [--schemas] [--templates] [--all] [--epics]
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

| Option                        | Value                   | Default               | Does                                                                       |
| ----------------------------- | ----------------------- | --------------------- | -------------------------------------------------------------------------- |
| `--root <DIR>`                | `<DIR>`                 | The working directory | Names the repository root to write under.                                  |
| `--schemas`                   | none                    | Off                   | Adds the selected frontmatter schemas.                                     |
| `--templates`                 | none                    | Off                   | Adds the four templates.                                                   |
| `--all`                       | none                    | Off                   | Adds the frontmatter schemas and templates.                                |
| `--epics`                     | none                    | Off                   | Keeps a new manifest's epic grammar, and the epic schema with the schemas. |
| `--upstream <ADDRESS>`        | HTTPS base address      | None                  | Records a public schema source in a new manifest.                          |
| `--upstream-version <LABEL>`  | Nonempty label          | None                  | Records the upstream version label; required with a source.                |
| `--upstream-hash <KIND=HASH>` | Repeated kind/hash pair | None                  | Supplies a SHA-256 hash for every declared schema kind.                    |
| `-h, --help`                  | none                    | Off                   | Prints the command's help and writes nothing.                              |

**Stdout.** One line per file, relative to the root: `written` for a file it
created, `skipped` for one that was already there. A run that exits `2` or `3` says
why on stderr.

Into a bare root, the selected schemas are Feature and item, and the manifest
declares no epic grammar; `--epics` keeps the grammar, and with `--schemas` or
`--all` adds the epic schema.
Beside an existing manifest, its epic grammar alone decides the epic schema.

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

## `specht schema`

```text
specht schema <COMMAND> [--help]
```

Groups the schema-version commands. Use `specht schema list` to see the versions
this tool ships.

| Option       | Value | Does                      |
| ------------ | ----- | ------------------------- |
| `-h, --help` | none  | Prints the branch's help. |

## `specht schema list`

```text
specht schema list [--root <DIR>]
```

Prints shipped schema versions in ascending semantic-version order. With a
manifest, the selected version has the suffix ` (pinned)`; without a manifest,
the list is unmarked. An omitted `schemaVersion` means `0.1.0`.

| Option         | Value   | Default               | Does                                      |
| -------------- | ------- | --------------------- | ----------------------------------------- |
| `--root <DIR>` | `<DIR>` | The working directory | Names the repository whose pin is marked. |
| `-h, --help`   | none    | none                  | Prints help.                              |

A missing root exits `2`. An unreadable or rejected manifest exits `3`, with
empty stdout and the problem, shipped versions and a recovery hint on stderr.
Listing reads no schema or specification files and fetches nothing.

```text
0.1.0
0.2.0 (pinned)
```

## `specht schema pin`

```text
specht schema pin [version] [--root <DIR>]
```

Pins any shipped version, up or down. Supply the version for scripts; omit it
at an interactive terminal to select a version with the arrow keys and confirm
with Enter. The Spectre.Console selector offers every shipped version.

| Option         | Value   | Default               | Does                                            |
| -------------- | ------- | --------------------- | ----------------------------------------------- |
| `--root <DIR>` | `<DIR>` | The working directory | Names the repository whose manifest is changed. |
| `-h, --help`   | none    | none                  | Prints help.                                    |

Only the root `schemaVersion` value changes. If the key is absent, it is added.
Other manifest bytes, schemas, templates and specification files are preserved.
UTF-8 and BOM-marked UTF-16/UTF-32 manifests retain their original encoding.
The command is offline. A downgrade preserves newer settings; the subsequent
check may reject them until you edit them. Pinning can repair an unsupported or
malformed current pin when the other settings are valid.

**Stdout.** A successful explicit pin prints the selected version and the
relative manifest path. Interactive runs also display the selection prompt.

**Exit codes.** `0` on success; `2` for a missing root or manifest, or no version
without an interactive terminal; `3` for invalid manifest settings; `4` for a
malformed or unshipped target. Failures write nothing and explain the problem
on stderr. Root and manifest validation precede target validation. Without a
terminal, supply a version as shown below.

```sh
specht schema list
specht schema pin 0.2.0
```

```text
pinned schemaVersion 0.2.0 in .specht/manifest.json
```

## `specht upgrade`

```text
specht upgrade [--root <DIR>]
```

Brings the frontmatter schemas and templates under `<DIR>/.specht/` to the bytes
the manifest's pinned version ships. Under `schemaSource` `disk` with no
`spechtSchema` recorded, the schemas are left as they are and listed as
skipped. A template named in `consumerTemplates` is never read or written,
whatever source is selected or recorded, and is listed as skipped. A name
matches a shipped template's file name exactly, case included; a name the
pinned version does not ship has no effect. It rewrites a file that is present and
differs, and leaves a file that is absent absent; `specht init --schemas` or
`--templates` adds a missing part. It adds to the manifest each top-level key
the pinned version's manifest defines and it lacks, other than `schemaVersion`,
`$`-prefixed keys and `identifiers`, so it never opts a repository into epics,
and keeps every key already there, whatever its value. It
never changes a pin, a file the pinned version does not ship, or anything
outside `.specht/manifest.json`, `.specht/schema/frontmatter/` and
`.specht/templates/`.

When the manifest records `spechtSchema`, each present schema whose SHA-256
differs from its recorded hash is fetched from that source and verified, then
written under its `frontmatterSchemas` name. A matching schema is not fetched,
and an absent one stays absent. A failed fetch or hash check writes nothing.
The record itself is never changed. A template the record hashes under
`templates`, and the manifest does not name in `consumerTemplates`, is treated
the same way; one it hashes and the manifest names there is left untouched, and
every template it neither hashes nor the manifest names still comes from the
pinned version.

| Option         | Value   | Default               | Does                                          |
| -------------- | ------- | --------------------- | --------------------------------------------- |
| `--root <DIR>` | `<DIR>` | The working directory | Names the repository root to upgrade.         |
| `-h, --help`   | none    | none                  | Prints the command's help and writes nothing. |

**Stdout.** When nothing differs and no key is missing,
`.specht/ is current at schemaVersion <pin>`. After a rewrite,
`.specht/ rewritten to schemaVersion <pin>`, then `rewritten <path>` for each
rewritten file; `.specht/manifest.json` is listed only when a key was added, and
then first. Then `skipped <path>: <reason>` for each file left alone, in path
order: with `schemaSource` `disk` and no `spechtSchema`, each present frontmatter
schema the pinned version ships; and each template named in `consumerTemplates`
that the pinned version ships, present or not, as
`skipped .specht/templates/<name>: named in consumerTemplates`. A run that only
skips is current. No diff.

**Exit codes.** `0` on success; `2` for a missing root or manifest; `3` for an
invalid manifest, an unshipped pin, or a recorded source that cannot be
reached, refuses, or serves content that fails its hash. Failures explain the
problem on stderr; a source failure names the source's address.
