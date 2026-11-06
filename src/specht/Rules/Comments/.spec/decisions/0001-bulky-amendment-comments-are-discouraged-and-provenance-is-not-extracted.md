---
title: "Decision 0001: Bulky amendment comments are discouraged, and provenance is not extracted"
description: "The owner's revision of 2026-10-27 to item 0289: the check discourages bulky amendment comments in specification Markdown, opt-in and off by default, and no longer extracts a date, author, authorization or affected keys from a comment; required stamps and generated markers stay valid"
type: decision
---

# Decision 0001: Bulky amendment comments are discouraged, and provenance is not extracted

**Date:** 2026-10-27
**Decided by:** the repository owner, revising item `0289`; recorded by
spec-author on 2026-11-06

## The call

As item `0289` records it:

> 2026-10-27, repository owner: revise the request to discourage bulky
> amendment comments; mandatory provenance extraction is superseded. Keep
> required stamps and generated markers.

The item's summary, rewritten with the revision in commit `4c993688`, adds: "Default off", and
"Do not require comments to carry dates, authors, user authorization or long
lists of affected keys. Necessary approval evidence stays in its owned
record; compact section 10 rows cite the affected keys."

## Why

The owner's reason, from the item's summary: concise current content,
"without decorative banners or long narratives announcing amendments and
listing benefits".

The author's reading, not stated by the owner:

- A comment that narrates an amendment repeats what the commit and § 10
  already hold, so the remedy is to report its bulk, not to read it.
- Judging size and not wording keeps the verdict deterministic and leaves
  out the prose word-count gate the item rules out (C-1).

## Rejected

**Extracting a date, an author, an authorization and the affected keys from
a large comment**, the intake of 2026-10-24. Superseded by the owner: it
would require every comment to carry provenance that its owned record
already carries.

## Affects

- `0101-F10` B-001, B-002, B-004, B-005, B-007, B-012; C-1, C-2, C-5.
- § 5 row 6.

## Reversal

None.
