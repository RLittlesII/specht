---
title: "Decision 0002: only the sections the manifest names follow it, and a record's status is claimed"
description: "The facts follow the manifest only for the roles and tables the shipped manifest declares; sections 4, 5, 10 and 11 are read under the engine's fixed headings and headers, with no 0001-F5 delta; B-011 claims a record's status, and OQ-3 asks only how it is read"
type: decision
---

# Decision 0002: only the sections the manifest names follow it, and a record's status is claimed

**Date:** 2026-10-30
**Decided by:** the repository owner, 2026-10-30, answering `spec-reviewer`'s
first review of this specification; recorded by spec-author

## The call

1. The manifest-driven reading applies only to the sections and tables the
   shipped manifest names: `roles` (`claims`, `matrix`, `signOff`) and
   `tables` (`matrix`, `signOff`). The constraints, out-of-scope, lessons and
   deltas, and open-questions sections are read under the engine's fixed
   headings and table headers. This Feature adds no dependency on a
   `0001-F5` delta that would put them in the manifest.
2. B-011 claims a record's status, as decision 0001 call 1 already said.
   OQ-3 asks only how that status is read.

## Why

- B-016 as first written named section and header keys no shipped manifest
  declares, so a test could not set them.
- Decision 0001 call 1 names the record's status; a claim without it left
  that call unclaimed.

## Rejected

- **A `0001-F5` delta adding the four sections to the manifest's roles and
  tables.** Cost: this Feature would wait on a manifest change and a new
  schema version (`0001-F7` C-11).
- **Leaving a record's status out of B-011 until OQ-3 is answered.** Cost:
  the claim falls short of decision 0001 call 1.

## Affects

A-2; B-006 to B-008, B-011, B-016; C-1, C-7; OQ-3. Amends decision 0001 call 5
("values read through the pinned manifest") to the sections and tables the
manifest names.

## Reversal

A later `0001-F5` delta that puts more sections in the manifest widens B-016
by amendment; the document's shape does not change (C-3, C-5).

## Amended

2026-10-30, by the owner, after `spec-reviewer` round 2. Call 1 rested on a
wrong premise: the engine has no fixed headings. The check takes every
required heading from the manifest's `sections` key, or the pinned version's
defaults where the manifest leaves it out. The facts therefore find the
constraints, out-of-scope, lessons and deltas, and open-questions sections by
the heading `sections` lists for them, as the check does. `sections` is a key
the shipped manifest names, so call 1's narrowing to shipped keys holds; the
table headers inside those four sections stay at the engine's reading,
because no shipped key names them. Affects B-016, C-1 and C-7.
