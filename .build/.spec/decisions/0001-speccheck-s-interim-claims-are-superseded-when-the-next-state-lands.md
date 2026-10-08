---
title: "Decision 0001: SpecCheck's interim claims are superseded when the next state lands"
description: "B-023 and B-025 describe SpecCheck before 0001-F2's command and before 0001-F5's rule settings; each is tested until the item that lands the next state, which marks it Superseded and removes its test and its scenario"
type: decision
---

# Decision 0001: SpecCheck's interim claims are superseded when the next state lands

**Date:** 2026-10-08
**Decided by:** spec-author, answering spec-reviewer's item-cut finding;
it follows from the owner's C-7, and the owner may reverse it

## The call

B-023 and B-025 describe interim states of `SpecCheck` (C-7). Each claim's
test runs on this repository until its superseding item lands. That item marks
the claim `Superseded` in § 3 and § 9 and, in the same change, removes its test
and its `@B-023` or `@B-025` scenario, as `0001-F1` B-009 and `0001-F3` B-002
were withdrawn:

- B-023 (no check command yet) is superseded by B-025 when `0061` points
  `SpecCheck` at the command.
- B-025 (command, no rule settings) is superseded by B-013, B-024 and
  `0001-F2` B-011 when `0062` makes `SpecCheck` gate.

Neither claim is kept alive on a synthetic tree.

## Why

- Each Given describes this repository's history, not a tree. Whether
  `0001-F2`'s command and `0001-F5`'s rule settings exist is a fact about the
  build and the tool, so a synthetic tree cannot make the Given hold again.
- Keeping B-025 testable after `0001-F5` would need a `SpecCheck` that gates or
  not depending on the manifest's content. No claim asks for that, and it would
  be the same gate removed and re-added that C-7 rules out.

## Rejected

**Run the tests on a synthetic tree forever.** That invents a build behaviour
no claim states. Cost of rejecting: the interim behaviour has no test once it
is gone, and nothing remains for a test to check.

**Withdraw the claims now.** That would leave the interim states untested
while they are live. Cost of rejecting: two claims change status later.

## Affects

- B-023 and B-025: the Source column cites this decision.
- The `@B-023` and `@B-025` scenarios, removed with their tests.
- Items `0061` and `0062`, which carry the supersession.

## Reversal

None.
