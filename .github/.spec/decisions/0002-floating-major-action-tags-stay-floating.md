---
title: "Decision 0002: floating major action tags stay floating"
description: "The build project's floating major action tags are kept, not pinned to full tags; they take minor and patch releases with no pull request, so B-002's grouping applies only to full-tag declarations"
type: decision
---

# Decision 0002: floating major action tags stay floating

**Date:** 2026-10-28
**Decided by:** the owner, asked directly while item 0075 was built

## The call

1. **The floating major tags stay.** `codecov/codecov-action@v5`,
   `actions/upload-artifact@v7`, `docker/login-action@v3` and `azure/login@v2`
   are kept as declared in the build project, not pinned to full tags.
2. **The gap is recorded, not closed.** Under `github-actions` versioning a
   floating major tag takes the action's minor and patch releases with no pull
   request and is offered only a new major (B-007). B-002's grouping applies to
   full-tag declarations, today only `renovatebot/github-action@v46.3.7`.

## Why

A floating tag already delivers the action author's minor and patch releases on
every run. Pinning would move each of them to a pull request and change the
`uses:` lines CI runs, for no behaviour a claim asks for.

## Rejected

- **Pinning every declaration to a full tag:** it makes B-002's Given reachable
  at the cost of a pull request per action release and a workflow change the
  owner did not ask for.

## Affects

B-002's source (no claim text changes); § 5 row 8 (new); § 10.

## Reversal

None.
