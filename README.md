# specht

A `dotnet tool` that checks a repository's `.spec/` specification tree against a
versioned schema, and reports every violation with a file, a line, and a rule id.

```text
src/specht/.spec/README.md(214): error SPEC031: § 9 has no row for claim B-007
```

## What

`specht` reads the Markdown specifications under a repository's `.spec/`
folders and proves each one is written and formatted the way the pinned schema
says: YAML frontmatter that validates against its JSON Schema, the contracted
sections in order, well-formed claim ids, a traceability matrix with exactly one
row per claim, `.feature` scenario tags that resolve to claims, and dependency
edges that resolve and agree from both ends.

Every finding is a fixed rule id — `SPEC001` through `SPEC061` — listed in
[brief § 4](.spec/brief.md#4-the-rule-vocabulary-fixed-versioned-with-the-schema).
Each violation also says what the rule expected, so the document can be
repaired from the report alone.

The check is deterministic and offline: the same tree gives the same report,
and the check never touches the network. Paths in every output are relative to
the root it was given.

## Why

Several repositories are written on the same `.spec/` approach, and each was
carrying a hand-copied schema, set of templates, and checker. Every improvement
had to land in each copy or drift.

`specht` is the one schema and the one checker. A repository pins a
`schemaVersion`, its build calls the tool, and no repository carries the engine.
Every schema version the tool has shipped stays embedded in it, so a repository
upgrades when it chooses — lagging is deliberate, not drift.

## Who

Written and maintained by Rodney Littles II for his own repositories. `hooked` is
the first consumer; Transporter, whose tree predates the schema, is the second.

This is a personal project. It is published openly, but issues and pull requests
are not being sought.

## When

**Pre-release. Nothing is published yet.** Work follows the order in
[brief § 8](.spec/brief.md#8-order-of-work):

1. Scaffold — solution, central package versions, NUKE build, CI. _In progress._
2. Copy the rule engine and its tests from `hooked`, renamed and otherwise unchanged.
3. The command line — exit codes, `--json`, `--report`, `init`; pack and self-host.
4. `hooked` consumes the tool in place of its own engine.
5. Move every hardcoded literal into the manifest, one rule at a time.
6. Schema versioning and `specht upgrade`.
7. Install in Transporter — "done".

## Where

- **Package:** `specht.tool`, on GitHub Packages under `rlittlesii/specht`, once
  the first version is published.
- **Engine:** [`src/specht`](src/specht) — discovery, frontmatter, schemas, the
  document model, the rules, the report.
- **Command line:** [`src/specht.tool`](src/specht.tool) — the `specht` command.
- **Schema version 1 and templates:** [`.spec/schema`](.spec/schema) and
  [`.spec/templates`](.spec/templates) — this repository's own copy, the same
  files `specht init` writes into a consumer.
- **Design:** [`.spec/brief.md`](.spec/brief.md) is the design authority; each
  part of the tool is specified in the `.spec/README.md` beside its code.

## How

### Install

Not yet available: the package has not been published. Install instructions —
the package source, the read token, and the local tool manifest commands — are
added here with the first release.

### Use

```sh
specht [--root <dir>] [--report <path>] [--strict] [--json]
specht init [--root <dir>]
specht upgrade [--root <dir>]
specht --explain SPEC031
```

| Command                 | Does                                                                                                                       |
| ----------------------- | -------------------------------------------------------------------------------------------------------------------------- |
| `specht`                | Checks the tree. One MSBuild-shaped line per violation, then a summary.                                                    |
| `--json`                | Replaces the output with the report document, including what each rule expected.                                           |
| `--report <path>`       | Also writes the JSON report to a path.                                                                                     |
| `--strict`              | Any violation, of any severity, fails the run.                                                                             |
| `specht init`           | Writes the schema set and templates into `<root>/.spec/`. Never overwrites a file.                                         |
| `specht upgrade`        | Moves `.spec/schema/` and `.spec/templates/` to the next schema version and prints what changed. Never touches a document. |
| `specht --explain <id>` | Prints a rule's full text.                                                                                                 |

| Exit | Meaning                                                               |
| ---- | --------------------------------------------------------------------- |
| `0`  | Clean                                                                 |
| `1`  | Violations (any violation, under `--strict`)                          |
| `2`  | Missing root or manifest                                              |
| `3`  | Invalid manifest                                                      |
| `4`  | Something named on the command line, such as a rule id, was not found |

A consumer configures the tool with one file,
`.spec/schema/spec-structure.schema.json`, which pins `schemaVersion`.

### Build from source

Requires the .NET SDK pinned in [`global.json`](global.json).

```sh
dotnet tool restore
./build.sh             # compile and test
./build.sh Pack        # specht.tool.<version>.nupkg
./build.sh SpecCheck   # run specht on this repository's own specifications
```

## License

[MIT](LICENSE.md) © 2026 Rodney Littles, II
