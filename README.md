# Specht

This is a repository for a dotnet tool that will check your specification document schema against the markdown generated.  Below is the seed work for this repository.

# Seed brief: the `spec-check` repository
> Hand this to the first agent in the new repository. It carries the decided
> requirements, what to extract from `hooked`, the draft design already
> written, the defaults to proceed on where the owner has not yet decided, and
> the order of work. Source material: `hooked` branch
> `grooming/0008-specification-governance` at `a6d056f`.
## 1. What this is
A command-line tool, delivered as a `dotnet tool`, that checks a repository's
`.spec/` specification tree against a versioned schema and reports every
violation with a file, a line and a rule id. One schema and one checker shared
by every repository in the owner's ecosystem, replacing three hand-copied
implementations in four repositories.
The repository holds **both the tool and the schema**. No consumer repository
owns the schema; `hooked` is consumer one.
## 2. Requirements, as decided (2026-10-07)
**Problem.** Four repositories are written in parallel on the same `.spec/`
approach. Three of them hand-copy `hooked`'s schema, templates and checker.
Every improvement lands four times or drifts.
**Personas.** The maintainer of four repositories. The agent authoring a
specification in any of them.
**Done means.** The tool installed in two repositories (`hooked` and one
sibling), both checked by it, no engine code in either.
**Must.**
1. One command, same verdict at three call sites: a pre-commit hook, CI, and
   an agent running it mid-write as a read-only oracle before it reverts or
   fixes a damaged draft.
2. The schema is versioned. A repository pins a version and upgrades when it
   chooses; lagging is deliberate, not drift.
3. A specification is proven correctly written and formatted against the
   pinned version. Every violation: file, line, rule id, message.
4. The consumer's build calls the tool. No project reference to an engine.
**Should.**
5. The schema doubles as the agent's generation contract: readable JSON Schema
   plus a manifest, as `hooked/.spec/schema/` is today.
6. Mid-write, the agent gets everything the tool knows, machine-readable.
7. `hooked`'s own run is unchanged by the extraction: same violations, same
   report, on the same tree (all fields but the timestamp).
