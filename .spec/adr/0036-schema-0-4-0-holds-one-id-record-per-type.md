---
title: "ADR-0036: Schema 0.4.0 holds one id record per type"
description: "The owner's selections of November 12 2026, Redesign: one record, Widen all seven and 0.4.0: from schema 0.4.0, a 0.minor step, each hand-cited type's form, grammar and mint settings are one record under a top-level ids key, the shipped records admit the default Nano ID and Snowflake draws, and the top-level mint key and the identifiers grammars for those types are rejected and moved by upgrade. Supersedes in part ADR-0033. The record's shape and grammars are spec-author's, in 0001-F5 decision 0024."
type: adr
---

# ADR-0036: Schema 0.4.0 holds one id record per type

## Status

accepted - 2026-11-12. Decider: the repository owner, selecting the options
"Redesign: one record", "Widen all seven (Recommended)", "Include decision
(Recommended)", "Rejected; upgrade migrates (Recommended)" and "Drop it,
ship default applies (Recommended)", and the
version by "0.4.0, widen all seven (Recommended)", answering `0001-F7`
OQ-78; all are recorded in `0001-F19`
[decision 0005](../../src/specht/Identifiers/.spec/decisions/0005-mint-reads-one-ids-record-per-type-from-schema-0-4-0.md).
Recorded by spec-author. The record's shape, the grammars and their reading
are spec-author's, not the owner's words.

Supersedes in part
[ADR-0033](0033-hand-cited-ids-are-minted-by-nano-id-and-snowflake-takes-random-fields.md),
from schema `0.4.0`: its judgement that no shipped default grammar changes,
its answer to `0001-F19` OQ-12, and the top-level `mint` key with a `prefix`
per type (`0001-F19` C-12). ADR-0033's body is unedited and its Status gains
one line pointing here, as ADR-0010's did for ADR-0026. The ADR template says
a new record supersedes an accepted one entirely; this partial supersession
is deliberate. ADR-0033's strategies, sizes, alphabet and fields stand.

## Context

- Under schema `0.3.1` a type's shape is in three places: its grammar under
  `identifiers`, its prefix under `mint`, and, for a prefixed Feature's
  claims, B-079's rule (`0001-F5` B-079). Mint and the check can disagree.
- A type opted in to minting under the shipped grammars is refused: a grammar
  that rejects the draw stops the id (`0001-F19` B-011), and an opted-in type
  with no grammar is an invalid manifest (`0001-F19` B-043).
- A published package carries `0.3.1` (`specht.tool` `0.6.1-preview`), so it
  is not changed in place (`0001-F7` C-1). A wider `claim` default can fail a
  tree `0.3.1` accepts, so the change is a `0.minor` step (`0001-F7` C-11,
  OQ-78).

## Decision drivers

- An id mint prints is one the check accepts.
- A consumer states each type's shape once.
- No id a `0.3.1` default admits is rejected by a `0.4.0` default (`0001-F5`
  C-15).

## Considered options

1. Keep ADR-0033: the `mint` key, the `identifiers` grammars, and no shipped
   default change.
2. A separate `featurePrefix` key beside them, the owner's earlier selection
   "One featurePrefix key", superseded the same day.
3. One `ids` record per type holding the form, the grammar and the mint
   settings.

## Decision

Option 3, on the owner's selection. From schema `0.4.0`:

- A type's `ids` record holds `plain`, `form`, `id` and `mint`; the check and
  mint both read it (`0001-F5` A-8, C-14; `0001-F19` C-19, C-21).
- The shipped records admit, beside every id the `0.3.1` default admits, the
  type's in-tree head followed by 6 or 13 lowercase base32 characters, or 8
  or 13 for `adr` and `lesson` (`0001-F5` B-089, B-090, B-099). The records
  are in `0001-F5`
  [decision 0024](../../src/specht/Manifest/.spec/decisions/0024-one-ids-record-per-type-shapes-every-id-from-schema-0-4-0.md).
- The top-level `mint` key and the `identifiers` members for the seven types
  are invalid (`0001-F5` B-092); `identifiers` keeps `task`, `feature` and
  `epic`.

`0.3.1` and every lower version keep their keys and defaults.

## Consequences

- A consumer moving its pin to `0.4.0` runs `specht upgrade`, which moves its
  customised `mint` entries and grammars into `ids` records once, drops a
  member byte-equal to a shipped default below `0.4.0` on the owner's ruling
  "Drop it, ship default applies (Recommended)", and refuses a move it cannot
  make exactly (`0001-F7` B-155 to B-161, OQ-79). Until then the manifest is
  invalid; `schema pin` can still move it back down (`0001-F7` B-051).
- The wider `claim` record can change the report of a tree that holds an id
  or a companion tag only it admits.
- The check reads the `claim` record's grammar and the `scenario` record's
  heads; the other records change what `mint` accepts, not a verdict
  (`0001-F5` B-091).
