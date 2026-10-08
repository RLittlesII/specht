---
title: Specifications in specht
description: Where a specification lives here and which layouts the tool reads, the twelve sections, the records beside it, and the specht rules that gate it
type: reference
---

# Specifications

Extends [`spec-and-traceability`](../../spec-and-traceability/SKILL.md).

## One layout here, two layouts the tool reads

This repository's own specifications are **co-located**: `<area>/.spec/README.md`
beside the code it specifies, from the first commit. The only `epics/` content
here is epic files, `epics/<epic>/epic.md`, where the version 1 epic glob finds
them (`0001-F6` decision 0001); no specification lives there and nothing
migrates.

The tool reads both layouts, because its consumers have both:

| Layout      | Path                                      | Here                     |
| ----------- | ----------------------------------------- | ------------------------ |
| co-located  | `<area>/.spec/README.md`                   | the only one             |
| legacy      | `epics/<epic>/<feature>/spec.md`           | never; read for consumers |

The same rules apply to each. A spec's identity is `(epic, id)` from its
frontmatter, not its path; the same `(epic, id)` discovered at two locations is
an **error** (`SPEC012`). The one rule that cares about the layout is `SPEC011`,
which checks a legacy spec's `epic`/`id` against the directory it sits in.

## A Feature's folder

```
<area>/.spec/README.md               the twelve-section specification
<area>/.spec/<feature-name>.feature  the companion Gherkin, @B-00n tagged
<area>/.spec/decisions/              product and scope calls, numbered per Feature
<area>/.spec/adr/                    Feature-scoped ADRs, numbered per Feature
<area>/.spec/lessons/                Feature-scoped lessons, numbered per Feature
```

Repo-wide blast radius goes to the root instead: `.spec/adr/`, `.spec/lessons/`,
both numbered repo-wide from `0001`. `.spec/templates/` holds the blanks and
`.spec/schema/` the frontmatter schemas and the manifest.

The root `.spec/schema/` and `.spec/templates/` are **the live copy this
repository checks itself with** — exactly what `specht init` writes into a
consumer. The shipping copy is embedded in `src/specht.tool/schema/v1/` and
`templates/v1/`; the two are the same bytes, and a test says so.

## The twelve sections

The section titles, their order, and the table headers that gate coverage are
written once, as data:
[`.spec/schema/spec-structure.schema.json`](../../../.spec/schema/spec-structure.schema.json).
Rule `SPEC010` reads that manifest, so the list is not restated here or in C#.

Exact text, exactly once each, in order. Who owns which section is in the
[companion skill](../SKILL.md) § "Section ownership", and nowhere else. Each
section carries a `<!-- last written by: <role>, <date> -->` stamp.

Start from [`.spec/templates/feature.md`](../../../.spec/templates/feature.md) and
delete its guidance comments in the copy.

## Frontmatter

22 keys, in a fixed order, validated against
[`.spec/schema/feature-spec.frontmatter.schema.json`](../../../.spec/schema/feature-spec.frontmatter.schema.json).
`spec_status` is **document maturity** (`draft` → `in-review` → `approved` →
`superseded`) and is owned by the file; `status` is **delivery lifecycle** and is
mirrored from the issue label — see [delivery.md](delivery.md) for which fields
mirror and which are derived.

## § 9 is the gate, and `Missing` is honest

Every § 3 claim id appears in § 9 exactly once (`SPEC031`). The row anchors to the
scenario's `@B-00n` **tag**, not the title in the Scenario column — a stale title
is a cleanup, not a gate failure.

`Missing` in the Test column is the correct value until the test exists, and it is
**not** a check failure: it blocks the issue reaching done, not the specification
reaching `approved`. `specht` only requires § 9 to be free of `Missing` once
`spec_status: approved` (`SPEC060`).

## Tables are pipe tables

The checker reads Markdig pipe tables only. A grid table, or a table whose
headers do not match the manifest's for that section, is "no table" (`SPEC013`).
Write the pipe form.

## The § 4 constraint-id gap

`C-nn` is supposed to be a stable id, but in some consumers' specs § 4's first
column is a bare ordinal (`1`..`15`), so every citation into those specs
resolves **by counting rows**. Inserting a row above an existing one silently
shifts every downstream citation.

A spec written here always uses the explicit column — the template does. For a
consumer's spec that does not:

- Never insert a § 4 row above an existing one. Append.
- Normalize the column when that Feature is next touched, one Feature at a
  time.
- `SPEC070` (constraint-citation resolution) is a warning until the
  normalization is done.

## `specht`

```sh
specht                                 # check the current directory; MSBuild-shaped lines, exit 0/1/2/3
specht --root <dir> --strict           # any violation fails
specht --json                          # the report document on stdout instead of the line stream
specht --report .artifacts/spec-check/spec-check.json
specht init                            # write schema and templates into <root>/.spec/, never overwriting
./build.sh SpecCheck                   # the same command, on this repository's own tree
```

Rules are `SPEC###`, grouped: `SPEC00x` frontmatter schema, `SPEC01x` structure
and layout, `SPEC02x` the `.feature` companion, `SPEC03x` claims and traceability,
`SPEC04x` children and tasks, `SPEC05x` the dependency graph, `SPEC06x` approval
consistency. The full vocabulary is README § 4; it is fixed per schema version
and is not restated here. Diagnostics are MSBuild-shaped —
`<path>(<line>): error SPEC031: …` — so GitHub annotates them on the diff.

It is **deterministic and offline**: it never calls GitHub, and every path it
prints is repository-relative. Label-mirror drift is a separate concern and not
this tool's.

## Never add

- A specification under `epics/`, or a `spec.md` anywhere, in this repository.
- A renumbered claim, constraint, question or task id.
- A § 4 row inserted above an existing one in an un-normalized spec.
- A `{{placeholder}}` or a template guidance comment left in a copy.
- A § 9 row deleted to clear a `Missing`.
- A grid table where the manifest expects a table.
- A network call, or an absolute path, in the checker.
