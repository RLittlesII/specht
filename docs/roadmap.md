---
title: "Roadmap"
description: "The planned milestones of the specht tool, what each one makes better for a consumer repository, the Features that deliver it, and the epics not yet scheduled"
type: reference
---

# Roadmap

Work ships in milestones named by the package minor version. A milestone is a
set of Features that together change what a consumer can do; it closes when
every one of them is done and the version is bumped. The ids are the
specifications' stable ids, and the finish line of each milestone is the first
section of `.issue/.goal`, which the queue ranks by. Each milestone is mirrored
to a read-only GitHub milestone of the same title. This page says why; the
goal file says what; the Feature specifications say how.

## Today: 0.2.x

A consumer pins a schema version, its build calls `specht`, and no repository
carries the engine. `init`, `schema list` and `schema pin`, `upgrade` of the
schema and template files, `frontmatter`, `evidence`, `--explain` and
`--json` are built. The repository publishes itself from a tag, restores its own
package and checks its own specification tree.

## 0.3.0 - Complete the tool contract

**Better for:** a consumer who adopts the tool and never has to read the source
to learn what it accepts.

Every command the command-line reference promises exists, and the schema
lifecycle is whole: a consumer can inspect the effective manifest, list the
rules of the pinned version, set one rule's level, upgrade a manifest without
losing a setting, and fetch a recorded upstream schema source on upgrade. The
maintainer can cut the next schema version from the newest one, and every
work item is visible on GitHub as a read-only issue.

| Feature    | Delivers                                                                                             |
| ---------- | ---------------------------------------------------------------------------------------------------- |
| `0001-F7`  | Schema versioning: upgrade keeps consumer settings, fetches a recorded source, cuts the next version |
| `0001-F4`  | `init` writes the templates a recorded upstream source hashes                                        |
| `0001-F9`  | `specht manifest`: the effective manifest, the shipped default, as text or JSON                      |
| `0001-F10` | `specht rules`: one entry per rule of the pinned version, as text or JSON                            |
| `0001-F11` | The set command: one rule's level, changed in the manifest bytes alone                               |
| `0001-F2`  | The check command's agreement closed against what is built                                           |
| `0055-F5`  | Package versioning proven apart from the schema version                                              |
| `0055-F9`  | The work-item mirror run to convergence                                                              |
| `0201-F7`  | Native Gherkin claim input documented for consumers                                                  |

Spike `0220`, rule-page versioning without a full copy per version, informs the
cut and lands here.

**Done means:** `docs/cli.md` names no command the tool lacks, and a consumer
pinned to the newest schema upgrades to the next one without touching a
document.

## 0.4.0 - A manifest-driven engine, measured before it is restructured

**Better for:** a consumer who declares its own layout, section roles and id
grammars in the manifest and gets rule messages that name them, and a
maintainer who restructures the engine only on evidence.

The benchmark spike `0216` ran first: the engine takes no mediator
(ADR-0026). The structural refactors proceed (ADR-0027): `0100`, then the
splits to one rule per id, then the later stages, each pull request showing no
regression on the whole-check benchmark. Discovery lists files through git,
the report writes to a path, and the traceability matrix is resolved against
the scenarios and tests it cites.

| Feature    | Delivers                                                                                             |
| ---------- | ---------------------------------------------------------------------------------------------------- |
| `0001-F5`  | Frontmatter keys, edge forms and id grammars from the manifest, named in messages                    |
| `0001-F6`  | Discovery through `git ls-files`, with the pruned walk as fallback, proven equal                     |
| `0001-F3`  | The report written to the `--report` path                                                            |
| `0001-F12` | Matrix resolution: § 9 rows held against the scenarios and the declared tests                        |
| `0001-F1`  | The engine's rules split one per id, the evaluator an instance, the runner gone - under ADR-0027     |
| `0109-F4`  | Tool cold start, with peak memory per row                                                            |
| `0109-F5`  | Release baseline comparison: advisory regressions between two published revisions                    |
| `0001-F7`  | The pin policy `0124`: a pin that floats to the newest shipped patch or minor, moved here from 0.3.0 |

Refactors `0168`, `0207` and `0218` simplify the test composition; spike
`0141` (rule settings from `.editorconfig`) waits on `0014`, and spikes `0169`
(four-digit rule ids) and `0170` (native AOT) on the stages they follow.

**Done means:** the default manifest reproduces the golden baseline report on
every moved literal, and the benchmark finding is recorded whichever way it
went.

## 0.5.0 - Pull-request surface

**Better for:** a pull request author, who sees violations on the diff from one
workflow step.

A GitHub Action runs `specht` in any repository, writes its violations onto the
pull request's diff and into the step summary, and passes on the tool's exit
code; this repository's CI adopts it. Epic `0142` has its need written and its
Features still to cut; a Feature joins the goal file when it is. Code analysis
reaches a consumer whose claims live only in Gherkin, for consumer request
#256.

