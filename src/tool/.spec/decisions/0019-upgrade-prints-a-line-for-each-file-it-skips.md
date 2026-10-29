---
title: "Decision 0019: upgrade prints a line for each file it skips"
description: "The owner's rulings of 2026-10-29 on what upgrade prints for a frontmatter schema or a template it does not write, which source it reads, and what a template name the pinned version does not ship does"
type: decision
---

# Decision 0019: upgrade prints a line for each file it skips

**Date:** 2026-10-29
**Decided by:** the repository owner, choosing the recommended default of each of five questions for items `0094` and `0131`, answering OQ-27 and the part of OQ-1 still open for `upgrade`; recorded by `spec-author`

## The call

1. **The skip line.** After the lines naming the files it rewrote, `upgrade`
   prints one line `skipped <path>: <reason>` for each file it skips, in the
   form `init` prints a file it skips. A run that rewrites nothing and skips
   at least one file is still current: it prints the line saying so, then the
   skip lines, and exits `0`.
2. **No `--schema-source` on `upgrade`.** Only the manifest's `schemaSource`
   selects the source `upgrade` reads (OQ-1, for `upgrade`).
3. **Which schemas are skipped.** Skip lines for frontmatter schemas are
   printed only when the manifest records no `spechtSchema`; a recorded
   source's fetch is unchanged. Each frontmatter schema the pinned version
   ships that the root holds is listed, by its shipped name.
4. **A name the pinned version does not ship.** A `consumerTemplates` name
   the pinned version does not ship is accepted and has no effect: no skip
   line and no warning (OQ-27, reading (b)).
5. **Which templates are skipped.** Every template the manifest names as the
   consumer's that the pinned version ships gets a skip line, without being
   read, so an absent one and one identical to the pinned version's are
   listed too. A name matches a template ordinally on its bare file name, so
   `Feature.md` does not name `feature.md`. The matching is a design choice
   for § 7, not a claim.

## Why

The reasons given with the recommendations the owner chose. The line is
`init`'s form, so one reader reads both commands. B-009 names the option on
the check alone, and § 7 already reads that way. A recorded source's fetch
(item `0093`) is unchanged by a skip. The parser item `0130` built already
accepts a name the pinned version does not ship, and B-063 holds either way.
§ 7 already says a named template is not read.

## Rejected

**`--schema-source` on `upgrade`.** Cost of rejecting: trying the on-disk
source for one run means editing the manifest.

**Skip lines under a recorded source.** Cost of rejecting: the output does not
say that a recorded source's schemas were left to it.

**Rejecting a name the pinned version does not ship as an invalid manifest
(OQ-27 (a)).** Cost of rejecting: a misspelt name protects nothing, and
nothing says so.

**Matching a name without regard to case.** Cost of rejecting: on a
case-insensitive file system, a name that differs from a template's only in
case does not protect it.

## Affects

- B-030 amended: the line, its place, and the schemas listed, with no record.
- B-061 amended: the line, its place, and every named template the pinned
  version ships.
- B-142 added: a run that only skips is current.
- B-143 added: no skip line for a frontmatter schema under a recorded source.
- B-144 added: a name the pinned version does not ship changes nothing.
- B-063's source: "whatever the manifest names" is the owner's.
- § 5 rows 31 and 32.
- OQ-1 resolved for `upgrade`; OQ-27 resolved.
- Items `0094` and `0131`.
- `0001-F5` § 5 row 3.

## Reversal

None.
