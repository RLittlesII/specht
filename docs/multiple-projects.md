---
title: "Checking several projects with specht"
description: "What one run reads, how to check projects that each carry their own manifest, and what projects under one root share"
type: reference
---

# Multiple projects

The root is the unit. One run reads one root and one manifest,
`<root>/.specht/schema/spec-structure.schema.json`.

## One root per project

Give each project its own `.specht/` and run the check once per project. Each
root has its own `schemaVersion` pin, `layouts`, `sections` and `rules` levels,
and nothing is shared between runs. One install checks them all: the tool
embeds every schema version it ships ([Configuration](configuration.md)).

```sh
specht init --root billing        # once per project; the directory must exist
for root in billing shipping; do specht --root "$root" || failed=1; done
exit "${failed:-0}"
```

A CI matrix over the roots does the same. Paths in the output are relative to
the root given and no line names the root, so keep each run's output apart.

## Several projects under one root

Every `.spec/README.md` at any depth below the root is found in one run and
checked against the root's one manifest, so the projects share one schema
version, one set of sections and one level per rule. Some rules read the whole
root:

| Rule                 | Scope                                                                            |
| -------------------- | -------------------------------------------------------------------------------- |
| `SPEC012`            | The root. Two projects that both declare `0001-F1` collide.                      |
| `SPEC050`–`SPEC052`  | The root. A `depends_on` or `blocks` edge may name a Feature in another project. |
| `SPEC041`            | The root. A spike may sit beside any specification.                              |
| `SPEC044`            | The root. Item ids are unique and contiguous per epic, across every project.     |
| `SPEC040`            | The specification's own folder.                                                  |
| `SPEC030`, `SPEC031` | One specification. Two projects may both have a `B-001`.                         |

[What it checks](what-it-checks.md) lists every rule.

## Not supported

- A manifest below the root. A nested `.specht/` is ignored by a run on the
  parent, and `--root` on a folder with no `.specht/` exits `2`.
- A rule level per path or per Feature. `rules` sets one level for the root.
- Several roots in one run. Of several `--root` options, only the last is
  checked.
- An edge to a Feature in another root. `SPEC050` reports it.

Two defaults matter when roots nest. Only the root's own `.spec/` is skipped,
so a run on the parent reads a nested project's `billing/.spec/README.md` as a
specification. The legacy layout `epics/**/spec.md` is matched from the root,
so a nested project's `epics/` is found only by a run on that project.
