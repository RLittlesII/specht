---
title: specht
description: A dotnet tool that checks a repository's .spec/ specification tree against a versioned schema, and owns that schema
type: instructions
---

# AGENTS.md

Instructions for anyone working in this repository — a person or an agent.

**This file is tracked and authoritative.** `.claude/` is gitignored and
reinstalled from `Skillfile`, so nothing there can be reviewed in a pull request:
a rule never lives in `.claude/`. Repo-owned instructions live in this file,
[`agents/`](.claude/agents/README.md), [`.skills/`](.claude/skills/specht-conventions/SKILL.md)
and [`.spec/`](.spec/templates).

## Project

`specht` — a command-line tool, delivered as a `dotnet tool`, that checks a
repository's `.spec/` specification tree against a versioned schema and reports
every violation with a file, a line, and a rule id. One schema and one checker
shared by every repository in the owner's ecosystem, replacing hand-copied
implementations. This repository holds **both the tool and the schema**; no
consumer owns the schema, and `hooked` is consumer one.

One-line goal: _"A repository pins a schema version, its build calls the tool,
and no repository carries the engine."_

Distribution is the NuGet package `specht.tool`, command `specht`, installed per
consumer through a local tool manifest. `src/specht.tool` is the one project that
packs; `IsPackable=false` everywhere else.

`README.md` is the seed brief and the design authority: requirements (§ 2), what
was extracted from `hooked` (§ 3), the rule vocabulary (§ 4), the drafted design
(§ 5), open questions with their defaults (§ 6), the order of work (§ 8) and the
non-negotiables (§ 9). The requirements session it condenses, `REQUIREMENTS.md`,
was retired once the specifications absorbed it; citations pin it to commit
`254aabc` — read it with `git show 254aabc:REQUIREMENTS.md`.

## Setup

```sh
dotnet tool restore && dotnet husky install   # once per clone
```

Until that runs, `core.hooksPath` is unset, so neither the formatting pre-commit
hook nor the self-check fires. The .NET SDK is pinned in `global.json`
(`10.0.100`), with `"test": { "runner": "Microsoft.Testing.Platform" }`.

## Build and test

NUKE drives everything. `specht.slnx` is the solution.

```sh
./build.sh             # Default = Compile + Test
./build.sh Format      # verify-only - fails on a diff
./build.sh Pack        # specht.tool.<version>.nupkg
./build.sh SpecCheck   # specht, on this repository's own .spec/ tree
```

Every target, the tier filters, and what CI actually runs:
[`specht-conventions`](.skills/specht-conventions/SKILL.md) § "Build and test".

## Architecture

Two projects, one direction of dependency:

