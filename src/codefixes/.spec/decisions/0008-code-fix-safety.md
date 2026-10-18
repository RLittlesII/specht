---
title: "Decision 0008: Code-fix safety"
description: "Require explicit target choices, preserve uncertain content and restrict Fix All to deterministic repairs"
type: decision
---

# Decision 0008: Code-fix safety

**Date:** 2026-10-17
**Decided by:** Repository owner

## The call

Apply these rules to invoked binding repairs:

- Permit materialization of a complete target from an unambiguous authoring
  default under the existing claim-only repair contract (B-003), and removal of
  valid relationship duplicates while preserving one complete binding (B-027).
- When choosing or replacing a target requires a choice, require explicit
  selection of an existing specification record. Without that selection, make
  no target edit. A candidate's spelling similarity is not a selection.
- Do not guess ID or attribute corrections. Do not delete unknown attributes or
  body content merely to make a reserved element valid. Where no safe repair is
  available, retain the diagnostic for manual correction or explicit target
  selection as appropriate.
- Permit Fix All only for supported deterministic repairs requiring no target
  choice, including eligible unambiguous materialization and duplicate removal.
  Each affected binding must meet that repair's preconditions independently.
- Never propagate a target selected for one declaration across unrelated
  declarations through Fix All.

Analysis remains read-only; individual and bulk repairs are invoked explicitly.
These rules do not create claims, edit specification records, weaken XML
validation or change the requirement for each checked binding's own explicit
identity. Existing documentation-preservation guarantees remain in force.

## Why

A repair can produce valid XML that points to the wrong claim. Requiring target
selection where intent is unknown keeps those decisions with the maintainer.
Default materialization and duplicate removal can be mechanical when their
existing preconditions hold, making them useful bulk repairs without spreading
one declaration's intended target to other code.

## Rejected

- Guess the nearest-looking ID or attribute: saves editing but can silently
  produce a valid-looking link with unintended meaning.
- Delete unknown attributes or content to satisfy validation: removes material
  whose meaning the binding model has not established.
- Reuse one selected target throughout Fix All: convenient in uniform files but
  silently retargets unrelated declarations.
- Disable every bulk repair: avoids target-choice risk but loses useful safe
  materialization and duplicate cleanup.

The accepted cost is manual correction or individual target selection when a
repair cannot establish intent, while deterministic repairs remain available.

## Affects

0201-F2 B-003, B-007, B-027, B-030 through B-034 and OQ-1; epic 0201 OQ-2.
The public XML and code-fix safety agreement is resolved; diagnostic identifiers,
severity, versioning and suppression remain in OQ-2 / epic OQ-9. Production
design and tests remain downstream of the agreement.

## Reversal

None.
