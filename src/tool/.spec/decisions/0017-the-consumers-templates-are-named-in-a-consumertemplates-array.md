---
title: "Decision 0017: the consumer's templates are named in a consumerTemplates array"
description: "Ratified by the repository owner on 2026-10-24: the manifest key that names the consumer's templates is consumerTemplates, a top-level array of template file names relative to .specht/templates/, part of schema 0.1.0, with no form check beyond the array of strings"
type: decision
---

# Decision 0017: the consumer's templates are named in a consumerTemplates array

**Date:** 2026-10-24
**Decided by:** the repository owner, 2026-10-24 (see Ratification). Proposed by the `implementer`, answering the part of `0001-F7` OQ-1 that decision 0006 left to the `implementer`

## Status

**Ratified 2026-10-24 by the repository owner, as proposed.** See
[Ratification](#ratification). The rest of this section and the sections down to
"Affects" are the proposal as it was put, and are not rewritten.

As proposed: **Proposed. Not ratified.** Nothing below was a decision until the
owner ratified it. No claim names the key and no code reads it; item `0130`
builds it now it is ratified.

## The proposal

- The key is `consumerTemplates`, a top-level array of strings.
- An entry is a template file name as the pinned version ships it, relative to
  `.specht/templates/`, for example `feature.md`.
- It is part of schema `0.1.0` (decision 0006): the engine knows the key at
  every pin, so `0001-F5` B-012 does not reject it. No schema version, embedded
  manifest or frontmatter schema changes.
- The parser reads the key through the existing `Strings` helper, which already
  faults a value that is not an array of strings. It adds no further form check:
  an empty entry, a path separator or `..` is accepted, because no claim states a
  rule for it. A name the pinned version does not ship is not checked (OQ-27, open).

## Why

- File names are the form `frontmatterSchemas` already uses, and arrays are the
  form the list-valued keys use.
- `adr`, `decision` and `lesson` templates have no kind, so a map keyed by kind
  cannot name them.

## Rejected

- `templates`: close to the `templates` member of `spechtSchema` (OQ-65).
- A map or an object per template: nothing configures a template beyond its name.
- A boolean: the all-or-nothing form decision 0006 rejected.

## Affects

`0001-F7` § 7 "The consumer's templates key", OQ-1, B-065; items `0130`, `0131`.

## Ratification

**Date:** 2026-10-24
**Ruled by:** the repository owner.

**Ratified as proposed.** The key is `consumerTemplates`, a top-level array of
template file names relative to `.specht/templates/`, part of schema `0.1.0`,
with no form check beyond the array of strings. `0001-F7` OQ-1 is resolved for
this key. `0001-F7` OQ-27, whether a name the pinned version does not ship is
checked, is not decided here and stays open.
