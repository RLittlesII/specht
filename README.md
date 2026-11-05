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
  <a href="https://www.nuget.org/packages/specht.tool"><img src="https://img.shields.io/nuget/vpre/specht.tool?logo=nuget&amp;label=NuGet" alt="specht.tool on NuGet" /></a>
  <a href="https://github.com/RLittlesII/specht/actions/workflows/ci.yml"><img src="https://github.com/RLittlesII/specht/actions/workflows/ci.yml/badge.svg" alt="CI" /></a>
  <a href="LICENSE.md"><img src="https://img.shields.io/badge/license-MIT-blue?style=flat" alt="MIT" /></a>
</p>

Specht checks Markdown frontmatter and structure, matches Gherkin tags and
traceability rows to claims, and verifies that feature dependencies agree in
both directions. Checks run offline and report each violation with a
repository-relative path, a line number, and a rule ID.

```text
src/checkout/.spec/README.md(42): error SPEC4004: claim 'B-007' has no row in § 9 - every § 3 claim appears there exactly once, with 'Missing' as its test until one exists [B-007]
```

## Quick start

Install the .NET 10 SDK, then run these commands from your repository root.
If you already have a local tool manifest, skip the first command.

```sh
dotnet new tool-manifest --output .config
dotnet tool install --local specht.tool --prerelease
dotnet specht init --all
```

Copy `.specht/templates/feature.md` to `src/checkout/.spec/README.md` or another
feature directory. Fill in the specification and put its `.feature` file beside it.

Commit `.config/dotnet-tools.json` to pin the tool version, along with your
`.specht/` configuration and specifications. Run the same check locally and in CI.

```sh
dotnet tool restore
dotnet specht
```

## Layout

A repository using the local tool and `dotnet specht init --all` might look like this.

```text
your-repository/
├── .config/
│   └── dotnet-tools.json
├── .specht/
│   ├── manifest.json
│   ├── schema/
│   │   └── frontmatter/
│   │       ├── feature.schema.json
│   │       └── task.schema.json
│   └── templates/
│       ├── adr.md
│       ├── decision.md
│       ├── feature.md
│       └── lesson.md
└── src/
    └── checkout/
        └── .spec/
            ├── README.md
            └── checkout.feature
```

`.specht/` holds the checker configuration and scaffolding. The `.spec/`
folders hold the specifications you write. Plain `dotnet specht init` writes
only the manifest. Add `--epics` to `init --all` on a fresh setup to include
the epic schema and its manifest settings.

## Manifest

This example is a complete `.specht/manifest.json` for a tool that ships
schema `0.3.0`. Keys starting with `$` are comments that Specht ignores.

```json
{
  "$comment.schemaVersion": "Selects the schema and rule IDs independently of the installed tool version.",
  "schemaVersion": "0.3.0",
  "$comment.pinPolicy": "Use the exact pin. Use latest-patch to allow newer shipped patches in the same minor.",
  "pinPolicy": "exact",
  "$comment.schemaSource": "Use the tool's bundled frontmatter schemas. Set disk to read schemas from .specht/schema/frontmatter/.",
  "schemaSource": "embedded",
  "$comment.rules": "Report traceability-row violations as warnings. Levels are error, warning, and off. Omitted rules keep their default levels.",
  "rules": {
    "SPEC4004": "warning"
  }
}
```

Run `dotnet specht schema list` to see the schema versions your installed tool
ships. Run `dotnet specht manifest` to see the settings in effect, including
defaults filled in for omitted keys.

## Documentation

[Browse the documentation](docs/README.md).

## License

[MIT](LICENSE.md) © 2026 Rodney Littles, II. A personal project, published
openly.
