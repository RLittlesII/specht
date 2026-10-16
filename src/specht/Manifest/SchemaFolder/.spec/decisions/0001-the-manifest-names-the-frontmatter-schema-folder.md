---
title: "Decision 0001: the manifest names the frontmatter schema folder"
description: "One optional top-level manifest key names the root-relative folder the frontmatter schemas are read from and written to; omitted, it is today's folder and no verdict changes; a folder outside the root is rejected; the key is a consumer setting first known to one schema version; cut as its own Feature rather than an amendment to 0001-F5"
type: decision
---

# Decision 0001: the manifest names the frontmatter schema folder

**Date:** 2026-10-15
**Decided by:** the repository owner (2026-10-15) for the need and for which
location moves - the frontmatter schema folder, not the specification roots and
not the manifest's own location; spec-author for each position below, as the
default put to the owner, awaiting the owner where § 11 says so; recorded by
spec-author.

## The call

The owner, 2026-10-15, asked for a feature request "to decouple the schemas
from the `.specht` folder", then: "I think I just realized it shouldn't matter.
You should be able to define where the spec lives in the manifest, right?" Of
the three locations that could move - the specification roots, the manifest's
own location, the frontmatter schema folder - the owner chose the frontmatter
schema folder. The specification roots are already the manifest's
(`0001-F6` B-001); the manifest's own location stays (`0001-F5` C-6).

1. One optional top-level manifest key, written `schemaFolder` here (OQ-1),
   names the folder the frontmatter schemas are read from and written to, as
   a root-relative directory path with `/` between its segments (B-001; C-1).
   Omitted, the folder is `<root>/.specht/schema/frontmatter/`, so the
   default manifest reproduces the baseline verdicts (B-002; `0001-F5` B-016,
   C-4). The shipped manifest of the defining version does not carry the key,
   as it does not carry `schemaSource` (B-003).
2. Every reader and writer of a frontmatter schema follows the key: the
   on-disk source (B-004; `0001-F7` B-009), the verified copy of a recorded
   upstream source (B-005; `0001-F7` B-024, B-077, B-080), `init` with the
   frontmatter schemas in its selection (B-007; `0001-F4` A-1) and `upgrade`
   (B-009; `0001-F7` B-026, B-082). Under the embedded source the key changes
   no verdict (B-006; `0001-F7` B-010). There is one place and no fallback
   (C-3; § 5 row 10).
