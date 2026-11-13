---
title: "Decision 0001: native claims fold into schema 0.4.0"
description: "The owner's selection of November 12 2026: schema 0.3.1 has gone out in NuGet packages, so this Feature's claim members and its three rules leave it and enter schema 0.4.0, which item 0436 cuts; the basis under 0001-F7 C-11 and the consequences for the items are spec-author's reading"
type: decision
---

# Decision 0001: native claims fold into schema 0.4.0

**Date:** 2026-11-12
**Decided by:** the repository owner, selecting an option relayed through the coordinating session; the facts, the basis and the consequences below are spec-author's reading; recorded by spec-author

## The call

Two options were put to the owner on 2026-11-12, "0.3.2 patch" and "Fold into
0.4.0". The owner selected **"Fold into 0.4.0"**: this Feature's `claim`
members and its three rules enter schema `0.4.0`, and item `0389` waits on
item `0436`, which cuts it.

Spec-author's reading, not the owner's words:

1. **The fact.** Schema `0.3.1` has gone out in a NuGet package. nuget.org
   lists `specht.tool` `0.5.35-preview`, `0.6.1-preview` and `0.6.4-preview`,
   checked 2026-11-12. Tags `v0.5.35-preview` (`cb12f05b`, 2026-11-08),
   `v0.6.1-preview` and `v0.6.4-preview` each hold
   `src/tool/manifest/v0.3.1/manifest.json`, and the version lock's `0.3.1`
   digest at `v0.5.35-preview` equals `main`'s. `0001-F7` C-1 and C-17 forbid
   changing or relocking it, so OQ-5's "with a relock" into `0.3.1` is no
   longer possible. Under the owner's rule in
   [`0001-F5` decision 0023](../../../../Manifest/.spec/decisions/0023-claim-prefixes-stay-in-schema-0-3-1-which-no-package-carries.md),
   a published version takes a breaking change in `0.4.0` and a non-breaking
   one in `0.3.2`; the two options were those branches.
2. **The basis.** `0.4.0` is a `0.minor` step (`0001-F7` OQ-78, item `0436`).
   Before `1.0.0` a `0.minor` step may fail a tree that passed, a new rule id
   included (`0001-F7` C-11), so the members and rules enter `0.4.0` under
   that clause, not under C-11's opt-in-addition clause or `0001-F7` decision
   0022's patch. Each verdict still reports only when `claim.source` is
   `gherkin` (C-3). `0001-F7` B-066 holds because the minor is greater.
   `0001-F7` B-154 compares patch neighbours only, and `0.3.1` and `0.4.0` are
   not patch neighbours (`0001-F7` OQ-78), so no item of this Feature carries
   a B-154 criterion.
3. **The in-place change.** Each item that adds a member or a rule id changes
   `0.4.0` after item `0436` has cut it, with a relock. `0001-F7` C-17 allows
   that only while no published package carries `0.4.0`, and C-11 does not
   count such a change as a step between versions. A package published with
   `0.4.0` before an item lands puts that item under the owner's rule again.
4. **The claim grammar.** From `0.4.0` the claim grammar is read from the
   `ids.claim` record and `identifiers.claim` is rejected (`0001-F5` C-14,
   B-092), so A-1 and the `.feature` Background name the record.

## Why

The owner's reason for the selection is not in this record. Spec-author's
reading of what it gives: the members and rules ride a version that is already
a `0.minor` step, so no opt-in mark and no patch-neighbour comparison is
needed to admit them.

## Rejected

"0.3.2 patch", put to the owner and not selected. Spec-author's reading of its
cost: a `0.3.2` cut for this Feature alone, each rule page carrying the opt-in
mark of `0001-F7` B-066, and `0001-F7` B-154 comparing `0.3.1` and `0.3.2` with
`0001-F7` OQ-75 still unanswered for items `0393` to `0395`.

## Affects

A-1, A-2, A-4; B-001, B-028; C-3; OQ-5 (note); § 10; Tasks; the `.feature`
Background and the B-028 scenarios; items `0389` to `0396`; `0001-F7` C-1,
C-11, C-17, B-066, B-154, OQ-78; `0001-F5` decision 0023, C-14; item `0436`.

## Reversal

None.
