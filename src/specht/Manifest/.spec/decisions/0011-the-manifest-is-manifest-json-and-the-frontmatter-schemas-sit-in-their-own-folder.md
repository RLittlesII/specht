---
title: "Decision 0011: The manifest is manifest.json, and the frontmatter schemas sit in their own folder"
description: "The owner's decision of 2026-10-11: the manifest is read at <root>/.specht/manifest.json, the frontmatter schemas under <root>/.specht/schema/frontmatter/ as feature.schema.json, task.schema.json and epic.schema.json, and a frontmatterSchemas value stays a plain file name; supersedes decision 0003's default names and the folder it reads them under, and overrules ADR-0011's open point 1"
type: decision
---

# Decision 0011: The manifest is `manifest.json`, and the frontmatter schemas sit in their own folder

**Date:** 2026-10-11
**Decided by:** the repository owner, answering three of four questions on the need; recorded by `spec-author`

## The call

The owner's need: "I'd like to rename from `schema/task.frontmatter.schema.json`
[to] `schema/frontmatter/task.schema.json`; I am also questioning are these
schema, or are they really just default configuration?"

| File                         | Was                                                   | Is                                               |
| ---------------------------- | ----------------------------------------------------- | ------------------------------------------------ |
| The manifest                 | `.specht/schema/spec-structure.schema.json`           | `.specht/manifest.json`                          |
| Feature frontmatter schema   | `.specht/schema/feature-spec.frontmatter.schema.json` | `.specht/schema/frontmatter/feature.schema.json` |
| Item frontmatter schema      | `.specht/schema/task.frontmatter.schema.json`         | `.specht/schema/frontmatter/task.schema.json`    |
| Epic frontmatter schema      | `.specht/schema/epic.frontmatter.schema.json`         | `.specht/schema/frontmatter/epic.schema.json`    |
| The templates the tool ships | `.specht/templates/*.md`                              | unchanged                                        |

1. **The manifest moves out of `schema/` and is renamed.** It is
   configuration, and its own `$comment` says "Not a JSON Schema".
2. **The frontmatter schemas are JSON Schemas and stay under `schema/`**, in a
   `frontmatter/` folder of their own, named `<kind>.schema.json`.
3. **A `frontmatterSchemas` value stays a plain file name**, now read under
   `<root>/.specht/schema/frontmatter/`. The default manifest's values are the
   three names above.

Four points were not put to the owner as questions, and none was stated by
the owner. The first three are defaults spec-author proposed, contained in a
plan the owner approved on 2026-10-11; the fourth is spec-author's reading.

- **A value that is not a plain file name is an invalid manifest**: one that is
  absolute, or holds `..` or a path separator, is rejected at load with exit
  `3` (B-047, B-022, B-023). Derived by spec-author to answer OQ-2 (a). The
  option the owner chose in point 3 was described as leaving OQ-2 able to
  reject a value with a path separator; the owner said no more on it.
- **No earlier location under `.specht/` is a retired one.** The names in the
  "Was" column never shipped, so the tool does not look for them and no
  message names them. The retired location of ADR-0011 stands.
- **An embedded schema's `$id` follows the layout**: the version segment, then
  `/frontmatter/` and the file name (`0001-F7` C-6). The form of the version
  segment stays `0001-F7` OQ-22.
- **`schemaVersion` does not change.** Schema `0.1.0` is unpublished
  (ADR-0011, open point 4; `0001-F7` C-1, decision 0007). Spec-author's
  reading of those records.

The rename is free only while nothing is published. Once a package ships
schema `0.1.0`, these names are that version's (`0001-F7` C-1).

## Why

- The owner's reason: the manifest is configuration, and a file named
  `*.schema.json` in a folder named `schema/` reads as a JSON Schema.
- With the manifest out of it, `schema/` holds only schemas.
- The folder says "frontmatter", so the file name need not.

## Rejected

**Keep `spec-structure.schema.json` under `schema/`**, the default of ADR-0011
open point 1. Cost of rejecting: every claim, test and document naming the path
is amended a second time on the same day.

**Keep the `*.frontmatter.schema.json` names beside the manifest**, the same
default. Cost of rejecting: the `$id` of each embedded schema changes with its
file name (`0001-F7` C-6).

**A `frontmatterSchemas` value that is a path**, which the owner's "plain file
names" excludes. Cost of rejecting: a consumer's own schema must sit in
`schema/frontmatter/`.

## Affects

- A-1, A-5, B-008, B-033, C-6, C-9: the paths. B-047 added; B-022 and B-023
  name it. § 5 row 4 withdrawn, row 15 added. OQ-2 (a) resolved; this
  Feature's OQ-22 raised.
- Decision 0003: its default file names and the folder it reads them under are
  superseded; the key, its kinds and its fill stand.
- ADR-0011: open point 1 overruled, recorded in its Status.
- `0001-F1` C-8; `0001-F2` A-3; `0001-F4` A-1, A-2, B-004, B-005, B-008, B-014, B-018, C-3;
  `0001-F7` B-007, B-009, B-024, B-026, B-077, B-080, B-082, C-2, C-4, C-6.

## Reversal

None.
