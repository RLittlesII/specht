---
title: "Decision 0006: the missing-scenario rule reads carried as the retired-claim rule does"
description: "SPEC4008 counts a claim tag on any tag line of a companion as carrying the claim, a Feature or Rule line with no scenario under it included, as SPEC4006 does, and the reading is claimed as B-026; B-011 and B-027 state the per-row and no-row cases as spec-author's reading"
type: decision
---

# Decision 0006: the missing-scenario rule reads carried as the retired-claim rule does

**Date:** 2026-11-13
**Decided by:** the repository owner (2026-11-13), selecting an option on a question card put after `spec-reviewer` round 11 on item `0336`; recorded by spec-author

## The call

The owner selected, verbatim:

- "(a) Keep, add claim (Recommended)"

The question put to the owner, worded by the session coordinating item
`0336` and not by the owner: does a claim tag on a `Feature:` or `Rule:`
line with no scenario under it count as carried for B-011? The options
were (a) keep it and spec-author writes it as a claim, as decision 0004
did, so `SPEC4006` and `SPEC4008` share one meaning of carried (C-3);
(b) keep it in § 7 alone; (c) count only a `Scenario:` or
`Scenario Outline:` carrying the tag.

As written here, option (a) means: `SPEC4008` keeps the verdict item `0336`
built, recorded in § 7 "Missing scenarios (0336)", and § 3 states it as
B-026.

## Why

- § 7 names the precedent: `SPEC4006` reads B-019's "a companion scenario
  carries its tag" as a claim tag on any tag line of a companion, through
  `MatrixResolver.ResolveClaims`, which `SPEC4008` reads unchanged. Reading
  B-011 otherwise would give one question two answers (C-3).
- A verdict no claim states is one no test is bound to and no reviewer can
  hold the build to (decision 0004).

## Rejected

Spec-author's reading of the options the owner did not select:

- **(b) Keep it in § 7 alone.** The build would decide a verdict the
  agreement does not state.
- **(c) Count only a scenario carrying the tag.** `SPEC4008` would call a
  claim uncarried that `SPEC4006` counts as carried.

## Affects

- B-026, new in § 3, citing this decision; its scenario in the companion and
  its § 9 row, `Missing` until test-writer binds it.
- Item `0336`'s `claims`.

Spec-author's readings, not stated by the owner:

- **B-026 is scoped to `SPEC4008`.** `SPEC4006`'s reading of B-019, the
  precedent, is § 7's and is not restated as a claim here, as decision 0004
  did not restate B-011 for B-022.
- **B-011 reports each row, and B-027 no row** (round 11 finding 3). B-011
  said "at the row" in the singular. A claim with two § 9 rows whose Status
  is neither the missing nor the waived marker is reported at each, since
  each is a line the finding can stand at; a claim with no § 9 row is not
  reported, since there is no row to stand at and `SPEC4004` reports a claim
  without exactly one row. Both are what § 7 records as built. They are two
  assertions with two tests, so B-011 is amended for the first and B-027
  holds the second.

## Reversal

None.
