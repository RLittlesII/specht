---
title: "Decision 0002: the release workflow is proven by records, and its dry run follows the merge"
description: "The owner kept the standing decision that the build has no tests for item 0083's five claims, whose proof is the generated file, recorded hand runs and the dry run; the delivering pull request closes the item before the dry run, which runs on main after the merge"
type: decision
---

# Decision 0002: the release workflow is proven by records, and its dry run follows the merge

**Date:** 2026-10-09
**Decided by:** the repository owner, 2026-10-09, asked while item `0083` was being designed; recorded by spec-author

Two calls, both about how item `0083` is proven and closed.

## The call

### 1. No tests, records only (OQ-9)

B-001, B-002, B-005, B-008 and B-009 get no test. The owner's standing
decision of 2026-10-08, quoted in [`0055-F1` § 8](../../../.spec/README.md),
stays as it is and is not narrowed for this Feature. Their proof is three
records, kept in § 8:

- the generated `publish.yml`, read against the design;
- hand runs of `VerifyTag`, written down with the commit they ran on;
- the dry run on `main`.

### 2. Merge, then dry-run `main` (OQ-10)

- The pull request that delivers item `0083` closes it on the generated file
  and the hand runs.
- The dry run is started on `main` right after the merge.
- A follow-up pull request records that run in § 8.
- A dry run that fails reopens the item.

## Why

**1.** The owner was asked whether any of the five claims gets an executable
proof, with what a test could assert for each set out in § 7 ("How each claim
can be proven without publishing"), and answered: no tests, records only.

**2.** GitHub starts a manual run only of a workflow file that is on the
default branch, so the dry run cannot happen on the pull request that delivers
the item, and that pull request is also the one that writes the item `done`.
That is GitHub's rule as documented; § 7 did not observe it. The owner
answered: merge, then dry-run `main`.

## Rejected

**1. Widening the rule to tests that read the committed `publish.yml`**, the
shape `ContinuousIntegrationSteps` has. Cost of rejecting: nothing pins the
triggers, the step order or the conditions against the next edit; each record
describes the file on the day it was made and nothing re-checks it; the five
§ 9 rows never name a test.

**2. Holding item `0083` open until the dry run has run**, with a second pull
request closing it. Cost of rejecting: between the merge and the follow-up the
item reads `done` while B-008 has no live proof, and the follow-up is work no
open item tracks unless one is cut for it.

## Affects

- § 9, rows B-001, B-002, B-005, B-008 and B-009: none names a test. § 8 holds
  the records (test-writer).
- § 5 #9: added.
- OQ-9 and OQ-10: resolved.
- `release.feature`: stays out of the acceptance tier; no step class binds it.
- Item `0083`: its criterion "§ 9 carries a row naming a test for each id
  above" is reworded, and a criterion for the post-merge dry run is added.
- `0055-F1` § 8, `0055-F2` § 8 and `0055-F5` decision 0002: unchanged.

## Reversal

None.

## Addendum - 2026-10-09

Recorded by spec-author after spec-reviewer round 4 (§ 12). The sections above
stand as written.

**The owner's answers.** "Why" gives both in the recorder's words. These are
the option labels the owner selected on 2026-10-09, verbatim:

- Call 1 (OQ-9): "No tests, records only"
- Call 2 (OQ-10): "Merge, then dry-run main"

**Affects, added: the rule this sets aside.** `spec-and-traceability` says "A
row whose test is missing blocks the item reaching done." For B-001, B-002,
B-005, B-008 and B-009 that rule is set aside: item `0083` closes on the
records while their § 9 rows read `Missing`, as `0055-F1`'s items did under the
standing decision. Whether § 9's vocabulary gains a by-record value, so that
such a row stops reading `Missing` and firing `SPEC060`, is `0001-F5` OQ-12:
open, and not put to the owner.

**The follow-up is tracked by a criterion, not an item.** No item was cut for
the post-merge dry run on the delivering branch, because an item id is reserved
on `main` before a branch claims it (`specht-conventions` § Delivery). Item
`0083`'s last criterion names what the follow-up pull request records.