**Out of scope.** Anything Roslyn (`hooked`'s 0008-F1 claim bridge and
0008-F2 convention analyzers stay there). Rule plugins. Spec revision history.
**Constraints.**
| Constraint | Hard/Soft |
| --- | --- |
| The folder is named `.spec/` | Hard |
| Own repository; tool and schema together | Hard |
| Ships after `hooked` PR #217 merges (the engine lands there first) | Hard |
| Consumer build calls the tool, never carries the engine | Hard |
| `dotnet tool` delivery; runtime otherwise open | Soft |
| Sections, grammars, markers: all reversible under schema versioning | Soft |
## 3. What to extract from `hooked`
**The code exists on disk. Copy it; do not regenerate it.** The engine, its
tests, the schemas and the templates are written, reviewed and passing. The
first agent's job is to move files and rename a namespace, not to write a
checker. Every `.cs` file below is the implementation; read it, keep it.
Checkout on this machine: `/Users/rlittlesii/source/rlittlesii/hooked`, branch
`grooming/0008-specification-governance`. Copy with history noted in the first
commit message (`extracted from RLittlesII/hooked@<sha>`); do not subtree-merge.
```sh
HOOKED=/Users/rlittlesii/source/rlittlesii/hooked
# the engine (library) and its tests - verbatim, then rename Hooked.SpecGovernance
cp -R "$HOOKED/tools/SpecGovernance"            src/SpecCheck
cp -R "$HOOKED/test/SpecGovernance.Tests"       test/SpecCheck.Tests
# schema version 1 and the templates - verbatim, then re-home the $id URLs
cp -R "$HOOKED/.spec/schema"                    schema/v1
cp -R "$HOOKED/.spec/templates"                 templates/v1
# the behaviour contract the CLI must match, and the two call sites
cp    "$HOOKED/.build/Build.SpecCheck.cs"       docs/reference/hooked-nuke-target.cs
cp    "$HOOKED/.husky/pre-commit"               docs/reference/hooked-pre-commit.sh
# the draft specification, the requirements session, and the prose model
cp    "$HOOKED/tools/SpecGovernance/.spec/README.md"                           docs/reference/0008-F3-draft-spec.md
cp    "$HOOKED/tools/SpecGovernance/.spec/spec-check-tool.feature"             docs/reference/0008-F3-draft.feature
cp    "$HOOKED/tools/SpecGovernance/.spec/decisions/"0001-*.md                 docs/reference/
cp    "$HOOKED/epics/audits/2026-10-07-spec-check-extraction-requirements.md"  docs/reference/
cp -R "$HOOKED/.skills/spec-and-traceability"                                  docs/reference/spec-and-traceability
cp    "$HOOKED/.skills/hooked-conventions/references/specs.md"                 docs/reference/hooked-specs-conventions.md
```
Remove `tools/SpecGovernance/.spec/` from the copied `src/SpecCheck` (it is the
draft spec, already copied to `docs/reference/`), and drop the `.csproj`
comments that talk about `hooked`'s `Directory.Build.props`. Everything else in
those directories is kept.
Inventory on disk, 44 files (verify with the same `find` before copying):
```
.spec/schema/epic.frontmatter.schema.json
.spec/schema/feature-spec.frontmatter.schema.json
.spec/schema/spec-structure.schema.json
.spec/schema/task.frontmatter.schema.json
.spec/templates/adr.md
.spec/templates/decision.md
.spec/templates/feature.md
.spec/templates/lesson.md
test/SpecGovernance.Tests/SpecCheckRunner.Integration.Tests.cs
test/SpecGovernance.Tests/SpecCheckRunner.Unit.Tests.cs
test/SpecGovernance.Tests/SpecCheckRunner.Violations.Unit.Tests.cs
test/SpecGovernance.Tests/SpecGovernance.Tests.csproj
test/SpecGovernance.Tests/SpecTree.cs
tools/SpecGovernance/.spec/decisions/0001-spec-check-is-a-dotnet-tool-over-the-library-with-the-layout-in-the-manifest.md
tools/SpecGovernance/.spec/README.md
tools/SpecGovernance/.spec/spec-check-tool.feature
tools/SpecGovernance/ChildItem.cs
tools/SpecGovernance/EpicFile.cs
tools/SpecGovernance/FeatureFileReader.cs
tools/SpecGovernance/FeatureSpec.cs
tools/SpecGovernance/FeatureTag.cs
tools/SpecGovernance/Frontmatter.cs
tools/SpecGovernance/FrontmatterReader.cs
tools/SpecGovernance/ISpecRule.cs
tools/SpecGovernance/Rules/ApprovalRule.cs
tools/SpecGovernance/Rules/ChildItemRule.cs
tools/SpecGovernance/Rules/ClaimRule.cs
tools/SpecGovernance/Rules/DependencyRule.cs
tools/SpecGovernance/Rules/FeatureFileRule.cs
tools/SpecGovernance/Rules/FrontmatterSchemaRule.cs
tools/SpecGovernance/Rules/IdentityRule.cs
tools/SpecGovernance/Rules/SectionStructureRule.cs
tools/SpecGovernance/SpecCheckReport.cs
tools/SpecGovernance/SpecCheckRunner.cs
tools/SpecGovernance/SpecDiscovery.cs
tools/SpecGovernance/SpecDocument.cs
tools/SpecGovernance/SpecGovernance.csproj
tools/SpecGovernance/SpecLayout.cs
tools/SpecGovernance/SpecLocation.cs
tools/SpecGovernance/SpecModel.cs
tools/SpecGovernance/SpecSchemas.cs
tools/SpecGovernance/SpecSection.cs
tools/SpecGovernance/SpecSeverity.cs
tools/SpecGovernance/SpecStructure.cs
tools/SpecGovernance/SpecViolation.cs
```
By purpose:
| Path in `hooked` | What | Notes |
| --- | --- | --- |
| `tools/SpecGovernance/*.cs`, `Rules/*.cs` | The rule engine: discovery, Markdig document model, YAML frontmatter reader, JSON Schema loader, eight rule classes, runner, report | ~930 lines. Namespace `Hooked.SpecGovernance` must be renamed (no `Hooked` in assembly, namespace or package) |
| `test/SpecGovernance.Tests/` | 56 xunit.v3 tests over a synthetic tree (`SpecTree.cs` builder) | Port with the engine; they are the regression net for Should-7 |
| `.spec/schema/` | `feature-spec.frontmatter.schema.json`, `task.frontmatter.schema.json`, `epic.frontmatter.schema.json`, `spec-structure.schema.json` (the manifest; "not a JSON Schema" by its own comment) | Becomes **schema version 1**. Schema `$id` URLs point at `hooked`; re-home them |
| `.spec/templates/` | `feature.md`, `decision.md`, `lesson.md`, `adr.md` | Ship with the tool (default below) |
| `.build/Build.SpecCheck.cs` | The Nuke target: log lines, `--strict`, `--report`, exit semantics | The CLI's behaviour contract; the target itself stays in `hooked` and will call the tool |
| `.husky/pre-commit` (SpecCheck block) | Conditional on staged spec paths, deletes and renames included | Reference for the pre-commit call site |
| `tools/SpecGovernance/.spec/README.md` | Draft specification 0008-F3: 34 claims, each citing the engine line it changes | The design input. Its § 11 open questions are § 6 below |
| `epics/audits/2026-10-07-spec-check-extraction-requirements.md` | The requirements session this brief condenses | Source of § 2 |
| `.skills/spec-and-traceability/`, `.skills/hooked-conventions/references/specs.md` | The specification model and the SPEC rule catalogue as prose | Generalise: strip `hooked` paths; this becomes the tool's user documentation |
Dependencies: Markdig (pipe tables), YamlDotNet (representation model, not
deserialization: it keeps dates as strings), JsonSchema.Net (format
assertions on). Pin exact versions from `hooked/Directory.Packages.props`.
## 4. The rule vocabulary (fixed; versioned with the schema)
| Id | Checks |
| --- | --- |
| SPEC001 | No YAML frontmatter |
| SPEC002/003/004 | Feature / item / epic frontmatter fails its JSON Schema |
| SPEC010 | Contracted sections missing, duplicated, or out of order |
| SPEC011 | Frontmatter `epic`/`id` disagree with the containing directory (legacy layout only) |
| SPEC012 | One identity specified in two places |
| SPEC013 | A contracted table has no table, or the wrong headers |
| SPEC020 | Not exactly one companion `.feature` beside the spec |
| SPEC021 | A scenario tag resolves to no § 3 claim |
| SPEC030 | Claim id breaks the grammar, or is declared twice |
| SPEC031 | § 9 row count per claim is not exactly one; § 9 cites a non-claim |
| SPEC040/041 | Declared children / spikes do not resolve to one file (spikes resolve repository-wide, children beside the spec) |
| SPEC043 | Item id disagrees with its file name or its parent |
| SPEC044 | Item id reused, or the per-epic sequence skips a number |
| SPEC050/051/052 | Dependency names no Feature; edge declared from one end only; self-dependency or cycle |
| SPEC060/061 | Approved spec still has a `Missing` cell / an unapproved sign-off row |
Severity: all `Error` today. A manifest severity override and per-rule
disable are part of the design (§ 5).
## 5. Design already drafted (0008-F3), to build on
**CLI.** `spec-check [--root <dir>] [--report <path>] [--strict] [--json]`.
Stdout: one MSBuild-shaped line per violation (`path(line): error SPEC031: …`),
then the summary lines (specification counts by layout, item count, rules
evaluated, errors/warnings). Exit 0 clean, 1 violations (any violation under
`--strict`), 2 missing root or manifest, 3 invalid manifest. `--json`
replaces the stdout stream with the report document. `--report` writes the
same JSON to a path. Never an absolute path in any output. `spec-check init`
writes the schema set (and templates) into `<root>/.spec/` from embedded
copies, never overwriting. `--help` from the command framework.
**Manifest-driven layout ("bring your own schema").** Everything the engine
hardcodes moves into the manifest so rules read *roles*, not literals:
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
The default manifest equals `hooked`'s, so its run is unchanged (Should-7).
**Discovery cost.** Inside a git repository: `git ls-files` (tracked, plus
`--others --exclude-standard`) with the manifest's globs as pathspecs, so
ignored trees are never opened. Outside git: a walk that prunes on entry
(`FileSystemEnumerable` with `ShouldRecursePredicate`) using the manifest's
exclusion list. Both must return the same set on `hooked`.
**Tables.** Markdig pipe tables only. A grid table is "no table" (SPEC013).
State it in the docs.
**Decision record (carry across).** Shipped as a dotnet tool over a library;
layout in the manifest; no rule plugins (disable and `--strict` instead).
Rejected: Nuke-only, a library consumers wire themselves, a plugin model, a
Roslyn analyzer (per-compilation; the rules are repository-wide).
## 6. Open questions, with the default to proceed on
Proceed on the default; record each as a decision the owner can reverse.
| Question | Default | Why |
| --- | --- | --- |
| Name, package id, command | Repository `spec-check`; command `spec-check`; package `SpecCheck.Tool`; library namespace `SpecCheck` | Says what it does; no `Hooked` anywhere |
| What a "schema version" is in the file | The manifest carries `"schemaVersion": 1`; the tool embeds every schema version it knows and validates with the pinned one; `spec-check upgrade` rewrites the manifest and schema files to the next version and prints what changed | A repository pins by editing one number; the tool, not the consumer, knows the diff between versions |
| Install path for four repositories | Local tool manifest per repository (`dotnet tool install --local`, committed `.config/dotnet-tools.json`), package published to NuGet.org | Pins the tool version beside the schema version; CI restores it with `dotnet tool restore` |
| Templates: in the tool or per repository | In the tool, written by `spec-check init` next to the schema, never overwritten | They are part of the generation contract (Should-5) and version with the schema |
| Agent-facing output contract | `--json` document with a published JSON Schema in this repository: violations (rule, severity, file, line, identifier, message), counts, layouts, schema version checked against; add `--explain SPEC031` printing the rule's full text | "Any information it might need", machine-readable |
| Where 0008-F3 lives | This repository's first epic is the tool; `hooked` keeps a small Feature "consume the tool": replace the project reference with the tool call, delete `tools/SpecGovernance` | Keeps `hooked`'s scope to consumption |
| Schema `$id` URLs | This repository's URL, versioned path (`…/schema/v1/feature-spec.frontmatter.schema.json`) | They move with the schema |
| Report timestamp | Drop `generatedAtUtc` from the report | Determinism; Should-7 becomes byte-identical |
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
   manifest, CI that builds, tests and packs. `.spec/` with the epic.
