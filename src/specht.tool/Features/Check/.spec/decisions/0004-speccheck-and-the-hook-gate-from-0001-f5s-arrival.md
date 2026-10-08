---
title: "Decision 0004: SpecCheck and the hook gate from 0001-F5's arrival"
description: "B-010 and B-011 hold once 0001-F5's rule settings exist; before then this repository's SpecCheck target reports and never fails and the pre-commit hook does not run the check, as 0055-F1 C-7, B-013 and B-025 decide"
type: decision
---

# Decision 0004: SpecCheck and the hook gate from 0001-F5's arrival

**Date:** 2026-10-08
**Decided by:** the repository owner (recorded in `0055-F1` C-7 and B-025,
`0055-F2` C-4 and `0055-F6` C-6); applied to this Feature by spec-author,
answering spec-reviewer's round 5 finding

## The call

B-010 and B-011 apply only once `0001-F5`'s rule settings exist. Before then,
this repository's `SpecCheck` target runs the command and exits 0 whatever it
reports (`0055-F1` B-025), and the pre-commit hook does not run the check
(`0055-F1` B-013). From `0001-F5`'s arrival both exit with the command's code
(C-8).

## Why

- Until `0001-F5` lands, `SPEC060` is an error (`0001-F1` C-9 holds every
  severity at the baseline), and every approved specification here has
  `Missing` rows in § 9. A target that exits with the command's code would fail
  every run and every commit that stages a `.spec/` file, which `0055-F1` C-6
  rules out.
- As B-010 and B-011 were written, they contradicted `0055-F1` B-025, and no
  landing order of their items could satisfy both.

## Rejected

**Leave B-010 and B-011 as written and gate when `0001-F2` lands.** The owner
rejected that window at `0055-F1`'s round 2. Cost of rejecting: the hook gates
nothing on specifications until `0001-F5` lands.

**Filter `SPEC060` out in the build.** `0055-F1` C-6 rules it out. Cost of
rejecting: none.

**Withdraw B-010 and B-011 and write new claims.** Once `0001-F5` exists, the
claims say the same thing, and new ids would cut the anchors in § 9 and the
`.feature` file. Cost of rejecting: none.

## Affects

- B-010 and B-011 (Amended): each now starts from `0001-F5`'s rule settings.
- C-8 (added); § 5 row 11 (added).
- The `@B-010` and `@B-011` scenarios each gain that Given.
- Items `0031`, `0061` and `0062`: the gate goes in `0062`, and `0031` follows
  it.

## Reversal

None.
