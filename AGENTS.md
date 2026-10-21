---
title: specht
description: A dotnet tool that checks a repository's .spec/ specification tree against a versioned schema, and owns that schema
type: instructions
---

# AGENTS.md

Instructions for anyone working in this repository — a person or an agent.

**This file is tracked and authoritative.** Shared sources live under `.agents/`.
`Skillfile` and `Skillfile.lock` are tracked; tool-local installed copies under
`.claude/` and `.codex/` are gitignored. Repo-owned instructions live in this file,
[`.agents/roles/`](.agents/roles/README.md), [`.agents/skills/`](.agents/skills/specht-conventions/SKILL.md),
[`.spec/`](.spec/templates) and [`.specht/`](.specht/templates).

## Project

`specht` — a command-line tool, delivered as a `dotnet tool`, that checks a
repository's `.spec/` specification tree against a versioned schema and reports
every violation with a file, a line, and a rule id. One schema and one checker
shared by every repository in the owner's ecosystem, replacing hand-copied
implementations. This repository holds **both the tool and the schema**; no
consumer owns the schema; every consumer installs the tool.

One-line goal: _"A repository pins a schema version, its build calls the tool,
and no repository carries the engine."_

Distribution is the NuGet package `specht.tool`, command `specht`, installed per
consumer through a local tool manifest; a global install is the second
documented path, for a shell (`0001-F2` decision 0009). `src/tool` is the one
tool project that packs. `src/codeanalysis` packs the companion
`Specht.CodeAnalysis` analyzer package; `IsPackable=false` elsewhere.

`README.md` is the public front page: what the tool is, why, how to install and
use it, and the layout; the reference pages are under `docs/`. The design
authority is the seed brief,
[`.spec/brief.md`](.spec/brief.md), cited as `brief § N`: requirements (§ 2), the
repository-owned baseline (§ 3), the rule vocabulary (§ 4), the drafted design
(§ 5), open questions with their defaults (§ 6), the order of work (§ 8) and the
non-negotiables (§ 9). The requirements session it condenses, `REQUIREMENTS.md`,
was retired once the specifications absorbed it; citations pin it to commit
`254aabc` — read it with `git show 254aabc:REQUIREMENTS.md`.

## Setup

```sh
dotnet tool restore && dotnet husky install   # once per clone
python3 .agents/install.py                   # shared skills and role registrations
```

The complete tool restore reads the published checker from GitHub Packages;
supply the source credential through `NuGetPackageSourceCredentials_specht`.
Without that credential, `dotnet restore specht.slnx` and `dotnet build specht.slnx`
still restore and build the solution. The NUKE build also restores local tools.

Until that runs, `core.hooksPath` is unset, so neither the formatting pre-commit
hook nor the self-check fires. The .NET SDK is pinned in `global.json`
(`11.0.100-rc.2.26504.105`, `allowPrerelease`), with
`"test": { "runner": "Microsoft.Testing.Platform" }`. Every project but
`.build` targets `net10.0` and `net11.0` (`0001-F2` B-045), so a clone needs
the .NET 10 runtime beside the .NET 11 SDK.

## Build and test

NUKE drives everything. `specht.slnx` is the solution.

```sh
./build.sh             # Default = Compile + CodeAnalysis + Test
./build.sh Format      # verify-only - fails on a diff
./build.sh CodeAnalysis # pack the current analyzer and check all three source projects
./build.sh Pack        # specht.tool and Specht.CodeAnalysis packages
./build.sh Specht   # specht, on this repository's own .spec/ tree
```

Every target, the tier filters, and what CI actually runs:
[`specht-conventions`](.agents/skills/specht-conventions/SKILL.md) § "Build and test".

## Architecture

Two projects, one direction of dependency:

