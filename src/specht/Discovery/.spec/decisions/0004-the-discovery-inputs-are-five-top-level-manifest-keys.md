---
title: "Decision 0004: the discovery inputs are five top-level manifest keys"
description: "The manifest declares what discovery finds and skips under five flat top-level keys - layouts, exclusions, taskFiles, epicFiles and companionFiles - and a glob has two wildcards, ** as a whole segment and * within one, matched case-sensitively against the root-relative path"
type: decision
---

# Decision 0004: the discovery inputs are five top-level manifest keys

**Date:** 2026-10-09
**Decided by:** the repository owner, asked directly during delivery of item `0005`, ratifying the proposal in § 7 (§ 11 OQ-4); recorded by spec-author

## The call

The manifest declares discovery's inputs under five flat top-level keys:

| Key              | Shape                               | Default                                                                                                     | Claim |
| ---------------- | ----------------------------------- | ----------------------------------------------------------------------------------------------------------- | ----- |
| `layouts`        | An array of `{ "name", "glob" }`    | `legacy` = `epics/**/spec.md`, then `coLocated` = `**/.spec/README.md`                                      | B-001 |
| `exclusions`     | An array of strings (decision 0003) | `.git`, `.artifacts`, `.skillfile`, `.claude`, `graphify-out`, `bin`, `obj`, `node_modules`, `/.spec` (A-2) | B-002 |
| `taskFiles`      | A string, a file-name glob          | `{task}-*.md`                                                                                               | B-003 |
| `epicFiles`      | A string, a glob                    | `epics/**/epic.md` (decision 0001)                                                                          | B-003 |
| `companionFiles` | A string, a file-name glob          | `*.feature`                                                                                                 | B-003 |

- The order of `layouts` is the order the summary and the report name the
  layouts in.
- A manifest that declares `layouts` replaces the default list whole.
- A layout's `name` is the name B-009 prints.
- `{task}` in `taskFiles` stands for the manifest's `identifiers.task` grammar.

The glob dialect: a glob is `/`-separated; a segment that is exactly `**`
matches zero or more directories; `*` matches any run of characters within one
segment; every other character is itself. A glob is matched case-sensitively
against a file's root-relative path with `/` separators; a file-name glob
(`taskFiles`, `companionFiles`) is matched against the file name alone.

## Why

- A top-level key gets `0001-F5` B-012's unknown-key rejection and B-019's
  default from the manifest loader as it stands.
- `layouts` is a list because its order is output order, and a list that is
  replaced whole lets a consumer drop the legacy layout.
- `legacy` and `coLocated` are the names the summary and the report print
  today, so the default manifest changes no output.
- `task` is the kind name `0001-F5` decision 0003 ratified.
- `{task}` keeps the task identifier's grammar written once (`0001-F5` C-2).
- Git's `:(glob)` pathspec reads `**` and `*` the same way, so both discovery
  modes can read one glob (C-3).

## Rejected

**One nested `discovery` object holding the five.** Groups the keys under one
name. A misspelt member would need its own unknown-key rejection and its own
default fill, or would silently read as the default. Cost of rejecting: five
more names at the manifest's top level.

**Different names.** None was preferred to the five proposed. Cost of
rejecting: `taskFiles` says task where B-003 says item, as `0001-F5`
decision 0003 already accepted for `frontmatterSchemas.task`.

## Affects

- B-001, B-002, B-003: each claim's input now has a key; B-003's default item
  shape is written `{task}-*.md`, which under the default task grammar is the
  `<epic>-<nn>-*.md` the claim named.
- B-009: the name printed is the layout's `name`.
- C-6 (new): the glob dialect. C-7 (new): `layouts` is replaced whole and
  ordered.
- § 11 OQ-4: resolved. OQ-7 stays open: what a glob token outside the dialect,
  a duplicate layout name, a malformed layout entry or a file two layouts match
  does was not decided here.
- `0001-F5`: the five keys join the keys its loader knows (B-012, B-019).
- `0001-F4` decision 0001: the five are tool-owned keys, shipped in the
  embedded version 1 manifest.

## Reversal

**Amended 2026-10-09 by the repository owner ([decision 0008](0008-the-layouts-are-named-epics-and-features-and-the-file-shape-keys-are-lists.md)).**
Two parts of the call above are superseded, and the sections above stand as
written.

- The default layout names: `legacy` reads as `epics` and `coLocated` as
  `features`. The Why line that the two names change no output no longer
  holds: the default manifest's summary and report now print the new names.
- `taskFiles`, `epicFiles` and `companionFiles` are each a list of globs, not
  a string. The defaults are the same globs as lists of one, a file matching
  any entry counts, and an empty list is rejected (`0001-F5` B-041).

The five keys, flat and top-level, `layouts` replaced whole and ordered,
`{task}` and the glob dialect stand.
