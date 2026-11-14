---
title: "Decision 0007: the status rule reads carried as the missing-scenario rule does"
description: "SPEC4010 counts a claim tag on any tag line of a companion as a scenario carrying the row's claim tag (A-1), as SPEC4008 does under B-026, claimed as B-028; three more of item 0337's open cases are read from claims already stated"
type: decision
---

# Decision 0007: the status rule reads carried as the missing-scenario rule does

**Date:** 2026-11-13
**Decided by:** spec-author (2026-11-13), reading C-3 and decision 0006 on test-writer's open cases for item `0337`; not stated by the owner

## The call

1. A row's claim tag is carried, for whether `SPEC4010` finds the row's
   scenario resolved (A-1), when it is on any tag line of a companion, one
   above a `Feature:` or a `Rule:` with no scenario under it included, as
   `SPEC4008` counts the claim carried (B-026). Claimed as B-028.
2. A covered or partial row whose claim is withdrawn or superseded in § 3,
   or absent from § 3, is judged by B-013 and B-014 like any other row.
3. A live claim's row that `SPEC4008` reports is not reported by
   `SPEC4010`, an unknown Status included: B-012 holds over B-017.
4. B-013's double report beside `SPEC4009` gains an acceptance scenario.

## Why

- C-3 rules out a second reading of a tag. The owner selected one meaning
  of carried for `SPEC4006` and `SPEC4008` on that ground (decision 0006);
  reading A-1 otherwise would have `SPEC4010` report a covered row whose
  scenario `SPEC4008` counts as carried.
- Calls 2 and 3 are what B-012, B-013 and B-014 say as written. In this
  Feature an exemption is written into the claim that has it (decision
  0004), and none of B-013 and B-014 exempts a row by its claim's § 3
  Status; B-012 says `SPEC4010` does not also report a row `SPEC4008`
  reports, with no exception for the Status. A rule reporting a row beside
  another is accepted here (B-006, B-013).
- A claim's stated verdict that only a unit theory observes is not
  documented where the scenario is read.

## Rejected

- **Reading A-1's carried from the scenarios alone for `SPEC4010`.** Two
  answers to one question (C-3).
- **Exempting a row whose claim is not live.** No claim states it, and it
  would add an exemption the owner did not make.
- **Reporting an unknown Status beside `SPEC4008`.** It contradicts
  B-012's text, which is the owner's (2026-10-14).

## Affects

- B-028, new in § 3; its scenario and its § 9 row, `Missing` until
  test-writer binds it. Item `0337`'s `claims`.
- New scenarios under `@B-012` and `@B-013`; no claim's text changes.
- Not decided here, and put to the owner: whether B-012 follows `SPEC4008`'s
  condition or its emitted report when `rules` sets `SPEC4008` to `off`, and
  whether an empty Scenario cell under `matrix.scenarioTitles` leaves a
  row's scenario resolved.

## Reversal

None.
