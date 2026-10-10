---
title: "Decision 0002: B-005 has an executable proof"
description: "The owner chose an executable proof for B-005 over a recorded hand check, narrowing the standing decision that the build has no tests for this one claim only"
type: decision
---

# Decision 0002: B-005 has an executable proof

**Date:** 2026-10-09
**Decided by:** the repository owner (2026-10-09), asked directly while item `0080` was being delivered; recorded by spec-author

## The call

B-005 is closed by a test that runs, not by a recorded hand check. The
`@B-005` scenario is bound and run in the acceptance tier, which CI runs, and
that tier packs the tool to observe the package.

This narrows the owner's standing decision of 2026-10-08, for B-005 only:

> I don't think we need ReqnRoll (or any tests) for the build. Keep what we
> have, in case I change my mind, don't wire them into CI.

That decision is quoted in [`0055-F1` § 8](../../../.spec/README.md) and
[`0055-F2` § 8](../../../ContinuousIntegration/.spec/README.md), and item `0079`
applied it to this Feature. It stands everywhere else:

- B-001 to B-004 and B-006 to B-008 of this Feature keep their § 9 rows as
  they are. The owner was asked about B-005 and nothing else.
- `0055-F1`, `0055-F2` and every other build Feature are untouched. Their
  `.feature` files stay unlinked and their step classes stay unbound.

## Why

The owner was asked how B-005 is closed given the standing decision, and was
offered two options: (a) a recorded hand check, as item `0079` used, or (b) an
executable proof. The owner chose (b), having been told what it entails:

- it reverses "no tests for the build" for this claim;
- `versioning.feature` is linked into `test/specht.acceptance`;
- a new step class binds the scenario;
- `Pack` runs inside the acceptance tier that CI runs.

The owner gave the choice without further reasons.

## Rejected

**(a) A recorded hand check**, as § 8 carries for item `0079`'s claims: the
result is written down once, with the commit it was observed on, and nothing
re-checks it. Cost of rejecting: every acceptance run, on each CI leg, now
packs the tool; and "the build has no tests" carries one exception a reader
has to know. Keeping it would have left B-005 true on the day it was checked
and unwatched afterwards, with § 9 `Missing` for good.

## Affects

- `0055-F5` B-005: its § 9 row is to name a test. § 8 and § 9 follow when the
  test exists (test-writer).
- `versioning.feature`: linked into the acceptance tier. The `@B-005` scenario
  is unchanged; the other seven scenarios gain no bindings.
- Item `0080`: its criterion "§ 9 carries a row naming a test for each id
  above" stands, unlike item `0079`'s, which the standing decision superseded.
- `0055-F2` B-004 and § 5 #18 are unchanged: a run gains no `Pack` step. The
  acceptance tier packing within its own step is what the owner accepted
  here.
- `0055-F1` § 8 and `0055-F2` § 8: unchanged; the standing decision still
  describes them.

## Reversal

None.
