---
title: "Decision 0015: Sign-off headers and per-role table defaults"
description: "The sign-off table has positional column roles and schema 0.2.0 fills omitted table roles without replacing explicit header lists"
type: decision
---

# Decision 0015: Sign-off headers and per-role table defaults

## Decision

Accepted by the owner, 2026-10-17, resolving OQ-24.

- `tables.signOff` supplies four positional columns: section, status, reviewer and note. Schema `0.2.0` defaults them to `Section`, `Status`, `Reviewer` and `Note`.
- From schema `0.2.0`, each omitted `tables` role is filled from the exact pin's default. An explicit header list replaces that role's whole list. Older pins retain whole-map replacement.
- `SPEC013` checks the sign-off headers through the existing role-driven header check. The waiver rule reads status and reviewer through the second and third columns rather than literal header names.
- No new minimum header-count rejection is introduced. Explicit lists keep their existing accepted shapes.
- A separate Manifest item delivers this prerequisite before 0227. Working schema `0.2.0` is relocked through the approved procedure; `0.1.0` remains unchanged.

## Alternatives

- A literal `Reviewer` header in the waiver rule would prevent consumer renaming and contradict the manifest-role direction.
- A separate member naming only the reviewer column would duplicate the table-role mechanism.
- Filling no missing table roles would make a manifest declaring only `matrix` lose the sign-off defaults.
- Filling individual header entries would alter explicit consumer lists; the whole list remains authoritative.
- Rejecting short lists adds an unapproved validation rule and is left outside this agreement.

## Affects

- `0001-F5` B-065 to B-067, B-002, B-019 and OQ-24.
- `0001-F12` B-018 and its renamed-column scenario.
- The default manifest and lock for working schema `0.2.0`; no new rule ID.

## Reversal

None.