| Epic or Feature | Delivers                                                                                       |
| --------------- | ---------------------------------------------------------------------------------------------- |
| `0142`          | A GitHub Action that runs `specht`, annotates the pull request and writes the step summary     |
| `0201-F6`       | One packaged consumer proving native Gherkin claims with xUnit v2 VSTest evidence, item `0302` |

Item `0128` (an id claimed twice) and spike `0289` (large comment chunks in
specification markdown) land here. The Gherkin claims' documentation lands in
0.3.0.

**Done means:** a consumer repository gets its violations on a pull request with
one workflow step and no code of its own.

## 0.6.0 - Agent surface

**Better for:** an agent that asks the tool instead of parsing its output.

`specht` answers through the Model Context Protocol: a local host the client
starts, answers equal to the command line's on the same tree, and an index that
spares reparsing unchanged Markdown. The four Features are specified; their
items are still to cut.

| Feature   | Delivers                                                                                        |
| --------- | ----------------------------------------------------------------------------------------------- |
| `0132-F2` | The tree's facts as structured data on the command line, working name `specht facts`            |
| `0132-F3` | The MCP host: a local process the client starts, the tool's own version, no port                |
| `0132-F4` | Reading through MCP: each answer is the command line's on the same tree, and nothing is written |
| `0132-F1` | A persistent specification index of keyed facts and relationships, rebuildable from the tree    |

Spike `0252` (MCP access to linked records, and local or online storage) lands
here; its proposal is ADR-0028. Spike `0256` (collision-free record ids) lands
here too.

**Done means:** an MCP client configured with one entry starts the host and
reads the rules, the tree's facts and the check's verdict, each equal to the
command line's `--json` answer on the same tree, and no file in the tree
changes.

## 0.7.0 - Review and presentation

**Better for:** a reviewer, who judges a change from the tool's answers, and a
person reading the console output.

The reviewer gets read-only answers the check does not give: the delta between
two runs, a citation resolved to its row, an approval gate on open questions and
a report scoped to the files a change touched. At a terminal the tree shows as a
tree, a closed-set argument is chosen from a list, and diagnostics read as a
coloured table; the MSBuild-shaped stream, the JSON document and the exit code
stay as they are. Linting adds style, ordering and uniqueness rules, and
`specht format` moves whole table rows and frontmatter keys.

| Epic or Feature        | Delivers                                                                                            |
| ---------------------- | --------------------------------------------------------------------------------------------------- |
| `0228-F1`              | The open-question gate: a specification approved with an open question is a violation               |
| `0228-F2`              | Dangling id references within one specification                                                     |
| `0228-F3` to `0228-F7` | The delta between two runs, citation resolution, a scoped report, item and commit-message checks    |
| `0194`                 | Spectre.Console presentation: the tree as a tree, a selection list, diagnostics as a coloured table |
| `0101`                 | Specification linting and `specht format`                                                           |

Spike `0307` (qualified key resolution, epic `0228` candidate C) lands here.
Epic `0228` reads the tree's facts from `0132-F2`, in 0.6.0. Epic `0101` is
`blocked`: every Feature waits on the schema version mechanism, `0001-F7`, its
rule Features on per-rule severity, `0001-F5`, and two of them on discovery,
`0001-F6`.

**Done means:** a reviewer gets the violations a change added and removed from
two report documents and resolves a cited id to its file and row; at a terminal
the check prints a coloured table while its redirected output is unchanged; and
`specht format` leaves a tree with no ordering finding, every other byte as it
was.

## 0.8.0 - Onboarding and documentation

**Better for:** a newcomer starting specification-driven development from
nothing.

A resumable journey reads the repository's state, explains the next step and
the condition that completes it, and leads from an unconfigured repository to a
first checked specification. The documentation the build generates and checks
is published as a static site.

| Epic or Feature | Delivers                                                                                        |
| --------------- | ----------------------------------------------------------------------------------------------- |
| `0196`          | Guided onboarding from an unconfigured repository to a first checked specification              |
| `0002-F1`       | Documentation generated and checked in the build: doc comments, an API reference, `docs/cli.md` |
| `0002-F2`       | The documentation site: `0002-F1`'s output at a stable URL                                      |

`0002-F2` waits on `0002-F1`, and its framework is not chosen.

**Done means:** a newcomer in a repository with no `.specht/manifest.json`
follows the journey to a Feature specification the check passes with 0 errors,
and the site serves the command-line and API references at a stable URL.

## Unscheduled

No milestone holds these epics yet. The goal file holds only milestones, so
this list is this page's alone.

| Epic   | Delivers                                                                                                                                          |
| ------ | ------------------------------------------------------------------------------------------------------------------------------------------------- |
| `0193` | JSON as a first-class specification format                                                                                                        |
| `0324` | A `specht` service: document sequences allocated without git coordination, and a database mode where each managed document keeps one lifetime key |

The online allocation in `0324` follows spike `0256` and ADR-0028's
recommendation that online allocation waits.

## What this page is not

It stores no id the goal file does not, and it decides nothing a Feature's
specification decides. A milestone's finish line is the goal file; a Feature's
agreement is its `.spec/README.md`. Moving work between milestones is an edit
to the goal file and a line here, in the same pull request.
