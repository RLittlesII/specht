---
title: "Decision 0024: one ids record per type shapes every id from schema 0.4.0"
description: "The owner's selections of November 12 2026 applied to the manifest: from schema 0.4.0 a top-level ids key holds one record per hand-cited type, with its plain text, its form in a prefixed Feature, its grammar and its mint settings; the check reads a Feature's claim grammar from the claim record, the shipped records admit the default minted ids, and the old mint key and identifiers members are rejected; the record's shape and the grammars are spec-author's design"
type: decision
---

# Decision 0024: one ids record per type shapes every id from schema 0.4.0

**Date:** 2026-11-12
**Decided by:** the repository owner, selecting options in a planning session; the record's shape, the head, the claim grammar and the shipped records are spec-author's design on those selections; recorded by spec-author

## The call

The owner's selections, by their option names, are recorded in [`0001-F19` decision 0005](../../../Identifiers/.spec/decisions/0005-mint-reads-one-ids-record-per-type-from-schema-0-4-0.md): "Widen all seven (Recommended)", "Same opt-in rule (Recommended)", "0.4.0, widen all seven (Recommended)", "Redesign: one record", "Include decision (Recommended)", "Rejected; upgrade migrates (Recommended)", "Drop it, ship default applies (Recommended)" and, answering OQ-32, "Later 0.minor (Recommended)". "One featurePrefix key (Recommended)" and "Feature form wins (Recommended)" are superseded by "Redesign: one record". This record holds what they mean for the manifest and the check.

Spec-author's design on those selections, not the owner's words:

1. **The key.** `ids` is a top-level manifest key from schema `0.4.0` (C-14), naming any of `claim`, `constraint`, `openQuestion`, `scenario`, `decision`, `adr` and `lesson`. A record holds:
   - `plain`: the id outside a prefixed Feature. It ends with its only `{id}` and holds no other brace (B-084).
   - `form`: the id in a Feature that declares a claim prefix. It holds `{prefix}` once, ends with its only `{id}` and holds no other brace; `adr` and `lesson` have none (B-084).
   - `id`: a regular expression that compiles and is not empty (B-085).
   - `mint`: `0001-F19` C-21's settings, with no `prefix`.

   A `$`-prefixed member is an annotation (B-083). A missing record or member is read from the shipped record (B-097).

2. **The head and the claim grammar** (A-8). A Feature's rendered form for a type is the record's `form` with the Feature's claim prefix for `{prefix}`, when both exist; otherwise the record's `plain`. The head is the rendered form without `{id}`. The grammar is the head as literal text followed by a whole match of `id`, case-sensitive. A `claim_prefix` that B-078 reports is read as no prefix (OQ-31's default).

3. **What the check reads.** The `claim` record's grammar for each Feature (B-086, B-087), and the `scenario` record's heads to tell a scenario tag from a claim tag (B-094, B-095). A Feature whose rendered claim and scenario heads are equal is reported, and its prefix read as none (B-100). A head that is a prefix of the other stays valid: a tag beginning with the longer scenario head is not a claim tag (B-095), and a claim ID whose text after the claim head begins with the rest of the scenario head is reported (B-101). Each test compares literal text, never two grammars. B-091 holds the check unchanged by the other records, by the scenario `id` and by every `mint`.

4. **The shipped `0.4.0` records** (B-089, B-090, B-098, B-099, C-15). `b32` stands for `[0-9a-hjkmnp-tv-z]`, the lowercase base32 alphabet Nano ID draws from and Snowflake writes in by default (`0001-F19` B-056):

   | Type           | `form`         | `plain`   | `id`                              |
   | -------------- | -------------- | --------- | --------------------------------- |
   | `claim`        | `{prefix}{id}` | `B-{id}`  | `[0-9]{3}[a-z]?\|b32{6}\|b32{13}` |
   | `constraint`   | none           | `C-{id}`  | `[0-9]+\|b32{6}\|b32{13}`         |
   | `openQuestion` | none           | `OQ-{id}` | `[0-9]+\|b32{6}\|b32{13}`         |
   | `scenario`     | none           | `S-{id}`  | `[0-9]+\|b32{6}\|b32{13}`         |
   | `decision`     | none           | `{id}`    | `[0-9]{4}\|b32{6}\|b32{13}`       |
   | `adr`          | none           | `{id}`    | `[0-9]{4}\|b32{8}\|b32{13}`       |
   | `lesson`       | none           | `{id}`    | `[0-9]{4}\|b32{8}\|b32{13}`       |

   Six characters is the per-Feature Nano ID default and eight the repository-wide one (`0001-F19` B-020, B-021); thirteen is a base32 Snowflake (`0001-F19` B-031). The claim `form` keeps B-079's prefix with no separator, so a prefixed Feature's numbered claims still pass. A consumer opts a type in to a form by writing one.

5. **The old settings.** From `0.4.0` the top-level `mint` key and the `identifiers` members for the seven types are invalid, and the rejection names `ids.<type>` (B-092, B-093). `identifiers` keeps `task`, `feature` and `epic`. `specht upgrade` moves the old settings once, verbatim, and drops a member byte-equal to a shipped default below `0.4.0`, so the `0.4.0` record applies (`0001-F7` B-155 to B-161, OQ-79).

A consumer that wants `ACTIVITY-k3x9ab` claims and `ACTIVITY-SCENARIO-k3x9ab` scenarios pins `0.4.0` and writes:

```json
"ids": {
  "claim":    { "form": "{prefix}-{id}",          "mint": { "strategy": "nanoid" } },
  "scenario": { "form": "{prefix}-SCENARIO-{id}", "mint": { "strategy": "snowflake" } }
}
```

The heads are `ACTIVITY-` and `ACTIVITY-SCENARIO-`. A tag beginning with the longer scenario head is not a claim tag (B-095).

## Why

The owner's reason is in `0001-F19` decision 0005. Spec-author's reading: the check and mint read one record, so neither can accept an id the other rejects, and a type's shape has one place.

## Rejected

Considered by spec-author and not chosen:

- A form in a Feature's frontmatter: one grammar stated per Feature as well as per manifest (C-14).
- A claim grammar that must also satisfy a repository-wide grammar: decision 0022 rejected that double check for B-079.
- `upgrade` widening a grammar the consumer wrote: explicit consumer values win (`0001-F7` B-155 moves them unchanged).

## Affects

Need 7; A-8; B-007, B-022, B-023, B-076, B-079 (amended); B-081 to B-095, B-097 to B-101; B-096 (withdrawn before merge); C-2, C-12 (amended); C-14, C-15; § 5 rows 23 (amended) and 25 to 29; OQ-32 (resolved), OQ-33; `0001-F19` decision 0005; `0001-F7` OQ-78, OQ-79, B-051, B-155 to B-161; ADR-0036; item `0436`.

## Reversal

None.
