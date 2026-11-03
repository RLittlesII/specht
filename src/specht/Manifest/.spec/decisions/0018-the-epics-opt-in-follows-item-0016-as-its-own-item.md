---
title: "Decision 0018: the epics opt-in follows item 0016 as its own item"
description: "The owner's ruling of November 2 2026 on item 0016, resolving OQ-18: one item, 0345, carries B-024 to B-035 and B-042 after 0016 in milestone 0.4.0, and 0016 builds the forms for the declared-epic shape and keeps today's epic-less behaviour"
type: decision
---

# Decision 0018: the epics opt-in follows item 0016 as its own item

**Date:** 2026-11-02
**Decided by:** the repository owner, answering item `0016`'s question 3 with "A cut item after 0016 (Recommended)"; recorded by spec-author

## The call

- One item, `0345`, carries the epics opt-in: B-024 to B-035 and B-042.
- Item `0345` depends on item `0016` and sits in milestone `0.4.0`. The
  first package that carries schema `0.3.0` then holds both (`0001-F7`
  OQ-72, default (a)).
- Item `0016` builds the identity and edge forms, the named task groups
  and the gap width for a manifest that declares the epic grammar.
- For a manifest that declares no epic grammar, item `0016` keeps the
  identity, the edge resolution and the item sequence as they are at
  `d73b7a51`. Changing them is item `0345`'s work (B-025 to B-032).
- Item `0016` checks the forms as declared, whatever the epic grammar
  (B-052).

## Why

- Since items `0321` and `0319` a `0.3.0` tree may hold no epic. No item
  carried B-024 to B-035 and B-042.
- The code B-005 replaces assumes an epic. So does the model's filter on a
  present `epic`, which B-027 removes.
- Building item `0016` first means the identity, the edges and the sequence
  are changed once for the forms, and then once for the epic-less shape.

## Rejected

- **(B) Build the epics item first.** Item `0016` would then adapt the
  identity, the edges and the sequence a second time. OQ-17's ruling
  rejected that cost for ADR-0010's stages.
- **(C) Fold the claims into item `0016`.** One change above risk 4,
  against A-2's one move at a time.

## Affects

- `0001-F5` OQ-18, resolved.
- Items `0016`, `0345` and `0010`; `.issue/.goal`.
- OQ-4, OQ-5 and OQ-13 stay open and are item `0345`'s to settle.

## Reversal

None.
