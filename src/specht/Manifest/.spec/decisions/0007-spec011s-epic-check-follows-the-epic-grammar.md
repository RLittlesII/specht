---
title: "Decision 0007: SPEC011's epic check follows the epic grammar"
description: "Without the manifest's epic grammar SPEC011 skips the epic path segment a layout declares and checks the Feature segment alone; the layout's declaration says where the epic sits, never whether it is checked, and no second toggle is added"
type: decision
---

# Decision 0007: SPEC011's epic check follows the epic grammar

**Date:** 2026-10-10
**Decided by:** the repository owner, asked during delivery of `0001-F6` item `0006`; recorded by spec-author

## The call

Resolves OQ-7.

- The manifest's epic grammar is the one switch
  ([decision 0004](0004-epics-are-opt-in-by-the-manifests-epic-grammar.md),
  C-10). A layout's epic segment
  ([`0001-F6` decision 0009](../../../Discovery/.spec/decisions/0009-a-layout-declares-its-path-identity-as-segment-indexes.md))
  says only where the epic sits, never whether it is checked.
- With no epic grammar declared, `SPEC011` skips the epic segment and checks
  the Feature segment only (B-042).
- No second toggle is added.

## Why

- Decision 0004 turned down a second manifest key for the epic tier because
  two signals could disagree. A layout's epic segment read as a switch would
  be that second signal.
- A manifest that drops the epic grammar and keeps the default layouts stays
  valid, and its Feature directories stay checked.

## Rejected

**Checking the epic segment regardless of the epic grammar.** Cost of
rejecting: in a repository without epics, a directory at the declared epic
segment is compared with nothing.

**Rejecting a manifest that declares an epic segment without the epic
grammar.** Cost of rejecting: such a manifest loads with no diagnostic, and
its epic segment is declared and unread.

## The owner's further request

The owner also asked that epics be "optional, off by default". That is the
shipped default manifest ceasing to declare the epic grammar. It belongs to
the item that delivers B-024 to B-035, and is not part of item `0006`. What it
does to A-4, C-4 and B-016 is OQ-13; no claim is written against it here.

## Affects

- B-042 (new).
- § 11 OQ-7: resolved. OQ-13: opened.
- `0001-F6` B-004: amended to name the epic grammar; `0001-F6` § 5 row 7.
- B-019 as amended by decision 0004 is unbuilt: the engine fills the epic
  grammar whatever the manifest declares, so no manifest reaches B-042 today.
  B-042 is owed with the item that delivers B-024 to B-035.
- B-035: unchanged.

## Reversal

None.
