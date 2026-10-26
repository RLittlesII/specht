---
title: "specht in a repository with no .NET code"
description: "What the check reads, what the runner needs, and which parts of the tool need a .NET project"
type: reference
---

# A repository with no .NET code

The check reads Markdown, YAML and Gherkin. No rule reads source code, so a
Flutter, Go or TypeScript repository that writes specifications with the
shipped template and `.feature` files gets every rule. The .NET SDK is needed
to run the tool and for nothing else.

## Install

The SDK on the machine is the one .NET requirement. Pin the tool per
repository with a local tool manifest, or put it on PATH for a shell
([Setup](setup.md)):

```sh
dotnet new tool-manifest            # once
dotnet tool install specht.tool
dotnet specht init --all
```

In CI, install the SDK before the restore. The tool restores from the manifest
like any other local tool:

```yaml
- uses: actions/setup-dotnet@v4
- run: dotnet tool restore
- run: dotnet specht --strict
```

## What runs

Every rule. [What it checks](what-it-checks.md) lists the groups; none reads a
project file, an assembly or a test runner.

| Input        | Read by                                                                                    |
| ------------ | ------------------------------------------------------------------------------------------ |
| Frontmatter  | `SPEC001`–`SPEC003`, against the pinned schema or your own under `.specht/schema/`         |
| Sections     | `SPEC010`, `SPEC013`: the twelve sections in order, the table headers the manifest names   |
| Claims       | `SPEC012`, `SPEC030`, `SPEC031`: ids unique, § 9 one row per claim                         |
| `.feature`   | `SPEC020`, `SPEC021`: one companion beside the specification, every claim tag resolves     |
| Dependencies | `SPEC050`–`SPEC052`: edges declared from both ends, no cycle                               |
| Approval     | `SPEC060`, `SPEC061`: an approved specification has no `Missing` cell and no open sign-off |

The companion is any Gherkin file. A runner such as `flutter_gherkin` or
`bdd_widget_test` reads the same `.feature` the check reads; the check needs
only the claim tags. Under schema `0.2.0`, `claim.tag` in the manifest changes
the tag shape, `@req:{claim}` for `@req:B-001`
([Configuration](configuration.md)).

## Layout

A specification sits beside the code it covers, in any tree:

```text
lib/checkout/.spec/README.md
lib/checkout/.spec/checkout.feature
```

`.issue/` is never read. The item rules run only on item files beside a
specification, and only when its frontmatter lists them, so a repository that
tracks work elsewhere gets every other check.

## Test names

A § 9 Test cell is text in inline code. Under schema `0.2.0` the manifest
accepts `tests` and `matrix`, and no rule reads them yet; `0001-F12`, in
milestone 0.4.0 ([Roadmap](roadmap.md)), resolves each name against the files
`tests.files` matches in a form `tests.declarations` gives. For Dart:

```json
"tests": {
  "files": ["test/**/*_test.dart", "integration_test/**/*.dart"],
  "declarations": ["test('{test}'", "testWidgets('{test}'"]
}
```

Until then a Test cell is checked for shape, not for the test existing.

## Not supported

- `specht evidence`. It reconciles `specht.test-evidence` exports against the
  bindings the analyzer reads from C# attributes; with no bindings there is
  nothing to reconcile ([Command line](cli.md#specht-evidence)).
- The `Specht.CodeAnalysis` package and its `SPCT` diagnostics. A Roslyn
  analyzer runs inside a C# compilation.
- Native Gherkin claim input, `codeAnalysis.records.claimSource`. The analyzer
  reads claims from `.feature` files without a § 3 table; the check does not,
  so a specification still carries its claims table. The reader is text alone,
  and the checker-side Feature belongs to epic `0001`
  (`0201-F7` § 5, row 7).
