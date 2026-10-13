---
title: "Seed brief: the specht repository"
description: "The decided requirements, the repository-owned engine baseline, the rule vocabulary, the drafted design, the open-question decisions, the order of work and the non-negotiables - the design authority cited as brief § N"
type: reference
---

# Seed brief: the `specht` repository

This brief records the decided requirements, the baseline engine, the rule
vocabulary, the design decisions, and the order of work. The engine import is
recorded in this repository at commit `e7dba24`. The requirements session is
retired at `REQUIREMENTS.md`, commit `254aabc`.

## 1. What this is

A command-line tool, delivered as a `dotnet tool`, that checks a repository's
`.spec/` specification tree against a versioned schema and reports every
violation with a file, a line, and a rule id. One schema and one checker shared
by every repository in the owner's ecosystem, replacing three hand-copied
implementations in four repositories.
The repository holds **both the tool and the schema**. No consumer repository
owns the schema; every consumer installs the tool.

## 2. Requirements, as decided (2026-10-07)

**Problem.** Four repositories are written in parallel on the same `.spec/`
approach. Three of them hand-copy the consumer's schema, templates, and checker.
Every improvement lands four times or drifts.
**Personas.** The maintainer of four repositories. The agent authoring a
specification in any of them.
**Done means.** The tool installed in two repositories (a consumer repository and
Transporter), both checked by it, no engine code in either. Transporter
originated the `.spec/` model and its tree predates the schema, so "done" also
proves the tool handles a tree it did not write.
**Must.**

1. One command, same verdict at three call sites: a pre-commit hook, CI, and
   an agent running it mid-write as a read-only oracle before it reverts or
   fixes a damaged draft.
2. The schema is versioned. The manifest pins `schemaVersion`; the tool embeds
   the schema versions it ships - every published one from schema `1.0.0` on
   (`0001-F7` decision 0007) - and validates with the pinned one.
   A repository upgrades when it chooses; lagging is deliberate, not drift.
   A version is `major.minor.patch`, numbered by whether a tree's verdict can
   change and apart from the package version (`0001-F7` decision 0005).
3. A specification is proven correctly written and formatted against the
   pinned version. Every violation: file, line, rule id, message.
4. The consumer's build calls the tool. No project reference to an engine.
5. Every violation also carries what the rule expected — the headers, the
   grammar, the section order — so the agent repairs the document from the
   report alone, without reading the rule catalogue or the template. (Was
   Should; promoted in session 2. Number kept: it is cited.)
   **Should.**
6. The schema doubles as the agent's generation contract: readable JSON Schema
   plus a manifest, shipped with the tool.
7. The baseline verdict is unchanged by the extraction: same violations, same
   report, on the same tree (all fields but the timestamp).
8. The schema source is configurable: the embedded schema for the pinned
   version by default; the consumer's on-disk `.specht/schema/` files when the
   manifest or a CLI argument says so. The manifest itself is always read from
   disk.
   **Out of scope.** Roslyn claim bridges and convention analyzers. Rule plugins. Spec revision history.
   Autofix: no `--fix`, and `upgrade` never rewrites a document — it touches
   `.specht/schema/` and `.specht/templates/` only; the agent migrates documents from
   the violations (Must-6). Reordering is the one exception, decided 2026-10-08:
   `specht format`, a separate command, moves whole table rows and frontmatter
   keys and edits nothing else; the check keeps no `--fix` (epic `0101`,
   `0101-F5` decision 0001).
   **Constraints.**
   | Constraint                                                                                                                                   | Hard/Soft |
   | -------------------------------------------------------------------------------------------------------------------------------------------- | --------- |
   | The folder is named `.spec/`                                                                                                                 | Hard      |
   | Own repository; tool and schema together. No consumer owns the schema; each holds a copy `init` wrote                                        | Hard      |
   | The engine import is recorded at repository commit `e7dba24` (item `0021`)                                                                   | Hard      |
   | Consumers install the tool; no engine code lands in a consumer repository                                                                    | Hard      |
   | Consumer build calls the tool, never carries the engine                                                                                      | Hard      |
   | `dotnet tool` delivery (was Soft; everything downstream assumes it)                                                                          | Hard      |
   | A schema version, once shipped in the tool, is never changed in place, and from schema `1.0.0` on is never deleted (`0001-F7` decision 0007) | Hard      |
   | Published to GitHub Packages on `rlittlesii/specht` first; NuGet.org is the reversal                                                         | Soft      |
   | Sections, grammars, markers: all reversible under schema versioning                                                                          | Soft      |
   | Deadline: none. Order of work (§ 8) is the only sequencing                                                                                   | —         |

