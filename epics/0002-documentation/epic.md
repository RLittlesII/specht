---
title: "Epic 0002: Documentation"
description: "Documentation as a first-class deliverable of every change: generated and checked inside the build, and published as a DocFX site on GitHub Pages"
id: "0002"
type: epic
status: ready-for-architecture
priority: med
milestone: null
children: ["0002-F1", "0002-F2"]
created: "2026-10-07"
updated: "2026-11-06"
github_issue: null
---

# Epic 0002: Documentation

## Summary

Documentation as a first-class deliverable of every change from here on, not
something bolted on once `specht` is feature-complete. Epic `0001` is silent on
documentation as a delivery concern; this epic names it before the gap
compounds.

The owner requested this work on 2026-09-29. Two Features, the second built
on the first's output:

- **`0002-F1`**: documentation generated and checked inside the build. An
  XML doc comment on every public type and member, enforced as `CS1591`; an API
  reference generated from them by DocFX in a NUKE target as a build artifact
  (`0002-F1` decision 0002); and one
  hand-written command-line reference page, `docs/cli.md`, held to the tool's
  own help by a test and changed with the commands (`0002-F1` decision 0001).
  This is mechanism, not a publishing surface.
- **`0002-F2`**: a static site, built with DocFX from `0002-F1`'s generated
  reference and the `docs/*.md` pages, and published from CI to GitHub Pages
  at a stable URL. The owner lifted its deferral on 2026-11-06 (`0002-F2`
  decision 0001).

## Business Value

Spectre.Console.Cli generates `--help` from the command declarations (`0001-F2`
B-008 and each command's own Feature), and `0001-F3`
publishes the report's JSON Schema and prints a rule's full text through
`--explain`. Both exist so that what a user reads cannot drift from what the
tool does. This epic applies that principle to the two surfaces nothing yet
covers: the public API, and the narrative of how each command is used. The
first is generated from the code that defines it and the second is held to the
tool's own help by a test; neither is written once and left to rot.

Without it, the public surface ships undocumented by convention. Usage docs, if
written at all, live wherever their author put them, with nothing keeping them
in step with the command they describe. brief § 3 already expects the
specification model's prose to "become the tool's user documentation", and no
Feature owns that today.

## Features

| Feature   | Name                        | Specification               | Status                         |
| --------- | --------------------------- | --------------------------- | ------------------------------ |
| `0002-F1` | Docs as first-class citizen | `docs/.spec/README.md`      | `ready-for-architecture`, high |
| `0002-F2` | Documentation site          | `docs/site/.spec/README.md` | `ready-for-architecture`, med  |

The site name leaves the framework out: the framework was `0002-F2` OQ-2,
resolved as DocFX on 2026-11-06.

## Placement

Both specifications sit under `docs/`, where the documentation and the brief
§ 3 reference copies will live. `0002-F1` touches `Directory.Build.props`,
`.build/`, `docs/cli.md` and both readmes, so no single code folder owns it. The
`implementer` may `git mv` either specification with the code, because a
specification's identity is its frontmatter `epic` and `id` (`SPEC012`).

This epic file lives at `epics/0002-documentation/epic.md`, where schema
version 1's epic glob `epics/**/epic.md` discovers it (`0001-F6` decision 0001).

## Work items

Cut from each Feature's § 3. Every claim not yet `Covered` or `Withdrawn` is in
exactly one item:

| Item                                                                                   | Feature   | Work                                                     | Claims                | Prerequisites |
| -------------------------------------------------------------------------------------- | --------- | -------------------------------------------------------- | --------------------- | ------------- |
| [0181](../../docs/.issue/0181-cli-reference.yml)                                       | `0002-F1` | The command-line reference page, held to the tool's help | B-003, B-006 to B-011 | None; done    |
| [0404](../../docs/.issue/0404-fail-the-build-on-an-undocumented-public-member.yml)     | `0002-F1` | `CS1591` as an error on the public surface               | B-001                 | None          |
| [0405](../../docs/.issue/0405-generate-the-api-reference-with-docfx.yml)               | `0002-F1` | The API reference by DocFX in a NUKE target, and its ADR | B-002, B-004          | None          |
| [0406](../../docs/.issue/0406-run-documentation-in-the-default-build.yml)              | `0002-F1` | The default build runs enforcement and generation        | B-005                 | 0404, 0405    |
| [0407](../../docs/site/.issue/0407-build-the-documentation-site-with-docfx.yml)        | `0002-F2` | The DocFX site over the reference and `docs/*.md`        | B-001                 | 0405          |
| [0408](../../docs/site/.issue/0408-publish-the-documentation-site-to-github-pages.yml) | `0002-F2` | CI publishes the site to GitHub Pages                    | B-002                 | 0407          |

`0002-F2` B-003, the deferral, is withdrawn (`0002-F2` decision 0001). Item
`0362`, the front page and documentation index, carries no claim.

## Cross-epic notes

**This epic does not reopen `0001`.** `--help` (`0001-F2` B-008 and each command's Feature), `--explain`
and the report schema (`0001-F3`) are separate mechanisms that independently
satisfy the same principle; this epic neither consumes nor changes them. A
usage doc links to them and never copies them (`0002-F1` § 5).

## Out of this epic

| Item                                                                      | Where it lives instead                                                                                                                                                           |
| ------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `--help` text                                                             | `0001-F2`                                                                                                                                                                        |
| `--explain` rule text and the report's JSON Schema                        | `0001-F3`                                                                                                                                                                        |
| Consumer documentation                                                    | Each consumer repository                                                                                                                                                         |
| Rewriting `.spec/**` documents as usage docs                              | Rejected: a specification is the agreement                                                                                                                                       |
| A documentation page per rule, from the rule-catalogue prose (brief § 3)  | `0001-F3` (B-031 to B-033, decision 0003): the text `--explain` prints. Its files sit at `docs/rules/` and stay `0001-F3`'s                                                      |
| Turning the specification-model prose into user documentation (brief § 3) | Unowned. Neither `0002` Feature claims it, and `0002-F1` OQ-2, resolved 2026-10-11, did not decide it; the owner decides whether it is a `0002-F1` claim or a Feature of its own |
