---
title: "Decision 0002: Four XML binding roles"
description: "Use spec for type authoring defaults, verifies for test claims, implements for implementation claims and follows for ADR relationships"
type: decision
---

# Decision 0002: Four XML binding roles

**Date:** 2026-10-17
**Decided by:** Repository owner

## The call

Adopt these XML documentation elements and meanings:

| Element      | Meaning                                             | Target components                                                |
| ------------ | --------------------------------------------------- | ---------------------------------------------------------------- |
| `spec`       | Type-level authoring defaults only.                 | Local `feature` and optional `epic`.                             |
| `verifies`   | A declared test-to-claim verification relationship. | `feature`, `claim` and optional `epic`.                          |
| `implements` | A declared implementation-to-claim relationship.    | `feature`, `claim` and optional `epic`.                          |
| `follows`    | A declared code-to-ADR relationship.                | `adr` and the owner components defined in 0201-F1 decision 0003. |

The adopted claim identity fields, ADR ownership and lifecycle policy remain in
0201-F1 decisions 0002 through 0004. The following illustrate those components
with the now-adopted element names:

```csharp
/// <spec epic="0201" feature="F1" />
/// <verifies epic="0201" feature="F1" claim="B-001" />
/// <implements epic="0201" feature="F1" claim="B-001" />
/// <follows adr="ADR-0016" />
```

These illustrate the four roles individually, not one combined declaration.
A type default supplies authoring context; it is not checked evidence. Checked
claim bindings retain their own explicit identity. A verification or
implementation relationship does not prove behavior, and an ADR relationship
does not prove architectural compliance.

Supported CSharp declarations, repeated or multiple-target XML forms, validation
of extra attributes and remaining repair details are not settled by choosing
role names. They remain in OQ-1 or the diagnostic/default questions that own them.
This decision does not select diagnostic identifiers, severity or enforcement
scope, or add generator output to the production scope.

## Why

The element name makes the relationship readable without inferring intent from
an enclosing class or an assertion. Separate role names preserve the distinction
between authoring context, claimed implementation, verification and an
architectural reference.

## Rejected

- One generic binding element with an additional role attribute: fewer element
  names, but the relationship is less immediate when reading the documentation.
- Treat a type default as evidence: conflicts with the adopted hardened hybrid
  and permits silent retargeting when members move.
- Infer the role from a resolving reference alone: does not distinguish a test,
  implementation and an architectural citation.

The accepted cost is recognizing four explicit role names while reusing the
same identity and resolution model.

## Affects

0201-F2 B-008 through B-011 and OQ-1; epic 0201 OQ-2. 0201-F1 remains the owner of
reference components, ADR owner selection and target lifecycle eligibility.
Declaration kinds and other outstanding public contracts remain open.

## Reversal

None.
