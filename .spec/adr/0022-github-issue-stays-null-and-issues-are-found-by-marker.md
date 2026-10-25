---
title: "ADR-0022: github_issue stays null, and mirrored issues are found by marker"
description: "Every Feature specification and epic keeps github_issue and synced_at null; the work-item mirror finds each GitHub issue by the hidden marker in its body, and the shipped Feature template is not edited for the mirror"
type: adr
---

# ADR-0022: github_issue stays null, and mirrored issues are found by marker

## Status

Accepted, 2026-10-24 by the repository owner (`0055-F9` OQ-3).

## Context

`0055-F9` mirrors every work item and epic as a GitHub issue and finds each
issue by the hidden `specht-item` marker in its body (`0055-F9` B-003, B-049).
Every Feature specification and every epic carries a `github_issue` key, `null`
today. A filled `github_issue` makes `SPEC005` require a `synced_at` stamp,
and the Feature template's comment reads a filled `github_issue` as making
`status` mirrored from the issue - the reverse of `0055-F9` C-2. A shipped
template is never changed in place (`0001-F7` C-1). The rule binds 12 epics and
every Feature specification across all epics, so its blast radius is the
repository.

## Considered options

1. Leave `github_issue` and `synced_at` `null` everywhere; the marker is the
   only link between a file and its issue.
2. Fill `github_issue` once, by hand, from the first mirror run's output.

## Decision

Option 1. `github_issue` and `synced_at` stay `null` in every Feature
specification and every epic, the mirror finds issues by marker, and the
shipped Feature template is not edited for the mirror.

## Consequences

- One link between a file and its issue, held on the GitHub side; nothing in
  the tree can drift from it, and the mirror writes nothing into the tree
  (`0055-F9` C-1).
- No `synced_at` stamps to keep, and no template change.
- A reader of a specification finds no issue number in its frontmatter; the
  issue is found by searching GitHub for the id.
- Option 2 is rejected: a hand edit to every epic and specification, a `synced_at` in each to satisfy
  `SPEC005`, and a template whose comment then contradicts the mirror.
