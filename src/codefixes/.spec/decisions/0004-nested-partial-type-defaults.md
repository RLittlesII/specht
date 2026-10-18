---
title: "Decision 0004: Nested and partial type defaults"
description: "Select the nearest lexical default, share consistent partial defaults and reject conflicts without inheriting base defaults"
type: decision
---

# Decision 0004: Nested and partial type defaults

**Date:** 2026-10-17
**Decided by:** Repository owner

## The call

Authoring defaults follow lexical containment, not class inheritance:

- For a member of nested types, select the nearest enclosing type that declares
  a `spec` default. A nested type without a default can use an outer default.
- A nested type's own default replaces the complete outer default. Components
  never merge; an omitted optional `epic` is not supplied from an outer type.
- Treat all declarations of a partial type as one type for default selection.
  One default on any part, or identical repeated defaults, supplies the shared
  context to members in every part. An unannotated part is not a conflict.
- Different defaults on parts of the same type produce a diagnostic and block
  automatic materialization from those defaults. No source-file ordering or
  arbitrary part selects the winner; an outer or base default cannot resolve
  the conflict.
- Base classes do not supply defaults to derived types.

These rules assist invoked authoring repairs only. They do not create checked
bindings or complete an explicit reference during analysis. An explicit binding
retains its own identity even when defaults change or conflict. Missing or
otherwise ambiguous defaults still cannot silently select a repair target
(0201-F2 B-004); an explicitly selected known target is separate from automatic
materialization.

Identical repeated defaults carry the same identity components and values;
XML formatting and attribute order do not choose a different default. This
policy does not add a new identity grammar or change consumer ID comparison.
Diagnostic identifiers and severity remain open in OQ-2.

## Why

Lexical defaults reduce repeated authoring context in nested code. A partial
type has one identity across its files, so consistent defaults must work
regardless of which part contains the member. Whole-default replacement and
explicit conflicts keep repairs predictable without changing checked evidence.

## Rejected

- Require a separate default on every nested type and every partial part:
  simpler local lookup, but repeated context adds maintenance and drift.
- Merge missing inner components with outer components: fewer attributes, but
  the resulting scope is hidden and can change when an outer default changes.
- Let the first partial declaration win: avoids a conflict diagnostic but makes
  authoring context depend on arbitrary declaration or file order.
- Inherit base-class defaults: saves annotations but couples authoring context
  to an inheritance hierarchy beyond the lexical scope being edited.

The accepted cost is inspecting containing types and all parts of a partial
symbol, plus reporting conflicts instead of guessing an authoring target.

## Affects

0201-F2 B-014 through B-018 and OQ-3; epic 0201 OQ-12. Public XML forms and
remaining diagnostic, enforcement and repair contracts stay in their existing
open questions.

## Reversal

None.
