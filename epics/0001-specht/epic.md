---
title: "Epic 0001: specht, the specification checker as a dotnet tool"
description: "One command, delivered as a dotnet tool, that checks a repository's .spec/ tree against a versioned schema and reports every violation with a file, a line and a rule id, so each consumer pins a version and carries no engine"
id: "0001"
type: epic
status: ready-for-architecture
priority: high
milestone: null
children:
  ["0001-F1", "0001-F2", "0001-F3", "0001-F4", "0001-F5", "0001-F6", "0001-F7"]
created: "2026-10-07"
updated: "2026-10-08"
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
brief § 5. This epic is the tool. `hooked` consuming it and Transporter
installing it are the two "done" events (brief § 2) and are not Features of
this repository: each is a change in that consumer.

The infrastructure in brief § 8 steps 1 and 3 - the build, CI, coverage,
dependency updates, versioning and the release that publishes the package -
is epic `0055`. Steps 4 and 7 are tracked in `hooked` and Transporter, not
here (owner, 2026-10-08).

## Business Value

Four repositories are written in parallel on one `.spec/` model, and three of
them hand-copy `hooked`'s schema, templates and checker. Every improvement
lands four times or drifts. With this epic delivered, an improvement lands once,
a repository upgrades when it chooses, and an agent authoring a specification in
any of them has the same read-only oracle and the same generation contract.

## Features

Decomposed by capability dimension, not by brief § 8 step. A step is a pull
request; a Feature is a thing the tool does, with its own invariant.

| Feature   | Name                            | Specification                                    | brief § 8 step |
| --------- | ------------------------------- | ------------------------------------------------ | -------------- |
| `0001-F1` | The engine, extracted unchanged | `src/specht/.spec/README.md`                     | 2              |
| `0001-F2` | The check command               | `src/specht.tool/Features/Check/.spec/README.md` | 3              |
| `0001-F3` | The report contract             | `src/specht/Report/.spec/README.md`              | 3              |
| `0001-F4` | `init`                          | `src/specht.tool/Features/Init/.spec/README.md`  | 3              |
| `0001-F5` | The manifest carries the roles  | `src/specht/Manifest/.spec/README.md`            | 5              |
| `0001-F6` | Discovery                       | `src/specht/Discovery/.spec/README.md`           | 5              |
| `0001-F7` | Schema versioning               | `src/specht.tool/.spec/README.md`                | 6              |

Why these seven and not `hooked`'s one draft (`0008-F3`, 34 claims): that draft
bundled the command, `init`, the manifest roles, discovery and the baseline in
one Feature. Each of those has a different invariant - the command never
writes except the report the caller names, `init` never overwrites, the manifest never adds a rule, discovery
never opens what it excludes, the engine never changes before step 5 - and a
Feature is cut where the invariant changes (AGENTS.md § "Feature naming
heuristic").

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

| Item                                                   | Where it lives instead                                           |
| ------------------------------------------------------ | ---------------------------------------------------------------- |
| `hooked` replacing its engine with the tool            | `hooked`, a Feature of its own                                   |
| Installing in Transporter and repairing its tree       | Transporter; the repair is the agent's, from the report (Must-6) |
| The claim bridge and the convention analyzers (Roslyn) | `hooked` `0008-F1`, `0008-F2`                                    |
| Rule plugins                                           | Rejected: `0001-F2` decision 0001                                |
| Autofix: `--fix`, or `upgrade` rewriting a document    | Out of scope (brief § 2)                                         |
| A second schema version's content                      | After `0001-F7`, as its own epic                                 |
