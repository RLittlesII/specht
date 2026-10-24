---
title: "Decision 0002: the default is the embedded manifest, and the text form is the JSON document"
description: "--defaults prints the manifest the tool embeds for the newest shipped version, not what an unbuilt init option writes; the text form is the JSON document indented in the default manifest's key order; the Feature moves from blocked to ready-for-architecture"
type: decision
---

# Decision 0002: the default is the embedded manifest, and the text form is the JSON document

**Date:** 2026-10-24
**Decided by:** `spec-author`, grooming. Neither call changes what the owner decided in decision 0001; the text form is a derived default (OQ-1).

## The call

1. **Status.** The Feature was drafted `blocked` on `0001-F2`, `0001-F4`,
   `0001-F5` and `0001-F7`. `0001-F2` and `0001-F5` are built; `0001-F7`'s list
   and pin commands are built (items `0121` to `0125`) and only `upgrade`
   (item `0047`) is outstanding, which no claim depends on. The status is
   `ready-for-architecture`; `spec_status` stays `draft` for the spec-reviewer.
2. **The default is the embedded copy.** A-3 and B-012 named what
   `specht init --epics` writes. That option is specified and not built and
   has no item (`0001-F4` B-018 Missing, OQ-18). They now name the
   `manifest.json` the tool embeds for the newest version it ships, which is
   the file `0001-F4` B-003 holds `init`'s output to. B-012 keeps its id.
3. **Text form (B-020).** Without `--json`, stdout is the document `--json`
   prints, indented. Keys follow the default manifest of the version being
   printed; rule settings follow ascending ordinal rule id.

## Why

- A claim cannot be tested against a command that does not exist. The embedded
  copy is the source `init` copies from, so the claim says the same thing
  without the dependency.
- One document shape and no second renderer: the manifest is JSON, so a
  key-per-line listing would be a second form to specify and keep in step
  (C-2).
- A fixed key order makes the output deterministic (B-019, C-3) and diffable
  against `--defaults`.

## Rejected

**Wait for `init --epics`.** Cost of rejecting: none; the embedded copy is what
that command would copy.

**A key-per-line listing, or the on-disk manifest's own key order.** Cost of
rejecting: a declared order the author chose is not echoed; the effective
manifest is the engine's view, not the file's.

**Alphabetical key order.** Cost of rejecting: it separates keys the default
groups together.

## Affects

- `0001-F9` A-3, B-012, B-020 (new), § 9 rows B-012 and B-020, OQ-1.
- Not decided: a key whose absence is a setting (OQ-2), a version for
  `--defaults` (OQ-3), a published schema (OQ-4), provenance of a value (OQ-5).

## Reversal

None. The owner's answer to OQ-1 supersedes B-020 by amendment.
