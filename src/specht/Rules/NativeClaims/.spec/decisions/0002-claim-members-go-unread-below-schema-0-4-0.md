---
title: "Decision 0002: claim members go unread below schema 0.4.0"
description: "The owner's selection of November 13 2026, Ignore below 0.4.0: under a pin below schema 0.4.0 the claim members this Feature adds go unread, as they do today, and change no verdict, so B-028 and A-4, which said such a pin refuses them with exit 3, are amended; the fact, the basis and the consequences are spec-author's reading"
type: decision
---

# Decision 0002: claim members go unread below schema 0.4.0

**Date:** 2026-11-13
**Decided by:** the repository owner, selecting an option relayed through the coordinating session; the fact, the basis and the consequences below are spec-author's reading; recorded by spec-author

## The call

The owner selected **"Ignore below 0.4.0"**: under a manifest pinned below
schema `0.4.0`, a `claim` member this Feature adds goes unread, as it does
today, and changes no verdict.

Spec-author's reading, not the owner's words:

1. **The fact.** B-028, A-4, B-028's second scenario and item `0389`'s B-028
   criterion said a pin below `0.4.0` refuses a new `claim` member with exit
   `3` "as today". No pin does today: `SpechtManifestParser.Parse` reads
   `claim.tag` and `claim.tagIgnoreCase` and leaves every other `claim` member
   unread at every pin, so a manifest pinned to `0.3.1` that sets
   `claim.source` to `gherkin` checks as it does without the member.
2. **The basis.** Refusing the member below `0.4.0` would change the verdict
   published `0.3.1`, and each earlier published version, gives that
   manifest, which `0001-F7` C-1 rules out; under `0001-F7` C-4 the pin
   selects how the manifest is read, so an earlier pin keeps its reading.
   [`0001-F5` decision 0021](../../../../Manifest/.spec/decisions/0021-unread-names-are-kept-and-blank-values-and-annotations-are-ruled-from-schema-0-3-0.md)
   call 7 is the precedent: a manifest rule a later version adds leaves the
   earlier pins to today's behaviour.
3. **The consequence.** B-028 and A-4 are amended in place, keeping their
   ids. B-028's second scenario becomes "An earlier pin ignores claim.source":
   a tree that passes without the member passes with it, with the same
   output. B-029's faults are read only where the members are read, from
   `0.4.0`, which B-001's pin and the `.feature` Background give; B-029 is not
   amended.

## Why

The owner's reason for the selection is not in this record. Spec-author's
reading of what it gives: no published pin changes its verdict on any
manifest, and the gate sits where the members are read, at `0.4.0`.

## Rejected

Refusing a new `claim` member below `0.4.0` with exit `3`, the reading B-028
and A-4 held before this amendment. Spec-author's reading of its cost: a
manifest `0.3.1` accepts today would fail under a later tool still pinned to
`0.3.1`, which `0001-F7` C-1 forbids, and the claim rested on a refusal no pin
makes.

## Affects

A-4; B-028; the second B-028 scenario in `native-claims.feature`; § 10; item
`0389`'s summary, B-028 criterion and decisions; `0001-F7` C-1, C-4;
`0001-F5` decision 0021.

## Reversal

None.
