---
title: "Decision 0007: Duplicate relationship handling"
description: "Count relationships once per symbol, role and resolved target; diagnose duplicates and offer invoked redundant-copy removal"
type: decision
---

# Decision 0007: Duplicate relationship handling

**Date:** 2026-10-17
**Decided by:** Repository owner

## The call

Valid relationship elements with the same code symbol, XML role and resolved
target describe one relationship. This applies to `verifies`, `implements` and
`follows`, including repeated relationships across declarations of the same
partial-type symbol. Duplicate classification uses the existing resolved
identity model rather than a textual spelling or attribute-order comparison.

Analysis counts that relationship once and reports a duplicate diagnostic.
An invoked duplicate-removal fix removes redundant copies while preserving one
complete binding. Analysis itself does not edit documentation; unrelated XML
remains preserved under decision 0006.

Different roles on one symbol remain distinct even when they reference the
same claim. Different symbols remain distinct even when their role and target
match: two test methods verifying the same claim are separate relationships.

`spec` authoring defaults retain decision 0004's policy. Identical defaults
across partial-type declarations remain allowed; they are not relationship
duplicates under this rule. Invalid or unresolved elements retain their own
validation or resolution findings rather than being reclassified as valid
duplicates. Deduplication does not make a lifecycle-ineligible target count as
current evidence.

Diagnostic identifiers and severity, remaining code-fix safety rules and bulk
repair behavior stay in their existing open questions. This decision does not
choose an arbitrary target, change assertion adequacy or alter eligibility.

## Why

Counting the same declaration repeatedly would inflate apparent traceability.
Reporting redundant documentation keeps that problem visible, and a focused
invoked fix removes the repeated relationship without changing its meaning.
Including the symbol and role in the identity preserves intentional independent
verification and different declared relationships.

## Rejected

- Silently deduplicate: keeps reports accurate but leaves redundant documentation
  undiscovered and gives maintainers no focused cleanup action.
- Count every copy separately: makes evidence totals depend on repeated XML.
- Reject the entire relationship because it is duplicated: discards a valid
  declared relationship instead of preserving one copy.
- Deduplicate by target alone: collapses different tests or different roles and
  loses intentional traceability information.

The accepted cost is a duplicate diagnostic and an invoked documentation-only
repair, while analysis retains one relationship.

## Affects

0201-F2 B-025 through B-029 and OQ-1; epic 0201 OQ-2. Existing repeated-element,
strict-validation, lifecycle and partial-default decisions remain in force.

## Reversal

None.
