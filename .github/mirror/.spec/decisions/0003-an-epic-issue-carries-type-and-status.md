---
title: "Decision 0003: an epic's issue carries a type label and its status"
description: "An epic's issue is labelled type:epic and status:<status> from the epic's frontmatter, is closed when the epic is done, and its body is the epic's Summary section plus the file in a fenced markdown block"
type: decision
---

# Decision 0003: an epic's issue carries a type label and its status

**Date:** 2026-10-24
**Decided by:** the repository owner (2026-10-24), answering OQ-6; recorded by spec-author

## The call

An epic's issue is labelled `type:epic` and `status:<status>`, the status
taken from the epic's frontmatter. It is closed when the epic's `status` is
`done` and open otherwise. Its body renders the epic's Summary section and
carries the epic file verbatim in a fenced `markdown` block, between the
marker and provenance line before it and the footer after it.

## Why

An epic is Markdown, not a work item, so B-007, B-008, B-012, B-013 and B-020
say nothing about it. Giving it the same labels and the same state rule as an
item lets a reader filter and close epics as they do items, and its Summary is
the part of the file that says what the epic is.

## Rejected

**Summary only, always open.** Cost of rejecting: an epic's issue changes state
and labels as its frontmatter does, which is more to keep in line. Taking it
would leave a finished epic open on GitHub forever and unfilterable by type
or status.

## Affects

- `0055-F9` B-043 to B-048 (new); OQ-6 resolved; OQ-9 (new).

## Reversal

None.
