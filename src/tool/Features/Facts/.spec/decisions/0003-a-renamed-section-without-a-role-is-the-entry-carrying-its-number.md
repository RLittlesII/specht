---
title: "Decision 0003: a renamed section without a role is the entry carrying its number"
description: "The owner's answer to OQ-7: when a manifest renames the constraints, out-of-scope, lessons-and-deltas or open-questions heading, the facts take the sections entry whose leading number is 4, 5, 10 or 11, as 0001-F5 decision 0019 reads a title's number"
type: decision
---

# Decision 0003: a renamed section without a role is the entry carrying its number

**Date:** 2026-11-14
**Decided by:** the repository owner, 2026-11-14, answering OQ-7 in the
words "(a) match leading number for OQ-7", relayed by the coordinating
session. Option (a)'s wording is `implementer`'s, raised in item `0367`;
recorded by spec-author

## The call

1. When a manifest's `sections` renames the constraints, out-of-scope,
   lessons-and-deltas or open-questions heading, the facts find that
   section under the `sections` entry whose leading number is 4, 5, 10 or
   11 respectively. The leading number is the run of digits that opens the
   title, as `0001-F5` decision 0019 reads it.
2. As option (a) states, a heading with no leading number, or with another
   number, is not that section.

spec-author's reading, not the owner's words: with no entry carrying the
number, that member is `[]` and every other member is given, as B-015 and
§ 7 already state for a section `sections` does not list. Two entries
carrying one number, and a number carried by a section a `roles` entry
names, are OQ-8.

## Why

- Every default title leads with its number, so under every shipped
  manifest the facts read the same sections as before.
- B-016's `4. Limits` is found with no new manifest key, which decision
  0002 call 1 keeps out.
- The engine already reads a title's leading number for a message's
  `§ N` (`0001-F5` decision 0019), so a consumer meets one reading.

## Rejected

- **(b) The entry at the place the pinned shipped `sections` holds the
  default title.** Cost: a consumer who adds, drops or reorders a section
  gets another section read with no sign, and the parsed manifest must
  keep the shipped list beside the effective one.
- **(c) A `roles` entry for each of the four.** Cost: a `0001-F5` delta and
  a schema version, which decision 0002 rejected.
- **(d) The default title only, as built.** Cost: B-016 and its
  renamed-constraints row narrow to `roles` and `tables`.

## Affects

B-016; C-7; OQ-7, OQ-8; § 5 row 11. § 7's paragraph on the names the
manifest gives, which `implementer` brings true in item `0367`, and § 9's
B-016 row, which `test-writer` brings true there.

## Reversal

A later `0001-F5` delta that gives the four sections `roles` entries
supersedes this reading (decision 0002 § Reversal); the document's shape
does not change (C-3, C-5).
