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

Requires the .NET 10 SDK. `specht.tool` is published on
[nuget.org](https://www.nuget.org/packages/specht.tool). No package source entry
is committed for it and no credential is needed to install or restore it.

Every version below `1.0.0` is a prerelease, so each install command below
opts in to prereleases.

For a new repository, create the manifest and install locally:

```sh
dotnet new tool-manifest --output .config
dotnet tool install --local specht.tool --prerelease
dotnet specht init
dotnet specht init --all
dotnet specht
```

Commit `.config/dotnet-tools.json` to pin the tool version. A clone, or CI,
runs:

```sh
dotnet tool restore
dotnet specht
```

A local tool manifest install runs as `dotnet specht`.

For a workstation shell outside a repository, install globally:

```sh
dotnet tool install --global specht.tool --prerelease
specht init
specht
```

A global install puts `specht` on PATH; ensure the SDK's global tools directory
is on your shell's PATH. The local manifest is the install path for a repository
and its build; a global install is a second path for a shell.

## Usage

```sh
specht [--root <dir>] [--strict] [--json] [--schema-source <embedded|disk>] [--explain <SPEC###>]
specht schema list [--root <dir>]
specht schema pin [version] [--root <dir>]
specht rules [--root <dir>] [--json]
specht rules set <id> <level>
specht manifest [--root <dir>] [--defaults] [--json]
specht frontmatter --kind <kind> (<path> | --stdin) [--root <dir>]
specht evidence [--root <dir>] [<path>...] [--run <id>] [--json] [--report <path>]
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

Every Feature is specified in a `.spec/README.md` beside the code it covers, such
as `src/specht/Rules/Form/.spec/`, with its `.feature` file. Its work items sit in
a `.issue/` beside that `.spec/`.

## Documentation

- [Setup](docs/setup.md)
- [Command line](docs/cli.md)
- [What it checks](docs/what-it-checks.md)
- [Configuration](docs/configuration.md)
- [Multiple projects](docs/multiple-projects.md)
- [Without .NET code](docs/non-dotnet.md)
- [Exit codes](docs/exit-codes.md)
- [Building](docs/building.md)
- [Roadmap](docs/roadmap.md)
- [Rule pages](docs/rules/v0.1.0/)

## License

[MIT](LICENSE.md) © 2026 Rodney Littles, II. A personal project, published
openly; issues and pull requests are not being sought.
