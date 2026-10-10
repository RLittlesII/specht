---
title: "Decision 0001: the schema is the one the check selects, with no fallback"
description: "specht frontmatter checks a document against the kind's frontmatter schema of the version and source the check selects for the root, and a root without a manifest exits 2 as it does for the check; the embedded schemas are never a fallback"
type: decision
---

# Decision 0001: the schema is the one the check selects, with no fallback

**Date:** 2026-10-09
**Decided by:** the repository owner, in the need for this Feature, which states no fallback as the default; recorded by `spec-author`

## The call

The command reads the manifest at the root it is given and checks the document
against the kind's frontmatter schema of the version that manifest pins, from
the source `0001-F7` selects for that root: the embedded set by default, the
on-disk `.spec/schema/` copy when B-009 there selects it, an upstream copy when
one is recorded. It is the schema the check would use on the same root.

A root with no manifest is a missing input and exits `2`, as it does for the
check (`0001-F2` B-006). The embedded schemas of the newest version are never
used in its place.

## Why

- The owner's need is that the pinned schema version applies. The pin selects
  a version and `0001-F7` selects its source; a command that read the on-disk
  schema files directly would disagree with the check whenever the embedded
  source is selected (`0001-F7` B-010), and one frontmatter would get two
  verdicts.
- A fallback silently checks against a version the consumer never pinned, so
  a document passes here and fails once it is in the tree.

## Rejected

**Read `<root>/.spec/schema/*.frontmatter.schema.json` directly.** Simple, but
diverges from the check under the default embedded source. Cost of rejecting:
the command depends on `0001-F7`'s source selection.

**Fall back to the newest embedded version when the root has no manifest.**
Lets an author with no repository at hand check a document. Cost of rejecting:
such an author runs `specht init` in an empty directory first, or passes
`--root` at a repository that has a manifest.

## Affects

- `B-006`, `B-009`, `B-017`.
- `C-2`.

## Reversal

None.
