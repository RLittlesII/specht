---
title: "Decision 0014: A rule whose own level is off is not run"
description: "A rule whose own level is off, and which the manifest does not raise, is not run and is not counted among the rules evaluated, as a rule the manifest sets off is not (B-011)"
type: decision
---

# Decision 0014: A rule whose own level is off is not run

**Date:** 2026-10-15
**Decided by:** the repository owner, answering spec-reviewer's round-2 question on item `0223`; recorded by spec-author

## The call

A rule whose own level is off, and which the manifest's `rules` does not set
to error or warning, is not run, and the count of rules evaluated excludes
it (B-062). A manifest that sets a rule off gets the same treatment under
B-011.

The owner's words: "If the rule is off, why would we waste cycles evaluating
it?"

## Why

- B-061 said such a rule reports nothing. It did not say whether the rule
  runs or is counted. B-011 says both for a rule the manifest sets off, and
  the engine already skips such a rule (ADR-0004: settings are selection).
  A rule that is off by its own level is now treated the same way.
- `0001-F7` B-014 says the check evaluates exactly the pinned version's
  vocabulary. Under B-011 it already holds while a manifest turns an id
  off, so it holds the same way for an id whose own level is off.
- Running a rule only to discard what it finds costs time on every check
  and changes no report.

## Rejected

**Run the rule and drop its violations.** Cost of rejecting: none recorded.
The owner's reason is the time it wastes.

**Skip the rule but count it among the rules evaluated.** The count would
then hold a rule that was never run, which B-011 rules out for a manifest's
`off`. Cost of rejecting: none.

## Affects

- Added: B-062, its § 9 row and its `@B-062` scenario.
- B-061 and B-011: unchanged.
- `0001-F7` B-014 and `0001-F12` B-020: unchanged.
- Item `0223` gains B-062. Item `0227` binds the `@B-062` scenario, which
  observes the waiver rule.

## Reversal

None.
