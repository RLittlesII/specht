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

For the `specht.tool` 0.5.0 release, `dotnet specht init` pins schema `0.3.0`
in a new `.specht/manifest.json`. The package and schema have independent
versions. The sample below uses that schema pin and shows how to change
settings. Specht ignores keys starting with `$`, so they can hold comments.

After running `dotnet specht init --all` in a fresh repository, this example
uses the frontmatter schemas written to disk and checks specifications under
`src/`.

```json
{
  "schemaVersion": "0.3.0",
  "$comment.pinPolicy": "Use the exact pin. Set latest-patch to allow newer shipped patches in the same minor.",
  "pinPolicy": "exact",
  "$comment.layouts": "Find feature specifications beside the code under src/.",
  "layouts": [{ "name": "features", "glob": "src/**/.spec/README.md" }],
  "$comment.exclusions": "This replaces the default list. Bare names match at any depth. A leading slash anchors a path to the repository root.",
  "exclusions": [
    ".git",
    ".artifacts",
    ".skillfile",
    ".claude",
    "graphify-out",
    "bin",
    "obj",
    "node_modules",
    "/.spec",
    "/src/checkout/generated"
  ],
  "$comment.schemaSource": "Read local frontmatter schemas. Omit this setting to use the bundled schemas when there is no recorded upstream.",
  "schemaSource": "disk",
  "$comment.frontmatterSchemas": "Names are relative to .specht/schema/frontmatter/. Rename these entries if you rename the files.",
  "frontmatterSchemas": {
    "feature": "feature.schema.json",
    "task": "task.schema.json"
  },
  "$comment.rules": "Report traceability-row violations as warnings. Rule levels are error, warning, and off.",
  "rules": {
    "SPEC4004": "warning"
  },
  "$comment.matrix": "Check that each traceability row's scenario title resolves to a tagged companion scenario.",
  "matrix": { "scenarioTitles": true },
  "$comment.tests": "Resolve named tests in the traceability matrix against these files. Adjust the glob to your test tree.",
  "tests": { "files": ["test/**/*.cs"] }
}
```

Use `spechtSchema` to share frontmatter schemas across repositories with a
recorded source and hashes. For .NET projects, `codeAnalysis` selects the
projects and features whose code is checked against the specifications. The
Configuration guide in the documentation index has examples of both.

Run `dotnet specht manifest` to see your effective settings, including defaults
for omitted keys. Run `dotnet specht manifest --defaults --json` to see the
manifest for the newest schema shipped by your installed tool.

## Documentation

[Browse the documentation](docs/README.md).

## License

[MIT](LICENSE.md) © 2026 Rodney Littles, II. A personal project, published
openly.
