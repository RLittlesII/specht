---
name: specht-conventions
description: This repository's conventions — paths and layout, the delivery flow through local .issue/ work items, the test tiers, the ID schemes, section ownership, and how the tool checks its own specification tree. Use for any change in this repository, alongside the method skill the work belongs to.
---

# `specht` conventions

The companion skill: **the only skill that names this repository.** Each method
skill states a portable rule; this one says where that rule lands here. Read the
method skill your work belongs to _and_ this one.

| Area         | Extends                                                      | Detail                                                   |
| ------------ | ------------------------------------------------------------ | -------------------------------------------------------- |
| Coding       | [`coding-conventions`](../coding-conventions/SKILL.md)       | [references/coding.md](references/coding.md)             |
| Delivery     | [`deliver-change`](../deliver-change/SKILL.md)               | [references/delivery.md](references/delivery.md)         |
| Next item    | [`next`](../next/SKILL.md)                                   | [references/delivery.md](references/delivery.md)         |
| Testing      | [`test-from-scenarios`](../test-from-scenarios/SKILL.md)     | [references/testing.md](references/testing.md)           |
| Questions    | [`clarify-requirements`](../clarify-requirements/SKILL.md)   | [references/questions.md](references/questions.md)       |
| Specs        | [`spec-and-traceability`](../spec-and-traceability/SKILL.md) | [references/specs.md](references/specs.md)               |
| Benchmarking | [`benchmarkdotnet`](../benchmarkdotnet/SKILL.md)             | [references/benchmarking.md](references/benchmarking.md) |

## Read before starting

- `.spec/brief.md` — the seed brief and the project-level design authority,
  cited as `brief § N`: what the
  tool is (§ 1), the requirements as decided (§ 2), what was extracted from
  `hooked` and why it is never regenerated (§ 3, § 9), the rule vocabulary
  (§ 4), the drafted design (§ 5), the open questions and their defaults (§ 6),
  and the order of work (§ 8).
- `AGENTS.md` — the artifact chain and the four non-discretionary rules.
- The Feature's specification, its `.feature` file, and its `lessons/`.
- The repo-wide lessons in `.spec/lessons/`, once any exist.

## Layout

The target layout. brief § 8 step 1 creates the scaffold; until it lands, a
path below that does not exist yet is a plan, not a fact. brief § 3 names the
engine project `src/SpecCheck`; the decided name is `specht` (brief § 6, first
row).

```
specht.slnx                    the solution - a project not listed here is not built
Directory.Build.props          repo-wide MSBuild: nullable, warnings as errors, IsPackable=false
Directory.Packages.props       central package versions - the only place a version is written
global.json                    the pinned SDK and the Microsoft.Testing.Platform runner
version.json                   the package version line, read by nbgv - the only source of a version
.config/dotnet-tools.json      local tools: husky, nbgv, nuke, and specht itself once published
build.sh / build.cmd           entry to the NUKE build

src/
  specht/                      specht - the rule engine: discovery, readers, schema loader,
                               rules, runner, report. Copied from hooked (brief § 3).
    .spec/                     this Feature's specification, .feature and records (epic 0001)
    Rules/docs/v1/             embedded rule pages, one SPEC###.md per rule id of the version
  specht.tool/                 specht.tool - the CLI host; PackAsTool, command `specht`
    Features/<Command>/        one folder per Spectre command - dotnet-tool owns the inside
    schema/v1/                 embedded shipping copy of the schema set
    templates/v1/              embedded shipping copy of the templates

test/
  specht.tests/                specht.tests - unit and integration tests; SpecTree builder
  specht.acceptance/           specht.acceptance - Reqnroll over src/**/.spec/*.feature

.build/                        the NUKE build project - targets and CI generation; tooling, not product
.nuke/                         NUKE parameters and the generated build schema
.husky/                        git hooks - pre-commit verifies staged .cs and .md through Format; specht from 0062
.github/                       workflows (ci, publish), renovate.json, settings checklist; issue
                               templates for outside reports only - github_mode stays false and a
                               maintainer turns a report into a .issue/ item; the only label is
                               Renovate's `dependencies` on update pull requests (epic 0055)
.issue/                        item.yml (the work-item schema), .sequence, and items
                               belonging to no Feature; a Feature's items sit in
                               <home>/.issue/ beside its .spec/
epics/                         one folder per epic - ids share the items' number space

.spec/                         repo-wide adr/, lessons/, templates/, schema/, and
                               brief.md - the seed brief and design authority (brief § N)
                               schema/ and templates/ are the live copy specht checks
                               itself with - the same bytes as the embedded v1 for the
                               tool-owned files; the manifest on its tool-owned keys
.claude/agents/                the five role contracts - tracked
.claude/skills/                these skills - tracked; the rest of .claude/ is gitignored
                               (no Skillfile is committed)
AGENTS.md                      the entry point for agents
README.md                      the public front page - what, why, status, usage
```

**Everything below `src/specht.tool/Features/` is
[`dotnet-tool` § Vertical Slice](../dotnet-tool/references/vertical-slice.md)** —
how a command folder is laid out, how it joins `Program.cs`, and what belongs in
the engine instead. Not restated here.

## Section ownership

The one place this is written. Each section of a Feature's specification has
exactly one owning role; a role that needs another's section changed escalates and
does not write there.

