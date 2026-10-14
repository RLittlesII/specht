<p align="center">
  <picture>
    <source media="(prefers-color-scheme: dark)" srcset=".assets/logos/specht-book-logo-dark.svg" />
    <source media="(prefers-color-scheme: light)" srcset=".assets/logos/specht-book-logo-light.svg" />
    <img src=".assets/logos/specht-book-logo-light.svg" width="180" alt="specht" />
  </picture>
</p>

<h1 align="center">specht</h1>

<p align="center">
  A <code>dotnet tool</code> that checks a repository's <code>.spec/</code> tree against a versioned schema.
</p>

<p align="center">
  <a href="https://github.com/RLittlesII/specht/actions/workflows/ci.yml"><img src="https://github.com/RLittlesII/specht/actions/workflows/ci.yml/badge.svg" alt="ci" /></a>
  <img src="https://img.shields.io/badge/.NET-10-512BD4?style=flat&logo=dotnet&logoColor=white" alt=".NET 10" />
  <img src="https://img.shields.io/badge/schema-v0.1.0-1e1e1e?style=flat" alt="schema v0.1.0" />
  <img src="https://img.shields.io/badge/status-pre--release-orange?style=flat" alt="pre-release" />
  <a href="LICENSE.md"><img src="https://img.shields.io/badge/license-MIT-blue?style=flat" alt="MIT" /></a>
</p>

<p align="center">
  <a href="#quick-start">Quick start</a> •
  <a href="#usage">Usage</a> •
  <a href="#layout">Layout</a> •
  <a href="#documentation">Documentation</a> •
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
resolve to claims, and dependency edges that agree from both ends.

Deterministic and offline: the same tree gives the same report, and every path
is relative to the root.

**Why:** one schema and one checker, not a hand-copied set per repository. A
repository pins `schemaVersion`, its build calls the tool, and it upgrades when
it chooses.

---

## Quick start

> Published on [GitHub Packages](https://github.com/RLittlesII/specht/pkgs/nuget/specht.tool). Package reads require authentication.

```sh
dotnet tool install specht.tool      # into the repository's local tool manifest
dotnet specht init                   # write .specht/manifest.json
dotnet specht init --all             # add the frontmatter schemas and templates
dotnet specht                        # check the tree
```

```sh
dotnet tool install -g specht.tool   # puts specht on PATH
specht init
specht
```

## Usage

```sh
specht [--root <dir>] [--strict] [--json] [--schema-source <embedded|disk>]
specht schema list [--root <dir>]
specht schema pin [version] [--root <dir>]
specht init [--root <dir>] [--schemas] [--templates] [--all]
            [--upstream <address>] [--upstream-version <label>] [--upstream-hash <kind=hash>]...
specht --version
```

Every command and option, what each prints and the exit codes it returns:
[docs/cli.md](docs/cli.md).

One file configures it: `.specht/manifest.json`, which can
pin `schemaVersion` - a `major.minor.patch` schema version, numbered apart from
the tool's own. A patch changes no verdict; from schema `1.0.0` on a minor only
accepts more and a major can fail a tree that passed, and before it a `0.minor`
step can fail one too. The pin is exact; a manifest without the key
is read as version `0.1.0`.

A build and CI call the tool through the local tool manifest, which pins the
tool's version beside `schemaVersion` so every clone runs the same release. A
global install puts `specht` on PATH for a shell and is not pinned per
repository.

## Layout

```text
specht/
├── .build/                 # NUKE build
├── .issue/                 # work items with no Feature; ids from .issue/.sequence
├── .performance/           # BenchmarkDotNet benchmarks
├── .spec/
│   ├── adr/                # repo-wide decision records
│   ├── lessons/            # repo-wide lessons
│   └── templates/          # the work-item blank
├── .specht/                # the tool's configuration, holding the files specht init can write
│   ├── manifest.json       # the manifest
│   ├── schema/frontmatter/ # the frontmatter schemas of schema v0.1.0
│   └── templates/          # blanks for specifications and records
├── docs/                   # reference pages, rule pages, report schema and documentation site
├── src/
│   ├── specht/             # engine, foldered by Feature over a shared model: discovery, manifest, report, versioning, rules
│   └── tool/               # the specht command, one folder per command under Features/
└── test/
    ├── acceptance/         # Reqnroll scenarios
    └── tests/              # unit and integration tiers, in folders that mirror src/
```

Every Feature is specified in a `.spec/README.md` beside the code it covers, such
as `src/specht/Rules/Form/.spec/`, with its `.feature` file. Its work items sit in
a `.issue/` beside that `.spec/`.

## Documentation

- [Setup](docs/setup.md)
- [Command line](docs/cli.md)
- [What it checks](docs/what-it-checks.md)
- [Configuration](docs/configuration.md)
- [Multiple projects](docs/multiple-projects.md)
- [Exit codes](docs/exit-codes.md)
- [Building](docs/building.md)
- [Rule pages](docs/rules/v0.1.0/)

## License

[MIT](LICENSE.md) © 2026 Rodney Littles, II. A personal project, published
openly; issues and pull requests are not being sought.
