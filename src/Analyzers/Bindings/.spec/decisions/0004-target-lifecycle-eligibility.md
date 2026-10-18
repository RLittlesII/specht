---
title: "Decision 0004: Target lifecycle eligibility"
description: "Separate resolving identity from eligibility for current evidence while preserving historical claim and ADR links"
type: decision
---

# Decision 0004: Target lifecycle eligibility

**Date:** 2026-10-17
**Decided by:** Repository owner

## The call

Adopt the following target lifecycle policy for current evidence:

| Resolved target                                                   | Eligible for current evidence                                                |
| ----------------------------------------------------------------- | ---------------------------------------------------------------------------- |
| Active or Amended claim in a draft, in-review or approved Feature | Yes.                                                                         |
| Withdrawn or Superseded claim                                     | No; preserve its resolved identity and lifecycle state as a historical link. |
| Any claim in a superseded Feature                                 | No; preserve its resolved identity and lifecycle state as a historical link. |
| Accepted ADR                                                      | Yes.                                                                         |
| Proposed, rejected, deprecated or superseded ADR                  | No; preserve its resolved identity and lifecycle state as a historical link. |

Identity resolution and lifecycle eligibility are separate results. A reference
to an existing retired record is resolving but ineligible, not an unresolved
reference merely because of its state. An actually missing record remains
unresolved. Reconciliation retains historical links but does not count their
bindings toward current required evidence.

An eligible target does not itself establish complete coverage, a passing test
or architectural compliance. Existing enforcement-scope, required-link policy
and execution-result questions still govern those separate conclusions.

Diagnostic namespace, severity, suppression and versioning remain open in
0201-F2 OQ-2 and epic OQ-9. This decision does not assign a warning or error level
to an ineligible target and does not choose a replacement target automatically.

## Why

Allowing current claims in draft and in-review Features supports writing tests
before approval. Retaining retired records keeps stable citations inspectable
without presenting historical obligations as evidence for current requirements.
Accepted ADRs describe adopted architecture; proposed or retired decisions
should remain distinguishable from it.

## Rejected

- Require approved Features before their current claim bindings are eligible:
  stricter document gating, but prevents those bindings from counting while
  tests are written before approval.
- Count every resolving record regardless of lifecycle: simpler lookup, but
  withdrawn claims or rejected architecture could satisfy current requirements.
- Treat a retired record as missing: conflates a resolvable historical citation
  with a broken identity and discards the distinction reviewers need.

The accepted cost is maintaining a separate eligibility result and historical
links, rather than using one resolving/not-resolving flag as a coverage verdict.

## Affects

0201-F1 B-008 through B-011 and OQ-2; 0201-F5 B-007; epic 0201 OQ-7.
No existing claim, decision or ADR is retired by adopting this policy. XML roles,
supported declarations and diagnostic policy remain separate open questions.

## Reversal

None.