- **`src/specht`** — the rule engine, imported at commit `e7dba24` (brief § 3). A
  pipeline: `SpecDiscovery` finds specifications (co-located `**/.spec/README.md`
  and consumers' legacy `epics/**/spec.md`) → `FrontmatterReader` reads YAML
  through YamlDotNet's representation model, so dates stay strings →
  `SpecSchemas` loads the frontmatter schemas and the manifest with JsonSchema.Net
  → `SpecDocument`/`SpecStructure` model the Markdown with Markdig, pipe tables
  only → the `ISpecRule`s run → `SpechtRunner` collects → `SpechtReport`
  carries violations and counts. It is foldered by Feature, with a shared
  `Model/`; the layout is
  [`dotnet-tool` § Vertical Slice](.agents/skills/dotnet-tool/references/vertical-slice.md).
- **`src/tool`** — the Spectre.Console.Cli host. One folder per command;
  a command parses, calls the runner, and folds the report into an exit code.
  Nothing else lives in a command.

That is the engine as built. ADR-0010 describes a target architecture;
[ADR-0014](.spec/adr/0014-architecture-rollout-requires-benchmark-evidence.md)
gates further engine and host migration on repository benchmark evidence that
justifies the added cost. The source-generated mediator candidate is
martinothamar/Mediator, to be evaluated before adoption. A feature does not
waive the gate or the staged prerequisites. Existing stores, parsers and
composition remain the implementation basis until that evidence is agreed.

The design direction (brief § 5) moves every literal the engine hardcodes —
layouts, section roles, table headers, markers, frontmatter key roles, id
grammars, per-rule severity — into the manifest, one rule at a time, each with a
test that the default manifest reproduces the golden baseline report (`0001-F1`
B-004, decision 0004). Until that step the engine preserves the imported baseline verdicts.

## Configuration

A consumer configures the tool with one file: `.specht/manifest.json`, the
manifest. It carries `schemaVersion` and will carry the roles above. It is
configuration, not a JSON Schema. The three frontmatter schemas sit under
`.specht/schema/frontmatter/`, one `<kind>.schema.json` each (`feature`,
`task`, `epic`), and the templates under `.specht/templates/` (`0001-F5`
decision 0011).

The check selects local frontmatter schemas with `--schema-source disk`, or
with `"schemaSource": "disk"` in a manifest pinned to schema `0.2.0` or later.
The CLI selection overrides a valid manifest selection and works with every
shipped pin. Files live under `.specht/schema/frontmatter/` with names from
`frontmatterSchemas`; Feature and task are required, epic only when the manifest
explicitly declares its grammar. A recorded `spechtSchema` retains hash
verification under `disk`; explicit `embedded` contradicts that record.

`specht init` writes the manifest and nothing else. `--schemas` adds the
frontmatter schemas, `--templates` adds the templates and `--all` adds both;
the epic schema is written only with the schemas, and only with `--epics` or
beside a manifest that declares the epic grammar (`0001-F4` decision 0004,
B-029 to B-032). Each file comes from the embedded copies - the frontmatter
schemas from an upstream schema source instead, where the manifest records one
(`0001-F4` B-014) - and `init` never overwrites a file that exists.

The layout and the three part options are built (items `0185` and `0186`).
`--epics` is specified and not built; it has no item (`0001-F5` OQ-18).

`schemaVersion` is a `major.minor.patch` schema version, numbered apart from
the `specht.tool` package version; the first is `0.1.0`, and a manifest with no
key means it. A patch changes no tree's verdict; from schema `1.0.0` on a minor
only accepts more and a major can fail a tree that passed, and before it a
`0.minor` step can fail one too. A published version is never changed in
place; from schema `1.0.0` on it is in every later tool, and before it a later
tool may stop shipping a `0.x.y` (`0001-F7` C-1, C-11, decision 0007). A pin is
exact. `specht schema pin <version>` moves it to
any shipped version, up or down, and changes nothing else; `specht upgrade`
then rewrites the schema files, and every template the manifest does not name
as the consumer's, to match the pin and prints what changed. In the manifest
`upgrade` adds only the keys the pinned version defines that are missing, so
consumer settings survive (`0001-F7` decisions 0001, 0005 and 0006).

This repository's own `.specht/` is that live copy
— the same bytes as the newest embedded version for the tool-owned files (the three
frontmatter schemas and the templates), and the same manifest on its tool-owned
keys, with rule settings and other consumer configuration excluded; a test says
so (`0001-F4` B-004).

## CLI

```
specht [--root <dir>] [--report <path>] [--strict] [--json] [--schema-source <embedded|disk>]
specht init [--root <dir>] [--schemas] [--templates] [--all] [--epics]
            [--upstream <address>] [--upstream-version <label>] [--upstream-hash <kind=hash>]...
specht schema list
specht schema pin [<version>] [--root <dir>]
specht schema list [--root <dir>]
specht schema pin [version] [--root <dir>]
specht frontmatter --kind <kind> (<path> | --stdin) [--root <dir>]
specht evidence [--root <dir>] --evidence <path>... [--run <id>] [--json] [--report <path>]
specht upgrade [--root <dir>]
specht --explain SPEC031
specht --version
specht manifest [--root <dir>] [--defaults] [--json]
specht rules [--root <dir>] [--json]
```

Stdout: one MSBuild-shaped line per violation
(`path(line): error SPEC031: … [identifier]`, the brackets absent when the
violation has no identifier), then the summary lines. `--json` replaces the stream with the report document;
`--report` writes the same JSON to a path. Exit `0` clean, `1` violations (any
violation under `--strict`), `2` missing root or manifest, `3` invalid manifest,
`4` a thing named on the command line not found (such as an `--explain` rule id
or a schema version to pin).

As the only argument, `specht --version` prints one line, the informational
version the tool's assembly states - the package version and the commit - and
exits `0`; it needs no manifest and writes nothing (`0001-F2` B-040 to B-043).
Beside another argument it is not decided (`0001-F2` OQ-8).

`specht schema list` lists the shipped versions in semantic-version order and
marks the root's selected version with ` (pinned)`. Without a manifest the list is
unmarked. `specht schema pin <version>` changes only the manifest's exact pin,
up or down. With no version, an interactive Spectre.Console selector uses arrow
keys and Enter; without a terminal the command prints an explicit-version hint
and exits `2`. Both commands are built (`0001-F7`, items `0121` to `0123`, `0125`).

`specht evidence` reconciles explicitly supplied build and runner exports against
current claims selected by the consumer's `codeAnalysis` policy. It requires
current inputs, required links and passing outcomes by default and emits a
separate versioned report (`0201-F5`, item `0246`).

`specht upgrade`, the check command's `--report` and the `init --epics` option are
specified and not built (`0001-F7`, `0001-F3`, `0001-F4`). What the tool accepts today is
[`docs/cli.md`](docs/cli.md), which a test holds to its help (`0002-F1`).

`specht --explain <SPEC###>` prints the entire rule page, including YAML frontmatter,
for the manifest's exact pin without running a check. It is built (item `0040`).

`specht manifest` and `specht rules` are specified and not built (`0001-F9`,
`0001-F10`). A command that sets one rule's level is specified and not yet
named (`0001-F11` OQ-1).

## Invariants

- **Never an absolute path** in any output, report, log line, or test fixture
  (brief § 9). Paths are relative to the root the tool was given.
- **Never a write into a consumer's tree** except through `init`, `upgrade`,
  the pin command, which changes only `schemaVersion` in the manifest
  (`0001-F7` decision 0005), the set command, which changes only one rule's
  level in the manifest's `rules` (`0001-F11` decision 0001), the caller-named
  `--report` file and, from epic
  `0101`, `format`, which only moves whole table rows and frontmatter keys
  (brief § 9; `0101-F5` decision 0001). `init` never overwrites.
- **Deterministic and offline.** The check never touches the network and never
  calls GitHub; only `init` and `upgrade` may fetch, and only an upstream
  schema source the manifest records or the same init run will record
  (`0001-F7` decision 0004; `0001-F4` decision 0003, ratified 2026-10-12). The same tree
  gives the same report. `generatedAtUtc` is not in the report for this reason.
- **The rule vocabulary is fixed per schema version.** A new `SPEC###` is a new
  schema version - a major from schema `1.0.0` on, at least a `0.minor` before
  it (`0001-F7` C-11) - after brief § 8 step 6.

## Specification-Driven Development

This repo is specification-driven: the specification is the artifact of record,
and code is downstream of it. It also eats its own cooking (brief § 7): its
specifications are written with the template it ships, checked by the tool it
ships, in its own CI.

### The artifact chain

Every change flows in one direction:

```
need (epic / issue)
  → scenario           the Feature's <feature-name>.feature
  → specification      the Feature's spec (twelve sections + frontmatter)
  → work items         .issue/ items, cut from § 3 claims after agreement
  → tests              test/acceptance step definitions, and
                       test/tests/**/*.{Unit,Integration}.Tests.cs
  → code               src/**
```

Never skip upstream. If a change needs behaviour no scenario states, the scenario
is the first edit, not the last.

A **Feature begins with its specification**, authored with no issue in existence —
the specification is the agreement that issues are later cut from. A bug, spike, or
chore begins at the issue instead, and may produce a spec delta afterwards. A
refactor begins there too and produces none; its item cites the constraints it
preserves, as an exemption does.

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
  is [`.spec/templates/item.yml`](.spec/templates/item.yml). The consumer schema's `000X-NN` task
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
./build.sh Specht
```

It builds and packs the working checker, installs it through an isolated local
tool manifest under .artifacts/self-check/, and checks this repository's own
tree (0001-F2 decision 0014). The hook uses its staged snapshot. The committed
published package pin remains the consumer installation contract. It validates frontmatter against `.specht/schema/frontmatter/`, the
twelve sections and their order, claim-id uniqueness, every `@B-00n` tag
resolving to a claim, § 9 carrying exactly one row per § 3 claim,
`children`/`depends_on` resolving to real files, and dependency symmetry. Rules
are `SPEC###` (brief § 4) and diagnostics are MSBuild-shaped, so GitHub
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

Five documented role contracts own the chain, each trusting only the artifact the
role before it produced — never a chat summary of it. They live in
[`.agents/roles/`](.agents/roles/README.md):

| Role                                              | Turns          | Into                                             |
| ------------------------------------------------- | -------------- | ------------------------------------------------ |
| [`spec-author`](.agents/roles/spec-author.md)     | a decided need | the agreement, and the `.feature` file           |
| [`test-writer`](.agents/roles/test-writer.md)     | a claim        | a failing scenario and failing tests             |
| [`implementer`](.agents/roles/implementer.md)     | a failing test | production code, and the design that explains it |
| [`benchmarker`](.agents/roles/benchmarker.md)     | a concern      | a benchmark, and the finding it measured         |
| [`spec-reviewer`](.agents/roles/spec-reviewer.md) | a diff         | a sign-off, or findings                          |

Which role owns which section is written in exactly one place:
[`specht-conventions`](.agents/skills/specht-conventions/SKILL.md) § "Section ownership".

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
[`.specht/templates/lesson.md`](.specht/templates/lesson.md).

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

| What                                                      | Where it is written                                                      |
| --------------------------------------------------------- | ------------------------------------------------------------------------ |
| Naming, modifier order, braces, line length, suppressions | `.editorconfig` — the compiler reads it (scaffold, brief § 8 step 1)     |
| Framework, nullability, strictness, packability           | `Directory.Build.props`                                                  |
| Package versions                                          | `Directory.Packages.props`                                               |
| Build targets, CI generation                              | `.build/SpechtBuild*.cs`                                                 |
| Project layout, test conventions, commands, ID schemes    | [`specht-conventions`](.agents/skills/specht-conventions/SKILL.md)       |
| Command layout, packaging, what sits below a command      | [`dotnet-tool`](.agents/skills/dotnet-tool/SKILL.md)                     |
| When an abstraction is earned, and how terse to be        | [`coding-conventions`](.agents/skills/coding-conventions/SKILL.md)       |
| The specification model, claims, records, blast radius    | [`spec-and-traceability`](.agents/skills/spec-and-traceability/SKILL.md) |

A skill that needs a fact another skill owns **links it**. The role contracts in
`.agents/roles/` name what a role produces and refuses; they are not where a rule is
written down.

Seven traps are worth carrying here, because each costs real damage when missed and
none of them changes:

- **`.github/workflows/ci.yml` is generated.** Regenerate it with
  `./build.sh Compile`, commit the regeneration, and **diff the step order** — the
  generator orders CI steps by dependency-graph depth, not declaration order, and
  can push `Format` late in the workflow.
- **Every test class declares exactly one `Tier` trait.** Why is in
  [`specht-conventions` § Testing](.agents/skills/specht-conventions/references/testing.md).
- **Never pin a package version in a `.csproj`.** Versions are central. The
  engine's three baseline dependencies are pinned to the baseline dependency versions
  for a reason that is written beside them; its fourth, System.IO.Abstractions,
  is not.
- **An item stores no `priority`, `rank` or `blocks`.** The queue script
  computes rank and priority on every run, from `value`, `risk`, `depends_on`,
  `parent` and `status`; never write one into an item.
  `status: in-progress` is the only signal an item is taken.
- **An id is not reserved until it merges.** Before a branch takes a number,
  check `main` _and_ every open pull request: two branches cut from one `main`
  take the same next id, and git reports nothing. The rule is in
  [`specht-conventions` § Delivery](.agents/skills/specht-conventions/references/delivery.md).
- **Never commit a generated report.** `format.json` is `dotnet format`'s output
  and `.artifacts/spec-check/*.json` is the tool's; both carry machine-specific
  or already-stale content and both are gitignored.
- **Repository-relative paths in every fixture and expected report.** An
  absolute path in a test is the same defect as one in the tool's output, and it
  fails on the next machine.

Which packages are referenced and which targets exist is repository state, not a
rule: read `Directory.Packages.props` and `.build/SpechtBuild.cs`. A green build proves
only that the targets it declares ran.

## Documentation structure

No vendor-specific AI names in any artifacts: documentation, work items, decision
records, review/sign-off rows, reports, comments and commit messages. Required
role fields and ownership stamps describe responsibilities, never vendor branding
or authorship credits.

Every tracked markdown file opens with YAML frontmatter. Skill and role files
declare `name` and `description`; everything else declares `title`, `description`
and `type`. The blanks in [`.specht/templates/`](.specht/templates/) carry the `type`
of the file they produce, so a copy needs no frontmatter edit beyond its title and
description. Runtime prompts, `README.md` and `src/tool/README.md`, the readme
the package carries, are exempt.

## Skills

**Project skills live in [`.agents/skills/`](.agents/skills/)**, one directory per skill, each a
`SKILL.md` declaring `name` and `description`. A skill holds the rule, the trap, and
the `Never add` list; facts live where they are authoritative and the skill links
them.

Skills come in three kinds, and a skill is never a mixture of them.

**Method** — portable to another repository; names no path, command, or provider.
Each one's **Project rules** preamble points at the companion:

| Skill                                                                    | Covers                                                                  |
| ------------------------------------------------------------------------ | ----------------------------------------------------------------------- |
| [`deliver-change`](.agents/skills/deliver-change/SKILL.md)               | issue → branch → specification → build → pull request                   |
| [`next`](.agents/skills/next/SKILL.md)                                   | the next item, what it waits on, and which items can run in parallel    |
| [`coding-conventions`](.agents/skills/coding-conventions/SKILL.md)       | orient, stop on a gap, keep the design direct, put a rule where it runs |
| [`test-from-scenarios`](.agents/skills/test-from-scenarios/SKILL.md)     | a claim first, an injected clock, synthetic fixtures, both tiers        |
| [`clarify-requirements`](.agents/skills/clarify-requirements/SKILL.md)   | when to ask versus decide, and writing the answer back                  |
| [`spec-and-traceability`](.agents/skills/spec-and-traceability/SKILL.md) | the specification model; claims, records and blast radius               |

**Companion** — the only skill that names this repository:

| Skill                                                              | Covers                                                                                                              |
| ------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------- |
| [`specht-conventions`](.agents/skills/specht-conventions/SKILL.md) | paths and layout, section ownership, the ID schemes, the spec/issue mirror, the tiers, the commands, the self-check |

**Technology** — about a library this tool is built on, with the repository's
own decisions marked where they apply:

| Skill                                                        | Covers                                                                                                                                       |
| ------------------------------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------- |
| [`spectre-cli`](.agents/skills/spectre-cli/SKILL.md)         | commands and settings, branching, help, async and exit codes, DI, testing, the execution pipeline                                            |
| [`dotnet-tool`](.agents/skills/dotnet-tool/SKILL.md)         | the tool's shape — project setup, command folders, conventions, packing and the local tool manifest                                          |
| [`benchmarkdotnet`](.agents/skills/benchmarkdotnet/SKILL.md) | measuring .NET code — benchmark classes, parameters, setup, toolchains, the memory diagnoser, reading the summary, and the measurement traps |

**A skill needed to deliver a feature lives in `.agents/skills/`.** The tracked
`Skillfile` declares only those local skill directories for both project-local
tools. Run `python3 .agents/install.py` after a clone or an edit to these sources;
it runs Skillfile for skills and installs the contracts in `.agents/roles/`
directly as Markdown and native TOML agents. Agent installation does not depend
on Skillfile. See [shared setup](.agents/README.md).

The tracked skills are maintained for this repository and ship with it.
Their paths, commands and delivery rules describe `specht`; no consumer
repository or local checkout supplies these instructions.

**Optional library guides** may come from the private `littlestechnology/.agents`
repository, but the default installation has no remote dependencies. Maintain
optional personal guides separately from the repository's required local set;
they must never replace a repository-owned skill with the same name. The
repository's own traps for library surfaces live in `specht-conventions`.

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
issue, and there are **no GitHub issues or milestones** in this workflow. PR labels
categorize release notes only; they never track delivery state. The
item owns delivery state; the Feature's `.spec/README.md` owns content. The
schema, the status vocabulary and the rank definition are in
[`.spec/templates/item.yml`](.spec/templates/item.yml): rank is computed on
every run, never stored, and orders work by what ships the product, then a
Feature, soonest.

- **An item sits beside the specification it was cut from**: `<home>/.issue/`,
  a sibling of that Feature's `.spec/`. An item that belongs to no Feature — a
  bug, a spike, a chore, a refactor spanning Features — goes in the
  repository-root `.issue/`, the same blast radius split `.spec/adr/` and
  `.spec/lessons/` use. Ids stay repository-wide
  from one `.issue/.sequence`, which holds the last id claimed; find one with
  `**/.issue/<id>-*.yml`.
- `status: in-progress` marks an item taken, and `in-review` once the pull
  request opens. Bump `updated:` on every edit.
- **`done` and the `closed:` date are written by the pull request that delivers
  the item, before it is opened** — after the merge nobody is present to flip
  them. A pull request closes every item its work finishes, not only the one it
  was cut for.
- Every commit's body opens with `Delivers <id>`, or
  `Specifies <feature path>` when authoring a specification — the commit, not
  only the pull request, carries the citation (rule 3 above), and a commit with
  no body carries none. A squash of several commits lists them, and the
  citation inside that list counts. Merge without overriding the squash body,
  then read the merged commit on `main`: a citation it lacks is recorded, never
  restored by rewriting history
  ([`specht-conventions` § Delivery](.agents/skills/specht-conventions/references/delivery.md)).
  There is no issue for `Closes` to close.
- Branch `<id>/<short-description>` from the item id, or `spec/<feature-slug>`
  when authoring a specification, which has no item to take an id from.

Each step of brief § 8 is one pull request naming the § 2 requirement it serves.

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
