---
title: "Decision 0003: a root-relative exclusion begins with a slash"
description: "In the manifest's exclusion list, an entry beginning with / is a root-relative path excluded at that place; an entry with no / is a directory name excluded at any depth; any other form is an invalid manifest"
type: decision
---

# Decision 0003: a root-relative exclusion begins with a slash

**Date:** 2026-10-07
**Decided by:** spec-author, on spec-reviewer finding 3; follows git's own anchoring convention

## The call

An entry in the manifest's exclusion list that begins with `/` is a path
relative to the root, excluded at that place only (`/.spec`). An entry with no
`/` is a directory name, excluded at every depth (`node_modules`). An entry of
any other form - a `/` inside a name with no leading `/` - is not a valid
exclusion.

## Why

- The default list carries both kinds: `bin` everywhere, the root `.spec/` only
  at the root. Written bare, `.spec` would exclude every co-located
  specification, so the two must be told apart by the entry alone.
- A leading `/` anchors a pattern to the root in `.gitignore`. A maintainer
  reading the manifest beside `.gitignore` reads the same meaning.

## Rejected

**Two keys, one per kind.** A list of names and a list of paths.

- Two lists to keep in step, against C-5's "declared once".
- Cost of rejecting: one rule of form to learn. Taken.

**Any `/` anywhere anchors, as in `.gitignore`.** `docs/generated` would be
root-relative.

- `.spec/` with a trailing `/` would read as a path to a careless eye and as a
  name to git; one form per meaning removes the guess.
- Cost of rejecting: `docs/generated` must be written `/docs/generated`. Taken.

## Affects

- § 3 B-002.
- `0001-F5`: the exclusion entry's grammar and the rejection of a malformed
  entry belong to the manifest's validation; that Feature's author applies it.

## Reversal

None.