2. **Copy the engine and its tests from disk** (§ 3 script), rename the
   namespace, nothing else. Prove the 56 tests pass. Tag the commit: this is
   the behaviour baseline for Should-7. No file in `src/SpecCheck` is rewritten
   in this step; a diff against `hooked` must show only the rename.
3. **CLI.** The command, exit codes, `--json`, `--report`, `init`,
   `--help`. Pack as a tool; install it into this repository's own manifest;
   switch its CI and pre-commit to it.
4. **Consume from `hooked`.** Replace the Nuke target's library call with the
   tool; delete `tools/SpecGovernance` and its tests there; prove the report
   on `hooked` is identical to the baseline. This is "installed in one".
5. **Manifest roles.** Move each hardcoded literal into the manifest, one rule
   at a time, each with a test that the default manifest reproduces the
   baseline report. Discovery via `git ls-files` with the pruned-walk
   fallback.
6. **Schema versioning.** `schemaVersion`, embedded version set, `upgrade`.
7. **Install in a second repository.** That is "done".
Each step is a pull request; each pull request's description names the
requirement (§ 2 number) it serves.
## 9. Non-negotiables for the first agent
- The engine is copied from `/Users/rlittlesii/source/rlittlesii/hooked`,
  never regenerated. If a file looks like it should be rewritten, the answer
  is a later step with a test, not a fresh draft.
