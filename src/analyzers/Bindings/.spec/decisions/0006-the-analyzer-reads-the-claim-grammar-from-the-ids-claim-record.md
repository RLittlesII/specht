---
title: "Decision 0006: The analyzer reads the claim grammar from the ids claim record"
description: "The owner selected option A, built in item 0436: the analyzer and specht evidence read the claim grammar from ids.claim, so this repository moves to schema 0.4.0 with the CodeAnalysis target green; how they select and validate the record is spec-author's"
type: decision
---

# Decision 0006: The analyzer reads the claim grammar from the ids claim record

**Date:** 2026-11-13
**Decided by:** Repository owner; recorded by spec-author

## The call

Schema `0.4.0` moves the claim grammar from `identifiers.claim` to the `ids`
claim record and rejects `identifiers.claim` (`0001-F5` B-092, decision 0024).
The analyzer's policy reader and `specht evidence` both require
`identifiers.claim`, so moving this repository's pin to `0.4.0` (`0001-F4`
B-004) fails the `CodeAnalysis` target. `0001-F5` § 5 row 25 left the analyzer
reading `ids` to this Feature, and `0001-F5` § 7 recorded the conflict as not
settled.

On 2026-11-13 the repository owner selected "A, built in 0436": spec-author
amends 0201-F1, narrowing `0001-F5` § 5 row 25, so the analyzer and
`specht evidence` read the claim grammar from `ids.claim` (plain head + id),
built in the same 0436 pull request, so this repository moves to `0.4.0` and
the `CodeAnalysis` target stays green.

The rest of this record is spec-author's design under that selection, not the
owner's.

1. The analyzer reads the claim grammar from the `ids` claim record when its
   policy projection supplies one: the record's `plain` text ahead of `{id}`,
   matched as literal text, followed by a whole match of its `id` - the grammar
   `0001-F5` A-8 gives a Feature that declares no prefix (B-023).
2. A projection supplying `identifiers.claim` and no `ids` claim record is read
   as before (B-024).
3. The record is selected by what the projection supplies, not by the
   consumer's schema version. The analyzer treats that version as the
   consumer's (`0201-F6` B-024, decision 0005) and infers no mapping from it
   (`0201-F6` § 7).
4. A projection supplying both, or an `ids` claim record without its `plain` or
   `id`, or with a `plain` or `id` the checker would reject (`0001-F5` B-084,
   B-085), is rejected (B-025). No member is filled from the shipped record:
   the analyzer substitutes no shipped Specht manifest default for a supplied
   input (`0201-F6` § 7). A consumer adopting the analyzer at `0.4.0` writes
   the claim record's `plain` and `id` in its manifest; `specht upgrade` does
   not add `ids` (`0001-F7` B-016).
5. `specht evidence` reads the same projection and holds to the same reading
   (`0201-F5` B-045 to B-047).
6. The shipped manifests carry no `codeAnalysis` key, so no shipped mapping
   changes. This repository's own manifest carries the shipped `ids` record by
   record (`0001-F4` B-004), and its `codeAnalysis.mappings` gains one mapping
   of the claim record beside `/identifiers`, which still carries the Feature
   and epic grammars at `0.4.0`.

## Why

The owner's selection keeps one item for the cut and the pin move, and keeps
the analyzer usable at the pin this repository moves to. Selecting by the
projected record keeps the analyzer's input contract free of Specht's version
catalogue, as `0201-F6` decision 0005's 2026-10-18 reversal requires. Rejecting
both homes at once, rather than preferring one, keeps a single grammar per
claim (C-1: no parallel regex configuration or first-match resolution).

## Rejected

This section is spec-author's reading. Of the owner's words it holds only what
"The call" quotes.

- Selecting `ids.claim` or `identifiers.claim` by the projected schema version:
  the analyzer reads no Specht schema catalogue (`0201-F6` B-024).
- Filling a missing `plain` or `id` from the shipped `0.4.0` record
  (`0001-F5` B-097): a shipped default substituted for a supplied input, and a
  copy of the shipped record inside the analyzer.
- Preferring one of `ids.claim` and `identifiers.claim` when both are supplied:
  a silent first match.
- Reading a prefixed Feature's `form`, or the other six `ids` records: beyond
  the owner's selection, which names the claim grammar only (§ 5 row 7).

## Affects

0201-F1 B-023 to B-025, § 5 rows 7 and 8; `0201-F5` B-045 to B-047 and § 5
row 14; `0001-F5` § 5 row 25; item 0436.

## Reversal

None.
