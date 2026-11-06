---
title: "Decision 0022: An opt-in addition below 1.0.0 is a patch"
description: "The owner's ruling of 2026-11-06 on `0001-F16` OQ-5: a new schema version moves to the next 0.minor only for a breaking change; an addition that is not required, and reaches no tree that does not opt in, is a patch, so `0001-F16`'s keys and rules enter a new schema 0.3.1 and published 0.3.0 stays frozen"
type: decision
---

# Decision 0022: An opt-in addition below 1.0.0 is a patch

**Date:** 2026-11-06
**Decided by:** the repository owner, in session, answering `0001-F16` OQ-5;
recorded by `spec-author`

## The call

1. **The owner's rule, verbatim:** "Only move to 0.4.0 is we are making
   BREAKING CHANGES to the schema. Additions are not breaking as long as they
   are not required."
2. **The owner's choice of how it lands:** "0.3.1 patch, amend C-11". A new
   schema version `0.3.1` carries `0001-F16`'s added `claim` members and its
   three new rules. Each new rule reports only when a manifest sets
   `claim.source` to `gherkin`, a key `0.3.0` does not define, so no tree the
   check accepts under `0.3.0` gets a different report under `0.3.1`. Published
   `0.3.0` stays frozen (C-1).
3. **C-11 is amended in place.** Below `1.0.0`, an opt-in addition is a patch.
   An opt-in addition is an optional manifest key that the lower version does
   not define, with the rule ids and verdicts it adds reached only when a
   manifest sets that key. A change under which a tree that passed can fail
   still takes a new `0.minor`. B-066 is amended to match. B-154 states the
   property a patch must keep.

## Why

The owner's reason is in call 1: an addition is breaking only when it is
required.

The author's reading, not stated by the owner:

- A patch still changes no verdict of a tree the lower version accepts. A
  manifest that sets the new key is rejected by the lower version (`0001-F5`
  B-012) and gets a verdict for the first time under the patch. That is the
  "addition" the owner named.
- The ruling speaks of `0.4.0`, a `0.minor` step. It says nothing about schema
  `1.0.0` and later, where C-11 has a minor only accept more and B-040 makes a
  new rule id a major. That case is OQ-74, and B-040 is unchanged.
- How the numbering check tells an opt-in rule id from another is design. It
  belongs to § 7, for the implementer of the item that delivers B-066 as
  amended.

## Rejected

- **Changing `0.3.0` in place.** `0.3.0` is published and C-1 forbids it.
- **`0.3.1` without amending C-11.** C-11's Rules Out had excluded "at any
  version, a new rule ... in a patch", so `0.3.1` would have broken this
  Feature's own constraint.
- **A new `0.4.0`.** Call 1 keeps the `0.minor` step for breaking changes.

## Affects

- C-11 amended, with its Rules Out.
- B-066 amended, with a scenario for the opt-in patch and a `@boundary`
  scenario for a rule an existing tree can meet. Its existing scenario keeps
  its bound wording until the delivering item rebinds it.
- B-154 added, with its scenario and a `Missing` § 9 row.
- OQ-74 raised, for schema `1.0.0` and later.
- `0001-F16` OQ-5 resolved, and its A-2, A-4, B-001, B-028 and C-3 amended
  to name `0.3.1`.
- AGENTS.md § Configuration and § Invariants, which restate C-11.
