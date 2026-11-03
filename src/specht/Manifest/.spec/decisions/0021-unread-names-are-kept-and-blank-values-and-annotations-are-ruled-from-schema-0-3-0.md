---
title: "Decision 0021: unread names are kept, and blank values and annotations are ruled from schema 0.3.0"
description: "The owner's eight rulings of November 3 2026 on item 0347: an unread name inside roles or markers is kept and never read; a white-space marker and an empty or dot schema file name are rejected; a dollar-prefixed path identity member is ignored; a by-record matrix value and a case-insensitive rule level are later, unscheduled items 0348 and 0349, the latter from the first schema version after 0.3.0; the other schema pins are spec-author's reading"
type: decision
---

# Decision 0021: unread names are kept, and blank values and annotations are ruled from schema 0.3.0

**Date:** 2026-11-03
**Decided by:** the repository owner, answering six questions and then two more, each on its own; calls 7 and 8 are spec-author's reading, not the owner's; recorded by spec-author

## The calls

1. **OQ-10, "A kept, unread (Recommended)".** A name inside `roles` or
   `markers` that the engine does not read, such as `roles.constraints`, is
   kept and never read. B-072 states it. It is not rejected as B-012 rejects
   an unknown top-level key.
2. **OQ-11, "A rejected (Recommended)".** A marker text of white space alone
   is rejected, as B-040 rejects the empty text. The rejection names the
   marker (B-073).
3. **OQ-14, "A accept $-keys (Recommended)".** A member whose name begins
   with `$` inside a layout's path identity is ignored, so
   `{ "epic": 1, "$comment": "x" }` is accepted (B-074). B-043 no longer
   rejects such a member from schema `0.3.0`. That reverses today's
   rejection, which `0001-F6` item `0006` built.
4. **OQ-22, "A rejected (Recommended)".** A `frontmatterSchemas` value that is
   the empty text or `.` is rejected as an invalid manifest. The rejection
   names the kind and the value (B-075). B-047 rejects the other names that
   are not plain file names.
5. **OQ-12, "A later item (Recommended)".** A by-record value in § 9, which
   the not-covered rule accepts, is its own item, `0348`, outside milestone
   `0.4.0`. It has no claim yet. Call 10 leaves it under no milestone.
6. **OQ-15, "case-insensitive. this isn't a breaking change for 0.4.0 so we
   can add an item to do this later."** (a) A rule level that differs from
   `error`, `warning` or `off` only in letter case, such as `Warning`, will be
   read as that level. Item `0349` builds it, outside milestone `0.4.0`;
   call 10 leaves it under no milestone. Until then B-045 rejects it. (b) The proposed default stands: when a value
   is not text, the rejection names it as the manifest's JSON writes it. The
   owner chose the option that carries (b), and the note spoke only to (a).
7. **Pins, spec-author's reading.** B-073, B-074 and B-075 hold under a
   manifest pinned to schema `0.3.0` or later. `0.1.0`, `0.2.0` and `0.2.1`
   keep today's behaviour (B-049): a white-space marker is accepted, a
   `$`-prefixed member is rejected under B-043, and an empty or `.` schema
   file name is not rejected when the manifest loads. A-6 lists the three.
   B-072 holds at every pin.
8. **Item `0349`'s pin is not decided.** It is raised as OQ-28. Call 9
   resolves it.
9. **OQ-28, "A next 0.minor (Recommended)".** The case-insensitive rule level
   holds from the first schema version after `0.3.0`. `0.3.0` and every
   older pin keep rejecting `Warning` (B-045). Item `0349` builds it later
   and writes its claim.
10. **Items `0348` and `0349`, "Leave unscheduled (Recommended)".** Neither
    sits under a milestone in `.issue/.goal`. Only item `0347` of this
    decision's items is in `0.4.0`.

## Why

- Calls 1 to 6, 9 and 10 are the owner's words.
- Call 7 follows decision 0016's precedent. Schema `0.3.0` is in no
  published package, so it may change in place (decision 0016;
  [ADR-0030](../../../../../.spec/adr/0030-schema-0-3-0-first-carries-the-gap-width-and-the-malformed-edge-text.md)).
  `0.1.0`, `0.2.0` and `0.2.1` are published. A published version never
  changes in place (`0001-F7` C-1), and the pin selects the manifest
  interpretation (`0001-F7` C-4). Each of B-073 to B-075 changes the verdict
  of a manifest that a published pin accepts or rejects today.
- B-072 is what the parser does today at every pin: it validates only the
  names the pinned default manifest gives `roles` and `markers`. It changes
  no verdict, so it holds at every pin.
- No shipped file of `0.3.0` holds a white-space marker, a `$`-prefixed
  identity member or an empty or `.` schema file name. The three calls change
  how a manifest is read, not a shipped file, so `0.3.0`'s lock entry is
  unchanged.
- Call 8: `0.3.0` will be published with milestone `0.4.0`, before item
  `0349` lands. Under `0001-F7` C-1 and C-4, a published pin keeps rejecting
  `Warning`. Accepting it changes that manifest's verdict, which `0001-F7`
  C-11 keeps out of a patch. The owner had not said which version first
  reads the level ignoring case; call 9 says so.

## Rejected

- **OQ-10:** rejecting an unread name inside `roles` or `markers`, as B-012
  rejects an unknown top-level key.
- **OQ-11:** accepting a marker text of white space alone.
- **OQ-14:** rejecting a `$`-prefixed member with every other member that is
  neither the epic nor the Feature segment (B-043 as built).
- **OQ-22:** accepting the empty text or `.` as a schema file name.
- **OQ-12:** building the by-record value in milestone `0.4.0`. The ruling
  as put to spec-author names no other option.
- **OQ-15:** rejecting a level that differs only in letter case.
- **OQ-28:** reading the level ignoring case at `0.3.0` or an older pin.
- **Call 10:** scheduling `0348` and `0349` under milestone `0.5.0`, as
  they first were.
- **Call 7, spec-author's:** the three calls at every pin, which `0001-F7`
  C-1 and C-4 rule out; a new schema version beside `0.3.0`, which spends a
  version no consumer can pin (decision 0016, option C).

## Affects

- `0001-F5` A-6, B-022, B-023 and B-043 amended; B-072 to B-075 added.
- `0001-F5` § 5 row 16 amended; rows 21 to 23 added; B-074's provenance
  notes an identity of `$` members alone.
- `0001-F5` OQ-10, OQ-11, OQ-12, OQ-14, OQ-15 and OQ-22 resolved; OQ-28
  raised and resolved.
- Items `0347`, `0348` and `0349`; Feature item `0010`; `.issue/.goal`.

## Reversal

Call 7 holds only while no published package carries schema `0.3.0`. If
one does before item `0347` merges, B-073 to B-075 need a new schema version,
and a new decision reverses call 7.
