---
title: "Epic 0001: specht, the specification checker as a dotnet tool"
description: "One command, delivered as a dotnet tool, that checks a repository's .spec/ tree against a versioned schema and reports every violation with a file, a line and a rule id, so each consumer pins a version and carries no engine"
id: "0001"
type: epic
status: ready-for-architecture
priority: high
milestone: null
children:
  [
    "0001-F1",
    "0001-F2",
    "0001-F3",
    "0001-F4",
    "0001-F5",
    "0001-F6",
    "0001-F7",
    "0001-F8",
    "0001-F9",
    "0001-F10",
    "0001-F11",
    "0001-F12",
    "0001-F13",
    "0001-F14",
  ]
created: "2026-10-07"
updated: "2026-10-15"
github_issue: null
---

# Epic 0001: `specht`, the specification checker as a dotnet tool

## Summary

One command, delivered as a `dotnet tool`, that checks a repository's `.spec/`
tree against a versioned schema and reports every violation with a file, a
line, a rule id, a message and what the rule expected. The schema and the
checker live here, once; every consumer pins a schema version, installs the
tool through a local tool manifest, and carries no engine.

The need is decided in `REQUIREMENTS.md` (at commit `254aabc`) and the design direction in
brief § 5. This epic is the tool. A consumer repository consuming it and Transporter
installing it are the two "done" events (brief § 2) and are not Features of
this repository: each is a change in that consumer.

The infrastructure in brief § 8 steps 1 and 3 - the build, CI, coverage,
dependency updates, versioning and the release that publishes the package -
is epic `0055`. Steps 4 and 7 are tracked in a consumer repository and Transporter, not
here (owner, 2026-10-08).

## Business Value

Four repositories are written in parallel on one `.spec/` model, and three of
them hand-copy the consumer's schema, templates and checker. Every improvement
lands four times or drifts. With this epic delivered, an improvement lands once,
a repository upgrades when it chooses, and an agent authoring a specification in
any of them has the same read-only oracle and the same generation contract.

## Features

Decomposed by capability dimension, not by brief § 8 step. A step is a pull
request; a Feature is a thing the tool does, with its own invariant.

| Feature    | Name                            | Specification                                      | brief § 8 step |
| ---------- | ------------------------------- | -------------------------------------------------- | -------------- |
| `0001-F1`  | The engine, extracted unchanged | `src/specht/.spec/README.md`                       | 2              |
| `0001-F2`  | The check command               | `src/tool/Features/Check/.spec/README.md`          | 3              |
| `0001-F3`  | The report contract             | `src/specht/Report/.spec/README.md`                | 3              |
| `0001-F4`  | `init`                          | `src/tool/Features/Init/.spec/README.md`           | 3              |
| `0001-F5`  | The manifest carries the roles  | `src/specht/Manifest/.spec/README.md`              | 5              |
| `0001-F6`  | Discovery                       | `src/specht/Discovery/.spec/README.md`             | 5              |
| `0001-F7`  | Schema versioning               | `src/tool/.spec/README.md`                         | 6              |
| `0001-F8`  | The frontmatter command         | `src/tool/Features/Frontmatter/.spec/README.md`    | after 6        |
| `0001-F9`  | The manifest command            | `src/tool/Features/Manifest/.spec/README.md`       | after 6        |
| `0001-F10` | The rules command               | `src/tool/Features/Rules/.spec/README.md`          | after 6        |
| `0001-F11` | Setting a rule's level          | `src/tool/Features/Rules/Set/.spec/README.md`      | after 6        |
| `0001-F12` | Matrix resolution               | `src/specht/Rules/Matrix/.spec/README.md`          | after 6        |
| `0001-F13` | The trace command               | `src/tool/Features/Trace/.spec/README.md`          | after 6        |
| `0001-F14` | The schema folder               | `src/specht/Manifest/SchemaFolder/.spec/README.md` | after 6        |

The initial draft bundled the command, `init`, the manifest roles, discovery and the baseline in
one Feature. Each of those has a different invariant - the command never
writes except the report the caller names, `init` never overwrites, the manifest never adds a rule, discovery
never opens what it excludes, the engine never changes before step 5 - and a
Feature is cut where the invariant changes (AGENTS.md § "Feature naming
heuristic").

`0001-F8` joined on 2026-10-09 from the owner's need to check frontmatter that
is not in a tree: its invariant is that one document is checked by the
frontmatter rule alone, read-only, with no tree beside it.

`0001-F9`, `0001-F10` and `0001-F11` joined on 2026-10-11 from the owner's need
to see and change the configuration a check runs under. Each has its own
invariant: the manifest command prints what the engine reads and writes
nothing; the rules command lists the pinned version's vocabulary and keeps no
second catalogue; the set command writes one rule's level and nothing else.
The same need amended `0001-F4`: `init` writes a level for every rule.

`0001-F12` and `0001-F13` joined on 2026-10-14 from the owner's need to hold
each traceability row against the tree. Each has its own invariant: matrix
resolution reads scenarios and test files as text and runs nothing, with its
rules in schema `0.2.0`; the trace command lists every claim's resolution
through that one resolver, judges nothing and writes nothing.

`0001-F14` joined on 2026-10-15 from the owner's need to place the frontmatter
schemas where a repository's tree wants them, named in the manifest rather than
fixed under the configuration folder. Its invariant: the engine and the tool
read and write the frontmatter schemas where the manifest says, never where a
literal says, and the default manifest changes no verdict.

Dependencies are declared in each Feature's frontmatter and checked by the tool
(`SPEC050`-`SPEC052`). The order they imply is brief § 8's.

## Placement

A Feature's specification sits beside the code it specifies (AGENTS.md § "Where
a specification lives"). The engine folders `Report/`, `Manifest/` and
`Discovery/` under `src/specht` do not exist yet; they are the spec-author's
placement and the `implementer` may move a specification with the code
(`git mv`), because a specification's identity is its frontmatter `epic` and
`id`, never its path (`SPEC012`).

This epic file lives at `epics/0001-specht/epic.md`, where schema version 1's
epic glob `epics/**/epic.md` discovers it (`0001-F6` decision 0001).

## Out of this epic

| Item                                                     | Where it lives instead                                                                                       |
| -------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------ |
| a consumer repository replacing its engine with the tool | a consumer repository, a Feature of its own                                                                  |
| Installing in Transporter and repairing its tree         | Transporter; the repair is the agent's, from the report (Must-6)                                             |
| The claim bridge and the convention analyzers (Roslyn)   | Outside the checker's scope (brief § 2); deferred code bindings are recorded in epic `0201`                  |
| Rule plugins                                             | Rejected: `0001-F2` decision 0001                                                                            |
| Autofix: `--fix`, or `upgrade` rewriting a document      | Out of scope (brief § 2); reordering rows and keys is `specht format`, epic `0101` (`0101-F5` decision 0001) |
| Style and ordering rules                                 | Epic `0101`, as schema version 2                                                                             |
| A second schema version's content                        | After `0001-F7`, as its own epic                                                                             |
