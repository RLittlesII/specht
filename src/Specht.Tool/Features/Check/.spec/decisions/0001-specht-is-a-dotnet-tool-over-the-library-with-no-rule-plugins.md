---
title: "Decision 0001: specht ships as a dotnet tool over the library, with the layout in the manifest and no rule plugins"
description: "The rule engine stays a library and is installed as the dotnet tool specht; everything it hardcodes about a repository's layout moves into the manifest; the SPEC rule vocabulary stays fixed and a manifest may disable or re-grade a rule but not add one. Nuke-only, a consumer-wired library, a plugin model and a Roslyn analyzer were turned down"
type: decision
---

# Decision 0001: specht ships as a dotnet tool over the library, with the layout in the manifest and no rule plugins

**Date:** 2026-10-07
**Decided by:** the repository owner, raised during the review of `hooked` PR #217; recorded by spec-author there as `0008-F3` decision 0001 and carried across here with the names decided since (README § 6)

## The call

The engine is shipped as a `dotnet tool`: package `Specht.Tool`, command
`specht`, built over the engine library `Specht`, which stays a library with no
entry point. A consumer installs it through a committed local tool manifest and
its build calls the tool; no consumer carries the engine.

The remainder the engine hardcodes - the two layouts and their globs, the
exclusion list, the item, epic and companion file shapes, the section roles,
the `Missing` cell and the sign-off markers, the frontmatter key names, the
identity and edge forms, the schema file names - moves into the manifest the
engine already reads from `.spec/schema/` (`0001-F5`, `0001-F6`), so a rule
reads a role and another repository supplies its own values. `specht init`
writes the defaults into a repository that has none (`0001-F4`).

The `SPEC###` rule vocabulary stays fixed per schema version. A manifest may
disable a rule or lower it to a warning, and `--strict` promotes every warning;
a manifest may not add a rule, and no assembly is loaded at run time.

## Why

- The engine's rules are repository-wide - the dependency graph, the per-epic
  sequence, one identity across two layouts - and the manifest already carries
  the section list and the grammars as data because a C# string array would be
  a second, invisible home for the twelve-section rule. The literals that
  remain in code are the same kind of fact, left behind. A second repository
  exposes them: it cannot change a section title without forking the engine.
- A tool is the installation shape that fits a CI step and a shell: one
  command, a manifest at a known path, an exit code, and diagnostics in the form
  GitHub already annotates. A local tool manifest pins the tool version beside
  the schema version, and `dotnet tool restore` restores it in CI.
- Keeping the vocabulary fixed keeps the rule ids as permanent as claim ids and
  keeps "SPEC031 fired" meaning one thing in every repository that runs the
  tool. Disable and severity cover the adoption cases seen so far - a repository
  with no items, a repository not yet normalised - without a second place a
  rule can be defined.

## Rejected

**Nuke-only - `hooked`'s shape at the time.** The engine stays a project
reference from `.build/` and `./build.sh SpecCheck` is the only entry.

- Reaches no other repository: adopting the model means copying the engine,
  the schemas and the target, and every copy drifts from the next. This is the
  problem statement.
- Bootstraps a build orchestrator to check markdown.
- Cost of rejecting: a second project, a package, a publish channel and a
  reversal of `IsPackable=false` for one project. Taken.

**A NuGet library consumers wire themselves.** Publish the engine as a package
and let each repository write its own Nuke target or console host around the
runner.

- Every adopter writes the same thirty lines - root, report, strict, exit code
  - and gets them slightly different; the diagnostic form and the exit codes
  stop being one contract.
- Still needs the layout in the manifest to be usable at all, so it is this
  decision minus the command.
- Cost of rejecting: nothing - the library is still the thing the tool wraps.

**A rule-plugin model.** Load rule assemblies named in the manifest or dropped
in a folder, so an adopting repository adds its own `SPEC` rules.

- Assembly loading at run time is a second product surface - versioning,
  isolation, a contract `ISpecRule` was never designed to be - for a need no
  adopter has stated.
- A rule id minted outside the engine collides with the next one the engine
  ships, and the vocabulary stops meaning one thing.
- Cost of rejecting: an adopter with a genuinely new rule contributes it to the
  engine, as a new schema version, or does without. Taken.

**A Roslyn analyzer.** Report `SPEC###` violations from an analyzer over the
specification files as `AdditionalFiles`.

- An analyzer sees one compilation; the rules are repository-wide.
- Ties the check to a compile, which the gate was built not to need.
- Cost of rejecting: none for this repository; `hooked` `0008-F1` is the
  per-compilation half and stays there.

**A global tool install** (`dotnet tool install -g`) as the documented path.

- A global install pins nothing beside the repository; CI and a second machine
  get whatever version is current. The local manifest is what makes the tool
  version a fact of the commit.
- Cost of rejecting: each consumer commits `.config/dotnet-tools.json` and a
  `nuget.config` source entry. Taken.

## Affects

- `0001-F2` § 3 B-001 to B-013, § 4 C-1 to C-7, § 5 rows 7 and 8.
- `0001-F5` § 4 C-3 (the fixed vocabulary) and § 5 row 1.
- `0001-F4`: `init` exists because the defaults are written, not copied.

## Reversal

None.
