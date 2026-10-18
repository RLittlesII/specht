---
title: "Decision 0006: Strict validation of reserved XML elements"
description: "Diagnose invalid binding XML, exclude invalid relationships and defaults, and preserve unrelated documentation"
type: decision
---

# Decision 0006: Strict validation of reserved XML elements

**Date:** 2026-10-17
**Decided by:** Repository owner

## The call

Validate the four reserved elements against their defined role contracts:

| Element      | Required attributes | Optional attributes                                     |
| ------------ | ------------------- | ------------------------------------------------------- |
| `spec`       | `feature`           | `epic`                                                  |
| `verifies`   | `feature`, `claim`  | `epic`                                                  |
| `implements` | `feature`, `claim`  | `epic`                                                  |
| `follows`    | `adr`               | `feature`, `epic`, interpreted by 0201-F1 decision 0003 |

These fields reuse the existing identity grammars and owner-selection rules;
this decision does not redefine identity comparison or ADR ownership.

Reserved elements must be empty, with no body text or nested elements, and must
appear on a declaration supported for that role by decision 0003. Missing or
unknown attributes, nonempty content, malformed XML and unsupported placements
produce validation diagnostics. For example, `cliam` is an unknown attribute;
it cannot substitute for the required `claim` attribute.

An invalid `verifies`, `implements` or `follows` element supplies no checked
evidence. An invalid `spec` element supplies no authoring context. Analysis does
not silently reinterpret invalid input as another valid target. Existing
explicit-identity and safe-repair rules continue to apply.

An empty element with the correct attributes and placement passes XML-shape
validation; its IDs must still pass format validation, resolve under the adopted
ownership rules and meet lifecycle eligibility before supplying current evidence.

Unrelated XML documentation, including other custom tags, is preserved by
analysis and invoked binding repairs. The strict attribute and content rules
apply to the reserved elements rather than imposing a new schema on the rest
of a consumer's documentation.

Duplicate-relationship handling and remaining invoked repair details stay open.
This decision does not choose diagnostic identifiers, severity or suppression
policy, and does not authorize automatic edits or guessed attribute corrections.

## Why

A permissive parser can hide misspelled attributes or accept content that the
binding model never interprets. Strict validation makes those failures visible
and prevents malformed documentation from appearing as traceability evidence.
Preserving unrelated XML lets consumers retain their existing documentation.

## Rejected

- Ignore unknown attributes as extension metadata: more flexible but hides typos
  and leaves unsupported metadata looking meaningful.
- Permit body text or nested binding content: allows richer prose in the binding
  element, but introduces another interpretation contract beyond a single target.
- Count a partially valid relationship or default: produces misleading evidence
  or authoring context from an invalid contract.
- Apply the reserved-element schema to every XML documentation tag: constrains
  unrelated consumer documentation without helping binding validation.

The accepted cost is explicit validation diagnostics and requiring consumers to
place additional prose or metadata in separate documentation elements.

## Affects

0201-F2 B-020 through B-024 and OQ-1; epic 0201 OQ-2. Existing identity,
resolution, lifecycle and default-selection decisions remain in force.

## Reversal

None.
