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
[GitHub Packages](https://github.com/RLittlesII/specht/pkgs/nuget/specht.tool).
Package reads require authentication, including for this public package.

Commit this `nuget.config` in the consumer repository. The named `specht`
source supplies the tool; nuget.org supplies other packages and audit data.
Merge these entries into an existing configuration if the repository has one.

```xml
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="specht" value="https://nuget.pkg.github.com/rlittlesii/index.json" />
  </packageSources>
  <packageSourceMapping>
    <clear />
    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
    <packageSource key="specht">
      <package pattern="specht.tool" />
    </packageSource>
  </packageSourceMapping>
  <auditSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </auditSources>
</configuration>
```

Use a GitHub personal access token (classic) with `read:packages` and access to
the package. Supply your GitHub username as `GITHUB_USER` and the token as
`SPECHT_PACKAGES_TOKEN` through your shell's environment, then export NuGet's
credential for the source named `specht`:

```sh
export NuGetPackageSourceCredentials_specht="Username=$GITHUB_USER;Password=$SPECHT_PACKAGES_TOKEN;ValidAuthenticationTypes=Basic"
```

Keep the token in the runtime environment; commit no credential. See
[GitHub's authentication requirements](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry)
and [NuGet's environment credential format](https://learn.microsoft.com/en-us/nuget/consume-packages/consuming-packages-authenticated-feeds).

For a new repository, create the manifest and install locally:

```sh
dotnet new tool-manifest --output .config
dotnet tool install --local specht.tool
dotnet specht init
dotnet specht init --all
dotnet specht
```

Commit `.config/dotnet-tools.json` alongside `nuget.config` to pin the tool
version. A clone supplies the same environment credential, then runs:

```sh
dotnet tool restore
dotnet specht
```

A local tool manifest install runs as `dotnet specht`.

For CI, store the read token in the consumer repository's
`SPECHT_PACKAGES_TOKEN` secret, store its owner's username in the
`SPECHT_PACKAGES_USER` repository variable, and supply NuGet's credential as a
job environment variable. The token's owner must have package access:

```yaml
env:
  NuGetPackageSourceCredentials_specht: Username=${{ vars.SPECHT_PACKAGES_USER }};Password=${{ secrets.SPECHT_PACKAGES_TOKEN }};ValidAuthenticationTypes=Basic
```

This repository's own workflows use `${{ github.token }}` with `packages: read`
instead of a stored read token. Another repository can use its workflow token
only when it has been granted read access to the package.

For a workstation shell outside a repository, use the same environment
credential and register the named source in user-level NuGet configuration.
Run this once, unless `dotnet nuget list source` already lists `specht`:

```sh
dotnet nuget add source https://nuget.pkg.github.com/rlittlesii/index.json --name specht
dotnet tool install --global specht.tool --add-source https://nuget.pkg.github.com/rlittlesii/index.json
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
