---
id: "0002"
type: epic
status: needs-decomposition
priority: med
milestone: null
children: ["0002-F1", "0002-F2"]
created: "2026-10-07"
updated: "2026-10-07"
github_issue: null
---

# Epic 0002: Documentation

## Summary

Documentation as a first-class deliverable of every change from here on, not
something bolted on once `specht` is feature-complete. Epic `0001` is silent on
documentation as a delivery concern; this epic names it before the gap
compounds.

It mirrors `hooked`'s Documentation epic (hooked PR #212, grooming of
2026-09-29), adapted to this repository's surfaces and conventions. Two
Features, deliberately sequenced:

- **`0002-F1` (now)**: documentation generated and checked inside the build. An
  XML doc comment on every public type and member, enforced as `CS1591`; an API
  reference generated from them by a NUKE target as a build artifact; and each
  command's usage doc co-located with the command and changed with it. This is
  mechanism, not a publishing surface.
- **`0002-F2` (later, `status: blocked` on `0002-F1`)**: a static site that
  publishes `0002-F1`'s output to a stable URL. **Deferred by the owner** until
  `specht` itself (epic `0001`) is built. It exists now as a placeholder, so
  `0002-F1`'s output is structured to be consumed by a site. Its framework is
  undecided: astro.build is a candidate, not a decision.

## Business Value

Spectre.Console.Cli generates `--help` from the command declarations (`0001-F2`
B-008 and each command's own Feature), and `0001-F3`
publishes the report's JSON Schema and prints a rule's full text through
`--explain`. Both exist so that what a user reads cannot drift from what the
tool does. This epic applies that principle to the two surfaces nothing yet
covers: the public API, and the narrative of how each command is used. Both are
generated or kept beside the code that defines them, not written once and left
to rot.

Without it, the public surface ships undocumented by convention. Usage docs, if
written at all, live wherever their author put them, with nothing keeping them
in step with the command they describe. README § 3 already expects the
specification model's prose to "become the tool's user documentation", and no
Feature owns that today.

## Features

| Feature   | Name                        | Specification               | Status                      |
| --------- | --------------------------- | --------------------------- | --------------------------- |
| `0002-F1` | Docs as first-class citizen | `docs/.spec/README.md`      | `needs-decomposition`, high |
| `0002-F2` | Documentation site          | `docs/site/.spec/README.md` | `blocked` on `0002-F1`, med |

`hooked` names its F2 "Astro Documentation Site". Here the name leaves the
framework out, because the framework is `0002-F2` OQ-2.

## Placement

Both specifications sit under `docs/`, where the documentation and the README
§ 3 reference copies will live. `0002-F1` touches `Directory.Build.props`,
`.build/` and every command folder, so no single code folder owns it. The
`implementer` may `git mv` either specification with the code, because a
specification's identity is its frontmatter `epic` and `id` (`SPEC012`).

This epic file lives at `epics/0002-documentation/epic.md`, where schema
version 1's epic glob `epics/**/epic.md` discovers it (`0001-F6` decision 0001).

## Candidate task cut

Not filed. Tasks are GitHub issues, cut from § 3 after agreement, and this
repository has no remote yet. The ids are reserved in epic order and mirror
`hooked`'s 0008-01..06:

| Task      | Feature   | Summary                                                                             | Claims       |
| --------- | --------- | ----------------------------------------------------------------------------------- | ------------ |
| `0002-01` | `0002-F1` | XML doc-comment enforcement: `CS1591` as an error, scoped to the public surface     | B-001; C-3   |
| `0002-02` | `0002-F1` | API-reference generation: a NUKE target writing under `.artifacts/`                 | B-002, B-004 |
| `0002-03` | `0002-F1` | Usage-doc co-location convention, applied to every command folder                   | B-003; C-5   |
| `0002-04` | `0002-F1` | Build verification: the default build runs both and fails on an undocumented member | B-001, B-005 |
| `0002-05` | `0002-F2` | Site scaffold over `0002-F1`'s output, after the framework ADR                      | B-001        |
| `0002-06` | `0002-F2` | CI publish pipeline to a stable public URL                                          | B-002        |

`0002-05` and `0002-06` are not cut until the deferral is lifted (`0002-F2`
B-003).

## Cross-epic notes

**This epic does not reopen `0001`.** `--help` (`0001-F2` B-008 and each command's Feature), `--explain`
and the report schema (`0001-F3`) are separate mechanisms that independently
satisfy the same principle; this epic neither consumes nor changes them. A
usage doc links to them and never copies them (`0002-F1` § 5).

**hooked numbering.** Epic `0001`'s "Out of this epic" cites `hooked` `0008-F1`
and `0008-F2` as the claim bridge and the convention analyzers, while hooked
PR #212 numbers its Documentation epic `0008` too. This file cites the pull
request, not the number. The owner should confirm which numbering `hooked`
settles on before either citation is relied on.

**`hooked`'s 0005-F1 amendment has no counterpart here.** PR #212 rewords a
`hooked` CLI specification that claimed "no separate docs site"; no `0001`
specification makes that claim.

## Out of this epic

| Item                                                                                          | Where it lives instead                                                                                                                  |
| --------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------- |
| `--help` text                                                                                 | `0001-F2`                                                                                                                               |
| `--explain` rule text and the report's JSON Schema                                            | `0001-F3`                                                                                                                               |
| `hooked`'s own documentation                                                                  | `hooked`, its Documentation epic (PR #212)                                                                                              |
| Rewriting `.spec/**` documents as usage docs                                                  | Rejected: a specification is the agreement                                                                                              |
| Turning the specification-model and rule-catalogue prose into user documentation (README § 3) | Unowned. Neither `0002` Feature claims it; the owner decides whether it is a `0002-F1` claim once OQ-2 settles, or a Feature of its own |