## 3. The repository-owned baseline

The engine and its tests were imported at commit `e7dba24` (item `0021`).
That commit records the namespace rename and permitted project-file edits;
`0001-F1` C-2 preserves the import boundary. Read the existing implementation
and its tests before changing it; do not regenerate the checker.

The source of truth is in this repository:

| Path                                                                          | What                                                                                         |
| ----------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------- |
| `src/specht/`                                                                 | The rule engine: discovery, Markdown model, frontmatter, schemas, rules, runner and report   |
| `test/tests/`                                                                 | Unit and integration tests, including the synthetic `SpecTree` builder and baseline fixtures |
| `src/tool/schema/v0.1.0/frontmatter/`                                         | The embedded frontmatter schemas                                                             |
| `src/tool/manifest/v0.1.0/`                                                   | The embedded manifest                                                                        |
| `src/tool/templates/v0.1.0/`                                                  | The embedded templates                                                                       |
| `.specht/`                                                                    | The live configuration this repository checks itself with                                    |
| `.build/SpechtBuild.cs`, `.husky/pre-commit`                                  | Build and pre-commit call sites                                                              |
| `.claude/skills/spec-and-traceability/`, `.claude/skills/specht-conventions/` | The specification model and repository conventions                                           |
| `Directory.Packages.props`                                                    | Central dependency versions, including the baseline engine pins                              |

`0001-F1` decision 0004 defines the synthetic baseline tree and golden report.
Tests never require a consumer checkout. The requirements and rule vocabulary
below remain the agreement; a consumer repository is not their authority.

## 4. The rule vocabulary (fixed; versioned with the schema)

| Id                                                                     | Checks                                                                                                           |
| ---------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------- |
| SPEC001                                                                | No YAML frontmatter                                                                                              |
| SPEC002/003/004                                                        | Feature / item / epic frontmatter fails its JSON Schema                                                          |
| SPEC010                                                                | Contracted sections missing, duplicated, or out of order                                                         |
| SPEC011                                                                | Frontmatter `epic`/`id` disagree with the containing directory (legacy layout only)                              |
| SPEC012                                                                | One identity specified in two places                                                                             |
| SPEC013                                                                | A contracted table has no table, or the wrong headers                                                            |
| SPEC020                                                                | Not exactly one companion `.feature` beside the spec                                                             |
| SPEC021                                                                | A scenario tag resolves to no § 3 claim                                                                          |
| SPEC030                                                                | Claim id breaks the grammar, or is declared twice                                                                |
| SPEC031                                                                | § 9 row count per claim is not exactly one; § 9 cites a non-claim                                                |
| SPEC040/041                                                            | Declared children / spikes do not resolve to one file (spikes resolve repository-wide, children beside the spec) |
| SPEC043                                                                | Item id disagrees with its file name or its parent                                                               |
| SPEC044                                                                | Item id reused, or the per-epic sequence skips a number                                                          |
| SPEC050/051/052                                                        | Dependency names no Feature; edge declared from one end only; self-dependency or cycle                           |
| SPEC060/061                                                            | Approved spec still has a `Missing` cell / an unapproved sign-off row                                            |
| Severity: all `Error` today. A manifest severity override and per-rule |
| disable are part of the design (§ 5).                                  |

