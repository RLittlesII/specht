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

## Addendum: Complete identities under the consumer model — 2026-10-17

**Decided by:** Repository owner

The owner adopts the OQ-1 recommendation with these completeness rules. For a
claim owner, `epic` is required when the manifest declares an epic tier and is
absent when it does not. The original optionality describes support for both
consumer models; it does not permit incomplete ownership in an epic-enabled
model. `feature` remains the local Feature ID rather than its combined display
citation, and each component uses its own pinned consumer grammar.

A checked member persists every required target component. Enclosing defaults
never complete its identity during resolution. Exactly one declaration must
match in the selected owner: no match is unresolved and multiple matches are
ambiguous, regardless of discovery order. Moving a record without changing its
identity or owner preserves the target. Paths never serve as identity components.

### Alternatives and cost

- Allow omission of an epic and select the only currently matching Feature:
  less repetition, but adding another epic could change a persisted binding's
  meaning or make its formerly implicit owner ambiguous.
- Fill missing member components from a type default: shorter documentation,
  but moving the member could silently retarget it.
- Select the first duplicate declaration: avoids an ambiguity result, but makes
  discovery order determine meaning.
- Bind through a record path: convenient direct lookup, but record moves break
  stable citations.

The accepted cost is explicit owner repetition and visible incomplete or
ambiguous references. These rules add 0201-F1 B-013 through B-016 and amend
B-006 without changing its ID. The companion XML agreement owns syntax and
invoked repairs; no new public ownership attribute is introduced here.
