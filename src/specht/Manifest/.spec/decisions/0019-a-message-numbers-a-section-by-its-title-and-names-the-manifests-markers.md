---
title: "Decision 0019: a message numbers a section by its title and names the manifest's markers"
description: "The owner's two rulings of November 3 2026 on item 0018: a message names a section § N by the leading number of its role's title, or ## <title> when the title has none, at every pin; and B-009 widens to markers, so a message naming a marker names the manifest's text"
type: decision
---

# Decision 0019: a message numbers a section by its title and names the manifest's markers

**Date:** 2026-11-03
**Decided by:** the repository owner, answering item `0018`'s two questions; recorded by spec-author

## The call

1. **Section spelling.** A rule message that refers to a section derives
   `§ N` from the leading number of that role's title in the manifest. A
   claims role titled `3. Claims` gives `§ 3`. A title with no leading
   number is named in full, as `## <title>`.
2. Call 1 holds at every schema pin. Under the default manifest no golden
   report changes.
3. **Markers.** B-009 widens to markers. A message that names a marker -
   missing, approved, done, withdrawn and the rest - names the manifest's
   text for it. Under the default manifest no golden report changes.
4. The manifest's own path in a message is `.specht/manifest.json`. It is a
   fixed constant that no manifest key sets, and `SPEC2001` already writes
   it.

## Why

- The default titles all lead with their number, so call 1 reproduces
  `§ 3`, `§ 9` and `§ 12` exactly. The golden report compares the message
  (`0001-F1` C-9), and no golden report is relocked.
- A message that keeps `§ 9` after the manifest moved the matrix to another
  section points the reader at the wrong place.
- § 7's "What does not move" paragraph already assigns `'Missing'` and
  `'approved'` in messages to B-009 and item `0018`. Call 3 makes the claim
  say what § 7 already says.
- The default marker texts are the literals the messages hold today (A-4),
  so call 3 changes no verdict under the default manifest.

## Rejected

- **(Section) Name the full title from schema `0.3.0` and relock the
  golden report `engine-v0.3.0.json` (`0001-F1` B-018).** Every message
  citing a section would change its text under the default manifest, and
  older pins would spell a section differently from `0.3.0`.
- **(Markers) Keep markers out of B-009 and correct § 7.** A consumer that
  renames a marker would still read the old literal in every message, which
  brief § 5 and C-1 rule out.

## Affects

- `0001-F5` B-009 and its `@B-009` scenarios.
- Item `0018`.
- § 7's "What does not move" paragraph, which `implementer` brings true in
  the change that builds item `0018`.

## Reversal

None.