## 5. Drafted design, to build on

**CLI.** `specht [--root <dir>] [--report <path>] [--strict] [--json]`.
Stdout: one MSBuild-shaped line per violation (`path(line): error SPEC031: …`),
then the summary lines (specification counts by layout, item count, rules
evaluated, errors/warnings). Exit 0 clean, 1 violations (any violation under
`--strict`), 2 missing root or manifest, 3 invalid manifest, 4 a named
thing not found (`0001-F2` decision 0003). `--json`
replaces the stdout stream with the report document; each violation in it
carries what the rule expected (Must-6). `--report` writes the same JSON to a
path. `--explain SPEC031` prints the rule's full text. Never an absolute path
in any output. `specht init` writes the schema set (and templates) into
`<root>/.specht/` (ADR-0011) from embedded copies, or the frontmatter schemas from an
upstream schema source the manifest records or the same init run will record
(`0001-F4` B-014, B-035 to B-040; decision 0003, ratified 2026-10-12), never
overwriting. `specht upgrade`
brings `.specht/schema/` and `.specht/templates/` to the pinned version and prints
what changed; it never touches a document. A list command names the shipped
schema versions and a pin command moves `schemaVersion` to one of them, up or
down; their names are open (`0001-F7` decision 0005, OQ-11). `--help` from the
command framework.
**Manifest-driven layout ("bring your own schema").** Everything the engine
hardcodes moves into the manifest so rules read _roles_, not literals:

- discovery layouts (today: legacy `epics/**/spec.md`, co-located
  `**/.spec/README.md`), the root `.spec/` exclusion, excluded directories,
  the item file-name shape, the epic file name, the companion glob;
- section roles (`claims` → "3. Acceptance Criteria", `matrix` → "9. …",
  `signOff` → "12. …") and table headers per role;
- cell markers (`Missing`, the 🟡/🔴 sign-off markers, the approved status);
- frontmatter key roles (`children`, `spikes`, `depends_on`, `blocks`,
  `parent`, `epic`, `id`), the identity form `{epic}-{id}`, the edge
  forms (`F2`, `0002/F1`);
- id grammars (already there: claim, task, feature, epic, …), which every
  rule reads instead of its own regex (`FeatureFileReader` and
  `ChildItemRule` duplicate them today);
- per-rule severity override and disable.
  The default manifest preserves the baseline values, so its run is unchanged (Should-7).
  **Discovery cost.** Inside a git repository: `git ls-files` (tracked, plus
  `--others --exclude-standard`) with the manifest's globs as pathspecs, so
  ignored trees are never opened. Outside git: a walk that prunes on entry
  (`FileSystemEnumerable` with `ShouldRecursePredicate`) using the manifest's
  exclusion list. Both must return the same set for the same repository tree.
  **Tables.** Markdig pipe tables only. A grid table is "no table" (SPEC013).
  State it in the docs.
  **Decision record (carry across).** Shipped as a dotnet tool over a library;
  layout in the manifest; no rule plugins (disable and `--strict` instead).
  Rejected: Nuke-only, a library consumers wire themselves, a plugin model, a
  Roslyn analyzer (per-compilation; the rules are repository-wide).

## 6. Open questions, decided (2026-10-07)

Every default below was confirmed or replaced by the owner in the second
requirements session (`REQUIREMENTS.md` § Open Questions, at commit
`254aabc`), except a row carrying its own later date, which the owner asked
and decided on that date. Each is a decision
the owner can reverse; record a reversal as a decision, never silently.

