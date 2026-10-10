<p align="center">
  <img src=".assets/logos/specht-book-logo.png" width="180" alt="specht" />
</p>

<h1 align="center">specht</h1>

<p align="center">
  A <code>dotnet tool</code> that checks a repository's <code>.spec/</code> tree against a versioned schema.
</p>

<p align="center">
  <a href="https://github.com/RLittlesII/specht/actions/workflows/ci.yml"><img src="https://github.com/RLittlesII/specht/actions/workflows/ci.yml/badge.svg" alt="ci" /></a>
  <img src="https://img.shields.io/badge/.NET-10-512BD4?style=flat&logo=dotnet&logoColor=white" alt=".NET 10" />
  <img src="https://img.shields.io/badge/schema-v1-1e1e1e?style=flat" alt="schema v1" />
  <img src="https://img.shields.io/badge/status-pre--release-orange?style=flat" alt="pre-release" />
  <a href="LICENSE.md"><img src="https://img.shields.io/badge/license-MIT-blue?style=flat" alt="MIT" /></a>
</p>

<p align="center">
  <a href="#quick-start">Quick start</a> •
  <a href="#usage">Usage</a> •
  <a href="#exit-codes">Exit codes</a> •
  <a href="#layout">Layout</a> •
  <a href="#build">Build</a> •
  <a href="#status">Status</a> •
  <a href="#license">License</a>
</p>

---

Every violation comes back with a file, a line and a rule id.

```text
src/specht/.spec/README.md(214): error SPEC031: § 9 has no row for claim B-007
```

`specht` checks frontmatter against JSON Schema, the contracted sections and
their order, claim ids, one traceability row per claim, `.feature` tags that
resolve to claims, and dependency edges that agree from both ends. Rules are
`SPEC001`–`SPEC061`
([brief § 4](.spec/brief.md#4-the-rule-vocabulary-fixed-versioned-with-the-schema)),
and each finding says what the rule expected.

Deterministic and offline: the same tree gives the same report, and every path
is relative to the root.

**Why:** one schema and one checker, not a hand-copied set per repository. A
repository pins `schemaVersion`, its build calls the tool, and it upgrades when
it chooses.

---

## Quick start

> Not published yet. Install steps land with the first release.

```sh
dotnet tool install specht.tool   # into the repository's local tool manifest
specht init                       # write .spec/schema and .spec/templates
specht                            # check the tree
```

## Usage

```sh
specht [--root <dir>] [--report <path>] [--strict] [--json]
specht init [--root <dir>]
specht upgrade [--root <dir>]
specht --explain SPEC031
```

| Command                 | Does                                                                                          |
| ----------------------- | --------------------------------------------------------------------------------------------- |
| `specht`                | Check the tree. One MSBuild-shaped line per violation, then a summary.                        |
| `--json`                | Print the report document instead, with what each rule expected.                              |
| `--report <path>`       | Also write the JSON report to a path.                                                         |
| `--strict`              | Fail on any violation, of any severity.                                                       |
| `specht init`           | Write the schema set and templates into `<root>/.spec/`. Never overwrites.                    |
| `specht upgrade`        | Bring `.spec/schema/` and `.spec/templates/` to the pinned version; list each file rewritten. |
| `specht --explain <id>` | Print a rule's full text.                                                                     |

One file configures it: `.spec/schema/spec-structure.schema.json`, which pins
`schemaVersion` - a `major.minor.patch` schema version, numbered apart from the
tool's own. A patch changes no verdict, a minor only accepts more, and a major
can fail a tree that passed. The pin is exact; a manifest with no
`schemaVersion` is `0.1.0`.

The schema is per repository however the tool is installed. `specht` reads the
manifest from the root it is given, embeds every schema version it ships, and
checks with the pinned one, so one install can check a `0.1.0` repository and
a `1.0.0` one. A pin the tool does not ship exits `3`, naming the pin and the versions it
ships. Install through the local tool manifest anyway: it pins the tool's
version beside `schemaVersion`, so CI and every clone run the same release. A
global install is not the documented path.

## Exit codes

| Exit | Meaning                                                               |
| ---- | --------------------------------------------------------------------- |
| `0`  | Clean                                                                 |
| `1`  | Violations (any violation, under `--strict`)                          |
| `2`  | Missing root or manifest                                              |
| `3`  | Invalid manifest                                                      |
| `4`  | Something named on the command line, such as a rule id, was not found |

## Layout

```text
specht/
├── .build/                 # NUKE build
├── .issue/                 # work items with no Feature; ids from .issue/.sequence
├── .performance/           # BenchmarkDotNet benchmarks
├── .spec/
│   ├── adr/                # repo-wide decision records
│   ├── lessons/            # repo-wide lessons
│   ├── schema/             # schema v1, the same files specht init writes
│   └── templates/          # blanks for specifications, records and work items
├── docs/                   # report schema and documentation site
├── src/
│   ├── specht/             # engine: discovery, frontmatter, schemas, document model, rules, report
│   └── specht.tool/        # the specht command, one folder per command under Features/
└── test/
    ├── specht.acceptance/  # Reqnroll scenarios
    └── specht.tests/       # unit and integration tiers
```

Every Feature is specified in a `.spec/README.md` beside the code it covers, such
as `src/specht/Rules/Form/.spec/`, with its `.feature` file. Its work items sit in
a `.issue/` beside that `.spec/`.

## Build

Requires the .NET SDK pinned in [`global.json`](global.json).

```sh
dotnet tool restore
./build.sh             # compile and test
./build.sh Pack        # specht.tool.<version>.nupkg
./build.sh SpecCheck   # run specht on this repository's own specifications
```

## Status

**Pre-release.** Work follows [brief § 8](.spec/brief.md#8-order-of-work):

1. ✅ Scaffold — solution, central package versions, NUKE, CI.
2. ✅ Copy the rule engine and its tests from `hooked`.
3. 🚧 Command line — `specht` with `--root` and `--strict` ships; `init`, `--json`, `--report` and `--explain` next.
4. `hooked` consumes the tool.
5. Move every hardcoded literal into the manifest.
6. Schema versioning and `specht upgrade`.
7. Install in Transporter — done.

## License

[MIT](LICENSE.md) © 2026 Rodney Littles, II. A personal project, published
openly; issues and pull requests are not being sought.
