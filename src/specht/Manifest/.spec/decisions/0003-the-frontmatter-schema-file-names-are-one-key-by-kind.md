---
title: "Decision 0003: The frontmatter schema file names are one key, by kind"
description: "B-008's file names live under one top-level manifest key, frontmatterSchemas, keyed feature, task and epic, filled per kind from the default manifest and shipped as a tool-owned key"
type: decision
---

# Decision 0003: The frontmatter schema file names are one key, by kind

**Date:** 2026-10-09
**Decided by:** the repository owner, during planning of item `0017`; recorded by spec-author

## The call

The manifest names the frontmatter schema files B-008 loads under one top-level
key, `frontmatterSchemas`, an object keyed by kind:

| Kind      | Default file name                      |
| --------- | -------------------------------------- |
| `feature` | `feature-spec.frontmatter.schema.json` |
| `task`    | `task.frontmatter.schema.json`         |
| `epic`    | `epic.frontmatter.schema.json`         |

Each value is a file name read under `<root>/.spec/schema/`. B-008's item kind
is spelled `task`. A manifest that leaves out a kind reads that kind's file name
from the default manifest, per kind, as `identifiers` grammars are filled
(B-019). The key ships in this repository's live manifest and in the embedded v1
manifest, so it is tool-owned and `init` writes it. The names are read only
under the on-disk source (C-9).

## Why

- `task` matches the file stem, so the key and the file it names read alike.
- One object keyed by kind is the shape `identifiers` already has, so the fill
  rule is the one B-019 already states, not a new granularity.
- Shipping the key in both manifests makes it tool-owned under `0001-F4`
  decision 0001, so B-004's comparison covers it and `init` gives a consumer
  the defaults to edit. Version 1 is still open to change before first publish
  (`0001-F7` decision 0002).

## Rejected

**`item` as the kind name.** Matches B-008's wording and this repository's
`.issue/` items. Cost of rejecting: the key says `task` where B-008 says item,
and a reader maps one to the other.

**`schemas` as the key name.** Shorter. Rejected because it is ambiguous beside
`schemaVersion` and the schema-source keys `0001-F7` OQ-1 will add. Cost of
rejecting: a longer key.

## Affects

- B-008: the manifest's file names now have a key; the claim's text is
  unchanged.
- B-019: `frontmatterSchemas` joins the per-name fill `identifiers` has.
- C-9: unchanged; the names are read only under the on-disk source, and the
  embedded source is not built yet (`0001-F7`).
- § 11 OQ-1: partly resolved, for this key only.
- `0001-F4` decision 0001: `frontmatterSchemas` is a tool-owned key.

## Reversal

None.
