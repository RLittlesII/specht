---
title: "Decision 0002: ids are ordered by number, and retired rows keep their slot"
description: "The claims, constraints and open questions are held in ascending numeric id order, § 9 follows § 3, a withdrawn or superseded row stays where its number puts it, and the order of ids in prose is not checked"
type: decision
---

# Decision 0002: ids are ordered by number, and retired rows keep their slot

**Date:** 2026-10-08
**Decided by:** the repository owner, in the epic `0101` requirements session (2026-10-08); recorded by spec-author

## The call

The linting scope is: table row order, § 9 rows following § 3's claim order,
retired ids marked, exact heading text, pipe-table form, and frontmatter key
order. The order of ids inside prose - bullets and sentences - is not
checked.

The order of a table of ids is ascending by number. A withdrawn or
superseded row stays in its numeric slot; it is not moved to the end.

## Why

- Ascending numeric order is the order ids are cited in, so it is the order
  a reader looks for. It needs no data beyond the id.
- A retired row in its slot keeps the table a complete, gapless index of the
  ids ever issued; moving it would make the order depend on a status that
  changes, so the same row would move twice in its life.
- Prose carries ids in the order the argument needs. Checking it would flag
  writing, not structure.

## Rejected

**Retired rows grouped at the end.** The order then depends on status, and a
withdrawal becomes a row move in every diff. Cost of rejecting: active
claims are interleaved with retired ones. Taken.

**Ordering by text.** `C-10` sorts before `C-2`. Cost of rejecting: none.

**Checking the order of ids in prose.** Flags an author's sentence as a
finding. Cost of rejecting: a bulleted list of ids may be out of order.
Taken.

## Affects

- `0101-F1` B-001 to B-009, B-013; C-1, C-2; § 5 rows 1 and 2.
- `0101-F2` (retired ids marked), `0101-F3` (heading and table form),
  `0101-F4` (frontmatter key order): the rest of the scope.
- `0101-F5`: the order `format` applies.

## Reversal

None.