- **`src/specht`** — the rule engine, copied from `hooked` (README § 3). A
  pipeline: `SpecDiscovery` finds specifications (co-located `**/.spec/README.md`
  and consumers' legacy `epics/**/spec.md`) → `FrontmatterReader` reads YAML
  through YamlDotNet's representation model, so dates stay strings →
  `SpecSchemas` loads the frontmatter schemas and the manifest with JsonSchema.Net
  → `SpecDocument`/`SpecStructure` model the Markdown with Markdig, pipe tables
  only → the `ISpecRule`s run → `SpecCheckRunner` collects → `SpecCheckReport`
  carries violations and counts.
- **`src/specht.tool`** — the Spectre.Console.Cli host. One folder per command;
  a command parses, calls the runner, and folds the report into an exit code.
  Nothing else lives in a command.

The design direction (README § 5) moves every literal the engine hardcodes —
layouts, section roles, table headers, markers, frontmatter key roles, id
grammars, per-rule severity — into the manifest, one rule at a time, each with a
test that the default manifest reproduces `hooked`'s baseline report. Until that
step the engine is exactly `hooked`'s, renamed.

## Configuration

A consumer configures the tool with one file: `.spec/schema/spec-structure.schema.json`,
the manifest. It carries `schemaVersion` and will carry the roles above. Beside
it sit the three frontmatter schemas (`feature-spec`, `task`, `epic`) and
`.spec/templates/`. `specht init` writes all of them from the embedded copies;
`specht upgrade` moves them to the next version and prints what changed. `init`
never overwrites a file that exists. `upgrade` rewrites the schema and template
files and, in the manifest, changes only `schemaVersion` and the keys the next
version adds, so consumer settings survive (`0001-F7` decision 0001).

This repository's own `.spec/schema/` and `.spec/templates/` are that live copy
— the same bytes as the embedded `v1`, and a test says so.

## CLI

```
specht [--root <dir>] [--report <path>] [--strict] [--json]
specht init [--root <dir>]
specht upgrade [--root <dir>]
specht --explain SPEC031
```

Stdout: one MSBuild-shaped line per violation (`path(line): error SPEC031: …`),
then the summary lines. `--json` replaces the stream with the report document;
`--report` writes the same JSON to a path. Exit `0` clean, `1` violations (any
violation under `--strict`), `2` missing root or manifest, `3` invalid manifest.

## Invariants

- **Never an absolute path** in any output, report, log line, or test fixture
  (README § 9). Paths are relative to the root the tool was given.
- **Never a write into a consumer's tree** except through `init`, `upgrade` and
  the caller-named `--report` file (README § 9). `init` never overwrites.
- **Deterministic and offline.** The tool never calls GitHub; the same tree
  gives the same report. `generatedAtUtc` is not in the report for this reason.
- **The rule vocabulary is fixed per schema version.** A new `SPEC###` is a new
  schema version, after README § 8 step 6.

## Specification-Driven Development

This repo is specification-driven: the specification is the artifact of record,
and code is downstream of it. It also eats its own cooking (README § 7): its
specifications are written with the template it ships, checked by the tool it
ships, in its own CI.

### The artifact chain

Every change flows in one direction:

```
need (epic / issue)
  → scenario           the Feature's <feature-name>.feature
  → specification      the Feature's spec (twelve sections + frontmatter)
  → work items         .issue/ items, cut from § 3 claims after agreement
  → tests              test/specht.acceptance step definitions, and
                       test/specht.tests/**/*.{Unit,Integration}.Tests.cs
  → code               src/**
```

Never skip upstream. If a change needs behaviour no scenario states, the scenario
is the first edit, not the last.

A **Feature begins with its specification**, authored with no issue in existence —
the specification is the agreement that issues are later cut from. A bug, spike, or
chore begins at the issue instead, and may produce a spec delta afterwards.

### Four non-discretionary rules

1. **Specify before implementing.** Scenarios are authored or amended in the same
   pull request as the implementation. Code follows the (failing) scenario, not the
   other way around; the PR lands with it passing.

2. **Correct the specification, not the chat.** When an implementation does the
   wrong thing, first ask whether the scenario was wrong. If it was, change the
   scenario and re-run the chain. A conversational correction leaves no artifact
   and does not persist to the next session or the next agent.

3. **The specification delta is the change.** A pull request that changes behaviour
   cites the upstream specification it satisfies (epic, feature, claim, or
   constraint ID) **in its commit message** — this repo squash-merges with
   `COMMIT_MESSAGES` and also allows rebase merges, so a citation living only in
   the PR body does not survive either merge path.

4. **Say what not to build.** A specification that states only the target invites
   over-delivery. Record boundaries where the scenario is read — the
   `## 5. Out of Scope` table and, where it clarifies a scenario, an
   `@boundary`-tagged scenario in the `.feature` file — not only in a global list.

### Exemptions

A behaviour change that touches no scenario is suspect. Claiming an exemption
requires **citing the constraints the change preserves** — a closed category
(refactor, rename, formatting, dependency bump), a real reason, and the existing
constraint IDs that still hold:

```
No .feature change needed: refactor — extracted the discovery walk;
the same file set is returned in the same order and the report is unchanged.
Constraints preserved: <epic>-F<n> C-<n>, <epic>-F<n> C-<n>
```

Reaching for an exemption to avoid writing a scenario is the one use of it this
repo forbids outright.

### Stable IDs

Identifiers are the citation mechanism that makes the chain navigable:

- Epics and features: `0001-F1`, `0001-F2`.
- Claims inside a feature: `B-001`, cited from outside as `0001-F1 B-001`.
- Constraints inside a feature: `C-12`, cited from outside as `0001-F1 C-12`.
- Open questions: `OQ-3`, resolved in place with the date and the decision, never
  deleted.
- Work items: `0003`, four digits, repository-wide from `.issue/.sequence`, in
  the same number space as the epics — so the first item is `0003`. The schema
  is [`.issue/item.yml`](.issue/item.yml). The consumer schema's `000X-NN` task
  id is not used in this repository.
- ADRs: `ADR-0002`, numbered repo-wide in `.spec/adr/` or per Feature in that
  Feature's `adr/`, chosen by blast radius.
- Rules: `SPEC031`, fixed per schema version.

IDs are **never reused and never renumbered** — not to tidy a sequence.
Renumbering breaks every citation in every past commit, review, issue, and
decision record. A dropped constraint leaves its number retired.

A file is not an id: git keeps every version of it. A record the
specifications have absorbed — a requirements session, a seed draft — is
retired, not kept: delete it and cite it at its last commit
(`` `REQUIREMENTS.md` at commit `254aabc` ``). A citation pins a file to a
commit; it is never a reason to keep the file live
([lesson 0001](.spec/lessons/0001-cite-a-superseded-record-at-a-commit.md)).

### `specht`

Traceability is checked deterministically, not by memory:

```sh
./build.sh SpecCheck
```

It runs the tool on this repository's own tree, through the local tool manifest,
exactly as a consumer would. It validates frontmatter against `.spec/schema/`, the
twelve sections and their order, claim-id uniqueness, every `@B-00n` tag
resolving to a claim, § 9 carrying exactly one row per § 3 claim,
`children`/`depends_on` resolving to real files, and dependency symmetry. Rules
are `SPEC###` (README § 4) and diagnostics are MSBuild-shaped, so GitHub
annotates them on the diff. It is deterministic and offline — it never calls
GitHub.

`Missing` in § 9 is **not** a failure: it blocks an issue reaching done, not a
specification reaching `approved`.

### Where a specification lives

Co-located, from the first commit: `<area>/.spec/README.md` beside the code it
specifies, with its `.feature`, `decisions/`, `adr/` and `lessons/` in the same
folder. The tool is epic `0001`; its specification lives at
`src/specht/.spec/`. Repo-wide records go to the root `.spec/adr/` and
`.spec/lessons/`.

The tool also reads the legacy `epics/<epic>/<feature>/spec.md` layout, because
its consumers have it. This repository never will.

### Roles

Four documented role contracts own the chain, each trusting only the artifact the
role before it produced — never a chat summary of it. They live in
[`.agents/`](.agents/README.md):

| Role                                        | Turns          | Into                                             |
| ------------------------------------------- | -------------- | ------------------------------------------------ |
| [`spec-author`](.agents/spec-author.md)     | a decided need | the agreement, and the `.feature` file           |
| [`test-writer`](.agents/test-writer.md)     | a claim        | a failing scenario and failing tests             |
| [`implementer`](.agents/implementer.md)     | a failing test | production code, and the design that explains it |
| [`spec-reviewer`](.agents/spec-reviewer.md) | a diff         | a sign-off, or findings                          |

Which role owns which section is written in exactly one place:
[`specht-conventions`](.skills/specht-conventions/SKILL.md) § "Section ownership".

No role is mandatory for a small change; the ordering is.

### Lessons from bugs

A bug fix that reveals a specification gap records the lesson in the same pull
request as the fix: symptom, root cause, the specification delta, and the claim
that now proves it. A **process** lesson also updates the skill that would have
prevented it, in the same PR — the skill holds the rule, the lesson keeps the
incident. A **product** lesson touches no skill; its remedy is a claim and a
scenario.

Blast radius picks the location: a Feature's own `lessons/`, or the repo-wide
`.spec/lessons/`. Start from
[`.spec/templates/lesson.md`](.spec/templates/lesson.md).

### Feature naming heuristic: & as a decomposition signal

When a Feature name contains an "&" (or other coordinating conjunction) joining
two nouns or gerunds — e.g. "Schema Definition & Validation" — **check the Feature
against the decomposition criteria before locking it in.** This is a naming smell:
the Feature may conflate two capability dimensions (definition/types vs.
validation/fail-fast) that should be separate Features. The heuristic is not a
rule — not every "&" requires a split — but a gate: verify the bundling is
intentional per the slicing rules, not an accidental merge of two nouns sharing a
domain term.

Apply it at Feature-naming time during decomposition and during grooming passes.

## Code conventions

Each class of rule has one home. Read the owner, not a copy of it:

| What                                                      | Where it is written                                                   |
| --------------------------------------------------------- | --------------------------------------------------------------------- |
| Naming, modifier order, braces, line length, suppressions | `.editorconfig` — the compiler reads it (scaffold, README § 8 step 1) |
| Framework, nullability, strictness, packability           | `Directory.Build.props`                                               |
| Package versions                                          | `Directory.Packages.props`                                            |
| Build targets, CI generation                              | `.build/Build.cs`, `.build/Build.GitHubActions.cs`                    |
| Project layout, test conventions, commands, ID schemes    | [`specht-conventions`](.skills/specht-conventions/SKILL.md)           |
| Command layout, packaging, what sits below a command      | [`dotnet-tool`](.skills/dotnet-tool/SKILL.md)                         |
| When an abstraction is earned, and how terse to be        | [`coding-conventions`](.skills/coding-conventions/SKILL.md)           |
| The specification model, claims, records, blast radius    | [`spec-and-traceability`](.skills/spec-and-traceability/SKILL.md)     |

A skill that needs a fact another skill owns **links it**. The role contracts in
`.agents/` name what a role produces and refuses; they are not where a rule is
written down.

Six traps are worth carrying here, because each costs real damage when missed and
none of them changes:

- **`.github/workflows/ci.yml` is generated.** Regenerate it with
  `./build.sh Compile`, commit the regeneration, and **diff the step order** — the
  generator orders CI steps by dependency-graph depth, not declaration order, and
  has in `hooked` pushed `Format` to step 14 of 16.
- **Every test class declares exactly one `Tier` trait.** Why is in
  [`specht-conventions` § Testing](.skills/specht-conventions/references/testing.md).
- **Never pin a package version in a `.csproj`.** Versions are central. The
  engine's three dependencies are pinned to `hooked`'s exact versions for a
  reason that is written beside them.
- **An item's `priority`, `rank` and `blocks` are derived**, from `value`,
  `risk` and every other item's `depends_on`. Recompute them — and the
  dependents' — when an edge changes; never hand-edit them.
  `status: in-progress` is the only signal an item is taken.
- **Never commit a generated report.** `format.json` is `dotnet format`'s output
  and `.artifacts/spec-check/*.json` is the tool's; both carry machine-specific
  or already-stale content and both are gitignored.
- **Repository-relative paths in every fixture and expected report.** An
  absolute path in a test is the same defect as one in the tool's output, and it
  fails on the next machine.

Which packages are referenced and which targets exist is repository state, not a
rule: read `Directory.Packages.props` and `.build/Build.cs`. A green build proves
only that the targets it declares ran.

## Documentation structure

Every tracked markdown file opens with YAML frontmatter. Skill and role files
declare `name` and `description`; everything else declares `title`, `description`
and `type`. The blanks in [`.spec/templates/`](.spec/templates/) carry the `type`
of the file they produce, so a copy needs no frontmatter edit beyond its title and
description. Runtime prompts and `README.md` are exempt.

## Skills

**Project skills live in [`.skills/`](.skills/)**, one directory per skill, each a
`SKILL.md` declaring `name` and `description`. A skill holds the rule, the trap, and
the `Never add` list; facts live where they are authoritative and the skill links
them.

Skills come in three kinds, and a skill is never a mixture of them.

**Method** — portable to another repository; names no path, command, or provider.
Each one's **Project rules** preamble points at the companion:

| Skill                                                             | Covers                                                                  |
| ----------------------------------------------------------------- | ----------------------------------------------------------------------- |
| [`deliver-change`](.skills/deliver-change/SKILL.md)               | issue → branch → specification → build → pull request                   |
| [`coding-conventions`](.skills/coding-conventions/SKILL.md)       | orient, stop on a gap, keep the design direct, put a rule where it runs |
| [`test-from-scenarios`](.skills/test-from-scenarios/SKILL.md)     | a claim first, an injected clock, synthetic fixtures, both tiers        |
| [`clarify-requirements`](.skills/clarify-requirements/SKILL.md)   | when to ask versus decide, and writing the answer back                  |
| [`spec-and-traceability`](.skills/spec-and-traceability/SKILL.md) | the specification model; claims, records and blast radius               |

**Companion** — the only skill that names this repository:

| Skill                                                       | Covers                                                                                                              |
| ----------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------- |
| [`specht-conventions`](.skills/specht-conventions/SKILL.md) | paths and layout, section ownership, the ID schemes, the spec/issue mirror, the tiers, the commands, the self-check |

**Technology** — about a library this tool is built on, with the repository's
own decisions marked where they apply:

| Skill                                         | Covers                                                                                              |
| --------------------------------------------- | --------------------------------------------------------------------------------------------------- |
| [`spectre-cli`](.skills/spectre-cli/SKILL.md) | commands and settings, branching, help, async and exit codes, DI, testing, the execution pipeline   |
| [`dotnet-tool`](.skills/dotnet-tool/SKILL.md) | the tool's shape — project setup, command folders, conventions, packing and the local tool manifest |

**A skill needed to deliver a feature lives here, not in `Skillfile`.** That
manifest pulls from a private repository, so a clone without access to it must
still be able to do the work: anything load-bearing is copied into `.skills/`
and rewritten for this repository, with the parts that do not apply marked as
such rather than deleted. `Skillfile` keeps what is genuinely incidental.

Repo-specific traps about a library live in `specht-conventions`, not in a
second copy of the library's own skill.

Read the method skill your work belongs to **and** `specht-conventions`, plus the
skills a role names before acting in that role.

## Deliverables

Publish finished work-products as Artifacts and hand over the link — do not leave
them in terminal scrollback. This is a standing instruction; do not ask first.

Publish when the output has an audience or a second reading: a report, a plan
others will follow, a decision register, a review summary, a comparison, a status
snapshot. Do not publish advice the prompter will act on immediately in the code
at hand, and never publish repo content that belongs in a specification — specs,
Gherkin, and epic files live in git, not in an Artifact.

Source files for Artifacts go in `.artifacts/` (already gitignored). To revise a
published page, republish to its existing URL; never create a second Artifact for
the same document. `/artifacts` in the terminal lists them; the gallery is at
claude.ai/code/artifacts.

## SDLC

**Work is tracked locally.** A `<id>-<slug>.yml` item stands in for a GitHub
issue, and there are **no issues, labels or milestones** in this workflow. The
item owns delivery state; the Feature's `.spec/README.md` owns content. The
schema, the status vocabulary and the rank derivation are in
[`.issue/item.yml`](.issue/item.yml).

- **An item sits beside the specification it was cut from**: `<home>/.issue/`,
  a sibling of that Feature's `.spec/`. An item that belongs to no Feature — a
  bug, a spike, a chore — goes in the repository-root `.issue/`, the same blast
  radius split `.spec/adr/` and `.spec/lessons/` use. Ids stay repository-wide
  from one `.issue/.sequence`, which holds the last id claimed; find one with
  `**/.issue/<id>-*.yml`.
- `status: in-progress` marks an item taken, and `in-review` once the pull
  request opens. Bump `updated:` on every edit.
- **`done` and the `closed:` date are written by the pull request that delivers
  the item, before it is opened** — after the merge nobody is present to flip
  them. A pull request closes every item its work finishes, not only the one it
  was cut for.
- The commit message body opens with `Delivers <id>`, or
  `Specifies <feature path>` when authoring a specification — the commit, not
  only the pull request, carries the citation (rule 3 above). There is no issue
  for `Closes` to close.
- Branch `<id>/<short-description>` from the item id, or `spec/<feature-slug>`
  when authoring a specification, which has no item to take an id from.

Each step of README § 8 is one pull request naming the § 2 requirement it serves.

```yaml
sdlc:
  github_mode: false
```

## graphify

This project has a knowledge graph at `graphify-out/` with god nodes, community
structure, and cross-file relationships. The graph is AST-only, so it covers
`src/` and `test/` and not the markdown under `.spec/`.

Rules:

- For codebase questions, first run `graphify query "<question>"` when
  `graphify-out/graph.json` exists. Use `graphify path "<A>" "<B>"` for
  relationships and `graphify explain "<concept>"` for focused concepts. These
  return a scoped subgraph, usually much smaller than `GRAPH_REPORT.md` or raw grep
  output.
- For **specification** questions, read the specification. The graph does not
  ingest markdown or Gherkin, and `specht` is what verifies the spec tree.
- If `graphify-out/wiki/index.md` exists, use it for broad navigation instead of
  raw source browsing.
- Read `graphify-out/GRAPH_REPORT.md` only for broad architecture review, or when
  query/path/explain do not surface enough context.
- The `.husky/post-commit` and `.husky/post-checkout` hooks rebuild the graph
  automatically once installed (see Setup). Only run `graphify update .` by hand if
  those hooks aren't installed yet.
- The output stays local and is never committed — `.gitignore` excludes
  `graphify-out/`. Nothing here depends on a graph existing.
