---
title: "Decision 0007: Rule settings are one rules key mapping a rule id to a level"
description: "The manifest lowers, promotes and disables a rule under one top-level key, rules, each rule id mapped to error, warning or off; any other value is an invalid manifest; an id not named keeps the rule's own severity"
type: decision
---

# Decision 0007: Rule settings are one rules key mapping a rule id to a level

**Date:** 2026-10-10
**Decided by:** the repository owner, asked during delivery of item `0014`; recorded by spec-author

## The call

Three calls, on the shape item `0014` proposed for B-010, B-011 and B-013
(A-3).

1. **`rules`.** One top-level manifest key, an object mapping a rule id to a
   level:

   ```json
   { "rules": { "SPEC010": "warning", "SPEC031": "off" } }
   ```

   | Level     | The rule                                                                  | Claim |
   | --------- | ------------------------------------------------------------------------- | ----- |
   | `error`   | is evaluated, and its violations are reported at error severity           | B-010 |
   | `warning` | is evaluated, and its violations are reported at warning severity         | B-010 |
   | `off`     | is not evaluated, reports nothing and leaves the count of rules evaluated | B-011 |

   An id is one of the pinned version's vocabulary (B-013, C-3).

2. **An id `rules` does not name keeps the rule's own severity** (B-019). The
   default manifest declares `"rules": {}`: nothing lowered, nothing disabled
   (A-4, C-4).

3. **A value that is not one of the three levels is an invalid manifest**
   (B-042): exit `3`, naming the rule id and the value.

## Why

- A rule cannot be both disabled and re-graded under one value, so there is no
  contradictory state to define or reject.
- `error` is a level so that a rule whose own default is warning can be
  promoted: epic `0101`'s rules, through ADR-0004's `DefaultSeverity`. Today
  every rule's own severity is error.
- A value that is an object - a type name, a library - is the wrong shape and
  is rejected, which serves B-017 and C-3.
- One key holds the concern, so the vocabulary check (B-013) runs in one
  place.

## Rejected

**`rules` mapping each id to an object with `severity` and `enabled`.** It
leaves room for a third per-rule setting. Rejected because C-7 rules out a
setting that changes a rule's logic, and because it needs rejections the
chosen shape does not: an unknown member, and what `enabled: false` beside a
`severity` means. Cost of rejecting: a later per-rule setting that is not a
level needs a new key or a new value shape, and a decision.

**Two keys, a `severities` map and a `disabled` list.** Rejected because one
id can appear in both, the vocabulary check runs in two places, and one
concern takes two top-level keys. Cost of rejecting: none recorded.

## Affects

- B-010, B-011, B-013: their key and values have names; the claims' text is
  unchanged.
- B-042: added.
- B-022, B-023: their list of rejections gains B-042.
- B-017: a rule declared as a value under `rules` is rejected by B-042; text
  unchanged.
- B-019: an id `rules` leaves out is the omitted rule setting; text unchanged.
- B-016, C-4: the default manifest's `rules` is empty, so the golden report is
  unchanged.
- § 5 row 14: added.
- § 11 OQ-1: partly resolved, for rule settings.
- § 11 OQ-13: opened; the owner was not asked.

Left open: the letter case of a level and what the rejection names for a value
that is not text (OQ-13). Not decided here: the severity of a rule's fault
violation, which `0001-F1` B-015 fixes at error (§ 5 row 7).

## Reversal

None.
