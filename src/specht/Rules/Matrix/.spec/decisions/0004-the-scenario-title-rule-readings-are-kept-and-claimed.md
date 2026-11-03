---
title: "Decision 0004: the scenario-title rule's readings are kept and claimed"
description: "SPEC4007 keeps the three readings item 0335 built from engine precedent - Gherkin tag inheritance, case-sensitive titles beside a tag check under claim.tagIgnoreCase, and no row exempt - and each becomes a claim, B-022 to B-025"
type: decision
---

# Decision 0004: the scenario-title rule's readings are kept and claimed

**Date:** 2026-11-02
**Decided by:** the repository owner (2026-11-02), answering a question card; recorded by spec-author

## The call

The owner answered, verbatim:

- "Keep, spec-author claims them (Recommended)"

As put to the owner, the option meant: keep, as built by item `0335`, the
three behaviours § 7 recorded under "Three behaviours no claim states", and
state each in § 3 in the pull request that builds `SPEC4007`, keyed
`SPEC022` (ADR-0029):

1. A claim tag on the `Feature:` line, a `Rule:` line, or an outline's
   `Examples:` or `Scenarios:` line is carried by the scenarios under it
   (B-022).
2. A Scenario cell title is compared with a scenario's title ordinally,
   case included, after trimming (B-023); whether that scenario carries the
   row's claim tag is decided under `claim.tagIgnoreCase` (B-024).
3. No row is exempt from `SPEC4007`: a row whose Status is the missing,
   waived, withdrawn or superseded marker is still reported (B-025).

## Why

- Each reading was taken from precedent in this engine, not invented, and
  § 7 names the precedent: `SPEC4006` counts a claim tag anywhere in the
  companion as carried (B-019), every marker and `SPEC4009`'s test names
  compare ordinally, the engine ignores case only where `claim.tagIgnoreCase`
  says so, and in this Feature an exemption is written into the claim that
  has it (B-002, B-006, B-011).
- A behaviour no claim states is one no test is bound to and no reviewer can
  hold the build to. Stating it makes it the agreement.

## Rejected

Spec-author's reading of the alternatives, none of which the owner chose:

- **Leave the three in § 7 alone.** The build would then decide a verdict
  the agreement does not state.
- **Change any of them.** Ignoring `Feature:`, `Rule:` or `Examples:` tags
  would have `SPEC4007` call a scenario untagged that `SPEC4006` counts as
  carrying the tag; ignoring case in titles would extend `claim.tagIgnoreCase`
  past tags; exempting rows by Status would make `SPEC4007` read a marker,
  which B-008 and B-009 do not state.

## Affects

- B-022 to B-025, new in § 3, each citing this decision; their scenarios in
  the companion and their § 9 rows, `Missing` until test-writer binds them.
- Item `0335`'s `claims`.
- § 7's "Three behaviours no claim states", which the implementer folds into
  the claims; no change here.

Spec-author's readings, not stated by the owner:

- **The second behaviour is two claims.** The title comparison and the tag
  comparison are separate assertions with separate tests, so B-023 and B-024
  each hold one.
- **The claims are scoped to `SPEC4007`.** B-022 says what a scenario carries
  for a title's tag check (B-009); `SPEC4008`'s "whose tag no companion
  scenario carries" (B-011, item `0336`) is not restated by it.
- **None is a boundary only.** Each states a verdict `SPEC4007` gives, so each
  is a § 3 claim; B-023's case-only difference is also an `@boundary`
  scenario, since it marks where a title stops matching.
- **A retired row passes both rules only with no title.** With
  `matrix.scenarioTitles` on and `SPEC4006` reporting, a row whose Status is
  the withdrawn or superseded marker passes `SPEC4006` and `SPEC4007` only
  when its Scenario cell holds no title: a title naming a scenario that
  carries the retired tag is reported by B-019, and one naming no such
  scenario by B-025 with B-008 or B-009. This follows from "no row exempt"
  as ruled; no claim's text changes.

## Reversal

None.
