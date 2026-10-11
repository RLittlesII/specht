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
  <a href="#what-it-checks">What it checks</a> •
  <a href="#exit-codes">Exit codes</a> •
  <a href="#layout">Layout</a> •
  <a href="#build">Build</a> •
  <a href="#status">Status</a> •
  <a href="#license">License</a>
</p>

---

Every violation comes back with a file, a line and a rule id.

```text
src/specht/.spec/README.md(214): error SPEC031: § 9 has no row for claim B-007 [B-007]
```

A line ends with the identifier it is about in square brackets, and at the
message when the violation has none.

`specht` checks frontmatter against JSON Schema, the contracted sections and
their order, claim ids, one traceability row per claim, `.feature` tags that
resolve to claims, and dependency edges that agree from both ends. Rules are
`SPEC001`–`SPEC061`
([brief § 4](.spec/brief.md#4-the-rule-vocabulary-fixed-versioned-with-the-schema)
summarises them; each has a page under
[`docs/rules/v1/`](docs/rules/v1/)), and each finding says
what the rule expected.

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

One file configures it: `.spec/schema/spec-structure.schema.json`, which can
pin `schemaVersion` - a `major.minor.patch` schema version, numbered apart from
the tool's own. A patch changes no verdict, a minor only accepts more, and a
major can fail a tree that passed. The pin is exact; a manifest without the key
is read as version `0.1.0`.

The schema is per repository however the tool is installed. `specht` reads the
manifest from the root it is given, embeds every schema version it ships, and
checks with the pinned one, so one install can check a `0.1.0` repository and
a `1.0.0` one. A pin the tool does not ship exits `3`, naming the pin and the versions it
ships. Install through the local tool manifest anyway: it pins the tool's
version beside `schemaVersion`, so CI and every clone run the same release. A
global install is not the documented path.

## What it checks

A specification is a `<area>/.spec/README.md` beside the code it covers, with
its `.feature` file in the same folder. The repository-root `.spec/` holds the
schema, the templates and repo-wide records, and is never checked as a
specification. No rule reads `adr/`, `lessons/` or `decisions/`. A `.spec/`
under `.git`, `.artifacts`, `.claude`, `.skillfile`, `graphify-out`, `bin`,
`obj` or `node_modules` is skipped.

| Rules                                      | Checks                                                                                                                                                                                              |
| ------------------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `SPEC001`–`SPEC003`                        | Frontmatter. Every specification and item file opens with YAML frontmatter that satisfies its schema.                                                                                               |
| `SPEC010`, `SPEC013`                       | Sections. Each contracted section is present once and in order, and a table the manifest names has the headers it names.                                                                            |
| `SPEC012`                                  | Identity. No `<epic>-<id>` is claimed by two specifications.                                                                                                                                        |
| `SPEC020`, `SPEC021`                       | Companion. Exactly one `.feature` sits beside the specification, and every claim tag in it resolves to a § 3 claim.                                                                                 |
| `SPEC030`, `SPEC031`                       | Claims. Each § 3 claim id matches the claim grammar and is unique in its specification, and § 9 has exactly one row per claim and none for anything else.                                           |
| `SPEC040`, `SPEC041`, `SPEC043`, `SPEC044` | Items. Each id under `children` or `spikes` resolves to one item file, an item's `id` and `parent` agree with its file name and its specification, and item ids are unique and contiguous per epic. |
| `SPEC050`–`SPEC052`                        | Dependencies. Every `depends_on` and `blocks` edge names a discovered specification, is declared from both ends, and forms no cycle.                                                                |
| `SPEC060`, `SPEC061`                       | Approval. A specification with `spec_status: approved` has no `Missing` cell in § 9 and no draft or blocked row in § 12.                                                                            |

§ 3, § 9 and § 12 are the sections the manifest's `roles` name: by default
`3. Acceptance Criteria`, `9. Traceability Matrix` and `12. Sign-off`. A
`Missing` cell in § 9 is not a violation until the specification is approved.

### Work items are optional

`specht` never reads `.issue/`. The item rules run only on
`<epic>-<nn>-<slug>.md` files in a specification's folder, and `SPEC040` and
`SPEC041` only when a specification's frontmatter lists an id under `children`
or `spikes`. A repository with specifications, companions and a manifest, and no
work items, gets every other check.

### What is configurable

| Manifest key        | Read today                                                                                                                                                                                          |
| ------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `schemaVersion`     | Which embedded version checks the tree: its frontmatter schemas and its rule vocabulary.                                                                                                            |
| `layouts`           | Where specifications are found: an ordered list, each entry a name and a path glob, which discovery matches and the summary and the report name each entry by.                                      |
| `exclusions`        | The directories discovery never looks in: a bare name at any depth, or a path from the root when the entry begins with `/`.                                                                         |
| `taskFiles`         | The file-name shapes of an item beside a specification, a list of globs, `{task}` standing for `identifiers.task`. A file matching any one is an item the item rules read; an empty list exits `3`. |
| `epicFiles`         | The path globs of an epic file, a list. A file matching any one is an epic file; an empty list exits `3`.                                                                                           |
| `companionFiles`    | The file-name globs of a companion beside a specification, a list. A file matching any one is a companion `SPEC020` and `SPEC021` read; an empty list exits `3`.                                    |
| `sections`          | The `##` headings `SPEC010` requires, in order.                                                                                                                                                     |
| `tables`            | The header cells `SPEC013` expects in a role's section, by role name; a key that is not a role exits `3`.                                                                                           |
| `roles`             | The `sections` title that holds the claims, the matrix and the sign-off, which `SPEC013`, `SPEC021`, `SPEC030`, `SPEC031`, `SPEC060` and `SPEC061` look up.                                         |
| `markers`           | The text of a missing-test cell, a draft and a blocked sign-off row and the approved status, which `SPEC060` and `SPEC061` look for.                                                                |
| `identifiers.claim` | The claim id grammar `SPEC030` enforces in § 3, and the § 3 ids a tag may resolve to under `SPEC021`.                                                                                               |

A key left out takes its value from the default manifest. A key the engine does
not know exits `3`; a key starting with `$` is ignored. The other `identifiers`
entries are accepted and no rule reads them yet. The frontmatter schemas a check
uses are the tool's embedded copy of the pinned version, not the files under
`.spec/schema/`.

Claim tags in a `.feature` file are read as `@B-nnn`, with an optional
lowercase letter suffix, whatever `identifiers.claim` says. A configurable tag
form and letter case are specified
([`src/specht/Manifest/.spec/README.md`](src/specht/Manifest/.spec/README.md)
B-007, B-036–B-038) and not built yet. The frontmatter keys the rules read are
still literals in the engine; they move into the manifest in
[brief § 8](.spec/brief.md#8-order-of-work) step 5.

### Default manifest

It lives at `<root>/.spec/schema/spec-structure.schema.json`. `specht init`
writes it when the file is absent. The block leaves out the five discovery keys
(`layouts`, `exclusions`, `taskFiles`, `epicFiles` and `companionFiles`): the
file carries each at its default, and a manifest without them is read the same
way.

<!-- prettier-ignore -->
```json
{
  "$comment": "Not a JSON Schema. The ordered section manifest Specht rule SPEC010 reads, kept beside the schemas so the section contract - the twelve numbered sections plus Tasks and Scoring - lives in one place rather than as a string array in C#.",
  "sections": [
    "1. Business Goal",
    "2. User Needs",
    "3. Acceptance Criteria",
    "4. Constraints",
    "5. Out of Scope",
    "6. Concern Separation",
    "7. Technical Design",
    "8. Testing Strategy",
    "9. Traceability Matrix",
    "10. Lessons / Spec Deltas",
    "11. Open Questions",
    "12. Sign-off",
    "Tasks",
    "Scoring"
  ],
  "tables": {
    "matrix": [
      "Claim ID",
      "Scenario",
      "Test",
      "Status"
    ]
  },
  "identifiers": {
    "claim": "^B-[0-9]{3}[a-z]?$",
    "constraint": "^C-[0-9]+$",
    "openQuestion": "^OQ-[0-9]+$",
    "task": "^[0-9]{4}-[0-9]{2}$",
    "feature": "^F[0-9]+[a-z]?$",
    "epic": "^[0-9]{4}$"
  },
  "frontmatterSchemas": {
    "feature": "feature-spec.frontmatter.schema.json",
    "task": "task.frontmatter.schema.json",
    "epic": "epic.frontmatter.schema.json"
  },
  "roles": {
    "claims": "3. Acceptance Criteria",
    "matrix": "9. Traceability Matrix",
    "signOff": "12. Sign-off"
  },
  "markers": {
    "missing": "Missing",
    "draft": "🟡",
    "blocked": "🔴",
    "approved": "approved"
  }
}
```

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
│   └── tool/               # the specht command, one folder per command under Features/
└── test/
    ├── acceptance/         # Reqnroll scenarios
    └── tests/              # unit and integration tiers
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
./build.sh Specht   # run specht on this repository's own specifications
```

## Status

**Pre-release.** Work follows [brief § 8](.spec/brief.md#8-order-of-work):

1. ✅ Scaffold — solution, central package versions, NUKE, CI.
2. ✅ Copy the rule engine and its tests from `hooked`.
3. 🚧 Command line — `specht` with `--root`, `--strict` and `--json`, and `specht init`, ship; `--report` and `--explain` next.
4. `hooked` consumes the tool.
5. Move every hardcoded literal into the manifest.
6. Schema versioning and `specht upgrade`.
7. Install in Transporter — done.

## License

[MIT](LICENSE.md) © 2026 Rodney Littles, II. A personal project, published
openly; issues and pull requests are not being sought.
