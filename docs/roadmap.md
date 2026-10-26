---
title: "Roadmap"
description: "The next three milestones of the specht tool, what each one makes better for a consumer repository, and the Features that deliver it"
type: reference
---

# Roadmap

Work ships in milestones named by the package minor version. A milestone is a
set of Features that together change what a consumer can do; it closes when
every one of them is done and the version is bumped. The ids are the
specifications' stable ids, and the finish line of each milestone is the first
section of `.issue/.goal`, which the queue ranks by. This page says why; the
goal file says what; the Feature specifications say how.

## Today: 0.2.x

A consumer pins a schema version, its build calls `specht`, and no repository
carries the engine. `init`, `schema list` and `schema pin`, `upgrade` of the
schema and template files, `frontmatter`, `evidence`, `--explain` and
`--json` are built. The repository publishes itself from a tag, restores its own
package and checks its own specification tree.

## 0.3.0 - Complete the tool contract

**Better for:** a consumer who adopts the tool and never has to read the source
to learn what it accepts, and a maintainer whose repository runs itself.

Every command the command-line reference promises exists, and the schema
lifecycle is whole: a consumer can inspect the effective manifest, list the
rules of the pinned version, set one rule's level, upgrade a manifest without
losing a setting, and fetch a recorded upstream schema source on upgrade. The
maintainer can cut the next schema version from the newest one, and the
repository's dependency updates, branch protection and work-item mirror run
unattended.

| Feature    | Delivers                                                                                             |
| ---------- | ---------------------------------------------------------------------------------------------------- |
| `0001-F7`  | Schema versioning: upgrade keeps consumer settings, fetches a recorded source, cuts the next version |
| `0001-F4`  | `init` writes the templates a recorded upstream source hashes                                        |
| `0001-F9`  | `specht manifest`: the effective manifest, the shipped default, as text or JSON                      |
| `0001-F10` | `specht rules`: one entry per rule of the pinned version, as text or JSON                            |
| `0001-F11` | The set command: one rule's level, changed in the manifest bytes alone                               |
| `0001-F2`  | The check command's agreement closed against what is built                                           |
| `0055-F2`  | Continuous integration on the owner's runner pool                                                    |
| `0055-F4`  | Dependency updates: action versions from the build project, green minors merged, majors alone        |
| `0055-F5`  | Package versioning proven apart from the schema version                                              |
| `0055-F8`  | The repository settings checklist, written and applied                                               |
| `0055-F9`  | The work-item mirror run to convergence                                                              |

Spike `0220`, rule-page versioning without a full copy per version, informs the
cut and lands here.

**Done means:** `docs/cli.md` names no command the tool lacks, and a consumer
pinned to the newest schema upgrades to the next one without touching a
document.

## 0.4.0 - A manifest-driven engine, measured before it is restructured

**Better for:** a consumer who declares its own layout, section roles and id
grammars in the manifest and gets rule messages that name them, and a
maintainer who restructures the engine only on evidence.

The benchmark spike `0216` runs first and decides, under ADR-0014, whether the
engine moves to one rule per id behind a mediator. If it does, the refactors
follow in this milestone; if it does not, the milestone closes without them and
the manifest work stands on the engine as built. Either way discovery lists
files through git, the report writes to a path, and the traceability matrix is
resolved against the scenarios and tests it cites.

| Feature    | Delivers                                                                                                |
| ---------- | ------------------------------------------------------------------------------------------------------- |
| `0001-F5`  | Frontmatter keys, edge forms and id grammars from the manifest, named in messages                       |
| `0001-F6`  | Discovery through `git ls-files`, with the pruned walk as fallback, proven equal                        |
| `0001-F3`  | The report written to the `--report` path                                                               |
| `0001-F12` | Matrix resolution: § 9 rows held against the scenarios and the declared tests                           |
| `0001-F1`  | The engine's rules split one per id, the evaluator an instance, the runner gone - conditional on `0216` |
| `0109-F4`  | Tool cold start, with peak memory per row                                                               |
| `0109-F5`  | Release baseline comparison: advisory regressions between two published revisions                       |

Refactors `0168`, `0207` and `0218` simplify the test composition; spikes
`0141` (rule settings from `.editorconfig`), `0169` (four-digit rule ids) and
`0170` (native AOT) wait on the gate's outcome.

**Done means:** the default manifest reproduces the golden baseline report on
every moved literal, and the benchmark finding is recorded whichever way it
went.

## 0.5.0 - Beyond the command line

**Better for:** a pull request author who sees violations on the diff with one
step, an agent that asks the tool instead of parsing its output, and a person
starting specification-driven development from nothing.

The tool reaches new surfaces. Each epic below has its need written and its
Features still to cut; a Feature joins the goal file when it is.

| Epic   | Delivers                                                                                   |
| ------ | ------------------------------------------------------------------------------------------ |
| `0142` | A GitHub Action that runs `specht` and annotates the pull request                          |
| `0132` | `specht` through the Model Context Protocol, starting from spike `0252`                    |
| `0196` | Guided onboarding from an unconfigured repository to a first checked specification         |
| `0228` | Review surfaces: the tree as one document, the delta between two runs, a citation resolved |
| `0194` | Spectre console presentation of trees, options and diagnostics                             |
| `0193` | JSON as a first-class specification format                                                 |
| `0101` | Specification linting and `specht format`                                                  |
| `0002` | The documentation site                                                                     |

Spikes `0256` (collision-free record ids) and `0289` (large comment chunks in
specification markdown), and item `0128` (an id claimed twice), land here.

**Done means:** a consumer repository gets its violations on a pull request with
one workflow step and no code of its own.

## What this page is not

It stores no id the goal file does not, and it decides nothing a Feature's
specification decides. A milestone's finish line is the goal file; a Feature's
agreement is its `.spec/README.md`. Moving work between milestones is an edit
to the goal file and a line here, in the same pull request.
