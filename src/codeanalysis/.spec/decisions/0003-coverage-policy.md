---
title: "Decision 0003: Coverage and enforcement policy"
description: "Require test bindings for selected current claims, make additional link requirements explicit and keep exemptions visible"
type: decision
---

# Decision 0003: Coverage and enforcement policy

**Date:** 2026-10-17
**Decided by:** Repository owner

## The call

- Each non-exempt current claim in the selected Feature scope requires at least
  one valid test binding. Current eligibility follows 0201-F1 decision 0004.
  Reconciliation starts from the claim inventory even when no method names a
  claim; a valid binding is distinct from discovery, execution and a passing result.
- Implementation and scenario links are optional unless the consumer explicitly
  requires them. Reqnroll remains optional. Enabling a required link kind makes
  an absent link a coverage gap under 0201-F5 B-004.
- Validate explicit bindings in analyzed projects. Require missing authored
  bindings only within explicitly selected enforcement scopes. Being outside
  authoring enforcement does not make an explicit invalid binding valid.
- Generated code requires no authored bindings. This removes a mandatory-authoring
  requirement, not all possible use of generated artifacts by evidence adapters.
  Shared infrastructure has no blanket requirement to bind every method; its
  authoring requirements follow explicit enforcement selection.
- Exemptions require an explicit reason and remain visible as exempt, never
  covered. A reasonless exemption is invalid. Diagnostic suppression is not a
  coverage exemption and does not fill an evidence gap (decision 0002).

The selected analysis projects, selected Feature inventory and required-authoring
scope are explicit boundaries; they are not inferred from absent annotations.
Configuration encoding, selection syntax and exemption record shape remain in
OQ-1 / epic OQ-8. This decision settles behavior without inventing those fields.
It does not decide test runner versions, completeness aggregation, input failure
severity, result freshness or a report format.

## Why

Test bindings provide a useful minimum across xUnit and Playwright without a
scenario framework. Optional link requirements let consumers add implementation
or scenario traceability where it adds value. Explicit enforcement scope avoids
blanket annotation of infrastructure, while validation of existing bindings
continues to catch broken references. Visible exemptions prevent opt-outs from
inflating apparent coverage.

## Rejected

- Require implementation, scenario and test links for every claim: adds mandatory
  documentation and scenario machinery regardless of the consumer's needs.
- Require bindings on every method merely because a project is analyzed:
  mandatory authoring must follow the consumer's explicit enforcement scope.
- Require handwritten bindings on generated code: asks consumers to maintain
  documentation on declarations they do not author.
- Silently exempt records or classify exemptions as covered: hides missing
  evidence and makes coverage totals misleading.
- Ignore explicit bindings outside mandatory-authoring scope: leaves broken
  references unchecked even though the project is analyzed.

The accepted cost is explicit scope and optional-link configuration, plus
reasoned exemptions that remain distinguishable from covered claims.

## Affects

0201-F5 B-008 through B-010 and OQ-1; 0201-F6 B-012 through B-015 and OQ-2;
epic 0201 OQ-5. Required-link evaluation belongs to F5; selected authoring scope
and policy validation belong to F6. Configuration encoding remains open in F6
OQ-1; existing completeness and result-state distinctions remain unchanged.

## Reversal

None.

## Repository adoption

Repository adoption decision 0006 selects every eligible type, constructor and
method in `specht` and `specht.tool` explicitly. Shared infrastructure in those
projects is therefore inside the selected authoring scope; the general policy
above still permits narrower consumer scopes.
