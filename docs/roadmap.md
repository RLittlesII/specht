---
title: "Roadmap"
description: "The planned milestones of the specht tool, what each one makes better for a consumer repository, the Features that deliver it, and the epics not yet scheduled"
type: reference
---

# Roadmap

Work ships in milestones named by the package minor version. A milestone is a
set of Feature deliveries that together change what a consumer can do; it
closes when its selected work is done and the version is bumped. Tables cite
the specifications' stable ids and identify partial deliveries. `.issue/.goal`
holds each milestone's finish line, which the queue ranks by; its first section
is the current milestone. Each milestone is mirrored to a read-only GitHub
milestone of the same title. This page says why; the goal file says what; the
Feature specifications say how.

Open finish lines select child items explicitly. Feature containers `0010`,
`0004`, `0033` and `0224` remain delivery summaries outside release gates, so
newly cut children do not silently enlarge a release. A completed release
slice does not close its whole Feature; that requires checking every claim.

## Today: 0.4.x preview available, 0.5.0 in development

A consumer pins a schema version, its build calls `specht`, and no repository
carries the engine. `init`, `schema list` and `schema pin`, `upgrade` of the
schema and template files, `frontmatter`, `evidence`, `--explain` and
`--json` are built. Manifest configuration and pin policy are delivered in
the completed 0.4.0 slice, available in `v0.4.2-preview`. The repository
publishes itself from a tag, restores its own package and checks its own
specification tree. Development now targets 0.5.0, the MCP first release;
`version.json` carries `0.5-preview`.

## 0.3.0 - Complete the tool contract (completed)

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

## 0.4.0 - Manifest configuration (completed)

**Better for:** a consumer who declares frontmatter keys, identity and edge
forms, and id grammars in the manifest, gets messages that name those values,
and chooses an exact or floating schema pin.

All 22 selected work items are done. The model builder, parsers and reader
(`0166`), manifest keys, forms and grammars (`0016`), diagnostic wording
(`0018`), epics opt-in (`0345`), rule unit coverage (`0152`), pin policy
(`0124`) and the remaining manifest gaps (`0347`) are delivered. The goal
file now starts at 0.5.0; the mirror closes the 0.4.0 milestone when this
closeout reaches `main`.

| Feature   | Delivers                                                                                                  |
| --------- | --------------------------------------------------------------------------------------------------------- |
| `0001-F5` | Part: frontmatter keys, edge forms and id grammars from the manifest, named in messages, and epics opt-in |
| `0001-F1` | Part: the model builder and reader prerequisite, and unit coverage for the rules already split            |
| `0001-F7` | Part: a pin policy selecting the exact version, newest permitted patch or newest permitted minor          |

The mediator evaluation ended without adoption (ADR-0026). The type rename,
rule splits, instance evaluator, tree move, release baseline comparison,
schema `0.3.0` cut and rule-id migration are also delivered. The structural
stages retain ADR-0027's whole-check benchmark guard. Runner removal belongs
to 0.6.0.

`0001-F5` B-024 to B-035 and B-042, the epics opt-in, landed through `0345`
after `0016`. This closes the selected release slice; the Feature containers
and claims outside it retain their own delivery and coverage states.

**Delivered:** the selected manifest configuration, model and rule coverage,
and pin-policy work. The default-manifest baseline, older-pin compatibility,
tests and structural benchmark guard remain the validation contracts for
these capabilities.

## 0.5.0 - MCP first release (current)

**Better for:** an agent that asks the tool instead of parsing its output.

`specht-mcp` is a local program of its own, in the package `specht.mcp`, that a
client starts from one configuration entry. It runs the repository's pinned
`specht` and relays the answers whose commands are built: the verdict, a rule's
text, the rule list and the manifest, each the command's own output on the same
tree (the owner, 2026-11-01 and 2026-11-04).

| Feature   | Delivers                                                                                          |
| --------- | ------------------------------------------------------------------------------------------------- |
| `0132-F3` | The MCP host: a local program of its own, finding the pinned `specht`, no port, and `docs/mcp.md` |
| `0132-F4` | Part: the verdict, a rule's text, the rule list and the manifest, and nothing is written          |

