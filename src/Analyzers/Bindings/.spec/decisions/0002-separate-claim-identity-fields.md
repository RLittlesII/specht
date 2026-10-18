---
title: "Decision 0002: Separate claim identity fields"
description: "Canonical claim references carry separate epic, feature and claim fields, with epic optional"
type: decision
---

# Decision 0002: Separate claim identity fields

**Date:** 2026-10-17
**Decided by:** Repository owner

## The call

Use separate `epic`, `feature` and `claim` fields for a claim reference. `epic`
is optional. The `feature` value is the local Feature identity, not a string
constructed by combining its epic and local identity. Each supplied component
uses its existing consumer-configured grammar; the format does not introduce
a mandatory qualification delimiter.

Illustrative XML, with the element name still open in 0201-F2 OQ-1:

```csharp
/// <verifies epic="0201" feature="F1" claim="B-001" />
```

A consumer without an epic tier can omit that component:

```csharp
/// <verifies feature="CAP-012" claim="REQ-AU-054" />
```

The adopted hardened hybrid still applies: type defaults assist authoring,
while checked member bindings persist their own target identity. This decision
chooses claim identity fields, not XML element names, binding roles, declaration
kinds, ADR reference syntax or a new lookup rule for incomplete owner scope.
Those remaining choices stay open in the owning Features.

## Why

Separate components let each consumer grammar validate the value it describes.
They support an absent epic tier without forcing a separator or parsing a
compound Feature reference back into its components. Stable identity continues
to resolve independently of record paths and test frameworks.

## Rejected

A combined Feature reference such as `feature="0201-F1"` as the canonical way to
encode epic and local Feature identity. It is shorter, but needs an additional
qualification rule separate from the local Feature grammar. The accepted cost is
an additional epic field when the reference includes that component.

This does not forbid delimiter characters inside a consumer's local Feature
IDs. Such a value is validated as a whole local ID, not split by an assumed
separator.

## Affects

0201-F1 B-006 and OQ-1; epic 0201 OQ-3 (claim-reference encoding resolved,
ADR ownership and other record kinds still open); 0201-F2 OQ-1 retains XML
names and supported declarations as outstanding decisions. No production code
or previously assigned ID is changed.

## Reversal

None.
