---
title: "Decision 0009: a version's folder is its number, one number covers the set, and a version lock freezes it"
description: "The folder that holds a schema version is named v and its major.minor.patch, and the $id segment is the same; the schema set, the templates and the rule pages share one number, which a rule page states; the newest version is editable until a committed lock records its digest, a test holds every locked version to it, and a build target cuts the next version"
type: decision
---

# Decision 0009: a version's folder is its number, one number covers the set, and a version lock freezes it

**Date:** 2026-10-11
**Decided by:** the repository owner (2026-10-11), three decisions in a planning session; recorded by spec-author

## The call

The shipped files sat in integer folders - `src/tool/schema/v1/`,
`src/tool/templates/v1/`, `docs/rules/v1/` - under a version that decision 0005
made `major.minor.patch`. OQ-22 asked what the folder and the `$id` segment
become, and nothing said how a published version is kept from changing (C-1,
B-013). The owner chose among options on each of three points.

1. **The folder is the version, `v<major.minor.patch>`.** OQ-22, reading (a).
   The folders become `schema/v0.1.0/`, `templates/v0.1.0/` and
   `docs/rules/v0.1.0/`, the embedded resource names likewise, and the `$id`
   segment is the same:
   `https://github.com/rlittlesii/specht/schema/v0.1.0/frontmatter/<file>`.
2. **One number for the whole set, stated in the files.** Templates and rule
   pages get no version number of their own; C-10 and `0001-F3` C-6 stand. A
   rule page's `Schema version` row becomes the `major.minor.patch` - `0.1.0`,
   not `1`. A change to a template alone, or to a rule page's text alone, is a
   patch, since it changes no verdict (C-11), and is still a new version, since
   bytes changed (B-013).
3. **A working folder and a hash lock.** The newest version's folder is the
   working version, editable until it is published. Publishing records the
   version in one committed lock file. A test fails if a locked version's bytes
   change, or if more than one version is unlocked. A build target copies the
   newest version to the next number.

The `$id` past the version segment, `/frontmatter/` and the file's name, and
the live copy's paths - `.specht/manifest.json`, `.specht/schema/frontmatter/`
and `.specht/templates/` - follow `0001-F5` decision 0011, made the same day;
this decision gives the segment that record left to OQ-22. Where one version's
manifest sits in the shipping copy is decided by neither (OQ-52), so "three
trees" below is the schemas, the templates and the rule pages as they sit
today, and the claims are written on every tree a version's files sit in.
Added 2026-10-12 by spec-author, when that decision reached `main`.

The design direction under decision 3 is the plan the owner approved the same
day. It was approved as a plan and not dictated word for word:

- The lock holds one entry for each published version: the version, and one
  SHA-256 over that version's files in all three trees, in ordinal path order,
  each file's path and then its bytes. One digest for a version and no list of
  its files, so the lock is not the index of rule pages `0001-F3` § 5 row 10
  and `0001-F10` C-2 keep out. Suggested home:
  `src/tool/schema/versions.lock.json`, outside the folders a version embeds.
- A lock build target writes the newest version's digest. The release's tag
  check refuses a tag while a shipped version is unlocked, so nothing is
  published unfrozen.
- A cut build target takes the new version's number, refuses unless the newest
  version is locked and the new number is higher than every shipped one, copies
  the newest folder in all three trees, and rewrites the `$id` segment and each
  rule page's `Schema version` row to the new number.
- Each version's folders are a whole snapshot: no fallback to an older folder
  and no deltas.
- A template carries no version inside it. Put to the owner in the plan as the
  planner's recommendation.

Read by spec-author, not stated by the owner:

- **The cut and the lock are claimed in `0001-F7`, not in a `.build/` Feature.**
  This Feature already owns what a schema version is, how it is numbered (C-11)
  and that a published one never changes (C-1, B-013). The lock is the
  mechanism of that claim and the cut is how its next subject comes to exist.
  The claims are written on the version set, not on the build. A Feature under
  `.build/` would split one invariant across two agreements that must agree,
  and would put it under `0055-F1` § 8, where the build has no tests, although
  decision 3 asks for a test. Only the tag check is another Feature's: it is a
  condition of publishing, so it is `0055-F6` B-013. Where the targets' code
  sits is the `implementer`'s (OQ-52).
- B-013 gains rule pages: decision 2 makes a changed page a new version.
- B-088 compares a version with the next higher one the tool ships. The owner
  said such a change "is a patch"; a claim needs two versions to compare.
- Locking again never rewrites a recorded digest (C-17, B-093). Otherwise a
  changed locked version is made to pass by running the lock.
- The cut creates nothing but the new version's folders (B-100): it writes no
  lock entry, since the cut version is the working version, and it does not
  move this repository's live copy (OQ-56).