**Built so far:** the host over standard input and output, the pinned-tool
resolution, the verdict through the `check` tool, and the rule text, the rule
list and the manifest through the `explain`, `rules` and `manifest` tools, and
`docs/mcp.md`. Nothing is released.

**Done means:** an MCP client configured with one entry starts the host and
reads the verdict, a rule's text, the rule list and the manifest, each equal to
its command's output on the same tree; `docs/mcp.md` documents the entry and
those tools; and no file changes.

## 0.6.0 - Repository integration and pull-request surface

**Better for:** a consumer whose discovery respects repository exclusions,
whose check can save a report, and whose pull request shows violations from
one workflow step.

A GitHub Action runs `specht` in any repository, writes its violations onto the
pull request's diff and into the step summary, and passes on the tool's exit
code; this repository's CI adopts it. Epic `0142` has its need written and its
Features still to cut; a Feature joins the goal file when it is. Code analysis
reaches a consumer whose claims live only in Gherkin, for consumer request
#256.

Discovery `0007` and `0008` follow the model reader `0166` delivered in 0.4.0;
their parity proof `0009` follows both. Report output `0036` follows the
direct engine entry point and runner removal `0167`, which also follows
`0166`. Host-factory and test-support cleanup do not gate report output.

| Epic or Feature | Delivers                                                                                          |
| --------------- | ------------------------------------------------------------------------------------------------- |
| `0142`          | A GitHub Action that runs `specht`, annotates the pull request and writes the step summary        |
| `0201-F6`       | One packaged consumer proving native Gherkin claims with xUnit v2 VSTest evidence, item `0302`    |
| `0001-F6`       | Part: discovery through `git ls-files` and the pruned walk, with the same files in the same order |
| `0001-F3`       | Part: the report written to the `--report` path                                                   |
| `0001-F1`       | Part: one direct check entry point and runner removal, under ADR-0027                             |

Item `0128` (an id claimed twice) and spike `0289` (large comment chunks in
specification markdown) land here. The Gherkin claims' documentation lands in
0.3.0.

`0167` retains its owner-review hold on the direct-call redesign. Before
implementing `0036`, correct its obsolete mediator design under ADR-0026 and
resolve `0001-F3` OQ-8, report behavior on input failure. Discovery still needs
its design pass, including git failure behavior; B-013 to B-015 (orphan
companions and required directories) have no items, and B-004 is Partial.
Cut and schedule that work here before claiming the whole discovery Feature
is finished. Allocate report expectation claims B-034 to B-043 with the rules
that produce them: discovery here, matrix validation in 0.7.0, and reconcile
already-built cases and the remaining Partial rows.

These additions share a repository-integration outcome with the Action; they
are not new Action prerequisites. Epic `0142` proposes reading `--json`.
The Action still needs decomposition, so existing item counts are not the
complete release size. Item `0128` remains blocked by status.

**Done means:** a consumer repository gets its violations on a pull request with
one workflow step and no code of its own, both discovery modes pass their
parity proof, and `--report` writes the same document as `--json` while
preserving stdout and the verdict.

## 0.7.0 - Agent surface completed

**Better for:** an agent that reads the whole tree through MCP, not only the
verdict and the rules.

The tree's facts reach the command line and MCP, with claim coverage and the
frontmatter schemas and templates of the pinned version, so an agent with no
`specht` skill authors a Feature specification the check passes.

| Feature    | Delivers                                                                                           |
| ---------- | -------------------------------------------------------------------------------------------------- |
| `0132-F2`  | The tree's facts as structured data on the command line, working name `specht facts`               |
| `0132-F4`  | The rest of reading through MCP: the tree's facts, claim coverage, the schema and template answers |
| `0001-F13` | Claim coverage, `specht trace`                                                                     |
| `0001-F18` | The frontmatter schemas and templates printed, and `specht schema list --json`                     |
| `0001-F12` | Remaining matrix validation: scenario titles, missing scenarios, row status and older-pin guards   |

