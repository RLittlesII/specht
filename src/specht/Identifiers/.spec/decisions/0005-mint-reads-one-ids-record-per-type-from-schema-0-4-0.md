---
title: "Decision 0005: mint reads one ids record per type from schema 0.4.0"
description: "The owner's selections of November 12 2026, ending in Redesign: one record: from schema 0.4.0, a 0.minor step, each hand-cited type's id shape and mint settings are one record under a top-level ids key, which the check and mint both read; a Feature that declares a claim prefix mints a record's form, every shipped grammar admits the default draws, and the old mint and identifiers settings are rejected and moved by upgrade"
type: decision
---

# Decision 0005: mint reads one ids record per type from schema 0.4.0

**Date:** 2026-11-12
**Decided by:** the repository owner, selecting options in a planning session; the claims, the record's shape and the grammars are spec-author's design on those selections; recorded by spec-author

## The call

A consumer's Features declare `claim_prefix: ACTIVITY` and want every per-Feature id keyed `<prefix>-<minted id>`: claims `ACTIVITY-k3x9ab`, scenarios `ACTIVITY-SCENARIO-k3x9ab`, drawn by Nano ID or Snowflake. Under schema `0.3.1` mint prints `B-` and `S-` ids, the check reads a prefixed Feature's claims as the prefix and three digits (`0001-F5` B-079), and the shipped `claim` grammar rejects every minted id.

The owner's selections, by their option names, in order:

1. **"One featurePrefix key (Recommended)"** and **"Feature form wins (Recommended)"**: a separate top-level key for the per-Feature form, beside `mint` and `identifiers`. Superseded the same day by selection 6.
2. **"Widen all seven (Recommended)"**: every shipped grammar for the seven C-13 types admits the default Nano ID and Snowflake draws, beside the ids it admits under `0.3.1`.
3. **"Same opt-in rule (Recommended)"**: `constraint` and `openQuestion` take a Feature's form by the rule `claim` and `scenario` do.
4. **"0.4.0, widen all seven (Recommended)"**, answering `0001-F7` OQ-78: the change ships in schema `0.4.0`, a `0.minor` step (`0001-F7` C-11); `0.3.1`, which `specht.tool` `0.6.1-preview` carries, is unchanged (`0001-F7` C-1).
5. The owner, answering spec-reviewer's finding 2 against the draft of selection 1, which asked whether an explicit `mint.<type>.prefix` applies, in a prefixed Feature, to a type `featurePrefix` does not name: "maybe we should re-design the api surface, because every session points out a deficiency with how mint uses feature".
6. **"Redesign: one record"**: one record per type holds its form, its grammar and its mint settings.
7. **"Include decision (Recommended)"**: a `decision` record may have a form; it is opt-in.
8. **"Rejected; upgrade migrates (Recommended)"**: "At 0.4.0 those keys exit 3 with a message naming ids.<type>. When the pin moves to 0.4.0, specht upgrade rewrites them into ids records once. identifiers keeps task, feature and epic. One source of truth, and an explicit migration."
9. **"Drop it, ship default applies (Recommended)"**, on how `upgrade` moves a member: "A member byte-equal to the old pin's shipped default is the consumer's default, not a choice; upgrade drops it and the 0.4.0 shipped record (widened) applies. Customised members are moved verbatim."
10. **"Later 0.minor (Recommended)"**, answering `0001-F5` OQ-32: "Revise row 23 to a 0.minor after 0.4.0; 0.4.0 ships without 0349 and puppy.power is not held up."

Spec-author's design on those selections, not the owner's words:

- The record, its members, the head, the claim grammar and the shipped records: [`0001-F5` decision 0024](../../../Manifest/.spec/decisions/0024-one-ids-record-per-type-shapes-every-id-from-schema-0-4-0.md), `0001-F5` A-8, B-081 to B-099, C-14.
- Mint reads the type's record (A-9, C-19). A request whose `--feature` names a Feature that declares a claim prefix mints the record's `form` with that prefix (B-063); any other request, and a type whose record has no `form`, mints its `plain` (B-065, B-066). The part after the head must wholly match the record's `id` (B-068, C-4).
- The record's `mint` holds the strategy settings C-12 lists, without `prefix` (C-21): the head comes from `plain` or `form`, so a type has one place to state its prefix.
- A Feature whose `claim_prefix` `0001-F5` B-078 or B-100 reports is read as declaring none, for every type, in mint and the check (A-9, B-067). Where one of a request's claim and scenario heads is a prefix of the other, both stay valid, and mint prints no id whose text after the shorter head begins with the rest of the longer (B-076, B-077); the check reports the same claim ids (`0001-F5` B-101).
- The old settings exit `3` on a mint request (B-075); `specht upgrade` moves them, dropping a member byte-equal to a shipped default (`0001-F7` B-155 to B-161).

## Why

Selection 5 is the owner's reason. Spec-author's reading: when the check and mint read one record, an id mint prints is one the check accepts, and a consumer states each type's prefix once.

## Rejected

- Selection 1, superseded by selection 6: the form lived in one key and the grammar and the prefix in two others, so mint and the check could disagree.

The other options put to the owner are not in the record this decision was written from.

## Affects

A-1, A-6 (amended); A-9; B-009, B-011, B-012, B-043, B-055 (scoped below `0.4.0`); B-063, B-065 to B-068, B-070 to B-077; B-064, B-069 (withdrawn before merge); C-4, C-6, C-12, C-18 (amended); C-14 (reversed); C-17, C-19, C-21; C-20 (retired before merge); § 5 rows 16, 18, 20, 22 and 23; OQ-12's answer, from `0.4.0`; `0001-F5` decision 0024; `0001-F5` B-100, B-101, OQ-32; `0001-F7` OQ-78, B-016, B-020, B-051, B-155 to B-161; ADR-0036; items `0436`, `0437` and `0438`.

## Reversal

None.