| Question                                                           | Decision                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             | Why                                                                                                                                                                                                                            |
| ------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Name, package id, command                                          | Product Specht; command `specht`; package `specht.tool`; library namespace `specht`. `Specht` survives only as the NUKE target name and engine type names                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            | Says what it does; the product owns its names                                                                                                                                                                                  |
| What a "schema version" is in the file                             | The manifest carries `"schemaVersion": "0.1.0"`, a `major.minor.patch` version numbered by verdict and apart from the package version; no key means `0.1.0`; the tool embeds the schema versions it ships and validates with exactly the pinned one; a published version is never changed in place, and from schema `1.0.0` on none is ever deleted, while a later tool may stop shipping a published `0.x.y` (`0001-F7` decision 0007, 2026-10-11); a pin command moves `schemaVersion` to any shipped version, up or down; `specht upgrade` rewrites the schema files, and the templates the manifest does not name as the consumer's (`0001-F7` decision 0006), to match the pin, adds only the manifest keys the pinned version defines that are missing (`0001-F7` decision 0001), prints what changed, and never touches a document. Was an integer moved one step by `upgrade`; reversed 2026-10-09 (`0001-F7` decision 0005) | A repository pins one version, and its number says whether moving can fail the build; the tool, not the consumer, knows the diff between versions; a repository pinned to `0.1.0` restores any later tool and is still checked |
| Which schema files are authoritative at check time                 | The embedded schema for the pinned version by default; the consumer's on-disk `.specht/schema/` files when a manifest field or a CLI argument selects them (Should-8). Field and argument names: open, `spec-author`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 | Supports "bring your own schema" without making a hand-edited copy the silent default                                                                                                                                          |
| Install path for four repositories                                 | Local tool manifest per repository (`dotnet tool install --local`, committed `.config/dotnet-tools.json`), package published to GitHub Packages on `rlittlesii/specht` first; NuGet.org later, if ever. A global install (`dotnet tool install -g specht.tool`) is a second documented path, for a shell, and no build depends on it (`0001-F2` decision 0009, 2026-10-11)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           | Pins the tool version beside the schema version; CI restores it with `dotnet tool restore`. Consumers carry a `nuget.config` source and a token with package read                                                              |
| Templates: in the tool or per repository                           | In the tool, written by `specht init` next to the schema; `init` never overwrites one, and `specht upgrade` rewrites those the manifest does not name as the consumer's to the pinned version and lists each (`0001-F7` decisions 0005 and 0006)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     | They are part of the generation contract (Should-5) and version with the schema                                                                                                                                                |
| Agent-facing output contract                                       | `--json` document with a published JSON Schema in this repository: violations (rule, severity, file, line, identifier, message, **expected** — what the rule wanted to see), counts, layouts, schema version checked against; `--explain SPEC031` prints the rule's full text. Shape of `expected` per rule: open, `spec-author`                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     | The agent repairs, and migrates between schema versions, from the report alone (Must-6)                                                                                                                                        |
| Ownership of the tool                                              | This repository owns epic `0001`; consumer adoption is tracked in each consumer repository                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           | Keeps tool delivery separate from adoption                                                                                                                                                                                     |
| Schema `$id` URLs                                                  | Under `https://github.com/rlittlesii/specht/schema/`, carrying the schema version as the segment `v<major.minor.patch>`, the name of the version's folder, then `/frontmatter/` and the file's name: `schema/v0.1.0/frontmatter/<file>` (`0001-F7` C-6, B-087; decided 2026-10-11, `0001-F7` decision 0009 and `0001-F5` decision 0011). Was `v1`, its later form open                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               | They move with the schema                                                                                                                                                                                                      |
| Report timestamp                                                   | Drop `generatedAtUtc` from the report                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                | Determinism; Should-7 becomes byte-identical                                                                                                                                                                                   |
| Second install, the "done" event                                   | Transporter. Its `.spec/` tree predates the schema                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   | The origin of the model; proves the tool on a tree it did not write                                                                                                                                                            |
| Who migrates documents across schema versions                      | The agent, from the violations and their `expected`. Never the tool                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  | Autofix is out of scope; `specht format` only reorders rows and keys (`0101-F5` decision 0001)                                                                                                                                 |
| Are epics required? (asked and decided 2026-10-09; `0001-F5` OQ-3) | No: epics are opt-in. The manifest's epic grammar (`identifiers.epic`) is the signal. Declared, every rule behaves as before and a Feature's identity is `{epic}-{id}`. Absent, there is no epic tier: a Feature's identity is its `id` alone, unique across the repository, and `specht init` writes that shape unless given `--epics`. Ships as an additive change to schema version 1, with no new `SPEC###` (`0001-F5` decision 0004). This repository keeps its epics                                                                                                                                                                                                                                                                                                                                                                                                                                                           | Standalone Features with no epic tier are a real consumer shape. Version 1 is unpublished, so loosening it breaks no pin (`0001-F7` decision 0002)                                                                             |

