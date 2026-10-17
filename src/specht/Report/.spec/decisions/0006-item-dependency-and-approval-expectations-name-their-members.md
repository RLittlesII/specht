---
title: "Decision 0006: Item dependency and approval expectations name their members"
description: "The structured repair members for child items dependency edges cycles and approval rows"
type: decision
---

# Decision 0006: Item dependency and approval expectations name their members

**Date:** 2026-10-17
**Decided by:** the repository owner, answering item 0039's contract question; recorded by spec-author.

## The call

Use the proposed member names for B-016 to B-018, B-027 and B-028:

| Rules            | Members                                                                               |
| ---------------- | ------------------------------------------------------------------------------------- |
| SPEC040, SPEC041 | `fileName`; `type` for spikes                                                         |
| SPEC044          | `fileName`; `nextNumber` for a sequence violation; `firstDeclaration` for a reused id |
| SPEC050          | `features`                                                                            |
| SPEC051          | `edge`, with `feature`, `key`, `value`                                                |
| SPEC052          | `cycle`                                                                               |
| SPEC060          | `section`, `row`, `columns`                                                           |
| SPEC061          | `section`, `row`                                                                      |

Rows are one-based table body rows and columns are one-based cell positions, as proposed to the owner. Section titles and markers come from the effective manifest. Section 7 records the implementation reading: integer sequence numbers; ordinal qualified targets; existing first-holder order and cycle path; all missing cells in the one existing row diagnostic. These choices supply the stated repair information without changing which violations occur.

The report schema keeps `expected` open. No schema resource or rule page changes. Schema 0.1.0 and 0.2.0 retain their existing approval triggers; the separately specified 0.3.0 policy is outside this item.

## Rejected

- Leaving member names implicit: every future reader would need to infer an enduring public contract. The explicit names follow decisions 0004 and 0005.
- Repeating the diagnostic as a sentence: C-2 requires structured data a consumer can read without parsing prose.
- One diagnostic per missing cell or a newly trimmed cycle: either changes the baseline count or reported path; this item only adds report metadata.
- New sign-off header interpretation: Manifest OQ-24 and item 0227 own that separate work.

## Affects

- `0001-F3` B-016, B-017, B-018, B-027, B-028 and OQ-9.
- Item 0039, its scenarios, technical design and evidence.