- Read the extracted tests before the extracted engine. They say what the
  engine promises.
- Do not widen the rule vocabulary in the first three steps. New rules are
  schema versions, after step 6.
- Repository-relative paths in every output, every report, every test
  fixture. An absolute path anywhere is a defect.
- The tool never writes into a consumer's tree except through `init` and
  `upgrade`, and never overwrites an existing file.
- When a question in § 6 turns out to need the owner, stop and ask; do not
  pick a second default silently.

## Agent skills: what is in the repository, and what is installed

Instructions for AI agents come from two places, and the split is deliberate.

**Tracked here, in [`.skills/`](./.skills) — needed to deliver, so a clone has
it.** Eight skills, reviewable in a pull request like any other file:

| Kind | Skill | Covers |
| ---- | ----- | ------ |
| method | [`deliver-change`](./.skills/deliver-change/SKILL.md) | issue to branch to specification to pull request |
| method | [`coding-conventions`](./.skills/coding-conventions/SKILL.md) | orient, keep the design direct, say it once |
| method | [`test-from-scenarios`](./.skills/test-from-scenarios/SKILL.md) | a claim first, injected time, synthetic fixtures, both tiers |
| method | [`clarify-requirements`](./.skills/clarify-requirements/SKILL.md) | when to ask versus decide, and writing the answer back |
| method | [`spec-and-traceability`](./.skills/spec-and-traceability/SKILL.md) | the specification model, claims, records, blast radius |
| companion | [`specht-conventions`](./.skills/specht-conventions/SKILL.md) | this repository: paths, section ownership, ID schemes, tiers, commands, the self-check |
| technology | [`spectre-cli`](./.skills/spectre-cli/SKILL.md) | the CLI framework the tool is built on |
| technology | [`dotnet-tool`](./.skills/dotnet-tool/SKILL.md) | the tool's shape: project setup, command folders, packing, the local tool manifest |

