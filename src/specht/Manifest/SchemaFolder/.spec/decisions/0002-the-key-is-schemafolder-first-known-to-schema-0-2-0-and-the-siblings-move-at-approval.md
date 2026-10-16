---
title: "Decision 0002: the key is schemaFolder, schema 0.2.0 first knows it, and the siblings move at approval"
description: "The manifest key is schemaFolder, a flat top-level key; schema 0.2.0 is the first version that defines it; a frontmatter schema written under a named folder keeps the shipped $id because init copies the bytes unchanged; every sibling amendment and the dependency edges land in the pull request that flips 0001-F14 to approved, with a dated amendment note on ADR-0011 and no superseding ADR"
type: decision
---

# Decision 0002: the key is schemaFolder, schema 0.2.0 first knows it, and the siblings move at approval

**Date:** 2026-10-15
**Decided by:** the repository owner (2026-10-15); recorded by spec-author

## The call

1. The key is `schemaFolder`, a flat top-level manifest key, as every claim
   already writes it. B-001 to B-021 stand as written; A-4 is resolved. This
   resolves OQ-1.
2. Schema `0.2.0` is the first shipped version that defines the key. Under
   `0.1.0` the key is unknown and a manifest declaring it is rejected
   (B-013; `0001-F7` C-4, C-11). B-001, B-003 and B-010 to B-013 name `0.2.0`
   where they read "the defining version"; A-3, A-5 and C-6 likewise; the
   Background and the `@B-013` scenario name `0.2.0` and `0.1.0`. This
   resolves OQ-2.
3. The `$id` of a frontmatter schema names the shipped version's file and
   does not follow the consumer's folder. `init` copies the embedded bytes
   unchanged into the named folder, so `0001-F4` B-003 holds there as it
   holds under the default folder. B-022 states it as a claim; § 5 row 11
   cites it. This resolves OQ-3.
4. Every sibling amendment this Feature needs - `0001-F1` C-8; `0001-F4` A-2,
   B-008, B-014, C-2, C-3; `0001-F5` A-5, B-008, B-033, C-9 and decision
   0011's cost line; `0001-F7` B-007, B-009, B-024, B-026, B-077, B-080,
   B-082, C-2, C-4, C-6 - and this Feature's `depends_on` with the siblings'
   `blocks` land in the pull request that flips this Feature to `approved`.
   ADR-0011 gets a dated amendment note pointing at this Feature, in the
   form decision 0011 of `0001-F7` took on 2026-10-11; no superseding ADR is
   written. This resolves OQ-4.

## Why

Recorded by spec-author from the session:

- `schemaFolder` is the shortest name that says what the value is; the
  manifest's other location keys are flat (`0001-F6` B-001), and a nested
  object would be the manifest's first.
- `0.2.0` is unreleased (`0001-F7` OQ-58) and already gains `schemaSource`
  (`0001-F7` decision 0011); one unreleased version takes every new key of
  this round, so a consumer pins once.
- A copied schema that changed its `$id` would differ from the embedded copy
  and from the recorded upstream hash (`0001-F7` B-024), breaking
  `0001-F4` B-003 and the hash verification `0001-F7` B-077 relies on, for
  a field nothing reads.
- The siblings are amended once the claims they would cite are agreed; an
  amendment in pull request 207 would cite text that may still change in
  review, and one item at a time would leave the tree inconsistent between
  items.

## Rejected

The owner was offered these and chose against them; the cost after each is
spec-author's reading:

- **`frontmatterSchemaFolder`.** Longer, and the only schemas the manifest
  names are frontmatter schemas (`0001-F5` B-047). Rejecting it costs a
  rename if a second schema kind ever gains a folder; § 5 row 2 keeps the
  templates folder out.
- **A nested object for the schema settings.** Would group `schemaFolder`,
  `schemaSource` and `frontmatterSchemas`. Rejecting it means the three stay
  flat siblings; a later grouping would be a schema major from `1.0.0` on.
- **The next version after `0.2.0`.** Would ship `0.2.0` with `schemaSource`
  alone and leave this key to another `0.minor`. Rejecting it means `0.2.0`
  cannot ship until this Feature's claims are built or explicitly deferred
  from it (`0001-F7` OQ-58).
- **Reading (b) of OQ-3: the `$id` follows the folder.** Would make `init`
  rewrite a byte of every schema it copies. Rejecting it keeps `init` a copy
  and leaves the `$id` as the version's name, which no reader consults.
- **Amending the siblings now, in pull request 207.** Rejecting it leaves
  `0001-F1` C-8 and the others reading "the folder" until the approval pull
  request, and this Feature `blocked` with an empty `depends_on` until then.
- **Amending the siblings one item at a time.** Rejecting it keeps the
  amendment one reviewable change instead of many partial ones.
- **A new ADR superseding ADR-0011.** ADR-0011 fixes where the frontmatter
  schemas live by default; this Feature adds a setting over that default and
  changes nothing when the key is omitted (B-002). Rejecting it means ADR-0011
  stays accepted with a dated note, as its 2026-10-11 note already does.

## Affects

- OQ-1, OQ-2, OQ-3, OQ-4, each resolved in place.
- A-3 and A-4, resolved in place; A-5 names `0.2.0`.
- B-001, B-003, B-010, B-011, B-012, B-013 and C-6, naming `0.2.0` where they
  read "the defining version"; Status stays `Active`, since no agreed text on
  `main` changed.
- B-022, added, with its `@B-022` scenario and its § 9 row.
- § 5 row 11.
- The Background and the `@B-013` scenario of `schema-folder.feature`.
- Scheduled for the pull request that flips this Feature to `approved`, not
  amended here: `0001-F1` C-8; `0001-F4` A-2, B-008, B-014, C-2, C-3;
  `0001-F5` A-5, B-008, B-033, C-9 and decision 0011; `0001-F7` B-007, B-009,
  B-024, B-026, B-077, B-080, B-082, C-2, C-4, C-6; this Feature's
  `depends_on` and those Features' `blocks`; ADR-0011's amendment note;
  `epics/0001-specht/epic.md`'s row for this Feature.

## Reversal

None.