## 7. The repository should eat its own cooking

- Its own specifications live under `.spec/` in the co-located layout and
  are checked by the tool in its own CI from the first commit that can run it.
- Its specification model is the schema it ships; the epic for the tool is
  written with the Feature template it ships.
- Pre-commit and CI both call the tool exactly as a consumer would, through
  the local tool manifest, not through a project reference.

## 8. Order of work

1. **Scaffold.** Repository, `.slnx`, `Directory.Build.props` with
   `TreatWarningsAsErrors`, central package versions, the local tool
   manifest, CI that builds, tests, and packs. `.spec/` with the epic.
2. **Import the engine and its tests.** Completed at commit `e7dba24`
   (item `0021`), with the namespace rename and permitted project-file edits
   recorded there. The synthetic golden report protects Should-7
   (`0001-F1` decision 0004).
3. **CLI.** The command, exit codes, `--json`, `--report`, `init`,
   `--help`. Pack as a tool; install it into this repository's own manifest;
   switch its CI and pre-commit to it.
4. **First consumer adoption.** Replace the consumer build's library call
   with the tool and remove its engine copy and tests. Prove its verdicts
   match the baseline. Adoption work is tracked in that repository.
5. **Manifest roles.** Move each hardcoded literal into the manifest, one rule
   at a time, each with a test that the default manifest reproduces the
   baseline report. Discovery via `git ls-files` with the pruned-walk
   fallback.
6. **Schema versioning.** `schemaVersion`, embedded version set, `upgrade`.
7. **Install in Transporter.** Pin v1 onto its pre-schema tree; an agent
   repairs what the tool reports, from the report alone. That is "done".
   Each step is a pull request; each pull request's description names the
   requirement (§ 2 number) it serves.

## 9. Non-negotiables for the first agent

- The engine baseline is recorded at repository commit `e7dba24`,
  never regenerated. If a file looks like it should be rewritten, the answer
  is a later step with a test, not a fresh draft.
- Read the extracted tests before the extracted engine. They say what the
  engine promises.
- Do not widen the rule vocabulary in the first three steps. New rules are
  schema versions, after step 6.
- Repository-relative paths in every output, every report, every test
  fixture. An absolute path anywhere is a defect.
- The check is offline. Only `init` and `upgrade` may touch the network, and
  only to fetch an upstream schema source the manifest records, or the same
  init run will record (`0001-F4` decision 0003, ratified 2026-10-12), with its
  version and content hash (`0001-F7` decision 0004).
- The tool never writes into a consumer's tree except through `init`,
  `upgrade`, the pin command, the set command, the report file the caller
  names with `--report`, and `format`, which moves whole table rows and frontmatter keys
  in the files it reorders and nothing else (`0101-F5` decision 0001). `init`
  never overwrites an existing file. `upgrade` rewrites the schema and
  template files it owns to match the pinned version and, in the manifest,
  adds only the keys that version defines that are missing (`0001-F7`
  decision 0001). The pin command changes only `schemaVersion` in the
  manifest (`0001-F7` decision 0005). The set command changes only one rule's
  level in the manifest's `rules` (`0001-F11` decision 0001). `--report`
  overwrites the file it names
  (`0001-F3` decision 0002).
- When a question in § 6 turns out to need the owner, stop and ask; do not
  pick a second default silently.
