---
title: "Decision 0010: Binding element order"
description: "Order the binding elements a repair writes, and the target choices it offers, by epic, feature and claim or ADR id in natural order"
type: decision
---

# Decision 0010: Binding element order

**Date:** 2026-10-30
**Decided by:** Repository owner

## The call

When an invoked repair writes a `verifies`, `implements` or `follows` element
into a documentation comment, order that comment's elements of the same role
by a sort key read from each element's own attributes:

1. `epic`, then
2. `feature`, then
3. `claim` for `verifies` and `implements`, or `adr` for `follows`.

An absent component sorts before any present one. Components compare in
natural order: each value splits into digit runs and other runs; digit runs
compare by numeric value and other runs ordinally. So `F2` precedes `F10` and
`0001` precedes `0201`. Elements with equal keys keep their existing relative
order.

```csharp
/// <verifies epic="0001" feature="F2" claim="B-003" />
/// <verifies epic="0001" feature="F10" claim="B-001" />
/// <verifies epic="0201" feature="F2" claim="B-001" />
```

The order applies only to documentation comments a repair writes a binding
element into, whether through a single invocation or Fix All. It is a
permutation of the whole-line elements of one role within the line slots those
elements already occupy; elements of different roles do not exchange slots.
An element sharing its line with other content keeps its place and does not
take part. Summary, remarks, custom XML and all other text are untouched. A
duplicate removal writes no element and reorders nothing.

The target choices a repair offers ("Bind verifies to ..." and the like) are
ordered by the same key, with the record's relative path and then its line as
the tiebreak.

## Why

Repairs append or insert elements wherever their edit lands, so a comment can
carry `epic="002"` above `epic="001"`. A fixed identity order makes a comment
read the same however its elements arrived and keeps review diffs small. A
choice list in the same order lets the maintainer find a target by its
identity rather than by where its specification sits on disk.

## Rejected

- A new `SPCT` diagnostic for unordered binding elements: needs a new published
  ID under decision 0009, and no finding was requested.
- Ordering comments no repair writes into: rewrites documentation the
  maintainer did not ask to change, against the preservation guarantee of
  B-023.
- Ordering choices by specification path: groups targets by folder, not by
  identity, and splits one epic's records across the list.
- Plain ordinal comparison: puts `F10` before `F2`.

The accepted cost is that an untouched comment stays in whatever order it was
written until a repair writes into it.

## Affects

0201-F2 B-039, B-040 and § 5 row 4.

## Reversal

None.
