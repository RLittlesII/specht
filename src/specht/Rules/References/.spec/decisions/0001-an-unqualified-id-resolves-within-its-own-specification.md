---
title: "Decision 0001: an unqualified id resolves within its own specification"
description: "Each claim, constraint or open-question id a specification cites without a Feature key resolves to a row of that specification or is a warning; qualified ids, prose-state claims and history are out of reach, and the rule is the within-one-specification half of epic 0228's candidate C"
type: decision
---

# Decision 0001: an unqualified id resolves within its own specification

**Date:** 2026-10-28
**Decided by:** the repository owner (2026-10-28): calls 1 and 2, the need
recorded in `0228-F1` decision 0001. Call 3 is the owner's of 2026-10-15
(epic `0228` R5). Calls 4 and 5 are spec-author's reading, for the owner to
confirm in review.

## The call

1. Every claim, constraint and open-question id a specification cites in its
   own text resolves to a row declared in that same specification, or it is
   reported (B-001 to B-003).
2. Declined: holding a claim made in prose, such as "OQ-3 is Open", against
   the row's Resolution; and anything that needs git history.
3. The rule is a warning by default (B-020).
4. Where a citation is read: the body's prose, lists, table cells and HTML
   comments; not inline code, fenced code, frontmatter, the companion
   `.feature` file or the records beside the specification (B-004 to B-006,
   B-015, B-016).
5. An id that follows a Feature key in the same clause is not resolved here
   (B-011, B-012; OQ-3, OQ-4). This Feature is the within-one-specification
   half of epic `0228` candidate C; the qualified half and the command stay
   with candidate C and item `0307`, since a rule that waits for a schema
   version and a command that does not are the condition the epic names for
   cutting them apart.

## Why

- A consumer's ten review rounds came in large part from stale
  cross-references the check did not read.
- Measured on this tree, 2026-10-28, with the reading of calls 4 and 5:
  8078 unqualified citations, 29 dangling. HTML comments hold 234 of the
  citations and none of the dangling ones. Examples in claims are written in
  inline code, such as `0101-F2` B-001's, and `.feature` steps name ids that
  do not exist by design, so reading either would report fixtures.

## Rejected

- **Resolving qualified citations here.** A second resolver before item
  `0307`'s agreement; epic `0228` candidate C's invariant is one resolver.
- **Error by default.** Epic `0228` R5 decided warning.
- **Reading the companion's steps.** They describe example trees (call 4).
- **Counting § 4 rows where the ids are bare ordinals.** A row inserted above
  shifts every citation silently (OQ-6).

## Affects

- B-001 to B-021, C-1 to C-6, § 5 rows 1 to 16, OQ-1 to OQ-6.
- `0228-F1` C-3: the Blocks rule reads citations through this Feature.
- Epic `0228`: `children` gains `0228-F2`.
- `0001-F5` and `0001-F7`: `blocks` gains `0228/F2`.

## Reversal

None.