Plus the four role contracts in [`.agents/`](./.agents/README.md), the
templates in [`.spec/templates/`](./.spec/templates) and the schema set in
[`.spec/schema/`](./.spec/schema) — the live copy this repository checks itself
with, and the same bytes the tool embeds and writes into a consumer on
`specht init`. The entry point for all of it is [`AGENTS.md`](./AGENTS.md).

These were seeded from [`hooked`](https://github.com/RLittlesII/hooked) pull
request #217 and rewritten for this repository: `hooked`'s product skills
(webhooks, the event store, the two-host slice layout) were dropped, and
everything that said "not this repository's path" about shipping a `dotnet tool`
now says the opposite.

**Installed from [`Skillfile`](./Skillfile) — convenience, not a dependency.**
Eighteen library and tooling skills (`xunit`, `reqnroll`,
`rocket-surgery-testing-autofixtures`, `central-package-management`, `nuke`,
`msbuild`, `dotnet-build`, `github-actions`, `logging`, the git and GitHub
operations set, `code-evidence`, `code-reader`, `engram`, `neo4j`), plus six
agents, pulled from `littlestechnology/.agents` and pinned in
[`Skillfile.lock`](./Skillfile.lock).

### The gap, stated plainly

`littlestechnology/.agents` is **private**. Without access to it, `skillfile
install` fails and none of those eighteen skills land. Everything in `.skills/`,
`.agents/` and `.spec/` still works, and so does the build — `./build.sh` and
`./build.sh SpecCheck` have no dependency on any of it.

What is lost is library-level guidance an agent would otherwise read before
touching xUnit, Reqnroll or NUKE. The repository's own conventions for those
surfaces are **not** in the missing skills: the traps live in
[`specht-conventions`](./.skills/specht-conventions/SKILL.md) and its
references, which are tracked. So a contributor without access can still follow
this repository's rules; they just do not get the general guide to the library
underneath.

The rule is in [AGENTS.md](./AGENTS.md) § Skills: **a skill needed to deliver a
feature belongs in `.skills/`, not in `Skillfile`.** `spectre-cli` and
`dotnet-tool` are in `.skills/` for exactly that reason. If something in the
installed set becomes load-bearing, it gets copied in and rewritten for this
repository rather than relied on from outside.

### Installing the optional set

Declared in [`Skillfile`](./Skillfile), pinned in
[`Skillfile.lock`](./Skillfile.lock), pulled via the
[`skillfile`](https://github.com/anthropics/skillfile) CLI.

1. Install the `skillfile` CLI and make sure it's on your `PATH`.
2. From the repo root, install everything declared in `Skillfile`:

   ```bash
   skillfile install
   ```

   This reads `Skillfile`, resolves `Skillfile.lock`, and installs each
   listed agent and skill for Claude Code locally, into the gitignored
   `.claude/`.
