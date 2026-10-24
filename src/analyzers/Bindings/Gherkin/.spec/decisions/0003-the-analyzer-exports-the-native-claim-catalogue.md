---
title: "Decision 0003: The analyzer exports the native claim catalogue"
description: "specht evidence never parses Gherkin; it reads the native claim catalogue from the test-evidence artifact the analyzer writes"
type: decision
---

# Decision 0003: The analyzer exports the native claim catalogue

**Date:** 2026-10-23
**Decided by:** Repository owner, in
[review 5635389907](https://github.com/RLittlesII/specht/pull/260#pullrequestreview-5635389907)
and the chat that followed it

## The call

The owner's call, in the part this record carries: "withdraw ADR-0021, [...]
have the analyzer export the catalogue in the evidence artifact". The analyzer is the only reader of a consumer's
`.feature` files. Under native Gherkin claim input it exports the native claim
catalogue in the test-evidence artifact it already writes, and `specht evidence`
reads the catalogue from that artifact. The tool parses no Gherkin.

Not decided here: how the analyzer parses Gherkin, which keywords and languages
it reads, and whether it uses a Gherkin package. That is a design proposal, in
[ADR-0001](../adr/0001-native-claim-source-contract.md), for the owner to accept.

## Why

One reader of claim files means one interpretation of them. A second parser in
the tool would have to agree with the analyzer's on every element, tag and line,
and the evidence artifact already carries what reconciliation consumes.

## Rejected

- `specht evidence` parses the `.feature` files itself: a second Gherkin reader
  that can disagree with the analyzer.
- A shared claims library both reference: withdrawn by the owner in the same
  review.

## Affects

B-030 added; § 5 row 11; 0201-F5 § 5 row 5. B-029 and § 5 row 10, which state
the English-only reading, rest on the proposal in ADR-0001, not on this call.

## Reversal

None.
