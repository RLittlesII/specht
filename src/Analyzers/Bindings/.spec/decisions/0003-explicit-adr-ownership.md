---
title: "Decision 0003: Explicit ADR ownership"
description: "ADR references without a Feature select repository scope; explicitly Feature-qualified references select only that Feature"
type: decision
---

# Decision 0003: Explicit ADR ownership

**Date:** 2026-10-17
**Decided by:** Repository owner

## The call

An ADR reference carries its decision identity in `adr`. Without `feature`, it
selects a repository-owned ADR. With `feature`, it selects an ADR owned by that
explicitly named Feature; `epic` remains optional. Each reference is resolved
only in that selected owner scope.

Illustrative reference fields (XML element names remain open in 0201-F2 OQ-1):

| Reference fields                          | Selected owner                                                |
| ----------------------------------------- | ------------------------------------------------------------- |
| `adr="ADR-0016"`                          | Repository.                                                   |
| `epic="0201" feature="F6" adr="ADR-0001"` | The explicitly named Feature; this ADR is illustrative.       |
| `feature="CAP-012" adr="ADR-0001"`        | The explicitly named Feature in a model without an epic tier. |

Missing `feature` never inherits an enclosing type's authoring default. A missing
ADR in the selected scope remains unresolved even if another scope declares the
same local decision identity. Resolution never searches both scopes or silently
retargets an existing reference after a member moves.

This chooses identity and ownership, not lifecycle eligibility, diagnostic
severity, XML element names or additional record kinds. Those decisions remain
with their existing open questions.

## Why

Local ADR numbers can repeat in repository and Feature records. Explicit owner
selection preserves the reference's meaning across moves and distinguishes an
unresolved selected target from a different record with the same local number.
Omitting the Feature is a stable repository-scope rule, not inherited scope.

## Rejected

- Resolve an unqualified ADR using the nearest type's Feature default: convenient
  while authoring, but a member move could silently change the selected decision.
- Search Feature and repository scopes, or fall back when the selected owner
  lacks the ADR: may hide broken references and accept an unintended decision.
- Put Feature/epic qualification into the ADR string: needs another delimiter
  contract instead of reusing the separate owner components.

The accepted cost is explicit owner qualification for Feature ADRs and an
unresolved-reference finding when the selected owner lacks the decision.

## Affects

0201-F1 B-004 (amended), B-007 and OQ-1; epic 0201 OQ-3's ADR ownership portion.
0201-F1 OQ-2 / epic OQ-7 still own lifecycle eligibility. Epic OQ-3 retains the
question of additional supported record kinds; 0201-F2 OQ-1 retains element
names, binding roles and supported declarations.

## Reversal

None.
