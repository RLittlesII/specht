---
title: "Decision 0005: Repeated single-target relationship elements"
description: "Repeat verifies, implements and follows with one complete target per element and scalar identity attributes"
type: decision
---

# Decision 0005: Repeated single-target relationship elements

**Date:** 2026-10-17
**Decided by:** Repository owner

## The call

Each `verifies`, `implements` or `follows` element carries one complete target.
Repeat the element when a declaration has multiple relationships. Every element
supplies its own required identity components under 0201-F1 decisions 0002 and
0003; components do not carry over from sibling elements or enclosing defaults.

```csharp
/// <verifies epic="0201" feature="F1" claim="B-001" />
/// <verifies epic="0201" feature="F1" claim="B-002" />
```

The same repeated-element rule applies to implementation claims and ADR
relationships. Different targets can name different Features or ADR owners;
each is resolved using its own explicit components. Role placement remains as
defined in decision 0003.

An identity attribute contains one identifier governed by the consumer grammar.
There is no comma-, whitespace- or other delimiter-based list parsing. A value
containing a delimiter can still be a valid single ID when the consumer grammar
allows it; it never expands into multiple targets. A purported list that is not
one valid ID remains invalid under the existing format checks.

This relationship form does not turn `spec` authoring defaults into a target
list. Default selection remains governed by decision 0004. Exact-duplicate
handling, additional attributes, element content, other XML validation and safe
repair details remain open in OQ-1 / epic OQ-2. Diagnostic identifiers and
severity remain open in OQ-2 / epic OQ-9.

## Why

One complete target per element keeps each relationship readable and gives
analysis and invoked repairs an individual relationship to address. It also
preserves consumer ID grammars without introducing a conflicting list syntax.

## Rejected

- Put multiple IDs in one attribute: shorter comments, but requires list syntax
  alongside consumer grammars and complicates diagnostics and individual repairs.
- Group targets under a new wrapper or nested target structure: reduces repeated
  role names, but adds XML structure without improving the identity contract.
- Let repeated elements share missing identity components: reduces repeated
  attributes but makes a relationship depend on neighboring documentation.

The accepted cost is repeating role names and complete target components in
exchange for independently inspectable relationships.

## Affects

0201-F2 B-006, B-019 and OQ-1; epic 0201 OQ-2. This decision extends the public
XML form without changing target resolution, lifecycle eligibility or the
framework-independent binding model.

## Reversal

None.