| Section                                 | Owner                                   |
| --------------------------------------- | --------------------------------------- |
| 1. Business Goal                        | `spec-author`                           |
| 2. User Needs (+ Assumptions)           | `spec-author`                           |
| 3. Acceptance Criteria                  | `spec-author`                           |
| 4. Constraints                          | `spec-author`                           |
| 5. Out of Scope                         | `spec-author`                           |
| 6. Concern Separation                   | `implementer`                           |
| 7. Technical Design                     | `implementer`                           |
| 8. Testing Strategy                     | `test-writer`, except `### Performance` |
| 8. Testing Strategy — `### Performance` | `benchmarker`                           |
| 9. Traceability Matrix                  | `test-writer`                           |
| 10. Lessons / Spec Deltas               | `spec-author`                           |
| 11. Open Questions                      | whoever is blocked                      |
| 12. Sign-off                            | `spec-reviewer`                         |
| Tasks                                   | `spec-author`, after agreement          |
| Scoring                                 | derived — recompute, never hand-edit    |

The `.feature` file belongs to `spec-author`; its step definitions belong to
`test-writer`. Role contracts are in [`.claude/agents/`](../../agents/README.md).

## ID schemes

| Scheme        | Form              | Scope                                      | Cited from outside as |
| ------------- | ----------------- | ------------------------------------------ | --------------------- |
| Epic          | `0001`            | repository                                 | `0001`                |
| Feature       | `F1`, `F5b`       | its epic                                   | `0001-F1`             |
| Claim         | `B-001`, `B-006b` | its Feature                                | `0001-F1 B-001`       |
| Constraint    | `C-12`            | its Feature                                | `0001-F1 C-12`        |
| Open question | `OQ-3`            | its Feature                                | `0001-F1 OQ-3`        |
| Work item     | `0004`            | repository, shared with epics              | `0004`                |
| ADR           | `ADR-0002`        | repo or Feature, by blast radius           | `ADR-0002`            |
| `specht` rule | `SPEC031`         | the rule engine, versioned with the schema | `SPEC031`             |

Always carry the prefix in prose — several sequences have a twelfth member.

**Ids are never reused and never renumbered.** Renumbering breaks every citation
in every past commit, review, issue and sibling specification. A dropped claim
leaves its number retired.

That holds from the moment an id is on `main`. Until then a number is a claim
other branches cannot see; how one is checked, reserved, and renumbered when two
branches took it is in [references/delivery.md](references/delivery.md)
§ "Reserve an id before a branch claims it".

The rule vocabulary is fixed per schema version (brief § 4). A new `SPEC###` is
a new schema version, after brief § 8 step 6 — never a quiet addition.

Known gap in the model the tool checks: § 4's ID column is an explicit `C-nn` in
only some consumers' specs; the rest carry a bare ordinal, so those citations
resolve by counting rows. The template uses an explicit column; a spec written
here always does. Never insert a § 4 row above an existing one in a spec that
still uses ordinals.

## The spec / item split

**Delivery state is tracked in `.issue/` work items; content is tracked in the
specification.** There are no GitHub issues, labels or milestones. The item
cites claim ids and never their text; the specification never carries an item's
status. Which fields are authored where, and which are derived and must never be
hand-edited, is in [references/delivery.md](references/delivery.md) § "What is
authored, and what is derived". The item schema is
[`.spec/templates/item.yml`](../../../.spec/templates/item.yml).

## The flow

```
specification  →  grooming  →  .issue/ items  →  scenarios  →  tests  →  implementation  →  PR
```

The specification exists first and stands alone; items are cut from its § 3
claims after agreement. A bug, spike or chore starts at the item instead and may
produce a spec delta afterwards. A refactor starts at the item too, and produces
none.

## Build and test

```sh
./build.sh                  # Default = Compile + Test
./build.sh Format           # dotnet format --verify-no-changes
./build.sh UnitTest         # --filter-trait "Tier=Unit"
./build.sh IntegrationTest  # --filter-trait "Tier=Integration"
./build.sh AcceptanceTest   # test/specht.acceptance, Reqnroll, no filter
./build.sh Pack             # specht.tool.<version>.nupkg into .artifacts/nupkg
./build.sh SpecCheck        # the tool checking this repository's own .spec/ tree
```

`SpecCheck` runs `specht` exactly as a consumer would — through the local tool
manifest, never through a project reference (brief § 7). Until the first
package is published it runs `dotnet run --project src/specht.tool -- --root .`
instead; the manifest replaces that the moment a package exists. From 0062,
pre-commit calls the same thing when a staged file is under a `.spec/` directory
or is a `.feature`.

`dotnet tool restore && dotnet husky install` once per clone, or `core.hooksPath`
is unset and no git hook fires.

`dotnet nbgv get-version` prints the version a commit computes: public on `main`
and `v*` tags, a `-g<commit>` prerelease everywhere else, and a failure in a
shallow clone (`0055-F5` C-2). A release tag is never typed: `dotnet nbgv tag` on
a commit of `main` creates `v<version>` (`0055-F5` A-1, B-007).

## Never add

- A rule in `.claude/` outside the tracked `agents/` and `skills/`. The rest of
  that tree is gitignored, so a rule there cannot be reviewed in a pull request.
- A renumbered claim, constraint, question, task or rule id.
- A GitHub issue, label or milestone as a tracker.
- A hand edit to any derived field.
- A hand edit to `.github/workflows/ci.yml`. It is NUKE-generated.
- A package version in a `.csproj`.
- A specification in two places at once.
- A second copy of a schema file that can drift. `.spec/schema/` and the embedded
  `schema/v1/` are the same bytes for the tool-owned files, and the manifest
  matches on its tool-owned keys; a test says so (`0001-F4` B-004).
- An absolute path in any output, report, log line or test fixture.
- A write into a consumer's tree from anything but `init`, `upgrade`, the
  pin command, which writes only `schemaVersion` in the manifest (`0001-F7`
  decision 0005), the caller-named `--report` file and, from epic `0101`,
  `format`, which only moves whole table rows and frontmatter keys
  (brief § 9); or an overwrite of an existing file from `init`.
- A second index of anything this skill already indexes.
