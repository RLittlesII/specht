---
title: "Decision 0001: Declaration-comment boundary"
description: "Scope the requested ordinary-line-comment diagnostic to class and method surfaces while retaining documentation and standard suppression."
type: decision
---

# Decision 0001: Declaration-comment boundary

**Date:** 2026-11-04
**Decided by:** Repository owner for the rule; spec-author for the bounded declaration-surface interpretation.

## The call

Add `SPCT0013` to Specht.CodeAnalysis as an enabled warning for ordinary `//`
comments on class and method surfaces. The owner's example combines unwanted
ordinary comments with a valid XML `spec` tag; the XML must not hide the ordinary
comments and needs no additional summary.

A surface comprises leading material attached to the declaration and its header,
including attributes, modifiers and signature tokens, ending before the opening
body brace, expression-body arrow or terminating semicolon. Class means the
`class` declaration form, not record, struct or interface declarations. Method
means a method declaration in any containing type, including declarations without
bodies; constructors and local functions are outside this rule. Each partial
declaration is checked independently. A diagnostic identifies each offending
line comment at its own span.

A file's initial ordinary comment block is a header only when at least one empty
or whitespace-only line separates it from the first following code or XML
documentation. An adjacent comment immediately above XML documentation or a
class remains a surface comment even at the start of the file. Blank lines later
in a selected declaration's leading material do not turn its prose into a file
header. Body comments, comments trailing a completed declaration and explanations
attached to suppression directives remain outside the surface.

Skip generated source using standard Roslyn generated-code recognition. Honor
standard reporting controls, including severity overrides and pragma suppression.
Analyze source independently of manifest/specification input availability because
the comment syntax needs no record resolution. This is a narrowly authorized
exception to epic 0201's exclusion of generic coding-convention analyzers; other
convention families remain excluded.

## Why

The owner requested enforcement after ordinary commentary repeated an ADR,
implementation details and test history above a declaration that already carried
its intended XML traceability tag. A declaration-surface boundary removes that
failure without extending the request to body commentary or other declaration
kinds. Suppression explanations retain their established purpose.

The existing package is the delivery surface. Analyzer-only additions follow
0201-F2 decision 0009 and leave the checker's schema pin and `SPEC` rules intact.

## Rejected

- Requiring XML documentation on every declaration: that changes the request from
  rejecting ordinary prose to mandating documentation, and rejects valid
  uncommented declarations.
- Checking only comments immediately beside the declaration keyword: comments
  around XML, attributes or modifiers would evade the rule.
- Banning ordinary comments everywhere: bodies, file headers and suppression
  explanations have different purposes and were not requested.
- Automatically converting prose to XML or deleting it: that requires an intent
  and preservation policy beyond the requested diagnostic.
- Requiring manifest/specification inputs: invalid or absent traceability inputs
  would hide a source-local convention defect for no semantic reason.
- Treating every first-file comment as a header: it would exempt the owner's
  example when the class is the first declaration in a file.

## Affects

0201-F8 B-001 to B-011, C-1 to C-6; epic 0201 Feature decomposition and generic
coding-convention boundary.

## Reversal

None.