- A value that is not `major.minor.patch` is refused by the cut as a number
  that is not higher (B-095).

A limit of the lock, stated by spec-author: a change that edits a locked
version's file and its lock entry together passes the test. The lock makes that
change one visible line in a diff; it does not make it impossible.

Not decided, and raised: the targets' names, the lock's path and format, and
where the logic sits so that a test reaches it and where one version's manifest sits in the shipping copy (OQ-52);
renumbering or
abandoning the working version (OQ-53); a version locked and never published
(OQ-54); the lock entry of a `0.x.y` a later tool stops shipping (OQ-55); this
repository's live copy after a cut (OQ-56); and, in `0001-F5`, which default
manifest fills what a manifest pinned to an older version leaves out
(`0001-F5` OQ-23).

## Why

The owner chose among options and no reason of the owner's is on record. The
reasons below were put with the options chosen; they are the author's words.

- **Decision 1.** A folder named `v1` under version `0.1.0` needs a mapping
  from one to the other, kept in code and extended by hand for each version.
  With the folder named for the version, the folder, the `$id` and the pin say
  the same thing, and the mapping goes.
- **Decision 2.** The gap was that the files did not say their number: a rule
  page said `1`. A consumer pins one number (`schemaVersion`), so one number
  has to name everything that pin selects.
- **Decision 3.** Item `0044`, "Prove a shipped version never changes", had no
  mechanism: nothing recorded what a version's bytes were when it shipped, and
  no `v*` tag exists to compare against. A digest committed at the freeze is
  that record, and a test over it runs offline on every build.

## Rejected

Each cost below is the author's assessment.

**(b) The bare version, `0.1.0`, with no `v`.** OQ-22's second reading; put to
the owner and not chosen. Cost of rejecting: the folder's name and the `$id`
segment are not the `schemaVersion` value as written, so whatever reads a
version from a folder takes the `v` off first.

**(c) Keep the integer folder, and add a version metadata file in each
folder.** Put to the owner and not chosen. Cost of rejecting: every folder is
renamed once, the `$id` of every embedded and live frontmatter schema changes,
and the rename must land before the first tag, after which those bytes are a
published version's (OQ-30).

**Independent `major.minor.patch` numbers for the schemas, the templates and
the rule pages.** Put to the owner and not chosen: it would need three pins, or
a table saying which numbers go together. Cost of rejecting: a typo fixed in
one template is a new version of the whole set, and the tool carries a whole
copy of every file for it.

**The same cut target with no lock, the freeze held by review alone.** Put to
the owner and not chosen. Cost of rejecting: one more committed file and one
more build target, and a release that fails until the lock target has run.

**Leaving the cut and the freeze undecided.** Put to the owner and not chosen.
Cost of rejecting: none beyond the work.

**A version stated inside each template.** The planner's recommendation against
it, shown to the owner in the approved plan. A template is copied into a
consumer's tree and then into every specification written from it, so a stamp
would travel into the consumer's documents and go stale there. Cost of
rejecting: a template on a consumer's disk does not say which version it is;
the folder it shipped in does, and `specht upgrade` compares content, not a
stamp.

**A delta between version folders, or a fallback to an older folder.** Not
taken in the approved plan. Cost of rejecting: the one decision 0007 accepted -
a tool that ships several versions carries each one's files whole.

## Affects

- `0001-F7` § 2 need 9 and A-4 (added); § 3 B-012 and B-013 (amended), B-086 to
  B-100 (added); § 4 C-6 (amended), C-16 and C-17 (added); § 5 rows 22 to 26
  (added); § 11 OQ-22 (resolved), OQ-21 and OQ-30 (annotated), OQ-52 to OQ-56
  (raised).
- `0055-F6` § 3 B-013 (added); § 11 OQ-19 (raised).
- `0001-F4` § 2 A-2 and § 3 B-005 (amended: the folder and the segment are no
  longer provisional).
- `0001-F3` § 2 A-4 and § 3 B-033 (amended); its decision 0003, whose "Not
  decided" note is answered in its Reversal.
- `0001-F5` § 11 OQ-23 (raised). `0001-F5` decision 0011 is not amended: its
  line that the segment's form stays OQ-22 is answered here.
- brief § 6, the row on schema `$id` URLs.
- Items `0126` (no longer blocked), `0044` (given the lock), `0188`, `0189`,
  `0190` and `0191` (cut), `0041`, `0082` and `0010` (their children).
- Not amended, and still describing the integer folder as built: `0001-F7` § 6
  and § 7, `0001-F3` § 7 and § 8, `0001-F4` § 7 and § 8, `0001-F5` § 7, the
  layout lines in AGENTS.md and the skills, and `docs/`. They change with the
  rename.