Spike `0252` (MCP access to linked records, and local or online storage) lands
here; its proposal is ADR-0028. Spike `0256` (collision-free record ids) lands
here too.

The remaining matrix path is `0335` and `0336`, then `0337`, then `0338`;
the schema cut, repository pin and unresolved-test rule prerequisites are
done. This placement groups matrix validation with facts and coverage. The
trace command shares the already-built resolver `0225`; it need not wait
for every validation rule. Package milestone `0.7.0` does not change the
matrix rules' schema `0.3.0` guards. Facts, trace and the remaining MCP answers
still need their delivery items cut.

**Done means:** an agent in a fresh repository, with only the manifest `init`
writes, authors a Feature specification from the MCP answers alone and the
check reports no violation; the selected matrix rules reject unsupported
scenario/status rows and preserve the verdict under older pins.

## 0.8.0 - Review and presentation

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
Epic `0228` reads the tree's facts from `0132-F2`, in 0.7.0. Epic `0101` is
`blocked`: every Feature waits on the schema version mechanism, `0001-F7`, its
rule Features on per-rule severity, `0001-F5`, and two of them on discovery,
`0001-F6`.

**Done means:** a reviewer gets the violations a change added and removed from
two report documents and resolves a cited id to its file and row; at a terminal
the check prints a coloured table while its redirected output is unchanged; and
`specht format` leaves a tree with no ordering finding, every other byte as it
was.

## 0.9.0 - Onboarding and documentation

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

No milestone holds these epics and Features yet. The goal file holds only milestones, so
this list is this page's alone.

| Epic or Feature | Delivers                                                                                                                                          |
| --------------- | ------------------------------------------------------------------------------------------------------------------------------------------------- |
| `0193`          | JSON as a first-class specification format                                                                                                        |
| `0132-F1`       | A persistent specification index of keyed facts and relationships, rebuildable from the tree                                                      |
| `0324`          | A `specht` service: document sequences allocated without git coordination, and a database mode where each managed document keeps one lifetime key |
| `0109-F4`       | The tool process's peak memory for each cold-start row, once an observer meets its constraint                                                     |

The online allocation in `0324` follows spike `0256` and ADR-0028's
recommendation that online allocation waits.

### Maintenance backlog

These items remain tracked without gating a release. Their statuses and
dependencies are unchanged.

| Item   | Work                                                        |
| ------ | ----------------------------------------------------------- |
| `0168` | One shared test-support project                             |
| `0144` | One factory for the host and command tests                  |
| `0207` | Simpler shared engine-test composition                      |
| `0218` | Real project references for test and tooling dependencies   |
| `0342` | Rule classes and their Feature homes cohesive by rule group |

`0168` can start now. `0144` follows `0168` and 0.6.0's `0167`, and retains
its owner-review hold. `0207` and `0218` require coordination with the shared
test-support and host-factory work; overlapping write sets are sequenced.
`0342` follows the completed `0169` and needs the owner's layout ruling.

### Investigations

| Item   | Work                                                               |
| ------ | ------------------------------------------------------------------ |
| `0141` | Weigh rule settings from `.editorconfig`; its prerequisite is done |
| `0170` | Weigh native AOT and startup after `0167`                          |

Neither investigation gates the selected release promises. Before the AOT
trial, correct its obsolete dispatcher references; an adoption ruling still
needs the startup measurement its agreement requires. Rule-id migration
`0169`, originally proposed for this backlog, completed before this split
landed and remains in 0.4.0's history. The owner placed its new rule-folder
follow-up `0342` in the maintenance backlog on 2026-11-02.

## What this page is not

It records scheduled outcomes and unscheduled work, and decides nothing a
Feature's specification decides. A milestone's finish line is the goal file;
a Feature's agreement is its `.spec/README.md`. Moving work between milestones
is an edit to the goal file and a line here, in the same pull request.