3. The folder never leaves the root: an absolute path, a `..` segment, a `\`,
   the empty text and a `.` segment are each an invalid manifest, exit `3`,
   the key named and the value not repeated (B-010 to B-012; C-1, C-8). That
   the empty text and `.` are rejected is spec-author's default, the one
   `0001-F5` OQ-22 proposes for a `frontmatterSchemas` value: both name the
   root itself as the folder, which no file-name value can reach, and
   accepting one would put a frontmatter schema beside the consumer's own
   files at the root for no stated need.
4. The key is first known to one shipped schema version (OQ-2) and rejected
   under an older pin (B-013; `0001-F7` B-102, C-4). It composes with
   `frontmatterSchemas`, whose values stay plain file names (B-016; C-5;
   `0001-F5` B-047). It is a consumer setting in the sense of `0001-F5` C-13
   (B-019; C-7).
5. It moves nothing else: not the manifest (B-020; C-2), not the templates
   (B-021), not discovery (B-017). The retired location, `.spec/schema/`, is
   a folder like any other under this key: a consumer who names it reads it,
   and `0001-F2` B-037 to B-039 stand for a manifest left there (B-018).

## Why

- The design direction moves every literal the engine hardcodes into the
  manifest (brief § 5): the places discovery looks are already keys
  (`0001-F6` decision 0004), the schema source is a key (`0001-F7` decision
  0011), and the frontmatter schema folder is the one place the engine looks
  that no key names.
- A folder that stays inside the root keeps every path in every output
  root-relative (brief § 9) and keeps the check offline and deterministic: the
  same tree gives the same report, because the folder is in the tree.
- An omitted key reading today's folder is the Should-7 rule every manifest
  key has followed (`0001-F5` A-4, C-4): no consumer notices until it sets the
  key.
- Naming the key and the version openly (OQ-1, OQ-2) follows `0001-F7`, whose
  claims said "the pin command" until `0001-F7` OQ-11 named it; the claims here
  hold under any name.

## Why a new Feature, not an amendment to `0001-F5`

- `0001-F5` is `in-review` with some sixty claims; only one branch may amend a
  Feature's § 1-5 at a time, and a separate agreement lets the owner accept or
  reject this one without reopening that review.
- `0001-F5` decision 0011 rejected the nearest alternative, a path in a
  `frontmatterSchemas` value, and priced the rejection: "a consumer's own
  schema must sit in `schema/frontmatter/`". This Feature pays that cost down
  by another route, which is a new agreement, not a reopening of that one.
- A Feature is cut where the invariant changes (epic `0001`, "Features"). The
  invariant here - the engine reads and writes the frontmatter schemas where
  the manifest says, never where a literal says, and the default manifest
  changes no verdict - belongs to neither `0001-F5` (the manifest never adds a
  rule) nor `0001-F7` (the pin selects the version).
- The siblings it touches are amended on agreement, in one pull request or one
  item at a time (OQ-4); until then each states the literal folder, and A-1
  says so.

## Why `depends_on` is empty

The claims read `0001-F5`'s manifest parsing, `0001-F7`'s version gate and
`0001-F4`'s `init` selection, so F5, F7 and F4 are the natural edges. Declaring
them requires the symmetric `blocks` in each sibling's frontmatter
(`SPEC051`), and this specification is written without editing any sibling,
so that the owner reads one new Feature and nothing else moves before
agreement. The edges are added with the sibling amendments OQ-4 decides, in
the same change. `0001-F13` declared its edges at birth because its pull
request also edited the siblings; this one does not.

## Rejected

Each cost is spec-author's reading.

- **Amending `0001-F5` instead of a new Feature.** Cost of rejecting: a reader
  of `0001-F5` sees the literal folder until OQ-4's amendments land.
  Rejected because of the review lock and the invariant rule above.
- **A path in a `frontmatterSchemas` value.** Rejected by the owner,
  2026-10-11 (`0001-F5` decision 0011, B-047), and not reopened: a path per
  kind puts three places where one is needed and lets one kind escape the
  folder. Cost of rejecting: a consumer cannot spread the three schemas over
  two folders.
- **A command-line option naming the folder, with or without the key.** The
  command line selects a source for one run (`0001-F7` decision 0011); the
  folder is where the tree keeps its files, which the tree records, so the
  same tree gives the same report. Cost of rejecting: trying a folder means
  editing the manifest (§ 5 row 5).
- **A folder outside the root, or shared between repositories by path.**
  Rejected by the owner, 2026-10-08 (`0001-F7` decision 0004): it breaks
  root-relative paths and a CI checkout does not have the other path. The
  shared schema set is an upstream source, `spechtSchema`. Cost of rejecting:
  a monorepo of several roots carries the schemas once per root or records a
  source.
- **Moving the manifest's own location with the same key, or a second key.**
  Not chosen by the owner, 2026-10-15; `0001-F5` C-6 stands. Cost of
  rejecting: the configuration folder keeps the manifest, so a consumer
  separating configuration from content still has `.specht/`.
- **A fallback to the default folder for a file the named folder lacks.** Two
  places to look is the silent-divergence failure `0001-F7` § 1 removed. Cost
  of rejecting: a consumer setting the key moves every required file (§ 5
  row 9).
- **Rejecting a `.` segment and the empty text in a later question.** Leaving
  them open would leave B-010 and B-011 with a hole the implementer fills
  silently. Cost of deciding here: the owner may reverse it; a reversal is a
  decision record.
- **An `init` option that writes the key.** `init` writes defaults and never
  rewrites a manifest (`0001-F4` C-1, decisions 0002 and 0004). Cost of
  rejecting: a consumer edits the manifest by hand, as for the epic grammar.

## Affects

- B-001 to B-021; A-1 to A-6; C-1 to C-8; § 5 rows 1 to 11; OQ-1 to OQ-4.
- On agreement (OQ-4): `0001-F5` A-5, B-008, B-033, C-9 and decision 0011's
  "Rejected" cost line; `0001-F4` A-2, B-008, B-014, C-2, C-3; `0001-F7`
  B-007, B-009, B-024, B-026, B-077, B-080, B-082, C-2, C-4, C-6; `0001-F1`
  C-8; ADR-0011 decision 1.
- Epic `0001`: `0001-F14` joins its children and its Features table.

## Reversal

None.
