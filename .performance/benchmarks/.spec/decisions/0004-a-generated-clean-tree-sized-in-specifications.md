---
title: "Decision 0004: a generated, clean tree, sized in specifications"
description: "The benchmark fixture is a tree generated from its size alone - 1, 10, 100 or 1000 Feature specifications of one fixed shape, each with its companion .feature, under the shipped schema and manifest - on which the check reports nothing; a captured tree, random content and a tree with violations were turned down"
type: decision
---

# Decision 0004: a generated, clean tree, sized in specifications

**Date:** 2026-10-09
**Decided by:** spec-author, from the owner's call that fixtures are synthetic
(`SpecTree` approach, never a real `.spec/` tree, never `hooked` source); the
owner may reverse the sizes and the shape

## The call

- **Unit of size:** the number of Feature specifications. Every generated
  specification has the same shape - the twelve sections, a fixed number of
  claims, its companion `.feature` with one tagged scenario per claim - so a
  step in size changes the count and nothing else. The shape's numbers are the
  `implementer`'s.
- **Sizes:** 1, 10, 100 and 1000, the same set wherever a benchmark varies the
  tree (`0109-F2`, `0109-F3`).
- **Clean:** the check reports no violation on a generated tree (B-015), so a
  benchmark measures the path a passing repository takes.
- **Deterministic:** the tree is a function of its size (B-016, C-8). The
  temporary directory's own name may vary; nothing inside it does.
- **Schema and manifest** are the shipped copy, as the tests' `SpecTree` copies
  them; they are the contract under measurement, not a fixture (C-7).

## Why

- A count of specifications is what grows in a consumer's repository, and the
  one input every stage's cost can be stated against.
- Four sizes a decade apart give the growth shape - constant, linear, worse -
  over three decades, so a non-linear cost shows as a curve rather than a
  single point. Nobody has measured how large a consumer's tree is; if one
  exceeds 1000, the set gains a size by amendment.
- A clean tree keeps the measured path stable: a violation adds report and
  ordering work whose amount depends on which rule fired.
- A captured tree changes whenever its repository does, so two runs would
  measure different inputs; and the owner ruled it out (item `0108`;
  `specht-conventions` references/benchmarking.md). Determinism is AGENTS.md
  § Invariants: the same tree gives the same report.

## Rejected

**A copy of this repository's `.spec/` tree.** Moves with every commit, and is
a captured fixture (C-7). Cost of rejecting: the generated shape is a model of
a real specification, not one.

**Random content with a fixed seed.** Deterministic, but a reader cannot say
what the input was. Cost of rejecting: none.

**A tree with violations.** A second measured path whose cost depends on the
rules that fire. Cost of rejecting: the violation path is unmeasured until a
constraint asks (`0109-F1` § 5 row 7).

## Affects

- `0109-F1` B-015, B-016; C-7, C-8.
- `0109-F2` and `0109-F3`: every benchmark that varies the tree.
- `0109-F4`: the size-1 tree it starts the tool on.

## Reversal

None.
